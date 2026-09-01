using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Mothropolis.Core;

namespace Mothropolis.Player
{
    public class TongueAttack : MonoBehaviour
    {
        [Header("Attack Settings")]
        public float attackRange = 6f;
        [Tooltip("Maximum angle (in degrees) the tongue can fire from the forward facing direction")]
        public float maxAimAngle = 60f; 
        public float extendSpeed = 35f; // units per second
        public float retractSpeed = 45f; // units per second
        public LayerMask mothLayer;
        
        [Header("Visuals")]
        public Animator animator;
        public Transform spriteVisual; // Used to determine facing direction
        public Transform tongueOrigin; // Must be child of spriteVisual so it flips position
        public SpriteRenderer tongueSpriteRenderer; // Must NOT be child of spriteVisual, to avoid warping

        private Coroutine _attackRoutine;

        public bool IsAttacking => _attackRoutine != null;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (spriteVisual == null) spriteVisual = transform.Find("Visuals");
            if (tongueOrigin == null && spriteVisual != null) tongueOrigin = spriteVisual.Find("MouthOrigin");
            if (tongueSpriteRenderer == null)
            {
                var tv = transform.Find("TongueVisual");
                if (tv != null) tongueSpriteRenderer = tv.GetComponent<SpriteRenderer>();
            }
            if (mothLayer.value == 0) mothLayer = LayerMask.GetMask("Moth");
        }

        private void Start()
        {
            if (tongueSpriteRenderer != null)
            {
                tongueSpriteRenderer.enabled = false;
            }
        }

        private void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (GameLoopManager.Instance != null && GameLoopManager.Instance.CurrentState != GameState.Hunting) return;
                
                // Only attack if not already in the middle of a tongue stroke
                if (!IsAttacking)
                {
                    PerformAttack();
                }
            }
        }

        private void PerformAttack()
        {
            if (Camera.main == null || spriteVisual == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 origin = tongueOrigin != null ? (Vector2)tongueOrigin.position : (Vector2)transform.position;
            Vector2 directionToMouse = (worldMousePos - origin).normalized;
            
            // 1. Aim Constraints
            bool isFacingRight = spriteVisual.localScale.x > 0;
            Vector2 forwardDir = isFacingRight ? Vector2.right : Vector2.left;
            
            // Calculate angle between forward direction and mouse direction
            float aimAngle = Vector2.Angle(forwardDir, directionToMouse);
            
            if (aimAngle > maxAimAngle)
            {
                // Reject attack: clicked too far behind the frog
                return;
            }

            float targetDistance = Mathf.Min(Vector2.Distance(origin, worldMousePos), attackRange);

            if (_attackRoutine != null) StopCoroutine(_attackRoutine);
            _attackRoutine = StartCoroutine(TongueStrokeRoutine(directionToMouse, targetDistance));
        }

        private IEnumerator TongueStrokeRoutine(Vector2 direction, float targetDistance)
        {
            // 1. Open mouth & raise attack events
            if (animator != null)
            {
                animator.SetBool("IsAttacking", true);
                animator.SetTrigger("Attack");
            }

            Vector2 origin = tongueOrigin != null ? (Vector2)tongueOrigin.position : (Vector2)transform.position;
            GameEvents.RaiseTongueAttack(origin);

            if (tongueSpriteRenderer != null)
            {
                tongueSpriteRenderer.enabled = true;
            }

            float currentDistance = 0f;
            bool caughtMoth = false;

            // 2. EXTEND PHASE
            while (currentDistance < targetDistance)
            {
                currentDistance += extendSpeed * Time.deltaTime;
                currentDistance = Mathf.Min(currentDistance, targetDistance);

                origin = tongueOrigin != null ? (Vector2)tongueOrigin.position : (Vector2)transform.position;
                UpdateTongueVisual(origin, direction, currentDistance);

                // Hit Detection while extending
                if (!caughtMoth)
                {
                    RaycastHit2D hit = Physics2D.Raycast(origin, direction, currentDistance, mothLayer);
                    if (hit.collider != null)
                    {
                        var moth = hit.collider.GetComponent<Moths.MothController>();
                        if (moth != null)
                        {
                            caughtMoth = true;
                            Debug.Log($"Caught a moth! Raising OnMothCaught event.");
                            GameEvents.RaiseMothCaught(moth.Genome);
                            Destroy(moth.gameObject);
                            break; // Immediately begin retracting upon hitting target
                        }
                    }
                }

                yield return null;
            }

            // 3. RETRACT PHASE
            while (currentDistance > 0f)
            {
                currentDistance -= retractSpeed * Time.deltaTime;
                currentDistance = Mathf.Max(currentDistance, 0f);

                origin = tongueOrigin != null ? (Vector2)tongueOrigin.position : (Vector2)transform.position;
                UpdateTongueVisual(origin, direction, currentDistance);

                yield return null;
            }

            // 4. RETRACT COMPLETE -> Close mouth
            if (tongueSpriteRenderer != null)
            {
                tongueSpriteRenderer.enabled = false;
            }

            if (animator != null)
            {
                animator.SetBool("IsAttacking", false);
            }

            _attackRoutine = null;
        }

        private void UpdateTongueVisual(Vector2 origin, Vector2 direction, float distance)
        {
            if (tongueSpriteRenderer == null) return;

            tongueSpriteRenderer.transform.position = origin;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            tongueSpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);

            float spriteWidth = tongueSpriteRenderer.sprite != null ? tongueSpriteRenderer.sprite.bounds.size.x : 1f;
            float parentScaleX = tongueSpriteRenderer.transform.parent != null ? tongueSpriteRenderer.transform.parent.lossyScale.x : 1f;
            float requiredLocalScaleX = (distance / spriteWidth) / Mathf.Abs(parentScaleX);

            Vector3 currentScale = tongueSpriteRenderer.transform.localScale;
            tongueSpriteRenderer.transform.localScale = new Vector3(requiredLocalScaleX, Mathf.Abs(currentScale.y), Mathf.Abs(currentScale.z));
        }

        private void OnDisable()
        {
            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }
            if (tongueSpriteRenderer != null)
            {
                tongueSpriteRenderer.enabled = false;
            }
            if (animator != null)
            {
                animator.SetBool("IsAttacking", false);
            }
        }
    }
}
