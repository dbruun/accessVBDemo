using ChompMan.DataAccess;
using System.Drawing;
using System.Windows.Forms;

namespace ChompMan.UI;

/// <summary>Displays and allows editing of Settings key/value rows stored in the database.</summary>
public class SettingsForm : Form
{
    private Button _btnSave;
    private Button _btnClose;
    private DataGridView _grid;

    public SettingsForm()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        Text = "ChompMan – Settings";
        ClientSize = new Size(420, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.Black;

        var lblTitle = new Label
        {
            Text = "SETTINGS",
            Font = new Font("Courier New", 18, FontStyle.Bold),
            ForeColor = Color.Yellow,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(420, 44),
            Location = new Point(0, 10)
        };

        _grid = new DataGridView
        {
            Location = new Point(10, 65),
            Size = new Size(400, 270),
            BackgroundColor = Color.Black,
            GridColor = Color.DimGray,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Courier New", 10)
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold)
            },
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.CellSelect
        };
        _grid.Columns.Add("Key", "Key");
        _grid.Columns.Add("Value", "Value");
        _grid.Columns[0].Width = 190;
        _grid.Columns[0].ReadOnly = true;
        _grid.Columns[1].Width = 200;

        _btnSave = new Button
        {
            Text = "SAVE",
            Font = new Font("Courier New", 12, FontStyle.Bold),
            ForeColor = Color.Black,
            BackColor = Color.Yellow,
            Size = new Size(100, 36),
            Location = new Point(100, 348)
        };
        _btnSave.Click += BtnSave_Click;

        _btnClose = new Button
        {
            Text = "CLOSE",
            Font = new Font("Courier New", 12),
            ForeColor = Color.White,
            BackColor = Color.DimGray,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 36),
            Location = new Point(215, 348)
        };
        _btnClose.Click += (_, _) => Close();

        Controls.AddRange(new Control[] { lblTitle, _grid, _btnSave, _btnClose });
    }

    private void LoadSettings()
    {
        _grid.Rows.Clear();
        try
        {
            var repo = new SqliteSettingsRepository(Program.DbPath);
            var settings = repo.GetAll();
            foreach (var s in settings)
            {
                AddSettingRow(s.Key, s.Value);
            }
        }
        catch
        {
            foreach (var pair in DefaultSettings())
            {
                AddSettingRow(pair.Key, pair.Value);
            }
        }
    }

    private void AddSettingRow(string key, string value)
    {
        var index = _grid.Rows.Add(key, value);
        _grid.Rows[index].Tag = key;
    }

    private static Dictionary<string, string> DefaultSettings()
    {
        return new Dictionary<string, string>
        {
            { "StartingLives", "3" },
            { "PlayerSpeedTicks", "4" },
            { "GhostSpeedTicks", "6" },
            { "FrightenedDuration", "180" },
            { "PlayerColour", "Yellow" },
            { "WallColour", "DarkBlue" },
            { "PelletColour", "White" },
            { "PowerPelletColour", "Orange" }
        };
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        try
        {
            var repo = new SqliteSettingsRepository(Program.DbPath);
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow || row.Tag is not string key)
                {
                    continue;
                }

                var value = Convert.ToString(row.Cells[1].Value) ?? string.Empty;
                repo.Upsert(key, value);
            }

            MessageBox.Show("Settings saved.", "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not save settings: " + ex.Message, "ChompMan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
