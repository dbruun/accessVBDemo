Imports System
Imports System.IO
Imports System.Windows.Forms
Imports ChompMan.DataAccess

''' <summary>Application entry point.</summary>
Friend Module Program

    ''' <summary>Database path relative to the executable.</summary>
    Friend ReadOnly Property DbPath As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChompMan.accdb")
        End Get
    End Property

    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' Ensure database exists and is seeded
        Try
            Dim init As New DatabaseInitializer(DbPath)
            init.EnsureCreated()
        Catch ex As Exception
            MessageBox.Show(
                "Could not initialise the ChompMan database." & Environment.NewLine &
                Environment.NewLine &
                "Make sure the Microsoft ACE OLEDB 16.0 runtime (64-bit) is installed." &
                Environment.NewLine & Environment.NewLine &
                ex.Message,
                "ChompMan – Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            ' Allow the game to run without DB (scores won't be saved)
        End Try

        Application.Run(New UI.MainMenuForm())
    End Sub

End Module
