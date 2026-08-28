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
        }

        private void OnDisable()
        {
            GameEvents.OnMothCaught -= HandleMothCaught;
        }

        private void HandleMothCaught(MothGenome genome)
        {
            carriedFood++;
            Debug.Log($"Caught moth! Carried Food: {carriedFood}");
        }

        public void DepositCarriedFood()
        {
            int deposited = carriedFood;
            bankedFood += carriedFood;
            carriedFood = 0;
            
            Debug.Log($"Banked {deposited} food! Total Banked: {bankedFood}");
            
            // This triggers the NightManager to end the night safely
            GameEvents.RaiseFoodBanked(bankedFood);
        }

        public void LoseCarriedFood()
        {
            Debug.Log($"Lost {carriedFood} carried food due to dawn/knockout.");
            carriedFood = 0;
        }
    }
}
