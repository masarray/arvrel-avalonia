using System.Text.Json;
using System.Text.Json.Serialization;
using Arvrel.Protection;

namespace Arvrel.Application.Settings;

public sealed record ProtectionSettingGroupCatalog(
    string SchemaVersion,
    string? ActiveGroupName,
    IReadOnlyList<ProtectionSettings> Groups)
{
    public static ProtectionSettingGroupCatalog Empty { get; } = new(
        ProtectionSettingGroupStore.SchemaVersion,
        null,
        Array.Empty<ProtectionSettings>());
}

public sealed class ProtectionSettingGroupStore
{
    public const string SchemaVersion = "arvrel.protection-setting-groups/v1";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter() }
    };

    public ProtectionSettingGroupStore(string? filePath = null)
    {
        FilePath = string.IsNullOrWhiteSpace(filePath)
            ? DefaultFilePath
            : Path.GetFullPath(filePath);
    }

    public string FilePath { get; }

    public static string DefaultFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ARVREL-Avalonia",
        "protection-setting-groups.json");

    public ProtectionSettingGroupCatalog Load()
    {
        if (!File.Exists(FilePath))
            return ProtectionSettingGroupCatalog.Empty;

        try
        {
            var json = File.ReadAllText(FilePath);
            var document = JsonSerializer.Deserialize<ProtectionSettingGroupDocument>(json, SerializerOptions)
                ?? throw new InvalidDataException("Protection setting-group file is empty.");

            if (!string.Equals(document.SchemaVersion, SchemaVersion, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"Unsupported protection setting-group schema '{document.SchemaVersion}'.");
            }

            var normalized = NormalizeGroups(document.Groups ?? Array.Empty<ProtectionSettings>());
            var active = NormalizeActiveGroup(document.ActiveGroupName, normalized);
            return new ProtectionSettingGroupCatalog(SchemaVersion, active, normalized);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Protection setting-group JSON is invalid.", ex);
        }
    }

    public void Save(
        IEnumerable<ProtectionSettings> groups,
        string? activeGroupName)
    {
        ArgumentNullException.ThrowIfNull(groups);

        var normalized = NormalizeGroups(groups);
        var active = NormalizeActiveGroup(activeGroupName, normalized);
        var document = new ProtectionSettingGroupDocument(
            SchemaVersion,
            active,
            normalized.ToArray());

        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var temporaryPath = $"{FilePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(document, SerializerOptions) + Environment.NewLine);
            File.Move(temporaryPath, FilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static IReadOnlyList<ProtectionSettings> NormalizeGroups(
        IEnumerable<ProtectionSettings> groups)
    {
        var result = new List<ProtectionSettings>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in groups)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            var name = candidate.GroupName.Trim();
            if (name.Length == 0)
                throw new InvalidDataException("Persisted protection setting-group name is empty.");
            if (!names.Add(name))
                throw new InvalidDataException($"Duplicate protection setting-group '{name}'.");

            var normalized = candidate with { GroupName = name };
            try
            {
                normalized.Validate();
            }
            catch (ArgumentException ex)
            {
                throw new InvalidDataException($"Protection setting-group '{name}' is invalid.", ex);
            }
            result.Add(normalized);
        }

        return result
            .OrderBy(group => group.GroupName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? NormalizeActiveGroup(
        string? activeGroupName,
        IReadOnlyList<ProtectionSettings> groups)
    {
        if (string.IsNullOrWhiteSpace(activeGroupName))
            return null;

        var match = groups.FirstOrDefault(group => string.Equals(
            group.GroupName,
            activeGroupName.Trim(),
            StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            throw new InvalidDataException(
                $"Active protection setting-group '{activeGroupName}' is not present in the catalog.");
        }

        return match.GroupName;
    }

    private sealed record ProtectionSettingGroupDocument(
        string SchemaVersion,
        string? ActiveGroupName,
        IReadOnlyList<ProtectionSettings>? Groups);
}
