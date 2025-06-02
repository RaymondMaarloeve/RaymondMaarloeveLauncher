using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace RaymondMaarloeveLauncher.Models;

/// <summary>
/// Represents an NPC model, including its name, selected model, available models, and remove command.
/// </summary>
public class NpcModel : ReactiveObject
{
    /// <summary>
    /// The name of the NPC.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The currently selected model for the NPC.
    /// </summary>
    private string? _selectedModel;
    /// <summary>
    /// Gets or sets the selected model for the NPC.
    /// </summary>
    public string? SelectedModel
    {
        get => _selectedModel;
        set => this.RaiseAndSetIfChanged(ref _selectedModel, value);
    }

    /// <summary>
    /// The collection of available model names for the NPC.
    /// </summary>
    public ObservableCollection<string> AvailableModels { get; }
    /// <summary>
    /// Command to remove this NPC from the collection.
    /// </summary>
    public ReactiveCommand<NpcModel, Unit> RemoveCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NpcModel"/> class.
    /// </summary>
    /// <param name="availableModels">The collection of available model names.</param>
    /// <param name="removeCommand">The command to remove this NPC.</param>
    public NpcModel(IEnumerable<string> availableModels, ReactiveCommand<NpcModel, Unit> removeCommand)
    {
        AvailableModels = new ObservableCollection<string>(availableModels);
        RemoveCommand = removeCommand;
    }
}
