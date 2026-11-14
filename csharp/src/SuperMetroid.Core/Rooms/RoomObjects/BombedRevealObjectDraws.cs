using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectBombedRevealDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The one-block shape of each bombed reveal (#1862).</summary>
internal static class BombedRevealObjectDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the three reveals.</summary>
    /// <param name="request">The draw to describe; draws outside the three reveals belong to other families.</param>
    /// <param name="shape">The reveal's block; null when the draw is not a reveal.</param>
    /// <returns>Whether the draw is one of the reveals.</returns>
    /// <exception cref="InvalidOperationException">The draw is a reveal with no collision.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<RoomObjectBombedRevealDraw>)
        {
            shape = null;
            return false;
        }
        RoomObjectBombedRevealDraw frame = RoomObjectDraw.FrameOf<RoomObjectBombedRevealDraw>(request, "RoomObjectBombedRevealDraw");
        shape = RoomBlockShapes.Block(Collisions.TryGetValue(frame, out RoomCollisionType found)
            ? found
            : throw new InvalidOperationException($"Undefined bombed-reveal draw {request}."));
        return true;
    }
}
