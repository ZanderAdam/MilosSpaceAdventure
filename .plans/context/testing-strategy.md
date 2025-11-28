# Testing Strategy
## Milo's Space Adventure

## Overview

**Mandate**: 80% minimum test coverage for all AI-written code
**Rationale**: AI-generated code requires comprehensive testing to ensure correctness
**Timeline**: Tests written alongside implementation (test-first encouraged)

## Testing Philosophy

### Core Principles

1. **AI Code Must Be Tested**: Every AI-written script must have corresponding unit tests
2. **Clean Architecture Enables Testing**: Write code with testability in mind from the start
3. **Test Business Logic**: Focus on game logic, algorithms, and state management
4. **Accept Testing Limitations**: Phase 1 Singletons limit testability - refactor in Phase 2
5. **Test-Driven Development**: Write tests before or alongside implementation

### What to Test

**HIGH PRIORITY** (Must test):
- Business logic (math difficulty calculation, orbit mechanics, save/load logic)
- Data serialization (JSON parsing, SaveData)
- Game rules (scanning mechanics, stuffy rescue conditions)
- Algorithms (difficulty progression, orbit calculations)

**MEDIUM PRIORITY** (Should test):
- Controllers (PlayerShipController movement logic)
- Managers (StarSystemLoader, SaveManager - where possible)
- UI logic (puzzle validation, input handling)

**LOW PRIORITY** (Can skip for Phase 1):
- MonoBehaviour lifecycle methods (Unity handles this)
- Simple getters/setters
- Unity-specific rendering code
- Animation triggers

## Test Structure

```
Assets/
└── Tests/
    ├── EditMode/                          # Non-Unity tests (pure C#)
    │   ├── Data/
    │   │   ├── JsonSerializationTests.cs  # Test JSON parsing
    │   │   └── SaveDataTests.cs           # Test save data structure
    │   ├── Logic/
    │   │   ├── DifficultyCalculatorTests.cs      # Test math difficulty
    │   │   ├── OrbitCalculatorTests.cs           # Test orbit math
    │   │   └── ScanningLogicTests.cs             # Test scanning rules
    │   └── Utilities/
    │       └── ExtensionMethodTests.cs
    │
    └── PlayMode/                          # Unity-dependent tests
        ├── PlayerShipControllerTests.cs   # Test ship movement
        ├── CelestialBodyTests.cs          # Test planet behavior
        ├── StarSystemLoaderTests.cs       # Test async loading
        └── Integration/
            └── Phase1IntegrationTests.cs  # End-to-end tests
```

## Phase-Specific Coverage Requirements

### Phase 1: Flying Ship + Star System Import

**Minimum 50% coverage required**:
- PlayerShipController: 70%+ (core gameplay)
- StarSystemLoader: 80%+ (critical path)
- SaveManager: 80%+ (data integrity)
- JSON data classes: 90%+ (serialization critical)
- Orbit calculations: 100% (pure logic, easy to test)

**Total Target**: 60%+ overall coverage
**Notes**: Limited by Singleton pattern - logic embedded in MonoBehaviours

### Phase 1.5: Logic Extraction Refactoring (3 days)

**Refactoring Sprint - Logic Extraction**:
- Extract ShipMovementLogic from PlayerShipController
- Extract DifficultyCalculator (pure logic)
- Extract OrbitCalculator (pure logic)
- Add comprehensive unit tests for extracted logic

