using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Tourian room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class TourianRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_TourianFirst.</summary>
    public static RoomStateSettings TourianFirst { get; } =
        new(nameof(TourianFirst), RoomLevelDefinitions.TourianFirst, 0x0D, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.TourianFirst, Enemies.RoomEnemyPopulationDefinitions.TourianFirst, Enemies.RoomEnemyGraphicsSetDefinitions.TourianFirst, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianFirst, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.TourianFirst, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.SetCollectedMap);


    /// <summary>The disassembly's RoomState_Metroids1_0.</summary>
    public static RoomStateSettings Metroids10 { get; } =
        new(nameof(Metroids10), RoomLevelDefinitions.Metroids1, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1State01, Enemies.RoomEnemyPopulationDefinitions.Metroids10, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids10, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Metroids1State01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids1_1.</summary>
    public static RoomStateSettings Metroids11 { get; } =
        new(nameof(Metroids11), RoomLevelDefinitions.Metroids1, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1State01, Enemies.RoomEnemyPopulationDefinitions.Metroids11, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids11, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Metroids1State01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids2_0.</summary>
    public static RoomStateSettings Metroids20 { get; } =
        new(nameof(Metroids20), RoomLevelDefinitions.Metroids2, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2State01, Enemies.RoomEnemyPopulationDefinitions.Metroids20, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids20, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Metroids2State01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids2_1.</summary>
    public static RoomStateSettings Metroids21 { get; } =
        new(nameof(Metroids21), RoomLevelDefinitions.Metroids2, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2State01, Enemies.RoomEnemyPopulationDefinitions.Metroids21, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids21, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Metroids2State01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids3_0.</summary>
    public static RoomStateSettings Metroids30 { get; } =
        new(nameof(Metroids30), RoomLevelDefinitions.Metroids3, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3State01, Enemies.RoomEnemyPopulationDefinitions.Metroids30, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids30, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Metroids3State01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids3_1.</summary>
    public static RoomStateSettings Metroids31 { get; } =
        new(nameof(Metroids31), RoomLevelDefinitions.Metroids3, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3State01, Enemies.RoomEnemyPopulationDefinitions.Metroids31, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids31, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Metroids3State01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids4_0.</summary>
    public static RoomStateSettings Metroids40 { get; } =
        new(nameof(Metroids40), RoomLevelDefinitions.Metroids4, 0x0D, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4State01, Enemies.RoomEnemyPopulationDefinitions.Metroids40, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids40, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Metroids4State01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids4_1.</summary>
    public static RoomStateSettings Metroids41 { get; } =
        new(nameof(Metroids41), RoomLevelDefinitions.Metroids4, 0x0D, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4State01, Enemies.RoomEnemyPopulationDefinitions.Metroids41, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids41, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Metroids4State01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_BlueHopper_0.</summary>
    public static RoomStateSettings BlueHopper0 { get; } =
        new(nameof(BlueHopper0), RoomLevelDefinitions.BlueHopper, 0x0D, new RoomMusic(MusicDataSet.BabyMetroidBossFight2, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopperState01, Enemies.RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BlueHopperState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_BlueHopper_1.</summary>
    public static RoomStateSettings BlueHopper1 { get; } =
        new(nameof(BlueHopper1), RoomLevelDefinitions.BlueHopper, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopperState01, Enemies.RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BlueHopperState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_DustTorizo_0.</summary>
    public static RoomStateSettings DustTorizo0 { get; } =
        new(nameof(DustTorizo0), RoomLevelDefinitions.DustTorizo, 0x0D, new RoomMusic(MusicDataSet.BabyMetroidBossFight2, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizoState01, Enemies.RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DustTorizoState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_DustTorizo_1.</summary>
    public static RoomStateSettings DustTorizo1 { get; } =
        new(nameof(DustTorizo1), RoomLevelDefinitions.DustTorizo, 0x0D, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizoState01, Enemies.RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DustTorizoState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_BigBoy_0.</summary>
    public static RoomStateSettings BigBoy0 { get; } =
        new(nameof(BigBoy0), RoomLevelDefinitions.BigBoy, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoyState01, Enemies.RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BigBoyState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_BigBoy_1.</summary>
    public static RoomStateSettings BigBoy1 { get; } =
        new(nameof(BigBoy1), RoomLevelDefinitions.BigBoy, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoyState01, Enemies.RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BigBoyState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Seaweed.</summary>
    public static RoomStateSettings Seaweed { get; } =
        new(nameof(Seaweed), RoomLevelDefinitions.Seaweed, 0x0D, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.Seaweed, Enemies.RoomEnemyPopulationDefinitions.Seaweed, Enemies.RoomEnemyGraphicsSetDefinitions.Seaweed, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Seaweed, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Seaweed, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_TourianRecharge.</summary>
    public static RoomStateSettings TourianRecharge { get; } =
        new(nameof(TourianRecharge), RoomLevelDefinitions.TourianRecharge, 0x18, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianRecharge, Enemies.RoomEnemyPopulationDefinitions.TourianRecharge, Enemies.RoomEnemyGraphicsSetDefinitions.TourianRecharge, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianRecharge, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.TourianRecharge, null, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_MotherBrain_0.</summary>
    public static RoomStateSettings MotherBrain0 { get; } =
        new(nameof(MotherBrain0), RoomLevelDefinitions.MotherBrain, 0x0E, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainState01, Enemies.RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MotherBrainState012, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_MotherBrain_1.</summary>
    public static RoomStateSettings MotherBrain1 { get; } =
        new(nameof(MotherBrain1), RoomLevelDefinitions.MotherBrain, 0x0E, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainState01, Enemies.RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MotherBrainState012, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_MotherBrain_2.</summary>
    public static RoomStateSettings MotherBrain2 { get; } =
        new(nameof(MotherBrain2), RoomLevelDefinitions.MotherBrain, 0x0E, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.DebugMotherBrainState2, Enemies.RoomEnemyPopulationDefinitions.DraygonState1MotherBrainState2, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MotherBrainState012, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_TourianEyeDoor.</summary>
    public static RoomStateSettings TourianEyeDoor { get; } =
        new(nameof(TourianEyeDoor), RoomLevelDefinitions.TourianEyeDoor, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEyeDoor, Enemies.RoomEnemyPopulationDefinitions.TourianEyeDoor, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEyeDoor, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEyeDoor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.TourianEyeDoor, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_RinkaShaft.</summary>
    public static RoomStateSettings RinkaShaft { get; } =
        new(nameof(RinkaShaft), RoomLevelDefinitions.RinkaShaft, 0x0D, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.RinkaShaft, Enemies.RoomEnemyPopulationDefinitions.RinkaShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RinkaShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RinkaShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RinkaShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_MotherBrainSave.</summary>
    public static RoomStateSettings MotherBrainSave { get; } =
        new(nameof(MotherBrainSave), RoomLevelDefinitions.CrateriaSave, 0x19, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainSave, Enemies.RoomEnemyPopulationDefinitions.MotherBrainSave, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrainSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrainSave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MotherBrainSave, null, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_TourianEscape1.</summary>
    public static RoomStateSettings TourianEscape1 { get; } =
        new(nameof(TourianEscape1), RoomLevelDefinitions.TourianEscape1, 0x0D, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(7)), SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape1, Enemies.RoomEnemyPopulationDefinitions.TourianEscape1, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape1, null, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.TourianEscape1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoomLetterbox, RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking);


    /// <summary>The disassembly's RoomState_TourianEscape2.</summary>
    public static RoomStateSettings TourianEscape2 { get; } =
        new(nameof(TourianEscape2), RoomLevelDefinitions.TourianEscape2, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape2, Enemies.RoomEnemyPopulationDefinitions.TourianEscape2, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape2, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape2, null, RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.TourianEscape2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupCallback.SetLightHorizontalRoomShaking);


    /// <summary>The disassembly's RoomState_TourianEscape3.</summary>
    public static RoomStateSettings TourianEscape3 { get; } =
        new(nameof(TourianEscape3), RoomLevelDefinitions.TourianEscape3, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape3, Enemies.RoomEnemyPopulationDefinitions.TourianEscape3, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape3, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape3, null, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.TourianEscape3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoom, RoomSetupCallback.SetMediumHorizontalRoomShaking);


    /// <summary>The disassembly's RoomState_TourianEscape4.</summary>
    public static RoomStateSettings TourianEscape4 { get; } =
        new(nameof(TourianEscape4), RoomLevelDefinitions.TourianEscape4, 0x0D, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ToruianEscaper4, Enemies.RoomEnemyPopulationDefinitions.TourianEscape4, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape4, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape4, null, RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.TourianEscape4, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoom, RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking);


    /// <summary>The disassembly's RoomState_UpperTourianSave.</summary>
    public static RoomStateSettings UpperTourianSave { get; } =
        new(nameof(UpperTourianSave), RoomLevelDefinitions.WarehouseSave, 0x19, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.UpperTourianSave, Enemies.RoomEnemyPopulationDefinitions.UpperTourianSave, Enemies.RoomEnemyGraphicsSetDefinitions.UpperTourianSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.UpperTourianSave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.UpperTourianSave, null, RoomSetupCallback.None);
}
