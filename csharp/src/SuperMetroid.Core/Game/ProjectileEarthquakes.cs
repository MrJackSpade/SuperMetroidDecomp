using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Game;

/// <summary>Super Missile impact earthquakes: the last requested quake type and duration, published to the room's shared earthquake owner when one is bound.</summary>
internal sealed class ProjectileEarthquakes
{
    // This is a routing reference, not additional emulated state. The runtime restores
    // it before executing a frame; the actual shared quake words remain in the room owner.
    [NonSerialized]
    private RoomEnemySystem? _roomEarthquakeOwner;

    internal void BindRoomEarthquakeOwner(RoomEnemySystem owner)
        => _roomEarthquakeOwner = Ensure.NotNull(owner);

    /// <param name="earthquakeType">
    /// The impact's type at $93:8125, or the enemy-hit type at $A0:9CB7/$A0:A1E9.
    /// </param>
    internal void RequestSuperMissileEarthquake(ushort earthquakeType)
    {
        EarthquakeType = earthquakeType;
        EarthquakeTimer = SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration;
        if (_roomEarthquakeOwner is { } room)
        {
            // Publish at the native collision call, not at frame end: later enemy/room
            // producers may replace this request, and earlier draw owners may read it.
            room.Earthquake.EarthquakeType = EarthquakeType;
            room.Earthquake.EarthquakeTimer = EarthquakeTimer;
        }
    }

    /// <summary>Last impact's quake request; runtime requests also write the shared room owner immediately.</summary>
    public ushort EarthquakeType { get; internal set; }
    /// <summary>Duration of the most recent Super Missile impact quake request, not a ticking countdown; cleared by <see cref="SamusProjectileSystem.ResetForRoomTransition"/> and published immediately to the shared room earthquake owner when bound.</summary>
    public ushort EarthquakeTimer { get; internal set; }
}
