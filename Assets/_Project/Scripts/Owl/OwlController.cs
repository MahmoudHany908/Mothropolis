using UnityEngine;
using System.Collections;
using Mothropolis.Core;
using Mothropolis.Night;

namespace Mothropolis.Owl
{
    public enum OwlState { Idle, Charging, Telegraph, Swoop, Recover }

    public class OwlController : MonoBehaviour
    {
        [Header("State (Read Only)")]
        public OwlState currentState = OwlState.Idle;

        [Header("Timing Settings")]
        public float gracePeriod = 1.0f; // Seconds the player can dip back into shadows before owl commits
        public float telegraphDuration = 1.5f; // Warning time before the strike
        public float swoopDuration = 2.3f; // Speed of the actual attack (tuned to 2.3s for generous reaction time)
        public float recoverDuration = 2.0f; // Time before owl can attack again
        
        [Header("Combat Settings")]
        public float strikeRadius = 1.2f; // Hitbox size

        [Header("Movement Path")]
        public Transform startPoint; // Top corner of the screen
        public AnimationCurve swoopYCurve; // Arc of the swoop (e.g. dips down to 0 and back up)
        
        [Header("Visuals")]
        public Transform spriteVisual;
        public Animator animator;

        private float _currentExposure = 0f;
        private Coroutine _stateRoutine;
        private Transform _playerTransform;
        private Vector3 _lockedTargetPos;
        private GameObject _telegraphMarkerObj;
        private LineRenderer _telegraphRing;
        private LineRenderer _telegraphCrosshair;

        private void Awake()
        {
            CreateTelegraphVisual();
        }

        private void CreateTelegraphVisual()
        {
            if (_telegraphMarkerObj != null) return;

            _telegraphMarkerObj = new GameObject("OwlGroundTelegraph");
            _telegraphMarkerObj.transform.SetParent(null); // World space independent

            // Ring Renderer (Crimson Red Hazard Circle)
            var ringObj = new GameObject("TelegraphRing");
            ringObj.transform.SetParent(_telegraphMarkerObj.transform, false);
            _telegraphRing = ringObj.AddComponent<LineRenderer>();
            _telegraphRing.useWorldSpace = false;
            _telegraphRing.loop = true;
            _telegraphRing.positionCount = 32;
            _telegraphRing.startWidth = 0.08f;
            _telegraphRing.endWidth = 0.08f;
            _telegraphRing.sortingOrder = 15; // Above ground tiles

            // Set unlit material/color
            Material lineMat = new Material(Shader.Find("Sprites/Default"));
            _telegraphRing.material = lineMat;
            _telegraphRing.startColor = new Color(1f, 0.1f, 0.15f, 0.9f);
            _telegraphRing.endColor = new Color(1f, 0.1f, 0.15f, 0.9f);

            float radius = strikeRadius;
            for (int i = 0; i < 32; i++)
            {
                float angle = i * (Mathf.PI * 2f / 32);
                _telegraphRing.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f)); // Uniform circular reticle
            }

            // Crosshair / Warning Slash
            var crossObj = new GameObject("TelegraphCross");
            crossObj.transform.SetParent(_telegraphMarkerObj.transform, false);
            _telegraphCrosshair = crossObj.AddComponent<LineRenderer>();
            _telegraphCrosshair.useWorldSpace = false;
            _telegraphCrosshair.material = lineMat;
            _telegraphCrosshair.positionCount = 4;
            _telegraphCrosshair.startWidth = 0.06f;
            _telegraphCrosshair.endWidth = 0.06f;
            _telegraphCrosshair.sortingOrder = 16;
            _telegraphCrosshair.startColor = new Color(1f, 0.2f, 0.2f, 0.8f);
            _telegraphCrosshair.endColor = new Color(1f, 0.2f, 0.2f, 0.8f);

            _telegraphCrosshair.SetPosition(0, new Vector3(-radius * 0.7f, 0f, 0f));
            _telegraphCrosshair.SetPosition(1, new Vector3(radius * 0.7f, 0f, 0f));
            _telegraphCrosshair.SetPosition(2, new Vector3(0f, -radius * 0.7f, 0f));
            _telegraphCrosshair.SetPosition(3, new Vector3(0f, radius * 0.7f, 0f));

