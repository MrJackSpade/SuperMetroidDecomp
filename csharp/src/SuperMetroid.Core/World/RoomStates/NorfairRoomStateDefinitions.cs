using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Norfair room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class NorfairRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_IceBeamAcid.</summary>
    public static CartridgeRoomState IceBeamAcid { get; } =
        new(nameof(IceBeamAcid), 0xC6E4A4, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamAcid, Enemies.RoomEnemyPopulationDefinitions.IceBeamAcid, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamAcid, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeamAcid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_Cathedral.</summary>
    public static CartridgeRoomState Cathedral { get; } =
        new(nameof(Cathedral), 0xC6E5F5, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Cathedral, Enemies.RoomEnemyPopulationDefinitions.Cathedral, Enemies.RoomEnemyGraphicsSetDefinitions.Cathedral, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Cathedral, null, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_CathedralEntrance.</summary>
    public static CartridgeRoomState CathedralEntrance { get; } =
        new(nameof(CathedralEntrance), 0xC6ECB9, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CathedralEntrance, Enemies.RoomEnemyPopulationDefinitions.CathedralEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.CathedralEntrance, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CathedralEntrance, null, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_BusinessCenter.</summary>
    public static CartridgeRoomState BusinessCenter { get; } =
        new(nameof(BusinessCenter), 0xC6F2E1, 0x0A, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BusinessCenter, Enemies.RoomEnemyPopulationDefinitions.BusinessCenter, Enemies.RoomEnemyGraphicsSetDefinitions.BusinessCenter, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BusinessCenter, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeamGate.</summary>
    public static CartridgeRoomState IceBeamGate { get; } =
        new(nameof(IceBeamGate), 0xC6F8C1, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamGate, Enemies.RoomEnemyPopulationDefinitions.IceBeamGate, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamGate, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.IceBeamGate, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeamGate, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeamTutorial.</summary>
    public static CartridgeRoomState IceBeamTutorial { get; } =
        new(nameof(IceBeamTutorial), 0xC782ED, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamTutorial, Enemies.RoomEnemyPopulationDefinitions.IceBeamTutorial, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamTutorial, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeamTutorial, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeam.</summary>
    public static CartridgeRoomState IceBeam { get; } =
        new(nameof(IceBeam), 0xC784A3, 0x0A, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeam, Enemies.RoomEnemyPopulationDefinitions.IceBeam, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeam, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeam, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeamSnake.</summary>
    public static CartridgeRoomState IceBeamSnake { get; } =
        new(nameof(IceBeamSnake), 0xC785D6, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamSnake, Enemies.RoomEnemyPopulationDefinitions.IceBeamSnake, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamSnake, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.IceBeamSnake, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeamSnake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_CrumbleShaft.</summary>
    public static CartridgeRoomState CrumbleShaft { get; } =
        new(nameof(CrumbleShaft), 0xC78A47, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrumbleShaft, Enemies.RoomEnemyPopulationDefinitions.CrumbleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.CrumbleShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrumbleShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_CrocomireSpeedway.</summary>
    public static CartridgeRoomState CrocomireSpeedway { get; } =
        new(nameof(CrocomireSpeedway), 0xC78CFA, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSpeedway, Enemies.RoomEnemyPopulationDefinitions.CrocomireSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrocomireSpeedway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireSpeedway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_Crocomire_0.</summary>
    public static CartridgeRoomState Crocomire0 { get; } =
        new(nameof(Crocomire0), 0xC79D71, 0x1B, 0x27, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireState01, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainCallback.CrocomireRoomShaking, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.CrocomireState0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_Crocomire_1.</summary>
    public static CartridgeRoomState Crocomire1 { get; } =
        new(nameof(Crocomire1), 0xC79D71, 0x1B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireState01, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainCallback.CrocomireRoomShaking, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.CrocomireState1, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_HiJumpBoots.</summary>
    public static CartridgeRoomState HiJumpBoots { get; } =
        new(nameof(HiJumpBoots), 0xC7A036, 0x0A, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpBoots, Enemies.RoomEnemyPopulationDefinitions.HiJumpBoots, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpBoots, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.HiJumpBoots, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_CrocomireEscape.</summary>
    public static CartridgeRoomState CrocomireEscape { get; } =
        new(nameof(CrocomireEscape), 0xC7A18D, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireEscape, Enemies.RoomEnemyPopulationDefinitions.CrocomireEscape, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireEscape, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrocomireEscape, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireEscape, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_HiJumpETank.</summary>
    public static CartridgeRoomState HiJumpETank { get; } =
        new(nameof(HiJumpETank), 0xC7AA70, 0x0A, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpETank, Enemies.RoomEnemyPopulationDefinitions.HiJumpETank, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.HiJumpETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.HiJumpETank, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocFarming.</summary>
    public static CartridgeRoomState PostCrocFarming { get; } =
        new(nameof(PostCrocFarming), 0xC7AEB3, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocFarming, Enemies.RoomEnemyPopulationDefinitions.PostCrocFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocFarming, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PostCrocFarming, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocFarming, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocSave.</summary>
    public static CartridgeRoomState PostCrocSave { get; } =
        new(nameof(PostCrocSave), 0xCE95C2, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocSave, Enemies.RoomEnemyPopulationDefinitions.PostCrocSave, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocPowerBombs.</summary>
    public static CartridgeRoomState PostCrocPowerBombs { get; } =
        new(nameof(PostCrocPowerBombs), 0xC7B28B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocPowerBombs, Enemies.RoomEnemyPopulationDefinitions.PostCrocPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocPowerBombs, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocShaft.</summary>
    public static CartridgeRoomState PostCrocShaft { get; } =
        new(nameof(PostCrocShaft), 0xC7B3E7, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocShaft, Enemies.RoomEnemyPopulationDefinitions.PostCrocShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PostCrocShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocMissile.</summary>
    public static CartridgeRoomState PostCrocMissile { get; } =
        new(nameof(PostCrocMissile), 0xC7B780, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocMissile, Enemies.RoomEnemyPopulationDefinitions.PostCrocMissile, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocMissile, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocMissile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleTutorial3.</summary>
    public static CartridgeRoomState GrappleTutorial3 { get; } =
        new(nameof(GrappleTutorial3), 0xC7BB6B, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial3, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial3, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial3, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GrappleTutorial3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocJump.</summary>
    public static CartridgeRoomState PostCrocJump { get; } =
        new(nameof(PostCrocJump), 0xC7BECB, 0x0A, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocJump, Enemies.RoomEnemyPopulationDefinitions.PostCrocJump, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocJump, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PostCrocJump, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocJump, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleTutorial2.</summary>
    public static CartridgeRoomState GrappleTutorial2 { get; } =
        new(nameof(GrappleTutorial2), 0xC7CD91, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial2, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial2, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GrappleTutorial2, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GrappleTutorial2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleTutorial1.</summary>
    public static CartridgeRoomState GrappleTutorial1 { get; } =
        new(nameof(GrappleTutorial1), 0xC7CFCD, 0x0A, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial1, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial1, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GrappleTutorial1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleBeam.</summary>
    public static CartridgeRoomState GrappleBeam { get; } =
        new(nameof(GrappleBeam), 0xC7D13C, 0x0A, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleBeam, Enemies.RoomEnemyPopulationDefinitions.GrappleBeam, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleBeam, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GrappleBeam, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GrappleBeam, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_NorfairReserveTank.</summary>
    public static CartridgeRoomState NorfairReserveTank { get; } =
        new(nameof(NorfairReserveTank), 0xC7D4FE, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairReserveTank, Enemies.RoomEnemyPopulationDefinitions.NorfairReserveTank, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairReserveTank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.NorfairReserveTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GreenBubblesMissiles.</summary>
    public static CartridgeRoomState GreenBubblesMissiles { get; } =
        new(nameof(GreenBubblesMissiles), 0xC7D66F, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBubblesMissiles, Enemies.RoomEnemyPopulationDefinitions.GreenBubblesMissiles, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBubblesMissiles, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBubblesMissiles, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBubblesMissiles, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9Bubbles, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_BubbleMountain.</summary>
    public static CartridgeRoomState BubbleMountain { get; } =
        new(nameof(BubbleMountain), 0xC7D895, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountain, Enemies.RoomEnemyPopulationDefinitions.BubbleMountain, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountain, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BubbleMountain, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BubbleMountain, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9Bubbles, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpeedBoosterHall.</summary>
    public static CartridgeRoomState SpeedBoosterHall { get; } =
        new(nameof(SpeedBoosterHall), 0xC7E08C, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBoosterHall, Enemies.RoomEnemyPopulationDefinitions.SpeedBoosterHall, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpeedBoosterHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpeedBooster.</summary>
    public static CartridgeRoomState SpeedBooster { get; } =
        new(nameof(SpeedBooster), 0xC7EAA8, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBooster, Enemies.RoomEnemyPopulationDefinitions.SpeedBooster, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBooster, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpeedBooster, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SingleChamber.</summary>
    public static CartridgeRoomState SingleChamber { get; } =
        new(nameof(SingleChamber), 0xC7EC03, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SingleChamber, Enemies.RoomEnemyPopulationDefinitions.SingleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.SingleChamber, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SingleChamber, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SingleChamber, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_DoubleChamber.</summary>
    public static CartridgeRoomState DoubleChamber { get; } =
        new(nameof(DoubleChamber), 0xC7FF02, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.DoubleChamber, Enemies.RoomEnemyPopulationDefinitions.DoubleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.DoubleChamber, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.DoubleChamber, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DoubleChamber, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_WaveBeam.</summary>
    public static CartridgeRoomState WaveBeam { get; } =
        new(nameof(WaveBeam), 0xC88532, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.WaveBeam, Enemies.RoomEnemyPopulationDefinitions.WaveBeam, Enemies.RoomEnemyGraphicsSetDefinitions.WaveBeam, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WaveBeam, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpikyPlatformsTunnel.</summary>
    public static CartridgeRoomState SpikyPlatformsTunnel { get; } =
        new(nameof(SpikyPlatformsTunnel), 0xC8865C, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyPlatformsTunnel, Enemies.RoomEnemyPopulationDefinitions.SpikyPlatformsTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpikyPlatformsTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_Volcano.</summary>
    public static CartridgeRoomState Volcano { get; } =
        new(nameof(Volcano), 0xC88953, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Volcano, Enemies.RoomEnemyPopulationDefinitions.Volcano, Enemies.RoomEnemyGraphicsSetDefinitions.Volcano, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Volcano, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Volcano, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_KronicBoost.</summary>
    public static CartridgeRoomState KronicBoost { get; } =
        new(nameof(KronicBoost), 0xC8943A, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.KronicBoost, Enemies.RoomEnemyPopulationDefinitions.KronicBoost, Enemies.RoomEnemyGraphicsSetDefinitions.KronicBoost, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.KronicBoost, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.KronicBoost, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_MagdolliteTunnel.</summary>
    public static CartridgeRoomState MagdolliteTunnel { get; } =
        new(nameof(MagdolliteTunnel), 0xC89D5D, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MagdolliteTunnel, Enemies.RoomEnemyPopulationDefinitions.MagdolliteTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MagdolliteTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PurpleShaft.</summary>
    public static CartridgeRoomState PurpleShaft { get; } =
        new(nameof(PurpleShaft), 0xC89FE3, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleShaft, Enemies.RoomEnemyPopulationDefinitions.PurpleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PurpleShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PurpleShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_LavaDive.</summary>
    public static CartridgeRoomState LavaDive { get; } =
        new(nameof(LavaDive), 0xC8A2CF, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LavaDive, Enemies.RoomEnemyPopulationDefinitions.LavaDive, Enemies.RoomEnemyGraphicsSetDefinitions.LavaDive, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LavaDive, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_LowerNorfairElev.</summary>
    public static CartridgeRoomState LowerNorfairElev { get; } =
        new(nameof(LowerNorfairElev), 0xC8AA89, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LowerNorfairElev, Enemies.RoomEnemyPopulationDefinitions.LowerNorfairElev, Enemies.RoomEnemyGraphicsSetDefinitions.LowerNorfairElev, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LowerNorfairElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LowerNorfairElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_UpperNorfairFarming.</summary>
    public static CartridgeRoomState UpperNorfairFarming { get; } =
        new(nameof(UpperNorfairFarming), 0xC8ABED, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairFarming, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairFarming, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.UpperNorfairFarming, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.UpperNorfairFarming, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_RisingTide.</summary>
    public static CartridgeRoomState RisingTide { get; } =
        new(nameof(RisingTide), 0xC8B15B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RisingTide, Enemies.RoomEnemyPopulationDefinitions.RisingTide, Enemies.RoomEnemyGraphicsSetDefinitions.RisingTide, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RisingTide, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_AcidSnakesTunnel.</summary>
    public static CartridgeRoomState AcidSnakesTunnel { get; } =
        new(nameof(AcidSnakesTunnel), 0xC8B4F9, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.AcidSnakesTunnel, Enemies.RoomEnemyPopulationDefinitions.AcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AcidSnakesTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpikyAcidSnakesTunnel.</summary>
    public static CartridgeRoomState SpikyAcidSnakesTunnel { get; } =
        new(nameof(SpikyAcidSnakesTunnel), 0xC8B853, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyAcidSnakesTunnel, Enemies.RoomEnemyPopulationDefinitions.SpikyAcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpikyAcidSnakesTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_UpperNorfairRefill.</summary>
    public static CartridgeRoomState UpperNorfairRefill { get; } =
        new(nameof(UpperNorfairRefill), 0xCE8FA6, 0x17, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairRefill, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairRefill, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.UpperNorfairRefill, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PurpleFarming.</summary>
    public static CartridgeRoomState PurpleFarming { get; } =
        new(nameof(PurpleFarming), 0xC8B9CF, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleFarming, Enemies.RoomEnemyPopulationDefinitions.PurpleFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleFarming, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PurpleFarming, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_BatCave.</summary>
    public static CartridgeRoomState BatCave { get; } =
        new(nameof(BatCave), 0xC8BB21, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BatCave, Enemies.RoomEnemyPopulationDefinitions.BatCave, Enemies.RoomEnemyGraphicsSetDefinitions.BatCave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BatCave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BatCave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_NorfairMap.</summary>
    public static CartridgeRoomState NorfairMap { get; } =
        new(nameof(NorfairMap), 0xCE83C3, 0x15, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairMap, Enemies.RoomEnemyPopulationDefinitions.NorfairMap, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairMap, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.NorfairMap, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_BubbleMountainSave.</summary>
    public static CartridgeRoomState BubbleMountainSave { get; } =
        new(nameof(BubbleMountainSave), 0xCE92CB, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountainSave, Enemies.RoomEnemyPopulationDefinitions.BubbleMountainSave, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountainSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BubbleMountainSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_FrogSpeedway.</summary>
    public static CartridgeRoomState FrogSpeedway { get; } =
        new(nameof(FrogSpeedway), 0xC8BDE8, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSpeedway, Enemies.RoomEnemyPopulationDefinitions.FrogSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSpeedway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FrogSpeedway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FrogSpeedway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_RedPirateShaft.</summary>
    public static CartridgeRoomState RedPirateShaft { get; } =
        new(nameof(RedPirateShaft), 0xC8C165, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RedPirateShaft, Enemies.RoomEnemyPopulationDefinitions.RedPirateShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedPirateShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedPirateShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedPirateShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_FrogSave.</summary>
    public static CartridgeRoomState FrogSave { get; } =
        new(nameof(FrogSave), 0xCE9EF6, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSave, Enemies.RoomEnemyPopulationDefinitions.FrogSave, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FrogSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_CrocomireSave.</summary>
    public static CartridgeRoomState CrocomireSave { get; } =
        new(nameof(CrocomireSave), 0xCE95C2, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSave, Enemies.RoomEnemyPopulationDefinitions.CrocomireSave, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_LNElevSave.</summary>
    public static CartridgeRoomState LNElevSave { get; } =
        new(nameof(LNElevSave), 0xCE92CB, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNElevSave, Enemies.RoomEnemyPopulationDefinitions.LNElevSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNElevSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LNElevSave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNElevSave, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_AcidStatue.</summary>
    public static CartridgeRoomState AcidStatue { get; } =
        new(nameof(AcidStatue), 0xC8C4D3, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.AcidStatue, Enemies.RoomEnemyPopulationDefinitions.AcidStatue, Enemies.RoomEnemyGraphicsSetDefinitions.AcidStatue, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AcidStatue, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AcidStatue, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_MainHall.</summary>
    public static CartridgeRoomState MainHall { get; } =
        new(nameof(MainHall), 0xC8D59C, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.MainHall, Enemies.RoomEnemyPopulationDefinitions.MainHall, Enemies.RoomEnemyGraphicsSetDefinitions.MainHall, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MainHall, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MainHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_GoldenTorizo_0.</summary>
    public static CartridgeRoomState GoldenTorizo0 { get; } =
        new(nameof(GoldenTorizo0), 0xC8E09D, 0x09, 0x24, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizoState01, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GoldenTorizoState01, null, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_GoldenTorizo_1.</summary>
    public static CartridgeRoomState GoldenTorizo1 { get; } =
        new(nameof(GoldenTorizo1), 0xC8E09D, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizoState01, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GoldenTorizoState01, null, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_FastRipper.</summary>
    public static CartridgeRoomState FastRipper { get; } =
        new(nameof(FastRipper), 0xC8E900, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.FastRipper, Enemies.RoomEnemyPopulationDefinitions.FastRipper, Enemies.RoomEnemyGraphicsSetDefinitions.FastRipper, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FastRipper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_GTEnergyRefill.</summary>
    public static CartridgeRoomState GTEnergyRefill { get; } =
        new(nameof(GTEnergyRefill), 0xCE98DC, 0x17, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GTEnergyRefill, Enemies.RoomEnemyPopulationDefinitions.GTEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.GTEnergyRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GTEnergyRefill, null, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Ridley_0.</summary>
    public static CartridgeRoomState Ridley0 { get; } =
        new(nameof(Ridley0), 0xC8EBFD, 0x09, 0x24, 0x04, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyState01, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainCallback.RidleyRoomShaking, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RidleyState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Ridley_1.</summary>
    public static CartridgeRoomState Ridley1 { get; } =
        new(nameof(Ridley1), 0xC8EBFD, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyState01, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainCallback.RidleyRoomShaking, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RidleyState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNFarming.</summary>
    public static CartridgeRoomState LNFarming { get; } =
        new(nameof(LNFarming), 0xC8EDCE, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNFarming, Enemies.RoomEnemyPopulationDefinitions.LNFarming, Enemies.RoomEnemyGraphicsSetDefinitions.LNFarming, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNFarming, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_FastPillarsSetup.</summary>
    public static CartridgeRoomState FastPillarsSetup { get; } =
        new(nameof(FastPillarsSetup), 0xC8F01F, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FastPillarsSetup, Enemies.RoomEnemyPopulationDefinitions.FastPillarsSetup, Enemies.RoomEnemyGraphicsSetDefinitions.FastPillarsSetup, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FastPillarsSetup, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FastPillarsSetup, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9VerticalPatternBrick, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's UNUSED_RoomState_8FB3EE.</summary>
    public static CartridgeRoomState UnusedRoom8FB3EE { get; } =
        new(nameof(UnusedRoom8FB3EE), 0xC8F40B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.UnusedRoom8387EC, Enemies.RoomEnemyPopulationDefinitions.UnusedRoom8FB3EEA1AD06, Enemies.RoomEnemyGraphicsSetDefinitions.UnusedState8FB3EEB48899, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.UnusedRoom8FB3EE, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9HorizontalPatternBrick, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_MickeyMouse.</summary>
    public static CartridgeRoomState MickeyMouse { get; } =
        new(nameof(MickeyMouse), 0xC8F58B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MickeyMouse, Enemies.RoomEnemyPopulationDefinitions.MickeyMouse, Enemies.RoomEnemyGraphicsSetDefinitions.MickeyMouse, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MickeyMouse, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MickeyMouse, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Pillar.</summary>
    public static CartridgeRoomState Pillar { get; } =
        new(nameof(Pillar), 0xC8FCC5, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Pillar, Enemies.RoomEnemyPopulationDefinitions.Pillar, Enemies.RoomEnemyGraphicsSetDefinitions.Pillar, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Pillar, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Plowerhouse.</summary>
    public static CartridgeRoomState Plowerhouse { get; } =
        new(nameof(Plowerhouse), 0xC98222, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Plowerhouse, Enemies.RoomEnemyPopulationDefinitions.Plowerhouse, Enemies.RoomEnemyGraphicsSetDefinitions.Plowerhouse, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Plowerhouse, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_WorstRoomInTheGame.</summary>
    public static CartridgeRoomState WorstRoomInTheGame { get; } =
        new(nameof(WorstRoomInTheGame), 0xC984D3, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WorstRoomInTheGame, Enemies.RoomEnemyPopulationDefinitions.WorstRoomInTheGame, Enemies.RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WorstRoomInTheGame, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WorstRoomInTheGame, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernHorizontalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Amphitheatre.</summary>
    public static CartridgeRoomState Amphitheatre { get; } =
        new(nameof(Amphitheatre), 0xC9899F, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Amphitheatre, Enemies.RoomEnemyPopulationDefinitions.Amphitheatre, Enemies.RoomEnemyGraphicsSetDefinitions.Amphitheatre, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Amphitheatre, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNSpringBallMaze.</summary>
    public static CartridgeRoomState LNSpringBallMaze { get; } =
        new(nameof(LNSpringBallMaze), 0xC994BA, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNSpringballMaze, Enemies.RoomEnemyPopulationDefinitions.LNSpringBallMaze, Enemies.RoomEnemyGraphicsSetDefinitions.LNSpringBallMaze, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LNSpringBallMaze, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNSpringBallMaze, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNEscapePowerBombs.</summary>
    public static CartridgeRoomState LNEscapePowerBombs { get; } =
        new(nameof(LNEscapePowerBombs), 0xC99CE2, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.LNEscapePowerBombs, Enemies.RoomEnemyPopulationDefinitions.LNEscapePowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.LNEscapePowerBombs, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNEscapePowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_RedKihunterShaft.</summary>
    public static CartridgeRoomState RedKihunterShaft { get; } =
        new(nameof(RedKihunterShaft), 0xC99E7B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RedKihunterShaft, Enemies.RoomEnemyPopulationDefinitions.RedKihunterShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedKihunterShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedKihunterShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedKihunterShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Wasteland.</summary>
    public static CartridgeRoomState Wasteland { get; } =
        new(nameof(Wasteland), 0xC9A88C, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Wasteland, Enemies.RoomEnemyPopulationDefinitions.Wasteland, Enemies.RoomEnemyGraphicsSetDefinitions.Wasteland, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Wasteland, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Wasteland, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_MetalPirates.</summary>
    public static CartridgeRoomState MetalPirates { get; } =
        new(nameof(MetalPirates), 0xC9B1C7, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MetalPirates, Enemies.RoomEnemyPopulationDefinitions.MetalPirates, Enemies.RoomEnemyGraphicsSetDefinitions.MetalPirates, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MetalPirates, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_ThreeMusketeers.</summary>
    public static CartridgeRoomState ThreeMusketeers { get; } =
        new(nameof(ThreeMusketeers), 0xC9B4AB, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ThreeMusketeers, Enemies.RoomEnemyPopulationDefinitions.ThreeMusketeers, Enemies.RoomEnemyGraphicsSetDefinitions.ThreeMusketeers, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThreeMusketeers, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ThreeMusketeers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_RidleyETank.</summary>
    public static CartridgeRoomState RidleyETank { get; } =
        new(nameof(RidleyETank), 0xC9C30D, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyETank, Enemies.RoomEnemyPopulationDefinitions.RidleyETank, Enemies.RoomEnemyGraphicsSetDefinitions.RidleyETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RidleyETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_ScrewAttack.</summary>
    public static CartridgeRoomState ScrewAttack { get; } =
        new(nameof(ScrewAttack), 0xC9C428, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.ScrewAttack, Enemies.RoomEnemyPopulationDefinitions.ScrewAttack, Enemies.RoomEnemyGraphicsSetDefinitions.ScrewAttack, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ScrewAttack, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNFireflea.</summary>
    public static CartridgeRoomState LNFireflea { get; } =
        new(nameof(LNFireflea), 0xC9C706, 0x0A, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNFireflea, Enemies.RoomEnemyPopulationDefinitions.LNFireflea, Enemies.RoomEnemyGraphicsSetDefinitions.LNFireflea, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LNFireflea, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNFireflea, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_LNSave.</summary>
    public static CartridgeRoomState LNSave { get; } =
        new(nameof(LNSave), 0xCE95C2, 0x19, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNSave, Enemies.RoomEnemyPopulationDefinitions.LNSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNSave, null, RoomSetupCallback.None);
}
