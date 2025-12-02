using UnityEngine;
using System.Collections.Generic;

namespace MilosAdventure.Player
{
    public class GravityLockSystem : MonoBehaviour
    {
        [Header("Lock Settings")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Grace period before unlocking when planet leaves range (prevents edge bouncing)")]
        private float unlockGracePeriod = 0.3f;

        [SerializeField]
        [Range(0.1f, 2.0f)]
        [Tooltip("Maximum relative speed to planet for full lock (use brake/drag to slow down)")]
        private float lockActivationSpeed = 0.5f;

        [SerializeField]
        [Range(0.1f, 1.0f)]
        [Tooltip("Duration for visual indicator fade in/out")]
        private float indicatorFadeDuration = 0.3f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject lockIndicatorPrefab;

        private PlayerShipController _ship;
        private CelestialBodyController _lockedPlanet;
        private GameObject _lockIndicator;
        private List<CelestialBodyController> _planetsInRange = new List<CelestialBodyController>();
        private float _indicatorFadeTime = 0f;
        private bool _isFadingOut = false;
        private float _timeSinceLeftRange = 0f;

        public bool IsLocked => _lockedPlanet != null;
        public CelestialBodyController LockedPlanet => _lockedPlanet;
        public float LockActivationSpeed => lockActivationSpeed;

        private void Awake()
        {
            _ship = GetComponent<PlayerShipController>();

            if (_ship == null)
            {
                Debug.LogError("GravityLockSystem requires PlayerShipController component!");
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (_planetsInRange.Count > 0)
            {
                UpdateLockTarget();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var body = other.GetComponent<CelestialBodyController>();
            if (body != null && !body.IsStar && !_planetsInRange.Contains(body))
            {
                _planetsInRange.Add(body);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var body = other.GetComponent<CelestialBodyController>();
            if (body != null)
            {
                _planetsInRange.Remove(body);

                if (_lockedPlanet == body)
                {
                    ReleaseLock();
                }
            }
        }

        private void UpdateLockTarget()
        {
            CelestialBodyController nearest = null;
            float minDist = float.MaxValue;
            Vector2 shipPos = transform.position;

            foreach (var body in _planetsInRange)
            {
                if (body == null) continue;

                float distance = Vector2.Distance(shipPos, body.transform.position);

                if (distance < minDist)
                {
                    nearest = body;
                    minDist = distance;
                }
            }

            // Only change lock target if it's actually a different planet
            // Use ReferenceEquals to ensure we're comparing the same object instance
            if (!ReferenceEquals(nearest, _lockedPlanet))
            {
                if (nearest != null)
                {
                    // Found a new planet to lock to - reset grace timer and lock immediately
                    _timeSinceLeftRange = 0f;
                    LockToPlanet(nearest);
                }
                else if (_lockedPlanet != null)
                {
                    // Planet left range - start grace period timer
                    _timeSinceLeftRange += Time.fixedDeltaTime;

                    // Only unlock after grace period expires
                    if (_timeSinceLeftRange >= unlockGracePeriod)
                    {
                        ReleaseLock();
                        _timeSinceLeftRange = 0f;
                    }
                }
            }
            else
            {
                // Still locked to same planet - reset grace timer
                _timeSinceLeftRange = 0f;
            }
        }

        private void LockToPlanet(CelestialBodyController planet)
        {
            _lockedPlanet = planet;
            _indicatorFadeTime = 0f;
            _isFadingOut = false;
            ShowLockIndicator();
        }

        private void ReleaseLock()
        {
            if (_lockedPlanet != null)
            {
                _lockedPlanet = null;
                _indicatorFadeTime = 0f;
                _isFadingOut = true;
            }
        }


        private void ShowLockIndicator()
        {
            if (lockIndicatorPrefab == null)
                return;

            if (_lockIndicator == null)
            {
                _lockIndicator = Instantiate(lockIndicatorPrefab);
            }

            _lockIndicator.SetActive(true);
        }

        private void HideLockIndicator()
        {
            if (_lockIndicator != null)
            {
                _lockIndicator.SetActive(false);
            }
        }

        private void Update()
        {
            // Update fade time
            if (_indicatorFadeTime < indicatorFadeDuration)
            {
                _indicatorFadeTime += Time.deltaTime;
            }

            // Handle fade-out completion
            if (_isFadingOut && _indicatorFadeTime >= indicatorFadeDuration)
            {
                HideLockIndicator();
                _isFadingOut = false;
            }
        }

        private void LateUpdate()
        {
            // Update indicator position/visuals AFTER all physics is done (prevents jitter)
            if (_lockIndicator != null && _lockIndicator.activeSelf && _lockedPlanet != null)
            {
                _lockIndicator.transform.position = _lockedPlanet.transform.position;

                float scale = _lockedPlanet.VisualRadius * 2.5f;
                _lockIndicator.transform.localScale = Vector3.one * scale;

                // Fade indicator alpha
                var spriteRenderer = _lockIndicator.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    float progress = Mathf.Clamp01(_indicatorFadeTime / indicatorFadeDuration);
                    float alpha = _isFadingOut ? (1f - progress) * 0.8f : progress * 0.8f;

                    Color color = spriteRenderer.color;
                    color.a = alpha;
                    spriteRenderer.color = color;
                }
            }
        }

        private void OnDestroy()
        {
            if (_lockIndicator != null)
            {
                Destroy(_lockIndicator);
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            Gizmos.color = Color.yellow;
            foreach (var body in _planetsInRange)
            {
                if (body != null)
                {
                    Gizmos.DrawLine(transform.position, body.transform.position);
                }
            }

            if (_lockedPlanet != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _lockedPlanet.transform.position);
            }
        }
    }
}
