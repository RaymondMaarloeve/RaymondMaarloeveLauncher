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

namespace RaymondMaarloeveLauncher.ViewModels;

public class HuggingFacePageViewModel : ReactiveObject
{
    private static readonly HttpClient _httpClient = new();

    public ObservableCollection<string> AvailableModels { get; } = new();

    private string? _selectedModel;
    public string? SelectedModel
    {
        get => _selectedModel;
        set => this.RaiseAndSetIfChanged(ref _selectedModel, value);
    }

    private string _downloadStatus = "";
    public string DownloadStatus
    {
        get => _downloadStatus;
        set => this.RaiseAndSetIfChanged(ref _downloadStatus, value);
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

    public HuggingFacePageViewModel()
    {
        DownloadModelCommand = ReactiveCommand.CreateFromTask(DownloadSelectedModelAsync);
        LoadModelsCommand = ReactiveCommand.CreateFromTask(LoadAvailableModelsAsync);
        

        // automatyczne ładowanie modeli przy starcie
        _ = LoadAvailableModelsAsync();
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
                .Where(f => f.type == "file")
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
        System.Console.WriteLine(url);

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
}
