namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$B4 graphics sets of the Brinstar room-header region, in native table order. It holds the Tourian statue rooms, whose headers lie in this region.</summary>
internal static class BrinstarRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.BrinstarPreMap), [
            new(EnemyDefinitionId.Sidehopper, ColourStore: new(1)),
            new(EnemyDefinitionId.Zeela, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMap), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMissileRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawnKihunters), [
            new(EnemyDefinitionId.KihunterGreen, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EtecoonSuper), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DachoraEnergyRefill), [
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawnFarming), [
            new(EnemyDefinitionId.Zeb, ColourStore: new(1)),
            new(EnemyDefinitionId.Zeela, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Statues), [
            new(EnemyDefinitionId.TourianStatue, ColourStore: new(1)),
            new(EnemyDefinitionId.TourianStatueGhost, ColourStore: new(3), StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HopperETank), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BetaPowerBomb), [
            new(EnemyDefinitionId.YappingMaw, ColourStore: new(1)),
            new(EnemyDefinitionId.SidehopperLarge, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WaterwayETank), [
            new(EnemyDefinitionId.Skultera, ColourStore: new(1)),
            new(EnemyDefinitionId.Puyo, ColourStore: new(2)),
            new(EnemyDefinitionId.Zero, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ConstructionZone1), [
            new(EnemyDefinitionId.Zoomer, ColourStore: new(1)),
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank1), [
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(1)),
            new(EnemyDefinitionId.Zoomer, ColourStore: new(2)),
            new(EnemyDefinitionId.Skree, ColourStore: new(3)),
            new(EnemyDefinitionId.Rio, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NoobBridge), [
            new(EnemyDefinitionId.Cacatac, ColourStore: new(1)),
            new(EnemyDefinitionId.Zeela, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MorphBall1), [
            new(EnemyDefinitionId.SidehopperLarge, ColourStore: new(2)),
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MorphBall0), [
            new(EnemyDefinitionId.Eye, ColourStore: new(7)),
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseZeela), [
            new(EnemyDefinitionId.Zeela, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedTower), [
            new(EnemyDefinitionId.Ripper, ColourStore: new(1)),
            new(EnemyDefinitionId.Beetom, ColourStore: new(2)),
            new(EnemyDefinitionId.Geega, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueBrinstarBoulders), [
            new(EnemyDefinitionId.Boulder, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BillyMays), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BigPinkSaveRoom), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BigPink), [
            new(EnemyDefinitionId.Rio, ColourStore: new(1)),
            new(EnemyDefinitionId.Sidehopper, ColourStore: new(2)),
            new(EnemyDefinitionId.Zeb, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EtecoonSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BelowSpazer), [
            new(EnemyDefinitionId.YappingMaw, ColourStore: new(1)),
            new(EnemyDefinitionId.Cacatac, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Spazer), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PinkBrinstarPowerBombs), [
            new(EnemyDefinitionId.ShutterShootable, ColourStore: new(1)),
            new(EnemyDefinitionId.Metaree, ColourStore: new(1)),
            new(EnemyDefinitionId.SidehopperLarge, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedTowerRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.KraidRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.VariaSuit), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedBrinstarSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank0), [
            new(EnemyDefinitionId.Eye, ColourStore: new(7)),
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarFirefleas), [
            new(EnemyDefinitionId.Fireflea, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarBeetoms), [
            new(EnemyDefinitionId.Beetom, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EtecoonETank), [
            new(EnemyDefinitionId.Zebbo, ColourStore: new(1)),
            new(EnemyDefinitionId.Beetom, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PinkBrinstarHoppers), [
            new(EnemyDefinitionId.SidehopperLarge, ColourStore: new(1)),
            new(EnemyDefinitionId.Sidehopper, ColourStore: new(2)),
            new(EnemyDefinitionId.ShutterShootable, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseETank), [
            new(EnemyDefinitionId.Reflec, ColourStore: new(1)),
            new(EnemyDefinitionId.Beetom, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseEntrance), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseKihunter), [
            new(EnemyDefinitionId.KihunterGreen, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMainShaft), [
            new(EnemyDefinitionId.Etecoon, ColourStore: new(1)),
            new(EnemyDefinitionId.Zeela, ColourStore: new(2)),
            new(EnemyDefinitionId.Ripper2, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawnSuper), [
            new(EnemyDefinitionId.Zeb, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EarlySupers), [
            new(EnemyDefinitionId.Sidehopper, ColourStore: new(1)),
            new(EnemyDefinitionId.ShutterGrowing, ColourStore: new(2)),
            new(EnemyDefinitionId.Waver, ColourStore: new(3)),
            new(EnemyDefinitionId.Zeb, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedBrinstarFirefleas), [
            new(EnemyDefinitionId.Fireflea, ColourStore: new(1)),
            new(EnemyDefinitionId.Waver, ColourStore: new(2)),
            new(EnemyDefinitionId.YappingMaw, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ConstructionZone0), [
            new(EnemyDefinitionId.Sbug, ColourStore: new(7)),
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenHillZone), [
            new(EnemyDefinitionId.Sidehopper, ColourStore: new(1)),
            new(EnemyDefinitionId.Geega, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Dachora), [
            new(EnemyDefinitionId.Zeela, ColourStore: new(1)),
            new(EnemyDefinitionId.Metaree, ColourStore: new(2)),
            new(EnemyDefinitionId.Rio, ColourStore: new(3)),
            new(EnemyDefinitionId.Dachora, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Hellway), [
            new(EnemyDefinitionId.Zeela, ColourStore: new(1)),
            new(EnemyDefinitionId.Zebbo, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FirstMissile), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Kraid), [
            new(EnemyDefinitionId.Kraid, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BrinstarReserveTank), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.XrayScope), [
            new(EnemyDefinitionId.ShutterShootable, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.StatuesHallway), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AlphaPowerBomb), [
            new(EnemyDefinitionId.Boyon, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.KraidEyeDoor), [
            new(EnemyDefinitionId.Zeb, ColourStore: new(1)),
            new(EnemyDefinitionId.Sbug, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Caterpillar), [
            new(EnemyDefinitionId.Zero, ColourStore: new(2)),
            new(EnemyDefinitionId.Cacatac, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MiniKraid), [
            new(EnemyDefinitionId.PirateGreenWalking, ColourStore: new(1)),
            new(EnemyDefinitionId.MiniKraid, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawn), [
            new(EnemyDefinitionId.SporeSpawn, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SkreeBoost), [
            new(EnemyDefinitionId.Skree, ColourStore: new(1)),
            new(EnemyDefinitionId.Boyon, ColourStore: new(7)),
        ]),
    ];
}
