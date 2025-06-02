using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RaymondMaarloeveLauncher.Assets.Themes;

/// <summary>
/// Resource dictionary for the application's light theme.
/// </summary>
public partial class LightTheme : ResourceDictionary
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LightTheme"/> class and loads the XAML resources.
    /// </summary>
    public LightTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}