using UnityEngine;

namespace Mothropolis.Audio
{
    [CreateAssetMenu(fileName = "SFXLibrary", menuName = "Mothropolis/SFX Library")]
    public class SFXLibrary : ScriptableObject
    {
        [Header("Player Actions")]
        public AudioClip jump;
        public AudioClip tongueLash;
        public AudioClip catchMoth;

        [Header("Economy / Goal")]
        public AudioClip bankFood;

        [Header("Owl Predator")]
        public AudioClip owlTelegraph;
        public AudioClip owlSwoop;
        public AudioClip owlCaughtPlayer;

        [Header("Background Music / Ambience")]
        public AudioClip drainAlleyBGM;
        public AudioClip parkBGM;
        public AudioClip shopCornerBGM;
    }
}
