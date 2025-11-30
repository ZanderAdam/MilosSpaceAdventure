# MSA Unity Project Guidelines

## Workflow Rules
- ALWAYS stop Unity play mode before making code changes or modifying assets
- ALWAYS use game design agent to validate and review game changes
- ALWAYS use architecture reviewer agent for significant code changes
- ALWAYS use workflow orchestration agent to plan tasks
  - Tasks are for single developer
- Run tests after any logic changes - NO TEST FAILURES TOLERATED
- Check Unity console after script changes to verify compilation success
- Use async/await patterns for all I/O operations (JSON loading, web requests)

## AI Agent Decision Criteria

### Proceed Autonomously If:
- Adding serialized fields with `[Range]` and `[Tooltip]`
- Implementing transitions with documented easing functions
- Following established code patterns from existing scripts
- Adding performance optimizations (pooling, caching, throttling)
- Writing unit tests for pure logic classes
- Fixing compilation errors or test failures
- Implementing features defined in phase plans

### STOP and Ask User If:
- Feature requires kid playtesting (target age 5-8)
- Multiple valid design approaches exist
- Changing core game feel (ship physics, gravity lock timing values)
- Removing or significantly changing existing game mechanics
- Adding new game systems not documented in phase plans
- Performance optimization requires visual quality tradeoff
- Implementing features that require tutorials/instruction (game should teach through play)
- Adding complexity that violates KISS principle

## Unity-Specific Coding Standards

KISS - Keep It Simple, Stupid! Avoid unnecessary complexity. Lets get to a working prototype fast before over-optimizing. But keep code clean and maintainable so we can iterate quickly and make changes easily later.

### Component Structure (Strict Order)
1. Serialized fields (grouped by `[Header()]`)
2. Private fields (state variables with leading underscore)
3. Public properties (expose state as read-only)
4. Lifecycle methods (Awake → Start → Update → FixedUpdate)
5. Public methods (component API)
6. Private methods (implementation)
7. Physics callbacks (OnTriggerEnter2D, OnCollisionEnter, etc.)
8. Debug methods (OnDrawGizmos, OnDrawGizmosSelected)

### Serialization Patterns
- Use `[SerializeField]` for private fields needing inspector access
- Group related fields with `[Header("Category Name")]`
- Provide sensible defaults in field declarations
- Use `[Range(min, max)]` for numeric constraints
- Never serialize complex objects - use data classes instead

### MonoBehaviour Best Practices
- Separate input handling (Update) from physics (FixedUpdate)
- Cache component references in Awake(), not Update()
- Unsubscribe from events in OnDisable() to prevent memory leaks
- Use `null` checks before accessing external components
- Prefer composition over inheritance

### Physics & Movement
- All physics modifications must happen in FixedUpdate()
- Use `Time.fixedDeltaTime` for physics, `Time.deltaTime` for visuals
- Implement drag/friction with multiplicative decay: `velocity *= dragFactor`
- Apply forces to velocity, then integrate: `position += velocity * dt`
- Clamp speeds after all forces applied: `velocity = Vector2.ClampMagnitude(velocity, maxSpeed)`

