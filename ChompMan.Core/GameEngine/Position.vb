Namespace GameEngine

    ''' <summary>Immutable row/column grid coordinate.</summary>
    Public Structure Position
        Implements IEquatable(Of Position)

        ''' <summary>Zero-based row index.</summary>
        Public ReadOnly Property Row As Integer

        ''' <summary>Zero-based column index.</summary>
        Public ReadOnly Property Col As Integer

        ''' <summary>Initialises a new <see cref="Position"/>.</summary>
        Public Sub New(row As Integer, col As Integer)
            Me.Row = row
            Me.Col = col
        End Sub

        ''' <inheritdoc/>
        Public Overloads Function Equals(other As Position) As Boolean _
            Implements IEquatable(Of Position).Equals
            Return Row = other.Row AndAlso Col = other.Col
        End Function

        ''' <inheritdoc/>
        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj Is Position Then
                Return Equals(DirectCast(obj, Position))
            End If
            Return False
        End Function

        ''' <inheritdoc/>
        Public Overrides Function GetHashCode() As Integer
            Return (Row * 397) Xor Col
        End Function

        ''' <summary>Equality operator.</summary>
        Public Shared Operator =(left As Position, right As Position) As Boolean
            Return left.Equals(right)
        End Operator

        ''' <summary>Inequality operator.</summary>
        Public Shared Operator <>(left As Position, right As Position) As Boolean
            Return Not left.Equals(right)
        End Operator

        ''' <summary>
        ''' Returns a new <see cref="Position"/> stepped one cell in <paramref name="dir"/>.
        ''' </summary>
        Public Function Moved(dir As Direction) As Position
            Select Case dir
                Case Direction.Up    : Return New Position(Row - 1, Col)
                Case Direction.Down  : Return New Position(Row + 1, Col)
                Case Direction.Left  : Return New Position(Row, Col - 1)
                Case Direction.Right : Return New Position(Row, Col + 1)
                Case Else            : Return Me
            End Select
        End Function

        ''' <summary>Returns the Manhattan distance to <paramref name="other"/>.</summary>
        Public Function ManhattanDistance(other As Position) As Integer
            Return Math.Abs(Row - other.Row) + Math.Abs(Col - other.Col)
        End Function

        ''' <inheritdoc/>
        Public Overrides Function ToString() As String
            Return $"({Row},{Col})"
        End Function

    End Structure

End Namespace
