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

    /// <summary>Gets packed RGB5 tube-crack ink 0–31 corresponding to $AA:E2DD; the stock second sixteen-color half repeats the first, while supplied edits remain independent.</summary>
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

    private ushort ResolveStatue(ChozoStatuePalette palette, int color)
    {
        if ((uint)color >= ChozoAndTubeColorRomData.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        Dictionary<int, ushort> edits = palette == ChozoStatuePalette.WreckedShip ? wreckedShipEdits : lowerNorfairEdits;
        return edits.TryGetValue(color, out ushort edited) ? edited : ChozoStatuePaintDefinitions.Color(palette, color);
    }

    private ushort[] Statue(ChozoStatuePalette palette) =>
        Enumerable.Range(0, ChozoAndTubeColorRomData.ColorCount).Select(color => ResolveStatue(palette, color)).ToArray();

    /// <summary>Installs the tube-crack image at CGRAM 144–175, matching palette-only initializer $AA:E716; tube destruction and the initializer actor's removal remain runtime behavior.</summary>
    public void ApplyTubeCracks(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < ChozoAndTubeColorRomData.ColorCount; color++)
            cgram.SetColor(ChozoAndTubeColorRomData.Destination + color, ResolveTubeCracks(color));
    }
    /// <summary>Installs the 32 selected Wrecked Ship Chozo inks at CGRAM 144–175, replacing the $AA:E31D image copied by initializer $AA:E75C.</summary>
    public void ApplyWreckedShip(SnesCgram cgram) => Apply(cgram, ChozoStatuePalette.WreckedShip);
    /// <summary>Installs the 32 selected Lower Norfair Chozo inks at CGRAM 144–175, replacing the $AA:E35D image copied by initializer $AA:E784.</summary>
    public void ApplyLowerNorfair(SnesCgram cgram) => Apply(cgram, ChozoStatuePalette.LowerNorfair);

    /// <summary>Loads version-1 <c>chozo-and-tube-colors.json</c>, requiring three independent 32-color images and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned stream consumed from its current position and left open; unknown and duplicate JSON properties are rejected.</param>
    /// <returns>Compiled selected tube and statue inks; statue variant selection, hand PLMs, and tube behavior remain engine-owned.</returns>
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

    /// <summary>Serializes the three color images as indented camel-case UTF-8 JSON, validating version, 32-color dimensions, and RGB5 channel bounds before returning the bytes.</summary>
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

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Chozo/tube color property {name}."));
}

/// <summary>Editable tube-crack and Chozo-statue RGB5 images, each retaining two ordered sixteen-color OBJ palettes including their transparent-slot words.</summary>
public sealed record ChozoAndTubeColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>32 ordered tube-crack inks corresponding to $AA:E2DD–$E31C, filling OBJ palettes 1 and 2; stock halves repeat but edits need not.</summary>
    public required PaletteRgb5[] TubeCracks { get; init; }
    /// <summary>32 ordered Wrecked Ship Chozo inks corresponding to $AA:E31D–$E35C, filling OBJ palettes 1 and 2.</summary>
    public required PaletteRgb5[] WreckedShip { get; init; }
    /// <summary>32 ordered Lower Norfair Chozo inks corresponding to $AA:E35D–$E39C, filling OBJ palettes 1 and 2 independently of the Wrecked Ship image.</summary>
    public required PaletteRgb5[] LowerNorfair { get; init; }
}

/// <summary>Installed filename and supported schema revision for the three bounded editable tube/statue color images.</summary>
public static class ChozoAndTubeColorFormat
{
    /// <summary>Installed editable JSON filename for the tube-crack, Wrecked Ship Chozo, and Lower Norfair Chozo palette images.</summary>
    public const string FileName = "chozo-and-tube-colors.json";
    /// <summary>Supported color schema revision, requiring 32 RGB5 colors for each independent image.</summary>
    public const int Version = 1;
}
