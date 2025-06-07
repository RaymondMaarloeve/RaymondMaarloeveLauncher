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

/// <summary>
/// ViewModel for the NPC configuration page, responsible for managing NPCs and their associated models.
/// </summary>
public class NpcConfigPageViewModel : ReactiveObject, IDisposable
{
    /// <summary>
    /// Collection of NPCs displayed and managed in the UI.
    /// </summary>
    public ObservableCollection<NpcModel> Npcs { get; } = new();
    /// <summary>
    /// Collection of available model names for NPC assignment.
    /// </summary>
    public ObservableCollection<string> AvailableModels { get; } = new();

    /// <summary>
    /// Holds the current selection of the narrator model from the list of available models.
    /// </summary>
    private string? _selectedNarratorModel;
    public string? SelectedNarratorModel
    {
        get => _selectedNarratorModel;
        set => this.RaiseAndSetIfChanged(ref _selectedNarratorModel, value);
    }

    /// <summary>
    /// Command to remove an NPC from the collection.
    /// </summary>
    public ReactiveCommand<NpcModel, Unit> RemoveNpcCommand { get; }
    /// <summary>
    /// Command to add a new NPC to the collection.
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddNpcCommand { get; }
    /// <summary>
    /// Command to save the current NPC configuration to a JSON file.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    /// <summary>
    /// Path to the configuration file.
    /// </summary>
    private const string ConfigPath = "game_config.json";
    /// <summary>
    /// Path to the folder containing model files.
    /// </summary>
    private const string ModelsFolder = "Models";
    
    /// <summary>
    /// Subscription for startup initialization.
    /// </summary>
    private readonly IDisposable _startupSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpcConfigPageViewModel"/> class.
    /// Loads available models, sets up commands, and loads NPC configuration from JSON.
    /// </summary>
    public NpcConfigPageViewModel()
    {
        // Initialization
        LoadAvailableModels();
        
        SelectedNarratorModel = AvailableModels.FirstOrDefault();

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

        // Startup data
        _startupSubscription = AddNpcCommand.Execute().Subscribe();
        LoadNpcConfigFromJson();
    }

    /// <summary>
    /// Loads the list of available models from the configuration file.
    /// </summary>
    private void LoadAvailableModels()
    {
        AvailableModels.Clear();

        if (!Directory.Exists(ModelsFolder))
            Directory.CreateDirectory(ModelsFolder);

        GameData? config = null;
        if (File.Exists(ConfigPath))
        {
            var json = File.ReadAllText(ConfigPath);
            config = JsonSerializer.Deserialize<GameData>(json);
        }
    
        var modelNames = config?.Models?.Select(m => m.Name) ?? Enumerable.Empty<string>();
        
        foreach (var name in modelNames)
        {
            AvailableModels.Add(name);
        }
    }

    /// <summary>
    /// Loads the NPC configuration from the configuration file and populates the NPC and model collections.
    /// </summary>
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

        // Load models into the list of available models
        foreach (var model in config.Models)
        {
            AvailableModels.Add(model.Name);
        }

        foreach (var npc in config.Npcs)
        {
            // Find the model name based on the ID
            var modelName = config.Models.FirstOrDefault(m => m.Id == npc.ModelId)?.Name;

            Npcs.Add(new NpcModel(AvailableModels, RemoveNpcCommand)
            {
                Name = $"NPC{npc.Id + 1}",
                SelectedModel = modelName
            });
        }

        
        // Set the narrator model based on the configuration. If a narrator model ID is specified,
        // find its name from the models list, otherwise use the first available model as default
        SelectedNarratorModel = config.NarratorModelId.HasValue
            ? config.Models.FirstOrDefault(m => m.Id == config.NarratorModelId.Value)?.Name
            : AvailableModels.FirstOrDefault();
    }

    /// <summary>
    /// Saves the current NPC configuration to the configuration file.
    /// </summary>
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
        config.Npcs = Npcs.Select((npc, index) =>
        {
            var modelId = config.Models?.FirstOrDefault(m => m.Name == npc.SelectedModel)?.Id ?? -1;
            return new NpcConfig
            {
                Id = index,
                ModelId = modelId
            };
        }).ToList();

        config.NarratorModelId = config.Models?
            .FirstOrDefault(m => m.Name == SelectedNarratorModel)?.Id;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var result = JsonSerializer.Serialize(config, options);
        File.WriteAllText(ConfigPath, result);
    }
    
    /// <summary>
    /// Disposes of resources used by the view model.
    /// </summary>
    public void Dispose()
    {
        _startupSubscription.Dispose();
    }
}