            _telegraphMarkerObj.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnExposureChanged += HandleExposureChanged;
            GameEvents.OnNightStarted += HandleNightStarted;
            GameEvents.OnDawnReached += HandleNightEnded;
            GameEvents.OnFoodBanked += HandleFoodBanked;
        }

        private void OnDisable()
        {
            GameEvents.OnExposureChanged -= HandleExposureChanged;
            GameEvents.OnNightStarted -= HandleNightStarted;
            GameEvents.OnDawnReached -= HandleNightEnded;
            GameEvents.OnFoodBanked -= HandleFoodBanked;

            HideTelegraph();
            if (_telegraphMarkerObj != null) Destroy(_telegraphMarkerObj);
        }

        private void HandleNightStarted() => ResetToRoost();
        private void HandleNightEnded() => ResetToRoost();
        private void HandleFoodBanked(int amount) => ResetToRoost();

        public void ResetToRoost()
        {
            if (_stateRoutine != null)
            {
                StopCoroutine(_stateRoutine);
                _stateRoutine = null;
            }

            _currentExposure = 0f;
            currentState = OwlState.Idle;
            HideTelegraph();

            if (startPoint == null)
            {
                var startObj = GameObject.Find("OwlStartPoint");
                if (startObj != null) startPoint = startObj.transform;
            }

            if (startPoint != null)
            {
                transform.position = startPoint.position;
            }

            if (animator != null)
            {
                animator.ResetTrigger("Swoop");
                animator.ResetTrigger("Telegraph");
                animator.SetTrigger("ResetToIdle");
                animator.SetFloat("SwoopProgress", 0f);
            }

            GameEvents.RaiseOwlStateChanged(OwlState.Idle);
        }

        private void Start()
        {
            if (spriteVisual == null) spriteVisual = transform;
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (startPoint == null)
            {
                var startObj = GameObject.Find("OwlStartPoint");
                if (startObj != null) startPoint = startObj.transform;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
            
            ResetToRoost();
        }

        private void HandleExposureChanged(float exposureRatio)
        {
            _currentExposure = exposureRatio;
            
            // Trigger attack when exposure fills completely
            if (currentState == OwlState.Idle && _currentExposure >= 1.0f)
            {
                TransitionTo(OwlState.Charging);
            }
        }

        private void TransitionTo(OwlState newState)
        {
            if (_stateRoutine != null) StopCoroutine(_stateRoutine);
            currentState = newState;
            GameEvents.RaiseOwlStateChanged(newState);

            switch (newState)
            {
                case OwlState.Idle:
                    ResetToRoost();
                    break;
                case OwlState.Charging:
                    _stateRoutine = StartCoroutine(ChargingRoutine());
                    break;
                case OwlState.Telegraph:
                    _stateRoutine = StartCoroutine(TelegraphRoutine());
                    break;
                case OwlState.Swoop:
                    _stateRoutine = StartCoroutine(SwoopRoutine());
                    break;
                case OwlState.Recover:
                    _stateRoutine = StartCoroutine(RecoverRoutine());
                    break;
            }
        }

        private IEnumerator ChargingRoutine()
        {
            float chargeTimer = 0f;
            Debug.Log("[OWL] Charging... Player is fully exposed!");

            // Grace period: If player drops exposure, owl aborts.
            while (chargeTimer < gracePeriod)
            {
                if (_currentExposure < 1.0f)
                {
                    Debug.Log("[OWL] Player hid in time. Owl aborted charge.");
                    TransitionTo(OwlState.Idle);
                    yield break;
                }
                chargeTimer += Time.deltaTime;
                yield return null;
            }
            TransitionTo(OwlState.Telegraph);
        }

        private IEnumerator TelegraphRoutine()
        {
            Debug.Log("[OWL] Telegraphing! SCREECH! Ground hazard marker active.");
            if (animator != null) animator.SetTrigger("Telegraph");

            // Lock in ground strike target position
            _lockedTargetPos = _playerTransform != null ? _playerTransform.position : transform.position;
            ShowTelegraph(_lockedTargetPos);

            float timer = 0f;
            while (timer < telegraphDuration)
            {
                timer += Time.deltaTime;
                
                // Track player position during the first 0.5s of telegraph, then firmly lock position
                if (timer < 0.5f && _playerTransform != null)
                {
                    _lockedTargetPos = _playerTransform.position;
                }

                UpdateTelegraphPulse(_lockedTargetPos, timer / telegraphDuration);
                yield return null;
            }

            TransitionTo(OwlState.Swoop);
        }

        private void ShowTelegraph(Vector3 targetPos)
        {
            if (_telegraphMarkerObj == null) CreateTelegraphVisual();
            if (_telegraphMarkerObj != null)
            {
                _telegraphMarkerObj.transform.position = targetPos;
                _telegraphMarkerObj.SetActive(true);
            }
        }

        private void UpdateTelegraphPulse(Vector3 targetPos, float progress)
        {
            if (_telegraphMarkerObj == null || !_telegraphMarkerObj.activeSelf) return;

            _telegraphMarkerObj.transform.position = targetPos;

            // Crimson pulsing frequency increases as strike nears
            float pulse = 1f + Mathf.Sin(progress * Mathf.PI * 8f) * 0.15f;
            _telegraphMarkerObj.transform.localScale = Vector3.one * pulse;

            if (_telegraphRing != null)
            {
                float alpha = Mathf.Lerp(0.5f, 1f, progress);
                Color col = new Color(1f, 0.1f, 0.15f, alpha);
                _telegraphRing.startColor = col;
                _telegraphRing.endColor = col;
            }
        }

        private void HideTelegraph()
        {
            if (_telegraphMarkerObj != null)
            {
                _telegraphMarkerObj.SetActive(false);
            }
        }

        private IEnumerator SwoopRoutine()
        {
            Debug.Log("[OWL] SWOOPING down to locked target!");
            float t = 0f;
            
            Vector3 p0 = startPoint != null ? startPoint.position : new Vector3(0f, 8f, 0f);
            Vector3 targetPos = _lockedTargetPos;
            
            // Determine horizontal flight direction (ensure it flies across and up out of frame)
            float dx = targetPos.x - p0.x;
            if (Mathf.Abs(dx) < 2.5f)
            {
                dx = (targetPos.x >= p0.x) ? 6f : -6f;
            }

            // p2 is the exit fly-away point high above the screen on the other side
            Vector3 p2 = new Vector3(targetPos.x + dx, p0.y, 0f);

            // Calculate quadratic bezier control point P1 so curve passes exactly through targetPos at t=0.5
            Vector3 p1 = 2f * targetPos - 0.5f * (p0 + p2);

            // Sprite Flipping Logic
            if (spriteVisual != null)
            {
                float swoopDirectionX = p2.x - p0.x;
                if (swoopDirectionX != 0)
                {
                    Vector3 scale = spriteVisual.localScale;
                    scale.x = swoopDirectionX > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                    spriteVisual.localScale = scale;
                }
            }

            if (animator != null) animator.SetTrigger("Swoop");

            while (t < 1f)
            {
                t += Time.deltaTime / swoopDuration;
                
                if (animator != null) animator.SetFloat("SwoopProgress", Mathf.Clamp01(t));
                
                // Calculate Quadratic Bezier Position
                float u = 1f - t;
                Vector3 currentPos = (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
                transform.position = currentPos;

                // Hide ground indicator once the owl dives past ground level
                if (t >= 0.5f)
                {
                    HideTelegraph();
                }

                // Hit Detection
                if (_playerTransform != null && Vector3.Distance(transform.position, _playerTransform.position) < strikeRadius)
                {
                    Debug.Log("[OWL] CAUGHT THE PLAYER! Night Over.");
                    HideTelegraph();

                    var nightManager = FindFirstObjectByType<NightManager>();
                    if (nightManager != null)
                    {
                        nightManager.FailNight();
                    }

                    ResetToRoost();
                    yield break;
                }

                yield return null;
            }
            
            HideTelegraph();
            Debug.Log("[OWL] Missed the player. Flying away off-screen.");
            TransitionTo(OwlState.Recover);
        }

        private IEnumerator RecoverRoutine()
        {
            Debug.Log("[OWL] Recovering off-screen...");
            yield return new WaitForSeconds(recoverDuration);
            TransitionTo(OwlState.Idle);
        }

        private void OnDrawGizmos()
        {
            if (currentState == OwlState.Swoop || currentState == OwlState.Telegraph)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentState == OwlState.Telegraph ? _lockedTargetPos : transform.position, strikeRadius);
            }
        }
    }
}
