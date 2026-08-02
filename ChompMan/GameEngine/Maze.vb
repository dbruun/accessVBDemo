Imports System.Collections.Generic
Imports System.Linq

Namespace GameEngine

    ''' <summary>
    ''' Represents the maze grid parsed from a multi-line layout string.
    ''' Characters: # wall  . pellet  o power-pellet  P player-start
    '''             G ghost-start  - ghost-house-door  (space) empty
    ''' </summary>
    Public Class Maze

        Private ReadOnly _grid As CellType(,)
        Private ReadOnly _rows As Integer
        Private ReadOnly _cols As Integer
        Private ReadOnly _playerStart As Position
        Private ReadOnly _ghostStarts As List(Of Position)
        Private ReadOnly _totalPellets As Integer

        ' ── Public read-only properties ─────────────────────────────────────

        ''' <summary>Number of rows in the maze.</summary>
        Public ReadOnly Property Rows As Integer
            Get
                Return _rows
            End Get
        End Property

        ''' <summary>Number of columns in the maze.</summary>
        Public ReadOnly Property Cols As Integer
            Get
                Return _cols
            End Get
        End Property

        ''' <summary>Total pellet + power-pellet count at load time.</summary>
        Public ReadOnly Property TotalPellets As Integer
            Get
                Return _totalPellets
            End Get
        End Property

        ''' <summary>Player spawn position derived from the 'P' character.</summary>
        Public ReadOnly Property PlayerStart As Position
            Get
                Return _playerStart
            End Get
        End Property

        ''' <summary>Ghost spawn positions derived from 'G' characters.</summary>
        Public ReadOnly Property GhostStarts As IReadOnlyList(Of Position)
            Get
                Return _ghostStarts
            End Get
        End Property

        ' ── Constructor ─────────────────────────────────────────────────────

        ''' <summary>Parses a <see cref="Maze"/> from a multi-line layout string.</summary>
        ''' <param name="layout">
        ''' Newline-delimited string where each character maps to a <see cref="CellType"/>.
        ''' </param>
        Public Sub New(layout As String)
            Dim lines = SplitLines(layout)

            _rows = lines.Count
            _cols = If(lines.Count > 0, lines.Max(Function(l) l.Length), 0)
            _grid = New CellType(_rows - 1, _cols - 1) {}
            _ghostStarts = New List(Of Position)()
            Dim pellets As Integer = 0

            For r As Integer = 0 To _rows - 1
                Dim line As String = lines(r)
                For c As Integer = 0 To _cols - 1
                    Dim ch As Char = If(c < line.Length, line(c), " "c)
                    Dim cell As CellType = ParseChar(ch)

                    ' Treat spawn markers as navigable empty space in the grid
                    Select Case cell
                        Case CellType.PlayerStart
                            _playerStart = New Position(r, c)
                            _grid(r, c) = CellType.Empty
                        Case CellType.GhostStart
                            _ghostStarts.Add(New Position(r, c))
                            _grid(r, c) = CellType.Empty
                        Case CellType.Pellet, CellType.PowerPellet
                            pellets += 1
                            _grid(r, c) = cell
                        Case Else
                            _grid(r, c) = cell
                    End Select
                Next
            Next

            _totalPellets = pellets
        End Sub

        ' ── Public methods ───────────────────────────────────────────────────

        ''' <summary>Returns the cell type at <paramref name="pos"/>.</summary>
        Public Function GetCell(pos As Position) As CellType
            If Not InBounds(pos) Then Return CellType.Wall
            Return _grid(pos.Row, pos.Col)
        End Function

        ''' <summary>Overwrites the cell at <paramref name="pos"/> (used to consume pellets).</summary>
        Public Sub SetCell(pos As Position, cellType As CellType)
            If InBounds(pos) Then _grid(pos.Row, pos.Col) = cellType
        End Sub

        ''' <summary>Returns <c>True</c> if <paramref name="pos"/> is within maze bounds.</summary>
        Public Function InBounds(pos As Position) As Boolean
            Return pos.Row >= 0 AndAlso pos.Row < _rows AndAlso
                   pos.Col >= 0 AndAlso pos.Col < _cols
        End Function

        ''' <summary>
        ''' Returns <c>True</c> if a player (or ghost) can enter <paramref name="pos"/>.
        ''' </summary>
        ''' <param name="pos">Target position.</param>
        ''' <param name="isGhost">
        ''' When <c>True</c> ghosts are also allowed to pass the ghost-house door.
        ''' </param>
        Public Function IsPassable(pos As Position, Optional isGhost As Boolean = False) As Boolean
            Dim cell = GetCell(pos)
            If cell = CellType.Wall Then Return False
            If cell = CellType.GhostHouseDoor AndAlso Not isGhost Then Return False
            Return True
        End Function

        ''' <summary>
        ''' Wraps a column index around the horizontal tunnel edges when the maze
        ''' has open (non-wall) cells on both far edges of the same row.
        ''' </summary>
        Public Function WrapCol(row As Integer, col As Integer) As Integer
            If col < 0 Then Return _cols - 1
            If col >= _cols Then Return 0
            Return col
        End Function

        ''' <summary>Counts remaining pellets on the grid.</summary>
        Public Function RemainingPellets() As Integer
            Dim count As Integer = 0
            For r As Integer = 0 To _rows - 1
                For c As Integer = 0 To _cols - 1
                    Dim ct = _grid(r, c)
                    If ct = CellType.Pellet OrElse ct = CellType.PowerPellet Then
                        count += 1
                    End If
                Next
            Next
            Return count
        End Function

        ' ── Private helpers ──────────────────────────────────────────────────

        Private Shared Function ParseChar(ch As Char) As CellType
            Select Case ch
                Case "#"c             : Return CellType.Wall
                Case "."c             : Return CellType.Pellet
                Case "o"c             : Return CellType.PowerPellet
                Case "P"c             : Return CellType.PlayerStart
                Case "G"c             : Return CellType.GhostStart
                Case "-"c             : Return CellType.GhostHouseDoor
                Case Else             : Return CellType.Empty
            End Select
        End Function

        Private Shared Function SplitLines(layout As String) As List(Of String)
            Dim normalised = layout.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
            Dim parts = normalised.Split(vbLf(0))
            Dim result As New List(Of String)(parts)
            ' Trim trailing empty lines
            While result.Count > 0 AndAlso String.IsNullOrEmpty(result(result.Count - 1))
                result.RemoveAt(result.Count - 1)
            End While
            Return result
        End Function

    End Class

End Namespace
