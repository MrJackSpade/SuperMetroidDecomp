namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Selects Crocomire's power-bomb reaction from the mouth of the fight-body frame it shows.</summary>
internal static class CrocomirePowerBombReactions
{
    /// <summary>Selects the reaction program for the frame shown, rejecting skeleton frames, which never take one.</summary>
    internal static CrocomireBodyProgram ForFrame(CrocomireBodyFrame frame) =>
        frame.Set == CrocomireBodyFrameSet.Body && (uint)frame.Index < (uint)CrocomirePowerBombReactionDefinitions.ByBodyFrame.Length
            ? CrocomirePowerBombReactionDefinitions.ByBodyFrame[frame.Index]
            : throw new InvalidDataException($"Crocomire power-bomb frame {frame} has no compiled reaction.");
}
