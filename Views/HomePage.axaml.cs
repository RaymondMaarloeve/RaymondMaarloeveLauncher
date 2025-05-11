using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
        DataContext = new HomePageViewModel();
    }
}