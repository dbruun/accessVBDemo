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
        ' TODO:

        <TestMethod>
        Public Sub PelletScoreConstant_Is10()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        <TestMethod>
        Public Sub PowerPelletScoreConstant_Is50()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        <TestMethod>
        Public Sub GhostBaseScoreConstant_Is200()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        ' ── Pellet scoring ────────────────────────────────────────────────────

        <TestMethod>
        Public Sub CollectingPellet_AddsExactly10()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        <TestMethod>
        Public Sub CollectingPowerPellet_AddsExactly50()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        <TestMethod>
        Public Sub CollectingMultiplePellets_ScoreAccumulates()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        ' ── Ghost combo scoring ───────────────────────────────────────────────

        <TestMethod>
        Public Sub EatingFirstFrightenedGhost_Awards200()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        <TestMethod>
        Public Sub EatingSecondFrightenedGhost_Awards400()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        ' ── Player AddScore helper ────────────────────────────────────────────

        <TestMethod>
        Public Sub Player_AddScore_AccumulatesCorrectly()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        <TestMethod>
        Public Sub Player_InitialScore_IsZero()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        ' ── Position distance helper ──────────────────────────────────────────

        <TestMethod>
        Public Sub ManhattanDistance_CalculatesCorrectly()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub

        <TestMethod>
        Public Sub ManhattanDistance_SamePosition_IsZero()
            Assert.Inconclusive("TODO finish on Monday")
        End Sub


    End Class

End Namespace
