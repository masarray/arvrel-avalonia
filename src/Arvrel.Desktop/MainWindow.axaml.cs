using Avalonia.Controls;
using Avalonia.Threading;
using Arvrel.Desktop.ViewModels;

namespace Arvrel.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(40)
        };
        _timer.Tick += (_, _) =>
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;

            viewModel.Tick();
            viewModel.TickProcessBus();
        };

        Opened += (_, _) => _timer.Start();
        Closed += MainWindow_Closed;
    }

    private async void MainWindow_Closed(object? sender, EventArgs e)
    {
        _timer.Stop();
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.DisposeAsync();
    }
}
