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

    /// <summary>An animation-state flag and its bit in the statue room's native shared animation word.</summary>
    /// <param name="Flag">The flag.</param>
    /// <param name="Bit">Its bit in the word.</param>
    private readonly record struct AnimationStateBit(TourianStatueAnimationState Flag, int Bit);

    /// <summary>Each animation-state flag and the word bit the statue lists' set and clear operands name it by.</summary>
    private static readonly AnimationStateBit[] AnimationStateBits =
    [
        new(TourianStatueAnimationState.PhantoonAnimating, 1 << 0),
        new(TourianStatueAnimationState.RidleyAnimating, 1 << 1),
        new(TourianStatueAnimationState.KraidAnimating, 1 << 2),
        new(TourianStatueAnimationState.DraygonAnimating, 1 << 3),
        new(TourianStatueAnimationState.AllReleased, 1 << 4),
        new(TourianStatueAnimationState.ReleaseInProgress, 1 << 15),
    ];

    /// <summary>The animation-state flags a native set or clear operand names.</summary>
    /// <exception cref="InvalidDataException">The word sets a bit that names no flag; the message names <paramref name="context"/>.</exception>
    internal static TourianStatueAnimationState AnimationState(int word, string context)
    {
        TourianStatueAnimationState flags = TourianStatueAnimationState.None;
        int unnamed = word;
        foreach (AnimationStateBit entry in AnimationStateBits)
        {
            if ((word & entry.Bit) != 0)
                flags |= entry.Flag;
            unnamed &= ~entry.Bit;
        }
        if (unnamed != 0)
            throw new InvalidDataException($"{context}: statue animation word ${word:X4} sets bits ${unnamed:X4}, which name no flag.");
        return flags;
    }

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
