Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace GameEngine

    ''' <summary>
    ''' Core game-update logic.  Intentionally free of any WinForms or database
    ''' dependencies so it can be unit-tested and later ported.
    ''' </summary>
    Public Class Engine

        ' ── Scoring constants ─────────────────────────────────────────────────
        Public Const PelletScore As Integer = 10
        Public Const PowerPelletScore As Integer = 50
        Public Const GhostBaseScore As Integer = 200
        Public Const ExtraLifeThreshold As Integer = 10000

        ' ── Timing constants (ticks) ──────────────────────────────────────────
        Public Const FrightenedDuration As Integer = 180  ' ~3 s at 60 fps
        Public Const RespawnDelay As Integer = 120        ' ~2 s
        Public Const LevelCompleteDelay As Integer = 90   ' ~1.5 s
        Public Const ScatterPhaseTicks As Integer = 300
        Public Const ChasePhaseTicks As Integer = 600

        ' ── RNG ───────────────────────────────────────────────────────────────
        Private Shared ReadOnly _rng As New Random()

        ' ── State ─────────────────────────────────────────────────────────────
        Private ReadOnly _state As GameState

        ' ── Events ────────────────────────────────────────────────────────────

        ''' <summary>Raised when the player score changes.</summary>
        Public Event ScoreChanged(sender As Object, e As EventArgs)

        ''' <summary>Raised when the player loses a life.</summary>
        Public Event LifeLost(sender As Object, e As EventArgs)

        ''' <summary>Raised when all pellets are collected.</summary>
        Public Event LevelComplete(sender As Object, e As EventArgs)

        ''' <summary>Raised when all lives are exhausted.</summary>
        Public Event GameOver(sender As Object, e As EventArgs)

        ''' <summary>Raised when a ghost is eaten in frightened mode.</summary>
        Public Event GhostEaten(sender As Object, score As Integer)

        ' Tracks the last score milestone at which an extra life was granted
        Private _lastExtraLifeAt As Integer = 0

        ' ── Constructor ───────────────────────────────────────────────────────

        ''' <summary>Creates an engine for the given game state.</summary>
        Public Sub New(state As GameState)
            _state = state
        End Sub

        ' ── Public API ────────────────────────────────────────────────────────

        ''' <summary>
        ''' Advances the game by one logic tick (~16 ms at 60 fps).
        ''' Should only be called when <see cref="GameState.Phase"/> is
        ''' <see cref="GamePhase.Playing"/> or <see cref="GamePhase.Respawning"/>.
        ''' </summary>
        Public Sub Tick()
            Select Case _state.Phase
                Case GamePhase.Playing
                    TickModeSwitchTimer()
                    MovePlayer()
                    MoveGhosts()
                    CheckPlayerGhostCollisions()

                Case GamePhase.Respawning
                    _state.DelayTicksLeft -= 1
                    If _state.DelayTicksLeft <= 0 Then
                        _state.Phase = GamePhase.Playing
                    End If
            End Select
        End Sub

        ''' <summary>Queues a direction change from key input.</summary>
        Public Sub SetInput(dir As Direction)
            _state.Player.QueuedDirection = dir
        End Sub

        ''' <summary>Toggles Pause ↔ Playing.</summary>
        Public Sub TogglePause()
            If _state.Phase = GamePhase.Playing Then
                _state.Phase = GamePhase.Paused
            ElseIf _state.Phase = GamePhase.Paused Then
                _state.Phase = GamePhase.Playing
            End If
        End Sub

        ''' <summary>Exposes a read-only view of the current state for the UI.</summary>
        Public ReadOnly Property State As GameState
            Get
                Return _state
            End Get
        End Property

        ' ── Private: mode-switch timer ────────────────────────────────────────

        Private Sub TickModeSwitchTimer()
            ' Don't switch modes during frightened phase
            If _state.Ghosts.Any(Function(g) g.Mode = GhostMode.Frightened) Then Return

            _state.ModeSwitchTicksLeft -= 1
            If _state.ModeSwitchTicksLeft <= 0 Then
                If _state.IsScatterPhase Then
                    _state.IsScatterPhase = False
                    _state.ModeSwitchTicksLeft = ChasePhaseTicks
                    For Each g In _state.Ghosts
                        If g.Mode = GhostMode.Scatter Then g.Mode = GhostMode.Chase
                    Next
                Else
                    _state.IsScatterPhase = True
                    _state.ModeSwitchTicksLeft = ScatterPhaseTicks
                    For Each g In _state.Ghosts
                        If g.Mode = GhostMode.Chase Then g.Mode = GhostMode.Scatter
                    Next
                End If
            End If
        End Sub

        ' ── Private: player movement ─────────────────────────────────────────

        Private Sub MovePlayer()
            Dim p = _state.Player
            p.TickCounter += 1
            If p.TickCounter < p.SpeedTicks Then Return
            p.TickCounter = 0

            ' Try to honour queued direction first
            If p.QueuedDirection <> Direction.None Then
                Dim queuedCell = p.Position.Moved(p.QueuedDirection)
                queuedCell = New Position(queuedCell.Row, _state.Maze.WrapCol(queuedCell.Row, queuedCell.Col))
                If _state.Maze.IsPassable(queuedCell) Then
                    p.CurrentDirection = p.QueuedDirection
                    p.QueuedDirection = Direction.None
                End If
            End If

            If p.CurrentDirection = Direction.None Then Return

            Dim target = p.Position.Moved(p.CurrentDirection)
            target = New Position(target.Row, _state.Maze.WrapCol(target.Row, target.Col))

            If Not _state.Maze.IsPassable(target) Then Return

            p.Position = target
            CollectPellet(p.Position)
        End Sub

        Private Sub CollectPellet(pos As Position)
            Dim p = _state.Player
            Dim cell = _state.Maze.GetCell(pos)
            Select Case cell
                Case CellType.Pellet
                    _state.Maze.SetCell(pos, CellType.Empty)
                    p.AddScore(PelletScore)
                    RaiseEvent ScoreChanged(Me, EventArgs.Empty)
                    CheckLevelComplete()

                Case CellType.PowerPellet
                    _state.Maze.SetCell(pos, CellType.Empty)
                    p.AddScore(PowerPelletScore)
                    RaiseEvent ScoreChanged(Me, EventArgs.Empty)
                    FrightenAllGhosts()
                    _state.GhostsEatenCombo = 0
                    CheckLevelComplete()
            End Select
        End Sub

        Private Sub FrightenAllGhosts()
            For Each g In _state.Ghosts
                If g.Mode <> GhostMode.Eaten Then
                    g.Frighten(FrightenedDuration)
                End If
            Next
        End Sub

        Private Sub CheckLevelComplete()
            If _state.Maze.RemainingPellets() = 0 Then
                _state.Phase = GamePhase.LevelComplete
                RaiseEvent LevelComplete(Me, EventArgs.Empty)
            End If
        End Sub

        ' ── Private: ghost movement ───────────────────────────────────────────

        Private Sub MoveGhosts()
            For Each g In _state.Ghosts
                ' Tick down frightened timer
                If g.Mode = GhostMode.Frightened Then
                    g.FrightenedTicksLeft -= 1
                    If g.FrightenedTicksLeft <= 0 Then
                        g.Mode = If(_state.IsScatterPhase, GhostMode.Scatter, GhostMode.Chase)
                    End If
                End If

                g.TickCounter += 1
                Dim effectiveSpeed = g.SpeedTicks
                If g.Mode = GhostMode.Frightened Then effectiveSpeed = g.SpeedTicks + 2
                If g.Mode = GhostMode.Eaten Then effectiveSpeed = Math.Max(1, g.SpeedTicks - 2)
                If g.TickCounter < effectiveSpeed Then Continue For
                g.TickCounter = 0

                Dim nextDir = ChooseGhostDirection(g)
                If nextDir <> Direction.None Then
                    Dim nextPos = g.Position.Moved(nextDir)
                    nextPos = New Position(nextPos.Row, _state.Maze.WrapCol(nextPos.Row, nextPos.Col))
                    g.Position = nextPos
                    g.CurrentDirection = nextDir

                    ' Ghost reached home — resurrect
                    If g.Mode = GhostMode.Eaten AndAlso g.Position = g.SpawnPosition Then
                        g.Mode = If(_state.IsScatterPhase, GhostMode.Scatter, GhostMode.Chase)
                    End If
                End If
            Next
        End Sub

        Private Function ChooseGhostDirection(g As Ghost) As Direction
            Dim candidates = GetPassableDirections(g.Position, True)
            If candidates.Count = 0 Then Return Direction.None

            ' Never reverse unless no other option
            Dim noReverse = candidates.Where(Function(d) d <> Opposite(g.CurrentDirection)).ToList()
            Dim choices = If(noReverse.Count > 0, noReverse, candidates)

            Select Case g.Mode
                Case GhostMode.Frightened
                    ' Random movement
                    Return choices(_rng.Next(choices.Count))

                Case GhostMode.Eaten
                    ' Head toward spawn position
                    Return BestDirection(choices, g.Position, g.SpawnPosition)

                Case GhostMode.Scatter
                    Dim target = g.GetScatterTarget(_state.Maze.Rows, _state.Maze.Cols)
                    ' 20 % chance to take a random turn to prevent perfect lock
                    If _rng.NextDouble() < 0.2 Then
                        Return choices(_rng.Next(choices.Count))
                    End If
                    Return BestDirection(choices, g.Position, target)

                Case Else ' Chase
                    Dim playerPos = _state.Player.Position
                    ' Ghost 0: direct chase; Ghost 1: 4 tiles ahead of player; others: random
                    Select Case g.Index Mod 3
                        Case 0
                            Return BestDirection(choices, g.Position, playerPos)
                        Case 1
                            Dim ahead = playerPos.Moved(_state.Player.CurrentDirection).Moved(_state.Player.CurrentDirection)
                            Return BestDirection(choices, g.Position, ahead)
                        Case Else
                            If _rng.NextDouble() < 0.6 Then
                                Return BestDirection(choices, g.Position, playerPos)
                            End If
                            Return choices(_rng.Next(choices.Count))
                    End Select
            End Select
        End Function

        ''' <summary>Returns all passable neighbours of <paramref name="pos"/>.</summary>
        Friend Function GetPassableDirections(pos As Position, isGhost As Boolean) As List(Of Direction)
            Dim result As New List(Of Direction)()
            For Each d In New Direction() {Direction.Up, Direction.Down, Direction.Left, Direction.Right}
                Dim candidate = pos.Moved(d)
                candidate = New Position(candidate.Row, _state.Maze.WrapCol(candidate.Row, candidate.Col))
                If _state.Maze.IsPassable(candidate, isGhost) Then result.Add(d)
            Next
            Return result
        End Function

        Private Shared Function BestDirection(choices As List(Of Direction), from As Position, target As Position) As Direction
            Dim best = choices(0)
            Dim bestDist = Integer.MaxValue
            For Each d In choices
                Dim dist = from.Moved(d).ManhattanDistance(target)
                If dist < bestDist Then
                    bestDist = dist
                    best = d
                End If
            Next
            Return best
        End Function

        Private Shared Function Opposite(d As Direction) As Direction
            Select Case d
                Case Direction.Up    : Return Direction.Down
                Case Direction.Down  : Return Direction.Up
                Case Direction.Left  : Return Direction.Right
                Case Direction.Right : Return Direction.Left
                Case Else            : Return Direction.None
            End Select
        End Function

        ' ── Private: collision detection ──────────────────────────────────────

        Private Sub CheckPlayerGhostCollisions()
            Dim player = _state.Player
            For Each g In _state.Ghosts
                If g.Position <> player.Position Then Continue For

                If g.Mode = GhostMode.Frightened Then
                    ' Eat the ghost
                    g.Mode = GhostMode.Eaten
                    g.FrightenedTicksLeft = 0
                    _state.GhostsEatenCombo += 1
                    Dim bonus = GhostBaseScore * CInt(Math.Pow(2, _state.GhostsEatenCombo - 1))
                    player.AddScore(bonus)
                    RaiseEvent GhostEaten(Me, bonus)
                    RaiseEvent ScoreChanged(Me, EventArgs.Empty)

                ElseIf g.Mode = GhostMode.Chase OrElse g.Mode = GhostMode.Scatter Then
                    ' Player caught
                    player.Lives -= 1
                    If player.Lives <= 0 Then
                        _state.Phase = GamePhase.GameOver
                        RaiseEvent GameOver(Me, EventArgs.Empty)
                    Else
                        RaiseEvent LifeLost(Me, EventArgs.Empty)
                        RespawnEntities()
                    End If
                    Return
                End If
            Next

            ' Extra life milestone
            If player.Score \ ExtraLifeThreshold > _lastExtraLifeAt Then
                _lastExtraLifeAt = player.Score \ ExtraLifeThreshold
                player.Lives += 1
            End If
        End Sub

        Private Sub RespawnEntities()
            _state.Player.Respawn(_state.Maze.PlayerStart)
            For Each g In _state.Ghosts
                g.Respawn()
            Next
            _state.Phase = GamePhase.Respawning
            _state.DelayTicksLeft = RespawnDelay
            _state.GhostsEatenCombo = 0
        End Sub

    End Class

End Namespace
