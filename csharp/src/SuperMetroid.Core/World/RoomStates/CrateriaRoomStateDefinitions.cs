using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Crateria room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class CrateriaRoomStateDefinitions
{
    /// <summary>The disassembly's RoomState_LandingSite_0.</summary>
    public static RoomStateSettings LandingSite0 { get; } =
        new(nameof(LandingSite0), RoomLevelDefinitions.LandingSite, 0, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState01, RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(129, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LandingSiteState012, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The disassembly's RoomState_LandingSite_1.</summary>
    public static RoomStateSettings LandingSite1 { get; } =
        new(nameof(LandingSite1), RoomLevelDefinitions.LandingSite, 0, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState01, RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(129, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LandingSiteState012, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The disassembly's RoomState_LandingSite_2.</summary>
    public static RoomStateSettings LandingSite2 { get; } =
        new(nameof(LandingSite2), RoomLevelDefinitions.LandingSite, 0, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState2, RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(129, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LandingSiteState012, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The disassembly's RoomState_LandingSite_3.</summary>
    public static RoomStateSettings LandingSite3 { get; } =
        new(nameof(LandingSite3), RoomLevelDefinitions.LandingSite, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteState3, RoomEnemyPopulationDefinitions.LandingSite3, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite3, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(129, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLandZebesTimebombSet, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LandingSiteState3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape);


    /// <summary>The disassembly's RoomState_GauntletEntrance_0.</summary>
    public static RoomStateSettings GauntletEntrance0 { get; } =
        new(nameof(GauntletEntrance0), RoomLevelDefinitions.GauntletEntrance, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntranceState01, RoomEnemyPopulationDefinitions.GauntletEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GauntletEntranceState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_GauntletEntrance_1.</summary>
    public static RoomStateSettings GauntletEntrance1 { get; } =
        new(nameof(GauntletEntrance1), RoomLevelDefinitions.GauntletEntrance, 0, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntranceState01, RoomEnemyPopulationDefinitions.GauntletEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GauntletEntranceState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_Parlor_0.</summary>
    public static RoomStateSettings Parlor0 { get; } =
        new(nameof(Parlor0), RoomLevelDefinitions.Parlor, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorState0, RoomEnemyPopulationDefinitions.Parlor0, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ParlorState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_Parlor_1.</summary>
    public static RoomStateSettings Parlor1 { get; } =
        new(nameof(Parlor1), RoomLevelDefinitions.Parlor, 0, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorState1, RoomEnemyPopulationDefinitions.Parlor1, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ParlorState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_Parlor_2.</summary>
    public static RoomStateSettings Parlor2 { get; } =
        new(nameof(Parlor2), RoomLevelDefinitions.Parlor, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorState2, RoomEnemyPopulationDefinitions.Parlor2, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ParlorState2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen);


    /// <summary>The disassembly's RoomState_CrateriaPowerBombs.</summary>
    public static RoomStateSettings CrateriaPowerBombs { get; } =
        new(nameof(CrateriaPowerBombs), RoomLevelDefinitions.CrateriaPowerBombs, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaPowerBombs, RoomEnemyPopulationDefinitions.CrateriaPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaPowerBombs, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaPowerBomb, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrateriaSave.</summary>
    public static RoomStateSettings CrateriaSave { get; } =
        new(nameof(CrateriaSave), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSave, RoomEnemyPopulationDefinitions.CrateriaSave, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaSave, null, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_WestOcean.</summary>
    public static RoomStateSettings WestOcean { get; } =
        new(nameof(WestOcean), RoomLevelDefinitions.WestOcean, 0, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.WestOcean, RoomEnemyPopulationDefinitions.WestOcean, Enemies.RoomEnemyGraphicsSetDefinitions.WestOcean, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(129, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestOcean, null, RoomMainCallback.ScrollingSkyOcean, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WestOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsWreckedShipEntrance, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The disassembly's RoomState_BowlingAlleyPath.</summary>
    public static RoomStateSettings BowlingAlleyPath { get; } =
        new(nameof(BowlingAlleyPath), RoomLevelDefinitions.BowlingAlleyPath, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyPath, RoomEnemyPopulationDefinitions.BowlingAlleyPath, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlleyPath, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BowlingAlleyPath, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrateriaKihunter.</summary>
    public static RoomStateSettings CrateriaKihunter { get; } =
        new(nameof(CrateriaKihunter), RoomLevelDefinitions.CrateriaKihunter, 0, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaKihunters, RoomEnemyPopulationDefinitions.CrateriaKihunter, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaKihunter, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaKihunter, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaKihunter, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_ForgottenHighwayElev.</summary>
    public static RoomStateSettings ForgottenHighwayElev { get; } =
        new(nameof(ForgottenHighwayElev), RoomLevelDefinitions.ForgottenHighwayElev, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElev, RoomEnemyPopulationDefinitions.ForgottenHighwayElev, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElev, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ForgottenHighwayElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2Elevator, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_EastOcean.</summary>
    public static RoomStateSettings EastOcean { get; } =
        new(nameof(EastOcean), RoomLevelDefinitions.EastOcean, 0, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.EastOcean, RoomEnemyPopulationDefinitions.EastOcean, Enemies.RoomEnemyGraphicsSetDefinitions.EastOcean, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(129, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastOcean, null, RoomMainCallback.ScrollingSkyOcean, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EastOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsEastOcean, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The disassembly's RoomState_ForgottenHighwayKagos.</summary>
    public static RoomStateSettings ForgottenHighwayKagos { get; } =
        new(nameof(ForgottenHighwayKagos), RoomLevelDefinitions.ForgottenHighwayKagos, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayKagos, RoomEnemyPopulationDefinitions.ForgottenHighwayKagos, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayKagos, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ForgottenHighwayKagos, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0VerticalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrabMaze.</summary>
    public static RoomStateSettings CrabMaze { get; } =
        new(nameof(CrabMaze), RoomLevelDefinitions.CrabMaze, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrabMaze, RoomEnemyPopulationDefinitions.CrabMaze, Enemies.RoomEnemyGraphicsSetDefinitions.CrabMaze, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrabMaze, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_ForgottenHighwayElbow.</summary>
    public static RoomStateSettings ForgottenHighwayElbow { get; } =
        new(nameof(ForgottenHighwayElbow), RoomLevelDefinitions.ForgottenHighwayElbow, 0, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElbow, RoomEnemyPopulationDefinitions.ForgottenHighwayElbow, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElbow, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElbow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ForgottenHighwayElbow, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0VerticalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_CrateriaTube.</summary>
    public static RoomStateSettings CrateriaTube { get; } =
        new(nameof(CrateriaTube), RoomLevelDefinitions.CrateriaTube, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaTube, RoomEnemyPopulationDefinitions.CrateriaTube, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaTube, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaTube, null, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_Moat.</summary>
    public static RoomStateSettings Moat { get; } =
        new(nameof(Moat), RoomLevelDefinitions.Moat, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Moat, RoomEnemyPopulationDefinitions.Moat, Enemies.RoomEnemyGraphicsSetDefinitions.Moat, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Moat, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_RedBinstarElev.</summary>
    public static RoomStateSettings RedBinstarElev { get; } =
        new(nameof(RedBinstarElev), RoomLevelDefinitions.RedBinstarElev, 2, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarElev, RoomEnemyPopulationDefinitions.RedBinstarElev, Enemies.RoomEnemyGraphicsSetDefinitions.RedBinstarElev, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedBinstarElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedBinstarElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2Elevator, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_GauntletETank.</summary>
    public static RoomStateSettings GauntletETank { get; } =
        new(nameof(GauntletETank), RoomLevelDefinitions.GauntletETank, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletETank, RoomEnemyPopulationDefinitions.GauntletETank, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GauntletETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GauntletETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The disassembly's RoomState_PreBowling.</summary>
    public static RoomStateSettings PreBowling { get; } =
        new(nameof(PreBowling), RoomLevelDefinitions.PreBowling, 0, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PreBowling, RoomEnemyPopulationDefinitions.PreBowling, Enemies.RoomEnemyGraphicsSetDefinitions.PreBowling, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(129, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.ScrollingSkyOcean, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PreBowling, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsBowlingAlley, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The disassembly's RoomState_Climb_0.</summary>
    public static RoomStateSettings Climb0 { get; } =
        new(nameof(Climb0), RoomLevelDefinitions.Climb, 3, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbState0, RoomEnemyPopulationDefinitions.Climb0, Enemies.RoomEnemyGraphicsSetDefinitions.Climb0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ClimbState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Climb_1.</summary>
    public static RoomStateSettings Climb1 { get; } =
        new(nameof(Climb1), RoomLevelDefinitions.Climb, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbState1, RoomEnemyPopulationDefinitions.Climb1, Enemies.RoomEnemyGraphicsSetDefinitions.Climb1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ClimbState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_Climb_2.</summary>
    public static RoomStateSettings Climb2 { get; } =
        new(nameof(Climb2), RoomLevelDefinitions.Climb, 2, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(7)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbState2, RoomEnemyPopulationDefinitions.Climb2, Enemies.RoomEnemyGraphicsSetDefinitions.Climb2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ClimbState2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.AutoDestroyWallDuringEscape);


    /// <summary>The disassembly's RoomState_Pit_0.</summary>
    public static RoomStateSettings Pit0 { get; } =
        new(nameof(Pit0), RoomLevelDefinitions.Pit, 3, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PitState0, RoomEnemyPopulationDefinitions.Pit0, Enemies.RoomEnemyGraphicsSetDefinitions.Pit0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Pit0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Pit_1.</summary>
    public static RoomStateSettings Pit1 { get; } =
        new(nameof(Pit1), RoomLevelDefinitions.Pit, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PitState1, RoomEnemyPopulationDefinitions.Pit1, Enemies.RoomEnemyGraphicsSetDefinitions.Pit1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Pit1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_ElevToBlueBrinstar_0.</summary>
    public static RoomStateSettings ElevToBlueBrinstar0 { get; } =
        new(nameof(ElevToBlueBrinstar0), RoomLevelDefinitions.ElevToBlueBrinstar, 2, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarState0, RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, Enemies.RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ElevToBlueBrinstarState01, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_ElevToBlueBrinstar_1.</summary>
    public static RoomStateSettings ElevToBlueBrinstar1 { get; } =
        new(nameof(ElevToBlueBrinstar1), RoomLevelDefinitions.ElevToBlueBrinstar, 2, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarState1, RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, Enemies.RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ElevToBlueBrinstarState01, null, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_BombTorizo_0.</summary>
    public static RoomStateSettings BombTorizo0 { get; } =
        new(nameof(BombTorizo0), RoomLevelDefinitions.BombTorizo, 2, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoState01, RoomEnemyPopulationDefinitions.BombTorizo01, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BombTorizoState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_BombTorizo_1.</summary>
    public static RoomStateSettings BombTorizo1 { get; } =
        new(nameof(BombTorizo1), RoomLevelDefinitions.BombTorizo, 2, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoState01, RoomEnemyPopulationDefinitions.BombTorizo01, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BombTorizoState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_BombTorizo_2.</summary>
    public static RoomStateSettings BombTorizo2 { get; } =
        new(nameof(BombTorizo2), RoomLevelDefinitions.BombTorizo, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoState2, RoomEnemyPopulationDefinitions.BombTorizo2, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinitions.BombTorizoAnimalEscapeBlocks, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BombTorizoState2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape);


    /// <summary>The disassembly's RoomState_Flyway_0.</summary>
    public static RoomStateSettings Flyway0 { get; } =
        new(nameof(Flyway0), RoomLevelDefinitions.Flyway, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayState01, RoomEnemyPopulationDefinitions.Flyway01, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FlywayState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Flyway_1.</summary>
    public static RoomStateSettings Flyway1 { get; } =
        new(nameof(Flyway1), RoomLevelDefinitions.Flyway, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayState01, RoomEnemyPopulationDefinitions.Flyway01, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FlywayState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Flyway_2.</summary>
    public static RoomStateSettings Flyway2 { get; } =
        new(nameof(Flyway2), RoomLevelDefinitions.Flyway, 2, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(7)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayState2, RoomEnemyPopulationDefinitions.Flyway2, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FlywayState2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterEscapeWallSetup);


    /// <summary>The disassembly's RoomState_PreMapFlyway.</summary>
    public static RoomStateSettings PreMapFlyway { get; } =
        new(nameof(PreMapFlyway), RoomLevelDefinitions.PreMapFlyway, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, RoomEnemyPopulationDefinitions.PreMapFlyway, Enemies.RoomEnemyGraphicsSetDefinitions.PreMapFlyway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PreMapFlyway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoomWallArt, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_Terminator.</summary>
    public static RoomStateSettings Terminator { get; } =
        new(nameof(Terminator), RoomLevelDefinitions.Terminator, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, RoomEnemyPopulationDefinitions.Terminator, Enemies.RoomEnemyGraphicsSetDefinitions.Terminator, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Terminator, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2PurpleRocks, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_GreenBrinstarElev.</summary>
    public static RoomStateSettings GreenBrinstarElev { get; } =
        new(nameof(GreenBrinstarElev), RoomLevelDefinitions.GreenBrinstarElev, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, RoomEnemyPopulationDefinitions.GreenBrinstarElev, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarElev, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBrinstarElev, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_LowerMushrooms.</summary>
    public static RoomStateSettings LowerMushrooms { get; } =
        new(nameof(LowerMushrooms), RoomLevelDefinitions.LowerMushrooms, 2, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, RoomEnemyPopulationDefinitions.LowerMushrooms, Enemies.RoomEnemyGraphicsSetDefinitions.LowerMushrooms, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LowerMushrooms, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2PurpleRocks, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_CrateriaMap.</summary>
    public static RoomStateSettings CrateriaMap { get; } =
        new(nameof(CrateriaMap), RoomLevelDefinitions.CrateriaMap, 21, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, RoomEnemyPopulationDefinitions.CrateriaMap, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaMap, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaMap, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_GreenPiratesShaft.</summary>
    public static RoomStateSettings GreenPiratesShaft { get; } =
        new(nameof(GreenPiratesShaft), RoomLevelDefinitions.GreenPiratesShaft, 2, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381C2, RoomEnemyPopulationDefinitions.GreenPiratesShaft, Enemies.RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(1, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenPiratesShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenPiratesShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_CrateriaSuper.</summary>
    public static RoomStateSettings CrateriaSuper { get; } =
        new(nameof(CrateriaSuper), RoomLevelDefinitions.CrateriaSuper, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSuper, RoomEnemyPopulationDefinitions.CrateriaSuper, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaSuper, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrateriaSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissileBombway_0.</summary>
    public static RoomStateSettings FinalMissileBombway0 { get; } =
        new(nameof(FinalMissileBombway0), RoomLevelDefinitions.FinalMissileBombway, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayState0, RoomEnemyPopulationDefinitions.FinalMissileBombway0, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissileBombway0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FinalMissileBombwayState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissileBombway_1.</summary>
    public static RoomStateSettings FinalMissileBombway1 { get; } =
        new(nameof(FinalMissileBombway1), RoomLevelDefinitions.FinalMissileBombway, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayState1, RoomEnemyPopulationDefinitions.FinalMissileBombway1, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissileBombway1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FinalMissileBombwayState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissile_0.</summary>
    public static RoomStateSettings FinalMissile0 { get; } =
        new(nameof(FinalMissile0), RoomLevelDefinitions.FinalMissile, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileState0, RoomEnemyPopulationDefinitions.FinalMissile0, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissile0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FinalMissileState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_FinalMissile_1.</summary>
    public static RoomStateSettings FinalMissile1 { get; } =
        new(nameof(FinalMissile1), RoomLevelDefinitions.FinalMissile, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileState1, RoomEnemyPopulationDefinitions.FinalMissile1, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissile1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FinalMissileState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The disassembly's RoomState_StatuesHallway.</summary>
    public static RoomStateSettings StatuesHallway { get; } =
        new(nameof(StatuesHallway), RoomLevelDefinitions.StatuesHallway, 8, new RoomMusic(null, RoomMusicTrack.Plays(4)), SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState1, RoomEnemyPopulationDefinitions.StatuesHallway, Enemies.RoomEnemyGraphicsSetDefinitions.StatuesHallway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.StatuesHallway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar8NarrowVerticalTowerBrickGrey0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Statues.</summary>
    public static RoomStateSettings Statues { get; } =
        new(nameof(Statues), RoomLevelDefinitions.Statues, 21, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.Statues, RoomEnemyPopulationDefinitions.Statues, Enemies.RoomEnemyGraphicsSetDefinitions.Statues, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(129, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Statues, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Statues, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Tourian15Statues, RoomSetupCallback.RunStatueUnlockingAnimations);
}
