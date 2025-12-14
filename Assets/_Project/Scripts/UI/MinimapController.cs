using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MilosAdventure.UI.Utils;
using MilosAdventure.UI.CustomElements;
using MilosAdventure.Data;

public enum MinimapPosition
{
    TopRight,
    TopLeft,
    BottomRight,
    BottomLeft
}

/// <summary>
/// UI Toolkit-based minimap controller.
/// Displays player and celestial body positions on a compact corner minimap.
/// </summary>
public class MinimapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private StarSystemRenderer starSystem;
    [SerializeField] private Transform playerShip;
    [SerializeField] private Camera mainCamera;

    [Header("Minimap Settings")]
    [SerializeField]
    [Tooltip("Sprite to display for the player ship on minimap")]
    private Sprite playerIconSprite;

    [SerializeField]
    [Range(0.05f, 0.5f)]
    [Tooltip("Scale of player icon relative to sprite size (0.1 = 10% of original)")]
    private float playerIconScale = 0.1f;

    [SerializeField]
    [Range(200f, 1000f)]
    [Tooltip("Total world size to display")]
    private float worldSize = 800f;

    [SerializeField]
    [Tooltip("Screen corner where minimap appears")]
    private MinimapPosition position = MinimapPosition.TopRight;

    [SerializeField]
    [Range(10f, 200f)]
    [Tooltip("Minimap size on desktop in pixels")]
    private float desktopSize = 150f;

    [SerializeField]
    [Range(10f, 150f)]
    [Tooltip("Minimap size on mobile in pixels")]
    private float mobileSize = 120f;

    [SerializeField]
    [Range(8f, 32f)]
    [Tooltip("Padding from screen edge in pixels")]
    private float edgePadding = 16f;

    private const float UPDATE_INTERVAL = 0.1f;

    private VisualElement _root;
    private VisualElement _minimapContainer;
    private VisualElement _planetsContainer;
    private VisualElement _playerIcon;
    private Dictionary<CelestialBodyController, VisualElement> _planetIcons = new Dictionary<CelestialBodyController, VisualElement>();
    private float _updateTimer;
    private Vector2 _minimapSize;
    private Vector2 _worldSizeVector;

    private bool IsMobilePlatform => Application.isMobilePlatform || Input.touchSupported;

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[MinimapController] No main camera found! Minimap will not update.");
            }
        }

        _root = uiDocument.rootVisualElement;
        Debug.Log($"[MinimapController] Root element: {_root}, child count: {_root?.childCount}");

        _minimapContainer = _root.Q("minimap-container");
        _planetsContainer = _root.Q("planets-container");
        _playerIcon = _root.Q("player-icon");

        Debug.Log($"[MinimapController] Container: {_minimapContainer}, Planets: {_planetsContainer}, Player: {_playerIcon}");

        if (_minimapContainer == null)
        {
            Debug.LogError("[MinimapController] minimap-container not found in UXML");
            return;
        }

        Debug.Log($"[MinimapController] Minimap container style - width: {_minimapContainer.style.width}, height: {_minimapContainer.style.height}, display: {_minimapContainer.style.display}");

        if (_playerIcon != null && playerIconSprite != null)
        {
            float width = playerIconSprite.rect.width * playerIconScale;
            float height = playerIconSprite.rect.height * playerIconScale;

            _playerIcon.style.width = width;
            _playerIcon.style.height = height;
            _playerIcon.style.backgroundImage = new StyleBackground(playerIconSprite);
        }

        _worldSizeVector = new Vector2(worldSize, worldSize);

        UpdateMinimapSize();
        ApplyPosition();
        ApplySafeAreaPadding();
        SetupMobileButton();
    }

    private void Start()
    {
        SubscribeToSystemLoader();
    }

    private void OnDisable()
    {
        if (_minimapContainer != null && IsMobilePlatform)
        {
            _minimapContainer.UnregisterCallback<PointerDownEvent>(OnMinimapTapped);
        }

        if (StarSystemLoader.Instance != null)
        {
            StarSystemLoader.Instance.OnSystemLoaded -= OnSystemLoaded;
        }
    }

    private void UpdateMinimapSize()
    {
        bool isMobile = IsMobilePlatform;
        float size = isMobile ? mobileSize : desktopSize;
        _minimapSize = new Vector2(size, size);

        if (_minimapContainer != null)
        {
            _minimapContainer.style.width = size;
            _minimapContainer.style.height = size;

            if (isMobile)
            {
                _minimapContainer.AddToClassList("minimap--mobile");
            }
        }
    }

    private void ApplyPosition()
    {
        if (_minimapContainer == null) return;

        _minimapContainer.RemoveFromClassList("minimap--top-right");
        _minimapContainer.RemoveFromClassList("minimap--top-left");
        _minimapContainer.RemoveFromClassList("minimap--bottom-right");
        _minimapContainer.RemoveFromClassList("minimap--bottom-left");

        switch (position)
        {
            case MinimapPosition.TopRight:
                _minimapContainer.AddToClassList("minimap--top-right");
                break;
            case MinimapPosition.TopLeft:
                _minimapContainer.AddToClassList("minimap--top-left");
                break;
            case MinimapPosition.BottomRight:
                _minimapContainer.AddToClassList("minimap--bottom-right");
                break;
            case MinimapPosition.BottomLeft:
                _minimapContainer.AddToClassList("minimap--bottom-left");
                break;
        }
    }

    private void ApplySafeAreaPadding()
    {
        if (_minimapContainer == null) return;

        Rect safeArea = Screen.safeArea;
        float topPadding = Screen.height - (safeArea.y + safeArea.height);
        float bottomPadding = safeArea.y;
        float leftPadding = safeArea.x;
        float rightPadding = Screen.width - (safeArea.x + safeArea.width);

        switch (position)
        {
            case MinimapPosition.TopRight:
                if (topPadding > 0) _minimapContainer.style.top = edgePadding + topPadding;
                if (rightPadding > 0) _minimapContainer.style.right = edgePadding + rightPadding;
                break;
            case MinimapPosition.TopLeft:
                if (topPadding > 0) _minimapContainer.style.top = edgePadding + topPadding;
                if (leftPadding > 0) _minimapContainer.style.left = edgePadding + leftPadding;
                break;
            case MinimapPosition.BottomRight:
                if (bottomPadding > 0) _minimapContainer.style.bottom = edgePadding + bottomPadding;
                if (rightPadding > 0) _minimapContainer.style.right = edgePadding + rightPadding;
                break;
            case MinimapPosition.BottomLeft:
                if (bottomPadding > 0) _minimapContainer.style.bottom = edgePadding + bottomPadding;
                if (leftPadding > 0) _minimapContainer.style.left = edgePadding + leftPadding;
                break;
        }
    }

    private void SetupMobileButton()
    {
        if (IsMobilePlatform && _minimapContainer != null)
        {
            _minimapContainer.RegisterCallback<PointerDownEvent>(OnMinimapTapped);
        }
    }

    private void OnMinimapTapped(PointerDownEvent evt)
    {
        if (evt.button == 0)
        {
            Debug.Log("[MinimapController] Minimap tapped! (Interactive map not implemented yet)");
        }
    }

    private void SubscribeToSystemLoader()
    {
        if (StarSystemLoader.Instance != null)
        {
            StarSystemLoader.Instance.OnSystemLoaded += OnSystemLoaded;
        }
        else
        {
            Debug.LogWarning("[MinimapController] StarSystemLoader.Instance is null, cannot subscribe");
        }
    }

    private void OnSystemLoaded(StarSystemJson systemJson)
    {
        StartCoroutine(CreatePlanetIconsNextFrame());
    }

    private System.Collections.IEnumerator CreatePlanetIconsNextFrame()
    {
        yield return null;
        CreatePlanetIcons();
    }

    private void CreatePlanetIcons()
    {
        if (starSystem == null || _planetsContainer == null)
        {
            Debug.LogWarning($"[MinimapController] Cannot create planet icons - starSystem: {starSystem}, planetsContainer: {_planetsContainer}");
            return;
        }

        _planetsContainer.Clear();
        _planetIcons.Clear();

        CelestialBodyController[] bodies = starSystem.GetAllBodies();
        Debug.Log($"[MinimapController] GetAllBodies returned: {bodies?.Length ?? 0} bodies");

        if (bodies == null || bodies.Length == 0)
        {
            Debug.LogWarning("[MinimapController] No celestial bodies found in star system");
            return;
        }

        foreach (var body in bodies)
        {
            if (body == null) continue;

            var icon = new VisualElement();
            icon.name = $"planet-icon-{body.Id}";
            icon.AddToClassList("planet-icon");

            if (body.IsStar)
            {
                icon.AddToClassList("planet-icon--star");
            }
            else
            {
                icon.AddToClassList("planet-icon--planet");
            }

            Color bodyColor = GetPlanetColor(body);
            icon.style.backgroundColor = bodyColor;

            _planetsContainer.Add(icon);
            _planetIcons[body] = icon;
        }
    }

    private Color GetPlanetColor(CelestialBodyController body)
    {
        if (body.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            return spriteRenderer.color;
        }
        return body.IsStar ? Color.yellow : Color.cyan;
    }

    private void LateUpdate()
    {
        if (playerShip == null || _minimapContainer == null || mainCamera == null)
            return;

        _updateTimer += Time.deltaTime;

        if (_updateTimer >= UPDATE_INTERVAL)
        {
            _updateTimer = 0f;
            UpdateMinimapPositions();
        }
    }

    private void UpdateMinimapPositions()
    {
        if (playerShip == null) return;

        Vector3 playerPos = playerShip.position;
        Vector2 centerPos = new Vector2(_minimapSize.x / 2f, _minimapSize.y / 2f);

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
            var icon = kvp.Value;

            if (body != null && icon != null)
            {
                Vector3 relativePos = body.transform.position - playerPos;

                Vector2 uiPos = CoordinateConverter.WorldToUIPosition(
                    relativePos,
                    _worldSizeVector,
                    _minimapSize
                );

                icon.style.left = uiPos.x;
                icon.style.top = uiPos.y;
            }
        }
    }
}
