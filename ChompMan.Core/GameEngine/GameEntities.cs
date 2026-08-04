namespace ChompMan.GameEngine;

/// <summary>Represents the player-controlled character.</summary>
public class Player
{
    /// <summary>Current grid position.</summary>
    public Position Position { get; set; }

    /// <summary>Direction currently being travelled.</summary>
    public Direction CurrentDirection { get; set; } = Direction.None;

    /// <summary>Next direction the player has queued via key input.</summary>
    public Direction QueuedDirection { get; set; } = Direction.None;

    /// <summary>Accumulated score for this game session.</summary>
    public int Score { get; set; }

    /// <summary>Remaining lives (default 3).</summary>
    public int Lives { get; set; } = 3;

    /// <summary>Ticks between each movement step.</summary>
    public int SpeedTicks { get; set; } = 4;

    /// <summary>Internal tick counter used to pace movement.</summary>
    public int TickCounter { get; set; }

    /// <summary>Creates a player at the given spawn <paramref name="startPos"/>.</summary>
    public Player(Position startPos, int lives = 3)
    {
        Position = startPos;
        Lives = lives;
    }

    /// <summary>Resets position and direction to spawn state.</summary>
    public void Respawn(Position startPos)
    {
        Position = startPos;
        CurrentDirection = Direction.None;
        QueuedDirection = Direction.None;
        TickCounter = 0;
    }

    /// <summary>Adds <paramref name="points"/> to the player score.</summary>
    public void AddScore(int points)
    {
        Score += points;
    }
}

/// <summary>Represents a single ghost entity.</summary>
public class Ghost
{
    /// <summary>Ghost index (0–3); also used to pick scatter-corner target.</summary>
    public int Index { get; }

    /// <summary>Display colour identifier (maps to a <see cref="System.Drawing.Color"/> in the UI).</summary>
    public string ColourName { get; }

    /// <summary>Current grid position.</summary>
    public Position Position { get; set; }

    /// <summary>Direction currently being travelled.</summary>
    public Direction CurrentDirection { get; set; } = Direction.None;

    /// <summary>Behavioural mode.</summary>
    public GhostMode Mode { get; set; } = GhostMode.Scatter;

    /// <summary>Original spawn position used when the ghost is eaten and returns home.</summary>
    public Position SpawnPosition { get; }

    /// <summary>Remaining ticks in <see cref="GhostMode.Frightened"/> mode.</summary>
    public int FrightenedTicksLeft { get; set; }

    /// <summary>Ticks between each movement step (lower = faster).</summary>
    public int SpeedTicks { get; set; } = 6;

    /// <summary>Internal tick counter for movement pacing.</summary>
    public int TickCounter { get; set; }

    /// <summary>Creates a ghost with the given identity and spawn position.</summary>
    public Ghost(int index, string colourName, Position spawnPos)
    {
        Index = index;
        ColourName = colourName;
        SpawnPosition = spawnPos;
        Position = spawnPos;
    }

    /// <summary>Resets the ghost to spawn state.</summary>
    public void Respawn()
    {
        Position = SpawnPosition;
        CurrentDirection = Direction.None;
        Mode = GhostMode.Scatter;
        FrightenedTicksLeft = 0;
        TickCounter = 0;
    }

    /// <summary>Enters frightened mode for the given number of ticks.</summary>
    public void Frighten(int ticks)
    {
        Mode = GhostMode.Frightened;
        FrightenedTicksLeft = ticks;
    }

    /// <summary>
    /// Returns the scatter-corner target for this ghost.
    /// Ghosts 0/1 target the top corners; 2/3 target the bottom corners.
    /// </summary>
    public Position GetScatterTarget(int mazeRows, int mazeCols)
    {
        return (Index % 4) switch
        {
            0 => new Position(0, mazeCols - 1),
            1 => new Position(0, 0),
            2 => new Position(mazeRows - 1, mazeCols - 1),
            _ => new Position(mazeRows - 1, 0)
        };
    }
}
