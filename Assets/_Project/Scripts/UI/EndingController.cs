using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Mothropolis.Core;
using Mothropolis.Economy;

namespace Mothropolis.UI
{
    public class EndingController : MonoBehaviour
    {
        public static EndingController Instance { get; private set; }

        [Header("Ending Canvas / Panel")]
        public GameObject endingPanel;
        public TMP_Text outcomeTitleText;
        public TMP_Text tierBadgeText;
        public TMP_Text totalFoodText;
        public TMP_Text narrativeBodyText;
        public Button mainMenuButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildEndingUI();
        }

        private void BuildEndingUI()
        {
            if (endingPanel != null) return;

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9998;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            // Fullscreen Dark Backdrop
            endingPanel = new GameObject("EndingPanel");
            endingPanel.transform.SetParent(transform, false);
            var panelRect = endingPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            var bgImg = endingPanel.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.05f, 0.08f, 0.98f);

            // Container Card
            var card = new GameObject("Card");
            card.transform.SetParent(endingPanel.transform, false);
            var cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.2f, 0.1f);
            cardRect.anchorMax = new Vector2(0.8f, 0.9f);
            cardRect.sizeDelta = Vector2.zero;

            // Title
            var titleObj = new GameObject("OutcomeTitle");
            titleObj.transform.SetParent(card.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.8f);
            titleRect.anchorMax = new Vector2(1f, 0.98f);
            titleRect.sizeDelta = Vector2.zero;
            outcomeTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            outcomeTitleText.alignment = TextAlignmentOptions.Center;
            outcomeTitleText.fontSize = 50f;
            outcomeTitleText.fontStyle = FontStyles.Bold;

            // Tier Badge
            var badgeObj = new GameObject("TierBadge");
            badgeObj.transform.SetParent(card.transform, false);
            var badgeRect = badgeObj.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0f, 0.72f);
            badgeRect.anchorMax = new Vector2(1f, 0.8f);
            badgeRect.sizeDelta = Vector2.zero;
            tierBadgeText = badgeObj.AddComponent<TextMeshProUGUI>();
            tierBadgeText.alignment = TextAlignmentOptions.Center;
            tierBadgeText.fontSize = 26f;
            tierBadgeText.fontStyle = FontStyles.Bold;

            // Food Tally
            var foodObj = new GameObject("TotalFood");
            foodObj.transform.SetParent(card.transform, false);
            var foodRect = foodObj.AddComponent<RectTransform>();
            foodRect.anchorMin = new Vector2(0f, 0.62f);
            foodRect.anchorMax = new Vector2(1f, 0.72f);
            foodRect.sizeDelta = Vector2.zero;
            totalFoodText = foodObj.AddComponent<TextMeshProUGUI>();
            totalFoodText.alignment = TextAlignmentOptions.Center;
            totalFoodText.fontSize = 32f;
            totalFoodText.color = new Color(1f, 0.85f, 0.4f);

            // Narrative Body
            var bodyObj = new GameObject("NarrativeBody");
            bodyObj.transform.SetParent(card.transform, false);
            var bodyRect = bodyObj.AddComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.05f, 0.22f);
            bodyRect.anchorMax = new Vector2(0.95f, 0.60f);
            bodyRect.sizeDelta = Vector2.zero;
            narrativeBodyText = bodyObj.AddComponent<TextMeshProUGUI>();
            narrativeBodyText.alignment = TextAlignmentOptions.Center;
            narrativeBodyText.fontSize = 24f;
            narrativeBodyText.enableWordWrapping = true;
            narrativeBodyText.color = new Color(0.85f, 0.9f, 0.92f);

            // Main Menu Button
            var btnObj = new GameObject("MainMenuBtn");
            btnObj.transform.SetParent(card.transform, false);
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.35f, 0.05f);
            btnRect.anchorMax = new Vector2(0.65f, 0.18f);
            btnRect.sizeDelta = Vector2.zero;

            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.18f, 0.55f, 0.35f, 1f);
            mainMenuButton = btnObj.AddComponent<Button>();

            var btnTextObj = new GameObject("BtnText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            var btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            var btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSize = 26f;
            btnText.fontStyle = FontStyles.Bold;
            btnText.text = "RETURN TO MENU";
            btnText.color = Color.white;

            mainMenuButton.onClick.AddListener(OnReturnToMenuClicked);

            endingPanel.SetActive(false);
        }

        public static void ShowEnding(int totalFood)
        {
            if (Instance == null)
            {
                var obj = new GameObject("EndingController");
                Instance = obj.AddComponent<EndingController>();
            }

            Instance.DisplayEnding(totalFood);
        }

        public void DisplayEnding(int totalFood)
        {
            Time.timeScale = 0f;
            endingPanel.SetActive(true);

            totalFoodText.text = $"Total Campaign Harvest: <b>{totalFood} Food</b>";

            if (totalFood >= 60)
            {
                // Tier 3: Thriving
                outcomeTitleText.text = "APEX OF MOTHROPOLIS";
                outcomeTitleText.color = new Color(0.4f, 1f, 0.5f);
                tierBadgeText.text = "★ ★ ★  THRIVING COLONY  ★ ★ ★";
                tierBadgeText.color = new Color(0.4f, 1f, 0.5f);
                narrativeBodyText.text = "A masterclass in urban evolution.\n\nYour drain overflows with an abundant winter harvest, and the colony thrives with unmatched genetic vigor. The nocturnal cityscape now belongs to the masters of the shadows.";
            }
            else if (totalFood >= 30)
            {
                // Tier 2: Surviving
                outcomeTitleText.text = "A FRAGILE FOOTHOLD";
                outcomeTitleText.color = new Color(0.95f, 0.85f, 0.35f);
                tierBadgeText.text = "★ ★  SURVIVING COLONY  ★ ★";
                tierBadgeText.color = new Color(0.95f, 0.85f, 0.35f);
                narrativeBodyText.text = "Through cunning hops and calculated risks, enough sustenance was hoarded to endure.\n\nThe colony survives, adapted to the neon concrete jungle, watchful of the predator above as cold winds sweep through the alleys.";
            }
            else
            {
                // Tier 1: Starving
                outcomeTitleText.text = "THE METROPOLIS WITHERS";
                outcomeTitleText.color = new Color(1f, 0.35f, 0.35f);
                tierBadgeText.text = "★  STARVING COLONY  ★";
                tierBadgeText.color = new Color(1f, 0.35f, 0.35f);
                narrativeBodyText.text = "The harsh urban nights proved too unyielding.\n\nWith only meager rations stored in the drain, the colony faces an agonizing winter in the cold shadows. The city lights slowly fade into silence.";
            }
        }

        private void OnReturnToMenuClicked()
        {
            Time.timeScale = 1f;
            endingPanel.SetActive(false);
            GameLoopManager.ResetCampaign();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
