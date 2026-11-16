using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Norfair room-header region, in table order.</summary>
internal static class NorfairRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleTutorial3), [
            new(EnemyDefinitionId.Gamet),
            new(EnemyDefinitionId.Puyo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocomireShaft), [
            new(EnemyDefinitionId.Viola),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MetalPirates), [
            new(EnemyDefinitionId.PirateGoldNinja),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AcidStatue), [
            new(EnemyDefinitionId.Chozo),
            new(EnemyDefinitionId.Magdollite),
            new(EnemyDefinitionId.Holtz),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PurpleFarming), [
            new(EnemyDefinitionId.Gamet),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamGate), [
            new(EnemyDefinitionId.ShutterGrowing),
            new(EnemyDefinitionId.Dessgeega),
            new(EnemyDefinitionId.Mella),
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel), [
            new(EnemyDefinitionId.YappingMaw),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedKihunterShaft), [
            new(EnemyDefinitionId.KihunterRed),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FastRipper), [
            new(EnemyDefinitionId.JetRipper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WaveBeam), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FastPillarsSetup), [
            new(EnemyDefinitionId.PirateGoldWall),
            new(EnemyDefinitionId.PirateGoldWalking),
            new(EnemyDefinitionId.Viola),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerNorfairElevator), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerNorfairSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UpperNorfairRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NorfairMap), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MickeyMouse), [
            new(EnemyDefinitionId.DessgeegaLarge),
            new(EnemyDefinitionId.Multiviola),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GoldenTorizoEnergyRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Ridley), [
            new(EnemyDefinitionId.Ridley),
            new(EnemyDefinitionId.RidleyBreakupPiece),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocomirePowerBombs), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeam), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HiJumpBoots), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocomireSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FrogSpeedway), [
            new(EnemyDefinitionId.Beetom),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RisingTide), [
            new(EnemyDefinitionId.Squeept),
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Dragon),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocomireFarming), [
            new(EnemyDefinitionId.Gamet),
            new(EnemyDefinitionId.GrappleRipper),
            new(EnemyDefinitionId.SuspensorPlatform),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame), [
            new(EnemyDefinitionId.Namihe),
            new(EnemyDefinitionId.PirateGoldWall),
            new(EnemyDefinitionId.PirateGoldWalking),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DoubleChamber), [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.Kamer),
            new(EnemyDefinitionId.Kago),
            new(EnemyDefinitionId.GrappleRipper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleBeam), [
            new(EnemyDefinitionId.Multiviola),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedPirateShaft), [
            new(EnemyDefinitionId.PirateRedWalking),
            new(EnemyDefinitionId.ArmoredZoomer),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway), [
            new(EnemyDefinitionId.PirateRedWalking),
            new(EnemyDefinitionId.Multiviola),
            new(EnemyDefinitionId.Cacatac),
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Plowerhouse), [
            new(EnemyDefinitionId.Zebbo),
            new(EnemyDefinitionId.Holtz),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerNorfairFireflea), [
            new(EnemyDefinitionId.Fireflea),
            new(EnemyDefinitionId.Boulder),
            new(EnemyDefinitionId.Fune),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel), [
            new(EnemyDefinitionId.Multiviola),
            new(EnemyDefinitionId.Magdollite),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LavaDive), [
            new(EnemyDefinitionId.Namihe),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pillar), [
            new(EnemyDefinitionId.NuclearWaffle),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireEscape), [
            new(EnemyDefinitionId.Dragon),
            new(EnemyDefinitionId.Geruta),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerNorfairSpringBallMaze), [
            new(EnemyDefinitionId.Alcoon),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerNorfairEscapePowerBombs), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PurpleShaft), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RidleyEnergyTank), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ScrewAttack), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GreenBubblesMissiles), [
            new(EnemyDefinitionId.ShutterShootable),
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Geruta),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Amphitheatre), [
            new(EnemyDefinitionId.PirateGoldWalking),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CathedralEntrance), [
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Dessgeega),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MainHall), [
            new(EnemyDefinitionId.Hibashi),
            new(EnemyDefinitionId.Dragon),
            new(EnemyDefinitionId.Gamet),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocomireJump), [
            new(EnemyDefinitionId.Mella),
            new(EnemyDefinitionId.GrappleRipper),
            new(EnemyDefinitionId.SuspensorPlatform),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FrogSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrumbleShaft), [
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Wasteland), [
            new(EnemyDefinitionId.Hibashi),
            new(EnemyDefinitionId.DessgeegaLarge),
            new(EnemyDefinitionId.ShutterShootable),
            new(EnemyDefinitionId.KzanTop),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel), [
            new(EnemyDefinitionId.Dragon),
            new(EnemyDefinitionId.Gamet),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleTutorial1), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BubbleMountainSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HiJumpEnergyTank), [
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BubbleMountain), [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.JetRipper),
            new(EnemyDefinitionId.Cacatac),
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamAcid), [
            new(EnemyDefinitionId.Tripper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamTutorial), [
            new(EnemyDefinitionId.Boyon),
            new(EnemyDefinitionId.JetRipper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocomireMissile), [
            new(EnemyDefinitionId.Metaree),
            new(EnemyDefinitionId.Gamet),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Cathedral), [
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Geruta),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NorfairReserveTank), [
            new(EnemyDefinitionId.Dragon),
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleTutorial2), [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.Multiviola),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamSnake), [
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Fune),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GoldenTorizo), [
            new(EnemyDefinitionId.GoldenTorizo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel), [
            new(EnemyDefinitionId.Tripper),
            new(EnemyDefinitionId.ShutterShootable),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpeedBooster), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerNorfairFarming), [
            new(EnemyDefinitionId.Viola),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SingleChamber), [
            new(EnemyDefinitionId.Multiviola),
            new(EnemyDefinitionId.Alcoon),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall), [
            new(EnemyDefinitionId.Geruta),
            new(EnemyDefinitionId.Metaree),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BatCave), [
            new(EnemyDefinitionId.Gamet),
            new(EnemyDefinitionId.Skree),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.KronicBoost), [
            new(EnemyDefinitionId.Viola),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BusinessCenter), [
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ThreeMusketeers), [
            new(EnemyDefinitionId.Hibashi),
            new(EnemyDefinitionId.KihunterRed),
            new(EnemyDefinitionId.ShutterShootable),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Crocomire), [
            new(EnemyDefinitionId.Crocomire),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerNorfairElevatorSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Volcano), [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.Polyp),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming), [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.Gamet),
        ]),
    ];
}
