using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mothropolis.Core;
using Mothropolis.Genetics;
using Mothropolis.Audio;

namespace Mothropolis.UI
{
    public class EvolutionReportUI : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject reportPanel;
        
        [Header("Header Text Elements")]
        public TMP_Text statusTitleText; // e.g. "NIGHT 1 COMPLETE" / "NIGHT SURVIVED!"
        public TMP_Text subTitleText;    // e.g. "Natural Selection Report"
        public TMP_Text foodBankedText;
        public TMP_Text survivorStatsText; // e.g. "Moths Eaten: 8 / 24 | Survived: 16"

        [Header("Stat Rows (Optional direct bindings)")]
        public TMP_Text speedStatText;
        public TMP_Text camoStatText;
        public TMP_Text lightStatText;
        public Slider speedBar;
        public Slider camoBar;
        public Slider lightBar;

        [Header("Buttons")]
        public Button continueButton;

        private Coroutine _revealRoutine;

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = true;

            EnsureUIHierarchy();
        }

        private void EnsureUIHierarchy()
        {
            if (reportPanel == null)
            {
                var panel = transform.Find("Report Panel");
                if (panel != null) reportPanel = panel.gameObject;
            }

            if (reportPanel != null)
            {
                if (statusTitleText == null)
                {
                    var title = reportPanel.transform.Find("Title");
                    if (title != null) statusTitleText = title.GetComponent<TMP_Text>();
                }

                if (foodBankedText == null)
                {
                    var food = reportPanel.transform.Find("FoodCount");
                    if (food != null) foodBankedText = food.GetComponent<TMP_Text>();
                }

                if (continueButton == null)
                {
                    var btn = reportPanel.transform.Find("ContinueBtn");
                    if (btn != null) continueButton = btn.GetComponent<Button>();
                    if (continueButton == null) continueButton = reportPanel.GetComponentInChildren<Button>(true);
                }

                // Look for dynamic or existing stat containers
                if (survivorStatsText == null)
                {
                    var surv = reportPanel.transform.Find("SurvivorStats");
                    if (surv != null) survivorStatsText = surv.GetComponent<TMP_Text>();
                }
            }
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

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void HandleFoodBanked(int totalBanked)
        {
            ShowReport(true, totalBanked);
        }

        private void HandleDawnReached()
        {
            ShowReport(false, 0);
        }

        public void ShowReport(bool survived, int foodBanked)
        {
            EnsureUIHierarchy();

            if (reportPanel != null)
            {
                reportPanel.SetActive(true);
            }

            // Pause physics and simulation while reading the report
            Time.timeScale = 0f;

            if (_revealRoutine != null) StopCoroutine(_revealRoutine);
            _revealRoutine = StartCoroutine(AnimateReportRoutine(survived, foodBanked));
        }

        private IEnumerator AnimateReportRoutine(bool survived, int foodBanked)
        {
            var popManager = FindFirstObjectByType<MothPopulationManager>();
            
            var initialList = popManager != null && popManager.InitialNightPopulation != null 
                ? popManager.InitialNightPopulation 
                : new List<MothGenome>();

            var survivorList = popManager != null && popManager.CurrentGeneration != null 
                ? popManager.CurrentGeneration 
                : new List<MothGenome>();

            int initialCount = initialList.Count > 0 ? initialList.Count : 24;
            int survivorCount = survivorList.Count;
            int eatenCount = Mathf.Max(0, initialCount - survivorCount);

            int currentNight = GameLoopManager.CurrentNightIndex + 1;
            string titleStr = survived ? $"NIGHT {currentNight} SURVIVED" : $"NIGHT {currentNight} FAILED (DAWN)";

            if (statusTitleText != null)
            {
                statusTitleText.text = titleStr;
                statusTitleText.color = survived ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.35f, 0.35f);
            }

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }

            // Calculate trait averages before & after
            float avgSpeedBefore = CalculateAverage(initialList, g => g.speed);
            float avgSpeedAfter = survivorList.Count > 0 ? CalculateAverage(survivorList, g => g.speed) : avgSpeedBefore;

            float avgCamoBefore = CalculateAverage(initialList, g => g.camouflage);
            float avgCamoAfter = survivorList.Count > 0 ? CalculateAverage(survivorList, g => g.camouflage) : avgCamoBefore;

            float avgLightBefore = CalculateAverage(initialList, g => g.lightAttraction);
            float avgLightAfter = survivorList.Count > 0 ? CalculateAverage(survivorList, g => g.lightAttraction) : avgLightBefore;

            // 1. Tally Food & Survivor counts
            if (foodBankedText != null)
            {
                foodBankedText.text = $"Food Banked: 0";
            }

            yield return new WaitForSecondsRealtime(0.2f);

            int targetFood = foodBanked;
            int displayFood = 0;
            while (displayFood < targetFood)
            {
                displayFood++;
                if (foodBankedText != null) foodBankedText.text = $"Food Banked: +{displayFood}";
                AudioService.PlayCatchMoth();
                yield return new WaitForSecondsRealtime(0.04f);
            }
            if (foodBankedText != null)
            {
                foodBankedText.text = survived ? $"Food Banked: +{targetFood}" : "Food Lost (Caught by Owl or Dawn)";
            }

            yield return new WaitForSecondsRealtime(0.15f);

            // 2. Survivor Details
            string summaryDetails = $"Moths Eaten: <color=#FFD54F>{eatenCount}</color> / {initialCount}   |   Survivors: <color=#81C784>{survivorCount}</color>\n\n" +
                                   $"<b><size=115%>GENETIC SELECTION SHIFTS</size></b>\n" +
                                   FormatTraitRow("Speed", avgSpeedBefore, avgSpeedAfter) + "\n" +
                                   FormatTraitRow("Camouflage", avgCamoBefore, avgCamoAfter) + "\n" +
                                   FormatTraitRow("Light Attraction", avgLightBefore, avgLightAfter);

            if (survivorStatsText != null)
            {
                survivorStatsText.text = summaryDetails;
            }
            else if (foodBankedText != null)
            {
                // Fallback: Append directly to details if secondary text block isn't present
                foodBankedText.text = (survived ? $"Food Banked: +{targetFood}\n\n" : "Food Lost to Predator/Dawn\n\n") + summaryDetails;
            }

            AudioService.PlayBankFood();
            yield return new WaitForSecondsRealtime(0.3f);

            // 3. Reveal Continue Button
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
            }

            _revealRoutine = null;
        }

        private string FormatTraitRow(string traitName, float before, float after)
        {
            int beforePct = Mathf.RoundToInt(before * 100f);
            int afterPct = Mathf.RoundToInt(after * 100f);
            int delta = afterPct - beforePct;

            string arrow = delta > 0 ? "▲" : (delta < 0 ? "▼" : "—");
            string colorHex = delta > 0 ? "#81C784" : (delta < 0 ? "#E57373" : "#B0BEC5");
            string sign = delta > 0 ? "+" : "";

            return $"• {traitName,-16}: {beforePct}% → <b>{afterPct}%</b> <color={colorHex}>({sign}{delta}% {arrow})</color>";
        }

        private float CalculateAverage(List<MothGenome> list, System.Func<MothGenome, float> selector)
        {
            if (list == null || list.Count == 0) return 0.5f;
            float sum = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                sum += selector(list[i]);
            }
            return sum / list.Count;
        }

        public void OnContinueClicked()
        {
            Time.timeScale = 1f;
            if (reportPanel != null)
            {
                reportPanel.SetActive(false);
            }
            
            var loopManager = GameLoopManager.Instance != null ? GameLoopManager.Instance : FindFirstObjectByType<GameLoopManager>();
            if (loopManager != null)
            {
                loopManager.TransitionTo(GameState.Reproduce);
            }
            else
            {
                Debug.LogWarning("No GameLoopManager found to trigger next generation!");
            }
        }
    }
}
