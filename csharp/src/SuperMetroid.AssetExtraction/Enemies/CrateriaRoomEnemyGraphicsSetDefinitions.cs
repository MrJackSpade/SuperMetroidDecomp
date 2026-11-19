using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Crateria room-header region, in table order. It opens with the set Draygon and Mother Brain share.</summary>
internal static class CrateriaRoomEnemyGraphicsSetDefinitions
{
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
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileBombwayWhenZebesAwake, [
            new(EnemyDefinitionId.FaceBlock),
            new(UnplacedEnemyArtwork.Stoke),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FinalMissileBombwayDefault, [
            new(EnemyDefinitionId.FaceBlock),
            new(UnplacedEnemyArtwork.Stoke),
        ]),
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
        new(RoomEnemyGraphicsSetDefinitions.ClimbDefault, [
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestOcean, [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Zeb),
            new(EnemyDefinitionId.GrappleRipper),
            new(EnemyDefinitionId.Tripper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ParlorDefault, [
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LandingSiteBeforeTimebomb, [
            new(EnemyDefinitionId.ShipTop),
            new(EnemyDefinitionId.ShipBottomEntrance),
        ]),
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
        new(RoomEnemyGraphicsSetDefinitions.PitDefault, [
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.FaceBlock),
        ]),
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
        new(RoomEnemyGraphicsSetDefinitions.LandingSiteWhenZebesTimebombSet, [
            new(EnemyDefinitionId.ShipTop),
            new(EnemyDefinitionId.ShipBottomEntrance),
        ]),
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
