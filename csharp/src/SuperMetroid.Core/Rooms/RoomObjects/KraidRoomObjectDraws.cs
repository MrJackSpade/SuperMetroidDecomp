using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Kraid's room's ceiling and spike block draws: which room-object draws are the room's, and the block shape each draws,
/// from <see cref="KraidRoomObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class KraidRoomObjectDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the ten Kraid room frames.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the room's.</param>
    /// <returns>Whether the draw is one of the Kraid room's frames.</returns>
    /// <exception cref="InvalidOperationException">The draw is a Kraid room frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (!RoomObjectDraw.TryFrame(request, out KraidRoomDraw frame))
        {
            shape = null;
            return false;
        }
        shape = KraidRoomObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {frame}.");
        return true;
    }
}
