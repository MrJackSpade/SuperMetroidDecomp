using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The collision the game defines for Crocomire's fifty selected body frames and their thirteen hitbox lists. Each frame lists
/// only the components that hold hitboxes, each once (#1937): the original's empty low and upper components and the
/// central body it repeated at the same place could never be the first hitbox a probe met.
/// Sprite and body-layer art are deliberately absent: installed visual overrides cannot
/// alter the game's component offsets, rectangles, or reaction identities.
/// Corpse-frame requests delegate to the separate corpse collision catalog.
/// </summary>
/// <remarks>
/// Component placements (jaw, body bands) and list rectangles are fitted to the drawings and retained; Crocomire has no
/// mirrored facing.
/// </remarks>
internal static class CrocomireBodyCollisionDefinitions
{
    /// <summary>Lower-front body rectangle using the standard touch and shot reactions.</summary>
    /// <remarks>The hitbox lists precede the frames that reference them: static fields initialise in declaration
    /// order.</remarks>
    private static readonly EnemyCollisionHitbox[] LowerFrontBody = [new(-78, 32, -16, 43, CrocomireReactions.ClawTouch, CrocomireReactions.DustShot)];
    /// <summary>Thin horizontal jaw/body contact strip using standard reactions.</summary>
    private static readonly EnemyCollisionHitbox[] JawStrip = [new(-95, 11, -16, 11, CrocomireReactions.ClawTouch, CrocomireReactions.DustShot)];
    /// <summary>Lower body rectangle for another mouth pose, retaining the standard reactions.</summary>
    private static readonly EnemyCollisionHitbox[] LowerBodyOtherMouthPose = [new(-69, 31, -16, 44, CrocomireReactions.ClawTouch, CrocomireReactions.DustShot)];
    /// <summary>Mid-body rectangle with the standard damage reactions.</summary>
    private static readonly EnemyCollisionHitbox[] MidBody = [new(-60, 18, -16, 32, CrocomireReactions.ClawTouch, CrocomireReactions.DustShot)];
    /// <summary>Wider mid-body rectangle with the standard damage reactions.</summary>
    private static readonly EnemyCollisionHitbox[] WideMidBody = [new(-80, 13, -16, 27, CrocomireReactions.ClawTouch, CrocomireReactions.DustShot)];
    /// <summary>Raised mouth rectangle whose shot reaction differs from the standard body set.</summary>
    private static readonly EnemyCollisionHitbox[] RaisedMouth = [new(-95, -6, -16, 27, CrocomireReactions.ClawTouch, CrocomireReactions.AbsorbedShot)];
    /// <summary>Upper mouth rectangle using the alternate shot reaction.</summary>
    private static readonly EnemyCollisionHitbox[] UpperMouth = [new(-59, -9, -16, 5, CrocomireReactions.ClawTouch, CrocomireReactions.AbsorbedShot)];
    /// <summary>High mouth rectangle used by a raised body pose with alternate shot handling.</summary>
    private static readonly EnemyCollisionHitbox[] HighMouth = [new(-93, -36, -30, -13, CrocomireReactions.ClawTouch, CrocomireReactions.AbsorbedShot)];
    /// <summary>Three-piece upper body collision list with separate shot handling for its first piece.</summary>
    private static readonly EnemyCollisionHitbox[] UpperBody = [new(-50, -37, 40, -16, EnemyReaction.NormalEnemyTouch, CrocomireReactions.MouthShot), new(-52, -58, 42, -38, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot), new(-32, -13, 40, 0, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot)];
    /// <summary>Three-piece raised upper-body list with a taller first rectangle.</summary>
    private static readonly EnemyCollisionHitbox[] RaisedUpperBody = [new(-46, -53, 13, -16, EnemyReaction.NormalEnemyTouch, CrocomireReactions.MouthShot), new(-41, -74, 16, -57, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot), new(-37, -16, 16, -3, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot)];
    /// <summary>Broad high body rectangle whose shot reaction is the common no-op.</summary>
    private static readonly EnemyCollisionHitbox[] BroadHighBody = [new(-37, -50, 38, -4, CrocomireReactions.ClawTouch, EnemyReaction.NormalEnemyShot)];
    /// <summary>Full-height central body rectangle with the main fight shot reaction.</summary>
    private static readonly EnemyCollisionHitbox[] CentralBody = [new(-38, -48, 37, 52, CrocomireReactions.ClawTouch, CrocomireReactions.DustShot)];
    /// <summary>Lower central body rectangle using the alternate shot reaction.</summary>
    private static readonly EnemyCollisionHitbox[] LowerCentralBody = [new(-38, -4, 42, 52, CrocomireReactions.ClawTouch, CrocomireReactions.AbsorbedShot)];

