using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Crateria room-header region, in table order. It opens with the set Draygon and Mother Brain share.</summary>
internal static class CrateriaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The members of every set here that loads a face block and Stoke, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] FaceBlockAndStoke =
    [
        new(EnemyDefinitionId.FaceBlock, row: 7), new(UnplacedEnemyArtwork.Stoke, row: 7),
    ];

    /// <summary>The members of every set here that loads Sbugs and a face block, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] SbugAndFaceBlock =
    [
        new(EnemyDefinitionId.Sbug, row: 7), new(EnemyDefinitionId.FaceBlock, row: 7),
    ];

    /// <summary>The members of every set here that loads the gunship, its top and its entrance bottom, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] Gunship =
    [
        new(EnemyDefinitionId.ShipTop, row: 2), new(EnemyDefinitionId.ShipBottomEntrance, row: 7),
    ];

    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastOcean, [
            new(EnemyDefinitionId.Choot, row: 1),
            new(EnemyDefinitionId.Skultera, row: 2),
            new(EnemyDefinitionId.SuspensorPlatform, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaPowerBombs, [
            new(EnemyDefinitionId.Alcoon, row: 1),
            new(EnemyDefinitionId.Zero, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Terminator, [
            new(EnemyDefinitionId.Waver, row: 1),
            new(EnemyDefinitionId.Zoomer, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileBombwayWhenZebesAwake, FaceBlockAndStoke),
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileBombwayDefault, FaceBlockAndStoke),
        new(RoomEnemyGraphicsSetDefinitions.ParlorWhenZebesAwake, [
            new(EnemyDefinitionId.Zoomer, row: 1),
            new(EnemyDefinitionId.Skree, row: 2),
            new(EnemyDefinitionId.Ripper, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FlywayBeforeTimebomb, [
            new(EnemyDefinitionId.Waver, row: 1),
            new(EnemyDefinitionId.Mellow, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PitWithMorphBallAndMissiles, [
            new(EnemyDefinitionId.PirateGreyWalking, row: 1),
            new(EnemyDefinitionId.PirateGreyWall, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GauntletEntrance, [
            new(EnemyDefinitionId.YappingMaw, row: 1),
            new(EnemyDefinitionId.Waver, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BombTorizoBeforeTimebomb, [
            new(EnemyDefinitionId.BombTorizo, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft, [
            new(EnemyDefinitionId.PirateGreenWalking, row: 1),
            new(EnemyDefinitionId.Beetom, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarElevator, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileDefault, [
            new(EnemyDefinitionId.MorphBallEye, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaMap, [
            new(EnemyDefinitionId.BombTorizo, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileWhenZebesAwake, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ClimbWhenZebesTimebombSet, [
            new(EnemyDefinitionId.PirateSilverNinja, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElbow, [
            new(EnemyDefinitionId.FaceBlock, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Moat, [
            new(EnemyDefinitionId.FaceBlock, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ClimbDefault, SbugAndFaceBlock),
        new(RoomEnemyGraphicsSetDefinitions.WestOcean, [
            new(EnemyDefinitionId.Skultera, row: 1),
            new(EnemyDefinitionId.Zeb, row: 2),
            new(EnemyDefinitionId.GrappleRipper, row: 3),
            new(EnemyDefinitionId.Tripper, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ParlorDefault, [
            new(EnemyDefinitionId.Sbug, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LandingSiteBeforeTimebomb, Gunship),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaSuper, [
            new(EnemyDefinitionId.Boyon, row: 1),
            new(EnemyDefinitionId.YappingMaw, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PreBowling, [
            new(EnemyDefinitionId.OrangeZoomer, row: 1),
            new(EnemyDefinitionId.Sova, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ClimbWhenZebesAwake, [
            new(EnemyDefinitionId.PirateGreyWall, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BowlingAlleyPath, [
            new(EnemyDefinitionId.Waver, row: 1),
            new(EnemyDefinitionId.Choot, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedBrinstarElevator, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GauntletEnergyTank, [
            new(EnemyDefinitionId.YappingMaw, row: 1),
            new(EnemyDefinitionId.Zebbo, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PitDefault, SbugAndFaceBlock),
        new(RoomEnemyGraphicsSetDefinitions.CrabMaze, [
            new(EnemyDefinitionId.Sciser, row: 7),
            new(EnemyDefinitionId.Puyo, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaTube, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayKagos, [
            new(EnemyDefinitionId.Kago, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ElevatorToBlueBrinstar, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElevator, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PreMapFlyway, [
            new(EnemyDefinitionId.Mellow, row: 1),
            new(EnemyDefinitionId.Rio, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerMushrooms, [
            new(EnemyDefinitionId.Kago, row: 2),
            new(EnemyDefinitionId.ArmoredZoomer, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LandingSiteWhenZebesTimebombSet, Gunship),
        new(RoomEnemyGraphicsSetDefinitions.ParlorWhenZebesTimebombSet, [
            new(EnemyDefinitionId.Sbug, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BombTorizoWhenZebesTimebombSet, [
            new(EnemyDefinitionId.EtecoonEscape, row: 1),
            new(EnemyDefinitionId.DachoraEscape, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FlywayWhenZebesTimebombSet, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaKihunter, [
            new(EnemyDefinitionId.KihunterGreen, row: 1),
            new(EnemyDefinitionId.Sciser, row: 2),
            new(EnemyDefinitionId.Zero, row: 2),
        ]),
    ];
}
