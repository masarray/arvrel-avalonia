using Arvrel.Application.Settings;
using Arvrel.Desktop.ViewModels;
using Arvrel.Protection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class ProtectionSettingsParityTests
{
    [TestMethod]
    public void EditorBuildsAllFeederFunctionsAndUserDefinedCurveParameters()
    {
        var editor = new ProtectionSettingsEditorViewModel();
        editor.Apply(new ProtectionSettings());

        editor.GroupName = "GROUP PARITY";
        editor.RevisionText = "11";
        editor.Phase51.Curve = IecCurveFamily.UserDefined;
        editor.Phase51.UserKText = "0.23";
        editor.Phase51.UserAlphaText = "0.37";
        editor.Phase51.UserCText = "0.08";
        editor.Earth51.Curve = IecCurveFamily.UserDefined;
        editor.Earth51.UserKText = "0.45";
        editor.Earth51.UserAlphaText = "0.67";
        editor.Earth51.UserCText = "0.12";

        editor.Undervoltage27.Enabled = true;
        editor.Undervoltage27.PickupText = "87";
        editor.Undervoltage27.DelayMsText = "710";
        editor.Undervoltage27.RatioText = "1.07";
        editor.Undervoltage27.Mode = VoltageMeasurementMode.PositiveSequence;
        editor.Undervoltage27.Logic = VoltageSelectionLogic.ThreeOfThree;

        editor.Overvoltage59.Enabled = true;
        editor.Overvoltage59.PickupText = "119";
        editor.Overvoltage59.DelayMsText = "620";
        editor.Overvoltage59.RatioText = "0.94";
        editor.Overvoltage59.Mode = VoltageMeasurementMode.PhaseToPhase;
        editor.Overvoltage59.Logic = VoltageSelectionLogic.TwoOfThree;

        editor.ResidualOvervoltage59N.Enabled = true;
        editor.ResidualOvervoltage59N.PickupText = "11.5";
        editor.ResidualOvervoltage59N.DelayMsText = "430";
        editor.ResidualOvervoltage59N.DropoutText = "0.91";

        editor.DirectionalPhase67.Enabled = true;
        editor.DirectionalPhase67.PickupText = "1.75";
        editor.DirectionalPhase67.DelayMsText = "280";
        editor.DirectionalPhase67.DropoutText = "0.92";
        editor.DirectionalPhase67.CharacteristicAngleText = "38";
        editor.DirectionalPhase67.MinimumPolarizingVoltageText = "6.5";
        editor.DirectionalPhase67.Sense = DirectionalSense.Reverse;

        editor.DirectionalEarth67N.Enabled = true;
        editor.DirectionalEarth67N.PickupText = "0.42";
        editor.DirectionalEarth67N.DelayMsText = "330";
        editor.DirectionalEarth67N.DropoutText = "0.90";
        editor.DirectionalEarth67N.CharacteristicAngleText = "-52";
        editor.DirectionalEarth67N.MinimumPolarizingVoltageText = "2.5";
        editor.DirectionalEarth67N.Sense = DirectionalSense.Forward;

        var success = editor.TryBuild(new ProtectionSettings(), out var settings, out var error);

        Assert.IsTrue(success, error);
        Assert.AreEqual("GROUP PARITY", settings.GroupName);
        Assert.AreEqual(11, settings.Revision);
        Assert.AreEqual(IecCurveFamily.UserDefined, settings.PhaseTimeCurve);
        Assert.AreEqual(0.23, settings.PhaseTimeUserK, 1e-12);
        Assert.AreEqual(0.37, settings.PhaseTimeUserAlpha, 1e-12);
        Assert.AreEqual(0.08, settings.PhaseTimeUserC, 1e-12);
        Assert.AreEqual(0.45, settings.EarthTimeUserK, 1e-12);
        Assert.AreEqual(0.67, settings.EarthTimeUserAlpha, 1e-12);
        Assert.AreEqual(0.12, settings.EarthTimeUserC, 1e-12);

        Assert.IsTrue(settings.Feeder.Undervoltage27Enabled);
        Assert.AreEqual(VoltageMeasurementMode.PositiveSequence, settings.Feeder.Undervoltage27Mode);
        Assert.AreEqual(VoltageSelectionLogic.ThreeOfThree, settings.Feeder.Undervoltage27Logic);
        Assert.IsTrue(settings.Feeder.Overvoltage59Enabled);
        Assert.AreEqual(VoltageMeasurementMode.PhaseToPhase, settings.Feeder.Overvoltage59Mode);
        Assert.AreEqual(VoltageSelectionLogic.TwoOfThree, settings.Feeder.Overvoltage59Logic);
        Assert.IsTrue(settings.Feeder.ResidualOvervoltage59NEnabled);
        Assert.IsTrue(settings.Feeder.DirectionalPhase67Enabled);
        Assert.AreEqual(DirectionalSense.Reverse, settings.Feeder.DirectionalPhase67Sense);
        Assert.IsTrue(settings.Feeder.DirectionalEarth67NEnabled);
        Assert.AreEqual(DirectionalSense.Forward, settings.Feeder.DirectionalEarth67NSense);
        StringAssert.Contains(editor.Phase51.CurveFormula, "0.23");
        StringAssert.Contains(editor.Phase51.CurveFormula, "0.37");
    }

    [TestMethod]
    public async Task SavedGroupRestoresAllSettingsAndNineProtectionCards()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var path = Path.Combine(directory, "groups.json");
            await using (var viewModel = new MainWindowViewModel(new ProtectionSettingGroupStore(path)))
            {
                viewModel.SettingsEditor.GroupName = "GROUP SAVED";
                viewModel.SettingsEditor.RevisionText = "4";
                viewModel.SettingsEditor.Phase51.Curve = IecCurveFamily.UserDefined;
                viewModel.SettingsEditor.Phase51.UserKText = "0.24";
                viewModel.SettingsEditor.Phase51.UserAlphaText = "0.28";
                viewModel.SettingsEditor.Phase51.UserCText = "0.09";
                viewModel.SettingsEditor.Undervoltage27.Enabled = true;
                viewModel.SettingsEditor.Undervoltage27.PickupText = "86";
                viewModel.SettingsEditor.DirectionalPhase67.Enabled = true;
                viewModel.SettingsEditor.DirectionalPhase67.Sense = DirectionalSense.Reverse;

                viewModel.SaveSettingGroupCommand.Execute(null);

                Assert.AreEqual(9, viewModel.ProtectionElements.Count);
                Assert.IsTrue(File.Exists(path));
                Assert.AreEqual("GROUP SAVED · REV 4", viewModel.SettingsGroupText);
                Assert.AreEqual("GROUP SAVED", viewModel.SelectedPersistedSettingGroup);
                StringAssert.Contains(viewModel.SettingGroupPersistenceStatus, "SAVED");
            }

            await using var restored = new MainWindowViewModel(new ProtectionSettingGroupStore(path));

            Assert.AreEqual("GROUP SAVED · REV 4", restored.SettingsGroupText);
            Assert.AreEqual("GROUP SAVED", restored.SelectedPersistedSettingGroup);
            Assert.AreEqual(IecCurveFamily.UserDefined, restored.SettingsEditor.Phase51.Curve);
            Assert.AreEqual("0.24", restored.SettingsEditor.Phase51.UserKText);
            Assert.AreEqual("0.28", restored.SettingsEditor.Phase51.UserAlphaText);
            Assert.AreEqual("0.09", restored.SettingsEditor.Phase51.UserCText);
            Assert.IsTrue(restored.SettingsEditor.Undervoltage27.Enabled);
            Assert.AreEqual("86", restored.SettingsEditor.Undervoltage27.PickupText);
            Assert.IsTrue(restored.SettingsEditor.DirectionalPhase67.Enabled);
            Assert.AreEqual(DirectionalSense.Reverse, restored.SettingsEditor.DirectionalPhase67.Sense);
            StringAssert.Contains(restored.SettingGroupPersistenceStatus, "RESTORED");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void EditorRejectsInvalidDirectionalAngleAndUndervoltageResetRatio()
    {
        var editor = new ProtectionSettingsEditorViewModel();
        editor.Apply(new ProtectionSettings());
        editor.DirectionalPhase67.CharacteristicAngleText = "181";

        Assert.IsFalse(editor.TryBuild(new ProtectionSettings(), out _, out var angleError));
        StringAssert.Contains(angleError, "-180");

        editor.DirectionalPhase67.CharacteristicAngleText = "45";
        editor.Undervoltage27.RatioText = "0.95";

        Assert.IsFalse(editor.TryBuild(new ProtectionSettings(), out _, out var ratioError));
        StringAssert.Contains(ratioError, "between one and two");
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "arvrel-desktop-settings-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
