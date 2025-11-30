using UnityEngine;

namespace MilosAdventure.Player
{
    public class LockIndicatorPulse : MonoBehaviour
    {
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.1f;

        private Vector3 _baseScale;

        private void Start()
        {
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            float pulse = 1f + pulseAmount * Mathf.Sin(Time.time * pulseSpeed);
            transform.localScale = _baseScale * pulse;
        }
    }
}
