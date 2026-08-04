# ChompMan Business and Modernization Requirements

## 1. Document Purpose

This document defines the business outcomes, product behavior, and modernization constraints for ChompMan. It is intended to be the primary requirements baseline for maintainers and future implementation agents.

Requirement keywords **MUST**, **SHOULD**, and **MAY** indicate mandatory, recommended, and optional behavior. Requirement IDs are stable references for plans, commits, tests, and architecture decisions.

## 2. Product Context

ChompMan is a single-player maze arcade game. The current product is a Windows-only VB.NET WinForms application targeting .NET Framework 4.8. It renders with GDI+, stores players, scores, levels, and settings in a local Microsoft Access database, and can continue without persistence when the database provider is unavailable.

The modernization program must preserve the recognizable game and its data while removing avoidable dependencies on WinForms, .NET Framework, Microsoft Access, ACE OLEDB, and Windows-only deployment. Modernization is an incremental product evolution, not a game redesign.

## 3. Business Goals

| ID | Goal | Success measure |
|---|---|---|
| BG-01 | Preserve the playable ChompMan experience during modernization. | Existing gameplay rules pass automated regression tests before and after each migration stage. |
| BG-02 | Reduce installation and runtime prerequisites. | A player can launch the modernized product without installing Microsoft Access or ACE OLEDB. |
| BG-03 | Expand platform and deployment options. | The core application runs on a supported modern .NET runtime and is not coupled to Windows APIs. |
| BG-04 | Protect existing player, score, level, and setting data. | Migration reconciles source and destination record counts and reports rejected records without silently dropping data. |
| BG-05 | Make future changes safer and faster. | Business logic, presentation, and infrastructure can be tested and replaced independently. |
| BG-06 | Provide enough specification for agent-assisted delivery. | Every implementation task cites requirement IDs and includes automated evidence for its acceptance criteria. |

## 4. Stakeholders

- **Players:** want responsive, predictable gameplay and durable scores.
- **Product owner:** prioritizes feature continuity, broader reach, and low operating cost.
- **Maintainers:** need clear boundaries, diagnostics, tests, and supported dependencies.
- **Content administrators:** need a safe way to manage levels and gameplay settings.
- **Migration operators:** need repeatable migration, validation, rollback, and audit output.

## 5. Scope

### 5.1 In Scope

- Existing game loop, movement, collision, scoring, lives, levels, pause, restart, and game-over behavior.
- Main menu, game, high-score, game-over, and settings workflows.
- Players, high scores, level definitions, and settings data.
- Migration from .NET Framework/WinForms/Access to supported, replaceable technologies.
- Automated tests, observability, accessibility, security, packaging, and migration tooling.

### 5.2 Out of Scope Unless Separately Approved

- Multiplayer, social networking, chat, advertising, purchases, and achievements.
- Material changes to scoring values or game rules.
- Use of copyrighted Pac-Man names, artwork, audio, maps, or other assets.
- Mandatory cloud accounts or always-online play.
- A microservice architecture solely for technology modernization.

## 6. Business Requirements

| ID | Requirement | Priority |
|---|---|---|
| BR-01 | The product MUST remain playable as a single-player maze arcade game throughout the modernization program. | Must |
| BR-02 | The product MUST preserve the player's ability to start a game, view scores, configure supported settings, and exit or return to the menu. | Must |
| BR-03 | The product MUST preserve established scoring, movement, collision, life, level-completion, and game-over rules unless a separately approved requirement changes them. | Must |
| BR-04 | The product MUST preserve valid legacy players, high scores, levels, and settings through a verified data migration path. | Must |
| BR-05 | Gameplay MUST remain available when score persistence is temporarily unavailable; the player MUST receive a clear status without losing control of the game. | Must |
| BR-06 | The modernized product MUST not require Microsoft Access, ACE OLEDB, COM activation, or .NET Framework on a player's machine. | Must |
| BR-07 | The modernized product MUST use a currently supported .NET release at the time its migration stage enters production. | Must |
| BR-08 | The game engine MUST remain independent of UI and persistence technologies so either can be replaced without rewriting game rules. | Must |
| BR-09 | Deployment MUST be repeatable from source and MUST support a clean installation and upgrade path. | Must |
| BR-10 | Modernization MUST be delivered in reversible, independently testable stages rather than as an unverified full rewrite. | Must |
| BR-11 | The product SHOULD support Windows, macOS, and Linux where the selected presentation technology makes this practical. Any platform exclusion requires a documented decision. | Should |
| BR-12 | The architecture SHOULD permit a future web or service-backed experience without requiring that complexity for local play. | Should |

