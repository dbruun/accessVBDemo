namespace ChompMan.GameEngine;

/// <summary>
/// Immutable snapshot of the maze-level definition loaded from the database.
/// </summary>
public class LevelDefinition
{
    /// <summary>Database level number (1-based).</summary>
    public int LevelNumber { get; }

    /// <summary>Multi-line maze layout string.</summary>
    public string MazeLayout { get; }

    /// <summary>Ghost speed expressed as ticks-per-move (lower = faster).</summary>
    public int GhostSpeedTicks { get; }

    /// <summary>Expected pellet count (informational).</summary>
    public int PelletCount { get; }

    /// <summary>Initialises a new <see cref="LevelDefinition"/>.</summary>
    public LevelDefinition(int levelNumber, string mazeLayout, int ghostSpeedTicks, int pelletCount)
    {
        LevelNumber = levelNumber;
        MazeLayout = mazeLayout;
        GhostSpeedTicks = ghostSpeedTicks;
        PelletCount = pelletCount;
    }
}

/// <summary>Encapsulates the complete runtime state of a game session.</summary>
public class GameState
{
    /// <summary>Parsed maze for the current level.</summary>
    public Maze Maze { get; }

    /// <summary>The player entity.</summary>
    public Player Player { get; }

    /// <summary>All ghost entities in play.</summary>
    public IReadOnlyList<Ghost> Ghosts { get; }

    /// <summary>1-based current level number.</summary>
    public int CurrentLevel { get; set; } = 1;

    /// <summary>Current game phase.</summary>
    public GamePhase Phase { get; set; } = GamePhase.Playing;

    /// <summary>Ticks remaining in a mode-switch or respawn delay.</summary>
    public int DelayTicksLeft { get; set; }

    /// <summary>Number of ghosts eaten since the last power pellet (for bonus scoring).</summary>
    public int GhostsEatenCombo { get; set; }

    /// <summary>
    /// Ticks remaining before ghosts switch from Scatter ↔ Chase.
    /// Negative means Chase phase is active.
    /// </summary>
    public int ModeSwitchTicksLeft { get; set; } = 300;

    /// <summary>Whether we are currently in scatter (vs chase) phase.</summary>
    public bool IsScatterPhase { get; set; } = true;

    /// <summary>Builds game state from a parsed maze and ghost list.</summary>
    public GameState(Maze maze, Player player, List<Ghost> ghosts)
    {
        Maze = maze;
        Player = player;
        Ghosts = ghosts.AsReadOnly();
    }
}
