using UnityEngine;
using UnityEngine.InputSystem;
using Mothropolis.Core;
using Mothropolis.Economy;

namespace Mothropolis.Night
{
    public class ReturnDrain : MonoBehaviour
    {
        private bool _playerInRange = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Player.PlayerController>() != null || other.CompareTag("Player"))
            {
                _playerInRange = true;
                Debug.Log("At the drain. Press DOWN (or S) to return home and bank food.");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<Player.PlayerController>() != null || other.CompareTag("Player"))
            {
                _playerInRange = false;
            }
        }

        private void Update()
        {
            if (!_playerInRange) return;

            // Update runs every frame regardless of physics sleep state.
            // wasPressedThisFrame is perfect here so it fires exactly once per press.
            if (Keyboard.current != null && (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame))
            {
                var foodBank = GameServices.Get<FoodBank>();
                if (foodBank == null) return;

                if (foodBank.carriedFood > 0)
                {
                    foodBank.DepositCarriedFood();
                }
                else
                {
                    Debug.Log("Nothing to bank — catch some moths first!");
                }
            }
        }
    }
}
