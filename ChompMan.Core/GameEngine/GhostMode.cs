namespace ChompMan.GameEngine;

/// <summary>Behavioural mode of a ghost entity.</summary>
public enum GhostMode
{
    /// <summary>Ghost actively pursues the player or a scatter target.</summary>
    Chase,
    /// <summary>Ghost retreats to its corner scatter target.</summary>
    Scatter,
    /// <summary>Ghost wanders randomly after player ate a power pellet.</summary>
    Frightened,
    /// <summary>Ghost has been eaten and is returning to the ghost house.</summary>
    Eaten
}
