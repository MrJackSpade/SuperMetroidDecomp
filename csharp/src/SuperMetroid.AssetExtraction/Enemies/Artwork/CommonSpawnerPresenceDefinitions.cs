using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The room data each common spawner's code runs under (#1513), by the spawner (<see cref="CommonSpawner"/>) the frame
/// generator names (<see cref="SpawnedObjectFrameDefinitions.Common"/>): the PLMs, room mains and setups that run it, and the enemies whose
/// code calls it. A set's shared sheet holds a spawner's objects only when the set's rooms run it.
/// </summary>
internal static class CommonSpawnerPresenceDefinitions
{
    /// <summary>The eye doors, whose eyes fire, sweat and smoke.</summary>
    private static readonly RoomObjectKind[] EyeDoors =
    [
        RoomObjectKind.EyeDoorEyeFacingRight, RoomObjectKind.EyeDoorFacingRight, RoomObjectKind.EyeDoorBottomFacingRight,
        RoomObjectKind.EyeDoorEyeFacingLeft, RoomObjectKind.EyeDoorFacingLeft, RoomObjectKind.EyeDoorBottomFacingLeft,
    ];

    /// <summary>The gates whose sliding bars are enemy projectiles.</summary>
    private static readonly RoomObjectKind[] Gates =
    [
        RoomObjectKind.DownwardGate, RoomObjectKind.DownwardGateShotBlock,
    ];

    /// <summary>The corpses whose rot and crumble raise dust.</summary>
    private static readonly EnemyDefinitionId[] Corpses =
    [
        EnemyDefinitionId.CorpseTorizo, EnemyDefinitionId.CorpseSidehopper,
        EnemyDefinitionId.CorpseZoomer, EnemyDefinitionId.CorpseRipper, EnemyDefinitionId.CorpseSkree,
    ];

    /// <summary>Both Ridleys.</summary>
    private static readonly EnemyDefinitionId[] Ridleys = [EnemyDefinitionId.Ridley, EnemyDefinitionId.RidleyCeres, EnemyDefinitionId.RidleyBreakupPiece];

    /// <summary>The enemies whose code raises the room-graphics dust and explosions.</summary>
    private static readonly EnemyDefinitionId[] DustRaisers =
    [
        .. Corpses, .. Ridleys, EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue, EnemyDefinitionId.CeresElevatorDoor,
        EnemyDefinitionId.Botwoon, EnemyDefinitionId.DraygonBody, EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonTail,
        EnemyDefinitionId.DraygonArms, EnemyDefinitionId.Kraid, EnemyDefinitionId.BabyMetroidCutscene, EnemyDefinitionId.MotherBrainBody,
        EnemyDefinitionId.MotherBrainHead, EnemyDefinitionId.Rinka, EnemyDefinitionId.PhantoonBody, EnemyDefinitionId.SporeSpawn,
        EnemyDefinitionId.Boulder,
    ];

    /// <summary>The Ceres rooms whose ceiling tiles fall during the escape.</summary>
    private static readonly RoomMainBehavior[] FallingDebris = [RoomMainBehavior.SpawnCeresPreElevatorHallFallingDebris];

    /// <summary>The Ceres elevator rooms, where Samus arrives on the elevator pad.</summary>
    private static readonly RoomMainBehavior[] CeresElevator = [RoomMainBehavior.CeresElevatorShaft];

    /// <summary>The Tourian statue room, whose unlocking plays the statues' souls, glows and dust.</summary>
    private static readonly CommonSpawnerPresence StatueUnlocking = new() { Setups = [RoomSetupBehavior.RunStatueUnlockingAnimations] };

    /// <summary>The escape rooms that set off random explosions.</summary>
    private static readonly CommonSpawnerPresence EscapeExplosions = new() { Mains = [.. RoomBehaviorPictureDefinitions.EscapeExplosionMains] };

    /// <summary>The Ceres elevator's arrival pad and platform, in the concealer's shade (#2425).</summary>
    private static readonly CommonSpawnerPresence ElevatorArrival = new()
    {
        Mains = CeresElevator, Shades = [CommonSpawnerShades.CeresElevatorConcealer],
    };

    /// <summary>The escape's falling Ceres tiles.</summary>
    private static readonly CommonSpawnerPresence CeresDebris = new() { Mains = FallingDebris, Shades = [CommonSpawnerShades.CeresDoorLooks] };

