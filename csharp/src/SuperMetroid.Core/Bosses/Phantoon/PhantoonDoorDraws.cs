using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's door block draws: which room-object draws are the door's, and the block shape each draws, from
/// <see cref="PhantoonDoorObjectDrawDefinitions"/>. What each frame shows is its installed picture.
/// </summary>
internal static class PhantoonDoorDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of Phantoon's door frames.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the door's.</param>
    /// <returns>Whether the draw is one of the door's frames.</returns>
    /// <exception cref="InvalidOperationException">The draw is a door frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (!RoomObjectDraw.TryFrame(request, out PhantoonDoorDraw frame))
        {
            shape = null;
            return false;
        }
        shape = PhantoonDoorObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(PhantoonDoorDraw)} {frame}.");
        return true;
    }
}
