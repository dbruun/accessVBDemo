# ChompMan .NET 8 and SQL Server Upgrade

ChompMan now targets .NET 8 and persists data through EF Core with SQL Server
or Azure SQL. The Access/OleDb repositories and `.accdb` initializer have been
removed.

## Database setup

At startup the application applies `DataAccess/Migrations/InitialCreate` using
the connection string in `CHOMPMAN_CONNECTION_STRING`. If the variable is not
set, ChompMan uses a LocalDB connection:

```text
Server=(localdb)\MSSQLLocalDB;Database=ChompMan;Integrated Security=True;TrustServerCertificate=True
```

For Azure SQL, set `CHOMPMAN_CONNECTION_STRING` to the connection string
provided by Azure. Prefer managed identity or Azure AD authentication in
production rather than embedding SQL credentials in source code.

## Persistence implementation

- `ChompManDbContext` maps Players, HighScores, Levels, and Settings.
- `EfScoreRepository`, `EfLevelRepository`, and `EfSettingsRepository` replace
  the Access repositories while retaining the score and level repository
  interfaces.
- `ChompMan/DbSetup.sql` is a SQL Server reference script; the EF migration is
  the authoritative schema.

The WinForms UI and the platform-independent game engine remain unchanged in
their responsibilities.
