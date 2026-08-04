using ChompMan.DataAccess;
using ChompMan.GameEngine;
using System.Drawing;
using System.Windows.Forms;

namespace ChompMan.UI;

/// <summary>
/// Main game screen. Hosts the maze panel, score/lives labels and the
/// game-loop timer. Rendering is done with GDI+ in the Panel's Paint event.
/// </summary>
public class GameForm : Form
{
    private const int CellSize = 22;
    private const int HudHeight = 50;

    private readonly ILevelRepository _levelRepo;
    private readonly IScoreRepository _scoreRepo;

    private System.Windows.Forms.Timer _timer;
    private Engine _engine;
    private GameState _state;
    private List<LevelData> _levelDefs;

    private SolidBrush _wallBrush;
    private SolidBrush _pelletBrush;
    private SolidBrush _powerBrush;
    private SolidBrush _playerBrush;
    private SolidBrush _frightenBrush;
    private SolidBrush _eatenBrush;
    private SolidBrush _bgBrush;
    private Font _hudFont;
    private Font _bigFont;
    private SolidBrush[] _ghostBrushes;

    private BufferedPanel _panel;
    private Label _lblScore;
    private Label _lblLives;
    private Label _lblLevel;

    public GameForm()
    {
        _levelRepo = new SqliteLevelRepository(Program.DbPath);
        _scoreRepo = new SqliteScoreRepository(Program.DbPath);
        _levelDefs = TryLoadLevels();

        InitializeComponent();
        AllocateBrushes();
        StartNewGame();
    }

