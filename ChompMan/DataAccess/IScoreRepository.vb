Imports System.Collections.Generic

Namespace DataAccess

    ''' <summary>
    ''' Contract for high-score storage.
    ''' Implementations may back this with Access, SQL Server, EF Core, etc.
    ''' </summary>
    Public Interface IScoreRepository

        ''' <summary>Returns the top <paramref name="count"/> scores, ordered descending.</summary>
        Function GetTopScores(Optional count As Integer = 10) As List(Of ScoreEntry)

        ''' <summary>
        ''' Persists a new high-score row.
        ''' Creates the player record if the name has not been seen before.
        ''' </summary>
        Sub SaveScore(playerName As String, score As Integer, levelReached As Integer)

    End Interface

End Namespace
