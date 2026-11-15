using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectGrappleBlockDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Grapple-block draws (#2034): each one block at the object's block, colliding as its draw installs it.</summary>
internal static class GrappleBlockObjectDraws
{
    /// <summary>
    /// The five Grapple-block frames, pictured by the installed Grapple-block frames: the terrain keeps Grapple or air
    /// collision whatever the art shows.
    /// </summary>
    internal static RoomObjectDrawFamily<GrappleBlockDraw> Family { get; } =
        RoomObjectDrawFamily<GrappleBlockDraw>.Pictured<InstalledRoomObjectGrappleBlockVisuals>("Grapple-block",
            frame => RoomBlockShapes.Block(Collision(frame)), (visuals, draw) => visuals.Image(draw));

    /// <summary>
    /// How the block a draw installs collides: the Grapple block, then the breaking and blank blocks as air.
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame is undefined.</exception>
    internal static RoomCollisionType Collision(GrappleBlockDraw frame) =>
        Collisions.TryGetValue(frame, out RoomCollisionType value) ? value : throw new InvalidOperationException($"Undefined {nameof(GrappleBlockDraw)} {frame}.");

    /// <summary>The block <paramref name="frame"/> draws and how it collides.</summary>
    internal static DrawnBlock Drawn(GrappleBlockDraw frame) => new(frame, Collision(frame));
}
