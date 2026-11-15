using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Ridley.RidleyCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collision of Ridley's body: whether it is inside the interaction window, and the components of the body frame it shows.</summary>
internal static class RidleyCollision
{
    /// <summary>Ridley's off-screen test: signed world coordinates and the
    /// camera-relative origin plus 32 must be inside 320 by 288 pixels.</summary>
    internal static bool IsOutsideInteractionWindow(Position16<RoomPixels> position, Position16<RoomPixels> camera)
    {
        Offset16<ScreenPixels> relative = (position + InteractionWindowMargin).RelativeToCamera(camera);
        int relativeX = relative.X;
        int relativeY = relative.Y;
        return CoordinateAxis.AsSigned(position.X) < 0 || CoordinateAxis.AsSigned(position.Y) < 0 ||
            relativeX < 0 || relativeY < 0 ||
            CoordinateAxis.Difference(relativeX, 320) >= 0 ||
            CoordinateAxis.Difference(relativeY, 288) >= 0;
    }

    /// <summary>Each body frame's components in authored order: Ridley facing forward, or a side-facing pose's legs, hand, torso and head.</summary>
    private static readonly Dictionary<BodyFrame, ImmutableArray<RidleyCollisionComponent>> FrameComponents =
        Enum.GetValues<BodyFrame>().ToDictionary(frame => frame, ComponentsOf);

    /// <summary>
    /// The components of <paramref name="frame"/>: the forward body alone, or the side-facing legs, hand, torso and head
    /// (#1937). A right-facing pose mirrors the left-facing origins but keeps its own authored rectangles.
    /// </summary>
    private static ImmutableArray<RidleyCollisionComponent> ComponentsOf(BodyFrame frame)
    {
        if (frame == BodyFrame.Forward)
            return [new(new(0, ForwardYOffset), Forward)];
        bool right = RightFacing.Contains(frame);
        EnemyHitboxRectangle[] legs = Legs.TryGetValue(frame, out EnemyHitboxRectangle[]? shownLegs)
            ? shownLegs
            : throw new InvalidOperationException($"Undefined BodyFrame {frame}.");
        EnemyHitboxRectangle[] head = Heads.TryGetValue(frame, out EnemyHitboxRectangle[]? shownHead)
            ? shownHead
            : throw new InvalidOperationException($"Undefined BodyFrame {frame}.");
        return
        [
            Side(LeftLegsOrigin, legs),
            Side(LeftHandOrigin, right ? RightHand : LeftHand),
            Side(LeftTorsoOrigin, right ? RightTorso : LeftTorso),
            Side(LeftHeadOrigin, head),
        ];

        RidleyCollisionComponent Side(Offset16<RoomPixels> leftOrigin, EnemyHitboxRectangle[] hitboxes) =>
            new(right ? leftOrigin.MirroredX() : leftOrigin, hitboxes);
    }

    /// <summary>The computed collision components of the body frame <paramref name="frame"/> shows.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> shows no Ridley body frame.</exception>
    internal static ImmutableArray<RidleyCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && RidleyBodyFrameDefinitions.ShownFrames.TryFind(shown, out BodyFrame body)
            ? FrameComponents[body]
            : throw new InvalidDataException($"Ridley frame {frame} has no compiled collision.");
}
