using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MinimapPosition
{
    TopRight,
    TopLeft,
    BottomRight,
    BottomLeft
}

public class Minimap : MonoBehaviour
{
    [Header("Position")]
    [SerializeField]
    [Tooltip("Screen corner where minimap appears")]
    private MinimapPosition position = MinimapPosition.TopRight;

    [SerializeField]
    [Range(0.05f, 0.2f)]
    [Tooltip("Padding from screen edge as percentage of minimap size")]
    private float edgePaddingPercent = 0.1f;

    [Header("References")]
    [SerializeField] private StarSystemRenderer starSystem;
    [SerializeField] private Transform playerShip;
    [SerializeField] private Camera mainCamera;

    [Header("Minimap Settings")]
    [SerializeField]
    [Range(50f, 300f)]
    [Tooltip("Size of the minimap in pixels")]
    private float minimapSize = 150f;

    [SerializeField]
    [Range(200f, 1000f)]
    [Tooltip("Total world size to display (like room_width/room_height in Game Maker)")]
    private float worldSize = 800f;

    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
    [SerializeField] private Color starColor = Color.yellow;
    [SerializeField] private Color planetColor = Color.cyan;
    [SerializeField] private Color playerColor = Color.green;

    [SerializeField]
    [Range(2f, 10f)]
    [Tooltip("Size of planet dots on minimap")]
    private float planetDotSize = 4f;

    [SerializeField]
    [Range(2f, 10f)]
    [Tooltip("Size of player dot on minimap")]
    private float playerDotSize = 5f;

    private RectTransform _minimapContainer;
    private Image _backgroundImage;
    private RectTransform _playerDot;
    private Dictionary<CelestialBodyController, RectTransform> _bodyDots = new Dictionary<CelestialBodyController, RectTransform>();
    private Sprite _triangleSprite;
    private Sprite _circleSprite;

    private void Start()
    {
        Debug.Log("[Minimap] Start() called");
        _triangleSprite = CreateTriangleSprite();
        _circleSprite = CreateCircleSprite();
        UpdateMinimapSize();
        CreateMinimapUI();
        SubscribeToSystemLoader();
        SetupMobileButton();
    }

    private void OnValidate()
    {
        // Update position when inspector values change
        if (_minimapContainer != null)
        {
            _minimapContainer.sizeDelta = new Vector2(minimapSize, minimapSize);
            ApplyPosition();
        }
    }

    private void UpdateMinimapSize()
    {
        bool isMobile = Application.isMobilePlatform;
        if (isMobile)
        {
            minimapSize = 120f;
        }
        Debug.Log($"[Minimap] Platform: {(isMobile ? "Mobile" : "PC")}, Size: {minimapSize}px");
    }

    private void SetupMobileButton()
    {
        if (Application.isMobilePlatform && _minimapContainer != null)
        {
            Button btn = _minimapContainer.GetComponent<Button>();
            if (btn == null)
            {
                btn = _minimapContainer.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(OnMinimapTapped);
                Debug.Log("[Minimap] Added Button component for mobile tap-to-open");
            }
        }
    }

    private void OnMinimapTapped()
    {
        Debug.Log("[Minimap] Minimap tapped! (Interactive map not implemented yet)");
        // TODO: MapInputManager.Instance?.ToggleMap();
    }

    private Sprite CreateTriangleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        // Clear to transparent
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // Draw filled triangle pointing right (0 degrees in Unity)
        // Using simple scanline fill
        int centerY = size / 2;
        int leftX = (int)(size * 0.15f);
        int rightX = (int)(size * 0.85f);
        int baseHalfHeight = (int)(size * 0.3f);

        for (int x = leftX; x <= rightX; x++)
        {
            float t = (x - leftX) / (float)(rightX - leftX);
            int height = (int)(baseHalfHeight * (1f - t));

            for (int y = centerY - height; y <= centerY + height; y++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        Debug.Log($"[Minimap] Created triangle sprite: {sprite != null}, texture size: {size}x{size}");
        return sprite;
    }

    private Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[size * size];
        float radius = size / 2f;
        float centerX = size / 2f;
        float centerY = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance <= radius - 1f)
                {
                    pixels[y * size + x] = Color.white;
                }
                else if (distance <= radius)
                {
                    // Anti-aliasing edge
                    float alpha = radius - distance;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        Debug.Log($"[Minimap] Created circle sprite: {sprite != null}, texture size: {size}x{size}");
        return sprite;
    }

