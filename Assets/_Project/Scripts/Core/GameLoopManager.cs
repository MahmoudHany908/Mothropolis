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

            EnsureNightConfigs();
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

            EnsureNightConfigs();

            // Sync current night config based on CurrentNightIndex
            ApplyCurrentNightConfig();

            // Initialize or carry over population
            if (populationManager != null)
            {
                populationManager.InitializeStartingPopulation();
            }

            TransitionTo(GameState.Intro);
        }

        private void EnsureNightConfigs()
        {
            bool needsPopulate = (nightConfigs == null || nightConfigs.Length < 5);
            if (!needsPopulate)
            {
                for (int i = 0; i < nightConfigs.Length; i++)
                {
                    if (nightConfigs[i] == null) { needsPopulate = true; break; }
                }
            }

            if (needsPopulate)
            {
                nightConfigs = new NightConfig[5];
                nightConfigs[0] = Resources.Load<NightConfig>("NightConfigs/Night1_Day");
                nightConfigs[1] = Resources.Load<NightConfig>("NightConfigs/Night2_Day");
                nightConfigs[2] = Resources.Load<NightConfig>("NightConfigs/Night3_Day");
                nightConfigs[3] = Resources.Load<NightConfig>("NightConfigs/Night4_Day");
                nightConfigs[4] = Resources.Load<NightConfig>("NightConfigs/Night5_Day");

                // If any are still null, construct fallback runtime instances
                for (int i = 0; i < 5; i++)
                {
                    if (nightConfigs[i] == null)
                    {
                        nightConfigs[i] = ScriptableObject.CreateInstance<NightConfig>();
                        nightConfigs[i].nightLabel = $"Night {i + 1}";
                        nightConfigs[i].sceneName = GetDefaultSceneForNight(i);
                        nightConfigs[i].mothPopulation = 24;
                        nightConfigs[i].spawnMode = SpawnMode.Random;
                        nightConfigs[i].owlAggressionMultiplier = 1.0f + (i * 0.2f);
                    }
                }
            }
        }

        public string GetDefaultSceneForNight(int index)
        {
            switch (index)
            {
                case 0: return "Level1";
                case 1: return "Level2";
                case 2: return "Level3";
                case 3: return "Level2";
                case 4: return "Level1";
                default: return "Level1";
            }
        }

        private void ApplyCurrentNightConfig()
        {
            EnsureNightConfigs();

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
                    // Unpause time
                    Time.timeScale = 1f;

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
            EnsureNightConfigs();

            CurrentNightIndex++;

            Time.timeScale = 1f;

            if (CurrentNightIndex < nightConfigs.Length)
            {
                var nextConfig = nightConfigs[CurrentNightIndex];
                string targetScene = (nextConfig != null && !string.IsNullOrEmpty(nextConfig.sceneName))
                    ? nextConfig.sceneName
                    : GetDefaultSceneForNight(CurrentNightIndex);

                string currentScene = SceneManager.GetActiveScene().name;

                Debug.Log($"[GameLoopManager] Advancing to Night {CurrentNightIndex + 1} ({nextConfig?.nightLabel}) -> Target Scene: {targetScene} (Current Scene: {currentScene})");

                if (targetScene != currentScene)
                {
                    // Load the designated scene for the next night
                    SceneManager.LoadScene(targetScene);
                }
                else
                {
                    // Same scene reused: Apply new night config and restart loop in place
                    ApplyCurrentNightConfig();
                    TransitionTo(GameState.Intro);
                }
            }
            else
            {
                Debug.Log("[GameLoopManager] ALL 5 NIGHTS COMPLETE! Moth evolution campaign finished successfully.");
                CurrentNightIndex = 0;
                MothPopulationManager.ResetCampaign();
                SceneManager.LoadScene("MainMenu");
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