**New Tests**:
- ShipMovementLogic: 100% (pure C#, no Unity dependencies)
- DifficultyCalculator: 100% (pure logic)
- OrbitCalculator: 100% (pure math)

**Total Target**: 60%+ overall coverage (achievable after extraction)
**Notes**: Logic extraction makes previously untestable code fully testable

### Phase 2: Planet Interaction + Math Minigame

**Add coverage for**:
- MathPuzzle logic: 90%+
- Difficulty calculator: 100%
- Galaxy-center progression: 100%

**Total Target**: 70%+ overall coverage (before Phase 2.5)
**Notes**: Still limited by Singleton pattern in managers

### Phase 2.5: DI + Interfaces + EventBus Refactoring (1 week)

**Refactoring Sprint - Dependency Injection**:
- Create interfaces (ISaveService, IAudioService, IStuffyService)
- Implement constructor injection
- Introduce EventBus for decoupled communication
- Refactor managers to use DI

**New Tests**:
- Service mocks: Full coverage via interfaces
- EventBus: 100% (pure logic)
- Refactored managers: 85%+ (now testable via DI)

**Total Target**: 70%+ overall coverage (achievable after DI)
**Notes**: Dependency injection enables testing of previously untestable Singleton managers

### Phase 3: Password Puzzle + First Stuffy

**Add coverage for**:
- Fill-in-blank puzzle logic: 90%+
- Spelling validation: 100%
- StuffyManager: 80%+

**Total Target**: 75%+ overall coverage

### Phase 4: All Stuffies + Abilities

**Add coverage for**:
- Ability system: 85%+
- Multi-system management: 80%+

**Total Target**: 78%+ overall coverage (before Phase 4.5)

### Phase 4.5: Event-Driven Ability System (1 week)

**Refactoring Sprint - Event-Driven Abilities**:
- Refactor stuffy abilities to use event-driven pattern
- Create Ability Registry with O(1) caching
- Decouple abilities from core systems

**New Tests**:
- Event-driven abilities: 90%+
- Ability Registry: 100% (pure logic)
- Ability integration tests: 80%+

**Total Target**: 80%+ overall coverage (achievable after event-driven refactoring)
**Notes**: Event-driven pattern enables isolated testing of abilities

### Phase 5: Progression + Game Loop

**Add coverage for**:
- Progression logic: 90%+
- Game state management: 85%+

**Total Target**: 80%+ overall coverage

### Phase 6: Polish + Mobile

**Add coverage for**:
- Mobile input handling: 75%+
- Performance optimizations: 70%+
- All game rules: 90%+

**Total Target**: 80%+ overall coverage (FINAL)
**Notes**: Final target achieved through incremental refactoring in Phases 1.5, 2.5, 4.5

## Example Tests

### Edit Mode Test (Pure Logic)

```csharp
using NUnit.Framework;

namespace MilosAdventure.Tests.EditMode
{
    [TestFixture]
    public class DifficultyCalculatorTests
    {
        [Test]
        public void CalculateMathDifficulty_AtGalaxyEdge_ReturnsEasy()
        {
            // Arrange
            float distanceFromCenter = 0.0f; // At edge
            var calculator = new DifficultyCalculator();

            // Act
            int difficulty = calculator.CalculateMathDifficulty(distanceFromCenter);

            // Assert
            Assert.AreEqual(1, difficulty, "Difficulty should be 1 (easiest) at galaxy edge");
        }

        [Test]
        public void CalculateMathDifficulty_AtGalaxyCenter_ReturnsHard()
        {
            // Arrange
            float distanceFromCenter = 1.0f; // At center
            var calculator = new DifficultyCalculator();

            // Act
            int difficulty = calculator.CalculateMathDifficulty(distanceFromCenter);

            // Assert
            Assert.AreEqual(5, difficulty, "Difficulty should be 5 (hardest) at galaxy center");
        }

        [Test]
        [TestCase(0.25f, 2)]
        [TestCase(0.50f, 3)]
        [TestCase(0.75f, 4)]
        public void CalculateMathDifficulty_VariousDistances_ReturnsCorrectDifficulty(
            float distance, int expectedDifficulty)
        {
            // Arrange
            var calculator = new DifficultyCalculator();

            // Act
            int difficulty = calculator.CalculateMathDifficulty(distance);

            // Assert
            Assert.AreEqual(expectedDifficulty, difficulty);
        }
    }
}
```

### Play Mode Test (Unity Integration)

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace MilosAdventure.Tests.PlayMode
{
    [TestFixture]
    public class PlayerShipControllerTests
    {
        private GameObject _shipObject;
        private PlayerShipController _controller;

        [SetUp]
        public void SetUp()
        {
            _shipObject = new GameObject("TestShip");
            _controller = _shipObject.AddComponent<PlayerShipController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_shipObject);
        }

        [UnityTest]
        public IEnumerator ApplyThrust_IncreasesVelocity()
        {
            // Arrange
            Vector2 initialVelocity = _controller.Velocity;

            // Act
            _controller.ApplyThrust(Vector2.up, 1.0f);
            yield return null; // Wait one frame

            // Assert
            Assert.Greater(_controller.Velocity.magnitude, initialVelocity.magnitude,
                "Velocity should increase after applying thrust");
        }

        [Test]
        public void ClampSpeed_ExceedingMaxSpeed_ClampsToMax()
        {
            // Arrange
            float maxSpeed = 10f;
            _controller.maxSpeed = maxSpeed;

            // Act
            _controller.SetVelocity(new Vector2(15f, 15f)); // Exceeds max
            _controller.ClampSpeed();

            // Assert
            Assert.LessOrEqual(_controller.Velocity.magnitude, maxSpeed,
                "Velocity should be clamped to max speed");
        }
    }
}
```

### JSON Serialization Test

```csharp
using NUnit.Framework;
using UnityEngine;
using MilosAdventure.Data;

