namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// One common spawner (#1513, #2548): code outside the enemy behaviours whose spawns go on the shared sheet of the sets
/// whose rooms run it. The frame generator names each one by the code that spawns, and <see cref="CommonSpawnerKeys"/>
/// writes out the key the installed spawned-object sheets name it by.
/// </summary>
internal enum CommonSpawner
{
    /// <summary>The escape's random explosions over the escape rooms.</summary>
    EscapeExplosionsBehavior,

    /// <summary>The explosions the Zebes escape sets off.</summary>
    ZebesEscapeExplosionDefinitions,

    /// <summary>The Tourian statues' souls, glows and dust as the statues unlock.</summary>
    TourianStatueActors,

    /// <summary>Every enemy's death and the drop it leaves.</summary>
    EnemyDeathsAndDrops,

    /// <summary>Every enemy's collision with Samus's shots.</summary>
    EnemyCollision,

    /// <summary>The eye doors' shots, sweat and smoke.</summary>
    EyeDoorBehavior,

    /// <summary>The sparks falling from the ceiling sparks.</summary>
    FallingSparksBehavior,

    /// <summary>The glass tube's cracks and shards as it shatters.</summary>
    NoobTubeBehavior,

    /// <summary>The old Tourian escape shaft's wall as it explodes away.</summary>
    OldTourianEscapeShaftWallBehavior,

    /// <summary>The downward gates' sliding bars.</summary>
    DownwardGateBehavior,

    /// <summary>The save station's electricity.</summary>
    SaveStationElectricity,

    /// <summary>The dud shot an enemy immune to a shot leaves.</summary>
    EnemyProjectileEngineCreateEnemyProjectileDudShot,

    /// <summary>The dud a bomb leaves on an enemy it cannot hurt.</summary>
    EnemyProjectileEngineCreateBombDudShot,

    /// <summary>The random death sprite an enemy shows as it dies.</summary>
    EnemyProjectileEngineSpawnRandomEnemyDeathSprite,

    /// <summary>The dust and explosions room graphics raise.</summary>
    EnemyProjectileEngineSpawnDustOrExplosion,

    /// <summary>The Ceres ceiling tiles falling during the escape.</summary>
    CeresFallingTiles,

    /// <summary>The dust the gunship raises as it lifts off.</summary>
    GunshipLiftoffDust,

    /// <summary>The particles Skrees and Metarees burst into.</summary>
    SkreeMetareeParticles,

    /// <summary>The room main routines' debris and the Ceres elevator's concealer.</summary>
    RoomMains,

    /// <summary>The Ceres elevator's arrival pad and platform (#2425).</summary>
    CeresElevatorArrivalState,

    /// <summary>Ridley's fireballs, afterburns and dust.</summary>
    RidleyProjectiles,

    /// <summary>Ridley's tail.</summary>
    RidleyTail,

    /// <summary>Ceres Ridley's fight and getaway.</summary>
    CeresRidleyBehavior,

    /// <summary>Crocomire's fight.</summary>
    CrocomireEncounterAccess,

    /// <summary>The dust a row of rotting corpses raises.</summary>
    CorpseRowDust,

    /// <summary>The room before Phantoon's, which holds its projectile place.</summary>
    PrePhantoonRoomBehavior,
}
