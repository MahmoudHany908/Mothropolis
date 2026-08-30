using UnityEngine;
using System.Collections.Generic;

namespace Mothropolis.Moths
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MothBoid : MonoBehaviour
    {
        [Header("Boid Settings")]
        public float neighborRadius = 3f;
        public float separationRadius = 1f;
        public LayerMask boidLayer;
        
        [Header("Force Weights")]
        public float alignmentWeight = 1f;
        public float cohesionWeight = 1f;
        public float separationWeight = 1.5f;
        public float boundaryWeight = 3f;
        
        [Header("Movement")]
        public float maxSpeed = 5f;
        public float maxSteerForce = 3f;
        
        [Header("Boundaries (Steer away)")]
        public Vector2 minBounds = new Vector2(-15f, -10f);
        public Vector2 maxBounds = new Vector2(15f, 10f);
        public float boundaryMargin = 2f;

        private Rigidbody2D _rb;
        private MothController _controller;
        private Collider2D[] _neighbors = new Collider2D[24]; // Pre-allocate to max population

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _controller = GetComponent<MothController>();
            
            // Prevent Unity Physics from adding gravity
            _rb.gravityScale = 0f;
        }

        private void Start()
        {
            // Genetic Overrides
            if (_controller != null)
            {
                // Genome.speed is 0f to 1f. Map this to sensible world-space speeds.
                // Base speed of 3, plus up to 5 extra speed.
                maxSpeed = 3f + (_controller.Genome.speed * 5f);
                
                // Faster moths need more steering force to corner effectively
                maxSteerForce = 2f + (_controller.Genome.speed * 4f);
            }

            // Start with a random velocity constrained by the new genetic speed
            _rb.linearVelocity = Random.insideUnitCircle.normalized * maxSpeed;
        }

        private void FixedUpdate()
        {
            UpdateLightTarget();

            Vector2 alignment = Vector2.zero;
            Vector2 cohesion = Vector2.zero;
            Vector2 separation = Vector2.zero;
            
            int neighborCount = 0;
            int separationCount = 0;

            // Find all neighbors using the modern Unity 6 non-allocating method
            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.layerMask = boidLayer;
            int hitCount = Physics2D.OverlapCircle(transform.position, neighborRadius, filter, _neighbors);

            for (int i = 0; i < hitCount; i++)
            {
                if (_neighbors[i].gameObject == gameObject) continue;
                
                var otherRb = _neighbors[i].attachedRigidbody;
                if (otherRb == null) continue;

                // Alignment: Steer towards the average heading of local flockmates
                alignment += otherRb.linearVelocity;
                
                // Cohesion: Steer to move toward the average position of local flockmates
                cohesion += (Vector2)_neighbors[i].transform.position;
                
                neighborCount++;

                // Separation: Steer to avoid crowding local flockmates
                float dist = Vector2.Distance(transform.position, _neighbors[i].transform.position);
                if (dist > 0 && dist < separationRadius)
                {
                    Vector2 diff = (Vector2)transform.position - (Vector2)_neighbors[i].transform.position;
                    // Weight closer objects more heavily
                    separation += diff.normalized / dist; 
                    separationCount++;
                }
            }

            if (neighborCount > 0)
            {
                alignment /= neighborCount;
                alignment = SteerTowards(alignment);

                cohesion /= neighborCount;
                cohesion = SteerTowards(cohesion - (Vector2)transform.position);
            }

            if (separationCount > 0)
            {
                separation /= separationCount;
                separation = SteerTowards(separation);
            }

            // Environment Reactions
            Vector2 seekForce = Vector2.zero;
            if (_targetLight != null && _targetLight.IsActive)
            {
                seekForce = SteerTowards(_targetLight.Position - (Vector2)transform.position);
            }

            Vector2 activeFleeForce = Vector2.zero;
            if (_fleeTimer > 0)
            {
                _fleeTimer -= Time.fixedDeltaTime;
                activeFleeForce = _fleeForce;
            }

            // Boundary Avoidance (bounce/steer away)
            Vector2 boundaryForce = CalculateBoundaryForce();

            // Combine all forces
            Vector2 acceleration = (alignment * alignmentWeight) + 
                                   (cohesion * cohesionWeight) + 
                                   (separation * separationWeight) +
                                   (boundaryForce * boundaryWeight) +
                                   (seekForce * seekWeight) +
                                   (activeFleeForce * fleeWeight);

            // Apply acceleration
            _rb.linearVelocity += acceleration * Time.fixedDeltaTime;
            
            // Limit to max speed
            _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, maxSpeed);
            
            // Visuals: Face the direction of travel
            if (_controller != null && _rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                _controller.SetFacingDirection(_rb.linearVelocity.x < 0);
            }
        }

        private Vector2 SteerTowards(Vector2 vector)
        {
            if (vector.sqrMagnitude == 0) return Vector2.zero;
            
            // Scale to desired max speed
            vector = vector.normalized * maxSpeed;
            
            // Steering = Desired - Velocity
            Vector2 steer = vector - _rb.linearVelocity;
            
            // Clamp to max steering force
            return Vector2.ClampMagnitude(steer, maxSteerForce);
        }

        private Vector2 CalculateBoundaryForce()
        {
            Vector2 force = Vector2.zero;
            Vector2 pos = transform.position;

            if (pos.x < minBounds.x + boundaryMargin)
                force.x = maxSpeed;
            else if (pos.x > maxBounds.x - boundaryMargin)
                force.x = -maxSpeed;

            if (pos.y < minBounds.y + boundaryMargin)
                force.y = maxSpeed;
            else if (pos.y > maxBounds.y - boundaryMargin)
                force.y = -maxSpeed;

            return SteerTowards(force);
        }

        [Header("Environmental Reactions")]
        public float fleeWeight = 5f;
        public float seekWeight = 2f;
        public float fleeDuration = 1.5f;
        
        private float _fleeTimer = 0f;
        private Vector2 _fleeForce = Vector2.zero;
        
        private float _lightCheckTimer = 0f;
        private Lighting.ILightSource _targetLight;

        private void OnEnable() => Core.GameEvents.OnTongueAttack += HandleTongueAttack;
        private void OnDisable() => Core.GameEvents.OnTongueAttack -= HandleTongueAttack;

        private void HandleTongueAttack(Vector2 attackPos)
        {
            // If the tongue strikes nearby, panic and flee!
            float dist = Vector2.Distance(transform.position, attackPos);
            if (dist < 15f) // Fear radius
            {
                _fleeTimer = fleeDuration;
                Vector2 diff = (Vector2)transform.position - attackPos;
                _fleeForce = SteerTowards(diff.normalized * maxSpeed);
            }
        }

        private void UpdateLightTarget()
        {
            _lightCheckTimer += Time.deltaTime;
            // Only check occasionally to save performance
            if (_lightCheckTimer > Random.Range(0.5f, 1.5f))
            {
                _lightCheckTimer = 0f;
                _targetLight = null;
                
                float closestDist = float.MaxValue;
                // Genetic trait determines how far they can see lights
                float visionRadius = _controller != null ? _controller.Genome.PreferredLightRadius : 50f;

                var allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var script in allScripts)
                {
                    if (script is Lighting.ILightSource light && light.IsActive)
                    {
                        float dist = Vector2.Distance(transform.position, light.Position);
                        if (dist < visionRadius && dist < closestDist)
                        {
                            closestDist = dist;
                            _targetLight = light;
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize the boundaries in the editor when the moth is selected
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0f);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
            Gizmos.DrawWireCube(center, size);

            // Visualize the margin
            Gizmos.color = Color.red;
            Vector3 marginSize = new Vector3(size.x - (boundaryMargin * 2), size.y - (boundaryMargin * 2), 0f);
            Gizmos.DrawWireCube(center, marginSize);
        }
    }
}
