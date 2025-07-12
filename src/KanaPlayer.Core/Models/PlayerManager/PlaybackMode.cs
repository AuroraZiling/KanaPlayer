using System.Runtime.Serialization;

namespace KanaPlayer.Core.Models.PlayerManager;

public enum PlaybackMode
{
    /// <summary>
    /// Randomly play audios
    /// </summary>
    [EnumMember(Value = "随机播放")]
    Shuffle,
    
    /// <summary>
    /// Repeat all audios
    /// </summary>
    [EnumMember(Value = "列表循环")]
    RepeatAll,
    
    /// <summary>
    /// Repeat the current audio
    /// </summary>
    [EnumMember(Value = "单曲循环")]
    RepeatOne,
    
    /// <summary>
    /// Play audios in order, only once
    /// </summary>
    [EnumMember(Value = "顺序播放")]
    Sequential,
    
    MaxValue
}