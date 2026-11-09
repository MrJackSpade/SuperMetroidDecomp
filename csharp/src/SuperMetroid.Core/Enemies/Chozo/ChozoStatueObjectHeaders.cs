using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Checks that a room object a Chozo statue publishes is one of the statues' own.</summary>
internal static class ChozoStatueObjectHeaders
{
    /// <summary>Returns <paramref name="header"/> when it is one of the five Chozo statue objects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="header"/> is not one of the five Chozo statue objects.</exception>
    internal static RoomObjectHeaderId Require(RoomObjectHeaderId header) =>
        ChozoStatueObjectDefinitions.Headers.Contains(header)
            ? header
            : throw new InvalidDataException($"{header} is not a Chozo statue's room object.");
}
