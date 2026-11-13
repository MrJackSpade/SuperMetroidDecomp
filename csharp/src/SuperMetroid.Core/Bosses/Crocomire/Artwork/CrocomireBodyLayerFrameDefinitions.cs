using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>The 42 fight-body frames Crocomire draws partly on his body layer (#1466); their sprite parts are among his installed frames.</summary>
internal static class CrocomireBodyLayerFrameDefinitions
{
    /// <summary>The frames Crocomire draws on his body layer, each a frame of his installed body sheet.</summary>
    internal static IReadOnlySet<EnemyFrameId> Frames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFirst,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSecond,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopThird,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFourth,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFifth,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSixth,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSeventh,
        EnemyFrameId.CrocomireBodySteppingBackFifth,
        EnemyFrameId.CrocomireBodySteppingBackFourth,
        EnemyFrameId.CrocomireBodySteppingBackThird,
        EnemyFrameId.CrocomireBodySteppingBackSecond,
        EnemyFrameId.CrocomireBodySteppingBackFirst,
        EnemyFrameId.CrocomireBodyInitial,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageSixth,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFifth,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFourth,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageThird,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageSecond,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFirst,
        EnemyFrameId.CrocomireBodyProjectileAttackFirst,
        EnemyFrameId.CrocomireBodyProjectileAttackSecond,
        EnemyFrameId.CrocomireBodyProjectileAttackThird,
        EnemyFrameId.CrocomireBodyProjectileAttackFourth,
        EnemyFrameId.CrocomireBodyProjectileAttackFifth,
        EnemyFrameId.CrocomireBodyProjectileAttackSixth,
        EnemyFrameId.CrocomireBodyProjectileAttackSeventh,
        EnemyFrameId.CrocomireBodyProjectileAttackEighth,
        EnemyFrameId.CrocomireBodyStepForwardAfterDelay,
        EnemyFrameId.CrocomireBodyStepForwardFourth,
        EnemyFrameId.CrocomireBodyStepForwardFifth,
        EnemyFrameId.CrocomireBodyStepForwardSixth,
        EnemyFrameId.CrocomireBodyStepForwardSeventh,
        EnemyFrameId.CrocomireBodyStepForwardEighth,
        EnemyFrameId.CrocomireBodyStepForwardNinth,
        EnemyFrameId.CrocomireBodyStepForwardTenth,
        EnemyFrameId.CrocomireBodyStepForwardEleventh,
        EnemyFrameId.CrocomireBodyStepForwardTwelfth,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopFirst,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopSecond,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopThird,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopFourth,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopFifth,
    };
}
