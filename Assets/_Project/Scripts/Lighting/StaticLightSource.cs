using UnityEngine;

namespace Mothropolis.Lighting
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class StaticLightSource : MonoBehaviour, ILightSource
    {
        [Header("Light Settings")]
        public float radius = 5f;
        public int mothPoolWeight = 1; // Small pool for static ambient lights
        public float exposureFillRate = 0.1f; // Fills 10% of meter per second

        public float Radius => radius;
        public int MothPoolWeight => mothPoolWeight;
        public float ExposureFillRate => exposureFillRate;
        public bool IsActive => true; // Always on
        public Vector2 Position => transform.position;

        private void OnValidate()
        {
            var col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.radius = radius;
                col.isTrigger = true;
            }
        }
    }
}
