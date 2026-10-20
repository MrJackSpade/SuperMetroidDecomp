namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$B4 graphics sets of the Brinstar room-header region, in native table order. It holds the Tourian statue rooms, whose headers lie in this region.</summary>
internal static class BrinstarRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.BrinstarPreMap), [
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 1),
            new(EnemyDefinitionId.Zeela, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMap), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMissileRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawnKihunters), [
            new(EnemyDefinitionId.KihunterGreen, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EtecoonSuper), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DachoraEnergyRefill), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawnFarming), [
            new(EnemyDefinitionId.Zeb, PaletteRow: 1),
            new(EnemyDefinitionId.Zeela, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Statues), [
            new(EnemyDefinitionId.TourianStatue, PaletteRow: 1),
            new(EnemyDefinitionId.TourianStatueGhost, PaletteRow: 3, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HopperETank), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BetaPowerBomb), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.SidehopperLarge, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WaterwayETank), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Zero, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ConstructionZone1), [
            new(EnemyDefinitionId.Zoomer, PaletteRow: 1),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank1), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 1),
            new(EnemyDefinitionId.Zoomer, PaletteRow: 2),
            new(EnemyDefinitionId.Skree, PaletteRow: 3),
            new(EnemyDefinitionId.Rio, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NoobBridge), [
            new(EnemyDefinitionId.Cacatac, PaletteRow: 1),
            new(EnemyDefinitionId.Zeela, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MorphBall1), [
            new(EnemyDefinitionId.SidehopperLarge, PaletteRow: 2),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MorphBall0), [
            new(EnemyDefinitionId.Eye, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseZeela), [
            new(EnemyDefinitionId.Zeela, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedTower), [
            new(EnemyDefinitionId.Ripper, PaletteRow: 1),
            new(EnemyDefinitionId.Beetom, PaletteRow: 2),
            new(EnemyDefinitionId.Geega, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueBrinstarBoulders), [
            new(EnemyDefinitionId.Boulder, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BillyMays), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BigPinkSaveRoom), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BigPink), [
            new(EnemyDefinitionId.Rio, PaletteRow: 1),
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 2),
            new(EnemyDefinitionId.Zeb, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EtecoonSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BelowSpazer), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.Cacatac, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Spazer), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PinkBrinstarPowerBombs), [
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 1),
            new(EnemyDefinitionId.Metaree, PaletteRow: 1),
            new(EnemyDefinitionId.SidehopperLarge, PaletteRow: 2),
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
            new(EnemyDefinitionId.Eye, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarFirefleas), [
            new(EnemyDefinitionId.Fireflea, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarBeetoms), [
            new(EnemyDefinitionId.Beetom, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EtecoonETank), [
            new(EnemyDefinitionId.Zebbo, PaletteRow: 1),
            new(EnemyDefinitionId.Beetom, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PinkBrinstarHoppers), [
            new(EnemyDefinitionId.SidehopperLarge, PaletteRow: 1),
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 2),
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseETank), [
            new(EnemyDefinitionId.Reflec, PaletteRow: 1),
            new(EnemyDefinitionId.Beetom, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseEntrance), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WarehouseKihunter), [
            new(EnemyDefinitionId.KihunterGreen, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarMainShaft), [
            new(EnemyDefinitionId.Etecoon, PaletteRow: 1),
            new(EnemyDefinitionId.Zeela, PaletteRow: 2),
            new(EnemyDefinitionId.Ripper2, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawnSuper), [
            new(EnemyDefinitionId.Zeb, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EarlySupers), [
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 1),
            new(EnemyDefinitionId.ShutterGrowing, PaletteRow: 2),
            new(EnemyDefinitionId.Waver, PaletteRow: 3),
            new(EnemyDefinitionId.Zeb, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedBrinstarFirefleas), [
            new(EnemyDefinitionId.Fireflea, PaletteRow: 1),
            new(EnemyDefinitionId.Waver, PaletteRow: 2),
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ConstructionZone0), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenHillZone), [
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 1),
            new(EnemyDefinitionId.Geega, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Dachora), [
            new(EnemyDefinitionId.Zeela, PaletteRow: 1),
            new(EnemyDefinitionId.Metaree, PaletteRow: 2),
            new(EnemyDefinitionId.Rio, PaletteRow: 3),
            new(EnemyDefinitionId.Dachora, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Hellway), [
            new(EnemyDefinitionId.Zeela, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FirstMissile), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Kraid), [
            new(EnemyDefinitionId.Kraid, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BrinstarReserveTank), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.XrayScope), [
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.StatuesHallway), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AlphaPowerBomb), [
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.KraidEyeDoor), [
            new(EnemyDefinitionId.Zeb, PaletteRow: 1),
            new(EnemyDefinitionId.Sbug, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Caterpillar), [
            new(EnemyDefinitionId.Zero, PaletteRow: 2),
            new(EnemyDefinitionId.Cacatac, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MiniKraid), [
            new(EnemyDefinitionId.PirateGreenWalking, PaletteRow: 1),
            new(EnemyDefinitionId.MiniKraid, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SporeSpawn), [
            new(EnemyDefinitionId.SporeSpawn, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SkreeBoost), [
            new(EnemyDefinitionId.Skree, PaletteRow: 1),
            new(EnemyDefinitionId.Boyon, PaletteRow: 7),
        ]),
    ];
}
