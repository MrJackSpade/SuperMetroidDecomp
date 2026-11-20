using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Crateria room-header region, in table order. It opens with the set Draygon and Mother Brain share.</summary>
internal static class CrateriaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The members of every set here that loads a face block and Stoke, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] FaceBlockAndStoke =
    [
        new(EnemyDefinitionId.FaceBlock), new(UnplacedEnemyArtwork.Stoke),
    ];

    /// <summary>The members of every set here that loads Sbugs and a face block, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] SbugAndFaceBlock =
    [
        new(EnemyDefinitionId.Sbug), new(EnemyDefinitionId.FaceBlock),
    ];

    /// <summary>The members of every set here that loads the gunship, its top and its entrance bottom, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] Gunship =
    [
        new(EnemyDefinitionId.ShipTop), new(EnemyDefinitionId.ShipBottomEntrance),
    ];

    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastOcean, [
            new(EnemyDefinitionId.Choot),
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.SuspensorPlatform),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaPowerBombs, [
            new(EnemyDefinitionId.Alcoon),
            new(EnemyDefinitionId.Zero),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Terminator, [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Zoomer),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileBombwayWhenZebesAwake, FaceBlockAndStoke),
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileBombwayDefault, FaceBlockAndStoke),
        new(RoomEnemyGraphicsSetDefinitions.ParlorWhenZebesAwake, [
            new(EnemyDefinitionId.Zoomer),
            new(EnemyDefinitionId.Skree),
            new(EnemyDefinitionId.Ripper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FlywayBeforeTimebomb, [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Mellow),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PitWithMorphBallAndMissiles, [
            new(EnemyDefinitionId.PirateGreyWalking),
            new(EnemyDefinitionId.PirateGreyWall),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GauntletEntrance, [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.Waver),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BombTorizoBeforeTimebomb, [
            new(EnemyDefinitionId.BombTorizo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft, [
            new(EnemyDefinitionId.PirateGreenWalking),
            new(EnemyDefinitionId.Beetom),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBrinstarElevator, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileDefault, [
            new(EnemyDefinitionId.MorphBallEye),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaMap, [
            new(EnemyDefinitionId.BombTorizo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileWhenZebesAwake, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ClimbWhenZebesTimebombSet, [
            new(EnemyDefinitionId.PirateSilverNinja),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElbow, [
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Moat, [
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ClimbDefault, SbugAndFaceBlock),
        new(RoomEnemyGraphicsSetDefinitions.WestOcean, [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Zeb),
            new(EnemyDefinitionId.GrappleRipper),
            new(EnemyDefinitionId.Tripper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ParlorDefault, [
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LandingSiteBeforeTimebomb, Gunship),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaSuper, [
            new(EnemyDefinitionId.Boyon),
            new(EnemyDefinitionId.YappingMaw),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PreBowling, [
            new(EnemyDefinitionId.OrangeZoomer),
            new(EnemyDefinitionId.Sova),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ClimbWhenZebesAwake, [
            new(EnemyDefinitionId.PirateGreyWall),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BowlingAlleyPath, [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Choot),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedBrinstarElevator, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GauntletEnergyTank, [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PitDefault, SbugAndFaceBlock),
        new(RoomEnemyGraphicsSetDefinitions.CrabMaze, [
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Puyo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaTube, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayKagos, [
            new(EnemyDefinitionId.Kago),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ElevatorToBlueBrinstar, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElevator, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PreMapFlyway, [
            new(EnemyDefinitionId.Mellow),
            new(EnemyDefinitionId.Rio),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerMushrooms, [
            new(EnemyDefinitionId.Kago),
            new(EnemyDefinitionId.ArmoredZoomer),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LandingSiteWhenZebesTimebombSet, Gunship),
        new(RoomEnemyGraphicsSetDefinitions.ParlorWhenZebesTimebombSet, [
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BombTorizoWhenZebesTimebombSet, [
            new(EnemyDefinitionId.EtecoonEscape),
            new(EnemyDefinitionId.DachoraEscape),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FlywayWhenZebesTimebombSet, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrateriaKihunter, [
            new(EnemyDefinitionId.KihunterGreen),
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Zero),
        ]),
    ];
}
