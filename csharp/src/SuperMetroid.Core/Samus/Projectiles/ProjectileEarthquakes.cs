using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Super Missile impact earthquakes: the last requested quake type and duration, published to the room's shared earthquake owner when one is bound.</summary>
internal sealed class ProjectileEarthquakes
{
    /// <summary>
    /// Nonserialized routing reference rebound before a frame; quake state itself remains
    /// in the shared room owner rather than in a second emulated state store.
    /// </summary>
    [NonSerialized]
    private RoomEnemySystem? _roomEarthquakeOwner;

    /// <summary>Reconnects the shared room quake owner before projectile collision can request an impact.</summary>
    internal void BindRoomEarthquakeOwner(RoomEnemySystem owner)
        => _roomEarthquakeOwner = Ensure.NotNull(owner);

    /// <summary>
    /// Records the Super Missile impact quake and publishes it at the collision call when a
    /// room owner is bound, allowing later quake producers to supersede this request.
    /// </summary>
    /// <param name="earthquakeType">
    /// The impact's type at $93:8125, or the enemy-hit type at $A0:9CB7/$A0:A1E9.
    /// </param>
    internal void RequestSuperMissileEarthquake(int earthquakeType)
    {
        EarthquakeType = earthquakeType;
        EarthquakeTimer = SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration;
        if (_roomEarthquakeOwner is { } room)
        {
            // Publish at the native collision call, not at frame end: later enemy/room
            // producers may replace this request, and earlier draw owners may read it.
            room.Earthquake.EarthquakeType = EarthquakeType;
            room.Earthquake.EarthquakeTimer = NativeCountdown.Start(EarthquakeTimer);
        }
    }

    /// <summary>
    /// Last impact's quake type, zero or one of the two Super Missile quake types; runtime requests also write the
    /// shared room owner, whose quake type is the same 0 through 65535 integer, immediately.
    /// </summary>
    public int EarthquakeType { get; internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }
    /// <summary>
    /// Duration in frames of the most recent Super Missile impact quake request, not a ticking countdown: zero, or
    /// the Super Missile quake duration. Cleared by <see cref="SamusProjectileSystem.ClearProjectileData"/> and
    /// published immediately to the shared room earthquake owner, whose countdown starts at it, when bound.
    /// </summary>
    public int EarthquakeTimer { get; internal set; }
}
