using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's ceiling block draws: which room-object draws are the ceiling's, and the block shape each draws, from
/// <see cref="SporeSpawnCeilingObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class SporeSpawnCeilingDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the four ceiling frames.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the ceiling's.</param>
    /// <returns>Whether the draw is one of the ceiling's frames.</returns>
    /// <exception cref="InvalidOperationException">The draw is a ceiling frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<SporeSpawnCeilingDraw>)
        {
            shape = null;
            return false;
        }
        SporeSpawnCeilingDraw frame = RoomObjectDraw.FrameOf<SporeSpawnCeilingDraw>(request, "SporeSpawnCeilingDraw");
        shape = SporeSpawnCeilingObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(SporeSpawnCeilingDraw)} {frame}.");
        return true;
    }
}
