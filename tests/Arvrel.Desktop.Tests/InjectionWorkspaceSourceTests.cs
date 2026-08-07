using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class InjectionWorkspaceSourceTests
{
    [TestMethod]
    public void InjectionWorkspace_ProvidesP6ParityTableAndExistingSourceAuthority()
    {
        var xaml = Read("src", "Arvrel.Desktop", "Controls", "InjectionWorkspace.axaml");
        var behavior = Read("src", "Arvrel.Desktop", "Controls", "InjectionWorkspace.axaml.cs");
        var channel = Read("src", "Arvrel.Desktop", "ViewModels", "InjectionChannelViewModel.cs");
        var mainWindow = Read("src", "Arvrel.Desktop", "MainWindow.axaml.cs");

        _ = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

        StringAssert.Contains(xaml, "ADVANCED INJECTION LABORATORY");
        StringAssert.Contains(xaml, "Virtual secondary-injection workspace");
        StringAssert.Contains(xaml, "DIRECT");
        StringAssert.Contains(xaml, "ItemsSource=\"{Binding InjectionChannels}\"");
        StringAssert.Contains(xaml, "SelectedItem=\"{Binding SelectedPreset, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "Text=\"{Binding FrequencyText, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "IsChecked=\"{Binding Enabled, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "Text=\"{Binding RmsText, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "Text=\"{Binding AngleText, Mode=TwoWay}\"");
        StringAssert.Contains(xaml, "EngineeringSignalLabel");
        StringAssert.Contains(xaml, "ChannelFamily");
        StringAssert.Contains(xaml, "ApplyInjectionCommand");
        StringAssert.Contains(xaml, "FaultCommand");
        StringAssert.Contains(xaml, "ClearInjectionCommand");
        StringAssert.Contains(xaml, "ResetRelayCommand");
        StringAssert.Contains(xaml, "APPLY CONFIGURED SOURCE");
        StringAssert.Contains(xaml, "LOAD + START A-G FAULT");
        StringAssert.Contains(xaml, "CLEAR INJECTION");
        StringAssert.Contains(xaml, "RESET RELAY");

        StringAssert.Contains(behavior, "partial class InjectionWorkspace");
        StringAssert.Contains(channel, "V L1-E");
        StringAssert.Contains(channel, "V L2-E");
        StringAssert.Contains(channel, "V L3-E");
        StringAssert.Contains(channel, "I L1");
        StringAssert.Contains(channel, "I L2");
        StringAssert.Contains(channel, "I L3");
        StringAssert.Contains(channel, "I RES / 3I0");
        StringAssert.Contains(channel, "V RES / 3V0");

        StringAssert.Contains(mainWindow, "InstallInjectionWorkspace");
        StringAssert.Contains(mainWindow, "new InjectionWorkspace()");
        StringAssert.Contains(mainWindow, "string.Equals(");
        StringAssert.Contains(mainWindow, "\"INJECT\"");
        Assert.IsFalse(mainWindow.Contains("System.Windows", StringComparison.Ordinal));
    }

    [TestMethod]
    public void HardwareButtons_KeepExplicitReadableForegroundAndRecoverCaptureLoss()
    {
        var behavior = Read("src", "Arvrel.Desktop", "Controls", "VirtualRelayFaceplate.axaml.cs");

        StringAssert.Contains(behavior, "HardwareHoverBackground");
        StringAssert.Contains(behavior, "#586871");
        StringAssert.Contains(behavior, "HardwareTextForeground");
        StringAssert.Contains(behavior, "#F7FAFB");
        StringAssert.Contains(behavior, "HardwareAccentForeground");
        StringAssert.Contains(behavior, "PointerCaptureLost");
        StringAssert.Contains(behavior, "_pressedHardwareButtons.Remove(button)");
        StringAssert.Contains(behavior, "textBlock.Foreground = foreground");
        StringAssert.Contains(behavior, "button.Foreground = foreground");
        StringAssert.Contains(behavior, "button.Opacity = 1");
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
