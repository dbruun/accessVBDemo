# ChompMan .NET 10, C#, SQLite, and EF Core Upgrade Plan

## 1. Purpose and completion contract

This document is the implementation runbook for replacing the current VB.NET, .NET Framework 4.8, Microsoft Access application with a complete C# application using .NET 10, Windows Forms, SQLite, and Entity Framework Core 10.

The Markdown file itself is not executable. An engineer or coding agent executes the phases and commands in this runbook. The migration is complete only when every exit criterion in Section 12 is satisfied.

A generated solution, empty forms, placeholder repositories, TODOs, or a game that only reaches the main menu is a failure. The final application must:

- Build from a clean checkout with the .NET 10 SDK.
- Contain production code in C# only; no `.vb` or `.vbproj` files remain in the final solution.
- Start, display the menu, play all three levels, pause, restart, lose, win, save a score, display high scores, and apply settings.
- Create and migrate a SQLite database on first launch without Access, ACE OLEDB, ADOX, or Microsoft Office.
- Import an existing Access database when one is supplied and produce a reconciliation report.
- Continue to offer playable baseline content when SQLite is unavailable, while clearly reporting that scores and settings cannot be saved.
- Render gameplay through a dedicated double-buffered WinForms panel without visible flicker.
- Pass all automated tests, release build checks, database checks, and the playable smoke test.

## 2. Current state

### 2.1 Solution and runtime

| Area | Current implementation | Migration consequence |
|---|---|---|
| Application | `ChompMan/ChompMan.vbproj`, SDK-style VB WinExe targeting `net48` | Replace with a C# WinForms project targeting `net10.0-windows`. |
| Core | `ChompMan.Core/ChompMan.Core.vbproj`, VB class library targeting `net48` | Port engine and contracts to a C# `net10.0` library. |
| Tests | `ChompMan.Tests/ChompMan.Tests.vbproj`, MSTest on `net48` | Port all 35 test methods to C# and add persistence, migration, UI, and end-to-end coverage. |
| UI | Five manually constructed WinForms forms using GDI+ | Preserve the desktop interaction model and redraw the game using C# GDI+. |
| Persistence | ADO.NET `OleDb`, ACE OLEDB 16.0, and late-bound ADOX | Replace runtime persistence with EF Core 10 and SQLite. |
| Initialization | Creates `ChompMan.accdb` beside the executable and seeds schema/data only when the file is absent | Store writable data under the user's local application-data directory and use EF migrations plus idempotent seed logic. |
| Source layout | Game engine and shared data files exist under both `ChompMan` and `ChompMan.Core`; the app project excludes duplicates with `Compile Remove` | Establish one source of truth under the new C# projects and delete the duplicate VB trees at final cutover. |

No `.accdb` or SQLite database is currently checked into the repository. An actual deployed `.accdb` must therefore be supplied separately for production-data migration. The three baseline levels and eight settings can be recovered from `DatabaseInitializer.vb` when no source database exists.

### 2.2 Current database

The Access schema contains:

| Table | Current columns | Important behavior |
|---|---|---|
| `Players` | `PlayerId`, `Name`, `CreatedOn` | Score saving reuses the first exact matching name, but the database does not enforce uniqueness. |
| `HighScores` | `ScoreId`, `PlayerId`, `Score`, `LevelReached`, `PlayedOn` | Top scores sort by score only, so ties are nondeterministic. Player creation and score insertion are not wrapped in a transaction. |
| `Levels` | `LevelId`, `LevelNumber`, `MazeLayout`, `GhostSpeed`, `PelletCount` | Three levels are seeded; uniqueness and content validity are not database-enforced. |
| `Settings` | `Key`, `Value` | Values are unrestricted strings. The form saves them, but gameplay currently ignores them. |

Access-specific behavior that must not survive in the production application includes `SELECT TOP`, positional `?` parameters, `SELECT @@IDENTITY`, ADOX COM database creation, and a database path beside the executable.

### 2.3 Current game and UI behavior

