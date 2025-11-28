# JSON Schema Reference
## Milo's Space Adventure - Star System Data Format

## Overview

This document defines the complete JSON schema for star system data in Milo's Space Adventure. The schema is based on the actual structure from the content creation tool, as exemplified by `sol.json`.

**Key Characteristics:**
- Hierarchical structure with nested `children` arrays
- Rich visual properties for rendering (scale, colors, rotation)
- Orbital mechanics data for physics simulation
- Gameplay state (scanned status, stuffy signals)
- Galaxy-wide progression (distance from center)
- Type-specific fields (planet numbers, moon letters, star luminosity)

## File Structure

```
StreamingAssets/
└── Systems/
    ├── sol.json           # Solar System (starting system)
    ├── alpha-centauri.json
    ├── barnards-star.json
    └── ... (more star systems)
```

## Root Schema

### StarSystemJson (Root Object)

```csharp
[Serializable]
public class StarSystemJson
{
    public SystemInfo system;              // System metadata
    public CelestialBodyJson[] rootBodies; // Top-level bodies (usually 1 star)
}
```

**Example:**
```json
{
  "system": { ... },
  "rootBodies": [ { "id": "Sol", "type": "star", ... } ]
}
```

**Validation:**
- `system` is required
- `rootBodies` must have at least 1 element
- `rootBodies` typically contains 1 star (binary systems may have 2)

## System Metadata

### SystemInfo

```csharp
[Serializable]
public class SystemInfo
{
    public string id;                      // Unique system identifier (e.g., "sol")
    public string name;                    // Display name (e.g., "Sol")
    public Bounds bounds;                  // System boundaries for camera/gameplay
    public float distanceFromGalaxyCenter; // 0.0 (edge) to 1.0 (center) - CRITICAL for difficulty
                                           // NOTE: REQUIRED field - assume 0.0 if missing in Phase 1
}

[Serializable]
public class Bounds
{
    public int width;  // System width in world units
    public int height; // System height in world units
}
```

**Example:**
```json
{
  "system": {
    "id": "sol",
    "name": "Sol",
    "bounds": { "width": 1500, "height": 1500 },
    "distanceFromGalaxyCenter": 0.0
  }
}
```

**Field Details:**

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `id` | string | ✅ | - | Lowercase identifier (kebab-case recommended) |
| `name` | string | ✅ | - | Human-readable name shown in UI |
| `bounds.width` | int | ✅ | - | System width (typical: 1000-2000) |
| `bounds.height` | int | ✅ | - | System height (typical: 1000-2000) |
| `distanceFromGalaxyCenter` | float | ✅ | - | **CRITICAL**: 0.0 = galaxy edge (easy), 1.0 = center (hard) |

**`distanceFromGalaxyCenter` Scaling:**
- **0.0 - 0.2**: Easy math (addition/subtraction to 10)
- **0.2 - 0.4**: Medium math (multiplication tables to 5)
- **0.4 - 0.6**: Challenging math (multiplication to 10)
- **0.6 - 0.8**: Hard math (division, multi-step)
- **0.8 - 1.0**: Very hard math (fractions, percentages)

**Validation:**
- `distanceFromGalaxyCenter` must be 0.0 ≤ x ≤ 1.0
- `bounds` width/height must be positive integers
- `id` must be unique across all systems
- `id` should match filename (e.g., `sol.json` → `"id": "sol"`)

## Celestial Bodies

### CelestialBodyJson (Hierarchical)

