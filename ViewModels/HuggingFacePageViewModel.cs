using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json.Serialization;
using RaymondMaarloeveLauncher.Models;

namespace RaymondMaarloeveLauncher.ViewModels;

/// <summary>
/// ViewModel for the Hugging Face page, responsible for managing available and local models,
/// downloading models, and updating configuration.
/// </summary>
public class HuggingFacePageViewModel : ReactiveObject
{
    /// <summary>
    /// Shared HTTP client for API requests.
    /// </summary>
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// List of models available for download from Hugging Face.
    /// </summary>
    public ObservableCollection<string> AvailableModels { get; } = new();
    /// <summary>
    /// List of models available locally.
    /// </summary>
    public ObservableCollection<string> LocalModels { get; } = new();

    /// <summary>
    /// The currently selected model from Hugging Face.
    /// </summary>
    private string? _selectedModel;
    /// <summary>
    /// Gets or sets the selected model from Hugging Face.
    /// </summary>
    public string? SelectedModel
    {
        get => _selectedModel;
        set => this.RaiseAndSetIfChanged(ref _selectedModel, value);
    }
    
    /// <summary>
    /// The currently selected local model.
    /// </summary>
    private string? _selectedLocalModel;
    /// <summary>
    /// Gets or sets the selected local model.
    /// </summary>
    public string? SelectedLocalModel
    {
        get => _selectedLocalModel;
        set => this.RaiseAndSetIfChanged(ref _selectedLocalModel, value);
    }

    /// <summary>
    /// Status message for the download process.
    /// </summary>
    private string _downloadStatus = "";
    /// <summary>
    /// Gets or sets the download status message.
    /// </summary>
    public string DownloadStatus
    {
        get => _downloadStatus;
        set => this.RaiseAndSetIfChanged(ref _downloadStatus, value);
    }

    /// <summary>
    /// Status message for local models operations.
    /// </summary>
    private string _localModelsStatus = "";
    /// <summary>
    /// Gets or sets the local models status message.
    /// </summary>
    public string LocalModelsStatus
    {
        get => _localModelsStatus;
        set => this.RaiseAndSetIfChanged(ref _localModelsStatus, value);
    }
    
    /// <summary>
    /// Progress of the current download operation (0-100).
    /// </summary>
    private double _downloadProgress;
    /// <summary>
    /// Gets or sets the download progress percentage.
    /// </summary>
    public double DownloadProgress
    {
        get => _downloadProgress;
        set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
    }

