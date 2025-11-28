# Milo's Space Adventure
## Simplified Project Plan

**Platforms:** PC + Mobile  
**Philosophy:** Playable first, iterate often, no fake timelines

---

## Overview

| Phase | Focus | Playable? |
|-------|-------|-----------|
| 1 | Flying Ship + JSON Star System Import | ✅ Fly in imported system |
| 2 | Planet Interaction + Math Minigame | ✅ Scan planets |
| 3 | Password Puzzle + First Stuffy | ✅ Rescue a stuffy |
| 4 | All Stuffies + Abilities | ✅ Full collection |
| 5 | Progression + Game Loop | ✅ Complete game |
| 6 | Polish + Mobile | ✅ Shippable |

---

## Phase 1: Flying Ship + Star System Import

**Goal:** Fly Milo's ship around a star system your kid designed

### What to Build

1. **Ship Controls (Top-Down Arcade)**
   - W = thrust forward
   - A/D = rotate
   - S = brake
   - Gamepad support
   - Touch controls (virtual joystick for mobile)

2. **JSON Star System Import** ⭐ CRITICAL
   - Load star system from JSON file
   - Load planet/star sprites from folder
   - Render system with orbiting planets
   - Editor tool: "Milo → Import Star System JSON"

3. **Basic Scene**
   - Camera follows ship
   - Ship can fly around the imported star system
   - Planets visible and orbiting

### Deliverables
- [ ] Ship flies with W/A/S/D + gamepad
- [ ] JSON importer loads star system data
- [ ] Sprites load from designated folder
- [ ] Planets render and orbit the star
- [ ] Ship can fly between planets
- [ ] Camera follows ship smoothly
- [ ] **Unit tests: 50% coverage minimum** (StarSystemLoader, SaveManager, JSON parsing)
- [ ] **All tests pass** before completing phase

**Testing**: See `.plans/context/testing-strategy.md` - Phase 1 target: 60% coverage

### Content Pipeline
```
Your Kid's Workflow:
1. Design star system in external tool
2. Export → sol_system.json + sprite PNGs
3. Place in Assets/_Project/Content/StarSystems/Sol/
4. Unity Editor: Milo → Import Star System JSON
5. Play and fly around!
```

### Phase 1 Enhancements (Based on Review)

**Gravity Lock Mechanic:**
- Ship locks into planet gravity when within 2x visual radius
- Smooth attraction force pulls ship toward planet
- Follows planet's orbit automatically
- Visual indicator shows locked planet
- **Purpose:** Help ages 5-8 target moving planets

**Autosave System:**
- Save after every planet scanned
- Resume from last save on app reopen
- Persistent across sessions
- **Purpose:** Flexible stop/resume for mobile sessions (15-30 min play sessions)

**Object Pooling:**
- Generic ObjectPool<T> for particles and UI elements
- Pre-populated pools to prevent GC spikes
- **Purpose:** Prevent mobile frame drops from garbage collection

**Sprite Atlas:**
- CelestialsAtlas.spriteatlas for all celestial sprites
- Reduces draw calls from 100+ to ~5
- **Purpose:** Achieve 30 FPS target on mobile

---

## Phase 2: Planet Interaction + Math Minigame

**Goal:** Click planets, solve math puzzles to "scan" them

### What to Build

1. **Planet Clicking**
   - Click/tap a planet to select it
   - Ship travels to planet
   - UI shows planet info

2. **Math Scanner Minigame**
   - Problem display (e.g., "7 + 5 = ?")
   - Number buttons for input (touch-friendly)
   - 3 problems to complete a scan
   - Correct = progress, Wrong = hint (no punishment)
   - Difficulty based on planet distance from star

3. **Planet States**
   - Unscanned (gray/locked)
   - Scanned (revealed)
   - Has stuffy signal (glowing)

