Imports Microsoft.EntityFrameworkCore

Namespace DataAccess

    Public Class ChompManDbContext
        Inherits DbContext

        Public Sub New(options As DbContextOptions(Of ChompManDbContext))
            MyBase.New(options)
        End Sub

        Public Property Players As DbSet(Of PlayerEntity)
        Public Property HighScores As DbSet(Of HighScoreEntity)
        Public Property Levels As DbSet(Of LevelEntity)
        Public Property Settings As DbSet(Of SettingEntity)

        Protected Overrides Sub OnModelCreating(modelBuilder As ModelBuilder)
            modelBuilder.Entity(Of PlayerEntity)(
                Sub(entity)
                    entity.HasKey(Function(player) player.PlayerId)
                    entity.Property(Function(player) player.Name).HasMaxLength(100).IsRequired()
                    entity.HasIndex(Function(player) player.Name).IsUnique()
                    entity.Property(Function(player) player.CreatedOn).IsRequired()
                End Sub)

            modelBuilder.Entity(Of HighScoreEntity)(
                Sub(entity)
                    entity.HasKey(Function(score) score.ScoreId)
                    entity.HasOne(Function(score) score.Player).
                        WithMany(Function(player) player.HighScores).
                        HasForeignKey(Function(score) score.PlayerId).
                        OnDelete(DeleteBehavior.Cascade)
                    entity.Property(Function(score) score.PlayedOn).IsRequired()
                End Sub)

            modelBuilder.Entity(Of LevelEntity)(
                Sub(entity)
                    entity.HasKey(Function(level) level.LevelId)
                    entity.HasIndex(Function(level) level.LevelNumber).IsUnique()
                    entity.Property(Function(level) level.MazeLayout).HasColumnType("nvarchar(max)").IsRequired()
                End Sub)

            modelBuilder.Entity(Of SettingEntity)(
                Sub(entity)
                    entity.HasKey(Function(setting) setting.Key)
                    entity.Property(Function(setting) setting.Key).HasMaxLength(100)
                    entity.Property(Function(setting) setting.Value).HasMaxLength(255).IsRequired()
                End Sub)
        End Sub

    End Class

End Namespace
