using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Mothropolis.Core;
using Mothropolis.Economy;
using Mothropolis.Audio;

namespace Mothropolis.Night
{
    public class ReturnDrain : MonoBehaviour
    {
        [Header("Floating Prompt Settings")]
        public Vector3 promptOffset = new Vector3(0f, 1.2f, 0f);
        
        private bool _playerInRange = false;
        private GameObject _promptContainer;
        private TMP_Text _promptText;
        private Coroutine _rejectAnimationRoutine;

        private void Awake()
        {
            CreateWorldSpacePrompt();
        }

        private void CreateWorldSpacePrompt()
        {
            // Build a clean, self-contained world space floating prompt
            _promptContainer = new GameObject("DrainPrompt");
            _promptContainer.transform.SetParent(transform);
            _promptContainer.transform.localPosition = promptOffset;

            var canvas = _promptContainer.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = _promptContainer.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(4f, 1f);

            var textObj = new GameObject("PromptText");
            textObj.transform.SetParent(_promptContainer.transform);
            textObj.transform.localPosition = Vector3.zero;

            _promptText = textObj.AddComponent<TextMeshPro>();
            _promptText.alignment = TextAlignmentOptions.Center;
            _promptText.fontSize = 4.5f;
            _promptText.color = Color.white;
            _promptText.text = "";

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(4f, 1f);

            _promptContainer.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Player.PlayerController>() != null || other.CompareTag("Player"))
            {
                _playerInRange = true;
                UpdatePromptVisual();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<Player.PlayerController>() != null || other.CompareTag("Player"))
            {
                _playerInRange = false;
                if (_promptContainer != null) _promptContainer.SetActive(false);
            }
        }

        private void UpdatePromptVisual()
        {
            if (!_playerInRange || _promptContainer == null || _promptText == null) return;

            _promptContainer.SetActive(true);
            var foodBank = GameServices.Get<FoodBank>();
            int carried = foodBank != null ? foodBank.carriedFood : 0;

            if (carried > 0)
            {
                _promptText.text = $"<color=#81C784><b>[S / ▼]</b> Bank Food (+{carried})</color>";
            }
            else
            {
                _promptText.text = "<color=#B0BEC5>[S / ▼] Need Moths</color>";
            }
        }

        private void Update()
        {
            if (!_playerInRange) return;

            // Keep prompt updated if player eats a moth while standing near drain
            UpdatePromptVisual();

            if (Keyboard.current != null && (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame))
            {
                if (GameLoopManager.Instance != null && GameLoopManager.Instance.CurrentState != GameState.Hunting) return;
                
                var foodBank = GameServices.Get<FoodBank>();
                if (foodBank == null) return;

                if (foodBank.carriedFood > 0)
                {
                    foodBank.DepositCarriedFood();
                    if (_promptContainer != null) _promptContainer.SetActive(false);
                }
                else
                {
                    // Visual and Audio feedback for empty banking attempt
                    if (_rejectAnimationRoutine != null) StopCoroutine(_rejectAnimationRoutine);
                    _rejectAnimationRoutine = StartCoroutine(RejectFeedbackRoutine());
                }
            }
        }

        private IEnumerator RejectFeedbackRoutine()
        {
            if (_promptText != null)
            {
                _promptText.text = "<color=#EF5350><b>Catch moths first!</b></color>";
            }

            // Punch scale
            if (_promptContainer != null)
            {
                Vector3 originalScale = Vector3.one;
                for (float t = 0; t < 0.25f; t += Time.deltaTime)
                {
                    float punch = 1f + Mathf.Sin(t * Mathf.PI / 0.25f) * 0.25f;
                    _promptContainer.transform.localScale = originalScale * punch;
                    yield return null;
                }
                _promptContainer.transform.localScale = originalScale;
            }

            yield return new WaitForSeconds(0.4f);
            UpdatePromptVisual();
            _rejectAnimationRoutine = null;
        }
    }
}
