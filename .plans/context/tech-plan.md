# 🚀 Milo's Space Adventure
## Simplified Unity Technical Design Document

---

## Table of Contents
1. [Project Structure](#1-project-structure)
2. [Architecture Overview](#2-architecture-overview)
3. [Core Scripts](#3-core-scripts)
4. [Star System (JSON Import)](#4-star-system-json-import)
5. [Minigames](#5-minigames)
6. [Stuffies & Abilities](#6-stuffies--abilities)
7. [Save System](#7-save-system)
8. [UI](#8-ui)
9. [Audio](#9-audio)
10. [Mobile Considerations](#10-mobile-considerations)

---

## 1. Project Structure

Keep it simple. Expand only when needed.

```
Assets/
├── _Project/
│   ├── Scenes/
│   │   ├── BootScene.unity
│   │   ├── MainMenuScene.unity
│   │   └── GameScene.unity
│   │
│   ├── Scripts/
│   │   ├── Player/
│   │   │   └── PlayerShipController.cs
│   │   ├── StarSystem/
│   │   │   ├── StarSystemLoader.cs
│   │   │   ├── StarSystemRenderer.cs
│   │   │   └── CelestialBody.cs
│   │   ├── Minigames/
│   │   │   ├── MathPuzzle.cs
│   │   │   └── PasswordPuzzle.cs
│   │   ├── Stuffies/
│   │   │   └── StuffyManager.cs
│   │   ├── UI/
│   │   │   ├── MainMenu.cs
│   │   │   ├── GameHUD.cs
│   │   │   └── StuffyAlbum.cs
│   │   ├── Save/
│   │   │   └── SaveManager.cs
│   │   └── Audio/
│   │       └── AudioManager.cs
│   │
│   ├── Content/
│   │   └── StarSystems/        ← Kid's JSON + sprites (editor import)
│   │
│   ├── Prefabs/
│   │   ├── MiloShip.prefab
│   │   ├── CelestialBody.prefab
│   │   └── UI/
│   │
│   ├── Art/
│   │   ├── Sprites/
│   │   └── UI/
│   │
│   ├── Audio/
│   │   ├── Music/
│   │   └── SFX/
│   │
│   └── Editor/
│       └── StarSystemImporter.cs
│
└── StreamingAssets/
    └── StarSystems/            ← Runtime JSON loading (for mobile)
        ├── sol/
        ├── system2/
        └── system3/
```

**What's NOT here (removed):**
- ❌ EventBus.cs (added in Phase 2.5 refactoring)
- ❌ ServiceLocator.cs
- ❌ GameManager.cs (SaveManager handles state)
- ❌ ProceduralStarGenerator.cs (hand-crafted systems)
- ❌ AdaptiveDifficulty.cs (fixed difficulty curve)
- ❌ AsteroidMaze/ folder (cut feature)
- ❌ CloudSaveHandler.cs (local save only)
- ✅ ObjectPool.cs (Phase 1 - prevent mobile GC spikes)

---

## 2. Architecture Overview

### Philosophy: Direct & Simple

```
┌─────────────────────────────────────────────────────────────┐
│                    SIMPLE ARCHITECTURE                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│   ┌──────────────┐         ┌──────────────┐                 │
│   │  SaveManager │ ◄─────► │    Scenes    │                 │
│   │  (Singleton) │         │              │                 │
│   └──────────────┘         └──────────────┘                 │
│          │                        │                          │
│          │    ┌──────────────────┴───────────────┐          │
│          │    │                                   │          │
│          ▼    ▼                                   ▼          │
│   ┌─────────────────┐  ┌─────────────────┐  ┌──────────┐   │
│   │ StarSystem      │  │ Minigames       │  │ UI       │   │
│   │ Loader/Renderer │  │ Math + Password │  │ Screens  │   │
│   └─────────────────┘  └─────────────────┘  └──────────┘   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Core Principles

| Principle | What It Means |
|-----------|---------------|
| **Direct References** | Drag & drop in Inspector, no service locators |
| **Simple Singletons** | Only SaveManager and AudioManager |
| **JSON for Content** | Star systems loaded from JSON files |
| **Scene-Based State** | Current scene = current state |

### What We're NOT Using

| Pattern | Why Skip |
|---------|----------|
| EventBus | Overkill - direct method calls work fine |
| ServiceLocator | Overkill - Inspector references are clearer |
| State Machine | Scene loading handles state transitions |
| ScriptableObjects | JSON is simpler for external content pipeline |
| Dependency Injection | Way overkill for this project |

---

## 3. Core Scripts

### 3.1 PlayerShipController.cs

Top-down arcade controls with momentum.

```csharp
using UnityEngine;

public class PlayerShipController : MonoBehaviour
{
    [Header("Movement")]
    public float thrustForce = 8f;
    public float maxSpeed = 10f;
    public float rotationSpeed = 180f;
    public float drag = 0.5f;

    [Header("Visuals")]
    public TrailRenderer thrusterTrail;

    private Vector2 _velocity;
    private bool _isThrusting;

    public float CurrentSpeed => _velocity.magnitude;
    public Vector2 Velocity => _velocity;

    private void Update()
    {
        // Rotation
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            rotationInput = 1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            rotationInput = -1f;
        
        // Gamepad
        float gamepadH = Input.GetAxis("Horizontal");
        if (Mathf.Abs(gamepadH) > 0.1f) rotationInput = -gamepadH;

        transform.Rotate(0, 0, rotationInput * rotationSpeed * Time.deltaTime);

        // Thrust
        _isThrusting = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        float gamepadV = Input.GetAxis("Vertical");
        if (gamepadV > 0.1f) _isThrusting = true;

        // Brake
        bool isBraking = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || gamepadV < -0.1f;
        if (isBraking) _velocity *= 0.95f;

        // Visuals
        if (thrusterTrail) thrusterTrail.emitting = _isThrusting;
    }

    private void FixedUpdate()
    {
        // Apply thrust
        if (_isThrusting)
        {
            _velocity += (Vector2)transform.up * thrustForce * Time.fixedDeltaTime;
            if (_velocity.magnitude > maxSpeed)
                _velocity = _velocity.normalized * maxSpeed;
        }

        // Apply drag
        if (!_isThrusting && _velocity.magnitude > 0.01f)
        {
            _velocity *= (1f - drag * Time.fixedDeltaTime);
            if (_velocity.magnitude < 0.05f) _velocity = Vector2.zero;
        }

        // Move
        transform.position += (Vector3)(_velocity * Time.fixedDeltaTime);
    }

    public void TeleportTo(Vector3 position)
    {
        transform.position = position;
        _velocity = Vector2.zero;
    }
}
```

#### Gravity Lock Mechanic (Phase 1)

**Purpose:** Help ages 5-8 target moving planets (based on plan review)

```csharp
public class PlayerShipController : MonoBehaviour
{
    [Header("Gravity Lock")]
    public float gravityRadius = 100f; // 2x planet visual size
    public float gravityStrength = 50f;

    private CelestialBody _lockedPlanet = null;

    private void Update()
    {
        // ... existing rotation/thrust code ...

        CheckGravityLock();
    }

    private void FixedUpdate()
    {
        // Apply gravity attraction if locked
        if (_lockedPlanet != null)
        {
            ApplyGravityAttraction();
        }

        // ... existing movement code ...
    }

    private void CheckGravityLock()
    {
        // Find closest planet within gravity radius
        var planets = FindObjectsOfType<CelestialBody>();
        float closestDist = gravityRadius;
        _lockedPlanet = null;

        foreach (var planet in planets)
        {
            float dist = Vector2.Distance(transform.position, planet.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                _lockedPlanet = planet;
            }
        }

        // Visual indicator: Highlight locked planet
        if (_lockedPlanet != null)
        {
            _lockedPlanet.ShowGravityIndicator(true);
        }
    }

    private void ApplyGravityAttraction()
    {
        // Smooth attraction to locked planet
        Vector2 direction = (Vector2)_lockedPlanet.transform.position - (Vector2)transform.position;
        float gravityScale = 1f - (direction.magnitude / gravityRadius);
        _velocity += direction.normalized * gravityScale * gravityStrength * Time.fixedDeltaTime;
    }
}
```

**Benefits:**
- Natural physics-based solution (feels game-like)
- Smooth attraction within 2x visual radius
- Ship follows planet's orbit automatically
- Visual indicator shows locked planet
- Easier for young players to click moving targets

### 3.2 CameraController.cs

Simple smooth follow.

```csharp
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 _velocity;

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);
    }
}
```

### 3.3 ObjectPool.cs

Generic object pooling for mobile GC spike prevention (Phase 1).

```csharp
using UnityEngine;
using System.Collections.Generic;

// Assets/Scripts/Core/ObjectPool.cs
public class ObjectPool<T> where T : Component
{
    private Queue<T> _pool = new Queue<T>();
    private T _prefab;
    private Transform _parent;

    public ObjectPool(T prefab, int initialSize = 10)
    {
        _prefab = prefab;
        _parent = new GameObject($"{typeof(T).Name} Pool").transform;

        for (int i = 0; i < initialSize; i++)
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (_pool.Count == 0)
        {
            var obj = Object.Instantiate(_prefab, _parent);
            return obj;
        }

        var pooled = _pool.Dequeue();
        pooled.gameObject.SetActive(true);
        return pooled;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
```

**Usage Example:**
```csharp
// Create pool in GameManager or relevant system
private ObjectPool<ParticleEffect> _particlePool;

void Start()
{
    _particlePool = new ObjectPool<ParticleEffect>(particlePrefab, 20);
}

// Get from pool
var particle = _particlePool.Get();
particle.transform.position = targetPosition;
particle.Play();

// Return to pool when done
_particlePool.Return(particle);
```

---

## 4. Star System (JSON Import)

### 4.1 JSON Data Format

Your kid exports star systems in this format (hierarchical with nested children):

```json
{
  "system": {
    "id": "sol",
    "name": "Sol",
    "bounds": { "width": 1500, "height": 1500 },
    "distanceFromGalaxyCenter": 0.0
  },
  "rootBodies": [
    {
      "id": "Sol",
      "name": "Sol",
      "description": "A G-type main-sequence star at the center of the Solar System.",
      "type": "star",
      "sprite": "",
      "parentId": null,
      "orbitDistance": 0,
      "orbitSpeed": 0,
      "orbitAngle": 0,
      "scale": 2,
      "rotation": 0,
      "rotationSpeed": 0.1,
      "luminosity": 1,
      "baseSize": 64,
      "fallbackColor": "#FFD700",
      "children": [
        {
          "id": "Sol 3",
          "name": "Earth",
          "description": "A blue-green world teeming with life.",
          "type": "planet",
          "sprite": "",
          "parentId": "Sol",
          "orbitDistance": 190,
          "orbitSpeed": 1,
          "orbitAngle": 90,
          "scale": 1,
          "rotation": 0,
          "rotationSpeed": 0.25,
          "planetNumber": 3,
          "baseSize": 48,
          "fallbackColor": "#3B82F6",
          "orbitRingColor": "rgba(100, 116, 139, 0.3)",
          "orbitRingWidth": 1,
          "scanned": false,
          "hasStuffySignal": true,
          "stuffyId": "earthy",
          "children": []
        }
      ]
    }
  ]
}
```

**See `.plans/context/json-schema-reference.md` for complete schema documentation.**

### 4.2 Data Classes

```csharp
using System;
using UnityEngine;

namespace MilosAdventure.Data
{
    [Serializable]
    public class StarSystemJson
    {
        public SystemInfo system;
        public CelestialBodyJson[] rootBodies; // Hierarchical with nested children
    }

    [Serializable]
    public class SystemInfo
    {
        public string id;
        public string name;
        public Bounds bounds;
        public float distanceFromGalaxyCenter; // 0.0 (edge) to 1.0 (center) - for difficulty scaling
    }

    [Serializable]
    public class Bounds
    {
        public int width;
        public int height;
    }

    [Serializable]
    public class CelestialBodyJson
    {
        // Core identity
        public string id;
        public string name;
        public string description;
        public string type;  // "star", "planet", "moon"

        // Visual
        public string sprite;
        public float scale;
        public int baseSize;
        public string fallbackColor;

        // Orbital mechanics
        public string parentId;
        public float orbitDistance;
        public float orbitSpeed;
        public float orbitAngle;
        public string orbitRingColor;
        public float orbitRingWidth;

        // Rotation
        public float rotation;
        public float rotationSpeed;

        // Gameplay state
        public bool scanned = false;  // Has player scanned this body?
        public bool hasStuffySignal = false;
        public string stuffyId = null;

        // Type-specific
        public int? planetNumber = null;   // 1-8 for planets
        public string moonLetter = null;   // "a"-"z" for moons
        public float? luminosity = null;   // 0.0-1.0 for stars

        // Hierarchy - CRITICAL: Nested structure!
        public CelestialBodyJson[] children = new CelestialBodyJson[0];
    }
}
```

### 4.3 StarSystemLoader.cs

**CRITICAL: Use UnityWebRequest for Android compatibility** (File.ReadAllText fails on Android APKs)

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MilosAdventure.Data;

public class StarSystemLoader : MonoBehaviour
{
    public static StarSystemLoader Instance { get; private set; }

    private Dictionary<string, StarSystemJson> _systems = new();
    private Dictionary<string, Sprite> _sprites = new();

    public System.Action<StarSystemJson> OnSystemLoaded;
    public System.Action<string> OnLoadFailed;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadSystemAsync(string systemId)
    {
        StartCoroutine(LoadSystemCoroutine(systemId));
    }

    private IEnumerator LoadSystemCoroutine(string systemId)
    {
        // Check cache
        if (_systems.TryGetValue(systemId, out var cached))
        {
            OnSystemLoaded?.Invoke(cached);
            yield break;
        }

        // CRITICAL: Use UnityWebRequest for Android compatibility
        string jsonPath = $"{Application.streamingAssetsPath}/Systems/{systemId}.json";

        using (UnityWebRequest request = UnityWebRequest.Get(jsonPath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load system {systemId}: {request.error}");
                OnLoadFailed?.Invoke(systemId);
                yield break;
            }

            // Parse JSON
            string json = request.downloadHandler.text;
            StarSystemJson system = JsonUtility.FromJson<StarSystemJson>(json);

            if (system == null || system.rootBodies == null)
            {
                Debug.LogError($"Invalid JSON for system: {systemId}");
                OnLoadFailed?.Invoke(systemId);
                yield break;
            }

            // Load sprites for all bodies recursively
            yield return LoadBodiesSpritesRecursive(system.rootBodies, systemId);

            // Cache and notify
            _systems[systemId] = system;
            OnSystemLoaded?.Invoke(system);
        }
    }

    private IEnumerator LoadBodiesSpritesRecursive(CelestialBodyJson[] bodies, string systemId)
    {
        foreach (var body in bodies)
        {
            if (!string.IsNullOrEmpty(body.sprite))
            {
                yield return LoadSpriteAsync(systemId, body.sprite);
            }

            // Recursively load sprites for children
            if (body.children != null && body.children.Length > 0)
            {
                yield return LoadBodiesSpritesRecursive(body.children, systemId);
            }

            // Spread loading over multiple frames to avoid frame drops
            yield return null;
        }
    }

    private IEnumerator LoadSpriteAsync(string systemId, string filename)
    {
        // Check cache
        if (_sprites.ContainsKey(filename))
            yield break;

        string path = $"{Application.streamingAssetsPath}/Systems/{systemId}/{filename}";

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(path))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                tex.filterMode = FilterMode.Bilinear;

                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                _sprites[filename] = sprite;
            }
            else
            {
                Debug.LogWarning($"Failed to load sprite {filename}: {request.error}");
            }
        }
    }

    public Sprite GetSprite(string filename)
    {
        _sprites.TryGetValue(filename, out var sprite);
        return sprite;
    }

    public StarSystemJson GetCachedSystem(string systemId)
    {
        _systems.TryGetValue(systemId, out var system);
        return system;
    }
}
```

**See `.plans/context/performance-targets.md` for async loading performance guidelines.**

### 4.4 StarSystemRenderer.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

public class StarSystemRenderer : MonoBehaviour
{
    public GameObject celestialBodyPrefab;
    
    private StarSystemJson _currentSystem;
    private Dictionary<string, CelestialBody> _bodies = new();

    public System.Action<CelestialBody> OnBodyClicked;

    public void RenderSystem(StarSystemJson system)
    {
        ClearSystem();
        _currentSystem = system;

        foreach (var data in system.bodies)
        {
            var go = Instantiate(celestialBodyPrefab, transform);
            go.name = data.name;

            var body = go.GetComponent<CelestialBody>();
            body.Initialize(data, this);
            body.OnClicked = () => OnBodyClicked?.Invoke(body);

            _bodies[data.id] = body;
        }
    }

    public CelestialBody GetBody(string id)
    {
        _bodies.TryGetValue(id, out var body);
        return body;
    }

    public Vector3 GetParentPosition(string parentId)
    {
        if (string.IsNullOrEmpty(parentId)) return Vector3.zero;
        var parent = GetBody(parentId);
        return parent?.transform.position ?? Vector3.zero;
    }

    public int GetDifficulty() => _currentSystem?.system.difficulty ?? 1;

    private void ClearSystem()
    {
        foreach (var body in _bodies.Values)
            if (body != null) Destroy(body.gameObject);
        _bodies.Clear();
    }
}
```

### 4.5 CelestialBody.cs

```csharp
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject stuffySignalIndicator;
    public GameObject scannedIndicator;

    private CelestialBodyJson _data;
    private StarSystemRenderer _system;
    private float _currentAngle;

    public string Id => _data?.id;
    public string BodyName => _data?.name;
    public bool IsStar => _data?.type == "star";
    public bool IsPlanet => _data?.type == "planet";
    public bool HasStuffySignal => _data?.hasStuffySignal ?? false;
    public string StuffyId => _data?.stuffyId;

    public bool IsScanned { get; private set; }

    public System.Action OnClicked;

    public void Initialize(CelestialBodyJson data, StarSystemRenderer system)
    {
        _data = data;
        _system = system;
        _currentAngle = data.startAngle;

        // Sprite or fallback color
        var sprite = StarSystemLoader.Instance.GetSprite(data.sprite);
        if (sprite != null)
            spriteRenderer.sprite = sprite;
        else if (!string.IsNullOrEmpty(data.color) && ColorUtility.TryParseHtmlString(data.color, out Color c))
            spriteRenderer.color = c;

        transform.localScale = Vector3.one * data.size;
        
        UpdateIndicators();
        UpdatePosition();
    }

    private void Update()
    {
        if (_data == null || IsStar) return;
        _currentAngle += _data.orbitSpeed * Time.deltaTime;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (IsStar)
        {
            transform.position = Vector3.zero;
            return;
        }

        Vector3 parentPos = _system.GetParentPosition(_data.parentId);
        float rad = _currentAngle * Mathf.Deg2Rad;
        float x = parentPos.x + Mathf.Cos(rad) * _data.orbitDistance;
        float y = parentPos.y + Mathf.Sin(rad) * _data.orbitDistance;
        transform.position = new Vector3(x, y, 0);
    }

    private void OnMouseDown()
    {
        OnClicked?.Invoke();
    }

    public void SetScanned(bool scanned)
    {
        IsScanned = scanned;
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        if (stuffySignalIndicator) stuffySignalIndicator.SetActive(HasStuffySignal && IsScanned);
        if (scannedIndicator) scannedIndicator.SetActive(IsScanned);
    }
}
```

---

## 5. Minigames

### 5.1 MathPuzzle.cs

**Math difficulty scales by `distanceFromGalaxyCenter` (0.0 = easy, 1.0 = hard)**. See `.plans/context/game-design-fixes.md`.

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MathPuzzle : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI problemText;
    public Button[] answerButtons;
    public Image progressBar;
    public GameObject panel;

    [Header("Settings")]
    public int problemsToSolve = 3;

    private int _currentAnswer;
    private int _problemsSolved;
    private int _difficulty; // 1-5 based on galaxy distance

    public System.Action OnCompleted;
    public System.Action OnCancelled;

    public void Show(float distanceFromGalaxyCenter)
    {
        // Calculate difficulty from galaxy distance (0.0 = edge, 1.0 = center)
        _difficulty = CalculateDifficulty(distanceFromGalaxyCenter);
        _problemsSolved = 0;
        panel.SetActive(true);
        GenerateProblem();
        UpdateProgress();
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private int CalculateDifficulty(float distanceFromGalaxyCenter)
    {
        // Galaxy-center based difficulty progression
        // See game-design-fixes.md for complete scaling
        if (distanceFromGalaxyCenter < 0.2f) return 1; // Easy: +/- to 10
        if (distanceFromGalaxyCenter < 0.4f) return 2; // Medium: × to 5
        if (distanceFromGalaxyCenter < 0.6f) return 3; // Challenging: × to 10
        if (distanceFromGalaxyCenter < 0.8f) return 4; // Hard: ÷, multi-step
        return 5; // Very hard: mixed operations
    }

    private void GenerateProblem()
    {
        int a, b, answer;
        string symbol;

        // Difficulty determines operation and number range
        switch (_difficulty)
        {
            case 1: // Easy addition/subtraction to 10
                a = Random.Range(1, 11);
                b = Random.Range(1, 11);
                if (Random.value > 0.5f)
                {
                    answer = a + b;
                    symbol = "+";
                }
                else
                {
                    if (a < b) (a, b) = (b, a); // Ensure positive result
                    answer = a - b;
                    symbol = "-";
                }
                break;

            case 2: // Multiplication tables to 5
                a = Random.Range(1, 6);
                b = Random.Range(1, 6);
                answer = a * b;
                symbol = "×";
                break;

            case 3: // Multiplication to 10
                a = Random.Range(1, 11);
                b = Random.Range(1, 11);
                answer = a * b;
                symbol = "×";
                break;

            case 4: // Division
                b = Random.Range(2, 11);
                answer = Random.Range(1, 11);
                a = b * answer; // Ensure clean division
                symbol = "÷";
                break;

            default: // Mixed operations (age-appropriate for 5-8)
                // Examples: "(2 + 3) × 2 = ?", "10 - (4 + 2) = ?"
                // More challenging but no fractions
                int first = Random.Range(1, 6);
                int second = Random.Range(1, 6);
                int innerResult = first + second;
                int multiplier = Random.Range(2, 4);
                answer = innerResult * multiplier;
                a = first;
                b = second;
                symbol = $"+{second})×{multiplier}"; // Display as "(a+b)×c"
                problemText.text = $"({a} {symbol} = ?"; // Override format
                break;
        }

        _currentAnswer = answer;
        problemText.text = $"{a} {symbol} {b} = ?";

        // Generate answer options
        var options = new List<int> { answer };
        while (options.Count < answerButtons.Length)
        {
            int wrong = answer + Random.Range(-5, 6);
            if (wrong != answer && wrong >= 0 && !options.Contains(wrong))
                options.Add(wrong);
        }

        // Shuffle and assign to buttons
        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (options[i], options[j]) = (options[j], options[i]);
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int opt = options[i];
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = opt.ToString();
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerClicked(opt));
        }
    }

    private void OnAnswerClicked(int answer)
    {
        if (answer == _currentAnswer)
        {
            _problemsSolved++;
            UpdateProgress();

            if (_problemsSolved >= problemsToSolve)
            {
                Hide();
                OnCompleted?.Invoke();
            }
            else
            {
                GenerateProblem();
            }
        }
        else
        {
            // Wrong - show gentle feedback, don't punish
            problemText.text += "\nTry again!";
        }
    }

    private void UpdateProgress()
    {
        progressBar.fillAmount = (float)_problemsSolved / problemsToSolve;
    }
}
```

**Usage:**
```csharp
// Show math puzzle based on current system's galaxy distance
StarSystemJson currentSystem = StarSystemLoader.Instance.GetCachedSystem("sol");
mathPuzzle.Show(currentSystem.system.distanceFromGalaxyCenter);
```

**See `.plans/context/game-design-fixes.md` for complete difficulty scaling table.**

### 5.2 PasswordPuzzle.cs (Fill-in-the-Blank)

**Redesigned as fill-in-the-blank** (no failure state, teaches spelling). See `.plans/context/game-design-fixes.md`.

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class FillInTheBlankPuzzle
{
    public string sentence;     // "Earth is the third _ _ _ _ _ _ from the Sun."
    public string missingWord;  // "PLANET"
    public string hint;         // "This word starts with 'P' and has 6 letters."
    public string stuffyId;     // Which stuffy this puzzle rescues
}

public class PasswordPuzzle : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI sentenceText;
    public TextMeshProUGUI wordDisplay;
    public TextMeshProUGUI hintText;
    public Transform keyboardContainer;
    public Button letterButtonPrefab;
    public GameObject panel;

    private FillInTheBlankPuzzle _currentPuzzle;
    private char[] _playerAnswer;
    private int _currentLetterIndex;
    private List<Button> _letterButtons = new();

    public System.Action OnCompleted;

    public void Show(FillInTheBlankPuzzle puzzle)
    {
        _currentPuzzle = puzzle;
        _playerAnswer = new char[puzzle.missingWord.Length];
        _currentLetterIndex = 0;

        // Show first letter as hint
        _playerAnswer[0] = puzzle.missingWord[0];
        _currentLetterIndex = 1;

        panel.SetActive(true);
        sentenceText.text = puzzle.sentence;
        hintText.text = puzzle.hint;

        SetupKeyboard();
        UpdateDisplay();
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void SetupKeyboard()
    {
        // Clear existing
        foreach (var btn in _letterButtons)
            Destroy(btn.gameObject);
        _letterButtons.Clear();

        // Create A-Z buttons
        for (char c = 'A'; c <= 'Z'; c++)
        {
            char letter = c;
            var btn = Instantiate(letterButtonPrefab, keyboardContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = letter.ToString();
            btn.onClick.AddListener(() => OnLetterClicked(letter));
            _letterButtons.Add(btn);
        }
    }

    private void OnLetterClicked(char letter)
    {
        // No failure state - just check if correct for current position
        if (_currentLetterIndex >= _playerAnswer.Length)
            return; // Already complete

        char correctLetter = _currentPuzzle.missingWord[_currentLetterIndex];

        if (letter == correctLetter)
        {
            // Correct! Fill in letter
            _playerAnswer[_currentLetterIndex] = correctLetter;
            _currentLetterIndex++;

            // Visual feedback: Green flash
            FlashGreen();
            AudioManager.Instance?.PlaySFX("correct_letter");

            UpdateDisplay();

            // Check if complete
            if (_currentLetterIndex >= _playerAnswer.Length)
            {
                OnPuzzleComplete();
            }
        }
        else
        {
            // Wrong letter: Red flash, but no penalty
            FlashRed();
            AudioManager.Instance?.PlaySFX("wrong_letter");
        }
    }

    private void UpdateDisplay()
    {
        // Show word with blanks filled in
        string display = "";
        for (int i = 0; i < _playerAnswer.Length; i++)
        {
            if (_playerAnswer[i] != '\0')
                display += _playerAnswer[i] + " ";
            else
                display += "_ ";
        }
        wordDisplay.text = display.Trim();
    }

    private void OnPuzzleComplete()
    {
        AudioManager.Instance?.PlaySFX("puzzle_complete");

        // Show completion message
        string message = $"Great job! {_currentPuzzle.missingWord} is spelled: " +
                         string.Join("-", _currentPuzzle.missingWord.ToCharArray());

        // Award stuffy
        StuffyManager.Instance.RescueStuffy(_currentPuzzle.stuffyId);

        // Delay before hiding to show success
        Invoke(nameof(CompletePuzzle), 2f);
    }

    private void CompletePuzzle()
    {
        Hide();
        OnCompleted?.Invoke();
    }

    private void FlashGreen()
    {
        // Flash word display green (implement visual feedback)
        wordDisplay.color = Color.green;
        Invoke(nameof(ResetColor), 0.2f);
    }

    private void FlashRed()
    {
        // Flash word display red (implement visual feedback)
        wordDisplay.color = Color.red;
        Invoke(nameof(ResetColor), 0.2f);
    }

    private void ResetColor()
    {
        wordDisplay.color = Color.white;
    }
}
```

**Example Puzzle Data:**
```csharp
// In StuffyManager or puzzle config
var brainyPuzzle = new FillInTheBlankPuzzle
{
    sentence = "Earth is the third ________ from the Sun.",
    missingWord = "PLANET",
    hint = "This word starts with 'P' and has 6 letters. It orbits a star!",
    stuffyId = "brainy"
};

passwordPuzzle.Show(brainyPuzzle);
```

**See `.plans/context/game-design-fixes.md` for complete puzzle redesign and examples.**

#### Word Length Progression (Phase 3+)

Progressive difficulty through word length tiers (based on plan review):

```
Stuffy 1-2 (Early): 3-5 letters
- SUN, MOON, STAR, EARTH, MARS

Stuffy 3-4 (Medium): 5-6 letters
- VENUS, ORBIT, COMET, SOLAR

Stuffy 5-7 (Late): 6-8 letters
- JUPITER, MERCURY, ECLIPSE, ASTEROID
```

#### Audio Pronunciation Hints (Phase 3+)

Add phonetic audio hints to help younger players:

```csharp
public class PasswordPuzzle : MonoBehaviour
{
    [Header("Audio Hints")]
    public AudioClip wordPronunciation; // Set per puzzle

    public void PlayAudioHint()
    {
        if (wordPronunciation != null)
        {
            AudioManager.Instance?.PlaySFX(wordPronunciation);
            // Play phonetic pronunciation of the target word
        }
    }
}
```

**Implementation Notes:**
- Record pronunciation audio for each word
- Button to trigger hint (optional for struggling players)
- Helps ages 5-6 who are still learning to read

---

## 6. Stuffies & Abilities

### 6.1 Stuffy Data

Stuffies defined in a simple JSON file:

```json
{
    "stuffies": [
        {
            "id": "earthy",
            "name": "Earthy",
            "description": "A fuzzy blue-green planet",
            "sprite": "earthy.png",
            "ability": "scan_tutorial",
            "phrases": ["HELLO FRIEND", "SPACE IS FUN", "LETS EXPLORE"]
        },
        {
            "id": "starlet", 
            "name": "Starlet",
            "description": "A tiny sparkly star",
            "sprite": "starlet.png", 
            "ability": "reveal_hidden",
            "phrases": ["SHINE BRIGHT", "TWINKLE STAR"]
        }
    ]
}
```

### 6.2 StuffyManager.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

public class StuffyManager : MonoBehaviour
{
    public static StuffyManager Instance { get; private set; }

    private Dictionary<string, StuffyData> _allStuffies = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadStuffyData();
    }

    private void LoadStuffyData()
    {
        // Load from Resources or StreamingAssets
        var json = Resources.Load<TextAsset>("stuffies");
        if (json != null)
        {
            var data = JsonUtility.FromJson<StuffyListJson>(json.text);
            foreach (var s in data.stuffies)
                _allStuffies[s.id] = s;
        }
    }

    public StuffyData GetStuffy(string id)
    {
        _allStuffies.TryGetValue(id, out var stuffy);
        return stuffy;
    }

    public bool HasAbility(string abilityId)
    {
        var rescued = SaveManager.Instance.Data.rescuedStuffies;
        foreach (var stuffyId in rescued)
        {
            if (_allStuffies.TryGetValue(stuffyId, out var stuffy))
            {
                if (stuffy.ability == abilityId) return true;
            }
        }
        return false;
    }

    public string GetRandomPhrase(string stuffyId)
    {
        if (_allStuffies.TryGetValue(stuffyId, out var stuffy) && stuffy.phrases.Length > 0)
        {
            return stuffy.phrases[Random.Range(0, stuffy.phrases.Length)];
        }
        return "FIND ME";
    }
}

[System.Serializable]
public class StuffyListJson
{
    public StuffyData[] stuffies;
}

[System.Serializable]
public class StuffyData
{
    public string id;
    public string name;
    public string description;
    public string sprite;
    public string ability;
    public string[] phrases;
}
```

### 6.3 Abilities (Simplified - Phase 1)

**CRITICAL**: Scanning is ALWAYS available (no unlock required). See `.plans/context/game-design-fixes.md`.

All abilities are passive checks - no complex systems needed.

| Stuffy | Ability ID | Effect | Where Checked |
|--------|-----------|--------|---------------|
| Earthy | `auto_reveal_signals` | Auto-reveals stuffy signals on minimap when scanning system | MinimapManager |
| (Phase 4+) | `speed_boost` | Ship moves 25% faster | PlayerShipController |
| (Phase 4+) | `show_path` | Shows line to target | GameController |
| (Phase 4+) | `extra_hints` | +1 free hint in password | PasswordPuzzle |
| (Phase 6) | `unlock_center` | Can enter galaxy center | SystemSelectUI |

**Phase 1 Implementation (KISS):**
```csharp
// Scanning is ALWAYS available - no ability check needed
private void Update()
{
    if (InputManager.Instance.ScanPressed)
    {
        ScanNearbyPlanets(); // Always works
    }
}

// Earthy's ability: Auto-reveal stuffy signals
private void ScanNearbyPlanets()
{
    foreach (var planet in planetsInRange)
    {
        planet.SetScanned(true);

        // If Earthy rescued, auto-reveal stuffy signals
        if (StuffyManager.Instance.IsRescued("earthy") && planet.HasStuffySignal)
        {
            MinimapManager.Instance.ShowStuffySignal(planet);
        }
    }
}
```

**See `.plans/context/game-design-fixes.md` for complete scanning gameplay loop.**

---

## 7. Save System

### 7.1 SaveData

```csharp
[System.Serializable]
public class SaveData
{
    // Progress
    public List<string> rescuedStuffies = new();
    public List<string> scannedPlanets = new();
    public List<string> unlockedSystems = new();
    
    // Current state
    public string currentSystemId;
    public float shipX;
    public float shipY;
    public float shipRotation;
    
    // Stats
    public int mathProblemsSolved;
    public int passwordsSolved;
}
```

### 7.2 SaveManager.cs

```csharp
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    public SaveData Data { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            Data = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Data = new SaveData();
            Data.unlockedSystems.Add("sol"); // Start with Sol unlocked
        }
    }

    public void NewGame()
    {
        Data = new SaveData();
        Data.unlockedSystems.Add("sol");
        Save();
    }

    public void RescueStuffy(string stuffyId)
    {
        if (!Data.rescuedStuffies.Contains(stuffyId))
        {
            Data.rescuedStuffies.Add(stuffyId);
            Save();
        }
    }

    public void ScanPlanet(string planetId)
    {
        if (!Data.scannedPlanets.Contains(planetId))
        {
            Data.scannedPlanets.Add(planetId);
            AutoSave(); // Autosave after every planet scan
        }
    }

    // Autosave (Phase 1) - Based on plan review
    // Frequent autosave enables flexible stop/resume for mobile sessions
    public void AutoSave()
    {
        Save();
        Debug.Log("Autosaved progress");
    }

    public void UnlockSystem(string systemId)
    {
        if (!Data.unlockedSystems.Contains(systemId))
        {
            Data.unlockedSystems.Add(systemId);
            Save();
        }
    }

    public bool IsPlanetScanned(string planetId) => Data.scannedPlanets.Contains(planetId);
    public bool IsStuffyRescued(string stuffyId) => Data.rescuedStuffies.Contains(stuffyId);
    public bool IsSystemUnlocked(string systemId) => Data.unlockedSystems.Contains(systemId);
}
```

---

## 8. UI

### 8.1 Scene Flow

```
Boot → MainMenu → GameScene
                     ↓
              (System Select overlay)
                     ↓
              (Flying in system)
                     ↓
              (Math Puzzle overlay)
                     ↓
              (Password Puzzle overlay)
                     ↓
              (Stuffy Album overlay)
```

### 8.2 MainMenu.cs

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button continueButton;
    public Button newGameButton;
    public Button albumButton;
    public Button quitButton;

    private void Start()
    {
        continueButton.interactable = SaveManager.Instance.Data.rescuedStuffies.Count > 0;
        
        newGameButton.onClick.AddListener(OnNewGame);
        continueButton.onClick.AddListener(OnContinue);
        quitButton.onClick.AddListener(OnQuit);
    }

    private void OnNewGame()
    {
        SaveManager.Instance.NewGame();
        SceneManager.LoadScene("GameScene");
    }

    private void OnContinue()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnQuit()
    {
        Application.Quit();
    }
}
```

### 8.3 StuffyAlbum.cs

```csharp
using UnityEngine;
using UnityEngine.UI;

public class StuffyAlbum : MonoBehaviour
{
    public Transform gridContainer;
    public GameObject stuffyCardPrefab;
    public GameObject panel;

    public void Show()
    {
        panel.SetActive(true);
        PopulateAlbum();
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void PopulateAlbum()
    {
        // Clear existing
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        // Add all stuffies
        string[] allIds = { "earthy", "starlet", "veenee", "plushy_comet", "moonlings", "black_hole", "threadbare" };
        
        foreach (var id in allIds)
        {
            var card = Instantiate(stuffyCardPrefab, gridContainer);
            var stuffy = StuffyManager.Instance.GetStuffy(id);
            bool rescued = SaveManager.Instance.IsStuffyRescued(id);

            // Configure card appearance
            var image = card.GetComponentInChildren<Image>();
            var text = card.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (rescued && stuffy != null)
            {
                // Show full color and name
                text.text = stuffy.name;
                image.color = Color.white;
                // Load sprite...
            }
            else
            {
                // Show silhouette
                text.text = "???";
                image.color = Color.black;
            }
        }
    }
}
```

---

## 9. Audio

### 9.1 AudioManager.cs

Simple audio - no complex mixing.

```csharp
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip correctSfx;
    public AudioClip wrongSfx;
    public AudioClip rescueFanfare;
    public AudioClip buttonClick;
    public AudioClip thrusterLoop;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    // Convenience methods
    public void PlayCorrect() => PlaySFX(correctSfx);
    public void PlayWrong() => PlaySFX(wrongSfx);
    public void PlayRescue() => PlaySFX(rescueFanfare);
    public void PlayClick() => PlaySFX(buttonClick);
}
```

---

## 10. Mobile Considerations

### 10.1 Touch Controls

Add a virtual joystick for mobile:

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;
    public float handleRange = 50f;

    private Vector2 _input;

    public float Horizontal => _input.x;
    public float Vertical => _input.y;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, eventData.pressEventCamera, out pos);

        pos = Vector2.ClampMagnitude(pos, handleRange);
        handle.anchoredPosition = pos;
        _input = pos / handleRange;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
```

Update PlayerShipController to use it:
```csharp
public VirtualJoystick joystick; // Assign in inspector, null on PC

private void Update()
{
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");

    // Override with joystick if available
    if (joystick != null)
    {
        if (Mathf.Abs(joystick.Horizontal) > 0.1f) h = joystick.Horizontal;
        if (Mathf.Abs(joystick.Vertical) > 0.1f) v = joystick.Vertical;
    }
    
    // Use h for rotation, v for thrust...
}
```

### 10.2 Screen Sizes

- Design UI at 1920x1080, use Canvas Scaler "Scale With Screen Size"
- Use anchors to keep buttons accessible on all screen sizes
- Make tap targets at least 44x44 pixels

### 10.3 Performance Tips

- Use Sprite Atlases for UI and common sprites
- Keep particle counts low
- Test on actual devices, not just editor

---

## 11. Error Handling Strategy

Basic validation to prevent crashes while maintaining development speed (Phase 1).

### 11.1 JSON Validation

Validate star system JSON on load:

```csharp
// StarSystemLoader.cs - Validate JSON
public IEnumerator LoadSystemAsync(string systemId)
{
    // ... UnityWebRequest code ...

    StarSystemJson system = JsonUtility.FromJson<StarSystemJson>(json);

    // Validate system
    if (!ValidateSystem(system, out string error))
    {
        Debug.LogError($"Invalid system JSON for {systemId}: {error}");
        OnLoadFailed?.Invoke(error);
        yield break;
    }

    // Continue loading...
}

private bool ValidateSystem(StarSystemJson system, out string error)
{
    if (system == null)
    {
        error = "System is null";
        return false;
    }

    if (system.system == null)
    {
        error = "SystemInfo is null";
        return false;
    }

    if (string.IsNullOrEmpty(system.system.id))
    {
        error = "System ID is required";
        return false;
    }

    if (system.rootBodies == null || system.rootBodies.Length == 0)
    {
        error = "rootBodies must have at least one body";
        return false;
    }

    error = null;
    return true;
}
```

### 11.2 Sprite Loading Fallback

Graceful degradation for missing sprites:

```csharp
// StarSystemLoader.cs - Fallback for missing sprites
private IEnumerator LoadSpriteAsync(string systemId, string spritePath)
{
    if (string.IsNullOrEmpty(spritePath))
    {
        // No sprite path - use fallback color
        yield break;
    }

    string fullPath = $"{Application.streamingAssetsPath}/Systems/{systemId}/{spritePath}";

    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullPath))
    {
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Failed to load sprite {spritePath}: {request.error}");
            // Continue without sprite - fallback color will be used
            yield break;
        }

        // Success - create sprite
        Texture2D tex = DownloadHandlerTexture.GetContent(request);
        // ... sprite creation ...
    }
}
```

### 11.3 Missing Field Defaults

Provide sensible defaults for optional JSON fields:

```csharp
// CelestialBodyJson.cs - Default values
[Serializable]
public class CelestialBodyJson
{
    // Required fields
    public string id;
    public string name;
    public string type;
    public float radius;
    public float orbitDistance;
    public float orbitSpeed;

    // Optional fields with defaults
    public string color = "rgba(200, 200, 200, 1.0)"; // Default gray
    public string orbitRingColor = "rgba(100, 116, 139, 0.3)";
    public float orbitRingWidth = 1f;
    public bool scanned = false;
    public bool hasStuffySignal = false;
    public string stuffyId = null;
    public CelestialBodyJson[] children = new CelestialBodyJson[0]; // Empty array default
}
```

**Rationale:** Log errors for debugging without crashing, use fallback colors for missing assets, provide sensible defaults for optional fields.

---

## Summary: What Changed

| Original | Simplified |
|----------|------------|
| 2000+ lines | ~800 lines |
| EventBus + ServiceLocator + GameManager | SaveManager only |
| ScriptableObjects for everything | JSON files |
| Adaptive difficulty AI | Fixed difficulty per system |
| Complex state machine | Scene-based state |
| Multiple minigame types | Math + Password only |
| Object pooling | Not needed yet |
| Cloud saves | Local only |

**Rule: Add complexity when you feel the pain of not having it.**

---

*Document Version: 2.0 (Simplified)*  
*Engine: Unity 2022.3 LTS*  
*Platforms: PC + Mobile*