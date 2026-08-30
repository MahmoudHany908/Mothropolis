using UnityEngine;
using TMPro; // Assuming TextMeshPro is being used for text
using Mothropolis.Core;
using Mothropolis.Economy;

namespace Mothropolis.UI
{
    public class EvolutionReportUI : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject reportPanel;
        
        [Header("Text Elements")]
        public TMP_Text statusTitleText; // "Survived!" or "Night Failed"
        public TMP_Text foodBankedText;

        [Header("Buttons")]
        public UnityEngine.UI.Button continueButton;

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = true;
        }

        private void OnEnable()
        {
            GameEvents.OnFoodBanked += HandleFoodBanked;
            GameEvents.OnDawnReached += HandleDawnReached;
        }

        private void OnDisable()
        {
            GameEvents.OnFoodBanked -= HandleFoodBanked;
            GameEvents.OnDawnReached -= HandleDawnReached;
        }

        private void Start()
        {
            if (reportPanel != null)
            {
                reportPanel.SetActive(false);
            }

            if (continueButton == null && reportPanel != null)
            {
                continueButton = reportPanel.GetComponentInChildren<UnityEngine.UI.Button>(true);
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void HandleFoodBanked(int totalBanked)
        {
            ShowReport("Night Survived!", $"Food Banked: {totalBanked}");
        }

        private void HandleDawnReached()
        {
            ShowReport("Night Failed", "Food Banked: 0 (Lost due to dawn or owl)");
        }

        private void ShowReport(string title, string details)
        {
            if (reportPanel != null)
            {
                reportPanel.SetActive(true);
            }

            if (statusTitleText != null) statusTitleText.text = title;
            if (foodBankedText != null) foodBankedText.text = details;

            // Pause the game while the report is open
            Time.timeScale = 0f;
        }

        // Hook this up to a UI Button OnClick event in the inspector
        public void OnContinueClicked()
        {
            Time.timeScale = 1f;
            if (reportPanel != null)
            {
                reportPanel.SetActive(false);
            }
            
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.TransitionTo(GameState.Reproduce);
            }
            else
            {
                Debug.LogWarning("No GameLoopManager found to trigger next generation!");
            }
        }
    }
}
