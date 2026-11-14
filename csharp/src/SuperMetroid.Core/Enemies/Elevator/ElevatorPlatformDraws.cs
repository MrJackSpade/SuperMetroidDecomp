using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator platform's block draws: which room-object draws are the platform's, and the block shape each frame draws
/// from <see cref="ElevatorPlatformObjectDefinitions"/> (#1862).
/// </summary>
internal static class ElevatorPlatformDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the three platform frames.</summary>
    /// <param name="request">The draw to describe; draws of other families are not the platform's.</param>
    /// <param name="shape">The platform's blocks; null when the draw is not the platform's.</param>
    /// <returns>Whether the draw is one of the platform's frames.</returns>
    /// <exception cref="InvalidOperationException">The draw is a platform frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<ElevatorPlatformDraw> { Frame: var platform })
        {
            shape = null;
            return false;
        }
        shape = ElevatorPlatformObjectDefinitions.Shapes.TryGetValue(platform, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(ElevatorPlatformDraw)} {platform}.");
        return true;
    }
}
