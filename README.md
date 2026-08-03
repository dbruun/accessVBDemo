# ChompMan

A Pac-Man-style arcade game written in **VB.NET / WinForms** (.NET 8),
backed by a **SQL Server or Azure SQL** database through EF Core.

## Screenshots

The game uses pure GDI+ shape-based graphics — no external image assets are required.

```
┌──────────────────────────────────────┐
│ Score: 1,540   Level: 1   Lives: 3   │
├──────────────────────────────────────┤
│  ████████████████████████████████    │
│  █  ·  ·  ·  ·  █  ·  ·  ·  ·  █   │
│  █  ████  ████  █  ████  ████  █   │
│  █  ○                          ○  █   │
│  …  (GDI+ maze)                    │
└──────────────────────────────────────┘
```

## Prerequisites

| Requirement | Details |
|---|---|
| **Visual Studio 2022** (or newer) | Community Edition is free |
| **.NET 8 SDK** | Required to build |
| **SQL Server or Azure SQL** | Required to persist scores, levels, and settings |

## Building

```bash
# Clone / open the repo
git clone <repo-url>
cd accessVBDemo

# Restore packages and build
dotnet restore ChompMan.sln
dotnet build   ChompMan.sln -c Release
```

Or open `ChompMan.sln` in Visual Studio and press **F5**.

Configure the database before running. The application reads the connection
string from `CHOMPMAN_CONNECTION_STRING`; when it is unset, it uses LocalDB:

```powershell
$env:CHOMPMAN_CONNECTION_STRING = "Server=(localdb)\MSSQLLocalDB;Database=ChompMan;Integrated Security=True;TrustServerCertificate=True"
```

## Running

```bash
dotnet run --project ChompMan/ChompMan.vbproj
```

On startup, EF Core applies the bundled SQL Server migration. If the database
is unavailable, the game runs in DB-less mode (scores and settings are not
saved).

## Controls

| Key | Action |
|---|---|
| Arrow keys / WASD | Move ChompMan |
| **P** | Pause / Resume |
| **R** | Restart (from level 1) |
| **Esc** | Quit to main menu |

## Running Tests

```bash
dotnet test ChompMan.Tests/ChompMan.Tests.vbproj -v normal
```

## Project Structure

```
ChompMan.sln
│
├── ChompMan/                   VB.NET WinForms project
│   ├── DataAccess/             EF Core / SQL Server implementation
│   │   ├── ChompManDbContext.vb
│   │   ├── Entities.vb
│   │   ├── EfScoreRepository.vb
│   │   ├── EfLevelRepository.vb
│   │   ├── EfSettingsRepository.vb
│   │   └── Migrations/         EF Core SQL Server migrations
│   │
│   ├── UI/                     WinForms screens
│   │   ├── MainMenuForm.vb
│   │   ├── GameForm.vb
│   │   ├── HighScoresForm.vb
│   │   ├── GameOverForm.vb
│   │   └── SettingsForm.vb
│   │
│   └── Program.vb              Entry point; applies DB migrations
│
├── ChompMan.Core/              Platform-independent contracts and game logic
│   ├── DataAccess/
│   │   ├── Models.vb           DTOs: ScoreEntry, LevelData, SettingEntry
│   │   ├── IScoreRepository.vb
│   │   ├── ILevelRepository.vb
│
│   └── GameEngine/             Pure game logic (no WinForms / DB deps)
│       ├── CellType.vb
│       ├── Direction.vb
│       ├── GameState.vb
│       ├── Maze.vb
│       └── Engine.vb           Main update/tick logic
│
└── ChompMan.Tests/             MSTest unit-test project
    ├── MovementTests.vb
    ├── CollisionTests.vb
    └── ScoringTests.vb
```

## Database Schema

| Table | Key Columns |
|---|---|
| `Players` | PlayerId (PK), Name, CreatedOn |
| `HighScores` | ScoreId (PK), PlayerId (FK), Score, LevelReached, PlayedOn |
| `Levels` | LevelId (PK), LevelNumber, MazeLayout (NVARCHAR(MAX)), GhostSpeed, PelletCount |
| `Settings` | Key (PK), Value |

Maze layouts are stored as multi-line strings using:

| Char | Meaning |
|---|---|
| `#` | Wall |
| `.` | Pellet (10 pts) |
| `o` | Power pellet (50 pts; frightens ghosts) |
| `P` | Player start position |
| `G` | Ghost start position |
| ` ` | Empty corridor |

## Licence

Original artwork and code; no copyrighted Pac-Man assets are used.