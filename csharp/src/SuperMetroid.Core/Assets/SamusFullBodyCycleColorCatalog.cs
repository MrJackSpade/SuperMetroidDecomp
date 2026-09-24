using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Authored Samus full-body colors keyed by the already-compiled native palette pointers.
/// The catalog never chooses a suit, cycle phase, or animation delay.
/// </summary>
public sealed class SamusFullBodyCycleColorCatalog
{
    private readonly Dictionary<ushort, ushort[]> colorsByPointer;

    private SamusFullBodyCycleColorCatalog(Dictionary<ushort, ushort[]> colorsByPointer) =>
        this.colorsByPointer = colorsByPointer;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Returns one BGR555 color at a compiled bank-$9B palette pointer.</summary>
    public ushort Resolve(ushort pointer, int colorIndex)
    {
        if (!colorsByPointer.TryGetValue(pointer, out ushort[]? palette))
            throw new ArgumentOutOfRangeException(nameof(pointer),
                $"Uncatalogued full-body palette pointer ${pointer:X4}.");
        if ((uint)colorIndex >= SamusFullBodyCycleColorFormat.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        return palette[colorIndex];
    }

    /// <summary>Copies sixteen display colors to Samus OBJ palette four.</summary>
    public void Apply(SnesCgram cgram, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int index = 0; index < SamusFullBodyCycleColorFormat.ColorsPerPalette; index++)
            cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + index,
                Resolve(pointer, index));
    }

    public static SamusFullBodyCycleColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        SamusFullBodyCycleColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SamusFullBodyCycleColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Samus full-body cycle color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Samus full-body cycle color JSON.", error);
        }
        if (document.Version != SamusFullBodyCycleColorFormat.Version)
            throw new InvalidDataException("Samus full-body cycle colors require the supported version.");

        var palettes = new Dictionary<ushort, ushort[]>();
        AddFamily(palettes, SamusFullBodyCycleFamily.SpeedBooster, document.SpeedBooster);
        AddFamily(palettes, SamusFullBodyCycleFamily.ScrewAttack, document.ScrewAttack);
        AddFamily(palettes, SamusFullBodyCycleFamily.StoredShine, document.StoredShine);
        AddFamily(palettes, SamusFullBodyCycleFamily.ActiveShinespark, document.ActiveShinespark);
        return new(palettes);
    }

    public static byte[] Write(SamusFullBodyCycleColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static void AddFamily(Dictionary<ushort, ushort[]> destination,
        SamusFullBodyCycleFamily family, PaletteRgb5[][][]? source)
    {
        if (source is null || source.Length != SamusFullBodyCycleColorFormat.SuitCount)
            throw new InvalidDataException($"{family} requires three suit palettes.");
        for (int suit = 0; suit < source.Length; suit++)
        {
            PaletteRgb5[][]? shades = source[suit];
            if (shades is null || shades.Length != SamusFullBodyCycleColorFormat.ShadesPerSuit)
                throw new InvalidDataException($"{family} suit {suit} requires four shades.");
            for (int shade = 0; shade < shades.Length; shade++)
            {
                PaletteRgb5[]? color = shades[shade];
                if (color is null || color.Length != SamusFullBodyCycleColorFormat.ColorsPerPalette)
                    throw new InvalidDataException($"{family} suit {suit}, shade {shade} requires sixteen colors.");
                var words = new ushort[color.Length];
                for (int index = 0; index < color.Length; index++)
                {
                    PaletteRgb5? rgb = color[index];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                        (uint)rgb.Blue > 31)
                        throw new InvalidDataException(
                            $"{family} suit {suit}, shade {shade}, color {index} requires RGB5 channels 0..31.");
                    words[index] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                }
                ushort pointer = SamusFullBodyCycleColorFormat.Pointer(family, suit, shade);
                if (!destination.TryAdd(pointer, words))
                    throw new InvalidDataException($"Duplicate full-body palette pointer ${pointer:X4}.");
            }
        }
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate full-body color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public enum SamusFullBodyCycleFamily : byte
{
    SpeedBooster,
    ScrewAttack,
    StoredShine,
    ActiveShinespark,
}

public sealed record SamusFullBodyCycleColorDocument
{
    public required int Version { get; init; }
    /// <summary>Suit order: Power, Varia, Gravity; each contains four 16-color shades.</summary>
    public required PaletteRgb5[][][] SpeedBooster { get; init; }
    public required PaletteRgb5[][][] ScrewAttack { get; init; }
    public required PaletteRgb5[][][] StoredShine { get; init; }
    public required PaletteRgb5[][][] ActiveShinespark { get; init; }
}

public static class SamusFullBodyCycleColorFormat
{
    public const string FileName = "samus-full-body-cycle-colors.json";
    public const int Version = 1;
    public const int SuitCount = 3;
    public const int ShadesPerSuit = 4;
    public const int ColorsPerPalette = SamusPaletteRomData.Common.ColorsPerObjPalette;

    /// <summary>Uses the cartridge-verified, bounded selector for each distinct shade.</summary>
    public static ushort Pointer(SamusFullBodyCycleFamily family, int suit, int shade)
    {
        if ((uint)suit >= SuitCount || (uint)shade >= ShadesPerSuit)
            throw new ArgumentOutOfRangeException(nameof(suit), "Suit and shade must be catalogued.");
        ushort suitOffset = (ushort)(suit * 2);
        ushort shadeOffset = (ushort)(shade * 2);
        ushort pointer;
        bool found = family switch
        {
            SamusFullBodyCycleFamily.SpeedBooster =>
                SamusPaletteRomData.FullBodyCycles.TryActiveSpeedBoosterPalettePointer(
                    suitOffset, shadeOffset, out pointer),
            SamusFullBodyCycleFamily.ScrewAttack =>
                SamusPaletteRomData.FullBodyCycles.TryScrewAttackPalettePointer(
                    suitOffset, shadeOffset, out pointer),
            SamusFullBodyCycleFamily.StoredShine =>
                SamusPaletteRomData.FullBodyCycles.TryStoredShinePalettePointer(
                    suitOffset, shadeOffset, out pointer),
            SamusFullBodyCycleFamily.ActiveShinespark =>
                SamusPaletteRomData.FullBodyCycles.TryActiveShinesparkPalettePointer(
                    suitOffset, shadeOffset, out pointer),
            _ => throw new ArgumentOutOfRangeException(nameof(family)),
        };
        if (!found)
            throw new InvalidDataException($"Uncatalogued {family} suit {suit}, shade {shade}.");
        return pointer;
    }
}
