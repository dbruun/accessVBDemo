Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.EntityFrameworkCore

Namespace DataAccess

    Public Class EfScoreRepository
        Implements IScoreRepository

        Private ReadOnly _context As ChompManDbContext

        Public Sub New(context As ChompManDbContext)
            _context = context
        End Sub

        Public Function GetTopScores(Optional count As Integer = 10) As List(Of ScoreEntry) _
            Implements IScoreRepository.GetTopScores

            If count < 1 Then Throw New ArgumentOutOfRangeException(NameOf(count))

            Return _context.HighScores.AsNoTracking().
                OrderByDescending(Function(score) score.Score).
                Take(count).
                Select(Function(score) New ScoreEntry With {
                    .PlayerName = score.Player.Name,
                    .Score = score.Score,
                    .LevelReached = score.LevelReached,
                    .PlayedOn = score.PlayedOn
                }).ToList()
        End Function

        Public Sub SaveScore(playerName As String, score As Integer, levelReached As Integer) _
            Implements IScoreRepository.SaveScore

            If String.IsNullOrWhiteSpace(playerName) Then Throw New ArgumentException("Player name is required.", NameOf(playerName))

            Dim player = _context.Players.SingleOrDefault(Function(candidate) candidate.Name = playerName)
            If player Is Nothing Then
                player = New PlayerEntity With {.Name = playerName, .CreatedOn = DateTime.UtcNow}
                _context.Players.Add(player)
            End If

            _context.HighScores.Add(New HighScoreEntity With {
                .Player = player,
                .Score = score,
                .LevelReached = levelReached,
                .PlayedOn = DateTime.UtcNow
            })
            _context.SaveChanges()
        End Sub

    End Class

End Namespace
