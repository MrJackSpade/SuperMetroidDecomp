using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Selects Crocomire's power-bomb reaction from the mouth of the fight-body frame it shows.</summary>
internal static class CrocomirePowerBombReactions
{
    /// <summary>Selects the reaction animation for the frame shown, rejecting skeleton frames, which never take one.</summary>
    internal static CrocomireBodyAnimation ForFrame(EnemyFrameId frame) =>
        CrocomireMouthDefinitions.Mouths.TryGetValue(frame, out CrocomireMouth mouth)
            ? mouth switch
            {
                CrocomireMouth.Closed => CrocomireBodyAnimation.PowerBombReactionMouthNotOpen,
                CrocomireMouth.PartlyOpen => CrocomireBodyAnimation.PowerBombReactionMouthPartiallyOpen,
                CrocomireMouth.FullyOpen => CrocomireBodyAnimation.PowerBombReactionMouthFullyOpen,
                _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireMouth)} {mouth}."),
            }
            : throw new InvalidDataException($"Crocomire power-bomb frame {frame} has no compiled reaction.");
}
