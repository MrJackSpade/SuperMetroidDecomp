using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Three editable, full-body normal suit palettes; suit selection remains gameplay code.</summary>
/// <remarks>Issue #1165 retains the22 distinct opaque ink colors and two
/// transparent-slot payloads from $9B:9400/9520/9800 under the nonsense exception.
/// $91:DD5B copies those words to fixed OBJ palette slots. Sprite tile pixels
/// choose categorical ink indices, not a time, distance, light level or shade
/// magnitude from which RGB can be calculated. For example, the yellow Power
/// inks at slots1/2/10/11/12 have intensities8/29/21/11/18: the order identifies
/// painted pixel classes, not samples of a brightness ramp. Their common hue
/// does not determine those chosen intensities. An index fit would encode the
/// painting. Color-zero payloads are also selected source data; OBJ rendering
/// skips zero-index pixels before reading their color. Shared suit words are
/// removed independently, and generated animation/tint rows are reviewed in
/// their own owners. This exception covers these base inks, not all palettes.</remarks>
public sealed class SamusSuitColorCatalog
{
    private readonly ushort[] power;
    private readonly Dictionary<int, ushort> varia;
    private readonly Dictionary<int, ushort> gravity;

    /// <summary>Stores a Power palette and only differing colors for the other suits.</summary>
    /// <remarks>Original $9B:9400/9520/9800 share twelve of sixteen slots
    /// per secondary suit. Native suit selection is categorical; named owners
    /// replace the palette roster. Comparing supplied values preserves arbitrary
    /// independent player edits. This removes duplicated words; the remaining
    /// base-ink disposition is documented on the catalog.</remarks>
    private SamusSuitColorCatalog(ushort[] power, ushort[] varia, ushort[] gravity)
    {
        this.power = power;
        this.varia = Differences(varia, power);
        this.gravity = Differences(gravity, power);
    }

    private static Dictionary<int, ushort> Differences(ushort[] supplied, ushort[] power)
    {
        var differences = new Dictionary<int, ushort>();
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != power[index]) differences.Add(index, supplied[index]);
        return differences;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort Resolve(ushort suitTableOffset, int colorIndex)
    {
        Dictionary<int, ushort>? differences = suitTableOffset switch
        {
            0 => null,
            2 => varia,
            4 => gravity,
            _ => throw new ArgumentOutOfRangeException(nameof(suitTableOffset)),
        };
        if ((uint)colorIndex >= SamusSuitColorFormat.ColorsPerSuit)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        return differences is not null && differences.TryGetValue(colorIndex, out ushort color)
            ? color : power[colorIndex];
    }

    /// <summary>Writes only Samus's sixteen OBJ colors; no equipment or phase state changes.</summary>
    public void Apply(SnesCgram cgram, ushort suitTableOffset)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int index = 0; index < SamusSuitColorFormat.ColorsPerSuit; index++)
            cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + index,
                Resolve(suitTableOffset, index));
    }

    public static SamusSuitColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        SamusSuitColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SamusSuitColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Samus suit color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Samus suit color JSON.", error);
        }
        if (document.Version != SamusSuitColorFormat.Version)
            throw new InvalidDataException("Samus suit colors require the supported version.");
        return new(Compile(document.Power, "Power"), Compile(document.Varia, "Varia"),
            Compile(document.Gravity, "Gravity"));
    }

    public static byte[] Write(SamusSuitColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SamusSuitColorFormat.ColorsPerSuit)
            throw new InvalidDataException($"Samus {name} suit requires sixteen RGB5 colors.");
        var result = new ushort[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus {name} color {index} requires RGB components 0..31.");
            result[index] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return result;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Samus suit color property {name}."));
}

public sealed record SamusSuitColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Power { get; init; }
    public required PaletteRgb5[] Varia { get; init; }
    public required PaletteRgb5[] Gravity { get; init; }
}

public static class SamusSuitColorFormat
{
    public const string FileName = "samus-suit-colors.json";
    public const int Version = 1;
    public const int ColorsPerSuit = SamusPaletteRomData.Common.ColorsPerObjPalette;
}
