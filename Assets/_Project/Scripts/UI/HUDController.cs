using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mothropolis.Core;
using Mothropolis.Owl;

namespace Mothropolis.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_Text mothsText;
        public Image owlWarningImage;
        public Slider exposureSlider;
        public GameObject hudPanel;

        [Header("Toast Notification")]
        public GameObject toastPanel;
        public TMP_Text toastText;

        [Header("Settings")]
        public string textPrefix = "x ";

        private Coroutine _warningRoutine;
        private Coroutine _toastRoutine;
        private Coroutine _mothPunchRoutine;
        private int _currentMoths = 0;
        private OwlState _currentOwlState = OwlState.Idle;

        private void Awake()
        {
            if (hudPanel == null)
            {
                var panel = transform.Find("HUDPanel");
                hudPanel = panel != null ? panel.gameObject : gameObject;
            }

            if (mothsText == null && hudPanel != null)
            {
                var textObj = hudPanel.transform.Find("MothsText");
                if (textObj != null) mothsText = textObj.GetComponent<TMP_Text>();
                if (mothsText == null) mothsText = hudPanel.GetComponentInChildren<TMP_Text>();
            }

            if (owlWarningImage == null && hudPanel != null)
            {
                var warningObj = hudPanel.transform.Find("OwlAttack");
                if (warningObj != null) owlWarningImage = warningObj.GetComponent<Image>();
            }

            if (exposureSlider == null && hudPanel != null)
            {
                var sliderObj = hudPanel.transform.Find("Slider");
                if (sliderObj != null) exposureSlider = sliderObj.GetComponent<Slider>();
                if (exposureSlider == null) exposureSlider = hudPanel.GetComponentInChildren<Slider>();
            }

            if (exposureSlider != null)
            {
                exposureSlider.minValue = 0f;
                exposureSlider.maxValue = 1f;
                exposureSlider.value = 0f;
                exposureSlider.interactable = false;
            }

            EnsureToastPanel();
        }

        private void EnsureToastPanel()
        {
            if (toastPanel == null)
            {
                var existing = transform.Find("ToastPanel");
                if (existing != null) toastPanel = existing.gameObject;
            }

            if (toastPanel == null)
            {
                toastPanel = new GameObject("ToastPanel");
                toastPanel.transform.SetParent(transform, false);

                var rect = toastPanel.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.85f);
                rect.anchorMax = new Vector2(0.5f, 0.85f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(650f, 70f);

                var img = toastPanel.AddComponent<Image>();
                img.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

                var textObj = new GameObject("ToastText");
                textObj.transform.SetParent(toastPanel.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(15f, 5f);
                textRect.offsetMax = new Vector2(-15f, -5f);

                toastText = textObj.AddComponent<TextMeshProUGUI>();
                toastText.alignment = TextAlignmentOptions.Center;
                toastText.fontSize = 16f;
                toastText.enableWordWrapping = true;
                toastText.color = new Color(1f, 0.85f, 0.4f);

                var cg = toastPanel.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                toastPanel.SetActive(false);
            }
            else if (toastText == null)
            {
                toastText = toastPanel.GetComponentInChildren<TMP_Text>();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnMothCaught += HandleMothCaught;
            GameEvents.OnCarriedFoodChanged += HandleCarriedFoodChanged;
            GameEvents.OnFoodBanked += HandleFoodBanked;
            GameEvents.OnNightStarted += HandleNightStarted;
            GameEvents.OnDawnReached += HandleNightEnded;
            GameEvents.OnOwlStateChanged += HandleOwlStateChanged;
            GameEvents.OnExposureChanged += HandleExposureChanged;
            GameEvents.OnImmigrantEvent += HandleImmigrantEvent;
        }

        private void OnDisable()
        {
            GameEvents.OnMothCaught -= HandleMothCaught;
            GameEvents.OnCarriedFoodChanged -= HandleCarriedFoodChanged;
            GameEvents.OnFoodBanked -= HandleFoodBanked;
            GameEvents.OnNightStarted -= HandleNightStarted;
            GameEvents.OnDawnReached -= HandleNightEnded;
            GameEvents.OnOwlStateChanged -= HandleOwlStateChanged;
            GameEvents.OnExposureChanged -= HandleExposureChanged;
            GameEvents.OnImmigrantEvent -= HandleImmigrantEvent;
        }

        private void Start()
        {
            UpdateMothsDisplay(0);
            SetWarningActive(false);
            if (exposureSlider != null) exposureSlider.value = 0f;
        }

        private void HandleMothCaught(Genetics.MothGenome genome)
        {
            _currentMoths++;
            UpdateMothsDisplay(_currentMoths);
            PunchMothsText();
        }

        private void HandleCarriedFoodChanged(int amount)
        {
            _currentMoths = amount;
            UpdateMothsDisplay(_currentMoths);
        }

        private void HandleFoodBanked(int amount)
        {
            _currentMoths = 0;
            UpdateMothsDisplay(0);
            SetWarningActive(false);
            if (exposureSlider != null) exposureSlider.value = 0f;
        }

        private void HandleNightStarted()
        {
            _currentMoths = 0;
            UpdateMothsDisplay(0);
            SetWarningActive(false);
            if (exposureSlider != null) exposureSlider.value = 0f;
        }

        private void HandleNightEnded()
        {
            _currentMoths = 0;
            UpdateMothsDisplay(0);
            SetWarningActive(false);
            if (exposureSlider != null) exposureSlider.value = 0f;
        }

        private void HandleExposureChanged(float exposure)
        {
            if (exposureSlider != null)
            {
                exposureSlider.value = Mathf.Clamp01(exposure);
            }
        }

        private void HandleOwlStateChanged(OwlState state)
        {
            _currentOwlState = state;
            switch (state)
            {
                case OwlState.Charging:
                case OwlState.Telegraph:
                case OwlState.Swoop:
                    SetWarningActive(true);
                    break;
                case OwlState.Idle:
                case OwlState.Recover:
                default:
                    SetWarningActive(false);
                    break;
            }
        }

        private void HandleImmigrantEvent()
        {
            ShowToast("<b>POPULATION WIPEOUT:</b> All local moths were consumed. Stray immigrants have fluttered in from the outskirts to rebuild the colony.");
        }

        public void ShowToast(string message)
        {
            EnsureToastPanel();
            if (toastPanel == null || toastText == null) return;

            toastText.text = message;
            if (_toastRoutine != null) StopCoroutine(_toastRoutine);
            _toastRoutine = StartCoroutine(ToastLifecycleRoutine());
        }

        private IEnumerator ToastLifecycleRoutine()
        {
            toastPanel.SetActive(true);
            var cg = toastPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = toastPanel.AddComponent<CanvasGroup>();

            // Fade In
            for (float t = 0; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                cg.alpha = t / 0.3f;
                yield return null;
            }
            cg.alpha = 1f;

            // Display Duration
            yield return new WaitForSecondsRealtime(4.5f);

            // Fade Out
            for (float t = 0.4f; t > 0; t -= Time.unscaledDeltaTime)
            {
                cg.alpha = t / 0.4f;
                yield return null;
            }
            cg.alpha = 0f;
            toastPanel.SetActive(false);
            _toastRoutine = null;
        }

        private void UpdateMothsDisplay(int count)
        {
            if (mothsText != null)
            {
                mothsText.text = $"{textPrefix}{count}";
            }
        }

        private void PunchMothsText()
        {
            if (mothsText == null) return;
            if (_mothPunchRoutine != null) StopCoroutine(_mothPunchRoutine);
            _mothPunchRoutine = StartCoroutine(PunchTextRoutine(mothsText.transform));
        }

        private IEnumerator PunchTextRoutine(Transform target)
        {
            Vector3 baseScale = Vector3.one;
            for (float t = 0; t < 0.2f; t += Time.unscaledDeltaTime)
            {
                float punch = 1f + Mathf.Sin(t * Mathf.PI / 0.2f) * 0.35f;
                target.localScale = baseScale * punch;
                yield return null;
            }
            target.localScale = baseScale;
            _mothPunchRoutine = null;
        }

        private void SetWarningActive(bool active)
        {
            if (owlWarningImage == null) return;

            if (_warningRoutine != null)
            {
                StopCoroutine(_warningRoutine);
                _warningRoutine = null;
            }

            if (active)
            {
                owlWarningImage.gameObject.SetActive(true);
                _warningRoutine = StartCoroutine(BlinkWarningRoutine());
            }
            else
            {
                owlWarningImage.gameObject.SetActive(false);
            }
        }

        private IEnumerator BlinkWarningRoutine()
        {
            while (true)
            {
                float interval = 0.25f;
                Color activeColor = new Color(1f, 0.75f, 0.2f); // Amber warning

                if (_currentOwlState == OwlState.Telegraph)
                {
                    interval = 0.08f; // Urgent rapid screech flash
                    activeColor = new Color(1f, 0.15f, 0.15f); // Crimson
                }
                else if (_currentOwlState == OwlState.Swoop)
                {
                    interval = 0.05f;
                    activeColor = Color.red;
                }

                if (owlWarningImage != null)
                {
                    owlWarningImage.color = activeColor;
                    owlWarningImage.enabled = !owlWarningImage.enabled;
                }

                yield return new WaitForSecondsRealtime(interval);
            }
        }
    }
}