    /// <summary>
    /// Ridley's fireballs, afterburns and dust, which only his fights spawn: the getaway after Ceres Ridley's fight flies
    /// him off over the room's floor and spawns none of them.
    /// </summary>
    private static readonly CommonSpawnerPresence RidleyFire = new() { Enemies = Ridleys, Shades = [CommonSpawnerShades.RidleySprites] };

    /// <summary>Ridley's fight code, with the getaway that follows Ceres Ridley's fight.</summary>
    private static readonly CommonSpawnerPresence RidleyGetaway = new()
    {
        Enemies = Ridleys, Mains = [RoomMainBehavior.CeresRidleyGetaway], Shades = [CommonSpawnerShades.RidleySprites],
    };

    /// <summary>Code every enemy runs: its death, its drop, its collision with shots.</summary>
    private static readonly CommonSpawnerPresence EveryEnemy = new() { AnyEnemy = true };

    /// <summary>Each common spawner's presence.</summary>
    internal static IReadOnlyDictionary<CommonSpawner, CommonSpawnerPresence> Presences { get; } = new Dictionary<CommonSpawner, CommonSpawnerPresence>
    {
        [CommonSpawner.EscapeExplosionsBehavior] = EscapeExplosions,
        [CommonSpawner.ZebesEscapeExplosionDefinitions] = EscapeExplosions,
        [CommonSpawner.TourianStatueActors] = StatueUnlocking,
        [CommonSpawner.EnemyDeathsAndDrops] = EveryEnemy,
        [CommonSpawner.EnemyCollision] = EveryEnemy,
        [CommonSpawner.EyeDoorBehavior] = new() { RoomObjects = EyeDoors },
        [CommonSpawner.FallingSparksBehavior] = new() { Enemies = [EnemyDefinitionId.Spark] },
        [CommonSpawner.NoobTubeBehavior] = new() { RoomObjects = [RoomObjectKind.NoobTube], Shades = [CommonSpawnerShades.TubeCracks] },
        [CommonSpawner.OldTourianEscapeShaftWallBehavior] = new() { Setups = [RoomSetupBehavior.AutoDestroyWallDuringEscape] },
        [CommonSpawner.DownwardGateBehavior] = new() { RoomObjects = Gates },
        [CommonSpawner.SaveStationElectricity] = new() { RoomObjects = [RoomObjectKind.SaveStation] },
        [CommonSpawner.EnemyProjectileEngineCreateEnemyProjectileDudShot] = EveryEnemy,
        [CommonSpawner.EnemyProjectileEngineCreateBombDudShot] = EveryEnemy,
        [CommonSpawner.EnemyProjectileEngineSpawnRandomEnemyDeathSprite] = EveryEnemy,
        [CommonSpawner.EnemyProjectileEngineSpawnDustOrExplosion] = new()
        {
            Enemies = DustRaisers, Mains = FallingDebris, Shades = [CommonSpawnerShades.CeresDoorLooks],
        },
        [CommonSpawner.CeresFallingTiles] = CeresDebris,
        [CommonSpawner.GunshipLiftoffDust] = new() { Enemies = [EnemyDefinitionId.ShipTop, EnemyDefinitionId.ShipBottomEntrance] },
        [CommonSpawner.SkreeMetareeParticles] = new() { Enemies = [EnemyDefinitionId.Skree, EnemyDefinitionId.Metaree] },
        [CommonSpawner.RoomMains] = new()
        {
            Mains = [.. FallingDebris, .. CeresElevator], Shades = [CommonSpawnerShades.CeresDoorLooks, CommonSpawnerShades.CeresElevatorConcealer],
        },
        [CommonSpawner.CeresElevatorArrivalState] = ElevatorArrival,
        [CommonSpawner.RidleyProjectiles] = RidleyFire,
        [CommonSpawner.RidleyTail] = RidleyGetaway,
        [CommonSpawner.CeresRidleyBehavior] = RidleyGetaway,
        [CommonSpawner.CrocomireEncounterAccess] = new() { Enemies = [EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue] },
        [CommonSpawner.CorpseRowDust] = new() { Enemies = Corpses },
        [CommonSpawner.PrePhantoonRoomBehavior] = new() { Setups = [RoomSetupBehavior.HoldPrePhantoonProjectilePlace] },
    };
}
