using KanaPlayer.Core.Models;

namespace KanaPlayer.Core.Services.Configuration;

public interface IConfigurationService<out TSettings> : IDisposable
    where TSettings : SettingsBase, new()
{
    TSettings Settings { get; }
    
    /// <summary>
    /// 延迟保存配置，避免频繁写入（推荐用于滑动条等频繁变化的设置）
    /// </summary>
    void SaveDelayed(string saveLog);
    
    /// <summary>
    /// 立即保存配置（如果有待保存的更改）
    /// </summary>
    void SaveImmediate(string saveLog);
}