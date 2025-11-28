# Performance Targets
## Milo's Space Adventure - FPS and Optimization Requirements

## Overview

This document defines performance targets, budgets, and optimization strategies for Milo's Space Adventure across PC and mobile platforms.

**Core Principle**: Smooth, responsive gameplay is critical for young players (ages 5-8). Frame drops cause frustration and hurt the learning experience.

## Target Frame Rates

### PC (Windows, Mac, Linux)

**Minimum**: 60 FPS
**Target**: 60 FPS (locked)
**Resolution**: 1920x1080 (Full HD)

**Minimum Specs:**
- CPU: Dual-core 2.0 GHz
- GPU: Integrated graphics (Intel HD 4000 / equivalent)
- RAM: 4 GB
- Storage: 500 MB

**Recommended Specs:**
- CPU: Quad-core 2.5 GHz
- GPU: Dedicated graphics (GTX 660 / equivalent)
- RAM: 8 GB
- Storage: 1 GB

### Mobile (iOS, Android)

**Minimum**: 30 FPS
**Target**: 30 FPS (stable)
**Resolution**: 1280x720 (HD) to 2340x1080 (FHD+)

**Minimum Specs:**
- **iOS**: iPhone 8 / iPad 2018 (A11 Bionic)
- **Android**: Snapdragon 660 / Exynos 7885 equivalent
- RAM: 2 GB
- Storage: 300 MB

**Recommended Specs:**
- **iOS**: iPhone XR / iPad Air 2020 (A12+)
- **Android**: Snapdragon 730 / Exynos 9610+
- RAM: 4 GB
- Storage: 500 MB

## Performance Budgets

### Frame Time Budgets

**PC (60 FPS target):**
- Total frame budget: 16.67ms
- Rendering: 8ms (48%)
- Game logic: 4ms (24%)
- Physics: 2ms (12%)
- UI: 1ms (6%)
- Audio: 0.5ms (3%)
- Overhead: 1.17ms (7%)

**Mobile (30 FPS target):**
- Total frame budget: 33.33ms
- Rendering: 16ms (48%)
- Game logic: 8ms (24%)
- Physics: 4ms (12%)
- UI: 2ms (6%)
- Audio: 1ms (3%)
- Overhead: 2.33ms (7%)

### Draw Call Budgets

**PC:**
- Maximum draw calls: 500 per frame
- Maximum batches: 150 per frame
- Maximum triangles: 100k per frame

**Mobile:**
- Maximum draw calls: 100 per frame
- Maximum batches: 50 per frame
- Maximum triangles: 30k per frame

### Memory Budgets

**PC:**
- Total memory: 1 GB
- Textures: 500 MB
- Audio: 100 MB
- Scene data: 200 MB
- Code/overhead: 200 MB

**Mobile:**
- Total memory: 400 MB
- Textures: 150 MB
- Audio: 50 MB
- Scene data: 100 MB
- Code/overhead: 100 MB

## Scene-Specific Targets

### Main Menu

| Platform | Target FPS | Draw Calls | Memory |
|----------|-----------|------------|--------|
| PC | 60 | < 50 | 200 MB |
| Mobile | 30 | < 20 | 80 MB |

**Scene Complexity:**
- Static UI elements
- Animated title logo
- Background music
- Minimal 3D rendering

### Star System (Gameplay)

| Platform | Target FPS | Draw Calls | Memory |
|----------|-----------|------------|--------|
| PC | 60 | < 300 | 600 MB |
| Mobile | 30 | < 80 | 250 MB |

**Scene Complexity:**
- 1 star + 8 planets + 10 moons = ~20 bodies
- Player ship + particles
- Minimap rendering
- UI overlay
- Background starfield

### Math Puzzle UI

| Platform | Target FPS | Draw Calls | Memory |
|----------|-----------|------------|--------|
| PC | 60 | < 100 | 300 MB |
| Mobile | 30 | < 40 | 120 MB |

**Scene Complexity:**
- 3D planet in background
- UI overlay (puzzle interface)
- Particle effects (on success)
- Audio clips

## Optimization Strategies

### Rendering Optimization

#### Sprite Atlasing

**Problem**: Each sprite = 1 draw call
**Solution**: Combine sprites into atlases

