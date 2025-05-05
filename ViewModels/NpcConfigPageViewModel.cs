using System;
using System.Collections.ObjectModel;
using System.Reactive;
using RaymondMaarloeveLauncher.Models;
using ReactiveUI;

namespace RaymondMaarloeveLauncher.ViewModels;

public class NpcConfigPageViewModel : ReactiveObject
{
    public ObservableCollection<NpcModel> Npcs { get; } = new();
    public ObservableCollection<string> AvailableModels { get; } = new()
    {
        "alpha.gguf", "beta.gguf", "gamma.gguf"
    };

    public ReactiveCommand<NpcModel, Unit> RemoveNpcCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNpcCommand { get; }

    public NpcConfigPageViewModel()
    {
        RemoveNpcCommand = ReactiveCommand.Create<NpcModel>(npc => Npcs.Remove(npc));
        AddNpcCommand = ReactiveCommand.Create(() =>
        {
            var npc = new NpcModel(AvailableModels, RemoveNpcCommand)
            {
                Name = $"NPC{Npcs.Count + 1}"
            };
            Npcs.Add(npc);
        });

        // Dodaj pierwszy NPC
        AddNpcCommand.Execute().Subscribe();
    }
}

