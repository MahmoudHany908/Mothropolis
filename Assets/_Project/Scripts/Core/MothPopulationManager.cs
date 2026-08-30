using System.Collections.Generic;
using UnityEngine;
using Mothropolis.Genetics;
using Mothropolis.Moths;
using Mothropolis.Night;

namespace Mothropolis.Core
{
    public class MothPopulationManager : MonoBehaviour
    {
        public static List<MothGenome> PersistentGeneration { get; set; } = null;
        public static int PersistentGenerationIndex { get; set; } = 1;

        public NightConfig config;
        public MutationSettings mutationSettings;
        public GameObject mothPrefab;
        
        public int CurrentGenerationCount 
        { 
            get => PersistentGenerationIndex; 
            private set => PersistentGenerationIndex = value; 
        }

        public List<MothGenome> CurrentGeneration 
        { 
            get => PersistentGeneration; 
            private set => PersistentGeneration = value; 
        }
        
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
            if (CurrentGeneration != null && CurrentGeneration.Contains(caughtMoth))
            {
                CurrentGeneration.Remove(caughtMoth);
            }
        }

        public void InitializeStartingPopulation()
        {
            // If we already have an active persistent generation carried over from previous night/scene, DO NOT wipe it!
            if (PersistentGeneration != null && PersistentGeneration.Count > 0)
            {
                Debug.Log($"[MothPopulationManager] Carrying over Gen {PersistentGenerationIndex} ({PersistentGeneration.Count} moths) into scene.");
                return;
            }

            PersistentGeneration = new List<MothGenome>();
            PersistentGenerationIndex = 1;
            
            int count = config != null ? config.mothPopulation : 24;
            
            // Generate basic random moths for Gen 1
            for (int i = 0; i < count; i++)
            {
                PersistentGeneration.Add(new MothGenome {
                    speed = Random.value,
                    camouflage = Random.value,
                    lightAttraction = Random.value
                });
            }
            Debug.Log($"[MothPopulationManager] Initialized fresh Gen 1 population with {count} moths.");
        }

        public void SpawnCurrentGeneration()
        {
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
            
            if (_currentSpawner != null && CurrentGeneration != null)
            {
                _currentSpawner.SpawnMoths(CurrentGeneration, mothPrefab);
            }
            else
            {
                Debug.LogError($"[MothPopulationManager] Required Spawner or Generation is missing!");
            }
        }

        public void ProcessReproduction(List<MothGenome> survivors)
        {
            if (_reproductionEngine == null)
            {
                if (mutationSettings != null) _reproductionEngine = new ReproductionEngine(mutationSettings);
                else _reproductionEngine = new ReproductionEngine(ScriptableObject.CreateInstance<MutationSettings>());
            }

            var rng = new System.Random();
            
            // Save a copy of the old generation for the event
            var oldGen = new List<MothGenome>(CurrentGeneration != null ? CurrentGeneration : new List<MothGenome>());
            
            int popCount = config != null ? config.mothPopulation : 24;
            // Generate the new generation from the real survivors
            PersistentGeneration = _reproductionEngine.Reproduce(survivors, popCount, rng);
            PersistentGenerationIndex++;
            
            Debug.Log($"[MothPopulationManager] Reproduced into Gen {PersistentGenerationIndex} with {PersistentGeneration.Count} moths from {survivors.Count} survivors.");

            // Fire the event if anything needs to know (UI, stats tracking, etc.)
            GameEvents.RaiseGenerationComplete(oldGen, PersistentGeneration);
        }
        
        public void ClearMoths()
        {
            if (_currentSpawner != null)
            {
                _currentSpawner.ClearMoths();
            }
        }

        public static void ResetCampaign()
        {
            PersistentGeneration = null;
            PersistentGenerationIndex = 1;
        }
    }
}
