Namespace GameEngine

    ''' <summary>Represents the player-controlled character.</summary>
    Public Class Player

        ' ── State ───────────────────────────────────────────────────────────

        ''' <summary>Current grid position.</summary>
        Public Property Position As Position

        ''' <summary>Direction currently being travelled.</summary>
        Public Property CurrentDirection As Direction = Direction.None

        ''' <summary>Next direction the player has queued via key input.</summary>
        Public Property QueuedDirection As Direction = Direction.None

        ''' <summary>Accumulated score for this game session.</summary>
        Public Property Score As Integer = 0

        ''' <summary>Remaining lives (default 3).</summary>
        Public Property Lives As Integer = 3

        ''' <summary>Ticks between each movement step.</summary>
        Public Property SpeedTicks As Integer = 4

        ''' <summary>Internal tick counter used to pace movement.</summary>
        Public Property TickCounter As Integer = 0

        ' ── Constructor ─────────────────────────────────────────────────────

        ''' <summary>Creates a player at the given spawn <paramref name="startPos"/>.</summary>
        Public Sub New(startPos As Position, Optional lives As Integer = 3)
            Position = startPos
            Me.Lives = lives
        End Sub

        ' ── Methods ─────────────────────────────────────────────────────────

        ''' <summary>Resets position and direction to spawn state.</summary>
        Public Sub Respawn(startPos As Position)
            Position = startPos
            CurrentDirection = Direction.None
            QueuedDirection = Direction.None
            TickCounter = 0
        End Sub

        ''' <summary>Adds <paramref name="points"/> to the player score.</summary>
        Public Sub AddScore(points As Integer)
            Score += points
        End Sub

    End Class


    ''' <summary>Represents a single ghost entity.</summary>
    Public Class Ghost

        ' ── Identity ────────────────────────────────────────────────────────

        ''' <summary>Ghost index (0–3); also used to pick scatter-corner target.</summary>
        Public ReadOnly Property Index As Integer

        ''' <summary>Display colour identifier (maps to a <see cref="System.Drawing.Color"/> in the UI).</summary>
        Public ReadOnly Property ColourName As String

        ' ── State ───────────────────────────────────────────────────────────

        ''' <summary>Current grid position.</summary>
        Public Property Position As Position

        ''' <summary>Direction currently being travelled.</summary>
        Public Property CurrentDirection As Direction = Direction.None

        ''' <summary>Behavioural mode.</summary>
        Public Property Mode As GhostMode = GhostMode.Scatter

        ''' <summary>Original spawn position used when the ghost is eaten and returns home.</summary>
        Public ReadOnly Property SpawnPosition As Position

        ''' <summary>Remaining ticks in <see cref="GhostMode.Frightened"/> mode.</summary>
        Public Property FrightenedTicksLeft As Integer = 0

        ''' <summary>Ticks between each movement step (lower = faster).</summary>
        Public Property SpeedTicks As Integer = 6

        ''' <summary>Internal tick counter for movement pacing.</summary>
        Public Property TickCounter As Integer = 0

        ' ── Constructor ─────────────────────────────────────────────────────

        ''' <summary>Creates a ghost with the given identity and spawn position.</summary>
        Public Sub New(index As Integer, colourName As String, spawnPos As Position)
            Me.Index = index
            Me.ColourName = colourName
            SpawnPosition = spawnPos
            Position = spawnPos
        End Sub

        ' ── Methods ─────────────────────────────────────────────────────────

        ''' <summary>Resets the ghost to spawn state.</summary>
        Public Sub Respawn()
            Position = SpawnPosition
            CurrentDirection = Direction.None
            Mode = GhostMode.Scatter
            FrightenedTicksLeft = 0
            TickCounter = 0
        End Sub

        ''' <summary>Enters frightened mode for the given number of ticks.</summary>
        Public Sub Frighten(ticks As Integer)
            Mode = GhostMode.Frightened
            FrightenedTicksLeft = ticks
        End Sub

        ''' <summary>
        ''' Returns the scatter-corner target for this ghost.
        ''' Ghosts 0/1 target the top corners; 2/3 target the bottom corners.
        ''' </summary>
        Public Function GetScatterTarget(mazeRows As Integer, mazeCols As Integer) As Position
            Select Case Index Mod 4
                Case 0 : Return New Position(0, mazeCols - 1)           ' top-right
                Case 1 : Return New Position(0, 0)                       ' top-left
                Case 2 : Return New Position(mazeRows - 1, mazeCols - 1) ' bottom-right
                Case Else : Return New Position(mazeRows - 1, 0)         ' bottom-left
            End Select
        End Function

    End Class

End Namespace
