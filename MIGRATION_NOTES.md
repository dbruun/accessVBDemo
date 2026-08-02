# Migration Notes — ChompMan to .NET 8 + SQL Server / Azure SQL + EF Core

This document describes how the current design was intentionally structured to
support a future migration away from .NET Framework 4.8, Microsoft Access, and
raw ADO.NET towards **.NET 8**, **SQL Server / Azure SQL**, and **EF Core**.

---

## 1. Why the Current Design is Migration-Friendly

### 1.1 GameEngine is WinForms/DB-free

`ChompMan.GameEngine` contains zero references to `System.Windows.Forms` or
`System.Data`.  All game logic (movement, collision, scoring, AI) is expressed
as plain VB.NET classes and structures (`Engine`, `GameState`, `Maze`, etc.).

**Migration impact:** The entire `GameEngine` folder copies verbatim into any
future target — .NET 8, MAUI, Blazor WASM, etc.  No changes needed.

### 1.2 DataAccess is behind interfaces

`IScoreRepository` and `ILevelRepository` define the contract consumed by the
rest of the application.  `AccessScoreRepository` and `AccessLevelRepository`
are the only classes that know about OleDb/Access.

**Migration path:** Add new implementations alongside the old ones, wire them
in `Program.vb`, then delete the Access implementations:

```
DataAccess/
  IScoreRepository.vb          ← keep
  ILevelRepository.vb          ← keep
  AccessScoreRepository.vb     ← delete after migration
  AccessLevelRepository.vb     ← delete after migration
  EfScoreRepository.vb         ← new (EF Core)
  EfLevelRepository.vb         ← new (EF Core)
```

### 1.3 Dependency injection is trivially addable

`Program.vb` currently instantiates repositories directly.  The code is already
structured so a DI container (Microsoft.Extensions.DependencyInjection) can be
dropped in with no changes to the interfaces or the UI layer.

---

## 2. Step-by-Step Migration Guide

### Step 1 — Retarget the project

Change `ChompMan.vbproj`:

```xml
<!-- Before -->
<TargetFramework>net48</TargetFramework>

<!-- After -->
<TargetFramework>net8.0-windows</TargetFramework>
<UseWindowsForms>true</UseWindowsForms>
```

WinForms is fully supported on .NET 8 Windows.

### Step 2 — Replace OleDb with EF Core

Add EF Core and the SQL Server provider:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

Create a `DbContext`:

```vb
' DataAccess/ChompManDbContext.vb
Public Class ChompManDbContext
    Inherits DbContext

    Public Property Players As DbSet(Of PlayerEntity)
    Public Property HighScores As DbSet(Of HighScoreEntity)
    Public Property Levels As DbSet(Of LevelEntity)
    Public Property Settings As DbSet(Of SettingEntity)

    Protected Overrides Sub OnConfiguring(optionsBuilder As DbContextOptionsBuilder)
        optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings("ChompMan").ConnectionString)
    End Sub
End Class
```

### Step 3 — Implement the EF Core repositories

```vb
' DataAccess/EfScoreRepository.vb
Public Class EfScoreRepository
    Implements IScoreRepository
    ' ... uses ChompManDbContext instead of OleDbConnection
End Class
```

### Step 4 — Generate the SQL Server schema

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Or generate a script for DBA review:

```bash
dotnet ef migrations script --output ChompMan/DbSetup_SqlServer.sql
```

### Step 5 — Swap the repository registrations

In `Program.vb` replace:

```vb
Dim scoreRepo As IScoreRepository = New AccessScoreRepository(DbPath)
```

with:

```vb
Dim scoreRepo As IScoreRepository = New EfScoreRepository(context)
```

### Step 6 — Remove legacy dependencies

Delete:

- `AccessScoreRepository.vb`, `AccessLevelRepository.vb`,
  `AccessSettingsRepository.vb`, `DatabaseInitializer.vb`
- `ChompMan.accdb`
- Any `System.Data.OleDb` references

---

## 3. Azure SQL Considerations

- **Connection string:** Replace `(localdb)\mssqllocaldb` with the Azure SQL
  connection string from Azure Portal.
- **Authentication:** Use Managed Identity or a connection string with
  `Authentication=Active Directory Default`.
- **Migrations:** Run `dotnet ef database update` or include a startup call to
  `context.Database.Migrate()` for zero-downtime deploys.

---

## 4. What Stays the Same

| Component | Status |
|---|---|
| GameEngine (`Engine`, `Maze`, `Player`, `Ghost`, …) | **No changes needed** |
| `IScoreRepository` / `ILevelRepository` interfaces | **No changes needed** |
| WinForms UI layer | **No changes needed** |
| MSTest unit tests | **No changes needed** |
| Maze layout format (text strings) | **No changes needed** — same strings move to SQL Server NVARCHAR(MAX) |

---

## 5. Modernisation Roadmap (Optional)

| Phase | Change | Benefit |
|---|---|---|
| 1 | Retarget to `net8.0-windows`, keep WinForms | Latest runtime, security patches |
| 2 | Swap Access → SQL Server via EF Core | Scalable, production-grade storage |
| 3 | Add `Microsoft.Extensions.DependencyInjection` | Cleaner wiring, testability |
| 4 | Replace WinForms with WPF or MAUI | Richer UI, cross-platform (MAUI) |
| 5 | Extract `GameEngine` to a NuGet package | Reuse across different frontends |