```csharp
[Serializable]
public class CelestialBodyJson
{
    // ===== CORE IDENTITY =====
    public string id;                      // Unique identifier (e.g., "Sol 3", "Sol 3 a")
    public string name;                    // Display name (e.g., "Earth", "Luna")
    public string description;             // Educational description for scanning UI
    public string type;                    // "star", "planet", "moon", "asteroid", "station"

    // ===== VISUAL PROPERTIES =====
    public string sprite;                  // Sprite path (empty = use fallback procedural)
    public float scale;                    // Visual scale multiplier (0.1 - 3.0 typical)
    public int baseSize;                   // Base sprite size in pixels (24, 48, 64)
    public string fallbackColor;           // Hex color if sprite missing (e.g., "#3B82F6")

    // ===== ORBITAL MECHANICS =====
    public string parentId;                // ID of parent body (null for stars)
    public float orbitDistance;            // Distance from parent in world units
    public float orbitSpeed;               // Orbit speed (degrees/second, can be negative)
    public float orbitAngle;               // Starting angle in degrees (0-360)
    public string orbitRingColor;          // Orbit path color (e.g., "rgba(100, 116, 139, 0.3)")
    public float orbitRingWidth;           // Orbit path line width

    // ===== ROTATION =====
    public float rotation;                 // Initial rotation angle in degrees
    public float rotationSpeed;            // Rotation speed (degrees/second, can be negative)

    // ===== GAMEPLAY STATE =====
    public bool scanned;                   // Has player scanned this body? (false = not on minimap)
    public bool hasStuffySignal;           // Does this body have a stuffy to rescue?
    public string stuffyId;                // Which stuffy is here? (null if no stuffy)

    // ===== TYPE-SPECIFIC FIELDS =====
    public int? planetNumber;              // 1-8 for planets (null for non-planets)
    public string moonLetter;              // "a", "b", "c"... for moons (null for non-moons)
    public float? luminosity;              // Light emission for stars (0.0-1.0, null for non-stars)

    // ===== HIERARCHY =====
    public CelestialBodyJson[] children;   // Child bodies (moons, stations, etc.)
}
```

### Type-Specific Examples

#### Star

```json
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
  "children": [ ... planets ... ]
}
```

**Star Validation:**
- `type` = `"star"`
- `parentId` = `null`
- `orbitDistance` = `0`
- `orbitSpeed` = `0`
- `luminosity` required (0.0-1.0, typically 1.0 for main star)
- `planetNumber` and `moonLetter` must be `null`
- `children` contains planets

#### Planet

```json
{
  "id": "Sol 3",
  "name": "Earth",
  "description": "A blue-green world teeming with life. The only known planet to harbor life.",
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
  "children": [ ... moons ... ]
}
```

**Planet Validation:**
- `type` = `"planet"`
- `parentId` must reference a star
- `planetNumber` required (1-N, sequential)
- `orbitDistance` > 0
- `moonLetter` and `luminosity` must be `null`
- `children` contains moons (can be empty)

#### Moon

```json
{
  "id": "Sol 3 a",
  "name": "Luna",
  "description": "Earth's only natural satellite, the fifth largest moon in the Solar System.",
  "type": "moon",
  "sprite": "",
  "parentId": "Sol 3",
  "orbitDistance": 35,
  "orbitSpeed": 2,
  "orbitAngle": 0,
  "scale": 0.27,
  "rotation": 0,
  "rotationSpeed": 0,
  "moonLetter": "a",
  "baseSize": 24,
  "fallbackColor": "#94A3B8",
  "orbitRingColor": "rgba(100, 116, 139, 0.2)",
  "orbitRingWidth": 0.5,
  "children": []
}
```

**Moon Validation:**
- `type` = `"moon"`
- `parentId` must reference a planet
- `moonLetter` required ("a", "b", "c", ... "z")
- `moonLetter` must be unique among siblings
- `orbitDistance` > 0
- `planetNumber` and `luminosity` must be `null`
- `children` typically empty (but could have sub-moons or stations)

## Field Reference

### Core Identity Fields

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `id` | string | ✅ | - | Unique identifier (format: "System N [letter]") |
| `name` | string | ✅ | - | Display name shown in UI |
| `description` | string | ✅ | - | Educational text shown when scanned |
| `type` | string | ✅ | - | "star", "planet", "moon", "asteroid", "station" |

**ID Format Convention:**
- **Stars**: System name (e.g., `"Sol"`)
- **Planets**: `"{SystemName} {N}"` (e.g., `"Sol 3"` for Earth)
- **Moons**: `"{PlanetId} {letter}"` (e.g., `"Sol 3 a"` for Luna)

### Visual Properties

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `sprite` | string | ✅ | `""` | Sprite path or empty for procedural |
| `scale` | float | ✅ | - | Visual size multiplier (0.1-3.0 typical) |
| `baseSize` | int | ✅ | - | Base sprite size: 24 (moon), 48 (planet), 64 (star) |
| `fallbackColor` | string | ✅ | - | Hex color (e.g., `"#3B82F6"`) |

