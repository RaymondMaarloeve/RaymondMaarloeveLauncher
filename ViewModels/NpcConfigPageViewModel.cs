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
    public ObservableCollection<string> AvailableModels { get; } = new()
    {
        "alpha.gguf", "beta.gguf", "gamma.gguf"
    };

    public ReactiveCommand<NpcModel, Unit> RemoveNpcCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNpcCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    
    private const string ConfigPath = "game_config.json";


    public NpcConfigPageViewModel()
    {
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


        // Dodaj pierwszy NPC
        AddNpcCommand.Execute().Subscribe();
        LoadNpcConfigFromJson();
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
            ModelPath = Path.Combine("Models", n.SelectedModel ?? "")
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

