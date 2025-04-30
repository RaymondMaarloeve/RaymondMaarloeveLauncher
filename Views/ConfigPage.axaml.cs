using Avalonia.Controls;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

public partial class ConfigPage : UserControl
{
    public ConfigPage()
    {
        InitializeComponent();
        DataContext = new ConfigPageViewModel();
    }
}