using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Ridley.RidleyCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collision of Ridley's body: whether it is inside the interaction window, and the components of the body frame it shows.</summary>
internal static class RidleyCollision
{
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

    /// <summary>Each body frame's components in native order: one for forward Ridley, four for each side-facing pose.</summary>
    private static readonly Dictionary<BodyFrame, ImmutableArray<RidleyCollisionComponent>> FrameComponents =
        Enum.GetValues<BodyFrame>().ToDictionary(frame => frame, frame => Enumerable.Range(0, frame == BodyFrame.Forward ? 1 : 4)
            .Select(component => ComponentAt(frame, component)).ToImmutableArray());

    /// <summary>Selects a component's offset and hitbox list from Ridley's pose, reflecting offsets but keeping the authored right-facing rectangles.</summary>
    private static RidleyCollisionComponent ComponentAt(BodyFrame frame, int component)
    {
        if (frame == BodyFrame.Forward) return new(new(0, ForwardYOffset), Forward);
        bool right = RightFacing.Contains(frame);
        EnemyHitboxRectangle[] hitboxes = component switch
        {
            0 => Legs.TryGetValue(frame, out EnemyHitboxRectangle[]? legs)
                ? legs
                : throw new InvalidOperationException($"Undefined BodyFrame {frame}."),
            1 => right ? RightHand : LeftHand,
            2 => right ? RightTorso : LeftTorso,
            _ => Heads.TryGetValue(frame, out EnemyHitboxRectangle[]? head)
                ? head
                : throw new InvalidOperationException($"Undefined BodyFrame {frame}."),
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
