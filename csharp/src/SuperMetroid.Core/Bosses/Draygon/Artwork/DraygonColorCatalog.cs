using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

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

    /// <summary>Selected opening colors for Draygon's intro CGRAM region.</summary>
    internal readonly DraygonIntroPaintDefinitions intro;
    /// <summary>Normal background material colors used outside white hurt flashes.</summary>
    internal readonly DraygonMaterialPaintDefinitions background;
    /// <summary>Normal sprite material colors used outside white hurt flashes.</summary>
    internal readonly DraygonMaterialPaintDefinitions sprite;
    /// <summary>Only edited flash colors; stock entries are calculated on lookup.</summary>
    internal readonly Dictionary<int, Bgr555> whiteFlash = new();
    /// <summary>Four selected colors for each of Draygon's eight health palette rows.</summary>
    internal readonly DraygonHealthPaintDefinitions healthBands;

    /// <summary>Retains validated colors and stores only flash entries that differ from stock white.</summary>
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

    /// <summary>Uses the published camel-case schema and rejects unknown JSON fields.</summary>
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
    public void ApplyIntro(IPaletteColors cgram) =>
        ApplyCalculated(new BossPalettes(cgram).DraygonIntro, DraygonColorRomData.IntroCount, ResolveIntro);

    /// <summary>Installs Draygon's normal or white-flash material colors and the active health band.</summary>
    public void ApplyHurt(IPaletteColors cgram, bool whiteFrame, int healthBand)
    {
        if (whiteFrame)
            ApplyCalculated(new BossPalettes(cgram).DraygonBackground, DraygonColorRomData.WhiteFlashCount, ResolveWhiteFlash);
        else
            ApplyCalculated(new BossPalettes(cgram).DraygonBackground, DraygonColorRomData.BackgroundCount, ResolveBackground);
        if (!whiteFrame)
            ApplyHealthBand(cgram, healthBand);
        if (whiteFrame)
            ApplyCalculated(new BossPalettes(cgram).DraygonSprite, DraygonColorRomData.WhiteFlashCount, ResolveWhiteFlash);
        else
            ApplyCalculated(new BossPalettes(cgram).DraygonSprite, DraygonColorRomData.SpriteCount, ResolveSprite);
    }

    /// <summary>Installs the four-color health band 0-7.</summary>
    public void ApplyHealthBand(IPaletteColors cgram, int band)
    {
        if (band is < 0 or >= DraygonColorRomData.HealthBandCount)
            throw new InvalidDataException($"Draygon health palette band {band} is not authored.");
        Ensure.NotNull(cgram);
        for (int color = 0; color < DraygonColorRomData.HealthBandColorCount; color++)
            new BossPalettes(cgram).DraygonHealth.Write(color,
                ResolveHealthBand(band, color));
    }

    /// <summary>Loads and validates the editable Draygon RGB5 color document.</summary>
    public static DraygonColorCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
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

    /// <summary>Validates a health palette row before resolving one of its colors.</summary>
    private static int CheckBand(int band) =>
        (uint)band < DraygonColorRomData.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));

    /// <summary>Writes the selected colors in index order to a native CGRAM palette view.</summary>
    private static void ApplyCalculated(CgramPalette destination, int count, Func<int, Bgr555> resolve)
    {
        for (int color = 0; color < count; color++) destination.Write(color, resolve(color));
    }
    /// <summary>Checks a named RGB5 array's required length and channel bounds before packing its colors.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Draygon {name}");

    /// <summary>Rejects repeated case-sensitive Draygon JSON property names before deserialization.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Draygon color property {name}."));
}
