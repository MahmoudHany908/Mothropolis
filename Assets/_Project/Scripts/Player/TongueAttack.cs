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
        public LayerMask mothLayer;
        
        [Header("Visuals")]
        public Animator animator;
        public Transform spriteVisual; // Used to determine facing direction
        public Transform tongueOrigin; // Must be child of spriteVisual so it flips position
        public SpriteRenderer tongueSpriteRenderer; // Must NOT be child of spriteVisual, to avoid warping

        private float _visualTimer = 0f;

        private void Start()
        {
            if (tongueSpriteRenderer != null)
            {
                tongueSpriteRenderer.enabled = false;
            }
        }

        private void Update()
        {
            if (_visualTimer > 0)
            {
                _visualTimer -= Time.deltaTime;
                if (_visualTimer <= 0)
                {
                    if (tongueSpriteRenderer != null) tongueSpriteRenderer.enabled = false;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (GameLoopManager.Instance != null && GameLoopManager.Instance.CurrentState != GameState.Hunting) return;
                
                // Allow consecutive attacks if visual timer is already running
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            if (Camera.main == null || spriteVisual == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 origin = tongueOrigin != null ? tongueOrigin.position : transform.position;
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

            // 2. Animation Trigger
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // 3. Tongue Logic
            float distance = Vector2.Distance(origin, worldMousePos);
            distance = Mathf.Min(distance, attackRange);

            if (tongueSpriteRenderer != null)
            {
                tongueSpriteRenderer.enabled = true;
                
                // We assume the TongueVisual object is a child of the ROOT frog, NOT the SpriteVisual.
                // This prevents the parent's negative scale from warping the tongue's rotation.
                
                // Set position EXACTLY to the mouth origin (Requires Sprite Pivot = Left)
                tongueSpriteRenderer.transform.position = origin;
                
                // Rotate to point at cursor
                float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
                tongueSpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
                
                // Calculate the correct scale by dividing by the sprite's actual width
                float spriteWidth = tongueSpriteRenderer.sprite != null ? tongueSpriteRenderer.sprite.bounds.size.x : 1f;
                
                // Account for the parent's scale! If the Frog is scaled down to 0.5, a localScale of 1 only covers 0.5 world units.
                float parentScaleX = tongueSpriteRenderer.transform.parent != null ? tongueSpriteRenderer.transform.parent.lossyScale.x : 1f;
                float requiredLocalScaleX = (distance / spriteWidth) / Mathf.Abs(parentScaleX);
                
                Vector3 currentScale = tongueSpriteRenderer.transform.localScale;
                tongueSpriteRenderer.transform.localScale = new Vector3(requiredLocalScaleX, Mathf.Abs(currentScale.y), Mathf.Abs(currentScale.z));
            }
            
            _visualTimer = 0.15f;

            // Broadcast attack position so moths can scatter
            GameEvents.RaiseTongueAttack(origin);

            // Raycast to catch moths
            RaycastHit2D hit = Physics2D.Raycast(origin, directionToMouse, distance, mothLayer);
            if (hit.collider != null)
            {
                var moth = hit.collider.GetComponent<Moths.MothController>();
                if (moth != null)
                {
                    Debug.Log($"Caught a moth! Raising OnMothCaught event.");
                    GameEvents.RaiseMothCaught(moth.Genome);
                    Destroy(moth.gameObject);
                }
            }
        }
    }
}
