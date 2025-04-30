using Octokit;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RaymondMaarloeveLauncher.ViewModels;

public class ReleasePageViewModel : ViewModelBase
{
    public ObservableCollection<string> Releases { get; } = new();

    public ReleasePageViewModel()
    {
        LoadReleases();
    }

    private async void LoadReleases()
    {
        var client = new GitHubClient(new ProductHeaderValue("GameLauncher"));
        
        var releases = await client.Repository.Release.GetAll("Gitmanik", "RaymondMaarloeve");

        Releases.Clear();
        foreach (var release in releases)
        {
            Releases.Add($"{release.TagName} - {release.Name}");
        }
    }
}