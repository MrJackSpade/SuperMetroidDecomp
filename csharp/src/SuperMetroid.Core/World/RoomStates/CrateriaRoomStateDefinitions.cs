using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Crateria room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class CrateriaRoomStateDefinitions
{
    /// <summary>The Landing Site room's state 0.</summary>
    public static RoomStateSettings LandingSiteDefault { get; } =
        new(nameof(LandingSiteDefault), RoomLevelDefinitions.LandingSite, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteDefaultOrWhenZebesAwake, RoomEnemyPopulationDefinitions.LandingSite012, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainBehavior.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyLandingSite, RoomSetupBehavior.ScrollingSkyLand);


    /// <summary>The Landing Site room's state 1.</summary>
    public static RoomStateSettings LandingSiteWhenZebesAwake { get; } =
        new(nameof(LandingSiteWhenZebesAwake), RoomLevelDefinitions.LandingSite, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteDefaultOrWhenZebesAwake, RoomEnemyPopulationDefinitions.LandingSite012, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainBehavior.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyLandingSite, RoomSetupBehavior.ScrollingSkyLand);


    /// <summary>The Landing Site room's state 2.</summary>
    public static RoomStateSettings LandingSiteWithPowerBombs { get; } =
        new(nameof(LandingSiteWithPowerBombs), RoomLevelDefinitions.LandingSite, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteWithPowerBombs, RoomEnemyPopulationDefinitions.LandingSite012, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainBehavior.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyLandingSite, RoomSetupBehavior.ScrollingSkyLand);


    /// <summary>The Landing Site room's state 3.</summary>
    public static RoomStateSettings LandingSiteWhenZebesTimebombSet { get; } =
        new(nameof(LandingSiteWhenZebesTimebombSet), RoomLevelDefinitions.LandingSite, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.LandingSiteWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainBehavior.ScrollingSkyLandZebesTimebombSet, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyLandingSite, RoomSetupBehavior.ShakeScreenAndCallScrollingSkyLandDuringEscape);


    /// <summary>The Gauntlet Entrance room's state 0.</summary>
    public static RoomStateSettings GauntletEntranceDefault { get; } =
        new(nameof(GauntletEntranceDefault), RoomLevelDefinitions.GauntletEntrance, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntrance, RoomEnemyPopulationDefinitions.GauntletEntrance, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaHorizontalPatternRocks, RoomSetupBehavior.Return);


    /// <summary>The Gauntlet Entrance room's state 1.</summary>
    public static RoomStateSettings GauntletEntranceWhenZebesAwake { get; } =
        new(nameof(GauntletEntranceWhenZebesAwake), RoomLevelDefinitions.GauntletEntrance, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntrance, RoomEnemyPopulationDefinitions.GauntletEntrance, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaHorizontalPatternRocks, RoomSetupBehavior.Return);


    /// <summary>The Parlor room's state 0.</summary>
    public static RoomStateSettings ParlorDefault { get; } =
        new(nameof(ParlorDefault), RoomLevelDefinitions.Parlor, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorDefault, RoomEnemyPopulationDefinitions.ParlorDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ParlorDefaultOrWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.Return);


    /// <summary>The Parlor room's state 1.</summary>
    public static RoomStateSettings ParlorWhenZebesAwake { get; } =
        new(nameof(ParlorWhenZebesAwake), RoomLevelDefinitions.Parlor, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorWhenZebesAwake, RoomEnemyPopulationDefinitions.ParlorWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ParlorDefaultOrWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.ReturnBeforeEscapeSkySetup);


    /// <summary>The Parlor room's state 2.</summary>
    public static RoomStateSettings ParlorWhenZebesTimebombSet { get; } =
        new(nameof(ParlorWhenZebesTimebombSet), RoomLevelDefinitions.Parlor, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.ParlorWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainBehavior.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ParlorWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.ClearBlocksAfterSavingAnimalsAndShakeScreen);


    /// <summary>The Crateria Power Bombs room's state.</summary>
    public static RoomStateSettings CrateriaPowerBombs { get; } =
        new(nameof(CrateriaPowerBombs), RoomLevelDefinitions.CrateriaPowerBombs, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaPowerBombs, RoomEnemyPopulationDefinitions.CrateriaPowerBombs, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaPowerBomb, null, RoomMainBehavior.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.Return);


    /// <summary>The Crateria Save room's state.</summary>
    public static RoomStateSettings CrateriaSave { get; } =
        new(nameof(CrateriaSave), RoomLevelDefinitions.CrateriaSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSave, RoomEnemyPopulationDefinitions.CrateriaSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaSave, null, RoomSetupBehavior.Return);


    /// <summary>The West Ocean room's state.</summary>
    public static RoomStateSettings WestOcean { get; } =
        new(nameof(WestOcean), RoomLevelDefinitions.WestOcean, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.WestOcean, RoomEnemyPopulationDefinitions.WestOcean, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestOcean, null, RoomMainBehavior.ScrollingSkyOcean, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyWreckedShipEntrance, RoomSetupBehavior.ScrollingSkyOcean);


    /// <summary>The Bowling Alley Path room's state.</summary>
    public static RoomStateSettings BowlingAlleyPath { get; } =
        new(nameof(BowlingAlleyPath), RoomLevelDefinitions.BowlingAlleyPath, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyPath, RoomEnemyPopulationDefinitions.BowlingAlleyPath, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BowlingAlleyPath, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaHorizontalPatternRocks, RoomSetupBehavior.Return);


    /// <summary>The Crateria Kihunter room's state.</summary>
    public static RoomStateSettings CrateriaKihunter { get; } =
        new(nameof(CrateriaKihunter), RoomLevelDefinitions.CrateriaKihunter, RoomGraphicsSet.UpperCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaKihunters, RoomEnemyPopulationDefinitions.CrateriaKihunter, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaKihunter, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaKihunter, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.Return);


    /// <summary>The Forgotten Highway Elev room's state.</summary>
    public static RoomStateSettings ForgottenHighwayElev { get; } =
        new(nameof(ForgottenHighwayElev), RoomLevelDefinitions.ForgottenHighwayElev, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElev, RoomEnemyPopulationDefinitions.ForgottenHighwayElev, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElev, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwayElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaElevator, RoomSetupBehavior.Return);


    /// <summary>The East Ocean room's state.</summary>
    public static RoomStateSettings EastOcean { get; } =
        new(nameof(EastOcean), RoomLevelDefinitions.EastOcean, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.EastOcean, RoomEnemyPopulationDefinitions.EastOcean, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastOcean, null, RoomMainBehavior.ScrollingSkyOcean, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyEastOcean, RoomSetupBehavior.ScrollingSkyOcean);


    /// <summary>The Forgotten Highway Kagos room's state.</summary>
    public static RoomStateSettings ForgottenHighwayKagos { get; } =
        new(nameof(ForgottenHighwayKagos), RoomLevelDefinitions.ForgottenHighwayKagos, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayKagos, RoomEnemyPopulationDefinitions.ForgottenHighwayKagos, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwayKagos, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaVerticalPatternRocks, RoomSetupBehavior.Return);


    /// <summary>The Crab Maze room's state.</summary>
    public static RoomStateSettings CrabMaze { get; } =
        new(nameof(CrabMaze), RoomLevelDefinitions.CrabMaze, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrabMaze, RoomEnemyPopulationDefinitions.CrabMaze, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabMaze, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.Return);


    /// <summary>The Forgotten Highway Elbow room's state.</summary>
    public static RoomStateSettings ForgottenHighwayElbow { get; } =
        new(nameof(ForgottenHighwayElbow), RoomLevelDefinitions.ForgottenHighwayElbow, RoomGraphicsSet.UpperCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElbow, RoomEnemyPopulationDefinitions.ForgottenHighwayElbow, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElbow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwayElbow, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaVerticalPatternRocks, RoomSetupBehavior.Return);


    /// <summary>The Crateria Tube room's state.</summary>
    public static RoomStateSettings CrateriaTube { get; } =
        new(nameof(CrateriaTube), RoomLevelDefinitions.CrateriaTube, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaTube, RoomEnemyPopulationDefinitions.CrateriaTube, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaTube, null, RoomSetupBehavior.Return);


    /// <summary>The Moat room's state.</summary>
    public static RoomStateSettings Moat { get; } =
        new(nameof(Moat), RoomLevelDefinitions.Moat, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Moat, RoomEnemyPopulationDefinitions.Moat, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Moat, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.Return);


    /// <summary>The Red Binstar Elev room's state.</summary>
    public static RoomStateSettings RedBinstarElev { get; } =
        new(nameof(RedBinstarElev), RoomLevelDefinitions.RedBinstarElev, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarElev, RoomEnemyPopulationDefinitions.RedBinstarElev, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedBinstarElev, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedBinstarElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaElevator, RoomSetupBehavior.Return);


    /// <summary>The Gauntlet E Tank room's state.</summary>
    public static RoomStateSettings GauntletETank { get; } =
        new(nameof(GauntletETank), RoomLevelDefinitions.GauntletETank, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletETank, RoomEnemyPopulationDefinitions.GauntletETank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GauntletETank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GauntletETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaHorizontalPatternRocks, RoomSetupBehavior.Return);


    /// <summary>The Pre Bowling room's state.</summary>
    public static RoomStateSettings PreBowling { get; } =
        new(nameof(PreBowling), RoomLevelDefinitions.PreBowling, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.PreBowling, RoomEnemyPopulationDefinitions.PreBowling, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.ScrollingSkyOcean, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PreBowling, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyBowlingAlley, RoomSetupBehavior.ScrollingSkyOcean);


    /// <summary>The Climb room's state 0.</summary>
    public static RoomStateSettings ClimbDefault { get; } =
        new(nameof(ClimbDefault), RoomLevelDefinitions.Climb, RoomGraphicsSet.OldTourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbDefault, RoomEnemyPopulationDefinitions.ClimbDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ClimbDefaultOrWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Climb room's state 1.</summary>
    public static RoomStateSettings ClimbWhenZebesAwake { get; } =
        new(nameof(ClimbWhenZebesAwake), RoomLevelDefinitions.Climb, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbWhenZebesAwake, RoomEnemyPopulationDefinitions.ClimbWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ClimbDefaultOrWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.ReturnBeforeEscapeSkySetup);


    /// <summary>The Climb room's state 2.</summary>
    public static RoomStateSettings ClimbWhenZebesTimebombSet { get; } =
        new(nameof(ClimbWhenZebesTimebombSet), RoomLevelDefinitions.Climb, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(MusicTrack.ThirdSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.ClimbWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainBehavior.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ClimbWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.AutoDestroyWallDuringEscape);


    /// <summary>The Pit room's state 0.</summary>
    public static RoomStateSettings PitDefault { get; } =
        new(nameof(PitDefault), RoomLevelDefinitions.Pit, RoomGraphicsSet.OldTourian, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.PitDefault, RoomEnemyPopulationDefinitions.PitDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PitDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Pit room's state 1.</summary>
    public static RoomStateSettings PitWithMorphBallAndMissiles { get; } =
        new(nameof(PitWithMorphBallAndMissiles), RoomLevelDefinitions.Pit, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.PitWithMorphBallAndMissiles, RoomEnemyPopulationDefinitions.PitWithMorphBallAndMissiles, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PitWithMorphBallAndMissiles, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.ReturnBeforeEscapeSkySetup);


    /// <summary>The Elev To Blue Brinstar room's state 0.</summary>
    public static RoomStateSettings ElevToBlueBrinstarDefault { get; } =
        new(nameof(ElevToBlueBrinstarDefault), RoomLevelDefinitions.ElevToBlueBrinstar, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarDefault, RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElevToBlueBrinstar, null, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Elev To Blue Brinstar room's state 1.</summary>
    public static RoomStateSettings ElevToBlueBrinstarWithMorphBallAndMissiles { get; } =
        new(nameof(ElevToBlueBrinstarWithMorphBallAndMissiles), RoomLevelDefinitions.ElevToBlueBrinstar, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevToBlueBrinstarWithMorphBallAndMissiles, RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElevToBlueBrinstar, null, RoomSetupBehavior.ReturnBeforeEscapeSkySetup);


    /// <summary>The Bomb Torizo room's state 0.</summary>
    public static RoomStateSettings BombTorizoDefault { get; } =
        new(nameof(BombTorizoDefault), RoomLevelDefinitions.BombTorizo, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoDefaultOrWhenAreaTorizoDead, RoomEnemyPopulationDefinitions.BombTorizo01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BombTorizoDefaultOrWhenAreaTorizoDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Bomb Torizo room's state 1.</summary>
    public static RoomStateSettings BombTorizoWhenAreaTorizoDead { get; } =
        new(nameof(BombTorizoWhenAreaTorizoDead), RoomLevelDefinitions.BombTorizo, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoDefaultOrWhenAreaTorizoDead, RoomEnemyPopulationDefinitions.BombTorizo01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BombTorizoDefaultOrWhenAreaTorizoDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Bomb Torizo room's state 2.</summary>
    public static RoomStateSettings BombTorizoWhenZebesTimebombSet { get; } =
        new(nameof(BombTorizoWhenZebesTimebombSet), RoomLevelDefinitions.BombTorizo, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.BombTorizoWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinitions.BombTorizoAnimalEscapeBlocks, RoomMainBehavior.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BombTorizoWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape);


    /// <summary>The Flyway room's state 0.</summary>
    public static RoomStateSettings FlywayDefault { get; } =
        new(nameof(FlywayDefault), RoomLevelDefinitions.Flyway, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayDefaultOrWhenAreaTorizoDead, RoomEnemyPopulationDefinitions.Flyway01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FlywayDefaultOrWhenAreaTorizoDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoomWallArtDark, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Flyway room's state 1.</summary>
    public static RoomStateSettings FlywayWhenAreaTorizoDead { get; } =
        new(nameof(FlywayWhenAreaTorizoDead), RoomLevelDefinitions.Flyway, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayDefaultOrWhenAreaTorizoDead, RoomEnemyPopulationDefinitions.Flyway01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FlywayDefaultOrWhenAreaTorizoDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoomWallArtDark, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Flyway room's state 2.</summary>
    public static RoomStateSettings FlywayWhenZebesTimebombSet { get; } =
        new(nameof(FlywayWhenZebesTimebombSet), RoomLevelDefinitions.Flyway, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(MusicTrack.ThirdSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.FlywayWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.SetScreenShakingAndGenerateRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FlywayWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoomWallArtDark, RoomSetupBehavior.ReturnAfterEscapeWallSetup);


    /// <summary>The Pre Map Flyway room's state.</summary>
    public static RoomStateSettings PreMapFlyway { get; } =
        new(nameof(PreMapFlyway), RoomLevelDefinitions.PreMapFlyway, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.PreMapFlyway, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PreMapFlyway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoomWallArt, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Terminator room's state.</summary>
    public static RoomStateSettings Terminator { get; } =
        new(nameof(Terminator), RoomLevelDefinitions.Terminator, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.Terminator, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Terminator, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaPurpleRocks, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Green Brinstar Elev room's state.</summary>
    public static RoomStateSettings GreenBrinstarElev { get; } =
        new(nameof(GreenBrinstarElev), RoomLevelDefinitions.GreenBrinstarElev, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.GreenBrinstarElev, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarElev, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarElev, null, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Lower Mushrooms room's state.</summary>
    public static RoomStateSettings LowerMushrooms { get; } =
        new(nameof(LowerMushrooms), RoomLevelDefinitions.LowerMushrooms, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.LowerMushrooms, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LowerMushrooms, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaPurpleRocks, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Crateria Map room's state.</summary>
    public static RoomStateSettings CrateriaMap { get; } =
        new(nameof(CrateriaMap), RoomLevelDefinitions.CrateriaMap, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.CrateriaMap, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaMap, null, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Green Pirates Shaft room's state.</summary>
    public static RoomStateSettings GreenPiratesShaft { get; } =
        new(nameof(GreenPiratesShaft), RoomLevelDefinitions.GreenPiratesShaft, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedWithoutRecords, RoomEnemyPopulationDefinitions.GreenPiratesShaft, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenPiratesShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenPiratesShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoom, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Crateria Super room's state.</summary>
    public static RoomStateSettings CrateriaSuper { get; } =
        new(nameof(CrateriaSuper), RoomLevelDefinitions.CrateriaSuper, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSuper, RoomEnemyPopulationDefinitions.CrateriaSuper, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Final Missile Bombway room's state 0.</summary>
    public static RoomStateSettings FinalMissileBombwayDefault { get; } =
        new(nameof(FinalMissileBombwayDefault), RoomLevelDefinitions.FinalMissileBombway, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayDefault, RoomEnemyPopulationDefinitions.FinalMissileBombwayDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileBombwayDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoom, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Final Missile Bombway room's state 1.</summary>
    public static RoomStateSettings FinalMissileBombwayWhenZebesAwake { get; } =
        new(nameof(FinalMissileBombwayWhenZebesAwake), RoomLevelDefinitions.FinalMissileBombway, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayWhenZebesAwake, RoomEnemyPopulationDefinitions.FinalMissileBombwayWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileBombwayWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoom, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Final Missile room's state 0.</summary>
    public static RoomStateSettings FinalMissileDefault { get; } =
        new(nameof(FinalMissileDefault), RoomLevelDefinitions.FinalMissile, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileDefault, RoomEnemyPopulationDefinitions.FinalMissileDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Final Missile room's state 1.</summary>
    public static RoomStateSettings FinalMissileWhenZebesAwake { get; } =
        new(nameof(FinalMissileWhenZebesAwake), RoomLevelDefinitions.FinalMissile, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileWhenZebesAwake, RoomEnemyPopulationDefinitions.FinalMissileWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.ReturnAfterOceanSkySetup);


    /// <summary>The Statues Hallway room's state.</summary>
    public static RoomStateSettings StatuesHallway { get; } =
        new(nameof(StatuesHallway), RoomLevelDefinitions.StatuesHallway, RoomGraphicsSet.StatuesHall, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.StatueHall)), SuperMetroid.Core.Fx.RoomFxDefinitions.StatuesHallway, RoomEnemyPopulationDefinitions.StatuesHallway, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.StatuesHallway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.StatuesHallwayGreyTowerBrick, RoomSetupBehavior.ReturnBeforeStatueSetupB);


    /// <summary>The Statues room's state.</summary>
    public static RoomStateSettings Statues { get; } =
        new(nameof(Statues), RoomLevelDefinitions.Statues, RoomGraphicsSet.MapStatues, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Statues, RoomEnemyPopulationDefinitions.Statues, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Statues, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Statues, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianStatues, RoomSetupBehavior.RunStatueUnlockingAnimations);
}
