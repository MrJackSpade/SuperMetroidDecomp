namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$B4 graphics sets of the Norfair room-header region, in native table order.</summary>
internal static class NorfairRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleTutorial3), [
            new(EnemyDefinitionId.Gamet, ColourStore: new(1)),
            new(EnemyDefinitionId.Puyo, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocShaft), [
            new(EnemyDefinitionId.Viola, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MetalPirates), [
            new(EnemyDefinitionId.PirateGoldNinja, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AcidStatue), [
            new(EnemyDefinitionId.Chozo, ColourStore: new(1)),
            new(EnemyDefinitionId.Magdollite, ColourStore: new(3)),
            new(EnemyDefinitionId.Holtz, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PurpleFarming), [
            new(EnemyDefinitionId.Gamet, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamGate), [
            new(EnemyDefinitionId.ShutterGrowing, ColourStore: new(1)),
            new(EnemyDefinitionId.Dessgeega, ColourStore: new(2)),
            new(EnemyDefinitionId.Mella, ColourStore: new(3)),
            new(EnemyDefinitionId.Sova, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel), [
            new(EnemyDefinitionId.YappingMaw, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedKihunterShaft), [
            new(EnemyDefinitionId.KihunterRed, ColourStore: new(1)),
            new(EnemyDefinitionId.Reflec, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FastRipper), [
            new(EnemyDefinitionId.Ripper2, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WaveBeam), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FastPillarsSetup), [
            new(EnemyDefinitionId.PirateGoldWall, ColourStore: new(1)),
            new(EnemyDefinitionId.PirateGoldWalking, ColourStore: new(2)),
            new(EnemyDefinitionId.Viola, ColourStore: new(3)),
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
            new(EnemyDefinitionId.DessgeegaLarge, ColourStore: new(1)),
            new(EnemyDefinitionId.Multiviola, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GTEnergyRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Ridley), [
            new(EnemyDefinitionId.Ridley, ColourStore: new(1)),
            new(EnemyDefinitionId.RidleyExplosion, ColourStore: new(1), StagingBlock: 2),
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
            new(EnemyDefinitionId.Beetom, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RisingTide), [
            new(EnemyDefinitionId.Squeept, ColourStore: new(1)),
            new(EnemyDefinitionId.Sova, ColourStore: new(2)),
            new(EnemyDefinitionId.Dragon, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocFarming), [
            new(EnemyDefinitionId.Gamet, ColourStore: new(1)),
            new(EnemyDefinitionId.GRipper, ColourStore: new(2)),
            new(EnemyDefinitionId.Kamer2, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame), [
            new(EnemyDefinitionId.Namihe, ColourStore: new(1)),
            new(EnemyDefinitionId.PirateGoldWall, ColourStore: new(2)),
            new(EnemyDefinitionId.PirateGoldWalking, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DoubleChamber), [
            new(EnemyDefinitionId.Fune, ColourStore: new(1)),
            new(EnemyDefinitionId.Kamer, ColourStore: new(2)),
            new(EnemyDefinitionId.Kago, ColourStore: new(3)),
            new(EnemyDefinitionId.GRipper, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleBeam), [
            new(EnemyDefinitionId.Multiviola, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedPirateShaft), [
            new(EnemyDefinitionId.PirateRedWalking, ColourStore: new(1)),
            new(EnemyDefinitionId.MZoomer, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway), [
            new(EnemyDefinitionId.PirateRedWalking, ColourStore: new(1)),
            new(EnemyDefinitionId.Multiviola, ColourStore: new(2)),
            new(EnemyDefinitionId.Cacatac, ColourStore: new(3)),
            new(EnemyDefinitionId.Sova, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Plowerhouse), [
            new(EnemyDefinitionId.Zebbo, ColourStore: new(1)),
            new(EnemyDefinitionId.Holtz, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNFireflea), [
            new(EnemyDefinitionId.Fireflea, ColourStore: new(1)),
            new(EnemyDefinitionId.Boulder, ColourStore: new(2)),
            new(EnemyDefinitionId.Fune, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel), [
            new(EnemyDefinitionId.Multiviola, ColourStore: new(1)),
            new(EnemyDefinitionId.Magdollite, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UnusedState8FB3EEB48899), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LavaDive), [
            new(EnemyDefinitionId.Namihe, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pillar), [
            new(EnemyDefinitionId.Puromi, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireEscape), [
            new(EnemyDefinitionId.Dragon, ColourStore: new(1)),
            new(EnemyDefinitionId.Geruta, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNSpringBallMaze), [
            new(EnemyDefinitionId.Alcoon, ColourStore: new(1)),
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
            new(EnemyDefinitionId.ShutterShootable, ColourStore: new(7)),
            new(EnemyDefinitionId.Sova, ColourStore: new(2)),
            new(EnemyDefinitionId.Geruta, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Amphitheatre), [
            new(EnemyDefinitionId.PirateGoldWalking, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CathedralEntrance), [
            new(EnemyDefinitionId.Sova, ColourStore: new(1)),
            new(EnemyDefinitionId.Dessgeega, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MainHall), [
            new(EnemyDefinitionId.Hibashi, ColourStore: new(1)),
            new(EnemyDefinitionId.Dragon, ColourStore: new(7)),
            new(EnemyDefinitionId.Gamet, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocJump), [
            new(EnemyDefinitionId.Mella, ColourStore: new(1)),
            new(EnemyDefinitionId.GRipper, ColourStore: new(2)),
            new(EnemyDefinitionId.Kamer2, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FrogSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrumbleShaft), [
            new(EnemyDefinitionId.Sova, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Wasteland), [
            new(EnemyDefinitionId.Hibashi, ColourStore: new(1)),
            new(EnemyDefinitionId.DessgeegaLarge, ColourStore: new(2)),
            new(EnemyDefinitionId.ShutterShootable, ColourStore: new(3)),
            new(EnemyDefinitionId.KzanTop, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel), [
            new(EnemyDefinitionId.Dragon, ColourStore: new(1)),
            new(EnemyDefinitionId.Gamet, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleTutorial1), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BubbleMountainSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HiJumpETank), [
            new(EnemyDefinitionId.Sova, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BubbleMountain), [
            new(EnemyDefinitionId.Waver, ColourStore: new(1)),
            new(EnemyDefinitionId.Ripper2, ColourStore: new(1)),
            new(EnemyDefinitionId.Cacatac, ColourStore: new(2)),
            new(EnemyDefinitionId.Sova, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamAcid), [
            new(EnemyDefinitionId.Tripper, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamTutorial), [
            new(EnemyDefinitionId.Boyon, ColourStore: new(1)),
            new(EnemyDefinitionId.Ripper2, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocMissile), [
            new(EnemyDefinitionId.Metaree, ColourStore: new(1)),
            new(EnemyDefinitionId.Gamet, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Cathedral), [
            new(EnemyDefinitionId.Sova, ColourStore: new(1)),
            new(EnemyDefinitionId.Geruta, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NorfairReserveTank), [
            new(EnemyDefinitionId.Dragon, ColourStore: new(1)),
            new(EnemyDefinitionId.Sova, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleTutorial2), [
            new(EnemyDefinitionId.Fune, ColourStore: new(1)),
            new(EnemyDefinitionId.Multiviola, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamSnake), [
            new(EnemyDefinitionId.Sova, ColourStore: new(1)),
            new(EnemyDefinitionId.Fune, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GoldenTorizo), [
            new(EnemyDefinitionId.GoldenTorizo, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel), [
            new(EnemyDefinitionId.Tripper, ColourStore: new(7)),
            new(EnemyDefinitionId.ShutterShootable, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpeedBooster), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNFarming), [
            new(EnemyDefinitionId.Viola, ColourStore: new(1)),
            new(EnemyDefinitionId.Zebbo, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SingleChamber), [
            new(EnemyDefinitionId.Multiviola, ColourStore: new(1)),
            new(EnemyDefinitionId.Alcoon, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall), [
            new(EnemyDefinitionId.Geruta, ColourStore: new(1)),
            new(EnemyDefinitionId.Metaree, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BatCave), [
            new(EnemyDefinitionId.Gamet, ColourStore: new(1)),
            new(EnemyDefinitionId.Skree, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.KronicBoost), [
            new(EnemyDefinitionId.Viola, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BusinessCenter), [
            new(EnemyDefinitionId.Sova, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ThreeMusketeers), [
            new(EnemyDefinitionId.Hibashi, ColourStore: new(1)),
            new(EnemyDefinitionId.KihunterRed, ColourStore: new(2)),
            new(EnemyDefinitionId.ShutterShootable, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Crocomire), [
            new(EnemyDefinitionId.Crocomire, ColourStore: new(7), StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNElevSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Volcano), [
            new(EnemyDefinitionId.Fune, ColourStore: new(1)),
            new(EnemyDefinitionId.LavaRocks, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming), [
            new(EnemyDefinitionId.Fune, ColourStore: new(1)),
            new(EnemyDefinitionId.Gamet, ColourStore: new(2)),
        ]),
    ];
}
