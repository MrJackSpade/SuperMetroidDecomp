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
            new(EnemyDefinitionId.Choot, ColourStore: new(1)),
            new(EnemyDefinitionId.Skultera, ColourStore: new(2)),
            new(EnemyDefinitionId.Kamer2, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaPowerBombs), [
            new(EnemyDefinitionId.Alcoon, ColourStore: new(1)),
            new(EnemyDefinitionId.Zero, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Terminator), [
            new(EnemyDefinitionId.Waver, ColourStore: new(1)),
            new(EnemyDefinitionId.Zoomer, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileBombway1), [
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(7)),
            new(EnemyDefinitionId.Stoke, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileBombway0), [
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(7)),
            new(EnemyDefinitionId.Stoke, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Parlor1), [
            new(EnemyDefinitionId.Zoomer, ColourStore: new(1)),
            new(EnemyDefinitionId.Skree, ColourStore: new(2)),
            new(EnemyDefinitionId.Ripper, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Flyway01), [
            new(EnemyDefinitionId.Waver, ColourStore: new(1)),
            new(EnemyDefinitionId.Mellow, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pit1), [
            new(EnemyDefinitionId.PirateGreyWalking, ColourStore: new(1)),
            new(EnemyDefinitionId.PirateGreyWall, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GauntletEntrance), [
            new(EnemyDefinitionId.YappingMaw, ColourStore: new(1)),
            new(EnemyDefinitionId.Waver, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BombTorizo01), [
            new(EnemyDefinitionId.BombTorizo, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft), [
            new(EnemyDefinitionId.PirateGreenWalking, ColourStore: new(1)),
            new(EnemyDefinitionId.Beetom, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissile0), [
            new(EnemyDefinitionId.Eye, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaMap), [
            new(EnemyDefinitionId.BombTorizo, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissile1), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Climb2), [
            new(EnemyDefinitionId.PirateSilverNinja, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElbow), [
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Moat), [
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Climb0), [
            new(EnemyDefinitionId.Sbug, ColourStore: new(7)),
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestOcean), [
            new(EnemyDefinitionId.Skultera, ColourStore: new(1)),
            new(EnemyDefinitionId.Zeb, ColourStore: new(2)),
            new(EnemyDefinitionId.GRipper, ColourStore: new(3)),
            new(EnemyDefinitionId.Tripper, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Parlor0), [
            new(EnemyDefinitionId.Sbug, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LandingSite012), [
            new(EnemyDefinitionId.ShipTop, ColourStore: new(2)),
            new(EnemyDefinitionId.ShipBottomEntrance, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaSuper), [
            new(EnemyDefinitionId.Boyon, ColourStore: new(1)),
            new(EnemyDefinitionId.YappingMaw, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PreBowling), [
            new(EnemyDefinitionId.HZoomer, ColourStore: new(1)),
            new(EnemyDefinitionId.Sova, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Climb1), [
            new(EnemyDefinitionId.PirateGreyWall, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BowlingAlleyPath), [
            new(EnemyDefinitionId.Waver, ColourStore: new(1)),
            new(EnemyDefinitionId.Choot, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedBinstarElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GauntletETank), [
            new(EnemyDefinitionId.YappingMaw, ColourStore: new(1)),
            new(EnemyDefinitionId.Zebbo, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pit0), [
            new(EnemyDefinitionId.Sbug, ColourStore: new(7)),
            new(EnemyDefinitionId.FaceBlock, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabMaze), [
            new(EnemyDefinitionId.Sciser, ColourStore: new(7)),
            new(EnemyDefinitionId.Puyo, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaTube), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayKagos), [
            new(EnemyDefinitionId.Kago, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PreMapFlyway), [
            new(EnemyDefinitionId.Mellow, ColourStore: new(1)),
            new(EnemyDefinitionId.Rio, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerMushrooms), [
            new(EnemyDefinitionId.Kago, ColourStore: new(2)),
            new(EnemyDefinitionId.MZoomer, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LandingSite3), [
            new(EnemyDefinitionId.ShipTop, ColourStore: new(2)),
            new(EnemyDefinitionId.ShipBottomEntrance, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Parlor2), [
            new(EnemyDefinitionId.Sbug, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BombTorizo2), [
            new(EnemyDefinitionId.EtecoonEscape, ColourStore: new(1)),
            new(EnemyDefinitionId.DachoraEscape, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Flyway2), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaKihunter), [
            new(EnemyDefinitionId.KihunterGreen, ColourStore: new(1)),
            new(EnemyDefinitionId.Sciser, ColourStore: new(2)),
            new(EnemyDefinitionId.Zero, ColourStore: new(2)),
        ]),
    ];
}
