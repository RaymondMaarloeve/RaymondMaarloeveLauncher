using Avalonia.Controls;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

/// <summary>
/// Interaction logic for the release page view.
/// </summary>
public partial class ReleasePage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReleasePage"/> class.
    /// Sets the DataContext to a new ReleasePageViewModel.
    /// </summary>
    public ReleasePage()
    {
        InitializeComponent();
        DataContext = new ReleasePageViewModel();
    }
}