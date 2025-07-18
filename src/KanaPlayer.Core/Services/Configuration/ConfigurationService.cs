using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Timers;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Models;
using Microsoft.Extensions.Logging;
using NLog;
using Timer = System.Timers.Timer;

namespace KanaPlayer.Core.Services.Configuration;

public class ConfigurationService<TSettings> : IConfigurationService<TSettings> 
    where TSettings : SettingsBase, new()
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(ConfigurationService<TSettings>));
    private readonly Timer _saveTimer;
    private readonly Lock _saveLock = new();
    private bool _hasChanges;
    
    public TSettings Settings { get; private set; }
    
    public ConfigurationService()
    {
        _saveTimer = new Timer(1000);
        _saveTimer.Elapsed += OnSaveTimerElapsed;
        _saveTimer.AutoReset = false;
        Load();
    }

    [MemberNotNull(nameof(Settings))]
    private void Load()
    {
        Directory.CreateDirectory(AppHelper.ApplicationDataFolderPath);
        Directory.CreateDirectory(AppHelper.ApplicationAudioCachesFolderPath);
        Directory.CreateDirectory(AppHelper.ApplicationImageCachesFolderPath);
        
        TSettings? settings = null;
        try
        {
            var settingsJson = File.ReadAllText(AppHelper.SettingsFilePath);
            settings = JsonSerializer.Deserialize<TSettings>(settingsJson);
            ScopedLogger.Info("配置文件加载成功");
        }
        catch (Exception ex)
        {
            ScopedLogger.Warn(ex, "配置文件加载失败，使用默认设置");
        }
        Settings = settings ?? new TSettings();
    }
    
    private void Save()
    {
        lock (_saveLock)
        {
            var settingsJson = JsonSerializer.Serialize(Settings, JsonHelper.JsonSerializerOptions);
            File.WriteAllText(AppHelper.SettingsFilePath, settingsJson);
            _hasChanges = false;
        }
    }
    
    public void SaveDelayed()
    {
        lock (_saveLock)
        {
            _hasChanges = true;
            _saveTimer.Stop();
            _saveTimer.Start();
        }
    }
    
    public void SaveImmediate()
    {
        _hasChanges = true;
        lock (_saveLock)
        {
            if (_hasChanges)
            {
                _saveTimer.Stop();
                Save();
            }
        }
    }
    
    private void OnSaveTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        lock (_saveLock)
            if (_hasChanges)
                Save();
    }
    
    public void Dispose()
    {
        SaveImmediate();
        _saveTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}