namespace ChompMan.DataAccess;

/// <summary>
/// Contract for high-score storage.
/// Implementations may back this with Access, SQL Server, EF Core, etc.
/// </summary>
public interface IScoreRepository
{
    /// <summary>Returns the top <paramref name="count"/> scores, ordered descending.</summary>
    List<ScoreEntry> GetTopScores(int count = 10);

    /// <summary>
    /// Persists a new high-score row.
    /// Creates the player record if the name has not been seen before.
    /// </summary>
    void SaveScore(string playerName, int score, int levelReached);
}
