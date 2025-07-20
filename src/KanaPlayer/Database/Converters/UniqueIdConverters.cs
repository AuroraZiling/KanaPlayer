using KanaPlayer.Core.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KanaPlayer.Database.Converters;

public class BiliMediaListUniqueIdConverter() : ValueConverter<BiliMediaListUniqueId, string>(v => v.ToString(),
    v => BiliMediaListUniqueId.Parse(v));
public class LocalMediaListUniqueIdConverter() : ValueConverter<LocalMediaListUniqueId, string>(v => v.ToString(),
    v => LocalMediaListUniqueId.Parse(v));
    
public class AudioUniqueIdConverter() : ValueConverter<AudioUniqueId, string>(v => v.ToString(),
    v => AudioUniqueId.Parse(v));