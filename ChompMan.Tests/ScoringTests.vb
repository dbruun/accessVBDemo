Imports System.Collections.Generic
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
            Dim pelletPos As New Position(2, 3)
            result.state.Maze.SetCell(pelletPos, CellType.Pellet)

            result.engine.SetInput(Direction.Right)
            For i = 1 To result.state.Player.SpeedTicks
                result.engine.Tick()
            Next

            Assert.AreEqual(Engine.PelletScore, result.state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub CollectingPowerPellet_AddsExactly50()
            Dim result = BuildEngine()
            Dim pelletPos As New Position(2, 3)
            result.state.Maze.SetCell(pelletPos, CellType.PowerPellet)

            result.engine.SetInput(Direction.Right)
            For i = 1 To result.state.Player.SpeedTicks
                result.engine.Tick()
            Next

            Assert.AreEqual(Engine.PowerPelletScore, result.state.Player.Score)
        End Sub

        <TestMethod>
        Public Sub CollectingMultiplePellets_ScoreAccumulates()
            Dim result = BuildEngine()
            result.state.Maze.SetCell(New Position(2, 3), CellType.Pellet)
            result.state.Maze.SetCell(New Position(3, 3), CellType.Pellet)

            result.engine.SetInput(Direction.Right)
            For i = 1 To result.state.Player.SpeedTicks
                result.engine.Tick()
            Next

            result.engine.SetInput(Direction.Down)
            For i = 1 To result.state.Player.SpeedTicks
                result.engine.Tick()
            Next

            Assert.AreEqual(2 * Engine.PelletScore, result.state.Player.Score)
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
            Dim firstGhost As New Ghost(0, "Blinky", player.Position)
            firstGhost.Frighten(200)
            Dim ghosts As New List(Of Ghost) From {firstGhost}
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.Tick()

            Dim secondGhost As New Ghost(1, "Pinky", player.Position)
            secondGhost.Frighten(200)
            ghosts.Add(secondGhost)
            engine.Tick()

            Assert.AreEqual(Engine.GhostBaseScore + (2 * Engine.GhostBaseScore), state.Player.Score)
        End Sub

        ' ── Player AddScore helper ────────────────────────────────────────────

        <TestMethod>
        Public Sub Player_AddScore_AccumulatesCorrectly()
            Dim player As New Player(New Position(0, 0))

            player.AddScore(25)
            player.AddScore(15)

            Assert.AreEqual(40, player.Score)
        End Sub

        <TestMethod>
        Public Sub Player_InitialScore_IsZero()
            Dim player As New Player(New Position(0, 0))

            Assert.AreEqual(0, player.Score)
        End Sub

        ' ── Position distance helper ──────────────────────────────────────────

        <TestMethod>
        Public Sub ManhattanDistance_CalculatesCorrectly()
            Dim a As New Position(1, 2)
            Dim b As New Position(4, 5)

            Assert.AreEqual(6, a.ManhattanDistance(b))
        End Sub

        <TestMethod>
        Public Sub ManhattanDistance_SamePosition_IsZero()
            Dim a As New Position(3, 4)

            Assert.AreEqual(0, a.ManhattanDistance(a))
        End Sub

    End Class

End Namespace
