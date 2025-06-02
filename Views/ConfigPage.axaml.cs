using Avalonia.Controls;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

/// <summary>
/// Interaction logic for the configuration page view.
/// </summary>
public partial class ConfigPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigPage"/> class.
    /// Sets the DataContext to a new ConfigPageViewModel.
    /// </summary>
    public ConfigPage()
    {
        InitializeComponent();
        DataContext = new ConfigPageViewModel();
    }
}