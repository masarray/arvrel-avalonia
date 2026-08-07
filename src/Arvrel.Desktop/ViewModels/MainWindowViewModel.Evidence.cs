using Arvrel.Application.Evidence;

namespace Arvrel.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private const string EvidenceProductVersion = "P5.10 · EVIDENCE EXPORT";
    private string _evidenceExportStatus = "READY · evidence follows the active display source";

    public string EvidenceExportStatus => _evidenceExportStatus;
    public string EvidenceOperationSummary => FormatOperationDetail(CurrentOperationRecord);
    public string EvidenceSourceFingerprintText => IsProcessBusDisplayActive
        ? DisplayFingerprintText
        : InjectionFingerprintText;

    public string SuggestedEvidenceFileName
    {
        get
        {
            var source = IsProcessBusDisplayActive ? "process-bus" : "internal-lab";
            return $"arvrel-evidence-{source}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.json";
        }
    }

    public RelayEvidenceBundle CreateEvidenceBundle(DateTimeOffset? exportedAtUtc = null)
    {
        var measurement = DisplayMeasurement;
        var protection = DisplayProtection;
        var waveform = DisplayWaveform;
        var trust = measurement.SmvTrust;
        var sampleCounter = IsProcessBusDisplayActive
            ? _processBusSnapshot.SampleCounter
            : _workspace.InternalLab.Scenario.SampleCounter;

        return new RelayEvidenceBundle(
            exportedAtUtc ?? DateTimeOffset.UtcNow,
            ProductTitle,
            EvidenceProductVersion,
            PlatformText,
            CreateEvidenceSource(sampleCounter),
            new RelayEvidenceSettings(
                _settings.GroupName,
                _settings.Revision,
                _settings.Fingerprint()),
            new RelayEvidenceMeasurement(
                measurement.PhaseA,
                measurement.PhaseB,
                measurement.PhaseC,
                measurement.Residual,
                waveform.FrequencyHz,
                waveform.SamplesPerCycle,
                sampleCounter),
            new RelayEvidenceTrust(
                trust.AllowsMeasurement,
                trust.AllowsPickup,
                trust.AllowsTrip,
                trust.Code,
                trust.Detail),
            new RelayEvidenceProtection(
                protection.Timestamp,
                protection.ActiveElement,
                protection.DecisionReason,
                PickupActive,
                TripLatched,
                protection.Blocked,
                PhaseAAnnunciation.ToString(),
                PhaseBAnnunciation.ToString(),
                PhaseCAnnunciation.ToString(),
                EarthAnnunciation.ToString()),
            CurrentOperationRecord,
            Events.ToArray());
    }

    public async Task WriteEvidenceAsync(
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        var evidence = CreateEvidenceBundle();
        await RelayEvidenceSerializer.WriteAsync(destination, evidence, cancellationToken);
        _evidenceExportStatus = $"EXPORTED · {ActiveDisplaySourceText} · {evidence.ExportedAtUtc:yyyy-MM-dd HH:mm:ss} UTC";
        AddEvent("EVIDENCE", $"JSON exported · {ActiveDisplaySourceText} · {RelayEvidenceSerializer.ComputePayloadSha256(evidence)[..12]}");
        OnPropertyChanged(nameof(EvidenceExportStatus));
        OnPropertyChanged(nameof(EvidenceOperationSummary));
        OnPropertyChanged(nameof(SuggestedEvidenceFileName));
    }

    public void ReportEvidenceExportFailure(string message)
    {
        _evidenceExportStatus = $"FAILED · {message}";
        AddEvent("EVIDENCE", _evidenceExportStatus);
        OnPropertyChanged(nameof(EvidenceExportStatus));
    }

    private RelayEvidenceSource CreateEvidenceSource(long sampleCounter)
    {
        if (!IsProcessBusDisplayActive)
        {
            return new RelayEvidenceSource(
                ActiveDisplaySourceText,
                ProfileNameText,
                _workspace.InternalLab.Scenario.InjectionFingerprint,
                ProvenanceText,
                false,
                null,
                null,
                null,
                null,
                null,
                sampleCounter);
        }

        var stream = _processBusSnapshot.Stream;
        return new RelayEvidenceSource(
            ActiveDisplaySourceText,
            stream.DisplayName,
            string.IsNullOrWhiteSpace(stream.Key)
                ? string.Empty
                : StableShortFingerprint(stream.Key),
            DisplayProvenanceText,
            _processBus.IsReplayMode,
            _processBus.IsReplayMode && !string.IsNullOrWhiteSpace(ReplayPath)
                ? Path.GetFileName(ReplayPath)
                : null,
            ProcessBusSclSummary,
            stream.Key,
            stream.SvId,
            stream.AppId,
            sampleCounter);
    }
}
