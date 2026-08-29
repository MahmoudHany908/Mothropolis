using UnityEngine;
using System.Collections.Generic;
using Mothropolis.Core;

namespace Mothropolis.Lighting
{
    [RequireComponent(typeof(Collider2D))]
    public class ExposureMeter : MonoBehaviour
    {
        [Header("Exposure Settings")]
        public float maxExposure = 100f;
        public float exposureDecayRate = 10f; // Drops per second when in shadows

        private float _currentExposure = 0f;
        private HashSet<ILightSource> _activeLights = new HashSet<ILightSource>();
        private float _lastLogTime = 0f;

        private void Update()
        {
            float fillRate = 0f;

            // Clean up missing/destroyed lights
            _activeLights.RemoveWhere(light => light == null || (light is MonoBehaviour mb && !mb.gameObject.activeInHierarchy));

            // Find the strongest active light we are currently standing in
            foreach (var light in _activeLights)
            {
                if (light.IsActive)
                {
                    fillRate = Mathf.Max(fillRate, light.ExposureFillRate);
                }
            }

            // Fill or Decay exposure
            if (fillRate > 0f)
            {
                _currentExposure += fillRate * 100f * Time.deltaTime; // fillRate is a percentage per sec
            }
            else
            {
                _currentExposure -= exposureDecayRate * Time.deltaTime;
            }

            _currentExposure = Mathf.Clamp(_currentExposure, 0f, maxExposure);

            // Fire event globally so HUD and Owl can react (0 to 1 ratio)
            float exposureRatio = _currentExposure / maxExposure;
            GameEvents.RaiseExposureChanged(exposureRatio);

            // Debug logging (debounced to avoid console spam)
            if (exposureRatio > 0f && Time.time - _lastLogTime > 1f)
            {
                Debug.Log($"[ExposureMeter] Current Exposure: {exposureRatio:P0}");
                _lastLogTime = Time.time;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var light = other.GetComponent<ILightSource>();
            if (light != null)
            {
                _activeLights.Add(light);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var light = other.GetComponent<ILightSource>();
            if (light != null)
            {
                _activeLights.Remove(light);
            }
        }
    }
}
