using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's wall-cannon block draws: which room-object draws are the cannon's, and the block shape each draws, from
/// <see cref="DraygonCannonObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class DraygonCannonDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the twelve reachable cannon frames.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the cannon's.</param>
    /// <returns>Whether the draw is one of the cannon's frames.</returns>
    /// <exception cref="InvalidOperationException">The draw is a cannon frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<DraygonCannonDraw>)
        {
            shape = null;
            return false;
        }
        DraygonCannonDraw frame = RoomObjectDraw.FrameOf<DraygonCannonDraw>(request, "DraygonCannonDraw");
        shape = DraygonCannonObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonDraw)} {request}.");
        return true;
    }
}
