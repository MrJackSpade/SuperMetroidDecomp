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
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("ChozoAndTubeColorCatalog-v1", content =>
        {
            Span<ushort> tube = stackalloc ushort[ChozoAndTubeColorRomData.ColorCount];
            for (int color = 0; color < tube.Length; color++) tube[color] = ResolveTubeCracks(color);
            content.AppendWords("tubeCracks", tube);
            content.AppendWords("wreckedShip", Statue(ChozoStatuePalette.WreckedShip));
            content.AppendWords("lowerNorfair", Statue(ChozoStatuePalette.LowerNorfair));
        });

    private readonly ushort[] tubeColorSeeds;
    private readonly Dictionary<int, ushort> tubeColorEdits = [];
    // Statue colors calculate from ChozoStatuePaintDefinitions; only supplied deviations are stored.
    private readonly Dictionary<int, ushort> wreckedShipEdits = [];
    private readonly Dictionary<int, ushort> lowerNorfairEdits = [];

    private ChozoAndTubeColorCatalog(ushort[] tubeCracks, ushort[] wreckedShip,
        ushort[] lowerNorfair)
    {
        // The eight crack seed colors are authored paint; the remaining stock values
        // are a linear ramp and an exact repeated palette half.
        tubeColorSeeds = tubeCracks[..8];
        for (int color = 8; color < tubeCracks.Length; color++)
            if (tubeCracks[color] != CalculateTubeColor(color))
                tubeColorEdits.Add(color, tubeCracks[color]);
        for (int color = 0; color < ChozoAndTubeColorRomData.ColorCount; color++)
        {
            if (wreckedShip[color] != ChozoStatuePaintDefinitions.Color(ChozoStatuePalette.WreckedShip, color))
                wreckedShipEdits.Add(color, wreckedShip[color]);
            if (lowerNorfair[color] != ChozoStatuePaintDefinitions.Color(ChozoStatuePalette.LowerNorfair, color))
                lowerNorfairEdits.Add(color, lowerNorfair[color]);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveTubeCracks(int color)
    {
        if ((uint)color >= ChozoAndTubeColorRomData.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return tubeColorEdits.TryGetValue(color, out ushort edited) ? edited : CalculateTubeColor(color);
    }

    /// <summary>$AA:E2DD: two equal16-color halves; colors8..15 linearly shade RGB5(31,27,29) to(0,0,1).</summary>
    private ushort CalculateTubeColor(int color)
    {
        int local = color % 16;
        if (local < 8) return tubeColorSeeds[local];
        int step = local - 8;
        int red = 31 - (31 * step + 3) / 7;
        int green = 27 - (27 * step + 3) / 7;
        int blue = 29 - 4 * step;
        return (ushort)(red | green << 5 | blue << 10);
    }
    public ushort ResolveWreckedShip(int color) => ResolveStatue(ChozoStatuePalette.WreckedShip, color);
    public ushort ResolveLowerNorfair(int color) => ResolveStatue(ChozoStatuePalette.LowerNorfair, color);

    private ushort ResolveStatue(ChozoStatuePalette palette, int color)
    {
        if ((uint)color >= ChozoAndTubeColorRomData.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        Dictionary<int, ushort> edits = palette == ChozoStatuePalette.WreckedShip ? wreckedShipEdits : lowerNorfairEdits;
        return edits.TryGetValue(color, out ushort edited) ? edited : ChozoStatuePaintDefinitions.Color(palette, color);
    }

    private ushort[] Statue(ChozoStatuePalette palette) =>
        Enumerable.Range(0, ChozoAndTubeColorRomData.ColorCount).Select(color => ResolveStatue(palette, color)).ToArray();

    public void ApplyTubeCracks(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < ChozoAndTubeColorRomData.ColorCount; color++)
            cgram.SetColor(ChozoAndTubeColorRomData.Destination + color, ResolveTubeCracks(color));
    }
    public void ApplyWreckedShip(SnesCgram cgram) => Apply(cgram, ChozoStatuePalette.WreckedShip);
    public void ApplyLowerNorfair(SnesCgram cgram) => Apply(cgram, ChozoStatuePalette.LowerNorfair);

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

    private void Apply(SnesCgram cgram, ChozoStatuePalette palette)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < ChozoAndTubeColorRomData.ColorCount; color++)
            cgram.SetColor(ChozoAndTubeColorRomData.Destination + color, ResolveStatue(palette, color));
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
