# Mobile Controls Specification
## Milo's Space Adventure - Input System Design

## Overview

This document defines the input control system for Milo's Space Adventure, designed for ages 5-8 with simultaneous PC and mobile support from Phase 1.

**Target Platforms:**
- **PC**: Windows, Mac, Linux (keyboard + mouse)
- **Mobile**: iOS, Android (touch + optional physical keyboard)
- **Future**: Gamepad support (Phase 4+)

**Design Philosophy:**
- Simple, kid-friendly controls
- Virtual joystick for mobile touch (primary mobile control)
- WASD keyboard for PC (primary PC control)
- Large touch targets (ages 5-8 need bigger buttons)
- Clear visual feedback
- No complex gestures (single finger touch only)

## Control Schemes

### Primary Controls (Phase 1)

#### PC Controls (Keyboard)

| Input | Action | Behavior |
|-------|--------|----------|
| **W** | Thrust Forward | Hold to accelerate in current direction |
| **A** | Rotate Left | Hold to rotate ship counter-clockwise |
| **D** | Rotate Right | Hold to rotate ship clockwise |
| **S** | Scan | Press to scan nearby planet/system |
| **Space** | Brake | Hold to slow down (drag applied) |
| **Mouse Click** | (Future) Point-to-Move | Optional in Phase 4+ |

**Key Characteristics:**
- Hold-to-thrust (not toggle)
- Rotation is separate from movement (Asteroids-style)
- Simple 5-key setup (easy for kids)

#### Mobile Controls (Touch)

| Input | Action | Behavior |
|-------|--------|----------|
| **Virtual Joystick (Left)** | Rotation | Drag to rotate ship left/right |
| **Boost Button (Right Bottom)** | Thrust Forward | Tap to thrust in current direction |
| **Brake Button (Right Middle)** | Brake | Hold to slow down |
| **Scan Button (Right Top)** | Scan | Tap to scan nearby planet/system |

**Key Characteristics:**
- Virtual joystick controls rotation ONLY (separates rotation from thrust)
- Separate boost/brake buttons for thrust control
- Matches PC mental model (rotation vs thrust are separate actions)
- Large touch targets (minimum 80px on mobile)
- Fixed joystick position (not floating)
- Visual feedback on touch

### Future Controls (Phase 4+)

| Input | Action | Notes |
|-------|--------|-------|
| **Gamepad** | Full support | Xbox/PlayStation controller mapping |
| **Point-to-Move** | Optional PC control | Click to fly toward cursor |
| **Pinch Zoom** | Camera zoom | Mobile gesture for minimap |

## Virtual Joystick Design

### Visual Design

```
┌─────────────────────────────────────┐
│                                     │
│                                     │
│                           ┌──────┐  │
│                           │  S   │  │ Scan Button
│                           └──────┘  │ (80x80px, top)
│  ┌─────┐                  ┌──────┐ │
│  │  ○  │                  │ ⬛   │ │ Brake Button
│  │ ┃   │                  └──────┘ │ (80x80px, middle)
│  │ ○   │                  ┌──────┐ │
│  └─────┘                  │  ↑   │ │ Boost Button
│  Joystick                 └──────┘ │ (80x80px, bottom)
│  (120x120px)                        │
│  (rotation only)                    │
└─────────────────────────────────────┘
 Bottom-left                Bottom-right
```

### Joystick Behavior

