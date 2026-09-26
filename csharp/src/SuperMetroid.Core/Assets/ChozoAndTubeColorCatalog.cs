using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable RGB5 target images for the two Chozo statues and n00b-tube cracks.
/// Statue variant selection, PLM placement and tube behavior remain compiled.
/// </summary>
public sealed class ChozoAndTubeColorCatalog
{
    private readonly ushort[] tubeCracks;
    private readonly ushort[] wreckedShip;
    private readonly ushort[] lowerNorfair;

    private ChozoAndTubeColorCatalog(ushort[] tubeCracks, ushort[] wreckedShip,
        ushort[] lowerNorfair)
    {
        this.tubeCracks = tubeCracks;
        this.wreckedShip = wreckedShip;
        this.lowerNorfair = lowerNorfair;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveTubeCracks(int color) => Get(tubeCracks, color);
    public ushort ResolveWreckedShip(int color) => Get(wreckedShip, color);
    public ushort ResolveLowerNorfair(int color) => Get(lowerNorfair, color);

    public void ApplyTubeCracks(SnesCgram cgram) => Apply(cgram, tubeCracks);
    public void ApplyWreckedShip(SnesCgram cgram) => Apply(cgram, wreckedShip);
    public void ApplyLowerNorfair(SnesCgram cgram) => Apply(cgram, lowerNorfair);

    public static ChozoAndTubeColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ChozoAndTubeColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ChozoAndTubeColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Chozo/tube color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Chozo/tube color JSON.", error);
        }
        if (document.Version != ChozoAndTubeColorFormat.Version)
            throw new InvalidDataException("Chozo/tube colors require the supported version.");
        return new(
            Compile(document.TubeCracks, "tube cracks"),
            Compile(document.WreckedShip, "Wrecked Ship"),
            Compile(document.LowerNorfair, "Lower Norfair"));
    }

    public static byte[] Write(ChozoAndTubeColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort Get(ushort[] colors, int index) =>
        (uint)index < colors.Length
            ? colors[index]
            : throw new ArgumentOutOfRangeException(nameof(index));

    private static void Apply(SnesCgram cgram, ushort[] colors)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < colors.Length; color++)
            cgram.SetColor(ChozoAndTubeColorRomData.Destination + color, colors[color]);
    }

    private static ushort[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != ChozoAndTubeColorRomData.ColorCount)
            throw new InvalidDataException(
                $"Chozo/tube {name} requires {ChozoAndTubeColorRomData.ColorCount} RGB5 colors.");
        var compiled = new ushort[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Chozo/tube {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate Chozo/tube color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record ChozoAndTubeColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] TubeCracks { get; init; }
    public required PaletteRgb5[] WreckedShip { get; init; }
    public required PaletteRgb5[] LowerNorfair { get; init; }
}

public static class ChozoAndTubeColorFormat
{
    public const string FileName = "chozo-and-tube-colors.json";
    public const int Version = 1;
}
