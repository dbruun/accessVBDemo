Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.EntityFrameworkCore

Namespace DataAccess

    Public Class EfLevelRepository
        Implements ILevelRepository

        Private ReadOnly _context As ChompManDbContext

        Public Sub New(context As ChompManDbContext)
            _context = context
        End Sub

        Public Function GetLevel(levelNumber As Integer) As LevelData Implements ILevelRepository.GetLevel
            Return _context.Levels.AsNoTracking().
                Where(Function(level) level.LevelNumber = levelNumber).
                Select(Function(level) New LevelData With {
                    .LevelNumber = level.LevelNumber,
                    .MazeLayout = level.MazeLayout,
                    .GhostSpeed = level.GhostSpeed,
                    .PelletCount = level.PelletCount
                }).SingleOrDefault()
        End Function

        Public Function GetAllLevels() As List(Of LevelData) Implements ILevelRepository.GetAllLevels
            Return _context.Levels.AsNoTracking().
                OrderBy(Function(level) level.LevelNumber).
                Select(Function(level) New LevelData With {
                    .LevelNumber = level.LevelNumber,
                    .MazeLayout = level.MazeLayout,
                    .GhostSpeed = level.GhostSpeed,
                    .PelletCount = level.PelletCount
                }).ToList()
        End Function

    End Class

End Namespace
