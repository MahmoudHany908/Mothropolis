using UnityEngine;
using Mothropolis.Night;
using Mothropolis.Economy;

namespace Mothropolis.Core
{
    public enum GameState { Intro, Hunting, Report, Reproduce }

    public class GameLoopManager : MonoBehaviour
    {
        public static GameLoopManager Instance { get; private set; }
        public GameState CurrentState { get; private set; } = GameState.Intro;

        [Header("References")]
        public NightManager nightManager;
        public MothPopulationManager populationManager;
        
        [Header("Player Tracking")]
        public Transform playerTransform;
        public Transform startPosition; // Where the player spawns at the start of a night

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;
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

            // Start Gen 1
            if (populationManager != null)
            {
                populationManager.InitializeStartingPopulation();
            }

            TransitionTo(GameState.Intro);
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
                    
                    // Right now we immediately jump to hunting. 
                    // In a future sub-phase, we could wait for a UI animation ("Generation X") to finish.
                    TransitionTo(GameState.Hunting);
                    break;

                case GameState.Hunting:
                    if (nightManager != null)
                    {
                        nightManager.StartNight();
                    }
                    break;

                case GameState.Report:
                    // Handled automatically by EvolutionReportUI which listens to the same events.
                    break;

                case GameState.Reproduce:
                    // Triggered by the UI Continue button
                    if (populationManager != null)
                    {
                        // The moths that are still in CurrentGeneration are the survivors
                        populationManager.ProcessReproduction(populationManager.CurrentGeneration);
                    }
                    
                    // Clear the bank for the next night
                    var foodBank = GameServices.Get<FoodBank>();
                    if (foodBank != null)
                    {
                        foodBank.LoseCarriedFood(); // Just in case, though it should be 0
                    }

                    // Loop back to spawn the next generation
                    TransitionTo(GameState.Intro);
                    break;
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
    }
}
