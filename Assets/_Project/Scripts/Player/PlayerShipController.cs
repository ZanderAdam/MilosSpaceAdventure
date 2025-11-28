using UnityEngine;

/// <summary>
/// Controls player ship movement with keyboard/gamepad input.
/// Implements momentum-based physics with thrust, drag, and rotation.
/// </summary>
public class PlayerShipController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float thrustForce = 8f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float drag = 0.5f;

    [Header("Boundaries")]
    [SerializeField] private float boundarySize = 50f;

    [Header("Visuals")]
    [SerializeField] private TrailRenderer thrusterTrail;
    [SerializeField] private ParticleSystem thrusterParticles;

    [Header("Mobile Controls")]
    [SerializeField] private MobileControls _mobileControls;

    private Vector2 _velocity;
    private bool _isThrusting;

    public float CurrentSpeed => _velocity.magnitude;
    public Vector2 Position => transform.position;

    private void Update()
    {
        HandleInput();
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        ApplyThrust();
        ApplyDrag();
        ApplyMovement();
        ClampToBoundary();
    }

    private void HandleInput()
    {
        float rotationInput = 0f;
        bool thrustInput = false;
        bool brakeInput = false;

        if (_mobileControls != null && _mobileControls.IsMobileControlsActive)
        {
            Vector2 joystickDir = _mobileControls.JoystickDirection;
            rotationInput = -joystickDir.x;
            thrustInput = joystickDir.y > 0.1f;
            brakeInput = _mobileControls.IsBrakePressed;
        }
        else
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                rotationInput = 1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                rotationInput = -1f;

            float gamepadH = Input.GetAxis("Horizontal");
            if (Mathf.Abs(gamepadH) > 0.1f)
                rotationInput = -gamepadH;

            thrustInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

            float gamepadV = Input.GetAxis("Vertical");
            if (gamepadV > 0.1f) thrustInput = true;

            brakeInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || gamepadV < -0.1f;
        }

        // Rotate the ship
        transform.Rotate(0, 0, rotationInput * rotationSpeed * Time.deltaTime);
        _isThrusting = thrustInput;

        // Braking
        if (brakeInput && _velocity.magnitude > 0.1f)
        {
            _velocity *= 0.95f;
        }
    }

    private void ApplyThrust()
    {
        if (_isThrusting)
        {
            // Use Unity's transform.right - ship points right by default (0° rotation)
            Vector2 forwardDirection = transform.right;
            _velocity += forwardDirection * thrustForce * Time.fixedDeltaTime;

            // Clamp to max speed
            if (_velocity.magnitude > maxSpeed)
                _velocity = _velocity.normalized * maxSpeed;
        }
    }

    private void ApplyDrag()
    {
        if (!_isThrusting && _velocity.magnitude > 0.01f)
        {
            _velocity *= (1f - drag * Time.fixedDeltaTime);

            // Stop completely if very slow
            if (_velocity.magnitude < 0.05f)
                _velocity = Vector2.zero;
        }
    }

    private void ApplyMovement()
    {
        transform.position += (Vector3)(_velocity * Time.fixedDeltaTime);
    }

    private void ClampToBoundary()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -boundarySize, boundarySize);
        pos.y = Mathf.Clamp(pos.y, -boundarySize, boundarySize);
        transform.position = pos;
    }

    private void UpdateVisuals()
    {
        if (thrusterTrail != null)
            thrusterTrail.emitting = _isThrusting;

        if (thrusterParticles != null)
        {
            if (_isThrusting && !thrusterParticles.isPlaying)
                thrusterParticles.Play();
            else if (!_isThrusting && thrusterParticles.isPlaying)
                thrusterParticles.Stop();
        }
    }
}
