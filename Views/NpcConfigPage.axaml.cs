using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

/// <summary>
/// Interaction logic for the NPC configuration page view.
/// </summary>
public partial class NpcConfigPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NpcConfigPage"/> class.
    /// Sets the DataContext to a new NpcConfigPageViewModel.
    /// </summary>
    public NpcConfigPage()
    {
        InitializeComponent();
        DataContext = new NpcConfigPageViewModel();
    }
}