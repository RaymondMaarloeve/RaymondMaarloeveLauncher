using System;
using ReactiveUI;
using System.Reactive;
using Avalonia.Controls;
using RaymondMaarloeveLauncher.Views;
using System.Diagnostics;
using System.IO;

namespace RaymondMaarloeveLauncher.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> ShowReleasePageCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowHuggingFacePageCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowConfigPageCommand { get; }
    
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


    public MainWindowViewModel()
    {
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

        CurrentPage = new ReleasePage(); // ustawienie domyślnego widoku
        
        LaunchGameCommand = ReactiveCommand.Create(LaunchGame);
        
        LoadCurrentVersionFromFile();
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