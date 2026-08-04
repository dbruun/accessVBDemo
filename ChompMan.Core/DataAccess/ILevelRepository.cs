namespace ChompMan.DataAccess;

/// <summary>
/// Contract for level-definition storage.
/// Implementations may back this with Access, SQL Server, EF Core, etc.
/// </summary>
public interface ILevelRepository
{
    /// <summary>Returns the level definition for <paramref name="levelNumber"/>.</summary>
    /// <returns><c>null</c> if the level does not exist.</returns>
    LevelData GetLevel(int levelNumber);

    /// <summary>Returns all available levels ordered by level number.</summary>
    List<LevelData> GetAllLevels();
}
