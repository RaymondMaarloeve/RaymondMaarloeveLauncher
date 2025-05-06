using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

public partial class NpcConfigPage : UserControl
{
    public NpcConfigPage()
    {
        InitializeComponent();
        DataContext = new NpcConfigPageViewModel();
    }
}