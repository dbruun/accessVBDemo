namespace ChompMan.GameEngine;

/// <summary>High-level phases of the game state machine.</summary>
public enum GamePhase
{
    /// <summary>Main menu is shown.</summary>
    Menu,
    /// <summary>Gameplay is active.</summary>
    Playing,
    /// <summary>Game is paused by the player.</summary>
    Paused,
    /// <summary>All lives lost; game-over screen shown.</summary>
    GameOver,
    /// <summary>All pellets cleared; transitioning to next level.</summary>
    LevelComplete,
    /// <summary>Brief respawn delay after player is caught.</summary>
    Respawning
}
