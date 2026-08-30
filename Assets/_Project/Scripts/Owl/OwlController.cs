using UnityEngine;
using System.Collections;
using Mothropolis.Core;
using Mothropolis.Night;

namespace Mothropolis.Owl
{
    public class OwlController : MonoBehaviour
    {
        public enum OwlState { Idle, Charging, Telegraph, Swoop, Recover }
        
        [Header("State (Read Only)")]
        public OwlState currentState = OwlState.Idle;

        [Header("Timing Settings")]
        public float gracePeriod = 1.0f; // Seconds the player can dip back into shadows before owl commits
        public float telegraphDuration = 1.5f; // Warning time before the strike
        public float swoopDuration = 0.8f; // Speed of the actual attack
        public float recoverDuration = 2.0f; // Time before owl can attack again
        
        [Header("Combat Settings")]
        public float strikeRadius = 1.5f; // Hitbox size

        [Header("Movement Path")]
        public Transform startPoint; // Top corner of the screen
        public AnimationCurve swoopYCurve; // Arc of the swoop (e.g. dips down to 0 and back up)
        
        [Header("Visuals")]
        public Transform spriteVisual;
        public Animator animator;

        private float _currentExposure = 0f;
        private Coroutine _stateRoutine;
        private Transform _playerTransform;

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
        }

        private void HandleNightStarted()
        {
            ResetToRoost();
        }

        private void HandleNightEnded()
        {
            ResetToRoost();
        }

        private void HandleFoodBanked(int amount)
        {
            ResetToRoost();
        }

        public void ResetToRoost()
        {
            if (_stateRoutine != null)
            {
                StopCoroutine(_stateRoutine);
                _stateRoutine = null;
            }

            _currentExposure = 0f;
            currentState = OwlState.Idle;

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
            Debug.Log("[OWL] Telegraphing! SCREECH!");
            if (animator != null) animator.SetTrigger("Telegraph");
            
            yield return new WaitForSeconds(telegraphDuration);
            TransitionTo(OwlState.Swoop);
        }

        private IEnumerator SwoopRoutine()
        {
            Debug.Log("[OWL] SWOOPING!");
            float t = 0f;
            
            Vector3 p0 = startPoint != null ? startPoint.position : new Vector3(0f, 8f, 0f);
            Vector3 targetPos = _playerTransform != null ? _playerTransform.position : Vector3.zero;
            
            // Determine horizontal flight direction (ensure it always flies across and up out of frame)
            float dx = targetPos.x - p0.x;
            if (Mathf.Abs(dx) < 2f)
            {
                dx = (targetPos.x >= 0) ? 5f : -5f;
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

                // Hit Detection
                if (_playerTransform != null && Vector3.Distance(transform.position, _playerTransform.position) < strikeRadius)
                {
                    Debug.Log("[OWL] CAUGHT THE PLAYER! Night Over.");
                    
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
            if (currentState == OwlState.Swoop)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, strikeRadius);
            }
        }
    }
}
