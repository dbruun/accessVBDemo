namespace ChompMan.GameEngine;

/// <summary>
/// Core game-update logic. Intentionally free of any WinForms or database
/// dependencies so it can be unit-tested and later ported.
/// </summary>
public class Engine
{
    public const int PelletScore = 10;
    public const int PowerPelletScore = 50;
    public const int GhostBaseScore = 200;
    public const int ExtraLifeThreshold = 10000;

    public const int FrightenedDuration = 180;
    public const int RespawnDelay = 120;
    public const int LevelCompleteDelay = 90;
    public const int ScatterPhaseTicks = 300;
    public const int ChasePhaseTicks = 600;

    private static readonly Random _rng = new();
    private readonly GameState _state;
    private int _lastExtraLifeAt;

    public event EventHandler ScoreChanged;
    public event EventHandler LifeLost;
    public event EventHandler LevelComplete;
    public event EventHandler GameOver;
    public event Action<object, int> GhostEaten;

    public Engine(GameState state)
    {
        _state = state;
    }

    public void Tick()
    {
        switch (_state.Phase)
        {
            case GamePhase.Playing:
                TickModeSwitchTimer();
                MovePlayer();
                MoveGhosts();
                CheckPlayerGhostCollisions();
                break;

            case GamePhase.Respawning:
                _state.DelayTicksLeft -= 1;
                if (_state.DelayTicksLeft <= 0)
                {
                    _state.Phase = GamePhase.Playing;
                }
                break;
        }
    }

    public void SetInput(Direction dir)
    {
        _state.Player.QueuedDirection = dir;
    }

    public void TogglePause()
    {
        if (_state.Phase == GamePhase.Playing)
        {
            _state.Phase = GamePhase.Paused;
        }
        else if (_state.Phase == GamePhase.Paused)
        {
            _state.Phase = GamePhase.Playing;
        }
    }

    public GameState State => _state;

    private void TickModeSwitchTimer()
    {
        if (_state.Ghosts.Any(g => g.Mode == GhostMode.Frightened))
        {
            return;
        }

        _state.ModeSwitchTicksLeft -= 1;
        if (_state.ModeSwitchTicksLeft <= 0)
        {
            if (_state.IsScatterPhase)
            {
                _state.IsScatterPhase = false;
                _state.ModeSwitchTicksLeft = ChasePhaseTicks;
                foreach (var g in _state.Ghosts)
                {
                    if (g.Mode == GhostMode.Scatter)
                    {
                        g.Mode = GhostMode.Chase;
                    }
                }
            }
            else
            {
                _state.IsScatterPhase = true;
                _state.ModeSwitchTicksLeft = ScatterPhaseTicks;
                foreach (var g in _state.Ghosts)
                {
                    if (g.Mode == GhostMode.Chase)
                    {
                        g.Mode = GhostMode.Scatter;
                    }
                }
            }
        }
    }

    private void MovePlayer()
    {
        var p = _state.Player;
        p.TickCounter += 1;
        if (p.TickCounter < p.SpeedTicks)
        {
            return;
        }
        p.TickCounter = 0;

        if (p.QueuedDirection != Direction.None)
        {
            var queuedCell = p.Position.Moved(p.QueuedDirection);
            queuedCell = new Position(queuedCell.Row, _state.Maze.WrapCol(queuedCell.Row, queuedCell.Col));
            if (_state.Maze.IsPassable(queuedCell))
            {
                p.CurrentDirection = p.QueuedDirection;
                p.QueuedDirection = Direction.None;
            }
        }

        if (p.CurrentDirection == Direction.None)
        {
            return;
        }

        var target = p.Position.Moved(p.CurrentDirection);
        target = new Position(target.Row, _state.Maze.WrapCol(target.Row, target.Col));

        if (!_state.Maze.IsPassable(target))
        {
            return;
        }

        p.Position = target;
        CollectPellet(p.Position);
    }

    private void CollectPellet(Position pos)
    {
        var p = _state.Player;
        var cell = _state.Maze.GetCell(pos);
        switch (cell)
        {
            case CellType.Pellet:
                _state.Maze.SetCell(pos, CellType.Empty);
                p.AddScore(PelletScore);
                ScoreChanged?.Invoke(this, EventArgs.Empty);
                CheckLevelComplete();
                break;

            case CellType.PowerPellet:
                _state.Maze.SetCell(pos, CellType.Empty);
                p.AddScore(PowerPelletScore);
                ScoreChanged?.Invoke(this, EventArgs.Empty);
                FrightenAllGhosts();
                _state.GhostsEatenCombo = 0;
                CheckLevelComplete();
                break;
        }
    }

    private void FrightenAllGhosts()
    {
        foreach (var g in _state.Ghosts)
        {
            if (g.Mode != GhostMode.Eaten)
            {
                g.Frighten(FrightenedDuration);
            }
        }
    }

    private void CheckLevelComplete()
    {
        if (_state.Maze.RemainingPellets() == 0)
        {
            _state.Phase = GamePhase.LevelComplete;
            LevelComplete?.Invoke(this, EventArgs.Empty);
        }
    }

