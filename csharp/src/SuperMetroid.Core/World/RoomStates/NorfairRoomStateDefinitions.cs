using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Norfair room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class NorfairRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_IceBeamAcid.</summary>
    public static RoomStateSettings IceBeamAcid { get; } =
        new(nameof(IceBeamAcid), RoomLevelDefinitions.IceBeamAcid, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamAcid, Enemies.RoomEnemyPopulationDefinitions.IceBeamAcid, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamAcid, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeamAcid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_Cathedral.</summary>
    public static RoomStateSettings Cathedral { get; } =
        new(nameof(Cathedral), RoomLevelDefinitions.Cathedral, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Cathedral, Enemies.RoomEnemyPopulationDefinitions.Cathedral, Enemies.RoomEnemyGraphicsSetDefinitions.Cathedral, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Cathedral, null, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_CathedralEntrance.</summary>
    public static RoomStateSettings CathedralEntrance { get; } =
        new(nameof(CathedralEntrance), RoomLevelDefinitions.CathedralEntrance, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CathedralEntrance, Enemies.RoomEnemyPopulationDefinitions.CathedralEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.CathedralEntrance, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CathedralEntrance, null, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_BusinessCenter.</summary>
    public static RoomStateSettings BusinessCenter { get; } =
        new(nameof(BusinessCenter), RoomLevelDefinitions.BusinessCenter, 0x0A, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BusinessCenter, Enemies.RoomEnemyPopulationDefinitions.BusinessCenter, Enemies.RoomEnemyGraphicsSetDefinitions.BusinessCenter, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BusinessCenter, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeamGate.</summary>
    public static RoomStateSettings IceBeamGate { get; } =
        new(nameof(IceBeamGate), RoomLevelDefinitions.IceBeamGate, 0x0A, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamGate, Enemies.RoomEnemyPopulationDefinitions.IceBeamGate, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamGate, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.IceBeamGate, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeamGate, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeamTutorial.</summary>
    public static RoomStateSettings IceBeamTutorial { get; } =
        new(nameof(IceBeamTutorial), RoomLevelDefinitions.IceBeamTutorial, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamTutorial, Enemies.RoomEnemyPopulationDefinitions.IceBeamTutorial, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamTutorial, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeamTutorial, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeam.</summary>
    public static RoomStateSettings IceBeam { get; } =
        new(nameof(IceBeam), RoomLevelDefinitions.IceBeam, 0x0A, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeam, Enemies.RoomEnemyPopulationDefinitions.IceBeam, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeam, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeam, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_IceBeamSnake.</summary>
    public static RoomStateSettings IceBeamSnake { get; } =
        new(nameof(IceBeamSnake), RoomLevelDefinitions.IceBeamSnake, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamSnake, Enemies.RoomEnemyPopulationDefinitions.IceBeamSnake, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamSnake, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.IceBeamSnake, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.IceBeamSnake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_CrumbleShaft.</summary>
    public static RoomStateSettings CrumbleShaft { get; } =
        new(nameof(CrumbleShaft), RoomLevelDefinitions.CrumbleShaft, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrumbleShaft, Enemies.RoomEnemyPopulationDefinitions.CrumbleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.CrumbleShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrumbleShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_CrocomireSpeedway.</summary>
    public static RoomStateSettings CrocomireSpeedway { get; } =
        new(nameof(CrocomireSpeedway), RoomLevelDefinitions.CrocomireSpeedway, 0x09, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSpeedway, Enemies.RoomEnemyPopulationDefinitions.CrocomireSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrocomireSpeedway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireSpeedway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnB);


    /// <summary>The disassembly's RoomState_Crocomire_0.</summary>
    public static RoomStateSettings Crocomire0 { get; } =
        new(nameof(Crocomire0), RoomLevelDefinitions.Crocomire, 0x1B, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireState01, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainCallback.CrocomireRoomShaking, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrocomireState0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_Crocomire_1.</summary>
    public static RoomStateSettings Crocomire1 { get; } =
        new(nameof(Crocomire1), RoomLevelDefinitions.Crocomire, 0x1B, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireState01, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainCallback.CrocomireRoomShaking, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrocomireState1, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_HiJumpBoots.</summary>
    public static RoomStateSettings HiJumpBoots { get; } =
        new(nameof(HiJumpBoots), RoomLevelDefinitions.HiJumpBoots, 0x0A, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpBoots, Enemies.RoomEnemyPopulationDefinitions.HiJumpBoots, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpBoots, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.HiJumpBoots, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_CrocomireEscape.</summary>
    public static RoomStateSettings CrocomireEscape { get; } =
        new(nameof(CrocomireEscape), RoomLevelDefinitions.CrocomireEscape, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireEscape, Enemies.RoomEnemyPopulationDefinitions.CrocomireEscape, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireEscape, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrocomireEscape, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireEscape, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_HiJumpETank.</summary>
    public static RoomStateSettings HiJumpETank { get; } =
        new(nameof(HiJumpETank), RoomLevelDefinitions.HiJumpETank, 0x0A, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpETank, Enemies.RoomEnemyPopulationDefinitions.HiJumpETank, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.HiJumpETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.HiJumpETank, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocFarming.</summary>
    public static RoomStateSettings PostCrocFarming { get; } =
        new(nameof(PostCrocFarming), RoomLevelDefinitions.PostCrocFarming, 0x09, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocFarming, Enemies.RoomEnemyPopulationDefinitions.PostCrocFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocFarming, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PostCrocFarming, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocFarming, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocSave.</summary>
    public static RoomStateSettings PostCrocSave { get; } =
        new(nameof(PostCrocSave), RoomLevelDefinitions.WarehouseSave, 0x19, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocSave, Enemies.RoomEnemyPopulationDefinitions.PostCrocSave, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocPowerBombs.</summary>
    public static RoomStateSettings PostCrocPowerBombs { get; } =
        new(nameof(PostCrocPowerBombs), RoomLevelDefinitions.PostCrocPowerBombs, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocPowerBombs, Enemies.RoomEnemyPopulationDefinitions.PostCrocPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocPowerBombs, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocShaft.</summary>
    public static RoomStateSettings PostCrocShaft { get; } =
        new(nameof(PostCrocShaft), RoomLevelDefinitions.PostCrocShaft, 0x0A, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocShaft, Enemies.RoomEnemyPopulationDefinitions.PostCrocShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PostCrocShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocMissile.</summary>
    public static RoomStateSettings PostCrocMissile { get; } =
        new(nameof(PostCrocMissile), RoomLevelDefinitions.PostCrocMissile, 0x0A, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocMissile, Enemies.RoomEnemyPopulationDefinitions.PostCrocMissile, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocMissile, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocMissile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleTutorial3.</summary>
    public static RoomStateSettings GrappleTutorial3 { get; } =
        new(nameof(GrappleTutorial3), RoomLevelDefinitions.GrappleTutorial3, 0x0A, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial3, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial3, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial3, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GrappleTutorial3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PostCrocJump.</summary>
    public static RoomStateSettings PostCrocJump { get; } =
        new(nameof(PostCrocJump), RoomLevelDefinitions.PostCrocJump, 0x0A, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocJump, Enemies.RoomEnemyPopulationDefinitions.PostCrocJump, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocJump, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PostCrocJump, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PostCrocJump, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleTutorial2.</summary>
    public static RoomStateSettings GrappleTutorial2 { get; } =
        new(nameof(GrappleTutorial2), RoomLevelDefinitions.GrappleTutorial2, 0x0A, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial2, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial2, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GrappleTutorial2, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GrappleTutorial2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleTutorial1.</summary>
    public static RoomStateSettings GrappleTutorial1 { get; } =
        new(nameof(GrappleTutorial1), RoomLevelDefinitions.GrappleTutorial1, 0x0A, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial1, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial1, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GrappleTutorial1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GrappleBeam.</summary>
    public static RoomStateSettings GrappleBeam { get; } =
        new(nameof(GrappleBeam), RoomLevelDefinitions.GrappleBeam, 0x0A, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleBeam, Enemies.RoomEnemyPopulationDefinitions.GrappleBeam, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleBeam, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GrappleBeam, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GrappleBeam, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_NorfairReserveTank.</summary>
    public static RoomStateSettings NorfairReserveTank { get; } =
        new(nameof(NorfairReserveTank), RoomLevelDefinitions.NorfairReserveTank, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairReserveTank, Enemies.RoomEnemyPopulationDefinitions.NorfairReserveTank, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairReserveTank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.NorfairReserveTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_GreenBubblesMissiles.</summary>
    public static RoomStateSettings GreenBubblesMissiles { get; } =
        new(nameof(GreenBubblesMissiles), RoomLevelDefinitions.GreenBubblesMissiles, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBubblesMissiles, Enemies.RoomEnemyPopulationDefinitions.GreenBubblesMissiles, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBubblesMissiles, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBubblesMissiles, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBubblesMissiles, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9Bubbles, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_BubbleMountain.</summary>
    public static RoomStateSettings BubbleMountain { get; } =
        new(nameof(BubbleMountain), RoomLevelDefinitions.BubbleMountain, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountain, Enemies.RoomEnemyPopulationDefinitions.BubbleMountain, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountain, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BubbleMountain, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BubbleMountain, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9Bubbles, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpeedBoosterHall.</summary>
    public static RoomStateSettings SpeedBoosterHall { get; } =
        new(nameof(SpeedBoosterHall), RoomLevelDefinitions.SpeedBoosterHall, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBoosterHall, Enemies.RoomEnemyPopulationDefinitions.SpeedBoosterHall, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpeedBoosterHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpeedBooster.</summary>
    public static RoomStateSettings SpeedBooster { get; } =
        new(nameof(SpeedBooster), RoomLevelDefinitions.SpeedBooster, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBooster, Enemies.RoomEnemyPopulationDefinitions.SpeedBooster, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBooster, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpeedBooster, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SingleChamber.</summary>
    public static RoomStateSettings SingleChamber { get; } =
        new(nameof(SingleChamber), RoomLevelDefinitions.SingleChamber, 0x09, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.SingleChamber, Enemies.RoomEnemyPopulationDefinitions.SingleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.SingleChamber, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SingleChamber, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SingleChamber, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_DoubleChamber.</summary>
    public static RoomStateSettings DoubleChamber { get; } =
        new(nameof(DoubleChamber), RoomLevelDefinitions.DoubleChamber, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.DoubleChamber, Enemies.RoomEnemyPopulationDefinitions.DoubleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.DoubleChamber, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.DoubleChamber, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DoubleChamber, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_WaveBeam.</summary>
    public static RoomStateSettings WaveBeam { get; } =
        new(nameof(WaveBeam), RoomLevelDefinitions.WaveBeam, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.WaveBeam, Enemies.RoomEnemyPopulationDefinitions.WaveBeam, Enemies.RoomEnemyGraphicsSetDefinitions.WaveBeam, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WaveBeam, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpikyPlatformsTunnel.</summary>
    public static RoomStateSettings SpikyPlatformsTunnel { get; } =
        new(nameof(SpikyPlatformsTunnel), RoomLevelDefinitions.SpikyPlatformsTunnel, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyPlatformsTunnel, Enemies.RoomEnemyPopulationDefinitions.SpikyPlatformsTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpikyPlatformsTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_Volcano.</summary>
    public static RoomStateSettings Volcano { get; } =
        new(nameof(Volcano), RoomLevelDefinitions.Volcano, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Volcano, Enemies.RoomEnemyPopulationDefinitions.Volcano, Enemies.RoomEnemyGraphicsSetDefinitions.Volcano, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Volcano, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Volcano, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_KronicBoost.</summary>
    public static RoomStateSettings KronicBoost { get; } =
        new(nameof(KronicBoost), RoomLevelDefinitions.KronicBoost, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KronicBoost, Enemies.RoomEnemyPopulationDefinitions.KronicBoost, Enemies.RoomEnemyGraphicsSetDefinitions.KronicBoost, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.KronicBoost, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.KronicBoost, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_MagdolliteTunnel.</summary>
    public static RoomStateSettings MagdolliteTunnel { get; } =
        new(nameof(MagdolliteTunnel), RoomLevelDefinitions.MagdolliteTunnel, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagdolliteTunnel, Enemies.RoomEnemyPopulationDefinitions.MagdolliteTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MagdolliteTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PurpleShaft.</summary>
    public static RoomStateSettings PurpleShaft { get; } =
        new(nameof(PurpleShaft), RoomLevelDefinitions.PurpleShaft, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleShaft, Enemies.RoomEnemyPopulationDefinitions.PurpleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PurpleShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PurpleShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_LavaDive.</summary>
    public static RoomStateSettings LavaDive { get; } =
        new(nameof(LavaDive), RoomLevelDefinitions.LavaDive, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LavaDive, Enemies.RoomEnemyPopulationDefinitions.LavaDive, Enemies.RoomEnemyGraphicsSetDefinitions.LavaDive, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LavaDive, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_LowerNorfairElev.</summary>
    public static RoomStateSettings LowerNorfairElev { get; } =
        new(nameof(LowerNorfairElev), RoomLevelDefinitions.LowerNorfairElev, 0x09, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LowerNorfairElev, Enemies.RoomEnemyPopulationDefinitions.LowerNorfairElev, Enemies.RoomEnemyGraphicsSetDefinitions.LowerNorfairElev, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LowerNorfairElev, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LowerNorfairElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_UpperNorfairFarming.</summary>
    public static RoomStateSettings UpperNorfairFarming { get; } =
        new(nameof(UpperNorfairFarming), RoomLevelDefinitions.UpperNorfairFarming, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairFarming, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairFarming, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.UpperNorfairFarming, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.UpperNorfairFarming, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_RisingTide.</summary>
    public static RoomStateSettings RisingTide { get; } =
        new(nameof(RisingTide), RoomLevelDefinitions.RisingTide, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RisingTide, Enemies.RoomEnemyPopulationDefinitions.RisingTide, Enemies.RoomEnemyGraphicsSetDefinitions.RisingTide, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RisingTide, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_AcidSnakesTunnel.</summary>
    public static RoomStateSettings AcidSnakesTunnel { get; } =
        new(nameof(AcidSnakesTunnel), RoomLevelDefinitions.AcidSnakesTunnel, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AcidSnakesTunnel, Enemies.RoomEnemyPopulationDefinitions.AcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AcidSnakesTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_SpikyAcidSnakesTunnel.</summary>
    public static RoomStateSettings SpikyAcidSnakesTunnel { get; } =
        new(nameof(SpikyAcidSnakesTunnel), RoomLevelDefinitions.SpikyAcidSnakesTunnel, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyAcidSnakesTunnel, Enemies.RoomEnemyPopulationDefinitions.SpikyAcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpikyAcidSnakesTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernStalactites, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_UpperNorfairRefill.</summary>
    public static RoomStateSettings UpperNorfairRefill { get; } =
        new(nameof(UpperNorfairRefill), RoomLevelDefinitions.UpperNorfairRefill, 0x17, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairRefill, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairRefill, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.UpperNorfairRefill, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_PurpleFarming.</summary>
    public static RoomStateSettings PurpleFarming { get; } =
        new(nameof(PurpleFarming), RoomLevelDefinitions.PurpleFarming, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleFarming, Enemies.RoomEnemyPopulationDefinitions.PurpleFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleFarming, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PurpleFarming, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_BatCave.</summary>
    public static RoomStateSettings BatCave { get; } =
        new(nameof(BatCave), RoomLevelDefinitions.BatCave, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BatCave, Enemies.RoomEnemyPopulationDefinitions.BatCave, Enemies.RoomEnemyGraphicsSetDefinitions.BatCave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BatCave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BatCave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_NorfairMap.</summary>
    public static RoomStateSettings NorfairMap { get; } =
        new(nameof(NorfairMap), RoomLevelDefinitions.GreenBrinstarMap, 0x15, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairMap, Enemies.RoomEnemyPopulationDefinitions.NorfairMap, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairMap, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.NorfairMap, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_BubbleMountainSave.</summary>
    public static RoomStateSettings BubbleMountainSave { get; } =
        new(nameof(BubbleMountainSave), RoomLevelDefinitions.CrateriaSave, 0x19, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountainSave, Enemies.RoomEnemyPopulationDefinitions.BubbleMountainSave, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountainSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BubbleMountainSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_FrogSpeedway.</summary>
    public static RoomStateSettings FrogSpeedway { get; } =
        new(nameof(FrogSpeedway), RoomLevelDefinitions.FrogSpeedway, 0x0A, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSpeedway, Enemies.RoomEnemyPopulationDefinitions.FrogSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSpeedway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FrogSpeedway, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FrogSpeedway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurpleBright, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_RedPirateShaft.</summary>
    public static RoomStateSettings RedPirateShaft { get; } =
        new(nameof(RedPirateShaft), RoomLevelDefinitions.RedPirateShaft, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RedPirateShaft, Enemies.RoomEnemyPopulationDefinitions.RedPirateShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedPirateShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedPirateShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedPirateShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ACavernVertical, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_FrogSave.</summary>
    public static RoomStateSettings FrogSave { get; } =
        new(nameof(FrogSave), RoomLevelDefinitions.FrogSave, 0x19, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSave, Enemies.RoomEnemyPopulationDefinitions.FrogSave, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FrogSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_CrocomireSave.</summary>
    public static RoomStateSettings CrocomireSave { get; } =
        new(nameof(CrocomireSave), RoomLevelDefinitions.WarehouseSave, 0x19, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSave, Enemies.RoomEnemyPopulationDefinitions.CrocomireSave, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CrocomireSave, null, RoomSetupCallback.SharedReturnC);


    /// <summary>The disassembly's RoomState_LNElevSave.</summary>
    public static RoomStateSettings LNElevSave { get; } =
        new(nameof(LNElevSave), RoomLevelDefinitions.CrateriaSave, 0x19, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LNElevSave, Enemies.RoomEnemyPopulationDefinitions.LNElevSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNElevSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LNElevSave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNElevSave, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_AcidStatue.</summary>
    public static RoomStateSettings AcidStatue { get; } =
        new(nameof(AcidStatue), RoomLevelDefinitions.AcidStatue, 0x09, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.AcidStatue, Enemies.RoomEnemyPopulationDefinitions.AcidStatue, Enemies.RoomEnemyGraphicsSetDefinitions.AcidStatue, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AcidStatue, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AcidStatue, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_MainHall.</summary>
    public static RoomStateSettings MainHall { get; } =
        new(nameof(MainHall), RoomLevelDefinitions.MainHall, 0x09, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.MainHall, Enemies.RoomEnemyPopulationDefinitions.MainHall, Enemies.RoomEnemyGraphicsSetDefinitions.MainHall, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MainHall, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MainHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_GoldenTorizo_0.</summary>
    public static RoomStateSettings GoldenTorizo0 { get; } =
        new(nameof(GoldenTorizo0), RoomLevelDefinitions.GoldenTorizo, 0x09, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizoState01, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GoldenTorizoState01, null, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_GoldenTorizo_1.</summary>
    public static RoomStateSettings GoldenTorizo1 { get; } =
        new(nameof(GoldenTorizo1), RoomLevelDefinitions.GoldenTorizo, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizoState01, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GoldenTorizoState01, null, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_FastRipper.</summary>
    public static RoomStateSettings FastRipper { get; } =
        new(nameof(FastRipper), RoomLevelDefinitions.FastRipper, 0x09, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.FastRipper, Enemies.RoomEnemyPopulationDefinitions.FastRipper, Enemies.RoomEnemyGraphicsSetDefinitions.FastRipper, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FastRipper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_GTEnergyRefill.</summary>
    public static RoomStateSettings GTEnergyRefill { get; } =
        new(nameof(GTEnergyRefill), RoomLevelDefinitions.GTEnergyRefill, 0x17, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.GTEnergyRefill, Enemies.RoomEnemyPopulationDefinitions.GTEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.GTEnergyRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GTEnergyRefill, null, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Ridley_0.</summary>
    public static RoomStateSettings Ridley0 { get; } =
        new(nameof(Ridley0), RoomLevelDefinitions.Ridley, 0x09, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(4)), SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyState01, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainCallback.RidleyRoomShaking, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RidleyState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Ridley_1.</summary>
    public static RoomStateSettings Ridley1 { get; } =
        new(nameof(Ridley1), RoomLevelDefinitions.Ridley, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyState01, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainCallback.RidleyRoomShaking, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RidleyState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNFarming.</summary>
    public static RoomStateSettings LNFarming { get; } =
        new(nameof(LNFarming), RoomLevelDefinitions.LNFarming, 0x09, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LNFarming, Enemies.RoomEnemyPopulationDefinitions.LNFarming, Enemies.RoomEnemyGraphicsSetDefinitions.LNFarming, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNFarming, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_FastPillarsSetup.</summary>
    public static RoomStateSettings FastPillarsSetup { get; } =
        new(nameof(FastPillarsSetup), RoomLevelDefinitions.FastPillarsSetup, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FastPillarsSetup, Enemies.RoomEnemyPopulationDefinitions.FastPillarsSetup, Enemies.RoomEnemyGraphicsSetDefinitions.FastPillarsSetup, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FastPillarsSetup, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FastPillarsSetup, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9VerticalPatternBrick, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's UNUSED_RoomState_8FB3EE.</summary>
    public static RoomStateSettings UnusedRoom8FB3EE { get; } =
        new(nameof(UnusedRoom8FB3EE), RoomLevelDefinitions.UnusedNorfairRoom, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.UnusedRoom8387EC, Enemies.RoomEnemyPopulationDefinitions.UnusedRoom8FB3EEA1AD06, Enemies.RoomEnemyGraphicsSetDefinitions.UnusedState8FB3EEB48899, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.UnusedRoom8FB3EE, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9HorizontalPatternBrick, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_MickeyMouse.</summary>
    public static RoomStateSettings MickeyMouse { get; } =
        new(nameof(MickeyMouse), RoomLevelDefinitions.MickeyMouse, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MickeyMouse, Enemies.RoomEnemyPopulationDefinitions.MickeyMouse, Enemies.RoomEnemyGraphicsSetDefinitions.MickeyMouse, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MickeyMouse, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MickeyMouse, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Pillar.</summary>
    public static RoomStateSettings Pillar { get; } =
        new(nameof(Pillar), RoomLevelDefinitions.Pillar, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Pillar, Enemies.RoomEnemyPopulationDefinitions.Pillar, Enemies.RoomEnemyGraphicsSetDefinitions.Pillar, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Pillar, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernVerticalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Plowerhouse.</summary>
    public static RoomStateSettings Plowerhouse { get; } =
        new(nameof(Plowerhouse), RoomLevelDefinitions.Plowerhouse, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Plowerhouse, Enemies.RoomEnemyPopulationDefinitions.Plowerhouse, Enemies.RoomEnemyGraphicsSetDefinitions.Plowerhouse, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Plowerhouse, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_WorstRoomInTheGame.</summary>
    public static RoomStateSettings WorstRoomInTheGame { get; } =
        new(nameof(WorstRoomInTheGame), RoomLevelDefinitions.WorstRoomInTheGame, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WorstRoomInTheGame, Enemies.RoomEnemyPopulationDefinitions.WorstRoomInTheGame, Enemies.RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WorstRoomInTheGame, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WorstRoomInTheGame, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernHorizontalRuins, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Amphitheatre.</summary>
    public static RoomStateSettings Amphitheatre { get; } =
        new(nameof(Amphitheatre), RoomLevelDefinitions.Amphitheatre, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Amphitheatre, Enemies.RoomEnemyPopulationDefinitions.Amphitheatre, Enemies.RoomEnemyGraphicsSetDefinitions.Amphitheatre, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Amphitheatre, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNSpringBallMaze.</summary>
    public static RoomStateSettings LNSpringBallMaze { get; } =
        new(nameof(LNSpringBallMaze), RoomLevelDefinitions.LNSpringBallMaze, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LNSpringballMaze, Enemies.RoomEnemyPopulationDefinitions.LNSpringBallMaze, Enemies.RoomEnemyGraphicsSetDefinitions.LNSpringBallMaze, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LNSpringBallMaze, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNSpringBallMaze, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNEscapePowerBombs.</summary>
    public static RoomStateSettings LNEscapePowerBombs { get; } =
        new(nameof(LNEscapePowerBombs), RoomLevelDefinitions.LNEscapePowerBombs, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.LNEscapePowerBombs, Enemies.RoomEnemyPopulationDefinitions.LNEscapePowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.LNEscapePowerBombs, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNEscapePowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_RedKihunterShaft.</summary>
    public static RoomStateSettings RedKihunterShaft { get; } =
        new(nameof(RedKihunterShaft), RoomLevelDefinitions.RedKihunterShaft, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RedKihunterShaft, Enemies.RoomEnemyPopulationDefinitions.RedKihunterShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedKihunterShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedKihunterShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedKihunterShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9ASmallPatternBrownPurple0, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_Wasteland.</summary>
    public static RoomStateSettings Wasteland { get; } =
        new(nameof(Wasteland), RoomLevelDefinitions.Wasteland, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Wasteland, Enemies.RoomEnemyPopulationDefinitions.Wasteland, Enemies.RoomEnemyGraphicsSetDefinitions.Wasteland, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Wasteland, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Wasteland, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_MetalPirates.</summary>
    public static RoomStateSettings MetalPirates { get; } =
        new(nameof(MetalPirates), RoomLevelDefinitions.MetalPirates, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MetalPirates, Enemies.RoomEnemyPopulationDefinitions.MetalPirates, Enemies.RoomEnemyGraphicsSetDefinitions.MetalPirates, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MetalPirates, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9RoomStatues, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_ThreeMusketeers.</summary>
    public static RoomStateSettings ThreeMusketeers { get; } =
        new(nameof(ThreeMusketeers), RoomLevelDefinitions.ThreeMusketeers, 0x09, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ThreeMusketeers, Enemies.RoomEnemyPopulationDefinitions.ThreeMusketeers, Enemies.RoomEnemyGraphicsSetDefinitions.ThreeMusketeers, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThreeMusketeers, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ThreeMusketeers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9PurplePatches, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_RidleyETank.</summary>
    public static RoomStateSettings RidleyETank { get; } =
        new(nameof(RidleyETank), RoomLevelDefinitions.RidleyETank, 0x09, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyETank, Enemies.RoomEnemyPopulationDefinitions.RidleyETank, Enemies.RoomEnemyGraphicsSetDefinitions.RidleyETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RidleyETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_ScrewAttack.</summary>
    public static RoomStateSettings ScrewAttack { get; } =
        new(nameof(ScrewAttack), RoomLevelDefinitions.ScrewAttack, 0x09, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.ScrewAttack, Enemies.RoomEnemyPopulationDefinitions.ScrewAttack, Enemies.RoomEnemyGraphicsSetDefinitions.ScrewAttack, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ScrewAttack, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Norfair9CavernRoomPillars, RoomSetupCallback.SharedReturnD);


    /// <summary>The disassembly's RoomState_LNFireflea.</summary>
    public static RoomStateSettings LNFireflea { get; } =
        new(nameof(LNFireflea), RoomLevelDefinitions.LNFireflea, 0x0A, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LNFireflea, Enemies.RoomEnemyPopulationDefinitions.LNFireflea, Enemies.RoomEnemyGraphicsSetDefinitions.LNFireflea, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LNFireflea, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNFireflea, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_LNSave.</summary>
    public static RoomStateSettings LNSave { get; } =
        new(nameof(LNSave), RoomLevelDefinitions.WarehouseSave, 0x19, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.LNSave, Enemies.RoomEnemyPopulationDefinitions.LNSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.LNSave, null, RoomSetupCallback.None);
}
