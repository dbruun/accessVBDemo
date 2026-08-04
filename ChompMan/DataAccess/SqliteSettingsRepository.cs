using Microsoft.Data.Sqlite;

namespace ChompMan.DataAccess;

/// <summary>Repository for the Settings table (key/value tunables).</summary>
public class SqliteSettingsRepository
{
    private readonly string _connectionString;

    public SqliteSettingsRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
    }

    public List<SettingEntry> GetAll()
    {
        var result = new List<SettingEntry>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = new SqliteCommand("SELECT [Key], Value FROM Settings ORDER BY [Key]", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new SettingEntry
            {
                Key = reader.GetString(0),
                Value = reader.GetString(1)
            });
        }

        return result;
    }

    public string GetValue(string key, string defaultValue = "")
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = new SqliteCommand("SELECT Value FROM Settings WHERE [Key] = $key", conn);
        cmd.Parameters.AddWithValue("$key", key);
        var val = cmd.ExecuteScalar();
        return val is not null and not DBNull ? (string)val : defaultValue;
    }

    public void Upsert(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = new SqliteCommand("INSERT INTO Settings ([Key], Value) VALUES ($key, $value) ON CONFLICT([Key]) DO UPDATE SET Value = excluded.Value", conn);
        cmd.Parameters.AddWithValue("$key", key);
        cmd.Parameters.AddWithValue("$value", value);
        cmd.ExecuteNonQuery();
    }
}
