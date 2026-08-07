using Arvrel.Application.Settings;
using Arvrel.Desktop.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class ProtectionSettingsPersistenceFailureTests
{
    [TestMethod]
    public async Task FailedSaveDoesNotApplyOrRetainTheUnsavedSettingGroup()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "arvrel-settings-failure-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            // The store target is deliberately an existing directory. The temporary
            // document can be written, but atomic replacement of the directory fails.
            var store = new ProtectionSettingGroupStore(directory);
            await using var viewModel = new MainWindowViewModel(store);
            var originalGroup = viewModel.SettingsGroupText;
            var originalFingerprint = viewModel.SettingsFingerprintText;

            viewModel.SettingsEditor.GroupName = "UNSAVED GROUP";
            viewModel.SettingsEditor.RevisionText = "8";
            viewModel.SaveSettingGroupCommand.Execute(null);

            Assert.AreEqual(originalGroup, viewModel.SettingsGroupText);
            Assert.AreEqual(originalFingerprint, viewModel.SettingsFingerprintText);
            Assert.AreEqual(0, viewModel.PersistedSettingGroupNames.Count);
            Assert.IsNull(viewModel.SelectedPersistedSettingGroup);
            StringAssert.Contains(viewModel.SettingGroupPersistenceStatus, "FAILED");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
