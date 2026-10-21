using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Defines the document identity and fixed native full-body palette geometry.</summary>
public static class SamusFullBodyCycleColorFormat
{
    /// <summary>JSON filename containing all Samus full-body cycle colors.</summary>
    public const string FileName = "samus-full-body-cycle-colors.json";
    /// <summary>Supported full-body cycle color schema revision.</summary>
    public const int Version = 1;
    /// <summary>Number of supported suit variants: Power, Varia, and Gravity.</summary>
    public const int SuitCount = 3;
    /// <summary>Number of ordered palette shades in each family and suit.</summary>
    public const int ShadesPerSuit = 4;
    /// <summary>Number of RGB5 entries in each complete Samus OBJ palette.</summary>
    public const int ColorsPerPalette = SamusPaletteRomData.Common.ColorsPerObjPalette;
    /// <summary>Four distinct shade palettes in each of four families for three suits.</summary>
    public const int PaletteCount = SuitCount * ShadesPerSuit * 4;

    /// <summary>Varia Screw slot2 interpolates red across dim/middle/bright, keeping raised green and base blue.</summary>
    /// <remarks>Original9EC4/9EE4/9F04 have red31/26/20: endpoints
    ///31/20 and their midpoint rounded upward. Green is base+5 saturated31,
    /// blue is base blue. Bright red remains an independent input; midpoint
    /// arithmetic uses nonnegative RGB5 values and numerator at most63.</remarks>
    internal static bool TryVariaScrewInk(int palette, int color, Bgr555 basis, Bgr555 bright, out Bgr555 value)
    {
        value = Bgr555.Black;
        if (palette is < 29 or > 31 || color != 2) return false;
        int red = palette == 29 ? basis.Red : palette == 31 ? bright.Red : ((basis.Red) + (bright.Red) + 1) / 2;
        value = new(red, Math.Min(31, basis.Green + 5), basis.Blue);
        return true;
    }

    /// <summary>Combines Power's base blue with its dim Speed Booster red/green for Gravity base slots1/12.</summary>
    /// <remarks>Original $9B:9F22/9F38 share red/green with9B42/9B58
    /// and blue with9B22/9B38. These are repeated channels of the same suit
    /// inks, not newly encoded color constants. Differing supplied Gravity
    /// words remain explicit overrides, including when Power sources change.</remarks>
    internal static Bgr555 GravitySharedBase(Bgr555 powerBase, Bgr555 powerDim) =>
        powerDim.WithBlue(powerBase.Blue);

    /// <summary>Calculates Power Screw Attack's descending-red ink and shared gold ink.</summary>
    /// <remarks>Original9CC4/9CE4/9D04 subtract five red and add five green
    /// per shade from base9CA4, preserving blue. Original9CD8/9CF8/9D18 use
    /// Speed Booster dim slot12's red/green (9B58), with green+10 per shade
    /// and the Screw base's blue plus10 at the final shade. RGB5 additions
    /// saturate31 and red subtraction clamps0. Independent source/shade edits
    /// remain explicit inputs; endpoint choices have the owning catalog disposition.</remarks>
    internal static bool TryScrewPowerInk(int palette, int color, Bgr555 basis, Bgr555 speedDim, out Bgr555 value)
    {
        value = Bgr555.Black;
        if (palette is < 13 or > 15 || color is not (2 or 12)) return false;
        int shade = palette - 12;
        int red = color == 2 ? Math.Max(0, (basis.Red) - 5 * shade) : speedDim.Red;
        int green = Math.Min(31, (color == 2 ? basis : speedDim).Green + (color == 2 ? 5 : 10) * shade);
        int blue = Math.Min(31, (basis.Blue) + (color == 12 && shade == 3 ? 10 : 0));
        value = new Bgr555(red, green, blue);
        return true;
    }

