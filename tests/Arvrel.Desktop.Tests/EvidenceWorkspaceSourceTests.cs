using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class EvidenceWorkspaceSourceTests
{
    [TestMethod]
    public void EvidenceWorkspaceUsesAvaloniaSavePickerAndPortableSerializer()
    {
        var xaml = Read("src", "Arvrel.Desktop", "Controls", "EvidenceWorkspace.axaml");
        var behavior = Read("src", "Arvrel.Desktop", "Controls", "EvidenceWorkspace.axaml.cs");
        var projection = Read("src", "Arvrel.Desktop", "ViewModels", "MainWindowViewModel.Evidence.cs");
        var serializer = Read("src", "Arvrel.Application", "Evidence", "RelayEvidenceSerializer.cs");

        _ = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

        StringAssert.Contains(xaml, "PORTABLE RELAY EVIDENCE");
        StringAssert.Contains(xaml, "EXPORT ACTIVE SOURCE EVIDENCE");
        StringAssert.Contains(xaml, "EvidenceExportStatus");
        StringAssert.Contains(behavior, "SaveFilePickerAsync");
        StringAssert.Contains(behavior, "OpenWriteAsync");
        StringAssert.Contains(behavior, "*.json");
        StringAssert.Contains(projection, "CreateEvidenceBundle");
        StringAssert.Contains(projection, "RelayEvidenceSerializer.WriteAsync");
        StringAssert.Contains(projection, "DisplayMeasurement");
        StringAssert.Contains(projection, "DisplayProtection");
        StringAssert.Contains(projection, "CurrentOperationRecord");
        StringAssert.Contains(serializer, "arvrel.relay-evidence/v1");
        StringAssert.Contains(serializer, "SHA256.HashData");

        Assert.IsFalse(behavior.Contains("Microsoft.Win32", StringComparison.Ordinal));
        Assert.IsFalse(behavior.Contains("System.Windows.Controls", StringComparison.Ordinal));
        Assert.IsFalse(projection.Contains("Avalonia.Controls", StringComparison.Ordinal));
    }

    [TestMethod]
    public void MainWindowMountsEvidenceInTheEngineeringToolsPaneBesideTheNativeOverview()
    {
        var xaml = Read("src", "Arvrel.Desktop", "MainWindow.axaml");
        var source = Read("src", "Arvrel.Desktop", "MainWindow.axaml.cs");

        _ = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

        StringAssert.Contains(xaml, "Header=\"EVIDENCE\"");
        StringAssert.Contains(xaml, "<controls:EvidenceWorkspace");
        StringAssert.Contains(xaml, "<controls:VirtualRelayFaceplate");
        StringAssert.Contains(xaml, "Engineering workspaces");
        Assert.IsFalse(source.Contains("InstallEvidenceWorkspace", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("ResolveRelayWorkspaceTabs", StringComparison.Ordinal));
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
