using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The presentation boundary of the save station electricity (#1296): the installed artwork key of every typed frame,
/// the bank-$86 address of the spritemap operand that drew it.
/// </summary>
internal static class SaveStationElectricityVisualDefinitions
{
    /// <summary>The cycle's first frame at $86:E687, after the list's loop-count command.</summary>
    private const ushort FirstFrame = 0xe687;
    /// <summary>A frame's spritemap operand follows its two-byte duration.</summary>
    private const int OperandOffset = 2;
    /// <summary>Each timed frame is a duration and an operand.</summary>
    private const int FrameBytes = 4;

    /// <summary>Every frame in installed-artwork order.</summary>
    internal static IReadOnlyList<SaveStationElectricityFrame> ProjectileFrames { get; } =
        [.. Enumerable.Range(0, SaveStationElectricityAnimations.PhaseCount).Select(phase => new SaveStationElectricityFrame(phase))];

    /// <summary>The installed presentation key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(SaveStationElectricityFrame frame) =>
        unchecked((ushort)(FirstFrame + FrameBytes * Ensure.BetweenInclusive(0, SaveStationElectricityAnimations.PhaseCount - 1, frame.Phase) + OperandOffset));
}