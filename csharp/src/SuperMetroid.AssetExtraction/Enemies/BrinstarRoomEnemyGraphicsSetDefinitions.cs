using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Brinstar room-header region, in table order. It holds the Tourian statue rooms, whose headers lie in this region.</summary>
internal static class BrinstarRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.BrinstarPreMap, [
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.Zeela),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMap, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMissileRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SporeSpawnKihunters, [
            new(EnemyDefinitionId.KihunterGreen),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EtecoonSuper, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DachoraEnergyRefill, [
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SporeSpawnFarming, [
            new(EnemyDefinitionId.Zeb),
            new(EnemyDefinitionId.Zeela),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Statues, [
            new(EnemyDefinitionId.TourianStatue),
            new(UnplacedEnemyArtwork.TourianStatueGhosts),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.HopperEnergyTank, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BetaPowerBomb, [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.SidehopperLarge),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WaterwayEnergyTank, [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Zero),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ConstructionZoneWhenZebesAwake, [
            new(EnemyDefinitionId.Zoomer),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BlueBrinstarEnergyTankWhenZebesAwake, [
            new(EnemyDefinitionId.FaceBlock),
            new(EnemyDefinitionId.Zoomer),
            new(EnemyDefinitionId.Skree),
            new(EnemyDefinitionId.Rio),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.NoobBridge, [
            new(EnemyDefinitionId.Cacatac),
            new(EnemyDefinitionId.Zeela),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MorphBallWhenZebesAwake, [
            new(EnemyDefinitionId.SidehopperLarge),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MorphBallDefault, [
            new(EnemyDefinitionId.MorphBallEye),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseZeela, [
            new(EnemyDefinitionId.Zeela),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedTower, [
            new(EnemyDefinitionId.Ripper),
            new(EnemyDefinitionId.Beetom),
            new(EnemyDefinitionId.Geega),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BlueBrinstarBoulders, [
            new(EnemyDefinitionId.Boulder),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BillyMays, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BigPinkSaveRoom, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BigPink, [
            new(EnemyDefinitionId.Rio),
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.Zeb),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EtecoonSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BelowSpazer, [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.Cacatac),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Spazer, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PinkBrinstarPowerBombs, [
            new(EnemyDefinitionId.ShutterShootable),
            new(EnemyDefinitionId.Metaree),
            new(EnemyDefinitionId.SidehopperLarge),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedTowerRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.KraidRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.VariaSuit, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedBrinstarSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BlueBrinstarEnergyTankDefault, [
            new(EnemyDefinitionId.MorphBallEye),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarFirefleas, [
            new(EnemyDefinitionId.Fireflea),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarBeetoms, [
            new(EnemyDefinitionId.Beetom),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EtecoonEnergyTank, [
            new(EnemyDefinitionId.Zebbo),
            new(EnemyDefinitionId.Beetom),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PinkBrinstarHoppers, [
            new(EnemyDefinitionId.SidehopperLarge),
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.ShutterShootable),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseEnergyTank, [
            new(EnemyDefinitionId.Beetom),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseEntrance, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WarehouseKihunter, [
            new(EnemyDefinitionId.KihunterGreen),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMainShaft, [
            new(EnemyDefinitionId.Etecoon),
            new(EnemyDefinitionId.Zeela),
            new(EnemyDefinitionId.JetRipper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SporeSpawnSuper, [
            new(EnemyDefinitionId.Zeb),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EarlySupers, [
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.ShutterGrowing),
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Zeb),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedBrinstarFirefleas, [
            new(EnemyDefinitionId.Fireflea),
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.YappingMaw),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ConstructionZoneDefault, [
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenHillZone, [
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.Geega),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Dachora, [
            new(EnemyDefinitionId.Zeela),
            new(EnemyDefinitionId.Metaree),
            new(EnemyDefinitionId.Rio),
            new(EnemyDefinitionId.Dachora),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Hellway, [
            new(EnemyDefinitionId.Zeela),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FirstMissile, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Kraid, [
            new(EnemyDefinitionId.Kraid),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BrinstarReserveTank, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.XrayScope, [
            new(EnemyDefinitionId.ShutterShootable),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.StatuesHallway, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AlphaPowerBomb, [
            new(EnemyDefinitionId.Boyon),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.KraidEyeDoor, [
            new(EnemyDefinitionId.Zeb),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Caterpillar, [
            new(EnemyDefinitionId.Zero),
            new(EnemyDefinitionId.Cacatac),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MiniKraid, [
            new(EnemyDefinitionId.PirateGreenWalking),
            new(EnemyDefinitionId.FakeKraid),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SporeSpawn, [
            new(EnemyDefinitionId.SporeSpawn),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SkreeBoost, [
            new(EnemyDefinitionId.Skree),
            new(EnemyDefinitionId.Boyon),
        ]),
    ];
}