**Color Palette Recommendations:**
- **Stars**: `#FFD700` (yellow), `#FF6B6B` (red giant), `#60A5FA` (blue giant)
- **Rocky Planets**: `#6366F1` (Mercury), `#EF4444` (Mars), `#3B82F6` (Earth)
- **Gas Giants**: `#F59E0B` (Jupiter), `#D97706` (Saturn)
- **Ice Giants**: `#06B6D4` (Uranus), `#3B82F6` (Neptune)
- **Moons**: `#94A3B8`, `#9CA3AF`, `#6B7280` (grays)

### Orbital Mechanics

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `parentId` | string | ⚠️ | `null` | Parent body ID (null for stars) |
| `orbitDistance` | float | ✅ | - | Distance from parent (0 for stars) |
| `orbitSpeed` | float | ✅ | - | Degrees/second (can be negative for retrograde) |
| `orbitAngle` | float | ✅ | - | Starting angle 0-360 degrees |
| `orbitRingColor` | string | ❌ | `null` | RGBA color string for orbit path |
| `orbitRingWidth` | float | ❌ | `1` | Orbit path line width |

**Orbit Speed Guidelines:**
- **Inner planets**: 1.0-5.0 (faster orbits)
- **Outer planets**: 0.01-0.5 (slower orbits)
- **Moons**: 1.5-4.0 (relative to parent)
- **Retrograde**: Negative values (e.g., Venus rotationSpeed: -0.004)

### Rotation

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `rotation` | float | ✅ | - | Initial rotation angle in degrees |
| `rotationSpeed` | float | ✅ | - | Degrees/second (can be negative) |

**Rotation Speed Guidelines:**
- **Fast rotators**: 0.5-1.0 (gas giants)
- **Normal**: 0.1-0.3 (Earth-like)
- **Slow**: 0.01-0.05 (Mercury, Venus)
- **Tidally locked**: 0.0 (most moons)

### Gameplay State

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `scanned` | bool | ❌ | `false` | Has player scanned this body? |
| `hasStuffySignal` | bool | ❌ | `false` | Does this body have a stuffy? |
| `stuffyId` | string | ❌ | `null` | Which stuffy ("earthy", "brainy", etc.) |

**Scanning Gameplay Loop:**
1. **Enter new system**: All `scanned` = `false` → planets invisible on minimap
2. **Scan system** (press S): Reveals all planets on minimap, still `scanned` = `false`
3. **Fly to planet**: Get within range, press S → `scanned` = `true`
4. **Scanned state**: Show name, description, stuffy signals on minimap

**Stuffy Signals:**
- Only visible when `scanned` = `true`
- `hasStuffySignal` = `true` shows special icon on minimap
- `stuffyId` determines which stuffy is rescued on this planet

### Type-Specific Fields

| Field | Type | For Types | Required | Description |
|-------|------|-----------|----------|-------------|
| `planetNumber` | int | planet | ✅ | 1-N (sequential from star) |
| `moonLetter` | string | moon | ✅ | "a"-"z" (sequential from planet) |
| `luminosity` | float | star | ✅ | 0.0-1.0 (light emission) |

**Validation:**
- Stars: `luminosity` required, others `null`
- Planets: `planetNumber` required, others `null`
- Moons: `moonLetter` required, others `null`

### Hierarchy

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `children` | array | ✅ | `[]` | Nested child bodies |

**Nesting Rules:**
- **Stars** → contain planets
- **Planets** → contain moons (and potentially stations)
- **Moons** → typically empty (but can contain sub-moons or stations)
- Maximum nesting depth: 3 levels recommended

## Complete C# Classes

### Unity-Compatible Classes

```csharp
using System;
using UnityEngine;

namespace MilosAdventure.Data
{
    [Serializable]
    public class StarSystemJson
    {
        public SystemInfo system;
        public CelestialBodyJson[] rootBodies;
    }

    [Serializable]
    public class SystemInfo
    {
        public string id;
        public string name;
        public Bounds bounds;
        public float distanceFromGalaxyCenter; // 0.0 = edge, 1.0 = center
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
        public string type; // "star", "planet", "moon", "asteroid", "station"

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
        public bool scanned = false;
        public bool hasStuffySignal = false;
        public string stuffyId = null;

        // Type-specific (nullable for types that don't use them)
        public int? planetNumber = null;      // 1-8 for planets
        public string moonLetter = null;      // "a"-"z" for moons
        public float? luminosity = null;      // 0.0-1.0 for stars

        // Hierarchy
        public CelestialBodyJson[] children = new CelestialBodyJson[0];
    }
}
```

