Imports System.Collections.Generic

Namespace GameEngine

    ''' <summary>
    ''' Immutable snapshot of the maze-level definition loaded from the database.
    ''' </summary>
    Public Class LevelDefinition

        ''' <summary>Database level number (1-based).</summary>
        Public ReadOnly Property LevelNumber As Integer

        ''' <summary>Multi-line maze layout string.</summary>
        Public ReadOnly Property MazeLayout As String

        ''' <summary>Ghost speed expressed as ticks-per-move (lower = faster).</summary>
        Public ReadOnly Property GhostSpeedTicks As Integer

        ''' <summary>Expected pellet count (informational).</summary>
        Public ReadOnly Property PelletCount As Integer

        ''' <summary>Initialises a new <see cref="LevelDefinition"/>.</summary>
        Public Sub New(levelNumber As Integer, mazeLayout As String,
                       ghostSpeedTicks As Integer, pelletCount As Integer)
            Me.LevelNumber = levelNumber
            Me.MazeLayout = mazeLayout
            Me.GhostSpeedTicks = ghostSpeedTicks
            Me.PelletCount = pelletCount
        End Sub

    End Class


    ''' <summary>Encapsulates the complete runtime state of a game session.</summary>
    Public Class GameState

        ' ── Structural state ─────────────────────────────────────────────────

        ''' <summary>Parsed maze for the current level.</summary>
        Public ReadOnly Property Maze As Maze

        ''' <summary>The player entity.</summary>
        Public ReadOnly Property Player As Player

        ''' <summary>All ghost entities in play.</summary>
        Public ReadOnly Property Ghosts As IReadOnlyList(Of Ghost)

        ' ── Session bookkeeping ──────────────────────────────────────────────

        ''' <summary>1-based current level number.</summary>
        Public Property CurrentLevel As Integer = 1

        ''' <summary>Current game phase.</summary>
        Public Property Phase As GamePhase = GamePhase.Playing

        ''' <summary>Ticks remaining in a mode-switch or respawn delay.</summary>
        Public Property DelayTicksLeft As Integer = 0

        ''' <summary>Number of ghosts eaten since the last power pellet (for bonus scoring).</summary>
        Public Property GhostsEatenCombo As Integer = 0

        ''' <summary>
        ''' Ticks remaining before ghosts switch from Scatter ↔ Chase.
        ''' Negative means Chase phase is active.
        ''' </summary>
        Public Property ModeSwitchTicksLeft As Integer = 300

        ''' <summary>Whether we are currently in scatter (vs chase) phase.</summary>
        Public Property IsScatterPhase As Boolean = True

        ' ── Constructor ──────────────────────────────────────────────────────

        ''' <summary>Builds game state from a parsed maze and ghost list.</summary>
        Public Sub New(maze As Maze, player As Player, ghosts As List(Of Ghost))
            Me.Maze = maze
            Me.Player = player
            Me.Ghosts = ghosts.AsReadOnly()
        End Sub

    End Class

End Namespace
