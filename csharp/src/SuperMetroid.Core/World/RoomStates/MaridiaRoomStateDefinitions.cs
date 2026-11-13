using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Maridia room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class MaridiaRoomStateDefinitions
{

    /// <summary>The Glass Tunnel Save room's state.</summary>
    public static RoomStateSettings GlassTunnelSave { get; } =
        new(nameof(GlassTunnelSave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnelSave, RoomEnemyPopulationDefinitions.GlassTunnelSave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GlassTunnelSave, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Glass Tunnel room's state 0.</summary>
    public static RoomStateSettings GlassTunnelDefault { get; } =
        new(nameof(GlassTunnelDefault), RoomLevelDefinitions.GlassTunnelDefault, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnel, RoomEnemyPopulationDefinitions.GlassTunnel, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GlassTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Glass Tunnel room's state 1.</summary>
    public static RoomStateSettings GlassTunnelWhenMaridiaNoobTubeBroken { get; } =
        new(nameof(GlassTunnelWhenMaridiaNoobTubeBroken), RoomLevelDefinitions.GlassTunnelWhenMaridiaNoobTubeBroken, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnel, RoomEnemyPopulationDefinitions.GlassTunnel, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GlassTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The West Tunnel room's state.</summary>
    public static RoomStateSettings WestTunnel { get; } =
        new(nameof(WestTunnel), RoomLevelDefinitions.WestTunnel, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestTunnel, RoomEnemyPopulationDefinitions.WestTunnel, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The East Tunnel room's state.</summary>
    public static RoomStateSettings EastTunnel { get; } =
        new(nameof(EastTunnel), RoomLevelDefinitions.EastTunnel, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.EastTunnel, RoomEnemyPopulationDefinitions.EastTunnel, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Main Street room's state.</summary>
    public static RoomStateSettings MainStreet { get; } =
        new(nameof(MainStreet), RoomLevelDefinitions.MainStreet, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.MainStreet, RoomEnemyPopulationDefinitions.MainStreet, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MainStreet, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MainStreet, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Fish Tank room's state.</summary>
    public static RoomStateSettings FishTank { get; } =
        new(nameof(FishTank), RoomLevelDefinitions.FishTank, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FishTank, RoomEnemyPopulationDefinitions.FishTank, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FishTank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FishTank, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Mama Turtle room's state.</summary>
    public static RoomStateSettings MamaTurtle { get; } =
        new(nameof(MamaTurtle), RoomLevelDefinitions.MamaTurtle, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MamaTurtle, RoomEnemyPopulationDefinitions.MamaTurtle, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MamaTurtle, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MamaTurtle, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Crab Tunnel room's state.</summary>
    public static RoomStateSettings CrabTunnel { get; } =
        new(nameof(CrabTunnel), RoomLevelDefinitions.CrabTunnel, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabTunnel, RoomEnemyPopulationDefinitions.CrabTunnel, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.MaridiaBBrownWall, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Mt Everest room's state.</summary>
    public static RoomStateSettings MtEverest { get; } =
        new(nameof(MtEverest), RoomLevelDefinitions.MtEverest, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MtEverest, RoomEnemyPopulationDefinitions.MtEverest, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MtEverest, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MtEverest, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Red Fish room's state.</summary>
    public static RoomStateSettings RedFish { get; } =
        new(nameof(RedFish), RoomLevelDefinitions.RedFish, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedFish, RoomEnemyPopulationDefinitions.RedFish, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedFish, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedFish, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Watering Hole room's state.</summary>
    public static RoomStateSettings WateringHole { get; } =
        new(nameof(WateringHole), RoomLevelDefinitions.WateringHole, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WateringHole, RoomEnemyPopulationDefinitions.WateringHole, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WateringHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WateringHole, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The N West Maridia Bug room's state.</summary>
    public static RoomStateSettings NWestMaridiaBug { get; } =
        new(nameof(NWestMaridiaBug), RoomLevelDefinitions.NWestMaridiaBug, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NWestMaridiaBug, RoomEnemyPopulationDefinitions.NWestMaridiaBug, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.NWestMaridiaBug, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.NWestMaridiaBug, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Crab Shaft room's state.</summary>
    public static RoomStateSettings CrabShaft { get; } =
        new(nameof(CrabShaft), RoomLevelDefinitions.CrabShaft, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabShaft, RoomEnemyPopulationDefinitions.CrabShaft, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabShaft, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Pseudo Plasma Spark room's state.</summary>
    public static RoomStateSettings PseudoPlasmaSpark { get; } =
        new(nameof(PseudoPlasmaSpark), RoomLevelDefinitions.PseudoPlasmaSpark, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PseudoPlasmaSpark, RoomEnemyPopulationDefinitions.PseudoPlasmaSpark, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PseudoPlasmaSpark, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PseudoPlasmaSpark, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Crab Hole room's state.</summary>
    public static RoomStateSettings CrabHole { get; } =
        new(nameof(CrabHole), RoomLevelDefinitions.CrabHole, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabHole, RoomEnemyPopulationDefinitions.CrabHole, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrabHole, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The West Sand Hall Tunnel room's state.</summary>
    public static RoomStateSettings WestSandHallTunnel { get; } =
        new(nameof(WestSandHallTunnel), RoomLevelDefinitions.WestSandHallTunnel, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHallTunnel, RoomEnemyPopulationDefinitions.WestSandHallTunnel, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestSandHallTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestSandHallTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Plasma Tutorial room's state.</summary>
    public static RoomStateSettings PlasmaTutorial { get; } =
        new(nameof(PlasmaTutorial), RoomLevelDefinitions.PlasmaTutorial, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaTutorial, RoomEnemyPopulationDefinitions.PlasmaTutorial, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaTutorial, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PlasmaTutorial, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Plasma room's state.</summary>
    public static RoomStateSettings Plasma { get; } =
        new(nameof(Plasma), RoomLevelDefinitions.Plasma, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Plasma, RoomEnemyPopulationDefinitions.Plasma, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Plasma, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Plasma, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Thread The Needle room's state.</summary>
    public static RoomStateSettings ThreadTheNeedle { get; } =
        new(nameof(ThreadTheNeedle), RoomLevelDefinitions.ThreadTheNeedle, RoomGraphicsSet.SandlessMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ThreadTheNeedle, RoomEnemyPopulationDefinitions.ThreadTheNeedle, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThreadTheNeedle, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThreadTheNeedle, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Maridia Elev room's state.</summary>
    public static RoomStateSettings MaridiaElev { get; } =
        new(nameof(MaridiaElev), RoomLevelDefinitions.MaridiaElev, RoomGraphicsSet.SandlessMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaElev, RoomEnemyPopulationDefinitions.MaridiaElev, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MaridiaElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaElev, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Plasma Spark room's state.</summary>
    public static RoomStateSettings PlasmaSpark { get; } =
        new(nameof(PlasmaSpark), RoomLevelDefinitions.PlasmaSpark, RoomGraphicsSet.SandlessMaridia, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaSpark, RoomEnemyPopulationDefinitions.PlasmaSpark, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(224, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaSpark, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PlasmaSpark, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Kassiuz room's state.</summary>
    public static RoomStateSettings Kassiuz { get; } =
        new(nameof(Kassiuz), RoomLevelDefinitions.Kassiuz, RoomGraphicsSet.SandlessMaridia, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Kassiuz, RoomEnemyPopulationDefinitions.Kassiuz, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kassiuz, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Kassiuz, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Maridia Map room's state.</summary>
    public static RoomStateSettings MaridiaMap { get; } =
        new(nameof(MaridiaMap), RoomLevelDefinitions.CrateriaMap, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMap, RoomEnemyPopulationDefinitions.MaridiaMap, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaMap, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The Forgotten Highway Save room's state.</summary>
    public static RoomStateSettings ForgottenHighwaySave { get; } =
        new(nameof(ForgottenHighwaySave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwaySave, RoomEnemyPopulationDefinitions.ForgottenHighwaySave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ForgottenHighwaySave, null, RoomSetupCallback.None);


    /// <summary>The Toilet room's state.</summary>
    public static RoomStateSettings Toilet { get; } =
        new(nameof(Toilet), RoomLevelDefinitions.Toilet, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Toilet, RoomEnemyPopulationDefinitions.Toilet, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.MaridiaElevatube, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Toilet, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Bug Sand Hole room's state.</summary>
    public static RoomStateSettings BugSandHole { get; } =
        new(nameof(BugSandHole), RoomLevelDefinitions.BugSandHole, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BugSandHole, RoomEnemyPopulationDefinitions.BugSandHole, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BugSandHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BugSandHole, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The West Sand Hall room's state.</summary>
    public static RoomStateSettings WestSandHall { get; } =
        new(nameof(WestSandHall), RoomLevelDefinitions.WestSandHall, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHall, RoomEnemyPopulationDefinitions.WestSandHall, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestSandHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.MaridiaCGreenWall, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Oasis room's state.</summary>
    public static RoomStateSettings Oasis { get; } =
        new(nameof(Oasis), RoomLevelDefinitions.Oasis, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Oasis, RoomEnemyPopulationDefinitions.Oasis, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Oasis, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Oasis, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The East Sand Hall room's state.</summary>
    public static RoomStateSettings EastSandHall { get; } =
        new(nameof(EastSandHall), RoomLevelDefinitions.EastSandHall, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHall, RoomEnemyPopulationDefinitions.EastSandHall, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastSandHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.MaridiaCGreenWall, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The West Sand Hole room's state.</summary>
    public static RoomStateSettings WestSandHole { get; } =
        new(nameof(WestSandHole), RoomLevelDefinitions.WestSandHole, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHole, RoomEnemyPopulationDefinitions.WestSandHole, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestSandHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestSandHole, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The East Sand Hole room's state.</summary>
    public static RoomStateSettings EastSandHole { get; } =
        new(nameof(EastSandHole), RoomLevelDefinitions.EastSandHole, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHole, RoomEnemyPopulationDefinitions.EastSandHole, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastSandHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastSandHole, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The West Aqueduct Quicksand room's state.</summary>
    public static RoomStateSettings WestAqueductQuicksand { get; } =
        new(nameof(WestAqueductQuicksand), RoomLevelDefinitions.WestAqueductQuicksand, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, RoomEnemyPopulationDefinitions.WestAqueductQuicksand, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestAqueductQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestAqueductQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The East Aqueduct Quicksand room's state.</summary>
    public static RoomStateSettings EastAqueductQuicksand { get; } =
        new(nameof(EastAqueductQuicksand), RoomLevelDefinitions.EastAqueductQuicksand, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, RoomEnemyPopulationDefinitions.EastAqueductQuicksand, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastAqueductQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastAqueductQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Aqueduct room's state.</summary>
    public static RoomStateSettings Aqueduct { get; } =
        new(nameof(Aqueduct), RoomLevelDefinitions.Aqueduct, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Aqueduct, RoomEnemyPopulationDefinitions.Aqueduct, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Aqueduct, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Aqueduct, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Butterfly room's state.</summary>
    public static RoomStateSettings Butterfly { get; } =
        new(nameof(Butterfly), RoomLevelDefinitions.Butterfly, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Butterfly, RoomEnemyPopulationDefinitions.Butterfly, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Butterfly, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Botwoon Hallway room's state.</summary>
    public static RoomStateSettings BotwoonHallway { get; } =
        new(nameof(BotwoonHallway), RoomLevelDefinitions.BotwoonHallway, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonHallway, RoomEnemyPopulationDefinitions.BotwoonHallway, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonHallway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BotwoonHallway, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Pants room's state.</summary>
    public static RoomStateSettings Pants { get; } =
        new(nameof(Pants), RoomLevelDefinitions.Pants, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Pants, RoomEnemyPopulationDefinitions.Pants, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pants, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Pants, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The East Pants room's state.</summary>
    public static RoomStateSettings EastPants { get; } =
        new(nameof(EastPants), RoomLevelDefinitions.EastPants, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastPants, RoomEnemyPopulationDefinitions.EastPants, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastPants, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastPants, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Springball room's state.</summary>
    public static RoomStateSettings Springball { get; } =
        new(nameof(Springball), RoomLevelDefinitions.Springball, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Springball, RoomEnemyPopulationDefinitions.Springball, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Springball, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Springball, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Below Botwoon E Tank room's state.</summary>
    public static RoomStateSettings BelowBotwoonETank { get; } =
        new(nameof(BelowBotwoonETank), RoomLevelDefinitions.BelowBotwoonETank, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BelowBotwoonETank, RoomEnemyPopulationDefinitions.BelowBotwoonETank, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BelowBotwoonETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BelowBotwoonETank, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Colosseum room's state.</summary>
    public static RoomStateSettings Colosseum { get; } =
        new(nameof(Colosseum), RoomLevelDefinitions.Colosseum, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Colosseum, RoomEnemyPopulationDefinitions.Colosseum, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Colosseum, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Colosseum, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Aqueduct Save room's state.</summary>
    public static RoomStateSettings AqueductSave { get; } =
        new(nameof(AqueductSave), RoomLevelDefinitions.CrateriaSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.AqueductSave, RoomEnemyPopulationDefinitions.AqueductSave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AqueductSave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AqueductSave, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The The Precious room's state 0.</summary>
    public static RoomStateSettings ThePreciousDefault { get; } =
        new(nameof(ThePreciousDefault), RoomLevelDefinitions.ThePrecious, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ThePrecious, RoomEnemyPopulationDefinitions.ThePrecious, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThePrecious, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The The Precious room's state 1.</summary>
    public static RoomStateSettings ThePreciousWhenAreaBossDead { get; } =
        new(nameof(ThePreciousWhenAreaBossDead), RoomLevelDefinitions.ThePrecious, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ThePrecious, RoomEnemyPopulationDefinitions.ThePrecious, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThePrecious, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Botwoon E Tank room's state.</summary>
    public static RoomStateSettings BotwoonETank { get; } =
        new(nameof(BotwoonETank), RoomLevelDefinitions.BotwoonETank, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonETank, RoomEnemyPopulationDefinitions.BotwoonETank, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BotwoonETank, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Draygon Save room's state.</summary>
    public static RoomStateSettings DraygonSave { get; } =
        new(nameof(DraygonSave), RoomLevelDefinitions.FrogSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.DraygonSave, RoomEnemyPopulationDefinitions.DraygonSave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DraygonSave, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Maridia Missile Refill room's state.</summary>
    public static RoomStateSettings MaridiaMissileRefill { get; } =
        new(nameof(MaridiaMissileRefill), RoomLevelDefinitions.MaridiaMissileRefill, RoomGraphicsSet.BlueRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMissileRefill, RoomEnemyPopulationDefinitions.MaridiaMissileRefill, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaMissileRefill, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Plasma Beach Quicksand room's state.</summary>
    public static RoomStateSettings PlasmaBeachQuicksand { get; } =
        new(nameof(PlasmaBeachQuicksand), RoomLevelDefinitions.PlasmaBeachQuicksand, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, RoomEnemyPopulationDefinitions.PlasmaBeachQuicksand, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaBeachQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PlasmaBeachQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Botwoon Quicksand room's state.</summary>
    public static RoomStateSettings BotwoonQuicksand { get; } =
        new(nameof(BotwoonQuicksand), RoomLevelDefinitions.BotwoonQuicksand, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, RoomEnemyPopulationDefinitions.BotwoonQuicksand, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BotwoonQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The Shaktool room's state 0.</summary>
    public static RoomStateSettings ShaktoolDefault { get; } =
        new(nameof(ShaktoolDefault), RoomLevelDefinitions.ShaktoolDefault, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Shaktool, RoomEnemyPopulationDefinitions.Shaktool, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Shaktool, null, RoomSetupCallback.SetupShaktoolRoomObject);


    /// <summary>The Shaktool room's state 1.</summary>
    public static RoomStateSettings ShaktoolWhenShaktoolClearedPath { get; } =
        new(nameof(ShaktoolWhenShaktoolClearedPath), RoomLevelDefinitions.ShaktoolWhenShaktoolClearedPath, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Shaktool, RoomEnemyPopulationDefinitions.Shaktool, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Shaktool, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Halfie Climb room's state.</summary>
    public static RoomStateSettings HalfieClimb { get; } =
        new(nameof(HalfieClimb), RoomLevelDefinitions.HalfieClimb, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.HalfieClimb, RoomEnemyPopulationDefinitions.HalfieClimb, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.HalfieClimb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.HalfieClimb, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Botwoon room's state 0.</summary>
    public static RoomStateSettings BotwoonDefault { get; } =
        new(nameof(BotwoonDefault), RoomLevelDefinitions.Botwoon, RoomGraphicsSet.SandyMaridia, new RoomMusic(MusicDataSet.MiniBossFight, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Botwoon, RoomEnemyPopulationDefinitions.Botwoon, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Botwoon, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Botwoon room's state 1.</summary>
    public static RoomStateSettings BotwoonWhenAreaMiniBossDead { get; } =
        new(nameof(BotwoonWhenAreaMiniBossDead), RoomLevelDefinitions.Botwoon, RoomGraphicsSet.SandyMaridia, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Botwoon, RoomEnemyPopulationDefinitions.Botwoon, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Botwoon, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Space Jump room's state.</summary>
    public static RoomStateSettings SpaceJump { get; } =
        new(nameof(SpaceJump), RoomLevelDefinitions.SpaceJump, RoomGraphicsSet.SandyMaridia, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SpaceJump, RoomEnemyPopulationDefinitions.SpaceJump, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpaceJump, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpaceJump, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The Maridia Energy Refill room's state.</summary>
    public static RoomStateSettings MaridiaEnergyRefill { get; } =
        new(nameof(MaridiaEnergyRefill), RoomLevelDefinitions.GTEnergyRefill, RoomGraphicsSet.BlueRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaEnergyRefill, RoomEnemyPopulationDefinitions.MaridiaEnergyRefill, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MaridiaEnergyRefill, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MaridiaEnergyRefill, null, RoomSetupCallback.None);


    /// <summary>The West Cactus Alley room's state.</summary>
    public static RoomStateSettings WestCactusAlley { get; } =
        new(nameof(WestCactusAlley), RoomLevelDefinitions.WestCactusAlley, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestCactusAlley, RoomEnemyPopulationDefinitions.WestCactusAlley, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestCactusAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WestCactusAlley, null, RoomSetupCallback.None);


    /// <summary>The East Cactus Alley room's state.</summary>
    public static RoomStateSettings EastCactusAlley { get; } =
        new(nameof(EastCactusAlley), RoomLevelDefinitions.EastCactusAlley, RoomGraphicsSet.SandyMaridia, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastCactusAlley, RoomEnemyPopulationDefinitions.EastCactusAlley, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastCactusAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EastCactusAlley, null, RoomSetupCallback.None);


    /// <summary>The Draygon room's state 0.</summary>
    public static RoomStateSettings DraygonDefault { get; } =
        new(nameof(DraygonDefault), RoomLevelDefinitions.Draygon, RoomGraphicsSet.Draygon, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Draygon, RoomEnemyPopulationDefinitions.DraygonDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Draygon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.DraygonDefault, RoomSetupCallback.SetPausingCodeForDraygon);


    /// <summary>The Draygon room's state 1.</summary>
    public static RoomStateSettings DraygonWhenAreaBossDead { get; } =
        new(nameof(DraygonWhenAreaBossDead), RoomLevelDefinitions.Draygon, RoomGraphicsSet.Draygon, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Draygon, RoomEnemyPopulationDefinitions.DraygonWhenAreaBossDeadOrMotherBrainWhenMainAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Draygon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.DraygonOrPhantoonWhenAreaBossDead, RoomSetupCallback.SetPausingCodeForDraygon);
}
