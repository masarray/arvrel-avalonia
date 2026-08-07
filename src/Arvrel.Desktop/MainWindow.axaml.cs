using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Arvrel.Desktop.Controls;
using Arvrel.Desktop.ViewModels;

namespace Arvrel.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private VirtualRelayFaceplate? _virtualRelayFaceplate;
    private EvidenceWorkspace? _evidenceWorkspace;
    private ProtectionSettingsWorkspace? _protectionSettingsWorkspace;
    private ProcessBusWorkspace? _processBusWorkspace;

    public MainWindow()
    {
        InitializeComponent();
        InstallVirtualRelayFaceplate();
        InstallEvidenceWorkspace();
        InstallProtectionSettingsWorkspace();
        InstallProcessBusWorkspace();

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

        DataContextChanged += (_, _) =>
        {
            if (_virtualRelayFaceplate is not null)
                _virtualRelayFaceplate.DataContext = DataContext;
            if (_evidenceWorkspace is not null)
                _evidenceWorkspace.DataContext = DataContext;
            if (_protectionSettingsWorkspace is not null)
                _protectionSettingsWorkspace.DataContext = DataContext;
            if (_processBusWorkspace is not null)
                _processBusWorkspace.DataContext = DataContext;
        };

        Opened += (_, _) => _timer.Start();
        Closed += MainWindow_Closed;
    }

    private void InstallVirtualRelayFaceplate()
    {
        var relayTabs = ResolveRelayWorkspaceTabs();

        _virtualRelayFaceplate = new VirtualRelayFaceplate
        {
            DataContext = DataContext,
            Margin = new Avalonia.Thickness(0, 8, 0, 0)
        };

        relayTabs.Items.Insert(0, new TabItem
        {
            Header = "FACEPLATE",
            Content = _virtualRelayFaceplate
        });
        relayTabs.SelectedIndex = 0;
    }

    private void InstallEvidenceWorkspace()
    {
        var relayTabs = ResolveRelayWorkspaceTabs();
        _evidenceWorkspace = new EvidenceWorkspace
        {
            DataContext = DataContext
        };

        relayTabs.Items.Insert(Math.Min(1, relayTabs.Items.Count), new TabItem
        {
            Header = "EVIDENCE",
            Content = _evidenceWorkspace
        });
    }

    private void InstallProtectionSettingsWorkspace()
    {
        var relayTabs = ResolveRelayWorkspaceTabs();
        _protectionSettingsWorkspace = new ProtectionSettingsWorkspace
        {
            DataContext = DataContext
        };

        relayTabs.Items.Insert(Math.Min(2, relayTabs.Items.Count), new TabItem
        {
            Header = "SETTINGS",
            Content = _protectionSettingsWorkspace
        });
    }

    private TabControl ResolveRelayWorkspaceTabs()
        => this.GetLogicalDescendants()
               .OfType<TabControl>()
               .LastOrDefault()
           ?? throw new InvalidOperationException("Avalonia relay workspace TabControl was not found.");

    private void InstallProcessBusWorkspace()
    {
        var sourceTabs = this.GetLogicalDescendants()
            .OfType<TabControl>()
            .FirstOrDefault();
        if (sourceTabs is null)
            throw new InvalidOperationException("Avalonia source workspace TabControl was not found.");

        _processBusWorkspace = new ProcessBusWorkspace
        {
            DataContext = DataContext
        };

        sourceTabs.Items.Insert(Math.Min(1, sourceTabs.Items.Count), new TabItem
        {
            Header = "PROCESS BUS",
            Content = _processBusWorkspace
        });
    }

    private async void MainWindow_Closed(object? sender, EventArgs e)
    {
        _timer.Stop();
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.DisposeAsync();
    }
}
