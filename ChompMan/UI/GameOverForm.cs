using ChompMan.DataAccess;
using System.Drawing;
using System.Windows.Forms;

namespace ChompMan.UI;

/// <summary>Game-Over screen: shows the final score, prompts for the player name and saves the score.</summary>
public class GameOverForm : Form
{
    private readonly string _message;
    private readonly int _finalScore;
    private readonly int _levelReached;
    private readonly IScoreRepository _scoreRepo;

    private TextBox _txtName;
    private Button _btnSave;
    private Button _btnSkip;

    public GameOverForm(string message, int finalScore, int levelReached, IScoreRepository scoreRepo)
    {
        _message = message;
        _finalScore = finalScore;
        _levelReached = levelReached;
        _scoreRepo = scoreRepo;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "ChompMan – Game Over";
        ClientSize = new Size(380, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.Black;

        var lblMessage = new Label
        {
            Text = _message,
            Font = new Font("Courier New", 22, FontStyle.Bold),
            ForeColor = Color.Yellow,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(380, 60),
            Location = new Point(0, 20)
        };

        var lblScore = new Label
        {
            Text = $"Final Score: {_finalScore:N0}",
            Font = new Font("Courier New", 13),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(380, 35),
            Location = new Point(0, 90)
        };

        var lblPrompt = new Label
        {
            Text = "Enter your name to save:",
            Font = new Font("Courier New", 11),
            ForeColor = Color.Cyan,
            AutoSize = true,
            Location = new Point(30, 140)
        };

        _txtName = new TextBox
        {
            Font = new Font("Courier New", 12),
            MaxLength = 20,
            Size = new Size(200, 28),
            Location = new Point(30, 170)
        };

        _btnSave = new Button
        {
            Text = "SAVE",
            Font = new Font("Courier New", 11, FontStyle.Bold),
            ForeColor = Color.Black,
            BackColor = Color.Yellow,
            Size = new Size(80, 32),
            Location = new Point(240, 170)
        };
        _btnSave.Click += BtnSave_Click;

        _btnSkip = new Button
        {
            Text = "SKIP",
            Font = new Font("Courier New", 11),
            ForeColor = Color.White,
            BackColor = Color.DimGray,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 32),
            Location = new Point(240, 212)
        };
        _btnSkip.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.AddRange(new Control[] { lblMessage, lblScore, lblPrompt, _txtName, _btnSave, _btnSkip });
        AcceptButton = _btnSave;
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        var name = _txtName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("Please enter your name.", "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        TrySaveScore(name);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void TrySaveScore(string name)
    {
        if (_scoreRepo is null)
        {
            return;
        }

        try
        {
            _scoreRepo.SaveScore(name, _finalScore, _levelReached);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not save score: " + ex.Message, "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
