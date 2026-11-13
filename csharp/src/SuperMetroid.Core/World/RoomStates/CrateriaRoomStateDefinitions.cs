using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Crateria room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class CrateriaRoomStateDefinitions
{
    /// <summary>The Landing Site room's state 0.</summary>
    public static RoomStateSettings LandingSiteDefault { get; } =
        new(nameof(LandingSiteDefault), RoomLevelDefinitions.LandingSite, 0, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteDefaultOrWhenZebesAwake, RoomEnemyPopulationDefinitions.LandingSite012, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The Landing Site room's state 1.</summary>
    public static RoomStateSettings LandingSiteWhenZebesAwake { get; } =
        new(nameof(LandingSiteWhenZebesAwake), RoomLevelDefinitions.LandingSite, 0, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteDefaultOrWhenZebesAwake, RoomEnemyPopulationDefinitions.LandingSite012, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The Landing Site room's state 2.</summary>
    public static RoomStateSettings LandingSiteWithPowerBombs { get; } =
        new(nameof(LandingSiteWithPowerBombs), RoomLevelDefinitions.LandingSite, 0, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteWithPowerBombs, RoomEnemyPopulationDefinitions.LandingSite012, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ScrollingSkyLand);


    /// <summary>The Landing Site room's state 3.</summary>
    public static RoomStateSettings LandingSiteWhenZebesTimebombSet { get; } =
        new(nameof(LandingSiteWhenZebesTimebombSet), RoomLevelDefinitions.LandingSite, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.LandingSiteWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainCallback.ScrollingSkyLandZebesTimebombSet, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsLandingSite, RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape);


    /// <summary>The Gauntlet Entrance room's state 0.</summary>
    public static RoomStateSettings GauntletEntranceDefault { get; } =
        new(nameof(GauntletEntranceDefault), RoomLevelDefinitions.GauntletEntrance, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntrance, RoomEnemyPopulationDefinitions.GauntletEntrance, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The Gauntlet Entrance room's state 1.</summary>
    public static RoomStateSettings GauntletEntranceWhenZebesAwake { get; } =
        new(nameof(GauntletEntranceWhenZebesAwake), RoomLevelDefinitions.GauntletEntrance, 0, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntrance, RoomEnemyPopulationDefinitions.GauntletEntrance, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The Parlor room's state 0.</summary>
    public static RoomStateSettings ParlorDefault { get; } =
        new(nameof(ParlorDefault), RoomLevelDefinitions.Parlor, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorDefault, RoomEnemyPopulationDefinitions.ParlorDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ParlorDefaultOrWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The Parlor room's state 1.</summary>
    public static RoomStateSettings ParlorWhenZebesAwake { get; } =
        new(nameof(ParlorWhenZebesAwake), RoomLevelDefinitions.Parlor, 0, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorWhenZebesAwake, RoomEnemyPopulationDefinitions.ParlorWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ParlorDefaultOrWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Parlor room's state 2.</summary>
    public static RoomStateSettings ParlorWhenZebesTimebombSet { get; } =
        new(nameof(ParlorWhenZebesTimebombSet), RoomLevelDefinitions.Parlor, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.ParlorWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ParlorWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen);


    /// <summary>The Crateria Power Bombs room's state.</summary>
    public static RoomStateSettings CrateriaPowerBombs { get; } =
        new(nameof(CrateriaPowerBombs), RoomLevelDefinitions.CrateriaPowerBombs, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaPowerBombs, RoomEnemyPopulationDefinitions.CrateriaPowerBombs, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaPowerBomb, null, RoomMainCallback.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The Crateria Save room's state.</summary>
    public static RoomStateSettings CrateriaSave { get; } =
        new(nameof(CrateriaSave), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSave, RoomEnemyPopulationDefinitions.CrateriaSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaSave, null, RoomSetupCallback.Return);


    /// <summary>The West Ocean room's state.</summary>
    public static RoomStateSettings WestOcean { get; } =
        new(nameof(WestOcean), RoomLevelDefinitions.WestOcean, 0, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.WestOcean, RoomEnemyPopulationDefinitions.WestOcean, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestOcean, null, RoomMainCallback.ScrollingSkyOcean, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsWreckedShipEntrance, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The Bowling Alley Path room's state.</summary>
    public static RoomStateSettings BowlingAlleyPath { get; } =
        new(nameof(BowlingAlleyPath), RoomLevelDefinitions.BowlingAlleyPath, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyPath, RoomEnemyPopulationDefinitions.BowlingAlleyPath, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BowlingAlleyPath, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The Crateria Kihunter room's state.</summary>
    public static RoomStateSettings CrateriaKihunter { get; } =
        new(nameof(CrateriaKihunter), RoomLevelDefinitions.CrateriaKihunter, 0, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaKihunters, RoomEnemyPopulationDefinitions.CrateriaKihunter, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaKihunter, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaKihunter, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The Forgotten Highway Elev room's state.</summary>
    public static RoomStateSettings ForgottenHighwayElev { get; } =
        new(nameof(ForgottenHighwayElev), RoomLevelDefinitions.ForgottenHighwayElev, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElev, RoomEnemyPopulationDefinitions.ForgottenHighwayElev, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwayElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2Elevator, RoomSetupCallback.Return);


    /// <summary>The East Ocean room's state.</summary>
    public static RoomStateSettings EastOcean { get; } =
        new(nameof(EastOcean), RoomLevelDefinitions.EastOcean, 0, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.EastOcean, RoomEnemyPopulationDefinitions.EastOcean, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastOcean, null, RoomMainCallback.ScrollingSkyOcean, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsEastOcean, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The Forgotten Highway Kagos room's state.</summary>
    public static RoomStateSettings ForgottenHighwayKagos { get; } =
        new(nameof(ForgottenHighwayKagos), RoomLevelDefinitions.ForgottenHighwayKagos, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayKagos, RoomEnemyPopulationDefinitions.ForgottenHighwayKagos, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwayKagos, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0VerticalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The Crab Maze room's state.</summary>
    public static RoomStateSettings CrabMaze { get; } =
        new(nameof(CrabMaze), RoomLevelDefinitions.CrabMaze, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrabMaze, RoomEnemyPopulationDefinitions.CrabMaze, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabMaze, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The Forgotten Highway Elbow room's state.</summary>
    public static RoomStateSettings ForgottenHighwayElbow { get; } =
        new(nameof(ForgottenHighwayElbow), RoomLevelDefinitions.ForgottenHighwayElbow, 0, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElbow, RoomEnemyPopulationDefinitions.ForgottenHighwayElbow, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElbow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwayElbow, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0VerticalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The Crateria Tube room's state.</summary>
    public static RoomStateSettings CrateriaTube { get; } =
        new(nameof(CrateriaTube), RoomLevelDefinitions.CrateriaTube, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaTube, RoomEnemyPopulationDefinitions.CrateriaTube, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaTube, null, RoomSetupCallback.Return);


    /// <summary>The Moat room's state.</summary>
    public static RoomStateSettings Moat { get; } =
        new(nameof(Moat), RoomLevelDefinitions.Moat, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Moat, RoomEnemyPopulationDefinitions.Moat, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Moat, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0Rocks, RoomSetupCallback.Return);


    /// <summary>The Red Binstar Elev room's state.</summary>
    public static RoomStateSettings RedBinstarElev { get; } =
        new(nameof(RedBinstarElev), RoomLevelDefinitions.RedBinstarElev, 2, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarElev, RoomEnemyPopulationDefinitions.RedBinstarElev, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedBinstarElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedBinstarElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2Elevator, RoomSetupCallback.Return);


    /// <summary>The Gauntlet E Tank room's state.</summary>
    public static RoomStateSettings GauntletETank { get; } =
        new(nameof(GauntletETank), RoomLevelDefinitions.GauntletETank, 0, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletETank, RoomEnemyPopulationDefinitions.GauntletETank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GauntletETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GauntletETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria0HorizontalPatternRocks, RoomSetupCallback.Return);


    /// <summary>The Pre Bowling room's state.</summary>
    public static RoomStateSettings PreBowling { get; } =
        new(nameof(PreBowling), RoomLevelDefinitions.PreBowling, 0, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.PreBowling, RoomEnemyPopulationDefinitions.PreBowling, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.ScrollingSkyOcean, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PreBowling, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyTilemapsBowlingAlley, RoomSetupCallback.ScrollingSkyOcean);


    /// <summary>The Climb room's state 0.</summary>
    public static RoomStateSettings ClimbDefault { get; } =
        new(nameof(ClimbDefault), RoomLevelDefinitions.Climb, 3, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbDefault, RoomEnemyPopulationDefinitions.ClimbDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ClimbDefaultOrWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Climb room's state 1.</summary>
    public static RoomStateSettings ClimbWhenZebesAwake { get; } =
        new(nameof(ClimbWhenZebesAwake), RoomLevelDefinitions.Climb, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbWhenZebesAwake, RoomEnemyPopulationDefinitions.ClimbWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ClimbDefaultOrWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Climb room's state 2.</summary>
    public static RoomStateSettings ClimbWhenZebesTimebombSet { get; } =
        new(nameof(ClimbWhenZebesTimebombSet), RoomLevelDefinitions.Climb, 2, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(MusicTrack.ThirdSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.ClimbWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ClimbWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.AutoDestroyWallDuringEscape);


    /// <summary>The Pit room's state 0.</summary>
    public static RoomStateSettings PitDefault { get; } =
        new(nameof(PitDefault), RoomLevelDefinitions.Pit, 3, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.PitDefault, RoomEnemyPopulationDefinitions.PitDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PitDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Pit room's state 1.</summary>
    public static RoomStateSettings PitWithMorphBallAndMissiles { get; } =
        new(nameof(PitWithMorphBallAndMissiles), RoomLevelDefinitions.Pit, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.PitWithMorphBallAndMissiles, RoomEnemyPopulationDefinitions.PitWithMorphBallAndMissiles, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PitWithMorphBallAndMissiles, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Elev To Blue Brinstar room's state 0.</summary>
    public static RoomStateSettings ElevToBlueBrinstarDefault { get; } =
        new(nameof(ElevToBlueBrinstarDefault), RoomLevelDefinitions.ElevToBlueBrinstar, 2, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarDefault, RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElevToBlueBrinstar, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Elev To Blue Brinstar room's state 1.</summary>
    public static RoomStateSettings ElevToBlueBrinstarWithMorphBallAndMissiles { get; } =
        new(nameof(ElevToBlueBrinstarWithMorphBallAndMissiles), RoomLevelDefinitions.ElevToBlueBrinstar, 2, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarWithMorphBallAndMissiles, RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElevToBlueBrinstar, null, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Bomb Torizo room's state 0.</summary>
    public static RoomStateSettings BombTorizoDefault { get; } =
        new(nameof(BombTorizoDefault), RoomLevelDefinitions.BombTorizo, 2, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoDefaultOrWhenAreaTorizoDead, RoomEnemyPopulationDefinitions.BombTorizo01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BombTorizoDefaultOrWhenAreaTorizoDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Bomb Torizo room's state 1.</summary>
    public static RoomStateSettings BombTorizoWhenAreaTorizoDead { get; } =
        new(nameof(BombTorizoWhenAreaTorizoDead), RoomLevelDefinitions.BombTorizo, 2, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoDefaultOrWhenAreaTorizoDead, RoomEnemyPopulationDefinitions.BombTorizo01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BombTorizoDefaultOrWhenAreaTorizoDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Bomb Torizo room's state 2.</summary>
    public static RoomStateSettings BombTorizoWhenZebesTimebombSet { get; } =
        new(nameof(BombTorizoWhenZebesTimebombSet), RoomLevelDefinitions.BombTorizo, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.BombTorizoWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinitions.BombTorizoAnimalEscapeBlocks, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BombTorizoWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape);


    /// <summary>The Flyway room's state 0.</summary>
    public static RoomStateSettings FlywayDefault { get; } =
        new(nameof(FlywayDefault), RoomLevelDefinitions.Flyway, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayDefaultOrWhenAreaTorizoDead, RoomEnemyPopulationDefinitions.Flyway01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FlywayDefaultOrWhenAreaTorizoDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Flyway room's state 1.</summary>
    public static RoomStateSettings FlywayWhenAreaTorizoDead { get; } =
        new(nameof(FlywayWhenAreaTorizoDead), RoomLevelDefinitions.Flyway, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayDefaultOrWhenAreaTorizoDead, RoomEnemyPopulationDefinitions.Flyway01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FlywayDefaultOrWhenAreaTorizoDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Flyway room's state 2.</summary>
    public static RoomStateSettings FlywayWhenZebesTimebombSet { get; } =
        new(nameof(FlywayWhenZebesTimebombSet), RoomLevelDefinitions.Flyway, 2, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(MusicTrack.ThirdSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.FlywayWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FlywayWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoomWallArtDark, RoomSetupCallback.ReturnAfterEscapeWallSetup);


    /// <summary>The Pre Map Flyway room's state.</summary>
    public static RoomStateSettings PreMapFlyway { get; } =
        new(nameof(PreMapFlyway), RoomLevelDefinitions.PreMapFlyway, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.PreMapFlyway, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PreMapFlyway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoomWallArt, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Terminator room's state.</summary>
    public static RoomStateSettings Terminator { get; } =
        new(nameof(Terminator), RoomLevelDefinitions.Terminator, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.Terminator, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Terminator, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2PurpleRocks, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Green Brinstar Elev room's state.</summary>
    public static RoomStateSettings GreenBrinstarElev { get; } =
        new(nameof(GreenBrinstarElev), RoomLevelDefinitions.GreenBrinstarElev, 2, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.GreenBrinstarElev, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarElev, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Lower Mushrooms room's state.</summary>
    public static RoomStateSettings LowerMushrooms { get; } =
        new(nameof(LowerMushrooms), RoomLevelDefinitions.LowerMushrooms, 2, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.LowerMushrooms, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LowerMushrooms, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2PurpleRocks, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Crateria Map room's state.</summary>
    public static RoomStateSettings CrateriaMap { get; } =
        new(nameof(CrateriaMap), RoomLevelDefinitions.CrateriaMap, 21, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.CrateriaMap, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaMap, null, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Green Pirates Shaft room's state.</summary>
    public static RoomStateSettings GreenPiratesShaft { get; } =
        new(nameof(GreenPiratesShaft), RoomLevelDefinitions.GreenPiratesShaft, 2, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.GreenPiratesShaft, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenPiratesShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenPiratesShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Crateria Super room's state.</summary>
    public static RoomStateSettings CrateriaSuper { get; } =
        new(nameof(CrateriaSuper), RoomLevelDefinitions.CrateriaSuper, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSuper, RoomEnemyPopulationDefinitions.CrateriaSuper, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Final Missile Bombway room's state 0.</summary>
    public static RoomStateSettings FinalMissileBombwayDefault { get; } =
        new(nameof(FinalMissileBombwayDefault), RoomLevelDefinitions.FinalMissileBombway, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayDefault, RoomEnemyPopulationDefinitions.FinalMissileBombwayDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileBombwayDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Final Missile Bombway room's state 1.</summary>
    public static RoomStateSettings FinalMissileBombwayWhenZebesAwake { get; } =
        new(nameof(FinalMissileBombwayWhenZebesAwake), RoomLevelDefinitions.FinalMissileBombway, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayWhenZebesAwake, RoomEnemyPopulationDefinitions.FinalMissileBombwayWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileBombwayWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria2BrickRoom, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Final Missile room's state 0.</summary>
    public static RoomStateSettings FinalMissileDefault { get; } =
        new(nameof(FinalMissileDefault), RoomLevelDefinitions.FinalMissile, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileDefault, RoomEnemyPopulationDefinitions.FinalMissileDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Final Missile room's state 1.</summary>
    public static RoomStateSettings FinalMissileWhenZebesAwake { get; } =
        new(nameof(FinalMissileWhenZebesAwake), RoomLevelDefinitions.FinalMissile, 2, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileWhenZebesAwake, RoomEnemyPopulationDefinitions.FinalMissileWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Crateria23Mechanical, RoomSetupCallback.ReturnAfterOceanSkySetup);


    /// <summary>The Statues Hallway room's state.</summary>
    public static RoomStateSettings StatuesHallway { get; } =
        new(nameof(StatuesHallway), RoomLevelDefinitions.StatuesHallway, 8, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.StatueHall)), SuperMetroid.Core.Fx.RoomFxDefinitions.StatuesHallway, RoomEnemyPopulationDefinitions.StatuesHallway, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.StatuesHallway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar8NarrowVerticalTowerBrickGrey0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Statues room's state.</summary>
    public static RoomStateSettings Statues { get; } =
        new(nameof(Statues), RoomLevelDefinitions.Statues, 21, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Statues, RoomEnemyPopulationDefinitions.Statues, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Statues, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Statues, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Tourian15Statues, RoomSetupCallback.RunStatueUnlockingAnimations);
}
