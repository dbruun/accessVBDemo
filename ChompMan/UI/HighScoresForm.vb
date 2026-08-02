Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports ChompMan.DataAccess

Namespace UI

    ''' <summary>Displays the top-10 high scores loaded from the Access database.</summary>
    Public Class HighScoresForm
        Inherits Form

        Private WithEvents _btnClose As Button

        ''' <summary>Creates the high-scores screen.</summary>
        Public Sub New()
            InitializeComponent()
            LoadScores()
        End Sub

        Private _listView As ListView

        Private Sub InitializeComponent()
            Me.Text = "ChompMan – High Scores"
            Me.ClientSize = New Size(500, 430)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent
            Me.BackColor = Color.Black

            Dim lblTitle As New Label() With {
                .Text = "HIGH SCORES",
                .Font = New Font("Courier New", 20, FontStyle.Bold),
                .ForeColor = Color.Yellow,
                .TextAlign = ContentAlignment.MiddleCenter,
                .AutoSize = False,
                .Size = New Size(500, 50),
                .Location = New Point(0, 10)
            }

            _listView = New ListView() With {
                .View = View.Details,
                .FullRowSelect = True,
                .GridLines = True,
                .BackColor = Color.Black,
                .ForeColor = Color.White,
                .Font = New Font("Courier New", 11),
                .Location = New Point(20, 75),
                .Size = New Size(460, 290)
            }
            _listView.Columns.Add("#", 40)
            _listView.Columns.Add("Name", 130)
            _listView.Columns.Add("Score", 100)
            _listView.Columns.Add("Level", 70)
            _listView.Columns.Add("Date", 110)

            _btnClose = New Button() With {
                .Text = "CLOSE",
                .Font = New Font("Courier New", 12, FontStyle.Bold),
                .ForeColor = Color.Yellow,
                .BackColor = Color.Black,
                .FlatStyle = FlatStyle.Flat,
                .Size = New Size(120, 38),
                .Location = New Point(190, 378)
            }

            Controls.AddRange(New Control() {lblTitle, _listView, _btnClose})
        End Sub

        Private Sub LoadScores()
            Try
                Dim repo As New AccessScoreRepository(Program.DbPath)
                Dim scores = repo.GetTopScores(10)
                PopulateList(scores)
            Catch ex As Exception
                _listView.Items.Add(New ListViewItem(New String() {"", "DB unavailable", ex.Message, "", ""}))
            End Try
        End Sub

        Private Sub PopulateList(scores As List(Of ScoreEntry))
            _listView.Items.Clear()
            For i As Integer = 0 To scores.Count - 1
                Dim s = scores(i)
                Dim item As New ListViewItem((i + 1).ToString())
                item.SubItems.Add(s.PlayerName)
                item.SubItems.Add(s.Score.ToString("N0"))
                item.SubItems.Add(s.LevelReached.ToString())
                item.SubItems.Add(s.PlayedOn.ToLocalTime().ToString("yyyy-MM-dd"))
                If i = 0 Then item.ForeColor = Color.Gold
                _listView.Items.Add(item)
            Next
        End Sub

        Private Sub _btnClose_Click(sender As Object, e As EventArgs) Handles _btnClose.Click
            Close()
        End Sub

    End Class

End Namespace