namespace MilosAdventure.Tests.EditMode
{
    [TestFixture]
    public class JsonSerializationTests
    {
        [Test]
        public void DeserializeStarSystem_ValidJson_ReturnsCorrectData()
        {
            // Arrange
            string json = @"{
                ""system"": {
                    ""id"": ""test"",
                    ""name"": ""Test System"",
                    ""bounds"": { ""width"": 1500, ""height"": 1500 },
                    ""distanceFromGalaxyCenter"": 0.5
                },
                ""rootBodies"": []
            }";

            // Act
            var system = JsonUtility.FromJson<StarSystemJson>(json);

            // Assert
            Assert.NotNull(system);
            Assert.AreEqual("test", system.system.id);
            Assert.AreEqual("Test System", system.system.name);
            Assert.AreEqual(1500, system.system.bounds.width);
            Assert.AreEqual(0.5f, system.system.distanceFromGalaxyCenter);
        }

        [Test]
        public void DeserializeCelestialBody_WithChildren_ParsesHierarchy()
        {
            // Arrange
            string json = @"{
                ""id"": ""earth"",
                ""name"": ""Earth"",
                ""children"": [
                    { ""id"": ""moon"", ""name"": ""Luna"", ""children"": [] }
                ]
            }";

            // Act
            var body = JsonUtility.FromJson<CelestialBodyJson>(json);

            // Assert
            Assert.NotNull(body);
            Assert.AreEqual("earth", body.id);
            Assert.AreEqual(1, body.children.Length);
            Assert.AreEqual("moon", body.children[0].id);
        }
    }
}
```

## Clean Architecture for Testability

### Pattern: Separate Logic from MonoBehaviours

**INSTEAD OF** (Untestable):
```csharp
public class PlayerShipController : MonoBehaviour
{
    private void FixedUpdate()
    {
        // All logic mixed with Unity lifecycle
        float input = Input.GetAxis("Horizontal");
        transform.Rotate(0, 0, input * 180 * Time.fixedDeltaTime);
        // More logic...
    }
}
```

**DO THIS** (Testable):
```csharp
// Pure logic class (easily testable)
public class ShipMovementLogic
{
    public Vector2 CalculateVelocity(Vector2 currentVelocity, Vector2 thrust,
        float deltaTime, float maxSpeed, float drag)
    {
        Vector2 newVelocity = currentVelocity + thrust * deltaTime;
        if (newVelocity.magnitude > maxSpeed)
            newVelocity = newVelocity.normalized * maxSpeed;

        if (thrust == Vector2.zero)
            newVelocity *= (1f - drag * deltaTime);

        return newVelocity;
    }
}

// MonoBehaviour (thin Unity wrapper)
public class PlayerShipController : MonoBehaviour
{
    private ShipMovementLogic _logic = new ShipMovementLogic();
    private Vector2 _velocity;

