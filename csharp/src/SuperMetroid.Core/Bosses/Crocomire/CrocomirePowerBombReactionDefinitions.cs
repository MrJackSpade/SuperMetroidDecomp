namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's power-bomb reactions for each of its fifty fight-body frames. The cartridge ($A4:B9D8) reads the
/// mouth from the component selectors of the frame's spritemap; editable OAM cannot change mouth admission.
/// </summary>
internal static class CrocomirePowerBombReactionDefinitions
{
    /// <summary>
    /// The reaction program for each fight-body frame index: frames 25 and 35 show the mouth partly open, 26, 28-34
    /// and 37-41 fully open, and every other frame closed.
    /// </summary>
    internal static readonly CrocomireBodyProgram[] ByBodyFrame =
    [
        .. Enumerable.Repeat(CrocomireBodyProgram.PowerBombReactionMouthNotOpen, 25),
        CrocomireBodyProgram.PowerBombReactionMouthPartiallyOpen,
        CrocomireBodyProgram.PowerBombReactionMouthFullyOpen,
        CrocomireBodyProgram.PowerBombReactionMouthNotOpen,
        .. Enumerable.Repeat(CrocomireBodyProgram.PowerBombReactionMouthFullyOpen, 7),
        CrocomireBodyProgram.PowerBombReactionMouthPartiallyOpen,
        CrocomireBodyProgram.PowerBombReactionMouthNotOpen,
        .. Enumerable.Repeat(CrocomireBodyProgram.PowerBombReactionMouthFullyOpen, 5),
        .. Enumerable.Repeat(CrocomireBodyProgram.PowerBombReactionMouthNotOpen, 8),
    ];
}
