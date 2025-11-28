# Phase 1: Flying Ship + Star System Import
## Milo's Space Adventure - Development Plan

**Goal:** Fly Milo's ship around a star system your kid designed  
**Critical:** JSON import system - this is how content gets into the game!

---

## Executive Summary

Phase 1 has TWO equally important parts:
1. **Flying Ship** - Playable controls (W/A/S/D + gamepad)
2. **JSON Star System Import** - Load your kid's custom star systems

By the end of Phase 1, you'll fly around an actual star system that your kid designed and exported.

---

## Week 1: Project Setup + Flying Ship

### 1.1 Day 1-2: Unity Project Creation

**Create New Project:**
- Unity 2022.3 LTS
- **2D Template** (not URP - keep it simple)
- Project name: `MilosSpaceAdventure`

**Verify Built-in Packages:**
| Package | Status | Check |
|---------|--------|-------|
| 2D Sprite | Built-in | ✅ |
| TextMeshPro | Built-in | Import Essentials when prompted |
| Input System | Built-in | Enable in Project Settings* |

*Project Settings → Player → Active Input Handling → "Both"

**Create Folder Structure:**
```
Assets/
├── _Project/
│   ├── Scenes/
│   │   └── TestScene.unity
│   ├── Scripts/
│   │   ├── Player/
│   │   ├── StarSystem/
│   │   └── Data/
│   ├── Content/
│   │   └── StarSystems/
│   │       └── Sol/           ← Your kid's JSON + sprites go here
│   │           ├── sol_system.json
│   │           ├── Sun.png
│   │           ├── Earth.png
│   │           └── ...
│   ├── Prefabs/
│   ├── Art/
│   └── Editor/                ← Custom editor tools
└── StreamingAssets/
    └── StarSystems/           ← For runtime loading (mobile)
```

---

### 1.2 Day 3-5: Player Ship Controller

**Create the Ship:**

1. Create new scene: `TestScene.unity`
2. Create empty GameObject: "MiloShip"
3. Add components:
   - `SpriteRenderer` (placeholder sprite for now)
   - `PlayerShipController.cs`

**PlayerShipController.cs:**

```csharp
using UnityEngine;

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

    private Vector2 _velocity;
    private bool _isThrusting;

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
        // Rotation: A/D or Left/Right arrows
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            rotationInput = 1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            rotationInput = -1f;

        // Gamepad
        float gamepadH = Input.GetAxis("Horizontal");
        if (Mathf.Abs(gamepadH) > 0.1f)
            rotationInput = -gamepadH;

        transform.Rotate(0, 0, rotationInput * rotationSpeed * Time.deltaTime);

        // Thrust: W/Up = forward, S/Down = brake
        _isThrusting = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        
        float gamepadV = Input.GetAxis("Vertical");
        if (gamepadV > 0.1f) _isThrusting = true;

        bool isBraking = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || gamepadV < -0.1f;
        if (isBraking && _velocity.magnitude > 0.1f)
        {
            _velocity *= 0.95f;
        }
    }

    private void ApplyThrust()
    {
        if (_isThrusting)
        {
            Vector2 forwardDirection = transform.up;
            _velocity += forwardDirection * thrustForce * Time.fixedDeltaTime;

            if (_velocity.magnitude > maxSpeed)
                _velocity = _velocity.normalized * maxSpeed;
        }
    }

    private void ApplyDrag()
    {
        if (!_isThrusting && _velocity.magnitude > 0.01f)
        {
            _velocity *= (1f - drag * Time.fixedDeltaTime);
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
    }

    public float CurrentSpeed => _velocity.magnitude;
}
```

**CameraController.cs:**

```csharp
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 _velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            smoothTime
        );
    }
}
```

### 1.3 Mobile Controls (Touch + Virtual Joystick)

**CRITICAL**: Implement mobile controls from Phase 1 for simultaneous PC/mobile support.

**See `.plans/context/mobile-controls-spec.md` for complete implementation details.**

**Quick Summary:**

1. **Install Unity Input System** (if not already):
   - Window → Package Manager → Unity Registry → "Input System"
   - Project Settings → Player → Active Input Handling → "Both"

2. **Create Virtual Joystick UI:**
   ```
   Canvas (Screen Space - Overlay)
   ├─ MobileControls
   │  ├─ VirtualJoystick (bottom-left)
   │  │  ├─ Background (120x120px, semi-transparent)
   │  │  └─ Knob (60px, draggable)
   │  ├─ ScanButton (bottom-right, 80x80px)
   │  └─ BrakeButton (bottom-right, above scan)
   ```

