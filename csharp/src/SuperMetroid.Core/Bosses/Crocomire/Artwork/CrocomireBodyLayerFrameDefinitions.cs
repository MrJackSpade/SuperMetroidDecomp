using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>The 42 fight-body frames Crocomire draws partly on his body layer (#1466); their sprite parts are among his installed frames.</summary>
internal static class CrocomireBodyLayerFrameDefinitions
{
    /// <summary>The frames Crocomire draws on his body layer, each a frame of his installed body sheet.</summary>
    internal static IReadOnlySet<EnemyFrameId> Frames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.CrocomireBodyChargingFirst,
        EnemyFrameId.CrocomireBodyChargingSecond,
        EnemyFrameId.CrocomireBodyChargingThird,
        EnemyFrameId.CrocomireBodyChargingFourth,
        EnemyFrameId.CrocomireBodyChargingFifth,
        EnemyFrameId.CrocomireBodyChargingSixth,
        EnemyFrameId.CrocomireBodyChargingSeventh,
        EnemyFrameId.CrocomireBodyChargingEighth,
        EnemyFrameId.CrocomireBodyChargingNinth,
        EnemyFrameId.CrocomireBodyChargingTenth,
        EnemyFrameId.CrocomireBodyChargingEleventh,
        EnemyFrameId.CrocomireBodyChargingTwelfth,
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
        EnemyFrameId.CrocomireBodySteppingForwardFirst,
        EnemyFrameId.CrocomireBodySteppingForwardSecond,
        EnemyFrameId.CrocomireBodySteppingForwardThird,
        EnemyFrameId.CrocomireBodySteppingForwardFourth,
        EnemyFrameId.CrocomireBodySteppingForwardFifth,
        EnemyFrameId.CrocomireBodySteppingForwardSixth,
        EnemyFrameId.CrocomireBodySteppingForwardSeventh,
        EnemyFrameId.CrocomireBodySteppingForwardEighth,
        EnemyFrameId.CrocomireBodySteppingForwardNinth,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopFirst,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopSecond,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopThird,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopFourth,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopFifth,
    };
}
