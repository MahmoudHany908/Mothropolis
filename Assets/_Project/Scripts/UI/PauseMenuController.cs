using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using Mothropolis.Core;
using Mothropolis.Audio;

namespace Mothropolis.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }

        [Header("Pause UI Elements")]
        public GameObject pausePanel;
        public Button resumeButton;
        public Button restartNightButton;
        public Button mainMenuButton;
        public Slider masterVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider musicVolumeSlider;

        public bool IsPaused { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildPauseUI();
        }

        private void BuildPauseUI()
        {
            if (pausePanel != null) return;

            // Check if existing canvas is present or build world/screen space overlay
            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9995;

                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                gameObject.AddComponent<GraphicRaycaster>();
            }

            // Dark Backdrop
            pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(transform, false);
            var panelRect = pausePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            var bg = pausePanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.06f, 0.09f, 0.94f);

            // Container Card
            var card = new GameObject("Card");
            card.transform.SetParent(pausePanel.transform, false);
            var cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.35f, 0.15f);
            cardRect.anchorMax = new Vector2(0.65f, 0.85f);
            cardRect.sizeDelta = Vector2.zero;

            var cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.1f, 0.12f, 0.16f, 0.98f);

            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(card.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.85f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.sizeDelta = Vector2.zero;
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 38f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.text = "PAUSED";
            titleText.color = new Color(1f, 0.95f, 0.85f);

            // Buttons Container
            float btnY = 0.72f;
            float btnHeight = 0.10f;
            float spacing = 0.03f;

            resumeButton = CreateButton(card.transform, "ResumeBtn", "RESUME", new Vector2(0.1f, btnY - btnHeight), new Vector2(0.9f, btnY), new Color(0.18f, 0.55f, 0.35f), OnResumeClicked);
            btnY -= (btnHeight + spacing);

            restartNightButton = CreateButton(card.transform, "RestartBtn", "RESTART NIGHT", new Vector2(0.1f, btnY - btnHeight), new Vector2(0.9f, btnY), new Color(0.35f, 0.40f, 0.50f), OnRestartNightClicked);
            btnY -= (btnHeight + spacing);

            mainMenuButton = CreateButton(card.transform, "MainMenuBtn", "MAIN MENU", new Vector2(0.1f, btnY - btnHeight), new Vector2(0.9f, btnY), new Color(0.55f, 0.25f, 0.25f), OnMainMenuClicked);
            btnY -= (btnHeight + spacing + 0.04f);

            // Volume Label & Slider
            var volLabelObj = new GameObject("VolLabel");
            volLabelObj.transform.SetParent(card.transform, false);
            var volLabelRect = volLabelObj.AddComponent<RectTransform>();
            volLabelRect.anchorMin = new Vector2(0.1f, btnY - 0.05f);
            volLabelRect.anchorMax = new Vector2(0.9f, btnY);
            volLabelRect.sizeDelta = Vector2.zero;
            var volText = volLabelObj.AddComponent<TextMeshProUGUI>();
            volText.alignment = TextAlignmentOptions.Center;
            volText.fontSize = 18f;
            volText.text = "Master Volume";
            volText.color = new Color(0.75f, 0.80f, 0.85f);

            masterVolumeSlider = CreateSlider(card.transform, "MasterVolSlider", new Vector2(0.1f, btnY - 0.12f), new Vector2(0.9f, btnY - 0.05f));

            pausePanel.SetActive(false);
        }

        private Button CreateButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;

            var img = obj.AddComponent<Image>();
            img.color = bgColor;

            var btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            var t = textObj.AddComponent<TextMeshProUGUI>();
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = 22f;
            t.fontStyle = FontStyles.Bold;
            t.text = text;
            t.color = Color.white;

            return btn;
        }

        private Slider CreateSlider(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;

            var slider = obj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = AudioListener.volume;
            slider.onValueChanged.AddListener(val => AudioListener.volume = val);

            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

            return slider;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // Toggle Pause
                if (IsPaused) ResumeGame();
                else PauseGame();
            }
            else if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            {
                if (IsPaused) ResumeGame();
                else PauseGame();
            }
        }

        public void PauseGame()
        {
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.CurrentState == GameState.Report) return;

            IsPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);

            if (resumeButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            }
        }

        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void OnResumeClicked()
        {
            AudioService.PlayCatchMoth();
            ResumeGame();
        }

        public void OnRestartNightClicked()
        {
            AudioService.PlayBankFood();
            ResumeGame();
            string sceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneName);
        }

        public void OnMainMenuClicked()
        {
            AudioService.PlayBankFood();
            ResumeGame();
            GameLoopManager.ResetCampaign();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
