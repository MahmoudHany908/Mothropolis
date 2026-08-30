using System.Collections.Generic;
using UnityEngine;
using Mothropolis.Genetics;
using Mothropolis.Moths;
using Mothropolis.Night;

namespace Mothropolis.Core
{
    public class MothPopulationManager : MonoBehaviour
    {
        public NightConfig config;
        public MutationSettings mutationSettings;
        public GameObject mothPrefab;
        
        public int CurrentGenerationCount { get; private set; } = 1;
        public List<MothGenome> CurrentGeneration { get; private set; } = new List<MothGenome>();
        
        private ReproductionEngine _reproductionEngine;
        private IMothSpawner _currentSpawner;

        private void Awake()
        {
            if (mutationSettings != null)
                _reproductionEngine = new ReproductionEngine(mutationSettings);
        }

        private void OnEnable()
        {
            GameEvents.OnMothCaught += HandleMothCaught;
        }

        private void OnDisable()
        {
            GameEvents.OnMothCaught -= HandleMothCaught;
        }

        private void HandleMothCaught(MothGenome caughtMoth)
        {
            // Remove the caught moth so it doesn't reproduce
            if (CurrentGeneration.Contains(caughtMoth))
            {
                CurrentGeneration.Remove(caughtMoth);
            }
        }

        public void InitializeStartingPopulation()
        {
            CurrentGeneration.Clear();
            CurrentGenerationCount = 1;
            
            if (config == null)
            {
                Debug.LogError("NightConfig is missing on MothPopulationManager!");
                return;
            }
            
            // Generate basic random moths for Gen 1
            for (int i = 0; i < config.mothPopulation; i++)
            {
                CurrentGeneration.Add(new MothGenome {
                    speed = Random.value,
                    camouflage = Random.value,
                    lightAttraction = Random.value
                });
            }
        }

        public void SpawnCurrentGeneration()
        {
            // Determine which spawner to use based on config
            if (config != null)
            {
                if (config.spawnMode == SpawnMode.Fixed)
                    _currentSpawner = GetComponent<FixedPointSpawner>();
                else
                    _currentSpawner = GetComponent<RandomAreaSpawner>();
            }
            else
            {
                _currentSpawner = GetComponent<IMothSpawner>();
            }
            
            if (_currentSpawner != null)
            {
                _currentSpawner.SpawnMoths(CurrentGeneration, mothPrefab);
            }
            else
            {
                Debug.LogError($"[MothPopulationManager] Required Spawner for mode {config?.spawnMode} is missing from this GameObject!");
            }
        }

        public void ProcessReproduction(List<MothGenome> survivors)
        {
            if (_reproductionEngine == null) return;

            var rng = new System.Random();
            
            // Save a copy of the old generation for the event
            var oldGen = new List<MothGenome>(CurrentGeneration);
            
            // Generate the new generation
            CurrentGeneration = _reproductionEngine.Reproduce(survivors, config.mothPopulation, rng);
            CurrentGenerationCount++;
            
            // Fire the event if anything needs to know (UI, stats tracking, etc.)
            GameEvents.RaiseGenerationComplete(oldGen, CurrentGeneration);
        }
        
        public void ClearMoths()
        {
            if (_currentSpawner != null)
            {
                _currentSpawner.ClearMoths();
            }
        }
    }
}
