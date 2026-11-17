using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Super Missile impact earthquakes, started on the room's earthquake, their one owner, when the impact happens.</summary>
/// <param name="roomEarthquake">The room's earthquake, composed with the room's projectiles and enemies.</param>
[RoomLifetime]
internal sealed class ProjectileEarthquakes(RoomEarthquake roomEarthquake)
{
    /// <summary>The room's earthquake.</summary>
    private readonly RoomEarthquake _roomEarthquake = Ensure.NotNull(roomEarthquake);

    /// <summary>
    /// Starts the Super Missile impact quake on the room's earthquake at the collision call, not at frame end: later
    /// enemy and room producers may replace it, and earlier draw owners may read it.
    /// </summary>
    /// <param name="shake">The impact's shake on a block, or the enemy-hit shake.</param>
    internal void RequestSuperMissileEarthquake(EarthquakeShake shake) =>
        _roomEarthquake.Start(shake, SuperMissileDefinitions.EarthquakeDuration);
}
