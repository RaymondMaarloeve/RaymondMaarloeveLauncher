using Avalonia.Controls;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

public partial class ReleasePage : UserControl
{
    public ReleasePage()
    {
        InitializeComponent();
        DataContext = new ReleasePageViewModel();
    }
}