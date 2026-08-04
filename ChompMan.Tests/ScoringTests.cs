using ChompMan.GameEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChompMan.Tests;

[TestClass]
public class ScoringTests
{
    private static string MazeWithOpenCentre => "#####\n#...#\n#.P.#\n#...#\n#####";

    private static (Engine engine, GameState state) BuildEngine()
    {
        var maze = new Maze(MazeWithOpenCentre);
        var player = new Player(maze.PlayerStart);
        var ghosts = new List<Ghost>();
        var state = new GameState(maze, player, ghosts);
        var engine = new Engine(state);
        return (engine, state);
    }

    [TestMethod] public void PelletScoreConstant_Is10() => Assert.AreEqual(10, Engine.PelletScore);
    [TestMethod] public void PowerPelletScoreConstant_Is50() => Assert.AreEqual(50, Engine.PowerPelletScore);
    [TestMethod] public void GhostBaseScoreConstant_Is200() => Assert.AreEqual(200, Engine.GhostBaseScore);

    [TestMethod]
    public void CollectingPellet_AddsExactly10()
    {
        var (engine, state) = BuildEngine();
        state.Maze.SetCell(new Position(2, 3), CellType.Pellet);
        engine.SetInput(Direction.Right);
        for (var i = 0; i < state.Player.SpeedTicks; i++) engine.Tick();
        Assert.AreEqual(10, state.Player.Score);
    }

    [TestMethod]
    public void CollectingPowerPellet_AddsExactly50()
    {
        var (engine, state) = BuildEngine();
        state.Maze.SetCell(new Position(2, 3), CellType.PowerPellet);
        engine.SetInput(Direction.Right);
        for (var i = 0; i < state.Player.SpeedTicks; i++) engine.Tick();
        Assert.AreEqual(50, state.Player.Score);
    }

    [TestMethod]
    public void CollectingMultiplePellets_ScoreAccumulates()
    {
        var (engine, state) = BuildEngine();
        engine.SetInput(Direction.Left);
        for (var i = 0; i < state.Player.SpeedTicks; i++) engine.Tick();
        engine.SetInput(Direction.Right);
        for (var i = 0; i < state.Player.SpeedTicks * 2; i++) engine.Tick();
        Assert.AreEqual(20, state.Player.Score);
    }

    [TestMethod]
    public void EatingFirstFrightenedGhost_Awards200()
    {
        var maze = new Maze(MazeWithOpenCentre);
        var player = new Player(maze.PlayerStart);
        var ghost = new Ghost(0, "Blinky", player.Position);
        ghost.Frighten(200);
        var state = new GameState(maze, player, new List<Ghost> { ghost });
        var engine = new Engine(state);
        engine.Tick();
        Assert.AreEqual(200, state.Player.Score);
    }

    [TestMethod]
    public void EatingSecondFrightenedGhost_Awards400()
    {
        var maze = new Maze(MazeWithOpenCentre);
        var player = new Player(maze.PlayerStart);
        var ghosts = new List<Ghost>
        {
            new(0, "Blinky", player.Position),
            new(1, "Pinky", player.Position)
        };
        ghosts[0].Frighten(200);
        ghosts[1].Frighten(200);
        var state = new GameState(maze, player, ghosts);
        var engine = new Engine(state);
        engine.Tick();
        Assert.AreEqual(600, state.Player.Score);
    }

    [TestMethod]
    public void Player_AddScore_AccumulatesCorrectly()
    {
        var player = new Player(new Position(0, 0));
        player.AddScore(10);
        player.AddScore(25);
        Assert.AreEqual(35, player.Score);
    }

    [TestMethod] public void Player_InitialScore_IsZero() => Assert.AreEqual(0, new Player(new Position(0, 0)).Score);
    [TestMethod] public void ManhattanDistance_CalculatesCorrectly() => Assert.AreEqual(7, new Position(1, 2).ManhattanDistance(new Position(4, 6)));
    [TestMethod] public void ManhattanDistance_SamePosition_IsZero() => Assert.AreEqual(0, new Position(1, 2).ManhattanDistance(new Position(1, 2)));
}
