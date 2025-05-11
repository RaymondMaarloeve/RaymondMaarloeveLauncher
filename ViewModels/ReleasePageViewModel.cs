using System;
using Avalonia;
using Octokit;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using System.IO.Compression;
using Avalonia.Controls.ApplicationLifetimes;
using MainWindowViewModel = RaymondMaarloeveLauncher.ViewModels.MainWindowViewModel;


namespace RaymondMaarloeveLauncher.ViewModels;

public class ReleasePageViewModel : ViewModelBase
{

    public ObservableCollection<Release> Releases { get; } = new();
    public ObservableCollection<Release> ServerReleases { get; } = new();
    
    
    private Release _selectedRelease;
    public Release SelectedRelease
    {
        get => _selectedRelease;
        set => this.RaiseAndSetIfChanged(ref _selectedRelease, value);
    }
    
    private Release _selectedServerRelease;
    public Release SelectedServerRelease
    {
        get => _selectedServerRelease;
        set => this.RaiseAndSetIfChanged(ref _selectedServerRelease, value);
    }

    private double _downloadProgress;
    public double DownloadProgress
    {
        get => _downloadProgress;
        set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
    }
    
    private double _serverDownloadProgress;
    public double ServerDownloadProgress
    {
        get => _serverDownloadProgress;
        set => this.RaiseAndSetIfChanged(ref _serverDownloadProgress, value);
    }

