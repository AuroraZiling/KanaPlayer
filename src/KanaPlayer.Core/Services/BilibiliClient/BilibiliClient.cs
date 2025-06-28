using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Services.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>(
    [FromKeyedServices("bilibili")] HttpClient httpClient,
    IConfigurationService<TSettings> configurationService)
    : ObservableObject, IBilibiliClient where TSettings : SettingsBase, new();