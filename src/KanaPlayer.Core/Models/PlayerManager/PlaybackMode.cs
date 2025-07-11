namespace KanaPlayer.Core.Models.PlayerManager;

public enum PlaybackMode
{
    /// <summary>
    /// Randomly play audios
    /// </summary>
    Shuffle,
    
    /// <summary>
    /// Repeat all audios
    /// </summary>
    RepeatAll,
    
    /// <summary>
    /// Repeat the current audio
    /// </summary>
    RepeatOne,
    
    /// <summary>
    /// Play audios in order, only once
    /// </summary>
    Sequential,
    
    MaxValue
}
