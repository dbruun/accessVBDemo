# ChompMan

A Pac-Man-style arcade game written in **VB.NET / WinForms** (.NET Framework 4.8),
backed by a **Microsoft Access (.accdb)** database.

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
| **.NET Framework 4.8** | Pre-installed on Windows 10/11 |
| **Microsoft ACE OLEDB 16.0 (64-bit)** | Part of Microsoft Access Database Engine 2016 Redistributable — see note below |

### ACE OLEDB Runtime

The game reads/writes the `.accdb` database via the ACE OLEDB 16.0 provider.
Download the 64-bit redistributable from Microsoft:

> **Microsoft Access Database Engine 2016 Redistributable**  
> <https://www.microsoft.com/en-us/download/details.aspx?id=54920>

Choose `AccessDatabaseEngine_X64.exe` (64-bit). If you have 32-bit Microsoft Office
installed you may need the 32-bit version and must also compile ChompMan as x86.

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

## Running

```bash
dotnet run --project ChompMan/ChompMan.vbproj
```

The first time the game starts it will create `ChompMan.accdb` in the same
directory as the executable and seed it with sample data.  If the ACE runtime
is not installed a warning is shown and the game runs in DB-less mode (scores
are not saved).

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
│   ├── GameEngine/             Pure game logic (no WinForms / DB deps)
│   │   ├── CellType.vb
│   │   ├── Direction.vb
│   │   ├── GamePhase.vb
│   │   ├── GhostMode.vb
│   │   ├── Position.vb
│   │   ├── Maze.vb
│   │   ├── GameEntities.vb     Player, Ghost
│   │   ├── GameState.vb        LevelDefinition, GameState
│   │   └── Engine.vb           Main update/tick logic
│   │
│   ├── DataAccess/             ADO.NET / Access layer
│   │   ├── Models.vb           DTOs: ScoreEntry, LevelData, SettingEntry
│   │   ├── IScoreRepository.vb
│   │   ├── ILevelRepository.vb
│   │   ├── AccessScoreRepository.vb
│   │   ├── AccessLevelRepository.vb
│   │   ├── AccessSettingsRepository.vb
│   │   ├── DatabaseInitializer.vb
│   │   └── DbSetup.sql         Reference DDL script
│   │
│   ├── UI/                     WinForms screens
│   │   ├── MainMenuForm.vb
│   │   ├── GameForm.vb
│   │   ├── HighScoresForm.vb
│   │   ├── GameOverForm.vb
│   │   └── SettingsForm.vb
│   │
│   └── Program.vb              Entry point; initialises DB
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
| `Levels` | LevelId (PK), LevelNumber, MazeLayout (memo), GhostSpeed, PelletCount |
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