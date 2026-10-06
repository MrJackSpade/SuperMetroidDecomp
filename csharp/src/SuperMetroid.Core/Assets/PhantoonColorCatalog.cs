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
    // and the eight native target deviations below. None has a retention exemption.
    private readonly ushort[] healthyColors;
    private readonly Dictionary<int, ushort> requiredHealthColors = [];
    private readonly Dictionary<int, ushort> healthEdits = [];
    private const int MinimumTintWeight = 8;
    private const int TintDenominator = 15;
    private readonly Dictionary<int, ushort> fadeOutEdits = [];
    private readonly ushort[] powerOn;

    private PhantoonColorCatalog(ushort[][] healthBands, ushort[] fadeOut, ushort[] powerOn)
    {
        healthyColors = healthBands[^1];
        for (int band = 0; band < healthBands.Length - 1; band++)
        for (int color = 0; color < healthyColors.Length; color++)
        {
            int key = band * healthyColors.Length + color;
            if (IsRequiredHealthChoice(band, color)) requiredHealthColors.Add(key, healthBands[band][color]);
            else if (healthBands[band][color] != TintHealthColor(band, color)) healthEdits.Add(key, healthBands[band][color]);
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
    /// interpolates healthy RGB toward saturated red as health decreases. Eight
    /// independently chosen deviations remain explicit required inputs.
    /// </summary>
    public ushort ResolveHealth(int band, int color)
    {
        _ = CheckBand(band);
        _ = Get(healthyColors, color);
        int key = band * healthyColors.Length + color;
        if (requiredHealthColors.TryGetValue(key, out ushort required)) return required;
        return healthEdits.TryGetValue(key, out ushort edit) ? edit : TintHealthColor(band, color);
    }

    private ushort TintHealthColor(int band, int color)
    {
        ushort healthy = healthyColors[color];
        int weight = MinimumTintWeight + band;
        int red = (31 * TintDenominator + ((healthy & 31) - 31) * weight) / TintDenominator;
        int green = ((healthy >> 5) & 31) * weight / TintDenominator;
        int blue = (healthy >> 10) * weight / TintDenominator;
        return (ushort)(red | green << 5 | blue << 10);
    }

    // These exact native words differ from the shared red-tint operation. Membership
    // is required source content, not a computed rounding correction or exemption.
    private static bool IsRequiredHealthChoice(int band, int color) => (band, color) is
        (5, 6) or (6, 6) or (3, 7) or (5, 8) or (6, 8) or (6, 9) or (0, 10) or (0, 11);
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
