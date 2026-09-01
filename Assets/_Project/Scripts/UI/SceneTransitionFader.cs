using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Mothropolis.UI
{
    public class SceneTransitionFader : MonoBehaviour
    {
        public static SceneTransitionFader Instance { get; private set; }

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private Image _blackOverlay;
        private TMP_Text _titleText;
        private TMP_Text _subTitleText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildOverlayUI();
        }

        private void BuildOverlayUI()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999; // Ensure it renders on top of everything

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;

            // Black Background
            var bgObj = new GameObject("BlackOverlay");
            bgObj.transform.SetParent(transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            _blackOverlay = bgObj.AddComponent<Image>();
            _blackOverlay.color = Color.black;

            // Title Text
            var titleObj = new GameObject("NightTitle");
            titleObj.transform.SetParent(bgObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.45f);
            titleRect.anchorMax = new Vector2(0.9f, 0.65f);
            titleRect.sizeDelta = Vector2.zero;

            _titleText = titleObj.AddComponent<TextMeshProUGUI>();
            _titleText.alignment = TextAlignmentOptions.Center;
            _titleText.fontSize = 54f;
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.color = new Color(1f, 0.95f, 0.8f);
            _titleText.text = "";

            // Subtitle Text
            var subObj = new GameObject("NightSubtitle");
            subObj.transform.SetParent(bgObj.transform, false);
            var subRect = subObj.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.1f, 0.35f);
            subRect.anchorMax = new Vector2(0.9f, 0.45f);
            subRect.sizeDelta = Vector2.zero;

            _subTitleText = subObj.AddComponent<TextMeshProUGUI>();
            _subTitleText.alignment = TextAlignmentOptions.Center;
            _subTitleText.fontSize = 26f;
            _subTitleText.color = new Color(0.7f, 0.8f, 0.75f);
            _subTitleText.text = "";
        }

        public static void LoadNightWithTransition(string sceneName, string nightTitle, string subtitle, System.Action onSceneLoadedCallback = null)
        {
            if (Instance == null)
            {
                var obj = new GameObject("SceneTransitionFader");
                Instance = obj.AddComponent<SceneTransitionFader>();
            }

            Instance.StartCoroutine(Instance.TransitionRoutine(sceneName, nightTitle, subtitle, onSceneLoadedCallback));
        }

        private IEnumerator TransitionRoutine(string sceneName, string nightTitle, string subtitle, System.Action onSceneLoadedCallback)
        {
            _canvasGroup.blocksRaycasts = true;
            _titleText.text = nightTitle;
            _subTitleText.text = subtitle;

            // 1. Fade to black
            for (float t = 0; t < 0.4f; t += Time.unscaledDeltaTime)
            {
                _canvasGroup.alpha = t / 0.4f;
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // 2. Display Night Title Card
            yield return new WaitForSecondsRealtime(1.1f);

            // 3. Load the scene
            string currentScene = SceneManager.GetActiveScene().name;
            if (sceneName != currentScene)
            {
                var asyncOp = SceneManager.LoadSceneAsync(sceneName);
                while (!asyncOp.isDone)
                {
                    yield return null;
                }
            }

            onSceneLoadedCallback?.Invoke();

            yield return new WaitForSecondsRealtime(0.2f);

            // 4. Fade from black
            for (float t = 0.4f; t > 0; t -= Time.unscaledDeltaTime)
            {
                _canvasGroup.alpha = t / 0.4f;
                yield return null;
            }
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
