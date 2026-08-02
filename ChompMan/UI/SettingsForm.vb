Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports ChompMan.DataAccess

Namespace UI

    ''' <summary>
    ''' Displays and allows editing of Settings key/value rows stored in the
    ''' Access database.
    ''' </summary>
    Public Class SettingsForm
        Inherits Form

        Private WithEvents _btnSave As Button
        Private WithEvents _btnClose As Button
        Private _grid As DataGridView

        ''' <summary>Creates the settings form.</summary>
        Public Sub New()
            InitializeComponent()
            LoadSettings()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "ChompMan – Settings"
            Me.ClientSize = New Size(420, 400)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent
            Me.BackColor = Color.Black

            Dim lblTitle As New Label() With {
                .Text = "SETTINGS",
                .Font = New Font("Courier New", 18, FontStyle.Bold),
                .ForeColor = Color.Yellow,
                .TextAlign = ContentAlignment.MiddleCenter,
                .AutoSize = False,
                .Size = New Size(420, 44),
                .Location = New Point(0, 10)
            }

            _grid = New DataGridView() With {
                .Location = New Point(10, 65),
                .Size = New Size(400, 270),
                .BackgroundColor = Color.Black,
                .GridColor = Color.DimGray,
                .DefaultCellStyle = New DataGridViewCellStyle() With {
                    .BackColor = Color.Black,
                    .ForeColor = Color.White,
                    .Font = New Font("Courier New", 10)
                },
                .ColumnHeadersDefaultCellStyle = New DataGridViewCellStyle() With {
                    .BackColor = Color.DarkBlue,
                    .ForeColor = Color.White,
                    .Font = New Font("Courier New", 10, FontStyle.Bold)
                },
                .AllowUserToAddRows = False,
                .AllowUserToDeleteRows = False,
                .RowHeadersVisible = False,
                .SelectionMode = DataGridViewSelectionMode.CellSelect
            }
            _grid.Columns.Add("Key", "Key")
            _grid.Columns.Add("Value", "Value")
            _grid.Columns(0).Width = 190
            _grid.Columns(0).ReadOnly = True
            _grid.Columns(1).Width = 200

            _btnSave = New Button() With {
                .Text = "SAVE",
                .Font = New Font("Courier New", 12, FontStyle.Bold),
                .ForeColor = Color.Black,
                .BackColor = Color.Yellow,
                .Size = New Size(100, 36),
                .Location = New Point(100, 348)
            }
            _btnClose = New Button() With {
                .Text = "CLOSE",
                .Font = New Font("Courier New", 12),
                .ForeColor = Color.White,
                .BackColor = Color.DimGray,
                .FlatStyle = FlatStyle.Flat,
                .Size = New Size(100, 36),
                .Location = New Point(215, 348)
            }

            Controls.AddRange(New Control() {lblTitle, _grid, _btnSave, _btnClose})
        End Sub

        Private Sub LoadSettings()
            _grid.Rows.Clear()
            Try
                Dim repo As New AccessSettingsRepository(Program.DbPath)
                Dim settings = repo.GetAll()
                For Each s In settings
                    _grid.Rows.Add(s.Key, s.Value)
                Next
            Catch
                ' DB unavailable — show placeholder rows
                For Each pair In DefaultSettings()
                    _grid.Rows.Add(pair.Key, pair.Value)
                Next
            End Try
        End Sub

        Private Shared Function DefaultSettings() As Dictionary(Of String, String)
            Return New Dictionary(Of String, String) From {
                {"StartingLives", "3"},
                {"PlayerSpeedTicks", "4"},
                {"GhostSpeedTicks", "6"},
                {"FrightenedDuration", "180"},
                {"PlayerColour", "Yellow"},
                {"WallColour", "DarkBlue"},
                {"PelletColour", "White"},
                {"PowerPelletColour", "Orange"}
            }
        End Function

        Private Sub _btnSave_Click(sender As Object, e As EventArgs) Handles _btnSave.Click
            Try
                Dim repo As New AccessSettingsRepository(Program.DbPath)
                For Each row As DataGridViewRow In _grid.Rows
                    If row.IsNewRow Then Continue For
                    Dim key = CStr(row.Cells(0).Value)
                    Dim value = CStr(If(row.Cells(1).Value, ""))
                    repo.Upsert(key, value)
                Next
                MessageBox.Show("Settings saved.", "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show("Could not save settings: " & ex.Message,
                                "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Sub _btnClose_Click(sender As Object, e As EventArgs) Handles _btnClose.Click
            Close()
        End Sub

    End Class

End Namespace
