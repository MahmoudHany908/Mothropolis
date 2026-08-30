using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mothropolis.Core;

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

        [Header("Buttons")]
        public Button playButton;
        public Button nextButton;

        [Header("Slides Configuration")]
        public List<SlideItem> slides = new List<SlideItem>();
        public string firstLevelSceneName = "Level1";

        private int _currentSlideIndex = 0;

        private void Awake()
        {
            // Auto-detect panels if not assigned
            if (mainMenuPanel == null)
            {
                var menuCanvas = GameObject.Find("Canvas");
                if (menuCanvas != null) mainMenuPanel = menuCanvas;
            }

            if (slidesPanel == null)
            {
                var slidesCanvas = GameObject.Find("SlidesCanvas");
                if (slidesCanvas != null) slidesPanel = slidesCanvas;
            }

            // Auto-detect buttons if not assigned
            if (playButton == null && mainMenuPanel != null)
            {
                playButton = mainMenuPanel.GetComponentInChildren<Button>(true);
            }

            if (nextButton == null && slidesPanel != null)
            {
                nextButton = slidesPanel.GetComponentInChildren<Button>(true);
            }

            // Auto-detect slides if empty
            if (slides.Count == 0 && slidesPanel != null)
            {
                AutoPopulateSlides();
            }
        }

        private void AutoPopulateSlides()
        {
            slides.Clear();
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
        }

        private void Start()
        {
            // Set initial visibility
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (slidesPanel != null) slidesPanel.SetActive(false);

            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayClicked);
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(OnNextClicked);
                nextButton.onClick.AddListener(OnNextClicked);
            }
        }

        public void OnPlayClicked()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (slidesPanel != null) slidesPanel.SetActive(true);

            _currentSlideIndex = 0;
            ShowSlide(_currentSlideIndex);
        }

        public void OnNextClicked()
        {
            _currentSlideIndex++;

            if (_currentSlideIndex < slides.Count)
            {
                ShowSlide(_currentSlideIndex);
            }
            else
            {
                // Last slide reached: Launch the game!
                StartGame();
            }
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
        }

        private void StartGame()
        {
            // Reset campaign progress for a fresh Gen 1 start
            GameLoopManager.ResetCampaign();

            Debug.Log($"[MainMenuController] Starting game -> Loading {firstLevelSceneName}...");
            SceneManager.LoadScene(firstLevelSceneName);
        }
    }
}