### Loading Example

```csharp
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using MilosAdventure.Data;

public class StarSystemLoader : MonoBehaviour
{
    public IEnumerator LoadStarSystemAsync(string systemId)
    {
        string path = $"{Application.streamingAssetsPath}/Systems/{systemId}.json";

        // CRITICAL: Use UnityWebRequest for Android compatibility
        using (UnityWebRequest request = UnityWebRequest.Get(path))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                StarSystemJson system = JsonUtility.FromJson<StarSystemJson>(json);

                // Validate
                if (system == null || system.rootBodies == null || system.rootBodies.Length == 0)
                {
                    Debug.LogError($"Invalid system JSON: {systemId}");
                    yield break;
                }

                // Process system
                ProcessStarSystem(system);
            }
            else
            {
                Debug.LogError($"Failed to load {systemId}: {request.error}");
            }
        }
    }

    private void ProcessStarSystem(StarSystemJson system)
    {
        Debug.Log($"Loaded system: {system.system.name}");
        Debug.Log($"Distance from galaxy center: {system.system.distanceFromGalaxyCenter}");

        // Recursively process all bodies
        foreach (var body in system.rootBodies)
        {
            ProcessCelestialBody(body, 0);
        }
    }

    private void ProcessCelestialBody(CelestialBodyJson body, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}- {body.name} ({body.type})");

        // Process children recursively
        if (body.children != null)
        {
            foreach (var child in body.children)
            {
                ProcessCelestialBody(child, depth + 1);
            }
        }
    }
}
```

## Validation Rules

### System-Level Validation

```csharp
public static bool ValidateStarSystem(StarSystemJson system, out string error)
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

    if (system.system.distanceFromGalaxyCenter < 0f || system.system.distanceFromGalaxyCenter > 1f)
    {
        error = "distanceFromGalaxyCenter must be 0.0-1.0";
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

### Body-Level Validation

```csharp
public static bool ValidateCelestialBody(CelestialBodyJson body, out string error)
{
    if (string.IsNullOrEmpty(body.id))
    {
        error = "Body ID is required";
        return false;
    }

    if (string.IsNullOrEmpty(body.type))
    {
        error = "Body type is required";
        return false;
    }

    switch (body.type)
    {
        case "star":
            if (body.parentId != null)
            {
                error = "Stars cannot have parent";
                return false;
            }
            if (!body.luminosity.HasValue)
            {
                error = "Stars must have luminosity";
                return false;
            }
            break;

        case "planet":
            if (string.IsNullOrEmpty(body.parentId))
            {
                error = "Planets must have parent";
                return false;
            }
            if (!body.planetNumber.HasValue)
            {
                error = "Planets must have planetNumber";
                return false;
            }
            break;

        case "moon":
            if (string.IsNullOrEmpty(body.parentId))
            {
                error = "Moons must have parent";
                return false;
            }
            if (string.IsNullOrEmpty(body.moonLetter))
            {
                error = "Moons must have moonLetter";
                return false;
            }
            break;
    }

    // Validate children recursively
    if (body.children != null)
    {
        foreach (var child in body.children)
        {
            if (!ValidateCelestialBody(child, out error))
            {
                return false;
            }
        }
    }

    error = null;
    return true;
}
```

## Phase-Specific Fields

### Phase 1: Core Gameplay

**Required Fields:**
- All core identity fields
- All visual properties
- All orbital mechanics
- `scanned` (scanning gameplay)

**Optional/Not Used:**
- `hasStuffySignal`, `stuffyId` (used in Phase 3+)

### Phase 2: Math Puzzles

**New Usage:**
- `distanceFromGalaxyCenter` → Calculate math difficulty
- Math gets harder as player approaches galaxy center

### Phase 3: Password Puzzles & Stuffies

**New Usage:**
- `hasStuffySignal` → Show stuffy icon on minimap
- `stuffyId` → Determine which stuffy to rescue
- `description` → Used in fill-in-the-blank word puzzles

### Phase 4+: Abilities & Progression

**New Usage:**
- All fields used
- Additional fields may be added (asteroids, stations, etc.)

## Testing JSON Files

### Minimal Valid System

```json
{
  "system": {
    "id": "test",
    "name": "Test System",
    "bounds": { "width": 1000, "height": 1000 },
    "distanceFromGalaxyCenter": 0.5
  },
  "rootBodies": [
    {
      "id": "TestStar",
      "name": "Test Star",
      "description": "A test star for validation.",
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
      "children": []
    }
  ]
}
```

### Unit Test Example

```csharp
using NUnit.Framework;
using UnityEngine;
using MilosAdventure.Data;

