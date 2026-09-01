using UnityEngine;
using Mothropolis.Core;

namespace Mothropolis.VFX
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameEvents.OnMothCaught += HandleMothCaught;
            GameEvents.OnFoodBanked += HandleFoodBanked;
        }

        private void OnDisable()
        {
            GameEvents.OnMothCaught -= HandleMothCaught;
            GameEvents.OnFoodBanked -= HandleFoodBanked;
        }

        private void HandleMothCaught(Genetics.MothGenome genome)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            Vector3 pos = player != null ? player.transform.position + Vector3.up * 0.5f : Vector3.zero;
            SpawnSparkleBurst(pos, new Color(1f, 0.9f, 0.4f));
        }

        private void HandleFoodBanked(int amount)
        {
            var drain = FindFirstObjectByType<Night.ReturnDrain>();
            Vector3 pos = drain != null ? drain.transform.position : Vector3.zero;
            SpawnSplashRipples(pos, new Color(0.3f, 0.8f, 1f));
        }

        public static void SpawnSparkleBurst(Vector3 position, Color color)
        {
            var pObj = new GameObject("VFX_SparkleBurst");
            pObj.transform.position = position;

            var ps = pObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = 0.15f;
            main.startLifetime = 0.4f;
            main.startSpeed = 3f;
            main.loop = false;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            var colOverLifetime = ps.colorOverLifetime;
            colOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLifetime.color = grad;

            ps.Play();
        }

        public static void SpawnLandingPuff(Vector3 position)
        {
            var pObj = new GameObject("VFX_LandingPuff");
            pObj.transform.position = position;

            var ps = pObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.85f, 0.85f, 0.85f, 0.6f);
            main.startSize = 0.2f;
            main.startLifetime = 0.3f;
            main.startSpeed = 1.5f;
            main.loop = false;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.15f;

            ps.Play();
        }

        public static void SpawnSplashRipples(Vector3 position, Color color)
        {
            var pObj = new GameObject("VFX_Splash");
            pObj.transform.position = position;

            var ps = pObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = 0.25f;
            main.startLifetime = 0.5f;
            main.startSpeed = 2.5f;
            main.loop = false;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

            ps.Play();
        }
    }
}
