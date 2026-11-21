using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Brinstar room-header region, in table order. It holds the Tourian statue rooms, whose headers lie in this region.</summary>
internal static class BrinstarRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The members of every set here that loads a morph ball eye and a face block, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] MorphBallEyeAndFaceBlock =
    [
        new(EnemyDefinitionId.MorphBallEye, row: 7), new(EnemyDefinitionId.FaceBlock, row: 7),
    ];

    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.BrinstarPreMap, [
            new(EnemyDefinitionId.Sidehopper, row: 1),
            new(EnemyDefinitionId.Zeela, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMap, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMissileRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SporeSpawnKihunters, [
            new(EnemyDefinitionId.KihunterGreen, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EtecoonSuper, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DachoraEnergyRefill, [
            new(EnemyDefinitionId.FaceBlock, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SporeSpawnFarming, [
            new(EnemyDefinitionId.Zeb, row: 1),
            new(EnemyDefinitionId.Zeela, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Statues, [
            new(EnemyDefinitionId.TourianStatue, row: 1),
            new(UnplacedEnemyArtwork.TourianStatueGhosts, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.HopperEnergyTank, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BetaPowerBomb, [
            new(EnemyDefinitionId.YappingMaw, row: 1),
            new(EnemyDefinitionId.SidehopperLarge, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WaterwayEnergyTank, [
            new(EnemyDefinitionId.Skultera, row: 1),
            new(EnemyDefinitionId.Puyo, row: 2),
            new(EnemyDefinitionId.Zero, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ConstructionZoneWhenZebesAwake, [
            new(EnemyDefinitionId.Zoomer, row: 1),
            new(EnemyDefinitionId.FaceBlock, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BlueBrinstarEnergyTankWhenZebesAwake, [
            new(EnemyDefinitionId.FaceBlock, row: 1),
            new(EnemyDefinitionId.Zoomer, row: 2),
            new(EnemyDefinitionId.Skree, row: 3),
            new(EnemyDefinitionId.Rio, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.NoobBridge, [
            new(EnemyDefinitionId.Cacatac, row: 1),
            new(EnemyDefinitionId.Zeela, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MorphBallWhenZebesAwake, [
            new(EnemyDefinitionId.SidehopperLarge, row: 2),
            new(EnemyDefinitionId.FaceBlock, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MorphBallDefault, MorphBallEyeAndFaceBlock),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseZeela, [
            new(EnemyDefinitionId.Zeela, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedTower, [
            new(EnemyDefinitionId.Ripper, row: 1),
            new(EnemyDefinitionId.Beetom, row: 2),
            new(EnemyDefinitionId.Geega, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BlueBrinstarBoulders, [
            new(EnemyDefinitionId.Boulder, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BillyMays, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BigPinkSaveRoom, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BigPink, [
            new(EnemyDefinitionId.Rio, row: 1),
            new(EnemyDefinitionId.Sidehopper, row: 2),
            new(EnemyDefinitionId.Zeb, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EtecoonSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BelowSpazer, [
            new(EnemyDefinitionId.YappingMaw, row: 1),
            new(EnemyDefinitionId.Cacatac, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Spazer, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PinkBrinstarPowerBombs, [
            new(EnemyDefinitionId.ShutterShootable, row: 1),
            new(EnemyDefinitionId.Metaree, row: 1),
            new(EnemyDefinitionId.SidehopperLarge, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedTowerRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.KraidRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.VariaSuit, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedBrinstarSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BlueBrinstarEnergyTankDefault, MorphBallEyeAndFaceBlock),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarFirefleas, [
            new(EnemyDefinitionId.Fireflea, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarBeetoms, [
            new(EnemyDefinitionId.Beetom, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EtecoonEnergyTank, [
            new(EnemyDefinitionId.Zebbo, row: 1),
            new(EnemyDefinitionId.Beetom, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PinkBrinstarHoppers, [
            new(EnemyDefinitionId.SidehopperLarge, row: 1),
            new(EnemyDefinitionId.Sidehopper, row: 2),
            new(EnemyDefinitionId.ShutterShootable, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseEnergyTank, [
            new(EnemyDefinitionId.Beetom, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseEntrance, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseKihunter, [
            new(EnemyDefinitionId.KihunterGreen, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMainShaft, [
            new(EnemyDefinitionId.Etecoon, row: 1),
            new(EnemyDefinitionId.Zeela, row: 2),
            new(EnemyDefinitionId.JetRipper, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SporeSpawnSuper, [
            new(EnemyDefinitionId.Zeb, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EarlySupers, [
            new(EnemyDefinitionId.Sidehopper, row: 1),
            new(EnemyDefinitionId.ShutterGrowing, row: 2),
            new(EnemyDefinitionId.Waver, row: 3),
            new(EnemyDefinitionId.Zeb, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedBrinstarFirefleas, [
            new(EnemyDefinitionId.Fireflea, row: 1),
            new(EnemyDefinitionId.Waver, row: 2),
            new(EnemyDefinitionId.YappingMaw, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ConstructionZoneDefault, [
            new(EnemyDefinitionId.Sbug, row: 7),
            new(EnemyDefinitionId.FaceBlock, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenHillZone, [
            new(EnemyDefinitionId.Sidehopper, row: 1),
            new(EnemyDefinitionId.Geega, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Dachora, [
            new(EnemyDefinitionId.Zeela, row: 1),
            new(EnemyDefinitionId.Metaree, row: 2),
            new(EnemyDefinitionId.Rio, row: 3),
            new(EnemyDefinitionId.Dachora, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Hellway, [
            new(EnemyDefinitionId.Zeela, row: 1),
            new(EnemyDefinitionId.Zebbo, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FirstMissile, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Kraid, [
            new(EnemyDefinitionId.Kraid, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BrinstarReserveTank, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.XrayScope, [
            new(EnemyDefinitionId.ShutterShootable, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.StatuesHallway, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AlphaPowerBomb, [
            new(EnemyDefinitionId.Boyon, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.KraidEyeDoor, [
            new(EnemyDefinitionId.Zeb, row: 1),
            new(EnemyDefinitionId.Sbug, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Caterpillar, [
            new(EnemyDefinitionId.Zero, row: 2),
            new(EnemyDefinitionId.Cacatac, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MiniKraid, [
            new(EnemyDefinitionId.PirateGreenWalking, row: 1),
            new(EnemyDefinitionId.FakeKraid, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SporeSpawn, [
            new(EnemyDefinitionId.SporeSpawn, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SkreeBoost, [
            new(EnemyDefinitionId.Skree, row: 1),
            new(EnemyDefinitionId.Boyon, row: 7),
        ]),
    ];
}
