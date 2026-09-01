using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Mothropolis.Core;
using Mothropolis.Audio;

namespace Mothropolis.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [System.Serializable]
        public class SlideItem
        {
            public GameObject visual;
            public GameObject text;
        }

        [Header("Canvases / Panels")]
        public Canvas mainMenuCanvas;
        public Canvas slidesCanvas;

        [Header("Menu Buttons")]
        public Button playButton;
        public Button exitButton;

        [Header("Slideshow Controls")]
        public Button nextButton;
        public Button previousButton;
        public Button skipIntroButton;

        [Header("Slides Configuration")]
        public List<SlideItem> slides = new List<SlideItem>();
        public string firstLevelSceneName = "Level1";

        private int _currentSlideIndex = 0;

        private void Awake()
        {
            EnsureCanvasScalers();
            EnsureDirectReferences();
        }

        private void EnsureCanvasScalers()
        {
            var scalers = FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);
            foreach (var scaler in scalers)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        private void EnsureDirectReferences()
        {
            // Direct lookups across active/inactive roots
            if (mainMenuCanvas == null)
            {
                var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (var c in canvases)
                {
                    if (c.gameObject.scene.isLoaded && (c.name == "Canvas" || c.name.Contains("MainMenu")))
                    {
                        mainMenuCanvas = c;
                        break;
                    }
                }
            }

            if (slidesCanvas == null)
            {
                var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (var c in canvases)
                {
                    if (c.gameObject.scene.isLoaded && (c.name == "SlidesCanvas" || c.name.Contains("Slide")))
                    {
                        slidesCanvas = c;
                        break;
                    }
                }
            }

            // Buttons
            if (mainMenuCanvas != null)
            {
                var allButtons = mainMenuCanvas.GetComponentsInChildren<Button>(true);
                foreach (var btn in allButtons)
                {
                    string n = btn.name.ToLower();
                    if (playButton == null && (n.Contains("play") || n.Contains("start"))) playButton = btn;
                    if (exitButton == null && (n.Contains("exit") || n.Contains("quit"))) exitButton = btn;
                }
                if (playButton == null && allButtons.Length > 0) playButton = allButtons[0];
            }

            if (slidesCanvas != null)
            {
                var allButtons = slidesCanvas.GetComponentsInChildren<Button>(true);
                foreach (var btn in allButtons)
                {
                    string n = btn.name.ToLower();
                    if (nextButton == null && (n.Contains("next") || n.Contains("forward"))) nextButton = btn;
                    if (previousButton == null && (n.Contains("prev") || n.Contains("back"))) previousButton = btn;
                    if (skipIntroButton == null && n.Contains("skip")) skipIntroButton = btn;
                }
                if (nextButton == null && allButtons.Length > 0) nextButton = allButtons[0];

                if (slides.Count == 0)
                {
                    AutoPopulateSlides();
                }
            }
        }

        private void AutoPopulateSlides()
        {
            slides.Clear();
            if (slidesCanvas == null) return;

            for (int i = 0; i < 10; i++)
            {
                var vis = slidesCanvas.transform.Find($"Slide{i}");
                var txt = slidesCanvas.transform.Find($"Slide{i}txt");

                if (vis != null || txt != null)
                {
                    slides.Add(new SlideItem
                    {
                        visual = vis != null ? vis.gameObject : null,
                        text = txt != null ? txt.gameObject : null
                    });
                }
                else
                {
                    break;
                }
            }
        }

        private void Start()
        {
            if (mainMenuCanvas != null) mainMenuCanvas.gameObject.SetActive(true);
            if (slidesCanvas != null) slidesCanvas.gameObject.SetActive(false);

            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayClicked);
                playButton.onClick.AddListener(OnPlayClicked);

                // Set keyboard/gamepad focus
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(OnExitClicked);
                exitButton.onClick.AddListener(OnExitClicked);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(OnNextClicked);
                nextButton.onClick.AddListener(OnNextClicked);
            }

            if (previousButton != null)
            {
                previousButton.onClick.RemoveListener(OnPrevClicked);
                previousButton.onClick.AddListener(OnPrevClicked);
            }

            if (skipIntroButton != null)
            {
                skipIntroButton.onClick.RemoveListener(OnSkipClicked);
                skipIntroButton.onClick.AddListener(OnSkipClicked);
            }
        }

        public void OnPlayClicked()
        {
            AudioService.PlayCatchMoth();

            if (mainMenuCanvas != null) mainMenuCanvas.gameObject.SetActive(false);
            if (slidesCanvas != null) slidesCanvas.gameObject.SetActive(true);

            _currentSlideIndex = 0;
            ShowSlide(_currentSlideIndex);

            if (nextButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
            }
        }

        public void OnExitClicked()
        {
            AudioService.PlayCatchMoth();
            Debug.Log("[MainMenuController] Exiting game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void OnNextClicked()
        {
            AudioService.PlayCatchMoth();
            _currentSlideIndex++;

            if (_currentSlideIndex < slides.Count)
            {
                ShowSlide(_currentSlideIndex);
            }
            else
            {
                StartGame();
            }
        }

        public void OnPrevClicked()
        {
            AudioService.PlayCatchMoth();
            if (_currentSlideIndex > 0)
            {
                _currentSlideIndex--;
                ShowSlide(_currentSlideIndex);
            }
        }

        public void OnSkipClicked()
        {
            AudioService.PlayBankFood();
            StartGame();
        }

        private void ShowSlide(int index)
        {
            for (int i = 0; i < slides.Count; i++)
            {
                bool isCurrent = (i == index);
                if (slides[i].visual != null) slides[i].visual.SetActive(isCurrent);
                if (slides[i].text != null) slides[i].text.SetActive(isCurrent);
            }

            if (previousButton != null)
            {
                previousButton.gameObject.SetActive(index > 0);
            }
        }

        private void StartGame()
        {
            GameLoopManager.ResetCampaign();

            Debug.Log($"[MainMenuController] Starting Campaign -> Night 1 ({firstLevelSceneName})...");
            SceneTransitionFader.LoadNightWithTransition(firstLevelSceneName, "NIGHT 1: DRAIN ALLEY", "Generation 1 • Fresh Colony");
        }
    }
}