    /// <summary>Calculates Screw Attack green/blue ramps before explicit endpoint channel overrides.</summary>
    /// <remarks>Power3..9/13..15 and Gravity1/2/10/11 add green10 per
    /// shade and blue10 only at shade3; Power15 shade3 retains its
    /// differing green. Power1/11 and Varia10/11/12 add green5 per shade
    /// with unchanged blue. Power10 uses that slower ramp, with its final red stored independently.
    /// Red stays unchanged and channel additions saturate at31. Original rows
    ///9CC0/9CE0/9D00 and suit offsets512/1024 establish these shared operations.
    /// Power slot4 blue ramps through quarter/half/full of the same ten-unit increment:
    /// floor(10/2^(3-shade)), producing2/5/10 before saturation. Other
    /// slots/components use the other catalog formulas or its explicit input dispositions.</remarks>
    internal static bool TryScrewAttackTint(int palette, int color, Bgr555 basis, out Bgr555 value)
    {
        value = Bgr555.Black;
        if ((uint)palette >= PaletteCount || palette % 16 < 13) return false;
        int suit = palette / 16, shade = palette % 4;
        bool fast = suit == 0 ? color is >= 3 and <= 9 or >= 13 and <= 15 :
            suit == 2 && color is 1 or 2 or 10 or 11;
        bool slow = suit == 0 ? color is 1 or 10 or 11 :
            suit == 1 && color is 10 or 11 or 12;
        if (!fast && !slow) return false;
        int green = Math.Min(31, (basis.Green) + (fast ? 10 : 5) * shade);
        int blueAdd = suit == 0 && color == 4 ? 10 >> (3 - shade) : fast && shade == 3 ? 10 : 0;
        int blue = Math.Min(31, (basis.Blue) + blueAdd);
        value = new(basis.Red, green, blue);
        return true;
    }

    /// <summary>Calculates active-shinespark gold ramps while preserving base blue before explicit channel overrides.</summary>
    /// <remarks>Power slots1/9/11/12 use the stored-shine quarter-white
    /// interpolation on red/green only. Power10 adds three per shade to both
    /// channels. Varia1/11/12 add five per shade to red/green; Varia10 changes only green. All sums saturate at31.
    /// Original rows9C40/60/80 and9E40/60/80 establish the mapping;
    /// Power/Varia slot2 also add five red/green per shade; their differing blue components remain explicit. Other slots and Gravity are outside this domain.</remarks>
    internal static bool TryActiveGoldRamp(int palette, int color, Bgr555 basis, out Bgr555 value)
    {
        value = Bgr555.Black;
        if ((uint)palette >= PaletteCount || palette % 16 is < 9 or > 11) return false;
        int suit = palette / 16, shade = palette % 4;
        if (suit == 0 && color is 1 or 9 or 11 or 12)
        {
            value = StoredShineColor(basis, shade).WithBlue(basis.Blue);
            return true;
        }
        int step = suit == 0 && color == 10 ? 3 :
            (suit is 0 or 1 && color == 2) || (suit == 1 && color is 1 or 10 or 11 or 12) ? 5 : 0;
        if (step == 0) return false;
        value = new(Math.Min(31, basis.Red + (suit == 1 && color == 10 ? 0 : step * shade)),
            Math.Min(31, basis.Green + step * shade), basis.Blue);
        return true;
    }

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
    /// first shade uses the same red/green operation with the catalogued blue input.
    /// Tint amounts describe shared phase operations, not per-color corrections.</remarks>
    internal static Bgr555 ActiveShineTint(Bgr555 basis, int shade)
    {
        var (warm, blue) = shade switch
        {
            0 => new ShineTint(0, 0),
            1 => new ShineTint(10, 5),
            2 => new ShineTint(16, 0),
            3 => new ShineTint(26, 10),
            _ => throw new ArgumentOutOfRangeException(nameof(shade)),
        };
        return new(Math.Min(31, basis.Red + warm),
            Math.Min(31, basis.Green + warm),
            Math.Min(31, basis.Blue + blue));
    }

    /// <summary>Returns a shared Speed Booster channel source, or -1 outside the five-word mapping.</summary>
    /// <remarks>Power dim10/11 share dim2's blue and retain base red/green.
    /// Power dim9 is its base's standard dim tint; middle9 uses the middle
    /// tint's red/green with bright9's blue. Varia bright11 retains base red,
    /// adds five green and shares bright10's blue. Native words9B52/54/56,
    /// 9B72 and9D96 establish these relationships; source payloads remain editable.</remarks>
    internal static int SpeedSharedChannelSource(int palette, int color) => new SpeedPaletteColor(palette, color) switch
    {
        (1, 9) => 9,
        (1, 10 or 11) => 16 + 2,
        (2, 9) => 3 * 16 + 9,
        (19, 11) => 19 * 16 + 10,
        _ => -1,
    };