[TestFixture]
public class JsonSchemaTests
{
    [Test]
    public void DeserializeSolSystem_ValidatesCorrectly()
    {
        // Load sol.json
        TextAsset jsonFile = Resources.Load<TextAsset>("TestData/sol");
        StarSystemJson system = JsonUtility.FromJson<StarSystemJson>(jsonFile.text);

        // Validate system
        Assert.IsNotNull(system);
        Assert.AreEqual("sol", system.system.id);
        Assert.AreEqual("Sol", system.system.name);
        Assert.AreEqual(1500, system.system.bounds.width);
        Assert.AreEqual(1500, system.system.bounds.height);

        // Validate root body
        Assert.AreEqual(1, system.rootBodies.Length);
        CelestialBodyJson sol = system.rootBodies[0];
        Assert.AreEqual("Sol", sol.id);
        Assert.AreEqual("star", sol.type);
        Assert.AreEqual(1f, sol.luminosity);

        // Validate hierarchy
        Assert.Greater(sol.children.Length, 0);

        // Validate Earth
        CelestialBodyJson earth = FindBodyById(sol, "Sol 3");
        Assert.IsNotNull(earth);
        Assert.AreEqual("Earth", earth.name);
        Assert.AreEqual("planet", earth.type);
        Assert.AreEqual(3, earth.planetNumber);

        // Validate Luna
        CelestialBodyJson luna = FindBodyById(earth, "Sol 3 a");
        Assert.IsNotNull(luna);
        Assert.AreEqual("Luna", luna.name);
        Assert.AreEqual("moon", luna.type);
        Assert.AreEqual("a", luna.moonLetter);
    }

    private CelestialBodyJson FindBodyById(CelestialBodyJson parent, string id)
    {
        if (parent.children == null) return null;

        foreach (var child in parent.children)
        {
            if (child.id == id) return child;
        }
        return null;
    }
}
```

## Common Pitfalls

### ❌ DON'T: Use flat structure

```json
{
  "bodies": [
    { "id": "Sol", "type": "star" },
    { "id": "Sol 1", "type": "planet", "parentId": "Sol" },
    { "id": "Sol 2", "type": "planet", "parentId": "Sol" }
  ]
}
```

### ✅ DO: Use hierarchical structure

```json
{
  "rootBodies": [
    {
      "id": "Sol",
      "type": "star",
      "children": [
        { "id": "Sol 1", "type": "planet", "children": [] },
        { "id": "Sol 2", "type": "planet", "children": [] }
      ]
    }
  ]
}
```

### ❌ DON'T: Use `File.ReadAllText` on Android

```csharp
string json = File.ReadAllText(path); // CRASHES ON ANDROID
```

### ✅ DO: Use `UnityWebRequest` for cross-platform compatibility

```csharp
using (UnityWebRequest request = UnityWebRequest.Get(path))
{
    yield return request.SendWebRequest();
    string json = request.downloadHandler.text;
}
```

### ❌ DON'T: Forget to initialize arrays

```csharp
public CelestialBodyJson[] children; // null by default - CRASHES
```

### ✅ DO: Initialize to empty array

```csharp
public CelestialBodyJson[] children = new CelestialBodyJson[0];
```

## Summary

**Key Takeaways:**
1. **Hierarchical structure** with nested `children` arrays
2. **Rich metadata** for visuals, orbits, rotation
3. **`distanceFromGalaxyCenter`** drives math difficulty progression
4. **`scanned`** controls minimap visibility and planet information
5. **Type-specific fields** (planetNumber, moonLetter, luminosity)
6. **Always use `UnityWebRequest`** for loading (Android compatibility)
7. **Validate recursively** through the hierarchy

**References:**
- See `sol.json` for complete Solar System example
- See `tech-plan.md` for implementation details
- See `testing-strategy.md` for JSON validation tests
