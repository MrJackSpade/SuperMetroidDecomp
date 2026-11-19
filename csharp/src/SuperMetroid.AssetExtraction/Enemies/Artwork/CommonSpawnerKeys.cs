using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The installed key of each <see cref="CommonSpawner"/> (#2548), written out so renaming the code a spawner is named for
/// never changes the name the installed spawned-object sheets' common lists spell it by.
/// </summary>
internal static class CommonSpawnerKeys
{
    /// <summary>Each CommonSpawner member's installed key.</summary>
    internal static InstalledKeys<CommonSpawner> Installed { get; } = new(new Dictionary<CommonSpawner, string>
    {
        [CommonSpawner.EscapeExplosionsBehavior] = "EscapeExplosionsBehavior",
        [CommonSpawner.ZebesEscapeExplosionDefinitions] = "ZebesEscapeExplosionDefinitions",
        [CommonSpawner.TourianStatueActors] = "TourianStatueActors",
        [CommonSpawner.EnemyDeathsAndDrops] = "EnemyDeathsAndDrops",
        [CommonSpawner.EnemyCollision] = "EnemyCollision",
        [CommonSpawner.EyeDoorBehavior] = "EyeDoorBehavior",
        [CommonSpawner.FallingSparksBehavior] = "FallingSparksBehavior",
        [CommonSpawner.NoobTubeBehavior] = "NoobTubeBehavior",
        [CommonSpawner.OldTourianEscapeShaftWallBehavior] = "OldTourianEscapeShaftWallBehavior",
        [CommonSpawner.DownwardGateBehavior] = "DownwardGateBehavior",
        [CommonSpawner.SaveStationElectricity] = "SaveStationElectricity",
        [CommonSpawner.EnemyProjectileEngineCreateEnemyProjectileDudShot] = "EnemyProjectileEngine.CreateEnemyProjectileDudShot",
        [CommonSpawner.EnemyProjectileEngineCreateBombDudShot] = "EnemyProjectileEngine.CreateBombDudShot",
        [CommonSpawner.EnemyProjectileEngineSpawnRandomEnemyDeathSprite] = "EnemyProjectileEngine.SpawnRandomEnemyDeathSprite",
        [CommonSpawner.EnemyProjectileEngineSpawnDustOrExplosion] = "EnemyProjectileEngine.SpawnDustOrExplosion",
        [CommonSpawner.CeresFallingTiles] = "CeresFallingTiles",
        [CommonSpawner.GunshipLiftoffDust] = "GunshipLiftoffDust",
        [CommonSpawner.SkreeMetareeParticles] = "SkreeMetareeParticles",
        [CommonSpawner.RoomMains] = "RoomMainRoutines",
        [CommonSpawner.CeresElevatorArrivalState] = "CeresElevatorArrivalState",
        [CommonSpawner.RidleyProjectiles] = "RidleyProjectiles",
        [CommonSpawner.RidleyTail] = "RidleyTail",
        [CommonSpawner.CeresRidleyBehavior] = "CeresRidleyBehavior",
        [CommonSpawner.CrocomireEncounterAccess] = "CrocomireEncounterAccess",
        [CommonSpawner.CorpseRowDust] = "CorpseRowDust",
        [CommonSpawner.PrePhantoonRoomBehavior] = "PrePhantoonRoomBehavior",
    });
}
