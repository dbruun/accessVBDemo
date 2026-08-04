using ChompMan.GameEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChompMan.Tests;

[TestClass]
public class MovementTests
{
    private static string OpenMazeLayout => "#####\n#...#\n#.P.#\n#...#\n#####";

    [TestMethod]
    public void Moved_Up_DecreasesRow()
    {
        var pos = new Position(5, 5);
        var result = pos.Moved(Direction.Up);
        Assert.AreEqual(4, result.Row);
        Assert.AreEqual(5, result.Col);
    }

    [TestMethod]
    public void Moved_Down_IncreasesRow()
    {
        var pos = new Position(5, 5);
        var result = pos.Moved(Direction.Down);
        Assert.AreEqual(6, result.Row);
    }

    [TestMethod]
    public void Moved_Left_DecreasesCol()
    {
        var pos = new Position(5, 5);
        var result = pos.Moved(Direction.Left);
        Assert.AreEqual(4, result.Col);
    }

    [TestMethod]
    public void Moved_Right_IncreasesCol()
    {
        var pos = new Position(5, 5);
        var result = pos.Moved(Direction.Right);
        Assert.AreEqual(6, result.Col);
    }

    [TestMethod]
    public void Moved_None_ReturnsSamePosition()
    {
        var pos = new Position(3, 7);
        var result = pos.Moved(Direction.None);
        Assert.AreEqual(pos, result);
    }

    [TestMethod]
    public void IsPassable_Wall_ReturnsFalse()
    {
        var maze = new Maze(OpenMazeLayout);
        Assert.IsFalse(maze.IsPassable(new Position(0, 0)));
    }

    [TestMethod]
    public void IsPassable_OpenCell_ReturnsTrue()
    {
        var maze = new Maze(OpenMazeLayout);
        Assert.IsTrue(maze.IsPassable(new Position(2, 2)));
    }

    [TestMethod]
    public void IsPassable_OutOfBounds_ReturnsFalse()
    {
        var maze = new Maze(OpenMazeLayout);
        Assert.IsFalse(maze.IsPassable(new Position(-1, 0)));
        Assert.IsFalse(maze.IsPassable(new Position(99, 99)));
    }

    [TestMethod]
    public void MazeParse_FindsPlayerStart()
    {
        var maze = new Maze(OpenMazeLayout);
        Assert.AreEqual(new Position(2, 2), maze.PlayerStart);
    }

    [TestMethod]
    public void MazeParse_PlayerStartCellBecomesEmpty()
    {
        var maze = new Maze(OpenMazeLayout);
        Assert.AreEqual(CellType.Empty, maze.GetCell(new Position(2, 2)));
    }

    [TestMethod]
    public void MazeParse_WallCell()
    {
        var maze = new Maze(OpenMazeLayout);
        Assert.AreEqual(CellType.Wall, maze.GetCell(new Position(0, 0)));
    }

    [TestMethod]
    public void Player_MovesIntoOpenCell_AfterSpeedTicks()
    {
        var maze = new Maze(OpenMazeLayout);
        var player = new Player(maze.PlayerStart);
        var ghosts = new List<Ghost>();
        var state = new GameState(maze, player, ghosts);
        var engine = new Engine(state);
        engine.SetInput(Direction.Up);
        for (var i = 1; i <= player.SpeedTicks; i++) engine.Tick();
        Assert.AreEqual(new Position(1, 2), state.Player.Position);
    }

    [TestMethod]
    public void Player_CannotMoveIntoWall()
    {
        var maze = new Maze(OpenMazeLayout);
        var player = new Player(new Position(1, 1));
        var ghosts = new List<Ghost>();
        var state = new GameState(maze, player, ghosts);
        var engine = new Engine(state);
        engine.SetInput(Direction.Up);
        for (var i = 1; i <= player.SpeedTicks * 3; i++) engine.Tick();
        Assert.AreEqual(new Position(1, 1), state.Player.Position);
    }

    [TestMethod]
    public void Player_DirectionQueued_HonouredOnNextOpenCell()
    {
        var maze = new Maze(OpenMazeLayout);
        var player = new Player(maze.PlayerStart);
        var ghosts = new List<Ghost>();
        var state = new GameState(maze, player, ghosts);
        var engine = new Engine(state);
        engine.SetInput(Direction.Right);
        for (var i = 1; i <= player.SpeedTicks; i++) engine.Tick();
        Assert.AreEqual(new Position(2, 3), state.Player.Position);
    }
}