```
Before:
- 8 planet sprites = 8 draw calls
- 10 moon sprites = 10 draw calls
- UI elements = 15 draw calls
- Total: 33 draw calls

After (with atlasing):
- 1 celestial atlas = 1 draw call (batched)
- 1 UI atlas = 1 draw call (batched)
- Total: ~5 draw calls
```

**Implementation:**
- Unity Sprite Atlas (2022.3+)
- Separate atlases: Celestials, UI, Effects
- Late binding for runtime texture swapping

#### Object Pooling

**Problem**: Instantiate/Destroy causes GC spikes
**Solution**: Pool frequently spawned objects

**Pooled Objects:**
- Particle effects (thrusters, explosions)
- UI popups (scan results, notifications)
- Orbit path renderers (reused per body)

```csharp
// ObjectPool.cs
public class ObjectPool<T> where T : Component
{
    private Queue<T> _pool = new Queue<T>();
    private T _prefab;
    private Transform _parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (_pool.Count > 0)
        {
            T obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            return Object.Instantiate(_prefab, _parent);
        }
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
```

#### Level of Detail (LOD)

**Problem**: Distant objects render at full detail
**Solution**: Reduce detail based on camera distance

**LOD Strategy:**
- **Close (< 200 units)**: Full detail sprites
- **Medium (200-500 units)**: Lower-res sprites
- **Far (> 500 units)**: Simplified dots/points

**Implementation (Phase 1 - Simple):**
```csharp
// CelestialBody.cs
private void Update()
{
    float distance = Vector3.Distance(transform.position, Camera.main.transform.position);

    if (distance > 500f)
    {
        _renderer.enabled = false; // Culled
    }
    else if (distance > 200f)
    {
        _renderer.sprite = _lowResSprite; // Low detail
    }
    else
    {
        _renderer.sprite = _highResSprite; // Full detail
    }
}
```

#### Frustum Culling

**Problem**: Off-screen objects still rendered
**Solution**: Unity's automatic frustum culling + manual checks

**Manual Culling (for large systems):**
```csharp
// StarSystemLoader.cs
private void UpdateVisibility()
{
    Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

    foreach (var body in _allBodies)
    {
        bool isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, body.Bounds);
        body.SetActive(isVisible);
    }
}
```

### Physics Optimization

#### Fixed Timestep Adjustment

**PC (60 FPS):**
```csharp
Time.fixedDeltaTime = 0.02f; // 50 Hz physics (default)
```

**Mobile (30 FPS):**
```csharp
Time.fixedDeltaTime = 0.04f; // 25 Hz physics (reduced for mobile)
```

#### Layer-Based Collisions

**Problem**: All objects check collisions with all other objects
**Solution**: Disable unnecessary layer interactions

**Layer Setup:**
```
Layers:
- Player (8)
- Planets (9)
- Moons (10)
- UI (5)
```

**Collision Matrix (Project Settings → Physics2D):**
```
         Player  Planets  Moons   UI
Player     ✓       ✓       ✓      ✗
Planets    ✓       ✗       ✗      ✗
Moons      ✓       ✗       ✗      ✗
UI         ✗       ✗       ✗      ✗
```

### Memory Optimization

#### Texture Compression

**PC:**
- Format: DXT5 (RGBA) / DXT1 (RGB)
- Max size: 2048x2048
- Mipmaps: Enabled

**Mobile:**
- Format: ASTC 6x6 (best quality/size)
- Max size: 1024x1024
- Mipmaps: Enabled

**Settings:**
```
Texture Import Settings:
- Max Size: 1024 (mobile), 2048 (PC)
- Compression: ASTC 6x6 (mobile), DXT5 (PC)
- Generate Mip Maps: Yes
- Filter Mode: Bilinear
- Aniso Level: 1
```

#### Audio Compression

**Music:**
- Format: Vorbis (streaming)
- Quality: 128 kbps
- Load type: Streaming

**SFX:**
- Format: ADPCM (short clips)
- Quality: 22050 Hz
- Load type: Decompress on load

#### Async Loading

**Problem**: Loading blocks main thread (frame drops)
**Solution**: Async scene/asset loading

