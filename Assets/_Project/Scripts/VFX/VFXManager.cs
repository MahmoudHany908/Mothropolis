using UnityEngine;
using Mothropolis.Core;

namespace Mothropolis.VFX
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        private static Material _sharedParticleMaterial;

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
            SpawnSparkleBurst(pos, new Color(1f, 0.92f, 0.35f, 1f));
        }

        private void HandleFoodBanked(int amount)
        {
            var drain = FindFirstObjectByType<Night.ReturnDrain>();
            Vector3 pos = drain != null ? drain.transform.position : Vector3.zero;
            SpawnSplashRipples(pos, new Color(0.35f, 0.85f, 1f, 0.9f));
        }

        private static Material GetOrCreateParticleMaterial()
        {
            if (_sharedParticleMaterial != null) return _sharedParticleMaterial;

            // Prioritize 2D Sprite Unlit / URP Particles / Sprites Default
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");

            _sharedParticleMaterial = new Material(shader);

            // Generate soft-circle alpha texture so particles are smooth glowing motes/puffs
            Texture2D circleTex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            circleTex.filterMode = FilterMode.Bilinear;
            float center = 15.5f;
            float radius = 15.0f;
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(1f - (dist / radius));
                    alpha = alpha * alpha; // Smooth quadratic falloff
                    circleTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            circleTex.Apply();
            _sharedParticleMaterial.mainTexture = circleTex;

            return _sharedParticleMaterial;
        }

        public static void SpawnSparkleBurst(Vector3 position, Color color)
        {
            var pObj = new GameObject("VFX_SparkleBurst");
            pObj.transform.position = position;

            var ps = pObj.AddComponent<ParticleSystem>();
            var renderer = pObj.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = GetOrCreateParticleMaterial();
                renderer.sortingOrder = 25;
            }

            var main = ps.main;
            main.startColor = color;
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            main.startLifetime = 0.45f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 4.5f);
            main.loop = false;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 18) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;

            var colOverLifetime = ps.colorOverLifetime;
            colOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(new Color(1f, 1f, 0.8f), 1f) },
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
            var renderer = pObj.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = GetOrCreateParticleMaterial();
                renderer.sortingOrder = 20;
            }

            var main = ps.main;
            main.startColor = new Color(0.9f, 0.9f, 0.9f, 0.65f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startLifetime = 0.35f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 2.5f);
            main.loop = false;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.2f;

            var colOverLifetime = ps.colorOverLifetime;
            colOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.8f, 0.8f, 0.8f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLifetime.color = grad;

            ps.Play();
        }

        public static void SpawnSplashRipples(Vector3 position, Color color)
        {
            var pObj = new GameObject("VFX_Splash");
            pObj.transform.position = position;

            var ps = pObj.AddComponent<ParticleSystem>();
            var renderer = pObj.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = GetOrCreateParticleMaterial();
                renderer.sortingOrder = 25;
            }

            var main = ps.main;
            main.startColor = color;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            main.startLifetime = 0.55f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 4.0f);
            main.loop = false;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 24) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            var colOverLifetime = ps.colorOverLifetime;
            colOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLifetime.color = grad;

            ps.Play();
        }
    }
}
