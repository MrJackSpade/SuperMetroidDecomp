using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Brinstar room-header region, in table order. It holds the Tourian statue rooms, whose headers lie in this region.</summary>
internal static class BrinstarRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.BrinstarPreMap), [
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.Zeela),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMap), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMissileRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawnKihunters), [
            new(EnemyDefinitionId.KihunterGreen),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EtecoonSuper), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DachoraEnergyRefill), [
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawnFarming), [
            new(EnemyDefinitionId.Zeb),
            new(EnemyDefinitionId.Zeela),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Statues), [
            new(EnemyDefinitionId.TourianStatue),
            new(UnplacedEnemyArtwork.TourianStatueGhosts),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HopperETank), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BetaPowerBomb), [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.SidehopperLarge),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WaterwayETank), [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Zero),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ConstructionZoneWhenZebesAwake), [
            new(EnemyDefinitionId.Zoomer),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueBrinstarETankWhenZebesAwake), [
            new(EnemyDefinitionId.FaceBlock),
            new(EnemyDefinitionId.Zoomer),
            new(EnemyDefinitionId.Skree),
            new(EnemyDefinitionId.Rio),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NoobBridge), [
            new(EnemyDefinitionId.Cacatac),
            new(EnemyDefinitionId.Zeela),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MorphBallWhenZebesAwake), [
            new(EnemyDefinitionId.SidehopperLarge),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MorphBallDefault), [
            new(EnemyDefinitionId.Eye),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseZeela), [
            new(EnemyDefinitionId.Zeela),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedTower), [
            new(EnemyDefinitionId.Ripper),
            new(EnemyDefinitionId.Beetom),
            new(EnemyDefinitionId.Geega),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueBrinstarBoulders), [
            new(EnemyDefinitionId.Boulder),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BillyMays), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BigPinkSaveRoom), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BigPink), [
            new(EnemyDefinitionId.Rio),
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.Zeb),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EtecoonSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BelowSpazer), [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.Cacatac),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Spazer), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PinkBrinstarPowerBombs), [
            new(EnemyDefinitionId.ShutterShootable),
            new(EnemyDefinitionId.Metaree),
            new(EnemyDefinitionId.SidehopperLarge),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedTowerRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.KraidRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.VariaSuit), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedBrinstarSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueBrinstarETankDefault), [
            new(EnemyDefinitionId.Eye),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarFirefleas), [
            new(EnemyDefinitionId.Fireflea),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarBeetoms), [
            new(EnemyDefinitionId.Beetom),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EtecoonETank), [
            new(EnemyDefinitionId.Zebbo),
            new(EnemyDefinitionId.Beetom),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PinkBrinstarHoppers), [
            new(EnemyDefinitionId.SidehopperLarge),
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.ShutterShootable),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseETank), [
            new(EnemyDefinitionId.Beetom),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseEntrance), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseKihunter), [
            new(EnemyDefinitionId.KihunterGreen),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMainShaft), [
            new(EnemyDefinitionId.Etecoon),
            new(EnemyDefinitionId.Zeela),
            new(EnemyDefinitionId.Ripper2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawnSuper), [
            new(EnemyDefinitionId.Zeb),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EarlySupers), [
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.ShutterGrowing),
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Zeb),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedBrinstarFirefleas), [
            new(EnemyDefinitionId.Fireflea),
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.YappingMaw),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ConstructionZoneDefault), [
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenHillZone), [
            new(EnemyDefinitionId.Sidehopper),
            new(EnemyDefinitionId.Geega),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Dachora), [
            new(EnemyDefinitionId.Zeela),
            new(EnemyDefinitionId.Metaree),
            new(EnemyDefinitionId.Rio),
            new(EnemyDefinitionId.Dachora),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Hellway), [
            new(EnemyDefinitionId.Zeela),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FirstMissile), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Kraid), [
            new(EnemyDefinitionId.Kraid),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BrinstarReserveTank), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.XrayScope), [
            new(EnemyDefinitionId.ShutterShootable),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.StatuesHallway), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AlphaPowerBomb), [
            new(EnemyDefinitionId.Boyon),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.KraidEyeDoor), [
            new(EnemyDefinitionId.Zeb),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Caterpillar), [
            new(EnemyDefinitionId.Zero),
            new(EnemyDefinitionId.Cacatac),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MiniKraid), [
            new(EnemyDefinitionId.PirateGreenWalking),
            new(EnemyDefinitionId.MiniKraid),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawn), [
            new(EnemyDefinitionId.SporeSpawn),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SkreeBoost), [
            new(EnemyDefinitionId.Skree),
            new(EnemyDefinitionId.Boyon),
        ]),
    ];
}
