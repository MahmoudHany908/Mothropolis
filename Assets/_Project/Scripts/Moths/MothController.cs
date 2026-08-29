using UnityEngine;
using Mothropolis.Genetics;

namespace Mothropolis.Moths
{
    [System.Serializable]
    public struct WingVariant
    {
        public Sprite[] flutterFrames; // 2-4 frames per variant
    }

    public class MothController : MonoBehaviour
    {
        public MothGenome Genome;

        [Header("Renderers (Child GameObjects)")]
        public SpriteRenderer bodyRenderer;
        public SpriteRenderer wingsRenderer;

        [Header("Visual Variants")]
        public WingVariant[] wingVariants; // Array of 3 variants
        public Sprite[] bodyVariants;      // Array of 3 variants (baked eyes)
        public float flutterSpeed = 15f;   // Speed of the frame animation

        private int _currentWingIndex;
        private float _flutterTimer;
        private int _currentFlutterFrame;

        private void Start()
        {
            ApplyVisuals();
        }

        public void ApplyVisuals()
        {
            // 1. Wings (Speed) -> determine which array of frames to use
            _currentWingIndex = GeneToVisualMapper.GetWingVariantIndex(Genome);

            // 2. Body (Light Attraction) -> swap the static sprite (eyes are baked in)
            int bodyIndex = GeneToVisualMapper.GetBodyVariantIndex(Genome);
            if (bodyVariants != null && bodyIndex < bodyVariants.Length && bodyRenderer != null)
            {
                bodyRenderer.sprite = bodyVariants[bodyIndex];
            }

            // 3. Body Opacity (Camouflage) -> apply alpha to all layers
            float alpha = GeneToVisualMapper.GetAlpha(Genome);
            SetAlpha(bodyRenderer, alpha);
            SetAlpha(wingsRenderer, alpha);
        }

        private void SetAlpha(SpriteRenderer sr, float alpha)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        private void Update()
        {
            // Continuous flutter animation in script
            if (wingVariants != null && _currentWingIndex < wingVariants.Length && wingsRenderer != null)
            {
                var frames = wingVariants[_currentWingIndex].flutterFrames;
                if (frames != null && frames.Length > 0)
                {
                    _flutterTimer += Time.deltaTime * flutterSpeed;
                    if (_flutterTimer >= 1f)
                    {
                        _flutterTimer = 0f;
                        _currentFlutterFrame = (_currentFlutterFrame + 1) % frames.Length;
                        wingsRenderer.sprite = frames[_currentFlutterFrame];
                    }
                }
            }
        }
    }
}