    /// <summary>The fifty living-body frames' physical component lists, with presentation art excluded.</summary>
    internal static EnemyFrameCollision<EnemyCollisionComponent> Frames { get; } = new()
    {
        { EnemyFrameId.CrocomireBodyNearFootForwardArmBent, [new(new(3, 11), MidBody), new(new(0, 0), CentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodyNearFootForwardArmSquare, [new(new(1, 11), WideMidBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyNearFootForwardArmReaching, [new(new(0, 8), JawStrip), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyNearFootForwardArmSweeping, [new(new(1, 11), LowerFrontBody), new(new(0, -1), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyNearFootForwardArmLifting, [new(new(1, 10), LowerFrontBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyNearFootForwardArmUpright, [new(new(1, 10), LowerBodyOtherMouthPose), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmBent, [new(new(1, 10), MidBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmSquare, [new(new(1, 10), WideMidBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmReaching, [new(new(1, 10), JawStrip), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmSweeping, [new(new(1, 12), LowerFrontBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmLifting, [new(new(1, 13), LowerFrontBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmUpright, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyArmUprightMouthOpen, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyArmUprightMouthOpenLess, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyArmUprightMouthClosing, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyArmUprightMouthHalfClosed, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyArmUprightMouthMostlyClosed, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyArmUprightMouthNearlyShut, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyArmUprightMouthShut, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodySwingingArmBent, [new(new(1, 11), UpperMouth), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodySwingingArmRaised, [new(new(0, 8), HighMouth), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodySwingingArmOutstretched, [new(new(1, 8), RaisedMouth), new(new(0, -4), CentralBody)] },
        { EnemyFrameId.CrocomireBodySwingingArmLowering, [new(new(0, 10), JawStrip), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodySwingingArmReturning, [new(new(1, 12), LowerFrontBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthShut, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 0), BroadHighBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthParted, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 0), UpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthGaping, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyNearFootForwardArmBentMouthShut, [new(new(3, 11), MidBody), new(new(0, 0), BroadHighBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBodyNearFootForwardArmSweepingMouthGaping, [new(new(1, 11), LowerFrontBody), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyNearFootForwardArmLiftingMouthGaping, [new(new(1, 10), LowerFrontBody), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyNearFootForwardArmUprightMouthGaping, [new(new(1, 10), LowerBodyOtherMouthPose), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmBentMouthGaping, [new(new(1, 10), MidBody), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmSquareMouthGaping, [new(new(1, 10), WideMidBody), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmReachingMouthGaping, [new(new(1, 10), JawStrip), new(new(0, 0), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmSweepingMouthGaping, [new(new(1, 12), LowerFrontBody), new(new(0, 0), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmLiftingMouthParted, [new(new(1, 13), LowerFrontBody), new(new(0, 0), CentralBody), new(new(0, 0), UpperBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthShutting, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 0), CentralBody), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyLungingArmBent, [new(new(1, 11), UpperMouth), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyLungingArmRaised, [new(new(0, 6), HighMouth), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyLungingArmOutstretched, [new(new(1, 4), RaisedMouth), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyLungingArmLowering, [new(new(0, 8), JawStrip), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyLungingArmReturning, [new(new(1, 11), LowerFrontBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody)] },
        { EnemyFrameId.CrocomireBodyMeltingFleshTopRow, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingFleshTopTwoRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingFleshTopThreeRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingFleshTopFourRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingBareSkullTopRow, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingBareSkullTopTwoRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingBareSkullTopThreeRows, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBodyMeltingBareSkullTopFourRows, [new(new(0, 0), JawStrip)] },
    };
}
