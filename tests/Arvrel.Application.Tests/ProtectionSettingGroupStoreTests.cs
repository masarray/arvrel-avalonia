using Arvrel.Application.Settings;
using Arvrel.Protection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Application.Tests;

[TestClass]
public sealed class ProtectionSettingGroupStoreTests
{
    [TestMethod]
    public void SaveAndLoad_RoundTripsFullProtectionSettingsAndActiveGroup()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var path = Path.Combine(directory, "groups.json");
            var store = new ProtectionSettingGroupStore(path);
            var groupZ = CreateFullSettings("GROUP Z", 9);
            var groupA = new ProtectionSettings
            {
                GroupName = "GROUP A",
                Revision = 2
            };

            store.Save(new[] { groupZ, groupA }, "group z");
            var catalog = store.Load();

            Assert.AreEqual(ProtectionSettingGroupStore.SchemaVersion, catalog.SchemaVersion);
            Assert.AreEqual("GROUP Z", catalog.ActiveGroupName);
            CollectionAssert.AreEqual(
                new[] { "GROUP A", "GROUP Z" },
                catalog.Groups.Select(group => group.GroupName).ToArray());

            var restored = catalog.Groups.Single(group => group.GroupName == "GROUP Z");
            Assert.AreEqual(IecCurveFamily.UserDefined, restored.PhaseTimeCurve);
            Assert.AreEqual(0.21, restored.PhaseTimeUserK, 1e-12);
            Assert.AreEqual(0.31, restored.PhaseTimeUserAlpha, 1e-12);
            Assert.AreEqual(0.41, restored.PhaseTimeUserC, 1e-12);
            Assert.AreEqual(IecCurveFamily.UserDefined, restored.EarthTimeCurve);
            Assert.AreEqual(0.51, restored.EarthTimeUserK, 1e-12);
            Assert.AreEqual(0.61, restored.EarthTimeUserAlpha, 1e-12);
            Assert.AreEqual(0.71, restored.EarthTimeUserC, 1e-12);

            Assert.IsTrue(restored.Feeder.Undervoltage27Enabled);
            Assert.AreEqual(VoltageMeasurementMode.PositiveSequence, restored.Feeder.Undervoltage27Mode);
            Assert.AreEqual(VoltageSelectionLogic.ThreeOfThree, restored.Feeder.Undervoltage27Logic);
            Assert.IsTrue(restored.Feeder.Overvoltage59Enabled);
            Assert.AreEqual(VoltageMeasurementMode.PhaseToPhase, restored.Feeder.Overvoltage59Mode);
            Assert.AreEqual(VoltageSelectionLogic.TwoOfThree, restored.Feeder.Overvoltage59Logic);
            Assert.IsTrue(restored.Feeder.ResidualOvervoltage59NEnabled);
            Assert.IsTrue(restored.Feeder.DirectionalPhase67Enabled);
            Assert.AreEqual(DirectionalSense.Reverse, restored.Feeder.DirectionalPhase67Sense);
            Assert.IsTrue(restored.Feeder.DirectionalEarth67NEnabled);
            Assert.AreEqual(DirectionalSense.Forward, restored.Feeder.DirectionalEarth67NSense);
            Assert.AreEqual(groupZ.Fingerprint(), restored.Fingerprint());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void Save_RejectsDuplicateGroupNamesIgnoringCase()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var store = new ProtectionSettingGroupStore(Path.Combine(directory, "groups.json"));
            var first = new ProtectionSettings { GroupName = "GROUP A" };
            var second = new ProtectionSettings { GroupName = "group a", Revision = 2 };

            Assert.ThrowsException<InvalidDataException>(() =>
                store.Save(new[] { first, second }, "GROUP A"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void Load_RejectsMalformedJsonAndParameterlessStoreSkipsAutomaticRestore()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var path = Path.Combine(directory, "groups.json");
            File.WriteAllText(path, "{ not-json }");

            var restoringStore = new ProtectionSettingGroupStore(path);
            Assert.ThrowsException<InvalidDataException>(() => restoringStore.Load());

            var nonRestoringStore = new ProtectionSettingGroupStore(path, restoreOnLoad: false);
            Assert.AreEqual(0, nonRestoringStore.Load().Groups.Count);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static ProtectionSettings CreateFullSettings(string name, int revision)
        => new()
        {
            GroupName = name,
            Revision = revision,
            PhaseTimeCurve = IecCurveFamily.UserDefined,
            PhaseTimeUserK = 0.21,
            PhaseTimeUserAlpha = 0.31,
            PhaseTimeUserC = 0.41,
            EarthTimeCurve = IecCurveFamily.UserDefined,
            EarthTimeUserK = 0.51,
            EarthTimeUserAlpha = 0.61,
            EarthTimeUserC = 0.71,
            Feeder = new FeederProtectionSettings
            {
                Undervoltage27Enabled = true,
                Undervoltage27PickupV = 88,
                Undervoltage27Delay = TimeSpan.FromMilliseconds(720),
                Undervoltage27ResetRatio = 1.08,
                Undervoltage27Mode = VoltageMeasurementMode.PositiveSequence,
                Undervoltage27Logic = VoltageSelectionLogic.ThreeOfThree,
                Overvoltage59Enabled = true,
                Overvoltage59PickupV = 118,
                Overvoltage59Delay = TimeSpan.FromMilliseconds(640),
                Overvoltage59DropoutRatio = 0.93,
                Overvoltage59Mode = VoltageMeasurementMode.PhaseToPhase,
                Overvoltage59Logic = VoltageSelectionLogic.TwoOfThree,
                ResidualOvervoltage59NEnabled = true,
                ResidualOvervoltage59NPickupV = 12.5,
                ResidualOvervoltage59NDelay = TimeSpan.FromMilliseconds(480),
                ResidualOvervoltage59NDropoutRatio = 0.91,
                DirectionalPhase67Enabled = true,
                DirectionalPhase67PickupA = 1.8,
                DirectionalPhase67Delay = TimeSpan.FromMilliseconds(260),
                DirectionalPhase67DropoutRatio = 0.92,
                DirectionalPhase67CharacteristicAngleDeg = 35,
                DirectionalPhase67MinimumPolarizingVoltageV = 7,
                DirectionalPhase67Sense = DirectionalSense.Reverse,
                DirectionalEarth67NEnabled = true,
                DirectionalEarth67NPickupA = 0.45,
                DirectionalEarth67NDelay = TimeSpan.FromMilliseconds(340),
                DirectionalEarth67NDropoutRatio = 0.90,
                DirectionalEarth67NCharacteristicAngleDeg = -55,
                DirectionalEarth67NMinimumPolarizingVoltageV = 3,
                DirectionalEarth67NSense = DirectionalSense.Forward
            }
        };

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "arvrel-settings-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
