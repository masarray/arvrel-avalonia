using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class ProtectionSettingsApplicationSourceTests
{
    [TestMethod]
    public void DesktopAppExplicitlyEnablesRestoreWhileParameterlessViewModelStaysHeadlessSafe()
    {
        var app = Read("src", "Arvrel.Desktop", "App.axaml.cs");
        var viewModel = Read("src", "Arvrel.Desktop", "ViewModels", "MainWindowViewModel.cs");
        var store = Read("src", "Arvrel.Application", "Settings", "ProtectionSettingGroupStore.cs");

        StringAssert.Contains(app, "restoreOnLoad: true");
        StringAssert.Contains(app, "new ProtectionSettingGroupStore");
        StringAssert.Contains(viewModel, "public MainWindowViewModel()");
        StringAssert.Contains(viewModel, ": this(null)");
        StringAssert.Contains(store, "restoreOnLoad: !string.IsNullOrWhiteSpace(filePath)");
        StringAssert.Contains(store, "if (!RestoreOnLoad || !File.Exists(FilePath))");
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
