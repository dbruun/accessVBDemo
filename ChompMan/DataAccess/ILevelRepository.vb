Imports System.Collections.Generic

Namespace DataAccess

    ''' <summary>
    ''' Contract for level-definition storage.
    ''' Implementations may back this with Access, SQL Server, EF Core, etc.
    ''' </summary>
    Public Interface ILevelRepository

        ''' <summary>Returns the level definition for <paramref name="levelNumber"/>.</summary>
        ''' <returns><c>Nothing</c> if the level does not exist.</returns>
        Function GetLevel(levelNumber As Integer) As LevelData

        ''' <summary>Returns all available levels ordered by level number.</summary>
        Function GetAllLevels() As List(Of LevelData)

    End Interface

End Namespace
