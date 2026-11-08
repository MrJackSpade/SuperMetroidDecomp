using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The bank-$86 projectile definitions the Tourian statue release spawns, and the statue selectors its eye glow and soul
/// spawners take, which only verification's import checks read.
/// </summary>
internal static class TourianStatueNativeWords
{
    /// <summary>$86:BA6A eye glow projectile definition.</summary>
    public const ushort EyeGlow = 0xba6a;
    /// <summary>$86:BA94 ascending soul projectile definition.</summary>
    public const ushort Soul = 0xba94;

    /// <summary>
    /// The eye of the statue a native eye-glow or soul spawn selector names: the byte offset into the $86:B90E eye
    /// tables, Phantoon 0, Ridley 2, Draygon 4 and Kraid 6.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="selector"/> names no statue.</exception>
    internal static TourianStatueEye Eye(int selector) => TourianStatueAnimatedTileMechanicsDefinitions.Statues[selector switch
    {
        0 => AnimatedTileObject.TourianStatuePhantoon,
        2 => AnimatedTileObject.TourianStatueRidley,
        4 => AnimatedTileObject.TourianStatueDraygon,
        6 => AnimatedTileObject.TourianStatueKraid,
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, "A statue selector is 0, 2, 4 or 6."),
    }].Eye;
}
