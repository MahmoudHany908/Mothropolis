using UnityEngine;
using UnityEngine.InputSystem;

namespace Mothropolis.Lighting
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class InteractiveLightSource : MonoBehaviour, ILightSource
    {
        [Header("Light Settings")]
        public float radius = 8f;
        public int mothPoolWeight = 3; // Attracts more moths when active
        public float exposureFillRate = 0.3f; // Fills 30% of meter per second
        public float activeDuration = 3f;

        [Header("Visuals")]
        public GameObject lightVisual; // E.g., 2D Light or glowing Sprite

        public float Radius => radius;
        public int MothPoolWeight => mothPoolWeight;
        public float ExposureFillRate => exposureFillRate;
        public bool IsActive => _isActive;
        public Vector2 Position => transform.position;

        private bool _isActive = false;
        private float _timer = 0f;
        private bool _playerInRange = false;

        private void Start()
        {
            if (lightVisual == null && transform.childCount > 0)
            {
                lightVisual = transform.GetChild(0).gameObject;
            }

            if (lightVisual != null) lightVisual.SetActive(_isActive);
        }

        private void OnValidate()
        {
            var col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.radius = radius;
                col.isTrigger = true;
            }
        }

        private void Update()
        {
            if (_isActive)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f)
                {
                    TurnOff();
                }
            }
            else if (_playerInRange && Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
            {
                TurnOn();
            }
        }

        private void TurnOn()
        {
            _isActive = true;
            _timer = activeDuration;
            if (lightVisual != null) lightVisual.SetActive(true);
            Debug.Log("Interactive Light Turned ON!");
        }

        private void TurnOff()
        {
            _isActive = false;
            if (lightVisual != null) lightVisual.SetActive(false);
            Debug.Log("Interactive Light Turned OFF.");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<Player.PlayerController>() != null)
            {
                _playerInRange = true;
                if (!_isActive)
                {
                    Debug.Log("Press E or UP to turn on the light trap!");
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<Player.PlayerController>() != null)
            {
                _playerInRange = false;
            }
        }
    }
}
