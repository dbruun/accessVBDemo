Imports System.Collections.Generic

Namespace DataAccess

    ''' <summary>A high-score row returned from the data store.</summary>
    Public Class ScoreEntry

        ''' <summary>Player display name.</summary>
        Public Property PlayerName As String = String.Empty

        ''' <summary>Numeric score.</summary>
        Public Property Score As Integer

        ''' <summary>Highest level reached in this run.</summary>
        Public Property LevelReached As Integer

        ''' <summary>Date/time the score was recorded.</summary>
        Public Property PlayedOn As Date

    End Class


    ''' <summary>A level definition row returned from the data store.</summary>
    Public Class LevelData

        ''' <summary>Database level number (1-based).</summary>
        Public Property LevelNumber As Integer

        ''' <summary>Multi-line maze layout string.</summary>
        Public Property MazeLayout As String = String.Empty

        ''' <summary>Ghost speed expressed as ticks-per-move.</summary>
        Public Property GhostSpeed As Integer = 6

        ''' <summary>Number of pellets in the layout.</summary>
        Public Property PelletCount As Integer

    End Class


    ''' <summary>A key/value settings row.</summary>
    Public Class SettingEntry

        ''' <summary>Setting key.</summary>
        Public Property Key As String = String.Empty

        ''' <summary>Setting value (stored as text).</summary>
        Public Property Value As String = String.Empty

    End Class

End Namespace
