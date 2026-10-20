namespace SuperMetroid.Core.Rooms;

/// <summary>The initialization settings of each Crateria room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class CrateriaRoomStateDefinitions
{
    /// <summary>The disassembly's RoomState_LandingSite_0.</summary>
    public static CartridgeRoomState LandingSite0 { get; } =
        new(nameof(LandingSite0), 0xC2C2BB, 0x00, 0x06, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState01, Enemies.RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, Plms.RoomPlmPopulationDefinitions.LandingSiteState012, Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The disassembly's RoomState_LandingSite_1.</summary>
    public static CartridgeRoomState LandingSite1 { get; } =
        new(nameof(LandingSite1), 0xC2C2BB, 0x00, 0x06, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState01, Enemies.RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, Plms.RoomPlmPopulationDefinitions.LandingSiteState012, Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The disassembly's RoomState_LandingSite_2.</summary>
    public static CartridgeRoomState LandingSite2 { get; } =
        new(nameof(LandingSite2), 0xC2C2BB, 0x00, 0x0C, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState2, Enemies.RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, Plms.RoomPlmPopulationDefinitions.LandingSiteState012, Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The disassembly's RoomState_LandingSite_3.</summary>
    public static CartridgeRoomState LandingSite3 { get; } =
        new(nameof(LandingSite3), 0xC2C2BB, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState3, Enemies.RoomEnemyPopulationDefinitions.LandingSite3, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite3, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLandZebesTimebombSet, Plms.RoomPlmPopulationDefinitions.LandingSiteState3, Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape);


    /// <summary>The disassembly's RoomState_GauntletEntrance_0.</summary>
    public static CartridgeRoomState GauntletEntrance0 { get; } =
        new(nameof(GauntletEntrance0), 0xC2D6E8, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntranceState01, Enemies.RoomEnemyPopulationDefinitions.GauntletEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletEntrance, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GauntletEntranceState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_GauntletEntrance_1.</summary>
    public static CartridgeRoomState GauntletEntrance1 { get; } =
        new(nameof(GauntletEntrance1), 0xC2D6E8, 0x00, 0x09, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntranceState01, Enemies.RoomEnemyPopulationDefinitions.GauntletEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletEntrance, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GauntletEntranceState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_Parlor_0.</summary>
    public static CartridgeRoomState Parlor0 { get; } =
        new(nameof(Parlor0), 0xC2DBC4, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorState0, Enemies.RoomEnemyPopulationDefinitions.Parlor0, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ParlorState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_Parlor_1.</summary>
    public static CartridgeRoomState Parlor1 { get; } =
        new(nameof(Parlor1), 0xC2DBC4, 0x00, 0x09, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorState1, Enemies.RoomEnemyPopulationDefinitions.Parlor1, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ParlorState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_Parlor_2.</summary>
    public static CartridgeRoomState Parlor2 { get; } =
        new(nameof(Parlor2), 0xC2DBC4, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorState2, Enemies.RoomEnemyPopulationDefinitions.Parlor2, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, Plms.RoomPlmPopulationDefinitions.ParlorState2, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen);


    /// <summary>The disassembly's RoomState_CrateriaPowerBombs.</summary>
    public static CartridgeRoomState CrateriaPowerBombs { get; } =
        new(nameof(CrateriaPowerBombs), 0xC2E977, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaPowerBombs, Enemies.RoomEnemyPopulationDefinitions.CrateriaPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaPowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrateriaPowerBomb, null, RoomMainCallback.ScrollingSkyLand, Plms.RoomPlmPopulationDefinitions.CrateriaPowerBombs, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrateriaSave.</summary>
    public static CartridgeRoomState CrateriaSave { get; } =
        new(nameof(CrateriaSave), 0xCE92CB, 0x19, 0x09, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSave, Enemies.RoomEnemyPopulationDefinitions.CrateriaSave, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrateriaSave, null, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_WestOcean.</summary>
    public static CartridgeRoomState WestOcean { get; } =
        new(nameof(WestOcean), 0xC2EB45, 0x00, 0x0C, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.WestOcean, Enemies.RoomEnemyPopulationDefinitions.WestOcean, Enemies.RoomEnemyGraphicsSetDefinitions.WestOcean, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.WestOcean, null, RoomMainCallback.ScrollingSkyOcean, Plms.RoomPlmPopulationDefinitions.WestOcean, Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsWreckedShipEntrance, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The disassembly's RoomState_BowlingAlleyPath.</summary>
    public static CartridgeRoomState BowlingAlleyPath { get; } =
        new(nameof(BowlingAlleyPath), 0xC38E1F, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyPath, Enemies.RoomEnemyPopulationDefinitions.BowlingAlleyPath, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlleyPath, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BowlingAlleyPath, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrateriaKihunter.</summary>
    public static CartridgeRoomState CrateriaKihunter { get; } =
        new(nameof(CrateriaKihunter), 0xC38FFE, 0x00, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaKihunters, Enemies.RoomEnemyPopulationDefinitions.CrateriaKihunter, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaKihunter, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrateriaKihunter, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrateriaKihunter, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_ForgottenHighwayElev.</summary>
    public static CartridgeRoomState ForgottenHighwayElev { get; } =
        new(nameof(ForgottenHighwayElev), 0xC39BFC, 0x02, 0x09, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElev, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwayElev, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElev, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ForgottenHighwayElev, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ForgottenHighwayElev, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2Elevator, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_EastOcean.</summary>
    public static CartridgeRoomState EastOcean { get; } =
        new(nameof(EastOcean), 0xC39DB8, 0x00, 0x0C, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.EastOcean, Enemies.RoomEnemyPopulationDefinitions.EastOcean, Enemies.RoomEnemyGraphicsSetDefinitions.EastOcean, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.EastOcean, null, RoomMainCallback.ScrollingSkyOcean, Plms.RoomPlmPopulationDefinitions.EastOcean, Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsEastOcean, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The disassembly's RoomState_ForgottenHighwayKagos.</summary>
    public static CartridgeRoomState ForgottenHighwayKagos { get; } =
        new(nameof(ForgottenHighwayKagos), 0xC3ACDB, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayKagos, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwayKagos, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayKagos, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ForgottenHighwayKagos, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0VerticalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrabMaze.</summary>
    public static CartridgeRoomState CrabMaze { get; } =
        new(nameof(CrabMaze), 0xC3B1BA, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrabMaze, Enemies.RoomEnemyPopulationDefinitions.CrabMaze, Enemies.RoomEnemyGraphicsSetDefinitions.CrabMaze, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrabMaze, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_ForgottenHighwayElbow.</summary>
    public static CartridgeRoomState ForgottenHighwayElbow { get; } =
        new(nameof(ForgottenHighwayElbow), 0xC3BB9B, 0x00, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElbow, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwayElbow, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElbow, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ForgottenHighwayElbow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ForgottenHighwayElbow, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0VerticalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrateriaTube.</summary>
    public static CartridgeRoomState CrateriaTube { get; } =
        new(nameof(CrateriaTube), 0xC3BCD2, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaTube, Enemies.RoomEnemyPopulationDefinitions.CrateriaTube, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaTube, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrateriaTube, null, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_Moat.</summary>
    public static CartridgeRoomState Moat { get; } =
        new(nameof(Moat), 0xC3BD6D, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Moat, Enemies.RoomEnemyPopulationDefinitions.Moat, Enemies.RoomEnemyGraphicsSetDefinitions.Moat, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Moat, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_RedBinstarElev.</summary>
    public static CartridgeRoomState RedBinstarElev { get; } =
        new(nameof(RedBinstarElev), 0xC3C145, 0x02, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarElev, Enemies.RoomEnemyPopulationDefinitions.RedBinstarElev, Enemies.RoomEnemyGraphicsSetDefinitions.RedBinstarElev, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RedBinstarElev, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedBinstarElev, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2Elevator, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_GauntletETank.</summary>
    public static CartridgeRoomState GauntletETank { get; } =
        new(nameof(GauntletETank), 0xC3C301, 0x00, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletETank, Enemies.RoomEnemyPopulationDefinitions.GauntletETank, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletETank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GauntletETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GauntletETank, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_PreBowling.</summary>
    public static CartridgeRoomState PreBowling { get; } =
        new(nameof(PreBowling), 0xC3C80C, 0x00, 0x0C, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PreBowling, Enemies.RoomEnemyPopulationDefinitions.PreBowling, Enemies.RoomEnemyGraphicsSetDefinitions.PreBowling, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.ScrollingSkyOcean, Plms.RoomPlmPopulationDefinitions.PreBowling, Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsBowlingAlley, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The disassembly's RoomState_Climb_0.</summary>
    public static CartridgeRoomState Climb0 { get; } =
        new(nameof(Climb0), 0xC3C998, 0x03, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbState0, Enemies.RoomEnemyPopulationDefinitions.Climb0, Enemies.RoomEnemyGraphicsSetDefinitions.Climb0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ClimbState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Climb_1.</summary>
    public static CartridgeRoomState Climb1 { get; } =
        new(nameof(Climb1), 0xC3C998, 0x02, 0x09, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbState1, Enemies.RoomEnemyPopulationDefinitions.Climb1, Enemies.RoomEnemyGraphicsSetDefinitions.Climb1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ClimbState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_Climb_2.</summary>
    public static CartridgeRoomState Climb2 { get; } =
        new(nameof(Climb2), 0xC3C998, 0x02, 0x24, 0x07, SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbState2, Enemies.RoomEnemyPopulationDefinitions.Climb2, Enemies.RoomEnemyGraphicsSetDefinitions.Climb2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, Plms.RoomPlmPopulationDefinitions.ClimbState2, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.AutoDestroyWallDuringEscape);


    /// <summary>The disassembly's RoomState_Pit_0.</summary>
    public static CartridgeRoomState Pit0 { get; } =
        new(nameof(Pit0), 0xC3D9F7, 0x03, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PitState0, Enemies.RoomEnemyPopulationDefinitions.Pit0, Enemies.RoomEnemyGraphicsSetDefinitions.Pit0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Pit, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Pit0, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Pit_1.</summary>
    public static CartridgeRoomState Pit1 { get; } =
        new(nameof(Pit1), 0xC3D9F7, 0x02, 0x09, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PitState1, Enemies.RoomEnemyPopulationDefinitions.Pit1, Enemies.RoomEnemyGraphicsSetDefinitions.Pit1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Pit, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Pit1, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_ElevToBlueBrinstar_0.</summary>
    public static CartridgeRoomState ElevToBlueBrinstar0 { get; } =
        new(nameof(ElevToBlueBrinstar0), 0xC3DF23, 0x02, 0x06, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarState0, Enemies.RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, Enemies.RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ElevToBlueBrinstarState01, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_ElevToBlueBrinstar_1.</summary>
    public static CartridgeRoomState ElevToBlueBrinstar1 { get; } =
        new(nameof(ElevToBlueBrinstar1), 0xC3DF23, 0x02, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarState1, Enemies.RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, Enemies.RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ElevToBlueBrinstarState01, null, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_BombTorizo_0.</summary>
    public static CartridgeRoomState BombTorizo0 { get; } =
        new(nameof(BombTorizo0), 0xC3E0D0, 0x02, 0x24, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoState01, Enemies.RoomEnemyPopulationDefinitions.BombTorizo01, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BombTorizoState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_BombTorizo_1.</summary>
    public static CartridgeRoomState BombTorizo1 { get; } =
        new(nameof(BombTorizo1), 0xC3E0D0, 0x02, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoState01, Enemies.RoomEnemyPopulationDefinitions.BombTorizo01, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BombTorizoState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_BombTorizo_2.</summary>
    public static CartridgeRoomState BombTorizo2 { get; } =
        new(nameof(BombTorizo2), 0xC3E0D0, 0x02, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoState2, Enemies.RoomEnemyPopulationDefinitions.BombTorizo2, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinitions.BombTorizoAnimalEscapeBlocks, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, Plms.RoomPlmPopulationDefinitions.BombTorizoState2, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape);


    /// <summary>The disassembly's RoomState_Flyway_0.</summary>
    public static CartridgeRoomState Flyway0 { get; } =
        new(nameof(Flyway0), 0xC3E16E, 0x02, 0x09, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayState01, Enemies.RoomEnemyPopulationDefinitions.Flyway01, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FlywayState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Flyway_1.</summary>
    public static CartridgeRoomState Flyway1 { get; } =
        new(nameof(Flyway1), 0xC3E16E, 0x02, 0x09, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayState01, Enemies.RoomEnemyPopulationDefinitions.Flyway01, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FlywayState01, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Flyway_2.</summary>
    public static CartridgeRoomState Flyway2 { get; } =
        new(nameof(Flyway2), 0xC3E16E, 0x02, 0x24, 0x07, SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayState2, Enemies.RoomEnemyPopulationDefinitions.Flyway2, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, Plms.RoomPlmPopulationDefinitions.FlywayState2, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterEscapeWallSetup);


    /// <summary>The disassembly's RoomState_PreMapFlyway.</summary>
    public static CartridgeRoomState PreMapFlyway { get; } =
        new(nameof(PreMapFlyway), 0xC3E232, 0x02, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.PreMapFlyway, Enemies.RoomEnemyGraphicsSetDefinitions.PreMapFlyway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PreMapFlyway, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoomWallArt, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Terminator.</summary>
    public static CartridgeRoomState Terminator { get; } =
        new(nameof(Terminator), 0xC3E2FC, 0x02, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.Terminator, Enemies.RoomEnemyGraphicsSetDefinitions.Terminator, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Terminator, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2PurpleRocks, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_GreenBrinstarElev.</summary>
    public static CartridgeRoomState GreenBrinstarElev { get; } =
        new(nameof(GreenBrinstarElev), 0xC3E985, 0x02, 0x09, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarElev, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarElev, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenBrinstarElev, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarElev, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_LowerMushrooms.</summary>
    public static CartridgeRoomState LowerMushrooms { get; } =
        new(nameof(LowerMushrooms), 0xC3EB35, 0x02, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.LowerMushrooms, Enemies.RoomEnemyGraphicsSetDefinitions.LowerMushrooms, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LowerMushrooms, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2PurpleRocks, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_CrateriaMap.</summary>
    public static CartridgeRoomState CrateriaMap { get; } =
        new(nameof(CrateriaMap), 0xCE86BD, 0x15, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.CrateriaMap, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrateriaMap, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_GreenPiratesShaft.</summary>
    public static CartridgeRoomState GreenPiratesShaft { get; } =
        new(nameof(GreenPiratesShaft), 0xC3EE60, 0x02, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.GreenPiratesShaft, Enemies.RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.GreenPiratesShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenPiratesShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_CrateriaSuper.</summary>
    public static CartridgeRoomState CrateriaSuper { get; } =
        new(nameof(CrateriaSuper), 0xC3F4D3, 0x02, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSuper, Enemies.RoomEnemyPopulationDefinitions.CrateriaSuper, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaSuper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrateriaSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrateriaSuper, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissileBombway_0.</summary>
    public static CartridgeRoomState FinalMissileBombway0 { get; } =
        new(nameof(FinalMissileBombway0), 0xC4811E, 0x02, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayState0, Enemies.RoomEnemyPopulationDefinitions.FinalMissileBombway0, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissileBombway0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FinalMissileBombwayState0, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissileBombway_1.</summary>
    public static CartridgeRoomState FinalMissileBombway1 { get; } =
        new(nameof(FinalMissileBombway1), 0xC4811E, 0x02, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayState1, Enemies.RoomEnemyPopulationDefinitions.FinalMissileBombway1, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissileBombway1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FinalMissileBombwayState1, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissile_0.</summary>
    public static CartridgeRoomState FinalMissile0 { get; } =
        new(nameof(FinalMissile0), 0xC48232, 0x02, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileState0, Enemies.RoomEnemyPopulationDefinitions.FinalMissile0, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissile0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FinalMissileState0, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissile_1.</summary>
    public static CartridgeRoomState FinalMissile1 { get; } =
        new(nameof(FinalMissile1), 0xC48232, 0x02, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileState1, Enemies.RoomEnemyPopulationDefinitions.FinalMissile1, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissile1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FinalMissileState1, Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_StatuesHallway.</summary>
    public static CartridgeRoomState StatuesHallway { get; } =
        new(nameof(StatuesHallway), 0xC6D88D, 0x08, 0x00, 0x04, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState1, Enemies.RoomEnemyPopulationDefinitions.StatuesHallway, Enemies.RoomEnemyGraphicsSetDefinitions.StatuesHallway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.StatuesHallway, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar8NarrowVerticalTowerBrickGrey0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Statues.</summary>
    public static CartridgeRoomState Statues { get; } =
        new(nameof(Statues), 0xCE9BE9, 0x15, 0x09, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.Statues, Enemies.RoomEnemyPopulationDefinitions.Statues, Enemies.RoomEnemyGraphicsSetDefinitions.Statues, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.Statues, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Statues, Backgrounds.LibraryBackgroundProgramDefinitions.Tourian15Statues, RoomSetupCallback.RunStatueUnlockingAnimations);
}
