using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block shape each station draw draws (<see cref="StationObjectDrawDefinitions"/>, #1862). What each frame shows is
/// its installed picture (#1557).
/// </summary>
internal static class StationObjectDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the stations' draws.</summary>
    /// <param name="request">The draw to describe; draws of other families are not the stations'.</param>
    /// <param name="shape">The station's blocks; null when the draw is not a station's.</param>
    /// <returns>Whether the draw is one of the stations'.</returns>
    /// <exception cref="InvalidOperationException">The draw is a station draw with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<StationDraw> { Frame: var station })
        {
            shape = null;
            return false;
        }
        shape = StationObjectDrawDefinitions.Shapes.TryGetValue(station, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined station draw {request}.");
        return true;
    }
}
