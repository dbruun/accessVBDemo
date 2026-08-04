# ChompMan Business Requirements

## 1. Purpose

This document describes the business capabilities and user-visible behavior of ChompMan that should be considered when replacing or modernizing the current VB.NET, Windows Forms, and Microsoft Access application.

It is a business requirements baseline, not a mandate to reproduce the current technical architecture. A replacement may use a web, desktop, mobile, or cross-platform client and a different data store as long as the required behavior and data are preserved.

## 2. Business Objective

ChompMan provides a self-contained arcade game in which a player navigates mazes, earns points, progresses through levels, and may record a result on a local leaderboard. Administrators or operators can alter selected game settings and level content without changing the game engine.

A modernization should:

- Preserve the recognizable game rules and short-session arcade experience.
- Preserve existing level, player, score, and configuration data where practical.
- Remove the dependency on Microsoft Access and the ACE OLEDB runtime.
- Make gameplay configuration explicit, validated, and maintainable.
- Support automated testing and future delivery to additional platforms.

## 3. Scope

### In scope for functional parity

- Main menu and game navigation.
- Real-time maze gameplay and keyboard controls.
- Scoring, lives, collisions, ghost behavior, and level progression.
- Data-defined maze layouts and level difficulty.
- Player-name capture and high-score persistence.
- Leaderboard display.
- Gameplay and appearance settings.
- Operation with limited functionality when persistence is unavailable.

### Outside the current product baseline

The existing application does not provide user accounts, cloud synchronization, multiplayer play, saved games, achievements, telemetry, an online level editor, touch controls, or gamepad support. These are modernization opportunities, not parity requirements, unless separately approved.

## 4. Users and Stakeholders

| Role | Need |
|---|---|
| Player | Start and control a game, understand progress, and optionally save and compare a result. |
| Operator | Configure supported game values and manage level content without modifying engine code. |
| Product owner | Preserve the established game behavior while deciding which current ambiguities become supported rules. |
| Support or development team | Deploy, diagnose, test, and evolve the application without machine-specific database components. |

## 5. Core User Journeys

### 5.1 Play a game

1. The player opens the application and sees the main menu.
2. The player starts a new game at level 1 with the configured number of lives.
3. The player moves through the maze, consumes pellets, avoids or eats ghosts, and sees score, level, and lives status.
4. Clearing all pellets completes the level and advances the player when another level exists.
5. Losing all lives ends the run. Completing the final level wins the run.
6. The player may enter a display name and save the result, or skip saving.
7. The player returns to the main menu.

### 5.2 View high scores

1. The player opens High Scores from the main menu.
2. The system displays up to ten results ordered from highest to lowest score.
3. Each result shows rank, player name, score, highest level reached, and play date.

### 5.3 Change settings

1. An operator opens Settings from the main menu.
2. The system displays supported setting names and current values.
3. The operator changes permitted values and saves them.
4. The system validates the values and applies them at a defined lifecycle point, normally the next new game.

## 6. Functional Requirements

Priority uses **Must** for modernization parity, **Should** for behavior needed to make the parity feature production-ready, and **Could** for an optional enhancement.

### 6.1 Navigation and session lifecycle

| ID | Priority | Requirement | Acceptance criteria |
|---|---|---|---|
| BR-NAV-001 | Must | Provide a main menu with New Game, High Scores, Settings, and Exit actions. | Each action opens the corresponding experience; Exit closes the application or session. |
| BR-NAV-002 | Must | Start each new run at level 1. | New Game and Restart both create a new run rather than resuming prior runtime state. |
| BR-NAV-003 | Must | Allow the player to leave an active game and return to the menu. | Escape or an equivalent platform action exits the current run without saving it automatically. |
| BR-NAV-004 | Must | Allow an active game to be paused and resumed. | While paused, entities, timers, and scoring do not advance. |
| BR-NAV-005 | Must | Allow the player to restart during a run. | Restart clears the current run and begins again at level 1 after confirmation if the target experience requires it. |

### 6.2 Gameplay and controls

