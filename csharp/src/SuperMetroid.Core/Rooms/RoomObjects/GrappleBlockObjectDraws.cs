using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectGrappleBlockDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The terrain of each Grapple-block draw.</summary>
internal static class GrappleBlockObjectDraws
{
    /// <summary>
    /// The terrain a draw installs: the Grapple block, the three consecutive breakup blocks as air, and blank air.
    /// </summary>
    private static RoomTerrainBlock Terrain(GrappleBlockDraw frame) =>
        Blocks.TryGetValue(frame, out RoomTerrainBlock value) ? value : throw new InvalidOperationException($"Undefined {nameof(GrappleBlockDraw)} {frame}.");

    /// <summary>Resolves one of the five Grapple-block draws; draws of other families produce a default record.</summary>
    internal static bool TryGet(RoomObjectDraw request, out DrawList definition)
    {
        if (request is not RoomObjectFrameDraw<GrappleBlockDraw> { Frame: var frame })
        {
            definition = default;
            return false;
        }
        definition = new(frame, Terrain(frame));
        return true;
    }
}
