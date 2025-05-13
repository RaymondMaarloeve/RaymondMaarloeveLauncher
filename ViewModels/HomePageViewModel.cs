using ReactiveUI;
using System.IO;
using System.Reactive;

namespace RaymondMaarloeveLauncher.ViewModels;

public class HomePageViewModel : ReactiveObject
{
    private const string TokenPath = "GITHUBTOKEN.txt";

    private string _githubToken = "";
    public string GithubToken
    {
        get => _githubToken;
        set => this.RaiseAndSetIfChanged(ref _githubToken, value);
    }

    private string _githubStatus = "Token not checked.";
    public string GithubStatus
    {
        get => _githubStatus;
        set => this.RaiseAndSetIfChanged(ref _githubStatus, value);
    }

    public ReactiveCommand<Unit, Unit> SubmitTokenCommand { get; }
    public ReactiveCommand<Unit, Unit> CheckTokenStatusCommand { get; }

    public HomePageViewModel()
    {
        SubmitTokenCommand = ReactiveCommand.Create(SaveTokenToFile);
        CheckTokenStatusCommand = ReactiveCommand.Create(CheckGithubStatusAsync);

        // wczytaj token jeśli istnieje
        if (File.Exists(TokenPath))
            GithubToken = File.ReadAllText(TokenPath).Trim();
    }

    private void SaveTokenToFile()
    {
        File.WriteAllText(TokenPath, GithubToken.Trim());
        GithubStatus = "✅ Token saved.";
    }

    private void CheckGithubStatusAsync()
    {
        GithubStatus = GitHubService.GithubStatus;
    }
}