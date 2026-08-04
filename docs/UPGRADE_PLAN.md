# ChompMan .NET 10, C#, SQLite, and EF Core Upgrade Plan

## 1. Purpose and Definition of Done

This plan migrates ChompMan from VB.NET, .NET Framework 4.8, Microsoft Access, ADO.NET/OLE DB, and ACE COM to a complete C# implementation using .NET 10, WinForms, SQLite, and EF Core 10.

The selected presentation target is **C# WinForms on `net10.0-windows`**. This deliberately preserves the current desktop game and GDI+ rendering model. It is Windows-only; this resolves OD-01 in favor of lowest-risk playable parity rather than cross-platform presentation. SQLite and EF Core resolve OD-02. These choices satisfy the requested upgrade but do not satisfy the optional cross-platform aspiration in BR-11; a later UI replacement can reuse the platform-neutral engine and application projects.

The upgrade is complete only when all of the following are true:

- `dotnet build ChompMan.sln -c Release` succeeds from a clean checkout with the .NET 10 SDK.
- `dotnet test ChompMan.sln -c Release --no-build` passes with no skipped or inconclusive parity tests.
- `dotnet run --project src/ChompMan.WinForms/ChompMan.WinForms.csproj` opens the menu and a player can complete the full workflow: start, move, pause, resume, restart, lose/win, save or skip a score, view scores, edit settings, and return to the menu.
- The final runtime uses C# only. No `.vbproj` or `.vb` file is built or shipped.
- The final runtime uses `Microsoft.EntityFrameworkCore.Sqlite`; it has no Access, ACE, ADOX, COM, `OleDb`, `.accdb`, or .NET Framework dependency.
- A versioned EF Core migration creates and upgrades `ChompMan.db`. `Database.EnsureCreated()` is not used.
- A C# migration tool imports a copied legacy `.accdb` into SQLite, reconciles every entity, and safely detects a repeated import.
- The active game surface is a custom double-buffered panel and passes flicker, disposal, resizing, and input/render smoke tests.
- A clean Windows machine can run the published game without installing .NET, Access, ACE, or a SQLite utility.

Creating projects, forms, entities, or repository stubs is not delivery. A scaffold is a failed migration. Each phase below has a playable or executable exit gate, and the legacy path remains available until the replacement passes its gate.

This plan implements the intent of BG-01 through BG-06, BR-01 through BR-10, the applicable functional requirements, MOD-001 through MOD-015, NFR-001 through NFR-006, SEC-001 through SEC-006, UX-001 through UX-006, OPS-001 through OPS-007, and AC-01 through AC-10 in `BUSINESS_REQUIREMENTS.md`.

## 2. Current State

### 2.1 Solution and runtime

| Area | Current implementation | Upgrade consequence |
|---|---|---|
| Desktop app | `ChompMan/ChompMan.vbproj`, VB.NET WinForms, `net48`, `WinExe` | Replace side-by-side with C# `net10.0-windows`; preserve all five workflows. |
| Core | `ChompMan.Core/ChompMan.Core.vbproj`, VB.NET, `net48` | Port behavior to a C# `net10.0` library with no UI or persistence references. |
| Tests | MSTest 3.3 / Test SDK 17.9, VB.NET, `net48` | Port and expand in C# on `net10.0`; remove all inconclusive tests. |
| Persistence | Access `.accdb`, ACE OLE DB 16.0, ADOX COM, handwritten SQL | Replace runtime persistence with EF Core 10 and SQLite. Keep Access reading only in the migration utility during the compatibility window. |
| Startup | Database file beside executable; schema inferred from file existence | Move writable data to `%LOCALAPPDATA%\ChompMan`; apply EF migrations transactionally. |
| Rendering | GDI+ in `GameForm`; 16 ms WinForms timer; custom `BufferedPanel` | Port drawing behavior and retain an explicit double-buffered panel. Separate fixed-step update timing from paint frequency. |

### 2.2 Current data

The Access database contains:

- `Players`: `PlayerId`, `Name`, `CreatedOn`.
- `HighScores`: `ScoreId`, `PlayerId`, `Score`, `LevelReached`, `PlayedOn`.
- `Levels`: `LevelId`, `LevelNumber`, `MazeLayout`, `GhostSpeed`, `PelletCount`.
- `Settings`: `Key`, `Value`.

