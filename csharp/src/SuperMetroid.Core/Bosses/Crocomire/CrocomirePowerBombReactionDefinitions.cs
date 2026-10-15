namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's power-bomb reactions for each of its fifty fight-body frames. The cartridge ($A4:B9D8) reads the
/// mouth from the component selectors of the frame's spritemap; editable OAM cannot change mouth admission.
/// </summary>
internal static class CrocomirePowerBombReactionDefinitions
{
    /// <summary>Selects the reaction program for the frame shown, rejecting skeleton frames, which never take one.</summary>
    internal static CrocomireBodyProgram ForFrame(CrocomireBodyFrame frame) => frame switch
    {
        { Set: CrocomireBodyFrameSet.Body, Index: 25 or 35 } => CrocomireBodyProgram.PowerBombReactionMouthPartiallyOpen,
        { Set: CrocomireBodyFrameSet.Body, Index: 26 or (>= 28 and <= 34) or (>= 37 and <= 41) } =>
            CrocomireBodyProgram.PowerBombReactionMouthFullyOpen,
        { Set: CrocomireBodyFrameSet.Body, Index: (>= 0 and <= 24) or 27 or 36 or (>= 42 and <= 49) } =>
            CrocomireBodyProgram.PowerBombReactionMouthNotOpen,
        _ => throw new InvalidDataException($"Crocomire power-bomb frame {frame} has no compiled reaction."),
    };
}
