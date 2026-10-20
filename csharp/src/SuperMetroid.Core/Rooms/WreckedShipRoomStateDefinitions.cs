namespace SuperMetroid.Core.Rooms;

/// <summary>The initialization settings of each Wrecked Ship room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class WreckedShipRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_BowlingAlley_0.</summary>
    public static CartridgeRoomState BowlingAlley0 { get; } =
        new(nameof(BowlingAlley0), 0xC48322, 0x05, 0x30, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyState0, Enemies.RoomEnemyPopulationDefinitions.BowlingAlley0, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlley0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BowlingAlleyState0, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_BowlingAlley_1.</summary>
    public static CartridgeRoomState BowlingAlley1 { get; } =
        new(nameof(BowlingAlley1), 0xC48322, 0x04, 0x30, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyState1, Enemies.RoomEnemyPopulationDefinitions.BowlingAlley1, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlley1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BowlingAlleyState1, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipEntrance_0.</summary>
    public static CartridgeRoomState WreckedShipEntrance0 { get; } =
        new(nameof(WreckedShipEntrance0), 0xC49D2E, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipEntrance0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipEntrance0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipEntranceState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45EntranceHall, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipEntrance_1.</summary>
    public static CartridgeRoomState WreckedShipEntrance1 { get; } =
        new(nameof(WreckedShipEntrance1), 0xC49D2E, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipEntrance1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipEntrance1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipEntranceState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45EntranceHall, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_Attic_0.</summary>
    public static CartridgeRoomState Attic0 { get; } =
        new(nameof(Attic0), 0xC49EAE, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.AtticState0, Enemies.RoomEnemyPopulationDefinitions.Attic0, Enemies.RoomEnemyGraphicsSetDefinitions.Attic0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xFF), Scrolling.RoomScrollDefinitions.Attic, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AtticState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ExperimentRoom, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_Attic_1.</summary>
    public static CartridgeRoomState Attic1 { get; } =
        new(nameof(Attic1), 0xC4A2E7, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.AtticState1, Enemies.RoomEnemyPopulationDefinitions.Attic1, Enemies.RoomEnemyGraphicsSetDefinitions.Attic1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xFF), Scrolling.RoomScrollDefinitions.Attic, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AtticState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ExperimentRoom, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_AssemblyLine_0.</summary>
    public static CartridgeRoomState AssemblyLine0 { get; } =
        new(nameof(AssemblyLine0), 0xC4A720, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineState0, Enemies.RoomEnemyPopulationDefinitions.AssemblyLine0, Enemies.RoomEnemyGraphicsSetDefinitions.AssemblyLine0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AssemblyLineState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsVertical, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_AssemblyLine_1.</summary>
    public static CartridgeRoomState AssemblyLine1 { get; } =
        new(nameof(AssemblyLine1), 0xC4A720, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineState1, Enemies.RoomEnemyPopulationDefinitions.AssemblyLine1, Enemies.RoomEnemyGraphicsSetDefinitions.AssemblyLine1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.AssemblyLineState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsVertical, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipMainShaft_0.</summary>
    public static CartridgeRoomState WreckedShipMainShaft0 { get; } =
        new(nameof(WreckedShipMainShaft0), 0xC4A9AC, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMainShaft0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaft0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipMainShaft, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipMainShaft_1.</summary>
    public static CartridgeRoomState WreckedShipMainShaft1 { get; } =
        new(nameof(WreckedShipMainShaft1), 0xC4BDC0, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMainShaft1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaft1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipMainShaft, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_SpikyDeath_0.</summary>
    public static CartridgeRoomState SpikyDeath0 { get; } =
        new(nameof(SpikyDeath0), 0xC4D187, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathState0, Enemies.RoomEnemyPopulationDefinitions.SpikyDeath0, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyDeath0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpikyDeathState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_SpikyDeath_1.</summary>
    public static CartridgeRoomState SpikyDeath1 { get; } =
        new(nameof(SpikyDeath1), 0xC4D187, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathState1, Enemies.RoomEnemyPopulationDefinitions.SpikyDeath1, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyDeath1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpikyDeathState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_ElectricDeath_0.</summary>
    public static CartridgeRoomState ElectricDeath0 { get; } =
        new(nameof(ElectricDeath0), 0xC4D3EE, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathState0, Enemies.RoomEnemyPopulationDefinitions.ElectricDeath0, Enemies.RoomEnemyGraphicsSetDefinitions.ElectricDeath0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ElectricDeathState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_ElectricDeath_1.</summary>
    public static CartridgeRoomState ElectricDeath1 { get; } =
        new(nameof(ElectricDeath1), 0xC4D3EE, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathState1, Enemies.RoomEnemyPopulationDefinitions.ElectricDeath1, Enemies.RoomEnemyGraphicsSetDefinitions.ElectricDeath1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.ElectricDeathState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipETank_0.</summary>
    public static CartridgeRoomState WreckedShipETank0 { get; } =
        new(nameof(WreckedShipETank0), 0xC4D883, 0x05, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipETank0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipETank0, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipETankState0, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipETank_1.</summary>
    public static CartridgeRoomState WreckedShipETank1 { get; } =
        new(nameof(WreckedShipETank1), 0xC4D883, 0x04, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipETank1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipETank1, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipETankState1, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_Basement_0.</summary>
    public static CartridgeRoomState Basement0 { get; } =
        new(nameof(Basement0), 0xC4E14E, 0x05, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.BasementState0, Enemies.RoomEnemyPopulationDefinitions.Basement0, Enemies.RoomEnemyGraphicsSetDefinitions.Basement0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Basement, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BasementState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile);


    /// <summary>The disassembly's RoomState_Basement_1.</summary>
    public static CartridgeRoomState Basement1 { get; } =
        new(nameof(Basement1), 0xC4E14E, 0x04, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.BasementState1, Enemies.RoomEnemyPopulationDefinitions.Basement1, Enemies.RoomEnemyGraphicsSetDefinitions.Basement1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.Basement, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.BasementState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile);


    /// <summary>The disassembly's RoomState_WreckedShipMap_0.</summary>
    public static CartridgeRoomState WreckedShipMap0 { get; } =
        new(nameof(WreckedShipMap0), 0xCE83C3, 0x16, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMap0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMap0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipMapState0, null, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WreckedShipMap_1.</summary>
    public static CartridgeRoomState WreckedShipMap1 { get; } =
        new(nameof(WreckedShipMap1), 0xCE83C3, 0x15, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMap1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMap1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipMapState1, null, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_Phantoon_0.</summary>
    public static CartridgeRoomState Phantoon0 { get; } =
        new(nameof(Phantoon0), 0xC4E58C, 0x05, 0x27, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonState0, Enemies.RoomEnemyPopulationDefinitions.Phantoon0, Enemies.RoomEnemyGraphicsSetDefinitions.Phantoon0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Phantoon, Backgrounds.LibraryBackgroundProgramDefinitions.PhantoonState0, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_Phantoon_1.</summary>
    public static CartridgeRoomState Phantoon1 { get; } =
        new(nameof(Phantoon1), 0xC4E58C, 0x04, 0x00, 0x03, SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonState1, Enemies.RoomEnemyPopulationDefinitions.Phantoon1, Enemies.RoomEnemyGraphicsSetDefinitions.Phantoon1, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.Phantoon, Backgrounds.LibraryBackgroundProgramDefinitions.PhantoonDraygonState1, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_SpongeBath_0.</summary>
    public static CartridgeRoomState SpongeBath0 { get; } =
        new(nameof(SpongeBath0), 0xC4E6A5, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathState0, Enemies.RoomEnemyPopulationDefinitions.SpongeBath0, Enemies.RoomEnemyGraphicsSetDefinitions.SpongeBath0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpongeBathState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_SpongeBath_1.</summary>
    public static CartridgeRoomState SpongeBath1 { get; } =
        new(nameof(SpongeBath1), 0xC4E6A5, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathState1, Enemies.RoomEnemyPopulationDefinitions.SpongeBath1, Enemies.RoomEnemyGraphicsSetDefinitions.SpongeBath1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.SpongeBathState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WSWestSuper_0.</summary>
    public static CartridgeRoomState WSWestSuper0 { get; } =
        new(nameof(WSWestSuper0), 0xC4E94E, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperState0, Enemies.RoomEnemyPopulationDefinitions.WSWestSuper0, Enemies.RoomEnemyGraphicsSetDefinitions.WSWestSuper0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WSWestSuperState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WSWestSuper_1.</summary>
    public static CartridgeRoomState WSWestSuper1 { get; } =
        new(nameof(WSWestSuper1), 0xC4E94E, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperState1, Enemies.RoomEnemyPopulationDefinitions.WSWestSuper1, Enemies.RoomEnemyGraphicsSetDefinitions.WSWestSuper1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WSWestSuperState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WSEastSuper_0.</summary>
    public static CartridgeRoomState WSEastSuper0 { get; } =
        new(nameof(WSEastSuper0), 0xC4EA8F, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRoomsState0839C74, Enemies.RoomEnemyPopulationDefinitions.WSEastSuper0, Enemies.RoomEnemyGraphicsSetDefinitions.WSEastSuper0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WSEastSuperState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WSEastSuper_1.</summary>
    public static CartridgeRoomState WSEastSuper1 { get; } =
        new(nameof(WSEastSuper1), 0xC4ED98, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.WSEastSuperState1, Enemies.RoomEnemyPopulationDefinitions.WSEastSuper1, Enemies.RoomEnemyGraphicsSetDefinitions.WSEastSuper1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WSEastSuperState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_GravitySuit_0.</summary>
    public static CartridgeRoomState GravitySuit0 { get; } =
        new(nameof(GravitySuit0), 0xC4F0A1, 0x05, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRoomsState0839C74, Enemies.RoomEnemyPopulationDefinitions.GravitySuit0, Enemies.RoomEnemyGraphicsSetDefinitions.GravitySuit0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GravitySuitState0WreckedShipSaveState0, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_GravitySuit_1.</summary>
    public static CartridgeRoomState GravitySuit1 { get; } =
        new(nameof(GravitySuit1), 0xC4F0A1, 0x04, 0x00, 0x00, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitState1WreckedShipSaveState1, Enemies.RoomEnemyPopulationDefinitions.GravitySuit1, Enemies.RoomEnemyGraphicsSetDefinitions.GravitySuit1, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GravitySuitState1, Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WreckedShipSave_0.</summary>
    public static CartridgeRoomState WreckedShipSave0 { get; } =
        new(nameof(WreckedShipSave0), 0xCE95C2, 0x16, 0x30, 0x05, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRoomsState0839C74, Enemies.RoomEnemyPopulationDefinitions.WreckedShipSave0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipSave0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.GravitySuitState0WreckedShipSaveState0, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_WreckedShipSave_1.</summary>
    public static CartridgeRoomState WreckedShipSave1 { get; } =
        new(nameof(WreckedShipSave1), 0xCE95C2, 0x15, 0x30, 0x06, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitState1WreckedShipSaveState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipSave1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipSave1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, Plms.RoomPlmPopulationDefinitions.WreckedShipSaveState1, null, RoomSetupCallback.BossRoomReturn);
}