    private bool IsPointInTriangle(float px, float py, float x1, float y1, float x2, float y2, float x3, float y3)
    {
        float d1 = Sign(px, py, x1, y1, x2, y2);
        float d2 = Sign(px, py, x2, y2, x3, y3);
        float d3 = Sign(px, py, x3, y3, x1, y1);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    private float Sign(float px, float py, float x1, float y1, float x2, float y2)
    {
        return (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);
    }

    private void OnDisable()
    {
        if (StarSystemLoader.Instance != null)
        {
            StarSystemLoader.Instance.OnSystemLoaded -= OnSystemLoaded;
        }
    }

    private void SubscribeToSystemLoader()
    {
        if (StarSystemLoader.Instance != null)
        {
            StarSystemLoader.Instance.OnSystemLoaded += OnSystemLoaded;
            Debug.Log($"[Minimap] Subscribed to OnSystemLoaded event, Instance = {StarSystemLoader.Instance.name}");

            // Check if system is already loaded (race condition fix)
            var cachedSystem = StarSystemLoader.Instance.GetCachedSystem("sol/sol");
            if (cachedSystem != null)
            {
                Debug.Log($"[Minimap] System already cached with {cachedSystem.bodies.Count} bodies, creating dots immediately");
                StartCoroutine(CreateBodyDotsDelayed());
            }
        }
        else
        {
            Debug.LogError("[Minimap] StarSystemLoader.Instance is null! Cannot subscribe to OnSystemLoaded");
        }
    }

    private void OnSystemLoaded(MilosAdventure.Data.StarSystemJson system)
    {
        Debug.Log($"[Minimap] OnSystemLoaded called, JSON has {system.bodies.Count} bodies");
        StartCoroutine(CreateBodyDotsDelayed());
    }

    private System.Collections.IEnumerator CreateBodyDotsDelayed()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("[Minimap] Creating body dots after waiting one frame");
        CreateBodyDots();
    }

    private void ApplyPosition()
    {
        if (_minimapContainer == null) return;

        // Calculate padding based on minimap size (percentage)
        float padding = minimapSize * edgePaddingPercent;

        switch (position)
        {
            case MinimapPosition.TopRight:
                _minimapContainer.anchorMin = new Vector2(1, 1);
                _minimapContainer.anchorMax = new Vector2(1, 1);
                _minimapContainer.pivot = new Vector2(1, 1);
                _minimapContainer.anchoredPosition = new Vector2(-padding, -padding);
                break;

            case MinimapPosition.TopLeft:
                _minimapContainer.anchorMin = new Vector2(0, 1);
                _minimapContainer.anchorMax = new Vector2(0, 1);
                _minimapContainer.pivot = new Vector2(0, 1);
                _minimapContainer.anchoredPosition = new Vector2(padding, -padding);
                break;

            case MinimapPosition.BottomRight:
                _minimapContainer.anchorMin = new Vector2(1, 0);
                _minimapContainer.anchorMax = new Vector2(1, 0);
                _minimapContainer.pivot = new Vector2(1, 0);
                _minimapContainer.anchoredPosition = new Vector2(-padding, padding);
                break;

            case MinimapPosition.BottomLeft:
                _minimapContainer.anchorMin = new Vector2(0, 0);
                _minimapContainer.anchorMax = new Vector2(0, 0);
                _minimapContainer.pivot = new Vector2(0, 0);
                _minimapContainer.anchoredPosition = new Vector2(padding, padding);
                break;
        }

        Debug.Log($"[Minimap] Applied position: {position}, padding: {padding}px");
    }

    private void CreateMinimapUI()
    {
        Debug.Log("[Minimap] CreateMinimapUI() called");

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("[Minimap] No existing Canvas found, creating new one");
            GameObject canvasGO = new GameObject("MinimapCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
        }
        else
        {
            Debug.Log($"[Minimap] Found existing Canvas: {canvas.name}");
            Image existingImage = canvas.GetComponent<Image>();
            if (existingImage != null)
            {
                Debug.LogWarning($"[Minimap] Existing Canvas has Image component with color {existingImage.color} - this might cause white outline!");
            }
        }

        GameObject containerGO = new GameObject("MinimapContainer");
        containerGO.transform.SetParent(canvas.transform, false);
        _minimapContainer = containerGO.AddComponent<RectTransform>();

        _minimapContainer.sizeDelta = new Vector2(minimapSize, minimapSize);
        ApplyPosition();

        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(_minimapContainer, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        _backgroundImage = bgGO.AddComponent<Image>();
        _backgroundImage.color = backgroundColor;

        GameObject playerDotGO = new GameObject("PlayerDot");
        playerDotGO.transform.SetParent(_minimapContainer, false);
        _playerDot = playerDotGO.AddComponent<RectTransform>();
        _playerDot.anchorMin = new Vector2(0.5f, 0.5f);
        _playerDot.anchorMax = new Vector2(0.5f, 0.5f);
        _playerDot.pivot = new Vector2(0.5f, 0.5f);
        _playerDot.sizeDelta = new Vector2(playerDotSize * 3f, playerDotSize * 3f); // Larger for triangle visibility

        Image playerImage = playerDotGO.AddComponent<Image>();
        playerImage.sprite = _triangleSprite;
        playerImage.type = Image.Type.Simple;
        playerImage.preserveAspect = true;
        playerImage.color = playerColor;

        Debug.Log($"[Minimap] Player icon created with triangle sprite: {_triangleSprite != null}, size: {_playerDot.sizeDelta}");

        Debug.Log($"[Minimap] UI created - Container size: {_minimapContainer.sizeDelta}, Background color: {_backgroundImage.color}, Player dot color: {playerImage.color}");
    }

    private Color GetPlanetColor(CelestialBodyController body)
    {
        // Get color from sprite renderer (which contains the JSON fallbackColor)
        // Note: SpriteRenderer.color is set from CelestialBodyJson.fallbackColor during Initialize()
        var spriteRenderer = body.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.color;
        }

        // Fall back to generic colors if sprite renderer not available
        return body.IsStar ? starColor : planetColor;
    }

