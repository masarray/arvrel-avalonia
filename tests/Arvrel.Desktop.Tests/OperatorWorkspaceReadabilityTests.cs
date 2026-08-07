using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class OperatorWorkspaceReadabilityTests
{
    [TestMethod]
    public void Toolbar_UsesResponsiveFieldsVectorIconsAndOperatorTooltips()
    {
        var xaml = Read("src", "Arvrel.Desktop", "MainWindow.axaml");
        _ = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

        StringAssert.Contains(xaml, "ColumnDefinitions=\"Auto,230,12,Auto,1.08*,12,Auto,1*,12,Auto,128,16,Auto\"");
        StringAssert.Contains(xaml, "MinWidth=\"190\"");
        StringAssert.Contains(xaml, "ToolTip.Tip=\"Refresh capture adapters\"");
        StringAssert.Contains(xaml, "ToolTip.Tip=\"Toggle SMV trust degradation\"");
        StringAssert.Contains(xaml, "ToolTip.Tip=\"Reset the displayed relay latch and timers\"");
        StringAssert.Contains(xaml, "Data=\"M17.65,6.35");
        Assert.IsFalse(xaml.Contains("Content=\"↻\"", StringComparison.Ordinal));
        Assert.IsFalse(xaml.Contains("Content=\"⚠\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public void DualEvidenceView_GivesPhasorMoreWidthAndWaveformMoreHeadroom()
    {
        var xaml = Read("src", "Arvrel.Desktop", "MainWindow.axaml");
        var scope = Read("src", "Arvrel.Desktop", "Controls", "WaveformScope.cs");
        var axis = Read("src", "Arvrel.Desktop", "Controls", "WaveformAxisLayout.cs");

        StringAssert.Contains(xaml, "ColumnDefinitions=\"2.02*,8,1*\"");
        StringAssert.Contains(scope, "DrawLegend(context, viewport)");
        StringAssert.Contains(scope, "viewport.Height * 0.39 / maximum");
        StringAssert.Contains(axis, "PlotTopInset = 26");
    }

    [TestMethod]
    public void Phasor_UsesDeterministicCollisionAvoidanceForVectorLabels()
    {
        var source = Read("src", "Arvrel.Desktop", "Controls", "PhasorScope.cs");

        StringAssert.Contains(source, "occupiedLabels");
        StringAssert.Contains(source, "PlaceLabel(");
        StringAssert.Contains(source, "IntersectionArea(");
        StringAssert.Contains(source, "OrderByDescending(vector => vector.Magnitude)");
    }

    [TestMethod]
    public void ProtectionOperation_SeparatesPrimarySecondaryAndEventEvidence()
    {
        var xaml = Read("src", "Arvrel.Desktop", "MainWindow.axaml");
        var projection = Read("src", "Arvrel.Desktop", "ViewModels", "MainWindowViewModel.P6Ux.cs");

        StringAssert.Contains(xaml, "Text=\"PRIMARY OPERATION\"");
        StringAssert.Contains(xaml, "DataContext=\"{Binding PrimaryProtectionElement}\"");
        StringAssert.Contains(xaml, "ItemsSource=\"{Binding SecondaryProtectionElements}\"");
        StringAssert.Contains(xaml, "Text=\"{Binding OperationBadgeText}\"");
        StringAssert.Contains(xaml, "LATEST FIRST");

        StringAssert.Contains(projection, "OperationBadgeText");
        StringAssert.Contains(projection, "PrimaryProtectionElement");
        StringAssert.Contains(projection, "SecondaryProtectionElements");
        StringAssert.Contains(projection, "OrderByDescending(element => element.Progress)");
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
