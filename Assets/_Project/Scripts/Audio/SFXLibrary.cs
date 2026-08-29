using UnityEngine;

namespace Mothropolis.Audio
{
    [CreateAssetMenu(fileName = "SFXLibrary", menuName = "Mothropolis/SFX Library")]
    public class SFXLibrary : ScriptableObject
    {
        public AudioClip jump;
        public AudioClip tongueLash;
        public AudioClip catchMoth;
        public AudioClip bankFood;
        public AudioClip owlTelegraph;
        public AudioClip owlSwoop;
        public AudioClip owlCaughtPlayer;
    }
}
