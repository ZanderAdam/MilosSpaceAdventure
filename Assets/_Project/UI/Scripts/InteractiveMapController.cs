using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MilosAdventure.UI.Utils;
using MilosAdventure.Data;

/// <summary>
/// UI Toolkit-based interactive fullscreen system map.
/// Shows all celestial bodies with clickable icons and info panel.
/// </summary>
public class InteractiveMapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private StarSystemRenderer starSystem;
    [SerializeField] private Transform playerShip;
    [SerializeField] private Camera mainCamera;

    [Header("Map Settings")]
    [SerializeField]
    [Tooltip("Sprite to display for the player ship on map")]
    private Sprite playerIconSprite;

    [SerializeField]
    [Range(0.05f, 0.5f)]
    [Tooltip("Scale of player icon relative to sprite size")]
    private float playerIconScale = 0.15f;

    [SerializeField]
    [Range(500f, 2000f)]
    [Tooltip("Total world size to display")]
    private float worldSize = 1200f;

    [Header("Animation Settings")]
    [SerializeField]
    [Range(0.5f, 3.0f)]
    [Tooltip("Duration of pulse animation cycle in seconds")]
    private float pulseDuration = 1.5f;

    [SerializeField]
    [Range(0.05f, 0.3f)]
    [Tooltip("Scale amplitude for pulse effect")]
    private float pulseAmplitude = 0.15f;

    [SerializeField]
    [Range(0.1f, 2.0f)]
    [Tooltip("Fade-in transition duration in seconds")]
    private float fadeInDuration = 0.8f;

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Show touch target debug outlines for planet icons")]
    private bool showDebugOutlines = false;

    private const float UPDATE_INTERVAL = 0.1f;
    private const int ANIMATION_FPS = 60;
    private const int ANIMATION_UPDATE_MS = 16;

    private VisualElement _root;
    private VisualElement _overlay;
    private VisualElement _mapPanel;
    private VisualElement _planetsContainer;
    private VisualElement _playerIcon;
    private VisualElement _selectionHighlight;
    private VisualElement _infoPanel;
    private Button _closeButton;
    private Button _dimmedBackground;

    private Label _planetName;
    private Label _planetType;
    private Label _planetDescription;

    private Dictionary<CelestialBodyController, (VisualElement icon, EventCallback<PointerDownEvent> handler)> _planetIcons = new Dictionary<CelestialBodyController, (VisualElement icon, EventCallback<PointerDownEvent> handler)>();
    private CelestialBodyController _selectedPlanet;
    private float _updateTimer;
    private Vector2 _mapPanelSize;
    private Vector2 _worldSizeVector;
    private bool _isVisible;

    private IVisualElementScheduledItem _playerPulseAnimation;
    private IVisualElementScheduledItem _selectionPulseAnimation;
    private IVisualElementScheduledItem _fadeInAnimation;
    private EventCallback<GeometryChangedEvent> _geometryChangedHandler;
    private Vector3 _pulseScaleVector = Vector3.one;

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        _root = uiDocument.rootVisualElement;
        _overlay = _root.Q("fullscreen-overlay");
        _mapPanel = _root.Q("map-panel");
        _planetsContainer = _root.Q("planets-container");
        _playerIcon = _root.Q("player-icon");
        _selectionHighlight = _root.Q("selection-highlight");
        _infoPanel = _root.Q("info-panel");
        _closeButton = _root.Q<Button>("close-button");
        _dimmedBackground = _root.Q<Button>("dimmed-background");

        _planetName = _root.Q<Label>("planet-name");
        _planetType = _root.Q<Label>("planet-type");
        _planetDescription = _root.Q<Label>("planet-description");

        if (_playerIcon != null && playerIconSprite != null)
        {
            float width = playerIconSprite.rect.width * playerIconScale;
            float height = playerIconSprite.rect.height * playerIconScale;

            _playerIcon.style.width = width;
            _playerIcon.style.height = height;
            _playerIcon.style.backgroundImage = new StyleBackground(playerIconSprite);
        }

        _worldSizeVector = new Vector2(worldSize, worldSize);

        _geometryChangedHandler = OnMapPanelGeometryChanged;
        if (_mapPanel != null)
        {
            _mapPanel.RegisterCallback<GeometryChangedEvent>(_geometryChangedHandler);
        }

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        if (_closeButton != null)
        {
            _closeButton.clicked += HideMap;
        }

        if (_dimmedBackground != null)
        {
            _dimmedBackground.clicked += HideMap;
        }
    }

    private void OnDestroy()
    {
        StopAllAnimations();
        ClearPlanetIcons();

        if (_mapPanel != null)
        {
            _mapPanel.UnregisterCallback<GeometryChangedEvent>(_geometryChangedHandler);
        }

        if (_closeButton != null)
        {
            _closeButton.clicked -= HideMap;
        }

        if (_dimmedBackground != null)
        {
            _dimmedBackground.clicked -= HideMap;
        }
    }

    private void Update()
    {
        if (!_isVisible) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideMap();
        }
    }

    private void LateUpdate()
    {
        if (!_isVisible || playerShip == null || _mapPanel == null) return;

        _updateTimer += Time.deltaTime;

        if (_updateTimer >= UPDATE_INTERVAL)
        {
            _updateTimer = 0f;
            UpdateMapPositions();
        }
    }

    public void ShowMap()
    {
        if (_overlay == null || starSystem == null) return;

        _isVisible = true;
        _overlay.style.display = DisplayStyle.Flex;

        ApplySafeAreaPadding();
        CreatePlanetIcons();
        UpdateMapPositions();

        StartFadeInAnimation(_overlay);
        StartPulseAnimation(_playerIcon, ref _playerPulseAnimation);
    }

    public void HideMap()
    {
        if (_overlay == null) return;

        _isVisible = false;
        StopAllAnimations();
        _overlay.style.display = DisplayStyle.None;

        ClearSelection();
        ClearPlanetIcons();
    }

    public bool IsVisible => _isVisible;

    private void ApplySafeAreaPadding()
    {
#if UNITY_IOS || UNITY_ANDROID
        Rect safeArea = Screen.safeArea;
        float topInset = Screen.height - safeArea.yMax;
        float rightInset = Screen.width - safeArea.xMax;
        float bottomInset = safeArea.yMin;

        if (_closeButton != null)
        {
            _closeButton.style.top = 10f + topInset;
            _closeButton.style.right = 10f + rightInset;
        }

        if (_infoPanel != null)
        {
            _infoPanel.style.bottom = 20f + bottomInset;
        }
#endif
    }

    private void OnMapPanelGeometryChanged(GeometryChangedEvent evt)
    {
        _mapPanelSize = new Vector2(evt.newRect.width, evt.newRect.height);
    }

    private void CreatePlanetIcons()
    {
        if (starSystem == null || _planetsContainer == null) return;

        ClearPlanetIcons();

        CelestialBodyController[] bodies = starSystem.GetAllBodies();
        if (bodies == null || bodies.Length == 0) return;

        foreach (var body in bodies)
        {
            if (body == null) continue;

            var icon = new VisualElement();
            icon.name = $"map-planet-icon-{body.Id}";
            icon.AddToClassList("map-planet-icon");
            icon.AddToClassList("touchable");

            if (body.IsStar)
            {
                icon.AddToClassList("map-planet-icon--star");
            }
            else
            {
                icon.AddToClassList("map-planet-icon--planet");
            }

            Color bodyColor = GetPlanetColor(body);
            icon.style.backgroundColor = bodyColor;

            if (showDebugOutlines)
            {
                icon.AddToClassList("debug-outline");
            }

            EventCallback<PointerDownEvent> clickHandler = evt => OnPlanetClicked(body);
            icon.RegisterCallback<PointerDownEvent>(clickHandler);

            _planetsContainer.Add(icon);
            _planetIcons[body] = (icon, clickHandler);
        }
    }

    private void ClearPlanetIcons()
    {
        if (_planetsContainer == null) return;

        foreach (var kvp in _planetIcons)
        {
            if (kvp.Value.icon != null)
            {
                kvp.Value.icon.UnregisterCallback<PointerDownEvent>(kvp.Value.handler);
                kvp.Value.icon.RemoveFromHierarchy();
            }
        }

        _planetsContainer.Clear();
        _planetIcons.Clear();
    }

    private Color GetPlanetColor(CelestialBodyController body)
    {
        if (body.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            return spriteRenderer.color;
        }
        return body.IsStar ? Color.yellow : Color.cyan;
    }

    private void UpdateMapPositions()
    {
        if (playerShip == null || _mapPanelSize == Vector2.zero) return;

        Vector3 playerPos = playerShip.position;
        Vector2 centerPos = _mapPanelSize / 2f;

        if (_playerIcon != null)
        {
            _playerIcon.style.left = centerPos.x;
            _playerIcon.style.top = centerPos.y;

            float rotationZ = -playerShip.eulerAngles.z;
            _playerIcon.style.rotate = new Rotate(new Angle(rotationZ, AngleUnit.Degree));
        }

        foreach (var kvp in _planetIcons)
        {
            var body = kvp.Key;
            var icon = kvp.Value.icon;

            if (body != null && icon != null)
            {
                Vector3 relativePos = body.transform.position - playerPos;

                Vector2 uiPos = CoordinateConverter.WorldToUIPosition(
                    relativePos,
                    _worldSizeVector,
                    _mapPanelSize
                );

                icon.style.left = uiPos.x;
                icon.style.top = uiPos.y;
            }
        }

        if (_selectedPlanet != null && _selectionHighlight != null)
        {
            Vector3 selectedPos = _selectedPlanet.transform.position - playerPos;
            Vector2 highlightPos = CoordinateConverter.WorldToUIPosition(
                selectedPos,
                _worldSizeVector,
                _mapPanelSize
            );

            _selectionHighlight.style.left = highlightPos.x;
            _selectionHighlight.style.top = highlightPos.y;
        }
    }

    private void OnPlanetClicked(CelestialBodyController planet)
    {
        _selectedPlanet = planet;

        if (_selectionHighlight != null)
        {
            _selectionHighlight.style.display = DisplayStyle.Flex;
            StartPulseAnimation(_selectionHighlight, ref _selectionPulseAnimation);
        }

        if (_infoPanel != null)
        {
            _infoPanel.style.display = DisplayStyle.Flex;
            UpdateInfoPanel(planet);
        }
    }

    private void UpdateInfoPanel(CelestialBodyController planet)
    {
        if (_planetName != null)
        {
            _planetName.text = planet.BodyName;
        }

        if (_planetType != null)
        {
            string type = planet.IsStar ? "Star" : "Planet";
            _planetType.text = $"Type: {type}";
        }

        if (_planetDescription != null)
        {
            _planetDescription.text = planet.Description;
        }
    }

    private void ClearSelection()
    {
        _selectedPlanet = null;

        if (_selectionHighlight != null)
        {
            _selectionHighlight.style.display = DisplayStyle.None;
            StopPulseAnimation(ref _selectionPulseAnimation, _selectionHighlight);
        }

        if (_infoPanel != null)
        {
            _infoPanel.style.display = DisplayStyle.None;
        }
    }

    private void StartPulseAnimation(VisualElement element, ref IVisualElementScheduledItem animation)
    {
        if (element == null) return;

        StopPulseAnimation(ref animation, element);

        float startTime = Time.time;

        animation = element.schedule.Execute(() =>
        {
            float elapsed = Time.time - startTime;
            float progress = (elapsed % pulseDuration) / pulseDuration;

            float scale = 1f + Mathf.Sin(progress * Mathf.PI * 2f) * pulseAmplitude;
            _pulseScaleVector.x = scale;
            _pulseScaleVector.y = scale;
            element.style.scale = new Scale(_pulseScaleVector);
        }).Every(ANIMATION_UPDATE_MS);
    }

    private void StopPulseAnimation(ref IVisualElementScheduledItem animation, VisualElement element)
    {
        if (animation != null)
        {
            animation.Pause();
            animation = null;
        }

        if (element != null)
        {
            element.style.scale = new Scale(Vector3.one);
        }
    }

    private void StartFadeInAnimation(VisualElement element)
    {
        if (element == null) return;

        element.style.opacity = 0f;
        float startTime = Time.time;

        _fadeInAnimation = element.schedule.Execute(() =>
        {
            float elapsed = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsed / fadeInDuration);

            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            element.style.opacity = eased;

            if (progress >= 1f)
            {
                element.style.opacity = 1f;
                _fadeInAnimation?.Pause();
                _fadeInAnimation = null;
            }
        }).Every(ANIMATION_UPDATE_MS);
    }

    private void StopAllAnimations()
    {
        StopPulseAnimation(ref _playerPulseAnimation, _playerIcon);
        StopPulseAnimation(ref _selectionPulseAnimation, _selectionHighlight);

        if (_fadeInAnimation != null)
        {
            _fadeInAnimation.Pause();
            _fadeInAnimation = null;
        }

        if (_overlay != null)
        {
            _overlay.style.opacity = 1f;
        }
    }
}