Startup creates and seeds three levels, eight settings, two sample players, and two sample scores. Repository behavior currently matches players by name, returns scores ordered only by score descending, and exposes settings as unvalidated strings.

### 2.3 Current behavior to characterize before porting

The engine already has no WinForms or database references and covers directional movement, wall blocking, pellets, power pellets, frightened/chase/scatter/eaten ghost states, collisions, lives, respawn, level completion, pause, scoring, and extra lives. The forms provide menu, game, high-score, settings, and game-over workflows.

The baseline suite is not sufficient as a migration oracle. `ScoringTests.vb` contains twelve `Assert.Inconclusive` placeholders. There are no repository integration tests, migration tests, UI workflow tests, settings-validation tests, malformed-maze tests, or final-level tests.

The following observed behaviors must be turned into explicit tests and product decisions before translation:

- `GameForm.LoadLevel` creates a new `Player` with three lives, so score and lives reset between levels. The target must preserve session score and remaining lives instead.
- `GetLevelDef` synthesizes a fallback for every missing level, so final-level completion may never reach the win path. The target must distinguish a missing next level from the level-1 offline fallback.
- Stored `StartingLives`, `PlayerSpeedTicks`, `FrightenedDuration`, colors, and default ghost speed are editable but are not consistently applied to a new game. The target must validate and apply supported settings.
- `GetTopScores` has no deterministic tie-break. The target order will be `Score DESC`, `PlayedOnUtc ASC`, then `HighScoreId ASC`, rewarding the earlier achievement and remaining deterministic (resolves OD-04).
- Player lookup currently depends on Access collation. The target will preserve display casing and match trimmed names case-insensitively using a normalized name (resolves OD-03). Distinct legacy rows are preserved and mapped; they are not silently merged during import.
- The maze parser accepts ragged rows, unknown characters, absent starts, and any ghost count. The target validator must reject malformed stored levels before play while retaining a validated built-in fallback level.
- The engine uses a shared nondeterministic `Random` and timer ticks as time. Inject random and monotonic-time abstractions so tests are deterministic and delayed rendering does not alter game speed.

## 3. Target State

### 3.1 Repository layout

```text
ChompMan.sln
Directory.Build.props
Directory.Packages.props
src/
  ChompMan.Core/                 C# net10.0: game domain and engine only
  ChompMan.Application/          C# net10.0: use cases, DTOs, validation, interfaces
  ChompMan.Infrastructure/       C# net10.0: EF Core SQLite context and repositories
  ChompMan.WinForms/             C# net10.0-windows: forms, rendering, composition root
  ChompMan.LegacyMigration/      C# net10.0-windows: read-only Access importer
tests/
  ChompMan.Core.Tests/           C# net10.0 unit and characterization tests
  ChompMan.Application.Tests/    C# net10.0 use-case and validation tests
  ChompMan.Infrastructure.Tests/ C# net10.0 SQLite integration/migration tests
  ChompMan.WinForms.Tests/       C# net10.0-windows UI/component smoke tests
  ChompMan.Migration.Tests/      C# net10.0-windows import/reconciliation tests
docs/
  adr/
  migration-runbook.md
```

All projects enable nullable reference types, implicit usings, warnings as errors in CI, deterministic builds, and analyzers. Package versions are centrally pinned. Use the latest stable matching `10.0.x` versions of `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`, and `Microsoft.EntityFrameworkCore.Tools` available when implementation starts; do not mix EF Core major versions. Use the current stable MSTest/Test SDK versions compatible with .NET 10.

Dependency direction is:

```text
ChompMan.WinForms -> ChompMan.Application -> ChompMan.Core
                            ^
                            |
ChompMan.Infrastructure ----+

ChompMan.LegacyMigration -> ChompMan.Application + ChompMan.Infrastructure
```