    private void FixedUpdate()
    {
        Vector2 thrust = GetThrustInput();
        _velocity = _logic.CalculateVelocity(_velocity, thrust,
            Time.fixedDeltaTime, maxSpeed, drag);
        transform.position += (Vector3)(_velocity * Time.fixedDeltaTime);
    }
}
```

## Testing Tools

### Unity Test Framework

- **Package**: com.unity.test-framework (already installed)
- **Runner**: Window → General → Test Runner
- **Documentation**: https://docs.unity3d.com/Packages/com.unity.test-framework@latest

### NUnit

- **Version**: 3.x (comes with Unity Test Framework)
- **Attributes**: `[Test]`, `[TestFixture]`, `[SetUp]`, `[TearDown]`, `[TestCase]`
- **Assertions**: `Assert.AreEqual`, `Assert.NotNull`, `Assert.Greater`, etc.

### Coverage Tools

- **Unity Package**: Code Coverage (com.unity.testtools.codecoverage)
- **Installation**: Window → Package Manager → Unity Registry → Code Coverage
- **Usage**: Test Runner → Enable Code Coverage → Run Tests
- **Reports**: Generate HTML reports showing line coverage

## Running Tests

### Via Test Runner (IDE)
1. Window → General → Test Runner
2. Select EditMode or PlayMode
3. Click "Run All" or select specific tests
4. View results in Test Runner window

### Via Command Line (CI/CD)
```bash
# Run all tests
/Applications/Unity/Hub/Editor/2022.3.XX/Unity.app/Contents/MacOS/Unity \
  -runTests \
  -batchmode \
  -projectPath . \
  -testResults test-results.xml \
  -testPlatform EditMode

# With code coverage
/Applications/Unity/Hub/Editor/2022.3.XX/Unity.app/Contents/MacOS/Unity \
  -runTests \
  -batchmode \
  -projectPath . \
  -testResults test-results.xml \
  -testPlatform EditMode \
  -enableCodeCoverage \
  -coverageResultsPath coverage-results
```

## Coverage Tracking

### Per-Phase Goals

| Phase | Coverage Target | Critical Areas |
|-------|----------------|----------------|
| 1     | 60%           | Ship, Loading, Save |
| 2     | 70%           | + Math puzzles |
| 3     | 75%           | + Password puzzles |
| 4     | 78%           | + Abilities |
| 5     | 80%           | + Progression |
| 6     | 80%+          | All systems |

### Measuring Coverage

1. Install Code Coverage package
2. Enable coverage in Test Runner
3. Run all tests
4. View coverage report: `coverage-results/index.html`
5. Focus on files with <80% coverage
6. Add tests for uncovered lines

## Phase 2 Refactoring Plan

### When to Refactor

After Phase 1 playable prototype is validated:
1. Extract testable interfaces from Singletons
2. Implement dependency injection where beneficial
3. Increase test coverage from 60% → 80%
4. Add integration tests for full gameplay loops

### Refactoring Priorities

**HIGH**:
- StarSystemLoader (make fully mockable)
- SaveManager (critical data integrity)
- Math difficulty calculator (pure logic)

**MEDIUM**:
- PlayerShipController (separate input from logic)
- Stuffy rescue logic (business rules)

**LOW**:
- Audio/visual effects (low risk)

## Best Practices

1. **Test Names**: Use descriptive names (MethodName_Scenario_ExpectedResult)
2. **AAA Pattern**: Arrange, Act, Assert
3. **One Assert**: Prefer one assertion per test (exceptions allowed)
4. **Fast Tests**: Edit mode tests should run in <1s total
5. **Isolated**: Tests should not depend on each other
6. **Readable**: Tests are documentation - make them clear
7. **No Logic in Tests**: Tests should be simple and obvious

## Common Pitfalls

❌ **Don't**: Test Unity lifecycle methods directly
✅ **Do**: Extract logic and test that

❌ **Don't**: Test private methods
✅ **Do**: Test public API, private methods are implementation details

❌ **Don't**: Create complex test setup
✅ **Do**: Use helper methods and factories

❌ **Don't**: Ignore failing tests
✅ **Do**: Fix or remove failing tests immediately

❌ **Don't**: Test framework code (Unity, .NET)
✅ **Do**: Test your code that uses frameworks

## Summary

**Phase 1 Testing Goals**:
- ✅ 60%+ overall code coverage
- ✅ All critical paths tested (loading, saving, core gameplay)
- ✅ Clean separation of logic from MonoBehaviours where possible
- ✅ Foundation for 80% coverage in Phase 2

**Remember**: Tests are insurance. AI-written code needs tests to verify correctness. Write tests as you code, not after.
