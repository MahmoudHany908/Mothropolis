using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mothropolis.Genetics
{
    public class ReproductionEngine 
    {
        private readonly MutationSettings _settings;

        public ReproductionEngine(MutationSettings settings) 
        {
            _settings = settings;
        }

        public List<MothGenome> Reproduce(List<MothGenome> survivors, int targetCount, System.Random rng) 
        {
            if (survivors == null || survivors.Count == 0)
            {
                survivors = _settings.GetImmigrantFallback();
            }

            var next = new List<MothGenome>(targetCount);
            for (int i = 0; i < targetCount; i++) 
            {
                var parent = survivors[rng.Next(survivors.Count)];
                next.Add(Mutate(parent, rng));
            }
            return next;
        }

        private MothGenome Mutate(MothGenome parent, System.Random rng) 
        {
            float M() => (float)(rng.NextDouble() * 2.0 - 1.0) * _settings.mutationRange;
            
            return new MothGenome {
                speed = Mathf.Clamp01(parent.speed + M()),
                camouflage = Mathf.Clamp01(parent.camouflage + M()),
                lightAttraction = Mathf.Clamp01(parent.lightAttraction + M())
            };
        }
    }
}
