using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable RGB5 color targets for Phantoon's health, materialization and ship-power
/// transitions. Boss phase selection and interpolation arithmetic remain compiled.
/// </summary>
public sealed class PhantoonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("PhantoonColorCatalog-v1", content =>
        {
            Span<ushort> target = stackalloc ushort[PhantoonColorRomData.FadeOutCount];
            for (int color = 0; color < target.Length; color++) target[color] = ResolveFadeOut(color);
            content.AppendWords("fadeOut", target);
            content.AppendWords("powerOn", powerOn);
            var frames = new ushort[PhantoonColorRomData.HealthBandCount][];
            for (int band = 0; band < frames.Length; band++)
            {
                frames[band] = new ushort[PhantoonColorRomData.HealthBandColorCount];
                for (int color = 0; color < frames[band].Length; color++) frames[band][color] = ResolveHealth(band, color);
            }
            content.AppendWordFrames("healthBands", frames);
        });

    // REQUIRED: sixteen independently chosen healthy paint colors, tint range 8/15,
    // and nine native channel deviations below. None has a retention exemption.
    private readonly record struct HealthyPaint(int Red, int? RequiredBlue);
    private readonly HealthyPaint[] healthyPaint;
    private readonly Dictionary<int, int> healthyGreenEdits = [];
    private readonly Dictionary<int, int> healthyBlueEdits = [];
    private const int YellowChannelSeparation = 7;
    private readonly Dictionary<int, int> requiredHealthChannels = [];
    private readonly Dictionary<int, ushort> healthEdits = [];
    private const int MinimumTintWeight = 8;
    private const int TintDenominator = 15;
    private readonly Dictionary<int, ushort> fadeOutEdits = [];
    private readonly ushort[] powerOn;

    private PhantoonColorCatalog(ushort[][] healthBands, ushort[] fadeOut, ushort[] powerOn)
    {
        ushort[] healthy = healthBands[^1];
        healthyPaint = new HealthyPaint[healthy.Length];
        for (int color = 0; color < healthy.Length; color++)
        {
            int red = healthy[color] & 31, blue = healthy[color] >> 10;
            healthyPaint[color] = new(red, SharesYellowBlue(color) ? null : blue);
            if (SharesYellowBlue(color) && blue != Math.Max(0, red - YellowChannelSeparation))
                healthyBlueEdits.Add(color, blue);
            int green = (healthy[color] >> 5) & 31;
            if (green != HealthyGreen(color)) healthyGreenEdits.Add(color, green);
        }
        for (int band = 0; band < healthBands.Length - 1; band++)
        for (int color = 0; color < healthyPaint.Length; color++)
        {
            int key = band * healthyPaint.Length + color;
            for (int channel = 0; channel < 3; channel++)
                if (IsRequiredHealthChannel(band, color, channel))
                    requiredHealthChannels.Add(key * 3 + channel, (healthBands[band][color] >> (channel * 5)) & 31);
            if (healthBands[band][color] != RequiredHealthColor(band, color)) healthEdits.Add(key, healthBands[band][color]);
        }
        for (int color = 0; color < fadeOut.Length; color++)
            if (fadeOut[color] != 0) fadeOutEdits.Add(color, fadeOut[color]);
        this.powerOn = powerOn;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>
    /// $A7:CB41..CC40 health targets, selected by $A7:DC0F. The matched subset
    /// interpolates healthy RGB toward saturated red as health decreases. Nine channel
    /// deviations in eight words remain explicit required inputs.
    /// </summary>
    public ushort ResolveHealth(int band, int color)
    {
        _ = CheckBand(band);
        _ = HealthyColor(color);
        int key = band * healthyPaint.Length + color;
        return healthEdits.TryGetValue(key, out ushort edit) ? edit : RequiredHealthColor(band, color);
    }

    /// <summary>
    /// $A7:CC21..CC40 healthy paint: slots0..12 share red/green channels for
    /// olive/yellow body and iris shading; red eye-surround slots13..15 omit green.
    /// R/B levels, grouping and unused transported slot0 remain required inputs.
    /// </summary>
    // REQUIRED grouping: these olive body/iris shades share subtractive yellow
    // separation. Brightness, separation7 and all other blue choices remain inputs.
    private static bool SharesYellowBlue(int color) => color is 2 or 3 or 4 or 5 or 7 or 8 or 11 or 12;
    private int HealthyGreen(int color) => color < 13 ? healthyPaint[color].Red : 0;

    private ushort HealthyColor(int color)
    {
        if ((uint)color >= healthyPaint.Length) throw new ArgumentOutOfRangeException(nameof(color));
        HealthyPaint paint = healthyPaint[color];
        int green = healthyGreenEdits.TryGetValue(color, out int edit) ? edit : HealthyGreen(color);
        int blue = healthyBlueEdits.TryGetValue(color, out int blueEdit) ? blueEdit
            : paint.RequiredBlue ?? Math.Max(0, paint.Red - YellowChannelSeparation);
        return (ushort)(paint.Red | green << 5 | blue << 10);
    }
    private ushort TintHealthColor(int band, int color)
    {
        ushort healthy = HealthyColor(color);
        int weight = MinimumTintWeight + band;
        int red = (31 * TintDenominator + ((healthy & 31) - 31) * weight) / TintDenominator;
        int green = ((healthy >> 5) & 31) * weight / TintDenominator;
        int blue = (healthy >> 10) * weight / TintDenominator;
        return (ushort)(red | green << 5 | blue << 10);
    }

    private ushort RequiredHealthColor(int band, int color)
    {
        int value = TintHealthColor(band, color);
        int key = (band * healthyPaint.Length + color) * 3;
        for (int channel = 0; channel < 3; channel++)
            if (requiredHealthChannels.TryGetValue(key + channel, out int required))
            {
                int shift = channel * 5;
                value = (value & ~(31 << shift)) | required << shift;
            }
        return (ushort)value;
    }

    // Only these nine channel values differ from the red-tint operation. Their
    // membership remains required content, not a rounding correction or exemption.
    private static bool IsRequiredHealthChannel(int band, int color, int channel) => (band, color, channel) is
        (5, 6, 2) or (6, 6, 1) or (3, 7, 2) or (5, 8, 1) or (6, 8, 2) or
        (6, 9, 1) or (6, 9, 2) or (0, 10, 2) or (0, 11, 1);
    /// <summary>
    /// $A7:CA41..CA60, Palette_Phantoon_FadeOutTarget: $A7:DBB1 fades every
    /// body palette component to black. Independent supplied target edits override it.
    /// </summary>
    public ushort ResolveFadeOut(int color)
    {
        if ((uint)color >= PhantoonColorRomData.FadeOutCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return fadeOutEdits.GetValueOrDefault(color);
    }
    public ushort ResolvePowerOn(int color) => Get(powerOn, color);

    public static PhantoonColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        PhantoonColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<PhantoonColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Phantoon color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Phantoon color JSON.", error);
        }
        if (document.Version != PhantoonColorFormat.Version)
            throw new InvalidDataException("Phantoon colors require the supported version.");
        if (document.HealthBands is null ||
            document.HealthBands.Length != PhantoonColorRomData.HealthBandCount)
            throw new InvalidDataException("Phantoon colors require eight health palettes.");
        return new(
            document.HealthBands.Select((band, index) =>
                Compile(band, PhantoonColorRomData.HealthBandColorCount,
                    $"health palette {index}")).ToArray(),
            Compile(document.FadeOut, PhantoonColorRomData.FadeOutCount, "fade-out"),
            Compile(document.PowerOn, PhantoonColorRomData.PowerOnCount, "power-on"));
    }

    public static byte[] Write(PhantoonColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static int CheckBand(int band) =>
        (uint)band < PhantoonColorRomData.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));

    private static ushort Get(ushort[] colors, int index) =>
        (uint)index < colors.Length
            ? colors[index]
            : throw new ArgumentOutOfRangeException(nameof(index));

    private static ushort[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Phantoon {name} requires {count} RGB5 colors.");
        var compiled = new ushort[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Phantoon {name} color {color} requires RGB5 channels 0..31.");
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
                        $"Duplicate Phantoon color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record PhantoonColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] HealthBands { get; init; }
    public required PaletteRgb5[] FadeOut { get; init; }
    public required PaletteRgb5[] PowerOn { get; init; }
}

public static class PhantoonColorFormat
{
    public const string FileName = "phantoon-colors.json";
    public const int Version = 1;
}
