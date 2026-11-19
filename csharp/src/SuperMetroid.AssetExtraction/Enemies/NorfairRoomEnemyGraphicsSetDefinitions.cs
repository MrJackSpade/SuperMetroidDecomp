using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Norfair room-header region, in table order.</summary>
internal static class NorfairRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.GrappleTutorial3, [
            new(EnemyDefinitionId.Gamet),
            new(EnemyDefinitionId.Puyo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomireShaft, [
            new(EnemyDefinitionId.Viola),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MetalPirates, [
            new(EnemyDefinitionId.PirateGoldNinja),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AcidStatue, [
            new(EnemyDefinitionId.Chozo),
            new(EnemyDefinitionId.Magdollite),
            new(EnemyDefinitionId.Holtz),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PurpleFarming, [
            new(EnemyDefinitionId.Gamet),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.IceBeamGate, [
            new(EnemyDefinitionId.ShutterGrowing),
            new(EnemyDefinitionId.Dessgeega),
            new(EnemyDefinitionId.Mella),
            new(EnemyDefinitionId.Sova),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel, [
            new(EnemyDefinitionId.YappingMaw),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedKihunterShaft, [
            new(EnemyDefinitionId.KihunterRed),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FastRipper, [
            new(EnemyDefinitionId.JetRipper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WaveBeam, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FastPillarsSetup, [
            new(EnemyDefinitionId.PirateGoldWall),
            new(EnemyDefinitionId.PirateGoldWalking),
            new(EnemyDefinitionId.Viola),
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
            new(EnemyDefinitionId.DessgeegaLarge),
            new(EnemyDefinitionId.Multiviola),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GoldenTorizoEnergyRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Ridley, [
            new(EnemyDefinitionId.Ridley),
            new(EnemyDefinitionId.RidleyBreakupPiece),
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
            new(EnemyDefinitionId.Beetom),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RisingTide, [
            new(EnemyDefinitionId.Squeept),
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Dragon),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomireFarming, [
            new(EnemyDefinitionId.Gamet),
            new(EnemyDefinitionId.GrappleRipper),
            new(EnemyDefinitionId.SuspensorPlatform),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame, [
            new(EnemyDefinitionId.Namihe),
            new(EnemyDefinitionId.PirateGoldWall),
            new(EnemyDefinitionId.PirateGoldWalking),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DoubleChamber, [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.Kamer),
            new(EnemyDefinitionId.Kago),
            new(EnemyDefinitionId.GrappleRipper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GrappleBeam, [
            new(EnemyDefinitionId.Multiviola),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedPirateShaft, [
            new(EnemyDefinitionId.PirateRedWalking),
            new(EnemyDefinitionId.ArmoredZoomer),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway, [
            new(EnemyDefinitionId.PirateRedWalking),
            new(EnemyDefinitionId.Multiviola),
            new(EnemyDefinitionId.Cacatac),
            new(EnemyDefinitionId.Sova),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Plowerhouse, [
            new(EnemyDefinitionId.Zebbo),
            new(EnemyDefinitionId.Holtz),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairFireflea, [
            new(EnemyDefinitionId.Fireflea),
            new(EnemyDefinitionId.Boulder),
            new(EnemyDefinitionId.Fune),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel, [
            new(EnemyDefinitionId.Multiviola),
            new(EnemyDefinitionId.Magdollite),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LavaDive, [
            new(EnemyDefinitionId.Namihe),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Pillar, [
            new(EnemyDefinitionId.NuclearWaffle),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrocomireEscape, [
            new(EnemyDefinitionId.Dragon),
            new(EnemyDefinitionId.Geruta),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairSpringBallMaze, [
            new(EnemyDefinitionId.Alcoon),
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
            new(EnemyDefinitionId.ShutterShootable),
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Geruta),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Amphitheatre, [
            new(EnemyDefinitionId.PirateGoldWalking),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CathedralEntrance, [
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Dessgeega),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MainHall, [
            new(EnemyDefinitionId.Hibashi),
            new(EnemyDefinitionId.Dragon),
            new(EnemyDefinitionId.Gamet),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomireJump, [
            new(EnemyDefinitionId.Mella),
            new(EnemyDefinitionId.GrappleRipper),
            new(EnemyDefinitionId.SuspensorPlatform),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FrogSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrocomireSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrumbleShaft, [
            new(EnemyDefinitionId.Sova),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Wasteland, [
            new(EnemyDefinitionId.Hibashi),
            new(EnemyDefinitionId.DessgeegaLarge),
            new(EnemyDefinitionId.ShutterShootable),
            new(EnemyDefinitionId.KzanTop),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel, [
            new(EnemyDefinitionId.Dragon),
            new(EnemyDefinitionId.Gamet),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GrappleTutorial1, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BubbleMountainSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.HiJumpEnergyTank, [
            new(EnemyDefinitionId.Sova),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BubbleMountain, [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.JetRipper),
            new(EnemyDefinitionId.Cacatac),
            new(EnemyDefinitionId.Sova),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.IceBeamAcid, [
            new(EnemyDefinitionId.Tripper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.IceBeamTutorial, [
            new(EnemyDefinitionId.Boyon),
            new(EnemyDefinitionId.JetRipper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PostCrocomireMissile, [
            new(EnemyDefinitionId.Metaree),
            new(EnemyDefinitionId.Gamet),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Cathedral, [
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Geruta),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.NorfairReserveTank, [
            new(EnemyDefinitionId.Dragon),
            new(EnemyDefinitionId.Sova),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GrappleTutorial2, [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.Multiviola),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.IceBeamSnake, [
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Fune),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GoldenTorizo, [
            new(EnemyDefinitionId.GoldenTorizo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel, [
            new(EnemyDefinitionId.Tripper),
            new(EnemyDefinitionId.ShutterShootable),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpeedBooster, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairFarming, [
            new(EnemyDefinitionId.Viola),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SingleChamber, [
            new(EnemyDefinitionId.Multiviola),
            new(EnemyDefinitionId.Alcoon),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall, [
            new(EnemyDefinitionId.Geruta),
            new(EnemyDefinitionId.Metaree),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BatCave, [
            new(EnemyDefinitionId.Gamet),
            new(EnemyDefinitionId.Skree),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.KronicBoost, [
            new(EnemyDefinitionId.Viola),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BusinessCenter, [
            new(EnemyDefinitionId.Sova),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ThreeMusketeers, [
            new(EnemyDefinitionId.Hibashi),
            new(EnemyDefinitionId.KihunterRed),
            new(EnemyDefinitionId.ShutterShootable),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Crocomire, [
            new(EnemyDefinitionId.Crocomire),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.LowerNorfairElevatorSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Volcano, [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.Polyp),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming, [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.Gamet),
        ]),
    ];
}