    /// <summary>
    /// Text representation of the current progress.
    /// </summary>
    private string _progressText = "";
    /// <summary>
    /// Gets or sets the progress text.
    /// </summary>
    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }

    /// <summary>
    /// Command to download the selected model from Hugging Face.
    /// </summary>
    public ReactiveCommand<Unit, Unit> DownloadModelCommand { get; }
    /// <summary>
    /// Command to load available models from Hugging Face.
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadModelsCommand { get; }
    /// <summary>
    /// Command to load local models from disk.
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadLocalModelsCommand { get; }
    /// <summary>
    /// Command to delete the selected local model.
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteModelCommand { get; }
    /// <summary>
    /// Command to save the current model list to the configuration file.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveToJsonCommand { get; }
    /// <summary>
    /// Path to the configuration file.
    /// </summary>
    private const string ConfigPath = "game_config.json";

    /// <summary>
    /// Initializes a new instance of the <see cref="HuggingFacePageViewModel"/> class.
    /// Sets up commands and loads models.
    /// </summary>
    public HuggingFacePageViewModel()
    {
        DownloadModelCommand = ReactiveCommand.CreateFromTask(DownloadSelectedModelAsync);
        LoadModelsCommand = ReactiveCommand.CreateFromTask(LoadAvailableModelsAsync);
        
        LoadLocalModelsCommand = ReactiveCommand.CreateFromTask(LoadLocalModelsAsync);
        DeleteModelCommand = ReactiveCommand.CreateFromTask(DeleteSelectedLocalModelAsync);
        SaveToJsonCommand = ReactiveCommand.Create(SaveToJson);
        

        _ = LoadLocalModelsAsync();
        _ = LoadAvailableModelsAsync();
    }

    /// <summary>
    /// Saves the list of local models to the configuration file.
    /// </summary>
    private void SaveToJson()
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

    /// <summary>
    /// Loads the list of available models from Hugging Face.
    /// </summary>
    private async Task LoadAvailableModelsAsync()
    {
        const string repoId = "wujoq/Reymond_Tuning";
        string apiUrl = $"https://huggingface.co/api/models/{repoId}/tree/main";

        try
        {
            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var files = JsonSerializer.Deserialize<List<HfFile>>(json);
            

            var ggufFiles = files!
                .Where(f => f.type == "file" && f.path.EndsWith(".gguf") )
                .Select(f => f.path)
                .ToList();

            AvailableModels.Clear();
            foreach (var model in ggufFiles)
                AvailableModels.Add(model);

            if (AvailableModels.Count == 0)
                DownloadStatus = "No .gguf models found.";
        }
        catch
        {
            DownloadStatus = "❌ Failed to fetch model list from Hugging Face.";
        }
    }

    /// <summary>
    /// Downloads the selected model from Hugging Face and saves it locally.
    /// </summary>
    private async Task DownloadSelectedModelAsync()
    {
        if (SelectedModel is null) return;

        var modelName = SelectedModel;
        var url = $"https://huggingface.co/wujoq/Reymond_Tuning/resolve/main/{modelName}?download=true";

        try
        {
            var targetPath = Path.Combine("Models", Path.GetFileName(modelName));
            Directory.CreateDirectory("Models");
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1;

            await using var remote = await response.Content.ReadAsStreamAsync();
            await using var local = File.Create(targetPath);

            var buffer = new byte[81920];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await remote.ReadAsync(buffer)) > 0)
            {
                await local.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;

                if (canReportProgress)
                {
                    double percent = (double)totalRead / totalBytes * 100;
                    DownloadProgress = percent;
                    ProgressText = $"{percent:F1}%";
                }
            }

            ProgressText = "✅ Done";
            DownloadStatus = $"✅ Downloaded {modelName} to:\n{Path.GetFullPath(targetPath)}";
            _ = LoadLocalModelsAsync();
        }
        catch
        {
            ProgressText = "";
            DownloadProgress = 0;
            DownloadStatus = $"❌ Failed to download {modelName}";
        }
    }

    /// <summary>
    /// Represents a file entry from the Hugging Face API.
    /// </summary>
    private class HfFile
    {
        /// <summary>
        /// The type of the file (e.g., "file", "folder").
        /// </summary>
        public string type { get; set; } = "";
        /// <summary>
        /// The path of the file.
        /// </summary>
        public string path { get; set; } = "";
    }
    
    /// <summary>
    /// Loads the list of local models from disk.
    /// </summary>
    private async Task LoadLocalModelsAsync()
    {
        try
        {
            Directory.CreateDirectory("Models");
            var files = Directory.GetFiles("Models", "*.gguf");
            
            LocalModels.Clear();
            foreach (var file in files)
                LocalModels.Add(Path.GetFileName(file));
        }
        catch
        {
            LocalModelsStatus = "❌ Failed to load local models.";
        }
    }

    /// <summary>
    /// Deletes the selected local model from disk.
    /// </summary>
    private async Task DeleteSelectedLocalModelAsync()
    {
        if (SelectedLocalModel is null)
            return;

        var path = Path.Combine("Models", SelectedLocalModel);
        try
        {
            if (File.Exists(path))
                File.Delete(path);

            await LoadLocalModelsAsync();
            LocalModelsStatus = $"🗑 Deleted {SelectedLocalModel}";
        }
        catch (Exception ex)
        {
            LocalModelsStatus = $"❌ Failed to delete {SelectedLocalModel}: {ex.Message}";
        }
    }

}