3. **Auto-detect input device** and show/hide mobile UI:
   ```csharp
   if (Application.isMobilePlatform || Input.touchSupported)
   {
       mobileControlsCanvas.SetActive(true);
   }
   ```

4. **Update PlayerShipController** to handle both:
   - **PC**: WASD + keyboard (existing code)
   - **Mobile**: Virtual joystick for thrust + rotation

**Mobile UI Layout:**
- Virtual joystick (left): Thrust direction + magnitude
- Scan button (right bottom): Tap to scan
- Brake button (right top): Hold to brake

**Performance Target**: 30 FPS minimum on mobile (see `.plans/context/performance-targets.md`)

### Week 1 Deliverables
- [ ] Project created with folder structure
- [ ] Ship flies with W/A/S/D + gamepad
- [ ] **Mobile controls: Virtual joystick + touch buttons**
- [ ] **Auto-detect PC vs mobile input**
- [ ] Camera follows ship
- [ ] Ship stays in bounds
- [ ] Thruster trail works

**🎮 CHECKPOINT: You can fly on both PC and mobile!**

---

## Week 2: JSON Star System Import

### 2.1 JSON Data Models

**StarSystemData.cs:**

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MilosAdventure.Data
{
    [Serializable]
    public class StarSystemJson
    {
        public SystemInfo system;
        public List<CelestialBodyJson> bodies;
    }

    [Serializable]
    public class SystemInfo
    {
        public string id;
        public string name;
        public float width;
        public float height;
    }

    [Serializable]
    public class CelestialBodyJson
    {
        public string id;
        public string name;
        public string type;        // "star", "planet", "moon"
        public string sprite;      // filename: "Earth.png"
        public string parentId;    // null for star, star id for planets
        
        public float orbitDistance;
        public float orbitSpeed;
        public float startAngle;
        
        public float size;
        public string color;       // "#FF5500" fallback if no sprite
        
        // Helper
        public bool IsStar => type?.ToLower() == "star";
        public bool IsPlanet => type?.ToLower() == "planet";
        public bool IsMoon => type?.ToLower() == "moon";
    }
}
```

### 2.2 Star System Loader

**StarSystemLoader.cs:**

**⚠️ CRITICAL: Use UnityWebRequest for Android compatibility!** (File.ReadAllText fails on Android APKs)

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MilosAdventure.Data;

public class StarSystemLoader : MonoBehaviour
{
    public static StarSystemLoader Instance { get; private set; }

    private Dictionary<string, StarSystemJson> _loadedSystems = new();
    private Dictionary<string, Sprite> _loadedSprites = new();

    public System.Action<StarSystemJson> OnSystemLoaded;
    public System.Action<string> OnLoadFailed;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Load a star system from StreamingAssets (async for Android compatibility)
    /// </summary>
    public void LoadSystemAsync(string systemId)
    {
        StartCoroutine(LoadSystemCoroutine(systemId));
    }

    private IEnumerator LoadSystemCoroutine(string systemId)
    {
        // Check cache first
        if (_loadedSystems.TryGetValue(systemId, out var cached))
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

            // Load sprites recursively (hierarchical structure)
            yield return LoadBodiesSpritesRecursive(system.rootBodies, systemId);

            // Cache and notify
            _loadedSystems[systemId] = system;
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

            // Recursively load child sprites
            if (body.children != null && body.children.Length > 0)
            {
                yield return LoadBodiesSpritesRecursive(body.children, systemId);
            }

            // Spread loading over frames to avoid stuttering
            yield return null;
        }
    }

    private IEnumerator LoadSpriteAsync(string systemId, string filename)
    {
        if (_loadedSprites.ContainsKey(filename))
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
                _loadedSprites[filename] = sprite;
            }
            else
            {
                Debug.LogWarning($"Failed to load sprite {filename}: {request.error}");
            }
        }
    }

    public Sprite GetSprite(string filename)
    {
        _loadedSprites.TryGetValue(filename, out var sprite);
        return sprite;
    }

    public StarSystemJson GetCachedSystem(string systemId)
    {
        _loadedSystems.TryGetValue(systemId, out var system);
        return system;
    }
}
```

