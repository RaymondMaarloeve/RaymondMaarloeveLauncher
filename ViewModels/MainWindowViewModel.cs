using System;
using ReactiveUI;
using System.Reactive;
using Avalonia.Controls;
using RaymondMaarloeveLauncher.Views;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace RaymondMaarloeveLauncher.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> ShowHomePageCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowReleasePageCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowHuggingFacePageCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowConfigPageCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowNpcConfigPageCommand { get; }
    
    public ReactiveCommand<Unit, Unit> LaunchGameCommand { get; }

    private UserControl _currentPage;
    public UserControl CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }
    
    private string _launchStatus;
    public string LaunchStatus
    {
        get => _launchStatus;
        set => this.RaiseAndSetIfChanged(ref _launchStatus, value);
    }
    
    private string _currentVersion = "Not downloaded.";
    public string CurrentVersion
    {
        get => _currentVersion;
        set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
    }
    
    private string _latestReleaseBody;
    public string LatestReleaseBody
    {
        get => _latestReleaseBody;
        set => this.RaiseAndSetIfChanged(ref _latestReleaseBody, value);
    }



    public MainWindowViewModel()
    {
        ShowHomePageCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new HomePage();
        });
        ShowReleasePageCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new ReleasePage();
        });
        ShowHuggingFacePageCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new HuggingFacePage();
        });
        ShowConfigPageCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new ConfigPage();
        });
        ShowNpcConfigPageCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new NpcConfigPage();
        });

        CurrentPage = new HomePage(); // ustawienie domyślnego widoku
        
        LaunchGameCommand = ReactiveCommand.Create(LaunchGame);
        
        LoadCurrentVersionFromFile();

        _ = LoadLatestReleaseDescription();
    }
    
    public async Task LoadLatestReleaseDescription()
    {
        var client = new GitHubClient(new ProductHeaderValue("GameLauncher"));

        try
        {
            var latest = await client.Repository.Release.GetLatest("Gitmanik", "RaymondMaarloeve");
            if (latest.Body == null)
            {
                LatestReleaseBody = "No description.";
                return;
            }
            var cleanedBody = RemoveMarkdownTitle(latest.Body);
            LatestReleaseBody = FormatMarkdownCommits(cleanedBody);
        }
        catch (Exception ex)
        {
            LatestReleaseBody = $"❌ Error loading changelog:\n{ex.Message}";
        }
    }
    
    private string FormatMarkdownCommits(string markdown)
    {
        var lines = markdown.Split('\n')
            .Select(line =>
            {
                // detect lines starting with 7-character commit ID
                if (System.Text.RegularExpressions.Regex.IsMatch(line.Trim(), @"^[0-9a-f]{7}\b"))
                    return "- " + line.Trim(); // change to md list item
                return line;
            });

        return string.Join("\n", lines);
    }
    
    private string RemoveMarkdownTitle(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var lines = markdown.Split('\n')
            .SkipWhile(line => line.TrimStart().StartsWith("#")) 
            .SkipWhile(string.IsNullOrWhiteSpace) 
            .ToList();

        return string.Join('\n', lines);
    }
    
    private void LaunchGame()
    {
        var gameDir = Path.Combine(AppContext.BaseDirectory, "GameBuild");
        var exePath = Path.Combine(gameDir, "StandaloneWindows64.exe");

        if (!Directory.Exists(gameDir))
        {
            // Możesz dodać status/komunikat
            LaunchStatus = "❌ Game directory doesn't exist.";
            return;
        }

        if (!File.Exists(exePath))
        {
            LaunchStatus = "❌ StandaloneWindows64.exe file doesn't exist.";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = gameDir,
                UseShellExecute = true
            });

            // Zamknij launcher
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            LaunchStatus = $"❌ Game launching error: {ex.Message}";
        }
    }
    
    public void LoadCurrentVersionFromFile()
    {
        try
        {
            var versionPath = Path.Combine(AppContext.BaseDirectory, "GameBuild", "version.txt");

            if (File.Exists(versionPath))
            {
                var version = File.ReadAllText(versionPath).Trim();
                CurrentVersion = $"Version: {version}";
            }
            else
            {
                CurrentVersion = "Not downloaded.";
            }
        }
        catch (Exception ex)
        {
            CurrentVersion = $"Error: {ex.Message}";
        }
    }

}