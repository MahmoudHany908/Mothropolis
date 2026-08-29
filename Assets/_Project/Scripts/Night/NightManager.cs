using UnityEngine;
using Mothropolis.Core;
using Mothropolis.Economy;

namespace Mothropolis.Night
{
    public class NightManager : MonoBehaviour
    {
        [Header("Settings")]
        public float nightDuration = 60f;
        
        [Header("State (Read Only)")]
        [SerializeField] private float _timeRemaining;
        [SerializeField] private bool _isHunting;

        private void Start()
        {
            // For Day 1 testing, auto-start the night
            StartNight();
        }

        private void OnEnable()
        {
            GameEvents.OnFoodBanked += HandleFoodBanked;
        }

        private void OnDisable()
        {
            GameEvents.OnFoodBanked -= HandleFoodBanked;
        }

        public void StartNight()
        {
            _timeRemaining = nightDuration;
            _isHunting = true;
            Debug.Log($"Night started. Time remaining: {_timeRemaining}s");
        }

        private void Update()
        {
            if (!_isHunting) return;

            _timeRemaining -= Time.deltaTime;
            
            if (_timeRemaining <= 0)
            {
                FailNight();
                Debug.Log("Dawn reached! You stayed out too late.");
            }
        }

        public void FailNight()
        {
            if (!_isHunting) return;
            
            _timeRemaining = 0;
            _isHunting = false;
            
            // Dawn reached or Owl caught! Food is lost.
            var foodBank = GameServices.Get<FoodBank>();
            if (foodBank != null) 
            {
                foodBank.LoseCarriedFood();
            }
            
            GameEvents.RaiseDawnReached();
            Debug.Log("Night failed.");
        }

        private void HandleFoodBanked(int totalBanked)
        {
            if (!_isHunting) return; // Prevent double-triggering
            
            _isHunting = false;
            Debug.Log("Player returned to drain. Night ended safely. Awaiting Report State...");
            
            // Future sub-phase: Trigger UI Report and ReproductionEngine here
        }
    }
}
