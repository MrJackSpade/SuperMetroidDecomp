using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Crateria room state, named for their rooms.</summary>
public static class CrateriaRoomStateDefinitions
{
    /// <summary>The Landing Site room's default state.</summary>
    public static RoomStateSettings LandingSiteDefault { get; } =
        new("LandingSiteDefault", RoomLevelDefinitions.LandingSite, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.CrateriaArrival)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteRain, RoomEnemyPopulationDefinitions.LandingSiteBeforeTimebomb, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 2)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainBehavior.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyLandingSite, RoomSetupBehavior.SpawnScrollingSkyBands);


    /// <summary>The Landing Site room's state once Zebes is awake.</summary>
    public static RoomStateSettings LandingSiteWhenZebesAwake { get; } =
        new("LandingSiteWhenZebesAwake", RoomLevelDefinitions.LandingSite, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.CrateriaArrivalZebesAwake)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteRain, RoomEnemyPopulationDefinitions.LandingSiteBeforeTimebomb, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 2)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainBehavior.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyLandingSite, RoomSetupBehavior.SpawnScrollingSkyBands);


    /// <summary>The Landing Site room's state once Samus has Power Bombs.</summary>
    public static RoomStateSettings LandingSiteWithPowerBombs { get; } =
        new("LandingSiteWithPowerBombs", RoomLevelDefinitions.LandingSite, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.SamusTheme)), SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteWithPowerBombs, RoomEnemyPopulationDefinitions.LandingSiteBeforeTimebomb, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 2)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainBehavior.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyLandingSite, RoomSetupBehavior.SpawnScrollingSkyBands);


    /// <summary>The Landing Site room's state once the Zebes time bomb is set.</summary>
    public static RoomStateSettings LandingSiteWhenZebesTimebombSet { get; } =
        new("LandingSiteWhenZebesTimebombSet", RoomLevelDefinitions.LandingSite, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.LandingSiteWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.LandingSiteWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 2)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LandingSite, null, RoomMainBehavior.ScrollingSkyLandWithEscapeExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LandingSiteWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyLandingSite, RoomSetupBehavior.LandSkyWithLandingSiteQuake);


    /// <summary>The Gauntlet Entrance room's default state.</summary>
    public static RoomStateSettings GauntletEntranceDefault { get; } =
        new("GauntletEntranceDefault", RoomLevelDefinitions.GauntletEntrance, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntrance, RoomEnemyPopulationDefinitions.GauntletEntrance, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaHorizontalPatternRocks, RoomSetupBehavior.None);


    /// <summary>The Gauntlet Entrance room's state once Zebes is awake.</summary>
    public static RoomStateSettings GauntletEntranceWhenZebesAwake { get; } =
        new("GauntletEntranceWhenZebesAwake", RoomLevelDefinitions.GauntletEntrance, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.LowerCrateria)), SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEntrance, RoomEnemyPopulationDefinitions.GauntletEntrance, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GauntletEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaHorizontalPatternRocks, RoomSetupBehavior.None);


    /// <summary>The Parlor room's default state.</summary>
    public static RoomStateSettings ParlorDefault { get; } =
        new("ParlorDefault", RoomLevelDefinitions.Parlor, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorDefault, RoomEnemyPopulationDefinitions.ParlorDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ParlorBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.None);


    /// <summary>The Parlor room's state once Zebes is awake.</summary>
    public static RoomStateSettings ParlorWhenZebesAwake { get; } =
        new("ParlorWhenZebesAwake", RoomLevelDefinitions.Parlor, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.LowerCrateria)), SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorWhenZebesAwake, RoomEnemyPopulationDefinitions.ParlorWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ParlorBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.None);


    /// <summary>The Parlor room's state once the Zebes time bomb is set.</summary>
    public static RoomStateSettings ParlorWhenZebesTimebombSet { get; } =
        new("ParlorWhenZebesTimebombSet", RoomLevelDefinitions.Parlor, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ParlorWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.ParlorWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Parlor, null, RoomMainBehavior.ShakeScreenWithRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ParlorWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.OpenRescuedAnimalsPassageAndQuake);


    /// <summary>The Crateria Power Bombs room's state.</summary>
    public static RoomStateSettings CrateriaPowerBombs { get; } =
        new("CrateriaPowerBombs", RoomLevelDefinitions.CrateriaPowerBombs, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaPowerBombs, RoomEnemyPopulationDefinitions.CrateriaPowerBombs, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaPowerBombs, null, RoomMainBehavior.ScrollingSkyLand, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.None);


    /// <summary>The Crateria Save room's state.</summary>
    public static RoomStateSettings CrateriaSave { get; } =
        new("CrateriaSave", RoomLevelDefinitions.SaveRoomDoorOnRight, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.LowerCrateria)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSave, RoomEnemyPopulationDefinitions.CrateriaSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaSave, null, RoomSetupBehavior.None);


    /// <summary>The West Ocean room's state.</summary>
    public static RoomStateSettings WestOcean { get; } =
        new("WestOcean", RoomLevelDefinitions.WestOcean, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.SamusTheme)), SuperMetroid.Core.Fx.RoomFxDefinitions.WestOcean, RoomEnemyPopulationDefinitions.WestOcean, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 2)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestOcean, null, RoomMainBehavior.ScrollingSkyOcean, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyWestOcean, RoomSetupBehavior.SpawnScrollingSkyBands);


    /// <summary>The Bowling Alley Path room's state.</summary>
    public static RoomStateSettings BowlingAlleyPath { get; } =
        new("BowlingAlleyPath", RoomLevelDefinitions.BowlingAlleyPath, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyPath, RoomEnemyPopulationDefinitions.BowlingAlleyPath, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BowlingAlleyPath, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaHorizontalPatternRocks, RoomSetupBehavior.None);


    /// <summary>The Crateria Kihunter room's state.</summary>
    public static RoomStateSettings CrateriaKihunter { get; } =
        new("CrateriaKihunter", RoomLevelDefinitions.CrateriaKihunter, RoomGraphicsSet.UpperCrateria, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaKihunters, RoomEnemyPopulationDefinitions.CrateriaKihunter, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaKihunter, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaKihunter, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.None);


    /// <summary>The Forgotten Highway Elevator room's state.</summary>
    public static RoomStateSettings ForgottenHighwayElevator { get; } =
        new("ForgottenHighwayElevator", RoomLevelDefinitions.ForgottenHighwayElevator, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElevator, RoomEnemyPopulationDefinitions.ForgottenHighwayElevator, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElevator, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwayElevator, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaElevator, RoomSetupBehavior.None);


    /// <summary>The East Ocean room's state.</summary>
    public static RoomStateSettings EastOcean { get; } =
        new("EastOcean", RoomLevelDefinitions.EastOcean, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.SamusTheme)), SuperMetroid.Core.Fx.RoomFxDefinitions.EastOcean, RoomEnemyPopulationDefinitions.EastOcean, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 2)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastOcean, null, RoomMainBehavior.ScrollingSkyOcean, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastOcean, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyEastOcean, RoomSetupBehavior.SpawnScrollingSkyBands);


    /// <summary>The Forgotten Highway Kagos room's state.</summary>
    public static RoomStateSettings ForgottenHighwayKagos { get; } =
        new("ForgottenHighwayKagos", RoomLevelDefinitions.ForgottenHighwayKagos, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayKagos, RoomEnemyPopulationDefinitions.ForgottenHighwayKagos, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllOpen, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwayKagos, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaVerticalPatternRocks, RoomSetupBehavior.None);


    /// <summary>The Crab Maze room's state.</summary>
    public static RoomStateSettings CrabMaze { get; } =
        new("CrabMaze", RoomLevelDefinitions.CrabMaze, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrabMaze, RoomEnemyPopulationDefinitions.CrabMaze, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllOpen, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabMaze, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.None);


    /// <summary>The Forgotten Highway Elbow room's state.</summary>
    public static RoomStateSettings ForgottenHighwayElbow { get; } =
        new("ForgottenHighwayElbow", RoomLevelDefinitions.ForgottenHighwayElbow, RoomGraphicsSet.UpperCrateria, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwayElbow, RoomEnemyPopulationDefinitions.ForgottenHighwayElbow, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ForgottenHighwayElbow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwayElbow, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaVerticalPatternRocks, RoomSetupBehavior.None);


    /// <summary>The Crateria Tube room's state.</summary>
    public static RoomStateSettings CrateriaTube { get; } =
        new("CrateriaTube", RoomLevelDefinitions.CrateriaTube, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaTube, RoomEnemyPopulationDefinitions.CrateriaTube, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaTube, null, RoomSetupBehavior.None);


    /// <summary>The Moat room's state.</summary>
    public static RoomStateSettings Moat { get; } =
        new("Moat", RoomLevelDefinitions.Moat, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Moat, RoomEnemyPopulationDefinitions.Moat, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Moat, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaRocks, RoomSetupBehavior.None);


    /// <summary>The Red Brinstar Elevator room's state.</summary>
    public static RoomStateSettings RedBrinstarElevator { get; } =
        new("RedBrinstarElevator", RoomLevelDefinitions.RedBrinstarElevator, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarElevator, RoomEnemyPopulationDefinitions.RedBrinstarElevator, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedBrinstarElevator, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedBrinstarElevator, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaElevator, RoomSetupBehavior.None);


    /// <summary>The Gauntlet Energy Tank room's state.</summary>
    public static RoomStateSettings GauntletEnergyTank { get; } =
        new("GauntletEnergyTank", RoomLevelDefinitions.GauntletEnergyTank, RoomGraphicsSet.UpperCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GauntletEnergyTank, RoomEnemyPopulationDefinitions.GauntletEnergyTank, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GauntletEnergyTank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GauntletEnergyTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaHorizontalPatternRocks, RoomSetupBehavior.None);


    /// <summary>The Pre Bowling room's state.</summary>
    public static RoomStateSettings PreBowling { get; } =
        new("PreBowling", RoomLevelDefinitions.PreBowling, RoomGraphicsSet.UpperCrateria, new RoomMusic(MusicDataSet.UpperCrateria, RoomMusicTrack.Plays(MusicTrack.SamusTheme)), SuperMetroid.Core.Fx.RoomFxDefinitions.PreBowling, RoomEnemyPopulationDefinitions.PreBowling, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 2)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.ScrollingSkyOcean, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PreBowling, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.ScrollingSkyPreBowling, RoomSetupBehavior.SpawnScrollingSkyBands);


    /// <summary>The Climb room's default state.</summary>
    public static RoomStateSettings ClimbDefault { get; } =
        new("ClimbDefault", RoomLevelDefinitions.Climb, RoomGraphicsSet.OldTourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbDefault, RoomEnemyPopulationDefinitions.ClimbDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ClimbBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.None);


    /// <summary>The Climb room's state once Zebes is awake.</summary>
    public static RoomStateSettings ClimbWhenZebesAwake { get; } =
        new("ClimbWhenZebesAwake", RoomLevelDefinitions.Climb, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.LowerCrateria)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbWhenZebesAwake, RoomEnemyPopulationDefinitions.ClimbWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ClimbBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.None);


    /// <summary>The Climb room's state once the Zebes time bomb is set.</summary>
    public static RoomStateSettings ClimbWhenZebesTimebombSet { get; } =
        new("ClimbWhenZebesTimebombSet", RoomLevelDefinitions.Climb, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.RidleyAndDraygonBattles, RoomMusicTrack.Plays(MusicTrack.Escape)), SuperMetroid.Core.Fx.RoomFxDefinitions.ClimbWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.ClimbWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Climb, null, RoomMainBehavior.ShakeScreenWithRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ClimbWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.AutoDestroyWallDuringEscape);


    /// <summary>The Pit room's default state.</summary>
    public static RoomStateSettings PitDefault { get; } =
        new("PitDefault", RoomLevelDefinitions.Pit, RoomGraphicsSet.OldTourian, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.PitDefault, RoomEnemyPopulationDefinitions.PitDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PitDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.None);


    /// <summary>The Pit room's state once Samus has the Morph Ball and missiles.</summary>
    public static RoomStateSettings PitWithMorphBallAndMissiles { get; } =
        new("PitWithMorphBallAndMissiles", RoomLevelDefinitions.Pit, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.LowerCrateria)), SuperMetroid.Core.Fx.RoomFxDefinitions.PitWithMorphBallAndMissiles, RoomEnemyPopulationDefinitions.PitWithMorphBallAndMissiles, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pit, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PitWithMorphBallAndMissiles, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.None);


    /// <summary>The Elevator To Blue Brinstar room's default state.</summary>
    public static RoomStateSettings ElevatorToBlueBrinstarDefault { get; } =
        new("ElevatorToBlueBrinstarDefault", RoomLevelDefinitions.ElevatorToBlueBrinstar, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.CrateriaArrival)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevatorToBlueBrinstarDefault, RoomEnemyPopulationDefinitions.ElevatorToBlueBrinstar, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevatorToBlueBrinstar, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElevatorToBlueBrinstar, null, RoomSetupBehavior.None);


    /// <summary>The Elevator To Blue Brinstar room's state once Samus has the Morph Ball and missiles.</summary>
    public static RoomStateSettings ElevatorToBlueBrinstarWithMorphBallAndMissiles { get; } =
        new("ElevatorToBlueBrinstarWithMorphBallAndMissiles", RoomLevelDefinitions.ElevatorToBlueBrinstar, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElevatorToBlueBrinstarWithMorphBallAndMissiles, RoomEnemyPopulationDefinitions.ElevatorToBlueBrinstar, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElevatorToBlueBrinstar, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElevatorToBlueBrinstar, null, RoomSetupBehavior.None);


    /// <summary>The Bomb Torizo room's default state.</summary>
    public static RoomStateSettings BombTorizoDefault { get; } =
        new("BombTorizoDefault", RoomLevelDefinitions.BombTorizo, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.RidleyAndDraygonBattles, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizo, RoomEnemyPopulationDefinitions.BombTorizoBeforeTimebomb, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BombTorizoBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.None);


    /// <summary>The Bomb Torizo room's state once the area's Torizo is dead.</summary>
    public static RoomStateSettings BombTorizoWhenAreaTorizoDead { get; } =
        new("BombTorizoWhenAreaTorizoDead", RoomLevelDefinitions.BombTorizo, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizo, RoomEnemyPopulationDefinitions.BombTorizoBeforeTimebomb, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BombTorizoBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.None);


    /// <summary>The Bomb Torizo room's state once the Zebes time bomb is set.</summary>
    public static RoomStateSettings BombTorizoWhenZebesTimebombSet { get; } =
        new("BombTorizoWhenZebesTimebombSet", RoomLevelDefinitions.BombTorizo, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BombTorizoWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.BombTorizoWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinitions.BombTorizoAnimalEscapeBlocks, RoomMainBehavior.ShakeScreenWithRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BombTorizoWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape);


    /// <summary>The Flyway room's default state.</summary>
    public static RoomStateSettings FlywayDefault { get; } =
        new("FlywayDefault", RoomLevelDefinitions.Flyway, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.LowerCrateria)), SuperMetroid.Core.Fx.RoomFxDefinitions.Flyway, RoomEnemyPopulationDefinitions.FlywayBeforeTimebomb, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FlywayBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoomWallArtDark, RoomSetupBehavior.None);


    /// <summary>The Flyway room's state once the area's Torizo is dead.</summary>
    public static RoomStateSettings FlywayWhenAreaTorizoDead { get; } =
        new("FlywayWhenAreaTorizoDead", RoomLevelDefinitions.Flyway, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.LowerCrateria)), SuperMetroid.Core.Fx.RoomFxDefinitions.Flyway, RoomEnemyPopulationDefinitions.FlywayBeforeTimebomb, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FlywayBeforeTimebomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoomWallArtDark, RoomSetupBehavior.None);


    /// <summary>The Flyway room's state once the Zebes time bomb is set.</summary>
    public static RoomStateSettings FlywayWhenZebesTimebombSet { get; } =
        new("FlywayWhenZebesTimebombSet", RoomLevelDefinitions.Flyway, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.RidleyAndDraygonBattles, RoomMusicTrack.Plays(MusicTrack.Escape)), SuperMetroid.Core.Fx.RoomFxDefinitions.FlywayWhenZebesTimebombSet, RoomEnemyPopulationDefinitions.FlywayWhenZebesTimebombSet, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.ShakeScreenWithRandomExplosions, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FlywayWhenZebesTimebombSet, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoomWallArtDark, RoomSetupBehavior.None);


    /// <summary>The Pre Map Flyway room's state.</summary>
    public static RoomStateSettings PreMapFlyway { get; } =
        new("PreMapFlyway", RoomLevelDefinitions.PreMapFlyway, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NoEffect, RoomEnemyPopulationDefinitions.PreMapFlyway, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PreMapFlyway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoomWallArt, RoomSetupBehavior.None);


    /// <summary>The Terminator room's state.</summary>
    public static RoomStateSettings Terminator { get; } =
        new("Terminator", RoomLevelDefinitions.Terminator, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NoEffect, RoomEnemyPopulationDefinitions.Terminator, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Terminator, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaPurpleRocks, RoomSetupBehavior.None);


    /// <summary>The Green Brinstar Elevator room's state.</summary>
    public static RoomStateSettings GreenBrinstarElevator { get; } =
        new("GreenBrinstarElevator", RoomLevelDefinitions.GreenBrinstarElevator, RoomGraphicsSet.LowerCrateria, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.NoEffect, RoomEnemyPopulationDefinitions.GreenBrinstarElevator, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarElevator, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarElevator, null, RoomSetupBehavior.None);


    /// <summary>The Lower Mushrooms room's state.</summary>
    public static RoomStateSettings LowerMushrooms { get; } =
        new("LowerMushrooms", RoomLevelDefinitions.LowerMushrooms, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.NoEffect, RoomEnemyPopulationDefinitions.LowerMushrooms, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LowerMushrooms, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaPurpleRocks, RoomSetupBehavior.None);


    /// <summary>The Crateria Map room's state.</summary>
    public static RoomStateSettings CrateriaMap { get; } =
        new("CrateriaMap", RoomLevelDefinitions.MapStationRoomDoorOnLeft, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NoEffect, RoomEnemyPopulationDefinitions.CrateriaMap, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaMap, null, RoomSetupBehavior.None);


    /// <summary>The Green Pirates Shaft room's state.</summary>
    public static RoomStateSettings GreenPiratesShaft { get; } =
        new("GreenPiratesShaft", RoomLevelDefinitions.GreenPiratesShaft, RoomGraphicsSet.LowerCrateria, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.NoEffect, RoomEnemyPopulationDefinitions.GreenPiratesShaft, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenPiratesShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenPiratesShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoom, RoomSetupBehavior.None);


    /// <summary>The Crateria Super room's state.</summary>
    public static RoomStateSettings CrateriaSuper { get; } =
        new("CrateriaSuper", RoomLevelDefinitions.CrateriaSuper, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrateriaSuper, RoomEnemyPopulationDefinitions.CrateriaSuper, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrateriaSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrateriaSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.None);


    /// <summary>The Final Missile Bombway room's default state.</summary>
    public static RoomStateSettings FinalMissileBombwayDefault { get; } =
        new("FinalMissileBombwayDefault", RoomLevelDefinitions.FinalMissileBombway, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayDefault, RoomEnemyPopulationDefinitions.FinalMissileBombwayDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileBombwayDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoom, RoomSetupBehavior.None);


    /// <summary>The Final Missile Bombway room's state once Zebes is awake.</summary>
    public static RoomStateSettings FinalMissileBombwayWhenZebesAwake { get; } =
        new("FinalMissileBombwayWhenZebesAwake", RoomLevelDefinitions.FinalMissileBombway, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileBombwayWhenZebesAwake, RoomEnemyPopulationDefinitions.FinalMissileBombwayWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissileBombway, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileBombwayWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaBrickRoom, RoomSetupBehavior.None);


    /// <summary>The Final Missile room's default state.</summary>
    public static RoomStateSettings FinalMissileDefault { get; } =
        new("FinalMissileDefault", RoomLevelDefinitions.FinalMissile, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileDefault, RoomEnemyPopulationDefinitions.FinalMissileDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.None);


    /// <summary>The Final Missile room's state once Zebes is awake.</summary>
    public static RoomStateSettings FinalMissileWhenZebesAwake { get; } =
        new("FinalMissileWhenZebesAwake", RoomLevelDefinitions.FinalMissile, RoomGraphicsSet.LowerCrateria, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FinalMissileWhenZebesAwake, RoomEnemyPopulationDefinitions.FinalMissileWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FinalMissile, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FinalMissileWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrateriaMechanical, RoomSetupBehavior.None);


    /// <summary>The Statues Hallway room's state.</summary>
    public static RoomStateSettings StatuesHallway { get; } =
        new("StatuesHallway", RoomLevelDefinitions.StatuesHallway, RoomGraphicsSet.StatuesHall, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.StatueHall)), SuperMetroid.Core.Fx.RoomFxDefinitions.StatuesHallway, RoomEnemyPopulationDefinitions.StatuesHallway, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.StatuesHallway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.StatuesHallwayGreyTowerBrick, RoomSetupBehavior.None);


    /// <summary>The Statues room's state.</summary>
    public static RoomStateSettings Statues { get; } =
        new("Statues", RoomLevelDefinitions.Statues, RoomGraphicsSet.MapStatues, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.TourianStatues)), SuperMetroid.Core.Fx.RoomFxDefinitions.Statues, RoomEnemyPopulationDefinitions.Statues, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 2)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Statues, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Statues, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianStatues, RoomSetupBehavior.RunStatueUnlockingAnimations);
}
