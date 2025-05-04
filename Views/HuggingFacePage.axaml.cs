using Avalonia.Controls;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

public partial class HuggingFacePage : UserControl
{
    public HuggingFacePage()
    {
        InitializeComponent();
        DataContext = new HuggingFacePageViewModel();
    }
}