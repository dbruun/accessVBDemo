Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports ChompMan.DataAccess
Imports ChompMan.GameEngine

Namespace UI

    ''' <summary>
    ''' Main game screen.  Hosts the maze panel, score/lives labels and the
    ''' game-loop timer.  Rendering is done with GDI+ in the Panel's Paint event.
    ''' </summary>
    Public Class GameForm
        Inherits Form

        ' ── Layout constants ──────────────────────────────────────────────────
        Private Const CellSize As Integer = 22      ' pixels per maze cell
        Private Const HudHeight As Integer = 50     ' header strip height
        Private Const SidebarWidth As Integer = 0   ' reserved for future use

        ' ── Game loop ─────────────────────────────────────────────────────────
        Private WithEvents _timer As Timer          ' ~60 fps
        Private _engine As Engine
        Private _state As GameState
        Private _levelDefs As List(Of LevelData)

        ' ── GDI+ objects (allocated once, disposed on close) ──────────────────
        Private _wallBrush As SolidBrush
        Private _pelletBrush As SolidBrush
        Private _powerBrush As SolidBrush
        Private _playerBrush As SolidBrush
        Private _frightenBrush As SolidBrush
        Private _eatenBrush As SolidBrush
        Private _bgBrush As SolidBrush
        Private _hudFont As Font
        Private _bigFont As Font
        Private _ghostBrushes() As SolidBrush

        ' ── Controls ──────────────────────────────────────────────────────────
        Private _panel As BufferedPanel
        Private _lblScore As Label
        Private _lblLives As Label
        Private _lblLevel As Label

        ' ── Repositories ──────────────────────────────────────────────────────
        Private ReadOnly _levelRepo As ILevelRepository
        Private ReadOnly _scoreRepo As IScoreRepository

        ''' <summary>Creates the game form and loads level definitions.</summary>
        Public Sub New()
            _levelRepo = New AccessLevelRepository(Program.DbPath)
            _scoreRepo = New AccessScoreRepository(Program.DbPath)
            _levelDefs = TryLoadLevels()

            InitializeComponent()
            AllocateBrushes()
            StartNewGame()
        End Sub

        ' ── Initialisation ────────────────────────────────────────────────────

        Private Sub InitializeComponent()
            Me.Text = "ChompMan"
            Me.FormBorderStyle = FormBorderStyle.FixedSingle
            Me.MaximizeBox = False
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.BackColor = Color.Black
            Me.KeyPreview = True

            _lblScore = New Label() With {
                .Font = New Font("Courier New", 12, FontStyle.Bold),
                .ForeColor = Color.White,
                .BackColor = Color.Black,
                .AutoSize = True,
                .Location = New Point(5, 8)
            }
            _lblLevel = New Label() With {
                .Font = New Font("Courier New", 12, FontStyle.Bold),
                .ForeColor = Color.Cyan,
                .BackColor = Color.Black,
                .AutoSize = True,
                .Location = New Point(200, 8)
            }
            _lblLives = New Label() With {
                .Font = New Font("Courier New", 12, FontStyle.Bold),
                .ForeColor = Color.Yellow,
                .BackColor = Color.Black,
                .AutoSize = True,
                .Location = New Point(340, 8)
            }

            _panel = New BufferedPanel() With {
                .Location = New Point(0, HudHeight),
                .BackColor = Color.Black
            }
            AddHandler _panel.Paint, AddressOf Panel_Paint

            Controls.AddRange(New Control() {_lblScore, _lblLevel, _lblLives, _panel})

            _timer = New Timer() With {.Interval = 16}  ' ~62.5 fps
        End Sub

        Private Sub AllocateBrushes()
            _wallBrush = New SolidBrush(Color.DarkBlue)
            _pelletBrush = New SolidBrush(Color.WhiteSmoke)
            _powerBrush = New SolidBrush(Color.Orange)
            _playerBrush = New SolidBrush(Color.Yellow)
            _frightenBrush = New SolidBrush(Color.DeepSkyBlue)
            _eatenBrush = New SolidBrush(Color.DimGray)
            _bgBrush = New SolidBrush(Color.Black)
            _hudFont = New Font("Courier New", 11, FontStyle.Bold)
            _bigFont = New Font("Courier New", 22, FontStyle.Bold)
            _ghostBrushes = New SolidBrush() {
                New SolidBrush(Color.Red),
                New SolidBrush(Color.HotPink),
                New SolidBrush(Color.Cyan),
                New SolidBrush(Color.Orange)
            }
        End Sub

        ' ── Game lifecycle ────────────────────────────────────────────────────

        Private Sub StartNewGame()
            LoadLevel(1)
            _timer.Start()
        End Sub

        Private Sub LoadLevel(levelNumber As Integer)
            Dim def = GetLevelDef(levelNumber)
            Dim maze As New Maze(def.MazeLayout)
            Dim player As New Player(maze.PlayerStart, lives:=3)

            Dim ghosts As New List(Of Ghost)()
            Dim names = New String() {"Blinky", "Pinky", "Inky", "Clyde"}
            For i As Integer = 0 To Math.Min(maze.GhostStarts.Count, 4) - 1
                Dim g As New Ghost(i, names(i), maze.GhostStarts(i))
                g.SpeedTicks = def.GhostSpeed
                ghosts.Add(g)
            Next

            _state = New GameState(maze, player, ghosts) With {.CurrentLevel = levelNumber}
            _engine = New Engine(_state)

            AddHandler _engine.ScoreChanged, AddressOf Engine_ScoreChanged
            AddHandler _engine.LifeLost, AddressOf Engine_LifeLost
            AddHandler _engine.LevelComplete, AddressOf Engine_LevelComplete
            AddHandler _engine.GameOver, AddressOf Engine_GameOver
            AddHandler _engine.GhostEaten, AddressOf Engine_GhostEaten

            ResizeFormToMaze(maze)
            UpdateHud()
        End Sub

        Private Function GetLevelDef(levelNumber As Integer) As LevelData
            If _levelDefs IsNot Nothing Then
                Dim def = _levelDefs.FirstOrDefault(Function(l) l.LevelNumber = levelNumber)
                If def IsNot Nothing Then Return def
            End If
            ' Fallback built-in layout for level 1
            Return New LevelData() With {
                .LevelNumber = levelNumber,
                .GhostSpeed = Math.Max(2, 7 - levelNumber),
                .MazeLayout = BuiltInMaze1()
            }
        End Function

        Private Shared Function BuiltInMaze1() As String
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
        End Function

        Private Function TryLoadLevels() As List(Of LevelData)
            Try
                Return _levelRepo.GetAllLevels()
            Catch
                Return Nothing
            End Try
        End Function

        Private Sub ResizeFormToMaze(maze As Maze)
            Dim panelW = maze.Cols * CellSize
            Dim panelH = maze.Rows * CellSize
            _panel.Size = New Size(panelW, panelH)
            Me.ClientSize = New Size(panelW, panelH + HudHeight)
        End Sub

        Private Sub UpdateHud()
            If _state Is Nothing Then Return
            _lblScore.Text = $"Score: {_state.Player.Score:N0}"
            _lblLevel.Text = $"Level: {_state.CurrentLevel}"
            _lblLives.Text = $"Lives: {_state.Player.Lives}"
        End Sub

        ' ── Game-loop timer ───────────────────────────────────────────────────

        Private Sub _timer_Tick(sender As Object, e As EventArgs) Handles _timer.Tick
            If _state.Phase = GamePhase.Playing OrElse _state.Phase = GamePhase.Respawning Then
                _engine.Tick()
                UpdateHud()
            End If
            _panel.Invalidate()
        End Sub

        ' ── Engine events ─────────────────────────────────────────────────────

        Private Sub Engine_ScoreChanged(sender As Object, e As EventArgs)
            UpdateHud()
        End Sub

        Private Sub Engine_LifeLost(sender As Object, e As EventArgs)
            UpdateHud()
        End Sub

        Private Sub Engine_LevelComplete(sender As Object, e As EventArgs)
            _timer.Stop()
            MessageBox.Show($"Level {_state.CurrentLevel} complete!  Get ready for the next level.",
                            "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Dim nextLevel = _state.CurrentLevel + 1
            Dim nextDef = GetLevelDef(nextLevel)
            If nextDef Is Nothing OrElse String.IsNullOrEmpty(nextDef.MazeLayout) Then
                ' No more levels — treat as game won
                ShowGameOver(won:=True)
            Else
                LoadLevel(nextLevel)
                _timer.Start()
            End If
        End Sub

        Private Sub Engine_GameOver(sender As Object, e As EventArgs)
            _timer.Stop()
            ShowGameOver(won:=False)
        End Sub

        Private Sub Engine_GhostEaten(sender As Object, score As Integer)
            ' Future: briefly show score popup at ghost position
        End Sub

        Private Sub ShowGameOver(won As Boolean)
            Dim msg = If(won, "You beat all levels!", "GAME OVER")
            Dim goForm As New GameOverForm(msg, _state.Player.Score, _state.CurrentLevel, _scoreRepo)
            goForm.ShowDialog(Me)
            Me.Close()
        End Sub

        ' ── Keyboard input ────────────────────────────────────────────────────

        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If _engine Is Nothing Then Return

            Select Case e.KeyCode
                Case Keys.Up    : _engine.SetInput(Direction.Up)
                Case Keys.Down  : _engine.SetInput(Direction.Down)
                Case Keys.Left  : _engine.SetInput(Direction.Left)
                Case Keys.Right : _engine.SetInput(Direction.Right)
                Case Keys.W     : _engine.SetInput(Direction.Up)
                Case Keys.S     : _engine.SetInput(Direction.Down)
                Case Keys.A     : _engine.SetInput(Direction.Left)
                Case Keys.D     : _engine.SetInput(Direction.Right)
                Case Keys.P     : _engine.TogglePause()
                Case Keys.R
                    _timer.Stop()
                    StartNewGame()
                Case Keys.Escape
                    _timer.Stop()
                    Me.Close()
            End Select
            e.Handled = True
        End Sub

        ' ── GDI+ rendering ───────────────────────────────────────────────────

        Private Sub Panel_Paint(sender As Object, e As PaintEventArgs)
            Dim g = e.Graphics
            g.Clear(Color.Black)
            If _state Is Nothing Then Return

            DrawMaze(g)
            DrawGhosts(g)
            DrawPlayer(g)
            If _state.Phase = GamePhase.Paused Then DrawOverlay(g, "PAUSED")
            If _state.Phase = GamePhase.Respawning Then DrawOverlay(g, "")
        End Sub

        Private Sub DrawMaze(g As Graphics)
            Dim maze = _state.Maze
            For r As Integer = 0 To maze.Rows - 1
                For c As Integer = 0 To maze.Cols - 1
                    Dim cell = maze.GetCell(New Position(r, c))
                    Dim x = c * CellSize
                    Dim y = r * CellSize
                    Select Case cell
                        Case CellType.Wall
                            g.FillRectangle(_wallBrush, x, y, CellSize, CellSize)
                            ' Draw inner border highlight
                            g.DrawRectangle(Pens.MidnightBlue, x + 1, y + 1, CellSize - 3, CellSize - 3)
                        Case CellType.Pellet
                            g.FillRectangle(_bgBrush, x, y, CellSize, CellSize)
                            Dim cx = x + CellSize \ 2 - 2
                            Dim cy = y + CellSize \ 2 - 2
                            g.FillEllipse(_pelletBrush, cx, cy, 4, 4)
                        Case CellType.PowerPellet
                            g.FillRectangle(_bgBrush, x, y, CellSize, CellSize)
                            Dim cx = x + CellSize \ 2 - 5
                            Dim cy = y + CellSize \ 2 - 5
                            g.FillEllipse(_powerBrush, cx, cy, 10, 10)
                        Case Else
                            g.FillRectangle(_bgBrush, x, y, CellSize, CellSize)
                    End Select
                Next
            Next
        End Sub

        Private Sub DrawPlayer(g As Graphics)
            If _state.Phase = GamePhase.Respawning Then Return
            Dim pos = _state.Player.Position
            Dim x = pos.Col * CellSize + 1
            Dim y = pos.Row * CellSize + 1
            Dim sz = CellSize - 2
            ' Draw Pac-Man as a filled circle with a "mouth" wedge cutout
            Dim dir = _state.Player.CurrentDirection
            Dim startAngle = DirectionToAngle(dir)
            g.FillPie(_playerBrush, x, y, sz, sz, startAngle + 30, 300)
        End Sub

        Private Sub DrawGhosts(g As Graphics)
            For i As Integer = 0 To _state.Ghosts.Count - 1
                Dim ghost = _state.Ghosts(i)
                Dim x = ghost.Position.Col * CellSize + 1
                Dim y = ghost.Position.Row * CellSize + 1
                Dim sz = CellSize - 2

                Dim brush As SolidBrush
                Select Case ghost.Mode
                    Case GhostMode.Frightened
                        brush = _frightenBrush
                    Case GhostMode.Eaten
                        brush = _eatenBrush
                    Case Else
                        brush = _ghostBrushes(i Mod _ghostBrushes.Length)
                End Select

                ' Ghost body: semicircle top + wavy bottom
                Dim rect As New Rectangle(x, y, sz, sz)
                g.FillEllipse(brush, x, y, sz, sz - 2)
                g.FillRectangle(brush, x, y + sz \ 2 - 2, sz, sz \ 2 + 2)
                ' Draw eyes (only when not eaten)
                If ghost.Mode <> GhostMode.Eaten Then
                    g.FillEllipse(Brushes.White, x + 3, y + 4, 5, 5)
                    g.FillEllipse(Brushes.White, x + sz - 8, y + 4, 5, 5)
                    g.FillEllipse(Brushes.DarkBlue, x + 4, y + 5, 3, 3)
                    g.FillEllipse(Brushes.DarkBlue, x + sz - 7, y + 5, 3, 3)
                End If
            Next
        End Sub

        Private Sub DrawOverlay(g As Graphics, text As String)
            Dim sz = _panel.ClientSize
            Using overlayBrush As New SolidBrush(Color.FromArgb(160, Color.Black))
                g.FillRectangle(overlayBrush, 0, 0, sz.Width, sz.Height)
            End Using
            If Not String.IsNullOrEmpty(text) Then
                Dim sf As New StringFormat() With {
                    .Alignment = StringAlignment.Center,
                    .LineAlignment = StringAlignment.Center
                }
                g.DrawString(text, _bigFont, Brushes.Yellow,
                             New RectangleF(0, 0, sz.Width, sz.Height), sf)
            End If
        End Sub

        Private Shared Function DirectionToAngle(dir As Direction) As Single
            Select Case dir
                Case Direction.Right : Return 0
                Case Direction.Down  : Return 90
                Case Direction.Left  : Return 180
                Case Direction.Up    : Return 270
                Case Else            : Return 0
            End Select
        End Function

        ' ── Dispose ──────────────────────────────────────────────────────────

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                _timer?.Dispose()
                _wallBrush?.Dispose()
                _pelletBrush?.Dispose()
                _powerBrush?.Dispose()
                _playerBrush?.Dispose()
                _frightenBrush?.Dispose()
                _eatenBrush?.Dispose()
                _bgBrush?.Dispose()
                _hudFont?.Dispose()
                _bigFont?.Dispose()
                For Each b In _ghostBrushes
                    b?.Dispose()
                Next
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class

    ''' <summary>
    ''' A <see cref="Panel"/> with double-buffering enabled so the game loop can
    ''' repaint every frame without flickering.
    ''' </summary>
    Friend Class BufferedPanel
        Inherits Panel

        Public Sub New()
            DoubleBuffered = True
            SetStyle(ControlStyles.OptimizedDoubleBuffer Or
                     ControlStyles.AllPaintingInWmPaint Or
                     ControlStyles.UserPaint, True)
            UpdateStyles()
        End Sub

    End Class

End Namespace
