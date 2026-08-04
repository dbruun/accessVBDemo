using ChompMan.DataAccess;
using System.Windows.Forms;

namespace ChompMan;

/// <summary>Application entry point.</summary>
internal static class Program
{
    /// <summary>Database path relative to the executable.</summary>
    internal static string DbPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChompMan.db");

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            var init = new DatabaseInitializer(DbPath);
            init.EnsureCreated();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Could not initialise the ChompMan database." + Environment.NewLine +
                Environment.NewLine +
                ex.Message,
                "ChompMan – Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        Application.Run(new UI.MainMenuForm());
    }
}
