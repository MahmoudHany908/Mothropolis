using System.Collections.Generic;
using UnityEngine;
using Mothropolis.Genetics;

namespace Mothropolis.Moths
{
    public class RandomAreaSpawner : MonoBehaviour, IMothSpawner
    {
        public BoxCollider2D spawnArea;
        private List<GameObject> _spawnedMoths = new List<GameObject>();

        public void SpawnMoths(List<MothGenome> population, GameObject mothPrefab)
        {
            ClearMoths();
            
            if (spawnArea == null)
            {
                Debug.LogWarning("[RandomAreaSpawner] No spawn area (BoxCollider2D) assigned!");
                return;
            }

            Bounds bounds = spawnArea.bounds;

            foreach (var genome in population)
            {
                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomY = Random.Range(bounds.min.y, bounds.max.y);
                Vector3 randomPosition = new Vector3(randomX, randomY, 0f);

                GameObject moth = Instantiate(mothPrefab, randomPosition, Quaternion.identity);
                moth.name = $"Moth_{genome.speed:F2}_{genome.camouflage:F2}_{genome.lightAttraction:F2}";
                
                if (moth.TryGetComponent<MothController>(out var controller))
                {
                    controller.Genome = genome;
                    controller.ApplyVisuals();
                }

                _spawnedMoths.Add(moth);
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
