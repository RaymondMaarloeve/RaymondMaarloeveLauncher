using ReactiveUI;
using System.Reactive;
using Avalonia.Controls;
using RaymondMaarloeveLauncher.Views;

namespace RaymondMaarloeveLauncher.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> ShowReleasePageCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowHuggingFacePageCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowConfigPageCommand { get; }

    private UserControl _currentPage;
    public UserControl CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public MainWindowViewModel()
    {
        ShowReleasePageCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new ReleasePage();
        });
        ShowHuggingFacePageCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new HuggingFacePage();
        });
        ShowConfigPageCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new ConfigPage();
        });

        CurrentPage = new ReleasePage(); // ustawienie domyślnego widoku
    }
}