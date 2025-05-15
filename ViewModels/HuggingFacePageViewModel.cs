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

public class HuggingFacePageViewModel : ReactiveObject
{
    private static readonly HttpClient _httpClient = new();

    public ObservableCollection<string> AvailableModels { get; } = new();
    
    public ObservableCollection<string> LocalModels { get; } = new();

    private string? _selectedModel;
    public string? SelectedModel
    {
        get => _selectedModel;
        set => this.RaiseAndSetIfChanged(ref _selectedModel, value);
    }
    
    private string? _selectedLocalModel;
    public string? SelectedLocalModel
    {
        get => _selectedLocalModel;
        set => this.RaiseAndSetIfChanged(ref _selectedLocalModel, value);
    }

    private string _downloadStatus = "";
    public string DownloadStatus
    {
        get => _downloadStatus;
        set => this.RaiseAndSetIfChanged(ref _downloadStatus, value);
    }

    private string _localModelsStatus = "";

    public string LocalModelsStatus
    {
        get => _localModelsStatus;
        set => this.RaiseAndSetIfChanged(ref _localModelsStatus, value);
    }
    
    private double _downloadProgress;
    public double DownloadProgress
    {
        get => _downloadProgress;
        set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
    }

    private string _progressText = "";
    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }

    public ReactiveCommand<Unit, Unit> DownloadModelCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadModelsCommand { get; }
    
    public ReactiveCommand<Unit, Unit> LoadLocalModelsCommand { get; }
    
    public ReactiveCommand<Unit, Unit> DeleteModelCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveToJsonCommand { get; }
    private const string ConfigPath = "game_config.json";

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

    private class HfFile
    {
        public string type { get; set; } = "";
        public string path { get; set; } = "";
    }
    
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
