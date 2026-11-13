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
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocShaft), [
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
            new(EnemyDefinitionId.Ripper2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WaveBeam), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FastPillarsSetup), [
            new(EnemyDefinitionId.PirateGoldWall),
            new(EnemyDefinitionId.PirateGoldWalking),
            new(EnemyDefinitionId.Viola),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LowerNorfairElev), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UpperNorfairRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NorfairMap), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MickeyMouse), [
            new(EnemyDefinitionId.DessgeegaLarge),
            new(EnemyDefinitionId.Multiviola),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GTEnergyRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Ridley), [
            new(EnemyDefinitionId.Ridley),
            new(EnemyDefinitionId.RidleyExplosion),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocPowerBombs), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeam), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HiJumpBoots), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FrogSpeedway), [
            new(EnemyDefinitionId.Beetom),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RisingTide), [
            new(EnemyDefinitionId.Squeept),
            new(EnemyDefinitionId.Sova),
            new(EnemyDefinitionId.Dragon),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocFarming), [
            new(EnemyDefinitionId.Gamet),
            new(EnemyDefinitionId.GRipper),
            new(EnemyDefinitionId.Kamer2),
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
            new(EnemyDefinitionId.GRipper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleBeam), [
            new(EnemyDefinitionId.Multiviola),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedPirateShaft), [
            new(EnemyDefinitionId.PirateRedWalking),
            new(EnemyDefinitionId.MZoomer),
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNFireflea), [
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
            new(EnemyDefinitionId.Puromi),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireEscape), [
            new(EnemyDefinitionId.Dragon),
            new(EnemyDefinitionId.Geruta),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNSpringBallMaze), [
            new(EnemyDefinitionId.Alcoon),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNEscapePowerBombs), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PurpleShaft), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RidleyETank), [
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocJump), [
            new(EnemyDefinitionId.Mella),
            new(EnemyDefinitionId.GRipper),
            new(EnemyDefinitionId.Kamer2),
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.HiJumpETank), [
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BubbleMountain), [
            new(EnemyDefinitionId.Waver),
            new(EnemyDefinitionId.Ripper2),
            new(EnemyDefinitionId.Cacatac),
            new(EnemyDefinitionId.Sova),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamAcid), [
            new(EnemyDefinitionId.Tripper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamTutorial), [
            new(EnemyDefinitionId.Boyon),
            new(EnemyDefinitionId.Ripper2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocMissile), [
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNFarming), [
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
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNElevSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Volcano), [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.LavaRocks),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming), [
            new(EnemyDefinitionId.Fune),
            new(EnemyDefinitionId.Gamet),
        ]),
    ];
}
