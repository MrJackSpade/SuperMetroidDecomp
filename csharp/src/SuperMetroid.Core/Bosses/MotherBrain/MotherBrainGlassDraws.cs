using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's glass block draws: which room-object draws are the glass's, and the block shape each draws, from
/// <see cref="MotherBrainGlassObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class MotherBrainGlassDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the eleven glass frames.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the glass's.</param>
    /// <returns>Whether the draw is one of the glass's frames; draws of other families are left unclaimed.</returns>
    /// <exception cref="InvalidOperationException">The draw is a glass frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<MotherBrainGlassDraw>)
        {
            shape = null;
            return false;
        }
        MotherBrainGlassDraw frame = RoomObjectDraw.FrameOf<MotherBrainGlassDraw>(request, "MotherBrainGlassDraw");
        shape = MotherBrainGlassObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(MotherBrainGlassDraw)} {request}.");
        return true;
    }
}
