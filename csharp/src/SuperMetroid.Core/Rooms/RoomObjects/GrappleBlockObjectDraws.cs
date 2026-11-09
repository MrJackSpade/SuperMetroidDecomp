using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectGrappleBlockDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How the block of each Grapple-block draw collides.</summary>
internal static class GrappleBlockObjectDraws
{
    /// <summary>
    /// How the block a draw installs collides: the Grapple block, then the three breakup frames and blank as air.
    /// </summary>
    private static RoomCollisionType Collision(GrappleBlockDraw frame) =>
        Collisions.TryGetValue(frame, out RoomCollisionType value) ? value : throw new InvalidOperationException($"Undefined {nameof(GrappleBlockDraw)} {frame}.");

    /// <summary>Resolves one of the five Grapple-block draws; draws of other families produce a default record.</summary>
    internal static bool TryGet(RoomObjectDraw request, out DrawList definition)
    {
        if (request is not RoomObjectFrameDraw<GrappleBlockDraw> { Frame: var frame })
        {
            definition = default;
            return false;
        }
        definition = new(frame, Collision(frame));
        return true;
    }
}
