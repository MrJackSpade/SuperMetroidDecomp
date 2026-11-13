using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.OldTourianWall;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The room data each common spawner's code runs under (#1513), by the spawner the frame generator names it by
/// (<see cref="SpawnedObjectFrameDefinitions.Common"/>): the PLMs, room mains and setups that run it, and the enemies whose
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
        EnemyDefinitionId.CorpseTorizo, EnemyDefinitionId.CorpseSidehopper, EnemyDefinitionId.CorpseSidehopper2,
        EnemyDefinitionId.CorpseZoomer, EnemyDefinitionId.CorpseRipper, EnemyDefinitionId.CorpseSkree,
    ];

    /// <summary>Both Ridleys.</summary>
    private static readonly EnemyDefinitionId[] Ridleys = [EnemyDefinitionId.Ridley, EnemyDefinitionId.RidleyCeres, EnemyDefinitionId.RidleyExplosion];

    /// <summary>The enemies whose code raises the room-graphics dust and explosions.</summary>
    private static readonly EnemyDefinitionId[] DustRaisers =
    [
        .. Corpses, .. Ridleys, EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue, EnemyDefinitionId.CeresDoor,
        EnemyDefinitionId.Botwoon, EnemyDefinitionId.DraygonBody, EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonTail,
        EnemyDefinitionId.DraygonArms, EnemyDefinitionId.Kraid, EnemyDefinitionId.BabyMetroidCutscene, EnemyDefinitionId.MotherBrainBody,
        EnemyDefinitionId.MotherBrainHead, EnemyDefinitionId.Rinka, EnemyDefinitionId.PhantoonBody, EnemyDefinitionId.SporeSpawn,
        EnemyDefinitionId.Boulder,
    ];

    /// <summary>The Ceres rooms whose ceiling tiles fall during the escape.</summary>
    private static readonly RoomMainCallback[] FallingDebris = [RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris];

    /// <summary>The Ceres elevator rooms, where Samus arrives on the elevator pad.</summary>
    private static readonly RoomMainCallback[] CeresElevator = [RoomMainCallback.CeresElevatorShaft];

    /// <summary>The Tourian statue room, whose unlocking plays the statues' souls, glows and dust.</summary>
    private static readonly CommonSpawnerPresence StatueUnlocking = new() { Setups = [RoomSetupCallback.RunStatueUnlockingAnimations] };

    /// <summary>The escape rooms that set off random explosions.</summary>
    private static readonly CommonSpawnerPresence EscapeExplosions = new() { Mains = [.. ZebesEscapeDefinitions.ExplosionRoomMains] };

    /// <summary>The Ceres elevator's arrival pad and platform.</summary>
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
        Enemies = Ridleys, Mains = [RoomMainCallback.HandleCeresRidleyGetawayCutscene], Shades = [CommonSpawnerShades.RidleySprites],
    };

    /// <summary>Code every enemy runs: its death, its drop, its collision with shots.</summary>
    private static readonly CommonSpawnerPresence EveryEnemy = new() { AnyEnemy = true };

    /// <summary>Each common spawner's presence, by the name the frame generator gives it.</summary>
    internal static IReadOnlyDictionary<string, CommonSpawnerPresence> Presences { get; } = new Dictionary<string, CommonSpawnerPresence>(StringComparer.Ordinal)
    {
        [nameof(EscapeExplosionsBehavior)] = EscapeExplosions,
        [nameof(ZebesEscapeExplosionDefinitions)] = EscapeExplosions,
        [nameof(TourianUnlockBehavior)] = StatueUnlocking,
        [nameof(TourianStatueProjectileStarts)] = StatueUnlocking,
        [nameof(TourianStatueActors)] = StatueUnlocking,
        [nameof(EnemyDeathsAndDrops)] = EveryEnemy,
        [nameof(EnemyCollision)] = EveryEnemy,
        [nameof(EyeDoorBehavior)] = new() { RoomObjects = EyeDoors },
        [nameof(FallingSparksBehavior)] = new() { Enemies = [EnemyDefinitionId.Spark] },
        [nameof(NoobTubeBehavior)] = new() { RoomObjects = [RoomObjectKind.NoobTube], Shades = [CommonSpawnerShades.TubeCracks] },
        [nameof(OldTourianEscapeShaftWallBehavior)] = new()
        {
            RoomObjects = [RoomObjectKind.OldTourianEscapeShaftFakeWall], Setups = [RoomSetupCallback.AutoDestroyWallDuringEscape],
        },
        [nameof(DownwardGateBehavior)] = new() { RoomObjects = Gates },
        [Engine(nameof(EnemyProjectileEngine.SpawnSaveStationElectricity))] =
            new() { RoomObjects = [RoomObjectKind.SaveStation] },
        [Engine(nameof(EnemyProjectileEngine.CreateEnemyProjectileDudShot))] = EveryEnemy,
        [Engine(nameof(EnemyProjectileEngine.SpawnRandomEnemyDeathSprite))] = EveryEnemy,
        [Engine(nameof(EnemyProjectileEngine.SpawnRoomGraphicsDustExplosion))] = new()
        {
            Enemies = DustRaisers, Mains = FallingDebris, Shades = [CommonSpawnerShades.CeresDoorLooks],
        },
        [Engine(nameof(EnemyProjectileEngine.SpawnCeresFallingDebris))] = CeresDebris,
        [Engine(nameof(EnemyProjectileEngine.AddCeresFallingTileFrameBehavior))] = CeresDebris,
        [Engine(nameof(EnemyProjectileEngine.SpawnGunshipLiftoffDustCloud))] =
            new() { Enemies = [EnemyDefinitionId.ShipTop, EnemyDefinitionId.ShipBottomEntrance] },
        [Engine(nameof(EnemyProjectileEngine.SpawnMetalParticleBurst))] = new() { Enemies = [EnemyDefinitionId.Skree, EnemyDefinitionId.Metaree] },
        [Engine(nameof(EnemyProjectileEngine.AddMetalSkreeParticleFrameBehavior))] = new() { Enemies = [EnemyDefinitionId.Skree, EnemyDefinitionId.Metaree] },
        [nameof(RoomMainRoutines)] = new()
        {
            Mains = [.. FallingDebris, .. CeresElevator], Shades = [CommonSpawnerShades.CeresDoorLooks, CommonSpawnerShades.CeresElevatorConcealer],
        },
        [nameof(CeresElevatorArrivalState)] = ElevatorArrival,
        [nameof(CeresElevatorArrivalDefinitions)] = ElevatorArrival,
        [nameof(CeresElevatorArrivalAnimations)] = ElevatorArrival,
        [nameof(RidleyProjectiles)] = RidleyFire,
        [nameof(RidleyTail)] = RidleyGetaway,
        [nameof(CeresRidleyBehavior)] = RidleyGetaway,
        [nameof(CrocomireEncounterAccess)] = new() { Enemies = [EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue] },
        [nameof(CorpseRowDust)] = new() { Enemies = Corpses },
        [nameof(PrePhantoonRoomBehavior)] = new() { Setups = [RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile] },
    };

    /// <summary>The name of the enemy projectile engine's spawn service <paramref name="member"/>.</summary>
    private static string Engine(string member) => $"{nameof(EnemyProjectileEngine)}.{member}";
}
