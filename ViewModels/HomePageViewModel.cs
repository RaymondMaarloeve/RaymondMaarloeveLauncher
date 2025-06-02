using ReactiveUI;
using System.IO;
using System.Reactive;

namespace RaymondMaarloeveLauncher.ViewModels;

/// <summary>
/// ViewModel for the home page, responsible for managing GitHub token and status.
/// </summary>
public class HomePageViewModel : ReactiveObject
{
    /// <summary>
    /// Path to the file storing the GitHub token.
    /// </summary>
    private const string TokenPath = "GITHUBTOKEN.txt";

    /// <summary>
    /// The GitHub token entered by the user.
    /// </summary>
    private string _githubToken = "";
    /// <summary>
    /// Gets or sets the GitHub token.
    /// </summary>
    public string GithubToken
    {
        get => _githubToken;
        set => this.RaiseAndSetIfChanged(ref _githubToken, value);
    }

    /// <summary>
    /// The current status of the GitHub token.
    /// </summary>
    private string _githubStatus = "Token not checked.";
    /// <summary>
    /// Gets or sets the GitHub status message.
    /// </summary>
    public string GithubStatus
    {
        get => _githubStatus;
        set => this.RaiseAndSetIfChanged(ref _githubStatus, value);
    }

    /// <summary>
    /// Command to submit and save the GitHub token to file.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SubmitTokenCommand { get; }
    /// <summary>
    /// Command to check the current GitHub authentication status.
    /// </summary>
    public ReactiveCommand<Unit, Unit> CheckTokenStatusCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HomePageViewModel"/> class.
    /// Loads the token from file if it exists.
    /// </summary>
    public HomePageViewModel()
    {
        SubmitTokenCommand = ReactiveCommand.Create(SaveTokenToFile);
        CheckTokenStatusCommand = ReactiveCommand.Create(CheckGithubStatusAsync);

        // load the token if it exists
        if (File.Exists(TokenPath))
            GithubToken = File.ReadAllText(TokenPath).Trim();
    }

    /// <summary>
    /// Saves the GitHub token to file and updates the status message.
    /// </summary>
    private void SaveTokenToFile()
    {
        File.WriteAllText(TokenPath, GithubToken.Trim());
        GithubStatus = "✅ Token saved.";
    }

    /// <summary>
    /// Checks the current GitHub authentication status and updates the status message.
    /// </summary>
    private void CheckGithubStatusAsync()
    {
        GithubStatus = GitHubService.GithubStatus;
    }
}