using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Arvrel.Desktop.Controls;
using Arvrel.Desktop.ViewModels;

namespace Arvrel.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private bool _injectionWorkspaceInstalled;

    public MainWindow()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => InstallInjectionWorkspace();

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

        Opened += (_, _) =>
        {
            InstallInjectionWorkspace();
            _timer.Start();
        };
        Closed += MainWindow_Closed;
    }

    private void InstallInjectionWorkspace()
    {
        if (_injectionWorkspaceInstalled)
            return;

        foreach (var tabControl in this.GetVisualDescendants().OfType<TabControl>())
        {
            var injectionTab = tabControl.Items
                .OfType<TabItem>()
                .FirstOrDefault(item => string.Equals(
                    item.Header?.ToString(),
                    "INJECT",
                    StringComparison.Ordinal));

            if (injectionTab is null)
                continue;

            injectionTab.Content = new InjectionWorkspace();
            _injectionWorkspaceInstalled = true;
            return;
        }
    }

    private async void MainWindow_Closed(object? sender, EventArgs e)
    {
        _timer.Stop();
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.DisposeAsync();
    }
}
