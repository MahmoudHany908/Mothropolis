using UnityEngine;
using UnityEngine.InputSystem;
using Mothropolis.Core;

namespace Mothropolis.Player
{
    public class TongueAttack : MonoBehaviour
    {
        [Header("Attack Settings")]
        public float attackRange = 6f;
        public LayerMask mothLayer;
        
        [Header("Visuals (Optional for now)")]
        public LineRenderer tongueVisual;
        public Transform tongueOrigin;

        private float _visualTimer = 0f;

        private void Update()
        {
            if (_visualTimer > 0)
            {
                _visualTimer -= Time.deltaTime;
                if (_visualTimer <= 0 && tongueVisual != null)
                {
                    tongueVisual.enabled = false;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (GameLoopManager.Instance != null && GameLoopManager.Instance.CurrentState != GameState.Hunting) return;
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            if (Camera.main == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 origin = tongueOrigin != null ? tongueOrigin.position : transform.position;
            Vector2 direction = (worldMousePos - origin).normalized;

            // Simple line visual
            if (tongueVisual != null)
            {
                tongueVisual.enabled = true;
                tongueVisual.SetPosition(0, origin);
                tongueVisual.SetPosition(1, origin + direction * attackRange);
                _visualTimer = 0.1f;
            }

            // Broadcast attack position so moths can scatter
            GameEvents.RaiseTongueAttack(origin);

            // Raycast to catch moths
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, attackRange, mothLayer);
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
