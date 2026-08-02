Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ChompMan.GameEngine

Namespace ChompMan.Tests

    ''' <summary>
    ''' Tests for collision detection: player vs pellets, player vs ghosts
    ''' (including frightened mode ghost eating).
    ''' </summary>
    <TestClass>
    Public Class CollisionTests

        Private Shared Function BuildOpenState() As GameState
            Dim layout = "#####" & vbLf &
                         "#...#" & vbLf &
                         "#.P.#" & vbLf &
                         "#...#" & vbLf &
                         "#####"
            Dim maze As New Maze(layout)
            Dim player As New Player(maze.PlayerStart)
            Dim ghosts As New List(Of Ghost)()
            Return New GameState(maze, player, ghosts)
        End Function

        ' ── Pellet collection ────────────────────────────────────────────────

        <TestMethod>
        Public Sub WalkingOverPellet_ConsumesIt()
            Dim state = BuildOpenState()
            Dim pelletPos As New Position(2, 3)
            state.Maze.SetCell(pelletPos, CellType.Pellet)

            Dim engine As New Engine(state)
            engine.SetInput(Direction.Right)
            For i = 1 To state.Player.SpeedTicks
                engine.Tick()
            Next

            Assert.AreEqual(CellType.Empty, state.Maze.GetCell(pelletPos))
        End Sub

        <TestMethod>
        Public Sub WalkingOverPellet_IncrementsScore()
            Dim state = BuildOpenState()
            Dim pelletPos As New Position(2, 3)
            state.Maze.SetCell(pelletPos, CellType.Pellet)

            Dim engine As New Engine(state)
            engine.SetInput(Direction.Right)
            For i = 1 To state.Player.SpeedTicks
                engine.Tick()
            Next

            Assert.AreEqual(Engine.PelletScore, state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub WalkingOverPowerPellet_IncrementsScoreByPowerAmount()
            Dim state = BuildOpenState()
            Dim ppPos As New Position(2, 3)
            state.Maze.SetCell(ppPos, CellType.PowerPellet)

            Dim engine As New Engine(state)
            engine.SetInput(Direction.Right)
            For i = 1 To state.Player.SpeedTicks
                engine.Tick()
            Next

            Assert.AreEqual(Engine.PowerPelletScore, state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub WalkingOverPowerPellet_FrightensGhosts()
            Dim state = BuildOpenState()
            Dim ppPos As New Position(2, 3)
            state.Maze.SetCell(ppPos, CellType.PowerPellet)

            Dim ghost As New Ghost(0, "Blinky", New Position(1, 1))
            Dim ghosts As New List(Of Ghost) From {ghost}
            Dim maze As New Maze("#####" & vbLf &
                                 "#...#" & vbLf &
                                 "#.P.#" & vbLf &
                                 "#...#" & vbLf &
                                 "#####")
            Dim player As New Player(maze.PlayerStart)
            Dim state2 As New GameState(maze, player, ghosts)
            state2.Maze.SetCell(ppPos, CellType.PowerPellet)

            Dim engine As New Engine(state2)
            engine.SetInput(Direction.Right)
            For i = 1 To player.SpeedTicks
                engine.Tick()
            Next

            Assert.AreEqual(GhostMode.Frightened, state2.Ghosts(0).Mode)
        End Sub

        ' ── Player–ghost collision ────────────────────────────────────────────

        <TestMethod>
        Public Sub TouchingNormalGhost_LosesLife()
            Dim maze As New Maze("#####" & vbLf &
                                 "#...#" & vbLf &
                                 "#.P.#" & vbLf &
                                 "#...#" & vbLf &
                                 "#####")
            Dim player As New Player(maze.PlayerStart)
            Dim ghost As New Ghost(0, "Blinky", player.Position)
            ghost.Mode = GhostMode.Chase
            Dim ghosts As New List(Of Ghost) From {ghost}
            Dim state As New GameState(maze, player, ghosts)
            Dim startLives = player.Lives
            Dim engine As New Engine(state)

            engine.Tick()

            Assert.AreEqual(startLives - 1, state.Player.Lives)
        End Sub

        <TestMethod>
        Public Sub TouchingNormalGhost_TriggersRespawn()
            Dim maze As New Maze("#####" & vbLf &
                                 "#...#" & vbLf &
                                 "#.P.#" & vbLf &
                                 "#...#" & vbLf &
                                 "#####")
            Dim player As New Player(maze.PlayerStart)
            Dim ghost As New Ghost(0, "Blinky", player.Position)
            ghost.Mode = GhostMode.Chase
            Dim ghosts As New List(Of Ghost) From {ghost}
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.Tick()

            Assert.AreEqual(GamePhase.Respawning, state.Phase)
        End Sub

        <TestMethod>
        Public Sub TouchingFrightenedGhost_EatsGhost_NotLoseLife()
            Dim maze As New Maze("#####" & vbLf &
                                 "#...#" & vbLf &
                                 "#.P.#" & vbLf &
                                 "#...#" & vbLf &
                                 "#####")
            Dim player As New Player(maze.PlayerStart)
            Dim ghost As New Ghost(0, "Blinky", player.Position)
            ghost.Frighten(200)
            Dim ghosts As New List(Of Ghost) From {ghost}
            Dim startLives = player.Lives
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.Tick()

            Assert.AreEqual(startLives, state.Player.Lives)
            Assert.AreEqual(GhostMode.Eaten, state.Ghosts(0).Mode)
        End Sub

        <TestMethod>
        Public Sub TouchingFrightenedGhost_AddsScore()
            Dim maze As New Maze("#####" & vbLf &
                                 "#...#" & vbLf &
                                 "#.P.#" & vbLf &
                                 "#...#" & vbLf &
                                 "#####")
            Dim player As New Player(maze.PlayerStart)
            Dim ghost As New Ghost(0, "Blinky", player.Position)
            ghost.Frighten(200)
            Dim ghosts As New List(Of Ghost) From {ghost}
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.Tick()

            Assert.IsTrue(state.Player.Score >= Engine.GhostBaseScore)
        End Sub

        ' ── Level complete ────────────────────────────────────────────────────

        <TestMethod>
        Public Sub CollectingLastPellet_TriggersLevelComplete()
            Dim layout = "#####" & vbLf &
                         "#   #" & vbLf &
                         "# P #" & vbLf &
                         "#   #" & vbLf &
                         "#####"
            Dim maze As New Maze(layout)
            Dim pelletPos As New Position(2, 3)
            maze.SetCell(pelletPos, CellType.Pellet)

            Dim player As New Player(maze.PlayerStart)
            Dim ghosts As New List(Of Ghost)()
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.SetInput(Direction.Right)
            For i = 1 To player.SpeedTicks
                engine.Tick()
            Next

            Assert.AreEqual(GamePhase.LevelComplete, state.Phase)
        End Sub

    End Class

End Namespace
