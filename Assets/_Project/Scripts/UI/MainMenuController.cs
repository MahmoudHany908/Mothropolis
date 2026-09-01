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
        public GameObject mainMenuPanel;
        public GameObject slidesPanel;

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
            EnsureReferences();
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

        private void EnsureReferences()
        {
            // 1. Locate mainMenuPanel
            if (mainMenuPanel == null)
            {
                mainMenuPanel = gameObject; // MainMenuController is attached to Canvas
            }

            // 2. Locate slidesPanel
            if (slidesPanel == null)
            {
                // Search root objects in active scene (finds inactive objects reliably)
                var roots = SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var r in roots)
                {
                    if (r.name == "SlidesCanvas" || r.name.Contains("Slide"))
                    {
                        slidesPanel = r;
                        break;
                    }
                }
            }

            // 3. Locate Buttons on Main Menu
            if (playButton == null && mainMenuPanel != null)
            {
                var buttons = mainMenuPanel.GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    string n = b.name.ToLower();
                    if (n.Contains("play") || n.Contains("start") || n == "button")
                    {
                        playButton = b;
                        break;
                    }
                }
                if (playButton == null && buttons.Length > 0)
                {
                    playButton = buttons[0];
                }
            }

            if (exitButton == null && mainMenuPanel != null)
            {
                var buttons = mainMenuPanel.GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    string n = b.name.ToLower();
                    if (n.Contains("exit") || n.Contains("quit"))
                    {
                        exitButton = b;
                        break;
                    }
                }
            }

            // 4. Locate Next Button on Slides
            if (nextButton == null && slidesPanel != null)
            {
                var buttons = slidesPanel.GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    string n = b.name.ToLower();
                    if (n.Contains("next") || n == "nextbtn")
                    {
                        nextButton = b;
                        break;
                    }
                }
                if (nextButton == null && buttons.Length > 0)
                {
                    nextButton = buttons[0];
                }
            }

            // 5. Populate slides
            AutoPopulateSlides();
        }

        private void AutoPopulateSlides()
        {
            slides.Clear();
            if (slidesPanel == null) return;

            for (int i = 0; i < 10; i++)
            {
                var vis = slidesPanel.transform.Find($"Slide{i}");
                var txt = slidesPanel.transform.Find($"Slide{i}txt");

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
            Debug.Log($"[MainMenuController] Registered {slides.Count} story slides.");
        }

        private void Start()
        {
            EnsureReferences();

            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (slidesPanel != null) slidesPanel.SetActive(false);

            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayClicked);
                playButton.onClick.AddListener(OnPlayClicked);

                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
                Debug.Log($"[MainMenuController] Play button bound: {playButton.name}");
            }
            else
            {
                Debug.LogWarning("[MainMenuController] Play button not found!");
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
                Debug.Log($"[MainMenuController] Next button bound: {nextButton.name}");
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
            Debug.Log("[MainMenuController] Play clicked -> Opening Slideshow...");
            AudioService.PlayCatchMoth();

            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (slidesPanel != null) slidesPanel.SetActive(true);

            _currentSlideIndex = 0;
            ShowSlide(_currentSlideIndex);

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
                }
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
            Debug.Log($"[MainMenuController] Next slide: {_currentSlideIndex} / {slides.Count}");

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

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
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
