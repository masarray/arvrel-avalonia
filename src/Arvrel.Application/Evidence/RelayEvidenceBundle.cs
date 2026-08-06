using Arvrel.Protection;

namespace Arvrel.Application.Evidence;

public sealed record RelayEvidenceBundle(
    DateTimeOffset ExportedAtUtc,
    string Product,
    string ProductVersion,
    string Platform,
    RelayEvidenceSource Source,
    RelayEvidenceSettings Settings,
    RelayEvidenceMeasurement Measurement,
    RelayEvidenceTrust Trust,
    RelayEvidenceProtection Protection,
    RelayOperationRecord? Operation,
    IReadOnlyList<string> Events);

public sealed record RelayEvidenceSource(
    string Mode,
    string Identity,
    string Fingerprint,
    string Provenance,
    bool IsReplay,
    string? CaptureFile,
    string? SclSummary,
    string? StreamKey,
    string? StreamId,
    int? AppId,
    int? SampleCounter);

public sealed record RelayEvidenceSettings(
    string GroupName,
    int Revision,
    string Fingerprint);

public sealed record RelayEvidenceMeasurement(
    double PhaseA,
    double PhaseB,
    double PhaseC,
    double Residual,
    double FrequencyHz,
    int SamplesPerCycle,
    int SampleCounter);

public sealed record RelayEvidenceTrust(
    bool AllowsMeasurement,
    bool AllowsPickup,
    bool AllowsTrip,
    string Code,
    string Detail);

public sealed record RelayEvidenceProtection(
    DateTimeOffset Timestamp,
    string ActiveElement,
    string DecisionReason,
    bool PickupActive,
    bool TripLatched,
    bool Blocked,
    string PhaseAAnnunciation,
    string PhaseBAnnunciation,
    string PhaseCAnnunciation,
    string EarthAnnunciation);

public sealed record RelayEvidenceEnvelope(
    string SchemaVersion,
    string PayloadSha256,
    RelayEvidenceBundle Evidence);
