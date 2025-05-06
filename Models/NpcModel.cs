using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace RaymondMaarloeveLauncher.Models;

public class NpcModel : ReactiveObject
{
    public string Name { get; set; } = "";

    private string? _selectedModel;
    public string? SelectedModel
    {
        get => _selectedModel;
        set => this.RaiseAndSetIfChanged(ref _selectedModel, value);
    }

    public ObservableCollection<string> AvailableModels { get; }
    public ReactiveCommand<NpcModel, Unit> RemoveCommand { get; }

    public NpcModel(IEnumerable<string> availableModels, ReactiveCommand<NpcModel, Unit> removeCommand)
    {
        AvailableModels = new ObservableCollection<string>(availableModels);
        RemoveCommand = removeCommand;
    }
}
