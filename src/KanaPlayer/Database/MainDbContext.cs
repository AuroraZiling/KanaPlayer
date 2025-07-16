using System.IO;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Database.Converters;
using Microsoft.EntityFrameworkCore;

namespace KanaPlayer.Database;

public partial class MainDbContext : DbContext
{
    public DbSet<LocalFavoriteFolderItem> LocalFavoriteFolderItemSet { get; set; }
    public DbSet<CachedAudioMetadata> CachedAudioMetadataSet { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppHelper.ApplicationFolderPath, "Main.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
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