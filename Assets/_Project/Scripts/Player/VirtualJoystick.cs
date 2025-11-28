using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI-based virtual joystick for mobile input.
/// Provides drag-based directional input with visual feedback.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Components")]
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _knob;

    [Header("Settings")]
    [SerializeField] private float _deadZone = 0.1f;
    [SerializeField] private float _maxDistance = 50f;

    private Vector2 _inputDirection;
    private bool _isActive;
    private Vector2 _knobStartPosition;

    /// <summary>
    /// Gets the current normalized input direction.
    /// </summary>
    public Vector2 Direction => _inputDirection;

    /// <summary>
    /// Gets whether the joystick is currently being used.
    /// </summary>
    public bool IsActive => _isActive;

    private void Start()
    {
        if (_knob != null)
        {
            _knobStartPosition = _knob.anchoredPosition;
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
        _inputDirection = Vector2.zero;

        if (_knob != null)
        {
            _knob.anchoredPosition = _knobStartPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_background == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        Vector2 offset = localPoint - _background.anchoredPosition;
        float distance = offset.magnitude;

        Vector2 direction = offset.normalized;
        float clampedDistance = Mathf.Min(distance, _maxDistance);

        if (_knob != null)
        {
            _knob.anchoredPosition = _knobStartPosition + direction * clampedDistance;
        }

        float normalizedDistance = clampedDistance / _maxDistance;

        if (normalizedDistance < _deadZone)
        {
            _inputDirection = Vector2.zero;
        }
        else
        {
            _inputDirection = direction * normalizedDistance;
        }
    }

    private void OnValidate()
    {
        _deadZone = Mathf.Clamp01(_deadZone);
        _maxDistance = Mathf.Max(1f, _maxDistance);
    }
}
