Imports System
Imports System.Collections.Generic
Imports System.Data.OleDb

Namespace DataAccess

    ''' <summary>
    ''' <see cref="IScoreRepository"/> implementation backed by a Microsoft Access
    ''' (.accdb) database via ACE OLEDB 16.0.
    ''' </summary>
    Public Class AccessScoreRepository
        Implements IScoreRepository

        Private ReadOnly _connectionString As String

        ''' <summary>
        ''' Creates the repository pointing at <paramref name="dbPath"/>.
        ''' </summary>
        Public Sub New(dbPath As String)
            _connectionString = $"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbPath};"
        End Sub

        ''' <inheritdoc/>
        Public Function GetTopScores(Optional count As Integer = 10) As List(Of ScoreEntry) _
            Implements IScoreRepository.GetTopScores

            Dim result As New List(Of ScoreEntry)()
            Using conn As New OleDbConnection(_connectionString)
                conn.Open()
                Dim sql = "SELECT TOP " & count & " p.Name, h.Score, h.LevelReached, h.PlayedOn " &
                          "FROM HighScores h INNER JOIN Players p ON p.PlayerId = h.PlayerId " &
                          "ORDER BY h.Score DESC"
                Using cmd As New OleDbCommand(sql, conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New ScoreEntry() With {
                                .PlayerName = reader.GetString(0),
                                .Score = reader.GetInt32(1),
                                .LevelReached = reader.GetInt32(2),
                                .PlayedOn = reader.GetDateTime(3)
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <inheritdoc/>
        Public Sub SaveScore(playerName As String, score As Integer, levelReached As Integer) _
            Implements IScoreRepository.SaveScore

            If String.IsNullOrWhiteSpace(playerName) Then Throw New ArgumentException("Player name is required.", NameOf(playerName))

            Using conn As New OleDbConnection(_connectionString)
                conn.Open()

                ' Upsert player
                Dim playerId As Integer = GetOrCreatePlayer(conn, playerName)

                ' Insert score
                Dim insertSql = "INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn) " &
                                "VALUES (?, ?, ?, ?)"
                Using cmd As New OleDbCommand(insertSql, conn)
                    cmd.Parameters.AddWithValue("@PlayerId", playerId)
                    cmd.Parameters.AddWithValue("@Score", score)
                    cmd.Parameters.AddWithValue("@LevelReached", levelReached)
                    cmd.Parameters.AddWithValue("@PlayedOn", DateTime.UtcNow)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ' ── Private helpers ──────────────────────────────────────────────────

        Private Shared Function GetOrCreatePlayer(conn As OleDbConnection, name As String) As Integer
            ' Try to find existing player
            Using cmd As New OleDbCommand("SELECT PlayerId FROM Players WHERE Name = ?", conn)
                cmd.Parameters.AddWithValue("@Name", name)
                Dim val = cmd.ExecuteScalar()
                If val IsNot Nothing AndAlso val IsNot DBNull.Value Then
                    Return CInt(val)
                End If
            End Using

            ' Create new player
            Using cmd As New OleDbCommand("INSERT INTO Players (Name, CreatedOn) VALUES (?, ?)", conn)
                cmd.Parameters.AddWithValue("@Name", name)
                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.UtcNow)
                cmd.ExecuteNonQuery()
            End Using

            ' Return new ID (Access-specific)
            Using cmd As New OleDbCommand("SELECT @@IDENTITY", conn)
                Return CInt(cmd.ExecuteScalar())
            End Using
        End Function

    End Class

End Namespace