    private void InitializeComponent()
    {
        Text = "ChompMan";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;
        KeyPreview = true;

        _lblScore = new Label
        {
            Font = new Font("Courier New", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Black,
            AutoSize = true,
            Location = new Point(5, 8)
        };
        _lblLevel = new Label
        {
            Font = new Font("Courier New", 12, FontStyle.Bold),
            ForeColor = Color.Cyan,
            BackColor = Color.Black,
            AutoSize = true,
            Location = new Point(200, 8)
        };
        _lblLives = new Label
        {
            Font = new Font("Courier New", 12, FontStyle.Bold),
            ForeColor = Color.Yellow,
            BackColor = Color.Black,
            AutoSize = true,
            Location = new Point(340, 8)
        };

        _panel = new BufferedPanel
        {
            Location = new Point(0, HudHeight),
            BackColor = Color.Black
        };
        _panel.Paint += Panel_Paint;

        Controls.AddRange(new Control[] { _lblScore, _lblLevel, _lblLives, _panel });

        _timer = new System.Windows.Forms.Timer { Interval = 16 };
        _timer.Tick += Timer_Tick;
    }

    private void AllocateBrushes()
    {
        _wallBrush = new SolidBrush(Color.DarkBlue);
        _pelletBrush = new SolidBrush(Color.WhiteSmoke);
        _powerBrush = new SolidBrush(Color.Orange);
        _playerBrush = new SolidBrush(Color.Yellow);
        _frightenBrush = new SolidBrush(Color.DeepSkyBlue);
        _eatenBrush = new SolidBrush(Color.DimGray);
        _bgBrush = new SolidBrush(Color.Black);
        _hudFont = new Font("Courier New", 11, FontStyle.Bold);
        _bigFont = new Font("Courier New", 22, FontStyle.Bold);
        _ghostBrushes =
        [
            new SolidBrush(Color.Red),
            new SolidBrush(Color.HotPink),
            new SolidBrush(Color.Cyan),
            new SolidBrush(Color.Orange)
        ];
    }

    private void StartNewGame()
    {
        LoadLevel(1);
        _timer.Start();
    }

    private void LoadLevel(int levelNumber)
    {
        var def = GetLevelDef(levelNumber);
        var maze = new Maze(def.MazeLayout);
        var player = new Player(maze.PlayerStart, lives: 3);

        var ghosts = new List<Ghost>();
        var names = new[] { "Blinky", "Pinky", "Inky", "Clyde" };
        for (var i = 0; i <= Math.Min(maze.GhostStarts.Count, 4) - 1; i++)
        {
            var g = new Ghost(i, names[i], maze.GhostStarts[i])
            {
                SpeedTicks = def.GhostSpeed
            };
            ghosts.Add(g);
        }

        _state = new GameState(maze, player, ghosts) { CurrentLevel = levelNumber };
        _engine = new Engine(_state);
        _engine.ScoreChanged += Engine_ScoreChanged;
        _engine.LifeLost += Engine_LifeLost;
        _engine.LevelComplete += Engine_LevelComplete;
        _engine.GameOver += Engine_GameOver;
        _engine.GhostEaten += Engine_GhostEaten;

        ResizeFormToMaze(maze);
        UpdateHud();
    }

    private LevelData GetLevelDef(int levelNumber)
    {
        if (_levelDefs is not null)
        {
            var def = _levelDefs.FirstOrDefault(l => l.LevelNumber == levelNumber);
            if (def is not null)
            {
                return def;
            }
        }

        return new LevelData
        {
            LevelNumber = levelNumber,
            GhostSpeed = Math.Max(2, 7 - levelNumber),
            MazeLayout = BuiltInMaze1()
        };
    }

    private static string BuiltInMaze1()
    {
        return "#####################\n" +
               "#.........#.........#\n" +
               "#.###.###.#.###.###.#\n" +
               "#o###.###.#.###.###o#\n" +
               "#...................#\n" +
               "#.###.#.#####.#.###.#\n" +
               "#.....#...#...#.....#\n" +
               "#####.###.#.###.#####\n" +
               "#####.#.GGG.#.#######\n" +
               "#####.#.....#.#######\n" +
               "#####.#.....#.#######\n" +
               "#####.#########.#####\n" +
               "#####.#.....#.#######\n" +
               "#####.###.#.###.#####\n" +
               "#.....#...#...#.....#\n" +
               "#.###.#.#####.#.###.#\n" +
               "#...................#\n" +
               "#o###.###.P.###.###o#\n" +
               "#.###.###.#.###.###.#\n" +
               "#.........#.........#\n" +
               "#####################";
    }

    private List<LevelData> TryLoadLevels()
    {
        try
        {
            return _levelRepo.GetAllLevels();
        }
        catch
        {
            return null;
        }
    }

    private void ResizeFormToMaze(Maze maze)
    {
        var panelW = maze.Cols * CellSize;
        var panelH = maze.Rows * CellSize;
        _panel.Size = new Size(panelW, panelH);
        ClientSize = new Size(panelW, panelH + HudHeight);
    }

    private void UpdateHud()
    {
        if (_state is null)
        {
            return;
        }

        _lblScore.Text = $"Score: {_state.Player.Score:N0}";
        _lblLevel.Text = $"Level: {_state.CurrentLevel}";
        _lblLives.Text = $"Lives: {_state.Player.Lives}";
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (_state.Phase == GamePhase.Playing || _state.Phase == GamePhase.Respawning)
        {
            _engine.Tick();
            UpdateHud();
        }
        _panel.Invalidate();
    }

    private void Engine_ScoreChanged(object sender, EventArgs e) => UpdateHud();
    private void Engine_LifeLost(object sender, EventArgs e) => UpdateHud();

    private void Engine_LevelComplete(object sender, EventArgs e)
    {
        _timer.Stop();
        MessageBox.Show($"Level {_state.CurrentLevel} complete!  Get ready for the next level.",
            "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Information);
        var nextLevel = _state.CurrentLevel + 1;
        var nextDef = GetLevelDef(nextLevel);
        if (nextDef is null || string.IsNullOrEmpty(nextDef.MazeLayout))
        {
            ShowGameOver(true);
        }
        else
        {
            LoadLevel(nextLevel);
            _timer.Start();
        }
    }

    private void Engine_GameOver(object sender, EventArgs e)
    {
        _timer.Stop();
        ShowGameOver(false);
    }

    private void Engine_GhostEaten(object sender, int score)
    {
    }

    private void ShowGameOver(bool won)
    {
        var msg = won ? "You beat all levels!" : "GAME OVER";
        using var goForm = new GameOverForm(msg, _state.Player.Score, _state.CurrentLevel, _scoreRepo);
        goForm.ShowDialog(this);
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_engine is null)
        {
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Up:
            case Keys.W:
                _engine.SetInput(Direction.Up);
                break;
            case Keys.Down:
            case Keys.S:
                _engine.SetInput(Direction.Down);
                break;
            case Keys.Left:
            case Keys.A:
                _engine.SetInput(Direction.Left);
                break;
            case Keys.Right:
            case Keys.D:
                _engine.SetInput(Direction.Right);
                break;
            case Keys.P:
                _engine.TogglePause();
                break;
            case Keys.R:
                _timer.Stop();
                StartNewGame();
                break;
            case Keys.Escape:
                _timer.Stop();
                Close();
                break;
        }
        e.Handled = true;
    }

    private void Panel_Paint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.Black);
        if (_state is null)
        {
            return;
        }

