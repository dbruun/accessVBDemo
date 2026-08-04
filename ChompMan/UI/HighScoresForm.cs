using ChompMan.DataAccess;
using System.Drawing;
using System.Windows.Forms;

namespace ChompMan.UI;

/// <summary>Displays the top-10 high scores loaded from the SQLite database.</summary>
public class HighScoresForm : Form
{
    private Button _btnClose;
    private ListView _listView;

    public HighScoresForm()
    {
        InitializeComponent();
        LoadScores();
    }

    private void InitializeComponent()
    {
        Text = "ChompMan – High Scores";
        ClientSize = new Size(500, 430);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.Black;

        var lblTitle = new Label
        {
            Text = "HIGH SCORES",
            Font = new Font("Courier New", 20, FontStyle.Bold),
            ForeColor = Color.Yellow,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(500, 50),
            Location = new Point(0, 10)
        };

        _listView = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = Color.Black,
            ForeColor = Color.White,
            Font = new Font("Courier New", 11),
            Location = new Point(20, 75),
            Size = new Size(460, 290)
        };
        _listView.Columns.Add("#", 40);
        _listView.Columns.Add("Name", 130);
        _listView.Columns.Add("Score", 100);
        _listView.Columns.Add("Level", 70);
        _listView.Columns.Add("Date", 110);

        _btnClose = new Button
        {
            Text = "CLOSE",
            Font = new Font("Courier New", 12, FontStyle.Bold),
            ForeColor = Color.Yellow,
            BackColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 38),
            Location = new Point(190, 378)
        };
        _btnClose.Click += (_, _) => Close();

        Controls.AddRange(new Control[] { lblTitle, _listView, _btnClose });
    }

    private void LoadScores()
    {
        try
        {
            var repo = new SqliteScoreRepository(Program.DbPath);
            var scores = repo.GetTopScores(10);
            PopulateList(scores);
        }
        catch (Exception ex)
        {
            _listView.Items.Add(new ListViewItem(new[] { "", "DB unavailable", ex.Message, "", "" }));
        }
    }

    private void PopulateList(List<ScoreEntry> scores)
    {
        _listView.Items.Clear();
        for (var i = 0; i < scores.Count; i++)
        {
            var s = scores[i];
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(s.PlayerName);
            item.SubItems.Add(s.Score.ToString("N0"));
            item.SubItems.Add(s.LevelReached.ToString());
            item.SubItems.Add(s.PlayedOn.ToLocalTime().ToString("yyyy-MM-dd"));
            if (i == 0)
            {
                item.ForeColor = Color.Gold;
            }
            _listView.Items.Add(item);
        }
    }
}
