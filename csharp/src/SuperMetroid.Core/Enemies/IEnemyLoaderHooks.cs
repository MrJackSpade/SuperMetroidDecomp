namespace SuperMetroid.Core.Enemies;

/// <summary>The enemy operations the room loader runs at its fixed points.</summary>
internal interface IEnemyLoaderHooks
{
    /// <summary>Whether enemies whose setup reads the camera as the load progresses still wait for the loader's camera.</summary>
    bool HasDeferredLoaderTimeCameraReads { get; }

    /// <summary>Completes the setups deferred to the loader's camera, of the enemies in places the loader has initialized.</summary>
    void CompleteLoaderTimeCameraReads(Func<int, bool> loaderInitializedPlace, Position16<RoomPixels> camera);

    /// <summary>Spawns the pre-Phantoon room's setup projectile.</summary>
    void SpawnPrePhantoonRoomProjectile();

    /// <summary>Whether a boss whose encounter hides the HUD's minimap is present in the room.</summary>
    bool BossHidesMinimap { get; }

    /// <summary>How many map rows, from the room's map position down, the present boss's encounter marks explored.</summary>
    int BossArenaMapRows { get; }
}
