using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Super Missile impact earthquakes, started on the room's earthquake, their one owner, when the impact happens.</summary>
[RoomLifetime]
internal sealed class ProjectileEarthquakes
{
    /// <summary>The room's earthquake, bound when the room loads.</summary>
    private RoomEarthquake? _roomEarthquake;

    /// <summary>Reconnects the shared room quake owner before projectile collision can request an impact.</summary>
    internal void BindRoomEarthquake(RoomEarthquake earthquake)
        => _roomEarthquake = Ensure.NotNull(earthquake);

    /// <summary>
    /// Starts the Super Missile impact quake on the room's earthquake at the collision call, not at frame end: later
    /// enemy and room producers may replace it, and earlier draw owners may read it.
    /// </summary>
    /// <param name="shake">The impact's shake on a block, or the enemy-hit shake.</param>
    /// <exception cref="InvalidOperationException">No room earthquake is bound.</exception>
    internal void RequestSuperMissileEarthquake(EarthquakeShake shake) =>
        (_roomEarthquake ?? throw new InvalidOperationException(
            "A Super Missile impact found no room earthquake bound to start."))
            .Start(shake, SuperMissileDefinitions.EarthquakeDuration);
}