```csharp
// StarSystemLoader.cs
public IEnumerator LoadSystemAsync(string systemId)
{
    // Load JSON asynchronously
    using (UnityWebRequest request = UnityWebRequest.Get(jsonPath))
    {
        yield return request.SendWebRequest();
        // Process in chunks to avoid frame spikes
    }

    // Load sprites asynchronously
    foreach (var bodyData in systemData.rootBodies)
    {
        StartCoroutine(LoadSpriteAsync(bodyData.sprite));
        yield return null; // Spread over multiple frames
    }
}
```

### UI Optimization

#### Canvas Separation

**Problem**: One canvas = rebuild entire UI on any change
**Solution**: Separate static and dynamic canvases

**Canvas Setup:**
```
UI Root
├─ StaticCanvas (World Space)
│  └─ Planet labels (rarely change)
├─ DynamicCanvas (Screen Space - Overlay)
│  ├─ Mobile controls (updates every frame)
│  └─ Speed/position display
└─ PopupCanvas (Screen Space - Overlay)
   └─ Scan results, puzzle UI (rarely visible)
```

#### Text Mesh Pro

**Problem**: Unity Text causes high draw calls
**Solution**: Use TextMesh Pro (TMP)

**Benefits:**
- Better batching
- Sharper text at any resolution
- SDF rendering (mobile-friendly)

### Garbage Collection (GC) Optimization

#### Avoid Allocations in Update()

**❌ Bad (allocates every frame):**
```csharp
void Update()
{
    string debugText = "Speed: " + velocity.magnitude; // Allocates string
}
```

**✅ Good (no allocations):**
```csharp
private StringBuilder _debugText = new StringBuilder(32);

void Update()
{
    _debugText.Clear();
    _debugText.Append("Speed: ");
    _debugText.Append(velocity.magnitude.ToString("F2"));
}
```

#### Cache Component References

**❌ Bad:**
```csharp
void Update()
{
    GetComponent<Rigidbody2D>().velocity = newVelocity; // Allocates
}
```

**✅ Good:**
```csharp
private Rigidbody2D _rb;

void Awake()
{
    _rb = GetComponent<Rigidbody2D>(); // Cache once
}

void Update()
{
    _rb.velocity = newVelocity; // No allocation
}
```

## Profiling and Monitoring

### Unity Profiler

**Required Metrics:**
1. **CPU Usage** (target < 70% average)
2. **Rendering** (target < 8ms PC, < 16ms mobile)
3. **Scripts** (target < 4ms PC, < 8ms mobile)
4. **Physics** (target < 2ms PC, < 4ms mobile)
5. **GC Alloc** (target < 1 MB per frame)

**Profiling Workflow:**
1. Profile in build (not editor)
2. Test on minimum-spec devices
3. Record 5-minute gameplay sessions
4. Identify frame spikes (> 100ms)
5. Fix highest-impact issues first

### Performance Testing

**Automated Tests:**
```csharp
// Assets/Tests/PlayMode/PerformanceTests.cs
using NUnit.Framework;
using Unity.PerformanceTesting;

[TestFixture]
public class PerformanceTests
{
    [Test, Performance]
    public void StarSystem_LoadTime_Under2Seconds()
    {
        Measure.Method(() =>
        {
            StarSystemLoader.Instance.LoadSystem("sol");
        })
        .WarmupCount(2)
        .MeasurementCount(10)
        .Run();

        // Assert load time < 2s
    }

    [Test, Performance]
    public void Gameplay_FrameTime_Under16ms()
    {
        Measure.Frames()
            .WarmupCount(60)
            .MeasurementCount(300)
            .Run();

        // Assert 95th percentile < 16ms
    }
}
```

### Mobile-Specific Profiling

**Tools:**
- **iOS**: Xcode Instruments
- **Android**: Android Profiler (Android Studio)
- **Unity**: Unity Remote for on-device profiling

**Key Metrics:**
- Battery usage (< 5% per 10 minutes)
- Thermal throttling (avoid sustained high CPU)
- Memory warnings (watch for spikes)

## Platform-Specific Settings

### PC Quality Settings

```csharp
// Graphics Settings (PC)
QualitySettings.SetQualityLevel(2, true); // "Good" preset

QualitySettings.vSyncCount = 1; // Enable VSync for 60 FPS cap
QualitySettings.antiAliasing = 2; // 2x MSAA
QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
QualitySettings.shadows = ShadowQuality.All;
Application.targetFrameRate = -1; // Unlimited (VSync controls it)
```

