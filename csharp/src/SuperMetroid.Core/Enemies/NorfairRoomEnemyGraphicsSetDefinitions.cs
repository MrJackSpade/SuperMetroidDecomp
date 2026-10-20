namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$B4 graphics sets of the Norfair room-header region, in native table order.</summary>
internal static class NorfairRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleTutorial3), [
            new(EnemyDefinitionId.Gamet, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocShaft), [
            new(EnemyDefinitionId.Viola, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MetalPirates), [
            new(EnemyDefinitionId.PirateGoldNinja, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AcidStatue), [
            new(EnemyDefinitionId.Chozo, PaletteRow: 1),
            new(EnemyDefinitionId.Magdollite, PaletteRow: 3),
            new(EnemyDefinitionId.Holtz, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PurpleFarming), [
            new(EnemyDefinitionId.Gamet, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamGate), [
            new(EnemyDefinitionId.ShutterGrowing, PaletteRow: 1),
            new(EnemyDefinitionId.Dessgeega, PaletteRow: 2),
            new(EnemyDefinitionId.Mella, PaletteRow: 3),
            new(EnemyDefinitionId.Sova, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedKihunterShaft), [
            new(EnemyDefinitionId.KihunterRed, PaletteRow: 1),
            new(EnemyDefinitionId.Reflec, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FastRipper), [
            new(EnemyDefinitionId.Ripper2, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WaveBeam), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FastPillarsSetup), [
            new(EnemyDefinitionId.PirateGoldWall, PaletteRow: 1),
            new(EnemyDefinitionId.PirateGoldWalking, PaletteRow: 2),
            new(EnemyDefinitionId.Viola, PaletteRow: 3),
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
            new(EnemyDefinitionId.DessgeegaLarge, PaletteRow: 1),
            new(EnemyDefinitionId.Multiviola, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GTEnergyRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Ridley), [
            new(EnemyDefinitionId.Ridley, PaletteRow: 1),
            new(EnemyDefinitionId.RidleyExplosion, PaletteRow: 1, StagingBlock: 2),
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
            new(EnemyDefinitionId.Beetom, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RisingTide), [
            new(EnemyDefinitionId.Squeept, PaletteRow: 1),
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
            new(EnemyDefinitionId.Dragon, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocFarming), [
            new(EnemyDefinitionId.Gamet, PaletteRow: 1),
            new(EnemyDefinitionId.GRipper, PaletteRow: 2),
            new(EnemyDefinitionId.Kamer2, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame), [
            new(EnemyDefinitionId.Namihe, PaletteRow: 1),
            new(EnemyDefinitionId.PirateGoldWall, PaletteRow: 2),
            new(EnemyDefinitionId.PirateGoldWalking, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DoubleChamber), [
            new(EnemyDefinitionId.Fune, PaletteRow: 1),
            new(EnemyDefinitionId.Kamer, PaletteRow: 2),
            new(EnemyDefinitionId.Kago, PaletteRow: 3),
            new(EnemyDefinitionId.GRipper, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleBeam), [
            new(EnemyDefinitionId.Multiviola, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedPirateShaft), [
            new(EnemyDefinitionId.PirateRedWalking, PaletteRow: 1),
            new(EnemyDefinitionId.MZoomer, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway), [
            new(EnemyDefinitionId.PirateRedWalking, PaletteRow: 1),
            new(EnemyDefinitionId.Multiviola, PaletteRow: 2),
            new(EnemyDefinitionId.Cacatac, PaletteRow: 3),
            new(EnemyDefinitionId.Sova, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Plowerhouse), [
            new(EnemyDefinitionId.Zebbo, PaletteRow: 1),
            new(EnemyDefinitionId.Holtz, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNFireflea), [
            new(EnemyDefinitionId.Fireflea, PaletteRow: 1),
            new(EnemyDefinitionId.Boulder, PaletteRow: 2),
            new(EnemyDefinitionId.Fune, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel), [
            new(EnemyDefinitionId.Multiviola, PaletteRow: 1),
            new(EnemyDefinitionId.Magdollite, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UnusedState8FB3EEB48899), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LavaDive), [
            new(EnemyDefinitionId.Namihe, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pillar), [
            new(EnemyDefinitionId.Puromi, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireEscape), [
            new(EnemyDefinitionId.Dragon, PaletteRow: 1),
            new(EnemyDefinitionId.Geruta, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNSpringBallMaze), [
            new(EnemyDefinitionId.Alcoon, PaletteRow: 1),
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
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 7),
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
            new(EnemyDefinitionId.Geruta, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Amphitheatre), [
            new(EnemyDefinitionId.PirateGoldWalking, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CathedralEntrance), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
            new(EnemyDefinitionId.Dessgeega, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MainHall), [
            new(EnemyDefinitionId.Hibashi, PaletteRow: 1),
            new(EnemyDefinitionId.Dragon, PaletteRow: 7),
            new(EnemyDefinitionId.Gamet, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocJump), [
            new(EnemyDefinitionId.Mella, PaletteRow: 1),
            new(EnemyDefinitionId.GRipper, PaletteRow: 2),
            new(EnemyDefinitionId.Kamer2, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FrogSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrocomireSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrumbleShaft), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Wasteland), [
            new(EnemyDefinitionId.Hibashi, PaletteRow: 1),
            new(EnemyDefinitionId.DessgeegaLarge, PaletteRow: 2),
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 3),
            new(EnemyDefinitionId.KzanTop, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel), [
            new(EnemyDefinitionId.Dragon, PaletteRow: 1),
            new(EnemyDefinitionId.Gamet, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleTutorial1), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BubbleMountainSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HiJumpETank), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BubbleMountain), [
            new(EnemyDefinitionId.Waver, PaletteRow: 1),
            new(EnemyDefinitionId.Ripper2, PaletteRow: 1),
            new(EnemyDefinitionId.Cacatac, PaletteRow: 2),
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamAcid), [
            new(EnemyDefinitionId.Tripper, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamTutorial), [
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
            new(EnemyDefinitionId.Ripper2, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PostCrocMissile), [
            new(EnemyDefinitionId.Metaree, PaletteRow: 1),
            new(EnemyDefinitionId.Gamet, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Cathedral), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
            new(EnemyDefinitionId.Geruta, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NorfairReserveTank), [
            new(EnemyDefinitionId.Dragon, PaletteRow: 1),
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GrappleTutorial2), [
            new(EnemyDefinitionId.Fune, PaletteRow: 1),
            new(EnemyDefinitionId.Multiviola, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.IceBeamSnake), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
            new(EnemyDefinitionId.Fune, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GoldenTorizo), [
            new(EnemyDefinitionId.GoldenTorizo, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel), [
            new(EnemyDefinitionId.Tripper, PaletteRow: 7),
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpeedBooster), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNFarming), [
            new(EnemyDefinitionId.Viola, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SingleChamber), [
            new(EnemyDefinitionId.Multiviola, PaletteRow: 1),
            new(EnemyDefinitionId.Alcoon, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall), [
            new(EnemyDefinitionId.Geruta, PaletteRow: 1),
            new(EnemyDefinitionId.Metaree, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BatCave), [
            new(EnemyDefinitionId.Gamet, PaletteRow: 1),
            new(EnemyDefinitionId.Skree, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.KronicBoost), [
            new(EnemyDefinitionId.Viola, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BusinessCenter), [
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ThreeMusketeers), [
            new(EnemyDefinitionId.Hibashi, PaletteRow: 1),
            new(EnemyDefinitionId.KihunterRed, PaletteRow: 2),
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Crocomire), [
            new(EnemyDefinitionId.Crocomire, PaletteRow: 7, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.LNElevSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Volcano), [
            new(EnemyDefinitionId.Fune, PaletteRow: 1),
            new(EnemyDefinitionId.LavaRocks, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming), [
            new(EnemyDefinitionId.Fune, PaletteRow: 1),
            new(EnemyDefinitionId.Gamet, PaletteRow: 2),
        ]),
    ];
}
