using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room block draws: which room-object draws are the wall fill, the escape door, the twelve
/// mural rows, the two solid rows and the six tube clears, and the block shape each draws, from
/// <see cref="MotherBrainFakeDeathObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class MotherBrainFakeDeathDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the twenty-two fake-death room frames.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not one of the fake-death room's.</param>
    /// <returns>Whether the draw is one of the fake-death room's frames; draws of other families are left unclaimed.</returns>
    /// <exception cref="InvalidOperationException">The draw is a fake-death room frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<MotherBrainFakeDeathDraw>)
        {
            shape = null;
            return false;
        }
        MotherBrainFakeDeathDraw frame = RoomObjectDraw.FrameOf<MotherBrainFakeDeathDraw>(request, "MotherBrainFakeDeathDraw");
        shape = MotherBrainFakeDeathObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(MotherBrainFakeDeathDraw)} {request}.");
        return true;
    }
}
