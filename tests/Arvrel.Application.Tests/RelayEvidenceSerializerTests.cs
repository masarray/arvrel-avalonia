using System.Text;
using System.Text.Json;
using Arvrel.Application.Evidence;
using Arvrel.Protection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Application.Tests;

[TestClass]
public sealed class RelayEvidenceSerializerTests
{
    [TestMethod]
    public void SerializeUtf8_IsDeterministicAndCarriesPayloadDigest()
    {
        var evidence = CreateEvidence();

        var first = RelayEvidenceSerializer.SerializeUtf8(evidence);
        var second = RelayEvidenceSerializer.SerializeUtf8(evidence);

        CollectionAssert.AreEqual(first, second);
        Assert.AreEqual((byte)'\n', first[^1]);

        using var document = JsonDocument.Parse(first);
        var root = document.RootElement;
        Assert.AreEqual(RelayEvidenceSerializer.SchemaVersion, root.GetProperty("schemaVersion").GetString());
        Assert.AreEqual(
            RelayEvidenceSerializer.ComputePayloadSha256(evidence),
            root.GetProperty("payloadSha256").GetString());
        Assert.AreEqual("PROCESS BUS · REPLAY", root.GetProperty("evidence").GetProperty("source").GetProperty("mode").GetString());
        Assert.AreEqual("50P-1", root.GetProperty("evidence").GetProperty("operation").GetProperty("tripElement").GetString());
    }

    [TestMethod]
    public async Task WriteAsync_ProducesTheSamePortableUtf8Document()
    {
        var evidence = CreateEvidence();
        await using var stream = new MemoryStream();

        await RelayEvidenceSerializer.WriteAsync(stream, evidence);

        CollectionAssert.AreEqual(RelayEvidenceSerializer.SerializeUtf8(evidence), stream.ToArray());
        StringAssert.Contains(Encoding.UTF8.GetString(stream.ToArray()), "\"schemaVersion\": \"arvrel.relay-evidence/v1\"");
    }

    private static RelayEvidenceBundle CreateEvidence()
    {
        var pickup = new DateTimeOffset(2026, 8, 7, 1, 2, 3, 400, TimeSpan.Zero);
        var trip = pickup.AddMilliseconds(62.5);
        return new RelayEvidenceBundle(
            new DateTimeOffset(2026, 8, 7, 1, 3, 0, TimeSpan.Zero),
            "ARVREL",
            "P5.10",
            "test-platform",
            new RelayEvidenceSource(
                "PROCESS BUS · REPLAY",
                "MU01 · APPID 0x4000",
                "SV 0123456789ab",
                "SCL mapped · CT 1000/1 A",
                true,
                "capture.pcapng",
                "station.scd · 1 SV stream(s)",
                "stream-key",
                "MU01",
                0x4000,
                3210),
            new RelayEvidenceSettings("GROUP A", 3, "settings-fingerprint"),
            new RelayEvidenceMeasurement(5.1, 1.0, 1.0, 4.1, 50, 80, 3210),
            new RelayEvidenceTrust(true, true, true, "HEALTHY", "Coherent two-cycle window."),
            new RelayEvidenceProtection(
                trip,
                "50P-1",
                "Phase instantaneous operated.",
                false,
                true,
                false,
                "Trip",
                "Off",
                "Off",
                "Off"),
            new RelayOperationRecord(
                7,
                pickup,
                trip,
                "50P-1",
                "50P-1",
                5.0,
                5.1,
                "I OP",
                "A"),
            new[]
            {
                "01:02:03  PICKUP      50P-1",
                "01:02:03  TRIP        50P-1"
            });
    }
}
