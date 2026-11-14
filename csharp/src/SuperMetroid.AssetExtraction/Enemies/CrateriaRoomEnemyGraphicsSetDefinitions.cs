using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Crateria room-header region, in table order. It opens with the set Draygon and Mother Brain share.</summary>
internal static class CrateriaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileBombwayWhenZebesAwake), [
            new(EnemyDefinitionId.FaceBlock),
            new(UnplacedEnemyArtwork.Stoke),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileBombwayDefault), [
            new(EnemyDefinitionId.FaceBlock),
            new(UnplacedEnemyArtwork.Stoke),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ParlorWhenZebesAwake), [
            new(EnemyDefinitionId.Zoomer),
            new(EnemyDefinitionId.Skree),
            new(EnemyDefinitionId.Ripper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Flyway01), [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Mellow),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PitWithMorphBallAndMissiles), [
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileDefault), [
            new(EnemyDefinitionId.Eye),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaMap), [
            new(EnemyDefinitionId.BombTorizo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FinalMissileWhenZebesAwake), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ClimbWhenZebesTimebombSet), [
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.ClimbDefault), [
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.FaceBlock),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestOcean), [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Zeb),
            new(EnemyDefinitionId.GRipper),
            new(EnemyDefinitionId.Tripper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ParlorDefault), [
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.ClimbWhenZebesAwake), [
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.PitDefault), [
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.LandingSiteWhenZebesTimebombSet), [
            new(EnemyDefinitionId.ShipTop),
            new(EnemyDefinitionId.ShipBottomEntrance),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ParlorWhenZebesTimebombSet), [
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BombTorizoWhenZebesTimebombSet), [
            new(EnemyDefinitionId.EtecoonEscape),
            new(EnemyDefinitionId.DachoraEscape),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FlywayWhenZebesTimebombSet), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrateriaKihunter), [
            new(EnemyDefinitionId.KihunterGreen),
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Zero),
        ]),
    ];
}
