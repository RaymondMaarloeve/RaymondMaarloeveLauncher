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

public class NpcConfigPageViewModel : ReactiveObject, IDisposable
{
    public ObservableCollection<NpcModel> Npcs { get; } = new();
    public ObservableCollection<string> AvailableModels { get; } = new();

    public ReactiveCommand<NpcModel, Unit> RemoveNpcCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNpcCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    private const string ConfigPath = "game_config.json";
    private const string ModelsFolder = "Models";
    
    private readonly IDisposable _startupSubscription;

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
        _startupSubscription = AddNpcCommand.Execute().Subscribe();
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
        if (config?.Npcs is null || config.Models is null)
            return;

        Npcs.Clear();
        AvailableModels.Clear();

        // Załaduj modele do listy dostępnych modeli
        foreach (var model in config.Models)
        {
            AvailableModels.Add(model.Name);
        }

        foreach (var npc in config.Npcs)
        {
            // Znajdź nazwę modelu na podstawie ID
            var modelName = config.Models.FirstOrDefault(m => m.Id == npc.ModelId)?.Name;

            Npcs.Add(new NpcModel(AvailableModels, RemoveNpcCommand)
            {
                Name = $"NPC{npc.Id + 1}",
                SelectedModel = modelName
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

        // Wygeneruj unikalną listę modeli
        var usedModels = Npcs
            .Select(n => n.SelectedModel)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct()
            .Select((modelName, index) => new ModelData
            {
                Id = index,
                Name = modelName!,
                Path = Path.GetFullPath(Path.Combine("Models", modelName!))
            })
            .ToList();

        config.Models = usedModels;

        // NPC przypisani do ID modeli
        config.Npcs = Npcs.Select((npc, index) =>
        {
            var modelId = usedModels.FirstOrDefault(m => m.Name == npc.SelectedModel)?.Id ?? -1;
            return new NpcConfig
            {
                Id = index,
                ModelId = modelId
            };
        }).ToList();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var result = JsonSerializer.Serialize(config, options);
        File.WriteAllText(ConfigPath, result);
    }
    
    public void Dispose()
    {
        _startupSubscription.Dispose();
    }
}
