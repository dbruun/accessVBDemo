namespace ChompMan.DataAccess;

/// <summary>A high-score row returned from the data store.</summary>
public class ScoreEntry
{
    /// <summary>Player display name.</summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>Numeric score.</summary>
    public int Score { get; set; }

    /// <summary>Highest level reached in this run.</summary>
    public int LevelReached { get; set; }

    /// <summary>Date/time the score was recorded.</summary>
    public DateTime PlayedOn { get; set; }
}

/// <summary>A level definition row returned from the data store.</summary>
public class LevelData
{
    /// <summary>Database level number (1-based).</summary>
    public int LevelNumber { get; set; }

    /// <summary>Multi-line maze layout string.</summary>
    public string MazeLayout { get; set; } = string.Empty;

    /// <summary>Ghost speed expressed as ticks-per-move.</summary>
    public int GhostSpeed { get; set; } = 6;

    /// <summary>Number of pellets in the layout.</summary>
    public int PelletCount { get; set; }
}

/// <summary>A key/value settings row.</summary>
public class SettingEntry
{
    /// <summary>Setting key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Setting value (stored as text).</summary>
    public string Value { get; set; } = string.Empty;
}
