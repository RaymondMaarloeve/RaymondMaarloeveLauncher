using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Reactive;
using Avalonia.Controls;
using RaymondMaarloeveLauncher.Views;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using RaymondMaarloeveLauncher.Models;
using Splat.ModeDetection;

namespace RaymondMaarloeveLauncher.ViewModels;

/// <summary>
/// ViewModel for the main window, responsible for navigation between pages and launching the game.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Command to show the home page.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ShowHomePageCommand { get; }
    /// <summary>
    /// Command to show the release page.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ShowReleasePageCommand { get; }
    /// <summary>
    /// Command to show the Hugging Face page.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ShowHuggingFacePageCommand { get; }
    /// <summary>
    /// Command to show the configuration page.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ShowConfigPageCommand { get; }
    /// <summary>
    /// Command to show the NPC configuration page.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ShowNpcConfigPageCommand { get; }
    /// <summary>
    /// Command to launch the game.
    /// </summary>
    public ReactiveCommand<Unit, Unit> LaunchGameCommand { get; }

    /// <summary>
    /// The currently displayed page in the main window.
    /// </summary>
    private UserControl _currentPage;
    /// <summary>
    /// Gets or sets the current page.
    /// </summary>
    public UserControl CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }
    
    /// <summary>
    /// Status message for the game launch process.
    /// </summary>
    private string _launchStatus;
    /// <summary>
    /// Gets or sets the launch status message.
    /// </summary>
    public string LaunchStatus
    {
        get => _launchStatus;
        set => this.RaiseAndSetIfChanged(ref _launchStatus, value);
    }
    
    /// <summary>
    /// The current version of the game.
    /// </summary>
    private string _currentVersion = "Not downloaded.";
    /// <summary>
    /// Gets or sets the current game version.
    /// </summary>
    public string CurrentVersion
    {
        get => _currentVersion;
        set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
    }
    
    /// <summary>
    /// The body text of the latest release.
    /// </summary>
    private string _latestReleaseBody;
    /// <summary>
    /// Gets or sets the latest release body text.
    /// </summary>
    public string LatestReleaseBody
    {
        get => _latestReleaseBody;
        set => this.RaiseAndSetIfChanged(ref _latestReleaseBody, value);
    }

    /// <summary>
    /// Indicates whether the application is using localhost for the server.
    /// </summary>
    private bool _localhost = true;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// Sets up commands, creates the config file if needed, and loads the current version and latest release description.
    /// </summary>
    public MainWindowViewModel()
    {
        CreateConfigFile();
        
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

        CurrentPage = new HomePage(); // set default view
        
        LaunchGameCommand = ReactiveCommand.Create(LaunchGame);
        
        LoadCurrentVersionFromFile();

        _ = LoadLatestReleaseDescription();
    }

    /// <summary>
    /// Creates the configuration file if it does not exist.
    /// </summary>
    private void CreateConfigFile()
    {
        const string configPath = "game_config.json";
        if (!File.Exists(configPath))
        {
            File.WriteAllText(configPath, "{}");
        }
    }
    
    /// <summary>
    /// Loads the latest release description from GitHub and formats it for display.
    /// </summary>
    private async Task LoadLatestReleaseDescription()
    {
        var client = GitHubService.GetClient();

        try
        {
            var latest = await client.Repository.Release.GetLatest("RaymondMaarloeve", "RaymondMaarloeve");
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
    
    /// <summary>
    /// Formats commit lines in markdown as list items.
    /// </summary>
    /// <param name="markdown">The markdown string to format.</param>
    /// <returns>The formatted markdown string.</returns>
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
    
    /// <summary>
    /// Removes the title from the markdown string.
    /// </summary>
    /// <param name="markdown">The markdown string to process.</param>
    /// <returns>The markdown string without the title.</returns>
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
    
    /// <summary>
    /// Launches the game and the server if required, handling permissions and process management.
    /// </summary>
    private void LaunchGame()
    {
        var gameDir = "GameBuild";
        var exePath = Path.Combine(gameDir, "StandaloneLinux64");
        if (OperatingSystem.IsWindows()) 
            exePath = Path.Combine(gameDir, "StandaloneWindows64.exe");
        var dataPath = Path.Combine(gameDir, "StandaloneLinux64_Data");
        if (OperatingSystem.IsWindows()) 
            dataPath = Path.Combine(gameDir, "StandaloneWindows64_Data");
        var serverDir = "Server";
        var serverExePath = Path.Combine(serverDir, "LLMServer");
        if (OperatingSystem.IsWindows()) 
            serverExePath += ".exe";
        
        


        if (!Directory.Exists(gameDir))
        {
            LaunchStatus = "❌ Game directory doesn't exist.";
            return;
        }
        
        const string configPath = "game_config.json";
        var targetPath = Path.Combine(dataPath, configPath);
        
        if (File.Exists(configPath))
        {
            File.Copy(configPath, targetPath, overwrite: true);
            _localhost = ReadLocalhostFromConfig(configPath);
        }

        if (!File.Exists(exePath))
        {
            LaunchStatus = "❌ StandaloneWindows64.exe file doesn't exist.";
            return;
        }

        if (!Directory.Exists(serverDir))
        {
            LaunchStatus = "❌ Server directory doesn't exist.";
        }

        if (!File.Exists(serverExePath))
        {
            LaunchStatus = "LLMServer.exe file doesn't exist.";
        }
        
        if (OperatingSystem.IsLinux())
        {
            var filePath = serverExePath;
            try
            {
                Process.Start("chmod", $"+x \"{filePath}\"")?.WaitForExit();
            }
            catch (Exception ex)
            {
                LaunchStatus = "Couldn't set executable permission" + ex.Message;
            }
            
            filePath = exePath;
            try
            {
                Process.Start("chmod", $"+x \"{filePath}\"")?.WaitForExit();
            }
            catch (Exception ex)
            {
                LaunchStatus = "Couldn't set executable permission" + ex.Message;
            }
        }

        try
        {
            Process? serverProcess = null;
            if (_localhost)
            {
                serverProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(Directory.GetCurrentDirectory(), serverExePath),
                    WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), serverDir),
                    UseShellExecute = false
                });
            }
            var gameProcess = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), exePath),
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), gameDir),
                UseShellExecute = false
            });

            if (!_localhost)
            {
                Environment.Exit(0);
            }
            
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime life)
            {
                life.MainWindow?.Hide(); // Hide the launcher window
            }
            
            gameProcess?.WaitForExit();

            try
            {
                if (serverProcess != null && !serverProcess.HasExited)
                {
                    serverProcess.Kill(true);
                    serverProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to stop server process: {ex.Message}");
            }
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            LaunchStatus = $"❌ Game launching error: {ex.Message}";
        }
    }

    /// <summary>
    /// Reads the 'localhost' setting from the configuration file.
    /// </summary>
    /// <param name="configPath">Path to the configuration file.</param>
    /// <returns>True if localhost is enabled; otherwise, false.</returns>
    private bool ReadLocalhostFromConfig(string configPath)
    {
        try
        {
            string jsonContent = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<GameData>(jsonContent);
            bool localhost = config?.Localhost ?? false;
            return localhost;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to read config file: {ex.Message}");
            return false;
        }
        return false;
    }
    
    /// <summary>
    /// Loads the current game version from the version file.
    /// </summary>
    public void LoadCurrentVersionFromFile()
    {
        try
        {
            var versionPath = Path.Combine("GameBuild", "version.txt");

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