    private void CreateBodyDots()
    {
        Debug.Log("[Minimap] CreateBodyDots() called");

        if (starSystem == null)
        {
            Debug.LogError("[Minimap] StarSystemRenderer reference is null!");
            return;
        }

        CelestialBodyController[] bodies = starSystem.GetAllBodies();
        Debug.Log($"[Minimap] GetAllBodies() returned {bodies.Length} bodies");

        if (bodies.Length == 0)
        {
            Debug.LogWarning("[Minimap] No bodies found! StarSystemRenderer may not have created them yet.");
            return;
        }

        foreach (var body in bodies)
        {
            if (body == null)
            {
                Debug.LogWarning("[Minimap] Null body in array, skipping");
                continue;
            }

            Debug.Log($"[Minimap] Creating dot for: {body.BodyName} at world position {body.transform.position}");

            GameObject dotGO = new GameObject($"Dot_{body.BodyName}");
            dotGO.transform.SetParent(_minimapContainer, false);
            RectTransform dotRect = dotGO.AddComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.5f, 0.5f);
            dotRect.anchorMax = new Vector2(0.5f, 0.5f);
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotRect.sizeDelta = new Vector2(planetDotSize, planetDotSize);

            Image dotImage = dotGO.AddComponent<Image>();
            dotImage.sprite = _circleSprite;
            dotImage.type = Image.Type.Simple;

            // Use the actual planet's sprite color
            Color planetIconColor = GetPlanetColor(body);
            dotImage.color = planetIconColor;

            _bodyDots[body] = dotRect;
        }

        Debug.Log($"[Minimap] Finished creating {_bodyDots.Count} body dots");
    }

    private void LateUpdate()
    {
        if (playerShip == null || _minimapContainer == null || mainCamera == null)
            return;

        UpdateMinimapPositions();
    }

    private bool _loggedPositions = false;

    private void UpdateMinimapPositions()
    {
        Vector2 cameraPos = mainCamera.transform.position;

        if (_playerDot != null && playerShip != null)
        {
            Vector2 localPos = WorldToMinimapPosition(playerShip.position, cameraPos);
            _playerDot.anchoredPosition = localPos;

            // Rotate player icon to match ship rotation
            _playerDot.rotation = Quaternion.Euler(0, 0, playerShip.eulerAngles.z);

            if (!_loggedPositions)
            {
                Debug.Log($"[Minimap] Player world pos: {playerShip.position}, camera: {cameraPos}, minimap pos: {localPos}");
            }
        }

        int logCount = 0;
        foreach (var kvp in _bodyDots)
        {
            CelestialBodyController body = kvp.Key;
            RectTransform dot = kvp.Value;

            if (body != null && dot != null)
            {
                Vector2 localPos = WorldToMinimapPosition(body.transform.position, cameraPos);
                dot.anchoredPosition = localPos;

                if (!_loggedPositions && logCount < 3)
                {
                    Debug.Log($"[Minimap] {body.BodyName} world pos: {body.transform.position}, minimap pos: {localPos}");
                    logCount++;
                }
            }
        }

        _loggedPositions = true;
    }

    private Vector2 WorldToMinimapPosition(Vector3 worldPos, Vector2 cameraPos)
    {
        // Game Maker formula: minimap_x = box_x + (box_width * (world_x / room_width))
        // In Unity RectTransform, we're already inside the box, so just:
        // minimap_pos = (world_pos / worldSize) * minimapSize - (minimapSize/2)

        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        // Normalize to 0-1 range (assuming world goes from -worldSize/2 to +worldSize/2)
        Vector2 normalized = (worldPos2D + Vector2.one * (worldSize / 2f)) / worldSize;

        // Scale to minimap pixel size
        Vector2 minimapPos = normalized * minimapSize;

        // Offset to center (since container children are anchored to center)
        minimapPos -= Vector2.one * (minimapSize / 2f);

        return minimapPos;
    }
}
