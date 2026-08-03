Imports System
Imports System.Collections.Generic

Namespace DataAccess

    Public Class PlayerEntity
        Public Property PlayerId As Integer
        Public Property Name As String = String.Empty
        Public Property CreatedOn As DateTime
        Public Property HighScores As ICollection(Of HighScoreEntity) = New List(Of HighScoreEntity)()
    End Class

    Public Class HighScoreEntity
        Public Property ScoreId As Integer
        Public Property PlayerId As Integer
        Public Property Player As PlayerEntity
        Public Property Score As Integer
        Public Property LevelReached As Integer
        Public Property PlayedOn As DateTime
    End Class

    Public Class LevelEntity
        Public Property LevelId As Integer
        Public Property LevelNumber As Integer
        Public Property MazeLayout As String = String.Empty
        Public Property GhostSpeed As Integer
        Public Property PelletCount As Integer
    End Class

    Public Class SettingEntity
        Public Property Key As String = String.Empty
        Public Property Value As String = String.Empty
    End Class

End Namespace
