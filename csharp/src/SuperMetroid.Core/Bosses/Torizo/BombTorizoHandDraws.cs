using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb Torizo's hand block draws: which room-object draws are the hand's, and the block shape each draws, from
/// <see cref="BombTorizoHandObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class BombTorizoHandDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is the intact or cleared hand.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the hand's.</param>
    /// <returns>Whether the draw is one of the hand's frames.</returns>
    /// <exception cref="InvalidOperationException">The draw is a hand frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<BombTorizoHandDraw>)
        {
            shape = null;
            return false;
        }
        BombTorizoHandDraw frame = RoomObjectDraw.FrameOf<BombTorizoHandDraw>(request, "BombTorizoHandDraw");
        shape = BombTorizoHandObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(BombTorizoHandDraw)} {frame}.");
        return true;
    }
}
