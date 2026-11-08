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

    /// <summary>A statue and its bit in the statue room's native shared animation word.</summary>
    /// <param name="Statue">The statue.</param>
    /// <param name="Bit">Its bit in the word.</param>
    private readonly record struct StatueBit(TourianGreyStatues Statue, int Bit);

    /// <summary>Each statue and the word bit the statue lists' set and clear operands name its animating state by.</summary>
    private static readonly StatueBit[] StatueBits =
    [
        new(TourianGreyStatues.Phantoon, 1 << 0),
        new(TourianGreyStatues.Ridley, 1 << 1),
        new(TourianGreyStatues.Kraid, 1 << 2),
        new(TourianGreyStatues.Draygon, 1 << 3),
    ];

    /// <summary>The native word bit of the all-released latch.</summary>
    private const int AllReleasedBit = 1 << 4;

    /// <summary>The native word bit of the release in progress.</summary>
    private const int ReleaseInProgressBit = 1 << 15;

    /// <summary>The parts of the shared animation state a native set or clear operand names.</summary>
    /// <param name="Statues">The statues whose animating state it names.</param>
    /// <param name="AllReleased">Whether it names the all-released latch.</param>
    /// <param name="ReleaseInProgress">Whether it names the release in progress.</param>
    internal readonly record struct AnimationOperand(TourianGreyStatues Statues, bool AllReleased, bool ReleaseInProgress);

    /// <summary>The parts of the shared animation state a native set or clear operand names.</summary>
    /// <exception cref="InvalidDataException">The word sets a bit that names no part; the message names <paramref name="context"/>.</exception>
    internal static AnimationOperand AnimationState(int word, string context)
    {
        TourianGreyStatues statues = TourianGreyStatues.None;
        int unnamed = word & ~(AllReleasedBit | ReleaseInProgressBit);
        foreach (StatueBit entry in StatueBits)
        {
            if ((word & entry.Bit) != 0)
                statues |= entry.Statue;
            unnamed &= ~entry.Bit;
        }
        if (unnamed != 0)
            throw new InvalidDataException($"{context}: statue animation word ${word:X4} sets bits ${unnamed:X4}, which name no part of the state.");
        return new(statues, (word & AllReleasedBit) != 0, (word & ReleaseInProgressBit) != 0);
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
