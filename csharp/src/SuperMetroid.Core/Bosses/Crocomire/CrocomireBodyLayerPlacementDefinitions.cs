using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Fixed placement data of his body layer, used when his background scroll is updated.
/// The original game searches 17 body frames in reverse order and, on a match, adds that frame's fourth-component Y
/// offset.
/// This is camera/presentation alignment, not editable boss artwork.
/// </summary>
internal static class CrocomireBodyLayerPlacementDefinitions
{
    /// <summary>The body layer's vertical-scroll origin.</summary>
    internal const int VerticalOrigin = 67;

    /// <summary>
    /// The body frames whose body-layer alignment needs a signed Y correction: steps 1 to 7 of the charge-forward step back and
    /// steps 1 to 4 of the moving claws. Every other frame, including mapped zero-offset poses, adds zero.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, int> Corrections = new()
    {
        [EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSecond] = -2,
        [EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopThird] = -2,
        [EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFourth] = -1,
        [EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFifth] = -2,
        [EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSixth] = -2,
        [EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSeventh] = -1,
        [EnemyFrameId.CrocomireBodySteppingBackFifth] = -1,
        [EnemyFrameId.CrocomireBodyProjectileAttackSecond] = -2,
        [EnemyFrameId.CrocomireBodyProjectileAttackThird] = -4,
        [EnemyFrameId.CrocomireBodyProjectileAttackFourth] = -2,
        [EnemyFrameId.CrocomireBodyProjectileAttackFifth] = -1,
    };
}
