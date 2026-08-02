Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports ChompMan.DataAccess

Namespace UI

    ''' <summary>Main menu screen — New Game, High Scores, Settings, Exit.</summary>
    Public Class MainMenuForm
        Inherits Form

        Private WithEvents _btnNewGame As Button
        Private WithEvents _btnHighScores As Button
        Private WithEvents _btnSettings As Button
        Private WithEvents _btnExit As Button
        Private _lblTitle As Label

        ''' <summary>Initialises the main menu form.</summary>
        Public Sub New()
            InitializeComponent()
        End Sub

        ' ── Designer-generated layout (manual) ───────────────────────────────

        Private Sub InitializeComponent()
            Me.Text = "ChompMan"
            Me.ClientSize = New Size(400, 480)
            Me.FormBorderStyle = FormBorderStyle.FixedSingle
            Me.MaximizeBox = False
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.BackColor = Color.Black

            _lblTitle = New Label() With {
                .Text = "CHOMPM" & vbLf & "  AN",
                .Font = New Font("Courier New", 32, FontStyle.Bold),
                .ForeColor = Color.Yellow,
                .TextAlign = ContentAlignment.MiddleCenter,
                .AutoSize = False,
                .Size = New Size(400, 120),
                .Location = New Point(0, 40)
            }

            _btnNewGame = MakeButton("NEW GAME", 200)
            _btnHighScores = MakeButton("HIGH SCORES", 265)
            _btnSettings = MakeButton("SETTINGS", 330)
            _btnExit = MakeButton("EXIT", 395)

            Controls.AddRange(New Control() {_lblTitle, _btnNewGame, _btnHighScores, _btnSettings, _btnExit})
        End Sub

        Private Shared Function MakeButton(text As String, top As Integer) As Button
            Return New Button() With {
                .Text = text,
                .Font = New Font("Courier New", 14, FontStyle.Bold),
                .ForeColor = Color.Yellow,
                .BackColor = Color.Black,
                .FlatStyle = FlatStyle.Flat,
                .Size = New Size(220, 45),
                .Location = New Point(90, top),
                .TabStop = True
            }
        End Function

        ' ── Event handlers ────────────────────────────────────────────────────

        Private Sub _btnNewGame_Click(sender As Object, e As EventArgs) Handles _btnNewGame.Click
            Dim gameForm As New GameForm()
            gameForm.ShowDialog(Me)
        End Sub

        Private Sub _btnHighScores_Click(sender As Object, e As EventArgs) Handles _btnHighScores.Click
            Dim hsForm As New HighScoresForm()
            hsForm.ShowDialog(Me)
        End Sub

        Private Sub _btnSettings_Click(sender As Object, e As EventArgs) Handles _btnSettings.Click
            Dim settingsForm As New SettingsForm()
            settingsForm.ShowDialog(Me)
        End Sub

        Private Sub _btnExit_Click(sender As Object, e As EventArgs) Handles _btnExit.Click
            Application.Exit()
        End Sub

    End Class

End Namespace