- `Engine.vb` is independent of WinForms and data access. It owns movement, collision detection, pellets, power pellets, ghost modes, scoring, extra lives, respawn, and level completion.
- The game loop uses `System.Windows.Forms.Timer` with a 16 ms interval and invalidates the game panel after each update.
- `GameForm.vb` draws walls, pellets, the player, ghosts, and overlays with GDI+.
- `BufferedPanel` already enables `DoubleBuffered`, `OptimizedDoubleBuffer`, `AllPaintingInWmPaint`, and `UserPaint`. The C# replacement must retain and strengthen this behavior as a dedicated component.
- The app catches database initialization failure and allows play with a built-in level. High scores and settings can still encounter database failures later because forms construct Access repositories directly.
- Forms instantiate concrete repositories themselves, so persistence is tightly coupled to the UI and difficult to test.

### 2.4 Behavior defects to fix during parity work

These are not optional redesigns; leaving them in place would violate the approved business requirements:

1. Loading a new level constructs a new `Player`, resetting score and lives. The C# run/session object must carry score, lives, extra-life milestones, and highest level across levels.
2. Missing levels are synthesized from level 1, making the win branch unreachable. The new level catalog must return no next level after level 3 and complete the run as a win.
3. Persisted settings do not affect engine timing or rendering. The C# settings service must parse, validate, snapshot, and apply them when a new run begins.
4. Engine randomness uses a shared static `Random`. Inject an `IRandomSource` or seeded `Random` so ghost decisions are reproducible in tests.
5. Ranking ties are unstable. Use score descending, played time ascending, then score ID ascending unless the product owner approves another rule.
6. Player names are stored as 100 characters but the business UI limit is 20. The importer must report names over 20 characters; it must not silently truncate them.
7. Level content is not validated before play. Add symbol, dimensions, spawn, pellet-count, reachability, speed, and level-number validation.

## 3. Target state

### 3.1 Supported platform

- Windows 10/11 supported by .NET 10 Windows Desktop.
- .NET 10 SDK for build and test.
- `net10.0-windows` for WinForms and `net10.0` for non-UI projects.
- x64 self-contained release as the primary distributable; framework-dependent x64 may also be produced for managed environments.
- Per-user database at `%LOCALAPPDATA%\ChompMan\ChompMan.db` by default. Tests and tools must accept an explicit path.

### 3.2 Final solution layout

```text
ChompMan.sln
Directory.Build.props
Directory.Packages.props
src/
  ChompMan.Core/
    ChompMan.Core.csproj
    GameEngine/
    Contracts/
    Models/
    Validation/
  ChompMan.Data/
    ChompMan.Data.csproj
    ChompManDbContext.cs
    Entities/
    Configurations/
    Migrations/
    Repositories/
    Seeding/
  ChompMan.WinForms/
    ChompMan.WinForms.csproj
    Program.cs
    UI/
      DoubleBufferedGamePanel.cs
      GameForm.cs
      MainMenuForm.cs
      GameOverForm.cs
      HighScoresForm.cs
      SettingsForm.cs
    Rendering/
tests/
  ChompMan.Core.Tests/
  ChompMan.Data.Tests/
  ChompMan.WinForms.Tests/
tools/
  ChompMan.AccessMigrator/
docs/
```

`ChompMan.Core` must not reference WinForms, EF Core, SQLite, or `System.Data.OleDb`. `ChompMan.Data` owns EF entities and maps them to core DTOs/contracts. `ChompMan.WinForms` composes the application and depends on abstractions rather than constructing repositories in individual forms.

The Access migrator is a C# Windows-only transition tool. It may depend on `System.Data.OleDb` and ACE, but it must not be referenced by or published with the game. Remove it from release artifacts after the migration retention period; the playable application itself must have no Access dependency.

### 3.3 Package policy

Pin a tested .NET 10-compatible patch version in `Directory.Packages.props`; do not use floating versions. All EF packages must use exactly the same `10.0.x` version.

- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design` with `PrivateAssets="all"`
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.Logging`
- Current .NET 10-compatible MSTest packages, retaining MSTest to minimize test conversion risk
- `Microsoft.Data.Sqlite` directly only where low-level integration assertions are needed
- `System.Data.OleDb` only in `ChompMan.AccessMigrator`

Enable nullable reference types, implicit usings, deterministic builds, analyzers, warnings as errors, and `RestorePackagesWithLockFile` for new C# projects. Commit each generated `packages.lock.json` so final verification can restore with `--locked-mode`. Commit `global.json` only if the repository requires an exact SDK feature band; otherwise document the minimum .NET 10 SDK.

### 3.4 Application composition