## 7. Functional Requirements

### 7.1 Navigation and Session

- **FR-001:** The player MUST be able to start a new game from the main menu.
- **FR-002:** The player MUST be able to open the high-score view and settings view from the main menu.
- **FR-003:** The player MUST be able to pause and resume an active game.
- **FR-004:** The player MUST be able to restart from level 1 and return to the main menu.
- **FR-005:** A new session MUST initialize the configured starting lives, score, and first playable level.

### 7.2 Gameplay

- **FR-010:** The game MUST accept directional input from arrow keys and WASD where a keyboard is available.
- **FR-011:** Movement MUST reject walls and other non-traversable cells.
- **FR-012:** Eating a pellet or power pellet MUST remove it from the maze and award its configured score exactly once.
- **FR-013:** A power pellet MUST place eligible ghosts into frightened behavior for the configured duration.
- **FR-014:** Contact with a dangerous ghost MUST reduce lives according to the game rules; contact with a frightened ghost MUST award points and reset that ghost according to the game rules.
- **FR-015:** Clearing required pellets MUST advance to the next available level.
- **FR-016:** Losing the final life MUST end the session and display the final score and highest level reached.
- **FR-017:** Completing the final available level MUST end the session with a distinct completion outcome.

### 7.3 Scores and Players

- **FR-020:** At session end, the player MUST be able to enter a display name and save the final score, or skip saving.
- **FR-021:** Player display names MUST be trimmed, required when saving, and limited to a documented maximum length of at least 20 characters.
- **FR-022:** A saved score MUST include player identity, score, level reached, and a UTC timestamp.
- **FR-023:** The high-score view MUST default to the top 10 scores ordered by score descending.
- **FR-024:** Equal-score ordering MUST be deterministic; the newer implementation MUST document and test its tie-break rule.
- **FR-025:** Score and level values MUST be validated as non-negative before persistence.

### 7.4 Levels and Settings

- **FR-030:** Levels MUST be loaded through a storage-independent contract and returned in level-number order.
- **FR-031:** A level definition MUST contain a unique positive level number, maze layout, ghost speed, and derived or validated pellet count.
- **FR-032:** Level validation MUST reject malformed layouts with an actionable error before gameplay begins.
- **FR-033:** The maze format MUST support walls, pellets, power pellets, a player start, ghost starts, and empty corridors.
- **FR-034:** Settings MUST be accessed through a storage-independent contract rather than directly from the UI.
- **FR-035:** Supported settings MUST have typed definitions, defaults, validation rules, and allowed ranges or values.
- **FR-036:** Invalid settings MUST not be applied; the player or administrator MUST receive an actionable validation message.
- **FR-037:** Missing persistence MUST cause documented defaults to be used without preventing gameplay.

## 8. Modernization and Quality Requirements

### 8.1 Architecture

- **MOD-001:** The solution MUST retain or strengthen a pure game-engine project with no UI framework, database provider, filesystem, or network dependency.
- **MOD-002:** UI code MUST depend on application use cases or interfaces, not concrete database repositories.
- **MOD-003:** Score, level, and settings persistence MUST each have explicit interfaces suitable for in-memory test doubles.
- **MOD-004:** Infrastructure implementations MUST be selected through dependency injection or an equivalent composition mechanism at the application boundary.
- **MOD-005:** Domain and application projects MUST enable strict typing and MUST not introduce late binding for normal operation.
- **MOD-006:** The default replacement persistence SHOULD be a portable, transactional embedded database such as SQLite unless an architecture decision establishes a stronger business need.
- **MOD-007:** Database schema changes MUST be versioned and repeatable. Startup MUST not infer schema health only from the existence of a database file.
- **MOD-008:** Public contracts and persisted formats MUST be documented before they are changed.