`ChompMan.Core` must not reference WinForms, EF Core, SQLite, filesystem, logging providers, or the migration tool. `ChompMan.Application` owns `IScoreRepository`, `ILevelRepository`, `ISettingsRepository`, session orchestration, typed settings, validation, and persistence-degraded behavior. `ChompMan.WinForms` composes concrete services with `Microsoft.Extensions.DependencyInjection` at startup; forms receive interfaces or use cases through constructors.

### 3.2 SQLite data location and lifecycle

- Default database: `%LOCALAPPDATA%\ChompMan\ChompMan.db`.
- Logs: `%LOCALAPPDATA%\ChompMan\Logs` with local retention and no remote telemetry.
- Backups/import reports: `%LOCALAPPDATA%\ChompMan\Backups` and `%LOCALAPPDATA%\ChompMan\MigrationReports`.
- Tests use a unique temporary on-disk SQLite file. Do not use EF Core's in-memory provider because it does not reproduce SQLite constraints or transactions.
- Startup creates the directory, opens SQLite with foreign keys enabled, applies `Database.Migrate()`, validates required seed/version data, and then opens the menu.
- Use WAL mode and a short busy timeout. Create a short-lived `DbContext` per application operation through `IDbContextFactory<ChompManDbContext>`; never hold a context in the game loop.
- Persistence errors are caught at application boundaries, logged, and represented as an unavailable state. Active gameplay continues with validated built-in levels/default settings; save controls explain that the score cannot be persisted.

### 3.3 EF Core model and schema

Use normal EF Core migrations checked into `ChompMan.Infrastructure/Migrations`. The initial schema is:

| Table | Required columns and constraints |
|---|---|
| `Players` | `PlayerId INTEGER PK`, `DisplayName TEXT NOT NULL`, `NormalizedName TEXT NOT NULL`, `CreatedOnUtc TEXT NOT NULL`; check trimmed length 1-100; index normalized name. |
| `HighScores` | `HighScoreId INTEGER PK`, `PlayerId INTEGER NOT NULL FK Players RESTRICT`, `Score INTEGER NOT NULL`, `LevelReached INTEGER NOT NULL`, `PlayedOnUtc TEXT NOT NULL`; checks `Score >= 0`, `LevelReached >= 0`; ranking index on score/time/id. |
| `Levels` | `LevelId INTEGER PK`, `LevelNumber INTEGER NOT NULL UNIQUE`, `MazeLayout TEXT NOT NULL`, `GhostSpeedTicks INTEGER NOT NULL`, `PelletCount INTEGER NOT NULL`; positive/range checks and validated pellet reconciliation. |
| `Settings` | `Key TEXT PK`, `Value TEXT NOT NULL`, `DefinitionVersion INTEGER NOT NULL`; known-key and typed-value validation occurs in the application layer. |
| `LegacyImports` | Import ID, source fingerprint, source path for operator display, started/completed UTC, status, report path; unique source fingerprint prevents accidental replay. |
| `LegacyIdMappings` | Import ID, entity type, legacy ID, new ID; unique import/entity/legacy key supports audit and retry. |

Map UTC values with explicit conversion and invariant round trips. Configure relationships, lengths, indexes, delete behavior, and check constraints in `IEntityTypeConfiguration<T>` classes. Repository reads use `AsNoTracking`; score save and player creation execute in one transaction. Do not concatenate input into SQL.

Seed only settings definitions/defaults and the three built-in levels through an idempotent application seeder keyed by stable natural keys. Do not seed demo players or scores in production. EF model-managed `HasData` is not used for mutable level content.

### 3.4 C# WinForms and double-buffered game surface

Port all five forms to C#: `MainMenuForm`, `GameForm`, `HighScoresForm`, `SettingsForm`, and `GameOverForm`. Keep keyboard operation, visible focus, accessible names, logical tab order, high-DPI support, and non-color status text.

The game board must be a dedicated sealed control similar to:

```csharp
internal sealed class DoubleBufferedGamePanel : Panel
{
    public DoubleBufferedGamePanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
        UpdateStyles();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Color.Black);
    }
}
```

`GameForm` renders only through this panel's paint path. It must not call `CreateGraphics`, render from a background thread, or allocate brushes/fonts every frame. Cache and dispose GDI objects, use `Invalidate()` to request paint, set high-DPI mode before creating forms, and keep input handling on the UI thread.

