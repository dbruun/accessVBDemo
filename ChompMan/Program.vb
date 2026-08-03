Imports System
Imports System.Windows.Forms
Imports ChompMan.DataAccess
Imports Microsoft.EntityFrameworkCore

''' <summary>Application entry point.</summary>
Friend Module Program

    Private ReadOnly Property ConnectionString As String
        Get
            Return Environment.GetEnvironmentVariable("CHOMPMAN_CONNECTION_STRING",
                EnvironmentVariableTarget.Process)
        End Get
    End Property

    Friend Function CreateScoreRepository() As IScoreRepository
        Return New EfScoreRepository(New ChompManDbContext(CreateDbContextOptions()))
    End Function

    Friend Function CreateLevelRepository() As ILevelRepository
        Return New EfLevelRepository(New ChompManDbContext(CreateDbContextOptions()))
    End Function

    Friend Function CreateSettingsRepository() As EfSettingsRepository
        Return New EfSettingsRepository(New ChompManDbContext(CreateDbContextOptions()))
    End Function

    Private Function CreateDbContextOptions() As DbContextOptions(Of ChompManDbContext)
        Dim dbConnectionString = ConnectionString
        If String.IsNullOrWhiteSpace(dbConnectionString) Then
            dbConnectionString = "Server=(localdb)\MSSQLLocalDB;Database=ChompMan;Integrated Security=True;TrustServerCertificate=True"
        End If
        Return New DbContextOptionsBuilder(Of ChompManDbContext)().UseSqlServer(dbConnectionString).Options
    End Function

    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Try
            Using context As New ChompManDbContext(CreateDbContextOptions())
                context.Database.Migrate()
            End Using
        Catch ex As Exception
            MessageBox.Show(
                "Could not initialise the ChompMan database." & Environment.NewLine &
                Environment.NewLine &
                "Set CHOMPMAN_CONNECTION_STRING to a valid SQL Server or Azure SQL connection string." &
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
