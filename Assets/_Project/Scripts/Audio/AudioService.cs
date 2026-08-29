using UnityEngine;

namespace Mothropolis.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioService : MonoBehaviour
    {
        public SFXLibrary sfxLibrary;
        
        private static AudioService _instance;
        private AudioSource _audioSource;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _audioSource = GetComponent<AudioSource>();
        }

        public static void PlayJump() => PlayClip(_instance?.sfxLibrary?.jump);
        public static void PlayTongueLash() => PlayClip(_instance?.sfxLibrary?.tongueLash);
        public static void PlayCatchMoth() => PlayClip(_instance?.sfxLibrary?.catchMoth);
        public static void PlayBankFood() => PlayClip(_instance?.sfxLibrary?.bankFood);
        public static void PlayOwlTelegraph() => PlayClip(_instance?.sfxLibrary?.owlTelegraph);
        public static void PlayOwlSwoop() => PlayClip(_instance?.sfxLibrary?.owlSwoop);
        public static void PlayOwlCaughtPlayer() => PlayClip(_instance?.sfxLibrary?.owlCaughtPlayer);

        private static void PlayClip(AudioClip clip)
        {
            if (clip != null && _instance?._audioSource != null)
            {
                _instance._audioSource.PlayOneShot(clip);
            }
        }
    }
}
