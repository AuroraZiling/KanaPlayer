using System.Collections.Generic;
using System.Linq;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Favorites;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services.Favorites;
using Microsoft.EntityFrameworkCore;

namespace KanaPlayer.Database;

public partial class MainDbContext : IFavoritesManager
{
    public List<LocalFavoriteFolderItem> GetLocalFavoriteFolders()
        => LocalFavoriteFolderItemSet
           .Include(item => item.CachedAudioMetadataSet)
           .OrderByDescending(item => item.CreatedTimestamp)
           .ToList();

    public List<CachedAudioMetadata> GetCachedAudioMetadataList(LocalFavoriteFolderItem item)
        => LocalFavoriteFolderItemSet
           .Include(folder => folder.CachedAudioMetadataSet)
           .Where(folder => folder.UniqueId.Equals(item.UniqueId))
           .SelectMany(folder => folder.CachedAudioMetadataSet)
           .OrderByDescending(metadata => metadata.PublishTimestamp)
           .ToList();

    public bool IsFolderExists(FavoriteUniqueId favoriteUniqueId)
        => LocalFavoriteFolderItemSet
            .Any(item => item.UniqueId.Equals(favoriteUniqueId));

    public void ImportFromBilibili(FavoriteFolderItem importItem, List<CollectionFolderCommonMediaModel> importMedias)
    {
        var localFavoriteFolderItem = LocalFavoriteFolderItemSet
                                      .Include(item => item.CachedAudioMetadataSet)
                                      .FirstOrDefault(item => item.UniqueId.Equals(new FavoriteUniqueId(importItem.Id, importItem.FavoriteType)));

        if (localFavoriteFolderItem is null)
        {
            localFavoriteFolderItem = new LocalFavoriteFolderItem
            {
                UniqueId = new FavoriteUniqueId(importItem.Id, importItem.FavoriteType),
                Title = importItem.Title.SafeSubstring(0, 128),
                CoverUrl = importItem.CoverUrl.SafeSubstring(0, 1024),
                Description = importItem.Description.SafeSubstring(0, 1024),
                OwnerMid = importItem.Owner.Mid,
                OwnerName = importItem.Owner.Name.SafeSubstring(0, 64),
                FavoriteType = importItem.FavoriteType,
                CreatedTimestamp = importItem.CreatedTimestamp,
                ModifiedTimestamp = importItem.ModifiedTimestamp,
                MediaCount = importItem.MediaCount
            };

            LocalFavoriteFolderItemSet.Add(localFavoriteFolderItem);
        }

        var importMediaUniqueIds = importMedias.Select(media => new AudioUniqueId(media.Bvid)).ToHashSet();
        var existedMediaList = CachedAudioMetadataSet
                               .Where(metadata => importMediaUniqueIds.Contains(metadata.UniqueId))
                               .ToList();

        foreach (var media in importMedias)
        {
            if (existedMediaList.FirstOrDefault(existed => existed.UniqueId.Equals(new AudioUniqueId(media.Bvid))) is not { } audioMetadata)
            {
                audioMetadata = new CachedAudioMetadata
                {
                    UniqueId = new AudioUniqueId(media.Bvid),
                    Title = media.Title.SafeSubstring(0, 128),
                    CoverUrl = media.CoverUrl.SafeSubstring(0, 1024),
                    DurationSeconds = media.DurationSeconds,
                    PublishTimestamp = media.PublishTimestamp,
                    OwnerMid = media.Owner.Mid,
                    OwnerName = media.Owner.Name.SafeSubstring(0, 64),
                    CollectCount = media.Statistics.CollectCount,
                    DanmakuCount = media.Statistics.DanmakuCount,
                    PlayCount = media.Statistics.PlayCount,
                };
                CachedAudioMetadataSet.Add(audioMetadata);
            }
            localFavoriteFolderItem.CachedAudioMetadataSet.Add(audioMetadata);
        }

        SaveChanges();
    }

    public void AddOrUpdateAudioToCache(AudioUniqueId audioUniqueId, AudioInfoDataModel audioInfoData)
    {
        if (CachedAudioMetadataSet.FirstOrDefault(metadata => metadata.UniqueId.Equals(audioUniqueId)) is { } audioMetadata)
        {
            audioMetadata.Title = audioInfoData.Title;
            audioMetadata.CoverUrl = audioInfoData.CoverUrl;
            audioMetadata.DurationSeconds = audioInfoData.DurationSeconds;
            audioMetadata.PublishTimestamp = audioInfoData.PublishTimestamp;
            audioMetadata.OwnerMid = audioInfoData.Owner.Mid;
            audioMetadata.OwnerName = audioInfoData.Owner.Name;
            audioMetadata.CollectCount = audioInfoData.Statistics.CollectCount;
            audioMetadata.DanmakuCount = audioInfoData.Statistics.DanmakuCount;
            audioMetadata.PlayCount = audioInfoData.Statistics.PlayCount;
        }
        else
        {
            audioMetadata = new CachedAudioMetadata
            {
                UniqueId = audioUniqueId,
                Title = audioInfoData.Title,
                CoverUrl = audioInfoData.CoverUrl,
                DurationSeconds = audioInfoData.DurationSeconds,
                PublishTimestamp = audioInfoData.PublishTimestamp,
                OwnerMid = audioInfoData.Owner.Mid,
                OwnerName = audioInfoData.Owner.Name,
                CollectCount = audioInfoData.Statistics.CollectCount,
                DanmakuCount = audioInfoData.Statistics.DanmakuCount,
                PlayCount = audioInfoData.Statistics.PlayCount
            };

            CachedAudioMetadataSet.Add(audioMetadata);
        }
        SaveChanges();
    }
}