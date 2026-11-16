using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// All eleven extended body frames selected by Ceres and Lower Norfair
/// Ridley's animations, and their seventeen distinct hitbox lists.
/// Touch and shots meet these offsets and rectangles, whatever his installed pictures show.
/// </summary>
internal static class RidleyCollisionDefinitions
{
    /// <summary>The 32-pixel margin Ridley's interaction check adds on each axis before the camera-relative test.</summary>
    internal static readonly Offset16<RoomPixels> InteractionWindowMargin = new(32, 32);

    /// <summary>Every body rectangle's touch reaction: Ridley's extended-body contact.</summary>
    internal static readonly EnemyReaction BodyTouchReaction = RidleyReactions.FrameHitboxTouch;

    /// <summary>Every body rectangle's shot reaction: Ridley's own shot reaction.</summary>
    internal static readonly EnemyReaction BodyShotReaction = RidleyReactions.Shot;

    /// <summary>Ridley's eleven body frames for both encounters, selected by facing, mouth and leg pose (#2234).</summary>
    internal static readonly IReadOnlyList<EnemyFrameId> BodyFrames =
    [
        EnemyFrameId.RidleyBodyLeft, EnemyFrameId.RidleyBodyRight, EnemyFrameId.RidleyBodyLeftMouthHalfOpen,
        EnemyFrameId.RidleyBodyLeftMouthOpen, EnemyFrameId.RidleyBodyRightMouthHalfOpen, EnemyFrameId.RidleyBodyRightMouthOpen,
        EnemyFrameId.RidleyBodyLeftLegsHalfExtended, EnemyFrameId.RidleyBodyLeftLegsExtended,
        EnemyFrameId.RidleyBodyRightLegsHalfExtended, EnemyFrameId.RidleyBodyRightLegsExtended, EnemyFrameId.RidleyBodyForward,
    ];
    /// <summary>The forward frame's single component Y offset, authored for its drawing.</summary>
    internal const int ForwardYOffset = -6;

    /// <summary>The closed-mouth head of left-facing Ridley.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftHeadMouthClosed =
        [new(-12, -26, 11, 13), new(-24, 3, -13, 21)];
    /// <summary>The half-open-mouth head of left-facing Ridley.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftHeadMouthHalfOpen =
        [new(-41, -19, -21, -9), new(-20, -29, 11, 5)];
    /// <summary>The open-mouth head of left-facing Ridley.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftHeadMouthOpen =
        [new(-37, -40, -14, -31), new(-25, -31, 9, 6)];
    /// <summary>Left-facing standing legs.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftLegsNotExtended = [new(-15, -10, 7, 2)];
    /// <summary>Left-facing legs half extended.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftLegsHalfExtended = [new(-17, -9, 6, 15)];
    /// <summary>Left-facing legs extended.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftLegsExtended = [new(-14, -1, 10, 23)];
    /// <summary>Ridley hitbox: facing left, hand.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftHand = [new(-15, -2, -1, 8)];
    /// <summary>Ridley hitbox: facing left, torso.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftTorso = [new(-16, -20, 12, 21)];
    /// <summary>The whole body of the front-facing turn pose.</summary>
    internal static readonly EnemyHitboxRectangle[] Forward =
        [new(-16, -32, 16, 34), new(-8, -45, 8, -33)];
    /// <summary>The closed-mouth head of right-facing Ridley, fitted to the right-facing drawing rather than mirrored.</summary>
    internal static readonly EnemyHitboxRectangle[] RightHeadMouthClosed =
        [new(-12, -25, 11, 13), new(12, 5, 24, 20)];
    /// <summary>The half-open-mouth head of right-facing Ridley.</summary>
    internal static readonly EnemyHitboxRectangle[] RightHeadMouthHalfOpen =
        [new(-13, -29, 20, 5), new(21, -18, 39, -8)];
    /// <summary>The open-mouth head of right-facing Ridley.</summary>
    internal static readonly EnemyHitboxRectangle[] RightHeadMouthOpen =
        [new(-10, -31, 25, 8), new(13, -42, 35, -32)];
    /// <summary>Right-facing standing legs.</summary>
    internal static readonly EnemyHitboxRectangle[] RightLegsNotExtended = [new(-10, -10, 17, 2)];
    /// <summary>Right-facing legs half extended.</summary>
    internal static readonly EnemyHitboxRectangle[] RightLegsHalfExtended = [new(-9, -8, 17, 15)];
    /// <summary>Right-facing legs extended.</summary>
    internal static readonly EnemyHitboxRectangle[] RightLegsExtended = [new(-11, -8, 14, 23)];
    /// <summary>Right-facing hand.</summary>
    internal static readonly EnemyHitboxRectangle[] RightHand = [new(1, -2, 14, 9)];
    /// <summary>Right-facing torso.</summary>
    internal static readonly EnemyHitboxRectangle[] RightTorso = [new(-13, -22, 14, 21)];