**Visuals:**
- **Background**: Semi-transparent circle (120px diameter)
- **Knob**: Solid circle (60px diameter)
- **Color**: Blue (#3B82F6) with 60% opacity
- **Active State**: Brightens to 90% opacity on touch

**Mechanics:**
- **Dead Zone**: 10% (no input if knob < 10% from center)
- **Max Distance**: Knob can move 50px from center
- **Return to Center**: Smooth spring animation (0.2s)
- **Input Mapping**:
  - Joystick horizontal axis → Ship rotation (left/right)
  - Joystick does NOT control thrust (thrust is separate boost/brake buttons)

**Physics:**
```csharp
// Pseudocode - Rotation only
Vector2 joystickInput = GetJoystickInput(); // (-1 to 1 for X axis)
float rotationInput = joystickInput.x; // Horizontal axis only

// Apply rotation based on joystick horizontal axis
if (Mathf.Abs(rotationInput) > 0.1f) // Dead zone
{
    shipController.ApplyRotation(rotationInput, rotationSpeed * Time.deltaTime);
}

// Thrust is handled by separate boost/brake buttons (not joystick)
```

### Button Design

**Boost Button:**
- **Size**: 80x80px (comfortable for small fingers)
- **Icon**: ↑ or custom thrust icon
- **Label**: "BOOST" (large, readable font)
- **Color**: Green (#10B981)
- **Feedback**: Pulse animation on tap, sound effect

**Brake Button:**
- **Size**: 80x80px
- **Icon**: ⬛ or custom brake icon
- **Label**: "BRAKE" or no label (icon-only)
- **Color**: Red (#EF4444)
- **Feedback**: Held state shows darker red, sound effect

**Scan Button:**
- **Size**: 80x80px
- **Icon**: 🔍 or custom scan icon
- **Label**: "SCAN" (large, readable font)
- **Color**: Blue (#3B82F6)
- **Feedback**: Pulse animation on tap, sound effect

### Layout

**Portrait Mode** (Primary for phones):
```
┌───────────────┐
│               │
│   Game View   │
│               │
│               │
│               │
│ ┌──┐      ┌─┐ │
│ │○ │      │S│ │
│ └──┘      └─┘ │
│           ┌─┐ │
│           │B│ │
└───────────└─┘─┘
```

**Landscape Mode** (Primary for tablets):
```
┌────────────────────────────┐
│                            │
│       Game View            │
│                            │
│ ┌──┐              ┌─┐ ┌─┐ │
│ │○ │              │S│ │B│ │
│ └──┘              └─┘ └─┘ │
└────────────────────────────┘
```

## Input Manager Architecture

### Unity Input System

**Package**: com.unity.inputsystem (New Input System)
**Rationale**: Better mobile support, easier rebinding, cross-platform

### InputActionAsset Structure

```
PlayerControls.inputactions
├─ Ship Movement
│  ├─ Thrust (WASD / Virtual Joystick)
│  ├─ Rotate (A/D keys)
│  └─ Brake (Space / Brake button)
├─ Scanning
│  └─ Scan (S key / Scan button)
└─ UI Navigation
   └─ Click (Mouse / Touch)
```

### Code Structure

```csharp
// Assets/Scripts/Input/InputManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerControls _controls;
    private InputDevice _currentDevice;

    // Input values
    public Vector2 ThrustInput { get; private set; }
    public bool IsBraking { get; private set; }
    public bool ScanPressed { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _controls = new PlayerControls();
    }

    private void OnEnable()
    {
        _controls.ShipMovement.Enable();
        _controls.Scanning.Enable();

        _controls.ShipMovement.Thrust.performed += OnThrust;
        _controls.ShipMovement.Thrust.canceled += OnThrustCanceled;
        _controls.ShipMovement.Brake.performed += OnBrake;
        _controls.ShipMovement.Brake.canceled += OnBrakeCanceled;
        _controls.Scanning.Scan.performed += OnScan;

        // Detect device changes
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        _controls.ShipMovement.Disable();
        _controls.Scanning.Disable();
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnThrust(InputAction.CallbackContext context)
    {
        ThrustInput = context.ReadValue<Vector2>();
    }

    private void OnThrustCanceled(InputAction.CallbackContext context)
    {
        ThrustInput = Vector2.zero;
    }

    private void OnBrake(InputAction.CallbackContext context)
    {
        IsBraking = true;
    }

    private void OnBrakeCanceled(InputAction.CallbackContext context)
    {
        IsBraking = false;
    }

    private void OnScan(InputAction.CallbackContext context)
    {
        ScanPressed = true; // Consumed by PlayerShipController
    }

    public void ConsumeScanInput()
    {
        ScanPressed = false;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            var action = obj as InputAction;
            _currentDevice = action?.activeControl?.device;

            // Show/hide mobile UI based on device
            UpdateUIVisibility();
        }
    }

    private void UpdateUIVisibility()
    {
        bool isTouchDevice = _currentDevice is Touchscreen;
        MobileUI.Instance?.SetVisible(isTouchDevice);
    }

    public bool IsMobileDevice()
    {
        return _currentDevice is Touchscreen || Application.isMobilePlatform;
    }
}
```

### Virtual Joystick Component

```csharp
// Assets/Scripts/Input/VirtualJoystick.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("References")]
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _knob;

    [Header("Settings")]
    [SerializeField] private float _maxDistance = 50f;
    [SerializeField] private float _deadZone = 0.1f;
    [SerializeField] private float _returnSpeed = 5f;

    private Vector2 _inputVector;
    private bool _isActive;

    public Vector2 InputVector => _inputVector;
    public float Magnitude => _inputVector.magnitude;
    public float Angle => Mathf.Atan2(_inputVector.y, _inputVector.x) * Mathf.Rad2Deg;

    private void Update()
    {
        // Smoothly return to center when not touched
        if (!_isActive && _knob.anchoredPosition != Vector2.zero)
        {
            _knob.anchoredPosition = Vector2.Lerp(_knob.anchoredPosition, Vector2.zero, _returnSpeed * Time.deltaTime);

            if (_knob.anchoredPosition.magnitude < 1f)
            {
                _knob.anchoredPosition = Vector2.zero;
                _inputVector = Vector2.zero;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isActive = true;
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isActive = false;
        _inputVector = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background, eventData.position, eventData.pressEventCamera, out position))
        {
            // Clamp to max distance
            position = Vector2.ClampMagnitude(position, _maxDistance);
            _knob.anchoredPosition = position;

            // Calculate input vector
            Vector2 normalizedInput = position / _maxDistance;

            // Apply dead zone
            if (normalizedInput.magnitude < _deadZone)
            {
                _inputVector = Vector2.zero;
            }
            else
            {
                // Remap magnitude from [deadZone, 1] to [0, 1]
                float remappedMagnitude = (normalizedInput.magnitude - _deadZone) / (1f - _deadZone);
                _inputVector = normalizedInput.normalized * remappedMagnitude;
            }
        }
    }
}
```

### PlayerShipController Integration

```csharp
// Assets/Scripts/Ship/PlayerShipController.cs
using UnityEngine;

public class PlayerShipController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _thrustForce = 5f;
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private float _drag = 0.98f;
    [SerializeField] private float _brakeForce = 0.9f;

    private Vector2 _velocity;
    private float _currentRotation;

    private void FixedUpdate()
    {
        HandleInput();
        ApplyPhysics();
    }

    private void HandleInput()
    {
        Vector2 rotationInput = InputManager.Instance.RotationInput;
        bool isBoosting = InputManager.Instance.IsBoosting;
        bool isBraking = InputManager.Instance.IsBraking;

        // Rotation (both PC and mobile use same rotation input)
        if (Mathf.Abs(rotationInput.x) > 0.1f)
        {
            _currentRotation += rotationInput.x * _rotationSpeed * Time.fixedDeltaTime;
        }

        // Thrust forward (PC: W key, Mobile: Boost button)
        if (isBoosting)
        {
            Vector2 forward = new Vector2(Mathf.Sin(_currentRotation * Mathf.Deg2Rad),
                                           Mathf.Cos(_currentRotation * Mathf.Deg2Rad));
            _velocity += forward * _thrustForce * Time.fixedDeltaTime;
        }

        // Apply brake (both PC and mobile)
        if (isBraking)
        {
            _velocity *= _brakeForce;
        }
        else
        {
            // Natural drag
            _velocity *= _drag;
        }

        // Clamp speed
        if (_velocity.magnitude > _maxSpeed)
        {
            _velocity = _velocity.normalized * _maxSpeed;
        }
    }

    private void ApplyPhysics()
    {
        transform.position += (Vector3)(_velocity * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0, 0, _currentRotation);
    }
}
```

## Mobile UI Canvas Setup

### Hierarchy

```
Canvas (Screen Space - Overlay)
├─ MobileControls (anchored bottom)
│  ├─ VirtualJoystick (bottom-left)
│  │  ├─ Background (Image)
│  │  └─ Knob (Image)
│  ├─ ScanButton (bottom-right)
│  │  ├─ Icon (Image)
│  │  └─ Label (Text)
│  └─ BrakeButton (bottom-right, above scan)
│     ├─ Icon (Image)
│     └─ Label (Text)
```

### Anchoring

**VirtualJoystick:**
- Anchor: Bottom-Left
- Pivot: (0, 0)
- Position: (40, 40) from bottom-left
- Size: 120x120

**ScanButton:**
- Anchor: Bottom-Right
- Pivot: (1, 0)
- Position: (-40, 40) from bottom-right
- Size: 80x80

**BrakeButton:**
- Anchor: Bottom-Right
- Pivot: (1, 0)
- Position: (-40, 140) from bottom-right (100px above scan)
- Size: 80x80

### Safe Area Handling

```csharp
// Assets/Scripts/UI/SafeAreaHandler.cs
using UnityEngine;

public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
    }
}
```

## Accessibility Features

### Large Touch Targets

**Minimum Size:**
- Buttons: 80x80px (1cm² on most phones)
- Joystick: 120x120px
- Spacing: Minimum 20px between interactive elements

### Visual Feedback

**Touch Feedback:**
- Scale animation on press (1.0 → 1.1 → 1.0)
- Color change (normal → bright → normal)
- Ripple effect for scan button
- Haptic feedback (mobile vibration)

```csharp
// Assets/Scripts/UI/ButtonFeedback.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image _image;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _pressedColor = Color.yellow;
    [SerializeField] private AudioClip _pressSound;

    public void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(PressAnimation());

        // Haptic feedback (mobile)
        if (Application.isMobilePlatform)
        {
            Handheld.Vibrate();
        }

        // Sound feedback
        if (_pressSound != null)
        {
            AudioManager.Instance?.PlaySFX(_pressSound);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        _image.color = _normalColor;
        transform.localScale = Vector3.one;
    }

    private IEnumerator PressAnimation()
    {
        // Scale up
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.1f;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, t);
            _image.color = Color.Lerp(_normalColor, _pressedColor, t);
            yield return null;
        }
    }
}
```

### Colorblind Support

**Button Colors:**
- Scan: Green (#10B981) + 🔍 icon
- Brake: Red (#EF4444) + ⬛ icon
- Always pair colors with icons (not color-only)

### Audio Feedback

**Sound Effects:**
- Thrust: Continuous engine hum (pitch scales with magnitude)
- Brake: Short brake sound
- Scan: Sonar ping
- Button press: Soft click

## Testing Strategy

### Manual Testing Checklist

**PC (Keyboard):**
- [ ] W key thrusts forward
- [ ] A/D rotate ship smoothly
- [ ] S key triggers scan
- [ ] Space brakes ship
- [ ] Controls responsive at 60 FPS

**Mobile (Touch):**
- [ ] Joystick appears on touch
- [ ] Joystick controls thrust direction
- [ ] Joystick magnitude affects thrust strength
- [ ] Scan button tappable
- [ ] Brake button hold works
- [ ] UI scales correctly on different screen sizes
- [ ] Safe area respected (iPhone notch, etc.)
- [ ] Controls responsive at 30 FPS

**Cross-Platform:**
- [ ] Switching from keyboard to touch updates UI
- [ ] Switching from touch to keyboard hides mobile UI
- [ ] Input doesn't double-trigger on hybrid devices

### Automated Testing

```csharp
// Assets/Tests/PlayMode/InputTests.cs
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class InputTests
{
    private InputManager _inputManager;

    [SetUp]
    public void SetUp()
    {
        GameObject inputObj = new GameObject("InputManager");
        _inputManager = inputObj.AddComponent<InputManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_inputManager.gameObject);
    }

    [UnityTest]
    public IEnumerator ThrustInput_WKey_ReturnsUpVector()
    {
        // Simulate W key press
        // (Requires Input System test fixtures)
        yield return null;

        // Assert thrust input is (0, 1)
        Assert.AreEqual(Vector2.up, _inputManager.ThrustInput, "W key should produce up thrust");
    }

    [Test]
    public void VirtualJoystick_DeadZone_ReturnsZero()
    {
        GameObject joystickObj = new GameObject("Joystick");
        VirtualJoystick joystick = joystickObj.AddComponent<VirtualJoystick>();

        // Simulate small input within dead zone (< 10%)
        // (Test implementation depends on joystick setup)

        Assert.AreEqual(Vector2.zero, joystick.InputVector, "Dead zone should return zero input");
    }
}
```

## Performance Considerations

### Mobile Optimization

**UI Draw Calls:**
- Combine joystick sprites into single atlas
- Use sprite masking for joystick knob
- Limit button animations to scale/color (no rotation)

**Input Polling:**
- Process touch input in Update() (not FixedUpdate)
- Cache RectTransform references
- Avoid allocations in touch handlers

**Target Performance:**
- PC: 60 FPS minimum
- Mobile: 30 FPS minimum
- Input latency: <50ms

## Future Enhancements (Phase 4+)

### Point-to-Move (Optional PC Control)

```csharp
// Future: Optional point-to-move for PC
if (Mouse.current.leftButton.wasPressedThisFrame)
{
    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    Vector2 direction = (mousePos - transform.position).normalized;
    shipController.SetAutoMove(direction);
}
```

### Gamepad Support

**Xbox/PlayStation Mapping:**
- Left Stick: Thrust direction (like virtual joystick)
- A/X Button: Scan
- B/Circle: Brake
- Right Stick: Camera pan (future)

### Gesture Controls

**Mobile Gestures (Phase 5+):**
- Pinch zoom: Minimap zoom
- Two-finger drag: Camera pan
- Double tap: Quick scan

## Summary

**Key Takeaways:**
1. **Virtual joystick** is primary mobile control (kid-friendly)
2. **WASD** is primary PC control (familiar)
3. **Unity Input System** for cross-platform support
4. **Large touch targets** for ages 5-8
5. **Visual + audio feedback** for all interactions
6. **Automatic UI switching** between touch/keyboard
7. **Safe area handling** for modern phones (notches, etc.)

**Phase 1 Scope:**
- Virtual joystick + WASD + Scan + Brake
- Basic mobile UI with accessibility
- Cross-platform input detection

**Future Scope (Phase 4+):**
- Point-to-move (optional PC)
- Gamepad support
- Advanced gestures
- Rebindable controls

**References:**
- See `tech-plan.md` for PlayerShipController implementation
- See `testing-strategy.md` for input testing approach
- See `performance-targets.md` for FPS requirements
