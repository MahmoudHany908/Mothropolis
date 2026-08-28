using UnityEngine;
using Mothropolis.Genetics;

namespace Mothropolis.Genetics
{
    public static class GeneToVisualMapper
    {
        // Speed (0 to 1): High speed = smaller, aerodynamic (scale 0.6). Low speed = bulky (scale 1.2).
        public static float GetScale(MothGenome genome)
        {
            return Mathf.Lerp(1.2f, 0.6f, genome.speed);
        }

        // Camouflage (0 to 1): Formula from GDD -> Opacity = 1.00 - (0.55 * camouflage)
        public static float GetAlpha(MothGenome genome)
        {
            return 1.00f - (0.55f * genome.camouflage);
        }

        // Light Attraction (0 to 1): Low = dark, dingy tint. High = bright, glowy tint.
        public static Color GetTint(MothGenome genome)
        {
            return Color.Lerp(new Color(0.4f, 0.4f, 0.4f), new Color(1f, 1f, 0.8f), genome.lightAttraction);
        }
    }
}
