using Microsoft.Data.Sqlite;

namespace ChompMan.DataAccess;

/// <summary>Creates and seeds the ChompMan SQLite database when it does not already exist.</summary>
public class DatabaseInitializer
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DatabaseInitializer(string dbPath)
    {
        _dbPath = dbPath;
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
    }

    public void EnsureCreated()
    {
        if (!File.Exists(_dbPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath) ?? AppContext.BaseDirectory);
            using (File.Create(_dbPath)) { }
            CreateSchema();
            SeedData();
        }
    }

    private void CreateSchema()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        Execute(conn, "PRAGMA foreign_keys = ON;");
        Execute(conn, "CREATE TABLE Players (PlayerId INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, CreatedOn TEXT NOT NULL)");
        Execute(conn, "CREATE TABLE HighScores (ScoreId INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId INTEGER NOT NULL, Score INTEGER NOT NULL, LevelReached INTEGER NOT NULL, PlayedOn TEXT NOT NULL, FOREIGN KEY(PlayerId) REFERENCES Players(PlayerId))");
        Execute(conn, "CREATE TABLE Levels (LevelId INTEGER PRIMARY KEY AUTOINCREMENT, LevelNumber INTEGER NOT NULL, MazeLayout TEXT NOT NULL, GhostSpeed INTEGER NOT NULL, PelletCount INTEGER NOT NULL)");
        Execute(conn, "CREATE TABLE Settings ([Key] TEXT NOT NULL PRIMARY KEY, Value TEXT NOT NULL)");
    }

    private void SeedData()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        InsertSetting(conn, "StartingLives", "3");
        InsertSetting(conn, "PlayerSpeedTicks", "4");
        InsertSetting(conn, "GhostSpeedTicks", "6");
        InsertSetting(conn, "FrightenedDuration", "180");
        InsertSetting(conn, "PlayerColour", "Yellow");
        InsertSetting(conn, "WallColour", "DarkBlue");
        InsertSetting(conn, "PelletColour", "White");
        InsertSetting(conn, "PowerPelletColour", "Orange");

        var p1 = InsertPlayer(conn, "ACE");
        var p2 = InsertPlayer(conn, "DEMO");
        InsertScore(conn, p1, 15200, 3);
        InsertScore(conn, p2, 8400, 2);
        InsertLevel(conn, 1, Maze1Layout, 6);
        InsertLevel(conn, 2, Maze2Layout, 5);
        InsertLevel(conn, 3, Maze3Layout, 4);
    }

    private static void Execute(SqliteConnection conn, string sql)
    {
        using var cmd = new SqliteCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static void InsertSetting(SqliteConnection conn, string key, string value)
    {
        using var cmd = new SqliteCommand("INSERT INTO Settings ([Key], Value) VALUES ($key, $value)", conn);
        cmd.Parameters.AddWithValue("$key", key);
        cmd.Parameters.AddWithValue("$value", value);
        cmd.ExecuteNonQuery();
    }

    private static long InsertPlayer(SqliteConnection conn, string name)
    {
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

    private static void InsertScore(SqliteConnection conn, long playerId, int score, int levelReached)
    {
        using var cmd = new SqliteCommand("INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn) VALUES ($playerId, $score, $levelReached, $playedOn)", conn);
        cmd.Parameters.AddWithValue("$playerId", playerId);
        cmd.Parameters.AddWithValue("$score", score);
        cmd.Parameters.AddWithValue("$levelReached", levelReached);
        cmd.Parameters.AddWithValue("$playedOn", DateTime.UtcNow.ToString("O"));
        cmd.ExecuteNonQuery();
    }

    private static void InsertLevel(SqliteConnection conn, int levelNumber, string layout, int ghostSpeed)
    {
        var pellets = 0;
        foreach (var ch in layout)
        {
            if (ch == '.' || ch == 'o')
            {
                pellets += 1;
            }
        }

        using var cmd = new SqliteCommand("INSERT INTO Levels (LevelNumber, MazeLayout, GhostSpeed, PelletCount) VALUES ($levelNumber, $layout, $ghostSpeed, $pellets)", conn);
        cmd.Parameters.AddWithValue("$levelNumber", levelNumber);
        cmd.Parameters.AddWithValue("$layout", layout);
        cmd.Parameters.AddWithValue("$ghostSpeed", ghostSpeed);
        cmd.Parameters.AddWithValue("$pellets", pellets);
        cmd.ExecuteNonQuery();
    }

    private static string Maze1Layout => "#####################\n" +
        "#.........#.........#\n" +
        "#.###.###.#.###.###.#\n" +
        "#o###.###.#.###.###o#\n" +
        "#...................#\n" +
        "#.###.#.#####.#.###.#\n" +
        "#.....#...#...#.....#\n" +
        "#####.###.#.###.#####\n" +
        "#####.#.GGG.#.#######\n" +
        "#####.#.....#.#######\n" +
        "#####.#.....#.#######\n" +
        "#####.#########.#####\n" +
        "#####.#.....#.#######\n" +
        "#####.###.#.###.#####\n" +
        "#.....#...#...#.....#\n" +
        "#.###.#.#####.#.###.#\n" +
        "#...................#\n" +
        "#o###.###.P.###.###o#\n" +
        "#.###.###.#.###.###.#\n" +
        "#.........#.........#\n" +
        "#####################";

    private static string Maze2Layout => "#####################\n" +
        "#o.......#.......o..#\n" +
        "#.#####.###.#####.#.#\n" +
        "#.#...........#...#.#\n" +
        "#.#.###.###.###.#.#.#\n" +
        "#...#.......#...#...#\n" +
        "#####.#####.#####.###\n" +
        "    #.#  G  #.#      \n" +
        "#####.# ### #.#######\n" +
        "      .     .        \n" +
        "#####.# ### #.#######\n" +
        "    #.#  G  #.#      \n" +
        "#####.#####.#####.###\n" +
        "#...#.......#...#...#\n" +
        "#.#.###.###.###.#.#.#\n" +
        "#.#...........#...#.#\n" +
        "#.#####.###.#####.#.#\n" +
        "#o.......P.......o..#\n" +
        "#####################";

    private static string Maze3Layout => "#####################\n" +
        "#o...#.......#...o..#\n" +
        "#.##.#.#####.#.##.#.#\n" +
        "#.##...#.G.#...##.#.#\n" +
        "#......#...#......#.#\n" +
        "###.##.#####.##.#####\n" +
        "#...##.......##.....#\n" +
        "#.####.#####.####.#.#\n" +
        "#.#  #.# G #.#  #.#.#\n" +
        "#.#  #.#   #.#  #.#.#\n" +
        "#.####.#####.####.#.#\n" +
        "#...##.......##.....#\n" +
        "###.##.#####.##.#####\n" +
        "#......#...#......#.#\n" +
        "#.##...#...#...##.#.#\n" +
        "#.##.#.#####.#.##.#.#\n" +
        "#o...#....P..#...o..#\n" +
        "#####################";
}
