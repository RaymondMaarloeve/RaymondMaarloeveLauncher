using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RaymondMaarloeveLauncher.Assets.Themes;

/// <summary>
/// Resource dictionary for the application's dark theme.
/// </summary>
public partial class DarkTheme : ResourceDictionary
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DarkTheme"/> class and loads the XAML resources.
    /// </summary>
    public DarkTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}