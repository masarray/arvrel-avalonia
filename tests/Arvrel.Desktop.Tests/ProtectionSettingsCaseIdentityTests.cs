using Arvrel.Application.Settings;
using Arvrel.Desktop.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class ProtectionSettingsCaseIdentityTests
{
    [TestMethod]
    public async Task CaseOnlyRenameKeepsOneCanonicalSettingGroup()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "arvrel-settings-case-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            var path = Path.Combine(directory, "groups.json");
            var store = new ProtectionSettingGroupStore(path);
            await using var viewModel = new MainWindowViewModel(store);

            viewModel.SettingsEditor.GroupName = "GROUP A";
            viewModel.SaveSettingGroupCommand.Execute(null);
            viewModel.SettingsEditor.GroupName = "Group A";
            viewModel.SettingsEditor.RevisionText = "2";
            viewModel.SaveSettingGroupCommand.Execute(null);

            Assert.AreEqual(1, viewModel.PersistedSettingGroupNames.Count);
            Assert.AreEqual("Group A", viewModel.PersistedSettingGroupNames[0]);
            Assert.AreEqual("Group A", viewModel.SelectedPersistedSettingGroup);

            var catalog = store.Load();
            Assert.AreEqual(1, catalog.Groups.Count);
            Assert.AreEqual("Group A", catalog.ActiveGroupName);
            Assert.AreEqual("Group A", catalog.Groups[0].GroupName);
            Assert.AreEqual(2, catalog.Groups[0].Revision);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
