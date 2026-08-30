using UnityEngine;

namespace Mothropolis.Night
{
    public enum SpawnMode { Fixed, Random }

    [CreateAssetMenu(fileName = "New NightConfig", menuName = "Mothropolis/Night Config")]
    public class NightConfig : ScriptableObject
    {
        public string nightLabel = "Night 1";
        
        [Header("Population")]
        public int mothPopulation = 24;
        public SpawnMode spawnMode = SpawnMode.Random;
        
        [Header("Difficulty")]
        public float owlAggressionMultiplier = 1f;
        
        [Header("Mechanics")]
        public bool grappleUnlocked = false;
        public bool interactiveLightsEnabled = false;

        // Note: Lighting presets will be added in Sub-phase 2
    }
}
