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
        public GameObject hudPanel;

        [Header("Settings")]
        public string textPrefix = "x ";
        public float blinkInterval = 0.15f;

        private Coroutine _warningRoutine;
        private int _currentMoths = 0;

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
        }

        private void OnEnable()
        {
            GameEvents.OnMothCaught += HandleMothCaught;
            GameEvents.OnCarriedFoodChanged += HandleCarriedFoodChanged;
            GameEvents.OnFoodBanked += HandleFoodBanked;
            GameEvents.OnNightStarted += HandleNightStarted;
            GameEvents.OnDawnReached += HandleNightEnded;
            GameEvents.OnOwlStateChanged += HandleOwlStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnMothCaught -= HandleMothCaught;
            GameEvents.OnCarriedFoodChanged -= HandleCarriedFoodChanged;
            GameEvents.OnFoodBanked -= HandleFoodBanked;
            GameEvents.OnNightStarted -= HandleNightStarted;
            GameEvents.OnDawnReached -= HandleNightEnded;
            GameEvents.OnOwlStateChanged -= HandleOwlStateChanged;
        }

        private void Start()
        {
            UpdateMothsDisplay(0);
            SetWarningActive(false);
        }

        private void HandleMothCaught(Genetics.MothGenome genome)
        {
            _currentMoths++;
            UpdateMothsDisplay(_currentMoths);
        }

        private void HandleCarriedFoodChanged(int amount)
        {
            _currentMoths = amount;
            UpdateMothsDisplay(_currentMoths);
        }

        private void HandleFoodBanked(int totalBanked)
        {
            _currentMoths = 0;
            UpdateMothsDisplay(0);
            SetWarningActive(false);
        }

        private void HandleNightStarted()
        {
            _currentMoths = 0;
            UpdateMothsDisplay(0);
            SetWarningActive(false);
        }

        private void HandleNightEnded()
        {
            SetWarningActive(false);
        }

        private void HandleOwlStateChanged(OwlController.OwlState state)
        {
            switch (state)
            {
                case OwlController.OwlState.Charging:
                case OwlController.OwlState.Telegraph:
                case OwlController.OwlState.Swoop:
                    SetWarningActive(true);
                    break;
                case OwlController.OwlState.Idle:
                case OwlController.OwlState.Recover:
                default:
                    SetWarningActive(false);
                    break;
            }
        }

        private void UpdateMothsDisplay(int count)
        {
            if (mothsText != null)
            {
                mothsText.text = $"{textPrefix}{count}";
            }
        }

        private void SetWarningActive(bool active)
        {
            if (_warningRoutine != null)
            {
                StopCoroutine(_warningRoutine);
                _warningRoutine = null;
            }

            if (owlWarningImage != null)
            {
                if (active)
                {
                    _warningRoutine = StartCoroutine(BlinkWarningRoutine());
                }
                else
                {
                    owlWarningImage.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator BlinkWarningRoutine()
        {
            if (owlWarningImage == null) yield break;

            while (true)
            {
                owlWarningImage.gameObject.SetActive(!owlWarningImage.gameObject.activeSelf);
                yield return new WaitForSecondsRealtime(blinkInterval);
            }
        }
    }
}