    /// <summary>Combines the reviewed base tint with the explicitly selected shared blue channel.</summary>
    internal static Bgr555 SpeedSharedChannelColor(int palette, int color, Bgr555 basis, Bgr555 shared)
    {
        if (SpeedSharedChannelSource(palette, color) < 0) throw new ArgumentOutOfRangeException(nameof(palette));
        if (palette == 1 && color == 9) return LoadingPaletteColorDefinitions.TintColor(basis, 2);
        Bgr555 tint = palette == 2 ? LoadingPaletteColorDefinitions.TintColor(basis, 1) :
            palette == 19 ? LoadingPaletteColorDefinitions.VariaTintColor(basis, 0) : basis;
        return tint.WithBlue(shared.Blue);
    }

    /// <summary>Brightens seven independently supplied dim Speed Booster inks.</summary>
    /// <remarks>Original Power slots1/2/10/11/12, Varia12 and Gravity2
    /// in the middle/bright Speed Booster rows derive from their dim row.
    /// Power1/12 and Gravity2 add green5/15 and blue10/10; Power2 and
    /// Varia12 add green0/0 and blue5/10; Power10/11 add green0/5 and
    /// blue5/10. Red is unchanged and sums saturate at31. Reuse the loading
    /// endpoint brightening formula while preserving separate full-body inputs.</remarks>
    internal static bool TrySpeedBoosterBrightening(int palette, int color, Bgr555 dim, out Bgr555 value)
    {
        value = Bgr555.Black;
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
    internal static bool TrySpeedBoosterTint(int palette, int color, Bgr555 basis, out Bgr555 value)
    {
        value = Bgr555.Black;
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
    internal static Bgr555 StoredShineColor(Bgr555 basis, int shade)
    {
        if ((uint)shade >= 4) throw new ArgumentOutOfRangeException(nameof(shade));
        int red = ((4 - shade) * (basis.Red) + 31 * shade) / 4;
        int green = ((4 - shade) * (basis.Green) + 31 * shade) / 4;
        int blue = ((4 - shade) * (basis.Blue) + 31 * shade) / 4;
        return new Bgr555(red, green, blue);
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
    /// the remaining shade/component relationships use the separate calculations below.
    /// Varia active-shine bright slot2 shares Power middle slot2 (original
    ///9E84/9C64). Explicit differing asset values override every alias.</remarks>
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
        if (palette == 27 && color == 2) return 10 * 16 + 2;
        int source = palette % ShadesPerSuit == 0 ? palette / 16 * 16 : palette;
        int suit = source / 16;
        int familyRow = source % 16;
        // Suit recoloring changes only these ink slots; all other inks use Power's row.
        bool suitInk = new SuitInkKey(suit, familyRow) switch
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

    /// <summary>The contiguous artwork row of a cycle palette.</summary>
    /// <remarks>Each suit owns sixteen consecutive rows, Power then Varia then Gravity: four Speed Booster shades, then
    /// stored shine, active shinespark and Screw Attack, the order the color formulas index.</remarks>
    internal static int PaletteIndex(SamusFullBodyCyclePalette palette)
    {
        if ((uint)palette.Suit >= SuitCount || (uint)palette.Shade >= ShadesPerSuit)
            throw new ArgumentOutOfRangeException(nameof(palette), $"Uncatalogued full-body palette {palette}.");
        int family = palette.Family switch
        {
            SamusFullBodyCycleFamily.SpeedBooster => 0,
            SamusFullBodyCycleFamily.StoredShine => 1,
            SamusFullBodyCycleFamily.ActiveShinespark => 2,
            SamusFullBodyCycleFamily.ScrewAttack => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(palette), $"Uncatalogued full-body palette family {palette.Family}."),
        };
        return palette.Suit * (PaletteCount / SuitCount) + family * ShadesPerSuit + palette.Shade;
    }

}
