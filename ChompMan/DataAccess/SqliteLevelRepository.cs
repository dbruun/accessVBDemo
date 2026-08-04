using Microsoft.Data.Sqlite;

namespace ChompMan.DataAccess;

/// <summary><see cref="ILevelRepository"/> implementation backed by SQLite.</summary>
public class SqliteLevelRepository : ILevelRepository
{
    private readonly string _connectionString;

    public SqliteLevelRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
    }

    public LevelData GetLevel(int levelNumber)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = new SqliteCommand("SELECT LevelNumber, MazeLayout, GhostSpeed, PelletCount FROM Levels WHERE LevelNumber = $levelNumber", conn);
        cmd.Parameters.AddWithValue("$levelNumber", levelNumber);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? ReadLevel(reader) : null;
    }

    public List<LevelData> GetAllLevels()
    {
        var result = new List<LevelData>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = new SqliteCommand("SELECT LevelNumber, MazeLayout, GhostSpeed, PelletCount FROM Levels ORDER BY LevelNumber", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(ReadLevel(reader));
        }

        return result;
    }

    private static LevelData ReadLevel(SqliteDataReader reader)
    {
        return new LevelData
        {
            LevelNumber = reader.GetInt32(0),
            MazeLayout = reader.GetString(1),
            GhostSpeed = reader.GetInt32(2),
            PelletCount = reader.GetInt32(3)
        };
    }
}
