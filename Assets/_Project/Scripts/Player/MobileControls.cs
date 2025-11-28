using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages mobile-specific controls and UI elements.
/// Automatically detects mobile platform and shows/hides UI accordingly.
/// </summary>
public class MobileControls : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _mobileUIContainer;
    [SerializeField] private VirtualJoystick _virtualJoystick;
    [SerializeField] private Button _brakeButton;
    [SerializeField] private Button _scanButton;

    [Header("Settings")]
    [SerializeField] private bool _forceEnableMobileUI;

    private bool _isMobilePlatform;
    private bool _isBrakePressed;
    private bool _isScanPressed;

    /// <summary>
    /// Gets the current joystick input direction.
    /// </summary>
    public Vector2 JoystickDirection => _virtualJoystick != null ? _virtualJoystick.Direction : Vector2.zero;

    /// <summary>
    /// Gets whether the brake button is currently pressed.
    /// </summary>
    public bool IsBrakePressed => _isBrakePressed;

    /// <summary>
    /// Gets whether the scan button is currently pressed.
    /// </summary>
    public bool IsScanPressed => _isScanPressed;

    /// <summary>
    /// Gets whether mobile controls are currently active.
    /// </summary>
    public bool IsMobileControlsActive => _isMobilePlatform || _forceEnableMobileUI;

    private void Awake()
    {
        DetectPlatform();
        SetupButtons();
        UpdateUIVisibility();
    }

    private void DetectPlatform()
    {
        _isMobilePlatform = Application.isMobilePlatform || Input.touchSupported;
    }

    private void SetupButtons()
    {
        if (_brakeButton != null)
        {
            _brakeButton.onClick.AddListener(() => { });

            var eventTrigger = _brakeButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown
            };
            pointerDown.callback.AddListener((data) => { _isBrakePressed = true; });
            eventTrigger.triggers.Add(pointerDown);

            var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp
            };
            pointerUp.callback.AddListener((data) => { _isBrakePressed = false; });
            eventTrigger.triggers.Add(pointerUp);
        }

        if (_scanButton != null)
        {
            _scanButton.onClick.AddListener(() => { });

            var eventTrigger = _scanButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown
            };
            pointerDown.callback.AddListener((data) => { _isScanPressed = true; });
            eventTrigger.triggers.Add(pointerDown);

            var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp
            };
            pointerUp.callback.AddListener((data) => { _isScanPressed = false; });
            eventTrigger.triggers.Add(pointerUp);
        }
    }

    private void UpdateUIVisibility()
    {
        if (_mobileUIContainer != null)
        {
            _mobileUIContainer.SetActive(IsMobileControlsActive);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateUIVisibility();
        }
    }
}
