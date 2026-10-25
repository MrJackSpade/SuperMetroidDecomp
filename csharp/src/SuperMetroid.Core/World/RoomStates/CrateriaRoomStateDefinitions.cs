using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Crateria room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class CrateriaRoomStateDefinitions
{
    /// <summary>The disassembly's RoomState_LandingSite_0.</summary>
    public static CartridgeRoomState LandingSite0 { get; } =
        new(nameof(LandingSite0), RoomLevelDefinitions.LandingSite, 0x00, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState01, Enemies.RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LandingSiteState012, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The disassembly's RoomState_LandingSite_1.</summary>
    public static CartridgeRoomState LandingSite1 { get; } =
        new(nameof(LandingSite1), RoomLevelDefinitions.LandingSite, 0x00, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState01, Enemies.RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LandingSiteState012, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The disassembly's RoomState_LandingSite_2.</summary>
    public static CartridgeRoomState LandingSite2 { get; } =
        new(nameof(LandingSite2), RoomLevelDefinitions.LandingSite, 0x00, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState2, Enemies.RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LandingSiteState012, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The disassembly's RoomState_LandingSite_3.</summary>
    public static CartridgeRoomState LandingSite3 { get; } =
        new(nameof(LandingSite3), RoomLevelDefinitions.LandingSite, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState3, Enemies.RoomEnemyPopulationDefinitions.LandingSite3, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite3, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLandZebesTimebombSet, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LandingSiteState3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape);


    /// <summary>The disassembly's RoomState_GauntletEntrance_0.</summary>
    public static CartridgeRoomState GauntletEntrance0 { get; } =
        new(nameof(GauntletEntrance0), RoomLevelDefinitions.GauntletEntrance, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntranceState01, Enemies.RoomEnemyPopulationDefinitions.GauntletEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GauntletEntranceState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_GauntletEntrance_1.</summary>
    public static CartridgeRoomState GauntletEntrance1 { get; } =
        new(nameof(GauntletEntrance1), RoomLevelDefinitions.GauntletEntrance, 0x00, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntranceState01, Enemies.RoomEnemyPopulationDefinitions.GauntletEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GauntletEntranceState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_Parlor_0.</summary>
    public static CartridgeRoomState Parlor0 { get; } =
        new(nameof(Parlor0), RoomLevelDefinitions.Parlor, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorState0, Enemies.RoomEnemyPopulationDefinitions.Parlor0, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ParlorState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_Parlor_1.</summary>
    public static CartridgeRoomState Parlor1 { get; } =
        new(nameof(Parlor1), RoomLevelDefinitions.Parlor, 0x00, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorState1, Enemies.RoomEnemyPopulationDefinitions.Parlor1, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ParlorState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_Parlor_2.</summary>
    public static CartridgeRoomState Parlor2 { get; } =
        new(nameof(Parlor2), RoomLevelDefinitions.Parlor, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorState2, Enemies.RoomEnemyPopulationDefinitions.Parlor2, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ParlorState2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen);


    /// <summary>The disassembly's RoomState_CrateriaPowerBombs.</summary>
    public static CartridgeRoomState CrateriaPowerBombs { get; } =
        new(nameof(CrateriaPowerBombs), RoomLevelDefinitions.CrateriaPowerBombs, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaPowerBombs, Enemies.RoomEnemyPopulationDefinitions.CrateriaPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaPowerBombs, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaPowerBomb, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrateriaSave.</summary>
    public static CartridgeRoomState CrateriaSave { get; } =
        new(nameof(CrateriaSave), RoomLevelDefinitions.CrateriaSave, 0x19, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSave, Enemies.RoomEnemyPopulationDefinitions.CrateriaSave, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaSave, null, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_WestOcean.</summary>
    public static CartridgeRoomState WestOcean { get; } =
        new(nameof(WestOcean), RoomLevelDefinitions.WestOcean, 0x00, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.WestOcean, Enemies.RoomEnemyPopulationDefinitions.WestOcean, Enemies.RoomEnemyGraphicsSetDefinitions.WestOcean, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestOcean, null, RoomMainCallback.ScrollingSkyOcean, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WestOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsWreckedShipEntrance, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The disassembly's RoomState_BowlingAlleyPath.</summary>
    public static CartridgeRoomState BowlingAlleyPath { get; } =
        new(nameof(BowlingAlleyPath), RoomLevelDefinitions.BowlingAlleyPath, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyPath, Enemies.RoomEnemyPopulationDefinitions.BowlingAlleyPath, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlleyPath, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BowlingAlleyPath, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrateriaKihunter.</summary>
    public static CartridgeRoomState CrateriaKihunter { get; } =
        new(nameof(CrateriaKihunter), RoomLevelDefinitions.CrateriaKihunter, 0x00, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaKihunters, Enemies.RoomEnemyPopulationDefinitions.CrateriaKihunter, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaKihunter, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaKihunter, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaKihunter, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_ForgottenHighwayElev.</summary>
    public static CartridgeRoomState ForgottenHighwayElev { get; } =
        new(nameof(ForgottenHighwayElev), RoomLevelDefinitions.ForgottenHighwayElev, 0x02, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElev, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwayElev, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElev, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ForgottenHighwayElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2Elevator, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_EastOcean.</summary>
    public static CartridgeRoomState EastOcean { get; } =
        new(nameof(EastOcean), RoomLevelDefinitions.EastOcean, 0x00, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.EastOcean, Enemies.RoomEnemyPopulationDefinitions.EastOcean, Enemies.RoomEnemyGraphicsSetDefinitions.EastOcean, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastOcean, null, RoomMainCallback.ScrollingSkyOcean, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EastOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsEastOcean, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The disassembly's RoomState_ForgottenHighwayKagos.</summary>
    public static CartridgeRoomState ForgottenHighwayKagos { get; } =
        new(nameof(ForgottenHighwayKagos), RoomLevelDefinitions.ForgottenHighwayKagos, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayKagos, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwayKagos, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayKagos, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ForgottenHighwayKagos, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0VerticalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrabMaze.</summary>
    public static CartridgeRoomState CrabMaze { get; } =
        new(nameof(CrabMaze), RoomLevelDefinitions.CrabMaze, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrabMaze, Enemies.RoomEnemyPopulationDefinitions.CrabMaze, Enemies.RoomEnemyGraphicsSetDefinitions.CrabMaze, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrabMaze, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_ForgottenHighwayElbow.</summary>
    public static CartridgeRoomState ForgottenHighwayElbow { get; } =
        new(nameof(ForgottenHighwayElbow), RoomLevelDefinitions.ForgottenHighwayElbow, 0x00, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElbow, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwayElbow, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElbow, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElbow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ForgottenHighwayElbow, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0VerticalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrateriaTube.</summary>
    public static CartridgeRoomState CrateriaTube { get; } =
        new(nameof(CrateriaTube), RoomLevelDefinitions.CrateriaTube, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaTube, Enemies.RoomEnemyPopulationDefinitions.CrateriaTube, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaTube, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaTube, null, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_Moat.</summary>
    public static CartridgeRoomState Moat { get; } =
        new(nameof(Moat), RoomLevelDefinitions.Moat, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Moat, Enemies.RoomEnemyPopulationDefinitions.Moat, Enemies.RoomEnemyGraphicsSetDefinitions.Moat, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Moat, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_RedBinstarElev.</summary>
    public static CartridgeRoomState RedBinstarElev { get; } =
        new(nameof(RedBinstarElev), RoomLevelDefinitions.RedBinstarElev, 0x02, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarElev, Enemies.RoomEnemyPopulationDefinitions.RedBinstarElev, Enemies.RoomEnemyGraphicsSetDefinitions.RedBinstarElev, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedBinstarElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedBinstarElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2Elevator, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_GauntletETank.</summary>
    public static CartridgeRoomState GauntletETank { get; } =
        new(nameof(GauntletETank), RoomLevelDefinitions.GauntletETank, 0x00, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletETank, Enemies.RoomEnemyPopulationDefinitions.GauntletETank, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GauntletETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GauntletETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_PreBowling.</summary>
    public static CartridgeRoomState PreBowling { get; } =
        new(nameof(PreBowling), RoomLevelDefinitions.PreBowling, 0x00, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PreBowling, Enemies.RoomEnemyPopulationDefinitions.PreBowling, Enemies.RoomEnemyGraphicsSetDefinitions.PreBowling, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.ScrollingSkyOcean, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PreBowling, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.ScrollingSkyTilemapsBowlingAlley, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The disassembly's RoomState_Climb_0.</summary>
    public static CartridgeRoomState Climb0 { get; } =
        new(nameof(Climb0), RoomLevelDefinitions.Climb, 0x03, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbState0, Enemies.RoomEnemyPopulationDefinitions.Climb0, Enemies.RoomEnemyGraphicsSetDefinitions.Climb0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ClimbState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Climb_1.</summary>
    public static CartridgeRoomState Climb1 { get; } =
        new(nameof(Climb1), RoomLevelDefinitions.Climb, 0x02, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbState1, Enemies.RoomEnemyPopulationDefinitions.Climb1, Enemies.RoomEnemyGraphicsSetDefinitions.Climb1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ClimbState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_Climb_2.</summary>
    public static CartridgeRoomState Climb2 { get; } =
        new(nameof(Climb2), RoomLevelDefinitions.Climb, 0x02, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(7)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbState2, Enemies.RoomEnemyPopulationDefinitions.Climb2, Enemies.RoomEnemyGraphicsSetDefinitions.Climb2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ClimbState2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.AutoDestroyWallDuringEscape);


    /// <summary>The disassembly's RoomState_Pit_0.</summary>
    public static CartridgeRoomState Pit0 { get; } =
        new(nameof(Pit0), RoomLevelDefinitions.Pit, 0x03, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PitState0, Enemies.RoomEnemyPopulationDefinitions.Pit0, Enemies.RoomEnemyGraphicsSetDefinitions.Pit0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Pit0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Pit_1.</summary>
    public static CartridgeRoomState Pit1 { get; } =
        new(nameof(Pit1), RoomLevelDefinitions.Pit, 0x02, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PitState1, Enemies.RoomEnemyPopulationDefinitions.Pit1, Enemies.RoomEnemyGraphicsSetDefinitions.Pit1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Pit1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_ElevToBlueBrinstar_0.</summary>
    public static CartridgeRoomState ElevToBlueBrinstar0 { get; } =
        new(nameof(ElevToBlueBrinstar0), RoomLevelDefinitions.ElevToBlueBrinstar, 0x02, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarState0, Enemies.RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, Enemies.RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ElevToBlueBrinstarState01, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_ElevToBlueBrinstar_1.</summary>
    public static CartridgeRoomState ElevToBlueBrinstar1 { get; } =
        new(nameof(ElevToBlueBrinstar1), RoomLevelDefinitions.ElevToBlueBrinstar, 0x02, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarState1, Enemies.RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, Enemies.RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ElevToBlueBrinstarState01, null, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_BombTorizo_0.</summary>
    public static CartridgeRoomState BombTorizo0 { get; } =
        new(nameof(BombTorizo0), RoomLevelDefinitions.BombTorizo, 0x02, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoState01, Enemies.RoomEnemyPopulationDefinitions.BombTorizo01, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BombTorizoState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_BombTorizo_1.</summary>
    public static CartridgeRoomState BombTorizo1 { get; } =
        new(nameof(BombTorizo1), RoomLevelDefinitions.BombTorizo, 0x02, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoState01, Enemies.RoomEnemyPopulationDefinitions.BombTorizo01, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BombTorizoState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_BombTorizo_2.</summary>
    public static CartridgeRoomState BombTorizo2 { get; } =
        new(nameof(BombTorizo2), RoomLevelDefinitions.BombTorizo, 0x02, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoState2, Enemies.RoomEnemyPopulationDefinitions.BombTorizo2, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinitions.BombTorizoAnimalEscapeBlocks, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BombTorizoState2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape);


    /// <summary>The disassembly's RoomState_Flyway_0.</summary>
    public static CartridgeRoomState Flyway0 { get; } =
        new(nameof(Flyway0), RoomLevelDefinitions.Flyway, 0x02, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayState01, Enemies.RoomEnemyPopulationDefinitions.Flyway01, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FlywayState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Flyway_1.</summary>
    public static CartridgeRoomState Flyway1 { get; } =
        new(nameof(Flyway1), RoomLevelDefinitions.Flyway, 0x02, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayState01, Enemies.RoomEnemyPopulationDefinitions.Flyway01, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FlywayState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Flyway_2.</summary>
    public static CartridgeRoomState Flyway2 { get; } =
        new(nameof(Flyway2), RoomLevelDefinitions.Flyway, 0x02, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(7)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayState2, Enemies.RoomEnemyPopulationDefinitions.Flyway2, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FlywayState2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterEscapeWallSetup);


    /// <summary>The disassembly's RoomState_PreMapFlyway.</summary>
    public static CartridgeRoomState PreMapFlyway { get; } =
        new(nameof(PreMapFlyway), RoomLevelDefinitions.PreMapFlyway, 0x02, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.PreMapFlyway, Enemies.RoomEnemyGraphicsSetDefinitions.PreMapFlyway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PreMapFlyway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoomWallArt, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Terminator.</summary>
    public static CartridgeRoomState Terminator { get; } =
        new(nameof(Terminator), RoomLevelDefinitions.Terminator, 0x02, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.Terminator, Enemies.RoomEnemyGraphicsSetDefinitions.Terminator, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Terminator, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2PurpleRocks, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_GreenBrinstarElev.</summary>
    public static CartridgeRoomState GreenBrinstarElev { get; } =
        new(nameof(GreenBrinstarElev), RoomLevelDefinitions.GreenBrinstarElev, 0x02, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarElev, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarElev, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBrinstarElev, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_LowerMushrooms.</summary>
    public static CartridgeRoomState LowerMushrooms { get; } =
        new(nameof(LowerMushrooms), RoomLevelDefinitions.LowerMushrooms, 0x02, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.LowerMushrooms, Enemies.RoomEnemyGraphicsSetDefinitions.LowerMushrooms, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LowerMushrooms, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2PurpleRocks, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_CrateriaMap.</summary>
    public static CartridgeRoomState CrateriaMap { get; } =
        new(nameof(CrateriaMap), RoomLevelDefinitions.CrateriaMap, 0x15, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.CrateriaMap, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaMap, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaMap, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_GreenPiratesShaft.</summary>
    public static CartridgeRoomState GreenPiratesShaft { get; } =
        new(nameof(GreenPiratesShaft), RoomLevelDefinitions.GreenPiratesShaft, 0x02, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, Enemies.RoomEnemyPopulationDefinitions.GreenPiratesShaft, Enemies.RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenPiratesShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenPiratesShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_CrateriaSuper.</summary>
    public static CartridgeRoomState CrateriaSuper { get; } =
        new(nameof(CrateriaSuper), RoomLevelDefinitions.CrateriaSuper, 0x02, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSuper, Enemies.RoomEnemyPopulationDefinitions.CrateriaSuper, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaSuper, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissileBombway_0.</summary>
    public static CartridgeRoomState FinalMissileBombway0 { get; } =
        new(nameof(FinalMissileBombway0), RoomLevelDefinitions.FinalMissileBombway, 0x02, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayState0, Enemies.RoomEnemyPopulationDefinitions.FinalMissileBombway0, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissileBombway0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FinalMissileBombwayState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissileBombway_1.</summary>
    public static CartridgeRoomState FinalMissileBombway1 { get; } =
        new(nameof(FinalMissileBombway1), RoomLevelDefinitions.FinalMissileBombway, 0x02, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayState1, Enemies.RoomEnemyPopulationDefinitions.FinalMissileBombway1, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissileBombway1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FinalMissileBombwayState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissile_0.</summary>
    public static CartridgeRoomState FinalMissile0 { get; } =
        new(nameof(FinalMissile0), RoomLevelDefinitions.FinalMissile, 0x02, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileState0, Enemies.RoomEnemyPopulationDefinitions.FinalMissile0, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissile0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FinalMissileState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissile_1.</summary>
    public static CartridgeRoomState FinalMissile1 { get; } =
        new(nameof(FinalMissile1), RoomLevelDefinitions.FinalMissile, 0x02, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileState1, Enemies.RoomEnemyPopulationDefinitions.FinalMissile1, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissile1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FinalMissileState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_StatuesHallway.</summary>
    public static CartridgeRoomState StatuesHallway { get; } =
        new(nameof(StatuesHallway), RoomLevelDefinitions.StatuesHallway, 0x08, new RoomMusic(null, RoomMusicTrack.Plays(4)), SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState1, Enemies.RoomEnemyPopulationDefinitions.StatuesHallway, Enemies.RoomEnemyGraphicsSetDefinitions.StatuesHallway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.StatuesHallway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar8NarrowVerticalTowerBrickGrey0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Statues.</summary>
    public static CartridgeRoomState Statues { get; } =
        new(nameof(Statues), RoomLevelDefinitions.Statues, 0x15, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.Statues, Enemies.RoomEnemyPopulationDefinitions.Statues, Enemies.RoomEnemyGraphicsSetDefinitions.Statues, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Statues, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Statues, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Tourian15Statues, RoomSetupCallback.RunStatueUnlockingAnimations);
}
