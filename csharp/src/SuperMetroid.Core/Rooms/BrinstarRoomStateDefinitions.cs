namespace SuperMetroid.Core.Rooms;

/// <summary>The initialization settings of each Brinstar room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class BrinstarRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_GreenBrinstarMainShaft.</summary>
    public static CartridgeRoomState GreenBrinstarMainShaft { get; } =
        new(nameof(GreenBrinstarMainShaft), 0xC4F1CE, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMainShaft, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarMainShaft, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMainShaft, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.GreenBrinstarMainShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarMainShaft, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6VerticalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawnSuper.</summary>
    public static CartridgeRoomState SporeSpawnSuper { get; } =
        new(nameof(SporeSpawnSuper), 0xC58BD5, 0x06, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnSuper, Enemies.RoomEnemyPopulationDefinitions.SporeSpawnSuper, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnSuper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.SporeSpawnSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnSuper, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BrinstarPreMap.</summary>
    public static CartridgeRoomState BrinstarPreMap { get; } =
        new(nameof(BrinstarPreMap), 0xC59642, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarPreMap, Enemies.RoomEnemyPopulationDefinitions.BrinstarPreMap, Enemies.RoomEnemyGraphicsSetDefinitions.BrinstarPreMap, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BrinstarPreMap, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6MediumHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_EarlySupers.</summary>
    public static CartridgeRoomState EarlySupers { get; } =
        new(nameof(EarlySupers), 0xC59755, 0x06, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.EarlySupers, Enemies.RoomEnemyPopulationDefinitions.EarlySupers, Enemies.RoomEnemyGraphicsSetDefinitions.EarlySupers, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.EarlySupers, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EarlySupers, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BrinstarReserveTank.</summary>
    public static CartridgeRoomState BrinstarReserveTank { get; } =
        new(nameof(BrinstarReserveTank), 0xC59B00, 0x06, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarReserveTank, Enemies.RoomEnemyPopulationDefinitions.BrinstarReserveTank, Enemies.RoomEnemyGraphicsSetDefinitions.BrinstarReserveTank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BrinstarReserveTank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BrinstarReserveTank, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6LargeHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarMap.</summary>
    public static CartridgeRoomState GreenBrinstarMap { get; } =
        new(nameof(GreenBrinstarMap), 0xCE83C3, 0x15, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMap, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarMap, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarMap, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarFirefleas.</summary>
    public static CartridgeRoomState GreenBrinstarFirefleas { get; } =
        new(nameof(GreenBrinstarFirefleas), 0xC59CAC, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarFirefleas, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarFirefleas, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarFirefleas, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarMissileRefill.</summary>
    public static CartridgeRoomState GreenBrinstarMissileRefill { get; } =
        new(nameof(GreenBrinstarMissileRefill), 0xCE8CB3, 0x18, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMissileRefill, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarMissileRefill, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMissileRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenBrinstarMissileRefill, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarMissileRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_Dachora.</summary>
    public static CartridgeRoomState Dachora { get; } =
        new(nameof(Dachora), 0xC5A15F, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Dachora, Enemies.RoomEnemyPopulationDefinitions.Dachora, Enemies.RoomEnemyGraphicsSetDefinitions.Dachora, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Dachora, null, RoomMainCallback.ScrollScreenRightInDachoraRoom, Plms.RoomPlmPopulationDefinitions.Dachora, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BigPink.</summary>
    public static CartridgeRoomState BigPink { get; } =
        new(nameof(BigPink), 0xC5B54D, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BigPink, Enemies.RoomEnemyPopulationDefinitions.BigPink, Enemies.RoomEnemyGraphicsSetDefinitions.BigPink, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BigPink, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BigPink, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6DarkPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawnKihunters.</summary>
    public static CartridgeRoomState SporeSpawnKihunters { get; } =
        new(nameof(SporeSpawnKihunters), 0xC5CBA7, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnKihunters, Enemies.RoomEnemyPopulationDefinitions.SporeSpawnKihunters, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnKihunters, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnKihunters, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawn_0.</summary>
    public static CartridgeRoomState SporeSpawn0 { get; } =
        new(nameof(SporeSpawn0), 0xC5CE34, 0x06, 0x2A, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnState01, Enemies.RoomEnemyPopulationDefinitions.SporeSpawn, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawn, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnState01, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawn_1.</summary>
    public static CartridgeRoomState SporeSpawn1 { get; } =
        new(nameof(SporeSpawn1), 0xC5CE34, 0x06, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnState01, Enemies.RoomEnemyPopulationDefinitions.SporeSpawn, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawn, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnState01, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_PinkBrinstarPowerBombs.</summary>
    public static CartridgeRoomState PinkBrinstarPowerBombs { get; } =
        new(nameof(PinkBrinstarPowerBombs), 0xC5D18F, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarPowerBombs, Enemies.RoomEnemyPopulationDefinitions.PinkBrinstarPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.PinkBrinstarPowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.PinkBrinstarPowerBombs, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PinkBrinstarPowerBombs, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenHillZone.</summary>
    public static CartridgeRoomState GreenHillZone { get; } =
        new(nameof(GreenHillZone), 0xC5D559, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenHillZone, Enemies.RoomEnemyPopulationDefinitions.GreenHillZone, Enemies.RoomEnemyGraphicsSetDefinitions.GreenHillZone, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenHillZone, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenHillZone, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SpheresPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_MorphBall_0.</summary>
    public static CartridgeRoomState MorphBall0 { get; } =
        new(nameof(MorphBall0), 0xC5DED5, 0x06, 0x06, 0x07, SuperMetroid.Core.Fx.RoomFxDefinitions.MorphBallState0, Enemies.RoomEnemyPopulationDefinitions.MorphBall0, Enemies.RoomEnemyGraphicsSetDefinitions.MorphBall0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MorphBallState0, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_MorphBall_1.</summary>
    public static CartridgeRoomState MorphBall1 { get; } =
        new(nameof(MorphBall1), 0xC5DED5, 0x06, 0x09, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381F4, Enemies.RoomEnemyPopulationDefinitions.MorphBall1, Enemies.RoomEnemyGraphicsSetDefinitions.MorphBall1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MorphBallState1, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_ConstructionZone_0.</summary>
    public static CartridgeRoomState ConstructionZone0 { get; } =
        new(nameof(ConstructionZone0), 0xC5E63A, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.ConstructionZoneState0, Enemies.RoomEnemyPopulationDefinitions.ConstructionZone0, Enemies.RoomEnemyGraphicsSetDefinitions.ConstructionZone0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ConstructionZoneState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_ConstructionZone_1.</summary>
    public static CartridgeRoomState ConstructionZone1 { get; } =
        new(nameof(ConstructionZone1), 0xC5E63A, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381F4, Enemies.RoomEnemyPopulationDefinitions.ConstructionZone1, Enemies.RoomEnemyGraphicsSetDefinitions.ConstructionZone1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ConstructionZoneState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_BlueBrinstarETank_0.</summary>
    public static CartridgeRoomState BlueBrinstarETank0 { get; } =
        new(nameof(BlueBrinstarETank0), 0xC5E86F, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarETankState0, Enemies.RoomEnemyPopulationDefinitions.BlueBrinstarETank0, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueBrinstarETankState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BlueBrinstarETank_1.</summary>
    public static CartridgeRoomState BlueBrinstarETank1 { get; } =
        new(nameof(BlueBrinstarETank1), 0xC5E86F, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381F4, Enemies.RoomEnemyPopulationDefinitions.BlueBrinstarETank1, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueBrinstarETankState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_NoobBridge.</summary>
    public static CartridgeRoomState NoobBridge { get; } =
        new(nameof(NoobBridge), 0xC5ECAE, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.NoobBridge, Enemies.RoomEnemyPopulationDefinitions.NoobBridge, Enemies.RoomEnemyGraphicsSetDefinitions.NoobBridge, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.NoobBridge, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarBeetoms.</summary>
    public static CartridgeRoomState GreenBrinstarBeetoms { get; } =
        new(nameof(GreenBrinstarBeetoms), 0xC5EF71, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarBeetoms, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarBeetoms, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarBeetoms, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenBrinstarBeetoms, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarBeetoms, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_EtecoonETank.</summary>
    public static CartridgeRoomState EtecoonETank { get; } =
        new(nameof(EtecoonETank), 0xC5F057, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonETank, Enemies.RoomEnemyPopulationDefinitions.EtecoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonETank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.EtecoonETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EtecoonETank, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_EtecoonSuper.</summary>
    public static CartridgeRoomState EtecoonSuper { get; } =
        new(nameof(EtecoonSuper), 0xC5F43E, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSuper, Enemies.RoomEnemyPopulationDefinitions.EtecoonSuper, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonSuper, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.EtecoonSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EtecoonSuper, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_DachoraEnergyRefill.</summary>
    public static CartridgeRoomState DachoraEnergyRefill { get; } =
        new(nameof(DachoraEnergyRefill), 0xCE89B6, 0x17, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.DachoraEnergyRefill, Enemies.RoomEnemyPopulationDefinitions.DachoraEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.DachoraEnergyRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.DachoraEnergyRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawnFarming.</summary>
    public static CartridgeRoomState SporeSpawnFarming { get; } =
        new(nameof(SporeSpawnFarming), 0xC5F4C9, 0x06, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnFarming, Enemies.RoomEnemyPopulationDefinitions.SporeSpawnFarming, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnFarming, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.SporeSpawnFarming, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SporeSpawnFarming, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_WaterwayETank.</summary>
    public static CartridgeRoomState WaterwayETank { get; } =
        new(nameof(WaterwayETank), 0xC5F778, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WaterwayETank, Enemies.RoomEnemyPopulationDefinitions.WaterwayETank, Enemies.RoomEnemyGraphicsSetDefinitions.WaterwayETank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.WaterwayETank, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WaterwayETank, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_FirstMissile.</summary>
    public static CartridgeRoomState FirstMissile { get; } =
        new(nameof(FirstMissile), 0xC5FD50, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.FirstMissile, Enemies.RoomEnemyPopulationDefinitions.FirstMissile, Enemies.RoomEnemyGraphicsSetDefinitions.FirstMissile, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.FirstMissile, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_PinkBrinstarHoppers.</summary>
    public static CartridgeRoomState PinkBrinstarHoppers { get; } =
        new(nameof(PinkBrinstarHoppers), 0xC5FE1B, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarHoppers, Enemies.RoomEnemyPopulationDefinitions.PinkBrinstarHoppers, Enemies.RoomEnemyGraphicsSetDefinitions.PinkBrinstarHoppers, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.PinkBrinstarHoppers, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_HopperETank.</summary>
    public static CartridgeRoomState HopperETank { get; } =
        new(nameof(HopperETank), 0xC681C2, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.HopperETank, Enemies.RoomEnemyPopulationDefinitions.HopperETank, Enemies.RoomEnemyGraphicsSetDefinitions.HopperETank, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.HopperETank, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BigPinkSaveRoom.</summary>
    public static CartridgeRoomState BigPinkSaveRoom { get; } =
        new(nameof(BigPinkSaveRoom), 0xCE92CB, 0x19, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BigPinkSaveRoom, Enemies.RoomEnemyPopulationDefinitions.BigPinkSaveRoom, Enemies.RoomEnemyGraphicsSetDefinitions.BigPinkSaveRoom, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BigPinkSaveRoom, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BlueBrinstarBoulders.</summary>
    public static CartridgeRoomState BlueBrinstarBoulders { get; } =
        new(nameof(BlueBrinstarBoulders), 0xC68318, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarBoulders, Enemies.RoomEnemyPopulationDefinitions.BlueBrinstarBoulders, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarBoulders, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BlueBrinstarBoulders, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BillyMays.</summary>
    public static CartridgeRoomState BillyMays { get; } =
        new(nameof(BillyMays), 0xC68437, 0x06, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BillyMays, Enemies.RoomEnemyPopulationDefinitions.BillyMays, Enemies.RoomEnemyGraphicsSetDefinitions.BillyMays, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BillyMays, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarSave.</summary>
    public static CartridgeRoomState GreenBrinstarSave { get; } =
        new(nameof(GreenBrinstarSave), 0xCE92CB, 0x19, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarSave, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarSave, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GreenBrinstarSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_EtecoonSave.</summary>
    public static CartridgeRoomState EtecoonSave { get; } =
        new(nameof(EtecoonSave), 0xCE92CB, 0x19, 0x0F, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSave, Enemies.RoomEnemyPopulationDefinitions.EtecoonSave, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.EtecoonSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_RedTower.</summary>
    public static CartridgeRoomState RedTower { get; } =
        new(nameof(RedTower), 0xC684EE, 0x07, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.RedTower, Enemies.RoomEnemyPopulationDefinitions.RedTower, Enemies.RoomEnemyGraphicsSetDefinitions.RedTower, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RedTower, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedTower, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7WideVerticalTowerBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_RedBrinstarFirefleas.</summary>
    public static CartridgeRoomState RedBrinstarFirefleas { get; } =
        new(nameof(RedBrinstarFirefleas), 0xC691E3, 0x07, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarFirefleas, Enemies.RoomEnemyPopulationDefinitions.RedBrinstarFirefleas, Enemies.RoomEnemyGraphicsSetDefinitions.RedBrinstarFirefleas, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.RedBrinstarFirefleas, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_XrayScope.</summary>
    public static CartridgeRoomState XrayScope { get; } =
        new(nameof(XrayScope), 0xC69BF9, 0x07, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.XrayScope, Enemies.RoomEnemyPopulationDefinitions.XrayScope, Enemies.RoomEnemyGraphicsSetDefinitions.XrayScope, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.XrayScope, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7MechanicalRoomSpikeFloor, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Hellway.</summary>
    public static CartridgeRoomState Hellway { get; } =
        new(nameof(Hellway), 0xC69D70, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.Hellway, Enemies.RoomEnemyPopulationDefinitions.Hellway, Enemies.RoomEnemyGraphicsSetDefinitions.Hellway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Hellway, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7VerticalTower, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Caterpillar.</summary>
    public static CartridgeRoomState Caterpillar { get; } =
        new(nameof(Caterpillar), 0xC69F4B, 0x07, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.Caterpillar, Enemies.RoomEnemyPopulationDefinitions.Caterpillar, Enemies.RoomEnemyGraphicsSetDefinitions.Caterpillar, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Caterpillar, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Caterpillar, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_BetaPowerBomb.</summary>
    public static CartridgeRoomState BetaPowerBomb { get; } =
        new(nameof(BetaPowerBomb), 0xC6B31F, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.BetaPowerBomb, Enemies.RoomEnemyPopulationDefinitions.BetaPowerBomb, Enemies.RoomEnemyGraphicsSetDefinitions.BetaPowerBomb, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BetaPowerBomb, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BetaPowerBomb, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_AlphaPowerBomb.</summary>
    public static CartridgeRoomState AlphaPowerBomb { get; } =
        new(nameof(AlphaPowerBomb), 0xC6B58C, 0x07, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.AlphaPowerBomb, Enemies.RoomEnemyPopulationDefinitions.AlphaPowerBomb, Enemies.RoomEnemyGraphicsSetDefinitions.AlphaPowerBomb, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AlphaPowerBomb, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AlphaPowerBomb, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_SkreeBoost.</summary>
    public static CartridgeRoomState SkreeBoost { get; } =
        new(nameof(SkreeBoost), 0xC6B739, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SkreeBoost, Enemies.RoomEnemyPopulationDefinitions.SkreeBoost, Enemies.RoomEnemyGraphicsSetDefinitions.SkreeBoost, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SkreeBoost, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_BelowSpazer.</summary>
    public static CartridgeRoomState BelowSpazer { get; } =
        new(nameof(BelowSpazer), 0xC6B91C, 0x07, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BelowSpazer, Enemies.RoomEnemyPopulationDefinitions.BelowSpazer, Enemies.RoomEnemyGraphicsSetDefinitions.BelowSpazer, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BelowSpazer, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BelowSpazer, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7VerticalBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Spazer.</summary>
    public static CartridgeRoomState Spazer { get; } =
        new(nameof(Spazer), 0xC6BCC7, 0x07, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.Spazer, Enemies.RoomEnemyPopulationDefinitions.Spazer, Enemies.RoomEnemyGraphicsSetDefinitions.Spazer, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Spazer, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Spazer, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_WarehouseZeela.</summary>
    public static CartridgeRoomState WarehouseZeela { get; } =
        new(nameof(WarehouseZeela), 0xC6BD83, 0x07, 0x00, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8383D2, Enemies.RoomEnemyPopulationDefinitions.WarehouseZeela, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseZeela, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WarehouseZeela, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseZeela, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_WarehouseETank.</summary>
    public static CartridgeRoomState WarehouseETank { get; } =
        new(nameof(WarehouseETank), 0xC6C469, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8383D2, Enemies.RoomEnemyPopulationDefinitions.WarehouseETank, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseETank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseETank, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_WarehouseKihunter.</summary>
    public static CartridgeRoomState WarehouseKihunter { get; } =
        new(nameof(WarehouseKihunter), 0xC6C630, 0x07, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8383D2, Enemies.RoomEnemyPopulationDefinitions.WarehouseKihunter, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseKihunter, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WarehouseKihunter, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseKihunter, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_MiniKraid_0.</summary>
    public static CartridgeRoomState MiniKraid0 { get; } =
        new(nameof(MiniKraid0), 0xC6CDB9, 0x07, 0x27, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraidState01, Enemies.RoomEnemyPopulationDefinitions.MiniKraid, Enemies.RoomEnemyGraphicsSetDefinitions.MiniKraid, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MiniKraidState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_MiniKraid_1.</summary>
    public static CartridgeRoomState MiniKraid1 { get; } =
        new(nameof(MiniKraid1), 0xC6CDB9, 0x07, 0x27, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraidState01, Enemies.RoomEnemyPopulationDefinitions.MiniKraid, Enemies.RoomEnemyGraphicsSetDefinitions.MiniKraid, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.MiniKraidState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_KraidEyeDoor.</summary>
    public static CartridgeRoomState KraidEyeDoor { get; } =
        new(nameof(KraidEyeDoor), 0xC6D2CB, 0x07, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidEyeDoor, Enemies.RoomEnemyPopulationDefinitions.KraidEyeDoor, Enemies.RoomEnemyGraphicsSetDefinitions.KraidEyeDoor, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.KraidEyeDoor, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KraidEyeDoor, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Kraid_0.</summary>
    public static CartridgeRoomState Kraid0 { get; } =
        new(nameof(Kraid0), 0xC6D620, 0x1A, 0x27, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState0, Enemies.RoomEnemyPopulationDefinitions.Kraid, Enemies.RoomEnemyGraphicsSetDefinitions.Kraid, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KraidState01, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar1AKraidUpperLower, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Kraid_1.</summary>
    public static CartridgeRoomState Kraid1 { get; } =
        new(nameof(Kraid1), 0xC6D620, 0x1A, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState0, Enemies.RoomEnemyPopulationDefinitions.Kraid, Enemies.RoomEnemyGraphicsSetDefinitions.Kraid, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KraidState01, Backgrounds.LibraryBackgroundProgramDefinitions.KraidDead, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_RedTowerRefill.</summary>
    public static CartridgeRoomState RedTowerRefill { get; } =
        new(nameof(RedTowerRefill), 0xCE89B6, 0x17, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.RedTowerRefill, Enemies.RoomEnemyPopulationDefinitions.RedTowerRefill, Enemies.RoomEnemyGraphicsSetDefinitions.RedTowerRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedTowerRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_KraidRefill.</summary>
    public static CartridgeRoomState KraidRefill { get; } =
        new(nameof(KraidRefill), 0xCEA201, 0x18, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidRefill, Enemies.RoomEnemyPopulationDefinitions.KraidRefill, Enemies.RoomEnemyGraphicsSetDefinitions.KraidRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.KraidRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_WarehouseEntrance.</summary>
    public static CartridgeRoomState WarehouseEntrance { get; } =
        new(nameof(WarehouseEntrance), 0xC6DEE0, 0x07, 0x12, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseEntrance, Enemies.RoomEnemyPopulationDefinitions.WarehouseEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseEntrance, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WarehouseEntrance, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseEntrance, Backgrounds.LibraryBackgroundProgramDefinitions.Brinstar7MechanicalRoom, RoomSetupCallback.SharedReturn);


    /// <summary>The disassembly's RoomState_VariaSuit.</summary>
    public static CartridgeRoomState VariaSuit { get; } =
        new(nameof(VariaSuit), 0xC6E355, 0x07, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.VariaSuit, Enemies.RoomEnemyPopulationDefinitions.VariaSuit, Enemies.RoomEnemyGraphicsSetDefinitions.VariaSuit, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.VariaSuit, null, RoomSetupCallback.SharedReturn);


    /// <summary>The disassembly's RoomState_WarehouseSave.</summary>
    public static CartridgeRoomState WarehouseSave { get; } =
        new(nameof(WarehouseSave), 0xCE95C2, 0x19, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseSave, Enemies.RoomEnemyPopulationDefinitions.WarehouseSave, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WarehouseSave, null, RoomSetupCallback.SharedReturn);


    /// <summary>The disassembly's RoomState_RedBrinstarSave.</summary>
    public static CartridgeRoomState RedBrinstarSave { get; } =
        new(nameof(RedBrinstarSave), 0xCE95C2, 0x19, 0x12, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarSave, Enemies.RoomEnemyPopulationDefinitions.RedBrinstarSave, Enemies.RoomEnemyGraphicsSetDefinitions.RedBrinstarSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.RedBrinstarSave, null, RoomSetupCallback.SharedReturn);
}
