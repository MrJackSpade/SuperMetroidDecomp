namespace SuperMetroid.Core.Enemies;

/// <summary>The graphics sets of the Crateria room-header region, in table order. It opens with the set Draygon and Mother Brain share.</summary>
internal static class CrateriaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastOcean), [
            new(EnemyDefinitionId.Choot),
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Kamer2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaPowerBombs), [
            new(EnemyDefinitionId.Alcoon),
            new(EnemyDefinitionId.Zero),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Terminator), [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Zoomer),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileBombway1), [
            new(EnemyDefinitionId.FaceBlock),
            new(EnemyDefinitionId.Stoke),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileBombway0), [
            new(EnemyDefinitionId.FaceBlock),
            new(EnemyDefinitionId.Stoke),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Parlor1), [
            new(EnemyDefinitionId.Zoomer),
            new(EnemyDefinitionId.Skree),
            new(EnemyDefinitionId.Ripper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Flyway01), [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Mellow),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pit1), [
            new(EnemyDefinitionId.PirateGreyWalking),
            new(EnemyDefinitionId.PirateGreyWall),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GauntletEntrance), [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.Waver),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BombTorizo01), [
            new(EnemyDefinitionId.BombTorizo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft), [
            new(EnemyDefinitionId.PirateGreenWalking),
            new(EnemyDefinitionId.Beetom),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBrinstarElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissile0), [
            new(EnemyDefinitionId.Eye),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaMap), [
            new(EnemyDefinitionId.BombTorizo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissile1), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Climb2), [
            new(EnemyDefinitionId.PirateSilverNinja),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElbow), [
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Moat), [
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Climb0), [
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestOcean), [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Zeb),
            new(EnemyDefinitionId.GRipper),
            new(EnemyDefinitionId.Tripper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Parlor0), [
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LandingSite012), [
            new(EnemyDefinitionId.ShipTop),
            new(EnemyDefinitionId.ShipBottomEntrance),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaSuper), [
            new(EnemyDefinitionId.Boyon),
            new(EnemyDefinitionId.YappingMaw),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PreBowling), [
            new(EnemyDefinitionId.HZoomer),
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Climb1), [
            new(EnemyDefinitionId.PirateGreyWall),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BowlingAlleyPath), [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Choot),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedBinstarElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GauntletETank), [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pit0), [
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabMaze), [
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Puyo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaTube), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayKagos), [
            new(EnemyDefinitionId.Kago),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PreMapFlyway), [
            new(EnemyDefinitionId.Mellow),
            new(EnemyDefinitionId.Rio),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerMushrooms), [
            new(EnemyDefinitionId.Kago),
            new(EnemyDefinitionId.MZoomer),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LandingSite3), [
            new(EnemyDefinitionId.ShipTop),
            new(EnemyDefinitionId.ShipBottomEntrance),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Parlor2), [
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BombTorizo2), [
            new(EnemyDefinitionId.EtecoonEscape),
            new(EnemyDefinitionId.DachoraEscape),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Flyway2), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaKihunter), [
            new(EnemyDefinitionId.KihunterGreen),
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Zero),
        ]),
    ];
}