Replace tick-count timing with a `Stopwatch`-based fixed update step (nominally 60 updates per second) and an accumulator with a bounded catch-up count. A WinForms timer may schedule work, but elapsed monotonic time determines updates. Painting can be delayed or coalesced without changing simulation results. Pause must stop simulation advancement without discarding input or blocking the UI thread.

### 3.5 Legacy data migration

`ChompMan.LegacyMigration` is a C# command-line utility and is never referenced by the game executable. It may use `System.Data.OleDb` and require ACE only on the machine performing a legacy import. It opens the source `.accdb` read-only and never changes, renames, or deletes it.

Command shape:

```powershell
dotnet run --project src/ChompMan.LegacyMigration -- \
  --source "C:\path\ChompMan.accdb" \
  --target "$env:LOCALAPPDATA\ChompMan\ChompMan.db" \
  --report ".\artifacts\migration-report.json"
```

The utility must:

1. Refuse to proceed if source and target resolve to the same file, the source is not readable, the target schema is newer than supported, or a completed source fingerprint already exists without `--verify-only`.
2. Compute a SHA-256 fingerprint and copy the source to a timestamped backup before reading.
3. Read players, scores, levels, and settings with parameterized or fixed commands and explicit field conversions.
4. Validate required fields, UTC meaning, non-negative scores/levels, player references, setting definitions, maze characters/shape/starts, speed ranges, and pellet counts.
5. Import all valid data in one SQLite transaction. Preserve source rows with ID mappings; do not merge case-colliding legacy players. Reject invalid rows with entity, legacy ID, field, and reason.
6. Write JSON and human-readable reports containing source, imported, skipped, failed, and destination counts for every entity, source fingerprint, timestamps, tool/schema versions, and sampled value comparisons.
7. Commit only when there are no unexplained count differences and all foreign keys pass `PRAGMA foreign_key_check`. Otherwise roll back the transaction and leave the existing target intact.
8. Return `0` only for a complete verified import, a distinct documented code for already imported/verified, and nonzero for validation or infrastructure failure.

The migrator must be self-contained-publishable for `win-x64`. After the compatibility window, archive its source and tests with the release record or remove it from the normal solution build; no Access package is shipped with the game.

## 4. Phased Implementation

### Phase 0: Freeze and characterize the legacy product

1. Record the current build and test outputs, package inventory, supported controls, data location, and screenshots of every form at 100% and 150% scaling.
2. Replace every inconclusive scoring test with a real assertion. Add tests for ghost combo values, extra-life boundaries, pause, respawn, final life, mode changes, direction queuing, wrap behavior, and level completion.
3. Add characterization tests for level transitions that assert score/lives carry forward and that missing next-level data produces a win. These tests document corrected requirements rather than the current defects.
4. Add maze-validation tests and capture the three seeded layouts byte-for-byte as fixtures.
5. Create a representative Access fixture containing Unicode/case-colliding names, tied scores, all levels/settings, UTC/boundary dates, and deliberately invalid copies for rejection tests. Never use the only copy of real player data.
6. Create ADRs for WinForms/Windows, SQLite/EF Core, name matching, high-score tie-breaks, and local data location.

**Exit gate:** legacy app remains playable; all non-migration baseline tests pass with zero skipped/inconclusive tests; fixtures and expected reconciliation counts are checked in.

### Phase 1: Port the engine and tests to C# side-by-side

1. Add `src/ChompMan.Core` and `tests/ChompMan.Core.Tests` without removing VB projects.
2. Translate value types, enums, maze, entities, state, and engine behavior file-by-file. Use idiomatic C# while preserving externally observed behavior; do not mechanically retain VB mutability where immutable records/value types are clearer.
3. Inject deterministic random and monotonic-time dependencies. Move session continuity and level transition behavior out of forms into a testable session object.
4. Port each baseline test before deleting its VB counterpart. Compare results against identical maze/input fixtures.
5. Add a headless deterministic scenario test that starts level 1, consumes pellets, transitions levels without losing score/lives, and reaches a terminal win state.

**Exit gate:** C# core tests cover AC-01 and pass independently; the legacy executable still builds and plays; the C# core has no Windows or persistence dependency.

