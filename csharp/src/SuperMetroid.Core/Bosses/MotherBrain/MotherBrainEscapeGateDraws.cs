using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's escape-gate block draws: which room-object draws are the gate's, and the block shape each draws, from
/// <see cref="MotherBrainEscapeGateObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class MotherBrainEscapeGateDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the three gate frames.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the gate's.</param>
    /// <returns>Whether the draw is one of the gate's frames; draws of other families are left unclaimed.</returns>
    /// <exception cref="InvalidOperationException">The draw is a gate frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<MotherBrainEscapeGateDraw>)
        {
            shape = null;
            return false;
        }
        MotherBrainEscapeGateDraw frame = RoomObjectDraw.FrameOf<MotherBrainEscapeGateDraw>(request, "MotherBrainEscapeGateDraw");
        shape = MotherBrainEscapeGateObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(MotherBrainEscapeGateDraw)} {request}.");
        return true;
    }
}