        DrawMaze(g);
        DrawGhosts(g);
        DrawPlayer(g);
        if (_state.Phase == GamePhase.Paused)
        {
            DrawOverlay(g, "PAUSED");
        }
        if (_state.Phase == GamePhase.Respawning)
        {
            DrawOverlay(g, string.Empty);
        }
    }

    private void DrawMaze(Graphics g)
    {
        var maze = _state.Maze;
        for (var r = 0; r < maze.Rows; r++)
        {
            for (var c = 0; c < maze.Cols; c++)
            {
                var cell = maze.GetCell(new Position(r, c));
                var x = c * CellSize;
                var y = r * CellSize;
                switch (cell)
                {
                    case CellType.Wall:
                        g.FillRectangle(_wallBrush, x, y, CellSize, CellSize);
                        g.DrawRectangle(Pens.MidnightBlue, x + 1, y + 1, CellSize - 3, CellSize - 3);
                        break;
                    case CellType.Pellet:
                        g.FillRectangle(_bgBrush, x, y, CellSize, CellSize);
                        var cx = x + CellSize / 2 - 2;
                        var cy = y + CellSize / 2 - 2;
                        g.FillEllipse(_pelletBrush, cx, cy, 4, 4);
                        break;
                    case CellType.PowerPellet:
                        g.FillRectangle(_bgBrush, x, y, CellSize, CellSize);
                        cx = x + CellSize / 2 - 5;
                        cy = y + CellSize / 2 - 5;
                        g.FillEllipse(_powerBrush, cx, cy, 10, 10);
                        break;
                    default:
                        g.FillRectangle(_bgBrush, x, y, CellSize, CellSize);
                        break;
                }
            }
        }
    }

    private void DrawPlayer(Graphics g)
    {
        if (_state.Phase == GamePhase.Respawning)
        {
            return;
        }

        var pos = _state.Player.Position;
        var x = pos.Col * CellSize + 1;
        var y = pos.Row * CellSize + 1;
        var sz = CellSize - 2;
        var dir = _state.Player.CurrentDirection;
        var startAngle = DirectionToAngle(dir);
        g.FillPie(_playerBrush, x, y, sz, sz, startAngle + 30, 300);
    }

    private void DrawGhosts(Graphics g)
    {
        for (var i = 0; i < _state.Ghosts.Count; i++)
        {
            var ghost = _state.Ghosts[i];
            var x = ghost.Position.Col * CellSize + 1;
            var y = ghost.Position.Row * CellSize + 1;
            var sz = CellSize - 2;

            SolidBrush brush = ghost.Mode switch
            {
                GhostMode.Frightened => _frightenBrush,
                GhostMode.Eaten => _eatenBrush,
                _ => _ghostBrushes[i % _ghostBrushes.Length]
            };

            g.FillEllipse(brush, x, y, sz, sz - 2);
            g.FillRectangle(brush, x, y + sz / 2 - 2, sz, sz / 2 + 2);
            if (ghost.Mode != GhostMode.Eaten)
            {
                g.FillEllipse(Brushes.White, x + 3, y + 4, 5, 5);
                g.FillEllipse(Brushes.White, x + sz - 8, y + 4, 5, 5);
                g.FillEllipse(Brushes.DarkBlue, x + 4, y + 5, 3, 3);
                g.FillEllipse(Brushes.DarkBlue, x + sz - 7, y + 5, 3, 3);
            }
        }
    }

    private void DrawOverlay(Graphics g, string text)
    {
        var sz = _panel.ClientSize;
        using (var overlayBrush = new SolidBrush(Color.FromArgb(160, Color.Black)))
        {
            g.FillRectangle(overlayBrush, 0, 0, sz.Width, sz.Height);
        }

        if (!string.IsNullOrEmpty(text))
        {
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(text, _bigFont, Brushes.Yellow, new RectangleF(0, 0, sz.Width, sz.Height), sf);
        }
    }

    private static float DirectionToAngle(Direction dir)
    {
        return dir switch
        {
            Direction.Right => 0,
            Direction.Down => 90,
            Direction.Left => 180,
            Direction.Up => 270,
            _ => 0
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Dispose();
            _wallBrush?.Dispose();
            _pelletBrush?.Dispose();
            _powerBrush?.Dispose();
            _playerBrush?.Dispose();
            _frightenBrush?.Dispose();
            _eatenBrush?.Dispose();
            _bgBrush?.Dispose();
            _hudFont?.Dispose();
            _bigFont?.Dispose();
            if (_ghostBrushes is not null)
            {
                foreach (var b in _ghostBrushes)
                {
                    b?.Dispose();
                }
            }
        }

        base.Dispose(disposing);
    }
}

/// <summary>A <see cref="Panel"/> with double-buffering enabled.</summary>
internal class BufferedPanel : Panel
{
    public BufferedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        UpdateStyles();
    }
}
