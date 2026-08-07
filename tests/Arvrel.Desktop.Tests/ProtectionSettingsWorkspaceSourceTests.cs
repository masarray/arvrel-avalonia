using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class ProtectionSettingsWorkspaceSourceTests
{
    [TestMethod]
    public void WorkspaceExposesAllProtectionFunctionsPersistenceAndUserCurves()
    {
        var xaml = Read("src", "Arvrel.Desktop", "Controls", "ProtectionSettingsWorkspace.axaml");
        _ = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

        foreach (var token in new[]
        {
            "Text=\"27\"",
            "Text=\"59\"",
            "Text=\"59N\"",
            "Text=\"67P\"",
            "Text=\"67N\"",
            "SettingsEditor.Phase51.UserKText",
            "SettingsEditor.Phase51.UserAlphaText",
            "SettingsEditor.Phase51.UserCText",
            "SettingsEditor.Earth51.UserKText",
            "SettingsEditor.Undervoltage27.Mode",
            "SettingsEditor.Overvoltage59.Logic",
            "SettingsEditor.DirectionalPhase67.Sense",
            "SettingsEditor.DirectionalEarth67N.CharacteristicAngleText",
            "SaveSettingGroupCommand",
            "LoadSettingGroupCommand",
            "DeleteSettingGroupCommand",
            "VALIDATE + APPLY ALL PROTECTION SETTINGS"
        })
        {
            StringAssert.Contains(xaml, token);
        }

        Assert.IsFalse(xaml.Contains("System.Windows", StringComparison.Ordinal));
        Assert.IsFalse(xaml.Contains("Microsoft.Win32", StringComparison.Ordinal));
    }

    [TestMethod]
    public void MainWindowMountsSettingsAsOneViewOfTheSharedViewModel()
    {
        var source = Read("src", "Arvrel.Desktop", "MainWindow.axaml.cs");

        StringAssert.Contains(source, "InstallProtectionSettingsWorkspace()");
        StringAssert.Contains(source, "new ProtectionSettingsWorkspace");
        StringAssert.Contains(source, "Header = \"SETTINGS\"");
        StringAssert.Contains(source, "DataContext = DataContext");
        StringAssert.Contains(source, "_protectionSettingsWorkspace.DataContext = DataContext");
    }

    [TestMethod]
    public void AppliedSettingsReachInternalAndProcessBusAuthoritiesAndAllNineCards()
    {
        var settings = Read("src", "Arvrel.Desktop", "ViewModels", "MainWindowViewModel.SettingsPersistence.cs");
        var display = Read("src", "Arvrel.Desktop", "ViewModels", "MainWindowViewModel.Display.cs");
        var processBus = Read("src", "Arvrel.ProcessBus", "SmvProcessBusController.cs");

        StringAssert.Contains(settings, "_workspace.InternalLab.ApplySettingsPreservingSource(settings)");
        StringAssert.Contains(settings, "_processBus.UpdateProtectionSettings(settings)");
        StringAssert.Contains(settings, "InjectionFingerprint");
        StringAssert.Contains(processBus, "settings.Validate()");
        StringAssert.Contains(processBus, "ClearStreams()");
        StringAssert.Contains(processBus, "SV runtimes will rebuild from fresh frames");

        foreach (var feederElement in new[]
        {
            "snapshot.Feeder.Undervoltage27",
            "snapshot.Feeder.Overvoltage59",
            "snapshot.Feeder.ResidualOvervoltage59N",
            "snapshot.Feeder.DirectionalPhase67",
            "snapshot.Feeder.DirectionalEarth67N"
        })
        {
            StringAssert.Contains(display, feederElement);
        }

        Assert.IsFalse(settings.Contains("new ProtectionEngine", StringComparison.Ordinal));
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
