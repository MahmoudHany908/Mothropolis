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
            GameEvents.OnDawnReached += HandleNightEnded;
        }

        private void OnDisable()
        {
            GameEvents.OnExposureChanged -= HandleExposureChanged;
            GameEvents.OnDawnReached -= HandleNightEnded;
        }

        private void HandleNightEnded()
        {
            if (_stateRoutine != null) StopCoroutine(_stateRoutine);
            this.enabled = false; // Disable the Owl completely once the night is over
        }

        private void Start()
        {
            if (spriteVisual == null) spriteVisual = transform;
            if (animator == null) animator = GetComponentInChildren<Animator>();

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
            
            TransitionTo(OwlState.Idle);
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
                    if (startPoint != null) transform.position = startPoint.position;
                    if (animator != null) animator.SetTrigger("ResetToIdle");
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
            
            // Here you would play the warning SFX and show a UI indicator (the red exclamation mark)
            
            yield return new WaitForSeconds(telegraphDuration);
            TransitionTo(OwlState.Swoop);
        }

        private IEnumerator SwoopRoutine()
        {
            Debug.Log("[OWL] SWOOPING!");
            float t = 0f;
            
            Vector3 p0 = startPoint != null ? startPoint.position : transform.position;
            // Lock onto where the player is right at the start of the swoop
            Vector3 targetPos = _playerTransform != null ? _playerTransform.position : Vector3.zero;
            
            // p2 is the end point (fly away to the opposite side of the screen, same height as start)
            Vector3 p2 = targetPos + new Vector3((targetPos.x - p0.x), 0f, 0f);
            p2.y = p0.y; 

            // We want the curve to pass EXACTLY through targetPos at the halfway point (t = 0.5).
            // Quadratic Bezier at t=0.5: B(0.5) = 0.25*P0 + 0.5*P1 + 0.25*P2
            // Solving for P1 (the invisible control point that pulls the curve):
            Vector3 p1 = 2f * targetPos - 0.5f * (p0 + p2);

            // Sprite Flipping Logic
            if (spriteVisual != null)
            {
                float swoopDirectionX = p2.x - p0.x;
                if (swoopDirectionX != 0)
                {
                    // The Owl_Attack sprite naturally faces LEFT.
                    // So if we are swooping RIGHT (> 0), we must flip the scale to negative!
                    Vector3 scale = spriteVisual.localScale;
                    scale.x = swoopDirectionX > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                    spriteVisual.localScale = scale;
                }
            }

            if (animator != null) animator.SetTrigger("Swoop");

            while (t < 1f)
            {
                t += Time.deltaTime / swoopDuration;
                
                // Scrub the animation to match the swoop progress EXACTLY
                if (animator != null) animator.SetFloat("SwoopProgress", t);
                
                // Calculate Quadratic Bezier Position
                float u = 1f - t;
                Vector3 currentPos = (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
                
                transform.position = currentPos;

                // Hit Detection - expanded strike radius slightly in case of frame-skips
                if (_playerTransform != null && Vector3.Distance(transform.position, _playerTransform.position) < strikeRadius)
                {
                    Debug.Log("[OWL] CAUGHT THE PLAYER! Night Over.");
                    
                    var nightManager = FindFirstObjectByType<NightManager>();
                    if (nightManager != null)
                    {
                        nightManager.FailNight();
                    }
                    
                    TransitionTo(OwlState.Recover);
                    yield break;
                }

                yield return null;
            }
            
            Debug.Log("[OWL] Missed the player. Flying away.");
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
