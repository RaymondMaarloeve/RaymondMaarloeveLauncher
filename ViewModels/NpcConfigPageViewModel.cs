using System;
using System.Collections.ObjectModel;
using System.Reactive;
using RaymondMaarloeveLauncher.Models;
using ReactiveUI;

namespace RaymondMaarloeveLauncher.ViewModels;

class NpcConfigPageViewModel : ReactiveObject
{
    public ObservableCollection<NpcModel> Npcs { get; } = new();
    public ObservableCollection<string> AvailableModels { get; } = new()
    {
        "model-alpha.gguf",
        "model-beta.gguf",
        "model-gamma.gguf"
    };

    public ReactiveCommand<NpcModel, Unit> RemoveNpcCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNpcCommand { get; }

    public NpcConfigPageViewModel()
    {
        RemoveNpcCommand = ReactiveCommand.Create<NpcModel>(npc => Npcs.Remove(npc));
        AddNpcCommand = ReactiveCommand.Create(() =>
        {
            var npcName = $"NPC{Npcs.Count + 1}";
            Npcs.Add(new NpcModel { Name = npcName });
        });

        // Dodaj domyślnie jednego NPC na start
        AddNpcCommand.Execute().Subscribe();
    }
}
