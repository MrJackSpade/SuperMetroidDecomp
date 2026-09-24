using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Kraid RGB5 sources; boss phase and fade arithmetic stay engine-owned.</summary>
public sealed class KraidColorCatalog
{
    private readonly Dictionary<KraidPaletteSource, ushort[]> colors;

    private KraidColorCatalog(Dictionary<KraidPaletteSource, ushort[]> colors) =>
        this.colors = colors;

    public ushort Resolve(KraidPaletteSource source, int index)
    {
        if (!colors.TryGetValue(source, out ushort[]? band))
            throw new ArgumentOutOfRangeException(nameof(source));
        if ((uint)index >= band.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return band[index];
    }

    public static KraidColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        KraidColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<KraidColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Kraid colors JSON is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Kraid colors JSON.", error);
        }
        if (document.Version != KraidColorFormat.Version)
            throw new InvalidDataException("Kraid colors require the supported version.");
        return new(new Dictionary<KraidPaletteSource, ushort[]>
        {
            [KraidPaletteSource.RoomBackdrop] = Compile(document.RoomBackdrop,
                KraidPaletteSource.RoomBackdrop),
            [KraidPaletteSource.InitialTarget] = Compile(document.InitialTarget,
                KraidPaletteSource.InitialTarget),
            [KraidPaletteSource.Health] = Compile(document.Health,
                KraidPaletteSource.Health),
            [KraidPaletteSource.Secondary] = Compile(document.Secondary,
                KraidPaletteSource.Secondary),
            [KraidPaletteSource.DeathArm] = Compile(document.DeathArm,
                KraidPaletteSource.DeathArm),
        });
    }

    public static byte[] Write(KraidColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[] Compile(PaletteRgb5[]? source, KraidPaletteSource kind)
    {
        int expected = KraidPaletteRomData.ColorCount(kind);
        if (source is null || source.Length != expected)
            throw new InvalidDataException($"Kraid {kind} requires {expected} colors.");
        var result = new ushort[expected];
        for (int index = 0; index < expected; index++)
        {
            PaletteRgb5? rgb = source[index];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Kraid {kind} color {index} requires RGB5 channels 0..31.");
            result[index] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return result;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate Kraid color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

public sealed record KraidColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] RoomBackdrop { get; init; }
    public required PaletteRgb5[] InitialTarget { get; init; }
    public required PaletteRgb5[] Health { get; init; }
    public required PaletteRgb5[] Secondary { get; init; }
    public required PaletteRgb5[] DeathArm { get; init; }
}

public static class KraidColorFormat
{
    public const string FileName = "kraid-colors.json";
    public const int Version = 1;
}
