namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The initialization settings of each of the 323 retail room states, named by the disassembly's RoomState_ labels.
/// Each field preserves its native byte/word width from LoadStateHeader ($82:DEF2).
/// </summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class RoomStateDefinitions
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

    /// <summary>The disassembly's RoomState_GreenBrinstarMainShaft.</summary>
    public static CartridgeRoomState GreenBrinstarMainShaft { get; } =
        new(nameof(GreenBrinstarMainShaft), 0xC4F1CE, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMainShaft, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarMainShaft, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMainShaft, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.GreenBrinstarMainShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarMainShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6VerticalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_SporeSpawnSuper.</summary>
    public static CartridgeRoomState SporeSpawnSuper { get; } =
        new(nameof(SporeSpawnSuper), 0xC58BD5, 0x06, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnSuper, Enemies.RoomEnemyPopulationDefinitions.SporeSpawnSuper, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnSuper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.SporeSpawnSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnSuper, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_BrinstarPreMap.</summary>
    public static CartridgeRoomState BrinstarPreMap { get; } =
        new(nameof(BrinstarPreMap), 0xC59642, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarPreMap, Enemies.RoomEnemyPopulationDefinitions.BrinstarPreMap, Enemies.RoomEnemyGraphicsSetDefinitions.BrinstarPreMap, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BrinstarPreMap, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6MediumHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_EarlySupers.</summary>
    public static CartridgeRoomState EarlySupers { get; } =
        new(nameof(EarlySupers), 0xC59755, 0x06, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.EarlySupers, Enemies.RoomEnemyPopulationDefinitions.EarlySupers, Enemies.RoomEnemyGraphicsSetDefinitions.EarlySupers, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.EarlySupers, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EarlySupers, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_BrinstarReserveTank.</summary>
    public static CartridgeRoomState BrinstarReserveTank { get; } =
        new(nameof(BrinstarReserveTank), 0xC59B00, 0x06, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarReserveTank, Enemies.RoomEnemyPopulationDefinitions.BrinstarReserveTank, Enemies.RoomEnemyGraphicsSetDefinitions.BrinstarReserveTank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BrinstarReserveTank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BrinstarReserveTank, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6LargeHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_GreenBrinstarMap.</summary>
    public static CartridgeRoomState GreenBrinstarMap { get; } =
        new(nameof(GreenBrinstarMap), 0xCE83C3, 0x15, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMap, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarMap, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarMap, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_GreenBrinstarFirefleas.</summary>
    public static CartridgeRoomState GreenBrinstarFirefleas { get; } =
        new(nameof(GreenBrinstarFirefleas), 0xC59CAC, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarFirefleas, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarFirefleas, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarFirefleas, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_GreenBrinstarMissileRefill.</summary>
    public static CartridgeRoomState GreenBrinstarMissileRefill { get; } =
        new(nameof(GreenBrinstarMissileRefill), 0xCE8CB3, 0x18, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMissileRefill, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarMissileRefill, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMissileRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenBrinstarMissileRefill, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarMissileRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_Dachora.</summary>
    public static CartridgeRoomState Dachora { get; } =
        new(nameof(Dachora), 0xC5A15F, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Dachora, Enemies.RoomEnemyPopulationDefinitions.Dachora, Enemies.RoomEnemyGraphicsSetDefinitions.Dachora, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Dachora, null, RoomMainCallback.ScrollScreenRightInDachoraRoom, Plms.RoomPlmPopulationDefinitions.Dachora, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_BigPink.</summary>
    public static CartridgeRoomState BigPink { get; } =
        new(nameof(BigPink), 0xC5B54D, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BigPink, Enemies.RoomEnemyPopulationDefinitions.BigPink, Enemies.RoomEnemyGraphicsSetDefinitions.BigPink, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BigPink, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BigPink, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6DarkPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_SporeSpawnKihunters.</summary>
    public static CartridgeRoomState SporeSpawnKihunters { get; } =
        new(nameof(SporeSpawnKihunters), 0xC5CBA7, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnKihunters, Enemies.RoomEnemyPopulationDefinitions.SporeSpawnKihunters, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnKihunters, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnKihunters, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_SporeSpawn_0.</summary>
    public static CartridgeRoomState SporeSpawn0 { get; } =
        new(nameof(SporeSpawn0), 0xC5CE34, 0x06, 0x2A, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnState01, Enemies.RoomEnemyPopulationDefinitions.SporeSpawn, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawn, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnState01, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_SporeSpawn_1.</summary>
    public static CartridgeRoomState SporeSpawn1 { get; } =
        new(nameof(SporeSpawn1), 0xC5CE34, 0x06, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnState01, Enemies.RoomEnemyPopulationDefinitions.SporeSpawn, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawn, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnState01, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_PinkBrinstarPowerBombs.</summary>
    public static CartridgeRoomState PinkBrinstarPowerBombs { get; } =
        new(nameof(PinkBrinstarPowerBombs), 0xC5D18F, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarPowerBombs, Enemies.RoomEnemyPopulationDefinitions.PinkBrinstarPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.PinkBrinstarPowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.PinkBrinstarPowerBombs, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PinkBrinstarPowerBombs, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_GreenHillZone.</summary>
    public static CartridgeRoomState GreenHillZone { get; } =
        new(nameof(GreenHillZone), 0xC5D559, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenHillZone, Enemies.RoomEnemyPopulationDefinitions.GreenHillZone, Enemies.RoomEnemyGraphicsSetDefinitions.GreenHillZone, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenHillZone, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenHillZone, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SpheresPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_MorphBall_0.</summary>
    public static CartridgeRoomState MorphBall0 { get; } =
        new(nameof(MorphBall0), 0xC5DED5, 0x06, 0x06, 0x07, SuperMetroid.Core.Fx.RoomFxDefinitions.MorphBallState0, Enemies.RoomEnemyPopulationDefinitions.MorphBall0, Enemies.RoomEnemyGraphicsSetDefinitions.MorphBall0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MorphBallState0, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_MorphBall_1.</summary>
    public static CartridgeRoomState MorphBall1 { get; } =
        new(nameof(MorphBall1), 0xC5DED5, 0x06, 0x09, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381F4, Enemies.RoomEnemyPopulationDefinitions.MorphBall1, Enemies.RoomEnemyGraphicsSetDefinitions.MorphBall1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MorphBallState1, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);

    /// <summary>The disassembly's RoomState_ConstructionZone_0.</summary>
    public static CartridgeRoomState ConstructionZone0 { get; } =
        new(nameof(ConstructionZone0), 0xC5E63A, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ConstructionZoneState0, Enemies.RoomEnemyPopulationDefinitions.ConstructionZone0, Enemies.RoomEnemyGraphicsSetDefinitions.ConstructionZone0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ConstructionZoneState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_ConstructionZone_1.</summary>
    public static CartridgeRoomState ConstructionZone1 { get; } =
        new(nameof(ConstructionZone1), 0xC5E63A, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381F4, Enemies.RoomEnemyPopulationDefinitions.ConstructionZone1, Enemies.RoomEnemyGraphicsSetDefinitions.ConstructionZone1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ConstructionZoneState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeEscapeSkySetup);

    /// <summary>The disassembly's RoomState_BlueBrinstarETank_0.</summary>
    public static CartridgeRoomState BlueBrinstarETank0 { get; } =
        new(nameof(BlueBrinstarETank0), 0xC5E86F, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarETankState0, Enemies.RoomEnemyPopulationDefinitions.BlueBrinstarETank0, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueBrinstarETankState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_BlueBrinstarETank_1.</summary>
    public static CartridgeRoomState BlueBrinstarETank1 { get; } =
        new(nameof(BlueBrinstarETank1), 0xC5E86F, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381F4, Enemies.RoomEnemyPopulationDefinitions.BlueBrinstarETank1, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueBrinstarETankState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);

    /// <summary>The disassembly's RoomState_NoobBridge.</summary>
    public static CartridgeRoomState NoobBridge { get; } =
        new(nameof(NoobBridge), 0xC5ECAE, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.NoobBridge, Enemies.RoomEnemyPopulationDefinitions.NoobBridge, Enemies.RoomEnemyGraphicsSetDefinitions.NoobBridge, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.NoobBridge, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_GreenBrinstarBeetoms.</summary>
    public static CartridgeRoomState GreenBrinstarBeetoms { get; } =
        new(nameof(GreenBrinstarBeetoms), 0xC5EF71, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarBeetoms, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarBeetoms, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarBeetoms, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenBrinstarBeetoms, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarBeetoms, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_EtecoonETank.</summary>
    public static CartridgeRoomState EtecoonETank { get; } =
        new(nameof(EtecoonETank), 0xC5F057, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonETank, Enemies.RoomEnemyPopulationDefinitions.EtecoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonETank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.EtecoonETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EtecoonETank, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_EtecoonSuper.</summary>
    public static CartridgeRoomState EtecoonSuper { get; } =
        new(nameof(EtecoonSuper), 0xC5F43E, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSuper, Enemies.RoomEnemyPopulationDefinitions.EtecoonSuper, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonSuper, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.EtecoonSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EtecoonSuper, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_DachoraEnergyRefill.</summary>
    public static CartridgeRoomState DachoraEnergyRefill { get; } =
        new(nameof(DachoraEnergyRefill), 0xCE89B6, 0x17, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.DachoraEnergyRefill, Enemies.RoomEnemyPopulationDefinitions.DachoraEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.DachoraEnergyRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DachoraEnergyRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_SporeSpawnFarming.</summary>
    public static CartridgeRoomState SporeSpawnFarming { get; } =
        new(nameof(SporeSpawnFarming), 0xC5F4C9, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnFarming, Enemies.RoomEnemyPopulationDefinitions.SporeSpawnFarming, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnFarming, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.SporeSpawnFarming, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnFarming, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_WaterwayETank.</summary>
    public static CartridgeRoomState WaterwayETank { get; } =
        new(nameof(WaterwayETank), 0xC5F778, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WaterwayETank, Enemies.RoomEnemyPopulationDefinitions.WaterwayETank, Enemies.RoomEnemyGraphicsSetDefinitions.WaterwayETank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.WaterwayETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WaterwayETank, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_FirstMissile.</summary>
    public static CartridgeRoomState FirstMissile { get; } =
        new(nameof(FirstMissile), 0xC5FD50, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FirstMissile, Enemies.RoomEnemyPopulationDefinitions.FirstMissile, Enemies.RoomEnemyGraphicsSetDefinitions.FirstMissile, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FirstMissile, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_PinkBrinstarHoppers.</summary>
    public static CartridgeRoomState PinkBrinstarHoppers { get; } =
        new(nameof(PinkBrinstarHoppers), 0xC5FE1B, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarHoppers, Enemies.RoomEnemyPopulationDefinitions.PinkBrinstarHoppers, Enemies.RoomEnemyGraphicsSetDefinitions.PinkBrinstarHoppers, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PinkBrinstarHoppers, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_HopperETank.</summary>
    public static CartridgeRoomState HopperETank { get; } =
        new(nameof(HopperETank), 0xC681C2, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.HopperETank, Enemies.RoomEnemyPopulationDefinitions.HopperETank, Enemies.RoomEnemyGraphicsSetDefinitions.HopperETank, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.HopperETank, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_BigPinkSaveRoom.</summary>
    public static CartridgeRoomState BigPinkSaveRoom { get; } =
        new(nameof(BigPinkSaveRoom), 0xCE92CB, 0x19, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BigPinkSaveRoom, Enemies.RoomEnemyPopulationDefinitions.BigPinkSaveRoom, Enemies.RoomEnemyGraphicsSetDefinitions.BigPinkSaveRoom, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BigPinkSaveRoom, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_BlueBrinstarBoulders.</summary>
    public static CartridgeRoomState BlueBrinstarBoulders { get; } =
        new(nameof(BlueBrinstarBoulders), 0xC68318, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarBoulders, Enemies.RoomEnemyPopulationDefinitions.BlueBrinstarBoulders, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarBoulders, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueBrinstarBoulders, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_BillyMays.</summary>
    public static CartridgeRoomState BillyMays { get; } =
        new(nameof(BillyMays), 0xC68437, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BillyMays, Enemies.RoomEnemyPopulationDefinitions.BillyMays, Enemies.RoomEnemyGraphicsSetDefinitions.BillyMays, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BillyMays, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_GreenBrinstarSave.</summary>
    public static CartridgeRoomState GreenBrinstarSave { get; } =
        new(nameof(GreenBrinstarSave), 0xCE92CB, 0x19, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarSave, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarSave, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_EtecoonSave.</summary>
    public static CartridgeRoomState EtecoonSave { get; } =
        new(nameof(EtecoonSave), 0xCE92CB, 0x19, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSave, Enemies.RoomEnemyPopulationDefinitions.EtecoonSave, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EtecoonSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);

    /// <summary>The disassembly's RoomState_RedTower.</summary>
    public static CartridgeRoomState RedTower { get; } =
        new(nameof(RedTower), 0xC684EE, 0x07, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.RedTower, Enemies.RoomEnemyPopulationDefinitions.RedTower, Enemies.RoomEnemyGraphicsSetDefinitions.RedTower, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RedTower, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedTower, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7WideVerticalTowerBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_RedBrinstarFirefleas.</summary>
    public static CartridgeRoomState RedBrinstarFirefleas { get; } =
        new(nameof(RedBrinstarFirefleas), 0xC691E3, 0x07, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarFirefleas, Enemies.RoomEnemyPopulationDefinitions.RedBrinstarFirefleas, Enemies.RoomEnemyGraphicsSetDefinitions.RedBrinstarFirefleas, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.RedBrinstarFirefleas, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_XrayScope.</summary>
    public static CartridgeRoomState XrayScope { get; } =
        new(nameof(XrayScope), 0xC69BF9, 0x07, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.XrayScope, Enemies.RoomEnemyPopulationDefinitions.XrayScope, Enemies.RoomEnemyGraphicsSetDefinitions.XrayScope, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.XrayScope, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7MechanicalRoomSpikeFloor, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_Hellway.</summary>
    public static CartridgeRoomState Hellway { get; } =
        new(nameof(Hellway), 0xC69D70, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Hellway, Enemies.RoomEnemyPopulationDefinitions.Hellway, Enemies.RoomEnemyGraphicsSetDefinitions.Hellway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Hellway, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7VerticalTower, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_Caterpillar.</summary>
    public static CartridgeRoomState Caterpillar { get; } =
        new(nameof(Caterpillar), 0xC69F4B, 0x07, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Caterpillar, Enemies.RoomEnemyPopulationDefinitions.Caterpillar, Enemies.RoomEnemyGraphicsSetDefinitions.Caterpillar, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Caterpillar, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Caterpillar, null, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_BetaPowerBomb.</summary>
    public static CartridgeRoomState BetaPowerBomb { get; } =
        new(nameof(BetaPowerBomb), 0xC6B31F, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BetaPowerBomb, Enemies.RoomEnemyPopulationDefinitions.BetaPowerBomb, Enemies.RoomEnemyGraphicsSetDefinitions.BetaPowerBomb, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BetaPowerBomb, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BetaPowerBomb, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_AlphaPowerBomb.</summary>
    public static CartridgeRoomState AlphaPowerBomb { get; } =
        new(nameof(AlphaPowerBomb), 0xC6B58C, 0x07, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.AlphaPowerBomb, Enemies.RoomEnemyPopulationDefinitions.AlphaPowerBomb, Enemies.RoomEnemyGraphicsSetDefinitions.AlphaPowerBomb, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AlphaPowerBomb, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AlphaPowerBomb, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_SkreeBoost.</summary>
    public static CartridgeRoomState SkreeBoost { get; } =
        new(nameof(SkreeBoost), 0xC6B739, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SkreeBoost, Enemies.RoomEnemyPopulationDefinitions.SkreeBoost, Enemies.RoomEnemyGraphicsSetDefinitions.SkreeBoost, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SkreeBoost, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_BelowSpazer.</summary>
    public static CartridgeRoomState BelowSpazer { get; } =
        new(nameof(BelowSpazer), 0xC6B91C, 0x07, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BelowSpazer, Enemies.RoomEnemyPopulationDefinitions.BelowSpazer, Enemies.RoomEnemyGraphicsSetDefinitions.BelowSpazer, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BelowSpazer, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BelowSpazer, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7VerticalBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_Spazer.</summary>
    public static CartridgeRoomState Spazer { get; } =
        new(nameof(Spazer), 0xC6BCC7, 0x07, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.Spazer, Enemies.RoomEnemyPopulationDefinitions.Spazer, Enemies.RoomEnemyGraphicsSetDefinitions.Spazer, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Spazer, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Spazer, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_WarehouseZeela.</summary>
    public static CartridgeRoomState WarehouseZeela { get; } =
        new(nameof(WarehouseZeela), 0xC6BD83, 0x07, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8383D2, Enemies.RoomEnemyPopulationDefinitions.WarehouseZeela, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseZeela, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WarehouseZeela, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseZeela, null, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_WarehouseETank.</summary>
    public static CartridgeRoomState WarehouseETank { get; } =
        new(nameof(WarehouseETank), 0xC6C469, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8383D2, Enemies.RoomEnemyPopulationDefinitions.WarehouseETank, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseETank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseETank, null, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_WarehouseKihunter.</summary>
    public static CartridgeRoomState WarehouseKihunter { get; } =
        new(nameof(WarehouseKihunter), 0xC6C630, 0x07, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8383D2, Enemies.RoomEnemyPopulationDefinitions.WarehouseKihunter, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseKihunter, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WarehouseKihunter, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseKihunter, null, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_MiniKraid_0.</summary>
    public static CartridgeRoomState MiniKraid0 { get; } =
        new(nameof(MiniKraid0), 0xC6CDB9, 0x07, 0x27, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraidState01, Enemies.RoomEnemyPopulationDefinitions.MiniKraid, Enemies.RoomEnemyGraphicsSetDefinitions.MiniKraid, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MiniKraidState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_MiniKraid_1.</summary>
    public static CartridgeRoomState MiniKraid1 { get; } =
        new(nameof(MiniKraid1), 0xC6CDB9, 0x07, 0x27, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraidState01, Enemies.RoomEnemyPopulationDefinitions.MiniKraid, Enemies.RoomEnemyGraphicsSetDefinitions.MiniKraid, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MiniKraidState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_KraidEyeDoor.</summary>
    public static CartridgeRoomState KraidEyeDoor { get; } =
        new(nameof(KraidEyeDoor), 0xC6D2CB, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidEyeDoor, Enemies.RoomEnemyPopulationDefinitions.KraidEyeDoor, Enemies.RoomEnemyGraphicsSetDefinitions.KraidEyeDoor, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.KraidEyeDoor, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KraidEyeDoor, null, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_Kraid_0.</summary>
    public static CartridgeRoomState Kraid0 { get; } =
        new(nameof(Kraid0), 0xC6D620, 0x1A, 0x27, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState0, Enemies.RoomEnemyPopulationDefinitions.Kraid, Enemies.RoomEnemyGraphicsSetDefinitions.Kraid, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KraidState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar1AKraidUpperLower, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_Kraid_1.</summary>
    public static CartridgeRoomState Kraid1 { get; } =
        new(nameof(Kraid1), 0xC6D620, 0x1A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState0, Enemies.RoomEnemyPopulationDefinitions.Kraid, Enemies.RoomEnemyGraphicsSetDefinitions.Kraid, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KraidState01, Backgrounds.LibraryBackgroundProgramDefinitions.KraidDead, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_StatuesHallway.</summary>
    public static CartridgeRoomState StatuesHallway { get; } =
        new(nameof(StatuesHallway), 0xC6D88D, 0x08, 0x00, 0x04, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState1, Enemies.RoomEnemyPopulationDefinitions.StatuesHallway, Enemies.RoomEnemyGraphicsSetDefinitions.StatuesHallway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.StatuesHallway, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar8NarrowVerticalTowerBrickGrey0, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_RedTowerRefill.</summary>
    public static CartridgeRoomState RedTowerRefill { get; } =
        new(nameof(RedTowerRefill), 0xCE89B6, 0x17, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RedTowerRefill, Enemies.RoomEnemyPopulationDefinitions.RedTowerRefill, Enemies.RoomEnemyGraphicsSetDefinitions.RedTowerRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedTowerRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_KraidRefill.</summary>
    public static CartridgeRoomState KraidRefill { get; } =
        new(nameof(KraidRefill), 0xCEA201, 0x18, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidRefill, Enemies.RoomEnemyPopulationDefinitions.KraidRefill, Enemies.RoomEnemyGraphicsSetDefinitions.KraidRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KraidRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);

    /// <summary>The disassembly's RoomState_Statues.</summary>
    public static CartridgeRoomState Statues { get; } =
        new(nameof(Statues), 0xCE9BE9, 0x15, 0x09, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.Statues, Enemies.RoomEnemyPopulationDefinitions.Statues, Enemies.RoomEnemyGraphicsSetDefinitions.Statues, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.Statues, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Statues, Backgrounds.LibraryBackgroundProgramDefinitions.Tourian15Statues, RoomSetupCallback.RunStatueUnlockingAnimations);

    /// <summary>The disassembly's RoomState_WarehouseEntrance.</summary>
    public static CartridgeRoomState WarehouseEntrance { get; } =
        new(nameof(WarehouseEntrance), 0xC6DEE0, 0x07, 0x12, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseEntrance, Enemies.RoomEnemyPopulationDefinitions.WarehouseEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseEntrance, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WarehouseEntrance, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseEntrance, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7MechanicalRoom, RoomSetupCallback.SharedReturn);

    /// <summary>The disassembly's RoomState_VariaSuit.</summary>
    public static CartridgeRoomState VariaSuit { get; } =
        new(nameof(VariaSuit), 0xC6E355, 0x07, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.VariaSuit, Enemies.RoomEnemyPopulationDefinitions.VariaSuit, Enemies.RoomEnemyGraphicsSetDefinitions.VariaSuit, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.VariaSuit, null, RoomSetupCallback.SharedReturn);

    /// <summary>The disassembly's RoomState_WarehouseSave.</summary>
    public static CartridgeRoomState WarehouseSave { get; } =
        new(nameof(WarehouseSave), 0xCE95C2, 0x19, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseSave, Enemies.RoomEnemyPopulationDefinitions.WarehouseSave, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseSave, null, RoomSetupCallback.SharedReturn);

    /// <summary>The disassembly's RoomState_RedBrinstarSave.</summary>
    public static CartridgeRoomState RedBrinstarSave { get; } =
        new(nameof(RedBrinstarSave), 0xCE95C2, 0x19, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarSave, Enemies.RoomEnemyPopulationDefinitions.RedBrinstarSave, Enemies.RoomEnemyGraphicsSetDefinitions.RedBrinstarSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedBrinstarSave, null, RoomSetupCallback.SharedReturn);

    /// <summary>The disassembly's RoomState_IceBeamAcid.</summary>
    public static CartridgeRoomState IceBeamAcid { get; } =
        new(nameof(IceBeamAcid), 0xC6E4A4, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamAcid, Enemies.RoomEnemyPopulationDefinitions.IceBeamAcid, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamAcid, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeamAcid, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_Cathedral.</summary>
    public static CartridgeRoomState Cathedral { get; } =
        new(nameof(Cathedral), 0xC6E5F5, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Cathedral, Enemies.RoomEnemyPopulationDefinitions.Cathedral, Enemies.RoomEnemyGraphicsSetDefinitions.Cathedral, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Cathedral, null, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_CathedralEntrance.</summary>
    public static CartridgeRoomState CathedralEntrance { get; } =
        new(nameof(CathedralEntrance), 0xC6ECB9, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CathedralEntrance, Enemies.RoomEnemyPopulationDefinitions.CathedralEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.CathedralEntrance, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CathedralEntrance, null, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_BusinessCenter.</summary>
    public static CartridgeRoomState BusinessCenter { get; } =
        new(nameof(BusinessCenter), 0xC6F2E1, 0x0A, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BusinessCenter, Enemies.RoomEnemyPopulationDefinitions.BusinessCenter, Enemies.RoomEnemyGraphicsSetDefinitions.BusinessCenter, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BusinessCenter, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_IceBeamGate.</summary>
    public static CartridgeRoomState IceBeamGate { get; } =
        new(nameof(IceBeamGate), 0xC6F8C1, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamGate, Enemies.RoomEnemyPopulationDefinitions.IceBeamGate, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamGate, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.IceBeamGate, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeamGate, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_IceBeamTutorial.</summary>
    public static CartridgeRoomState IceBeamTutorial { get; } =
        new(nameof(IceBeamTutorial), 0xC782ED, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamTutorial, Enemies.RoomEnemyPopulationDefinitions.IceBeamTutorial, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamTutorial, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeamTutorial, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_IceBeam.</summary>
    public static CartridgeRoomState IceBeam { get; } =
        new(nameof(IceBeam), 0xC784A3, 0x0A, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeam, Enemies.RoomEnemyPopulationDefinitions.IceBeam, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeam, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeam, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_IceBeamSnake.</summary>
    public static CartridgeRoomState IceBeamSnake { get; } =
        new(nameof(IceBeamSnake), 0xC785D6, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamSnake, Enemies.RoomEnemyPopulationDefinitions.IceBeamSnake, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamSnake, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.IceBeamSnake, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeamSnake, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_CrumbleShaft.</summary>
    public static CartridgeRoomState CrumbleShaft { get; } =
        new(nameof(CrumbleShaft), 0xC78A47, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrumbleShaft, Enemies.RoomEnemyPopulationDefinitions.CrumbleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.CrumbleShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrumbleShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_CrocomireSpeedway.</summary>
    public static CartridgeRoomState CrocomireSpeedway { get; } =
        new(nameof(CrocomireSpeedway), 0xC78CFA, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSpeedway, Enemies.RoomEnemyPopulationDefinitions.CrocomireSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrocomireSpeedway, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrocomireSpeedway, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);

    /// <summary>The disassembly's RoomState_Crocomire_0.</summary>
    public static CartridgeRoomState Crocomire0 { get; } =
        new(nameof(Crocomire0), 0xC79D71, 0x1B, 0x27, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireState01, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainCallback.CrocomireRoomShaking, Plms.RoomPlmPopulationDefinitions.CrocomireState01, Backgrounds.LibraryBackgroundProgramDefinitions.CrocomireState0, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_Crocomire_1.</summary>
    public static CartridgeRoomState Crocomire1 { get; } =
        new(nameof(Crocomire1), 0xC79D71, 0x1B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireState01, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainCallback.CrocomireRoomShaking, Plms.RoomPlmPopulationDefinitions.CrocomireState01, Backgrounds.LibraryBackgroundProgramDefinitions.CrocomireState1, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_HiJumpBoots.</summary>
    public static CartridgeRoomState HiJumpBoots { get; } =
        new(nameof(HiJumpBoots), 0xC7A036, 0x0A, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpBoots, Enemies.RoomEnemyPopulationDefinitions.HiJumpBoots, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpBoots, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.HiJumpBoots, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_CrocomireEscape.</summary>
    public static CartridgeRoomState CrocomireEscape { get; } =
        new(nameof(CrocomireEscape), 0xC7A18D, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireEscape, Enemies.RoomEnemyPopulationDefinitions.CrocomireEscape, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireEscape, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.CrocomireEscape, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrocomireEscape, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_HiJumpETank.</summary>
    public static CartridgeRoomState HiJumpETank { get; } =
        new(nameof(HiJumpETank), 0xC7AA70, 0x0A, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpETank, Enemies.RoomEnemyPopulationDefinitions.HiJumpETank, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpETank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.HiJumpETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.HiJumpETank, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_PostCrocFarming.</summary>
    public static CartridgeRoomState PostCrocFarming { get; } =
        new(nameof(PostCrocFarming), 0xC7AEB3, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocFarming, Enemies.RoomEnemyPopulationDefinitions.PostCrocFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocFarming, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.PostCrocFarming, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocFarming, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_PostCrocSave.</summary>
    public static CartridgeRoomState PostCrocSave { get; } =
        new(nameof(PostCrocSave), 0xCE95C2, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocSave, Enemies.RoomEnemyPopulationDefinitions.PostCrocSave, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocSave, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_PostCrocPowerBombs.</summary>
    public static CartridgeRoomState PostCrocPowerBombs { get; } =
        new(nameof(PostCrocPowerBombs), 0xC7B28B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocPowerBombs, Enemies.RoomEnemyPopulationDefinitions.PostCrocPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocPowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocPowerBombs, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_PostCrocShaft.</summary>
    public static CartridgeRoomState PostCrocShaft { get; } =
        new(nameof(PostCrocShaft), 0xC7B3E7, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocShaft, Enemies.RoomEnemyPopulationDefinitions.PostCrocShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.PostCrocShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_PostCrocMissile.</summary>
    public static CartridgeRoomState PostCrocMissile { get; } =
        new(nameof(PostCrocMissile), 0xC7B780, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocMissile, Enemies.RoomEnemyPopulationDefinitions.PostCrocMissile, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocMissile, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocMissile, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_GrappleTutorial3.</summary>
    public static CartridgeRoomState GrappleTutorial3 { get; } =
        new(nameof(GrappleTutorial3), 0xC7BB6B, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial3, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial3, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial3, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GrappleTutorial3, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_PostCrocJump.</summary>
    public static CartridgeRoomState PostCrocJump { get; } =
        new(nameof(PostCrocJump), 0xC7BECB, 0x0A, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocJump, Enemies.RoomEnemyPopulationDefinitions.PostCrocJump, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocJump, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.PostCrocJump, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocJump, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_GrappleTutorial2.</summary>
    public static CartridgeRoomState GrappleTutorial2 { get; } =
        new(nameof(GrappleTutorial2), 0xC7CD91, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial2, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial2, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GrappleTutorial2, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GrappleTutorial2, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_GrappleTutorial1.</summary>
    public static CartridgeRoomState GrappleTutorial1 { get; } =
        new(nameof(GrappleTutorial1), 0xC7CFCD, 0x0A, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial1, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial1, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GrappleTutorial1, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_GrappleBeam.</summary>
    public static CartridgeRoomState GrappleBeam { get; } =
        new(nameof(GrappleBeam), 0xC7D13C, 0x0A, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleBeam, Enemies.RoomEnemyPopulationDefinitions.GrappleBeam, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleBeam, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GrappleBeam, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GrappleBeam, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_NorfairReserveTank.</summary>
    public static CartridgeRoomState NorfairReserveTank { get; } =
        new(nameof(NorfairReserveTank), 0xC7D4FE, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairReserveTank, Enemies.RoomEnemyPopulationDefinitions.NorfairReserveTank, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairReserveTank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.NorfairReserveTank, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_GreenBubblesMissiles.</summary>
    public static CartridgeRoomState GreenBubblesMissiles { get; } =
        new(nameof(GreenBubblesMissiles), 0xC7D66F, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBubblesMissiles, Enemies.RoomEnemyPopulationDefinitions.GreenBubblesMissiles, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBubblesMissiles, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenBubblesMissiles, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBubblesMissiles, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9Bubbles, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_BubbleMountain.</summary>
    public static CartridgeRoomState BubbleMountain { get; } =
        new(nameof(BubbleMountain), 0xC7D895, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountain, Enemies.RoomEnemyPopulationDefinitions.BubbleMountain, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountain, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BubbleMountain, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BubbleMountain, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9Bubbles, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_SpeedBoosterHall.</summary>
    public static CartridgeRoomState SpeedBoosterHall { get; } =
        new(nameof(SpeedBoosterHall), 0xC7E08C, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBoosterHall, Enemies.RoomEnemyPopulationDefinitions.SpeedBoosterHall, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpeedBoosterHall, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_SpeedBooster.</summary>
    public static CartridgeRoomState SpeedBooster { get; } =
        new(nameof(SpeedBooster), 0xC7EAA8, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBooster, Enemies.RoomEnemyPopulationDefinitions.SpeedBooster, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBooster, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpeedBooster, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_SingleChamber.</summary>
    public static CartridgeRoomState SingleChamber { get; } =
        new(nameof(SingleChamber), 0xC7EC03, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SingleChamber, Enemies.RoomEnemyPopulationDefinitions.SingleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.SingleChamber, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), Scrolling.RoomScrollDefinitions.SingleChamber, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SingleChamber, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_DoubleChamber.</summary>
    public static CartridgeRoomState DoubleChamber { get; } =
        new(nameof(DoubleChamber), 0xC7FF02, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.DoubleChamber, Enemies.RoomEnemyPopulationDefinitions.DoubleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.DoubleChamber, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.DoubleChamber, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DoubleChamber, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_WaveBeam.</summary>
    public static CartridgeRoomState WaveBeam { get; } =
        new(nameof(WaveBeam), 0xC88532, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.WaveBeam, Enemies.RoomEnemyPopulationDefinitions.WaveBeam, Enemies.RoomEnemyGraphicsSetDefinitions.WaveBeam, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WaveBeam, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_SpikyPlatformsTunnel.</summary>
    public static CartridgeRoomState SpikyPlatformsTunnel { get; } =
        new(nameof(SpikyPlatformsTunnel), 0xC8865C, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyPlatformsTunnel, Enemies.RoomEnemyPopulationDefinitions.SpikyPlatformsTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpikyPlatformsTunnel, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_Volcano.</summary>
    public static CartridgeRoomState Volcano { get; } =
        new(nameof(Volcano), 0xC88953, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Volcano, Enemies.RoomEnemyPopulationDefinitions.Volcano, Enemies.RoomEnemyGraphicsSetDefinitions.Volcano, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Volcano, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Volcano, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_KronicBoost.</summary>
    public static CartridgeRoomState KronicBoost { get; } =
        new(nameof(KronicBoost), 0xC8943A, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.KronicBoost, Enemies.RoomEnemyPopulationDefinitions.KronicBoost, Enemies.RoomEnemyGraphicsSetDefinitions.KronicBoost, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.KronicBoost, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KronicBoost, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_MagdolliteTunnel.</summary>
    public static CartridgeRoomState MagdolliteTunnel { get; } =
        new(nameof(MagdolliteTunnel), 0xC89D5D, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MagdolliteTunnel, Enemies.RoomEnemyPopulationDefinitions.MagdolliteTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MagdolliteTunnel, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_PurpleShaft.</summary>
    public static CartridgeRoomState PurpleShaft { get; } =
        new(nameof(PurpleShaft), 0xC89FE3, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleShaft, Enemies.RoomEnemyPopulationDefinitions.PurpleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.PurpleShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PurpleShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_LavaDive.</summary>
    public static CartridgeRoomState LavaDive { get; } =
        new(nameof(LavaDive), 0xC8A2CF, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LavaDive, Enemies.RoomEnemyPopulationDefinitions.LavaDive, Enemies.RoomEnemyGraphicsSetDefinitions.LavaDive, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LavaDive, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_LowerNorfairElev.</summary>
    public static CartridgeRoomState LowerNorfairElev { get; } =
        new(nameof(LowerNorfairElev), 0xC8AA89, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LowerNorfairElev, Enemies.RoomEnemyPopulationDefinitions.LowerNorfairElev, Enemies.RoomEnemyGraphicsSetDefinitions.LowerNorfairElev, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.LowerNorfairElev, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LowerNorfairElev, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_UpperNorfairFarming.</summary>
    public static CartridgeRoomState UpperNorfairFarming { get; } =
        new(nameof(UpperNorfairFarming), 0xC8ABED, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairFarming, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairFarming, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.UpperNorfairFarming, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.UpperNorfairFarming, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_RisingTide.</summary>
    public static CartridgeRoomState RisingTide { get; } =
        new(nameof(RisingTide), 0xC8B15B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RisingTide, Enemies.RoomEnemyPopulationDefinitions.RisingTide, Enemies.RoomEnemyGraphicsSetDefinitions.RisingTide, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RisingTide, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_AcidSnakesTunnel.</summary>
    public static CartridgeRoomState AcidSnakesTunnel { get; } =
        new(nameof(AcidSnakesTunnel), 0xC8B4F9, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.AcidSnakesTunnel, Enemies.RoomEnemyPopulationDefinitions.AcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AcidSnakesTunnel, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_SpikyAcidSnakesTunnel.</summary>
    public static CartridgeRoomState SpikyAcidSnakesTunnel { get; } =
        new(nameof(SpikyAcidSnakesTunnel), 0xC8B853, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyAcidSnakesTunnel, Enemies.RoomEnemyPopulationDefinitions.SpikyAcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpikyAcidSnakesTunnel, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_UpperNorfairRefill.</summary>
    public static CartridgeRoomState UpperNorfairRefill { get; } =
        new(nameof(UpperNorfairRefill), 0xCE8FA6, 0x17, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairRefill, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairRefill, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.UpperNorfairRefill, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_PurpleFarming.</summary>
    public static CartridgeRoomState PurpleFarming { get; } =
        new(nameof(PurpleFarming), 0xC8B9CF, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleFarming, Enemies.RoomEnemyPopulationDefinitions.PurpleFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleFarming, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PurpleFarming, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_BatCave.</summary>
    public static CartridgeRoomState BatCave { get; } =
        new(nameof(BatCave), 0xC8BB21, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BatCave, Enemies.RoomEnemyPopulationDefinitions.BatCave, Enemies.RoomEnemyGraphicsSetDefinitions.BatCave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BatCave, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BatCave, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_NorfairMap.</summary>
    public static CartridgeRoomState NorfairMap { get; } =
        new(nameof(NorfairMap), 0xCE83C3, 0x15, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairMap, Enemies.RoomEnemyPopulationDefinitions.NorfairMap, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.NorfairMap, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_BubbleMountainSave.</summary>
    public static CartridgeRoomState BubbleMountainSave { get; } =
        new(nameof(BubbleMountainSave), 0xCE92CB, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountainSave, Enemies.RoomEnemyPopulationDefinitions.BubbleMountainSave, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountainSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BubbleMountainSave, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_FrogSpeedway.</summary>
    public static CartridgeRoomState FrogSpeedway { get; } =
        new(nameof(FrogSpeedway), 0xC8BDE8, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSpeedway, Enemies.RoomEnemyPopulationDefinitions.FrogSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSpeedway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.FrogSpeedway, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FrogSpeedway, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_RedPirateShaft.</summary>
    public static CartridgeRoomState RedPirateShaft { get; } =
        new(nameof(RedPirateShaft), 0xC8C165, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RedPirateShaft, Enemies.RoomEnemyPopulationDefinitions.RedPirateShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedPirateShaft, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.RedPirateShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedPirateShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_FrogSave.</summary>
    public static CartridgeRoomState FrogSave { get; } =
        new(nameof(FrogSave), 0xCE9EF6, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSave, Enemies.RoomEnemyPopulationDefinitions.FrogSave, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FrogSave, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_CrocomireSave.</summary>
    public static CartridgeRoomState CrocomireSave { get; } =
        new(nameof(CrocomireSave), 0xCE95C2, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSave, Enemies.RoomEnemyPopulationDefinitions.CrocomireSave, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrocomireSave, null, RoomSetupCallback.SharedReturnC);

    /// <summary>The disassembly's RoomState_LNElevSave.</summary>
    public static CartridgeRoomState LNElevSave { get; } =
        new(nameof(LNElevSave), 0xCE92CB, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNElevSave, Enemies.RoomEnemyPopulationDefinitions.LNElevSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNElevSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.LNElevSave, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNElevSave, null, RoomSetupCallback.None);

    /// <summary>The disassembly's RoomState_AcidStatue.</summary>
    public static CartridgeRoomState AcidStatue { get; } =
        new(nameof(AcidStatue), 0xC8C4D3, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.AcidStatue, Enemies.RoomEnemyPopulationDefinitions.AcidStatue, Enemies.RoomEnemyGraphicsSetDefinitions.AcidStatue, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AcidStatue, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AcidStatue, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_MainHall.</summary>
    public static CartridgeRoomState MainHall { get; } =
        new(nameof(MainHall), 0xC8D59C, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.MainHall, Enemies.RoomEnemyPopulationDefinitions.MainHall, Enemies.RoomEnemyGraphicsSetDefinitions.MainHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.MainHall, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MainHall, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_GoldenTorizo_0.</summary>
    public static CartridgeRoomState GoldenTorizo0 { get; } =
        new(nameof(GoldenTorizo0), 0xC8E09D, 0x09, 0x24, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizoState01, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GoldenTorizoState01, null, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_GoldenTorizo_1.</summary>
    public static CartridgeRoomState GoldenTorizo1 { get; } =
        new(nameof(GoldenTorizo1), 0xC8E09D, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizoState01, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GoldenTorizoState01, null, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_FastRipper.</summary>
    public static CartridgeRoomState FastRipper { get; } =
        new(nameof(FastRipper), 0xC8E900, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.FastRipper, Enemies.RoomEnemyPopulationDefinitions.FastRipper, Enemies.RoomEnemyGraphicsSetDefinitions.FastRipper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FastRipper, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_GTEnergyRefill.</summary>
    public static CartridgeRoomState GTEnergyRefill { get; } =
        new(nameof(GTEnergyRefill), 0xCE98DC, 0x17, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GTEnergyRefill, Enemies.RoomEnemyPopulationDefinitions.GTEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.GTEnergyRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GTEnergyRefill, null, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_Ridley_0.</summary>
    public static CartridgeRoomState Ridley0 { get; } =
        new(nameof(Ridley0), 0xC8EBFD, 0x09, 0x24, 0x04, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyState01, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainCallback.RidleyRoomShaking, Plms.RoomPlmPopulationDefinitions.RidleyState01, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_Ridley_1.</summary>
    public static CartridgeRoomState Ridley1 { get; } =
        new(nameof(Ridley1), 0xC8EBFD, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyState01, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainCallback.RidleyRoomShaking, Plms.RoomPlmPopulationDefinitions.RidleyState01, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_LNFarming.</summary>
    public static CartridgeRoomState LNFarming { get; } =
        new(nameof(LNFarming), 0xC8EDCE, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNFarming, Enemies.RoomEnemyPopulationDefinitions.LNFarming, Enemies.RoomEnemyGraphicsSetDefinitions.LNFarming, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNFarming, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_FastPillarsSetup.</summary>
    public static CartridgeRoomState FastPillarsSetup { get; } =
        new(nameof(FastPillarsSetup), 0xC8F01F, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FastPillarsSetup, Enemies.RoomEnemyPopulationDefinitions.FastPillarsSetup, Enemies.RoomEnemyGraphicsSetDefinitions.FastPillarsSetup, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FastPillarsSetup, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FastPillarsSetup, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9VerticalPatternBrick, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's UNUSED_RoomState_8FB3EE.</summary>
    public static CartridgeRoomState UnusedRoom8FB3EE { get; } =
        new(nameof(UnusedRoom8FB3EE), 0xC8F40B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.UnusedRoom8387EC, Enemies.RoomEnemyPopulationDefinitions.UnusedRoom8FB3EEA1AD06, Enemies.RoomEnemyGraphicsSetDefinitions.UnusedState8FB3EEB48899, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.UnusedRoom8FB3EE, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9HorizontalPatternBrick, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_MickeyMouse.</summary>
    public static CartridgeRoomState MickeyMouse { get; } =
        new(nameof(MickeyMouse), 0xC8F58B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MickeyMouse, Enemies.RoomEnemyPopulationDefinitions.MickeyMouse, Enemies.RoomEnemyGraphicsSetDefinitions.MickeyMouse, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MickeyMouse, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MickeyMouse, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_Pillar.</summary>
    public static CartridgeRoomState Pillar { get; } =
        new(nameof(Pillar), 0xC8FCC5, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Pillar, Enemies.RoomEnemyPopulationDefinitions.Pillar, Enemies.RoomEnemyGraphicsSetDefinitions.Pillar, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Pillar, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_Plowerhouse.</summary>
    public static CartridgeRoomState Plowerhouse { get; } =
        new(nameof(Plowerhouse), 0xC98222, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Plowerhouse, Enemies.RoomEnemyPopulationDefinitions.Plowerhouse, Enemies.RoomEnemyGraphicsSetDefinitions.Plowerhouse, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Plowerhouse, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_WorstRoomInTheGame.</summary>
    public static CartridgeRoomState WorstRoomInTheGame { get; } =
        new(nameof(WorstRoomInTheGame), 0xC984D3, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WorstRoomInTheGame, Enemies.RoomEnemyPopulationDefinitions.WorstRoomInTheGame, Enemies.RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WorstRoomInTheGame, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WorstRoomInTheGame, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernHorizontalRuins, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_Amphitheatre.</summary>
    public static CartridgeRoomState Amphitheatre { get; } =
        new(nameof(Amphitheatre), 0xC9899F, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Amphitheatre, Enemies.RoomEnemyPopulationDefinitions.Amphitheatre, Enemies.RoomEnemyGraphicsSetDefinitions.Amphitheatre, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Amphitheatre, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_LNSpringBallMaze.</summary>
    public static CartridgeRoomState LNSpringBallMaze { get; } =
        new(nameof(LNSpringBallMaze), 0xC994BA, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNSpringballMaze, Enemies.RoomEnemyPopulationDefinitions.LNSpringBallMaze, Enemies.RoomEnemyGraphicsSetDefinitions.LNSpringBallMaze, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.LNSpringBallMaze, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNSpringBallMaze, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_LNEscapePowerBombs.</summary>
    public static CartridgeRoomState LNEscapePowerBombs { get; } =
        new(nameof(LNEscapePowerBombs), 0xC99CE2, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.LNEscapePowerBombs, Enemies.RoomEnemyPopulationDefinitions.LNEscapePowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.LNEscapePowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNEscapePowerBombs, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_RedKihunterShaft.</summary>
    public static CartridgeRoomState RedKihunterShaft { get; } =
        new(nameof(RedKihunterShaft), 0xC99E7B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RedKihunterShaft, Enemies.RoomEnemyPopulationDefinitions.RedKihunterShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedKihunterShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RedKihunterShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedKihunterShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_Wasteland.</summary>
    public static CartridgeRoomState Wasteland { get; } =
        new(nameof(Wasteland), 0xC9A88C, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Wasteland, Enemies.RoomEnemyPopulationDefinitions.Wasteland, Enemies.RoomEnemyGraphicsSetDefinitions.Wasteland, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Wasteland, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Wasteland, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_MetalPirates.</summary>
    public static CartridgeRoomState MetalPirates { get; } =
        new(nameof(MetalPirates), 0xC9B1C7, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MetalPirates, Enemies.RoomEnemyPopulationDefinitions.MetalPirates, Enemies.RoomEnemyGraphicsSetDefinitions.MetalPirates, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MetalPirates, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_ThreeMusketeers.</summary>
    public static CartridgeRoomState ThreeMusketeers { get; } =
        new(nameof(ThreeMusketeers), 0xC9B4AB, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ThreeMusketeers, Enemies.RoomEnemyPopulationDefinitions.ThreeMusketeers, Enemies.RoomEnemyGraphicsSetDefinitions.ThreeMusketeers, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ThreeMusketeers, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ThreeMusketeers, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_RidleyETank.</summary>
    public static CartridgeRoomState RidleyETank { get; } =
        new(nameof(RidleyETank), 0xC9C30D, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyETank, Enemies.RoomEnemyPopulationDefinitions.RidleyETank, Enemies.RoomEnemyGraphicsSetDefinitions.RidleyETank, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RidleyETank, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_ScrewAttack.</summary>
    public static CartridgeRoomState ScrewAttack { get; } =
        new(nameof(ScrewAttack), 0xC9C428, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.ScrewAttack, Enemies.RoomEnemyPopulationDefinitions.ScrewAttack, Enemies.RoomEnemyGraphicsSetDefinitions.ScrewAttack, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ScrewAttack, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);

    /// <summary>The disassembly's RoomState_LNFireflea.</summary>
    public static CartridgeRoomState LNFireflea { get; } =
        new(nameof(LNFireflea), 0xC9C706, 0x0A, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNFireflea, Enemies.RoomEnemyPopulationDefinitions.LNFireflea, Enemies.RoomEnemyGraphicsSetDefinitions.LNFireflea, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.LNFireflea, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNFireflea, null, RoomSetupCallback.None);

    /// <summary>The disassembly's RoomState_LNSave.</summary>
    public static CartridgeRoomState LNSave { get; } =
        new(nameof(LNSave), 0xCE95C2, 0x19, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNSave, Enemies.RoomEnemyPopulationDefinitions.LNSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNSave, null, RoomSetupCallback.None);

    /// <summary>The disassembly's RoomState_BowlingAlley_0.</summary>
    public static CartridgeRoomState BowlingAlley0 { get; } =
        new(nameof(BowlingAlley0), 0xC48322, 0x05, 0x30, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyState0, Enemies.RoomEnemyPopulationDefinitions.BowlingAlley0, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlley0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BowlingAlleyState0, null, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_BowlingAlley_1.</summary>
    public static CartridgeRoomState BowlingAlley1 { get; } =
        new(nameof(BowlingAlley1), 0xC48322, 0x04, 0x30, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyState1, Enemies.RoomEnemyPopulationDefinitions.BowlingAlley1, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlley1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BowlingAlleyState1, null, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_WreckedShipEntrance_0.</summary>
    public static CartridgeRoomState WreckedShipEntrance0 { get; } =
        new(nameof(WreckedShipEntrance0), 0xC49D2E, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipEntrance0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipEntrance0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipEntranceState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45EntranceHall, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_WreckedShipEntrance_1.</summary>
    public static CartridgeRoomState WreckedShipEntrance1 { get; } =
        new(nameof(WreckedShipEntrance1), 0xC49D2E, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipEntrance1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipEntrance1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipEntranceState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45EntranceHall, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_Attic_0.</summary>
    public static CartridgeRoomState Attic0 { get; } =
        new(nameof(Attic0), 0xC49EAE, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.AtticState0, Enemies.RoomEnemyPopulationDefinitions.Attic0, Enemies.RoomEnemyGraphicsSetDefinitions.Attic0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xFF), Scrolling.RoomScrollDefinitions.Attic, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AtticState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ExperimentRoom, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_Attic_1.</summary>
    public static CartridgeRoomState Attic1 { get; } =
        new(nameof(Attic1), 0xC4A2E7, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.AtticState1, Enemies.RoomEnemyPopulationDefinitions.Attic1, Enemies.RoomEnemyGraphicsSetDefinitions.Attic1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xFF), Scrolling.RoomScrollDefinitions.Attic, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AtticState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ExperimentRoom, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_AssemblyLine_0.</summary>
    public static CartridgeRoomState AssemblyLine0 { get; } =
        new(nameof(AssemblyLine0), 0xC4A720, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineState0, Enemies.RoomEnemyPopulationDefinitions.AssemblyLine0, Enemies.RoomEnemyGraphicsSetDefinitions.AssemblyLine0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AssemblyLineState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsVertical, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_AssemblyLine_1.</summary>
    public static CartridgeRoomState AssemblyLine1 { get; } =
        new(nameof(AssemblyLine1), 0xC4A720, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineState1, Enemies.RoomEnemyPopulationDefinitions.AssemblyLine1, Enemies.RoomEnemyGraphicsSetDefinitions.AssemblyLine1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AssemblyLineState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsVertical, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_WreckedShipMainShaft_0.</summary>
    public static CartridgeRoomState WreckedShipMainShaft0 { get; } =
        new(nameof(WreckedShipMainShaft0), 0xC4A9AC, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMainShaft0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaft0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipMainShaft, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_WreckedShipMainShaft_1.</summary>
    public static CartridgeRoomState WreckedShipMainShaft1 { get; } =
        new(nameof(WreckedShipMainShaft1), 0xC4BDC0, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMainShaft1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaft1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipMainShaft, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_SpikyDeath_0.</summary>
    public static CartridgeRoomState SpikyDeath0 { get; } =
        new(nameof(SpikyDeath0), 0xC4D187, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathState0, Enemies.RoomEnemyPopulationDefinitions.SpikyDeath0, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyDeath0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpikyDeathState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_SpikyDeath_1.</summary>
    public static CartridgeRoomState SpikyDeath1 { get; } =
        new(nameof(SpikyDeath1), 0xC4D187, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathState1, Enemies.RoomEnemyPopulationDefinitions.SpikyDeath1, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyDeath1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpikyDeathState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_ElectricDeath_0.</summary>
    public static CartridgeRoomState ElectricDeath0 { get; } =
        new(nameof(ElectricDeath0), 0xC4D3EE, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathState0, Enemies.RoomEnemyPopulationDefinitions.ElectricDeath0, Enemies.RoomEnemyGraphicsSetDefinitions.ElectricDeath0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ElectricDeathState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_ElectricDeath_1.</summary>
    public static CartridgeRoomState ElectricDeath1 { get; } =
        new(nameof(ElectricDeath1), 0xC4D3EE, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathState1, Enemies.RoomEnemyPopulationDefinitions.ElectricDeath1, Enemies.RoomEnemyGraphicsSetDefinitions.ElectricDeath1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ElectricDeathState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_WreckedShipETank_0.</summary>
    public static CartridgeRoomState WreckedShipETank0 { get; } =
        new(nameof(WreckedShipETank0), 0xC4D883, 0x05, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipETank0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipETank0, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipETankState0, null, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_WreckedShipETank_1.</summary>
    public static CartridgeRoomState WreckedShipETank1 { get; } =
        new(nameof(WreckedShipETank1), 0xC4D883, 0x04, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipETank1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipETank1, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipETankState1, null, RoomSetupCallback.OrdinaryReturn);

    /// <summary>The disassembly's RoomState_Basement_0.</summary>
    public static CartridgeRoomState Basement0 { get; } =
        new(nameof(Basement0), 0xC4E14E, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BasementState0, Enemies.RoomEnemyPopulationDefinitions.Basement0, Enemies.RoomEnemyGraphicsSetDefinitions.Basement0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Basement, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BasementState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile);

    /// <summary>The disassembly's RoomState_Basement_1.</summary>
    public static CartridgeRoomState Basement1 { get; } =
        new(nameof(Basement1), 0xC4E14E, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.BasementState1, Enemies.RoomEnemyPopulationDefinitions.Basement1, Enemies.RoomEnemyGraphicsSetDefinitions.Basement1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.Basement, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BasementState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile);

    /// <summary>The disassembly's RoomState_WreckedShipMap_0.</summary>
    public static CartridgeRoomState WreckedShipMap0 { get; } =
        new(nameof(WreckedShipMap0), 0xCE83C3, 0x16, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMap0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMap0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipMapState0, null, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_WreckedShipMap_1.</summary>
    public static CartridgeRoomState WreckedShipMap1 { get; } =
        new(nameof(WreckedShipMap1), 0xCE83C3, 0x15, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMap1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMap1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipMapState1, null, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_Phantoon_0.</summary>
    public static CartridgeRoomState Phantoon0 { get; } =
        new(nameof(Phantoon0), 0xC4E58C, 0x05, 0x27, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonState0, Enemies.RoomEnemyPopulationDefinitions.Phantoon0, Enemies.RoomEnemyGraphicsSetDefinitions.Phantoon0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Phantoon, Backgrounds.LibraryBackgroundProgramDefinitions.PhantoonState0, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_Phantoon_1.</summary>
    public static CartridgeRoomState Phantoon1 { get; } =
        new(nameof(Phantoon1), 0xC4E58C, 0x04, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonState1, Enemies.RoomEnemyPopulationDefinitions.Phantoon1, Enemies.RoomEnemyGraphicsSetDefinitions.Phantoon1, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Phantoon, Backgrounds.LibraryBackgroundProgramDefinitions.PhantoonDraygonState1, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_SpongeBath_0.</summary>
    public static CartridgeRoomState SpongeBath0 { get; } =
        new(nameof(SpongeBath0), 0xC4E6A5, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathState0, Enemies.RoomEnemyPopulationDefinitions.SpongeBath0, Enemies.RoomEnemyGraphicsSetDefinitions.SpongeBath0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpongeBathState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_SpongeBath_1.</summary>
    public static CartridgeRoomState SpongeBath1 { get; } =
        new(nameof(SpongeBath1), 0xC4E6A5, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathState1, Enemies.RoomEnemyPopulationDefinitions.SpongeBath1, Enemies.RoomEnemyGraphicsSetDefinitions.SpongeBath1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpongeBathState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_WSWestSuper_0.</summary>
    public static CartridgeRoomState WSWestSuper0 { get; } =
        new(nameof(WSWestSuper0), 0xC4E94E, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperState0, Enemies.RoomEnemyPopulationDefinitions.WSWestSuper0, Enemies.RoomEnemyGraphicsSetDefinitions.WSWestSuper0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WSWestSuperState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_WSWestSuper_1.</summary>
    public static CartridgeRoomState WSWestSuper1 { get; } =
        new(nameof(WSWestSuper1), 0xC4E94E, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperState1, Enemies.RoomEnemyPopulationDefinitions.WSWestSuper1, Enemies.RoomEnemyGraphicsSetDefinitions.WSWestSuper1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WSWestSuperState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_WSEastSuper_0.</summary>
    public static CartridgeRoomState WSEastSuper0 { get; } =
        new(nameof(WSEastSuper0), 0xC4EA8F, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRoomsState0839C74, Enemies.RoomEnemyPopulationDefinitions.WSEastSuper0, Enemies.RoomEnemyGraphicsSetDefinitions.WSEastSuper0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WSEastSuperState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_WSEastSuper_1.</summary>
    public static CartridgeRoomState WSEastSuper1 { get; } =
        new(nameof(WSEastSuper1), 0xC4ED98, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WSEastSuperState1, Enemies.RoomEnemyPopulationDefinitions.WSEastSuper1, Enemies.RoomEnemyGraphicsSetDefinitions.WSEastSuper1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WSEastSuperState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_GravitySuit_0.</summary>
    public static CartridgeRoomState GravitySuit0 { get; } =
        new(nameof(GravitySuit0), 0xC4F0A1, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRoomsState0839C74, Enemies.RoomEnemyPopulationDefinitions.GravitySuit0, Enemies.RoomEnemyGraphicsSetDefinitions.GravitySuit0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GravitySuitState0WreckedShipSaveState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_GravitySuit_1.</summary>
    public static CartridgeRoomState GravitySuit1 { get; } =
        new(nameof(GravitySuit1), 0xC4F0A1, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitState1WreckedShipSaveState1, Enemies.RoomEnemyPopulationDefinitions.GravitySuit1, Enemies.RoomEnemyGraphicsSetDefinitions.GravitySuit1, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GravitySuitState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_WreckedShipSave_0.</summary>
    public static CartridgeRoomState WreckedShipSave0 { get; } =
        new(nameof(WreckedShipSave0), 0xCE95C2, 0x16, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRoomsState0839C74, Enemies.RoomEnemyPopulationDefinitions.WreckedShipSave0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipSave0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GravitySuitState0WreckedShipSaveState0, null, RoomSetupCallback.None);

    /// <summary>The disassembly's RoomState_WreckedShipSave_1.</summary>
    public static CartridgeRoomState WreckedShipSave1 { get; } =
        new(nameof(WreckedShipSave1), 0xCE95C2, 0x15, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitState1WreckedShipSaveState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipSave1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipSave1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipSaveState1, null, RoomSetupCallback.BossRoomReturn);

    /// <summary>The disassembly's RoomState_GlassTunnelSave.</summary>
    public static CartridgeRoomState GlassTunnelSave { get; } =
        new(nameof(GlassTunnelSave), 0xCE95C2, 0x19, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnelSave, Enemies.RoomEnemyPopulationDefinitions.GlassTunnelSave, Enemies.RoomEnemyGraphicsSetDefinitions.GlassTunnelSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GlassTunnelSave, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_GlassTunnel_0.</summary>
    public static CartridgeRoomState GlassTunnel0 { get; } =
        new(nameof(GlassTunnel0), 0xC9DB52, 0x0B, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnelState01, Enemies.RoomEnemyPopulationDefinitions.GlassTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.GlassTunnel, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GlassTunnelState01, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_GlassTunnel_1.</summary>
    public static CartridgeRoomState GlassTunnel1 { get; } =
        new(nameof(GlassTunnel1), 0xC9E129, 0x0B, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnelState01, Enemies.RoomEnemyPopulationDefinitions.GlassTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.GlassTunnel, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GlassTunnelState01, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_WestTunnel.</summary>
    public static CartridgeRoomState WestTunnel { get; } =
        new(nameof(WestTunnel), 0xC9E6AE, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WestTunnel, Enemies.RoomEnemyPopulationDefinitions.WestTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.WestTunnel, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.WestTunnel, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WestTunnel, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_EastTunnel.</summary>
    public static CartridgeRoomState EastTunnel { get; } =
        new(nameof(EastTunnel), 0xC9E809, 0x0B, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.EastTunnel, Enemies.RoomEnemyPopulationDefinitions.EastTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.EastTunnel, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.EastTunnel, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EastTunnel, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_MainStreet.</summary>
    public static CartridgeRoomState MainStreet { get; } =
        new(nameof(MainStreet), 0xC9F225, 0x0B, 0x1B, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.MainStreet, Enemies.RoomEnemyPopulationDefinitions.MainStreet, Enemies.RoomEnemyGraphicsSetDefinitions.MainStreet, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), Scrolling.RoomScrollDefinitions.MainStreet, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MainStreet, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_FishTank.</summary>
    public static CartridgeRoomState FishTank { get; } =
        new(nameof(FishTank), 0xCA8EFF, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FishTank, Enemies.RoomEnemyPopulationDefinitions.FishTank, Enemies.RoomEnemyGraphicsSetDefinitions.FishTank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), Scrolling.RoomScrollDefinitions.FishTank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FishTank, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_MamaTurtle.</summary>
    public static CartridgeRoomState MamaTurtle { get; } =
        new(nameof(MamaTurtle), 0xCAA113, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MamaTurtle, Enemies.RoomEnemyPopulationDefinitions.MamaTurtle, Enemies.RoomEnemyGraphicsSetDefinitions.MamaTurtle, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.MamaTurtle, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MamaTurtle, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_CrabTunnel.</summary>
    public static CartridgeRoomState CrabTunnel { get; } =
        new(nameof(CrabTunnel), 0xCAAF99, 0x0B, 0x1B, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.CrabTunnel, Enemies.RoomEnemyPopulationDefinitions.CrabTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.CrabTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrabTunnel, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrabTunnel, Backgrounds.LibraryBackgroundProgramDefinitions.MaridiaBBrownWall, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_MtEverest.</summary>
    public static CartridgeRoomState MtEverest { get; } =
        new(nameof(MtEverest), 0xCAB24F, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MtEverest, Enemies.RoomEnemyPopulationDefinitions.MtEverest, Enemies.RoomEnemyGraphicsSetDefinitions.MtEverest, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), Scrolling.RoomScrollDefinitions.MtEverest, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MtEverest, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_RedFish.</summary>
    public static CartridgeRoomState RedFish { get; } =
        new(nameof(RedFish), 0xCACE42, 0x0B, 0x1B, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.RedFish, Enemies.RoomEnemyPopulationDefinitions.RedFish, Enemies.RoomEnemyGraphicsSetDefinitions.RedFish, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.RedFish, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedFish, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_WateringHole.</summary>
    public static CartridgeRoomState WateringHole { get; } =
        new(nameof(WateringHole), 0xCAD474, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WateringHole, Enemies.RoomEnemyPopulationDefinitions.WateringHole, Enemies.RoomEnemyGraphicsSetDefinitions.WateringHole, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WateringHole, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WateringHole, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_NWestMaridiaBug.</summary>
    public static CartridgeRoomState NWestMaridiaBug { get; } =
        new(nameof(NWestMaridiaBug), 0xCADBC8, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.NWestMaridiaBug, Enemies.RoomEnemyPopulationDefinitions.NWestMaridiaBug, Enemies.RoomEnemyGraphicsSetDefinitions.NWestMaridiaBug, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.NWestMaridiaBug, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.NWestMaridiaBug, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_CrabShaft.</summary>
    public static CartridgeRoomState CrabShaft { get; } =
        new(nameof(CrabShaft), 0xCAE458, 0x0B, 0x1B, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.CrabShaft, Enemies.RoomEnemyPopulationDefinitions.CrabShaft, Enemies.RoomEnemyGraphicsSetDefinitions.CrabShaft, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.CrabShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrabShaft, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_PseudoPlasmaSpark.</summary>
    public static CartridgeRoomState PseudoPlasmaSpark { get; } =
        new(nameof(PseudoPlasmaSpark), 0xCAF59C, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PseudoPlasmaSpark, Enemies.RoomEnemyPopulationDefinitions.PseudoPlasmaSpark, Enemies.RoomEnemyGraphicsSetDefinitions.PseudoPlasmaSpark, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.PseudoPlasmaSpark, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PseudoPlasmaSpark, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_CrabHole.</summary>
    public static CartridgeRoomState CrabHole { get; } =
        new(nameof(CrabHole), 0xCB83DB, 0x0B, 0x1B, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrabHole, Enemies.RoomEnemyPopulationDefinitions.CrabHole, Enemies.RoomEnemyGraphicsSetDefinitions.CrabHole, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.CrabHole, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrabHole, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_WestSandHallTunnel.</summary>
    public static CartridgeRoomState WestSandHallTunnel { get; } =
        new(nameof(WestSandHallTunnel), 0xCB883A, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHallTunnel, Enemies.RoomEnemyPopulationDefinitions.WestSandHallTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.WestSandHallTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WestSandHallTunnel, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WestSandHallTunnel, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_PlasmaTutorial.</summary>
    public static CartridgeRoomState PlasmaTutorial { get; } =
        new(nameof(PlasmaTutorial), 0xCB89E0, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaTutorial, Enemies.RoomEnemyPopulationDefinitions.PlasmaTutorial, Enemies.RoomEnemyGraphicsSetDefinitions.PlasmaTutorial, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.PlasmaTutorial, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PlasmaTutorial, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_Plasma.</summary>
    public static CartridgeRoomState Plasma { get; } =
        new(nameof(Plasma), 0xCB8BD4, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Plasma, Enemies.RoomEnemyPopulationDefinitions.Plasma, Enemies.RoomEnemyGraphicsSetDefinitions.Plasma, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Plasma, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Plasma, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_ThreadTheNeedle.</summary>
    public static CartridgeRoomState ThreadTheNeedle { get; } =
        new(nameof(ThreadTheNeedle), 0xCB9792, 0x0B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ThreadTheNeedle, Enemies.RoomEnemyPopulationDefinitions.ThreadTheNeedle, Enemies.RoomEnemyGraphicsSetDefinitions.ThreadTheNeedle, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.ThreadTheNeedle, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ThreadTheNeedle, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_MaridiaElev.</summary>
    public static CartridgeRoomState MaridiaElev { get; } =
        new(nameof(MaridiaElev), 0xCBA0D0, 0x0B, 0x1B, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaElev, Enemies.RoomEnemyPopulationDefinitions.MaridiaElev, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaElev, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.MaridiaElev, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MaridiaElev, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_PlasmaSpark.</summary>
    public static CartridgeRoomState PlasmaSpark { get; } =
        new(nameof(PlasmaSpark), 0xCBA878, 0x0B, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaSpark, Enemies.RoomEnemyPopulationDefinitions.PlasmaSpark, Enemies.RoomEnemyGraphicsSetDefinitions.PlasmaSpark, Scrolling.Layer2ScrollModes.FromHeader(0xE0, 0x00), Scrolling.RoomScrollDefinitions.PlasmaSpark, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PlasmaSpark, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_Kassiuz.</summary>
    public static CartridgeRoomState Kassiuz { get; } =
        new(nameof(Kassiuz), 0xCBC64F, 0x0B, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Kassiuz, Enemies.RoomEnemyPopulationDefinitions.Kassiuz, Enemies.RoomEnemyGraphicsSetDefinitions.Kassiuz, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.Kassiuz, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Kassiuz, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_MaridiaMap.</summary>
    public static CartridgeRoomState MaridiaMap { get; } =
        new(nameof(MaridiaMap), 0xCE86BD, 0x15, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMap, Enemies.RoomEnemyPopulationDefinitions.MaridiaMap, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MaridiaMap, null, RoomSetupCallback.BossRoomReturnB);

    /// <summary>The disassembly's RoomState_ForgottenHighwaySave.</summary>
    public static CartridgeRoomState ForgottenHighwaySave { get; } =
        new(nameof(ForgottenHighwaySave), 0xCE95C2, 0x19, 0x1B, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwaySave, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwaySave, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwaySave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ForgottenHighwaySave, null, RoomSetupCallback.None);

    /// <summary>The disassembly's RoomState_Toilet.</summary>
    public static CartridgeRoomState Toilet { get; } =
        new(nameof(Toilet), 0xCBCD9F, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Toilet, Enemies.RoomEnemyPopulationDefinitions.Toilet, Enemies.RoomEnemyGraphicsSetDefinitions.Toilet, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.MaridiaElevatube, Plms.RoomPlmPopulationDefinitions.Toilet, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_BugSandHole.</summary>
    public static CartridgeRoomState BugSandHole { get; } =
        new(nameof(BugSandHole), 0xCBDCF3, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BugSandHole, Enemies.RoomEnemyPopulationDefinitions.BugSandHole, Enemies.RoomEnemyGraphicsSetDefinitions.BugSandHole, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BugSandHole, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BugSandHole, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_WestSandHall.</summary>
    public static CartridgeRoomState WestSandHall { get; } =
        new(nameof(WestSandHall), 0xCBDE8F, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHall, Enemies.RoomEnemyPopulationDefinitions.WestSandHall, Enemies.RoomEnemyGraphicsSetDefinitions.WestSandHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WestSandHall, Backgrounds.LibraryBackgroundProgramDefinitions.MaridiaCGreenWall, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_Oasis.</summary>
    public static CartridgeRoomState Oasis { get; } =
        new(nameof(Oasis), 0xCBE472, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Oasis, Enemies.RoomEnemyPopulationDefinitions.Oasis, Enemies.RoomEnemyGraphicsSetDefinitions.Oasis, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.Oasis, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Oasis, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_EastSandHall.</summary>
    public static CartridgeRoomState EastSandHall { get; } =
        new(nameof(EastSandHall), 0xCBE899, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHall, Enemies.RoomEnemyPopulationDefinitions.EastSandHall, Enemies.RoomEnemyGraphicsSetDefinitions.EastSandHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EastSandHall, Backgrounds.LibraryBackgroundProgramDefinitions.MaridiaCGreenWall, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_WestSandHole.</summary>
    public static CartridgeRoomState WestSandHole { get; } =
        new(nameof(WestSandHole), 0xCBEC32, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHole, Enemies.RoomEnemyPopulationDefinitions.WestSandHole, Enemies.RoomEnemyGraphicsSetDefinitions.WestSandHole, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.WestSandHole, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WestSandHole, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_EastSandHole.</summary>
    public static CartridgeRoomState EastSandHole { get; } =
        new(nameof(EastSandHole), 0xCBF580, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHole, Enemies.RoomEnemyPopulationDefinitions.EastSandHole, Enemies.RoomEnemyGraphicsSetDefinitions.EastSandHole, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.EastSandHole, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EastSandHole, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_WestAqueductQuicksand.</summary>
    public static CartridgeRoomState WestAqueductQuicksand { get; } =
        new(nameof(WestAqueductQuicksand), 0xCBFEC8, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, Enemies.RoomEnemyPopulationDefinitions.WestAqueductQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.WestAqueductQuicksand, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.WestAqueductQuicksand, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WestAqueductQuicksand, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_EastAqueductQuicksand.</summary>
    public static CartridgeRoomState EastAqueductQuicksand { get; } =
        new(nameof(EastAqueductQuicksand), 0xCC80B8, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, Enemies.RoomEnemyPopulationDefinitions.EastAqueductQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.EastAqueductQuicksand, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.EastAqueductQuicksand, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EastAqueductQuicksand, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_Aqueduct.</summary>
    public static CartridgeRoomState Aqueduct { get; } =
        new(nameof(Aqueduct), 0xCC82A8, 0x0C, 0x1B, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Aqueduct, Enemies.RoomEnemyPopulationDefinitions.Aqueduct, Enemies.RoomEnemyGraphicsSetDefinitions.Aqueduct, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Aqueduct, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Aqueduct, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_Butterfly.</summary>
    public static CartridgeRoomState Butterfly { get; } =
        new(nameof(Butterfly), 0xCCA13B, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Butterfly, Enemies.RoomEnemyPopulationDefinitions.Butterfly, Enemies.RoomEnemyGraphicsSetDefinitions.Butterfly, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Butterfly, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_BotwoonHallway.</summary>
    public static CartridgeRoomState BotwoonHallway { get; } =
        new(nameof(BotwoonHallway), 0xCCA34A, 0x0C, 0x1B, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonHallway, Enemies.RoomEnemyPopulationDefinitions.BotwoonHallway, Enemies.RoomEnemyGraphicsSetDefinitions.BotwoonHallway, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.BotwoonHallway, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BotwoonHallway, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_Pants.</summary>
    public static CartridgeRoomState Pants { get; } =
        new(nameof(Pants), 0xCCAC48, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Pants, Enemies.RoomEnemyPopulationDefinitions.Pants, Enemies.RoomEnemyGraphicsSetDefinitions.Pants, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.Pants, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Pants, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_EastPants.</summary>
    public static CartridgeRoomState EastPants { get; } =
        new(nameof(EastPants), 0xCCB843, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EastPants, Enemies.RoomEnemyPopulationDefinitions.EastPants, Enemies.RoomEnemyGraphicsSetDefinitions.EastPants, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.EastPants, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EastPants, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_Springball.</summary>
    public static CartridgeRoomState Springball { get; } =
        new(nameof(Springball), 0xCCBD31, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Springball, Enemies.RoomEnemyPopulationDefinitions.Springball, Enemies.RoomEnemyGraphicsSetDefinitions.Springball, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Springball, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Springball, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_BelowBotwoonETank.</summary>
    public static CartridgeRoomState BelowBotwoonETank { get; } =
        new(nameof(BelowBotwoonETank), 0xCCC22F, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BelowBotwoonETank, Enemies.RoomEnemyPopulationDefinitions.BelowBotwoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.BelowBotwoonETank, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BelowBotwoonETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BelowBotwoonETank, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_Colosseum.</summary>
    public static CartridgeRoomState Colosseum { get; } =
        new(nameof(Colosseum), 0xCCC9F1, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Colosseum, Enemies.RoomEnemyPopulationDefinitions.Colosseum, Enemies.RoomEnemyGraphicsSetDefinitions.Colosseum, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Colosseum, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Colosseum, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_AqueductSave.</summary>
    public static CartridgeRoomState AqueductSave { get; } =
        new(nameof(AqueductSave), 0xCE92CB, 0x19, 0x1B, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.AqueductSave, Enemies.RoomEnemyPopulationDefinitions.AqueductSave, Enemies.RoomEnemyGraphicsSetDefinitions.AqueductSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AqueductSave, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AqueductSave, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_ThePrecious_0.</summary>
    public static CartridgeRoomState ThePrecious0 { get; } =
        new(nameof(ThePrecious0), 0xCCE5B1, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ThePreciousState01, Enemies.RoomEnemyPopulationDefinitions.ThePrecious, Enemies.RoomEnemyGraphicsSetDefinitions.ThePrecious, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ThePreciousState01, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_ThePrecious_1.</summary>
    public static CartridgeRoomState ThePrecious1 { get; } =
        new(nameof(ThePrecious1), 0xCCE5B1, 0x0C, 0x1B, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ThePreciousState01, Enemies.RoomEnemyPopulationDefinitions.ThePrecious, Enemies.RoomEnemyGraphicsSetDefinitions.ThePrecious, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ThePreciousState01, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_BotwoonETank.</summary>
    public static CartridgeRoomState BotwoonETank { get; } =
        new(nameof(BotwoonETank), 0xCCEE0C, 0x0C, 0x1B, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonETank, Enemies.RoomEnemyPopulationDefinitions.BotwoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.BotwoonETank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.BotwoonETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BotwoonETank, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_DraygonSave.</summary>
    public static CartridgeRoomState DraygonSave { get; } =
        new(nameof(DraygonSave), 0xCE9EF6, 0x19, 0x1B, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.DraygonSave, Enemies.RoomEnemyPopulationDefinitions.DraygonSave, Enemies.RoomEnemyGraphicsSetDefinitions.DraygonSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DraygonSave, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_MaridiaMissileRefill.</summary>
    public static CartridgeRoomState MaridiaMissileRefill { get; } =
        new(nameof(MaridiaMissileRefill), 0xCEAB31, 0x17, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMissileRefill, Enemies.RoomEnemyPopulationDefinitions.MaridiaMissileRefill, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaMissileRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MaridiaMissileRefill, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_PlasmaBeachQuicksand.</summary>
    public static CartridgeRoomState PlasmaBeachQuicksand { get; } =
        new(nameof(PlasmaBeachQuicksand), 0xCCFA8D, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, Enemies.RoomEnemyPopulationDefinitions.PlasmaBeachQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.PlasmaBeachQuicksand, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.PlasmaBeachQuicksand, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PlasmaBeachQuicksand, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_BotwoonQuicksand.</summary>
    public static CartridgeRoomState BotwoonQuicksand { get; } =
        new(nameof(BotwoonQuicksand), 0xCCFB88, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, Enemies.RoomEnemyPopulationDefinitions.BotwoonQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.BotwoonQuicksand, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BotwoonQuicksand, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BotwoonQuicksand, null, RoomSetupCallback.BossRoomReturnC);

    /// <summary>The disassembly's RoomState_Shaktool_0.</summary>
    public static CartridgeRoomState Shaktool0 { get; } =
        new(nameof(Shaktool0), 0xCCFD75, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ShaktoolState01, Enemies.RoomEnemyPopulationDefinitions.Shaktool, Enemies.RoomEnemyGraphicsSetDefinitions.Shaktool, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ShaktoolState01, null, RoomSetupCallback.SetupShaktoolRoomPlm);

    /// <summary>The disassembly's RoomState_Shaktool_1.</summary>
    public static CartridgeRoomState Shaktool1 { get; } =
        new(nameof(Shaktool1), 0xCD8404, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ShaktoolState01, Enemies.RoomEnemyPopulationDefinitions.Shaktool, Enemies.RoomEnemyGraphicsSetDefinitions.Shaktool, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ShaktoolState01, null, RoomSetupCallback.ReturnBeforeDraygonSetup);

    /// <summary>The disassembly's RoomState_HalfieClimb.</summary>
    public static CartridgeRoomState HalfieClimb { get; } =
        new(nameof(HalfieClimb), 0xCD8A37, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.HalfieClimb, Enemies.RoomEnemyPopulationDefinitions.HalfieClimb, Enemies.RoomEnemyGraphicsSetDefinitions.HalfieClimb, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.HalfieClimb, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.HalfieClimb, null, RoomSetupCallback.ReturnBeforeDraygonSetup);

    /// <summary>The disassembly's RoomState_Botwoon_0.</summary>
    public static CartridgeRoomState Botwoon0 { get; } =
        new(nameof(Botwoon0), 0xCD950E, 0x0C, 0x2A, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonState01, Enemies.RoomEnemyPopulationDefinitions.Botwoon, Enemies.RoomEnemyGraphicsSetDefinitions.Botwoon, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BotwoonState01, null, RoomSetupCallback.ReturnBeforeDraygonSetup);

    /// <summary>The disassembly's RoomState_Botwoon_1.</summary>
    public static CartridgeRoomState Botwoon1 { get; } =
        new(nameof(Botwoon1), 0xCD950E, 0x0C, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonState01, Enemies.RoomEnemyPopulationDefinitions.Botwoon, Enemies.RoomEnemyGraphicsSetDefinitions.Botwoon, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BotwoonState01, null, RoomSetupCallback.ReturnBeforeDraygonSetup);

    /// <summary>The disassembly's RoomState_SpaceJump.</summary>
    public static CartridgeRoomState SpaceJump { get; } =
        new(nameof(SpaceJump), 0xCD991E, 0x0C, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.SpaceJump, Enemies.RoomEnemyPopulationDefinitions.SpaceJump, Enemies.RoomEnemyGraphicsSetDefinitions.SpaceJump, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.SpaceJump, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpaceJump, null, RoomSetupCallback.ReturnBeforeDraygonSetup);

    /// <summary>The disassembly's RoomState_MaridiaEnergyRefill.</summary>
    public static CartridgeRoomState MaridiaEnergyRefill { get; } =
        new(nameof(MaridiaEnergyRefill), 0xCE98DC, 0x17, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaEnergyRefill, Enemies.RoomEnemyPopulationDefinitions.MaridiaEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaEnergyRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.MaridiaEnergyRefill, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MaridiaEnergyRefill, null, RoomSetupCallback.None);

    /// <summary>The disassembly's RoomState_WestCactusAlley.</summary>
    public static CartridgeRoomState WestCactusAlley { get; } =
        new(nameof(WestCactusAlley), 0xCD9B28, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WestCactusAlley, Enemies.RoomEnemyPopulationDefinitions.WestCactusAlley, Enemies.RoomEnemyGraphicsSetDefinitions.WestCactusAlley, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.WestCactusAlley, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WestCactusAlley, null, RoomSetupCallback.None);

    /// <summary>The disassembly's RoomState_EastCactusAlley.</summary>
    public static CartridgeRoomState EastCactusAlley { get; } =
        new(nameof(EastCactusAlley), 0xCDA00D, 0x0C, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EastCactusAlley, Enemies.RoomEnemyPopulationDefinitions.EastCactusAlley, Enemies.RoomEnemyGraphicsSetDefinitions.EastCactusAlley, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.EastCactusAlley, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EastCactusAlley, null, RoomSetupCallback.None);

    /// <summary>The disassembly's RoomState_Draygon_0.</summary>
    public static CartridgeRoomState Draygon0 { get; } =
        new(nameof(Draygon0), 0xCDB19D, 0x1C, 0x24, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.DraygonState01, Enemies.RoomEnemyPopulationDefinitions.Draygon0, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DraygonState01, Backgrounds.LibraryBackgroundProgramDefinitions.DraygonState0, RoomSetupCallback.SetPausingCodeForDraygon);

    /// <summary>The disassembly's RoomState_Draygon_1.</summary>
    public static CartridgeRoomState Draygon1 { get; } =
        new(nameof(Draygon1), 0xCDB19D, 0x1C, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.DraygonState01, Enemies.RoomEnemyPopulationDefinitions.DraygonState1MotherBrainState2, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DraygonState01, Backgrounds.LibraryBackgroundProgramDefinitions.PhantoonDraygonState1, RoomSetupCallback.SetPausingCodeForDraygon);

    /// <summary>The disassembly's RoomState_TourianFirst.</summary>
    public static CartridgeRoomState TourianFirst { get; } =
        new(nameof(TourianFirst), 0xCDC4FE, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianFirst, Enemies.RoomEnemyPopulationDefinitions.TourianFirst, Enemies.RoomEnemyGraphicsSetDefinitions.TourianFirst, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.TourianFirst, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.TourianFirst, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.SetCollectedMap);

    /// <summary>The disassembly's RoomState_Metroids1_0.</summary>
    public static CartridgeRoomState Metroids10 { get; } =
        new(nameof(Metroids10), 0xCDC8DC, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1State01, Enemies.RoomEnemyPopulationDefinitions.Metroids10, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids10, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids1State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_Metroids1_1.</summary>
    public static CartridgeRoomState Metroids11 { get; } =
        new(nameof(Metroids11), 0xCDC8DC, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1State01, Enemies.RoomEnemyPopulationDefinitions.Metroids11, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids11, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids1State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_Metroids2_0.</summary>
    public static CartridgeRoomState Metroids20 { get; } =
        new(nameof(Metroids20), 0xCDCDA0, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2State01, Enemies.RoomEnemyPopulationDefinitions.Metroids20, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids20, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids2State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_Metroids2_1.</summary>
    public static CartridgeRoomState Metroids21 { get; } =
        new(nameof(Metroids21), 0xCDCDA0, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2State01, Enemies.RoomEnemyPopulationDefinitions.Metroids21, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids21, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids2State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_Metroids3_0.</summary>
    public static CartridgeRoomState Metroids30 { get; } =
        new(nameof(Metroids30), 0xCDD02D, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3State01, Enemies.RoomEnemyPopulationDefinitions.Metroids30, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids30, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids3State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_Metroids3_1.</summary>
    public static CartridgeRoomState Metroids31 { get; } =
        new(nameof(Metroids31), 0xCDD02D, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3State01, Enemies.RoomEnemyPopulationDefinitions.Metroids31, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids31, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids3State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_Metroids4_0.</summary>
    public static CartridgeRoomState Metroids40 { get; } =
        new(nameof(Metroids40), 0xCDD3E5, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4State01, Enemies.RoomEnemyPopulationDefinitions.Metroids40, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids40, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids4State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_Metroids4_1.</summary>
    public static CartridgeRoomState Metroids41 { get; } =
        new(nameof(Metroids41), 0xCDD3E5, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4State01, Enemies.RoomEnemyPopulationDefinitions.Metroids41, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids41, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids4State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_BlueHopper_0.</summary>
    public static CartridgeRoomState BlueHopper0 { get; } =
        new(nameof(BlueHopper0), 0xCDD5EB, 0x0D, 0x45, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopperState01, Enemies.RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueHopperState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_BlueHopper_1.</summary>
    public static CartridgeRoomState BlueHopper1 { get; } =
        new(nameof(BlueHopper1), 0xCDD5EB, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopperState01, Enemies.RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueHopperState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_DustTorizo_0.</summary>
    public static CartridgeRoomState DustTorizo0 { get; } =
        new(nameof(DustTorizo0), 0xCDD7C4, 0x0D, 0x45, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizoState01, Enemies.RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DustTorizoState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_DustTorizo_1.</summary>
    public static CartridgeRoomState DustTorizo1 { get; } =
        new(nameof(DustTorizo1), 0xCDD7C4, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizoState01, Enemies.RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DustTorizoState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_BigBoy_0.</summary>
    public static CartridgeRoomState BigBoy0 { get; } =
        new(nameof(BigBoy0), 0xCDD930, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoyState01, Enemies.RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BigBoyState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_BigBoy_1.</summary>
    public static CartridgeRoomState BigBoy1 { get; } =
        new(nameof(BigBoy1), 0xCDD930, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoyState01, Enemies.RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BigBoyState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_Seaweed.</summary>
    public static CartridgeRoomState Seaweed { get; } =
        new(nameof(Seaweed), 0xCDDBF8, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Seaweed, Enemies.RoomEnemyPopulationDefinitions.Seaweed, Enemies.RoomEnemyGraphicsSetDefinitions.Seaweed, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Seaweed, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Seaweed, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_TourianRecharge.</summary>
    public static CartridgeRoomState TourianRecharge { get; } =
        new(nameof(TourianRecharge), 0xCEA823, 0x18, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianRecharge, Enemies.RoomEnemyPopulationDefinitions.TourianRecharge, Enemies.RoomEnemyGraphicsSetDefinitions.TourianRecharge, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.TourianRecharge, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.TourianRecharge, null, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_MotherBrain_0.</summary>
    public static CartridgeRoomState MotherBrain0 { get; } =
        new(nameof(MotherBrain0), 0xCDDEDE, 0x0E, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainState01, Enemies.RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MotherBrainState012, Backgrounds.LibraryBackgroundProgramDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_MotherBrain_1.</summary>
    public static CartridgeRoomState MotherBrain1 { get; } =
        new(nameof(MotherBrain1), 0xCDDEDE, 0x0E, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainState01, Enemies.RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MotherBrainState012, Backgrounds.LibraryBackgroundProgramDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_MotherBrain_2.</summary>
    public static CartridgeRoomState MotherBrain2 { get; } =
        new(nameof(MotherBrain2), 0xCDDEDE, 0x0E, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.DebugMotherBrainState2, Enemies.RoomEnemyPopulationDefinitions.DraygonState1MotherBrainState2, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MotherBrainState012, Backgrounds.LibraryBackgroundProgramDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_TourianEyeDoor.</summary>
    public static CartridgeRoomState TourianEyeDoor { get; } =
        new(nameof(TourianEyeDoor), 0xCDE20F, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEyeDoor, Enemies.RoomEnemyPopulationDefinitions.TourianEyeDoor, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEyeDoor, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEyeDoor, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.TourianEyeDoor, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_RinkaShaft.</summary>
    public static CartridgeRoomState RinkaShaft { get; } =
        new(nameof(RinkaShaft), 0xCDE518, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.RinkaShaft, Enemies.RoomEnemyPopulationDefinitions.RinkaShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RinkaShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RinkaShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RinkaShaft, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_MotherBrainSave.</summary>
    public static CartridgeRoomState MotherBrainSave { get; } =
        new(nameof(MotherBrainSave), 0xCE92CB, 0x19, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainSave, Enemies.RoomEnemyPopulationDefinitions.MotherBrainSave, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrainSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.MotherBrainSave, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MotherBrainSave, null, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);

    /// <summary>The disassembly's RoomState_TourianEscape1.</summary>
    public static CartridgeRoomState TourianEscape1 { get; } =
        new(nameof(TourianEscape1), 0xCDE914, 0x0D, 0x24, 0x07, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape1, Enemies.RoomEnemyPopulationDefinitions.TourianEscape1, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape1, null, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, Plms.RoomPlmPopulationDefinitions.TourianEscape1, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoomLetterbox, RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking);

    /// <summary>The disassembly's RoomState_TourianEscape2.</summary>
    public static CartridgeRoomState TourianEscape2 { get; } =
        new(nameof(TourianEscape2), 0xCDEB5B, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape2, Enemies.RoomEnemyPopulationDefinitions.TourianEscape2, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape2, null, RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal, Plms.RoomPlmPopulationDefinitions.TourianEscape2, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupCallback.SetLightHorizontalRoomShaking);

    /// <summary>The disassembly's RoomState_TourianEscape3.</summary>
    public static CartridgeRoomState TourianEscape3 { get; } =
        new(nameof(TourianEscape3), 0xCDED7A, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape3, Enemies.RoomEnemyPopulationDefinitions.TourianEscape3, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape3, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape3, null, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, Plms.RoomPlmPopulationDefinitions.TourianEscape3, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoom, RoomSetupCallback.SetMediumHorizontalRoomShaking);

    /// <summary>The disassembly's RoomState_TourianEscape4.</summary>
    public static CartridgeRoomState TourianEscape4 { get; } =
        new(nameof(TourianEscape4), 0xCDF534, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ToruianEscaper4, Enemies.RoomEnemyPopulationDefinitions.TourianEscape4, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape4, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape4, null, RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal, Plms.RoomPlmPopulationDefinitions.TourianEscape4, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoom, RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking);

    /// <summary>The disassembly's RoomState_UpperTourianSave.</summary>
    public static CartridgeRoomState UpperTourianSave { get; } =
        new(nameof(UpperTourianSave), 0xCE95C2, 0x19, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperTourianSave, Enemies.RoomEnemyPopulationDefinitions.UpperTourianSave, Enemies.RoomEnemyGraphicsSetDefinitions.UpperTourianSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.UpperTourianSave, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.UpperTourianSave, null, RoomSetupCallback.None);

    /// <summary>The disassembly's RoomState_CeresElev_0.</summary>
    public static CartridgeRoomState CeresElev0 { get; } =
        new(nameof(CeresElev0), 0xCDB846, 0x11, 0x2D, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElevState01, Enemies.RoomEnemyPopulationDefinitions.CeresElev0, Enemies.RoomEnemyGraphicsSetDefinitions.CeresElev0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.CeresElevatorShaft, Plms.RoomPlmPopulationDefinitions.CeresElevState01, Backgrounds.LibraryBackgroundProgramDefinitions.CeresElevatorState01, RoomSetupCallback.SpawnCeresHaze);

    /// <summary>The disassembly's RoomState_CeresElev_1.</summary>
    public static CartridgeRoomState CeresElev1 { get; } =
        new(nameof(CeresElev1), 0xCDB846, 0x12, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElevState01, Enemies.RoomEnemyPopulationDefinitions.CeresElev1, Enemies.RoomEnemyGraphicsSetDefinitions.CeresElev1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.CeresElevatorShaft, Plms.RoomPlmPopulationDefinitions.CeresElevState01, Backgrounds.LibraryBackgroundProgramDefinitions.CeresElevatorState01, RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze);

    /// <summary>The disassembly's RoomState_FallingTile_0.</summary>
    public static CartridgeRoomState FallingTile0 { get; } =
        new(nameof(FallingTile0), 0xCDBBFE, 0x0F, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileState0, Enemies.RoomEnemyPopulationDefinitions.FallingTile0, Enemies.RoomEnemyGraphicsSetDefinitions.FallingTile0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, Plms.RoomPlmPopulationDefinitions.FallingTileState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);

    /// <summary>The disassembly's RoomState_FallingTile_1.</summary>
    public static CartridgeRoomState FallingTile1 { get; } =
        new(nameof(FallingTile1), 0xCDBBFE, 0x10, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileState1, Enemies.RoomEnemyPopulationDefinitions.FallingTile1, Enemies.RoomEnemyGraphicsSetDefinitions.FallingTile1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, Plms.RoomPlmPopulationDefinitions.FallingTileState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);

    /// <summary>The disassembly's RoomState_MagnetStairs_0.</summary>
    public static CartridgeRoomState MagnetStairs0 { get; } =
        new(nameof(MagnetStairs0), 0xCDBD78, 0x0F, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsState0, Enemies.RoomEnemyPopulationDefinitions.MagnetStairs0, Enemies.RoomEnemyGraphicsSetDefinitions.MagnetStairs0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x41), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, Plms.RoomPlmPopulationDefinitions.MagnetStairsState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);

    /// <summary>The disassembly's RoomState_MagnetStairs_1.</summary>
    public static CartridgeRoomState MagnetStairs1 { get; } =
        new(nameof(MagnetStairs1), 0xCDBD78, 0x10, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsState1, Enemies.RoomEnemyPopulationDefinitions.MagnetStairs1, Enemies.RoomEnemyGraphicsSetDefinitions.MagnetStairs1, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, Plms.RoomPlmPopulationDefinitions.MagnetStairsState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);

    /// <summary>The disassembly's RoomState_DeadScientist_0.</summary>
    public static CartridgeRoomState DeadScientist0 { get; } =
        new(nameof(DeadScientist0), 0xCDBFC9, 0x0F, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistState0, Enemies.RoomEnemyPopulationDefinitions.DeadScientist0, Enemies.RoomEnemyGraphicsSetDefinitions.DeadScientist0, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, Plms.RoomPlmPopulationDefinitions.DeadScientistState01, null, RoomSetupCallback.SpawnCeresHaze);

    /// <summary>The disassembly's RoomState_DeadScientist_1.</summary>
    public static CartridgeRoomState DeadScientist1 { get; } =
        new(nameof(DeadScientist1), 0xCDBFC9, 0x10, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistState1, Enemies.RoomEnemyPopulationDefinitions.DeadScientist1, Enemies.RoomEnemyGraphicsSetDefinitions.DeadScientist1, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, Plms.RoomPlmPopulationDefinitions.DeadScientistState01, null, RoomSetupCallback.SpawnCeresHaze);

    /// <summary>The disassembly's RoomState_58Escape_0.</summary>
    public static CartridgeRoomState Room58Escape0 { get; } =
        new(nameof(Room58Escape0), 0xCDC330, 0x0F, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Room58EscapeState0, Enemies.RoomEnemyPopulationDefinitions.Room58Escape0, Enemies.RoomEnemyGraphicsSetDefinitions.Room58Escape0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, Plms.RoomPlmPopulationDefinitions.Room58EscapeState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);

    /// <summary>The disassembly's RoomState_58Escape_1.</summary>
    public static CartridgeRoomState Room58Escape1 { get; } =
        new(nameof(Room58Escape1), 0xCDC330, 0x10, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Room58EscapeState1, Enemies.RoomEnemyPopulationDefinitions.Room58Escape1, Enemies.RoomEnemyGraphicsSetDefinitions.Room58Escape1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, Plms.RoomPlmPopulationDefinitions.Room58EscapeState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);

    /// <summary>The disassembly's RoomState_CeresRidley_0.</summary>
    public static CartridgeRoomState CeresRidley0 { get; } =
        new(nameof(CeresRidley0), 0xCDC43F, 0x13, 0x24, 0x80, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyState0, Enemies.RoomEnemyPopulationDefinitions.CeresRidley0, Enemies.RoomEnemyGraphicsSetDefinitions.CeresRidley0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.HandleCeresRidleyGetawayCutscene, Plms.RoomPlmPopulationDefinitions.CeresRidleyState01, null, RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze);

    /// <summary>The disassembly's RoomState_CeresRidley_1.</summary>
    public static CartridgeRoomState CeresRidley1 { get; } =
        new(nameof(CeresRidley1), 0xCDC43F, 0x14, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyState1, Enemies.RoomEnemyPopulationDefinitions.CeresRidley1, Enemies.RoomEnemyGraphicsSetDefinitions.CeresRidley1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.HandleCeresRidleyGetawayCutscene, Plms.RoomPlmPopulationDefinitions.CeresRidleyState01, null, RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze);

    /// <summary>Every retail room state, in native order.</summary>
    public static IReadOnlyList<CartridgeRoomState> All { get; } =
    [
        LandingSite0,
        LandingSite1,
        LandingSite2,
        LandingSite3,
        GauntletEntrance0,
        GauntletEntrance1,
        Parlor0,
        Parlor1,
        Parlor2,
        CrateriaPowerBombs,
        CrateriaSave,
        WestOcean,
        BowlingAlleyPath,
        CrateriaKihunter,
        ForgottenHighwayElev,
        EastOcean,
        ForgottenHighwayKagos,
        CrabMaze,
        ForgottenHighwayElbow,
        CrateriaTube,
        Moat,
        RedBinstarElev,
        GauntletETank,
        PreBowling,
        Climb0,
        Climb1,
        Climb2,
        Pit0,
        Pit1,
        ElevToBlueBrinstar0,
        ElevToBlueBrinstar1,
        BombTorizo0,
        BombTorizo1,
        BombTorizo2,
        Flyway0,
        Flyway1,
        Flyway2,
        PreMapFlyway,
        Terminator,
        GreenBrinstarElev,
        LowerMushrooms,
        CrateriaMap,
        GreenPiratesShaft,
        CrateriaSuper,
        FinalMissileBombway0,
        FinalMissileBombway1,
        FinalMissile0,
        FinalMissile1,
        GreenBrinstarMainShaft,
        SporeSpawnSuper,
        BrinstarPreMap,
        EarlySupers,
        BrinstarReserveTank,
        GreenBrinstarMap,
        GreenBrinstarFirefleas,
        GreenBrinstarMissileRefill,
        Dachora,
        BigPink,
        SporeSpawnKihunters,
        SporeSpawn0,
        SporeSpawn1,
        PinkBrinstarPowerBombs,
        GreenHillZone,
        MorphBall0,
        MorphBall1,
        ConstructionZone0,
        ConstructionZone1,
        BlueBrinstarETank0,
        BlueBrinstarETank1,
        NoobBridge,
        GreenBrinstarBeetoms,
        EtecoonETank,
        EtecoonSuper,
        DachoraEnergyRefill,
        SporeSpawnFarming,
        WaterwayETank,
        FirstMissile,
        PinkBrinstarHoppers,
        HopperETank,
        BigPinkSaveRoom,
        BlueBrinstarBoulders,
        BillyMays,
        GreenBrinstarSave,
        EtecoonSave,
        RedTower,
        RedBrinstarFirefleas,
        XrayScope,
        Hellway,
        Caterpillar,
        BetaPowerBomb,
        AlphaPowerBomb,
        SkreeBoost,
        BelowSpazer,
        Spazer,
        WarehouseZeela,
        WarehouseETank,
        WarehouseKihunter,
        MiniKraid0,
        MiniKraid1,
        KraidEyeDoor,
        Kraid0,
        Kraid1,
        StatuesHallway,
        RedTowerRefill,
        KraidRefill,
        Statues,
        WarehouseEntrance,
        VariaSuit,
        WarehouseSave,
        RedBrinstarSave,
        IceBeamAcid,
        Cathedral,
        CathedralEntrance,
        BusinessCenter,
        IceBeamGate,
        IceBeamTutorial,
        IceBeam,
        IceBeamSnake,
        CrumbleShaft,
        CrocomireSpeedway,
        Crocomire0,
        Crocomire1,
        HiJumpBoots,
        CrocomireEscape,
        HiJumpETank,
        PostCrocFarming,
        PostCrocSave,
        PostCrocPowerBombs,
        PostCrocShaft,
        PostCrocMissile,
        GrappleTutorial3,
        PostCrocJump,
        GrappleTutorial2,
        GrappleTutorial1,
        GrappleBeam,
        NorfairReserveTank,
        GreenBubblesMissiles,
        BubbleMountain,
        SpeedBoosterHall,
        SpeedBooster,
        SingleChamber,
        DoubleChamber,
        WaveBeam,
        SpikyPlatformsTunnel,
        Volcano,
        KronicBoost,
        MagdolliteTunnel,
        PurpleShaft,
        LavaDive,
        LowerNorfairElev,
        UpperNorfairFarming,
        RisingTide,
        AcidSnakesTunnel,
        SpikyAcidSnakesTunnel,
        UpperNorfairRefill,
        PurpleFarming,
        BatCave,
        NorfairMap,
        BubbleMountainSave,
        FrogSpeedway,
        RedPirateShaft,
        FrogSave,
        CrocomireSave,
        LNElevSave,
        AcidStatue,
        MainHall,
        GoldenTorizo0,
        GoldenTorizo1,
        FastRipper,
        GTEnergyRefill,
        Ridley0,
        Ridley1,
        LNFarming,
        FastPillarsSetup,
        UnusedRoom8FB3EE,
        MickeyMouse,
        Pillar,
        Plowerhouse,
        WorstRoomInTheGame,
        Amphitheatre,
        LNSpringBallMaze,
        LNEscapePowerBombs,
        RedKihunterShaft,
        Wasteland,
        MetalPirates,
        ThreeMusketeers,
        RidleyETank,
        ScrewAttack,
        LNFireflea,
        LNSave,
        BowlingAlley0,
        BowlingAlley1,
        WreckedShipEntrance0,
        WreckedShipEntrance1,
        Attic0,
        Attic1,
        AssemblyLine0,
        AssemblyLine1,
        WreckedShipMainShaft0,
        WreckedShipMainShaft1,
        SpikyDeath0,
        SpikyDeath1,
        ElectricDeath0,
        ElectricDeath1,
        WreckedShipETank0,
        WreckedShipETank1,
        Basement0,
        Basement1,
        WreckedShipMap0,
        WreckedShipMap1,
        Phantoon0,
        Phantoon1,
        SpongeBath0,
        SpongeBath1,
        WSWestSuper0,
        WSWestSuper1,
        WSEastSuper0,
        WSEastSuper1,
        GravitySuit0,
        GravitySuit1,
        WreckedShipSave0,
        WreckedShipSave1,
        GlassTunnelSave,
        GlassTunnel0,
        GlassTunnel1,
        WestTunnel,
        EastTunnel,
        MainStreet,
        FishTank,
        MamaTurtle,
        CrabTunnel,
        MtEverest,
        RedFish,
        WateringHole,
        NWestMaridiaBug,
        CrabShaft,
        PseudoPlasmaSpark,
        CrabHole,
        WestSandHallTunnel,
        PlasmaTutorial,
        Plasma,
        ThreadTheNeedle,
        MaridiaElev,
        PlasmaSpark,
        Kassiuz,
        MaridiaMap,
        ForgottenHighwaySave,
        Toilet,
        BugSandHole,
        WestSandHall,
        Oasis,
        EastSandHall,
        WestSandHole,
        EastSandHole,
        WestAqueductQuicksand,
        EastAqueductQuicksand,
        Aqueduct,
        Butterfly,
        BotwoonHallway,
        Pants,
        EastPants,
        Springball,
        BelowBotwoonETank,
        Colosseum,
        AqueductSave,
        ThePrecious0,
        ThePrecious1,
        BotwoonETank,
        DraygonSave,
        MaridiaMissileRefill,
        PlasmaBeachQuicksand,
        BotwoonQuicksand,
        Shaktool0,
        Shaktool1,
        HalfieClimb,
        Botwoon0,
        Botwoon1,
        SpaceJump,
        MaridiaEnergyRefill,
        WestCactusAlley,
        EastCactusAlley,
        Draygon0,
        Draygon1,
        TourianFirst,
        Metroids10,
        Metroids11,
        Metroids20,
        Metroids21,
        Metroids30,
        Metroids31,
        Metroids40,
        Metroids41,
        BlueHopper0,
        BlueHopper1,
        DustTorizo0,
        DustTorizo1,
        BigBoy0,
        BigBoy1,
        Seaweed,
        TourianRecharge,
        MotherBrain0,
        MotherBrain1,
        MotherBrain2,
        TourianEyeDoor,
        RinkaShaft,
        MotherBrainSave,
        TourianEscape1,
        TourianEscape2,
        TourianEscape3,
        TourianEscape4,
        UpperTourianSave,
        CeresElev0,
        CeresElev1,
        FallingTile0,
        FallingTile1,
        MagnetStairs0,
        MagnetStairs1,
        DeadScientist0,
        DeadScientist1,
        Room58Escape0,
        Room58Escape1,
        CeresRidley0,
        CeresRidley1,
    ];
}
