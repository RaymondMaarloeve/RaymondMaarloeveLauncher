using System.IO;
using Octokit;

namespace RaymondMaarloeveLauncher.ViewModels;

public static class GitHubService
{
    private static GitHubClient? _client;
    public static string GithubStatus { get; private set; } = "";

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