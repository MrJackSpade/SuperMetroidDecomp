using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Which of Mother Brain's Rinka spawn points are reserved in this room, and how a special Rinka reserves one (#1699).
/// A point stays reserved until the Rinka holding it releases it, so no two Rinkas share a point.
/// </summary>
[RoomLifetime]
internal sealed class RinkaSpawnReservations
{
    /// <summary>The points reserved and not yet released.</summary>
    private readonly HashSet<RinkaSpawnPoint> _reserved = [];

    /// <summary>Whether a Rinka holds <paramref name="point"/>.</summary>
    internal bool IsReserved(RinkaSpawnPoint point) => _reserved.Contains(point);

    /// <summary>
    /// Reserves a point for a special Rinka whose spawn point is <paramref name="own"/>. It keeps that point when the
    /// point is on <paramref name="camera"/>'s screen and free; otherwise it takes the first free point on screen, or
    /// failing that the first free point anywhere, which becomes its spawn point (#2256).
    /// </summary>
    /// <returns>The point reserved, or null when every point is reserved: no point is invented or taken from its holder.</returns>
    internal RinkaSpawnPoint? Reserve(RinkaSpawnPoint own, Position16<RoomPixels> camera)
    {
        if (IsOnScreen(Centre(own), camera) && !IsReserved(own))
            return Take(own);

        foreach (RinkaSpawnPointDefinition candidate in RinkaSpawnPointDefinitions.SearchOrder)
        {
            if (IsOnScreen(candidate.Centre, camera) && !IsReserved(candidate.Point))
                return Take(candidate.Point);
        }
        // The second pass ignores the screen but still refuses a reserved point.
        foreach (RinkaSpawnPointDefinition candidate in RinkaSpawnPointDefinitions.SearchOrder)
        {
            if (!IsReserved(candidate.Point))
                return Take(candidate.Point);
        }
        return null;
    }

    /// <summary>Releases <paramref name="point"/>, which its Rinka held until it left, died or was terminated.</summary>
    internal void Release(RinkaSpawnPoint point) => _reserved.Remove(point);

    /// <summary>Where a Rinka placed at <paramref name="point"/> appears.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="point"/> is not one of the room's spawn points.</exception>
    internal static Position16<RoomPixels> Centre(RinkaSpawnPoint point)
    {
        foreach (RinkaSpawnPointDefinition definition in RinkaSpawnPointDefinitions.SearchOrder)
        {
            if (definition.Point == point)
                return definition.Centre;
        }
        throw new InvalidOperationException($"Rinka spawn point {point} has no place in Mother Brain's room.");
    }

    /// <summary>Reserves <paramref name="point"/> and returns it.</summary>
    private RinkaSpawnPoint Take(RinkaSpawnPoint point)
    {
        _reserved.Add(point);
        return point;
    }

    /// <summary>
    /// Whether <paramref name="position"/> lies on the screen whose top-left is
    /// <paramref name="camera"/>; a position above the room's top edge never does.
    /// </summary>
    private static bool IsOnScreen(Position16<RoomPixels> position, Position16<RoomPixels> camera) =>
        !OffRoomPlaces.LiesBeforeRoom(position.Y) &&
        CameraPastRoomEdge.FromCamera(position.Y, camera.Y) is >= 0 and < ScreenGeometry.ScreenHeightPixels &&
        CameraPastRoomEdge.FromCamera(position.X, camera.X) is >= 0 and < ScreenGeometry.ScreenWidthPixels;
}
