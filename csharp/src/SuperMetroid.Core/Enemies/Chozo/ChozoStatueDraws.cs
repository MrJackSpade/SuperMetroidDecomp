using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Chozo statue room objects' block draws: which room-object draws are the statues', and the block shape each draws,
/// from <see cref="ChozoStatueObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class ChozoStatueDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the statues' frames.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the statues'.</param>
    /// <returns>Whether the draw is one of the two Wrecked Ship slope-access frames or Lower Norfair's cleared hand.</returns>
    /// <exception cref="InvalidOperationException">The draw is a statue frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<ChozoStatueDraw>)
        {
            shape = null;
            return false;
        }
        ChozoStatueDraw frame = RoomObjectDraw.FrameOf<ChozoStatueDraw>(request, nameof(ChozoStatueDraw));
        shape = ChozoStatueObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(ChozoStatueDraw)} {frame}.");
        return true;
    }
}
