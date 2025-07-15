using KanaPlayer.Core.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KanaPlayer.Database.Converters;

public class FavoriteUniqueIdConverter() : ValueConverter<FavoriteUniqueId, string>(v => v.ToString(),
    v => FavoriteUniqueId.Parse(v));
    
public class AudioUniqueIdConverter() : ValueConverter<AudioUniqueId, string>(v => v.ToString(),
    v => AudioUniqueId.Parse(v));