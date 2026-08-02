Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ChompMan.GameEngine

Namespace ChompMan.Tests

    ''' <summary>
    ''' Tests for player and ghost movement through the maze,
    ''' including wall collision and direction queuing.
    ''' </summary>
    <TestClass>
    Public Class MovementTests

        Private Shared ReadOnly Property OpenMazeLayout As String
            Get
                Dim layout = "#####" & vbLf &
                             "#...#" & vbLf &
                             "#.P.#" & vbLf &
                             "#...#" & vbLf &
                             "#####"
                Return layout
            End Get
        End Property

        ' ── Position.Moved ────────────────────────────────────────────────────

        <TestMethod>
        Public Sub Moved_Up_DecreasesRow()
            Dim pos As New Position(5, 5)
            Dim result = pos.Moved(Direction.Up)
            Assert.AreEqual(4, result.Row)
            Assert.AreEqual(5, result.Col)
        End Sub

        <TestMethod>
        Public Sub Moved_Down_IncreasesRow()
            Dim pos As New Position(5, 5)
            Dim result = pos.Moved(Direction.Down)
            Assert.AreEqual(6, result.Row)
        End Sub

        <TestMethod>
        Public Sub Moved_Left_DecreasesCol()
            Dim pos As New Position(5, 5)
            Dim result = pos.Moved(Direction.Left)
            Assert.AreEqual(4, result.Col)
        End Sub

        <TestMethod>
        Public Sub Moved_Right_IncreasesCol()
            Dim pos As New Position(5, 5)
            Dim result = pos.Moved(Direction.Right)
            Assert.AreEqual(6, result.Col)
        End Sub

        <TestMethod>
        Public Sub Moved_None_ReturnsSamePosition()
            Dim pos As New Position(3, 7)
            Dim result = pos.Moved(Direction.None)
            Assert.AreEqual(pos, result)
        End Sub

        ' ── Maze passability ─────────────────────────────────────────────────

        <TestMethod>
        Public Sub IsPassable_Wall_ReturnsFalse()
            Dim maze As New Maze(OpenMazeLayout)
            Assert.IsFalse(maze.IsPassable(New Position(0, 0)))
        End Sub

        <TestMethod>
        Public Sub IsPassable_OpenCell_ReturnsTrue()
            Dim maze As New Maze(OpenMazeLayout)
            Assert.IsTrue(maze.IsPassable(New Position(2, 2)))
        End Sub

        <TestMethod>
        Public Sub IsPassable_OutOfBounds_ReturnsFalse()
            Dim maze As New Maze(OpenMazeLayout)
            Assert.IsFalse(maze.IsPassable(New Position(-1, 0)))
            Assert.IsFalse(maze.IsPassable(New Position(99, 99)))
        End Sub

        ' ── Maze parsing ──────────────────────────────────────────────────────

        <TestMethod>
        Public Sub MazeParse_FindsPlayerStart()
            Dim maze As New Maze(OpenMazeLayout)
            Assert.AreEqual(New Position(2, 2), maze.PlayerStart)
        End Sub

        <TestMethod>
        Public Sub MazeParse_PlayerStartCellBecomesEmpty()
            Dim maze As New Maze(OpenMazeLayout)
            Assert.AreEqual(CellType.Empty, maze.GetCell(New Position(2, 2)))
        End Sub

        <TestMethod>
        Public Sub MazeParse_WallCell()
            Dim maze As New Maze(OpenMazeLayout)
            Assert.AreEqual(CellType.Wall, maze.GetCell(New Position(0, 0)))
        End Sub

        ' ── Engine: player movement ───────────────────────────────────────────

        <TestMethod>
        Public Sub Player_MovesIntoOpenCell_AfterSpeedTicks()
            Dim maze As New Maze(OpenMazeLayout)
            Dim player As New Player(maze.PlayerStart)
            Dim ghosts As New List(Of Ghost)()
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.SetInput(Direction.Up)

            For i = 1 To player.SpeedTicks
                engine.Tick()
            Next

            Assert.AreEqual(New Position(1, 2), state.Player.Position)
        End Sub

        <TestMethod>
        Public Sub Player_CannotMoveIntoWall()
            Dim maze As New Maze(OpenMazeLayout)
            Dim player As New Player(New Position(1, 1))
            Dim ghosts As New List(Of Ghost)()
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.SetInput(Direction.Up)
            For i = 1 To player.SpeedTicks * 3
                engine.Tick()
            Next

            Assert.AreEqual(New Position(1, 1), state.Player.Position)
        End Sub

        <TestMethod>
        Public Sub Player_DirectionQueued_HonouredOnNextOpenCell()
            Dim layout = "#####" & vbLf &
                         "#...#" & vbLf &
                         "#.P.#" & vbLf &
                         "#...#" & vbLf &
                         "#####"
            Dim maze As New Maze(layout)
            Dim player As New Player(maze.PlayerStart)
            Dim ghosts As New List(Of Ghost)()
            Dim state As New GameState(maze, player, ghosts)
            Dim engine As New Engine(state)

            engine.SetInput(Direction.Right)
            For i = 1 To player.SpeedTicks
                engine.Tick()
            Next
            Assert.AreEqual(New Position(2, 3), state.Player.Position)
        End Sub

    End Class

End Namespace