### Phase 2: Add application use cases and typed settings

1. Add application DTOs and async interfaces for scores, levels, and settings with cancellation support.
2. Implement `StartGame`, `LoadLevels`, `CompleteSession`, `SaveScore`, `GetLeaderboard`, `GetSettings`, and `SaveSettings` use cases.
3. Define settings with defaults and ranges: starting lives, player/ghost speed ticks, frightened duration, and supported colors. Validate the complete edit before saving any values.
4. Add a level validator for dimensions, legal characters, exactly one player start, at least one ghost start, reachable required pellets, positive unique level number, speed range, and pellet-count agreement.
5. Implement explicit persistence-unavailable results so menus and active gameplay do not depend on exceptions.

**Exit gate:** use-case tests pass using in-memory fakes, invalid writes are rejected, defaults permit a full headless game without persistence, and no application class references EF Core or WinForms.

### Phase 3: Implement SQLite with EF Core 10

1. Add the context, entities, configurations, design-time factory, initial migration, and idempotent content seeder.
2. Implement repository interfaces with deterministic score ordering, transactional score saves, normalized-name lookup, ordered levels, and atomic settings updates.
3. Apply migrations at startup through a dedicated initializer; report schema errors without exposing connection details.
4. Add temporary-file SQLite integration tests for empty creation, upgrade from every checked-in migration, constraints, foreign keys, ordering/ties, transactions, concurrent reads, UTC round trips, Unicode names, failure rollback, and reopen persistence.
5. Add a test that deletes/locks the database during startup and proves the application can still create an offline game session.

**Exit gate:** a fresh SQLite database is created exclusively by EF migrations, all repository tests pass against real SQLite, and no runtime project references Access APIs.

### Phase 4: Build and verify the C# legacy migrator

1. Implement backup, fingerprinting, read-only extraction, validation, transactional import, ID mapping, reconciliation, reports, verify-only mode, and documented exit codes.
2. Test the representative Access fixture and each invalid fixture. Run twice to prove idempotency/detection. Inject a failure midway and prove the target transaction rolls back.
3. Compare source/destination counts and sampled values, including multiline maze bytes after newline normalization, name casing, score values, player relationships, level numbers, setting values, and UTC instants.
4. Document ACE as a migration-operator-only prerequisite and publish the migration utility separately from the game.

**Exit gate:** AC-05 passes; the source hash is unchanged after every test; a second import cannot duplicate rows; failure leaves the pre-import SQLite target usable.

### Phase 5: Port all WinForms workflows and rendering to C#

1. Add the C# composition root, structured local logging, EF startup initialization, and graceful offline composition.
2. Port menu, leaderboard, settings, game-over, and game forms against application interfaces. Remove repository construction from forms.
3. Add `DoubleBufferedGamePanel` with the exact style flags described above and port GDI+ drawing. Cache/dispose rendering resources and support high-DPI layout without overlapping text.
4. Implement fixed-step monotonic game timing, bounded catch-up, keyboard input, pause/resume, restart, Escape/menu return, score/lives/level HUD, level transitions, win/game-over, save/skip, and persistence-unavailable messages.
5. Apply validated settings to new sessions and rendering. Keep active-session settings stable until restart.
6. Add UI/component tests for form construction, accessible names/tab order, double-buffer style behavior, control bounds at 100%/150%/200%, and persistence failure. Add a manual rendering check for flicker because ordinary unit tests cannot prove perceived smoothness.

**Exit gate:** AC-03, AC-04, and AC-08 pass; a player can complete all levels in the C# application; the board visibly renders without flicker; score/lives persist across levels; no form directly constructs a repository or context.

### Phase 6: Cut over, publish, and remove the legacy runtime

1. Run full Release build/test, format, analyzer, and vulnerable-package checks. Treat warnings and skipped/inconclusive tests as failures.
2. Run the end-to-end manual test matrix on a clean supported Windows 10/11 machine and at common DPI settings.
3. Publish self-contained `win-x64` output and verify it on a machine without .NET, Access, or ACE:

   ```powershell
   dotnet publish src/ChompMan.WinForms/ChompMan.WinForms.csproj `
     -c Release -r win-x64 --self-contained true `
     -p:PublishSingleFile=true -o artifacts/publish/win-x64
   ```

