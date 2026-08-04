using System.Drawing;
using System.Windows.Forms;

namespace ChompMan.UI;

/// <summary>Main menu screen — New Game, High Scores, Settings, Exit.</summary>
public class MainMenuForm : Form
{
    private Button _btnNewGame;
    private Button _btnHighScores;
    private Button _btnSettings;
    private Button _btnExit;
    private Label _lblTitle;

    public MainMenuForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "ChompMan";
        ClientSize = new Size(400, 480);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;

        _lblTitle = new Label
        {
            Text = "CHOMPM\n  AN",
            Font = new Font("Courier New", 32, FontStyle.Bold),
            ForeColor = Color.Yellow,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(400, 120),
            Location = new Point(0, 40)
        };

        _btnNewGame = MakeButton("NEW GAME", 200);
        _btnHighScores = MakeButton("HIGH SCORES", 265);
        _btnSettings = MakeButton("SETTINGS", 330);
        _btnExit = MakeButton("EXIT", 395);

        _btnNewGame.Click += (_, _) => { using var gameForm = new GameForm(); gameForm.ShowDialog(this); };
        _btnHighScores.Click += (_, _) => { using var hsForm = new HighScoresForm(); hsForm.ShowDialog(this); };
        _btnSettings.Click += (_, _) => { using var settingsForm = new SettingsForm(); settingsForm.ShowDialog(this); };
        _btnExit.Click += (_, _) => Application.Exit();

        Controls.AddRange(new Control[] { _lblTitle, _btnNewGame, _btnHighScores, _btnSettings, _btnExit });
    }

    private static Button MakeButton(string text, int top)
    {
        return new Button
        {
            Text = text,
            Font = new Font("Courier New", 14, FontStyle.Bold),
            ForeColor = Color.Yellow,
            BackColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(220, 45),
            Location = new Point(90, top),
            TabStop = true
        };
    }
}
