using System.Collections.Generic;
using UnityEngine;
using Mothropolis.Genetics;

namespace Mothropolis.Core
{
    [System.Serializable]
    public class CampaignSaveData
    {
        public int currentNightIndex = 0;
        public int totalCampaignFood = 0;
        public int generationIndex = 1;
        public List<MothGenome> population = new List<MothGenome>();
    }

    public static class CampaignSaveSystem
    {
        private const string SAVE_KEY = "Mothropolis_CampaignSave";

        public static void Save(int nightIndex, int totalFood, int genIndex, List<MothGenome> population)
        {
            var data = new CampaignSaveData
            {
                currentNightIndex = nightIndex,
                totalCampaignFood = totalFood,
                generationIndex = genIndex,
                population = population != null ? new List<MothGenome>(population) : new List<MothGenome>()
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log($"[CampaignSaveSystem] Campaign saved successfully! Night {nightIndex + 1}, Gen {genIndex}, Food {totalFood}, Moths {data.population.Count}");
        }

        public static CampaignSaveData Load()
        {
            if (!HasSave()) return null;

            string json = PlayerPrefs.GetString(SAVE_KEY);
            try
            {
                var data = JsonUtility.FromJson<CampaignSaveData>(json);
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[CampaignSaveSystem] Failed to parse save data: {ex.Message}");
                return null;
            }
        }

        public static bool HasSave()
        {
            return PlayerPrefs.HasKey(SAVE_KEY);
        }

        public static void DeleteSave()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                PlayerPrefs.DeleteKey(SAVE_KEY);
                PlayerPrefs.Save();
                Debug.Log("[CampaignSaveSystem] Campaign save cleared.");
            }
        }
    }
}
