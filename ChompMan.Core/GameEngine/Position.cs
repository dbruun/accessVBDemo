namespace ChompMan.GameEngine;

/// <summary>Immutable row/column grid coordinate.</summary>
public struct Position : IEquatable<Position>
{
    /// <summary>Zero-based row index.</summary>
    public int Row { get; }

    /// <summary>Zero-based column index.</summary>
    public int Col { get; }

    /// <summary>Initialises a new <see cref="Position"/>.</summary>
    public Position(int row, int col)
    {
        Row = row;
        Col = col;
    }

    /// <inheritdoc/>
    public bool Equals(Position other)
    {
        return Row == other.Row && Col == other.Col;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is Position other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return (Row * 397) ^ Col;
    }

    /// <summary>Equality operator.</summary>
    public static bool operator ==(Position left, Position right)
    {
        return left.Equals(right);
    }

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(Position left, Position right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Returns a new <see cref="Position"/> stepped one cell in <paramref name="dir"/>.
    /// </summary>
    public Position Moved(Direction dir)
    {
        return dir switch
        {
            Direction.Up => new Position(Row - 1, Col),
            Direction.Down => new Position(Row + 1, Col),
            Direction.Left => new Position(Row, Col - 1),
            Direction.Right => new Position(Row, Col + 1),
            _ => this
        };
    }

    /// <summary>Returns the Manhattan distance to <paramref name="other"/>.</summary>
    public int ManhattanDistance(Position other)
    {
        return Math.Abs(Row - other.Row) + Math.Abs(Col - other.Col);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({Row},{Col})";
    }
}
