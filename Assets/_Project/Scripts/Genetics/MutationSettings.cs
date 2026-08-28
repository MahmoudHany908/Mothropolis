using System.Collections.Generic;
using UnityEngine;

namespace Mothropolis.Genetics
{
    [CreateAssetMenu(fileName = "MutationSettings", menuName = "Mothropolis/Mutation Settings")]
    public class MutationSettings : ScriptableObject
    {
        [Tooltip("± range for mutation per gene per generation")]
        public float mutationRange = 0.08f;

        [Tooltip("Fallback moths to inject if population is entirely wiped out")]
        public MothGenome[] immigrantMoths = new MothGenome[]
        {
            new MothGenome { speed = 0.5f, camouflage = 0.5f, lightAttraction = 0.5f },
            new MothGenome { speed = 0.5f, camouflage = 0.5f, lightAttraction = 0.5f }
        };

        public List<MothGenome> GetImmigrantFallback()
        {
            Core.GameEvents.RaiseImmigrantEvent();
            return new List<MothGenome>(immigrantMoths);
        }
    }
}
