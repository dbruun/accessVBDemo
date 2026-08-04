# Migration Notes — Completed: ChompMan on .NET 8 + C# + SQLite

This migration has now been completed. The repository no longer targets VB.NET/.NET Framework or Microsoft Access.

## Current State

- All application, core, and test code has been ported to **C#**.
- Projects now target **.NET 8**.
- The WinForms app targets `net8.0-windows` with `EnableWindowsTargeting=true` so it can still be built from Linux with the .NET 8 SDK.
- Data storage now uses **SQLite** via **Microsoft.Data.Sqlite** and plain ADO.NET.
- The database file is `ChompMan.db`, created and seeded automatically on first run.

## Resulting Project Layout

- `ChompMan.Core/ChompMan.Core.csproj` — core game logic and shared data contracts
- `ChompMan/ChompMan.csproj` — WinForms application and SQLite repositories
- `ChompMan.Tests/ChompMan.Tests.csproj` — MSTest unit tests

## Database Notes

The Access/OleDb/ADOX implementation was replaced with SQLite equivalents:
- `SqliteScoreRepository`
- `SqliteLevelRepository`
- `SqliteSettingsRepository`
- `DatabaseInitializer` now creates and seeds SQLite schema directly

No ACE OLEDB runtime or Access components are required.

## Build and Test

```bash
dotnet build ChompMan.sln
dotnet test ChompMan.Tests/ChompMan.Tests.csproj
```
