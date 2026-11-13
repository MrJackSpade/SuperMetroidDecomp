using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Engine-owned collision data for Crocomire's fifty selected body frames
/// and their fifteen distinct hitbox lists.
/// Sprite and body-layer art are deliberately absent: installed visual overrides cannot
/// alter the game's component offsets, rectangles, or reaction identities.
/// Values are pinned to the project's NTSC J/U v1.0 retail release.
/// Corpse-frame requests delegate to the separate corpse collision catalog.
/// </summary>
/// <remarks>
/// Reviewed for #1165. The 50 frame records lie in four contiguous runs, so frame identities
/// derive from run starts and record sizes. Component placements (jaw, legs, body bands) and
/// list rectangles are fitted to the drawings and retained; Crocomire has no mirrored facing.
/// </remarks>
internal static class CrocomireBodyCollisionDefinitions
{
    /// <summary>Lower-front body rectangle using the standard touch and shot reactions.</summary>
    /// <remarks>The hitbox lists precede the frames that reference them: static fields initialise in declaration
    /// order.</remarks>
    private static readonly EnemyCollisionHitbox[] LowerFrontBody = [new(-78, 32, -16, 43, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>Thin horizontal jaw/body contact strip using standard reactions.</summary>
    private static readonly EnemyCollisionHitbox[] JawStrip = [new(-95, 11, -16, 11, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>Lower body rectangle for another mouth pose, retaining the standard reactions.</summary>
    private static readonly EnemyCollisionHitbox[] LowerBodyOtherMouthPose = [new(-69, 31, -16, 44, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>Mid-body rectangle with the standard damage reactions.</summary>
    private static readonly EnemyCollisionHitbox[] MidBody = [new(-60, 18, -16, 32, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>Wider mid-body rectangle with the standard damage reactions.</summary>
    private static readonly EnemyCollisionHitbox[] WideMidBody = [new(-80, 13, -16, 27, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>The empty list of the two low components every fight frame carries.</summary>
    private static readonly EnemyCollisionHitbox[] EmptyLowComponent = [];
    /// <summary>Raised mouth rectangle whose shot reaction differs from the standard body set.</summary>
    private static readonly EnemyCollisionHitbox[] RaisedMouth = [new(-95, -6, -16, 27, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireHitCountShot)];
    /// <summary>Upper mouth rectangle using the alternate shot reaction.</summary>
    private static readonly EnemyCollisionHitbox[] UpperMouth = [new(-59, -9, -16, 5, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireHitCountShot)];
    /// <summary>High mouth rectangle used by a raised body pose with alternate shot handling.</summary>
    private static readonly EnemyCollisionHitbox[] HighMouth = [new(-93, -36, -30, -13, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireHitCountShot)];
    /// <summary>The empty list of the upper component in the attack frames.</summary>
    private static readonly EnemyCollisionHitbox[] EmptyAttackUpperComponent = [];
    /// <summary>Three-piece upper body collision list with separate shot handling for its first piece.</summary>
    private static readonly EnemyCollisionHitbox[] UpperBody = [new(-50, -37, 40, -16, EnemyReaction.NormalEnemyTouch, EnemyReaction.CrocomireMouthShot), new(-52, -58, 42, -38, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot), new(-32, -13, 40, 0, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot)];
    /// <summary>Three-piece raised upper-body list with a taller first rectangle.</summary>
    private static readonly EnemyCollisionHitbox[] RaisedUpperBody = [new(-46, -53, 13, -16, EnemyReaction.NormalEnemyTouch, EnemyReaction.CrocomireMouthShot), new(-41, -74, 16, -57, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot), new(-37, -16, 16, -3, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot)];
    /// <summary>Broad high body rectangle whose shot reaction is the common no-op.</summary>
    private static readonly EnemyCollisionHitbox[] BroadHighBody = [new(-37, -50, 38, -4, EnemyReaction.CrocomireClawTouch, EnemyReaction.NormalEnemyShot)];
    /// <summary>Full-height central body rectangle with the main fight shot reaction.</summary>
    private static readonly EnemyCollisionHitbox[] CentralBody = [new(-38, -48, 37, 52, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireDustHitboxShot)];
    /// <summary>Lower central body rectangle using the alternate shot reaction.</summary>
    private static readonly EnemyCollisionHitbox[] LowerCentralBody = [new(-38, -4, 42, 52, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireHitCountShot)];

    /// <summary>The fifty living-body frames' physical component lists, with presentation art excluded.</summary>
    internal static EnemyFrameCollision<EnemyCollisionComponent> Frames { get; } = new()
    {
        { EnemyFrameId.CrocomireBodyChargingFirst, [new(new(3, 11), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingSecond, [new(new(1, 11), WideMidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingThird, [new(new(0, 8), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingFourth, [new(new(1, 11), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingFifth, [new(new(1, 10), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingSixth, [new(new(1, 10), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingSeventh, [new(new(1, 10), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingEighth, [new(new(1, 10), WideMidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingNinth, [new(new(1, 10), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingTenth, [new(new(1, 12), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingEleventh, [new(new(1, 13), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyChargingTwelfth, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyInitial, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageSixth, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFifth, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFourth, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageThird, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageSecond, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFirst, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackFirst, [new(new(1, 11), UpperMouth), new(new(0, 37), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackSecond, [new(new(0, 8), HighMouth), new(new(0, 39), EmptyLowComponent), new(new(-29, 39), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackThird, [new(new(1, 8), RaisedMouth), new(new(0, 41), EmptyLowComponent), new(new(-29, 32), EmptyLowComponent), new(new(0, -4), CentralBody), new(new(0, -4), CentralBody), new(new(0, -4), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackFourth, [new(new(0, 10), JawStrip), new(new(0, 39), EmptyLowComponent), new(new(-29, 37), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackFifth, [new(new(1, 12), LowerFrontBody), new(new(0, 37), EmptyLowComponent), new(new(-29, 40), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackSixth, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackSeventh, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), UpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackEighth, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyStepForwardAfterDelay, [new(new(3, 11), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodySteppingForwardFirst, [new(new(1, 11), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodySteppingForwardSecond, [new(new(1, 10), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodySteppingForwardThird, [new(new(1, 10), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodySteppingForwardFourth, [new(new(1, 10), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodySteppingForwardFifth, [new(new(1, 10), WideMidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodySteppingForwardSixth, [new(new(1, 10), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodySteppingForwardSeventh, [new(new(1, 12), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodySteppingForwardEighth, [new(new(1, 13), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), UpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodySteppingForwardNinth, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackLoopFirst, [new(new(1, 11), UpperMouth), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackLoopSecond, [new(new(0, 6), HighMouth), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackLoopThird, [new(new(1, 4), RaisedMouth), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackLoopFourth, [new(new(0, 8), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyProjectileAttackLoopFifth, [new(new(1, 11), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyMeltingOneTopRow, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingOneTopTwoRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingOneTopThreeRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingOneTopFourRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingTwoTopRow, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingTwoTopTwoRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingTwoTopThreeRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingTwoTopFourRows, [new(new(0, 0), JawStrip)] },
    };
}