| ID | Priority | Requirement | Acceptance criteria |
|---|---|---|---|
| BR-GAME-001 | Must | Render a grid-based maze containing walls, corridors, pellets, power pellets, a player start, and one or more ghost starts. | The rendered maze corresponds to the selected level definition and all required spawn points are usable. |
| BR-GAME-002 | Must | Accept four-direction player movement. | Arrow keys and WASD queue movement; movement starts when the requested adjacent cell is passable. Modern clients may add equivalent controls. |
| BR-GAME-003 | Must | Prevent traversal through walls and prevent the player from traversing ghost-house doors. | Invalid movement leaves the player in the current passable cell. |
| BR-GAME-004 | Must | Support horizontal tunnel wrapping where a level exposes a traversable edge. | Exiting one horizontal edge enters the opposite edge on the same row. |
| BR-GAME-005 | Must | Move ghosts independently according to chase, scatter, frightened, and eaten modes. | Ghosts alternate between chase and scatter, move unpredictably when frightened, and return toward spawn when eaten. |
| BR-GAME-006 | Must | Differentiate ghost behavior during chase. | At least one ghost targets the player directly, one anticipates player movement, and remaining ghosts combine pursuit with variation. |
| BR-GAME-007 | Must | Make ghosts temporarily vulnerable after a power pellet is consumed. | Eligible ghosts enter frightened mode for the configured duration, move more slowly, and can be eaten by the player. |
| BR-GAME-008 | Must | Resolve player and ghost collisions. | Contact with an active chase/scatter ghost removes one life; contact with a frightened ghost awards points and sends that ghost home. Eaten ghosts do not harm the player. |
| BR-GAME-009 | Must | Respawn entities after a non-final life is lost. | Player and ghosts return to their level spawn positions after a short delay; consumed pellets and the run score remain unchanged. |
| BR-GAME-010 | Must | End the run when no lives remain. | Gameplay stops and the game-over workflow displays the final score. |
| BR-GAME-011 | Must | Complete a level when no pellets remain. | Gameplay pauses, completion is communicated, and the next defined level starts or the run is declared won. |
| BR-GAME-012 | Must | Display score, current level, and remaining lives throughout play. | Values update no later than the next rendered game frame after state changes. |

### 6.3 Scoring and progression

| ID | Priority | Requirement | Acceptance criteria |
|---|---|---|---|
| BR-SCORE-001 | Must | Award 10 points for a standard pellet and 50 points for a power pellet. | The score increases exactly once when each item is consumed. |
| BR-SCORE-002 | Must | Award escalating points for ghosts eaten during one power-pellet period. | Consecutive ghosts award 200, 400, 800, and 1,600 points; the combination resets on the next power pellet or player respawn. |
| BR-SCORE-003 | Must | Award an extra life at each 10,000-point milestone reached during a run. | Each milestone grants no more than one life, including when one score event crosses a milestone. |
| BR-SCORE-004 | Must | Increase difficulty through level-specific ghost speed. | Lower ticks-per-move values cause more frequent ghost movement. |
| BR-SCORE-005 | Must | Preserve run-level score and remaining lives across level transitions. | Entering the next level retains the score and lives earned in the current run. |
| BR-SCORE-006 | Must | Record the highest level reached when saving a completed run. | The persisted result contains the level active when the run ended or was won. |

### 6.4 Levels and content

| ID | Priority | Requirement | Acceptance criteria |
|---|---|---|---|
| BR-LEVEL-001 | Must | Load level definitions from managed data rather than compiled rendering logic. | A level includes a unique positive level number, maze layout, ghost speed, and pellet count or a derivable equivalent. |
| BR-LEVEL-002 | Must | Support the existing maze symbols. | `#`, `.`, `o`, `P`, `G`, `-`, and space represent wall, pellet, power pellet, player start, ghost start, ghost-house door, and empty corridor respectively. |
| BR-LEVEL-003 | Should | Validate level content before publication or play. | Validation rejects unknown symbols, missing player/ghost starts, duplicate level numbers, invalid dimensions, unreachable required pellets, and pellet-count mismatches. |
| BR-LEVEL-004 | Must | Define a deterministic end to the available level sequence. | When no next level exists, the current run ends in a win; the system must not silently synthesize endless levels unless endless mode is an approved feature. |
| BR-LEVEL-005 | Must | Retain the three seeded level definitions during migration. | Migrated layouts and speeds produce functionally equivalent playable levels. |

### 6.5 Players and high scores

| ID | Priority | Requirement | Acceptance criteria |
|---|---|---|---|
| BR-HS-001 | Must | Offer score submission after a loss or win. | The player sees final score and may save or skip without being blocked from returning to the menu. |
| BR-HS-002 | Must | Require a non-blank display name when saving. | Blank or whitespace-only input is rejected with a clear message. |
| BR-HS-003 | Must | Support display names up to 20 characters for parity with the current user interface. | Names at or below the limit save successfully; longer input is prevented or validated. |
| BR-HS-004 | Must | Reuse the same player record for an exact matching name and allow multiple scores per player. | A returning name does not require a new identity, and each submitted run creates a result. |
| BR-HS-005 | Must | Display the top ten results by descending score. | Rank, display name, formatted score, level reached, and local play date are shown. |
| BR-HS-006 | Should | Define deterministic ranking for tied scores. | An approved secondary ordering, such as earliest achievement, is consistently applied. |
| BR-HS-007 | Should | Validate score submissions at the trusted application boundary. | Invalid names, negative values, impossible levels, and malformed requests are rejected. |

### 6.6 Settings

