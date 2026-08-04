using Microsoft.Data.Sqlite;

namespace ChompMan.DataAccess;

/// <summary><see cref="IScoreRepository"/> implementation backed by SQLite.</summary>
public class SqliteScoreRepository : IScoreRepository
{
    private readonly string _connectionString;

    public SqliteScoreRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
    }

    public List<ScoreEntry> GetTopScores(int count = 10)
    {
        var result = new List<ScoreEntry>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var sql = "SELECT p.Name, h.Score, h.LevelReached, h.PlayedOn " +
                  "FROM HighScores h INNER JOIN Players p ON p.PlayerId = h.PlayerId " +
                  "ORDER BY h.Score DESC LIMIT $count";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("$count", count);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ScoreEntry
            {
                PlayerName = reader.GetString(0),
                Score = reader.GetInt32(1),
                LevelReached = reader.GetInt32(2),
                PlayedOn = DateTime.Parse(reader.GetString(3), null, System.Globalization.DateTimeStyles.RoundtripKind)
            });
        }

        return result;
    }

    public void SaveScore(string playerName, int score, int levelReached)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            throw new ArgumentException("Player name is required.", nameof(playerName));
        }

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var playerId = GetOrCreatePlayer(conn, playerName);
        using var cmd = new SqliteCommand("INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn) VALUES ($playerId, $score, $levelReached, $playedOn)", conn);
        cmd.Parameters.AddWithValue("$playerId", playerId);
        cmd.Parameters.AddWithValue("$score", score);
        cmd.Parameters.AddWithValue("$levelReached", levelReached);
        cmd.Parameters.AddWithValue("$playedOn", DateTime.UtcNow.ToString("O"));
        cmd.ExecuteNonQuery();
    }

    private static long GetOrCreatePlayer(SqliteConnection conn, string name)
    {
        using (var cmd = new SqliteCommand("SELECT PlayerId FROM Players WHERE Name = $name", conn))
        {
            cmd.Parameters.AddWithValue("$name", name);
            var val = cmd.ExecuteScalar();
            if (val is not null and not DBNull)
            {
                return (long)val;
            }
        }

        using (var cmd = new SqliteCommand("INSERT INTO Players (Name, CreatedOn) VALUES ($name, $createdOn)", conn))
        {
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$createdOn", DateTime.UtcNow.ToString("O"));
            cmd.ExecuteNonQuery();
        }

        using (var idCmd = new SqliteCommand("SELECT last_insert_rowid()", conn))
        {
            return (long)idCmd.ExecuteScalar();
        }
    }
}
