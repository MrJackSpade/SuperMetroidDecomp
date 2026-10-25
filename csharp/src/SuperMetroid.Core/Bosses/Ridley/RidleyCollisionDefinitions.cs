using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
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
    private static readonly Offset16<RoomPixels> InteractionWindowMargin = new(32, 32);

    /// <summary>$A6:DE7A, CheckIfRidleyIsOffScreen: signed world coordinates and the
    /// camera-relative origin plus 32 must be inside 320 by 288 pixels.</summary>
    internal static bool IsOutsideInteractionWindow(Position16<RoomPixels> position, Position16<RoomPixels> camera)
    {
        Offset16<ScreenPixels> relative = (position + InteractionWindowMargin).RelativeToCamera(camera);
        int relativeX = relative.X;
        int relativeY = relative.Y;
        return unchecked((short)position.X) < 0 || unchecked((short)position.Y) < 0 ||
            relativeX < 0 || relativeY < 0 ||
            unchecked((short)(relativeX - 320)) >= 0 ||
            unchecked((short)(relativeY - 288)) >= 0;
    }

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
    private const short ForwardYOffset = -6;

    /// <summary>Hitbox_Ridley_FacingLeft_MouthClosed: the closed-mouth head of left-facing Ridley.</summary>
    private static readonly EnemyHitboxRectangle[] LeftHeadMouthClosed =
        [new(-12, -26, 11, 13), new(-24, 3, -13, 21)];
    /// <summary>The half-open-mouth head of left-facing Ridley.</summary>
    private static readonly EnemyHitboxRectangle[] LeftHeadMouthHalfOpen =
        [new(-41, -19, -21, -9), new(-20, -29, 11, 5)];
    /// <summary>The open-mouth head of left-facing Ridley.</summary>
    private static readonly EnemyHitboxRectangle[] LeftHeadMouthOpen =
        [new(-37, -40, -14, -31), new(-25, -31, 9, 6)];
    /// <summary>Hitbox_Ridley_FacingLeft_LegsNotExtended: left-facing standing legs.</summary>
    private static readonly EnemyHitboxRectangle[] LeftLegsNotExtended = [new(-15, -10, 7, 2)];
    /// <summary>Left-facing legs half extended.</summary>
    private static readonly EnemyHitboxRectangle[] LeftLegsHalfExtended = [new(-17, -9, 6, 15)];
    /// <summary>Left-facing legs extended.</summary>
    private static readonly EnemyHitboxRectangle[] LeftLegsExtended = [new(-14, -1, 10, 23)];
    /// <summary>Hitbox_Ridley_FacingLeft_Hand.</summary>
    private static readonly EnemyHitboxRectangle[] LeftHand = [new(-15, -2, -1, 8)];
    /// <summary>Hitbox_Ridley_FacingLeft_Torso.</summary>
    private static readonly EnemyHitboxRectangle[] LeftTorso = [new(-16, -20, 12, 21)];
    /// <summary>Hitbox_Ridley_FacingForward: the whole body of the front-facing turn pose.</summary>
    private static readonly EnemyHitboxRectangle[] Forward =
        [new(-16, -32, 16, 34), new(-8, -45, 8, -33)];
    /// <summary>The closed-mouth head of right-facing Ridley, fitted to the right-facing drawing rather than mirrored.</summary>
    private static readonly EnemyHitboxRectangle[] RightHeadMouthClosed =
        [new(-12, -25, 11, 13), new(12, 5, 24, 20)];
    /// <summary>The half-open-mouth head of right-facing Ridley.</summary>
    private static readonly EnemyHitboxRectangle[] RightHeadMouthHalfOpen =
        [new(-13, -29, 20, 5), new(21, -18, 39, -8)];
    /// <summary>The open-mouth head of right-facing Ridley.</summary>
    private static readonly EnemyHitboxRectangle[] RightHeadMouthOpen =
        [new(-10, -31, 25, 8), new(13, -42, 35, -32)];
    /// <summary>Right-facing standing legs.</summary>
    private static readonly EnemyHitboxRectangle[] RightLegsNotExtended = [new(-10, -10, 17, 2)];
    /// <summary>Right-facing legs half extended.</summary>
    private static readonly EnemyHitboxRectangle[] RightLegsHalfExtended = [new(-9, -8, 17, 15)];
    /// <summary>Right-facing legs extended.</summary>
    private static readonly EnemyHitboxRectangle[] RightLegsExtended = [new(-11, -8, 14, 23)];
    /// <summary>Right-facing hand.</summary>
    private static readonly EnemyHitboxRectangle[] RightHand = [new(1, -2, 14, 9)];
    /// <summary>Right-facing torso.</summary>
    private static readonly EnemyHitboxRectangle[] RightTorso = [new(-13, -22, 14, 21)];

    /// <summary>Authored origins for left-facing legs, hand, torso, and head; right-facing frames negate X.</summary>
    private static readonly Offset16<RoomPixels>[] LeftBase =
        [new(15, 22), new(-8, 7), new(16, 0), new(-3, -24)];

    /// <summary>Each body frame's components in native order: one for forward Ridley, four for each side-facing pose.</summary>
    private static readonly Dictionary<BodyFrame, ImmutableArray<RidleyCollisionComponent>> FrameComponents =
        Enum.GetValues<BodyFrame>().ToDictionary(frame => frame, frame => Enumerable.Range(0, frame == BodyFrame.Forward ? 1 : 4)
            .Select(component => ComponentAt(frame, component)).ToImmutableArray());

    /// <summary>Selects a component's offset and hitbox list from Ridley's pose, reflecting offsets but keeping the authored right-facing rectangles.</summary>
    private static RidleyCollisionComponent ComponentAt(BodyFrame frame, int component)
    {
        if (frame == BodyFrame.Forward) return new(new(0, ForwardYOffset), Forward);
        bool right = frame is BodyFrame.Right or BodyFrame.RightMouthHalfOpen or BodyFrame.RightMouthOpen
            or BodyFrame.RightLegsHalfExtended or BodyFrame.RightLegsExtended;
        EnemyHitboxRectangle[] hitboxes = component switch
        {
            0 => frame switch
            {
                BodyFrame.LeftLegsHalfExtended => LeftLegsHalfExtended,
                BodyFrame.RightLegsHalfExtended => RightLegsHalfExtended,
                BodyFrame.LeftLegsExtended => LeftLegsExtended,
                BodyFrame.RightLegsExtended => RightLegsExtended,
                BodyFrame.Left or BodyFrame.LeftMouthHalfOpen or BodyFrame.LeftMouthOpen => LeftLegsNotExtended,
                BodyFrame.Right or BodyFrame.RightMouthHalfOpen or BodyFrame.RightMouthOpen => RightLegsNotExtended,
                BodyFrame.Forward => throw new InvalidOperationException("Forward Ridley has a single body component."),
                _ => throw new InvalidOperationException($"Undefined BodyFrame {frame}."),
            },
            1 => right ? RightHand : LeftHand,
            2 => right ? RightTorso : LeftTorso,
            _ => frame switch
            {
                BodyFrame.LeftMouthHalfOpen => LeftHeadMouthHalfOpen,
                BodyFrame.RightMouthHalfOpen => RightHeadMouthHalfOpen,
                BodyFrame.LeftMouthOpen => LeftHeadMouthOpen,
                BodyFrame.RightMouthOpen => RightHeadMouthOpen,
                BodyFrame.Left or BodyFrame.LeftLegsHalfExtended or BodyFrame.LeftLegsExtended => LeftHeadMouthClosed,
                BodyFrame.Right or BodyFrame.RightLegsHalfExtended or BodyFrame.RightLegsExtended => RightHeadMouthClosed,
                BodyFrame.Forward => throw new InvalidOperationException("Forward Ridley has a single body component."),
                _ => throw new InvalidOperationException($"Undefined BodyFrame {frame}."),
            },
        };
        Offset16<RoomPixels> origin = LeftBase[component];
        return new(right ? origin.MirroredX() : origin, hitboxes);
    }

    /// <summary>Whether <paramref name="frame"/> shows one of Ridley's eleven body frames.</summary>
    internal static bool HasFrame(EnemyFrameId? frame) =>
        frame is { } shown && RidleyBodyFrameDefinitions.ShownFrames.TryFind(shown, out _);

    /// <summary>The computed collision components of the body frame <paramref name="frame"/> shows.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> shows no Ridley body frame.</exception>
    internal static ImmutableArray<RidleyCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && RidleyBodyFrameDefinitions.ShownFrames.TryFind(shown, out BodyFrame body)
            ? FrameComponents[body]
            : throw new InvalidDataException($"Ridley frame {frame} has no compiled collision.");
}
