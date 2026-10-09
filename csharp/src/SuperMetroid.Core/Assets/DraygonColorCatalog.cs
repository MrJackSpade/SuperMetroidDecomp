using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Draygon's editable RGB5 opening, normal, flash, and health-band colors. Health
/// thresholds, hit reactions, and flash timing remain cartridge-mechanics code.
/// </summary>
public sealed class DraygonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("DraygonColorCatalog-v1", content =>
        {
            Span<ushort> introWords = stackalloc ushort[DraygonColorRomData.IntroCount];
            for (int color = 0; color < introWords.Length; color++) introWords[color] = intro.Color(color);
            content.AppendWords("intro", introWords);
            Span<ushort> backgroundWords = stackalloc ushort[DraygonColorRomData.BackgroundCount];
            for (int color = 0; color < backgroundWords.Length; color++) backgroundWords[color] = background.Color(color);
            content.AppendWords("background", backgroundWords);
            Span<ushort> spriteWords = stackalloc ushort[DraygonColorRomData.SpriteCount];
            for (int color = 0; color < spriteWords.Length; color++) spriteWords[color] = sprite.Color(color);
            content.AppendWords("sprite", spriteWords);
            Span<ushort> flash = stackalloc ushort[DraygonColorRomData.WhiteFlashCount];
            for (int color = 0; color < flash.Length; color++) flash[color] = ResolveWhiteFlash(color);
            content.AppendWords("whiteFlash", flash);
            content.Append("healthBands", DraygonColorRomData.HealthBandCount);
            Span<ushort> row = stackalloc ushort[DraygonColorRomData.HealthBandColorCount];
            for (int band = 0; band < DraygonColorRomData.HealthBandCount; band++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = ResolveHealthBand(band, color);
                content.AppendWords("row", row);
            }
        });

    /// <summary>Selected opening RGB5 colors used when the intro palette is applied.</summary>
    private readonly DraygonIntroPaintDefinitions intro;

    /// <summary>Selected normal-background colors, with stock values resolved from their native layout.</summary>
    private readonly DraygonMaterialPaintDefinitions background;

    /// <summary>Selected normal-sprite colors, with stock values resolved from their native layout.</summary>
    private readonly DraygonMaterialPaintDefinitions sprite;

    /// <summary>Only white-flash entries that differ from the cartridge's white-and-transparent stock row.</summary>
    private readonly Dictionary<int, ushort> whiteFlash = new();

    /// <summary>Selected per-health-row RGB5 values used by Draygon's damage palette.</summary>
    private readonly DraygonHealthPaintDefinitions healthBands;

    /// <summary>Creates the compiled color views and retains only flash overrides that differ from stock.</summary>
    /// <param name="intro">Packed RGB5 values for the opening palette.</param>
    /// <param name="background">Packed RGB5 values for normal background colors.</param>
    /// <param name="sprite">Packed RGB5 values for normal sprite colors.</param>
    /// <param name="whiteFlash">Packed RGB5 values for the hurt-flash row.</param>
    /// <param name="healthBands">Packed color rows selected at the eight native health-table offsets.</param>
    private DraygonColorCatalog(ushort[] intro, ushort[] background, ushort[] sprite,
        ushort[] whiteFlash, ushort[][] healthBands)
    {
        this.intro = new(intro);
        this.background = new(background);
        this.sprite = new(sprite);
        for (int color = 0; color < whiteFlash.Length; color++)
            if (whiteFlash[color] != StockWhiteFlash(color)) this.whiteFlash.Add(color, whiteFlash[color]);
        this.healthBands = new(healthBands);
    }

    /// <summary>Strict camel-case JSON settings shared by Draygon color document loading and serialization.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Gets one of the 25 selected RGB5 colors used by Draygon's opening palette.</summary>
    public ushort ResolveIntro(int color) => intro.Color(color);
    /// <summary>Gets one of the 16 selected normal background colors.</summary>
    public ushort ResolveBackground(int color) => background.Color(color);
    /// <summary>Gets one of the 16 selected normal sprite colors.</summary>
    public ushort ResolveSprite(int color) => sprite.Color(color);
    /// <summary>Gets one of the 16 selected white-flash colors, preserving transparent entry zero.</summary>
    public ushort ResolveWhiteFlash(int color)
    {
        if ((uint)color >= DraygonColorRomData.WhiteFlashCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return whiteFlash.TryGetValue(color, out ushort selected) ? selected : StockWhiteFlash(color);
    }

    /// <summary>Gets one of the four selected colors in an eight-row health palette table.</summary>
    public ushort ResolveHealthBand(int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= DraygonColorRomData.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return healthBands.Color(band, color);
    }

    /// <summary><c>Palette_Draygon_WhiteFlash</c> at $A5:A297 preserves the backdrop; all visible inks are white.</summary>
    private static ushort StockWhiteFlash(int color) => color == 0 ? DraygonMaterialPaintDefinitions.ClearTarget : (ushort)0x7fff;

    /// <summary>Installs the selected opening colors at their native CGRAM destination.</summary>
    public void ApplyIntro(SnesCgram cgram) =>
        ApplyCalculated(cgram, DraygonColorRomData.IntroCount, DraygonColorRomData.IntroDestination, ResolveIntro);

    /// <summary>Installs Draygon's normal or white-flash material colors and the active health band.</summary>
    public void ApplyHurt(SnesCgram cgram, bool whiteFrame, ushort healthTableByteIndex)
    {
        if (whiteFrame)
            ApplyCalculated(cgram, DraygonColorRomData.WhiteFlashCount, DraygonColorRomData.BackgroundDestination, ResolveWhiteFlash);
        else
            ApplyCalculated(cgram, DraygonColorRomData.BackgroundCount, DraygonColorRomData.BackgroundDestination, ResolveBackground);
        if (!whiteFrame)
            ApplyHealthBand(cgram, healthTableByteIndex);
        if (whiteFrame)
            ApplyCalculated(cgram, DraygonColorRomData.WhiteFlashCount, DraygonColorRomData.SpriteDestination, ResolveWhiteFlash);
        else
            ApplyCalculated(cgram, DraygonColorRomData.SpriteCount, DraygonColorRomData.SpriteDestination, ResolveSprite);
    }

    /// <summary>Installs the four-color health band selected by its even native table byte index.</summary>
    public void ApplyHealthBand(SnesCgram cgram, ushort tableByteIndex)
    {
        if ((tableByteIndex & 1) != 0 ||
            tableByteIndex / sizeof(ushort) >= DraygonColorRomData.HealthBandCount)
            throw new InvalidDataException(
                $"Draygon health palette byte index ${tableByteIndex:X4} is not authored.");
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < DraygonColorRomData.HealthBandColorCount; color++)
            cgram.SetColor(DraygonColorRomData.HealthDestination + color,
                ResolveHealthBand(tableByteIndex / sizeof(ushort), color));
    }

    /// <summary>Loads and validates the editable Draygon RGB5 color document.</summary>
    public static DraygonColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        DraygonColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<DraygonColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Draygon color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Draygon color JSON.", error);
        }
        if (document.Version != DraygonColorFormat.Version)
            throw new InvalidDataException("Draygon colors require the supported version.");
        if (document.HealthBands is null ||
            document.HealthBands.Length != DraygonColorRomData.HealthBandCount)
            throw new InvalidDataException("Draygon colors require eight health bands.");
        return new(
            Compile(document.Intro, DraygonColorRomData.IntroCount, "intro"),
            Compile(document.Background, DraygonColorRomData.BackgroundCount, "background"),
            Compile(document.Sprite, DraygonColorRomData.SpriteCount, "sprite"),
            Compile(document.WhiteFlash, DraygonColorRomData.WhiteFlashCount, "white flash"),
            document.HealthBands.Select((band, index) =>
                Compile(band, DraygonColorRomData.HealthBandColorCount,
                    $"health band {index}")).ToArray());
    }

    /// <summary>Validates and serializes a Draygon color document as indented camel-case JSON.</summary>
    public static byte[] Write(DraygonColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Validates a zero-based index into Draygon's eight health-palette rows.</summary>
    /// <param name="band">Health-row index to check.</param>
    /// <returns>The same index when it names an authored row.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the authored health-band range.</exception>
    private static int CheckBand(int band) =>
        (uint)band < DraygonColorRomData.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));

    /// <summary>Writes a contiguous run of resolved palette colors to its selected CGRAM destination.</summary>
    /// <param name="cgram">CGRAM receiving the color words.</param>
    /// <param name="count">Number of entries to resolve and write.</param>
    /// <param name="destination">First CGRAM color index in the run.</param>
    /// <param name="resolve">Function selecting the packed RGB5 word for each zero-based entry.</param>
    private static void ApplyCalculated(SnesCgram cgram, int count, int destination, Func<int, ushort> resolve)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < count; color++) cgram.SetColor(destination + color, resolve(color));
    }
    /// <summary>Validates RGB5 channel ranges and packs a named document color row into SNES CGRAM words.</summary>
    /// <param name="source">Document colors to compile; it must contain exactly the requested number of entries.</param>
    /// <param name="count">Required number of colors in the row.</param>
    /// <param name="name">Row label included in validation errors.</param>
    /// <returns>Packed 15-bit RGB color words in source order.</returns>
    /// <exception cref="InvalidDataException">The row has the wrong length or contains a null color or channel outside 0 through 31.</exception>
    private static ushort[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Draygon {name} requires {count} RGB5 colors.");
        var compiled = new ushort[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Draygon {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    /// <summary>Rejects duplicate JSON property names before deserializing the authored document.</summary>
    /// <param name="value">Parsed JSON value whose nested objects are checked using ordinal property-name comparison.</param>
    /// <exception cref="InvalidDataException">Any object contains a duplicate property name.</exception>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Draygon color property {name}."));
}

/// <summary>Defines editable RGB5 colors for Draygon's opening, combat, flash, and health states.</summary>
public sealed record DraygonColorDocument
{
    /// <summary>Gets the document schema version.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the 25-color opening palette.</summary>
    public required PaletteRgb5[] Intro { get; init; }
    /// <summary>Gets the 16 normal background colors.</summary>
    public required PaletteRgb5[] Background { get; init; }
    /// <summary>Gets the 16 normal sprite colors.</summary>
    public required PaletteRgb5[] Sprite { get; init; }
    /// <summary>Gets the 16 colors used by a white hurt-flash frame.</summary>
    public required PaletteRgb5[] WhiteFlash { get; init; }
    /// <summary>Gets eight health-band rows containing four colors each.</summary>
    public required PaletteRgb5[][] HealthBands { get; init; }
}

/// <summary>Defines the filename and schema revision for editable Draygon colors.</summary>
public static class DraygonColorFormat
{
    /// <summary>JSON filename containing Draygon's RGB5 color selections.</summary>
    public const string FileName = "draygon-colors.json";
    /// <summary>Supported Draygon color document schema revision.</summary>
    public const int Version = 1;
}
