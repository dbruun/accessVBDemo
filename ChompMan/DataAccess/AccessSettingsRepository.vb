Imports System.Collections.Generic
Imports System.Data.OleDb

Namespace DataAccess

    ''' <summary>Repository for the Settings table (key/value tunables).</summary>
    Public Class AccessSettingsRepository

        Private ReadOnly _connectionString As String

        ''' <summary>Creates the repository pointing at <paramref name="dbPath"/>.</summary>
        Public Sub New(dbPath As String)
            _connectionString = $"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbPath};"
        End Sub

        ''' <summary>Returns all settings rows.</summary>
        Public Function GetAll() As List(Of SettingEntry)
            Dim result As New List(Of SettingEntry)()
            Using conn As New OleDbConnection(_connectionString)
                conn.Open()
                Using cmd As New OleDbCommand("SELECT [Key], Value FROM Settings ORDER BY [Key]", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New SettingEntry() With {
                                .Key = reader.GetString(0),
                                .Value = reader.GetString(1)
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Returns the value for <paramref name="key"/>, or <paramref name="defaultValue"/> if not found.</summary>
        Public Function GetValue(key As String, Optional defaultValue As String = "") As String
            Using conn As New OleDbConnection(_connectionString)
                conn.Open()
                Using cmd As New OleDbCommand("SELECT Value FROM Settings WHERE [Key] = ?", conn)
                    cmd.Parameters.AddWithValue("@Key", key)
                    Dim val = cmd.ExecuteScalar()
                    If val IsNot Nothing AndAlso val IsNot DBNull.Value Then
                        Return CStr(val)
                    End If
                End Using
            End Using
            Return defaultValue
        End Function

        ''' <summary>Inserts or updates a setting row.</summary>
        Public Sub Upsert(key As String, value As String)
            Using conn As New OleDbConnection(_connectionString)
                conn.Open()
                ' Check exists
                Dim exists As Boolean = False
                Using cmd As New OleDbCommand("SELECT COUNT(*) FROM Settings WHERE [Key] = ?", conn)
                    cmd.Parameters.AddWithValue("@Key", key)
                    exists = CInt(cmd.ExecuteScalar()) > 0
                End Using

                If exists Then
                    Using cmd As New OleDbCommand("UPDATE Settings SET Value = ? WHERE [Key] = ?", conn)
                        cmd.Parameters.AddWithValue("@Value", value)
                        cmd.Parameters.AddWithValue("@Key", key)
                        cmd.ExecuteNonQuery()
                    End Using
                Else
                    Using cmd As New OleDbCommand("INSERT INTO Settings ([Key], Value) VALUES (?, ?)", conn)
                        cmd.Parameters.AddWithValue("@Key", key)
                        cmd.Parameters.AddWithValue("@Value", value)
                        cmd.ExecuteNonQuery()
                    End Using
                End If
            End Using
        End Sub

    End Class

End Namespace
