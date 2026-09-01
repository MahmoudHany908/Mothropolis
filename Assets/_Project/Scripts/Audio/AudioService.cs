using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mothropolis.Core;
using Mothropolis.Owl;

namespace Mothropolis.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioService : MonoBehaviour
    {
        public SFXLibrary sfxLibrary;
        
        [Header("Music / Ambience Source")]
        public AudioSource bgmSource;

        private static AudioService _instance;
        private AudioSource _sfxSource;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _sfxSource = GetComponent<AudioSource>();

            if (bgmSource == null)
            {
                var bgmObj = transform.Find("BGMSource");
                if (bgmObj != null) bgmSource = bgmObj.GetComponent<AudioSource>();
                if (bgmSource == null)
                {
                    var newBgmObj = new GameObject("BGMSource");
                    newBgmObj.transform.SetParent(transform);
                    bgmSource = newBgmObj.AddComponent<AudioSource>();
                    bgmSource.loop = true;
                    bgmSource.playOnAwake = false;
                }
            }

            if (sfxLibrary == null)
            {
                sfxLibrary = Resources.Load<SFXLibrary>("SFXLibrary");
                if (sfxLibrary == null)
                {
                    sfxLibrary = Resources.Load<SFXLibrary>("Audio/SFXLibrary");
                }
            }
        }

        private void OnEnable()
        {
            GameEvents.OnTongueAttack += HandleTongueAttack;
            GameEvents.OnMothCaught += HandleMothCaught;
            GameEvents.OnFoodBanked += HandleFoodBanked;
            GameEvents.OnOwlStateChanged += HandleOwlStateChanged;
            GameEvents.OnDawnReached += HandleDawnReached;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            GameEvents.OnTongueAttack -= HandleTongueAttack;
            GameEvents.OnMothCaught -= HandleMothCaught;
            GameEvents.OnFoodBanked -= HandleFoodBanked;
            GameEvents.OnOwlStateChanged -= HandleOwlStateChanged;
            GameEvents.OnDawnReached -= HandleDawnReached;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            PlayBGMForCurrentScene();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayBGMForCurrentScene();
        }

        private void PlayBGMForCurrentScene()
        {
            if (sfxLibrary == null || bgmSource == null) return;

            string sceneName = SceneManager.GetActiveScene().name;
            AudioClip track = null;

            if (sceneName == "Level1" || sceneName.Contains("Alley") || sceneName == "MainMenu")
            {
                track = sfxLibrary.drainAlleyBGM;
            }
            else if (sceneName == "Level2" || sceneName.Contains("Park"))
            {
                track = sfxLibrary.parkBGM;
            }
            else if (sceneName == "Level3" || sceneName.Contains("Shop"))
            {
                track = sfxLibrary.shopCornerBGM;
            }

            if (track != null && bgmSource.clip != track)
            {
                bgmSource.clip = track;
                bgmSource.loop = true;
                bgmSource.volume = 0.35f;
                bgmSource.Play();
            }
        }

        public static void PlayJump() => PlayClip(_instance?.sfxLibrary?.jump);
        public static void PlayTongueLash() => PlayClip(_instance?.sfxLibrary?.tongueLash);
        public static void PlayCatchMoth() => PlayClip(_instance?.sfxLibrary?.catchMoth);
        public static void PlayBankFood() => PlayClip(_instance?.sfxLibrary?.bankFood);
        public static void PlayOwlTelegraph() => PlayClip(_instance?.sfxLibrary?.owlTelegraph);
        public static void PlayOwlSwoop() => PlayClip(_instance?.sfxLibrary?.owlSwoop);
        public static void PlayOwlCaughtPlayer() => PlayClip(_instance?.sfxLibrary?.owlCaughtPlayer);

        private void HandleTongueAttack(Vector2 pos) => PlayTongueLash();
        private void HandleMothCaught(Genetics.MothGenome genome) => PlayCatchMoth();
        private void HandleFoodBanked(int amount) => PlayBankFood();
        private void HandleDawnReached() => PlayOwlCaughtPlayer();

        private Coroutine _duckingRoutine;
        private float _defaultBgmVolume = 0.35f;

        private void HandleOwlStateChanged(OwlState state)
        {
            switch (state)
            {
                case OwlState.Charging:
                case OwlState.Telegraph:
                    PlayOwlTelegraph();
                    DuckBGM(0.12f);
                    break;
                case OwlState.Swoop:
                    PlayOwlSwoop();
                    DuckBGM(0.08f);
                    break;
                case OwlState.Idle:
                case OwlState.Recover:
                default:
                    RestoreBGM();
                    break;
            }
        }

        private void DuckBGM(float targetVol)
        {
            if (_duckingRoutine != null) StopCoroutine(_duckingRoutine);
            _duckingRoutine = StartCoroutine(FadeBgmVolumeRoutine(targetVol, 0.2f));
        }

        private void RestoreBGM()
        {
            if (_duckingRoutine != null) StopCoroutine(_duckingRoutine);
            _duckingRoutine = StartCoroutine(FadeBgmVolumeRoutine(_defaultBgmVolume, 0.6f));
        }

        private IEnumerator FadeBgmVolumeRoutine(float targetVol, float duration)
        {
            if (bgmSource == null) yield break;
            float startVol = bgmSource.volume;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                bgmSource.volume = Mathf.Lerp(startVol, targetVol, t / duration);
                yield return null;
            }
            bgmSource.volume = targetVol;
            _duckingRoutine = null;
        }

        public static void SetMasterVolume(float vol) => AudioListener.volume = Mathf.Clamp01(vol);

        private static void PlayClip(AudioClip clip)
        {
            if (clip != null && _instance?._sfxSource != null)
            {
                _instance._sfxSource.PlayOneShot(clip);
            }
        }
    }
}
