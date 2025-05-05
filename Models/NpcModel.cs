using ReactiveUI;

namespace RaymondMaarloeveLauncher.Models;

public class NpcModel : ReactiveObject
{
    private string _name = "NPC";
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private string? _selectedModel;
    public string? SelectedModel
    {
        get => _selectedModel;
        set => this.RaiseAndSetIfChanged(ref _selectedModel, value);
    }
}