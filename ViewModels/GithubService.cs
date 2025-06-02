using System.IO;
using Octokit;

namespace RaymondMaarloeveLauncher.ViewModels;

/// <summary>
/// Provides methods for authenticating and interacting with the GitHub API.
/// </summary>
public static class GitHubService
{
    /// <summary>
    /// Cached instance of the GitHubClient.
    /// </summary>
    private static GitHubClient? _client;
    /// <summary>
    /// Status message describing the current authentication state with GitHub.
    /// </summary>
    public static string GithubStatus { get; private set; } = "";

    /// <summary>
    /// Gets an authenticated or unauthenticated GitHubClient instance.
    /// If a token is found in GITHUBTOKEN.txt, uses it for authentication.
    /// Otherwise, returns a client in unauthenticated mode (60 requests/hour limit).
    /// </summary>
    /// <returns>A GitHubClient instance, authenticated if possible.</returns>
    public static GitHubClient GetClient()
    {
        if (_client != null)
            return _client;

        var product = new ProductHeaderValue("RaymondMaarloeveLauncher");
        string? token = null;
        var tokenPath = "GITHUBTOKEN.txt";

        if (!File.Exists(tokenPath) )
        {
            GithubStatus = "No txt file found. Using unauthenticated mode (60 req/h limit).";
            _client = new GitHubClient(product);
            return _client;
        }

        token = File.ReadAllText(tokenPath).Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            GithubStatus = "No token found in txt file. Using unauthenticated mode (60 req/h limit).";
            _client = new GitHubClient(product);
            return _client;
        }
        
        var client = new GitHubClient(product)
        {
            Credentials = new Credentials(token)
        };
        
        try
        {
            var user = client.User.Current().Result;
            GithubStatus = "Authenticated as: " + user.Login;
            _client = client;
            return _client;
        }
        catch
        {
            GithubStatus = "Invalid token. Falling back to unauthenticated mode.";
        }

        _client = new GitHubClient(product);
        return _client;
    }
}