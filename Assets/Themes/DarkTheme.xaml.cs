using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RaymondMaarloeveLauncher.Assets.Themes;

public partial class DarkTheme : ResourceDictionary
{
    public DarkTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}