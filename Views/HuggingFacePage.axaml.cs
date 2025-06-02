using Avalonia.Controls;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher.Views;

/// <summary>
/// Interaction logic for the Hugging Face page view.
/// </summary>
public partial class HuggingFacePage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HuggingFacePage"/> class.
    /// Sets the DataContext to a new HuggingFacePageViewModel.
    /// </summary>
    public HuggingFacePage()
    {
        InitializeComponent();
        DataContext = new HuggingFacePageViewModel();
    }
}