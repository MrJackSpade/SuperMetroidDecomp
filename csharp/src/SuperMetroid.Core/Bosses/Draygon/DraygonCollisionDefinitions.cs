using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Owned collision for Draygon's body-layer and ordinary sprite frames. The
/// visual streams referenced by these frames are independently replaceable;
/// editing their artwork cannot move a hitbox or change a reaction.
/// </summary>
internal static class DraygonCollisionDefinitions
{
    /// <summary>Left-facing body rectangles: one shootable eye and three touch-only regions.</summary>
    internal static readonly EnemyCollisionHitbox[] FirstBodyHitboxes =
    [
        new(-17, -16, 18, 30, EnemyReaction.Ignore, EnemyReaction.DraygonShot),
        new(-59, -66, 3, -30, EnemyReaction.DraygonTouch, EnemyReaction.DudShot),
        new(16, 1, 60, 53, EnemyReaction.DraygonTouch, EnemyReaction.DudShot),
        new(-28, -37, 61, -10, EnemyReaction.DraygonTouch, EnemyReaction.DudShot),
    ];

    /// <summary>Mirrored right-facing body rectangles with the same shootable-eye rule.</summary>
    internal static readonly EnemyCollisionHitbox[] SecondBodyHitboxes =
    [
        new(-18, -7, 18, 30, EnemyReaction.Ignore, EnemyReaction.DraygonShot),
        new(-10, -63, 62, -26, EnemyReaction.DraygonTouch, EnemyReaction.DudShot),
        new(-55, 0, -21, 57, EnemyReaction.DraygonTouch, EnemyReaction.DudShot),
        new(-62, -33, 15, -5, EnemyReaction.DraygonTouch, EnemyReaction.DudShot),
    ];

    /// <summary>The empty list of the background frames in which the body cannot be hit or touched.</summary>
    internal static readonly EnemyCollisionHitbox[] EmptyHitboxes = [];

    /// <summary>Ordinary sprite art has no collision components.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> NoComponents = [];
    /// <summary>The left-facing body's one zero-offset component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> FirstBodyComponents = [new(Offset16<RoomPixels>.Zero, FirstBodyHitboxes)];
    /// <summary>The right-facing body's one zero-offset component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> SecondBodyComponents = [new(Offset16<RoomPixels>.Zero, SecondBodyHitboxes)];
    /// <summary>The one zero-offset component of a background frame in which the body cannot be hit or touched.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> EmptyBodyComponents = [new(Offset16<RoomPixels>.Zero, EmptyHitboxes)];

    /// <summary>
    /// Each Draygon frame's collision components. Every body-layer frame has one zero-offset component: the first eight and
    /// final frame of each facing use that facing's body hitboxes and the frames between use the empty list. Ordinary sprite
    /// frames have no collision components.
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
        [EnemyFrameId.DraygonEyeFacingLeftIdleFirst] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleSecond] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleThird] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingLeftDyingLoopFifth] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleFourth] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingLeftDyingLoopThird] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleFifth] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingLeftIdleSixth] = EmptyBodyComponents,
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
        [EnemyFrameId.DraygonEyeFacingRightIdleFirst] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleSecond] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleThird] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingRightDyingLoopFifth] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleFourth] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingRightDyingLoopThird] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleFifth] = EmptyBodyComponents,
        [EnemyFrameId.DraygonEyeFacingRightIdleSixth] = EmptyBodyComponents,
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
