using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Maridia room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class MaridiaRoomStateDefinitions
{

    /// <summary>The Glass Tunnel Save room's state.</summary>
    public static RoomStateSettings GlassTunnelSave { get; } =
        new(nameof(GlassTunnelSave), RoomLevelDefinitions.WarehouseSave, 25, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnelSave, RoomEnemyPopulationDefinitions.GlassTunnelSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GlassTunnelSave, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Glass Tunnel room's state 0.</summary>
    public static RoomStateSettings GlassTunnelDefault { get; } =
        new(nameof(GlassTunnelDefault), RoomLevelDefinitions.GlassTunnelDefault, 11, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnel, RoomEnemyPopulationDefinitions.GlassTunnel, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GlassTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Glass Tunnel room's state 1.</summary>
    public static RoomStateSettings GlassTunnelWhenMaridiaNoobTubeBroken { get; } =
        new(nameof(GlassTunnelWhenMaridiaNoobTubeBroken), RoomLevelDefinitions.GlassTunnelWhenMaridiaNoobTubeBroken, 11, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnel, RoomEnemyPopulationDefinitions.GlassTunnel, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GlassTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The West Tunnel room's state.</summary>
    public static RoomStateSettings WestTunnel { get; } =
        new(nameof(WestTunnel), RoomLevelDefinitions.WestTunnel, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestTunnel, RoomEnemyPopulationDefinitions.WestTunnel, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The East Tunnel room's state.</summary>
    public static RoomStateSettings EastTunnel { get; } =
        new(nameof(EastTunnel), RoomLevelDefinitions.EastTunnel, 11, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.EastTunnel, RoomEnemyPopulationDefinitions.EastTunnel, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Main Street room's state.</summary>
    public static RoomStateSettings MainStreet { get; } =
        new(nameof(MainStreet), RoomLevelDefinitions.MainStreet, 11, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.MainStreet, RoomEnemyPopulationDefinitions.MainStreet, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MainStreet, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MainStreet, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Fish Tank room's state.</summary>
    public static RoomStateSettings FishTank { get; } =
        new(nameof(FishTank), RoomLevelDefinitions.FishTank, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FishTank, RoomEnemyPopulationDefinitions.FishTank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FishTank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FishTank, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Mama Turtle room's state.</summary>
    public static RoomStateSettings MamaTurtle { get; } =
        new(nameof(MamaTurtle), RoomLevelDefinitions.MamaTurtle, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MamaTurtle, RoomEnemyPopulationDefinitions.MamaTurtle, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MamaTurtle, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MamaTurtle, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Crab Tunnel room's state.</summary>
    public static RoomStateSettings CrabTunnel { get; } =
        new(nameof(CrabTunnel), RoomLevelDefinitions.CrabTunnel, 11, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabTunnel, RoomEnemyPopulationDefinitions.CrabTunnel, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.MaridiaBBrownWall, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Mt Everest room's state.</summary>
    public static RoomStateSettings MtEverest { get; } =
        new(nameof(MtEverest), RoomLevelDefinitions.MtEverest, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MtEverest, RoomEnemyPopulationDefinitions.MtEverest, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MtEverest, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MtEverest, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Red Fish room's state.</summary>
    public static RoomStateSettings RedFish { get; } =
        new(nameof(RedFish), RoomLevelDefinitions.RedFish, 11, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedFish, RoomEnemyPopulationDefinitions.RedFish, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedFish, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedFish, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Watering Hole room's state.</summary>
    public static RoomStateSettings WateringHole { get; } =
        new(nameof(WateringHole), RoomLevelDefinitions.WateringHole, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WateringHole, RoomEnemyPopulationDefinitions.WateringHole, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WateringHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WateringHole, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The N West Maridia Bug room's state.</summary>
    public static RoomStateSettings NWestMaridiaBug { get; } =
        new(nameof(NWestMaridiaBug), RoomLevelDefinitions.NWestMaridiaBug, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NWestMaridiaBug, RoomEnemyPopulationDefinitions.NWestMaridiaBug, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.NWestMaridiaBug, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.NWestMaridiaBug, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Crab Shaft room's state.</summary>
    public static RoomStateSettings CrabShaft { get; } =
        new(nameof(CrabShaft), RoomLevelDefinitions.CrabShaft, 11, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabShaft, RoomEnemyPopulationDefinitions.CrabShaft, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabShaft, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Pseudo Plasma Spark room's state.</summary>
    public static RoomStateSettings PseudoPlasmaSpark { get; } =
        new(nameof(PseudoPlasmaSpark), RoomLevelDefinitions.PseudoPlasmaSpark, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PseudoPlasmaSpark, RoomEnemyPopulationDefinitions.PseudoPlasmaSpark, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PseudoPlasmaSpark, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PseudoPlasmaSpark, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Crab Hole room's state.</summary>
    public static RoomStateSettings CrabHole { get; } =
        new(nameof(CrabHole), RoomLevelDefinitions.CrabHole, 11, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabHole, RoomEnemyPopulationDefinitions.CrabHole, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabHole, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The West Sand Hall Tunnel room's state.</summary>
    public static RoomStateSettings WestSandHallTunnel { get; } =
        new(nameof(WestSandHallTunnel), RoomLevelDefinitions.WestSandHallTunnel, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHallTunnel, RoomEnemyPopulationDefinitions.WestSandHallTunnel, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestSandHallTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestSandHallTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Plasma Tutorial room's state.</summary>
    public static RoomStateSettings PlasmaTutorial { get; } =
        new(nameof(PlasmaTutorial), RoomLevelDefinitions.PlasmaTutorial, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaTutorial, RoomEnemyPopulationDefinitions.PlasmaTutorial, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaTutorial, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PlasmaTutorial, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Plasma room's state.</summary>
    public static RoomStateSettings Plasma { get; } =
        new(nameof(Plasma), RoomLevelDefinitions.Plasma, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Plasma, RoomEnemyPopulationDefinitions.Plasma, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Plasma, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Plasma, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Thread The Needle room's state.</summary>
    public static RoomStateSettings ThreadTheNeedle { get; } =
        new(nameof(ThreadTheNeedle), RoomLevelDefinitions.ThreadTheNeedle, 11, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ThreadTheNeedle, RoomEnemyPopulationDefinitions.ThreadTheNeedle, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThreadTheNeedle, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThreadTheNeedle, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Maridia Elev room's state.</summary>
    public static RoomStateSettings MaridiaElev { get; } =
        new(nameof(MaridiaElev), RoomLevelDefinitions.MaridiaElev, 11, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaElev, RoomEnemyPopulationDefinitions.MaridiaElev, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MaridiaElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaElev, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Plasma Spark room's state.</summary>
    public static RoomStateSettings PlasmaSpark { get; } =
        new(nameof(PlasmaSpark), RoomLevelDefinitions.PlasmaSpark, 11, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaSpark, RoomEnemyPopulationDefinitions.PlasmaSpark, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(224), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaSpark, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PlasmaSpark, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Kassiuz room's state.</summary>
    public static RoomStateSettings Kassiuz { get; } =
        new(nameof(Kassiuz), RoomLevelDefinitions.Kassiuz, 11, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Kassiuz, RoomEnemyPopulationDefinitions.Kassiuz, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kassiuz, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Kassiuz, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Maridia Map room's state.</summary>
    public static RoomStateSettings MaridiaMap { get; } =
        new(nameof(MaridiaMap), RoomLevelDefinitions.CrateriaMap, 21, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMap, RoomEnemyPopulationDefinitions.MaridiaMap, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaMap, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Forgotten Highway Save room's state.</summary>
    public static RoomStateSettings ForgottenHighwaySave { get; } =
        new(nameof(ForgottenHighwaySave), RoomLevelDefinitions.WarehouseSave, 25, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwaySave, RoomEnemyPopulationDefinitions.ForgottenHighwaySave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwaySave, null, RoomSetupCallback.None);


    /// <summary>The Toilet room's state.</summary>
    public static RoomStateSettings Toilet { get; } =
        new(nameof(Toilet), RoomLevelDefinitions.Toilet, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Toilet, RoomEnemyPopulationDefinitions.Toilet, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.MaridiaElevatube, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Toilet, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Bug Sand Hole room's state.</summary>
    public static RoomStateSettings BugSandHole { get; } =
        new(nameof(BugSandHole), RoomLevelDefinitions.BugSandHole, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BugSandHole, RoomEnemyPopulationDefinitions.BugSandHole, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BugSandHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BugSandHole, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The West Sand Hall room's state.</summary>
    public static RoomStateSettings WestSandHall { get; } =
        new(nameof(WestSandHall), RoomLevelDefinitions.WestSandHall, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHall, RoomEnemyPopulationDefinitions.WestSandHall, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestSandHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.MaridiaCGreenWall, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Oasis room's state.</summary>
    public static RoomStateSettings Oasis { get; } =
        new(nameof(Oasis), RoomLevelDefinitions.Oasis, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Oasis, RoomEnemyPopulationDefinitions.Oasis, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Oasis, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Oasis, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The East Sand Hall room's state.</summary>
    public static RoomStateSettings EastSandHall { get; } =
        new(nameof(EastSandHall), RoomLevelDefinitions.EastSandHall, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHall, RoomEnemyPopulationDefinitions.EastSandHall, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastSandHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.MaridiaCGreenWall, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The West Sand Hole room's state.</summary>
    public static RoomStateSettings WestSandHole { get; } =
        new(nameof(WestSandHole), RoomLevelDefinitions.WestSandHole, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHole, RoomEnemyPopulationDefinitions.WestSandHole, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestSandHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestSandHole, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The East Sand Hole room's state.</summary>
    public static RoomStateSettings EastSandHole { get; } =
        new(nameof(EastSandHole), RoomLevelDefinitions.EastSandHole, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHole, RoomEnemyPopulationDefinitions.EastSandHole, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastSandHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastSandHole, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The West Aqueduct Quicksand room's state.</summary>
    public static RoomStateSettings WestAqueductQuicksand { get; } =
        new(nameof(WestAqueductQuicksand), RoomLevelDefinitions.WestAqueductQuicksand, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, RoomEnemyPopulationDefinitions.WestAqueductQuicksand, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestAqueductQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestAqueductQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The East Aqueduct Quicksand room's state.</summary>
    public static RoomStateSettings EastAqueductQuicksand { get; } =
        new(nameof(EastAqueductQuicksand), RoomLevelDefinitions.EastAqueductQuicksand, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, RoomEnemyPopulationDefinitions.EastAqueductQuicksand, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastAqueductQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastAqueductQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Aqueduct room's state.</summary>
    public static RoomStateSettings Aqueduct { get; } =
        new(nameof(Aqueduct), RoomLevelDefinitions.Aqueduct, 12, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Aqueduct, RoomEnemyPopulationDefinitions.Aqueduct, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Aqueduct, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Aqueduct, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Butterfly room's state.</summary>
    public static RoomStateSettings Butterfly { get; } =
        new(nameof(Butterfly), RoomLevelDefinitions.Butterfly, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Butterfly, RoomEnemyPopulationDefinitions.Butterfly, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Butterfly, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Botwoon Hallway room's state.</summary>
    public static RoomStateSettings BotwoonHallway { get; } =
        new(nameof(BotwoonHallway), RoomLevelDefinitions.BotwoonHallway, 12, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonHallway, RoomEnemyPopulationDefinitions.BotwoonHallway, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonHallway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BotwoonHallway, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Pants room's state.</summary>
    public static RoomStateSettings Pants { get; } =
        new(nameof(Pants), RoomLevelDefinitions.Pants, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Pants, RoomEnemyPopulationDefinitions.Pants, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pants, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Pants, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The East Pants room's state.</summary>
    public static RoomStateSettings EastPants { get; } =
        new(nameof(EastPants), RoomLevelDefinitions.EastPants, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastPants, RoomEnemyPopulationDefinitions.EastPants, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastPants, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastPants, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Springball room's state.</summary>
    public static RoomStateSettings Springball { get; } =
        new(nameof(Springball), RoomLevelDefinitions.Springball, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Springball, RoomEnemyPopulationDefinitions.Springball, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Springball, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Springball, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Below Botwoon E Tank room's state.</summary>
    public static RoomStateSettings BelowBotwoonETank { get; } =
        new(nameof(BelowBotwoonETank), RoomLevelDefinitions.BelowBotwoonETank, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BelowBotwoonETank, RoomEnemyPopulationDefinitions.BelowBotwoonETank, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BelowBotwoonETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BelowBotwoonETank, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Colosseum room's state.</summary>
    public static RoomStateSettings Colosseum { get; } =
        new(nameof(Colosseum), RoomLevelDefinitions.Colosseum, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Colosseum, RoomEnemyPopulationDefinitions.Colosseum, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Colosseum, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Colosseum, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Aqueduct Save room's state.</summary>
    public static RoomStateSettings AqueductSave { get; } =
        new(nameof(AqueductSave), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.AqueductSave, RoomEnemyPopulationDefinitions.AqueductSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AqueductSave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AqueductSave, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The The Precious room's state 0.</summary>
    public static RoomStateSettings ThePreciousDefault { get; } =
        new(nameof(ThePreciousDefault), RoomLevelDefinitions.ThePrecious, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ThePrecious, RoomEnemyPopulationDefinitions.ThePrecious, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThePrecious, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The The Precious room's state 1.</summary>
    public static RoomStateSettings ThePreciousWhenAreaBossDead { get; } =
        new(nameof(ThePreciousWhenAreaBossDead), RoomLevelDefinitions.ThePrecious, 12, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ThePrecious, RoomEnemyPopulationDefinitions.ThePrecious, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThePrecious, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Botwoon E Tank room's state.</summary>
    public static RoomStateSettings BotwoonETank { get; } =
        new(nameof(BotwoonETank), RoomLevelDefinitions.BotwoonETank, 12, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonETank, RoomEnemyPopulationDefinitions.BotwoonETank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BotwoonETank, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Draygon Save room's state.</summary>
    public static RoomStateSettings DraygonSave { get; } =
        new(nameof(DraygonSave), RoomLevelDefinitions.FrogSave, 25, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.DraygonSave, RoomEnemyPopulationDefinitions.DraygonSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DraygonSave, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Maridia Missile Refill room's state.</summary>
    public static RoomStateSettings MaridiaMissileRefill { get; } =
        new(nameof(MaridiaMissileRefill), RoomLevelDefinitions.MaridiaMissileRefill, 23, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMissileRefill, RoomEnemyPopulationDefinitions.MaridiaMissileRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaMissileRefill, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Plasma Beach Quicksand room's state.</summary>
    public static RoomStateSettings PlasmaBeachQuicksand { get; } =
        new(nameof(PlasmaBeachQuicksand), RoomLevelDefinitions.PlasmaBeachQuicksand, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, RoomEnemyPopulationDefinitions.PlasmaBeachQuicksand, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaBeachQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PlasmaBeachQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Botwoon Quicksand room's state.</summary>
    public static RoomStateSettings BotwoonQuicksand { get; } =
        new(nameof(BotwoonQuicksand), RoomLevelDefinitions.BotwoonQuicksand, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, RoomEnemyPopulationDefinitions.BotwoonQuicksand, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BotwoonQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Shaktool room's state 0.</summary>
    public static RoomStateSettings ShaktoolDefault { get; } =
        new(nameof(ShaktoolDefault), RoomLevelDefinitions.ShaktoolDefault, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Shaktool, RoomEnemyPopulationDefinitions.Shaktool, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Shaktool, null, RoomSetupCallback.SetupShaktoolRoomObject);


    /// <summary>The Shaktool room's state 1.</summary>
    public static RoomStateSettings ShaktoolWhenShaktoolClearedPath { get; } =
        new(nameof(ShaktoolWhenShaktoolClearedPath), RoomLevelDefinitions.ShaktoolWhenShaktoolClearedPath, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Shaktool, RoomEnemyPopulationDefinitions.Shaktool, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Shaktool, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Halfie Climb room's state.</summary>
    public static RoomStateSettings HalfieClimb { get; } =
        new(nameof(HalfieClimb), RoomLevelDefinitions.HalfieClimb, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.HalfieClimb, RoomEnemyPopulationDefinitions.HalfieClimb, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.HalfieClimb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.HalfieClimb, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Botwoon room's state 0.</summary>
    public static RoomStateSettings BotwoonDefault { get; } =
        new(nameof(BotwoonDefault), RoomLevelDefinitions.Botwoon, 12, new RoomMusic(MusicDataSet.MiniBossFight, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Botwoon, RoomEnemyPopulationDefinitions.Botwoon, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Botwoon, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Botwoon room's state 1.</summary>
    public static RoomStateSettings BotwoonWhenAreaMiniBossDead { get; } =
        new(nameof(BotwoonWhenAreaMiniBossDead), RoomLevelDefinitions.Botwoon, 12, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Botwoon, RoomEnemyPopulationDefinitions.Botwoon, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Botwoon, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Space Jump room's state.</summary>
    public static RoomStateSettings SpaceJump { get; } =
        new(nameof(SpaceJump), RoomLevelDefinitions.SpaceJump, 12, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SpaceJump, RoomEnemyPopulationDefinitions.SpaceJump, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpaceJump, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpaceJump, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Maridia Energy Refill room's state.</summary>
    public static RoomStateSettings MaridiaEnergyRefill { get; } =
        new(nameof(MaridiaEnergyRefill), RoomLevelDefinitions.GTEnergyRefill, 23, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaEnergyRefill, RoomEnemyPopulationDefinitions.MaridiaEnergyRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MaridiaEnergyRefill, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaEnergyRefill, null, RoomSetupCallback.None);


    /// <summary>The West Cactus Alley room's state.</summary>
    public static RoomStateSettings WestCactusAlley { get; } =
        new(nameof(WestCactusAlley), RoomLevelDefinitions.WestCactusAlley, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestCactusAlley, RoomEnemyPopulationDefinitions.WestCactusAlley, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestCactusAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestCactusAlley, null, RoomSetupCallback.None);


    /// <summary>The East Cactus Alley room's state.</summary>
    public static RoomStateSettings EastCactusAlley { get; } =
        new(nameof(EastCactusAlley), RoomLevelDefinitions.EastCactusAlley, 12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastCactusAlley, RoomEnemyPopulationDefinitions.EastCactusAlley, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastCactusAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastCactusAlley, null, RoomSetupCallback.None);


    /// <summary>The Draygon room's state 0.</summary>
    public static RoomStateSettings DraygonDefault { get; } =
        new(nameof(DraygonDefault), RoomLevelDefinitions.Draygon, 28, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Draygon, RoomEnemyPopulationDefinitions.DraygonDefault, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Draygon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.DraygonDefault, RoomSetupCallback.SetPausingCodeForDraygon);


    /// <summary>The Draygon room's state 1.</summary>
    public static RoomStateSettings DraygonWhenAreaBossDead { get; } =
        new(nameof(DraygonWhenAreaBossDead), RoomLevelDefinitions.Draygon, 28, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Draygon, RoomEnemyPopulationDefinitions.DraygonWhenAreaBossDeadOrMotherBrainWhenMainAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Draygon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.DraygonOrPhantoonWhenAreaBossDead, RoomSetupCallback.SetPausingCodeForDraygon);
}
