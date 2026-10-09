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
    /// <summary>Complete Power palette used as the shared baseline for resolving suit colors.</summary>
    private readonly ushort[] power;

    /// <summary>Varia color words stored only for slots that differ from the Power palette.</summary>
    private readonly Dictionary<int, ushort> varia;

    /// <summary>Gravity color words stored only for slots that differ from the Power palette.</summary>
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

    /// <summary>Builds a sparse set of palette words that differ from the Power baseline.</summary>
    /// <param name="supplied">Complete palette words for a secondary suit.</param>
    /// <param name="power">Complete Power palette used for same-slot comparisons.</param>
    /// <returns>Entries keyed by color slot only where the supplied word differs.</returns>
    private static Dictionary<int, ushort> Differences(ushort[] supplied, ushort[] power)
    {
        var differences = new Dictionary<int, ushort>();
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != power[index]) differences.Add(index, supplied[index]);
        return differences;
    }

    /// <summary>Configures the stable JSON shape used when reading and writing suit-color documents.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Resolves one packed RGB555 color from the selected normal suit palette.</summary>
    /// <param name="suitTableOffset">Native categorical table offset: 0 for Power, 2 for Varia, or 4 for Gravity.</param>
    /// <param name="colorIndex">OBJ palette slot from 0 through 15, including the transparent slot.</param>
    /// <returns>The selected packed SNES RGB555 word.</returns>
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

    /// <summary>Loads and validates the three complete normal-suit palettes from JSON.</summary>
    /// <param name="json">Caller-owned stream containing the suit-color document.</param>
    /// <returns>The compiled suit-color catalog.</returns>
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

    /// <summary>Validates and serializes a suit-color document as UTF-8 JSON.</summary>
    /// <param name="document">Document containing all three 16-color palettes.</param>
    /// <returns>A new caller-owned JSON byte array.</returns>
    public static byte[] Write(SamusSuitColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Validates one sixteen-color RGB5 palette and packs its channels into SNES RGB555 words.</summary>
    /// <param name="source">Palette entries to validate; the array must contain exactly sixteen non-null colors.</param>
    /// <param name="name">Suit name used to identify invalid palette data in the exception message.</param>
    /// <returns>Packed color words in the same slot order as the source palette.</returns>
    /// <exception cref="InvalidDataException">The palette is missing, has the wrong length, or contains an invalid RGB5 color.</exception>
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

    /// <summary>Rejects duplicate JSON property names before the suit-color document is deserialized.</summary>
    /// <param name="value">Parsed JSON value whose object properties must be unique using ordinal comparison.</param>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Samus suit color property {name}."));
}

/// <summary>JSON schema for the Power, Varia, and Gravity normal body palettes.</summary>
public sealed record SamusSuitColorDocument
{
    /// <summary>Gets the schema version required by <see cref="SamusSuitColorFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the sixteen editable Power-suit RGB5 colors.</summary>
    public required PaletteRgb5[] Power { get; init; }

    /// <summary>Gets the sixteen editable Varia-suit RGB5 colors.</summary>
    public required PaletteRgb5[] Varia { get; init; }

    /// <summary>Gets the sixteen editable Gravity-suit RGB5 colors.</summary>
    public required PaletteRgb5[] Gravity { get; init; }
}

/// <summary>Defines the installed normal-suit color resource and its fixed palette size.</summary>
public static class SamusSuitColorFormat
{
    /// <summary>Canonical normal-suit color asset file name.</summary>
    public const string FileName = "samus-suit-colors.json";
    /// <summary>Supported normal-suit color schema version.</summary>
    public const int Version = 1;
    /// <summary>Number of OBJ palette colors stored for each suit, including the transparent slot.</summary>
    public const int ColorsPerSuit = SamusPaletteRomData.Common.ColorsPerObjPalette;
}