### State Management
- Use private fields with public read-only properties
- Implement state transitions with easing functions (e.g., `Mathf.Pow(1f - progress, 3f)`)
- Timer-based transitions should normalize to [0,1] range
- Event-driven state changes (use C# events/UnityEvents)

### Pure Logic Classes
- Use static classes for stateless calculations (e.g., OrbitCalculator)
- All methods should be pure functions (same input → same output)
- Heavily unit test pure logic (15+ test cases minimum)
- No Unity-specific code in logic classes

## Game Feel Standards

### Timing Values (Use These Exact Values)
- **Gravity lock transition**: 0.8s (ease-out cubic)
- **UI button response**: <0.1s
- **State transitions**: 0.3-0.5s (satisfying action feel)
- **Touch target minimum**: 44pt × 44pt
- **Input to visual feedback**: <100ms (ideally 1 frame)

### Physics Feel Targets
- **Ship rotation speed**: 180°/sec (responsive but not twitchy)
- **Max ship speed**: 10 units/sec (current default in PlayerShipController)
- **Drag coefficient**: 0.95-0.98 (multiplicative per FixedUpdate)
- **Acceleration**: Gradual ramp-up for momentum feeling

### Easing Function Standard
- **ALWAYS use** `Mathf.Pow(1f - progress, 3f)` for ease-out cubic transitions
- **NEVER use** `Mathf.Lerp()` for game feel transitions (linear feels robotic)
- **Apply to**: Gravity lock, camera transitions, UI animations, state changes

### Visual Feedback Requirements
Every player action MUST have immediate visual response:
- **Thrust input**: Particles + trail appear within 1 frame
- **Rotation**: Ship sprite rotation immediate (no delay)
- **Gravity lock**: Indicator fade-in over 0.8s with pulsing
- **Speed**: Trail length/opacity scales with velocity
- **Collision**: Screen shake + particle burst + audio cue

### Required Code Pattern for Tunable Values
❌ **BAD** (hardcoded):
```csharp
private float lockDuration = 0.8f;
```

✅ **GOOD** (tunable with context):
```csharp
[Header("Lock Feel")]
[SerializeField]
[Range(0.1f, 2.0f)]
[Tooltip("Time for ship to smoothly lock into orbit (seconds)")]
private float lockTransitionDuration = 0.8f;
```

## Project-Specific Conventions

### Naming Conventions

**Classes:**
- `Controller` suffix = Main behavioral component (PlayerShipController)
- `System` suffix = Subsystem manager (GravityLockSystem)
- `Manager` suffix = Singleton (SaveManager, StarSystemLoader)
- `Calculator` suffix = Pure logic, no state (OrbitCalculator)
- `Renderer` suffix = Visual instantiation (StarSystemRenderer)
- `Data` suffix = Serializable structures (StarSystemData)

**Fields & Properties:**
- Private fields: `_camelCase` with leading underscore
- Public properties: `PascalCase`
- Serialized fields: `camelCase` (matches inspector)
- Constants: `UPPER_SNAKE_CASE`

**GameObjects & Prefabs:**
- PascalCase, descriptive names (e.g., "CelestialBody", "PlayerShip")
- Prefabs live in `Assets/_Project/Prefabs/`
- Prefer data-driven prefabs over hardcoded variants

**Scenes:**
- PascalCase with context (e.g., "TestScene", "MainMenu")
- Only one primary development scene at a time

### File Organization
```
Assets/_Project/
├── Scripts/
│   ├── Player/         (ship control, input, camera)
│   ├── StarSystem/     (celestial bodies, rendering, loading)
│   ├── Logic/          (pure math, game rules - NO Unity dependencies)
│   ├── Data/           (JSON serialization structures)
│   ├── UI/             (menus, HUD, mobile controls)
│   └── Editor/         (custom inspectors, tools)
├── Prefabs/            (reusable GameObjects)
├── Scenes/             (Unity scene files)
├── Content/            (game data - NOT in StreamingAssets yet)
├── Tests/
│   └── EditMode/       (unit tests for pure logic)
└── Art/                (sprites, textures, materials)
```

### Data-Driven Design
- All game content loaded from JSON at runtime
- JSON files eventually live in `StreamingAssets/` for builds
- Use `UnityWebRequest` for Android-compatible async loading
- Implement sprite caching to avoid redundant loads
- Flatten hierarchical data structures before rendering

### Component Communication
1. **Direct references**: Use for tightly coupled components on same GameObject
2. **Events**: Use for decoupled systems (e.g., OnSystemLoaded)
3. **Public methods**: Use for explicit API calls (e.g., ApplyExternalForce)
4. **Avoid singletons** unless truly needed (managers, loaders only)

### Mobile-First Optimization
- Object pooling for frequently instantiated objects
- Update UI at fixed intervals (e.g., 10 Hz), not every frame
- Use SpriteRenderer over UI Image for world-space elements
- Async loading with progress callbacks
- Touch input via Unity's Input System (new input system)

## Performance Requirements

### Hard Limits (AI Must Enforce)
- **Frame time**: <33ms (30 FPS minimum on mobile)
- **GC allocations**: <100KB per frame
- **Draw calls**: <50 for 2D game
- **Texture memory**: <500MB on mobile
- **SetPass calls**: <10 per frame
- **Active rigidbodies**: <100 simultaneously

### Performance Red Flags - STOP and Optimize If:
- Frame time exceeds 33ms consistently
- GC allocations >1MB per second
- `FindObjectOfType()` or similar used in Update/FixedUpdate loops
- Heavy computation in Update() method (move to coroutine or job)
- Instantiating objects without pooling
- String concatenation in frequently called methods
- LINQ queries in Update/FixedUpdate

### Optimization Priority Order:
1. **Remove GC allocations** (use object pooling, cache collections)
2. **Cache component references** (in Awake, never in Update)
3. **Throttle UI updates** (10Hz instead of 60Hz)
4. **Use sprite atlasing** (reduce draw calls)
5. **Profile before optimizing** (use Unity Profiler, not guesswork)

## Mobile Implementation Rules

### Required for All Mobile UI:
- **Touch targets**: Minimum 44pt × 44pt (Apple HIG standard)
- **Safe area handling**: Respect device notches and rounded corners
- **Orientation**: Support landscape (primary) + portrait, OR lock explicitly
- **Auto-pause**: Implement `OnApplicationPause(true)` handling
- **Battery optimization**: Disable unused sensors (gyro, location)

### Input Implementation Standards:
- **ALWAYS support** keyboard + gamepad + touch simultaneously
- **Use Unity Input System** (new), NOT `Input.GetKey()`
- **Provide visual feedback** for touch (show button press states)
- **No precision required**: Design for imprecise touch input
- **Swipe gestures**: Minimum distance threshold before triggering

### Mobile-Specific Code Pattern:
```csharp
[Header("Mobile Touch")]
[SerializeField] private Vector2 minTouchTargetSize = new Vector2(44, 44);
[SerializeField] private bool showTouchDebugBounds = false;

private void OnDrawGizmos()
{
    if (showTouchDebugBounds)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, minTouchTargetSize);
    }
}
```

### Testing Requirements
- Write NUnit tests for ALL pure logic classes
- Test edge cases: zero, negative, very large values, boundary conditions
- Test angle wrapping (0°, 90°, 180°, 270°, 360°, -45°)
- Run tests before committing any logic changes
- Tests live in `Assets/_Project/Tests/EditMode/`

## Anti-Patterns (AI Must Avoid)

### ❌ NEVER Do This:
- Use `Mathf.Lerp()` for game feel transitions (use easing functions instead)
- Hardcode tunable gameplay values (always use `[SerializeField]` with `[Range]` and `[Tooltip]`)
- Put physics logic in `Update()` (belongs in `FixedUpdate()`)
- Instantiate objects without pooling for frequently spawned objects
- Use `FindObjectOfType()` or `GetComponent()` in `Update()`/`FixedUpdate()` loops
- Create UI requiring precise touch input (44pt minimum targets)
- Add explicit tutorials or instruction text (game teaches through play)
- Implement features targeting adults (target age is 5-8)
- Allocate memory in frequently called methods (string concat, LINQ, new objects)
- Use linear interpolation for animations that need to feel good

### ✅ ALWAYS Do This:
- Use easing functions for all game feel transitions (`Mathf.Pow(1f - progress, 3f)`)
- Expose gameplay values as `[SerializeField]` with appropriate `[Range]` and `[Tooltip]`
- Separate input handling (`Update`) from physics simulation (`FixedUpdate`)
- Pool frequently instantiated objects (particles, projectiles, UI elements)
- Cache component references in `Awake()`, store in private fields
- Make touch targets generous (44pt × 44pt minimum)
- Design for discovery and implicit learning (no tutorials)
- Keep complexity appropriate for ages 5-8
- Reuse collections and avoid allocations in hot paths
- Profile before optimizing (use Unity Profiler data)

### Code Pattern Examples:

❌ **BAD** (Linear interpolation):
```csharp
transform.position = Vector3.Lerp(start, end, progress);
```

✅ **GOOD** (Eased interpolation):
```csharp
float easedProgress = 1f - Mathf.Pow(1f - progress, 3f); // ease-out cubic
transform.position = Vector3.Lerp(start, end, easedProgress);
```

❌ **BAD** (Hardcoded value):
```csharp
if (distance < 5f) // Magic number!
    LockToOrbit();
```

✅ **GOOD** (Serialized and documented):
```csharp
[Header("Gravity Lock")]
[SerializeField]
[Range(1f, 10f)]
[Tooltip("Distance from planet where gravity lock becomes available")]
private float lockActivationDistance = 5f;

if (distance < lockActivationDistance)
    LockToOrbit();
```

❌ **BAD** (Allocation in Update):
```csharp
void Update()
{
    var nearbyPlanets = FindObjectsOfType<CelestialBodyController>(); // Allocates every frame!
}
```

✅ **GOOD** (Cached reference):
```csharp
private List<CelestialBodyController> _nearbyPlanets = new List<CelestialBodyController>();

void OnTriggerEnter2D(Collider2D other)
{
    if (other.TryGetComponent<CelestialBodyController>(out var planet))
        _nearbyPlanets.Add(planet);
}
```

## Code Quality Checklist

### Code Structure
- [ ] Component follows standard structure order (serialized → private → properties → lifecycle → public → private → callbacks → debug)
- [ ] All serialized fields have sensible defaults
- [ ] Events properly unsubscribed in OnDisable()
- [ ] Physics code only in FixedUpdate()
- [ ] Public API has XML doc comments
- [ ] No hardcoded values (use serialized fields or data files)
- [ ] Null checks before accessing external references

### Game Feel & Polish
- [ ] All player actions have immediate visual feedback (<100ms, ideally 1 frame)
- [ ] Transitions use easing functions (`Mathf.Pow(1f - progress, 3f)`), not linear lerp
- [ ] Tunable values have `[Range]` and `[Tooltip]` attributes
- [ ] Touch targets are ≥44pt × 44pt for mobile
- [ ] Timing values match standards (0.1s for instant, 0.5s for actions, 0.8s for transitions)
- [ ] No features requiring tutorials (game teaches through play)

### Performance
- [ ] No GC allocations in Update/FixedUpdate loops
- [ ] Component references cached in Awake(), not found in Update()
- [ ] Object pooling used for frequently instantiated objects (>10 per session)
- [ ] UI updates throttled (10Hz), not updated every frame
- [ ] No `FindObjectOfType()` or LINQ in hot paths
- [ ] Profiler shows <33ms frame time

### Testing & Validation
- [ ] Tests written for any pure logic classes
- [ ] Edge cases tested (zero, negative, large values, boundaries)
- [ ] No compilation errors in Unity console
- [ ] All tests passing
- [ ] Feature appropriate for target age (5-8 years old)