    /// <summary>The left-facing legs' origin; a right-facing frame negates X.</summary>
    internal static readonly Offset16<RoomPixels> LeftLegsOrigin = new(15, 22);

    /// <summary>The left-facing hand's origin; a right-facing frame negates X.</summary>
    internal static readonly Offset16<RoomPixels> LeftHandOrigin = new(-8, 7);

    /// <summary>The left-facing torso's origin; a right-facing frame negates X.</summary>
    internal static readonly Offset16<RoomPixels> LeftTorsoOrigin = new(16, 0);

    /// <summary>The left-facing head's origin; a right-facing frame negates X.</summary>
    internal static readonly Offset16<RoomPixels> LeftHeadOrigin = new(-3, -24);

    /// <summary>The side-facing body frames that face right; their component origins mirror the left-facing ones.</summary>
    internal static readonly IReadOnlySet<EnemyFrameId> RightFacing = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.RidleyBodyRight, EnemyFrameId.RidleyBodyRightMouthHalfOpen, EnemyFrameId.RidleyBodyRightMouthOpen,
        EnemyFrameId.RidleyBodyRightLegsHalfExtended, EnemyFrameId.RidleyBodyRightLegsExtended,
    };

    /// <summary>Each side-facing body frame's leg rectangles.</summary>
    internal static readonly Dictionary<EnemyFrameId, EnemyHitboxRectangle[]> Legs = new()
    {
        [EnemyFrameId.RidleyBodyLeft] = LeftLegsNotExtended,
        [EnemyFrameId.RidleyBodyLeftMouthHalfOpen] = LeftLegsNotExtended,
        [EnemyFrameId.RidleyBodyLeftMouthOpen] = LeftLegsNotExtended,
        [EnemyFrameId.RidleyBodyLeftLegsHalfExtended] = LeftLegsHalfExtended,
        [EnemyFrameId.RidleyBodyLeftLegsExtended] = LeftLegsExtended,
        [EnemyFrameId.RidleyBodyRight] = RightLegsNotExtended,
        [EnemyFrameId.RidleyBodyRightMouthHalfOpen] = RightLegsNotExtended,
        [EnemyFrameId.RidleyBodyRightMouthOpen] = RightLegsNotExtended,
        [EnemyFrameId.RidleyBodyRightLegsHalfExtended] = RightLegsHalfExtended,
        [EnemyFrameId.RidleyBodyRightLegsExtended] = RightLegsExtended,
    };

    /// <summary>Each side-facing body frame's head rectangles.</summary>
    internal static readonly Dictionary<EnemyFrameId, EnemyHitboxRectangle[]> Heads = new()
    {
        [EnemyFrameId.RidleyBodyLeft] = LeftHeadMouthClosed,
        [EnemyFrameId.RidleyBodyLeftLegsHalfExtended] = LeftHeadMouthClosed,
        [EnemyFrameId.RidleyBodyLeftLegsExtended] = LeftHeadMouthClosed,
        [EnemyFrameId.RidleyBodyLeftMouthHalfOpen] = LeftHeadMouthHalfOpen,
        [EnemyFrameId.RidleyBodyLeftMouthOpen] = LeftHeadMouthOpen,
        [EnemyFrameId.RidleyBodyRight] = RightHeadMouthClosed,
        [EnemyFrameId.RidleyBodyRightLegsHalfExtended] = RightHeadMouthClosed,
        [EnemyFrameId.RidleyBodyRightLegsExtended] = RightHeadMouthClosed,
        [EnemyFrameId.RidleyBodyRightMouthHalfOpen] = RightHeadMouthHalfOpen,
        [EnemyFrameId.RidleyBodyRightMouthOpen] = RightHeadMouthOpen,
    };
}
