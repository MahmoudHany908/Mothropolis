using UnityEngine;
using Mothropolis.Core;
using Mothropolis.Genetics;

namespace Mothropolis.Economy
{
    public class FoodBank : MonoBehaviour
    {
        public int carriedFood { get; private set; }
        public int bankedFood { get; private set; }

        private void OnEnable()
        {
            GameServices.Register(this);
            GameEvents.OnMothCaught += HandleMothCaught;
            GameEvents.OnNightStarted += HandleNightStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnMothCaught -= HandleMothCaught;
            GameEvents.OnNightStarted -= HandleNightStarted;
        }

        private void HandleNightStarted()
        {
            carriedFood = 0;
            GameEvents.RaiseCarriedFoodChanged(0);
        }

        private void HandleMothCaught(MothGenome genome)
        {
            carriedFood++;
            Debug.Log($"Caught moth! Carried Food: {carriedFood}");
            GameEvents.RaiseCarriedFoodChanged(carriedFood);
        }

        public void DepositCarriedFood()
        {
            int deposited = carriedFood;
            bankedFood += carriedFood;
            carriedFood = 0;
            GameEvents.RaiseCarriedFoodChanged(0);
            
            Debug.Log($"Banked {deposited} food! Total Banked: {bankedFood}");
            
            // This triggers the NightManager to end the night safely
            GameEvents.RaiseFoodBanked(bankedFood);
        }

        public void LoseCarriedFood()
        {
            Debug.Log($"Lost {carriedFood} carried food due to dawn/knockout.");
            carriedFood = 0;
            GameEvents.RaiseCarriedFoodChanged(0);
        }
    }
}
