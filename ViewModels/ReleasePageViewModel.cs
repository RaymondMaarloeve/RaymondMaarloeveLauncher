using System;
using Octokit;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;
using Avalonia.Controls.ApplicationLifetimes;


namespace RaymondMaarloeveLauncher.ViewModels;

/// <summary>
/// ViewModel for the release page, responsible for managing and downloading game and server releases.
/// </summary>
public class ReleasePageViewModel : ViewModelBase
{
    /// <summary>
    /// Collection of game releases available for download.
    /// </summary>
    public ObservableCollection<Release> Releases { get; } = new();
    /// <summary>
    /// Collection of server releases available for download.
    /// </summary>
    public ObservableCollection<Release> ServerReleases { get; } = new();
    
    /// <summary>
    /// The currently selected game release.
    /// </summary>
    private Release _selectedRelease;
    /// <summary>
    /// Gets or sets the selected game release.
    /// </summary>
    public Release SelectedRelease
    {
        get => _selectedRelease;
        set => this.RaiseAndSetIfChanged(ref _selectedRelease, value);
    }
    
    /// <summary>
    /// The currently selected server release.
    /// </summary>
    private Release _selectedServerRelease;
    /// <summary>
    /// Gets or sets the selected server release.
    /// </summary>
    public Release SelectedServerRelease
    {
        get => _selectedServerRelease;
        set => this.RaiseAndSetIfChanged(ref _selectedServerRelease, value);
    }

    /// <summary>
    /// Progress of the current game release download (0-100).
    /// </summary>
    private double _downloadProgress;
    /// <summary>
    /// Gets or sets the download progress percentage for the game release.
    /// </summary>
    public double DownloadProgress
    {
        get => _downloadProgress;
        set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
    }
    
    /// <summary>
    /// Progress of the current server release download (0-100).
    /// </summary>
    private double _serverDownloadProgress;
    /// <summary>
    /// Gets or sets the download progress percentage for the server release.
    /// </summary>
    public double ServerDownloadProgress
    {
        get => _serverDownloadProgress;
        set => this.RaiseAndSetIfChanged(ref _serverDownloadProgress, value);
    }

    /// <summary>
    /// Text representation of the current game release download progress.
    /// </summary>
    private string _progressText = "";
    /// <summary>
    /// Gets or sets the progress text for the game release download.
    /// </summary>
    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }
    
    /// <summary>
    /// Text representation of the current server release download progress.
    /// </summary>
    private string _serverProgressText = "";
    /// <summary>
    /// Gets or sets the progress text for the server release download.
    /// </summary>
    public string ServerProgressText
    {
        get => _serverProgressText;
        set => this.RaiseAndSetIfChanged(ref _serverProgressText, value);
    }
    
    /// <summary>
    /// Status message for the game release download process.
    /// </summary>
    private string _downloadStatus;
    /// <summary>
    /// Gets or sets the download status message for the game release.
    /// </summary>
    public string DownloadStatus
    {
        get => _downloadStatus;
        set => this.RaiseAndSetIfChanged(ref _downloadStatus, value);
    }
    
    /// <summary>
    /// Status message for the server release download process.
    /// </summary>
    private string _serverDownloadStatus;
    /// <summary>
    /// Gets or sets the download status message for the server release.
    /// </summary>
    public string ServerDownloadStatus
    {
        get => _serverDownloadStatus;
        set => this.RaiseAndSetIfChanged(ref _serverDownloadStatus, value);
    }
    
    /// <summary>
    /// Command to download the selected game release.
    /// </summary>
    public ReactiveCommand<Unit, Unit> DownloadSelectedReleaseCommand { get; }
    /// <summary>
    /// Command to download the selected server release.
    /// </summary>
    public ReactiveCommand<Unit, Unit> DownloadSelectedServerReleaseCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReleasePageViewModel"/> class.
    /// Loads releases and sets up download commands.
    /// </summary>
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
    /// <summary>
    /// Loads the list of available game and server releases from GitHub.
    /// </summary>
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
    /// <summary>
    /// Downloads the selected game release and extracts it if necessary.
    /// </summary>
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
            if (OperatingSystem.IsWindows())
                asset = SelectedRelease.Assets.FirstOrDefault(a => a.Name == "Build-StandaloneWindows64.zip") ?? SelectedRelease.Assets[0];
            else if (OperatingSystem.IsLinux())
                asset = SelectedRelease.Assets.FirstOrDefault(a => a.Name == "Build-StandaloneLinux64.zip") ?? SelectedRelease.Assets[0];
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
    /// <summary>
    /// Downloads the selected server release and saves it to the server directory.
    /// </summary>
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