| ID | Priority | Requirement | Acceptance criteria |
|---|---|---|---|
| BR-SET-001 | Must | Manage the currently seeded settings: starting lives, player movement speed, default ghost speed, frightened duration, and player/wall/pellet/power-pellet colors. | Each supported setting can be viewed and updated by an authorized operator or local user, according to the deployment model. |
| BR-SET-002 | Must | Apply supported gameplay settings to the game engine. | A new run uses configured lives, movement speeds, and frightened duration; level-specific ghost speed takes precedence where defined. |
| BR-SET-003 | Must | Apply supported appearance settings to rendering. | A new game uses the configured colors, with accessible defaults if a value is invalid. |
| BR-SET-004 | Should | Use typed controls and validation instead of unrestricted text values. | Numeric ranges and valid color choices are enforced before save. |
| BR-SET-005 | Should | Communicate when a setting takes effect. | The operator knows whether a change applies immediately, on the next level, or on the next new game. |
| BR-SET-006 | Must | Supply safe defaults if settings cannot be loaded. | Gameplay remains available using documented default values. |

### 6.7 Persistence and degraded operation

| ID | Priority | Requirement | Acceptance criteria |
|---|---|---|---|
| BR-DATA-001 | Must | Persist players, high scores, levels, and settings in a supported data store. | Data survives application restarts and is retrievable through supported application operations. |
| BR-DATA-002 | Must | Initialize an empty deployment with schema, default settings, and baseline levels. | First launch or deployment produces a playable system without manual database editing. Sample scores may be limited to non-production environments. |
| BR-DATA-003 | Must | Store timestamps consistently and present dates in the user's local context. | Saved play times are unambiguous, and leaderboard dates render correctly for the user. |
| BR-DATA-004 | Must | Continue gameplay when score persistence is unavailable. | The player receives a concise availability message; a built-in baseline level and default settings allow play; unsaved scores are not represented as saved. |
| BR-DATA-005 | Should | Avoid partial writes when saving a player and score. | Player lookup/creation and result insertion complete atomically or leave no orphaned partial operation. |
| BR-DATA-006 | Should | Provide schema versioning and repeatable migrations. | Deployments can move between supported schema versions with an auditable migration result. |

## 7. Business Rules

| Rule | Definition |
|---|---|
| Starting lives | Default is 3 unless a valid setting overrides it. |
| Player movement speed | Default is 4 ticks per move. |
| Ghost movement speed | Default is 6 ticks per move; a level-specific value overrides the default. Lower values are faster. |
| Frightened duration | Default is 180 engine ticks, approximately 3 seconds at 60 updates per second. |
| Respawn delay | 120 engine ticks, approximately 2 seconds. |
| Ghost mode cycle | Scatter for 300 ticks, then chase for 600 ticks, repeating while no ghost is frightened. |
| Ranking | Higher score ranks first; tie handling requires a product decision. |
| Player identity | The current model treats an exact matching display name as the same player; it is not an authenticated identity. |
| Level completion | All standard and power pellets must be consumed. |
| Run completion | Zero lives is a loss; clearing the last defined level is a win. |

## 8. Data Requirements and Migration

The current Microsoft Access database contains four business entities:

| Entity | Required data | Migration notes |
|---|---|---|
| Player | Stable identifier, display name, creation timestamp | Preserve identifiers where feasible; define case-sensitivity and duplicate-name handling before import. |
| High score | Player reference, score, level reached, played timestamp | Preserve historical values and UTC meaning; verify row counts and top-ten results after import. |
| Level | Level number, maze layout, ghost speed, pellet count | Recalculate pellet count during validation and report discrepancies rather than silently changing layouts. |
| Setting | Unique key and value | Convert known keys to typed configuration; quarantine unknown keys for review. |

Migration acceptance should include:

- A read-only backup of the source `.accdb` file before transformation.
- Reconciled source and target row counts for every entity.
- A report of rejected, corrected, duplicate, or defaulted records.
- Comparison of the top-ten leaderboard before and after migration.
- Automated parsing and playability validation for every migrated maze.
- Documented rollback or re-run steps.

## 9. Non-Functional Requirements

