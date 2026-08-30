using UnityEngine;
using UnityEngine.SceneManagement;
using Mothropolis.Night;
using Mothropolis.Economy;

namespace Mothropolis.Core
{
    public enum GameState { Intro, Hunting, Report, Reproduce }

    public class GameLoopManager : MonoBehaviour
    {
        public static GameLoopManager Instance { get; private set; }
        public static int CurrentNightIndex = 0; // 0 for Night 1, 1 for Night 2, etc.

        public GameState CurrentState { get; private set; } = GameState.Intro;

        [Header("Campaign Progression")]
        public NightConfig[] nightConfigs;

        [Header("References")]
        public NightManager nightManager;
        public MothPopulationManager populationManager;
        
        [Header("Player Tracking")]
        public Transform playerTransform;
        public Transform startPosition; // Where the player spawns at the start of a night

        private void Awake()
        {
            if (Instance != null && Instance != this) 
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Auto-locate references if not assigned
            if (nightManager == null) nightManager = FindFirstObjectByType<NightManager>();
            if (populationManager == null) populationManager = FindFirstObjectByType<MothPopulationManager>();
            if (playerTransform == null) 
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }
            if (startPosition == null)
            {
                var startObj = GameObject.Find("StartPosition");
                if (startObj != null) startPosition = startObj.transform;
            }

            // Sync current night config based on CurrentNightIndex
            ApplyCurrentNightConfig();

            // Initialize or carry over population
            if (populationManager != null)
            {
                populationManager.InitializeStartingPopulation();
            }

            TransitionTo(GameState.Intro);
        }

        private void ApplyCurrentNightConfig()
        {
            if (nightConfigs != null && nightConfigs.Length > 0)
            {
                int index = Mathf.Clamp(CurrentNightIndex, 0, nightConfigs.Length - 1);
                var config = nightConfigs[index];
                if (config != null)
                {
                    if (populationManager != null)
                    {
                        populationManager.config = config;
                    }
                    Debug.Log($"[GameLoopManager] Active Config: {config.nightLabel} (Scene: {config.sceneName}, Pop: {config.mothPopulation}, Aggression: {config.owlAggressionMultiplier})");
                }
            }
        }

        private void OnEnable()
        {
            GameEvents.OnDawnReached += HandleNightEnded;
            GameEvents.OnFoodBanked += HandleFoodBanked;
        }

        private void OnDisable()
        {
            GameEvents.OnDawnReached -= HandleNightEnded;
            GameEvents.OnFoodBanked -= HandleFoodBanked;
        }

        public void TransitionTo(GameState newState)
        {
            CurrentState = newState;
            Debug.Log($"[GameLoopManager] Transitioned to {newState}");

            switch (newState)
            {
                case GameState.Intro:
                    // Reset player to the start of the level
                    if (playerTransform != null && startPosition != null)
                    {
                        playerTransform.position = startPosition.position;
                    }
                    
                    // Spawn the current generation of moths
                    if (populationManager != null)
                    {
                        populationManager.ClearMoths();
                        populationManager.SpawnCurrentGeneration();
                    }
                    
                    TransitionTo(GameState.Hunting);
                    break;

                case GameState.Hunting:
                    if (nightManager != null)
                    {
                        nightManager.StartNight();
                    }
                    break;

                case GameState.Report:
                    // Handled automatically by EvolutionReportUI
                    break;

                case GameState.Reproduce:
                    // Triggered by the UI Continue button
                    if (populationManager != null)
                    {
                        // The moths that are still in CurrentGeneration are the survivors
                        populationManager.ProcessReproduction(populationManager.CurrentGeneration);
                    }
                    
                    // Clear carried food for next night
                    var foodBank = GameServices.Get<FoodBank>();
                    if (foodBank != null)
                    {
                        foodBank.LoseCarriedFood();
                    }

                    // Advance Campaign Night Sequence
                    AdvanceToNextNight();
                    break;
            }
        }

        private void AdvanceToNextNight()
        {
            CurrentNightIndex++;

            if (nightConfigs != null && CurrentNightIndex < nightConfigs.Length)
            {
                var nextConfig = nightConfigs[CurrentNightIndex];
                string targetScene = !string.IsNullOrEmpty(nextConfig.sceneName) ? nextConfig.sceneName : "Level1";
                string currentScene = SceneManager.GetActiveScene().name;

                Debug.Log($"[GameLoopManager] Advancing to Night {CurrentNightIndex + 1} ({nextConfig.nightLabel}) -> Target Scene: {targetScene} (Current Scene: {currentScene})");

                if (targetScene != currentScene)
                {
                    // Load the designated scene for the next night
                    SceneManager.LoadScene(targetScene);
                }
                else
                {
                    // Same scene reused (e.g. Night 4 -> Level2 or Night 5 -> Level1): Apply new night config and restart loop in place
                    ApplyCurrentNightConfig();
                    TransitionTo(GameState.Intro);
                }
            }
            else
            {
                Debug.Log("[GameLoopManager] ALL 5 NIGHTS COMPLETE! Moth evolution campaign finished successfully.");
                CurrentNightIndex = 0;
                ApplyCurrentNightConfig();
                TransitionTo(GameState.Intro);
            }
        }

        private void HandleNightEnded()
        {
            if (CurrentState == GameState.Hunting)
            {
                TransitionTo(GameState.Report);
            }
        }

        private void HandleFoodBanked(int amount)
        {
            if (CurrentState == GameState.Hunting)
            {
                TransitionTo(GameState.Report);
            }
        }

        public static void ResetCampaign()
        {
            CurrentNightIndex = 0;
            MothPopulationManager.ResetCampaign();
        }
    }
}