    private void MoveGhosts()
    {
        foreach (var g in _state.Ghosts)
        {
            if (g.Mode == GhostMode.Frightened)
            {
                g.FrightenedTicksLeft -= 1;
                if (g.FrightenedTicksLeft <= 0)
                {
                    g.Mode = _state.IsScatterPhase ? GhostMode.Scatter : GhostMode.Chase;
                }
            }

            g.TickCounter += 1;
            var effectiveSpeed = g.SpeedTicks;
            if (g.Mode == GhostMode.Frightened)
            {
                effectiveSpeed = g.SpeedTicks + 2;
            }
            if (g.Mode == GhostMode.Eaten)
            {
                effectiveSpeed = Math.Max(1, g.SpeedTicks - 2);
            }
            if (g.TickCounter < effectiveSpeed)
            {
                continue;
            }
            g.TickCounter = 0;

            var nextDir = ChooseGhostDirection(g);
            if (nextDir != Direction.None)
            {
                var nextPos = g.Position.Moved(nextDir);
                nextPos = new Position(nextPos.Row, _state.Maze.WrapCol(nextPos.Row, nextPos.Col));
                g.Position = nextPos;
                g.CurrentDirection = nextDir;

                if (g.Mode == GhostMode.Eaten && g.Position == g.SpawnPosition)
                {
                    g.Mode = _state.IsScatterPhase ? GhostMode.Scatter : GhostMode.Chase;
                }
            }
        }
    }

    private Direction ChooseGhostDirection(Ghost g)
    {
        var candidates = GetPassableDirections(g.Position, true);
        if (candidates.Count == 0)
        {
            return Direction.None;
        }

        var noReverse = candidates.Where(d => d != Opposite(g.CurrentDirection)).ToList();
        var choices = noReverse.Count > 0 ? noReverse : candidates;

        switch (g.Mode)
        {
            case GhostMode.Frightened:
                return choices[_rng.Next(choices.Count)];
            case GhostMode.Eaten:
                return BestDirection(choices, g.Position, g.SpawnPosition);
            case GhostMode.Scatter:
                var target = g.GetScatterTarget(_state.Maze.Rows, _state.Maze.Cols);
                if (_rng.NextDouble() < 0.2)
                {
                    return choices[_rng.Next(choices.Count)];
                }
                return BestDirection(choices, g.Position, target);
            default:
                var playerPos = _state.Player.Position;
                switch (g.Index % 3)
                {
                    case 0:
                        return BestDirection(choices, g.Position, playerPos);
                    case 1:
                        var ahead = playerPos.Moved(_state.Player.CurrentDirection).Moved(_state.Player.CurrentDirection);
                        return BestDirection(choices, g.Position, ahead);
                    default:
                        if (_rng.NextDouble() < 0.6)
                        {
                            return BestDirection(choices, g.Position, playerPos);
                        }
                        return choices[_rng.Next(choices.Count)];
                }
        }
    }

    internal List<Direction> GetPassableDirections(Position pos, bool isGhost)
    {
        var result = new List<Direction>();
        foreach (var d in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
        {
            var candidate = pos.Moved(d);
            candidate = new Position(candidate.Row, _state.Maze.WrapCol(candidate.Row, candidate.Col));
            if (_state.Maze.IsPassable(candidate, isGhost))
            {
                result.Add(d);
            }
        }
        return result;
    }

    private static Direction BestDirection(List<Direction> choices, Position from, Position target)
    {
        var best = choices[0];
        var bestDist = int.MaxValue;
        foreach (var d in choices)
        {
            var dist = from.Moved(d).ManhattanDistance(target);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = d;
            }
        }
        return best;
    }

    private static Direction Opposite(Direction d)
    {
        return d switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.None
        };
    }

    private void CheckPlayerGhostCollisions()
    {
        var player = _state.Player;
        foreach (var g in _state.Ghosts)
        {
            if (g.Position != player.Position)
            {
                continue;
            }

            if (g.Mode == GhostMode.Frightened)
            {
                g.Mode = GhostMode.Eaten;
                g.FrightenedTicksLeft = 0;
                _state.GhostsEatenCombo += 1;
                var bonus = GhostBaseScore * (int)Math.Pow(2, _state.GhostsEatenCombo - 1);
                player.AddScore(bonus);
                GhostEaten?.Invoke(this, bonus);
                ScoreChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (g.Mode == GhostMode.Chase || g.Mode == GhostMode.Scatter)
            {
                player.Lives -= 1;
                if (player.Lives <= 0)
                {
                    _state.Phase = GamePhase.GameOver;
                    GameOver?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    LifeLost?.Invoke(this, EventArgs.Empty);
                    RespawnEntities();
                }
                return;
            }
        }

        if (player.Score / ExtraLifeThreshold > _lastExtraLifeAt)
        {
            _lastExtraLifeAt = player.Score / ExtraLifeThreshold;
            player.Lives += 1;
        }
    }

    private void RespawnEntities()
    {
        _state.Player.Respawn(_state.Maze.PlayerStart);
        foreach (var g in _state.Ghosts)
        {
            g.Respawn();
        }
        _state.Phase = GamePhase.Respawning;
        _state.DelayTicksLeft = RespawnDelay;
        _state.GhostsEatenCombo = 0;
    }
}
