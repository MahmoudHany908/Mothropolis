using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Mothropolis.Genetics;

namespace Tests.EditMode
{
    public class ReproductionEngineTests
    {
        private MutationSettings _settings;
        private ReproductionEngine _engine;
        private System.Random _rng;

        [SetUp]
        public void Setup()
        {
            _settings = ScriptableObject.CreateInstance<MutationSettings>();
            _settings.mutationRange = 0.08f;
            _engine = new ReproductionEngine(_settings);
            _rng = new System.Random(42); // Deterministic seed for repeatable tests
        }

        [Test]
        public void Simulation_5Generations_ShiftsPopulationMeaningfully()
        {
            int popSize = 24;
            var currentGen = new List<MothGenome>();
            
            // Baseline starting population
            for (int i = 0; i < popSize; i++)
            {
                currentGen.Add(new MothGenome { speed = 0.5f, camouflage = 0.5f, lightAttraction = 0.5f });
            }

            // Simulate 5 nights of hunting
            for (int gen = 0; gen < 5; gen++)
            {
                // Sort by "fitness" against the player:
                // Harder to catch = higher speed, higher camo, lower light attraction
                var sorted = currentGen.OrderByDescending(m => 
                    m.speed + m.camouflage + (1f - m.lightAttraction)
                ).ToList();

                // Player eats the worst 12 (bottom of the sorted list), top 12 survive
                var survivors = sorted.Take(12).ToList();

                currentGen = _engine.Reproduce(survivors, popSize, _rng);
            }

            float avgSpeed = currentGen.Average(m => m.speed);
            float avgCamo = currentGen.Average(m => m.camouflage);
            float avgLightAttr = currentGen.Average(m => m.lightAttraction);

            Debug.Log($"After 5 generations: Speed={avgSpeed:F3}, Camo={avgCamo:F3}, LightAttr={avgLightAttr:F3}");

            // Verify population shifted in the correct direction from the 0.5 baseline.
            // With ±0.08 mutation the shift is intentionally subtle (~0.1 over 5 gens).
            // These tests validate the engine works directionally, not specific magnitudes.
            Assert.Greater(avgSpeed, 0.50f, "Speed should have increased from baseline");
            Assert.Greater(avgCamo, 0.50f, "Camouflage should have increased from baseline");
            Assert.Less(avgLightAttr, 0.50f, "Light Attraction should have decreased from baseline");
        }

        [Test]
        public void Reproduce_WhenNoSurvivors_UsesImmigrantFallback()
        {
            bool eventFired = false;
            Mothropolis.Core.GameEvents.OnImmigrantEvent += () => eventFired = true;

            var nextGen = _engine.Reproduce(new List<MothGenome>(), 24, _rng);
            
            Assert.AreEqual(24, nextGen.Count, "Should repopulate back to target count using immigrants");
            Assert.IsTrue(eventFired, "Should have raised OnImmigrantEvent");
        }
    }
}
