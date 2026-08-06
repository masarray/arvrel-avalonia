using System.Globalization;
using System.Text.Json;
using Arvrel.Application.Evidence;
using Arvrel.Desktop.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class EvidenceExportTests
{
    [TestMethod]
    public async Task InternalTripEvidenceIncludesOperationFingerprintsAndEventTimelineAfterReset()
    {
        await using var viewModel = new MainWindowViewModel();

        viewModel.InjectAgFault();
        for (var index = 0; index < 8; index++)
            viewModel.Tick();

        Assert.IsTrue(viewModel.TripLatched);
        var completed = viewModel.CurrentOperationRecord;
        Assert.IsNotNull(completed);
        Assert.IsTrue(completed.TripTimestamp.HasValue);

        viewModel.ResetRelay();
        Assert.IsFalse(viewModel.TripLatched);

        var evidence = viewModel.CreateEvidenceBundle(
            new DateTimeOffset(2026, 8, 7, 2, 0, 0, TimeSpan.Zero));

        Assert.AreEqual("INTERNAL VIRTUAL TEST SET", evidence.Source.Mode);
        Assert.AreEqual(viewModel.ProfileNameText, evidence.Source.Identity);
        Assert.AreEqual(viewModel.CurrentOperationRecord, evidence.Operation);
        Assert.IsNotNull(evidence.Operation?.TripTimestamp);
        Assert.AreEqual(viewModel.SettingsGroupText.Split('·')[0].Trim(), evidence.Settings.GroupName);
        Assert.IsTrue(evidence.Events.Count > 0);
        Assert.AreEqual(
            viewModel.PhaseAText,
            string.Create(CultureInfo.InvariantCulture, $"{evidence.Measurement.PhaseA:0.000} A"));
    }

    [TestMethod]
    public async Task WriteEvidenceAsyncProducesVersionedDigestEnvelopeAndUpdatesOperatorStatus()
    {
        await using var viewModel = new MainWindowViewModel();
        await using var stream = new MemoryStream();

        await viewModel.WriteEvidenceAsync(stream);

        using var document = JsonDocument.Parse(stream.ToArray());
        Assert.AreEqual(
            RelayEvidenceSerializer.SchemaVersion,
            document.RootElement.GetProperty("schemaVersion").GetString());
        Assert.AreEqual(
            64,
            document.RootElement.GetProperty("payloadSha256").GetString()?.Length ?? 0);
        StringAssert.Contains(viewModel.EvidenceExportStatus, "EXPORTED");
        StringAssert.Contains(viewModel.Events[0], "EVIDENCE");
    }
}
