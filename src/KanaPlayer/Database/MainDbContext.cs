using System.IO;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services.Database;
using KanaPlayer.Database.Converters;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace KanaPlayer.Database;

public partial class MainDbContext : DbContext
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(MainDbContext));
    public DbSet<DbBiliMediaListItem> BiliMediaListItemSet { get; set; }
    public DbSet<DbLocalMediaListItem> LocalMediaListItemSet { get; set; }
    public DbSet<DbCachedMediaListAudioMetadata> CachedMediaListAudioMetadataSet { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppHelper.ApplicationDataFolderPath, "Main.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        ScopedLogger.Info("数据库已连接: {DbPath}", dbPath);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbMediaListItem>().UseTpcMappingStrategy();
        modelBuilder.Entity<DbBiliMediaListItem>().ToTable("BiliMediaListItems");
        modelBuilder.Entity<DbLocalMediaListItem>().ToTable("LocalMediaListItems");
        
        modelBuilder.Entity<DbBiliMediaListItem>()
                    .HasMany(e => e.CachedMediaListAudioMetadataSet);
        modelBuilder.Entity<DbLocalMediaListItem>()
                    .HasMany(e => e.CachedMediaListAudioMetadataSet);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<BiliMediaListUniqueId>().HaveConversion<BiliMediaListUniqueIdConverter>();
        configurationBuilder.Properties<LocalMediaListUniqueId>().HaveConversion<LocalMediaListUniqueIdConverter>();
        configurationBuilder.Properties<AudioUniqueId>().HaveConversion<AudioUniqueIdConverter>();
    }
}
