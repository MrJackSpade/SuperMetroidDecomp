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
            content.AppendWords("intro", intro);
            content.AppendWords("background", background);
            content.AppendWords("sprite", sprite);
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

    private readonly ushort[] intro;
    private readonly ushort[] background;
    private readonly ushort[] sprite;
    private readonly Dictionary<int, ushort> whiteFlash = new();
    private readonly Dictionary<int, ushort> healthBands = new();

    private DraygonColorCatalog(ushort[] intro, ushort[] background, ushort[] sprite,
        ushort[] whiteFlash, ushort[][] healthBands)
    {
        this.intro = intro;
        this.background = background;
        this.sprite = sprite;
        for (int color = 0; color < whiteFlash.Length; color++)
            if (whiteFlash[color] != StockWhiteFlash(color)) this.whiteFlash.Add(color, whiteFlash[color]);
        for (int color = 0; color < DraygonColorRomData.HealthBandColorCount; color++)
        {
            this.healthBands.Add(color, healthBands[0][color]);
            this.healthBands.Add(28 + color, healthBands[7][color]);
            for (int band = 1; band < 7; band++)
                if (healthBands[band][color] != InterpolateHealth(healthBands[0][color], healthBands[7][color], band))
                    this.healthBands.Add(band * 4 + color, healthBands[band][color]);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveIntro(int color) => Get(intro, color);
    public ushort ResolveBackground(int color) => Get(background, color);
    public ushort ResolveSprite(int color) => Get(sprite, color);
    public ushort ResolveWhiteFlash(int color)
    {
        if ((uint)color >= DraygonColorRomData.WhiteFlashCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return whiteFlash.TryGetValue(color, out ushort selected) ? selected : StockWhiteFlash(color);
    }

    public ushort ResolveHealthBand(int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= DraygonColorRomData.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return healthBands.TryGetValue(band * 4 + color, out ushort selected) ? selected
            : InterpolateHealth(healthBands[color], healthBands[28 + color], band);
    }

    /// <summary><c>Palette_Draygon_WhiteFlash</c> at $A5:A297 preserves the backdrop; all visible inks are white.</summary>
    private static ushort StockWhiteFlash(int color) => color == 0 ? (ushort)0x3800 : (ushort)0x7fff;

    // $A5:96AF advances RGB5 channels by nearest sevenths between the endpoint rows.
    // Endpoint colors remain unresolved inputs; independently edited middle rows override this ramp.
    private static ushort InterpolateHealth(ushort first, ushort last, int band)
    {
        int result = 0;
        for (int shift = 0; shift <= 10; shift += 5)
        {
            int start = first >> shift & 31;
            int difference = (last >> shift & 31) - start;
            int channel = start + Math.Sign(difference) * ((Math.Abs(difference) * band + 3) / 7);
            result |= channel << shift;
        }
        return (ushort)result;
    }

    public void ApplyIntro(SnesCgram cgram) =>
        Apply(cgram, intro, DraygonColorRomData.IntroDestination);

    public void ApplyHurt(SnesCgram cgram, bool whiteFrame, ushort healthTableByteIndex)
    {
        if (whiteFrame)
            ApplyCalculated(cgram, DraygonColorRomData.WhiteFlashCount, DraygonColorRomData.BackgroundDestination, ResolveWhiteFlash);
        else
            Apply(cgram, background, DraygonColorRomData.BackgroundDestination);
        if (!whiteFrame)
            ApplyHealthBand(cgram, healthTableByteIndex);
        if (whiteFrame)
            ApplyCalculated(cgram, DraygonColorRomData.WhiteFlashCount, DraygonColorRomData.SpriteDestination, ResolveWhiteFlash);
        else
            Apply(cgram, sprite, DraygonColorRomData.SpriteDestination);
    }

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

    private static ushort Get(ushort[] colors, int index) =>
        (uint)index < colors.Length
            ? colors[index]
            : throw new ArgumentOutOfRangeException(nameof(index));

    private static void Apply(SnesCgram cgram, ushort[] colors, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < colors.Length; color++)
            cgram.SetColor(destination + color, colors[color]);
    }

    private static void ApplyCalculated(SnesCgram cgram, int count, int destination, Func<int, ushort> resolve)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < count; color++) cgram.SetColor(destination + color, resolve(color));
    }
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

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate Draygon color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record DraygonColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Intro { get; init; }
    public required PaletteRgb5[] Background { get; init; }
    public required PaletteRgb5[] Sprite { get; init; }
    public required PaletteRgb5[] WhiteFlash { get; init; }
    public required PaletteRgb5[][] HealthBands { get; init; }
}

public static class DraygonColorFormat
{
    public const string FileName = "draygon-colors.json";
    public const int Version = 1;
}
