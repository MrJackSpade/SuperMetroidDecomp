namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    // This is a routing reference, not additional emulated state. The runtime restores
    // it before executing a frame; the actual shared quake words remain in the room owner.
    [NonSerialized]
    private RoomEnemySystem? _roomEarthquakeOwner;

    internal void BindRoomEarthquakeOwner(RoomEnemySystem owner)
        => _roomEarthquakeOwner = owner ?? throw new ArgumentNullException(nameof(owner));

    private void RequestSuperMissileEarthquake()
    {
        EarthquakeType = SamusProjectileRomData.NonBeam.SuperMissileEarthquakeType;
        EarthquakeTimer = SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration;
        if (_roomEarthquakeOwner is { } room)
        {
            // Publish at the native collision call, not at frame end: later enemy/room
            // producers may replace this request, and earlier draw owners may read it.
            room.EarthquakeType = EarthquakeType;
            room.EarthquakeTimer = EarthquakeTimer;
        }
    }
}