    private string _progressText = "";
    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }
    
    private string _serverProgressText = "";
    public string ServerProgressText
    {
        get => _serverProgressText;
        set => this.RaiseAndSetIfChanged(ref _serverProgressText, value);
    }
    

    
    private string _downloadStatus;
    public string DownloadStatus
    {
        get => _downloadStatus;
        set => this.RaiseAndSetIfChanged(ref _downloadStatus, value);
    }
    
    private string _serverDownloadStatus;
    public string ServerDownloadStatus
    {
        get => _serverDownloadStatus;
        set => this.RaiseAndSetIfChanged(ref _serverDownloadStatus, value);
    }
    
    public ReactiveCommand<Unit, Unit> DownloadSelectedReleaseCommand { get; }
    public ReactiveCommand<Unit, Unit> DownloadSelectedServerReleaseCommand { get; }

    public ReleasePageViewModel()
    {
        try
        {
            LoadReleases();
        }
        catch (Exception e)
        {
            DownloadStatus = $"❌ Unexpected error: {e.Message}";
        }

        DownloadSelectedReleaseCommand = ReactiveCommand.CreateFromTask(DownloadSelectedReleaseAsync);
        DownloadSelectedServerReleaseCommand = ReactiveCommand.CreateFromTask(DownloadSelecterServerReleaseAsync);
    }
    private async void LoadReleases()
    {
        // var client = new GitHubClient(new ProductHeaderValue("GameLauncher"));
        var client = GitHubService.GetClient();

        var releases = await client.Repository.Release.GetAll("RaymondMaarloeve", "RaymondMaarloeve");

        Releases.Clear();
        foreach (var release in releases)
            Releases.Add(release);
        
        var serverReleases = await client.Repository.Release.GetAll("RaymondMaarloeve", "LLMServer");
        ServerReleases.Clear();
        foreach (var release in serverReleases)
            ServerReleases.Add(release);
    }
    
    private async Task DownloadSelectedReleaseAsync()
    {
        try
        {
            DownloadStatus = "🔍 Checking your release choice...";

            if (SelectedRelease == null)
            {
                DownloadStatus = "❌ Any release wasn't selected.";
                return;
            }

            if (SelectedRelease.Assets == null || SelectedRelease.Assets.Count == 0)
            {
                DownloadStatus = $"⚠️ Release \"{SelectedRelease.Name}\" doesn't include any files.";
                return;
            }

            var asset = SelectedRelease.Assets[0];
            var url = asset.BrowserDownloadUrl;

            DownloadStatus = $"🔄 Downloading file: {asset.Name}...";
            ProgressText = "0%";
            DownloadProgress = 0;

            var filePath = asset.Name;

            try
            {
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes > 0;

                await using var remoteStream = await response.Content.ReadAsStreamAsync();
                await using var localFileStream = File.Create(filePath);

                var buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await remoteStream.ReadAsync(buffer)) > 0)
                {
                    await localFileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalRead += bytesRead;

                    if (canReportProgress)
                    {
                        double percent = (double)totalRead / totalBytes * 100;
                        DownloadProgress = percent;
                        ProgressText = $"{percent:F1}%";
                    }
                }

                ProgressText = "✅ Done";
            }
            catch (Exception ex)
            {
                ProgressText = "";
                DownloadProgress = 0;
                DownloadStatus = $"❌ Download error: {ex.Message}";
                return;
            }

            // Extract ZIP
            if (Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var extractPath ="GameBuild";

                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                ZipFile.ExtractToDirectory(filePath, extractPath);
                File.Delete(filePath);

                var nameParts = SelectedRelease.Name?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var version = nameParts != null && nameParts.Length >= 2 ? nameParts[1] : "unknown";

                var versionPath = Path.Combine(extractPath, "version.txt");
                await File.WriteAllTextAsync(versionPath, version);

                DownloadStatus = $"✅ Downloaded and extracted to: {extractPath}";

                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime
                    && lifetime.MainWindow?.DataContext is MainWindowViewModel mainVm)
                {
                    mainVm.LoadCurrentVersionFromFile();
                }
            }
            else
            {
                DownloadStatus = $"✅ Downloaded to: {filePath}";
            }
        }
        catch (Exception ex)
        {
            DownloadProgress = 0;
            ProgressText = "";
            DownloadStatus = $"❌ Unexpected error: {ex.Message}";
        }
    }

    private async Task DownloadSelecterServerReleaseAsync()
    {
        try
        {
            ServerDownloadStatus = "🔍 Checking your release choice...";

            if (SelectedServerRelease == null)
            {
                ServerDownloadStatus = "❌ Any release wasn't selected.";
                return;
            }

            if (SelectedServerRelease.Assets == null || SelectedServerRelease.Assets.Count == 0)
            {
                ServerDownloadStatus = $"⚠️ Release \"{SelectedServerRelease.Name}\" doesn't include any files.";
                return;
            }

            
            var asset = SelectedServerRelease.Assets[0];
            if (OperatingSystem.IsWindows() && SelectedServerRelease.Assets.Count > 1)
            {
                asset = SelectedServerRelease.Assets[1];
            }
            var url = asset.BrowserDownloadUrl;

            ServerDownloadStatus = $"🔄 Downloading file: {asset.Name}...";
            ServerProgressText = "0%";
            ServerDownloadProgress = 0;


            var filePath = Path.Combine("Server", "LLMServer");
            if (OperatingSystem.IsWindows())
                filePath += ".exe";
            Directory.CreateDirectory("Server");

            try
            {
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes > 0;

                await using var remoteStream = await response.Content.ReadAsStreamAsync();
                await using var localFileStream = File.Create(filePath);

                var buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await remoteStream.ReadAsync(buffer)) > 0)
                {
                    await localFileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalRead += bytesRead;

                    if (canReportProgress)
                    {
                        double percent = (double)totalRead / totalBytes * 100;
                        ServerDownloadProgress = percent;
                        ServerProgressText = $"{percent:F1}%";
                    }
                }

                ServerProgressText = "✅ Done";
            }
            catch (Exception ex)
            {
                ServerProgressText = "";
                ServerDownloadProgress = 0;
                ServerDownloadStatus = $"❌ Download error: {ex.Message}";
                return;
            }


            
            ServerDownloadStatus = $"✅ Downloaded to: {filePath}";
            
        }
        catch (Exception ex)
        {
            ServerDownloadProgress = 0;
            ServerProgressText = "";
            ServerDownloadStatus = $"❌ Unexpected error: {ex.Message}";
        }
    }
}