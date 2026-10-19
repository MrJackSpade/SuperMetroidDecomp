using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Enemies;

/// <summary>The enemy operations the room loader runs at its fixed points.</summary>
internal interface IEnemyLoaderHooks
{
    /// <summary>Whether Rinka spawns still wait for the loader's camera.</summary>
    bool HasDeferredRinkaLoaderTimeCameraReads { get; }

    /// <summary>Completes the Rinka spawns deferred to the loader's camera.</summary>
    void CompleteRinkaLoaderTimeCameraReads(Func<int, bool> loaderInitializedSlot, Position16<RoomPixels> camera);

    /// <summary>Spawns the pre-Phantoon room's setup projectile.</summary>
    void SpawnPrePhantoonRoomProjectile(BackgroundScrollState backgroundScroll);
}
