using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Areas.Tourian;
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
    private readonly record struct StatueBit(TourianStatue Statue, int Bit);

    /// <summary>Each statue and the word bit the statue lists' set and clear operands name its animating state by.</summary>
    private static readonly StatueBit[] StatueBits =
    [
        new(TourianStatue.Phantoon, 1 << 0),
        new(TourianStatue.Ridley, 1 << 1),
        new(TourianStatue.Kraid, 1 << 2),
        new(TourianStatue.Draygon, 1 << 3),
    ];

    /// <summary>The native word bit of the all-released latch.</summary>
    private const int AllReleasedBit = 1 << 4;

    /// <summary>The native word bit of the release in progress.</summary>
    private const int ReleaseInProgressBit = 1 << 15;

    /// <summary>The parts of the shared animation state a native set or clear operand names.</summary>
    /// <param name="Statue">The statue whose animating state it names, if it names one.</param>
    /// <param name="AllReleased">Whether it names the all-released latch.</param>
    /// <param name="ReleaseInProgress">Whether it names the release in progress.</param>
    internal readonly record struct AnimationOperand(TourianStatue? Statue, bool AllReleased, bool ReleaseInProgress);

    /// <summary>The parts of the shared animation state a native set or clear operand names.</summary>
    /// <exception cref="InvalidDataException">The word sets a bit that names no part; the message names <paramref name="context"/>.</exception>
    internal static AnimationOperand AnimationState(int word, string context)
    {
        TourianStatue[] statues = [.. StatueBits.Where(entry => (word & entry.Bit) != 0).Select(entry => entry.Statue)];
        int unnamed = StatueBits.Aggregate(word & ~(AllReleasedBit | ReleaseInProgressBit), (left, entry) => left & ~entry.Bit);
        if (unnamed != 0)
            throw new InvalidDataException($"{context}: statue animation word ${word:X4} sets bits ${unnamed:X4}, which name no part of the state.");
        if (statues.Length > 1)
            throw new InvalidDataException($"{context}: statue animation word ${word:X4} names {statues.Length} statues; an operand names one.");
        return new(statues.Length == 1 ? statues[0] : null, (word & AllReleasedBit) != 0, (word & ReleaseInProgressBit) != 0);
    }

    /// <summary>
    /// The eye of the statue a native eye-glow or soul spawn selector names: the byte offset into the $86:B90E eye
    /// tables, Phantoon 0, Ridley 2, Draygon 4 and Kraid 6.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="selector"/> names no statue.</exception>
    internal static TourianStatueEye Eye(int selector) => TourianStatueAnimationDefinitions.Statues[selector switch
    {
        0 => TourianStatue.Phantoon,
        2 => TourianStatue.Ridley,
        4 => TourianStatue.Draygon,
        6 => TourianStatue.Kraid,
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, "A statue selector is 0, 2, 4 or 6."),
    }].Eye;

    /// <summary>
    /// Resolves the statue whose pictures the native header of <paramref name="animatedObject"/> copies, through the
    /// import's statue-to-object mapping; any other defined object resolves to false and null.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="animatedObject"/> is undefined.</exception>
    internal static bool TryResolveStatue(BlockAnimation animatedObject, [NotNullWhen(true)] out TourianStatueAnimationDefinition? definition)
    {
        if (!Enum.IsDefined(animatedObject))
            throw new InvalidOperationException($"Undefined {nameof(BlockAnimation)} {animatedObject}.");
        definition = TourianStatueAnimatedTilePrograms.All.SingleOrDefault(
            statue => TourianStatueAnimatedTilePrograms.ObjectOf(statue.Statue) == animatedObject);
        return definition is not null;
    }
}
