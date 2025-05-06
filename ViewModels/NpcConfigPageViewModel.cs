using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Text.Json.Serialization;
using RaymondMaarloeveLauncher.Models;
using ReactiveUI;

namespace RaymondMaarloeveLauncher.ViewModels;

public class NpcConfigPageViewModel : ReactiveObject
{
    public ObservableCollection<NpcModel> Npcs { get; } = new();
    public ObservableCollection<string> AvailableModels { get; } = new();

    public ReactiveCommand<NpcModel, Unit> RemoveNpcCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNpcCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    private const string ConfigPath = "game_config.json";
    private const string ModelsFolder = "Models";

    public NpcConfigPageViewModel()
    {
        // Inicjalizacja
        LoadAvailableModels();

        RemoveNpcCommand = ReactiveCommand.Create<NpcModel>(npc => Npcs.Remove(npc));

        AddNpcCommand = ReactiveCommand.Create(() =>
        {
            var npc = new NpcModel(AvailableModels, RemoveNpcCommand)
            {
                Name = $"NPC{Npcs.Count + 1}"
            };
            Npcs.Add(npc);
        });

        SaveCommand = ReactiveCommand.Create(SaveNpcConfigToJson);

        // Startowe dane
        AddNpcCommand.Execute().Subscribe();
        LoadNpcConfigFromJson();
    }

    private void LoadAvailableModels()
    {
        AvailableModels.Clear();

        if (!Directory.Exists(ModelsFolder))
            Directory.CreateDirectory(ModelsFolder);

        var modelPaths = Directory.GetFiles(ModelsFolder, "*.gguf");
        foreach (var path in modelPaths)
        {
            AvailableModels.Add(Path.GetFileName(path));
        }
    }

    public void LoadNpcConfigFromJson()
    {
        if (!File.Exists(ConfigPath))
            return;

        var json = File.ReadAllText(ConfigPath);
        var config = JsonSerializer.Deserialize<GameData>(json);
        if (config?.Npcs is null) return;

        Npcs.Clear();

        foreach (var npc in config.Npcs)
        {
            Npcs.Add(new NpcModel(AvailableModels, RemoveNpcCommand)
            {
                Name = npc.Name,
                SelectedModel = npc.ModelName
            });
        }
    }

    public void SaveNpcConfigToJson()
    {
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

        config.Npcs = Npcs.Select(n => new NpcConfig
        {
            Name = n.Name,
            ModelName = n.SelectedModel ?? "",
            ModelPath = Path.GetFullPath(Path.Combine(ModelsFolder, n.SelectedModel ?? ""))
        }).ToList();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var result = JsonSerializer.Serialize(config, options);
        File.WriteAllText(ConfigPath, result);
    }
}