**Usage Example:**
```csharp
// In GameController or scene manager
private void Start()
{
    StarSystemLoader.Instance.OnSystemLoaded += OnSystemLoaded;
    StarSystemLoader.Instance.OnLoadFailed += OnLoadFailed;
    StarSystemLoader.Instance.LoadSystemAsync("sol");
}

private void OnSystemLoaded(StarSystemJson system)
{
    Debug.Log($"Loaded system: {system.system.name}");
    starSystemRenderer.RenderSystem(system);
}

private void OnLoadFailed(string systemId)
{
    Debug.LogError($"Failed to load {systemId}");
}
```

**See `.plans/context/json-schema-reference.md` for complete JSON format and `.plans/context/performance-targets.md` for async loading guidelines.**

### 2.3 Star System Renderer

**StarSystemRenderer.cs:**

```csharp
using System.Collections.Generic;
using UnityEngine;
using MilosAdventure.Data;

public class StarSystemRenderer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject celestialBodyPrefab;
    
    [Header("Settings")]
    [SerializeField] private float scaleMultiplier = 1f;
    [SerializeField] private float orbitSpeedMultiplier = 1f;

    private StarSystemJson _currentSystem;
    private Dictionary<string, CelestialBodyController> _bodies = new();

    public void RenderSystem(StarSystemJson system)
    {
        ClearSystem();
        _currentSystem = system;

        foreach (var bodyData in system.bodies)
        {
            CreateBody(bodyData);
        }
    }

    private void CreateBody(CelestialBodyJson data)
    {
        GameObject go = Instantiate(celestialBodyPrefab, transform);
        go.name = data.name;

        var controller = go.GetComponent<CelestialBodyController>();
        controller.Initialize(data, this);

        _bodies[data.id] = controller;
    }

    public CelestialBodyController GetBody(string id)
    {
        _bodies.TryGetValue(id, out var body);
        return body;
    }

    public Vector3 GetParentPosition(string parentId)
    {
        if (string.IsNullOrEmpty(parentId)) return Vector3.zero;
        var parent = GetBody(parentId);
        return parent != null ? parent.transform.position : Vector3.zero;
    }

    private void ClearSystem()
    {
        foreach (var body in _bodies.Values)
        {
            if (body != null)
                Destroy(body.gameObject);
        }
        _bodies.Clear();
    }
}
```

**CelestialBodyController.cs:**

```csharp
using UnityEngine;
using MilosAdventure.Data;

public class CelestialBodyController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private CelestialBodyJson _data;
    private StarSystemRenderer _system;
    private float _currentAngle;

    public string Id => _data?.id;
    public string BodyName => _data?.name;
    public bool IsStar => _data?.IsStar ?? false;

    public void Initialize(CelestialBodyJson data, StarSystemRenderer system)
    {
        _data = data;
        _system = system;
        _currentAngle = data.startAngle;

        // Set sprite or fallback color
        var sprite = StarSystemLoader.Instance.GetSprite(data.sprite);
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
        else if (!string.IsNullOrEmpty(data.color))
        {
            if (ColorUtility.TryParseHtmlString(data.color, out Color c))
                spriteRenderer.color = c;
        }

        // Set size
        transform.localScale = Vector3.one * data.size;

        // Position (star at center, planets orbit)
        UpdatePosition();
    }

    private void Update()
    {
        if (_data == null || _data.IsStar) return;

        _currentAngle += _data.orbitSpeed * Time.deltaTime;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_data.IsStar)
        {
            transform.position = Vector3.zero;
            return;
        }

        Vector3 parentPos = _system.GetParentPosition(_data.parentId);
        float x = parentPos.x + Mathf.Cos(_currentAngle * Mathf.Deg2Rad) * _data.orbitDistance;
        float y = parentPos.y + Mathf.Sin(_currentAngle * Mathf.Deg2Rad) * _data.orbitDistance;
        transform.position = new Vector3(x, y, 0);
    }
}
```

### 2.4 Editor Import Tool

