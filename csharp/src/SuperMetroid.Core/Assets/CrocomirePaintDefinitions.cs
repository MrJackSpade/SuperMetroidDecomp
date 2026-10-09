namespace SuperMetroid.Core.Assets;

/// <summary>
/// Selected Crocomire material inks at $A4:B89D-B93C. These choices paint the red
/// hide, bone, masonry, projectile and spike illustrations; they are not samples
/// of movement, damage or health. Shared channels, shade ramps and aliases calculate.
/// No complete stock palette is stored. Independently supplied inks belong to the catalog.
/// </summary>
/// <remarks>
/// Native $A4:8A99-8AA9 copies the wall/projectile targets, $8CEF-8CF8 restores
/// body inks, $9870-9879 copies spike colors and $999E-99A8 copies arm colors.
/// These consumers install selected pen colors without deriving them from a
/// physical state. The residual named hues/intensities and material assignments
/// are the illustration content itself: replacing them with a generated color
/// rule would invent different paint. This narrow content boundary also accounts
/// for uniformly copied accent inks not selected by the representative poses;
/// absence from those poses is not an exemption. All demonstrated interpolation,
/// channel equality, tint, neutral, saturation and alias relations calculate.
/// Movement, hurt-flash cadence, collision, pixels and unrelated palettes are excluded.
/// </remarks>
internal static class CrocomirePaintDefinitions
{
    /// <summary>$A4:B8BD/B8DD/B8FD/B91D: copied OBJ ink-zero metadata $3800; OBJ rendering skips ink zero.</summary>
    private const int TransparentBlue = 14;
    /// <summary>$A4:B8A1: saturated red hide highlight with selected warm green and blue channels.</summary>
    private const int HideHighlightGreen = 15, HideHighlightBlue = 3;
    /// <summary>$A4:B8A5-B8A9: selected darker red hide intensities; green falls one level per contour shade.</summary>
    private const int HideMiddleRed = 21, HideDeepRed = 12, HideOutlineRed = 7;
    /// <summary>$A4:B8A3-B8A9: warm-contour green peak and capped half-green blue contribution.</summary>
    private const int HideGreenPeak = 5, HideBlueCap = 2;
    /// <summary>$A4:B8AB/B901: lightest selected bone paint, also used by the body teeth.</summary>
    private const int BoneLightRed = 30, BoneLightGreen = 27, BoneLightBlue = 26;
    /// <summary>$A4:B903-B909: four bone shades use nearest red interpolation to this darkest intensity.</summary>
    private const int BoneDarkRed = 11;
    /// <summary>$A4:B903-B909: bone green is red minus the warm tint, clipped at its selected darkest green.</summary>
    private const int BoneGreenTint = 6, BoneGreenFloor = 6;
    /// <summary>$A4:B903-B909: selected blue channels of the four differently warm bone facets.</summary>
    private const int BoneBlueLight = 21, BoneBlueMiddle = 14, BoneBlueDeep = 8, BoneBlueDark = 3;
    /// <summary>$A4:B915-B919: copied gold accents shared with the original body OBJ palette, outside the eight-word fight field.</summary>
    private const int BoneGoldLightGreen = 28, BoneGoldMiddleGreen = 17,
        BoneGoldDarkRed = 22, BoneGoldDarkGreen = 11;
    /// <summary>$A4:B8C1-B8C9: repeated neutral masonry paint.</summary>
    private const int WallNeutral = 24;
    /// <summary>$A4:B8CB-B8D3: independently chosen masonry face, recess and edge paints.</summary>
    private const ushort WallFace = 0x4a7b, WallRecess = 0x1c90, WallEdge = 0x1469, WallDeepEdge = 0x1424;
    /// <summary>$A4:B8D3: selected red-only deepest masonry mark.</summary>
    private const int WallRedMark = 8;
    /// <summary>$A4:B8D5-B8D9: masonry shares hide red/green contour shading but selects violet-blue tint levels.</summary>
    private const int WallTintLight = 9, WallTintDeep = 7;
    /// <summary>$A4:B8DB: independent darkest wall paint, including its selected red-channel endpoint.</summary>
    private const ushort WallDark = 0x1045;
    /// <summary>$A4:B8DF: full-blue projectile glint with equal selected red and green.</summary>
    private const int ProjectileGlint = 26;
    /// <summary>$A4:B8E1-B8E5: yellow/orange projectile core endpoints; red uses floor gamma-two interpolation and green floor linear interpolation. Blue is absent.</summary>
    private const int FlameLightRed = 27, FlameLightGreen = 25, FlameDarkRed = 19, FlameDarkGreen = 8;
    /// <summary>$A4:B8E7-B8EB: violet projectile rim runs from full blue to its chosen darkest red with a shared blue excess.</summary>
    private const int VioletDarkRed = 10, VioletBlueTint = 2;
    /// <summary>$A4:B8ED-B8F1: green projectile endpoints; floor gamma-two interpolation supplies the middle intensity and blue scales proportionally from its highlight.</summary>
    private const int GreenDark = 12, GreenBluePeak = 14;
    /// <summary>$A4:B8F3-B8F7: linear neutral projectile ramp endpoints.</summary>
    private const int ProjectileNeutralLight = 20, ProjectileNeutralDark = 8;
    /// <summary>$A4:B8F9: dim violet outline has half the neutral shadow intensity and this blue excess.</summary>
    private const int ProjectileOutlineBlueTint = 5;
    /// <summary>$A4:B91F-B923: gold spike mounting shades, with a selected middle red level and green endpoint interpolation.</summary>
    private const int SpikeGoldMiddleRed = 23, SpikeGoldDarkRed = 12,
        SpikeGoldLightGreen = 22, SpikeGoldDarkGreen = 5;
    /// <summary>$A4:B925-B92B: four cool steel shades interpolate downward with floor rounding and a shared blue excess.</summary>
    private const int SteelLight = 19, SteelDark = 3, SteelBlueTint = 3;
    /// <summary>$A4:B92D/B92F/B937: three additional copied spike-palette accent paints; no visible-role claim is made for this fragment fixture.</summary>
    private const ushort SpikeAccent = 0x0bb1, SpikeWarmAccent = 0x48fb, SpikeCoolAccent = 0x44e5;