### 8.2 Compatibility and Migration

- **MOD-010:** Migration MUST use an explicit export/import or side-by-side process; production migration MUST never modify the only copy of a legacy `.accdb` file.
- **MOD-011:** Migration MUST be idempotent or detect prior completion and stop safely.
- **MOD-012:** Migration output MUST report source, imported, skipped, and failed counts per entity plus validation errors.
- **MOD-013:** UTC timestamps, player names, score values, level numbers, maze text, and setting values MUST retain their meaning after migration.
- **MOD-014:** The migration stage MUST define backup, rollback, and compatibility windows before release.
- **MOD-015:** Access-specific code MAY remain as a temporary migration adapter but MUST not be referenced by the modern runtime path after cutover.

### 8.3 Performance and Reliability

- **NFR-001:** On reference hardware documented by the team, application startup SHOULD reach an interactive menu within 2 seconds at the 95th percentile.
- **NFR-002:** During active play, input-to-render latency SHOULD remain below 100 ms at the 95th percentile.
- **NFR-003:** The game loop MUST use a monotonic time source and MUST remain behaviorally stable when rendering is delayed.
- **NFR-004:** Persistence failures MUST be contained outside the game loop and MUST not crash an active session.
- **NFR-005:** Score saves and schema migrations MUST be transactional where the persistence engine supports transactions.
- **NFR-006:** Releases MUST not contain known data-loss defects or unresolved critical/high-severity security vulnerabilities.

### 8.4 Security and Privacy

- **SEC-001:** All persisted or remotely submitted values MUST be validated at the application boundary.
- **SEC-002:** Database access MUST use parameterized operations; dynamic user values MUST not be concatenated into commands.
- **SEC-003:** Player names MUST be treated as untrusted display data and safely encoded by the presentation layer.
- **SEC-004:** The product MUST collect no personal data beyond the player-supplied display name unless a future requirement adds consent, retention, and deletion rules.
- **SEC-005:** Logs MUST not contain secrets or unnecessary personal data.
- **SEC-006:** Dependencies MUST be pinned through project manifests and checked for known vulnerabilities in continuous integration.

### 8.5 Accessibility and User Experience

- **UX-001:** All menus, settings, dialogs, and game controls MUST be operable by keyboard.
- **UX-002:** Focus MUST be visible, navigation order MUST be logical, and controls MUST expose accessible names.
- **UX-003:** Status and game state MUST not be communicated by color alone.
- **UX-004:** Text and essential controls MUST meet WCAG 2.2 AA contrast targets where the chosen UI platform supports measurement.
- **UX-005:** The UI MUST remain usable under common display scaling and window sizes supported by the selected platform.
- **UX-006:** Persistence, validation, and migration errors shown to users MUST explain impact and a recovery action without exposing implementation details.

### 8.6 Testing, Delivery, and Operations

- **OPS-001:** Every business rule changed or extracted during modernization MUST have automated unit tests in the engine or application layer.
- **OPS-002:** Repository implementations MUST have integration tests covering ordering, validation, timestamps, transactions, and failure behavior.
- **OPS-003:** A CI pipeline MUST restore, build, test, format-check, and vulnerability-check the solution on every proposed change.
- **OPS-004:** Release artifacts MUST be reproducible, versioned, and accompanied by upgrade and rollback notes.
- **OPS-005:** Runtime diagnostics MUST record application version, startup outcome, persistence availability, and unexpected failures using structured events.
- **OPS-006:** Diagnostics MUST be local by default. Remote telemetry MUST be opt-in until a separately approved privacy requirement defines collection and retention.
- **OPS-007:** The project MUST document supported operating systems, runtime versions, data locations, backup steps, and recovery steps.

## 9. Data Requirements

| Entity | Required data | Key constraints |
|---|---|---|
| Player | Stable ID, display name, created-on UTC | Display name required; identity matching behavior documented. |
| High score | Stable ID, player ID, score, level reached, played-on UTC | Non-negative values; valid player reference; deterministic ranking. |
| Level | Stable ID, unique level number, maze layout, ghost speed, pellet count | Positive level number; valid layout; pellet count reconciled with layout. |
| Setting | Unique key, typed value, definition/version metadata where needed | Known key, valid value, documented default. |