Use the .NET Generic Host in `Program.cs` to register logging, `IDbContextFactory<ChompManDbContext>`, repositories, settings, level validation, and forms. A short-lived DbContext must be created per repository operation; never retain one for the lifetime of a form.

Startup sequence:

1. Resolve and create the per-user application-data directory.
2. Configure file/debug logging without recording player names unnecessarily.
3. Run pending EF migrations and idempotent seed validation.
4. If persistence succeeds, register SQLite repositories.
5. If persistence fails, log the exception, show one concise warning, and register in-memory/read-only fallback levels and default settings plus an unavailable score repository.
6. Start `MainMenuForm` on the WinForms UI thread.

Repository operations used by forms should be asynchronous and accept `CancellationToken` where practical. UI continuations must return to the UI thread, and controls must be disabled while a save/load is in progress.

## 4. SQLite and EF Core design

### 4.1 Target schema

| Entity | Columns and constraints |
|---|---|
| `Player` | `PlayerId INTEGER PRIMARY KEY`, `Name TEXT NOT NULL`, `CreatedOnUtc TEXT NOT NULL`; exact-name uniqueness using SQLite binary collation after duplicate reconciliation. Enforce 1-20 characters in application validation. |
| `HighScore` | `ScoreId INTEGER PRIMARY KEY`, `PlayerId INTEGER NOT NULL`, `Score INTEGER NOT NULL CHECK Score >= 0`, `LevelReached INTEGER NOT NULL CHECK LevelReached >= 1`, `PlayedOnUtc TEXT NOT NULL`, foreign key to `Player`. Add index on `(Score DESC, PlayedOnUtc, ScoreId)`. |
| `Level` | `LevelId INTEGER PRIMARY KEY`, `LevelNumber INTEGER NOT NULL UNIQUE`, `MazeLayout TEXT NOT NULL`, `GhostSpeed INTEGER NOT NULL CHECK GhostSpeed > 0`, `PelletCount INTEGER NOT NULL CHECK PelletCount >= 0`. |
| `Setting` | `Key TEXT PRIMARY KEY`, `Value TEXT NOT NULL`; supported keys are validated by a typed settings service. Unknown imported keys are reported and retained only if explicitly approved. |
| `MigrationAudit` | Import ID, source file hash, source modified time, started/completed UTC, status, and reconciliation report path. This makes imports auditable and prevents accidental duplicate replay. |

Configure foreign keys with `DeleteBehavior.Restrict`. Enable `PRAGMA foreign_keys=ON`, a sensible busy timeout, and WAL mode during normal operation. Handle `SQLITE_BUSY` with a bounded retry policy; never retry validation or constraint errors.

Persist UTC timestamps and convert to local time only for display. Explicitly test round trips because SQLite has no native `DateTime` storage class.

### 4.2 Migrations and seed data

- Create and commit an initial EF migration. Do not use `EnsureCreated`, because it bypasses migration history.
- Call `Database.MigrateAsync()` on startup before repositories are used.
- Keep baseline level layouts in versioned embedded text/JSON resources, not duplicated string literals across UI and migrations.
- Run an idempotent seeder after migrations. Insert baseline settings and three levels only when the corresponding tables are empty; never overwrite operator changes on later starts.
- Validate every seed level before insertion. Store the calculated pellet count and fail startup seeding if it differs from the declared count.
- Keep one validated level and default settings as embedded fallback resources so gameplay remains possible during a database outage.

Useful implementation commands, run from the repository root:

```powershell
dotnet tool restore
dotnet ef migrations add InitialSqlite --project src/ChompMan.Data --startup-project src/ChompMan.WinForms --output-dir Migrations
dotnet ef database update --project src/ChompMan.Data --startup-project src/ChompMan.WinForms
```

## 5. Access-to-SQLite data migration

### 5.1 Migrator requirements

Implement `tools/ChompMan.AccessMigrator` in C#. It must accept explicit source, target, and report paths and support `--dry-run` and `--overwrite-target`. It must never modify the Access source.

```powershell
dotnet run --project tools/ChompMan.AccessMigrator -- `
  --source "C:\backup\ChompMan.accdb" `
  --target "C:\staging\ChompMan.db" `
  --report "C:\staging\migration-report.json" `
  --dry-run
```

The tool must:

1. Refuse to run if the source is writable by the process unless `--allow-writable-source` is explicitly supplied; always read it without updates.
2. Record the SHA-256 hash and modified time of the source.
3. Read all four tables with parameterized/structured APIs.
4. Validate source values before writing any target rows.
5. Create a new temporary SQLite database, apply EF migrations, import within a transaction, and atomically rename the completed file into place.
6. Preserve source integer identifiers when valid and collision-free so foreign-key reconciliation is straightforward.
7. Normalize line endings in maze text only after proving that layout rows and columns are unchanged.
8. Recalculate pellets and report mismatches without silently changing the declared or source value.
9. Detect exact duplicate player names. Repoint their scores to one canonical player only under an approved deterministic rule; otherwise fail with a report requiring a decision.
10. Reject/report blank names, names over 20 characters, negative scores, invalid levels, orphan scores, duplicate level numbers, unknown maze symbols, invalid settings, and ambiguous timestamps.
11. Commit only when there are no blocking errors.
12. Emit human-readable Markdown and machine-readable JSON reports.

### 5.2 Reconciliation acceptance

For each table, report source count, accepted count, rejected count, merged count, and target count. Also compare:

- Source and target primary-key sets.
- Every high-score-to-player relationship.
- Level layout SHA-256 hashes, dimensions, pellet counts, starts, and ghost speeds.
- All known settings after typed parsing.
- Top ten leaderboard results using the approved deterministic tie ordering.
- Minimum and maximum timestamps interpreted as UTC.

Import is accepted only when all blocking discrepancies are zero or are listed in a signed exception record. Re-running the same source against the same target must be refused or produce an identical clean target, never duplicate rows.

## 6. Game engine migration

Port the engine one type at a time with behavior-preserving C# tests. Avoid automated syntax conversion without review, especially around integer division, value equality, event signatures, optional arguments, nullable values, and VB line endings.

Required work:

1. Port enums, `Position`, entities, `Maze`, `GameState`, and `Engine` into `ChompMan.Core`.
2. Port the existing 35 MSTest methods before changing behavior and make them pass against C#.
3. Add injectable randomness and deterministic tests for chase, scatter, frightened, and eaten paths.
4. Add a `GameRun` or equivalent session state that owns score, lives, extra-life milestone, current/highest level, and settings snapshot across level transitions.
5. Make the level sequence finite. `TryGetNextLevel` returns false after the highest defined level, causing a win.
6. Move configurable values out of compiled constants where required: starting lives, player speed, default ghost speed, frightened duration, and colors. Keep scoring values fixed unless requirements change.
7. Preserve ghost no-reverse behavior, tunnel wrapping, passability rules, collision outcomes, combo scores of 200/400/800/1600, 10,000-point extra lives, respawn timing, pause behavior, and pellet consumption.
8. Validate a level before constructing `Maze`; do not allow malformed database content to crash the paint loop.

Add focused tests for cross-level score/lives continuity, final-level win, setting application, each extra-life threshold, one award when a score event crosses multiple thresholds, deterministic tied ranking, invalid level rejection, and fallback play.

## 7. WinForms and rendering migration

Port all five forms to C#. Preserve keyboard navigation, Arrow/WASD movement, `P` pause/resume, `R` restart, and Escape to menu. Inject repositories/services through constructors. Forms must not know database paths or create DbContexts.

Replace the unrestricted settings grid with typed controls:

- Numeric controls with documented ranges for lives and tick counts.
- Color pickers or constrained color choices with contrast validation.
- Validation messages before save.
- A clear statement that values apply to the next new game.

### 7.1 Required double-buffered panel

Create `DoubleBufferedGamePanel.cs` as a sealed dedicated control. Its constructor must enable:

```csharp
DoubleBuffered = true;
ResizeRedraw = true;
SetStyle(
    ControlStyles.UserPaint |
    ControlStyles.AllPaintingInWmPaint |
    ControlStyles.OptimizedDoubleBuffer,
    true);
UpdateStyles();
```

`GameForm` must host this type, not a standard `Panel`, and all maze rendering must occur in its paint path. Allocate reusable brushes/fonts outside the frame loop and dispose owned GDI objects when the form closes. Do not dispose `PaintEventArgs.Graphics` or shared framework brushes/pens.

Rendering acceptance:

- No visible flicker during at least five minutes of continuous movement, frightened mode, pause overlays, resize/DPI initialization, and level transitions.
- No steady increase in GDI handle count during a 15-minute run.
- The panel maintains stable dimensions derived from maze rows/columns and DPI scaling; HUD text does not overlap.
- `Invalidate` schedules repaint without calling `CreateGraphics`.
- A UI test asserts that `GameForm` contains `DoubleBufferedGamePanel`; a small control test verifies the expected styles through a testable subclass/property or reflection.

Keep the 16 ms timer for initial parity. Measure updates and paint cadence under load before considering a `Stopwatch` fixed-step accumulator; changing the timing model during syntax conversion adds unnecessary parity risk.

## 8. Phased implementation plan

Each phase ends in a buildable state. Do not delete the legacy implementation until Phase 8 passes.

### Phase 0: Baseline and recovery assets

1. Create a migration branch and tag the known-good legacy commit.
2. Run and record the current release build and all 35 tests.
3. Execute the gameplay checklist on the VB version and capture screenshots of every form and one frame of each level.
4. Copy the deployed `.accdb` to read-only backup storage, record SHA-256, file size, modified time, ACE bitness, row counts, top ten, and settings.
5. Record the current fallback behavior with ACE unavailable.

Exit: the legacy binary is reproducibly buildable/playable, source data is immutable and backed up, and baseline evidence is stored outside build output directories.

### Phase 1: Create the C# structure

1. Add the target C# projects alongside the legacy projects.
2. Add central build/package configuration and project references.
3. Configure nullable analysis and warnings as errors.
4. Keep the legacy projects in the solution for side-by-side comparison.

Exit: `dotnet restore` and `dotnet build -c Release` succeed with empty infrastructure only; this is an intermediate checkpoint, never a completion point.

### Phase 2: Port and characterize the core

1. Port core value types and tests in small slices.
2. Port maze parsing/entities and tests.
3. Port engine logic and all remaining tests.
4. Add deterministic random injection and the missing parity tests.
5. Implement cross-level run continuity and finite win behavior only after the direct-port suite is green.

Exit: all 35 converted tests and all new parity tests pass on `net10.0`; `ChompMan.Core` has no platform or persistence dependency.

### Phase 3: Build SQLite persistence

1. Implement entities, fluent mappings, DbContext factory, repositories, validation, migrations, and seed resources.
2. Add transaction-safe score save and deterministic leaderboard ordering.
3. Add typed settings parsing and update behavior.
4. Test against a unique temporary SQLite file per test; do not use EF's in-memory provider because it does not reproduce SQLite behavior.
5. Test first start, repeat start, migration from the previous schema version, foreign keys, constraints, concurrent score saves, busy handling, and UTC round trips.

Exit: data tests pass against real SQLite; the generated schema contains expected constraints/indexes; initialization is repeatable and non-destructive.

### Phase 4: Build and validate the Access migrator

1. Implement dry-run, import, reports, source hashing, and duplicate-run protection.
2. Test empty, seeded, malformed, duplicate, and orphaned fixture databases.
3. Run against a copy of the real Access database.
4. Review and resolve every blocking discrepancy, then rerun from a fresh target.

Exit: a clean import report reconciles every accepted source row and top-ten result; the source hash still matches the backup.

### Phase 5: Port the complete WinForms experience

1. Compose services and fallback implementations in `Program.cs`.
2. Port menu, leaderboard, settings, game-over, and game forms.
3. Implement and use `DoubleBufferedGamePanel`.
4. Apply settings snapshots to new games and rendering.
5. Ensure database failures are handled at every load/save boundary without ending active play.
6. Complete keyboard, DPI, disposal, and accessibility behavior.

Exit: every form is functional; all three levels can be played; both loss and win workflows can save/display scores; fallback mode is playable; double-buffering checks pass.

### Phase 6: Integration, performance, and endurance

1. Add an end-to-end smoke harness where practical and maintain a short manual play checklist for real input/rendering.
2. Run a 15-minute game while monitoring memory, CPU, GDI handles, timer responsiveness, and database errors.
3. Test database locked, read-only directory, corrupt file, migration failure, invalid level, and invalid setting scenarios.
4. Test 100%, 150%, and 200% DPI and supported display sizes.
5. Verify no UI-thread cross-thread exceptions and no unobserved task exceptions.

