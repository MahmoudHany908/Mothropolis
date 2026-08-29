using UnityEngine;
using Mothropolis.Genetics;

namespace Mothropolis.Genetics
{
    public static class GeneToVisualMapper
    {
        // Speed (0 to 1) -> Wings: 
        // 0 = Broad/Rounded (<0.33), 1 = Sharp (<0.66), 2 = Double-pair (>=0.66)
        public static int GetWingVariantIndex(MothGenome genome)
        {
            if (genome.speed < 0.33f) return 0;
            if (genome.speed < 0.66f) return 1;
            return 2;
        }

        // Camouflage (0 to 1) -> Opacity: 
        // Formula from GDD -> Opacity = 1.00 - (0.55 * camouflage)
        public static float GetAlpha(MothGenome genome)
        {
            return 1.00f - (0.55f * genome.camouflage);
        }

        // Light Attraction (0 to 1) -> Body: 
        // 0 = Small/Dull Eyes (<0.33), 1 = Medium Eyes (<0.66), 2 = Large/Bright/Amber Eyes (>=0.66)
        public static int GetBodyVariantIndex(MothGenome genome)
        {
            if (genome.lightAttraction < 0.33f) return 0;
            if (genome.lightAttraction < 0.66f) return 1;
            return 2;
        }
    }
}
