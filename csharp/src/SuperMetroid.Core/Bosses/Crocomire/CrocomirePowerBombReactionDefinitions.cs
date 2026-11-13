namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's power-bomb reactions for each of its fifty fight-body frames. The game reads the
/// mouth from the parts of the frame's picture; editable sprite art cannot change mouth admission.
/// </summary>
internal static class CrocomirePowerBombReactionDefinitions
{
    /// <summary>
    /// The reaction program for each fight-body frame index: frames 25 and 35 show the mouth partly open, 26, 28-34
    /// and 37-41 fully open, and every other frame closed.
    /// </summary>
    internal static readonly CrocomireBodySegment[] ByBodyFrame =
    [
        .. Enumerable.Repeat(CrocomireBodySegment.PowerBombReactionMouthNotOpen, 25),
        CrocomireBodySegment.PowerBombReactionMouthPartiallyOpen,
        CrocomireBodySegment.PowerBombReactionMouthFullyOpen,
        CrocomireBodySegment.PowerBombReactionMouthNotOpen,
        .. Enumerable.Repeat(CrocomireBodySegment.PowerBombReactionMouthFullyOpen, 7),
        CrocomireBodySegment.PowerBombReactionMouthPartiallyOpen,
        CrocomireBodySegment.PowerBombReactionMouthNotOpen,
        .. Enumerable.Repeat(CrocomireBodySegment.PowerBombReactionMouthFullyOpen, 5),
        .. Enumerable.Repeat(CrocomireBodySegment.PowerBombReactionMouthNotOpen, 8),
    ];
}
