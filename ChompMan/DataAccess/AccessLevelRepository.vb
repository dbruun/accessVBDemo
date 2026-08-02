Imports System
Imports System.Collections.Generic
Imports System.Data.OleDb

Namespace DataAccess

    ''' <summary>
    ''' <see cref="ILevelRepository"/> implementation backed by a Microsoft Access
    ''' (.accdb) database via ACE OLEDB 16.0.
    ''' </summary>
    Public Class AccessLevelRepository
        Implements ILevelRepository

        Private ReadOnly _connectionString As String

        ''' <summary>
        ''' Creates the repository pointing at <paramref name="dbPath"/>.
        ''' </summary>
        Public Sub New(dbPath As String)
            _connectionString = $"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbPath};"
        End Sub

        ''' <inheritdoc/>
        Public Function GetLevel(levelNumber As Integer) As LevelData _
            Implements ILevelRepository.GetLevel

            Using conn As New OleDbConnection(_connectionString)
                conn.Open()
                Dim sql = "SELECT LevelNumber, MazeLayout, GhostSpeed, PelletCount " &
                          "FROM Levels WHERE LevelNumber = ?"
                Using cmd As New OleDbCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@LevelNumber", levelNumber)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return ReadLevel(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        ''' <inheritdoc/>
        Public Function GetAllLevels() As List(Of LevelData) _
            Implements ILevelRepository.GetAllLevels

            Dim result As New List(Of LevelData)()
            Using conn As New OleDbConnection(_connectionString)
                conn.Open()
                Dim sql = "SELECT LevelNumber, MazeLayout, GhostSpeed, PelletCount " &
                          "FROM Levels ORDER BY LevelNumber"
                Using cmd As New OleDbCommand(sql, conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(ReadLevel(reader))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ' ── Private helpers ──────────────────────────────────────────────────

        Private Shared Function ReadLevel(reader As OleDbDataReader) As LevelData
            Return New LevelData() With {
                .LevelNumber = reader.GetInt32(0),
                .MazeLayout = reader.GetString(1),
                .GhostSpeed = reader.GetInt32(2),
                .PelletCount = reader.GetInt32(3)
            }
        End Function

    End Class

End Namespace
