using UnityEngine;
using Mothropolis.Genetics;

namespace Mothropolis.Moths
{
    public class MothController : MonoBehaviour
    {
        public MothGenome Genome;
        public SpriteRenderer spriteRenderer;

        private void Start()
        {
            ApplyVisuals();
        }

        public void ApplyVisuals()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                // Scale based on Speed
                float scale = GeneToVisualMapper.GetScale(Genome);
                transform.localScale = new Vector3(scale, scale, 1f);

                // Tint and Alpha based on Light Attraction & Camouflage
                Color tint = GeneToVisualMapper.GetTint(Genome);
                tint.a = GeneToVisualMapper.GetAlpha(Genome);
                spriteRenderer.color = tint;
            }
        }
    }
}