**Editor/StarSystemImporter.cs:**

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class StarSystemImporter : EditorWindow
{
    private string _jsonPath = "";
    
    [MenuItem("Milo/Import Star System JSON")]
    public static void ShowWindow()
    {
        GetWindow<StarSystemImporter>("Star System Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Star System from JSON", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Select JSON File"))
        {
            _jsonPath = EditorUtility.OpenFilePanel("Select Star System JSON", "", "json");
        }

        if (!string.IsNullOrEmpty(_jsonPath))
        {
            EditorGUILayout.LabelField("Selected:", _jsonPath);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Import to StreamingAssets"))
            {
                ImportSystem();
            }
        }
    }

    private void ImportSystem()
    {
        if (!File.Exists(_jsonPath))
        {
            EditorUtility.DisplayDialog("Error", "JSON file not found", "OK");
            return;
        }

        // Get folder containing JSON
        string sourceFolder = Path.GetDirectoryName(_jsonPath);
        string systemName = Path.GetFileNameWithoutExtension(_jsonPath).Replace("_system", "");
        
        // Create destination folder
        string destFolder = Path.Combine(Application.streamingAssetsPath, "StarSystems", systemName);
        Directory.CreateDirectory(destFolder);

        // Copy JSON
        string destJson = Path.Combine(destFolder, Path.GetFileName(_jsonPath));
        File.Copy(_jsonPath, destJson, overwrite: true);

        // Copy all PNGs from source folder
        foreach (string pngFile in Directory.GetFiles(sourceFolder, "*.png"))
        {
            string destPng = Path.Combine(destFolder, Path.GetFileName(pngFile));
            File.Copy(pngFile, destPng, overwrite: true);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Imported {systemName} to StreamingAssets!", "OK");
    }
}
#endif
```

### 2.5 Test Scene Setup

**TestSceneSetup.cs:**

```csharp
using UnityEngine;

public class TestSceneSetup : MonoBehaviour
{
    [SerializeField] private StarSystemRenderer systemRenderer;
    [SerializeField] private string testSystemId = "sol";

    private void Start()
    {
        var system = StarSystemLoader.Instance.LoadSystem(testSystemId);
        if (system != null)
        {
            systemRenderer.RenderSystem(system);
            Debug.Log($"Loaded system: {system.system.name}");
        }
    }
}
```

### 2.6 Example JSON Format

Your kid's exported JSON should look like this:

**sol_system.json:**
```json
{
    "system": {
        "id": "sol",
        "name": "Solar System",
        "width": 100,
        "height": 100
    },
    "bodies": [
        {
            "id": "sun",
            "name": "Sun",
            "type": "star",
            "sprite": "Sun.png",
            "size": 3.0,
            "color": "#FFD700"
        },
        {
            "id": "earth",
            "name": "Earth",
            "type": "planet",
            "sprite": "Earth.png",
            "parentId": "sun",
            "orbitDistance": 15,
            "orbitSpeed": 10,
            "startAngle": 45,
            "size": 1.0,
            "color": "#4444FF"
        },
        {
            "id": "mars",
            "name": "Mars",
            "type": "planet",
            "sprite": "Mars.png",
            "parentId": "sun",
            "orbitDistance": 22,
            "orbitSpeed": 7,
            "startAngle": 120,
            "size": 0.8,
            "color": "#FF4444"
        }
    ]
}
```

### Week 2 Deliverables
- [ ] JSON data models created
- [ ] StarSystemLoader loads JSON + sprites
- [ ] StarSystemRenderer displays system
- [ ] Planets orbit the star
- [ ] Editor tool imports from file
- [ ] Test with your kid's first system!

**🎮 CHECKPOINT: Flying in your kid's star system!**

---

## Week 3-4: Polish & Simple Save

### 3.1 Simple Save System

**SaveData.cs:**
```csharp
[System.Serializable]
public class SaveData
{
    public string currentSystemId;
    public float shipX;
    public float shipY;
    public float shipRotation;
}
```

**SaveManager.cs:**
```csharp
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public SaveData Load()
    {
        if (!File.Exists(SavePath)) return null;
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}
```

### 3.2 Debug HUD

```csharp
using UnityEngine;
using TMPro;

public class DebugHUD : MonoBehaviour
{
    [SerializeField] private PlayerShipController ship;
    [SerializeField] private TextMeshProUGUI infoText;

    private void Update()
    {
        if (ship != null && infoText != null)
        {
            infoText.text = $"Speed: {ship.CurrentSpeed:F1}\n" +
                           $"Pos: ({ship.transform.position.x:F0}, {ship.transform.position.y:F0})";
        }
    }

    public void OnSaveClicked()
    {
        var data = new SaveData
        {
            shipX = ship.transform.position.x,
            shipY = ship.transform.position.y,
            shipRotation = ship.transform.eulerAngles.z
        };
        SaveManager.Instance.Save(data);
        Debug.Log("Saved!");
    }

    public void OnLoadClicked()
    {
        var data = SaveManager.Instance.Load();
        if (data != null)
        {
            ship.transform.position = new Vector3(data.shipX, data.shipY, 0);
            ship.transform.rotation = Quaternion.Euler(0, 0, data.shipRotation);
            Debug.Log("Loaded!");
        }
    }
}
```

### Week 3-4 Deliverables
- [ ] Save/Load ship position works
- [ ] Thruster trail visual polish
- [ ] Test multiple star systems
- [ ] Fix any bugs from playtesting
- [ ] **ObjectPool system implemented** (+2 hours - prevent mobile GC spikes)
- [ ] **Sprite Atlas created** (+1 day - CelestialsAtlas.spriteatlas for all celestial sprites)
- [ ] **Unit tests written for core logic** (see testing requirements below)
- [ ] **Code coverage: 50%+ minimum** (target 60%)

### 3.2 Testing Requirements (Phase 1)

**CRITICAL**: Write unit tests for all AI-written code. See `.plans/context/testing-strategy.md`.

**Minimum Coverage (Phase 1):**
- **Overall**: 50% minimum, 60% target
- **Critical Path**: 80%+ (StarSystemLoader, SaveManager)
- **JSON Parsing**: 90%+ (data integrity)

**Test Structure:**
```
Assets/Tests/
├── EditMode/                    # Pure C# tests
│   ├── Data/
│   │   ├── JsonSerializationTests.cs
│   │   └── SaveDataTests.cs
│   └── Logic/
│       └─ [None for Phase 1]
└── PlayMode/                    # Unity-dependent tests
    ├── PlayerShipControllerTests.cs
    ├── StarSystemLoaderTests.cs
    └── SaveManagerTests.cs
```

**Example Test (Edit Mode):**
```csharp
[TestFixture]
public class JsonSerializationTests
{
    [Test]
    public void DeserializeStarSystem_ValidJson_Success()
    {
        string json = @"{""system"":{""id"":""test"",""name"":""Test""}}";
        var system = JsonUtility.FromJson<StarSystemJson>(json);
        Assert.NotNull(system);
        Assert.AreEqual("test", system.system.id);
    }
}
```

**Run Tests:**
- Window → General → Test Runner
- Click "Run All" for EditMode and PlayMode
- All tests must pass before committing

**See `.plans/context/testing-strategy.md` for complete testing approach and examples.**

---

## Phase 1 Complete Checklist

### Ship ✈️
- [ ] W/A/S/D + gamepad controls
- [ ] Momentum and drag
- [ ] Camera follow
- [ ] Boundary limits
- [ ] Thruster trail

### JSON Import 📦
- [ ] Load JSON from StreamingAssets
- [ ] Load PNG sprites
- [ ] Display star at center
- [ ] Planets orbit correctly
- [ ] Editor import tool works

### Infrastructure 🔧
- [ ] Simple save/load
- [ ] Debug HUD

**🎮 PHASE 1 COMPLETE: Flying in custom star systems!**

---

## Phase 1.5: Logic Extraction Refactoring

**Duration:** 3 days
**Trigger:** After Phase 1 prototype validated
**Goal:** Separate testable logic from Unity MonoBehaviours to achieve 60%+ test coverage

### Why Now?
- Phase 1 complete = core mechanics proven
- Before Phase 2 = prevents tech debt in math system
- Achieves 60%+ test coverage (Phase 1 target)
- Makes codebase more maintainable and testable

### What to Extract

#### 1. ShipMovementLogic (Pure C#)
**Purpose:** Calculate ship velocity based on inputs (fully testable, no Unity dependencies)

```csharp
// Assets/Scripts/Logic/ShipMovementLogic.cs
public class ShipMovementLogic
{
    public Vector2 CalculateVelocity(
        Vector2 currentVelocity,
        float thrustInput,
        float rotationInput,
        float acceleration,
        float maxSpeed,
        float deltaTime)
    {
        // Pure logic - fully testable
        float angle = rotationInput * Mathf.Deg2Rad;
        Vector2 thrust = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)) * thrustInput * acceleration;

        Vector2 newVelocity = currentVelocity + thrust * deltaTime;
        return Vector2.ClampMagnitude(newVelocity, maxSpeed);
    }
}
```

**Tests:**
- `CalculateVelocity_NoInput_ReturnsZero()`
- `CalculateVelocity_FullThrust_ReturnsMaxSpeed()`
- `CalculateVelocity_Rotation_ChangesDirection()`
- 10+ edge case tests

#### 2. DifficultyCalculator (Pure C#)
**Purpose:** Calculate math difficulty tier from galaxy distance

```csharp
// Assets/Scripts/Logic/DifficultyCalculator.cs
public class DifficultyCalculator
{
    public int CalculateDifficulty(float distanceFromGalaxyCenter)
    {
        if (distanceFromGalaxyCenter < 0.2f) return 1; // Addition to 10
        if (distanceFromGalaxyCenter < 0.4f) return 2; // Multiplication to 5
        if (distanceFromGalaxyCenter < 0.6f) return 3; // Multiplication to 10
        if (distanceFromGalaxyCenter < 0.8f) return 4; // Division
        return 5; // Mixed operations (NOT fractions)
    }
}
```

**Tests:**
- `CalculateDifficulty_GalaxyEdge_ReturnsTier1()`
- `CalculateDifficulty_GalaxyCenter_ReturnsTier5()`
- 5+ boundary tests

#### 3. OrbitCalculator (Pure C#)
**Purpose:** Calculate orbital positions for celestial bodies

```csharp
// Assets/Scripts/Logic/OrbitCalculator.cs
public class OrbitCalculator
{
    public Vector2 CalculateOrbitPosition(
        Vector2 parentPosition,
        float orbitDistance,
        float currentAngle)
    {
        float x = parentPosition.x + Mathf.Cos(currentAngle * Mathf.Deg2Rad) * orbitDistance;
        float y = parentPosition.y + Mathf.Sin(currentAngle * Mathf.Deg2Rad) * orbitDistance;
        return new Vector2(x, y);
    }