### Deliverables
- [ ] Click planet → ship flies to it
- [ ] Math puzzle UI appears
- [ ] Solve 3 problems → planet scanned
- [ ] Visual feedback: unscanned vs scanned planets
- [ ] Difficulty scales with planet position
- [ ] **Unit tests: Math difficulty calculator** (100% coverage - pure logic)
- [ ] **Tests for galaxy-center difficulty scaling**
- [ ] **All tests pass** before completing phase

**Testing**: See `.plans/context/testing-strategy.md` - Phase 2 target: 70% overall coverage

---

## Phase 3: Password Puzzle + First Stuffy Rescue

**Goal:** Find and rescue Earthy (first stuffy)

### What to Build

1. **Stuffy Signal Detection**
   - Scanned planets may have "stuffy signal"
   - Visual indicator (glow, icon)
   - Click to investigate

2. **Password Puzzle (Kid-Friendly Hangman)**
   - Display: "_ _ _ _ _" (hidden word/phrase)
   - Letter buttons A-Z
   - Themed hints available
   - Complete phrase → rescue stuffy!

3. **Stuffy Rescue**
   - Simple celebration animation
   - Stuffy added to collection
   - Unlock message: "Earthy joined! You can now..."

4. **Stuffy Album**
   - View collected stuffies
   - See their abilities
   - Simple grid UI

### Deliverables
- [ ] Stuffy signals appear on planets
- [ ] Password puzzle UI works
- [ ] Hints help without penalty
- [ ] Rescue celebration feels good
- [ ] Stuffy Album shows collection
- [ ] **Unit tests: Fill-in-the-blank puzzle logic** (90% coverage)
- [ ] **Tests for stuffy rescue mechanics**
- [ ] **All tests pass** before completing phase

**Testing**: See `.plans/context/testing-strategy.md` - Phase 3 target: 75% overall coverage

---

## Phase 4: All Stuffies + Abilities

**Goal:** All 7 stuffies rescuable with their abilities

### Stuffies & Simplified Abilities

| Stuffy | Location | Ability |
|--------|----------|---------|
| Earthy | Sol - Earth | Scanning range increased 2x (can scan from farther away) |
| Starlet | Sol - Near Sun | Shows planet types on minimap (rocky/gas/ice icons) |
| Veenee | System 2 | Ship moves faster (speed boost) |
| Plushy Comet | System 2 | Shows optimal path between planets |
| Moonlings | System 3 | Extra hints in password puzzles |
| Black Hole Stuffy | System 3 | Unlocks path to galaxy center |
| Threadbare | Galaxy Center | Final boss / ending trigger |

**Note:** Asteroid mazes and platform puzzles are CUT. Abilities are passive buffs or simple unlocks.

### What to Build

1. **Ability System (Simple)**
   - Each stuffy grants a passive ability
   - Abilities stored in save data
   - Check abilities when relevant (e.g., "if has Starlet, show hidden planets")

2. **Multiple Star Systems**
   - Import 3-4 systems via JSON
   - Galaxy "map" = simple menu to pick system
   - Some systems locked until ability unlocked

3. **Difficulty Scaling**
   - Systems closer to center = harder math
   - Password phrases get longer
   - More stuffies = more abilities to help

### Deliverables
- [ ] All 7 stuffies placed in systems
- [ ] Each ability works
- [ ] 3-4 star systems imported and playable
- [ ] System selection menu
- [ ] Progression: rescue stuffies → unlock systems
- [ ] **Unit tests: Ability system** (85% coverage)
- [ ] **Tests for progression logic** (90% coverage)
- [ ] **All tests pass** before completing phase

**Testing**: See `.plans/context/testing-strategy.md` - Phase 4 target: 78% overall coverage

### Phase 4+ Future Enhancements

**Scanning Variety:**
- Phase 4: Add memory games (remember planet colors)
- Phase 4: Add pattern matching (orbit speeds)
- Phase 5: Add space fact quizzes
- Phase 5: Add constellation identification