4. Back up and migrate representative legacy data, launch the published build against it, save another score, restart, and verify persistence.
5. Switch `ChompMan.sln` and README commands to C# projects. Remove VB/.NET Framework projects, duplicated source folders, Access runtime code, ACE instructions for players, and old build outputs only after approval of the release candidate.
6. Search source and publish output for `.vb`, `.vbproj`, `net48`, `System.Data.OleDb`, `ADOX`, `ACE.OLEDB`, `.accdb`, and Access packages. Only migration documentation/tooling may mention them during the compatibility window.

**Exit gate:** every item in Section 1 and every release gate in Section 6 passes from a clean checkout and clean machine. The release artifact is the playable game, not a framework scaffold.

## 5. Validation Commands and Evidence

Run these from the repository root in CI and before cutover:

```powershell
dotnet --info
dotnet restore ChompMan.sln --locked-mode
dotnet build ChompMan.sln -c Release --no-restore
dotnet test ChompMan.sln -c Release --no-build --logger "trx;LogFileName=tests.trx"
dotnet format ChompMan.sln --verify-no-changes --no-restore
dotnet list ChompMan.sln package --vulnerable --include-transitive
dotnet ef migrations has-pending-model-changes `
  --project src/ChompMan.Infrastructure `
  --startup-project src/ChompMan.WinForms
dotnet publish src/ChompMan.WinForms/ChompMan.WinForms.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -o artifacts/publish/win-x64
```

CI must fail if TRX results contain `NotExecuted`, `Skipped`, or `Inconclusive`. Store test results, migration reconciliation reports, package audit output, publish manifest/hash, startup timing, and the signed manual-play checklist as release evidence.

The final manual play checklist is:

1. Launch to an interactive keyboard-operable menu in under the agreed startup target.
2. Open and close high scores and settings; reject invalid settings; save valid settings and verify them after restart.
3. Start a game with configured lives/speeds/colors; move with arrows and WASD; confirm walls, tunnels, pellets, power pellets, ghosts, pause/resume, restart, and Escape.
4. Complete at least two level transitions and verify cumulative score and remaining lives do not reset.
5. Reach both game-over and final-win outcomes; test score save and skip; verify deterministic leaderboard order after restart.
6. Repeat with SQLite unavailable and verify playable fallback plus clear save-disabled status.
7. Observe gameplay for at least ten minutes at 100% and 150% scaling; verify the double-buffered panel has no visible flicker, stale frames, input stalls, clipping, overlap, or growing GDI handle count.

## 6. Release Acceptance Gates

The release owner signs every gate; none may be replaced by “compiles on my machine.”

| Gate | Required evidence |
|---|---|
| Behavioral parity | Automated coverage for movement, collision, scoring, extra lives, frightened/combo behavior, respawn, pause, level continuity, final win, and game over. |
| Complete C# conversion | No VB project in the solution or publish graph; clean C# build and test from source. |
| Complete SQLite/EF conversion | EF migrations create/upgrade the real SQLite file; all repositories integration-tested; no Access API in the game dependency graph. |
| Playability | Signed end-to-end checklist and a successful full multi-level session from the published executable. |
| Rendering | `DoubleBufferedGamePanel` styles verified plus manual flicker/DPI/GDI-resource checks. |
| Data safety | Read-only source, backup hash, per-entity reconciliation, sampled fields, foreign-key check, replay detection, and failure rollback evidence. |
| Offline resilience | Deliberately unavailable/locked SQLite does not prevent play and produces actionable UI status. |
| Deployment | Self-contained clean-machine test without .NET, Access, ACE, or COM registration. |
| Quality/security | No skipped/inconclusive tests, build warnings, pending EF model changes, or unresolved high/critical dependency findings. |
| Documentation | README, architecture decisions, data locations, migration runbook, backup, upgrade, rollback, controls, and known limitations are current. |

## 7. Risks and Mitigations

