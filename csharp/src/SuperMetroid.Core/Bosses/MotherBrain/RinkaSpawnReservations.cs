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
    /// Reserves a point for a special Rinka whose population spawns it at <paramref name="populationSpawn"/>. It keeps
    /// that place and reserves its point when the place is on <paramref name="camera"/>'s screen and its point is free;
    /// otherwise it moves to the first free point on screen, or failing that the first free point anywhere.
    /// </summary>
    /// <returns>The point reserved, or null when every point is reserved: no point is invented or taken from its holder.</returns>
    internal RinkaSpawnReservation? Reserve(Position16<RoomPixels> populationSpawn, Position16<RoomPixels> camera)
    {
        // The Rinka's own point is the one at its population spawn; a spawn at no point's centre counts as the first
        // point, while the screen test still reads the spawn itself.
        RinkaSpawnPointDefinition own = RinkaSpawnPointDefinitions.SearchOrder.FirstOrDefault(
            definition => definition.Centre == populationSpawn, RinkaSpawnPointDefinitions.SearchOrder[0]);
        if (IsOnScreen(populationSpawn, camera) && !IsReserved(own.Point))
            return new(Take(own.Point), MovesTo: null);

        foreach (RinkaSpawnPointDefinition candidate in RinkaSpawnPointDefinitions.SearchOrder)
        {
            if (IsOnScreen(candidate.Centre, camera) && !IsReserved(candidate.Point))
                return new(Take(candidate.Point), candidate.Centre);
        }
        // The second pass ignores the screen but still refuses a reserved point.
        foreach (RinkaSpawnPointDefinition candidate in RinkaSpawnPointDefinitions.SearchOrder)
        {
            if (!IsReserved(candidate.Point))
                return new(Take(candidate.Point), candidate.Centre);
        }
        return null;
    }

    /// <summary>Releases <paramref name="point"/>, which its Rinka held until it left, died or was terminated.</summary>
    internal void Release(RinkaSpawnPoint point) => _reserved.Remove(point);

    /// <summary>Reserves <paramref name="point"/> and returns it.</summary>
    private RinkaSpawnPoint Take(RinkaSpawnPoint point)
    {
        _reserved.Add(point);
        return point;
    }

    /// <summary>
    /// Whether <paramref name="position"/> lies on the 256 by 224 pixel screen whose top-left is
    /// <paramref name="camera"/>; a position above the room's top edge never does.
    /// </summary>
    private static bool IsOnScreen(Position16<RoomPixels> position, Position16<RoomPixels> camera) =>
        CoordinateAxis.AsSigned(position.Y) >= 0 &&
        CoordinateAxis.Difference(position.Y, camera.Y) is >= 0 and < 224 &&
        CoordinateAxis.Difference(position.X, camera.X) is >= 0 and < 256;
}
