namespace SuperMetroid.Core.Rooms;

/// <summary>The initialization settings of each Norfair room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class NorfairRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_IceBeamAcid.</summary>
    public static CartridgeRoomState IceBeamAcid { get; } =
        new(nameof(IceBeamAcid), 0xC6E4A4, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamAcid, Enemies.RoomEnemyPopulationDefinitions.IceBeamAcid, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamAcid, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeamAcid, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_Cathedral.</summary>
    public static CartridgeRoomState Cathedral { get; } =
        new(nameof(Cathedral), 0xC6E5F5, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Cathedral, Enemies.RoomEnemyPopulationDefinitions.Cathedral, Enemies.RoomEnemyGraphicsSetDefinitions.Cathedral, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Cathedral, null, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_CathedralEntrance.</summary>
    public static CartridgeRoomState CathedralEntrance { get; } =
        new(nameof(CathedralEntrance), 0xC6ECB9, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CathedralEntrance, Enemies.RoomEnemyPopulationDefinitions.CathedralEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.CathedralEntrance, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CathedralEntrance, null, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_BusinessCenter.</summary>
    public static CartridgeRoomState BusinessCenter { get; } =
        new(nameof(BusinessCenter), 0xC6F2E1, 0x0A, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BusinessCenter, Enemies.RoomEnemyPopulationDefinitions.BusinessCenter, Enemies.RoomEnemyGraphicsSetDefinitions.BusinessCenter, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BusinessCenter, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeamGate.</summary>
    public static CartridgeRoomState IceBeamGate { get; } =
        new(nameof(IceBeamGate), 0xC6F8C1, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamGate, Enemies.RoomEnemyPopulationDefinitions.IceBeamGate, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamGate, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.IceBeamGate, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeamGate, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeamTutorial.</summary>
    public static CartridgeRoomState IceBeamTutorial { get; } =
        new(nameof(IceBeamTutorial), 0xC782ED, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamTutorial, Enemies.RoomEnemyPopulationDefinitions.IceBeamTutorial, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamTutorial, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeamTutorial, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeam.</summary>
    public static CartridgeRoomState IceBeam { get; } =
        new(nameof(IceBeam), 0xC784A3, 0x0A, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeam, Enemies.RoomEnemyPopulationDefinitions.IceBeam, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeam, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeam, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeamSnake.</summary>
    public static CartridgeRoomState IceBeamSnake { get; } =
        new(nameof(IceBeamSnake), 0xC785D6, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamSnake, Enemies.RoomEnemyPopulationDefinitions.IceBeamSnake, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamSnake, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.IceBeamSnake, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.IceBeamSnake, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_CrumbleShaft.</summary>
    public static CartridgeRoomState CrumbleShaft { get; } =
        new(nameof(CrumbleShaft), 0xC78A47, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrumbleShaft, Enemies.RoomEnemyPopulationDefinitions.CrumbleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.CrumbleShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrumbleShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_CrocomireSpeedway.</summary>
    public static CartridgeRoomState CrocomireSpeedway { get; } =
        new(nameof(CrocomireSpeedway), 0xC78CFA, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSpeedway, Enemies.RoomEnemyPopulationDefinitions.CrocomireSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrocomireSpeedway, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrocomireSpeedway, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_Crocomire_0.</summary>
    public static CartridgeRoomState Crocomire0 { get; } =
        new(nameof(Crocomire0), 0xC79D71, 0x1B, 0x27, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireState01, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainCallback.CrocomireRoomShaking, Plms.RoomPlmPopulationDefinitions.CrocomireState01, Backgrounds.LibraryBackgroundProgramDefinitions.CrocomireState0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_Crocomire_1.</summary>
    public static CartridgeRoomState Crocomire1 { get; } =
        new(nameof(Crocomire1), 0xC79D71, 0x1B, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireState01, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainCallback.CrocomireRoomShaking, Plms.RoomPlmPopulationDefinitions.CrocomireState01, Backgrounds.LibraryBackgroundProgramDefinitions.CrocomireState1, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_HiJumpBoots.</summary>
    public static CartridgeRoomState HiJumpBoots { get; } =
        new(nameof(HiJumpBoots), 0xC7A036, 0x0A, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpBoots, Enemies.RoomEnemyPopulationDefinitions.HiJumpBoots, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpBoots, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.HiJumpBoots, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_CrocomireEscape.</summary>
    public static CartridgeRoomState CrocomireEscape { get; } =
        new(nameof(CrocomireEscape), 0xC7A18D, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireEscape, Enemies.RoomEnemyPopulationDefinitions.CrocomireEscape, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireEscape, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.CrocomireEscape, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrocomireEscape, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_HiJumpETank.</summary>
    public static CartridgeRoomState HiJumpETank { get; } =
        new(nameof(HiJumpETank), 0xC7AA70, 0x0A, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpETank, Enemies.RoomEnemyPopulationDefinitions.HiJumpETank, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpETank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.HiJumpETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.HiJumpETank, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocFarming.</summary>
    public static CartridgeRoomState PostCrocFarming { get; } =
        new(nameof(PostCrocFarming), 0xC7AEB3, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocFarming, Enemies.RoomEnemyPopulationDefinitions.PostCrocFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocFarming, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.PostCrocFarming, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocFarming, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocSave.</summary>
    public static CartridgeRoomState PostCrocSave { get; } =
        new(nameof(PostCrocSave), 0xCE95C2, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocSave, Enemies.RoomEnemyPopulationDefinitions.PostCrocSave, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocPowerBombs.</summary>
    public static CartridgeRoomState PostCrocPowerBombs { get; } =
        new(nameof(PostCrocPowerBombs), 0xC7B28B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocPowerBombs, Enemies.RoomEnemyPopulationDefinitions.PostCrocPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocPowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocPowerBombs, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocShaft.</summary>
    public static CartridgeRoomState PostCrocShaft { get; } =
        new(nameof(PostCrocShaft), 0xC7B3E7, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocShaft, Enemies.RoomEnemyPopulationDefinitions.PostCrocShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.PostCrocShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocMissile.</summary>
    public static CartridgeRoomState PostCrocMissile { get; } =
        new(nameof(PostCrocMissile), 0xC7B780, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocMissile, Enemies.RoomEnemyPopulationDefinitions.PostCrocMissile, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocMissile, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocMissile, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleTutorial3.</summary>
    public static CartridgeRoomState GrappleTutorial3 { get; } =
        new(nameof(GrappleTutorial3), 0xC7BB6B, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial3, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial3, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial3, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GrappleTutorial3, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocJump.</summary>
    public static CartridgeRoomState PostCrocJump { get; } =
        new(nameof(PostCrocJump), 0xC7BECB, 0x0A, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocJump, Enemies.RoomEnemyPopulationDefinitions.PostCrocJump, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocJump, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.PostCrocJump, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PostCrocJump, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleTutorial2.</summary>
    public static CartridgeRoomState GrappleTutorial2 { get; } =
        new(nameof(GrappleTutorial2), 0xC7CD91, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial2, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial2, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GrappleTutorial2, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GrappleTutorial2, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleTutorial1.</summary>
    public static CartridgeRoomState GrappleTutorial1 { get; } =
        new(nameof(GrappleTutorial1), 0xC7CFCD, 0x0A, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial1, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial1, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GrappleTutorial1, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleBeam.</summary>
    public static CartridgeRoomState GrappleBeam { get; } =
        new(nameof(GrappleBeam), 0xC7D13C, 0x0A, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleBeam, Enemies.RoomEnemyPopulationDefinitions.GrappleBeam, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleBeam, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GrappleBeam, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GrappleBeam, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_NorfairReserveTank.</summary>
    public static CartridgeRoomState NorfairReserveTank { get; } =
        new(nameof(NorfairReserveTank), 0xC7D4FE, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairReserveTank, Enemies.RoomEnemyPopulationDefinitions.NorfairReserveTank, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairReserveTank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.NorfairReserveTank, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GreenBubblesMissiles.</summary>
    public static CartridgeRoomState GreenBubblesMissiles { get; } =
        new(nameof(GreenBubblesMissiles), 0xC7D66F, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBubblesMissiles, Enemies.RoomEnemyPopulationDefinitions.GreenBubblesMissiles, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBubblesMissiles, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenBubblesMissiles, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBubblesMissiles, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9Bubbles, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_BubbleMountain.</summary>
    public static CartridgeRoomState BubbleMountain { get; } =
        new(nameof(BubbleMountain), 0xC7D895, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountain, Enemies.RoomEnemyPopulationDefinitions.BubbleMountain, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountain, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BubbleMountain, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BubbleMountain, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9Bubbles, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpeedBoosterHall.</summary>
    public static CartridgeRoomState SpeedBoosterHall { get; } =
        new(nameof(SpeedBoosterHall), 0xC7E08C, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBoosterHall, Enemies.RoomEnemyPopulationDefinitions.SpeedBoosterHall, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpeedBoosterHall, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpeedBooster.</summary>
    public static CartridgeRoomState SpeedBooster { get; } =
        new(nameof(SpeedBooster), 0xC7EAA8, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBooster, Enemies.RoomEnemyPopulationDefinitions.SpeedBooster, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBooster, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpeedBooster, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SingleChamber.</summary>
    public static CartridgeRoomState SingleChamber { get; } =
        new(nameof(SingleChamber), 0xC7EC03, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SingleChamber, Enemies.RoomEnemyPopulationDefinitions.SingleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.SingleChamber, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), Scrolling.RoomScrollDefinitions.SingleChamber, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SingleChamber, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_DoubleChamber.</summary>
    public static CartridgeRoomState DoubleChamber { get; } =
        new(nameof(DoubleChamber), 0xC7FF02, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.DoubleChamber, Enemies.RoomEnemyPopulationDefinitions.DoubleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.DoubleChamber, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.DoubleChamber, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DoubleChamber, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_WaveBeam.</summary>
    public static CartridgeRoomState WaveBeam { get; } =
        new(nameof(WaveBeam), 0xC88532, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.WaveBeam, Enemies.RoomEnemyPopulationDefinitions.WaveBeam, Enemies.RoomEnemyGraphicsSetDefinitions.WaveBeam, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WaveBeam, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpikyPlatformsTunnel.</summary>
    public static CartridgeRoomState SpikyPlatformsTunnel { get; } =
        new(nameof(SpikyPlatformsTunnel), 0xC8865C, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyPlatformsTunnel, Enemies.RoomEnemyPopulationDefinitions.SpikyPlatformsTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpikyPlatformsTunnel, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_Volcano.</summary>
    public static CartridgeRoomState Volcano { get; } =
        new(nameof(Volcano), 0xC88953, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Volcano, Enemies.RoomEnemyPopulationDefinitions.Volcano, Enemies.RoomEnemyGraphicsSetDefinitions.Volcano, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Volcano, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Volcano, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_KronicBoost.</summary>
    public static CartridgeRoomState KronicBoost { get; } =
        new(nameof(KronicBoost), 0xC8943A, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.KronicBoost, Enemies.RoomEnemyPopulationDefinitions.KronicBoost, Enemies.RoomEnemyGraphicsSetDefinitions.KronicBoost, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.KronicBoost, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KronicBoost, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_MagdolliteTunnel.</summary>
    public static CartridgeRoomState MagdolliteTunnel { get; } =
        new(nameof(MagdolliteTunnel), 0xC89D5D, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MagdolliteTunnel, Enemies.RoomEnemyPopulationDefinitions.MagdolliteTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MagdolliteTunnel, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PurpleShaft.</summary>
    public static CartridgeRoomState PurpleShaft { get; } =
        new(nameof(PurpleShaft), 0xC89FE3, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleShaft, Enemies.RoomEnemyPopulationDefinitions.PurpleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.PurpleShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PurpleShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_LavaDive.</summary>
    public static CartridgeRoomState LavaDive { get; } =
        new(nameof(LavaDive), 0xC8A2CF, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LavaDive, Enemies.RoomEnemyPopulationDefinitions.LavaDive, Enemies.RoomEnemyGraphicsSetDefinitions.LavaDive, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LavaDive, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_LowerNorfairElev.</summary>
    public static CartridgeRoomState LowerNorfairElev { get; } =
        new(nameof(LowerNorfairElev), 0xC8AA89, 0x09, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LowerNorfairElev, Enemies.RoomEnemyPopulationDefinitions.LowerNorfairElev, Enemies.RoomEnemyGraphicsSetDefinitions.LowerNorfairElev, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.LowerNorfairElev, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LowerNorfairElev, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_UpperNorfairFarming.</summary>
    public static CartridgeRoomState UpperNorfairFarming { get; } =
        new(nameof(UpperNorfairFarming), 0xC8ABED, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairFarming, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairFarming, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.UpperNorfairFarming, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.UpperNorfairFarming, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_RisingTide.</summary>
    public static CartridgeRoomState RisingTide { get; } =
        new(nameof(RisingTide), 0xC8B15B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RisingTide, Enemies.RoomEnemyPopulationDefinitions.RisingTide, Enemies.RoomEnemyGraphicsSetDefinitions.RisingTide, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RisingTide, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_AcidSnakesTunnel.</summary>
    public static CartridgeRoomState AcidSnakesTunnel { get; } =
        new(nameof(AcidSnakesTunnel), 0xC8B4F9, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.AcidSnakesTunnel, Enemies.RoomEnemyPopulationDefinitions.AcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AcidSnakesTunnel, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpikyAcidSnakesTunnel.</summary>
    public static CartridgeRoomState SpikyAcidSnakesTunnel { get; } =
        new(nameof(SpikyAcidSnakesTunnel), 0xC8B853, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyAcidSnakesTunnel, Enemies.RoomEnemyPopulationDefinitions.SpikyAcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpikyAcidSnakesTunnel, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_UpperNorfairRefill.</summary>
    public static CartridgeRoomState UpperNorfairRefill { get; } =
        new(nameof(UpperNorfairRefill), 0xCE8FA6, 0x17, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairRefill, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairRefill, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.UpperNorfairRefill, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PurpleFarming.</summary>
    public static CartridgeRoomState PurpleFarming { get; } =
        new(nameof(PurpleFarming), 0xC8B9CF, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleFarming, Enemies.RoomEnemyPopulationDefinitions.PurpleFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleFarming, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PurpleFarming, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_BatCave.</summary>
    public static CartridgeRoomState BatCave { get; } =
        new(nameof(BatCave), 0xC8BB21, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BatCave, Enemies.RoomEnemyPopulationDefinitions.BatCave, Enemies.RoomEnemyGraphicsSetDefinitions.BatCave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BatCave, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BatCave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_NorfairMap.</summary>
    public static CartridgeRoomState NorfairMap { get; } =
        new(nameof(NorfairMap), 0xCE83C3, 0x15, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairMap, Enemies.RoomEnemyPopulationDefinitions.NorfairMap, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.NorfairMap, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_BubbleMountainSave.</summary>
    public static CartridgeRoomState BubbleMountainSave { get; } =
        new(nameof(BubbleMountainSave), 0xCE92CB, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountainSave, Enemies.RoomEnemyPopulationDefinitions.BubbleMountainSave, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountainSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BubbleMountainSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_FrogSpeedway.</summary>
    public static CartridgeRoomState FrogSpeedway { get; } =
        new(nameof(FrogSpeedway), 0xC8BDE8, 0x0A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSpeedway, Enemies.RoomEnemyPopulationDefinitions.FrogSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSpeedway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.FrogSpeedway, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FrogSpeedway, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_RedPirateShaft.</summary>
    public static CartridgeRoomState RedPirateShaft { get; } =
        new(nameof(RedPirateShaft), 0xC8C165, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RedPirateShaft, Enemies.RoomEnemyPopulationDefinitions.RedPirateShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedPirateShaft, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.RedPirateShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedPirateShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_FrogSave.</summary>
    public static CartridgeRoomState FrogSave { get; } =
        new(nameof(FrogSave), 0xCE9EF6, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSave, Enemies.RoomEnemyPopulationDefinitions.FrogSave, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FrogSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_CrocomireSave.</summary>
    public static CartridgeRoomState CrocomireSave { get; } =
        new(nameof(CrocomireSave), 0xCE95C2, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSave, Enemies.RoomEnemyPopulationDefinitions.CrocomireSave, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.CrocomireSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_LNElevSave.</summary>
    public static CartridgeRoomState LNElevSave { get; } =
        new(nameof(LNElevSave), 0xCE92CB, 0x19, 0x15, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNElevSave, Enemies.RoomEnemyPopulationDefinitions.LNElevSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNElevSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.LNElevSave, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNElevSave, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_AcidStatue.</summary>
    public static CartridgeRoomState AcidStatue { get; } =
        new(nameof(AcidStatue), 0xC8C4D3, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.AcidStatue, Enemies.RoomEnemyPopulationDefinitions.AcidStatue, Enemies.RoomEnemyGraphicsSetDefinitions.AcidStatue, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AcidStatue, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AcidStatue, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_MainHall.</summary>
    public static CartridgeRoomState MainHall { get; } =
        new(nameof(MainHall), 0xC8D59C, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.MainHall, Enemies.RoomEnemyPopulationDefinitions.MainHall, Enemies.RoomEnemyGraphicsSetDefinitions.MainHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.MainHall, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MainHall, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_GoldenTorizo_0.</summary>
    public static CartridgeRoomState GoldenTorizo0 { get; } =
        new(nameof(GoldenTorizo0), 0xC8E09D, 0x09, 0x24, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizoState01, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GoldenTorizoState01, null, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_GoldenTorizo_1.</summary>
    public static CartridgeRoomState GoldenTorizo1 { get; } =
        new(nameof(GoldenTorizo1), 0xC8E09D, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizoState01, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GoldenTorizoState01, null, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_FastRipper.</summary>
    public static CartridgeRoomState FastRipper { get; } =
        new(nameof(FastRipper), 0xC8E900, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.FastRipper, Enemies.RoomEnemyPopulationDefinitions.FastRipper, Enemies.RoomEnemyGraphicsSetDefinitions.FastRipper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FastRipper, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_GTEnergyRefill.</summary>
    public static CartridgeRoomState GTEnergyRefill { get; } =
        new(nameof(GTEnergyRefill), 0xCE98DC, 0x17, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.GTEnergyRefill, Enemies.RoomEnemyPopulationDefinitions.GTEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.GTEnergyRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GTEnergyRefill, null, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Ridley_0.</summary>
    public static CartridgeRoomState Ridley0 { get; } =
        new(nameof(Ridley0), 0xC8EBFD, 0x09, 0x24, 0x04, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyState01, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainCallback.RidleyRoomShaking, Plms.RoomPlmPopulationDefinitions.RidleyState01, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Ridley_1.</summary>
    public static CartridgeRoomState Ridley1 { get; } =
        new(nameof(Ridley1), 0xC8EBFD, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyState01, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainCallback.RidleyRoomShaking, Plms.RoomPlmPopulationDefinitions.RidleyState01, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNFarming.</summary>
    public static CartridgeRoomState LNFarming { get; } =
        new(nameof(LNFarming), 0xC8EDCE, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNFarming, Enemies.RoomEnemyPopulationDefinitions.LNFarming, Enemies.RoomEnemyGraphicsSetDefinitions.LNFarming, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNFarming, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_FastPillarsSetup.</summary>
    public static CartridgeRoomState FastPillarsSetup { get; } =
        new(nameof(FastPillarsSetup), 0xC8F01F, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FastPillarsSetup, Enemies.RoomEnemyPopulationDefinitions.FastPillarsSetup, Enemies.RoomEnemyGraphicsSetDefinitions.FastPillarsSetup, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FastPillarsSetup, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FastPillarsSetup, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9VerticalPatternBrick, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's UNUSED_RoomState_8FB3EE.</summary>
    public static CartridgeRoomState UnusedRoom8FB3EE { get; } =
        new(nameof(UnusedRoom8FB3EE), 0xC8F40B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.UnusedRoom8387EC, Enemies.RoomEnemyPopulationDefinitions.UnusedRoom8FB3EEA1AD06, Enemies.RoomEnemyGraphicsSetDefinitions.UnusedState8FB3EEB48899, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.UnusedRoom8FB3EE, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9HorizontalPatternBrick, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_MickeyMouse.</summary>
    public static CartridgeRoomState MickeyMouse { get; } =
        new(nameof(MickeyMouse), 0xC8F58B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MickeyMouse, Enemies.RoomEnemyPopulationDefinitions.MickeyMouse, Enemies.RoomEnemyGraphicsSetDefinitions.MickeyMouse, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MickeyMouse, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MickeyMouse, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Pillar.</summary>
    public static CartridgeRoomState Pillar { get; } =
        new(nameof(Pillar), 0xC8FCC5, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Pillar, Enemies.RoomEnemyPopulationDefinitions.Pillar, Enemies.RoomEnemyGraphicsSetDefinitions.Pillar, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Pillar, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Plowerhouse.</summary>
    public static CartridgeRoomState Plowerhouse { get; } =
        new(nameof(Plowerhouse), 0xC98222, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Plowerhouse, Enemies.RoomEnemyPopulationDefinitions.Plowerhouse, Enemies.RoomEnemyGraphicsSetDefinitions.Plowerhouse, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Plowerhouse, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_WorstRoomInTheGame.</summary>
    public static CartridgeRoomState WorstRoomInTheGame { get; } =
        new(nameof(WorstRoomInTheGame), 0xC984D3, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WorstRoomInTheGame, Enemies.RoomEnemyPopulationDefinitions.WorstRoomInTheGame, Enemies.RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WorstRoomInTheGame, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WorstRoomInTheGame, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernHorizontalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Amphitheatre.</summary>
    public static CartridgeRoomState Amphitheatre { get; } =
        new(nameof(Amphitheatre), 0xC9899F, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Amphitheatre, Enemies.RoomEnemyPopulationDefinitions.Amphitheatre, Enemies.RoomEnemyGraphicsSetDefinitions.Amphitheatre, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Amphitheatre, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNSpringBallMaze.</summary>
    public static CartridgeRoomState LNSpringBallMaze { get; } =
        new(nameof(LNSpringBallMaze), 0xC994BA, 0x09, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNSpringballMaze, Enemies.RoomEnemyPopulationDefinitions.LNSpringBallMaze, Enemies.RoomEnemyGraphicsSetDefinitions.LNSpringBallMaze, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.LNSpringBallMaze, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNSpringBallMaze, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNEscapePowerBombs.</summary>
    public static CartridgeRoomState LNEscapePowerBombs { get; } =
        new(nameof(LNEscapePowerBombs), 0xC99CE2, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.LNEscapePowerBombs, Enemies.RoomEnemyPopulationDefinitions.LNEscapePowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.LNEscapePowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNEscapePowerBombs, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_RedKihunterShaft.</summary>
    public static CartridgeRoomState RedKihunterShaft { get; } =
        new(nameof(RedKihunterShaft), 0xC99E7B, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RedKihunterShaft, Enemies.RoomEnemyPopulationDefinitions.RedKihunterShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedKihunterShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RedKihunterShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedKihunterShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Wasteland.</summary>
    public static CartridgeRoomState Wasteland { get; } =
        new(nameof(Wasteland), 0xC9A88C, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Wasteland, Enemies.RoomEnemyPopulationDefinitions.Wasteland, Enemies.RoomEnemyGraphicsSetDefinitions.Wasteland, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Wasteland, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Wasteland, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_MetalPirates.</summary>
    public static CartridgeRoomState MetalPirates { get; } =
        new(nameof(MetalPirates), 0xC9B1C7, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MetalPirates, Enemies.RoomEnemyPopulationDefinitions.MetalPirates, Enemies.RoomEnemyGraphicsSetDefinitions.MetalPirates, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MetalPirates, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_ThreeMusketeers.</summary>
    public static CartridgeRoomState ThreeMusketeers { get; } =
        new(nameof(ThreeMusketeers), 0xC9B4AB, 0x09, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ThreeMusketeers, Enemies.RoomEnemyPopulationDefinitions.ThreeMusketeers, Enemies.RoomEnemyGraphicsSetDefinitions.ThreeMusketeers, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ThreeMusketeers, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ThreeMusketeers, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_RidleyETank.</summary>
    public static CartridgeRoomState RidleyETank { get; } =
        new(nameof(RidleyETank), 0xC9C30D, 0x09, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyETank, Enemies.RoomEnemyPopulationDefinitions.RidleyETank, Enemies.RoomEnemyGraphicsSetDefinitions.RidleyETank, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RidleyETank, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_ScrewAttack.</summary>
    public static CartridgeRoomState ScrewAttack { get; } =
        new(nameof(ScrewAttack), 0xC9C428, 0x09, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.ScrewAttack, Enemies.RoomEnemyPopulationDefinitions.ScrewAttack, Enemies.RoomEnemyGraphicsSetDefinitions.ScrewAttack, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ScrewAttack, Backgrounds.LibraryBackgroundProgramDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNFireflea.</summary>
    public static CartridgeRoomState LNFireflea { get; } =
        new(nameof(LNFireflea), 0xC9C706, 0x0A, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNFireflea, Enemies.RoomEnemyPopulationDefinitions.LNFireflea, Enemies.RoomEnemyGraphicsSetDefinitions.LNFireflea, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.LNFireflea, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNFireflea, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_LNSave.</summary>
    public static CartridgeRoomState LNSave { get; } =
        new(nameof(LNSave), 0xCE95C2, 0x19, 0x18, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.LNSave, Enemies.RoomEnemyPopulationDefinitions.LNSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.LNSave, null, RoomSetupCallback.None);
}
