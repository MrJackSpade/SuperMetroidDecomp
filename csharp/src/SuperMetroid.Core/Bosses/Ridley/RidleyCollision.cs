using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Ridley.RidleyCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collision of Ridley's body: whether it is inside the interaction window, and the components of the body frame it shows.</summary>
internal static class RidleyCollision
{
    /// <summary>
    /// Ridley's off-screen test: he is outside while he lies above or left of the room, or while his place moved by the
    /// window's margin lies outside the window from the camera.
    /// </summary>
    internal static bool IsOutsideInteractionWindow(Position16<RoomPixels> position, Position16<RoomPixels> camera)
    {
        Offset16<RoomPixels> place = RidleyFlight.SignedPlace(position);
        int relativeX = place.X + InteractionWindowMargin.X - camera.X;
        int relativeY = place.Y + InteractionWindowMargin.Y - camera.Y;
        return place.X < 0 || place.Y < 0 ||
            relativeX < 0 || relativeY < 0 ||
            relativeX >= InteractionWindowSize.X ||
            relativeY >= InteractionWindowSize.Y;
    }

    /// <summary>Each body frame's components in authored order: Ridley facing forward, or a side-facing pose's legs, hand, torso and head.</summary>
    private static readonly Dictionary<EnemyFrameId, ImmutableArray<RidleyCollisionComponent>> FrameComponents =
        BodyFrames.ToDictionary(frame => frame, ComponentsOf);

    /// <summary>
    /// The components of <paramref name="frame"/>: the forward body alone, or the side-facing legs, hand, torso and head
    /// (#1937). A right-facing pose mirrors the left-facing origins but keeps its own authored rectangles.
    /// </summary>
    private static ImmutableArray<RidleyCollisionComponent> ComponentsOf(EnemyFrameId frame)
    {
        if (frame == EnemyFrameId.RidleyBodyForward)
            return [new(new(0, ForwardYOffset), Forward)];
        RidleySideBodyCollision side = SideFacing.TryGetValue(frame, out RidleySideBodyCollision? shown)
            ? shown
            : throw new InvalidOperationException($"{frame} is no side-facing Ridley body frame.");
        bool right = side.Facing == SuperMetroid.Core.Spatial.HorizontalDirection.Right;
        return
        [
            Side(LeftLegsOrigin, side.Legs),
            Side(LeftHandOrigin, right ? RightHand : LeftHand),
            Side(LeftTorsoOrigin, right ? RightTorso : LeftTorso),
            Side(LeftHeadOrigin, side.Head),
        ];

        RidleyCollisionComponent Side(Offset16<RoomPixels> leftOrigin, EnemyHitboxRectangle[] hitboxes) =>
            new(right ? leftOrigin.MirroredX() : leftOrigin, hitboxes);
    }

    /// <summary>The computed collision components of the body frame <paramref name="frame"/> shows.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> shows no Ridley body frame.</exception>
    internal static ImmutableArray<RidleyCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && FrameComponents.TryGetValue(shown, out ImmutableArray<RidleyCollisionComponent> components)
            ? components
            : throw new InvalidDataException($"Ridley frame {frame} has no collision.");
}
