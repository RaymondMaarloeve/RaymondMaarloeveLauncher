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
    
    
    private Release _selectedRelease;
    public Release SelectedRelease
    {
        get => _selectedRelease;
        set => this.RaiseAndSetIfChanged(ref _selectedRelease, value);
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

    private async void LoadReleases()
    {
        var client = new GitHubClient(new ProductHeaderValue("GameLauncher"));

        var releases = await client.Repository.Release.GetAll("Gitmanik", "RaymondMaarloeve");

        Releases.Clear();
        foreach (var release in releases)
            Releases.Add(release);
    }
    
    private string _downloadStatus;
    public string DownloadStatus
    {
        get => _downloadStatus;
        set => this.RaiseAndSetIfChanged(ref _downloadStatus, value);
    }
    
    public ReactiveCommand<Unit, Unit> DownloadSelectedReleaseCommand { get; }

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

            var installPath = AppContext.BaseDirectory;
            var filePath = Path.Combine(installPath, asset.Name);

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
                var extractPath = Path.Combine(Path.GetDirectoryName(filePath)!, "GameBuild");

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


}