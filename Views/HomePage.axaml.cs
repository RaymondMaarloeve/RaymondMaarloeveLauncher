using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

/// <summary>
/// Interaction logic for the home page view.
/// </summary>
public partial class HomePage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// Sets the DataContext to a new HomePageViewModel.
    /// </summary>
    public HomePage()
    {
        InitializeComponent();
        DataContext = new HomePageViewModel();
    }
}