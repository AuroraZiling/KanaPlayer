using System.IO;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Database.Converters;
using Microsoft.EntityFrameworkCore;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace KanaPlayer.Database;

public partial class MainDbContext : DbContext
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(MainDbContext));
    public DbSet<LocalFavoriteFolderItem> LocalFavoriteFolderItemSet { get; set; }
    public DbSet<CachedAudioMetadata> CachedAudioMetadataSet { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppHelper.ApplicationDataFolderPath, "Main.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        ScopedLogger.Info("数据库已连接: {DbPath}", dbPath);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalFavoriteFolderItem>()
                    .HasMany(e => e.CachedAudioMetadataSet)
                    .WithMany(e => e.LocalFavoriteFolderItemSet);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<FavoriteUniqueId>().HaveConversion<FavoriteUniqueIdConverter>();
        configurationBuilder.Properties<AudioUniqueId>().HaveConversion<AudioUniqueIdConverter>();
    }
}