### Mobile Quality Settings

```csharp
// Graphics Settings (Mobile)
QualitySettings.SetQualityLevel(0, true); // "Very Low" preset

QualitySettings.vSyncCount = 0; // Disable VSync (use targetFrameRate)
QualitySettings.antiAliasing = 0; // No MSAA
QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
QualitySettings.shadows = ShadowQuality.Disable;
Application.targetFrameRate = 30; // Cap at 30 FPS
```

### Dynamic Quality Adjustment

```csharp
// QualityManager.cs
public class QualityManager : MonoBehaviour
{
    private float _avgFrameTime;
    private const float TARGET_FRAME_TIME_PC = 16.67f;
    private const float TARGET_FRAME_TIME_MOBILE = 33.33f;

    private void Update()
    {
        _avgFrameTime = Mathf.Lerp(_avgFrameTime, Time.deltaTime * 1000f, 0.1f);

        if (Application.isMobilePlatform)
        {
            if (_avgFrameTime > TARGET_FRAME_TIME_MOBILE * 1.2f)
            {
                ReduceQuality();
            }
        }
        else
        {
            if (_avgFrameTime > TARGET_FRAME_TIME_PC * 1.2f)
            {
                ReduceQuality();
            }
        }
    }

    private void ReduceQuality()
    {
        // Disable orbit rings
        // Reduce particle effects
        // Lower texture resolution
        Debug.LogWarning("Performance degraded - reducing quality");
    }
}
```

## Testing Devices

### Minimum Test Matrix

**PC:**
- Windows 10 (low-end laptop, integrated GPU)
- macOS Monterey (MacBook Air 2018)
- Linux Ubuntu 20.04 (mid-range desktop)

**Mobile:**
- **iOS**: iPhone 8 (2017), iPad 2018
- **Android**: Samsung Galaxy A50 (2019), Google Pixel 4a

### Performance Validation Checklist

- [ ] 60 FPS sustained on PC (minimum specs)
- [ ] 30 FPS sustained on mobile (minimum specs)
- [ ] No frame drops during scene transitions
- [ ] Load times < 2s for star systems
- [ ] GC spikes < 100ms
- [ ] Memory usage within budgets
- [ ] Battery drain < 5% per 10 minutes (mobile)
- [ ] No thermal throttling after 30 minutes (mobile)

## Common Performance Pitfalls

### ❌ Don't: Use OnGUI()

```csharp
void OnGUI() // NEVER USE - causes huge GC allocations
{
    GUI.Label(new Rect(10, 10, 100, 20), "Speed: " + speed);
}
```

### ✅ Do: Use TextMesh Pro

```csharp
[SerializeField] private TMP_Text _speedText;

void Update()
{
    _speedText.text = $"Speed: {speed:F2}";
}
```

### ❌ Don't: Use Find() in Update()

```csharp
void Update()
{
    GameObject player = GameObject.Find("Player"); // SLOW
}
```

### ✅ Do: Cache references

```csharp
private GameObject _player;

void Start()
{
    _player = GameObject.Find("Player"); // Cache once
}
```

### ❌ Don't: Use Camera.main repeatedly

```csharp
void Update()
{
    Vector3 pos = Camera.main.transform.position; // Allocates
}
```

### ✅ Do: Cache Camera reference

```csharp
private Camera _mainCamera;

void Awake()
{
    _mainCamera = Camera.main; // Cache once
}

void Update()
{
    Vector3 pos = _mainCamera.transform.position;
}
```

## Summary

**Key Targets:**
- **PC**: 60 FPS minimum
- **Mobile**: 30 FPS minimum
- **Load Times**: < 2s per system
- **Memory**: < 1 GB PC, < 400 MB mobile

**Critical Optimizations:**
1. Sprite atlasing (reduce draw calls)
2. Object pooling (reduce GC)
3. Async loading (no frame drops)
4. Layer-based collisions (reduce physics cost)
5. Texture compression (reduce memory)

**Performance First Mindset:**
- Profile early and often
- Test on minimum-spec devices
- Optimize bottlenecks, not guesses
- Never sacrifice frame rate for visual flair

**References:**
- Unity Manual: Optimization (https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html)
- Unity Profiler: https://docs.unity3d.com/Manual/Profiler.html
- See `testing-strategy.md` for performance test examples
