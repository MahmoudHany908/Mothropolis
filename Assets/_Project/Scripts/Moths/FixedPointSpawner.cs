using System.Collections.Generic;
using UnityEngine;
using Mothropolis.Genetics;

namespace Mothropolis.Moths
{
    public class FixedPointSpawner : MonoBehaviour, IMothSpawner
    {
        public Transform[] spawnPoints;
        private List<GameObject> _spawnedMoths = new List<GameObject>();

        public void SpawnMoths(List<MothGenome> population, GameObject mothPrefab)
        {
            ClearMoths();
            
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[FixedPointSpawner] No spawn points assigned!");
                return;
            }

            int spawnIndex = 0;
            foreach (var genome in population)
            {
                Transform t = spawnPoints[spawnIndex % spawnPoints.Length];
                
                GameObject moth = Instantiate(mothPrefab, t.position, Quaternion.identity);
                moth.name = $"Moth_{genome.speed:F2}_{genome.camouflage:F2}_{genome.lightAttraction:F2}";
                
                if (moth.TryGetComponent<MothController>(out var controller))
                {
                    controller.Genome = genome;
                    controller.ApplyVisuals();
                }

                _spawnedMoths.Add(moth);
                spawnIndex++;
            }
        }

        public void ClearMoths()
        {
            foreach (var moth in _spawnedMoths)
            {
                if (moth != null) Destroy(moth);
            }
            _spawnedMoths.Clear();
        }
    }
}
