using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Owned collision for Draygon's body-layer and ordinary sprite frames. The
/// pictures these frames show are independently replaceable;
/// editing their artwork cannot move a hitbox or change a reaction.
/// </summary>
internal static class DraygonCollisionDefinitions
{
    /// <summary>Left-facing body rectangles: one shootable eye and three touch-only regions.</summary>
    internal static readonly EnemyCollisionHitbox[] FirstBodyHitboxes =
    [
        new(-17, -16, 18, 30, EnemyReaction.Ignore, DraygonReactions.Shot),
        new(-59, -66, 3, -30, DraygonReactions.Touch, EnemyReaction.DudShot),
        new(16, 1, 60, 53, DraygonReactions.Touch, EnemyReaction.DudShot),
        new(-28, -37, 61, -10, DraygonReactions.Touch, EnemyReaction.DudShot),
    ];

    /// <summary>Mirrored right-facing body rectangles with the same shootable-eye rule.</summary>
    internal static readonly EnemyCollisionHitbox[] SecondBodyHitboxes =
    [
        new(-18, -7, 18, 30, EnemyReaction.Ignore, DraygonReactions.Shot),
        new(-10, -63, 62, -26, DraygonReactions.Touch, EnemyReaction.DudShot),
        new(-55, 0, -21, 57, DraygonReactions.Touch, EnemyReaction.DudShot),
        new(-62, -33, 15, -5, DraygonReactions.Touch, EnemyReaction.DudShot),
    ];

    /// <summary>A frame that cannot be hit or touched has no collision components.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> NoComponents = [];
    /// <summary>The left-facing body's one zero-offset component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> FirstBodyComponents = [new(Offset16<RoomPixels>.Zero, FirstBodyHitboxes)];
    /// <summary>The right-facing body's one zero-offset component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> SecondBodyComponents = [new(Offset16<RoomPixels>.Zero, SecondBodyHitboxes)];

    /// <summary>
    /// Each Draygon frame's collision components (#1937). The first eight body-layer frames and the final one of each facing
    /// have that facing's one zero-offset body component; the body-layer frames between and every ordinary sprite frame
    /// cannot be hit or touched and have none.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, ImmutableArray<EnemyCollisionComponent>> FrameComponents = new()
    {
        [EnemyFrameId.DraygonArmsFacingLeftIdleFirst] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftIdleSecond] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftIdleThird] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftIdleFourth] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftIdleFifth] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftIdleSixth] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftDyingFirst] = FirstBodyComponents,
        [EnemyFrameId.DraygonArmsFacingLeftDyingSecond] = FirstBodyComponents,
        [EnemyFrameId.DraygonArmsFacingLeftDyingThird] = FirstBodyComponents,
        [EnemyFrameId.DraygonArmsFacingLeftDyingFourth] = FirstBodyComponents,
        [EnemyFrameId.DraygonBodyFacingLeftFireGoopFirst] = FirstBodyComponents,
        [EnemyFrameId.DraygonBodyFacingLeftFireGoopSecond] = FirstBodyComponents,
        [EnemyFrameId.DraygonBodyFacingLeftFireGoopThird] = FirstBodyComponents,
        [EnemyFrameId.DraygonBodyFacingLeftFireGoopFourth] = FirstBodyComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleFirst] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleSecond] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleThird] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftDyingLoopFifth] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleFourth] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftDyingLoopThird] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleFifth] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleSixth] = NoComponents,
        [EnemyFrameId.DraygonBodyFacingLeftReset] = FirstBodyComponents,
        [EnemyFrameId.DraygonArmsFacingLeftNearSwoopApexFirst] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftNearSwoopApexSecond] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftNearSwoopApexThird] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftNearSwoopApexFourth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftIdleFirst] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftIdleSecond] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftIdleThird] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftIdleFourth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftIdleFifth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftIdleSixth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftIdleSeventh] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipSecond] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipThird] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipFourth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipFifth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipSixth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipSeventh] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipEighth] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightIdleFirst] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightIdleSecond] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightIdleThird] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightIdleFourth] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightIdleFifth] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightIdleSixth] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightDyingFirst] = SecondBodyComponents,
        [EnemyFrameId.DraygonArmsFacingRightDyingSecond] = SecondBodyComponents,
        [EnemyFrameId.DraygonArmsFacingRightDyingThird] = SecondBodyComponents,
        [EnemyFrameId.DraygonArmsFacingRightDyingFourth] = SecondBodyComponents,
        [EnemyFrameId.DraygonBodyFacingRightFireGoopFirst] = SecondBodyComponents,
        [EnemyFrameId.DraygonBodyFacingRightFireGoopSecond] = SecondBodyComponents,
        [EnemyFrameId.DraygonBodyFacingRightFireGoopThird] = SecondBodyComponents,
        [EnemyFrameId.DraygonBodyFacingRightFireGoopFourth] = SecondBodyComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleFirst] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleSecond] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleThird] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightDyingLoopFifth] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleFourth] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightDyingLoopThird] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleFifth] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleSixth] = NoComponents,
        [EnemyFrameId.DraygonBodyFacingRightReset] = SecondBodyComponents,
        [EnemyFrameId.DraygonArmsFacingRightNearSwoopApexFirst] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightNearSwoopApexSecond] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightNearSwoopApexThird] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightNearSwoopApexFourth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightIdleFirst] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightIdleSecond] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightIdleThird] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightIdleFourth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightIdleFifth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightIdleSixth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightIdleSeventh] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopSecond] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopThird] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopFourth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopFifth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopSixth] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopSeventh] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopEighth] = NoComponents,
    };
}
