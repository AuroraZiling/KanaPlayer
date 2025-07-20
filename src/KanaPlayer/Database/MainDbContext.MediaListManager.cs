using System.Collections.Generic;
using System.Linq;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.BiliMediaList;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services.MediaList;
using Microsoft.EntityFrameworkCore;

namespace KanaPlayer.Database;

public partial class MainDbContext : IBiliMediaListManager
{
    public DbCachedBiliMediaListAudioMetadata? GetCachedBiliMediaListAudioMetadataByUniqueId(AudioUniqueId uniqueId)
        => CachedBiliMediaListAudioMetadataSet
           .Include(metadata => metadata.BiliMediaListItemSet)
           .FirstOrDefault(metadata => metadata.UniqueId.Equals(uniqueId));

    public List<DbBiliMediaListItem> GetBiliMediaListItems()
        => BiliMediaListItemSet
           .Include(item => item.CachedBiliMediaListAudioMetadataSet)
           .OrderByDescending(item => item.CreatedTimestamp)
           .ToList();

    public List<DbCachedBiliMediaListAudioMetadata> GetCachedBiliMediaListAudioMetadataList(DbBiliMediaListItem item)
        => BiliMediaListItemSet
           .Include(folder => folder.CachedBiliMediaListAudioMetadataSet)
           .Where(folder => folder.UniqueId.Equals(item.UniqueId))
           .SelectMany(folder => folder.CachedBiliMediaListAudioMetadataSet)
           .OrderByDescending(metadata => metadata.PublishTimestamp)
           .ToList();

    public bool IsBiliMediaListExists(FavoriteUniqueId favoriteUniqueId)
        => BiliMediaListItemSet
            .Any(item => item.UniqueId.Equals(favoriteUniqueId));

    public void ImportFromBilibili(BiliMediaListItem importItem, List<BiliMediaListCommonMediaModel> importMedias)
    {
        var biliMediaListItem = BiliMediaListItemSet
                                      .Include(item => item.CachedBiliMediaListAudioMetadataSet)
                                      .FirstOrDefault(item => item.UniqueId.Equals(new FavoriteUniqueId(importItem.Id, importItem.BiliMediaListType)));

        if (biliMediaListItem is null)
        {
            biliMediaListItem = new DbBiliMediaListItem
            {
                UniqueId = new FavoriteUniqueId(importItem.Id, importItem.BiliMediaListType),
                Title = importItem.Title.SafeSubstring(0, 128),
                CoverUrl = importItem.CoverUrl.SafeSubstring(0, 1024),
                Description = importItem.Description.SafeSubstring(0, 1024),
                OwnerMid = importItem.Owner.Mid,
                OwnerName = importItem.Owner.Name.SafeSubstring(0, 64),
                BiliMediaListType = importItem.BiliMediaListType,
                CreatedTimestamp = importItem.CreatedTimestamp,
                ModifiedTimestamp = importItem.ModifiedTimestamp,
                MediaCount = importItem.MediaCount
            };

            BiliMediaListItemSet.Add(biliMediaListItem);
        }

        var importMediaUniqueIds = importMedias.Select(media => new AudioUniqueId(media.Bvid)).ToHashSet();
        var existedMediaList = CachedBiliMediaListAudioMetadataSet
                               .Where(metadata => importMediaUniqueIds.Contains(metadata.UniqueId))
                               .ToList();

        foreach (var media in importMedias)
        {
            if (existedMediaList.FirstOrDefault(existed => existed.UniqueId.Equals(new AudioUniqueId(media.Bvid))) is not { } audioMetadata)
            {
                audioMetadata = new DbCachedBiliMediaListAudioMetadata
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
                CachedBiliMediaListAudioMetadataSet.Add(audioMetadata);
            }
            biliMediaListItem.CachedBiliMediaListAudioMetadataSet.Add(audioMetadata);
        }

        SaveChanges();
    }
    public bool DeleteBiliMediaListItem(FavoriteUniqueId favoriteUniqueId)
    {
        var biliMediaListItem = BiliMediaListItemSet
            .Include(item => item.CachedBiliMediaListAudioMetadataSet)
            .FirstOrDefault(item => item.UniqueId.Equals(favoriteUniqueId));

        if (biliMediaListItem is null)
            return false;

        BiliMediaListItemSet.Remove(biliMediaListItem);
        SaveChanges();
        return true;
    }

    public void AddOrUpdateAudioToCache(AudioUniqueId audioUniqueId, AudioInfoDataModel audioInfoData)
    {
        if (CachedBiliMediaListAudioMetadataSet.FirstOrDefault(metadata => metadata.UniqueId.Equals(audioUniqueId)) is { } audioMetadata)
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
            audioMetadata = new DbCachedBiliMediaListAudioMetadata
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

            CachedBiliMediaListAudioMetadataSet.Add(audioMetadata);
        }
        SaveChanges();
    }
}