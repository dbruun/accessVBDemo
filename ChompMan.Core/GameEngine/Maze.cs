namespace ChompMan.GameEngine;

/// <summary>
/// Represents the maze grid parsed from a multi-line layout string.
/// Characters: # wall  . pellet  o power-pellet  P player-start
///             G ghost-start  - ghost-house-door  (space) empty
/// </summary>
public class Maze
{
    private readonly CellType[,] _grid;
    private readonly int _rows;
    private readonly int _cols;
    private readonly Position _playerStart;
    private readonly List<Position> _ghostStarts;
    private readonly int _totalPellets;

    public int Rows => _rows;
    public int Cols => _cols;
    public int TotalPellets => _totalPellets;
    public Position PlayerStart => _playerStart;
    public IReadOnlyList<Position> GhostStarts => _ghostStarts;

    public Maze(string layout)
    {
        var lines = SplitLines(layout);
        _rows = lines.Count;
        _cols = lines.Count > 0 ? lines.Max(l => l.Length) : 0;
        _grid = new CellType[_rows, _cols];
        _ghostStarts = new List<Position>();
        var pellets = 0;

        for (var r = 0; r < _rows; r++)
        {
            var line = lines[r];
            for (var c = 0; c < _cols; c++)
            {
                var ch = c < line.Length ? line[c] : ' ';
                var cell = ParseChar(ch);
                switch (cell)
                {
                    case CellType.PlayerStart:
                        _playerStart = new Position(r, c);
                        _grid[r, c] = CellType.Empty;
                        break;
                    case CellType.GhostStart:
                        _ghostStarts.Add(new Position(r, c));
                        _grid[r, c] = CellType.Empty;
                        break;
                    case CellType.Pellet:
                    case CellType.PowerPellet:
                        pellets += 1;
                        _grid[r, c] = cell;
                        break;
                    default:
                        _grid[r, c] = cell;
                        break;
                }
            }
        }

        _totalPellets = pellets;
    }

    public CellType GetCell(Position pos)
    {
        if (!InBounds(pos))
        {
            return CellType.Wall;
        }

        return _grid[pos.Row, pos.Col];
    }

    public void SetCell(Position pos, CellType cellType)
    {
        if (InBounds(pos))
        {
            _grid[pos.Row, pos.Col] = cellType;
        }
    }

    public bool InBounds(Position pos)
    {
        return pos.Row >= 0 && pos.Row < _rows && pos.Col >= 0 && pos.Col < _cols;
    }

    public bool IsPassable(Position pos, bool isGhost = false)
    {
        var cell = GetCell(pos);
        if (cell == CellType.Wall)
        {
            return false;
        }

        if (cell == CellType.GhostHouseDoor && !isGhost)
        {
            return false;
        }

        return true;
    }

    public int WrapCol(int row, int col)
    {
        if (col < 0)
        {
            return _cols - 1;
        }

        if (col >= _cols)
        {
            return 0;
        }

        return col;
    }

    public int RemainingPellets()
    {
        var count = 0;
        for (var r = 0; r < _rows; r++)
        {
            for (var c = 0; c < _cols; c++)
            {
                var ct = _grid[r, c];
                if (ct == CellType.Pellet || ct == CellType.PowerPellet)
                {
                    count += 1;
                }
            }
        }

        return count;
    }

    private static CellType ParseChar(char ch)
    {
        return ch switch
        {
            '#' => CellType.Wall,
            '.' => CellType.Pellet,
            'o' => CellType.PowerPellet,
            'P' => CellType.PlayerStart,
            'G' => CellType.GhostStart,
            '-' => CellType.GhostHouseDoor,
            _ => CellType.Empty
        };
    }

    private static List<string> SplitLines(string layout)
    {
        var normalised = layout.Replace("\r\n", "\n").Replace("\r", "\n");
        var parts = normalised.Split('\n');
        var result = new List<string>(parts);
        while (result.Count > 0 && string.IsNullOrEmpty(result[^1]))
        {
            result.RemoveAt(result.Count - 1);
        }

        return result;
    }
}
