using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arvrel.Application.Evidence;

public static class RelayEvidenceSerializer
{
    public const string SchemaVersion = "arvrel.relay-evidence/v1";

    private static readonly JsonSerializerOptions CanonicalOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions PresentationOptions = new(CanonicalOptions)
    {
        WriteIndented = true
    };

    public static RelayEvidenceEnvelope CreateEnvelope(RelayEvidenceBundle evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        return new RelayEvidenceEnvelope(
            SchemaVersion,
            ComputePayloadSha256(evidence),
            evidence);
    }

    public static string ComputePayloadSha256(RelayEvidenceBundle evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        var payload = JsonSerializer.SerializeToUtf8Bytes(evidence, CanonicalOptions);
        return Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
    }

    public static byte[] SerializeUtf8(RelayEvidenceBundle evidence)
    {
        var envelope = CreateEnvelope(evidence);
        var json = JsonSerializer.SerializeToUtf8Bytes(envelope, PresentationOptions);
        var result = new byte[json.Length + 1];
        Buffer.BlockCopy(json, 0, result, 0, json.Length);
        result[^1] = (byte)'\n';
        return result;
    }

    public static async Task WriteAsync(
        Stream destination,
        RelayEvidenceBundle evidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Evidence destination stream is not writable.", nameof(destination));

        var bytes = SerializeUtf8(evidence);
        await destination.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