The modern schema MAY normalize or add fields, but migration must preserve all source information required to reproduce current behavior. Identifiers do not need to retain the same physical type if a documented mapping is produced.

## 10. Modernization Delivery Stages

1. **Baseline:** Characterize current engine behavior with tests and record representative legacy data fixtures.
2. **Decouple:** Move all shared models and contracts into supported core/application projects; introduce a settings interface and composition root.
3. **Replace persistence:** Add the portable repository implementation, versioned schema, and tested Access migration adapter.
4. **Replace presentation:** Implement the selected cross-platform or modern presentation layer against application contracts while retaining parity tests.
5. **Cut over:** Run migration validation, package the supported runtime, publish rollback instructions, and remove Access from the normal execution path.
6. **Retire legacy:** Remove WinForms/.NET Framework/ACE components only after the acceptance gates and compatibility window are complete.

Each stage MUST build and test independently. A stage MUST NOT delete the previous working path before its replacement satisfies the applicable acceptance criteria.

## 11. Release Acceptance Gates

A modernization release is acceptable only when:

- **AC-01:** Automated tests demonstrate parity for movement, collision, scoring, lives, frightened mode, level completion, and game-over behavior.
- **AC-02:** A clean machine can install and run the release without Access, ACE OLEDB, COM registration, or .NET Framework.
- **AC-03:** New game, pause/resume, restart, menu return, score save/skip, high-score view, and settings workflows pass end-to-end tests.
- **AC-04:** The application remains playable with persistence deliberately unavailable and communicates that scores cannot be saved.
- **AC-05:** A representative `.accdb` fixture migrates successfully; entity counts and sampled field values reconcile, and a second migration attempt is safe.
- **AC-06:** Invalid names, scores, settings, and level layouts are rejected with tested errors.
- **AC-07:** Supported-platform build and packaging jobs pass, or exclusions are recorded in an approved architecture decision.
- **AC-08:** Accessibility checks cover keyboard operation, focus, accessible names, non-color cues, contrast, and display scaling.
- **AC-09:** Upgrade, backup, rollback, data-location, and known-limit documentation is complete.
- **AC-10:** No Must requirement is waived without an owner, rationale, expiry date, and approved follow-up work item.

## 12. Agent Implementation Contract

A future implementation agent working from this document MUST:

1. Cite the requirement IDs it intends to satisfy before changing code.
2. Inspect the current implementation and tests; treat this document as intent and verified tests as behavioral evidence.
3. State assumptions where this document is silent and request a decision for irreversible, externally visible, or data-destructive choices.
4. Prefer incremental changes that preserve a runnable product and existing public contracts.
5. Add or update automated tests with each behavioral change and report the exact validation commands and results.
6. Never delete or overwrite a legacy database as part of migration testing; operate on a copy and produce reconciliation output.
7. Avoid introducing cloud services, accounts, telemetry, microservices, or new paid dependencies without an approved business requirement.
8. Record consequential technology and compatibility decisions in an architecture decision record under `docs/adr/`.
9. Update this requirements baseline when approved behavior changes; do not silently reinterpret requirement IDs.
10. Flag contradictions between requirements, code, tests, and data before choosing which behavior to preserve.

## 13. Open Decisions

The following decisions are intentionally not prescribed and require documented evaluation:

- **OD-01:** Target presentation technology and exact supported platforms.
- **OD-02:** SQLite versus another portable persistence engine.
- **OD-03:** Whether player identity is case-sensitive and how legacy duplicate names are reconciled.
- **OD-04:** High-score tie-break ordering and score-retention limits.
- **OD-05:** Whether settings remain player-local or support named profiles.
- **OD-06:** Distribution channels, signing, automatic updates, and support lifetime.
- **OD-07:** Whether an optional online leaderboard is justified by a future business requirement.

Until resolved, agents must choose the least destructive option, keep the decision replaceable, and avoid expanding product scope.
