using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's bridge and invisible-wall block draws: which room-object draws are the arena's, and the block shape each
/// draws, from <see cref="CrocomireArenaObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class CrocomireArenaDraws
{
    /// <summary>The block shape of <paramref name="draw"/> when it is one of the five arena frames.</summary>
    /// <param name="draw">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the arena's.</param>
    /// <returns>Whether the draw is one of the arena's frames.</returns>
    /// <exception cref="InvalidOperationException">The draw is an arena frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw draw, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (draw is not RoomObjectFrameDraw<CrocomireArenaDraw> { Frame: var frame })
        {
            shape = null;
            return false;
        }
        shape = CrocomireArenaObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(CrocomireArenaDraw)} {frame}.");
        return true;
    }
}
