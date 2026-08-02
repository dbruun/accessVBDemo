Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ChompMan.GameEngine

Namespace ChompMan.Tests

    ''' <summary>
    ''' Tests for the scoring system: pellet values, power pellet values,
    ''' ghost combo multipliers, and extra life milestones.
    ''' </summary>
    <TestClass>
    Public Class ScoringTests

        Private Shared ReadOnly Property MazeWithOpenCentre As String
            Get
                Dim layout = "#####" & vbLf &
                             "#...#" & vbLf &
                             "#.P.#" & vbLf &
                             "#...#" & vbLf &
                             "#####"
                Return layout
            End Get
        End Property

        Private Shared Function BuildEngine() As (engine As Engine, state As GameState)
            Dim maze As New Maze(MazeWithOpenCentre)
            Dim player As New Player(maze.PlayerStart)
            Dim ghosts As New List(Of Ghost)()
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)
            Return (engine, state)
        End Function

        ' ── Scoring constants ─────────────────────────────────────────────────

        <TestMethod>
        Public Sub PelletScoreConstant_Is10()
            Assert.AreEqual(10, Engine.PelletScore)
        End Sub

        <TestMethod>
        Public Sub PowerPelletScoreConstant_Is50()
            Assert.AreEqual(50, Engine.PowerPelletScore)
        End Sub

        <TestMethod>
        Public Sub GhostBaseScoreConstant_Is200()
            Assert.AreEqual(200, Engine.GhostBaseScore)
        End Sub

        ' ── Pellet scoring ────────────────────────────────────────────────────

        <TestMethod>
        Public Sub CollectingPellet_AddsExactly10()
            Dim result = BuildEngine()
            Dim engine = result.engine
            Dim state = result.state
            state.Maze.SetCell(New Position(2, 3), CellType.Pellet)

            engine.SetInput(Direction.Right)
            For i = 1 To state.Player.SpeedTicks
                engine.Tick()
            Next
            Assert.AreEqual(10, state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub CollectingPowerPellet_AddsExactly50()
            Dim result = BuildEngine()
            Dim engine = result.engine
            Dim state = result.state
            state.Maze.SetCell(New Position(2, 3), CellType.PowerPellet)

            engine.SetInput(Direction.Right)
            For i = 1 To state.Player.SpeedTicks
                engine.Tick()
            Next
            Assert.AreEqual(50, state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub CollectingMultiplePellets_ScoreAccumulates()
            Dim layout = "#######" & vbLf &
                         "#.....#" & vbLf &
                         "#P....#" & vbLf &
                         "#.....#" & vbLf &
                         "#######"
            Dim maze As New Maze(layout)
            Dim player As New Player(maze.PlayerStart)
            Dim ghosts As New List(Of Ghost)()
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.SetInput(Direction.Right)
            For stepNum = 1 To 4
                For tick = 1 To player.SpeedTicks
                    engine.Tick()
                Next
            Next

            Assert.AreEqual(4 * Engine.PelletScore, state.Player.Score)
        End Sub

        ' ── Ghost combo scoring ───────────────────────────────────────────────

        <TestMethod>
        Public Sub EatingFirstFrightenedGhost_Awards200()
            Dim maze As New Maze(MazeWithOpenCentre)
            Dim player As New Player(maze.PlayerStart)
            Dim ghost0 As New Ghost(0, "Blinky", player.Position)
            ghost0.Frighten(500)
            Dim ghosts As New List(Of Ghost) From {ghost0}
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.Tick()

            Assert.AreEqual(Engine.GhostBaseScore, state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub EatingSecondFrightenedGhost_Awards400()
            Dim maze As New Maze(MazeWithOpenCentre)
            Dim player As New Player(maze.PlayerStart)
            Dim ghost0 As New Ghost(0, "Blinky", player.Position)
            Dim ghost1 As New Ghost(1, "Pinky", player.Position)
            ghost0.Frighten(500)
            ghost1.Frighten(500)
            Dim ghosts As New List(Of Ghost) From {ghost0, ghost1}
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.Tick()  ' eats ghost0 — combo=1 → 200
            engine.Tick()  ' eats ghost1 — combo=2 → 400

            Assert.AreEqual(600, state.Player.Score)
        End Sub

        ' ── Player AddScore helper ────────────────────────────────────────────

        <TestMethod>
        Public Sub Player_AddScore_AccumulatesCorrectly()
            Dim player As New Player(New Position(0, 0))
            player.AddScore(100)
            player.AddScore(250)
            Assert.AreEqual(350, player.Score)
        End Sub

        <TestMethod>
        Public Sub Player_InitialScore_IsZero()
            Dim player As New Player(New Position(0, 0))
            Assert.AreEqual(0, player.Score)
        End Sub

        ' ── Position distance helper ──────────────────────────────────────────

        <TestMethod>
        Public Sub ManhattanDistance_CalculatesCorrectly()
            Dim a As New Position(0, 0)
            Dim b As New Position(3, 4)
            Assert.AreEqual(7, a.ManhattanDistance(b))
        End Sub

        <TestMethod>
        Public Sub ManhattanDistance_SamePosition_IsZero()
            Dim a As New Position(5, 5)
            Assert.AreEqual(0, a.ManhattanDistance(a))
        End Sub

    End Class

End Namespace