**Rationale:** Keep Phase 1-3 simple (math-only scanning), add variety later when core loop proven. This addresses potential repetition from 75+ planets (225+ math problems) while maintaining focus on core mechanics first.

---

## Phase 5: Progression + Complete Game Loop

**Goal:** Full playable game from start to finish

### What to Build

1. **Game Flow**
   ```
   Boot → Main Menu → Select System → Gameplay → Return to Menu
   ```

2. **Main Menu**
   - New Game
   - Continue
   - Stuffy Album
   - Settings (volume, controls)
   - Quit

3. **Save System**
   - Auto-save after each rescue
   - Save: position, scanned planets, rescued stuffies, unlocked systems
   - Load on Continue

4. **Simple Story Beats**
   - Intro: Text + images (Milo meets Earthy)
   - Ending: Text + images (Fluffaverse restored!)
   - No cutscenes, just illustrated text screens

5. **Threadbare "Boss"**
   - Final system at galaxy center
   - Harder math (multiplication)
   - Longer password phrase
   - Rescue Threadbare → ending

6. **Endgame**
   - "You rescued all stuffies!"
   - Free play continues
   - All systems remain accessible

### Deliverables
- [ ] Main menu works
- [ ] New Game starts fresh
- [ ] Continue loads progress
- [ ] Save/Load works reliably
- [ ] Intro text/images
- [ ] Ending text/images
- [ ] Full game completable start to finish
- [ ] **Unit tests: Game progression logic** (90% coverage)
- [ ] **Integration tests: Complete game loop**
- [ ] **All tests pass** before completing phase

**Testing**: See `.plans/context/testing-strategy.md` - Phase 5 target: 80% overall coverage

---

## Phase 6: Polish + Mobile

**Goal:** Feels good, runs on mobile

### What to Build

1. **Touch Controls**
   - Virtual joystick for ship movement
   - Big tap targets for planets
   - Touch-friendly number/letter buttons

2. **Audio**
   - Background music (gentle space ambience)
   - SFX: thrust, button clicks, correct/wrong, rescue fanfare
   - No voice acting

3. **Visual Polish**
   - Thruster particles
   - Planet scan effect
   - Rescue celebration particles
   - UI transitions (simple fades)

4. **Mobile Optimization**
   - Test on actual devices
   - Sprite atlases for performance
   - Adjust UI for different screen sizes

5. **Bug Fixes & Testing**
   - Playtest with your kid!
   - Fix issues found
   - Balance difficulty

### Deliverables
- [ ] Touch controls work well
- [ ] Music and SFX in place
- [ ] Visual effects feel polished
- [ ] Runs smoothly on mobile
- [ ] No major bugs
- [ ] Kid-tested and approved!
- [ ] **Final test suite: 80%+ coverage**
- [ ] **Performance tests pass** (60 FPS PC, 30 FPS mobile)
- [ ] **All integration tests pass**
- [ ] **Zero test failures**

**Testing**: See `.plans/context/testing-strategy.md` - Phase 6 target: 80%+ overall coverage (FINAL)

---

## What's NOT in v1 (Saved for Later)

| Feature | Why Cut |
|---------|---------|
| Asteroid maze minigame | Whole new gameplay system |
| Platform puzzles | Different controls, physics |
| Wormhole fast-travel | Extra complexity |
| Procedural galaxy generation | Hand-crafted is enough |
| Adaptive AI difficulty | Fixed curve is simpler |
| Voice narration | Cost and complexity |
| Ship customization | Nice-to-have, not core |
| Daily challenges | Post-launch feature |
| Photo mode | Post-launch feature |

---

## Content Checklist

### Star Systems to Create (JSON + Sprites)
- [ ] Sol (Solar System) - Tutorial, Earthy + Starlet
- [ ] System 2 (name TBD) - Veenee + Plushy Comet
- [ ] System 3 (name TBD) - Moonlings + Black Hole Stuffy
- [ ] Galaxy Center - Threadbare (final)

