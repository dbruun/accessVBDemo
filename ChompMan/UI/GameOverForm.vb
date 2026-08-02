Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports ChompMan.DataAccess

Namespace UI

    ''' <summary>
    ''' Game-Over screen: shows the final score, prompts for the player name and
    ''' saves the score to the Access database.
    ''' </summary>
    Public Class GameOverForm
        Inherits Form

        Private ReadOnly _message As String
        Private ReadOnly _finalScore As Integer
        Private ReadOnly _levelReached As Integer
        Private ReadOnly _scoreRepo As IScoreRepository

        Private _txtName As TextBox
        Private WithEvents _btnSave As Button
        Private WithEvents _btnSkip As Button

        ''' <summary>
        ''' Creates the form.
        ''' </summary>
        ''' <param name="message">Headline text (e.g. "GAME OVER" or "You won!").</param>
        ''' <param name="finalScore">Score to display and save.</param>
        ''' <param name="levelReached">Highest level reached in the session.</param>
        ''' <param name="scoreRepo">Repository used to persist the score (may be <c>Nothing</c>).</param>
        Public Sub New(message As String, finalScore As Integer, levelReached As Integer,
                       scoreRepo As IScoreRepository)
            _message = message
            _finalScore = finalScore
            _levelReached = levelReached
            _scoreRepo = scoreRepo
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "ChompMan – Game Over"
            Me.ClientSize = New Size(380, 300)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent
            Me.BackColor = Color.Black

            Dim lblMessage As New Label() With {
                .Text = _message,
                .Font = New Font("Courier New", 22, FontStyle.Bold),
                .ForeColor = Color.Yellow,
                .TextAlign = ContentAlignment.MiddleCenter,
                .AutoSize = False,
                .Size = New Size(380, 60),
                .Location = New Point(0, 20)
            }

            Dim lblScore As New Label() With {
                .Text = $"Final Score: {_finalScore:N0}",
                .Font = New Font("Courier New", 13),
                .ForeColor = Color.White,
                .TextAlign = ContentAlignment.MiddleCenter,
                .AutoSize = False,
                .Size = New Size(380, 35),
                .Location = New Point(0, 90)
            }

            Dim lblPrompt As New Label() With {
                .Text = "Enter your name to save:",
                .Font = New Font("Courier New", 11),
                .ForeColor = Color.Cyan,
                .AutoSize = True,
                .Location = New Point(30, 140)
            }

            _txtName = New TextBox() With {
                .Font = New Font("Courier New", 12),
                .MaxLength = 20,
                .Size = New Size(200, 28),
                .Location = New Point(30, 170)
            }

            _btnSave = New Button() With {
                .Text = "SAVE",
                .Font = New Font("Courier New", 11, FontStyle.Bold),
                .ForeColor = Color.Black,
                .BackColor = Color.Yellow,
                .Size = New Size(80, 32),
                .Location = New Point(240, 170)
            }

            _btnSkip = New Button() With {
                .Text = "SKIP",
                .Font = New Font("Courier New", 11),
                .ForeColor = Color.White,
                .BackColor = Color.DimGray,
                .FlatStyle = FlatStyle.Flat,
                .Size = New Size(80, 32),
                .Location = New Point(240, 212)
            }

            Controls.AddRange(New Control() {lblMessage, lblScore, lblPrompt, _txtName, _btnSave, _btnSkip})
            AcceptButton = _btnSave
        End Sub

        Private Sub _btnSave_Click(sender As Object, e As EventArgs) Handles _btnSave.Click
            Dim name = _txtName.Text.Trim()
            If String.IsNullOrEmpty(name) Then
                MessageBox.Show("Please enter your name.", "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            TrySaveScore(name)
            DialogResult = DialogResult.OK
            Close()
        End Sub

        Private Sub _btnSkip_Click(sender As Object, e As EventArgs) Handles _btnSkip.Click
            DialogResult = DialogResult.Cancel
            Close()
        End Sub

        Private Sub TrySaveScore(name As String)
            If _scoreRepo Is Nothing Then Return
            Try
                _scoreRepo.SaveScore(name, _finalScore, _levelReached)
            Catch ex As Exception
                MessageBox.Show("Could not save score: " & ex.Message,
                                "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

    End Class

End Namespace
