Imports Microsoft.EntityFrameworkCore.Infrastructure
Imports Microsoft.EntityFrameworkCore.Migrations

Namespace DataAccess.Migrations

    <DbContext(GetType(ChompManDbContext))>
    <Migration("20260803223220_InitialCreate")>
    Public Partial Class InitialCreate
        Inherits Migration

        Protected Overrides Sub Up(migrationBuilder As MigrationBuilder)
            migrationBuilder.CreateTable(
                name:="Players",
                columns:=Function(table) New With {
                    .PlayerId = table.Column(Of Integer)(type:="int", nullable:=False).Annotation("SqlServer:Identity", "1, 1"),
                    .Name = table.Column(Of String)(type:="nvarchar(100)", maxLength:=100, nullable:=False),
                    .CreatedOn = table.Column(Of DateTime)(type:="datetime2", nullable:=False)
                },
                constraints:=Sub(table)
                    table.PrimaryKey("PK_Players", Function(player) player.PlayerId)
                End Sub)

            migrationBuilder.CreateTable(
                name:="Levels",
                columns:=Function(table) New With {
                    .LevelId = table.Column(Of Integer)(type:="int", nullable:=False).Annotation("SqlServer:Identity", "1, 1"),
                    .LevelNumber = table.Column(Of Integer)(type:="int", nullable:=False),
                    .MazeLayout = table.Column(Of String)(type:="nvarchar(max)", nullable:=False),
                    .GhostSpeed = table.Column(Of Integer)(type:="int", nullable:=False),
                    .PelletCount = table.Column(Of Integer)(type:="int", nullable:=False)
                },
                constraints:=Sub(table)
                    table.PrimaryKey("PK_Levels", Function(level) level.LevelId)
                End Sub)

            migrationBuilder.CreateTable(
                name:="Settings",
                columns:=Function(table) New With {
                    .Key = table.Column(Of String)(type:="nvarchar(100)", maxLength:=100, nullable:=False),
                    .Value = table.Column(Of String)(type:="nvarchar(255)", maxLength:=255, nullable:=False)
                },
                constraints:=Sub(table)
                    table.PrimaryKey("PK_Settings", Function(setting) setting.Key)
                End Sub)

            migrationBuilder.CreateTable(
                name:="HighScores",
                columns:=Function(table) New With {
                    .ScoreId = table.Column(Of Integer)(type:="int", nullable:=False).Annotation("SqlServer:Identity", "1, 1"),
                    .PlayerId = table.Column(Of Integer)(type:="int", nullable:=False),
                    .Score = table.Column(Of Integer)(type:="int", nullable:=False),
                    .LevelReached = table.Column(Of Integer)(type:="int", nullable:=False),
                    .PlayedOn = table.Column(Of DateTime)(type:="datetime2", nullable:=False)
                },
                constraints:=Sub(table)
                    table.PrimaryKey("PK_HighScores", Function(score) score.ScoreId)
                    table.ForeignKey(
                        name:="FK_HighScores_Players_PlayerId",
                        column:=Function(score) score.PlayerId,
                        principalTable:="Players",
                        principalColumn:="PlayerId",
                        onDelete:=ReferentialAction.Cascade)
                End Sub)

            migrationBuilder.CreateIndex(
                name:="IX_HighScores_PlayerId",
                table:="HighScores",
                column:="PlayerId")
            migrationBuilder.CreateIndex(
                name:="IX_Levels_LevelNumber",
                table:="Levels",
                column:="LevelNumber",
                unique:=True)
            migrationBuilder.CreateIndex(
                name:="IX_Players_Name",
                table:="Players",
                column:="Name",
                unique:=True)
        End Sub

        Protected Overrides Sub Down(migrationBuilder As MigrationBuilder)
            migrationBuilder.DropTable(name:="HighScores")
            migrationBuilder.DropTable(name:="Levels")
            migrationBuilder.DropTable(name:="Settings")
            migrationBuilder.DropTable(name:="Players")
        End Sub

    End Class

End Namespace
