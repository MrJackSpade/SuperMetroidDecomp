using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// All eleven bank-$A6 extended body frames selected by Ceres and Lower Norfair
/// Ridley's instruction programs, and their seventeen distinct hitbox lists.
/// The original $A0:9A5A/$9B7F collision walker consumes these offsets and
/// rectangles independently of the displayed sprite composition.
/// </summary>
internal static class RidleyCollisionDefinitions
{
    /// <summary>The 32-pixel margin $A6:DE7A adds on each axis before the camera-relative test.</summary>
    internal static readonly Offset16<RoomPixels> InteractionWindowMargin = new(32, 32);

    /// <summary>Native extended body maps for both Ridley encounters, selected by facing, mouth, and leg pose.</summary>
    internal enum BodyFrame
    {
        /// <summary>ExtendedSpritemap_Ridley_FacingLeft.</summary>
        Left,
        /// <summary>ExtendedSpritemap_Ridley_FacingRight.</summary>
        Right,
        /// <summary>ExtendedSpritemap_Ridley_FacingLeft_MouthHalfOpen.</summary>
        LeftMouthHalfOpen,
        /// <summary>ExtendedSpritemap_Ridley_FacingLeft_MouthOpen.</summary>
        LeftMouthOpen,
        /// <summary>ExtendedSpritemap_Ridley_FacingRight_MouthHalfOpen.</summary>
        RightMouthHalfOpen,
        /// <summary>ExtendedSpritemap_Ridley_FacingRight_MouthOpen.</summary>
        RightMouthOpen,
        /// <summary>ExtendedSpritemap_Ridley_FacingLeft_LegsHalfExtended.</summary>
        LeftLegsHalfExtended,
        /// <summary>ExtendedSpritemap_Ridley_FacingLeft_LegsExtended.</summary>
        LeftLegsExtended,
        /// <summary>ExtendedSpritemap_Ridley_FacingRight_LegsHalfExtended.</summary>
        RightLegsHalfExtended,
        /// <summary>ExtendedSpritemap_Ridley_FacingRight_LegsExtended.</summary>
        RightLegsExtended,
        /// <summary>ExtendedSpritemap_Ridley_FacingForward.</summary>
        Forward,
    }
    /// <summary>$A6:EADB, the forward frame's single component Y offset, authored for its drawing.</summary>
    internal const int ForwardYOffset = -6;

    /// <summary>Hitbox_Ridley_FacingLeft_MouthClosed: the closed-mouth head of left-facing Ridley.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftHeadMouthClosed =
        [new(-12, -26, 11, 13), new(-24, 3, -13, 21)];
    /// <summary>The half-open-mouth head of left-facing Ridley.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftHeadMouthHalfOpen =
        [new(-41, -19, -21, -9), new(-20, -29, 11, 5)];
    /// <summary>The open-mouth head of left-facing Ridley.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftHeadMouthOpen =
        [new(-37, -40, -14, -31), new(-25, -31, 9, 6)];
    /// <summary>Hitbox_Ridley_FacingLeft_LegsNotExtended: left-facing standing legs.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftLegsNotExtended = [new(-15, -10, 7, 2)];
    /// <summary>Left-facing legs half extended.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftLegsHalfExtended = [new(-17, -9, 6, 15)];
    /// <summary>Left-facing legs extended.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftLegsExtended = [new(-14, -1, 10, 23)];
    /// <summary>Hitbox_Ridley_FacingLeft_Hand.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftHand = [new(-15, -2, -1, 8)];
    /// <summary>Hitbox_Ridley_FacingLeft_Torso.</summary>
    internal static readonly EnemyHitboxRectangle[] LeftTorso = [new(-16, -20, 12, 21)];
    /// <summary>Hitbox_Ridley_FacingForward: the whole body of the front-facing turn pose.</summary>
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

    /// <summary>Authored origins for left-facing legs, hand, torso, and head; right-facing frames negate X.</summary>
    internal static readonly Offset16<RoomPixels>[] LeftBase =
        [new(15, 22), new(-8, 7), new(16, 0), new(-3, -24)];

    /// <summary>The side-facing body frames that face right; their component origins mirror the left-facing ones.</summary>
    internal static readonly IReadOnlySet<BodyFrame> RightFacing = new HashSet<BodyFrame>
    {
        BodyFrame.Right, BodyFrame.RightMouthHalfOpen, BodyFrame.RightMouthOpen,
        BodyFrame.RightLegsHalfExtended, BodyFrame.RightLegsExtended,
    };

    /// <summary>Each side-facing body frame's leg rectangles.</summary>
    internal static readonly Dictionary<BodyFrame, EnemyHitboxRectangle[]> Legs = new()
    {
        [BodyFrame.Left] = LeftLegsNotExtended,
        [BodyFrame.LeftMouthHalfOpen] = LeftLegsNotExtended,
        [BodyFrame.LeftMouthOpen] = LeftLegsNotExtended,
        [BodyFrame.LeftLegsHalfExtended] = LeftLegsHalfExtended,
        [BodyFrame.LeftLegsExtended] = LeftLegsExtended,
        [BodyFrame.Right] = RightLegsNotExtended,
        [BodyFrame.RightMouthHalfOpen] = RightLegsNotExtended,
        [BodyFrame.RightMouthOpen] = RightLegsNotExtended,
        [BodyFrame.RightLegsHalfExtended] = RightLegsHalfExtended,
        [BodyFrame.RightLegsExtended] = RightLegsExtended,
    };

    /// <summary>Each side-facing body frame's head rectangles.</summary>
    internal static readonly Dictionary<BodyFrame, EnemyHitboxRectangle[]> Heads = new()
    {
        [BodyFrame.Left] = LeftHeadMouthClosed,
        [BodyFrame.LeftLegsHalfExtended] = LeftHeadMouthClosed,
        [BodyFrame.LeftLegsExtended] = LeftHeadMouthClosed,
        [BodyFrame.LeftMouthHalfOpen] = LeftHeadMouthHalfOpen,
        [BodyFrame.LeftMouthOpen] = LeftHeadMouthOpen,
        [BodyFrame.Right] = RightHeadMouthClosed,
        [BodyFrame.RightLegsHalfExtended] = RightHeadMouthClosed,
        [BodyFrame.RightLegsExtended] = RightHeadMouthClosed,
        [BodyFrame.RightMouthHalfOpen] = RightHeadMouthHalfOpen,
        [BodyFrame.RightMouthOpen] = RightHeadMouthOpen,
    };
}
