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
            if (source == index && SamusFullBodyCycleColorFormat.TrySpeedBoosterTint(palette, color,
                palettes[palette / 16 * 16][color], out ushort tinted) && value == tinted) continue;
            if (source == index && SamusFullBodyCycleColorFormat.TrySpeedBoosterBrightening(palette, color,
                palettes[palette / 16 * 16 + 1][color], out ushort brightened) && value == brightened) continue;
            int channelSource = SamusFullBodyCycleColorFormat.SpeedSharedChannelSource(palette, color);
            if (source == index && channelSource >= 0 && value == SamusFullBodyCycleColorFormat.SpeedSharedChannelColor(
                palette, color, palettes[palette / 16 * 16][color], palettes[channelSource / 16][channelSource % 16])) continue;
            if (source == index && SamusFullBodyCycleColorFormat.IsActiveShineTint(palette, color) &&
                value == SamusFullBodyCycleColorFormat.ActiveShineTint(palettes[palette / 16 * 16 + 8][color], palette % 4)) continue;
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
        if (SamusFullBodyCycleColorFormat.IsActiveShineTint(palette, color))
            return SamusFullBodyCycleColorFormat.ActiveShineTint(ResolveIndex((palette / 16 * 16 + 8) * 16 + color), palette % 4);
        int channelSource = SamusFullBodyCycleColorFormat.SpeedSharedChannelSource(palette, color);
        if (channelSource >= 0) return SamusFullBodyCycleColorFormat.SpeedSharedChannelColor(
            palette, color, ResolveIndex(palette / 16 * 256 + color), ResolveIndex(channelSource));
        if (palette % 16 is 2 or 3 && SamusFullBodyCycleColorFormat.TrySpeedBoosterBrightening(palette, color,
            ResolveIndex((palette / 16 * 16 + 1) * 16 + color), out ushort brightened)) return brightened;
        if (SamusFullBodyCycleColorFormat.TrySpeedBoosterTint(palette, color,
            ResolveIndex(palette / 16 * 256 + color), out ushort tinted)) return tinted;
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

    /// <summary>Selects the47 canonical active-shinespark words that use the shared warm tint.</summary>
    internal static bool IsActiveShineTint(int palette, int color)
    {
        if ((uint)palette >= PaletteCount || palette % 16 is < 9 or > 11) return false;
        return (palette / 16) switch
        {
            0 => color is >= 3 and <= 7 or >= 13 and <= 15 || (color == 8 && palette != 9),
            1 => color == 9,
            _ => color is 1 or 2 or 9 or 10 or 11 or 12,
        };
    }

    /// <summary>Applies the active-shinespark warm tint, saturating each RGB5 channel.</summary>
    /// <remarks>Original rows9C40/9C60/9C80 and suit offsets512/1024
    /// share red/green offsets10/16/26 and blue offsets5/0/10. These
    /// three phase operations apply uniformly to47 opaque words selected by
    /// IsActiveShineTint. Phase0 is the unchanged supplied base. Power slot8's
    /// first shade is outside this mapping; its blue component needs separate review.
    /// Tint amounts describe shared phase operations, not per-color corrections.</remarks>
    internal static ushort ActiveShineTint(ushort basis, int shade)
    {
        var (warm, blue) = shade switch
        {
            0 => (0, 0),
            1 => (10, 5),
            2 => (16, 0),
            3 => (26, 10),
            _ => throw new ArgumentOutOfRangeException(nameof(shade)),
        };
        return (ushort)(Math.Min(31, (basis & 31) + warm) |
            Math.Min(31, (basis >> 5 & 31) + warm) << 5 |
            Math.Min(31, (basis >> 10 & 31) + blue) << 10);
    }

    /// <summary>Returns a shared Speed Booster channel source, or -1 outside the five-word mapping.</summary>
    /// <remarks>Power dim10/11 share dim2's blue and retain base red/green.
    /// Power dim9 is its base's standard dim tint; middle9 uses the middle
    /// tint's red/green with bright9's blue. Varia bright11 retains base red,
    /// adds five green and shares bright10's blue. Native words9B52/54/56,
    /// 9B72 and9D96 establish these relationships; source payloads remain editable.</remarks>
    internal static int SpeedSharedChannelSource(int palette, int color) => (palette, color) switch
    {
        (1, 9) => 9,
        (1, 10 or 11) => 16 + 2,
        (2, 9) => 3 * 16 + 9,
        (19, 11) => 19 * 16 + 10,
        _ => -1,
    };

    /// <summary>Combines the reviewed base tint with the explicitly selected shared blue channel.</summary>
    internal static ushort SpeedSharedChannelColor(int palette, int color, ushort basis, ushort shared)
    {
        if (SpeedSharedChannelSource(palette, color) < 0) throw new ArgumentOutOfRangeException(nameof(palette));
        if (palette == 1 && color == 9) return LoadingPaletteColorDefinitions.TintColor(basis, 2);
        ushort tint = palette == 2 ? LoadingPaletteColorDefinitions.TintColor(basis, 1) :
            palette == 19 ? LoadingPaletteColorDefinitions.VariaTintColor(basis, 0) : basis;
        return (ushort)((tint & 0x03ff) | (shared & 0x7c00));
    }

    /// <summary>Brightens seven independently supplied dim Speed Booster inks.</summary>
    /// <remarks>Original Power slots1/2/10/11/12, Varia12 and Gravity2
    /// in the middle/bright Speed Booster rows derive from their dim row.
    /// Power1/12 and Gravity2 add green5/15 and blue10/10; Power2 and
    /// Varia12 add green0/0 and blue5/10; Power10/11 add green0/5 and
    /// blue5/10. Red is unchanged and sums saturate at31. Reuse the loading
    /// endpoint brightening formula while preserving separate full-body inputs.</remarks>
    internal static bool TrySpeedBoosterBrightening(int palette, int color, ushort dim, out ushort value)
    {
        value = 0;
        if ((uint)palette >= PaletteCount || color is < 1 or > 15 || palette % 16 is not (2 or 3)) return false;
        int suit = palette / 16;
        if (!(suit == 0 ? color is 1 or 2 or 10 or 11 or 12 : suit == 1 ? color == 12 : color == 2)) return false;
        bool plateau = suit == 2 || (suit == 0 && color is 1 or 12);
        int greenPeak = plateau ? 15 : suit == 0 && color is 10 or 11 ? 5 : 0;
        value = LoadingPaletteColorDefinitions.BrightenDimColor(dim, 3 - palette % 4, greenPeak, plateau);
        return true;
    }

    /// <summary>Calculates the base-derived Speed Booster blue/green tints.</summary>
    /// <remarks>Original Power slots3..8/13..15 and Gravity10/11 in
    /// $9B:9B40..9B9F/$9F40..9F9F use the same saturating tint as loading:
    /// red unchanged, green+0/5/15 and blue+10/20/20 for dim/middle/bright.
    /// Varia1/2 use green+0/0/5, blue+10/20/30; Varia10/11 use only
    /// the first two levels. The bright Varia10/11 endpoint remains an input.
    /// These43 canonical words reuse the reviewed loading tint arithmetic;
    /// loading and full-body assets retain independent input ownership.</remarks>
    internal static bool TrySpeedBoosterTint(int palette, int color, ushort basis, out ushort value)
    {
        value = 0;
        if ((uint)palette >= PaletteCount || color is < 1 or > 15 || palette % 16 is < 1 or > 3) return false;
        int suit = palette / 16, shade = palette % 4;
        bool included = suit switch
        {
            0 => color is >= 3 and <= 8 or >= 13 and <= 15,
            1 => color is 1 or 2 || (shade < 3 && color is 10 or 11),
            _ => color is 10 or 11,
        };
        if (!included) return false;
        value = suit == 1 ? LoadingPaletteColorDefinitions.VariaTintColor(basis, 3 - shade) :
            LoadingPaletteColorDefinitions.TintColor(basis, 3 - shade);
        return true;
    }

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
