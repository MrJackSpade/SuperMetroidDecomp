using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Norfair room-header region, in table order.</summary>
internal static class NorfairRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.GrappleTutorial3, [
            new(EnemyDefinitionId.Gamet, row: 1),
            new(EnemyDefinitionId.Puyo, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomireShaft, [
            new(EnemyDefinitionId.Viola, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MetalPirates, [
            new(EnemyDefinitionId.PirateGoldNinja, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AcidStatue, [
            new(EnemyDefinitionId.Chozo, row: 1),
            new(EnemyDefinitionId.Magdollite, row: 3),
            new(EnemyDefinitionId.Holtz, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PurpleFarming, [
            new(EnemyDefinitionId.Gamet, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.IceBeamGate, [
            new(EnemyDefinitionId.ShutterGrowing, row: 1),
            new(EnemyDefinitionId.Dessgeega, row: 2),
            new(EnemyDefinitionId.Mella, row: 3),
            new(EnemyDefinitionId.Sova, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel, [
            new(EnemyDefinitionId.YappingMaw, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedKihunterShaft, [
            new(EnemyDefinitionId.KihunterRed, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FastRipper, [
            new(EnemyDefinitionId.JetRipper, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WaveBeam, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FastPillarsSetup, [
            new(EnemyDefinitionId.PirateGoldWall, row: 1),
            new(EnemyDefinitionId.PirateGoldWalking, row: 2),
            new(EnemyDefinitionId.Viola, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairElevator, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.UpperNorfairRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.NorfairMap, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MickeyMouse, [
            new(EnemyDefinitionId.DessgeegaLarge, row: 1),
            new(EnemyDefinitionId.Multiviola, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GoldenTorizoEnergyRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Ridley, [
            new(EnemyDefinitionId.Ridley, row: 1),
            new(EnemyDefinitionId.RidleyBreakupPiece, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomirePowerBombs, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.IceBeam, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.HiJumpBoots, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomireSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FrogSpeedway, [
            new(EnemyDefinitionId.Beetom, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RisingTide, [
            new(EnemyDefinitionId.Squeept, row: 1),
            new(EnemyDefinitionId.Sova, row: 2),
            new(EnemyDefinitionId.Dragon, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomireFarming, [
            new(EnemyDefinitionId.Gamet, row: 1),
            new(EnemyDefinitionId.GrappleRipper, row: 2),
            new(EnemyDefinitionId.SuspensorPlatform, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame, [
            new(EnemyDefinitionId.Namihe, row: 1),
            new(EnemyDefinitionId.PirateGoldWall, row: 2),
            new(EnemyDefinitionId.PirateGoldWalking, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DoubleChamber, [
            new(EnemyDefinitionId.Fune, row: 1),
            new(EnemyDefinitionId.Kamer, row: 2),
            new(EnemyDefinitionId.Kago, row: 3),
            new(EnemyDefinitionId.GrappleRipper, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GrappleBeam, [
            new(EnemyDefinitionId.Multiviola, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedPirateShaft, [
            new(EnemyDefinitionId.PirateRedWalking, row: 1),
            new(EnemyDefinitionId.ArmoredZoomer, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway, [
            new(EnemyDefinitionId.PirateRedWalking, row: 1),
            new(EnemyDefinitionId.Multiviola, row: 2),
            new(EnemyDefinitionId.Cacatac, row: 3),
            new(EnemyDefinitionId.Sova, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Plowerhouse, [
            new(EnemyDefinitionId.Zebbo, row: 1),
            new(EnemyDefinitionId.Holtz, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairFireflea, [
            new(EnemyDefinitionId.Fireflea, row: 1),
            new(EnemyDefinitionId.Boulder, row: 2),
            new(EnemyDefinitionId.Fune, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel, [
            new(EnemyDefinitionId.Multiviola, row: 1),
            new(EnemyDefinitionId.Magdollite, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LavaDive, [
            new(EnemyDefinitionId.Namihe, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Pillar, [
            new(EnemyDefinitionId.NuclearWaffle, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrocomireEscape, [
            new(EnemyDefinitionId.Dragon, row: 1),
            new(EnemyDefinitionId.Geruta, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairSpringBallMaze, [
            new(EnemyDefinitionId.Alcoon, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairEscapePowerBombs, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PurpleShaft, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RidleyEnergyTank, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ScrewAttack, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GreenBubblesMissiles, [
            new(EnemyDefinitionId.ShutterShootable, row: 7),
            new(EnemyDefinitionId.Sova, row: 2),
            new(EnemyDefinitionId.Geruta, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Amphitheatre, [
            new(EnemyDefinitionId.PirateGoldWalking, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CathedralEntrance, [
            new(EnemyDefinitionId.Sova, row: 1),
            new(EnemyDefinitionId.Dessgeega, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MainHall, [
            new(EnemyDefinitionId.Hibashi, row: 1),
            new(EnemyDefinitionId.Dragon, row: 7),
            new(EnemyDefinitionId.Gamet, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomireJump, [
            new(EnemyDefinitionId.Mella, row: 1),
            new(EnemyDefinitionId.GrappleRipper, row: 2),
            new(EnemyDefinitionId.SuspensorPlatform, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FrogSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrocomireSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrumbleShaft, [
            new(EnemyDefinitionId.Sova, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Wasteland, [
            new(EnemyDefinitionId.Hibashi, row: 1),
            new(EnemyDefinitionId.DessgeegaLarge, row: 2),
            new(EnemyDefinitionId.ShutterShootable, row: 3),
            new(EnemyDefinitionId.KzanTop, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel, [
            new(EnemyDefinitionId.Dragon, row: 1),
            new(EnemyDefinitionId.Gamet, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GrappleTutorial1, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BubbleMountainSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.HiJumpEnergyTank, [
            new(EnemyDefinitionId.Sova, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BubbleMountain, [
            new(EnemyDefinitionId.Waver, row: 1),
            new(EnemyDefinitionId.JetRipper, row: 1),
            new(EnemyDefinitionId.Cacatac, row: 2),
            new(EnemyDefinitionId.Sova, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.IceBeamAcid, [
            new(EnemyDefinitionId.Tripper, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.IceBeamTutorial, [
            new(EnemyDefinitionId.Boyon, row: 1),
            new(EnemyDefinitionId.JetRipper, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomireMissile, [
            new(EnemyDefinitionId.Metaree, row: 1),
            new(EnemyDefinitionId.Gamet, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Cathedral, [
            new(EnemyDefinitionId.Sova, row: 1),
            new(EnemyDefinitionId.Geruta, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.NorfairReserveTank, [
            new(EnemyDefinitionId.Dragon, row: 1),
            new(EnemyDefinitionId.Sova, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GrappleTutorial2, [
            new(EnemyDefinitionId.Fune, row: 1),
            new(EnemyDefinitionId.Multiviola, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.IceBeamSnake, [
            new(EnemyDefinitionId.Sova, row: 1),
            new(EnemyDefinitionId.Fune, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GoldenTorizo, [
            new(EnemyDefinitionId.GoldenTorizo, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel, [
            new(EnemyDefinitionId.Tripper, row: 7),
            new(EnemyDefinitionId.ShutterShootable, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpeedBooster, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairFarming, [
            new(EnemyDefinitionId.Viola, row: 1),
            new(EnemyDefinitionId.Zebbo, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SingleChamber, [
            new(EnemyDefinitionId.Multiviola, row: 1),
            new(EnemyDefinitionId.Alcoon, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall, [
            new(EnemyDefinitionId.Geruta, row: 1),
            new(EnemyDefinitionId.Metaree, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BatCave, [
            new(EnemyDefinitionId.Gamet, row: 1),
            new(EnemyDefinitionId.Skree, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.KronicBoost, [
            new(EnemyDefinitionId.Viola, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BusinessCenter, [
            new(EnemyDefinitionId.Sova, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ThreeMusketeers, [
            new(EnemyDefinitionId.Hibashi, row: 1),
            new(EnemyDefinitionId.KihunterRed, row: 2),
            new(EnemyDefinitionId.ShutterShootable, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Crocomire, [
            new(EnemyDefinitionId.Crocomire, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairElevatorSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Volcano, [
            new(EnemyDefinitionId.Fune, row: 1),
            new(EnemyDefinitionId.Polyp, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming, [
            new(EnemyDefinitionId.Fune, row: 1),
            new(EnemyDefinitionId.Gamet, row: 2),
        ]),
    ];
}
