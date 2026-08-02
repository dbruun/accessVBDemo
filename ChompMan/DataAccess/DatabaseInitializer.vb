Option Strict Off  ' Required for ADOX COM late-binding

Imports System
Imports System.Data.OleDb
Imports System.IO

Namespace DataAccess

    ''' <summary>
    ''' Creates and seeds the ChompMan.accdb Access database when it does not
    ''' already exist.  Uses the ADOX COM object (part of the ACE runtime) to
    ''' create the file, then DDL + DML to build the schema and seed data.
    ''' </summary>
    Public Class DatabaseInitializer

        Private ReadOnly _dbPath As String
        Private ReadOnly _connectionString As String

        ''' <summary>Creates the initialiser pointing at <paramref name="dbPath"/>.</summary>
        Public Sub New(dbPath As String)
            _dbPath = dbPath
            _connectionString = $"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbPath};"
        End Sub

        ''' <summary>
        ''' Ensures the database exists and contains the expected schema and seed
        ''' data.  Safe to call on every application start.
        ''' </summary>
        Public Sub EnsureCreated()
            If Not File.Exists(_dbPath) Then
                CreateDatabase()
                CreateSchema()
                SeedData()
            End If
        End Sub

        ' ── Private helpers ──────────────────────────────────────────────────

        ''' <summary>Creates an empty .accdb file using the ADOX Catalog COM object.</summary>
        Private Sub CreateDatabase()
            ' Late-bind ADOX so we don't need a COM reference at compile time.
            ' Requires the ACE OLEDB 16.0 runtime to be installed.
            Dim catalog As Object = Activator.CreateInstance(Type.GetTypeFromProgID("ADOX.Catalog"))
            catalog.Create(_connectionString)
            ' Release COM object
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(catalog)
        End Sub

        Private Sub CreateSchema()
            Using conn As New OleDbConnection(_connectionString)
                conn.Open()

                Execute(conn,
                    "CREATE TABLE Players (" &
                    "  PlayerId AUTOINCREMENT PRIMARY KEY," &
                    "  Name TEXT(100) NOT NULL," &
                    "  CreatedOn DATETIME NOT NULL" &
                    ")")

                Execute(conn,
                    "CREATE TABLE HighScores (" &
                    "  ScoreId AUTOINCREMENT PRIMARY KEY," &
                    "  PlayerId INTEGER NOT NULL," &
                    "  Score INTEGER NOT NULL," &
                    "  LevelReached INTEGER NOT NULL," &
                    "  PlayedOn DATETIME NOT NULL" &
                    ")")

                Execute(conn,
                    "CREATE TABLE Levels (" &
                    "  LevelId AUTOINCREMENT PRIMARY KEY," &
                    "  LevelNumber INTEGER NOT NULL," &
                    "  MazeLayout MEMO NOT NULL," &
                    "  GhostSpeed INTEGER NOT NULL," &
                    "  PelletCount INTEGER NOT NULL" &
                    ")")

                Execute(conn,
                    "CREATE TABLE Settings (" &
                    "  [Key] TEXT(100) NOT NULL PRIMARY KEY," &
                    "  Value TEXT(255) NOT NULL" &
                    ")")
            End Using
        End Sub

        Private Sub SeedData()
            Using conn As New OleDbConnection(_connectionString)
                conn.Open()

                ' ── Settings ──────────────────────────────────────────────────
                InsertSetting(conn, "StartingLives", "3")
                InsertSetting(conn, "PlayerSpeedTicks", "4")
                InsertSetting(conn, "GhostSpeedTicks", "6")
                InsertSetting(conn, "FrightenedDuration", "180")
                InsertSetting(conn, "PlayerColour", "Yellow")
                InsertSetting(conn, "WallColour", "DarkBlue")
                InsertSetting(conn, "PelletColour", "White")
                InsertSetting(conn, "PowerPelletColour", "Orange")

                ' ── Sample players ────────────────────────────────────────────
                Dim p1 = InsertPlayer(conn, "ACE")
                Dim p2 = InsertPlayer(conn, "DEMO")

                InsertScore(conn, p1, 15200, 3)
                InsertScore(conn, p2, 8400, 2)

                ' ── Levels ────────────────────────────────────────────────────
                InsertLevel(conn, 1, Maze1Layout, 6)
                InsertLevel(conn, 2, Maze2Layout, 5)
                InsertLevel(conn, 3, Maze3Layout, 4)
            End Using
        End Sub

        ' ── DML helpers ──────────────────────────────────────────────────────

        Private Shared Sub Execute(conn As OleDbConnection, sql As String)
            Using cmd As New OleDbCommand(sql, conn)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Private Shared Sub InsertSetting(conn As OleDbConnection, key As String, value As String)
            Using cmd As New OleDbCommand("INSERT INTO Settings ([Key], Value) VALUES (?, ?)", conn)
                cmd.Parameters.AddWithValue("@Key", key)
                cmd.Parameters.AddWithValue("@Value", value)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Private Shared Function InsertPlayer(conn As OleDbConnection, name As String) As Integer
            Using cmd As New OleDbCommand("INSERT INTO Players (Name, CreatedOn) VALUES (?, ?)", conn)
                cmd.Parameters.AddWithValue("@Name", name)
                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.UtcNow)
                cmd.ExecuteNonQuery()
            End Using
            Using cmd As New OleDbCommand("SELECT @@IDENTITY", conn)
                Return CInt(cmd.ExecuteScalar())
            End Using
        End Function

        Private Shared Sub InsertScore(conn As OleDbConnection, playerId As Integer,
                                       score As Integer, levelReached As Integer)
            Using cmd As New OleDbCommand(
                "INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn) VALUES (?, ?, ?, ?)",
                conn)
                cmd.Parameters.AddWithValue("@PlayerId", playerId)
                cmd.Parameters.AddWithValue("@Score", score)
                cmd.Parameters.AddWithValue("@LevelReached", levelReached)
                cmd.Parameters.AddWithValue("@PlayedOn", DateTime.UtcNow)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Private Shared Sub InsertLevel(conn As OleDbConnection, levelNumber As Integer,
                                       layout As String, ghostSpeed As Integer)
            ' Count pellets in the layout
            Dim pellets = 0
            For Each ch In layout
                If ch = "."c OrElse ch = "o"c Then pellets += 1
            Next

            Using cmd As New OleDbCommand(
                "INSERT INTO Levels (LevelNumber, MazeLayout, GhostSpeed, PelletCount) " &
                "VALUES (?, ?, ?, ?)", conn)
                cmd.Parameters.AddWithValue("@LevelNumber", levelNumber)
                cmd.Parameters.AddWithValue("@MazeLayout", layout)
                cmd.Parameters.AddWithValue("@GhostSpeed", ghostSpeed)
                cmd.Parameters.AddWithValue("@PelletCount", pellets)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        ' ── Maze layouts ─────────────────────────────────────────────────────
        '
        ' Legend:
        '   #  wall         .  pellet       o  power-pellet
        '   P  player-start  G  ghost-start  -  ghost-house-door
        '    (space) empty corridor

        Private Shared ReadOnly Property Maze1Layout As String
            Get
                Dim layout = "#####################" & vbLf &
                             "#.........#.........#" & vbLf &
                             "#.###.###.#.###.###.#" & vbLf &
                             "#o###.###.#.###.###o#" & vbLf &
                             "#...................#" & vbLf &
                             "#.###.#.#####.#.###.#" & vbLf &
                             "#.....#...#...#.....#" & vbLf &
                             "#####.###.#.###.#####" & vbLf &
                             "#####.#.GGG.#.#######" & vbLf &
                             "#####.#.....#.#######" & vbLf &
                             "#####.#.....#.#######" & vbLf &
                             "#####.#########.#####" & vbLf &
                             "#####.#.....#.#######" & vbLf &
                             "#####.###.#.###.#####" & vbLf &
                             "#.....#...#...#.....#" & vbLf &
                             "#.###.#.#####.#.###.#" & vbLf &
                             "#...................#" & vbLf &
                             "#o###.###.P.###.###o#" & vbLf &
                             "#.###.###.#.###.###.#" & vbLf &
                             "#.........#.........#" & vbLf &
                             "#####################"
                Return layout
            End Get
        End Property

        Private Shared ReadOnly Property Maze2Layout As String
            Get
                Dim layout = "#####################" & vbLf &
                             "#o.......#.......o..#" & vbLf &
                             "#.#####.###.#####.#.#" & vbLf &
                             "#.#...........#...#.#" & vbLf &
                             "#.#.###.###.###.#.#.#" & vbLf &
                             "#...#.......#...#...#" & vbLf &
                             "#####.#####.#####.###" & vbLf &
                             "    #.#  G  #.#      " & vbLf &
                             "#####.# ### #.#######" & vbLf &
                             "      .     .        " & vbLf &
                             "#####.# ### #.#######" & vbLf &
                             "    #.#  G  #.#      " & vbLf &
                             "#####.#####.#####.###" & vbLf &
                             "#...#.......#...#...#" & vbLf &
                             "#.#.###.###.###.#.#.#" & vbLf &
                             "#.#...........#...#.#" & vbLf &
                             "#.#####.###.#####.#.#" & vbLf &
                             "#o.......P.......o..#" & vbLf &
                             "#####################"
                Return layout
            End Get
        End Property

        Private Shared ReadOnly Property Maze3Layout As String
            Get
                Dim layout = "#####################" & vbLf &
                             "#o...#.......#...o..#" & vbLf &
                             "#.##.#.#####.#.##.#.#" & vbLf &
                             "#.##...#.G.#...##.#.#" & vbLf &
                             "#......#...#......#.#" & vbLf &
                             "###.##.#####.##.#####" & vbLf &
                             "#...##.......##.....#" & vbLf &
                             "#.####.#####.####.#.#" & vbLf &
                             "#.#  #.# G #.#  #.#.#" & vbLf &
                             "#.#  #.#   #.#  #.#.#" & vbLf &
                             "#.####.#####.####.#.#" & vbLf &
                             "#...##.......##.....#" & vbLf &
                             "###.##.#####.##.#####" & vbLf &
                             "#......#...#......#.#" & vbLf &
                             "#.##...#...#...##.#.#" & vbLf &
                             "#.##.#.#####.#.##.#.#" & vbLf &
                             "#o...#....P..#...o..#" & vbLf &
                             "#####################"
                Return layout
            End Get
        End Property

    End Class

End Namespace
