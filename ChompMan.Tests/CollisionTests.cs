using ChompMan.GameEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChompMan.Tests;

[TestClass]
public class CollisionTests
{
    private static GameState BuildOpenState()
    {
        var layout = "#####\n#...#\n#.P.#\n#...#\n#####";
        var maze = new Maze(layout);
        var player = new Player(maze.PlayerStart);
        var ghosts = new List<Ghost>();
        return new GameState(maze, player, ghosts);
    }

    [TestMethod]
    public void WalkingOverPellet_ConsumesIt()
    {
        var state = BuildOpenState();
        var pelletPos = new Position(2, 3);
        state.Maze.SetCell(pelletPos, CellType.Pellet);
        var engine = new Engine(state);
        engine.SetInput(Direction.Right);
        for (var i = 1; i <= state.Player.SpeedTicks; i++) engine.Tick();
        Assert.AreEqual(CellType.Empty, state.Maze.GetCell(pelletPos));
    }

    [TestMethod]
    public void WalkingOverPellet_IncrementsScore()
    {
        var state = BuildOpenState();
        var pelletPos = new Position(2, 3);
        state.Maze.SetCell(pelletPos, CellType.Pellet);
        var engine = new Engine(state);
        engine.SetInput(Direction.Right);
        for (var i = 1; i <= state.Player.SpeedTicks; i++) engine.Tick();
        Assert.AreEqual(Engine.PelletScore, state.Player.Score);
    }

    [TestMethod]
    public void WalkingOverPowerPellet_IncrementsScoreByPowerAmount()
    {
        var state = BuildOpenState();
        var ppPos = new Position(2, 3);
        state.Maze.SetCell(ppPos, CellType.PowerPellet);
        var engine = new Engine(state);
        engine.SetInput(Direction.Right);
        for (var i = 1; i <= state.Player.SpeedTicks; i++) engine.Tick();
        Assert.AreEqual(Engine.PowerPelletScore, state.Player.Score);
    }

    [TestMethod]
    public void WalkingOverPowerPellet_FrightensGhosts()
    {
        var ppPos = new Position(2, 3);
        var ghost = new Ghost(0, "Blinky", new Position(1, 1));
        var ghosts = new List<Ghost> { ghost };
        var maze = new Maze("#####\n#...#\n#.P.#\n#...#\n#####");
        var player = new Player(maze.PlayerStart);
        var state = new GameState(maze, player, ghosts);
        state.Maze.SetCell(ppPos, CellType.PowerPellet);
        var engine = new Engine(state);
        engine.SetInput(Direction.Right);
        for (var i = 1; i <= player.SpeedTicks; i++) engine.Tick();
        Assert.AreEqual(GhostMode.Frightened, state.Ghosts[0].Mode);
    }

    [TestMethod]
    public void TouchingNormalGhost_LosesLife()
    {
        var maze = new Maze("#####\n#...#\n#.P.#\n#...#\n#####");
        var player = new Player(maze.PlayerStart);
        var ghost = new Ghost(0, "Blinky", player.Position) { Mode = GhostMode.Chase };
        var ghosts = new List<Ghost> { ghost };
        var state = new GameState(maze, player, ghosts);
        var startLives = player.Lives;
        var engine = new Engine(state);
        engine.Tick();
        Assert.AreEqual(startLives - 1, state.Player.Lives);
    }

    [TestMethod]
    public void TouchingNormalGhost_TriggersRespawn()
    {
        var maze = new Maze("#####\n#...#\n#.P.#\n#...#\n#####");
        var player = new Player(maze.PlayerStart);
        var ghost = new Ghost(0, "Blinky", player.Position) { Mode = GhostMode.Chase };
        var ghosts = new List<Ghost> { ghost };
        var state = new GameState(maze, player, ghosts);
        var engine = new Engine(state);
        engine.Tick();
        Assert.AreEqual(GamePhase.Respawning, state.Phase);
    }

    [TestMethod]
    public void TouchingFrightenedGhost_EatsGhost_NotLoseLife()
    {
        var maze = new Maze("#####\n#...#\n#.P.#\n#...#\n#####");
        var player = new Player(maze.PlayerStart);
        var ghost = new Ghost(0, "Blinky", player.Position);
        ghost.Frighten(200);
        var ghosts = new List<Ghost> { ghost };
        var startLives = player.Lives;
        var state = new GameState(maze, player, ghosts);
        var engine = new Engine(state);
        engine.Tick();
        Assert.AreEqual(startLives, state.Player.Lives);
        Assert.AreEqual(GhostMode.Eaten, state.Ghosts[0].Mode);
    }

    [TestMethod]
    public void TouchingFrightenedGhost_AddsScore()
    {
        var maze = new Maze("#####\n#...#\n#.P.#\n#...#\n#####");
        var player = new Player(maze.PlayerStart);
        var ghost = new Ghost(0, "Blinky", player.Position);
        ghost.Frighten(200);
        var ghosts = new List<Ghost> { ghost };
        var state = new GameState(maze, player, ghosts);
        var engine = new Engine(state);
        engine.Tick();
        Assert.IsTrue(state.Player.Score >= Engine.GhostBaseScore);
    }

    [TestMethod]
    public void CollectingLastPellet_TriggersLevelComplete()
    {
        var layout = "#####\n#   #\n# P #\n#   #\n#####";
        var maze = new Maze(layout);
        var pelletPos = new Position(2, 3);
        maze.SetCell(pelletPos, CellType.Pellet);
        var player = new Player(maze.PlayerStart);
        var ghosts = new List<Ghost>();
        var state = new GameState(maze, player, ghosts);
        var engine = new Engine(state);
        engine.SetInput(Direction.Right);
        for (var i = 1; i <= player.SpeedTicks; i++) engine.Tick();
        Assert.AreEqual(GamePhase.LevelComplete, state.Phase);
    }
}
