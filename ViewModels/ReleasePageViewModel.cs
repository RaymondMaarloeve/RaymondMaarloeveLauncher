using System;
using Octokit;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using System.IO.Compression;


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
        LoadReleases();

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

            using var httpClient = new HttpClient();

            byte[] bytes;
            try
            {
                bytes = await httpClient.GetByteArrayAsync(url);
            }
            catch (Exception ex)
            {
                DownloadStatus = $"❌ Download error: {ex.Message}";
                return;
            }

            var installPath = AppContext.BaseDirectory;
            var filePath = Path.Combine(installPath, asset.Name);

            try
            {
                await File.WriteAllBytesAsync(filePath, bytes);
            }
            catch (Exception ex)
            {
                DownloadStatus = $"❌ Failed to save file:\n{ex.Message}";
                return;
            }

                            
            if (Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var extractPath = Path.Combine(Path.GetDirectoryName(filePath), "GameBuild");

                // Upewnij się, że folder docelowy nie istnieje lub go wyczyść
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                ZipFile.ExtractToDirectory(filePath, extractPath);

                DownloadStatus = $"✅ Downloaded and extracted to: {extractPath}";
            }
            else
            {
                DownloadStatus = $"✅ Downloaded to: {filePath}";
            }
        }
        catch (Exception ex)
        {
            DownloadStatus = $"❌ Unexpected error: {ex.Message}";
        }
    }


}