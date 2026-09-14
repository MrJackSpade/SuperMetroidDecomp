using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable visual muzzle offsets; never used for projectile physics or Grapple connection.</summary>
public sealed class ChargeFlarePlacementCatalog
{
    private readonly ChargeFlareOffset[] offsets;
    private ChargeFlarePlacementCatalog(ChargeFlareOffset[] offsets) => this.offsets = offsets;
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    public ChargeFlareOffset Resolve(bool running, int direction)
    {
        if ((uint)direction >= ChargeFlarePlacementDefinitions.DirectionCount) throw new ArgumentOutOfRangeException(nameof(direction));
        return offsets[(running ? ChargeFlarePlacementDefinitions.DirectionCount : 0) + direction];
    }
    public static ChargeFlarePlacementCatalog Load(Stream json)
    {
        ChargeFlarePlacementDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            ValidateUnique(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ChargeFlarePlacementDocument>(Options)
                ?? throw new InvalidDataException("Missing charge-flare placement document.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid charge-flare placement JSON.", error); }
        int count = ChargeFlarePlacementDefinitions.DirectionCount;
        if (document.Version != ChargeFlarePlacementDefinitions.Version || document.Offsets is null || document.Offsets.Count != count * 2)
            throw new InvalidDataException("Charge-flare placement requires version 1 and all standing/running directions.");
        var offsets = new ChargeFlareOffset[count * 2];
        for (int mode = 0; mode < 2; mode++)
        for (int direction = 0; direction < count; direction++)
        {
            string key = ChargeFlarePlacementDefinitions.Key(mode != 0, direction);
            if (!document.Offsets.TryGetValue(key, out var value) || value is null)
                throw new InvalidDataException($"Missing charge-flare placement {key}.");
            offsets[mode * count + direction] = value;
        }
        return new(offsets);
    }
    public static byte[] Write(ChargeFlarePlacementDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes));
        return bytes;
    }
    private static void ValidateUnique(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) return;
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!names.Add(property.Name)) throw new InvalidDataException("Duplicate charge-flare placement property.");
            ValidateUnique(property.Value);
        }
    }
}

public sealed record ChargeFlareOffset
{
    public required short X { get; init; }
    public required short Y { get; init; }
}
public sealed record ChargeFlarePlacementDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, ChargeFlareOffset> Offsets { get; init; }
}
