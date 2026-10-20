namespace SuperMetroid.Core.Rooms;

/// <summary>The initialization settings of each Tourian room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class TourianRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_TourianFirst.</summary>
    public static CartridgeRoomState TourianFirst { get; } =
        new(nameof(TourianFirst), 0xCDC4FE, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianFirst, Enemies.RoomEnemyPopulationDefinitions.TourianFirst, Enemies.RoomEnemyGraphicsSetDefinitions.TourianFirst, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.TourianFirst, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.TourianFirst, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.SetCollectedMap);


    /// <summary>The disassembly's RoomState_Metroids1_0.</summary>
    public static CartridgeRoomState Metroids10 { get; } =
        new(nameof(Metroids10), 0xCDC8DC, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1State01, Enemies.RoomEnemyPopulationDefinitions.Metroids10, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids10, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids1State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids1_1.</summary>
    public static CartridgeRoomState Metroids11 { get; } =
        new(nameof(Metroids11), 0xCDC8DC, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1State01, Enemies.RoomEnemyPopulationDefinitions.Metroids11, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids11, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids1State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids2_0.</summary>
    public static CartridgeRoomState Metroids20 { get; } =
        new(nameof(Metroids20), 0xCDCDA0, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2State01, Enemies.RoomEnemyPopulationDefinitions.Metroids20, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids20, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids2State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids2_1.</summary>
    public static CartridgeRoomState Metroids21 { get; } =
        new(nameof(Metroids21), 0xCDCDA0, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2State01, Enemies.RoomEnemyPopulationDefinitions.Metroids21, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids21, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids2State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids3_0.</summary>
    public static CartridgeRoomState Metroids30 { get; } =
        new(nameof(Metroids30), 0xCDD02D, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3State01, Enemies.RoomEnemyPopulationDefinitions.Metroids30, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids30, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids3State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids3_1.</summary>
    public static CartridgeRoomState Metroids31 { get; } =
        new(nameof(Metroids31), 0xCDD02D, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3State01, Enemies.RoomEnemyPopulationDefinitions.Metroids31, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids31, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids3State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids4_0.</summary>
    public static CartridgeRoomState Metroids40 { get; } =
        new(nameof(Metroids40), 0xCDD3E5, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4State01, Enemies.RoomEnemyPopulationDefinitions.Metroids40, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids40, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids4State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Metroids4_1.</summary>
    public static CartridgeRoomState Metroids41 { get; } =
        new(nameof(Metroids41), 0xCDD3E5, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4State01, Enemies.RoomEnemyPopulationDefinitions.Metroids41, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids41, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Metroids4State01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_BlueHopper_0.</summary>
    public static CartridgeRoomState BlueHopper0 { get; } =
        new(nameof(BlueHopper0), 0xCDD5EB, 0x0D, 0x45, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopperState01, Enemies.RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueHopperState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_BlueHopper_1.</summary>
    public static CartridgeRoomState BlueHopper1 { get; } =
        new(nameof(BlueHopper1), 0xCDD5EB, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopperState01, Enemies.RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueHopperState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_DustTorizo_0.</summary>
    public static CartridgeRoomState DustTorizo0 { get; } =
        new(nameof(DustTorizo0), 0xCDD7C4, 0x0D, 0x45, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizoState01, Enemies.RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DustTorizoState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_DustTorizo_1.</summary>
    public static CartridgeRoomState DustTorizo1 { get; } =
        new(nameof(DustTorizo1), 0xCDD7C4, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizoState01, Enemies.RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DustTorizoState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_BigBoy_0.</summary>
    public static CartridgeRoomState BigBoy0 { get; } =
        new(nameof(BigBoy0), 0xCDD930, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoyState01, Enemies.RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BigBoyState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_BigBoy_1.</summary>
    public static CartridgeRoomState BigBoy1 { get; } =
        new(nameof(BigBoy1), 0xCDD930, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoyState01, Enemies.RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BigBoyState01, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_Seaweed.</summary>
    public static CartridgeRoomState Seaweed { get; } =
        new(nameof(Seaweed), 0xCDDBF8, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Seaweed, Enemies.RoomEnemyPopulationDefinitions.Seaweed, Enemies.RoomEnemyGraphicsSetDefinitions.Seaweed, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Seaweed, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Seaweed, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_TourianRecharge.</summary>
    public static CartridgeRoomState TourianRecharge { get; } =
        new(nameof(TourianRecharge), 0xCEA823, 0x18, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianRecharge, Enemies.RoomEnemyPopulationDefinitions.TourianRecharge, Enemies.RoomEnemyGraphicsSetDefinitions.TourianRecharge, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.TourianRecharge, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.TourianRecharge, null, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_MotherBrain_0.</summary>
    public static CartridgeRoomState MotherBrain0 { get; } =
        new(nameof(MotherBrain0), 0xCDDEDE, 0x0E, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainState01, Enemies.RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MotherBrainState012, Backgrounds.LibraryBackgroundProgramDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_MotherBrain_1.</summary>
    public static CartridgeRoomState MotherBrain1 { get; } =
        new(nameof(MotherBrain1), 0xCDDEDE, 0x0E, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainState01, Enemies.RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MotherBrainState012, Backgrounds.LibraryBackgroundProgramDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_MotherBrain_2.</summary>
    public static CartridgeRoomState MotherBrain2 { get; } =
        new(nameof(MotherBrain2), 0xCDDEDE, 0x0E, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.DebugMotherBrainState2, Enemies.RoomEnemyPopulationDefinitions.DraygonState1MotherBrainState2, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MotherBrainState012, Backgrounds.LibraryBackgroundProgramDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_TourianEyeDoor.</summary>
    public static CartridgeRoomState TourianEyeDoor { get; } =
        new(nameof(TourianEyeDoor), 0xCDE20F, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEyeDoor, Enemies.RoomEnemyPopulationDefinitions.TourianEyeDoor, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEyeDoor, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEyeDoor, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.TourianEyeDoor, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_RinkaShaft.</summary>
    public static CartridgeRoomState RinkaShaft { get; } =
        new(nameof(RinkaShaft), 0xCDE518, 0x0D, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.RinkaShaft, Enemies.RoomEnemyPopulationDefinitions.RinkaShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RinkaShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RinkaShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RinkaShaft, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_MotherBrainSave.</summary>
    public static CartridgeRoomState MotherBrainSave { get; } =
        new(nameof(MotherBrainSave), 0xCE92CB, 0x19, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainSave, Enemies.RoomEnemyPopulationDefinitions.MotherBrainSave, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrainSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.MotherBrainSave, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MotherBrainSave, null, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The disassembly's RoomState_TourianEscape1.</summary>
    public static CartridgeRoomState TourianEscape1 { get; } =
        new(nameof(TourianEscape1), 0xCDE914, 0x0D, 0x24, 0x07, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape1, Enemies.RoomEnemyPopulationDefinitions.TourianEscape1, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape1, null, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, Plms.RoomPlmPopulationDefinitions.TourianEscape1, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoomLetterbox, RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking);


    /// <summary>The disassembly's RoomState_TourianEscape2.</summary>
    public static CartridgeRoomState TourianEscape2 { get; } =
        new(nameof(TourianEscape2), 0xCDEB5B, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape2, Enemies.RoomEnemyPopulationDefinitions.TourianEscape2, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape2, null, RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal, Plms.RoomPlmPopulationDefinitions.TourianEscape2, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupCallback.SetLightHorizontalRoomShaking);


    /// <summary>The disassembly's RoomState_TourianEscape3.</summary>
    public static CartridgeRoomState TourianEscape3 { get; } =
        new(nameof(TourianEscape3), 0xCDED7A, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape3, Enemies.RoomEnemyPopulationDefinitions.TourianEscape3, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape3, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape3, null, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, Plms.RoomPlmPopulationDefinitions.TourianEscape3, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoom, RoomSetupCallback.SetMediumHorizontalRoomShaking);


    /// <summary>The disassembly's RoomState_TourianEscape4.</summary>
    public static CartridgeRoomState TourianEscape4 { get; } =
        new(nameof(TourianEscape4), 0xCDF534, 0x0D, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ToruianEscaper4, Enemies.RoomEnemyPopulationDefinitions.TourianEscape4, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape4, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape4, null, RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal, Plms.RoomPlmPopulationDefinitions.TourianEscape4, Backgrounds.LibraryBackgroundProgramDefinitions.TourianDPipeRoom, RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking);


    /// <summary>The disassembly's RoomState_UpperTourianSave.</summary>
    public static CartridgeRoomState UpperTourianSave { get; } =
        new(nameof(UpperTourianSave), 0xCE95C2, 0x19, 0x1E, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperTourianSave, Enemies.RoomEnemyPopulationDefinitions.UpperTourianSave, Enemies.RoomEnemyGraphicsSetDefinitions.UpperTourianSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.UpperTourianSave, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.UpperTourianSave, null, RoomSetupCallback.None);
}
