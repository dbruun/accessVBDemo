Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Collections.Generic
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
            Dim pelletPos As New Position(2, 3)
            state.Maze.SetCell(pelletPos, CellType.Pellet)

            engine.SetInput(Direction.Right)
            For i = 1 To state.Player.SpeedTicks
                engine.Tick()
            Next

            Assert.AreEqual(Engine.PelletScore, state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub CollectingPowerPellet_AddsExactly50()
            Dim result = BuildEngine()
            Dim engine = result.engine
            Dim state = result.state
            Dim powerPelletPos As New Position(2, 3)
            state.Maze.SetCell(powerPelletPos, CellType.PowerPellet)

            engine.SetInput(Direction.Right)
            For i = 1 To state.Player.SpeedTicks
                engine.Tick()
            Next

            Assert.AreEqual(Engine.PowerPelletScore, state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub CollectingMultiplePellets_ScoreAccumulates()
            Dim layout = "#######" & vbLf &
                         "#P....#" & vbLf &
                         "#######"
            Dim maze As New Maze(layout)
            Dim player As New Player(maze.PlayerStart)
            player.SpeedTicks = 1
            Dim ghosts As New List(Of Ghost)()
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.SetInput(Direction.Right)
            For i = 1 To 4
                engine.Tick()
            Next

            Assert.AreEqual(4 * Engine.PelletScore, state.Player.Score)
        End Sub

        ' ── Ghost combo scoring ───────────────────────────────────────────────

        <TestMethod>
        Public Sub EatingFirstFrightenedGhost_Awards200()
            Dim maze As New Maze(MazeWithOpenCentre)
            Dim player As New Player(maze.PlayerStart)
            Dim ghost As New Ghost(0, "Blinky", player.Position)
            ghost.Frighten(200)
            Dim ghosts As New List(Of Ghost) From {ghost}
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.Tick()

            Assert.AreEqual(Engine.GhostBaseScore, state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub EatingSecondFrightenedGhost_Awards400()
            Dim maze As New Maze(MazeWithOpenCentre)
            Dim player As New Player(maze.PlayerStart)
            Dim ghost As New Ghost(0, "Blinky", player.Position)
            ghost.Frighten(200)
            Dim ghosts As New List(Of Ghost) From {ghost}
            Dim state As New GameState(maze, player, ghosts)
            state.GhostsEatenCombo = 1
            Dim engine As New Engine(state)

            engine.Tick()

            Assert.AreEqual(400, state.Player.Score)
        End Sub

        ' ── Player AddScore helper ────────────────────────────────────────────

        <TestMethod>
        Public Sub Player_AddScore_AccumulatesCorrectly()
            Dim player As New Player(New Position(0, 0))

            player.AddScore(10)
            player.AddScore(25)

            Assert.AreEqual(35, player.Score)
        End Sub

        <TestMethod>
        Public Sub Player_InitialScore_IsZero()
            Dim player As New Player(New Position(0, 0))

            Assert.AreEqual(0, player.Score)
        End Sub

        ' ── Position distance helper ──────────────────────────────────────────

        <TestMethod>
        Public Sub ManhattanDistance_CalculatesCorrectly()
            Dim a As New Position(2, 3)
            Dim b As New Position(5, 1)

            Assert.AreEqual(5, a.ManhattanDistance(b))
        End Sub

        <TestMethod>
        Public Sub ManhattanDistance_SamePosition_IsZero()
            Dim pos As New Position(4, 4)

            Assert.AreEqual(0, pos.ManhattanDistance(pos))
        End Sub

    End Class

End Namespace
