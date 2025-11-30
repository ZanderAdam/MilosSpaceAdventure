using UnityEngine;
using System.Collections.Generic;

namespace MilosAdventure.Player
{
    public class GravityLockSystem : MonoBehaviour
    {
        [Header("Transition Settings")]
        [SerializeField] private float lockTransitionDuration = 0.8f;
        [SerializeField] private float unlockTransitionDuration = 0.5f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject lockIndicatorPrefab;

        private PlayerShipController _ship;
        private CelestialBodyController _lockedPlanet;
        private CelestialBodyController _previousLockedPlanet;
        private GameObject _lockIndicator;
        private List<CelestialBodyController> _planetsInRange = new List<CelestialBodyController>();
        private float _lockTransitionTime = 0f;
        private float _unlockTransitionTime = 0f;
        private bool _isUnlocking = false;

        public bool IsLocked => _lockedPlanet != null;
        public CelestialBodyController LockedPlanet => _lockedPlanet;

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

                if (_lockedPlanet != null)
                {
                    ApplyOrbitalMovement();
                }
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

            if (nearest != _lockedPlanet)
            {
                if (nearest != null)
                {
                    LockToPlanet(nearest);
                }
                else
                {
                    ReleaseLock();
                }
            }
        }

        private void LockToPlanet(CelestialBodyController planet)
        {
            _lockedPlanet = planet;
            _lockTransitionTime = 0f;
            ShowLockIndicator();
        }

        private void ReleaseLock()
        {
            if (_lockedPlanet != null)
            {
                _previousLockedPlanet = _lockedPlanet;
                _lockedPlanet = null;
                _isUnlocking = true;
                _unlockTransitionTime = 0f;
            }
        }

        private void ApplyOrbitalMovement()
        {
            if (_ship == null)
                return;

            float influence = 0f;
            CelestialBodyController targetPlanet = null;

            if (_isUnlocking)
            {
                // Unlock transition: fade from 100% to 0%
                _unlockTransitionTime += Time.fixedDeltaTime;
                float unlockProgress = Mathf.Clamp01(_unlockTransitionTime / unlockTransitionDuration);

                // Ease-out cubic for smooth release
                float easedProgress = 1f - Mathf.Pow(1f - unlockProgress, 3f);
                influence = 1f - easedProgress;

                targetPlanet = _previousLockedPlanet;

                if (unlockProgress >= 1f)
                {
                    _isUnlocking = false;
                    _previousLockedPlanet = null;
                    HideLockIndicator();
                    return;
                }
            }
            else if (_lockedPlanet != null)
            {
                // Lock transition: fade from 0% to 100%
                if (_lockTransitionTime < lockTransitionDuration)
                {
                    _lockTransitionTime += Time.fixedDeltaTime;
                }

                float lockProgress = Mathf.Clamp01(_lockTransitionTime / lockTransitionDuration);

                // Ease-out cubic for gentle catch feeling
                influence = 1f - Mathf.Pow(1f - lockProgress, 3f);

                targetPlanet = _lockedPlanet;
            }
            else
            {
                return;
            }

            if (targetPlanet != null)
            {
                Vector2 orbitalVelocity = targetPlanet.GetOrbitalVelocity();
                Vector2 orbitalMovement = orbitalVelocity * Time.fixedDeltaTime * influence;

                _ship.ApplyOrbitalOffset(orbitalMovement);
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
            if (_lockIndicator != null && _lockIndicator.activeSelf)
            {
                CelestialBodyController targetPlanet = _lockedPlanet ?? _previousLockedPlanet;

                if (targetPlanet != null)
                {
                    _lockIndicator.transform.position = targetPlanet.transform.position;

                    float scale = targetPlanet.VisualRadius * 2.5f;
                    _lockIndicator.transform.localScale = Vector3.one * scale;

                    // Fade indicator alpha with transition progress
                    var spriteRenderer = _lockIndicator.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        float alpha = 0f;

                        if (_isUnlocking)
                        {
                            float unlockProgress = Mathf.Clamp01(_unlockTransitionTime / unlockTransitionDuration);
                            alpha = (1f - unlockProgress) * 0.8f;
                        }
                        else if (_lockedPlanet != null)
                        {
                            float lockProgress = Mathf.Clamp01(_lockTransitionTime / lockTransitionDuration);
                            alpha = lockProgress * 0.8f;
                        }

                        Color color = spriteRenderer.color;
                        color.a = alpha;
                        spriteRenderer.color = color;
                    }
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
