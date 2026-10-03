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
    private readonly Dictionary<int, ushort> colors;

    private SamusFullBodyCycleColorCatalog(ushort[][] palettes)
    {
        colors = new Dictionary<int, ushort>();
        for (int palette = 0; palette < palettes.Length; palette++)
        for (int color = 0; color < SamusFullBodyCycleColorFormat.ColorsPerPalette; color++)
        {
            int index = palette * SamusFullBodyCycleColorFormat.ColorsPerPalette + color;
            int source = SamusFullBodyCycleColorFormat.CanonicalColorIndex(palette, color);
            ushort value = palettes[palette][color];
            if (source != index && value == palettes[source / 16][source % 16]) continue;
            if (source == index && SamusFullBodyCycleColorFormat.IsStoredShineShade(palette, color) &&
                value == SamusFullBodyCycleColorFormat.StoredShineColor(palettes[palette / 16 * 16 + 4][color], palette % 4)) continue;
            colors.Add(index, value);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Returns one BGR555 color at a compiled bank-$9B palette pointer.</summary>
    public ushort Resolve(ushort pointer, int colorIndex)
    {
        int palette = SamusFullBodyCycleColorFormat.PaletteIndex(pointer);
        if ((uint)colorIndex >= SamusFullBodyCycleColorFormat.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        int index = palette * SamusFullBodyCycleColorFormat.ColorsPerPalette + colorIndex;
        return ResolveIndex(index);
    }

    private ushort ResolveIndex(int index)
    {
        if (colors.TryGetValue(index, out ushort value)) return value;
        int palette = index / 16, color = index % 16;
        int source = SamusFullBodyCycleColorFormat.CanonicalColorIndex(palette, color);
        if (source != index) return ResolveIndex(source);
        return SamusFullBodyCycleColorFormat.StoredShineColor(
            ResolveIndex((palette / 16 * 16 + 4) * 16 + color), palette % 4);
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

        var palettes = new ushort[SamusFullBodyCycleColorFormat.PaletteCount][];
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

    private static void AddFamily(ushort[][] destination,
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
                int paletteIndex = SamusFullBodyCycleColorFormat.PaletteIndex(pointer);
                if (destination[paletteIndex] is not null)
                    throw new InvalidDataException($"Duplicate full-body palette pointer ${pointer:X4}.");
                destination[paletteIndex] = words;
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
    /// <summary>Four distinct shade palettes in each of four families for three suits.</summary>
    public const int PaletteCount = SuitCount * ShadesPerSuit * 4;

    /// <summary>Stored-shine rows5..7 within each sixteen-row suit allocation, opaque colors only.</summary>
    internal static bool IsStoredShineShade(int palette, int color) =>
        (uint)palette < PaletteCount && color is >= 1 and < 16 && palette % 16 is >= 5 and <= 7;

    /// <summary>Interpolates each RGB5 channel toward white by shade/4, rounding down.</summary>
    /// <remarks>All opaque words of $9B:9BA0..9C1F and the Varia/Gravity
    /// blocks512/1024 bytes later obey floor(((4-shade)*base+31*shade)/4).
    /// Shade0 is the supplied base, shades1..3 are quarter steps toward white.
    /// Arithmetic is nonnegative and bounded by124 per channel; no saturation,
    /// wrapping or cross-channel carry is involved. Differing asset edits remain inputs.</remarks>
    internal static ushort StoredShineColor(ushort basis, int shade)
    {
        if (basis > 0x7fff) throw new ArgumentOutOfRangeException(nameof(basis));
        if ((uint)shade >= 4) throw new ArgumentOutOfRangeException(nameof(shade));
        int red = ((4 - shade) * (basis & 31) + 31 * shade) / 4;
        int green = ((4 - shade) * (basis >> 5 & 31) + 31 * shade) / 4;
        int blue = ((4 - shade) * (basis >> 10) + 31 * shade) / 4;
        return (ushort)(red | green << 5 | blue << 10);
    }

    /// <summary>Shares each family's opaque base row with its suit's Speed Booster base.</summary>
    /// <remarks>Original $9B:9B20/9BA0/9C20/9CA0 base rows agree at all15
    /// opaque slots, and the same holds512/1024 bytes later for Varia/Gravity.
    /// Every fourth row is a family base; each suit occupies16 rows. Therefore
    /// a base-row opaque color resolves to row16*(palette/16), same color.
    /// Transparent entries share four source payloads: Speed Booster's middle
    /// shades share Power shade1; other Speed Booster and stored-shine entries
    /// share Power shade0, except Gravity Speed Booster shade0's distinct
    /// payload. All active-shine/Screw Attack entries share Power active-shine
    /// shade0. The payload values remain inputs, not encoded constants.
    /// Opaque Varia/Gravity rows share Power's same shade except the suit ink
    /// slots selected below: rows1..3 are Speed Booster, rows5..7 stored shine,
    /// rows9..11 active shinespark, and rows13..15 Screw Attack. These are
    /// categorical sprite-ink selections, established from all original rows;
    /// they do not exempt the remaining shade/component inputs from derivation.
    /// Explicit differing asset values override this alias, including base edits.</remarks>
    internal static int CanonicalColorIndex(int palette, int color)
    {
        if ((uint)palette >= PaletteCount) throw new ArgumentOutOfRangeException(nameof(palette));
        if ((uint)color >= ColorsPerPalette) throw new ArgumentOutOfRangeException(nameof(color));
        if (color == 0)
        {
            if (palette == 32) return palette * ColorsPerPalette;
            int row = palette % 16;
            int transparentSource = row >= 8 ? 8 : row is 1 or 2 ? 1 : 0;
            return transparentSource * ColorsPerPalette;
        }
        int source = palette % ShadesPerSuit == 0 ? palette / 16 * 16 : palette;
        int suit = source / 16;
        int familyRow = source % 16;
        // Suit recoloring changes only these ink slots; all other inks use Power's row.
        bool suitInk = (suit, familyRow) switch
        {
            (0, _) => true,
            (1, >= 1 and <= 3) => color is 1 or 2 or 10 or 11 or 12,
            (2, >= 1 and <= 3) => color is 2 or 10 or 11,
            (1, 9) => color is 2 or 9 or 10 or 11 or 12,
            (_, 10 or 11) or (2, 9) => color is 1 or 2 or 9 or 10 or 11 or 12,
            (1, >= 13) => color is 2 or 10 or 11 or 12,
            (2, >= 13) => color is 1 or 2 or 10 or 11,
            (1, _) => color is 2 or 10 or 11,
            _ => color is 1 or 2 or 10 or 11 or 12,
        };
        if (!suitInk) source = familyRow;
        return source * ColorsPerPalette + color;
    }

    /// <summary>Maps an original full-body palette identity to its contiguous artwork row.</summary>
    /// <remarks>Native bank91 lists select all48 aligned32-byte records in
    /// $9B:9B20..A11F: speed boost, stored shine, active shine and Screw Attack
    /// occupy consecutive128-byte families, repeated every512 bytes per suit.
    /// Index=(pointer-$9B20)/32, requiring exact alignment and index0..47.
    /// This replaces generated pointer dictionary entries, not color artwork.</remarks>
    internal static int PaletteIndex(ushort pointer)
    {
        int offset = pointer - SamusPaletteRomData.FullBodyCycles.SpeedBoosterFirstPalette;
        if ((uint)offset >= PaletteCount * ColorsPerPalette * sizeof(ushort) ||
            offset % (ColorsPerPalette * sizeof(ushort)) != 0)
            throw new ArgumentOutOfRangeException(nameof(pointer), $"Uncatalogued full-body palette pointer ${pointer:X4}.");
        return offset / (ColorsPerPalette * sizeof(ushort));
    }

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
