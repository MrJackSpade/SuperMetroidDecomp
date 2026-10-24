using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Maridia room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class MaridiaRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_GlassTunnelSave.</summary>
    public static CartridgeRoomState GlassTunnelSave { get; } =
        new(nameof(GlassTunnelSave), 0xCE95C2, 0x19, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnelSave, Enemies.RoomEnemyPopulationDefinitions.GlassTunnelSave, Enemies.RoomEnemyGraphicsSetDefinitions.GlassTunnelSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GlassTunnelSave, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_GlassTunnel_0.</summary>
    public static CartridgeRoomState GlassTunnel0 { get; } =
        new(nameof(GlassTunnel0), 0xC9DB52, 0x0B, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnelState01, Enemies.RoomEnemyPopulationDefinitions.GlassTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.GlassTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GlassTunnelState01, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_GlassTunnel_1.</summary>
    public static CartridgeRoomState GlassTunnel1 { get; } =
        new(nameof(GlassTunnel1), 0xC9E129, 0x0B, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.GlassTunnelState01, Enemies.RoomEnemyPopulationDefinitions.GlassTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.GlassTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GlassTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GlassTunnelState01, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_WestTunnel.</summary>
    public static CartridgeRoomState WestTunnel { get; } =
        new(nameof(WestTunnel), 0xC9E6AE, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestTunnel, Enemies.RoomEnemyPopulationDefinitions.WestTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.WestTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WestTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_EastTunnel.</summary>
    public static CartridgeRoomState EastTunnel { get; } =
        new(nameof(EastTunnel), 0xC9E809, 0x0B, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.EastTunnel, Enemies.RoomEnemyPopulationDefinitions.EastTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.EastTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EastTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_MainStreet.</summary>
    public static CartridgeRoomState MainStreet { get; } =
        new(nameof(MainStreet), 0xC9F225, 0x0B, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.MainStreet, Enemies.RoomEnemyPopulationDefinitions.MainStreet, Enemies.RoomEnemyGraphicsSetDefinitions.MainStreet, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MainStreet, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MainStreet, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_FishTank.</summary>
    public static CartridgeRoomState FishTank { get; } =
        new(nameof(FishTank), 0xCA8EFF, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FishTank, Enemies.RoomEnemyPopulationDefinitions.FishTank, Enemies.RoomEnemyGraphicsSetDefinitions.FishTank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FishTank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FishTank, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_MamaTurtle.</summary>
    public static CartridgeRoomState MamaTurtle { get; } =
        new(nameof(MamaTurtle), 0xCAA113, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MamaTurtle, Enemies.RoomEnemyPopulationDefinitions.MamaTurtle, Enemies.RoomEnemyGraphicsSetDefinitions.MamaTurtle, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MamaTurtle, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MamaTurtle, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_CrabTunnel.</summary>
    public static CartridgeRoomState CrabTunnel { get; } =
        new(nameof(CrabTunnel), 0xCAAF99, 0x0B, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabTunnel, Enemies.RoomEnemyPopulationDefinitions.CrabTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.CrabTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrabTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.MaridiaBBrownWall, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_MtEverest.</summary>
    public static CartridgeRoomState MtEverest { get; } =
        new(nameof(MtEverest), 0xCAB24F, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MtEverest, Enemies.RoomEnemyPopulationDefinitions.MtEverest, Enemies.RoomEnemyGraphicsSetDefinitions.MtEverest, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MtEverest, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MtEverest, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_RedFish.</summary>
    public static CartridgeRoomState RedFish { get; } =
        new(nameof(RedFish), 0xCACE42, 0x0B, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedFish, Enemies.RoomEnemyPopulationDefinitions.RedFish, Enemies.RoomEnemyGraphicsSetDefinitions.RedFish, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedFish, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedFish, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_WateringHole.</summary>
    public static CartridgeRoomState WateringHole { get; } =
        new(nameof(WateringHole), 0xCAD474, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WateringHole, Enemies.RoomEnemyPopulationDefinitions.WateringHole, Enemies.RoomEnemyGraphicsSetDefinitions.WateringHole, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WateringHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WateringHole, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_NWestMaridiaBug.</summary>
    public static CartridgeRoomState NWestMaridiaBug { get; } =
        new(nameof(NWestMaridiaBug), 0xCADBC8, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NWestMaridiaBug, Enemies.RoomEnemyPopulationDefinitions.NWestMaridiaBug, Enemies.RoomEnemyGraphicsSetDefinitions.NWestMaridiaBug, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.NWestMaridiaBug, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.NWestMaridiaBug, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_CrabShaft.</summary>
    public static CartridgeRoomState CrabShaft { get; } =
        new(nameof(CrabShaft), 0xCAE458, 0x0B, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabShaft, Enemies.RoomEnemyPopulationDefinitions.CrabShaft, Enemies.RoomEnemyGraphicsSetDefinitions.CrabShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrabShaft, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_PseudoPlasmaSpark.</summary>
    public static CartridgeRoomState PseudoPlasmaSpark { get; } =
        new(nameof(PseudoPlasmaSpark), 0xCAF59C, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PseudoPlasmaSpark, Enemies.RoomEnemyPopulationDefinitions.PseudoPlasmaSpark, Enemies.RoomEnemyGraphicsSetDefinitions.PseudoPlasmaSpark, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PseudoPlasmaSpark, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PseudoPlasmaSpark, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_CrabHole.</summary>
    public static CartridgeRoomState CrabHole { get; } =
        new(nameof(CrabHole), 0xCB83DB, 0x0B, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrabHole, Enemies.RoomEnemyPopulationDefinitions.CrabHole, Enemies.RoomEnemyGraphicsSetDefinitions.CrabHole, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrabHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrabHole, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_WestSandHallTunnel.</summary>
    public static CartridgeRoomState WestSandHallTunnel { get; } =
        new(nameof(WestSandHallTunnel), 0xCB883A, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHallTunnel, Enemies.RoomEnemyPopulationDefinitions.WestSandHallTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.WestSandHallTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestSandHallTunnel, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WestSandHallTunnel, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_PlasmaTutorial.</summary>
    public static CartridgeRoomState PlasmaTutorial { get; } =
        new(nameof(PlasmaTutorial), 0xCB89E0, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaTutorial, Enemies.RoomEnemyPopulationDefinitions.PlasmaTutorial, Enemies.RoomEnemyGraphicsSetDefinitions.PlasmaTutorial, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaTutorial, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PlasmaTutorial, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_Plasma.</summary>
    public static CartridgeRoomState Plasma { get; } =
        new(nameof(Plasma), 0xCB8BD4, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Plasma, Enemies.RoomEnemyPopulationDefinitions.Plasma, Enemies.RoomEnemyGraphicsSetDefinitions.Plasma, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Plasma, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Plasma, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_ThreadTheNeedle.</summary>
    public static CartridgeRoomState ThreadTheNeedle { get; } =
        new(nameof(ThreadTheNeedle), 0xCB9792, 0x0B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ThreadTheNeedle, Enemies.RoomEnemyPopulationDefinitions.ThreadTheNeedle, Enemies.RoomEnemyGraphicsSetDefinitions.ThreadTheNeedle, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThreadTheNeedle, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ThreadTheNeedle, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_MaridiaElev.</summary>
    public static CartridgeRoomState MaridiaElev { get; } =
        new(nameof(MaridiaElev), 0xCBA0D0, 0x0B, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaElev, Enemies.RoomEnemyPopulationDefinitions.MaridiaElev, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaElev, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MaridiaElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MaridiaElev, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_PlasmaSpark.</summary>
    public static CartridgeRoomState PlasmaSpark { get; } =
        new(nameof(PlasmaSpark), 0xCBA878, 0x0B, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaSpark, Enemies.RoomEnemyPopulationDefinitions.PlasmaSpark, Enemies.RoomEnemyGraphicsSetDefinitions.PlasmaSpark, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xE0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaSpark, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PlasmaSpark, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_Kassiuz.</summary>
    public static CartridgeRoomState Kassiuz { get; } =
        new(nameof(Kassiuz), 0xCBC64F, 0x0B, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.Kassiuz, Enemies.RoomEnemyPopulationDefinitions.Kassiuz, Enemies.RoomEnemyGraphicsSetDefinitions.Kassiuz, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kassiuz, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Kassiuz, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_MaridiaMap.</summary>
    public static CartridgeRoomState MaridiaMap { get; } =
        new(nameof(MaridiaMap), 0xCE86BD, 0x15, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMap, Enemies.RoomEnemyPopulationDefinitions.MaridiaMap, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaMap, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MaridiaMap, null, RoomSetupCallback.BossRoomReturnB);


    /// <summary>The disassembly's RoomState_ForgottenHighwaySave.</summary>
    public static CartridgeRoomState ForgottenHighwaySave { get; } =
        new(nameof(ForgottenHighwaySave), 0xCE95C2, 0x19, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ForgottenHighwaySave, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwaySave, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwaySave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ForgottenHighwaySave, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_Toilet.</summary>
    public static CartridgeRoomState Toilet { get; } =
        new(nameof(Toilet), 0xCBCD9F, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Toilet, Enemies.RoomEnemyPopulationDefinitions.Toilet, Enemies.RoomEnemyGraphicsSetDefinitions.Toilet, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.MaridiaElevatube, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Toilet, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_BugSandHole.</summary>
    public static CartridgeRoomState BugSandHole { get; } =
        new(nameof(BugSandHole), 0xCBDCF3, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BugSandHole, Enemies.RoomEnemyPopulationDefinitions.BugSandHole, Enemies.RoomEnemyGraphicsSetDefinitions.BugSandHole, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BugSandHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BugSandHole, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_WestSandHall.</summary>
    public static CartridgeRoomState WestSandHall { get; } =
        new(nameof(WestSandHall), 0xCBDE8F, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHall, Enemies.RoomEnemyPopulationDefinitions.WestSandHall, Enemies.RoomEnemyGraphicsSetDefinitions.WestSandHall, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WestSandHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.MaridiaCGreenWall, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_Oasis.</summary>
    public static CartridgeRoomState Oasis { get; } =
        new(nameof(Oasis), 0xCBE472, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Oasis, Enemies.RoomEnemyPopulationDefinitions.Oasis, Enemies.RoomEnemyGraphicsSetDefinitions.Oasis, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Oasis, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Oasis, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_EastSandHall.</summary>
    public static CartridgeRoomState EastSandHall { get; } =
        new(nameof(EastSandHall), 0xCBE899, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHall, Enemies.RoomEnemyPopulationDefinitions.EastSandHall, Enemies.RoomEnemyGraphicsSetDefinitions.EastSandHall, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EastSandHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.MaridiaCGreenWall, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_WestSandHole.</summary>
    public static CartridgeRoomState WestSandHole { get; } =
        new(nameof(WestSandHole), 0xCBEC32, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestSandHole, Enemies.RoomEnemyPopulationDefinitions.WestSandHole, Enemies.RoomEnemyGraphicsSetDefinitions.WestSandHole, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestSandHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WestSandHole, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_EastSandHole.</summary>
    public static CartridgeRoomState EastSandHole { get; } =
        new(nameof(EastSandHole), 0xCBF580, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastSandHole, Enemies.RoomEnemyPopulationDefinitions.EastSandHole, Enemies.RoomEnemyGraphicsSetDefinitions.EastSandHole, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastSandHole, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EastSandHole, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_WestAqueductQuicksand.</summary>
    public static CartridgeRoomState WestAqueductQuicksand { get; } =
        new(nameof(WestAqueductQuicksand), 0xCBFEC8, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, Enemies.RoomEnemyPopulationDefinitions.WestAqueductQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.WestAqueductQuicksand, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestAqueductQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WestAqueductQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_EastAqueductQuicksand.</summary>
    public static CartridgeRoomState EastAqueductQuicksand { get; } =
        new(nameof(EastAqueductQuicksand), 0xCC80B8, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastAqueductQuicksandWestAqueductQuicksand, Enemies.RoomEnemyPopulationDefinitions.EastAqueductQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.EastAqueductQuicksand, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastAqueductQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EastAqueductQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_Aqueduct.</summary>
    public static CartridgeRoomState Aqueduct { get; } =
        new(nameof(Aqueduct), 0xCC82A8, 0x0C, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.Aqueduct, Enemies.RoomEnemyPopulationDefinitions.Aqueduct, Enemies.RoomEnemyGraphicsSetDefinitions.Aqueduct, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Aqueduct, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Aqueduct, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_Butterfly.</summary>
    public static CartridgeRoomState Butterfly { get; } =
        new(nameof(Butterfly), 0xCCA13B, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Butterfly, Enemies.RoomEnemyPopulationDefinitions.Butterfly, Enemies.RoomEnemyGraphicsSetDefinitions.Butterfly, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Butterfly, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_BotwoonHallway.</summary>
    public static CartridgeRoomState BotwoonHallway { get; } =
        new(nameof(BotwoonHallway), 0xCCA34A, 0x0C, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonHallway, Enemies.RoomEnemyPopulationDefinitions.BotwoonHallway, Enemies.RoomEnemyGraphicsSetDefinitions.BotwoonHallway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonHallway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BotwoonHallway, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_Pants.</summary>
    public static CartridgeRoomState Pants { get; } =
        new(nameof(Pants), 0xCCAC48, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Pants, Enemies.RoomEnemyPopulationDefinitions.Pants, Enemies.RoomEnemyGraphicsSetDefinitions.Pants, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Pants, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Pants, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_EastPants.</summary>
    public static CartridgeRoomState EastPants { get; } =
        new(nameof(EastPants), 0xCCB843, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastPants, Enemies.RoomEnemyPopulationDefinitions.EastPants, Enemies.RoomEnemyGraphicsSetDefinitions.EastPants, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastPants, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EastPants, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_Springball.</summary>
    public static CartridgeRoomState Springball { get; } =
        new(nameof(Springball), 0xCCBD31, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Springball, Enemies.RoomEnemyPopulationDefinitions.Springball, Enemies.RoomEnemyGraphicsSetDefinitions.Springball, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Springball, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Springball, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_BelowBotwoonETank.</summary>
    public static CartridgeRoomState BelowBotwoonETank { get; } =
        new(nameof(BelowBotwoonETank), 0xCCC22F, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BelowBotwoonETank, Enemies.RoomEnemyPopulationDefinitions.BelowBotwoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.BelowBotwoonETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BelowBotwoonETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BelowBotwoonETank, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_Colosseum.</summary>
    public static CartridgeRoomState Colosseum { get; } =
        new(nameof(Colosseum), 0xCCC9F1, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Colosseum, Enemies.RoomEnemyPopulationDefinitions.Colosseum, Enemies.RoomEnemyGraphicsSetDefinitions.Colosseum, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Colosseum, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Colosseum, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_AqueductSave.</summary>
    public static CartridgeRoomState AqueductSave { get; } =
        new(nameof(AqueductSave), 0xCE92CB, 0x19, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.AqueductSave, Enemies.RoomEnemyPopulationDefinitions.AqueductSave, Enemies.RoomEnemyGraphicsSetDefinitions.AqueductSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AqueductSave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AqueductSave, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_ThePrecious_0.</summary>
    public static CartridgeRoomState ThePrecious0 { get; } =
        new(nameof(ThePrecious0), 0xCCE5B1, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ThePreciousState01, Enemies.RoomEnemyPopulationDefinitions.ThePrecious, Enemies.RoomEnemyGraphicsSetDefinitions.ThePrecious, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ThePreciousState01, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_ThePrecious_1.</summary>
    public static CartridgeRoomState ThePrecious1 { get; } =
        new(nameof(ThePrecious1), 0xCCE5B1, 0x0C, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ThePreciousState01, Enemies.RoomEnemyPopulationDefinitions.ThePrecious, Enemies.RoomEnemyGraphicsSetDefinitions.ThePrecious, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThePrecious, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ThePreciousState01, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_BotwoonETank.</summary>
    public static CartridgeRoomState BotwoonETank { get; } =
        new(nameof(BotwoonETank), 0xCCEE0C, 0x0C, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonETank, Enemies.RoomEnemyPopulationDefinitions.BotwoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.BotwoonETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BotwoonETank, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_DraygonSave.</summary>
    public static CartridgeRoomState DraygonSave { get; } =
        new(nameof(DraygonSave), 0xCE9EF6, 0x19, new RoomMusic(MusicDataSet.Maridia, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.DraygonSave, Enemies.RoomEnemyPopulationDefinitions.DraygonSave, Enemies.RoomEnemyGraphicsSetDefinitions.DraygonSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DraygonSave, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_MaridiaMissileRefill.</summary>
    public static CartridgeRoomState MaridiaMissileRefill { get; } =
        new(nameof(MaridiaMissileRefill), 0xCEAB31, 0x17, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaMissileRefill, Enemies.RoomEnemyPopulationDefinitions.MaridiaMissileRefill, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaMissileRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MaridiaMissileRefill, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_PlasmaBeachQuicksand.</summary>
    public static CartridgeRoomState PlasmaBeachQuicksand { get; } =
        new(nameof(PlasmaBeachQuicksand), 0xCCFA8D, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, Enemies.RoomEnemyPopulationDefinitions.PlasmaBeachQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.PlasmaBeachQuicksand, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PlasmaBeachQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PlasmaBeachQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_BotwoonQuicksand.</summary>
    public static CartridgeRoomState BotwoonQuicksand { get; } =
        new(nameof(BotwoonQuicksand), 0xCCFB88, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PlasmaBeachQuicksandBotwoonQuicksand, Enemies.RoomEnemyPopulationDefinitions.BotwoonQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.BotwoonQuicksand, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BotwoonQuicksand, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BotwoonQuicksand, null, RoomSetupCallback.BossRoomReturnC);


    /// <summary>The disassembly's RoomState_Shaktool_0.</summary>
    public static CartridgeRoomState Shaktool0 { get; } =
        new(nameof(Shaktool0), 0xCCFD75, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ShaktoolState01, Enemies.RoomEnemyPopulationDefinitions.Shaktool, Enemies.RoomEnemyGraphicsSetDefinitions.Shaktool, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ShaktoolState01, null, RoomSetupCallback.SetupShaktoolRoomPlm);


    /// <summary>The disassembly's RoomState_Shaktool_1.</summary>
    public static CartridgeRoomState Shaktool1 { get; } =
        new(nameof(Shaktool1), 0xCD8404, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ShaktoolState01, Enemies.RoomEnemyPopulationDefinitions.Shaktool, Enemies.RoomEnemyGraphicsSetDefinitions.Shaktool, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Shaktool, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ShaktoolState01, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The disassembly's RoomState_HalfieClimb.</summary>
    public static CartridgeRoomState HalfieClimb { get; } =
        new(nameof(HalfieClimb), 0xCD8A37, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.HalfieClimb, Enemies.RoomEnemyPopulationDefinitions.HalfieClimb, Enemies.RoomEnemyGraphicsSetDefinitions.HalfieClimb, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.HalfieClimb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.HalfieClimb, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The disassembly's RoomState_Botwoon_0.</summary>
    public static CartridgeRoomState Botwoon0 { get; } =
        new(nameof(Botwoon0), 0xCD950E, 0x0C, new RoomMusic(MusicDataSet.MiniBossFight, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonState01, Enemies.RoomEnemyPopulationDefinitions.Botwoon, Enemies.RoomEnemyGraphicsSetDefinitions.Botwoon, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BotwoonState01, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The disassembly's RoomState_Botwoon_1.</summary>
    public static CartridgeRoomState Botwoon1 { get; } =
        new(nameof(Botwoon1), 0xCD950E, 0x0C, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.BotwoonState01, Enemies.RoomEnemyPopulationDefinitions.Botwoon, Enemies.RoomEnemyGraphicsSetDefinitions.Botwoon, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Botwoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BotwoonState01, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The disassembly's RoomState_SpaceJump.</summary>
    public static CartridgeRoomState SpaceJump { get; } =
        new(nameof(SpaceJump), 0xCD991E, 0x0C, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.SpaceJump, Enemies.RoomEnemyPopulationDefinitions.SpaceJump, Enemies.RoomEnemyGraphicsSetDefinitions.SpaceJump, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpaceJump, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpaceJump, null, RoomSetupCallback.ReturnBeforeDraygonSetup);


    /// <summary>The disassembly's RoomState_MaridiaEnergyRefill.</summary>
    public static CartridgeRoomState MaridiaEnergyRefill { get; } =
        new(nameof(MaridiaEnergyRefill), 0xCE98DC, 0x17, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MaridiaEnergyRefill, Enemies.RoomEnemyPopulationDefinitions.MaridiaEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaEnergyRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MaridiaEnergyRefill, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MaridiaEnergyRefill, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_WestCactusAlley.</summary>
    public static CartridgeRoomState WestCactusAlley { get; } =
        new(nameof(WestCactusAlley), 0xCD9B28, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WestCactusAlley, Enemies.RoomEnemyPopulationDefinitions.WestCactusAlley, Enemies.RoomEnemyGraphicsSetDefinitions.WestCactusAlley, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WestCactusAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WestCactusAlley, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_EastCactusAlley.</summary>
    public static CartridgeRoomState EastCactusAlley { get; } =
        new(nameof(EastCactusAlley), 0xCDA00D, 0x0C, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EastCactusAlley, Enemies.RoomEnemyPopulationDefinitions.EastCactusAlley, Enemies.RoomEnemyGraphicsSetDefinitions.EastCactusAlley, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EastCactusAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EastCactusAlley, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_Draygon_0.</summary>
    public static CartridgeRoomState Draygon0 { get; } =
        new(nameof(Draygon0), 0xCDB19D, 0x1C, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.DraygonState01, Enemies.RoomEnemyPopulationDefinitions.Draygon0, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DraygonState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.DraygonState0, RoomSetupCallback.SetPausingCodeForDraygon);


    /// <summary>The disassembly's RoomState_Draygon_1.</summary>
    public static CartridgeRoomState Draygon1 { get; } =
        new(nameof(Draygon1), 0xCDB19D, 0x1C, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.DraygonState01, Enemies.RoomEnemyPopulationDefinitions.DraygonState1MotherBrainState2, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Draygon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DraygonState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.PhantoonDraygonState1, RoomSetupCallback.SetPausingCodeForDraygon);
}