Exit: no crash, flicker, resource leak, overlapping UI, or silent persistence failure is observed; performance evidence is recorded.

### Phase 7: Release packaging and clean-room verification

1. Publish x64 release output to an empty directory.
2. Verify the artifact contains no `.vb`, Access database, ACE/ADOX/OleDb runtime dependency, migrator, development database, or test package.
3. Test first launch as a standard non-administrator user on a machine without ACE and without a preinstalled .NET runtime when using self-contained output.
4. Play, save, close, restart, and verify score/settings persistence.

Exit: the release artifact is independently playable and persists SQLite data in the per-user location.

### Phase 8: Final replacement and legacy removal

1. Remove legacy VB projects from the solution.
2. Delete all duplicated `.vb` source and obsolete Access DDL/runtime code after archival/tagging.
3. Rename target projects to the final product names if temporary migration suffixes were used.
4. Update `README.md` with .NET 10 build/run/test/publish commands, SQLite location, backup instructions, and migration-tool usage.
5. Re-run the complete clean-room verification from a fresh checkout.

Exit: the repository and release satisfy Section 12. A solution that still needs the VB application for any user journey has not completed migration.

## 9. Verification commands

Run from the repository root after final project paths exist:

```powershell
dotnet --info
dotnet restore ChompMan.sln --locked-mode
dotnet build ChompMan.sln -c Release --no-restore -warnaserror
dotnet test ChompMan.sln -c Release --no-build --logger "trx;LogFileName=chompman.trx"
dotnet ef migrations has-pending-model-changes --project src/ChompMan.Data --startup-project src/ChompMan.WinForms
dotnet publish src/ChompMan.WinForms/ChompMan.WinForms.csproj -c Release -r win-x64 --self-contained true -o artifacts/win-x64
```

Add a repository script such as `scripts/verify.ps1` during implementation to run these checks, inspect the publish manifest for forbidden dependencies, create a temporary database, and verify migration/seed counts. The script is part of the implementation deliverable; this plan does not pretend that Markdown itself can run the build.

Manual playable smoke test:

1. Launch the published executable as a standard user.
2. Open Settings, change starting lives and each visible color, save, then start a new game and verify application.
3. Verify Arrow and WASD movement, wall blocking, queued turns, tunnel wrapping, pellets, power pellets, frightened/eaten ghosts, collision, respawn, extra life, pause, and restart.
4. Complete levels 1 and 2 and confirm score/lives persist; complete level 3 and confirm the win workflow.
5. Complete a losing run, reject a blank/overlong name, save a valid score, and verify deterministic top-ten display.
6. Restart and confirm SQLite persistence.
7. Temporarily make the database unavailable, launch again, and confirm fallback gameplay plus accurate save-unavailable messaging.
8. Observe the game panel for flicker and check that GDI handles remain stable during the endurance run.

## 10. Risks and mitigations

| Risk | Likelihood/impact | Mitigation and proof |
|---|---|---|
| Syntax conversion changes engine semantics | Medium/high | Port in dependency order, preserve tests first, add seeded randomness, and compare deterministic state traces between VB and C#. |
| Access timestamps have ambiguous kind | High/medium | Report every interpretation, compare known samples, approve one policy, store target values as UTC, and retain source values in the migration report. |
| Duplicate or overlong player names conflict with new rules | Medium/medium | Dry-run report; require an explicit merge/reject exception; never truncate or merge silently. |
| Invalid or unreachable seeded maze data blocks play | Medium/high | Add validator and migration report; retain one independently validated embedded fallback level. |
| SQLite file is unwritable, locked, or corrupt | Medium/high | Use LocalAppData, short DbContext lifetimes, WAL/busy timeout, bounded retries, startup backup, logging, and playable fallback mode. |
| EF migration partially fails | Low/high | Back up the database before migration, let EF use transactions where SQLite permits, and restore the file instead of relying solely on down migrations. |
| Score/player save is partially committed | Low/medium | Use one DbContext transaction plus unique-name constraint and test forced failure between operations. |
| UI freezes during database I/O | Medium/medium | Async repository APIs, short operations, cancellation on form close, and UI-thread responsiveness tests. |
| GDI flicker or handle leak after C# port | Medium/high | Dedicated double-buffered panel, cached/disposed resources, five-minute visual test, and 15-minute handle monitoring. |
| Settings alter legacy feel | Medium/medium | Snapshot settings only at new-game start, validate ranges, retain documented defaults, and add engine tests for each setting. |
| Big-bang replacement prevents recovery | Medium/high | Keep legacy and modern projects side by side through Phase 7, use phase gates, immutable source backup, and versioned release artifacts. |
| Final artifact accidentally retains ACE/OleDb | Low/high | Keep migrator unreferenced, inspect dependency/publish manifests, and clean-room test on a machine without ACE. |

