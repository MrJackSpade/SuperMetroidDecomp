namespace SuperMetroid.Core.Rooms;

/// <summary>The initialization settings of each Maridia room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class MaridiaRoomStateDefinitions
{

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
}