| Risk | Impact | Mitigation and trigger |
|---|---|---|
| Rewrite changes game feel | High | Characterization tests, deterministic inputs/randomness, fixed-step scenarios, side-by-side manual comparison. Stop cutover on any unexplained parity difference. |
| Level-transition defects lose session state | High | Move transitions into tested session orchestration and require multi-level automated/manual evidence. |
| Access type/collation differences alter data | High | Explicit conversion, case-collision fixtures, UTC round trips, ID maps, sampled reconciliation, no silent merges. |
| Invalid legacy data blocks import | Medium | Preflight report every invalid row and import nothing until policy is approved; never silently coerce gameplay data. |
| Import partially corrupts target | High | Backup both files, one SQLite transaction, foreign-key check, atomic replacement, source hash verification. |
| EF schema drift | High | Checked-in migrations, pending-model CI check, upgrade tests from each prior schema, no `EnsureCreated`. |
| SQLite lock or disk failure affects gameplay | Medium | Short-lived contexts, WAL/busy timeout, bounded operations outside game loop, degraded mode and actionable status. |
| Timer/render coupling changes speed | High | `Stopwatch` fixed-step accumulator, bounded catch-up, deterministic delayed-render tests. |
| GDI flicker or resource leak | Medium | Dedicated double-buffered panel, cached/disposed resources, no `CreateGraphics`, long-run GDI handle check. |
| High DPI clips fixed layouts | Medium | DPI-aware startup, layout containers where practical, bounds tests and manual checks at 100/150/200%. |
| .NET 10/EF package mismatch | Medium | Pin matching stable 10.0.x packages and SDK via `global.json`; validate on CI and clean machine. |
| Single-file publish surprises SQLite/native assets | Medium | Publish smoke test, dependency inspection, startup/database-write test from final artifact. |
| Removing legacy too early eliminates fallback | High | Side-by-side phases, immutable tagged legacy release, compatibility window, deletion only in Phase 6. |

## 8. Rollback Strategy

### 8.1 Before cutover

- Keep the legacy branch/tag and known-good installer/build artifact immutable.
- Never overwrite the original `.accdb`; record its size, timestamp, and SHA-256 and create a verified backup.
- Build C# projects side-by-side. A failed phase is rolled back by stopping distribution of that phase and continuing to ship the last playable legacy version; do not delete replacement evidence needed to diagnose it.

### 8.2 During migration

- Put the application in an operator-controlled maintenance state for the brief import so no new score is written to either store.
- Back up any existing `ChompMan.db` before applying EF migrations or importing data.
- Import into a temporary SQLite file such as `ChompMan.db.importing`; run reconciliation, `PRAGMA integrity_check`, and `PRAGMA foreign_key_check`; close all connections; then atomically rename it into place.
- If validation/import fails, delete only the temporary SQLite file, retain reports, and continue using the unchanged Access application/database.
- If replacement of an existing SQLite file fails, restore its timestamped backup and verify its hash/integrity before launch.

### 8.3 After release

- Maintain a defined compatibility window of at least one release cycle during which the legacy executable and untouched `.accdb` backup remain available to the owner. Do not allow both applications to accept writes as peers.
- Trigger rollback for data-count mismatch, integrity failure, repeatable crash/startup failure, unplayable controls/rendering, score/life loss across levels, or a critical regression/security issue.
- Roll back the executable to the tagged legacy build and restore the original `.accdb` copy. Scores created only in SQLite after cutover are not automatically reverse-migrated; export them to a signed CSV/JSON report for later reconciliation rather than writing into Access with an untested reverse converter.
- After a rollback, preserve the failed SQLite database, logs, release version, migration report, and hashes for diagnosis. Never overwrite the last known-good files.
- Retire the legacy artifact and Access backup only after owner approval, the compatibility window, a successful restore drill, and documented retention requirements. Archive migration reports and source hashes with the release record.

## 9. Implementation Order Summary

The critical path is: characterize behavior, port and prove the C# engine, add tested application boundaries, implement real SQLite repositories and EF migrations, prove the read-only Access importer, port every WinForms workflow with the double-buffered panel, publish and play-test, then remove VB and Access from the runtime.

At every point there must be a known playable version. The final handoff is accepted only when the clean published C# executable runs the complete game against EF-managed SQLite data and the migration/rollback evidence is reproducible.