### Stuffies to Design (Sprites + Data)
- [ ] Earthy - fuzzy blue-green planet
- [ ] Starlet - tiny yellow star, sparkly
- [ ] Veenee - pink-orange fast planet
- [ ] Plushy Comet - comet with sparkly tail
- [ ] Moonlings - small bouncy moons
- [ ] Black Hole Stuffy - friendly dark sphere
- [ ] Threadbare - frayed villain (reforms to friend)

### Math Problems to Write
- [ ] Easy: Addition up to 10
- [ ] Medium: Addition/Subtraction up to 20
- [ ] Hard: Multiplication tables (2, 5, 10)
- [ ] Final: Mixed operations

### Password Phrases to Write
- [ ] Earthy themed phrases
- [ ] Starlet themed phrases
- [ ] etc. for each stuffy
- [ ] Final long phrase for Threadbare

---

## File Structure

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
│   │   │   ├── StuffyData.cs
│   │   │   └── StuffyAlbum.cs
│   │   ├── UI/
│   │   │   ├── MainMenu.cs
│   │   │   └── GameHUD.cs
│   │   └── Save/
│   │       ├── SaveData.cs
│   │       └── SaveManager.cs
│   │
│   ├── Content/
│   │   └── StarSystems/
│   │       ├── Sol/
│   │       │   ├── sol_system.json
│   │       │   ├── Sun.png
│   │       │   ├── Mercury.png
│   │       │   └── ...
│   │       ├── System2/
│   │       └── System3/
│   │
│   ├── Prefabs/
│   ├── Art/
│   └── Audio/
│
└── StreamingAssets/
    └── (for runtime JSON loading if needed)
```

---

## Timeline

**Total Duration:** 26 weeks (includes refactoring sprints)

### Phase Breakdown:
- **Phase 1:** Flying + System Import (4 weeks)
- **Phase 1.5:** Logic Extraction Refactoring (3 days)
- **Phase 2:** Planet Interaction + Math (4 weeks)
- **Phase 2.5:** DI + Interfaces Refactoring (1 week)
- **Phase 3:** Password Puzzle + First Stuffy (3 weeks)
- **Phase 4:** All Stuffies + Abilities (5 weeks)
- **Phase 4.5:** Event-Driven Ability System (1 week)
- **Phase 5:** Progression + Game Loop (4 weeks)
- **Phase 6:** Polish + Mobile (3 weeks)

### Refactoring Sprints

**Phase 1.5 (3 days):** Extract logic from MonoBehaviours
- Create ShipMovementLogic, DifficultyCalculator, OrbitCalculator
- Update MonoBehaviours to use extracted logic
- Achieve 60%+ test coverage

**Phase 2.5 (1 week):** Introduce DI + Interfaces + EventBus
- Create interfaces (ISaveService, IAudioService, etc.)
- Implement dependency injection (constructor injection)
- Introduce EventBus for decoupled communication
- Achieve 70%+ test coverage

**Phase 4.5 (1 week):** Event-Driven Ability System
- Refactor stuffy abilities to use event-driven pattern
- Create Ability Registry with O(1) caching
- Achieve 80%+ test coverage

**Investment Justification:** 2.5 weeks upfront investment prevents 6+ weeks of emergency refactoring in Phase 5-6. Planned refactoring is more efficient than reactive refactoring when architecture breaks down.

---

## Summary

**Core Loop:**
1. Fly to planet
2. Solve math to scan
3. Find stuffy signal
4. Solve password to rescue
5. Get ability, unlock more
6. Repeat until Threadbare

**What Makes It Fun:**
- Your kid's custom star systems!
- Cute stuffie collection
- Math that helps, not punishes
- Simple but satisfying progression

**What Keeps It Simple:**
- No complex abilities (just passive buffs)
- No extra minigame types
- Hand-crafted content (no procedural generation)
- KISS story (intro → play → ending)

---

*Ship it when it's fun, not when it's "done"* 🚀