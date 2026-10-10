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
            Span<Bgr555> introWords = stackalloc Bgr555[DraygonColorRomData.IntroCount];
            for (int color = 0; color < introWords.Length; color++) introWords[color] = intro.Color(color);
            content.AppendColors("intro", introWords);
            Span<Bgr555> backgroundWords = stackalloc Bgr555[DraygonColorRomData.BackgroundCount];
            for (int color = 0; color < backgroundWords.Length; color++) backgroundWords[color] = background.Color(color);
            content.AppendColors("background", backgroundWords);
            Span<Bgr555> spriteWords = stackalloc Bgr555[DraygonColorRomData.SpriteCount];
            for (int color = 0; color < spriteWords.Length; color++) spriteWords[color] = sprite.Color(color);
            content.AppendColors("sprite", spriteWords);
            Span<Bgr555> flash = stackalloc Bgr555[DraygonColorRomData.WhiteFlashCount];
            for (int color = 0; color < flash.Length; color++) flash[color] = ResolveWhiteFlash(color);
            content.AppendColors("whiteFlash", flash);
            content.Append("healthBands", DraygonColorRomData.HealthBandCount);
            Span<Bgr555> row = stackalloc Bgr555[DraygonColorRomData.HealthBandColorCount];
            for (int band = 0; band < DraygonColorRomData.HealthBandCount; band++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = ResolveHealthBand(band, color);
                content.AppendColors("row", row);
            }
        });

    private readonly DraygonIntroPaintDefinitions intro;
    private readonly DraygonMaterialPaintDefinitions background;
    private readonly DraygonMaterialPaintDefinitions sprite;
    private readonly Dictionary<int, Bgr555> whiteFlash = new();
    private readonly DraygonHealthPaintDefinitions healthBands;

    private DraygonColorCatalog(Bgr555[] intro, Bgr555[] background, Bgr555[] sprite,
        Bgr555[] whiteFlash, Bgr555[][] healthBands)
    {
        this.intro = new(intro);
        this.background = new(background);
        this.sprite = new(sprite);
        for (int color = 0; color < whiteFlash.Length; color++)
            if (whiteFlash[color] != StockWhiteFlash(color)) this.whiteFlash.Add(color, whiteFlash[color]);
        this.healthBands = new(healthBands);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Gets one of the 25 selected RGB5 colors used by Draygon's opening palette.</summary>
    public Bgr555 ResolveIntro(int color) => intro.Color(color);
    /// <summary>Gets one of the 16 selected normal background colors.</summary>
    public Bgr555 ResolveBackground(int color) => background.Color(color);
    /// <summary>Gets one of the 16 selected normal sprite colors.</summary>
    public Bgr555 ResolveSprite(int color) => sprite.Color(color);
    /// <summary>Gets one of the 16 selected white-flash colors, preserving transparent entry zero.</summary>
    public Bgr555 ResolveWhiteFlash(int color)
    {
        if ((uint)color >= DraygonColorRomData.WhiteFlashCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return whiteFlash.TryGetValue(color, out Bgr555 selected) ? selected : StockWhiteFlash(color);
    }

    /// <summary>Gets one of the four selected colors in an eight-row health palette table.</summary>
    public Bgr555 ResolveHealthBand(int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= DraygonColorRomData.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return healthBands.Color(band, color);
    }

    /// <summary><c>Palette_Draygon_WhiteFlash</c> at $A5:A297 preserves the backdrop; all visible inks are white.</summary>
    private static Bgr555 StockWhiteFlash(int color) => color == 0 ? DraygonMaterialPaintDefinitions.ClearTarget : Bgr555.FromWord(0x7fff);

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
            tableByteIndex / Bgr555.ByteCount >= DraygonColorRomData.HealthBandCount)
            throw new InvalidDataException(
                $"Draygon health palette byte index ${tableByteIndex:X4} is not authored.");
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < DraygonColorRomData.HealthBandColorCount; color++)
            cgram.SetColor(DraygonColorRomData.HealthDestination + color,
                ResolveHealthBand(tableByteIndex / Bgr555.ByteCount, color));
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

    private static int CheckBand(int band) =>
        (uint)band < DraygonColorRomData.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));

    private static void ApplyCalculated(SnesCgram cgram, int count, int destination, Func<int, Bgr555> resolve)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < count; color++) cgram.SetColor(destination + color, resolve(color));
    }
    private static Bgr555[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Draygon {name} requires {count} RGB5 colors.");
        var compiled = new Bgr555[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Draygon {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToBgr555();
        }
        return compiled;
    }

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
