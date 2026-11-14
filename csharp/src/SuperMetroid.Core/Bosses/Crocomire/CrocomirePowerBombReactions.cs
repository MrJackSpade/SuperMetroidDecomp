namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Selects Crocomire's power-bomb reaction from the mouth of the fight-body frame it shows.</summary>
internal static class CrocomirePowerBombReactions
{
    /// <summary>Selects the reaction animation for the frame shown, rejecting skeleton frames, which never take one.</summary>
    internal static CrocomireBodySegment ForFrame(CrocomireBodyFrame frame) => frame.Mouth switch
    {
        CrocomireMouth.Closed => CrocomireBodySegment.PowerBombReactionMouthNotOpen,
        CrocomireMouth.PartlyOpen => CrocomireBodySegment.PowerBombReactionMouthPartiallyOpen,
        CrocomireMouth.FullyOpen => CrocomireBodySegment.PowerBombReactionMouthFullyOpen,
        null => throw new InvalidDataException($"Crocomire power-bomb frame {frame} has no compiled reaction."),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireMouth)} {frame.Mouth}."),
    };
}
