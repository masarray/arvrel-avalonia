using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class InjectionWorkspaceSourceTests
{
    [TestMethod]
    public void InjectionWorkspace_PresentsFocusedCurrentAndVoltageTestSetHeroes()
    {
        var xaml = Read("src", "Arvrel.Desktop", "Controls", "InjectionWorkspace.axaml");
        var behavior = Read("src", "Arvrel.Desktop", "Controls", "InjectionWorkspace.axaml.cs");
        var channel = Read("src", "Arvrel.Desktop", "ViewModels", "InjectionChannelViewModel.cs");
        var projection = Read("src", "Arvrel.Desktop", "ViewModels", "MainWindowViewModel.InjectionUx.cs");
        var mainWindow = Read("src", "Arvrel.Desktop", "MainWindow.axaml.cs");

        _ = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

        StringAssert.Contains(xaml, "SECONDARY INJECTION TEST SET");
        StringAssert.Contains(xaml, "CURRENT OUTPUTS");
        StringAssert.Contains(xaml, "VOLTAGE OUTPUTS");
        StringAssert.Contains(xaml, "ItemsSource=\"{Binding CurrentInjectionChannels}\"");
        StringAssert.Contains(xaml, "ItemsSource=\"{Binding VoltageInjectionChannels}\"");
        StringAssert.Contains(xaml, "SelectedItem=\"{Binding SelectedPreset, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "Text=\"{Binding FrequencyText, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "IsChecked=\"{Binding Enabled, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "Text=\"{Binding RmsText, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "Text=\"{Binding AngleText, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "EngineeringSignalLabel");
        StringAssert.Contains(xaml, "ApplyInjectionCommand");
        StringAssert.Contains(xaml, "FaultCommand");
        StringAssert.Contains(xaml, "ClearInjectionCommand");
        StringAssert.Contains(xaml, "ResetRelayCommand");
        StringAssert.Contains(xaml, "Content=\"APPLY\"");
        StringAssert.Contains(xaml, "Content=\"A-G FAULT\"");
        StringAssert.Contains(xaml, "Content=\"CLEAR\"");
        StringAssert.Contains(xaml, "Content=\"RESET RELAY\"");

        Assert.IsFalse(xaml.Contains("PROVENANCE / MAPPING", StringComparison.Ordinal));
        Assert.IsFalse(xaml.Contains("ADVANCED INJECTION LABORATORY", StringComparison.Ordinal));
        Assert.IsFalse(xaml.Contains("Virtual secondary-injection workspace", StringComparison.Ordinal));
        Assert.IsFalse(xaml.Contains("ChannelFamily", StringComparison.Ordinal));

        StringAssert.Contains(behavior, "partial class InjectionWorkspace");
        StringAssert.Contains(channel, "V L1-E");
        StringAssert.Contains(channel, "V L2-E");
        StringAssert.Contains(channel, "V L3-E");
        StringAssert.Contains(channel, "I L1");
        StringAssert.Contains(channel, "I L2");
        StringAssert.Contains(channel, "I L3");
        StringAssert.Contains(channel, "I RES / 3I0");
        StringAssert.Contains(channel, "V RES / 3V0");
        StringAssert.Contains(projection, "CurrentInjectionChannels");
        StringAssert.Contains(projection, "VoltageInjectionChannels");

        StringAssert.Contains(mainWindow, "InstallInjectionWorkspace");
        StringAssert.Contains(mainWindow, "new InjectionWorkspace()");
        Assert.IsFalse(mainWindow.Contains("System.Windows", StringComparison.Ordinal));
    }

    [TestMethod]
    public void HardwareButtons_UseDedicatedAvaloniaControlThemeInsteadOfManualPointerState()
    {
        var app = Read("src", "Arvrel.Desktop", "App.axaml");
        var behavior = Read("src", "Arvrel.Desktop", "Controls", "VirtualRelayFaceplate.axaml.cs");

        _ = XDocument.Parse(app, LoadOptions.PreserveWhitespace);

        StringAssert.Contains(app, "x:Key=\"RelayHardwareButtonTheme\"");
        StringAssert.Contains(app, "TargetType=\"Button\"");
        StringAssert.Contains(app, "Theme\" Value=\"{StaticResource RelayHardwareButtonTheme}\"");
        StringAssert.Contains(app, "x:Name=\"PART_RelayButtonBorder\"");
        StringAssert.Contains(app, "x:Name=\"PART_ContentPresenter\"");
        StringAssert.Contains(app, "Foreground=\"{TemplateBinding Foreground}\"");
        StringAssert.Contains(app, "Selector=\"^:pointerover\"");
        StringAssert.Contains(app, "Selector=\"^:pressed\"");
        StringAssert.Contains(app, "Selector=\"^:focus-visible\"");
        StringAssert.Contains(app, "#F7FAFB");
        StringAssert.Contains(app, "#4F5E66");

        Assert.IsFalse(behavior.Contains("PointerEntered", StringComparison.Ordinal));
        Assert.IsFalse(behavior.Contains("PointerPressed", StringComparison.Ordinal));
        Assert.IsFalse(behavior.Contains("PointerCaptureLost", StringComparison.Ordinal));
        Assert.IsFalse(behavior.Contains("HardwareHoverBackground", StringComparison.Ordinal));
    }

    private static string Read(params string[] segments)
        => File.ReadAllText(Locate(segments));

    private static string Locate(params string[] segments)
    {
        var starts = new[]
        {
            new DirectoryInfo(Environment.CurrentDirectory),
            new DirectoryInfo(AppContext.BaseDirectory)
        };

        foreach (var start in starts)
        {
            for (var current = start; current is not null; current = current.Parent)
            {
                var candidate = Path.Combine(new[] { current.FullName }.Concat(segments).ToArray());
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        throw new FileNotFoundException($"Unable to locate {Path.Combine(segments)}.");
    }
}
