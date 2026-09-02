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
        public TMP_Text statusTitleText;
        public TMP_Text foodBankedText;
        public TMP_Text survivorStatsText;
        public TMP_Text geneticsReportText;

        [Header("Buttons")]
        public Button continueButton;

        private Coroutine _revealRoutine;
        private bool _hasClickedContinue = false;

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = true;

            EnsureUIHierarchy();

            // Guarantee report panel is completely hidden on level load
            if (reportPanel != null)
            {
                reportPanel.SetActive(false);
            }
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
                reportPanel.transform.localScale = Vector3.one;

                var panelRect = reportPanel.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                    panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                    panelRect.pivot = new Vector2(0.5f, 0.5f);
                    panelRect.anchoredPosition = Vector2.zero;
                    panelRect.sizeDelta = new Vector2(900f, 620f);
                }

                var panelImg = reportPanel.GetComponent<Image>();
                if (panelImg != null)
                {
                    panelImg.color = new Color(0.05f, 0.07f, 0.11f, 0.96f);
                }

                // 1. Status Title
                var title = reportPanel.transform.Find("Title");
                if (title != null)
                {
                    title.transform.localScale = Vector3.one;
                    statusTitleText = title.GetComponent<TMP_Text>();
                    var r = title.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(0.5f, 0.5f);
                    r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.anchoredPosition = new Vector2(0f, 230f);
                    r.sizeDelta = new Vector2(850f, 60f);
                    if (statusTitleText != null)
                    {
                        statusTitleText.fontSize = 32f;
                        statusTitleText.fontStyle = FontStyles.Bold;
                        statusTitleText.alignment = TextAlignmentOptions.Center;
                        statusTitleText.text = "";
                    }
                }

                // 2. Food Banked
                var food = reportPanel.transform.Find("FoodCount");
                if (food != null)
                {
                    food.transform.localScale = Vector3.one;
                    foodBankedText = food.GetComponent<TMP_Text>();
                    var r = food.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(0.5f, 0.5f);
                    r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.anchoredPosition = new Vector2(0f, 165f);
                    r.sizeDelta = new Vector2(850f, 45f);
                    if (foodBankedText != null)
                    {
                        foodBankedText.fontSize = 24f;
                        foodBankedText.alignment = TextAlignmentOptions.Center;
                        foodBankedText.text = "";
                    }
                }

                // 3. Survivor Stats
                var surv = reportPanel.transform.Find("SurvivorStats");
                if (surv == null)
                {
                    var survObj = new GameObject("SurvivorStats");
                    survObj.transform.SetParent(reportPanel.transform, false);
                    surv = survObj.transform;
                    survivorStatsText = survObj.AddComponent<TextMeshProUGUI>();
                }
                else
                {
                    survivorStatsText = surv.GetComponent<TMP_Text>();
                }
                if (survivorStatsText != null)
                {
                    surv.transform.localScale = Vector3.one;
                    var r = surv.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(0.5f, 0.5f);
                    r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.anchoredPosition = new Vector2(0f, 110f);
                    r.sizeDelta = new Vector2(850f, 40f);
                    survivorStatsText.fontSize = 19f;
                    survivorStatsText.alignment = TextAlignmentOptions.Center;
                    survivorStatsText.color = new Color(0.85f, 0.9f, 0.95f);
                    survivorStatsText.text = "";
                }

                // 4. Genetics Report Body
                var genObj = reportPanel.transform.Find("GeneticsReport");
                if (genObj == null)
                {
                    var gObj = new GameObject("GeneticsReport");
                    gObj.transform.SetParent(reportPanel.transform, false);
                    genObj = gObj.transform;
                    geneticsReportText = gObj.AddComponent<TextMeshProUGUI>();
                }
                else
                {
                    geneticsReportText = genObj.GetComponent<TMP_Text>();
                }
                if (geneticsReportText != null)
                {
                    genObj.transform.localScale = Vector3.one;
                    var r = genObj.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(0.5f, 0.5f);
                    r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.anchoredPosition = new Vector2(0f, -20f);
                    r.sizeDelta = new Vector2(850f, 180f);
                    geneticsReportText.fontSize = 18f;
                    geneticsReportText.alignment = TextAlignmentOptions.Center;
                    geneticsReportText.color = new Color(0.9f, 0.92f, 0.95f);
                    geneticsReportText.text = "";
                }

                // 5. Continue Button
                var btn = reportPanel.transform.Find("ContinueBtn");
                if (btn != null)
                {
                    btn.transform.localScale = Vector3.one;
                    continueButton = btn.GetComponent<Button>();
                    var r = btn.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(0.5f, 0.5f);
                    r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.anchoredPosition = new Vector2(0f, -220f);
                    r.sizeDelta = new Vector2(240f, 50f);

                    var btnImg = btn.GetComponent<Image>();
                    if (btnImg != null)
                    {
                        btnImg.color = new Color(0.18f, 0.55f, 0.32f, 1f);
                    }

                    var btnText = btn.GetComponentInChildren<TMP_Text>(true);
                    if (btnText != null)
                    {
                        btnText.text = "CONTINUE";
                        btnText.fontSize = 20f;
                        btnText.fontStyle = FontStyles.Bold;
                        btnText.alignment = TextAlignmentOptions.Center;
                    }
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
            _hasClickedContinue = false;
            EnsureUIHierarchy();

            if (reportPanel != null)
            {
                reportPanel.SetActive(true);
            }

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
                statusTitleText.color = survived ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.35f, 0.35f);
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

            // 1. Tally Food
            if (foodBankedText != null)
            {
                foodBankedText.text = "Food Banked: 0";
            }

            yield return new WaitForSecondsRealtime(0.2f);

            int targetFood = foodBanked;
            int displayFood = 0;
            while (displayFood < targetFood)
            {
                displayFood++;
                if (foodBankedText != null) foodBankedText.text = $"Food Banked: <color=#FFD54F>+{displayFood}</color>";
                AudioService.PlayCatchMoth();
                yield return new WaitForSecondsRealtime(0.04f);
            }
            if (foodBankedText != null)
            {
                foodBankedText.text = survived ? $"Food Banked: <color=#81C784>+{targetFood}</color>" : "<color=#EF5350>Food Lost to Predator / Dawn</color>";
            }

            yield return new WaitForSecondsRealtime(0.15f);

            // 2. Survivor Details
            if (survivorStatsText != null)
            {
                survivorStatsText.text = $"Moths Harvested: <color=#FFD54F><b>{eatenCount}</b></color> / {initialCount}    |    Survivors: <color=#81C784><b>{survivorCount}</b></color>";
            }

            // 3. Genetics Breakdown Box
            string shiftsTable = $"<b><size=115%>NATURAL SELECTION SHIFTS</size></b>\n\n" +
                                 FormatTraitRow("Speed", avgSpeedBefore, avgSpeedAfter) + "\n" +
                                 FormatTraitRow("Camouflage", avgCamoBefore, avgCamoAfter) + "\n" +
                                 FormatTraitRow("Light Attraction", avgLightBefore, avgLightAfter);

            if (geneticsReportText != null)
            {
                geneticsReportText.text = shiftsTable;
            }
            else if (foodBankedText != null && survivorStatsText == null)
            {
                foodBankedText.text += "\n\n" + shiftsTable;
            }

            AudioService.PlayBankFood();
            yield return new WaitForSecondsRealtime(0.3f);

            // 4. Reveal Continue Button
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

            return $"• {traitName,-16}: {beforePct}% → <b>{afterPct}%</b>  <color={colorHex}>({sign}{delta}% {arrow})</color>";
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
            if (_hasClickedContinue) return;
            _hasClickedContinue = true;

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
