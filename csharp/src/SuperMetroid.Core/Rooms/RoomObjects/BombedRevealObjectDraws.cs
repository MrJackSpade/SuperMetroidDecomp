using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectBombedRevealDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The single-block draw of each bombed reveal.</summary>
internal static class BombedRevealObjectDraws
{
    /// <summary>
    /// Describes a draw handed over by the shared room-object draw dispatch; draws outside the three reveals belong to
    /// other families.
    /// </summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList draw)
    {
        if (request is not RoomObjectFrameDraw<RoomObjectBombedRevealDraw>)
        {
            draw = default;
            return false;
        }
        RoomObjectBombedRevealDraw frame = RoomObjectDraw.FrameOf<RoomObjectBombedRevealDraw>(request, "RoomObjectBombedRevealDraw");
        RoomCollisionType collision = Collisions.TryGetValue(frame, out RoomCollisionType found)
            ? found
            : throw new InvalidOperationException($"Undefined bombed-reveal draw {request}.");
        draw = new(request, new RoomObjectShotBlockDrawDefinitions.Run[] { new(RoomObjectRunDirection.Horizontal, new[] { collision }, new(0, 0)) });
        return true;
    }
}
