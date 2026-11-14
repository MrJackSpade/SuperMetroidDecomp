using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Maridia room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class MaridiaRoomStateDefinitions
{

    /// <summary>The Glass Tunnel Save room's state.</summary>
    public static RoomStateSettings GlassTunnelSave { get; } =
        new(nameof(GlassTunnelSave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.RedBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnelSave, RoomEnemyPopulationDefinitions.GlassTunnelSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GlassTunnelSave, null, RoomSetupBehavior.None);


    /// <summary>The Glass Tunnel room's default state.</summary>
    public static RoomStateSettings GlassTunnelDefault { get; } =
        new(nameof(GlassTunnelDefault), RoomLevelDefinitions.GlassTunnelDefault, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.RedBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnel, RoomEnemyPopulationDefinitions.GlassTunnel, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GlassTunnel, null, RoomSetupBehavior.None);


    /// <summary>The Glass Tunnel room's state once Maridia's glass tube is broken.</summary>
    public static RoomStateSettings GlassTunnelWhenMaridiaNoobTubeBroken { get; } =
        new(nameof(GlassTunnelWhenMaridiaNoobTubeBroken), RoomLevelDefinitions.GlassTunnelWhenMaridiaNoobTubeBroken, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.RedBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnel, RoomEnemyPopulationDefinitions.GlassTunnel, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GlassTunnel, null, RoomSetupBehavior.None);


    /// <summary>The West Tunnel room's state.</summary>
    public static RoomStateSettings WestTunnel { get; } =
        new(nameof(WestTunnel), RoomLevelDefinitions.WestTunnel, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestTunnel, RoomEnemyPopulationDefinitions.WestTunnel, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestTunnel, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestTunnel, null, RoomSetupBehavior.None);


    /// <summary>The East Tunnel room's state.</summary>
    public static RoomStateSettings EastTunnel { get; } =
        new(nameof(EastTunnel), RoomLevelDefinitions.EastTunnel, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.RedBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.EastTunnel, RoomEnemyPopulationDefinitions.EastTunnel, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastTunnel, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastTunnel, null, RoomSetupBehavior.None);


    /// <summary>The Main Street room's state.</summary>
    public static RoomStateSettings MainStreet { get; } =
        new(nameof(MainStreet), RoomLevelDefinitions.MainStreet, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.WestMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.MainStreet, RoomEnemyPopulationDefinitions.MainStreet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MainStreet, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MainStreet, null, RoomSetupBehavior.None);


    /// <summary>The Fish Tank room's state.</summary>
    public static RoomStateSettings FishTank { get; } =
        new(nameof(FishTank), RoomLevelDefinitions.FishTank, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FishTank, RoomEnemyPopulationDefinitions.FishTank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FishTank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FishTank, null, RoomSetupBehavior.None);


    /// <summary>The Mama Turtle room's state.</summary>
    public static RoomStateSettings MamaTurtle { get; } =
        new(nameof(MamaTurtle), RoomLevelDefinitions.MamaTurtle, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MamaTurtle, RoomEnemyPopulationDefinitions.MamaTurtle, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MamaTurtle, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MamaTurtle, null, RoomSetupBehavior.None);


    /// <summary>The Crab Tunnel room's state.</summary>
    public static RoomStateSettings CrabTunnel { get; } =
        new(nameof(CrabTunnel), RoomLevelDefinitions.CrabTunnel, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.WestMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabTunnel, RoomEnemyPopulationDefinitions.CrabTunnel, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabTunnel, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.MaridiaBrownWall, RoomSetupBehavior.None);


    /// <summary>The Mt Everest room's state.</summary>
    public static RoomStateSettings MtEverest { get; } =
        new(nameof(MtEverest), RoomLevelDefinitions.MtEverest, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MtEverest, RoomEnemyPopulationDefinitions.MtEverest, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MtEverest, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MtEverest, null, RoomSetupBehavior.None);


    /// <summary>The Red Fish room's state.</summary>
    public static RoomStateSettings RedFish { get; } =
        new(nameof(RedFish), RoomLevelDefinitions.RedFish, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.WestMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedFish, RoomEnemyPopulationDefinitions.RedFish, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedFish, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedFish, null, RoomSetupBehavior.None);


    /// <summary>The Watering Hole room's state.</summary>
    public static RoomStateSettings WateringHole { get; } =
        new(nameof(WateringHole), RoomLevelDefinitions.WateringHole, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WateringHole, RoomEnemyPopulationDefinitions.WateringHole, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WateringHole, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WateringHole, null, RoomSetupBehavior.None);


    /// <summary>The N West Maridia Bug room's state.</summary>
    public static RoomStateSettings NWestMaridiaBug { get; } =
        new(nameof(NWestMaridiaBug), RoomLevelDefinitions.NWestMaridiaBug, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NWestMaridiaBug, RoomEnemyPopulationDefinitions.NWestMaridiaBug, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.NWestMaridiaBug, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.NWestMaridiaBug, null, RoomSetupBehavior.None);


    /// <summary>The Crab Shaft room's state.</summary>
    public static RoomStateSettings CrabShaft { get; } =
        new(nameof(CrabShaft), RoomLevelDefinitions.CrabShaft, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.WestMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabShaft, RoomEnemyPopulationDefinitions.CrabShaft, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabShaft, null, RoomSetupBehavior.None);


    /// <summary>The Pseudo Plasma Spark room's state.</summary>
    public static RoomStateSettings PseudoPlasmaSpark { get; } =
        new(nameof(PseudoPlasmaSpark), RoomLevelDefinitions.PseudoPlasmaSpark, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PseudoPlasmaSpark, RoomEnemyPopulationDefinitions.PseudoPlasmaSpark, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PseudoPlasmaSpark, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PseudoPlasmaSpark, null, RoomSetupBehavior.None);


    /// <summary>The Crab Hole room's state.</summary>
    public static RoomStateSettings CrabHole { get; } =
        new(nameof(CrabHole), RoomLevelDefinitions.CrabHole, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.EastMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabHole, RoomEnemyPopulationDefinitions.CrabHole, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabHole, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabHole, null, RoomSetupBehavior.None);


    /// <summary>The West Sand Hall Tunnel room's state.</summary>
    public static RoomStateSettings WestSandHallTunnel { get; } =
        new(nameof(WestSandHallTunnel), RoomLevelDefinitions.WestSandHallTunnel, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHallTunnel, RoomEnemyPopulationDefinitions.WestSandHallTunnel, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestSandHallTunnel, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestSandHallTunnel, null, RoomSetupBehavior.None);


    /// <summary>The Plasma Tutorial room's state.</summary>
    public static RoomStateSettings PlasmaTutorial { get; } =
        new(nameof(PlasmaTutorial), RoomLevelDefinitions.PlasmaTutorial, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaTutorial, RoomEnemyPopulationDefinitions.PlasmaTutorial, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaTutorial, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PlasmaTutorial, null, RoomSetupBehavior.None);


    /// <summary>The Plasma room's state.</summary>
    public static RoomStateSettings Plasma { get; } =
        new(nameof(Plasma), RoomLevelDefinitions.Plasma, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Plasma, RoomEnemyPopulationDefinitions.Plasma, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Plasma, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Plasma, null, RoomSetupBehavior.None);


    /// <summary>The Thread The Needle room's state.</summary>
    public static RoomStateSettings ThreadTheNeedle { get; } =
        new(nameof(ThreadTheNeedle), RoomLevelDefinitions.ThreadTheNeedle, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ThreadTheNeedle, RoomEnemyPopulationDefinitions.ThreadTheNeedle, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThreadTheNeedle, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThreadTheNeedle, null, RoomSetupBehavior.None);


    /// <summary>The Maridia Elev room's state.</summary>
    public static RoomStateSettings MaridiaElev { get; } =
        new(nameof(MaridiaElev), RoomLevelDefinitions.MaridiaElev, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.EastMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaElev, RoomEnemyPopulationDefinitions.MaridiaElev, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MaridiaElev, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaElev, null, RoomSetupBehavior.None);


    /// <summary>The Plasma Spark room's state.</summary>
    public static RoomStateSettings PlasmaSpark { get; } =
        new(nameof(PlasmaSpark), RoomLevelDefinitions.PlasmaSpark, RoomGraphicsSet.SandlessMaridia, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaSpark, RoomEnemyPopulationDefinitions.PlasmaSpark, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(224), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaSpark, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PlasmaSpark, null, RoomSetupBehavior.None);


    /// <summary>The Kassiuz room's state.</summary>
    public static RoomStateSettings Kassiuz { get; } =
        new(nameof(Kassiuz), RoomLevelDefinitions.Kassiuz, RoomGraphicsSet.SandlessMaridia, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.Kassiuz, RoomEnemyPopulationDefinitions.Kassiuz, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kassiuz, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Kassiuz, null, RoomSetupBehavior.None);


    /// <summary>The Maridia Map room's state.</summary>
    public static RoomStateSettings MaridiaMap { get; } =
        new(nameof(MaridiaMap), RoomLevelDefinitions.CrateriaMap, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMap, RoomEnemyPopulationDefinitions.MaridiaMap, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaMap, null, RoomSetupBehavior.None);


    /// <summary>The Forgotten Highway Save room's state.</summary>
    public static RoomStateSettings ForgottenHighwaySave { get; } =
        new(nameof(ForgottenHighwaySave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.EastMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwaySave, RoomEnemyPopulationDefinitions.ForgottenHighwaySave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwaySave, null, RoomSetupBehavior.None);


    /// <summary>The Toilet room's state.</summary>
    public static RoomStateSettings Toilet { get; } =
        new(nameof(Toilet), RoomLevelDefinitions.Toilet, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Toilet, RoomEnemyPopulationDefinitions.Toilet, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllOpen, null, RoomMainBehavior.MaridiaElevatube, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Toilet, null, RoomSetupBehavior.None);


    /// <summary>The Bug Sand Hole room's state.</summary>
    public static RoomStateSettings BugSandHole { get; } =
        new(nameof(BugSandHole), RoomLevelDefinitions.BugSandHole, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BugSandHole, RoomEnemyPopulationDefinitions.BugSandHole, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BugSandHole, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BugSandHole, null, RoomSetupBehavior.None);


    /// <summary>The West Sand Hall room's state.</summary>
    public static RoomStateSettings WestSandHall { get; } =
        new(nameof(WestSandHall), RoomLevelDefinitions.WestSandHall, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHall, RoomEnemyPopulationDefinitions.WestSandHall, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestSandHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.MaridiaGreenWall, RoomSetupBehavior.None);


    /// <summary>The Oasis room's state.</summary>
    public static RoomStateSettings Oasis { get; } =
        new(nameof(Oasis), RoomLevelDefinitions.Oasis, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Oasis, RoomEnemyPopulationDefinitions.Oasis, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Oasis, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Oasis, null, RoomSetupBehavior.None);


    /// <summary>The East Sand Hall room's state.</summary>
    public static RoomStateSettings EastSandHall { get; } =
        new(nameof(EastSandHall), RoomLevelDefinitions.EastSandHall, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHall, RoomEnemyPopulationDefinitions.EastSandHall, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastSandHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.MaridiaGreenWall, RoomSetupBehavior.None);


    /// <summary>The West Sand Hole room's state.</summary>
    public static RoomStateSettings WestSandHole { get; } =
        new(nameof(WestSandHole), RoomLevelDefinitions.WestSandHole, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHole, RoomEnemyPopulationDefinitions.WestSandHole, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestSandHole, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestSandHole, null, RoomSetupBehavior.None);


    /// <summary>The East Sand Hole room's state.</summary>
    public static RoomStateSettings EastSandHole { get; } =
        new(nameof(EastSandHole), RoomLevelDefinitions.EastSandHole, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHole, RoomEnemyPopulationDefinitions.EastSandHole, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastSandHole, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastSandHole, null, RoomSetupBehavior.None);


    /// <summary>The West Aqueduct Quicksand room's state.</summary>
    public static RoomStateSettings WestAqueductQuicksand { get; } =
        new(nameof(WestAqueductQuicksand), RoomLevelDefinitions.WestAqueductQuicksand, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, RoomEnemyPopulationDefinitions.WestAqueductQuicksand, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestAqueductQuicksand, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestAqueductQuicksand, null, RoomSetupBehavior.None);


    /// <summary>The East Aqueduct Quicksand room's state.</summary>
    public static RoomStateSettings EastAqueductQuicksand { get; } =
        new(nameof(EastAqueductQuicksand), RoomLevelDefinitions.EastAqueductQuicksand, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, RoomEnemyPopulationDefinitions.EastAqueductQuicksand, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastAqueductQuicksand, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastAqueductQuicksand, null, RoomSetupBehavior.None);


    /// <summary>The Aqueduct room's state.</summary>
    public static RoomStateSettings Aqueduct { get; } =
        new(nameof(Aqueduct), RoomLevelDefinitions.Aqueduct, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.EastMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.Aqueduct, RoomEnemyPopulationDefinitions.Aqueduct, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Aqueduct, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Aqueduct, null, RoomSetupBehavior.None);


    /// <summary>The Butterfly room's state.</summary>
    public static RoomStateSettings Butterfly { get; } =
        new(nameof(Butterfly), RoomLevelDefinitions.Butterfly, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Butterfly, RoomEnemyPopulationDefinitions.Butterfly, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Butterfly, null, RoomSetupBehavior.None);


    /// <summary>The Botwoon Hallway room's state.</summary>
    public static RoomStateSettings BotwoonHallway { get; } =
        new(nameof(BotwoonHallway), RoomLevelDefinitions.BotwoonHallway, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.EastMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonHallway, RoomEnemyPopulationDefinitions.BotwoonHallway, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonHallway, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BotwoonHallway, null, RoomSetupBehavior.None);


    /// <summary>The Pants room's state.</summary>
    public static RoomStateSettings Pants { get; } =
        new(nameof(Pants), RoomLevelDefinitions.Pants, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Pants, RoomEnemyPopulationDefinitions.Pants, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pants, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Pants, null, RoomSetupBehavior.None);


    /// <summary>The East Pants room's state.</summary>
    public static RoomStateSettings EastPants { get; } =
        new(nameof(EastPants), RoomLevelDefinitions.EastPants, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastPants, RoomEnemyPopulationDefinitions.EastPants, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastPants, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastPants, null, RoomSetupBehavior.None);


    /// <summary>The Springball room's state.</summary>
    public static RoomStateSettings Springball { get; } =
        new(nameof(Springball), RoomLevelDefinitions.Springball, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Springball, RoomEnemyPopulationDefinitions.Springball, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Springball, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Springball, null, RoomSetupBehavior.None);


    /// <summary>The Below Botwoon E Tank room's state.</summary>
    public static RoomStateSettings BelowBotwoonETank { get; } =
        new(nameof(BelowBotwoonETank), RoomLevelDefinitions.BelowBotwoonETank, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BelowBotwoonETank, RoomEnemyPopulationDefinitions.BelowBotwoonETank, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BelowBotwoonETank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BelowBotwoonETank, null, RoomSetupBehavior.None);


    /// <summary>The Colosseum room's state.</summary>
    public static RoomStateSettings Colosseum { get; } =
        new(nameof(Colosseum), RoomLevelDefinitions.Colosseum, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Colosseum, RoomEnemyPopulationDefinitions.Colosseum, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Colosseum, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Colosseum, null, RoomSetupBehavior.None);


    /// <summary>The Aqueduct Save room's state.</summary>
    public static RoomStateSettings AqueductSave { get; } =
        new(nameof(AqueductSave), RoomLevelDefinitions.CrateriaSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.EastMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.AqueductSave, RoomEnemyPopulationDefinitions.AqueductSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AqueductSave, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AqueductSave, null, RoomSetupBehavior.None);


    /// <summary>The The Precious room's default state.</summary>
    public static RoomStateSettings ThePreciousDefault { get; } =
        new(nameof(ThePreciousDefault), RoomLevelDefinitions.ThePrecious, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ThePrecious, RoomEnemyPopulationDefinitions.ThePrecious, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThePrecious, null, RoomSetupBehavior.None);


    /// <summary>The The Precious room's state once the area's boss is dead.</summary>
    public static RoomStateSettings ThePreciousWhenAreaBossDead { get; } =
        new(nameof(ThePreciousWhenAreaBossDead), RoomLevelDefinitions.ThePrecious, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.EastMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.ThePrecious, RoomEnemyPopulationDefinitions.ThePrecious, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThePrecious, null, RoomSetupBehavior.None);


    /// <summary>The Botwoon E Tank room's state.</summary>
    public static RoomStateSettings BotwoonETank { get; } =
        new(nameof(BotwoonETank), RoomLevelDefinitions.BotwoonETank, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.EastMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonETank, RoomEnemyPopulationDefinitions.BotwoonETank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonETank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BotwoonETank, null, RoomSetupBehavior.None);


    /// <summary>The Draygon Save room's state.</summary>
    public static RoomStateSettings DraygonSave { get; } =
        new(nameof(DraygonSave), RoomLevelDefinitions.FrogSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.EastMaridia)), SuperMetroid.Core.Fx.RoomFxDefinitions.DraygonSave, RoomEnemyPopulationDefinitions.DraygonSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DraygonSave, null, RoomSetupBehavior.None);


    /// <summary>The Maridia Missile Refill room's state.</summary>
    public static RoomStateSettings MaridiaMissileRefill { get; } =
        new(nameof(MaridiaMissileRefill), RoomLevelDefinitions.MaridiaMissileRefill, RoomGraphicsSet.BlueRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMissileRefill, RoomEnemyPopulationDefinitions.MaridiaMissileRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaMissileRefill, null, RoomSetupBehavior.None);


    /// <summary>The Plasma Beach Quicksand room's state.</summary>
    public static RoomStateSettings PlasmaBeachQuicksand { get; } =
        new(nameof(PlasmaBeachQuicksand), RoomLevelDefinitions.PlasmaBeachQuicksand, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, RoomEnemyPopulationDefinitions.PlasmaBeachQuicksand, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaBeachQuicksand, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PlasmaBeachQuicksand, null, RoomSetupBehavior.None);


    /// <summary>The Botwoon Quicksand room's state.</summary>
    public static RoomStateSettings BotwoonQuicksand { get; } =
        new(nameof(BotwoonQuicksand), RoomLevelDefinitions.BotwoonQuicksand, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, RoomEnemyPopulationDefinitions.BotwoonQuicksand, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonQuicksand, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BotwoonQuicksand, null, RoomSetupBehavior.None);


    /// <summary>The Shaktool room's default state.</summary>
    public static RoomStateSettings ShaktoolDefault { get; } =
        new(nameof(ShaktoolDefault), RoomLevelDefinitions.ShaktoolDefault, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Shaktool, RoomEnemyPopulationDefinitions.Shaktool, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Shaktool, null, RoomSetupBehavior.SetupShaktoolRoomObject);


    /// <summary>The Shaktool room's state once Shaktool has cleared the path.</summary>
    public static RoomStateSettings ShaktoolWhenShaktoolClearedPath { get; } =
        new(nameof(ShaktoolWhenShaktoolClearedPath), RoomLevelDefinitions.ShaktoolWhenShaktoolClearedPath, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Shaktool, RoomEnemyPopulationDefinitions.Shaktool, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Shaktool, null, RoomSetupBehavior.None);


    /// <summary>The Halfie Climb room's state.</summary>
    public static RoomStateSettings HalfieClimb { get; } =
        new(nameof(HalfieClimb), RoomLevelDefinitions.HalfieClimb, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.HalfieClimb, RoomEnemyPopulationDefinitions.HalfieClimb, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.HalfieClimb, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.HalfieClimb, null, RoomSetupBehavior.None);


    /// <summary>The Botwoon room's default state.</summary>
    public static RoomStateSettings BotwoonDefault { get; } =
        new(nameof(BotwoonDefault), RoomLevelDefinitions.Botwoon, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.MiniBossFight, RoomMusicTrack.Plays(MusicTrack.MiniBossBattle)), SuperMetroid.Core.Fx.RoomFxDefinitions.Botwoon, RoomEnemyPopulationDefinitions.Botwoon, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Botwoon, null, RoomSetupBehavior.None);


    /// <summary>The Botwoon room's state once the area's mini-boss is dead.</summary>
    public static RoomStateSettings BotwoonWhenAreaMiniBossDead { get; } =
        new(nameof(BotwoonWhenAreaMiniBossDead), RoomLevelDefinitions.Botwoon, RoomGraphicsSet.SandyMaridia, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Botwoon, RoomEnemyPopulationDefinitions.Botwoon, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Botwoon, null, RoomSetupBehavior.None);


    /// <summary>The Space Jump room's state.</summary>
    public static RoomStateSettings SpaceJump { get; } =
        new(nameof(SpaceJump), RoomLevelDefinitions.SpaceJump, RoomGraphicsSet.SandyMaridia, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SpaceJump, RoomEnemyPopulationDefinitions.SpaceJump, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpaceJump, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpaceJump, null, RoomSetupBehavior.None);


    /// <summary>The Maridia Energy Refill room's state.</summary>
    public static RoomStateSettings MaridiaEnergyRefill { get; } =
        new(nameof(MaridiaEnergyRefill), RoomLevelDefinitions.GTEnergyRefill, RoomGraphicsSet.BlueRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaEnergyRefill, RoomEnemyPopulationDefinitions.MaridiaEnergyRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MaridiaEnergyRefill, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaEnergyRefill, null, RoomSetupBehavior.None);


    /// <summary>The West Cactus Alley room's state.</summary>
    public static RoomStateSettings WestCactusAlley { get; } =
        new(nameof(WestCactusAlley), RoomLevelDefinitions.WestCactusAlley, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestCactusAlley, RoomEnemyPopulationDefinitions.WestCactusAlley, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestCactusAlley, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestCactusAlley, null, RoomSetupBehavior.None);


    /// <summary>The East Cactus Alley room's state.</summary>
    public static RoomStateSettings EastCactusAlley { get; } =
        new(nameof(EastCactusAlley), RoomLevelDefinitions.EastCactusAlley, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastCactusAlley, RoomEnemyPopulationDefinitions.EastCactusAlley, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastCactusAlley, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastCactusAlley, null, RoomSetupBehavior.None);


    /// <summary>The Draygon room's default state.</summary>
    public static RoomStateSettings DraygonDefault { get; } =
        new(nameof(DraygonDefault), RoomLevelDefinitions.Draygon, RoomGraphicsSet.Draygon, new RoomMusic(MusicDataSet.RidleyAndDraygonBattles, RoomMusicTrack.Plays(MusicTrack.RidleyAndDraygonBattle)), SuperMetroid.Core.Fx.RoomFxDefinitions.Draygon, RoomEnemyPopulationDefinitions.DraygonDefault, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Draygon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.DraygonDefault, RoomSetupBehavior.None);


    /// <summary>The Draygon room's state once the area's boss is dead.</summary>
    public static RoomStateSettings DraygonWhenAreaBossDead { get; } =
        new(nameof(DraygonWhenAreaBossDead), RoomLevelDefinitions.Draygon, RoomGraphicsSet.Draygon, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Draygon, RoomEnemyPopulationDefinitions.DraygonWhenAreaBossDeadOrMotherBrainWhenMainAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Draygon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.DraygonOrPhantoonWhenAreaBossDead, RoomSetupBehavior.None);
}