## 11. Rollback strategy

### 11.1 Before cutover

- Tag the legacy source and retain its verified release artifact.
- Keep the original Access database read-only and retain at least two verified backups with hashes.
- Build the modern database in staging; never convert the only source file in place.
- Keep each accepted SQLite database snapshot with application version, EF migration ID, source hash, and reconciliation reports.
- Do not dual-write Access and SQLite. Stop legacy writes, take the final Access backup, run the importer, verify, then open modern writes.

### 11.2 Rollback triggers

Rollback when any of these occur during pilot/cutover:

- The release cannot start or complete a new game on a supported machine.
- Imported row counts, relationships, levels, settings, or leaderboard do not reconcile.
- A repeatable score/settings data-loss or corruption defect occurs.
- Rendering/input regression makes gameplay materially unusable.
- Database migration cannot complete and fallback operation is insufficient for the deployment window.

### 11.3 Application rollback

1. Stop distribution and exit all modern application instances.
2. Copy the current SQLite file and logs to incident storage; do not delete or downgrade it.
3. Redeploy the tagged legacy binary and a writable copy of the last verified Access backup.
4. Confirm launch, level load, and score save on the legacy version.
5. Record scores created only in SQLite after cutover. The default rollback preserves them in the captured SQLite file but does not claim they exist in Access.

If zero leaderboard loss during rollback is mandatory, Phase 4 must also deliver and test a controlled C# delta exporter/importer that copies post-cutover players and scores from SQLite into a copy of Access, with the same reconciliation and transaction rules. Do not improvise reverse SQL during an incident.

### 11.4 Database migration rollback

SQLite schema rollback should restore the pre-migration file backup and matching application version. SQLite migrations often rebuild tables, so an EF `Down` method alone is not an adequate production rollback. Verify the restored file with `PRAGMA integrity_check`, expected migration history, row counts, and an application smoke test before reopening writes.

### 11.5 Roll-forward preference

After data has been written by the modern application, prefer a patched C# release that reads the existing SQLite schema. Preserve failed files and logs, reproduce against copies, and never ask users to delete their database as a recovery step.

## 12. Final acceptance gate

All items are mandatory:

- [ ] `dotnet restore`, Release build with warnings as errors, all tests, and publish succeed from a clean checkout.
- [ ] The final solution contains only C# production/test projects and no `.vb`/`.vbproj` files.
- [ ] The published game has no Access, ACE, ADOX, COM, or OleDb dependency.
- [ ] First launch creates `%LOCALAPPDATA%\ChompMan\ChompMan.db`, applies committed EF Core 10 migrations, and seeds valid settings and exactly three baseline levels.
- [ ] Access import dry-run and committed import reconcile all approved source rows and produce JSON/Markdown reports when a source database is supplied.
- [ ] All current engine behaviors have C# parity tests, and the current 35 tests remain represented.
- [ ] Score and lives persist across levels; clearing level 3 reaches a win rather than generating another level.
- [ ] Settings are typed, validated, persisted, and visibly affect the next new game.
- [ ] Player/score save is atomic; leaderboard ordering is deterministic; timestamps round-trip as UTC and display locally.
- [ ] Main menu, game, high scores, settings, game-over, loss, win, restart, pause, and Escape workflows operate in the published binary.
- [ ] `GameForm` renders through `DoubleBufferedGamePanel` with the required control styles and passes flicker/GDI-handle checks.
- [ ] The published game runs on a clean supported Windows machine as a standard user and can be played without ACE or a preinstalled database.
- [ ] Persistence failure does not crash gameplay; fallback content and defaults work, and unavailable saves are never reported as successful.
- [ ] README, backup/restore instructions, migration report location, data path, and support logging are current.
- [ ] Legacy source/data backups and rollback artifacts have been verified before the final cutover.

Only after this checklist is complete may the migration be described as finished and the legacy VB/Access runtime retired.