| ID | Area | Requirement |
|---|---|---|
| NFR-001 | Responsiveness | Player input and rendering should feel continuous at a target of 60 updates or frames per second on supported hardware. |
| NFR-002 | Determinism | Core movement, collision, scoring, and progression rules must be testable independently of the UI and data store. Random ghost choices should support a controllable seed in tests. |
| NFR-003 | Portability | The modernized core must not depend on Windows Forms, COM, ACE OLEDB, or a locally installed Microsoft Office component. |
| NFR-004 | Accessibility | Menus and settings must support keyboard navigation, visible focus, sufficient contrast, and non-color-only status cues. Controls and text must support platform scaling. |
| NFR-005 | Security | All data operations must use parameterized access; input must be validated; database credentials or connection secrets must not be embedded in client code. |
| NFR-006 | Privacy | Store only the display name and gameplay result needed for the leaderboard unless additional collection is approved and disclosed. |
| NFR-007 | Reliability | Persistence failures must not crash an active game. Failures must be logged with enough context for support while user-facing messages avoid implementation details. |
| NFR-008 | Observability | Record application version, startup failures, level-load failures, settings validation failures, and score-save failures using the target platform's diagnostics. Do not collect personal or gameplay telemetry without approval. |
| NFR-009 | Compatibility | The product owner must define supported operating systems, browsers, input methods, display sizes, and offline expectations before implementation approval. |

## 10. Modernization Decisions Required

These points are ambiguous, incomplete, or inconsistent in the current implementation and require an explicit product decision:

1. **Cross-level continuity:** The current UI creates a new player object on every level, resetting score and lives. BR-SCORE-005 assumes the conventional and user-expected rule that one run retains both.
2. **Final-level behavior:** A missing level currently receives a generated fallback based on level 1, so the win path is effectively unreachable. BR-LEVEL-004 requires a finite sequence unless an explicit endless mode is added.
3. **Settings effectiveness:** Settings are currently editable and persisted, but the game uses compiled values for lives, player speed, frightened duration, and colors. The modernization requirements make these settings operational.
4. **Leaderboard ties:** The current query sorts only by score. A stable secondary ordering must be approved.
5. **Player-name matching:** Exact-name reuse is current behavior. Decide whether matching is case-sensitive and whether unauthenticated names remain sufficient for shared or online deployments.
6. **Offline score handling:** The current application lets play continue but discards unsaved results. Decide whether a modern client should queue and retry scores; doing so requires integrity and duplicate-submission rules.
7. **Operator authorization:** A local desktop user can edit all settings. A shared or hosted deployment must define who can change configuration and levels.
8. **Timing model:** Current rules use frame-like ticks. A modernization should decide whether durations remain tick-based for exact parity or become elapsed-time based for consistency across devices.

## 11. Recommended Modernization Capabilities

These capabilities are not required to recreate the current product, but they reduce operational risk and make future enhancement easier:

| Priority | Capability | Business value |
|---|---|---|
| Recommended | Separate the game engine, presentation, and persistence contracts. | Allows UI and database replacement without changing proven game rules. |
| Recommended | Provide a validated level-management workflow with preview. | Prevents malformed content from reaching players and removes direct database editing. |
| Recommended | Version settings and level definitions. | Supports audit, rollback, and reproducible gameplay. |
| Recommended | Add configurable input mapping and gamepad/touch adapters. | Extends platform reach without altering the core rules. |
| Recommended | Add automated migration and parity test suites. | Demonstrates that scoring, movement, collisions, levels, and leaderboard results survive replacement. |
| Optional | Authenticated profiles and cloud leaderboards. | Enables trustworthy cross-device ranking but changes the current lightweight identity model. |
| Optional | Saved progress, achievements, and analytics. | Adds engagement features but requires new privacy, retention, and synchronization requirements. |

## 12. Modernization Acceptance Checklist

A replacement is ready for business acceptance when:

- All **Must** requirements have passing acceptance evidence.
- The baseline levels render and play correctly using migrated definitions.
- Scoring, ghost combinations, extra lives, collisions, pause, restart, level transitions, loss, and win behavior pass automated parity tests.
- Settings visibly affect new gameplay and invalid settings are rejected or safely defaulted.
- Existing production data is migrated and reconciled according to Section 8.
- The top-ten leaderboard matches the approved source ordering and tie rule.
- The game remains playable during a simulated persistence outage.
- Supported platforms pass keyboard, scaling, contrast, and performance checks.
- Microsoft Access, ACE OLEDB, ADOX COM, and machine-local database-file assumptions are absent from the deployed solution.

## 13. Current-State Reference

These requirements were derived from the current implementation and should be reviewed with the product owner before becoming a signed-off specification:

- [`README.md`](../README.md)
- [`Program.vb`](../ChompMan/Program.vb)
- [`GameForm.vb`](../ChompMan/UI/GameForm.vb)
- [`Engine.vb`](../ChompMan/GameEngine/Engine.vb)
- [`Maze.vb`](../ChompMan/GameEngine/Maze.vb)
- [`DatabaseInitializer.vb`](../ChompMan/DataAccess/DatabaseInitializer.vb)
- [`AccessScoreRepository.vb`](../ChompMan/DataAccess/AccessScoreRepository.vb)
- [`AccessLevelRepository.vb`](../ChompMan/DataAccess/AccessLevelRepository.vb)
- [`AccessSettingsRepository.vb`](../ChompMan/DataAccess/AccessSettingsRepository.vb)