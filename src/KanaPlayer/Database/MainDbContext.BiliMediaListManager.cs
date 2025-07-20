using System.Collections.Generic;
using System.Linq;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.BiliMediaList;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace KanaPlayer.Database;

public partial class MainDbContext : IBiliMediaListManager
{
    public DbCachedMediaListAudioMetadata? GetCachedMediaListAudioMetadataByUniqueId(AudioUniqueId uniqueId)
        => CachedMediaListAudioMetadataSet
           .FirstOrDefault(metadata => metadata.UniqueId.Equals(uniqueId));

    public List<DbBiliMediaListItem> GetBiliMediaListItems()
        => BiliMediaListItemSet
           .Include(item => item.CachedMediaListAudioMetadataSet)
           .OrderByDescending(item => item.CreatedTimestamp)
           .ToList();

    public List<DbCachedMediaListAudioMetadata> GetCachedMediaListAudioMetadataList(DbBiliMediaListItem item)
        => BiliMediaListItemSet
           .Include(folder => folder.CachedMediaListAudioMetadataSet)
           .Where(folder => folder.Id.Equals(item.UniqueId.ToString()))
           .SelectMany(folder => folder.CachedMediaListAudioMetadataSet)
           .OrderByDescending(metadata => metadata.PublishTimestamp)
           .ToList();

    public bool IsBiliMediaListExists(BiliMediaListUniqueId biliMediaListUniqueId)
        => BiliMediaListItemSet
            .Any(item => item.Id.Equals(biliMediaListUniqueId.ToString()));

    public void ImportFromBilibili(BiliMediaListItem importItem, List<BiliMediaListCommonMediaModel> importMedias)
    {
        var biliMediaListItem = BiliMediaListItemSet
                                      .Include(item => item.CachedMediaListAudioMetadataSet)
                                      .FirstOrDefault(item => item.Id.Equals(new BiliMediaListUniqueId(importItem.Id, importItem.BiliMediaListType).ToString()));

        if (biliMediaListItem is null)
        {
            biliMediaListItem = new DbBiliMediaListItem
            {
                UniqueId = new BiliMediaListUniqueId(importItem.Id, importItem.BiliMediaListType),
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
        var existedMediaList = CachedMediaListAudioMetadataSet
                               .Where(metadata => importMediaUniqueIds.Contains(metadata.UniqueId))
                               .ToList();

        foreach (var media in importMedias)
        {
            if (existedMediaList.FirstOrDefault(existed => existed.UniqueId.Equals(new AudioUniqueId(media.Bvid))) is not { } audioMetadata)
            {
                audioMetadata = new DbCachedMediaListAudioMetadata
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
                CachedMediaListAudioMetadataSet.Add(audioMetadata);
            }
            biliMediaListItem.CachedMediaListAudioMetadataSet.Add(audioMetadata);
        }

        SaveChanges();
    }
    public bool DeleteBiliMediaListItem(BiliMediaListUniqueId biliMediaListUniqueId)
    {
        var biliMediaListItem = BiliMediaListItemSet
            .Include(item => item.CachedMediaListAudioMetadataSet)
            .FirstOrDefault(item => item.Id.Equals(biliMediaListUniqueId.ToString()));

        if (biliMediaListItem is null)
            return false;

        BiliMediaListItemSet.Remove(biliMediaListItem);
        SaveChanges();
        return true;
    }

    public DbCachedMediaListAudioMetadata AddOrUpdateAudioToCache(AudioUniqueId audioUniqueId, AudioInfoDataModel audioInfoData)
    {
        if (CachedMediaListAudioMetadataSet.FirstOrDefault(metadata => metadata.UniqueId.Equals(audioUniqueId)) is { } audioMetadata)
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
            audioMetadata = new DbCachedMediaListAudioMetadata
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

            CachedMediaListAudioMetadataSet.Add(audioMetadata);
        }
        SaveChanges();
        return audioMetadata;
    }
}