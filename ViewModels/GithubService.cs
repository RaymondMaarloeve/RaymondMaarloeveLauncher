using System;
using System.IO;
using Octokit;

namespace RaymondMaarloeveLauncher.ViewModels;

public static class GitHubService
{
    private static GitHubClient? _client;

    public static GitHubClient GetClient()
    {
        if (_client != null)
            return _client;

        var product = new ProductHeaderValue("RaymondMaarloeveLauncher");
        string? token = null;
        var tokenPath = "GITHUBTOKEN.txt";

        if (!File.Exists(tokenPath) )
        {
            Console.WriteLine("No txt file found. Using unauthenticated mode (60 req/h limit).");
            _client = new GitHubClient(product);
            return _client;
        }

        token = File.ReadAllText(tokenPath).Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("No token found in txt file. Using unauthenticated mode (60 req/h limit).");
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
            Console.WriteLine($"Authenticated as: {user.Login}");
            _client = client;
            return _client;
        }
        catch
        {
            Console.WriteLine("Invalid token. Falling back to unauthenticated mode.");
        }

        _client = new GitHubClient(product);
        return _client;
    }
}