    /// <summary>Maps a four-bit body-palette index to the selected Crocomire fight-body color.</summary>
    internal static ushort FightBody(int ink) => ink switch
    {
        0 => 0,
        1 => White,
        2 => Rgb(31, HideHighlightGreen, HideHighlightBlue),
        >= 3 and <= 6 => HideShade(ink - 3),
        7 => BoneHighlight,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Maps a four-bit index from the initial wall palette to its packed BGR555 paint.</summary>
    internal static ushort InitialWall(int ink) => ink switch
    {
        0 or 16 => Transparent,
        1 => BoneShade(0),
        >= 2 and <= 6 => Rgb(WallNeutral, WallNeutral, WallNeutral),
        7 => WallFace, 8 => WallRecess, 9 => WallEdge, 10 => WallDeepEdge,
        11 => Rgb(WallRedMark, 0, 0),
        >= 12 and <= 14 => (ushort)((HideShade(ink - 12) & 0x3ff) |
            (ink == 14 ? WallTintDeep : WallTintLight) << 10),
        15 => WallDark,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Maps a four-bit index from the initial projectile palette to its packed BGR555 paint.</summary>
    internal static ushort InitialProjectile(int ink) => ink switch
    {
        0 or 16 => Transparent,
        1 => Rgb(ProjectileGlint, ProjectileGlint, 31),
        2 => Rgb(FlameLightRed, FlameLightGreen, 0),
        3 => Rgb(FloorGammaMidpoint(FlameLightRed, FlameDarkRed),
            (FlameLightGreen + FlameDarkGreen) / 2, 0),
        4 => Rgb(FlameDarkRed, FlameDarkGreen, 0),
        >= 5 and <= 7 => VioletShade(ink - 5),
        >= 8 and <= 10 => GreenShade(ink - 8),
        >= 11 and <= 13 => NeutralShade(ink - 11),
        14 => Rgb(ProjectileNeutralDark / 2, ProjectileNeutralDark / 2,
            ProjectileNeutralDark / 2 + ProjectileOutlineBlueTint),
        15 => Rgb(0, 31, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Maps an arm-palette index to Crocomire's bone, gold-accent, or transparent color.</summary>
    internal static ushort SkeletonArm(int ink) => ink switch
    {
        0 => Transparent, 1 => White, 2 or 7 => BoneHighlight,
        >= 3 and <= 6 => BoneShade(ink - 3),
        >= 8 and <= 11 => BoneShade(ink - 8),
        // These copied gold accents also belong to the original body OBJ palette;
        // the eight-word fight-body field does not own or contain them.
        12 => Rgb(31, BoneGoldLightGreen, 0),
        13 => Rgb((31 + BoneGoldDarkRed) / 2, BoneGoldMiddleGreen, 0),
        14 => Rgb(BoneGoldDarkRed, BoneGoldDarkGreen, 0),
        15 => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Maps a wall-spike palette index to its selected gold, steel, accent, or transparent paint.</summary>
    internal static ushort WallSpikes(int ink) => ink switch
    {
        0 => Transparent,
        >= 1 and <= 3 => GoldShade(ink - 1),
        >= 4 and <= 7 => SteelShade(ink - 4),
        8 => SpikeAccent, 9 => SpikeWarmAccent, 13 => SpikeCoolAccent,
        10 or 12 or 14 => White,
        11 or 15 => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Builds one of the four red-hide contour colors using the selected warm-channel ramp.</summary>
    private static ushort HideShade(int shade)
    {
        int red = shade switch { 0 => 31, 1 => HideMiddleRed, 2 => HideDeepRed, _ => HideOutlineRed };
        int green = HideGreenPeak - shade;
        return Rgb(red, green, Math.Min(HideBlueCap, (green + 1) / 2));
    }
    /// <summary>Interpolates the bone red ramp and combines it with the selected facet blue and warm tint.</summary>
    private static ushort BoneShade(int shade)
    {
        int red = Nearest(BoneLightRed, BoneDarkRed, shade, 3);
        int blue = shade switch { 0 => BoneBlueLight, 1 => BoneBlueMiddle, 2 => BoneBlueDeep, _ => BoneBlueDark };
        return Rgb(red, Math.Max(BoneGreenFloor, red - BoneGreenTint), blue);
    }
    /// <summary>Produces a violet projectile-rim shade with a constant blue excess over red.</summary>
    private static ushort VioletShade(int shade)
    {
        int red = Nearest(31 - VioletBlueTint, VioletDarkRed, shade, 2);
        return Rgb(red, 0, red + VioletBlueTint);
    }
    /// <summary>Produces the green projectile ramp, using gamma-two rounding for its middle green value.</summary>
    private static ushort GreenShade(int shade)
    {
        int green = shade switch { 0 => 31, 1 => FloorGammaMidpoint(31, GreenDark), _ => GreenDark };
        return Rgb(0, green, green * GreenBluePeak / 31);
    }
    /// <summary>Interpolates a neutral projectile shade equally across the three color channels.</summary>
    private static ushort NeutralShade(int shade)
    {
        int value = Nearest(ProjectileNeutralLight, ProjectileNeutralDark, shade, 2);
        return Rgb(value, value, value);
    }
    /// <summary>Builds a three-step gold spike shade with integer interpolation between selected endpoints.</summary>
    private static ushort GoldShade(int shade)
    {
        int red = shade switch { 0 => 31, 1 => SpikeGoldMiddleRed, _ => SpikeGoldDarkRed };
        int green = SpikeGoldDarkGreen + (red - SpikeGoldDarkRed) *
            (SpikeGoldLightGreen - SpikeGoldDarkGreen) / (31 - SpikeGoldDarkRed);
        return Rgb(red, green, 0);
    }
    /// <summary>Builds a four-step neutral steel shade with the selected blue-channel tint.</summary>
    private static ushort SteelShade(int shade)
    {
        int level = (SteelLight * (3 - shade) + SteelDark * shade) / 3;
        return Rgb(level, level, level + SteelBlueTint);
    }
    /// <summary>Interpolates integer channel levels with nearest-integer rounding.</summary>
    private static int Nearest(int from, int to, int step, int count) =>
        (from * (count - step) + to * step + count / 2) / count;
    // Squared midpoint of square-root intensity; endpoints remain their exact RGB5 values.
    // This is the selected material's exact gamma-two shade rule, not a historical-tool claim.
    private static int FloorGammaMidpoint(int from, int to) =>
        (int)Math.Floor((from + to + 2 * Math.Sqrt(from * to)) / 4);
    /// <summary>Packs three five-bit color channels into the game's BGR555 palette word.</summary>
    private static ushort Rgb(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
    /// <summary>Palette word used for the transparent OBJ index-zero color.</summary>
    private static ushort Transparent => Rgb(0, 0, TransparentBlue);
    /// <summary>Palette word for full intensity in all three five-bit channels.</summary>
    private static ushort White => Rgb(31, 31, 31);
    /// <summary>Selected light bone paint shared by the body teeth and bone palette.</summary>
    private static ushort BoneHighlight => Rgb(BoneLightRed, BoneLightGreen, BoneLightBlue);
}
