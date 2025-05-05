using System;
using ReactiveUI;
using System.IO;
using System.Text.Json;
using System.Reactive;
using RaymondMaarloeveLauncher.Models;

namespace RaymondMaarloeveLauncher.ViewModels;

public class ConfigPageViewModel : ReactiveObject
{
    private const string ConfigPath = "game_config.json";

    private string _llmServerApi = "http://127.0.0.1:5000/";
    public string LlmServerApi
    {
        get => _llmServerApi;
        set => this.RaiseAndSetIfChanged(ref _llmServerApi, value);
    }

    private bool _fullScreen = false;
    public bool FullScreen
    {
        get => _fullScreen;
        set => this.RaiseAndSetIfChanged(ref _fullScreen, value);
    }

    private string _resolution = "1920x1080";
    public string Resolution
    {
        get => _resolution;
        set => this.RaiseAndSetIfChanged(ref _resolution, value);
    }

    public string[] AvailableResolutions { get; } = new[]
    {
        "1280x720",
        "1600x900",
        "1920x1080",
        "2560x1440",
        "3840x2160"
    };

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ConfigPageViewModel()
    {
        SaveCommand = ReactiveCommand.Create(SaveConfig);
        LoadConfig();
    }

    private void LoadConfig()
    {
        if (!File.Exists(ConfigPath))
            return;

        var json = File.ReadAllText(ConfigPath);
        var config = JsonSerializer.Deserialize<GameData>(json);
        if (config is null) return;

        LlmServerApi = config.LlmServerApi;
        FullScreen = config.FullScreen;
        Resolution = $"{config.GameWindowWidth}x{config.GameWindowHeight}";
    }

    private void SaveConfig()
    {
        var parts = Resolution.Split('x');
        int.TryParse(parts[0], out var width);
        int.TryParse(parts[1], out var height);

        var config = new GameData
        {
            Revision = 1,
            LlmServerApi = LlmServerApi,
            FullScreen = FullScreen,
            GameWindowWidth = width,
            GameWindowHeight = height
        };

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }
}
