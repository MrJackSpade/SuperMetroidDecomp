namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$B4 graphics sets of the Crateria room-header region, in native table order. It opens with the set Draygon and Mother Brain share.</summary>
internal static class CrateriaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastOcean), [
            new(EnemyDefinitionId.Choot, PaletteRow: 1),
            new(EnemyDefinitionId.Skultera, PaletteRow: 2),
            new(EnemyDefinitionId.Kamer2, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaPowerBombs), [
            new(EnemyDefinitionId.Alcoon, PaletteRow: 1),
            new(EnemyDefinitionId.Zero, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Terminator), [
            new(EnemyDefinitionId.Waver, PaletteRow: 1),
            new(EnemyDefinitionId.Zoomer, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileBombway1), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
            new(EnemyDefinitionId.Stoke, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileBombway0), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
            new(EnemyDefinitionId.Stoke, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Parlor1), [
            new(EnemyDefinitionId.Zoomer, PaletteRow: 1),
            new(EnemyDefinitionId.Skree, PaletteRow: 2),
            new(EnemyDefinitionId.Ripper, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Flyway01), [
            new(EnemyDefinitionId.Waver, PaletteRow: 1),
            new(EnemyDefinitionId.Mellow, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pit1), [
            new(EnemyDefinitionId.PirateGreyWalking, PaletteRow: 1),
            new(EnemyDefinitionId.PirateGreyWall, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GauntletEntrance), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.Waver, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BombTorizo01), [
            new(EnemyDefinitionId.BombTorizo, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft), [
            new(EnemyDefinitionId.PirateGreenWalking, PaletteRow: 1),
            new(EnemyDefinitionId.Beetom, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissile0), [
            new(EnemyDefinitionId.Eye, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaMap), [
            new(EnemyDefinitionId.BombTorizo, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissile1), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Climb2), [
            new(EnemyDefinitionId.PirateSilverNinja, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElbow), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Moat), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Climb0), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestOcean), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.Zeb, PaletteRow: 2),
            new(EnemyDefinitionId.GRipper, PaletteRow: 3),
            new(EnemyDefinitionId.Tripper, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Parlor0), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LandingSite012), [
            new(EnemyDefinitionId.ShipTop, PaletteRow: 2),
            new(EnemyDefinitionId.ShipBottomEntrance, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaSuper), [
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PreBowling), [
            new(EnemyDefinitionId.HZoomer, PaletteRow: 1),
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Climb1), [
            new(EnemyDefinitionId.PirateGreyWall, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BowlingAlleyPath), [
            new(EnemyDefinitionId.Waver, PaletteRow: 1),
            new(EnemyDefinitionId.Choot, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedBinstarElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GauntletETank), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pit0), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabMaze), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 7),
            new(EnemyDefinitionId.Puyo, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaTube), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayKagos), [
            new(EnemyDefinitionId.Kago, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PreMapFlyway), [
            new(EnemyDefinitionId.Mellow, PaletteRow: 1),
            new(EnemyDefinitionId.Rio, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerMushrooms), [
            new(EnemyDefinitionId.Kago, PaletteRow: 2),
            new(EnemyDefinitionId.MZoomer, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LandingSite3), [
            new(EnemyDefinitionId.ShipTop, PaletteRow: 2),
            new(EnemyDefinitionId.ShipBottomEntrance, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Parlor2), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BombTorizo2), [
            new(EnemyDefinitionId.EtecoonEscape, PaletteRow: 1),
            new(EnemyDefinitionId.DachoraEscape, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Flyway2), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaKihunter), [
            new(EnemyDefinitionId.KihunterGreen, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 2),
            new(EnemyDefinitionId.Zero, PaletteRow: 2),
        ]),
    ];
}
