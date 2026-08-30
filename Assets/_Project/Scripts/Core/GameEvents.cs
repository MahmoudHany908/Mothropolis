using System;
using System.Collections.Generic;
using Mothropolis.Genetics;

namespace Mothropolis.Core
{
    public static class GameEvents 
    {
        public static event Action<MothGenome> OnMothCaught;
        public static event Action<UnityEngine.Vector2> OnTongueAttack;
        public static event Action OnDawnReached;
        public static event Action<float> OnExposureChanged;
        public static event Action<int> OnFoodBanked;
        public static event Action<List<MothGenome>, List<MothGenome>> OnGenerationComplete;
        public static event Action OnImmigrantEvent;

        public static void RaiseMothCaught(MothGenome genome) => OnMothCaught?.Invoke(genome);
        public static void RaiseTongueAttack(UnityEngine.Vector2 pos) => OnTongueAttack?.Invoke(pos);
        public static void RaiseDawnReached() => OnDawnReached?.Invoke();
        public static void RaiseExposureChanged(float exposure) => OnExposureChanged?.Invoke(exposure);
        public static void RaiseFoodBanked(int amount) => OnFoodBanked?.Invoke(amount);
        public static void RaiseGenerationComplete(List<MothGenome> before, List<MothGenome> after) => OnGenerationComplete?.Invoke(before, after);
        public static void RaiseImmigrantEvent() => OnImmigrantEvent?.Invoke();
    }
}