    public float UpdateOrbitAngle(float currentAngle, float orbitSpeed, float deltaTime)
    {
        return currentAngle + orbitSpeed * deltaTime;
    }
}
```

**Tests:**
- `CalculateOrbitPosition_ZeroAngle_ReturnsRightPosition()`
- `CalculateOrbitPosition_90Degrees_ReturnsTopPosition()`
- 8+ orbital tests

### Updating MonoBehaviours

**Before (Monolithic):**
```csharp
public class PlayerShipController : MonoBehaviour
{
    private void Update()
    {
        // Logic embedded in MonoBehaviour = untestable
        float angle = _currentRotation * Mathf.Deg2Rad;
        Vector2 thrust = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _thrustInput;
        _velocity = Vector2.ClampMagnitude(_velocity + thrust, _maxSpeed);
        transform.position += (Vector3)_velocity * Time.deltaTime;
    }
}
```

**After (Thin Wrapper):**
```csharp
public class PlayerShipController : MonoBehaviour
{
    private ShipMovementLogic _logic = new ShipMovementLogic();

    private void Update()
    {
        // Thin wrapper around testable logic
        _velocity = _logic.CalculateVelocity(
            _velocity,
            _thrustInput,
            _currentRotation,
            _acceleration,
            _maxSpeed,
            Time.deltaTime
        );

        transform.position += (Vector3)_velocity * Time.deltaTime;
    }
}
```

**Benefits:**
- ShipMovementLogic is fully testable (no Unity dependencies)
- MonoBehaviour is thin wrapper (minimal logic)
- Achieves 60%+ coverage

### Deliverables

**Code:**
- [ ] Assets/Scripts/Logic/ShipMovementLogic.cs
- [ ] Assets/Scripts/Logic/DifficultyCalculator.cs
- [ ] Assets/Scripts/Logic/OrbitCalculator.cs
- [ ] Update: PlayerShipController.cs (use ShipMovementLogic)
- [ ] Update: MathPuzzle.cs (use DifficultyCalculator)
- [ ] Update: CelestialBody.cs (use OrbitCalculator)

**Tests:**
- [ ] Assets/Tests/EditMode/Logic/ShipMovementLogicTests.cs (10+ tests)
- [ ] Assets/Tests/EditMode/Logic/DifficultyCalculatorTests.cs (5+ tests)
- [ ] Assets/Tests/EditMode/Logic/OrbitCalculatorTests.cs (8+ tests)

**Coverage:** 60%+ (Phase 1 target achieved)

**Timeline:** 3 days

**🎮 CHECKPOINT: Phase 1.5 refactoring complete - codebase is testable!**

---

## Content Your Kid Needs to Create

For Phase 1 testing:
1. Create a star system design
2. Export as JSON (format above)
3. Create/find PNG sprites for sun + planets
4. Put in a folder: `Sol/sol_system.json` + `Sol/Sun.png` + etc.
5. Use "Milo → Import Star System JSON" in Unity

That's the content pipeline you'll use throughout development!

---

*Phase 2 will add: clicking planets, math puzzles, planet scanning*