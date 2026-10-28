using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Bosses.Crocomire.Artwork.CrocomirePaintDefinitions;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Paints Crocomire's material inks from the channels the catalog selects: shared channels, shade ramps and aliases calculate, and no complete stock palette is stored.</summary>
internal static class CrocomirePaint
{
    /// <summary>Builds the eight-word combat body field from red hide shades, bone highlight, and neutral endpoints.</summary>
    internal static RgbColor FightBody(int ink) => ink switch
    {
        0 => RgbColor.Black,
        1 => White,
        2 => Rgb(31, HideHighlightGreen, HideHighlightBlue),
        >= 3 and <= 6 => HideShade(ink - 3),
        7 => BoneHighlight,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Combines bone, masonry, hide, and violet-tinted edge inks in the initial wall palette.</summary>
    internal static RgbColor InitialWall(int ink) => ink switch
    {
        0 or 16 => Transparent,
        1 => BoneShade(0),
        >= 2 and <= 6 => Rgb(WallNeutral, WallNeutral, WallNeutral),
        7 => WallFace, 8 => WallRecess, 9 => WallEdge, 10 => WallDeepEdge,
        11 => Rgb(WallRedMark, 0, 0),
        >= 12 and <= 14 => HideShade(ink - 12).WithBlue(ink == 14 ? WallTintDeep : WallTintLight),
        15 => WallDark,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Builds projectile glint, flame, violet, green, and neutral ramps for Crocomire's initial palette.</summary>
    internal static RgbColor InitialProjectile(int ink) => ink switch
    {
        0 => Transparent,
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

    /// <summary>Combines repeated bone shades with the original body palette's copied gold accents.</summary>
    internal static RgbColor SkeletonArm(int ink) => ink switch
    {
        0 => Transparent, 1 => White, 2 or 7 => BoneHighlight,
        >= 3 and <= 6 => BoneShade(ink - 3),
        >= 8 and <= 11 => BoneShade(ink - 8),
        // These copied gold accents also belong to the original body OBJ palette;
        // the eight-word fight-body field does not own or contain them.
        12 => Rgb(31, BoneGoldLightGreen, 0),
        13 => Rgb((31 + BoneGoldDarkRed) / 2, BoneGoldMiddleGreen, 0),
        14 => Rgb(BoneGoldDarkRed, BoneGoldDarkGreen, 0),
        15 => RgbColor.Black,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Builds gold mounts, cool steel, and copied accent inks for the wall spikes.</summary>
    internal static RgbColor WallSpikes(int ink) => ink switch
    {
        0 => Transparent,
        >= 1 and <= 3 => GoldShade(ink - 1),
        >= 4 and <= 7 => SteelShade(ink - 4),
        8 => SpikeAccent, 9 => SpikeWarmAccent, 13 => SpikeCoolAccent,
        10 or 12 or 14 => White,
        11 or 15 => RgbColor.Black,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Red hide contours darken by selected red levels while green and blue follow shared rules.</summary>
    private static RgbColor HideShade(int shade)
    {
        int red = shade switch { 0 => 31, 1 => HideMiddleRed, 2 => HideDeepRed, _ => HideOutlineRed };
        int green = HideGreenPeak - shade;
        return Rgb(red, green, Math.Min(HideBlueCap, (green + 1) / 2));
    }
    /// <summary>Interpolates bone red, applies a clipped warm green tint, and selects each facet's blue.</summary>
    private static RgbColor BoneShade(int shade)
    {
        int red = Nearest(BoneLightRed, BoneDarkRed, shade, 3);
        int blue = shade switch { 0 => BoneBlueLight, 1 => BoneBlueMiddle, 2 => BoneBlueDeep, _ => BoneBlueDark };
        return Rgb(red, Math.Max(BoneGreenFloor, red - BoneGreenTint), blue);
    }
    /// <summary>Interpolates the projectile rim's red while keeping blue two levels brighter.</summary>
    private static RgbColor VioletShade(int shade)
    {
        int red = Nearest(31 - VioletBlueTint, VioletDarkRed, shade, 2);
        return Rgb(red, 0, red + VioletBlueTint);
    }
    /// <summary>Uses a gamma-two middle green and scales the projectile blue channel from green.</summary>
    private static RgbColor GreenShade(int shade)
    {
        int green = shade switch { 0 => 31, 1 => FloorGammaMidpoint(31, GreenDark), _ => GreenDark };
        return Rgb(0, green, green * GreenBluePeak / 31);
    }
    /// <summary>Interpolates a three-step neutral projectile ramp with equal RGB channels.</summary>
    private static RgbColor NeutralShade(int shade)
    {
        int value = Nearest(ProjectileNeutralLight, ProjectileNeutralDark, shade, 2);
        return Rgb(value, value, value);
    }
    /// <summary>Chooses the spike mount's red and derives green between its selected endpoints.</summary>
    private static RgbColor GoldShade(int shade)
    {
        int red = shade switch { 0 => 31, 1 => SpikeGoldMiddleRed, _ => SpikeGoldDarkRed };
        int green = SpikeGoldDarkGreen + (red - SpikeGoldDarkRed) *
            (SpikeGoldLightGreen - SpikeGoldDarkGreen) / (31 - SpikeGoldDarkRed);
        return Rgb(red, green, 0);
    }
    /// <summary>Interpolates cool steel intensity while maintaining a three-level blue excess.</summary>
    private static RgbColor SteelShade(int shade)
    {
        int level = (SteelLight * (3 - shade) + SteelDark * shade) / 3;
        return Rgb(level, level, level + SteelBlueTint);
    }
    /// <summary>Rounds a discrete shade interpolation to the nearest RGB5 channel level.</summary>
    private static int Nearest(int from, int to, int step, int count) =>
        (from * (count - step) + to * step + count / 2) / count;
    /// <summary>Applies the selected material's gamma-two midpoint rule while preserving exact RGB5 endpoints.</summary>
    private static int FloorGammaMidpoint(int from, int to) =>
        (int)Math.Floor((from + to + 2 * Math.Sqrt(from * to)) / 4);
    /// <summary>Constructs a hardware BGR555 word from its separate five-bit paint channels.</summary>
    private static RgbColor Rgb(int red, int green, int blue) => new RgbColor(red, green, blue);
    /// <summary>Transparent OBJ ink zero retains the cartridge's unused blue metadata channel.</summary>
    private static RgbColor Transparent => Rgb(0, 0, TransparentBlue);
    /// <summary>Full-intensity neutral paint reused by the body, skeleton, and spike palettes.</summary>
    private static RgbColor White => Rgb(31, 31, 31);
    /// <summary>Lightest selected bone red, green, and blue shared by body and skeleton palettes.</summary>
    private static RgbColor BoneHighlight => Rgb(BoneLightRed, BoneLightGreen, BoneLightBlue);
}
