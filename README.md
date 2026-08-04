# ChompMan

A Pac-Man-style arcade game written in **C# / WinForms** on **.NET 8**, backed by a local **SQLite** database.

## Screenshots

The game uses pure GDI+ shape-based graphics — no external image assets are required.

## Building

```bash
dotnet restore ChompMan.sln
dotnet build ChompMan.sln -c Release
```

## Running

```bash
dotnet run --project ChompMan/ChompMan.csproj
```

On first startup, the app auto-creates `ChompMan.db` beside the executable and seeds it with sample levels, settings, and scores. No external database runtime is required.

## Controls

| Key | Action |
|---|---|
| Arrow keys / WASD | Move ChompMan |
| P | Pause / Resume |
| R | Restart |
| Esc | Quit to main menu |

## Running Tests

```bash
dotnet test ChompMan.Tests/ChompMan.Tests.csproj
```

## Project Structure

- `ChompMan.Core/` — net8.0 C# class library with `ChompMan.GameEngine` and shared `ChompMan.DataAccess` contracts/models
- `ChompMan/` — net8.0-windows C# WinForms app using `Microsoft.Data.Sqlite`
- `ChompMan.Tests/` — net8.0 MSTest project covering game-engine behavior

## Database

SQLite tables preserved from the original design:
- `Players`
- `HighScores`
- `Levels`
- `Settings`

`ChompMan/DbSetup.sql` documents the SQLite schema.
