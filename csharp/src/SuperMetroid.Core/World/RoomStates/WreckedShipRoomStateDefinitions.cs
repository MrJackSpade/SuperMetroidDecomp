using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Wrecked Ship room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class WreckedShipRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_BowlingAlley_0.</summary>
    public static RoomStateSettings BowlingAlley0 { get; } =
        new(nameof(BowlingAlley0), RoomLevelDefinitions.BowlingAlley, 0x05, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyState0, Enemies.RoomEnemyPopulationDefinitions.BowlingAlley0, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlley0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BowlingAlleyState0, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_BowlingAlley_1.</summary>
    public static RoomStateSettings BowlingAlley1 { get; } =
        new(nameof(BowlingAlley1), RoomLevelDefinitions.BowlingAlley, 0x04, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyState1, Enemies.RoomEnemyPopulationDefinitions.BowlingAlley1, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlley1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BowlingAlleyState1, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipEntrance_0.</summary>
    public static RoomStateSettings WreckedShipEntrance0 { get; } =
        new(nameof(WreckedShipEntrance0), RoomLevelDefinitions.WreckedShipEntrance, 0x05, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipEntrance0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipEntrance0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WreckedShipEntranceState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45EntranceHall, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipEntrance_1.</summary>
    public static RoomStateSettings WreckedShipEntrance1 { get; } =
        new(nameof(WreckedShipEntrance1), RoomLevelDefinitions.WreckedShipEntrance, 0x04, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipEntrance1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipEntrance1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WreckedShipEntranceState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45EntranceHall, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_Attic_0.</summary>
    public static RoomStateSettings Attic0 { get; } =
        new(nameof(Attic0), RoomLevelDefinitions.Attic0, 0x05, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.AtticState0, Enemies.RoomEnemyPopulationDefinitions.Attic0, Enemies.RoomEnemyGraphicsSetDefinitions.Attic0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xFF), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Attic, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AtticState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ExperimentRoom, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_Attic_1.</summary>
    public static RoomStateSettings Attic1 { get; } =
        new(nameof(Attic1), RoomLevelDefinitions.Attic1, 0x04, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.AtticState1, Enemies.RoomEnemyPopulationDefinitions.Attic1, Enemies.RoomEnemyGraphicsSetDefinitions.Attic1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xFF), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Attic, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AtticState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ExperimentRoom, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_AssemblyLine_0.</summary>
    public static RoomStateSettings AssemblyLine0 { get; } =
        new(nameof(AssemblyLine0), RoomLevelDefinitions.AssemblyLine, 0x05, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineState0, Enemies.RoomEnemyPopulationDefinitions.AssemblyLine0, Enemies.RoomEnemyGraphicsSetDefinitions.AssemblyLine0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AssemblyLineState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsVertical, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_AssemblyLine_1.</summary>
    public static RoomStateSettings AssemblyLine1 { get; } =
        new(nameof(AssemblyLine1), RoomLevelDefinitions.AssemblyLine, 0x04, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineState1, Enemies.RoomEnemyPopulationDefinitions.AssemblyLine1, Enemies.RoomEnemyGraphicsSetDefinitions.AssemblyLine1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AssemblyLineState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsVertical, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipMainShaft_0.</summary>
    public static RoomStateSettings WreckedShipMainShaft0 { get; } =
        new(nameof(WreckedShipMainShaft0), RoomLevelDefinitions.WreckedShipMainShaft0, 0x05, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMainShaft0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaft0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WreckedShipMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipMainShaft_1.</summary>
    public static RoomStateSettings WreckedShipMainShaft1 { get; } =
        new(nameof(WreckedShipMainShaft1), RoomLevelDefinitions.WreckedShipMainShaft1, 0x04, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMainShaft1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaft1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WreckedShipMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_SpikyDeath_0.</summary>
    public static RoomStateSettings SpikyDeath0 { get; } =
        new(nameof(SpikyDeath0), RoomLevelDefinitions.SpikyDeath, 0x05, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathState0, Enemies.RoomEnemyPopulationDefinitions.SpikyDeath0, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyDeath0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpikyDeathState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_SpikyDeath_1.</summary>
    public static RoomStateSettings SpikyDeath1 { get; } =
        new(nameof(SpikyDeath1), RoomLevelDefinitions.SpikyDeath, 0x04, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathState1, Enemies.RoomEnemyPopulationDefinitions.SpikyDeath1, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyDeath1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpikyDeathState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_ElectricDeath_0.</summary>
    public static RoomStateSettings ElectricDeath0 { get; } =
        new(nameof(ElectricDeath0), RoomLevelDefinitions.ElectricDeath, 0x05, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathState0, Enemies.RoomEnemyPopulationDefinitions.ElectricDeath0, Enemies.RoomEnemyGraphicsSetDefinitions.ElectricDeath0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ElectricDeathState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_ElectricDeath_1.</summary>
    public static RoomStateSettings ElectricDeath1 { get; } =
        new(nameof(ElectricDeath1), RoomLevelDefinitions.ElectricDeath, 0x04, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathState1, Enemies.RoomEnemyPopulationDefinitions.ElectricDeath1, Enemies.RoomEnemyGraphicsSetDefinitions.ElectricDeath1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ElectricDeathState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipETank_0.</summary>
    public static RoomStateSettings WreckedShipETank0 { get; } =
        new(nameof(WreckedShipETank0), RoomLevelDefinitions.WreckedShipETank, 0x05, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipETank0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipETank0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WreckedShipETankState0, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_WreckedShipETank_1.</summary>
    public static RoomStateSettings WreckedShipETank1 { get; } =
        new(nameof(WreckedShipETank1), RoomLevelDefinitions.WreckedShipETank, 0x04, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipETank1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipETank1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WreckedShipETankState1, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The disassembly's RoomState_Basement_0.</summary>
    public static RoomStateSettings Basement0 { get; } =
        new(nameof(Basement0), RoomLevelDefinitions.Basement, 0x05, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BasementState0, Enemies.RoomEnemyPopulationDefinitions.Basement0, Enemies.RoomEnemyGraphicsSetDefinitions.Basement0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Basement, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BasementState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile);


    /// <summary>The disassembly's RoomState_Basement_1.</summary>
    public static RoomStateSettings Basement1 { get; } =
        new(nameof(Basement1), RoomLevelDefinitions.Basement, 0x04, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.BasementState1, Enemies.RoomEnemyPopulationDefinitions.Basement1, Enemies.RoomEnemyGraphicsSetDefinitions.Basement1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Basement, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BasementState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile);


    /// <summary>The disassembly's RoomState_WreckedShipMap_0.</summary>
    public static RoomStateSettings WreckedShipMap0 { get; } =
        new(nameof(WreckedShipMap0), RoomLevelDefinitions.GreenBrinstarMap, 0x16, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapState0, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMap0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMap0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WreckedShipMapState0, null, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WreckedShipMap_1.</summary>
    public static RoomStateSettings WreckedShipMap1 { get; } =
        new(nameof(WreckedShipMap1), RoomLevelDefinitions.GreenBrinstarMap, 0x15, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMap1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMap1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WreckedShipMapState1, null, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_Phantoon_0.</summary>
    public static RoomStateSettings Phantoon0 { get; } =
        new(nameof(Phantoon0), RoomLevelDefinitions.Phantoon, 0x05, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonState0, Enemies.RoomEnemyPopulationDefinitions.Phantoon0, Enemies.RoomEnemyGraphicsSetDefinitions.Phantoon0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Phantoon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.PhantoonState0, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_Phantoon_1.</summary>
    public static RoomStateSettings Phantoon1 { get; } =
        new(nameof(Phantoon1), RoomLevelDefinitions.Phantoon, 0x04, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonState1, Enemies.RoomEnemyPopulationDefinitions.Phantoon1, Enemies.RoomEnemyGraphicsSetDefinitions.Phantoon1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Phantoon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.PhantoonDraygonState1, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_SpongeBath_0.</summary>
    public static RoomStateSettings SpongeBath0 { get; } =
        new(nameof(SpongeBath0), RoomLevelDefinitions.SpongeBath, 0x05, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathState0, Enemies.RoomEnemyPopulationDefinitions.SpongeBath0, Enemies.RoomEnemyGraphicsSetDefinitions.SpongeBath0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpongeBathState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_SpongeBath_1.</summary>
    public static RoomStateSettings SpongeBath1 { get; } =
        new(nameof(SpongeBath1), RoomLevelDefinitions.SpongeBath, 0x04, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathState1, Enemies.RoomEnemyPopulationDefinitions.SpongeBath1, Enemies.RoomEnemyGraphicsSetDefinitions.SpongeBath1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SpongeBathState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WSWestSuper_0.</summary>
    public static RoomStateSettings WSWestSuper0 { get; } =
        new(nameof(WSWestSuper0), RoomLevelDefinitions.WSWestSuper, 0x05, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperState0, Enemies.RoomEnemyPopulationDefinitions.WSWestSuper0, Enemies.RoomEnemyGraphicsSetDefinitions.WSWestSuper0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WSWestSuperState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WSWestSuper_1.</summary>
    public static RoomStateSettings WSWestSuper1 { get; } =
        new(nameof(WSWestSuper1), RoomLevelDefinitions.WSWestSuper, 0x04, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperState1, Enemies.RoomEnemyPopulationDefinitions.WSWestSuper1, Enemies.RoomEnemyGraphicsSetDefinitions.WSWestSuper1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WSWestSuperState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WSEastSuper_0.</summary>
    public static RoomStateSettings WSEastSuper0 { get; } =
        new(nameof(WSEastSuper0), RoomLevelDefinitions.WSEastSuper0, 0x05, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRoomsState0839C74, Enemies.RoomEnemyPopulationDefinitions.WSEastSuper0, Enemies.RoomEnemyGraphicsSetDefinitions.WSEastSuper0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WSEastSuperState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WSEastSuper_1.</summary>
    public static RoomStateSettings WSEastSuper1 { get; } =
        new(nameof(WSEastSuper1), RoomLevelDefinitions.WSEastSuper1, 0x04, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WSEastSuperState1, Enemies.RoomEnemyPopulationDefinitions.WSEastSuper1, Enemies.RoomEnemyGraphicsSetDefinitions.WSEastSuper1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WSEastSuperState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_GravitySuit_0.</summary>
    public static RoomStateSettings GravitySuit0 { get; } =
        new(nameof(GravitySuit0), RoomLevelDefinitions.GravitySuit, 0x05, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRoomsState0839C74, Enemies.RoomEnemyPopulationDefinitions.GravitySuit0, Enemies.RoomEnemyGraphicsSetDefinitions.GravitySuit0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GravitySuitState0WreckedShipSaveState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_GravitySuit_1.</summary>
    public static RoomStateSettings GravitySuit1 { get; } =
        new(nameof(GravitySuit1), RoomLevelDefinitions.GravitySuit, 0x04, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitState1WreckedShipSaveState1, Enemies.RoomEnemyPopulationDefinitions.GravitySuit1, Enemies.RoomEnemyGraphicsSetDefinitions.GravitySuit1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GravitySuitState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The disassembly's RoomState_WreckedShipSave_0.</summary>
    public static RoomStateSettings WreckedShipSave0 { get; } =
        new(nameof(WreckedShipSave0), RoomLevelDefinitions.WarehouseSave, 0x16, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRoomsState0839C74, Enemies.RoomEnemyPopulationDefinitions.WreckedShipSave0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipSave0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GravitySuitState0WreckedShipSaveState0, null, RoomSetupCallback.None);


    /// <summary>The disassembly's RoomState_WreckedShipSave_1.</summary>
    public static RoomStateSettings WreckedShipSave1 { get; } =
        new(nameof(WreckedShipSave1), RoomLevelDefinitions.WarehouseSave, 0x15, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitState1WreckedShipSaveState1, Enemies.RoomEnemyPopulationDefinitions.WreckedShipSave1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipSave1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WreckedShipSaveState1, null, RoomSetupCallback.BossRoomReturn);
}
