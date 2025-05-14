using ReactiveUI;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Reactive;
using System.Text.Json.Serialization;
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

    private string _llmModelPath = "";
    public string LlmModelPath
    {
        get => _llmModelPath;
        set => this.RaiseAndSetIfChanged(ref _llmModelPath, value);
    }
    
    private bool _localhost = true;
    public bool Localhost
    {
        get => _localhost;
        set
        {
            this.RaiseAndSetIfChanged(ref _localhost, value);
            if (value)
            {
                LlmServerApi = "http://127.0.0.1:5000/";
                LoadLocalModels();
            }
        }
    }

    private void LoadLocalModels()
    {
        Directory.CreateDirectory("Models");
        var files = Directory.GetFiles("Models", "*.gguf");
        
        var json = File.Exists(ConfigPath) ? File.ReadAllText(ConfigPath) : "{}";
        var config = JsonSerializer.Deserialize<GameData>(json) ?? new GameData();
        
        config.Models = files.Select((path, index) => new ModelData 
        {
            Id = index,
            Name = Path.GetFileName(path),
            Path = Path.Combine(Directory.GetCurrentDirectory(), path)
            // Path = path
        }).ToList();

        var result = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        File.WriteAllText(ConfigPath, result);
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
        Localhost = config.Localhost;
        if (!Localhost)
        {
            LlmModelPath = config.Models?.FirstOrDefault()?.Path ?? string.Empty;
        }
        FullScreen = config.FullScreen;
        Resolution = $"{config.GameWindowWidth}x{config.GameWindowHeight}";
    }

    private void SaveConfig()
    {
        var parts = Resolution.Split('x');
        int.TryParse(parts[0], out var width);
        int.TryParse(parts[1], out var height);

        GameData config;

        if (File.Exists(ConfigPath))
        {
            var json = File.ReadAllText(ConfigPath);
            config = JsonSerializer.Deserialize<GameData>(json) ?? new GameData();
        }
        else
        {
            config = new GameData();
        }

        config.Revision = 1;
        config.LlmServerApi = LlmServerApi;
        config.Localhost = Localhost;
        config.FullScreen = FullScreen;
        config.GameWindowWidth = width;
        config.GameWindowHeight = height;

        if (!Localhost)
        {
            config.Models =
            [
                new ModelData
                {
                    Id = 0,
                    Name = Path.GetFileName(LlmModelPath),
                    Path = LlmModelPath
                }
            ];
        }

        var result = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        File.WriteAllText(ConfigPath, result);
    }
}
