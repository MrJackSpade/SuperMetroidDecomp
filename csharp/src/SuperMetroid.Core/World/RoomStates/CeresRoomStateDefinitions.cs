using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Ceres room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class CeresRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_CeresElev_0.</summary>
    public static CartridgeRoomState CeresElev0 { get; } =
        new(nameof(CeresElev0), 0xCDB846, 0x11, new RoomMusic(MusicDataSet.Ceres, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElevState01, Enemies.RoomEnemyPopulationDefinitions.CeresElev0, Enemies.RoomEnemyGraphicsSetDefinitions.CeresElev0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.CeresElevatorShaft, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CeresElevState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.CeresElevatorState01, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_CeresElev_1.</summary>
    public static CartridgeRoomState CeresElev1 { get; } =
        new(nameof(CeresElev1), 0xCDB846, 0x12, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElevState01, Enemies.RoomEnemyPopulationDefinitions.CeresElev1, Enemies.RoomEnemyGraphicsSetDefinitions.CeresElev1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.CeresElevatorShaft, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CeresElevState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.CeresElevatorState01, RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze);


    /// <summary>The disassembly's RoomState_FallingTile_0.</summary>
    public static CartridgeRoomState FallingTile0 { get; } =
        new(nameof(FallingTile0), 0xCDBBFE, 0x0F, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileState0, Enemies.RoomEnemyPopulationDefinitions.FallingTile0, Enemies.RoomEnemyGraphicsSetDefinitions.FallingTile0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FallingTileState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_FallingTile_1.</summary>
    public static CartridgeRoomState FallingTile1 { get; } =
        new(nameof(FallingTile1), 0xCDBBFE, 0x10, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileState1, Enemies.RoomEnemyPopulationDefinitions.FallingTile1, Enemies.RoomEnemyGraphicsSetDefinitions.FallingTile1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FallingTileState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_MagnetStairs_0.</summary>
    public static CartridgeRoomState MagnetStairs0 { get; } =
        new(nameof(MagnetStairs0), 0xCDBD78, 0x0F, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsState0, Enemies.RoomEnemyPopulationDefinitions.MagnetStairs0, Enemies.RoomEnemyGraphicsSetDefinitions.MagnetStairs0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x41), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MagnetStairsState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_MagnetStairs_1.</summary>
    public static CartridgeRoomState MagnetStairs1 { get; } =
        new(nameof(MagnetStairs1), 0xCDBD78, 0x10, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsState1, Enemies.RoomEnemyPopulationDefinitions.MagnetStairs1, Enemies.RoomEnemyGraphicsSetDefinitions.MagnetStairs1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MagnetStairsState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_DeadScientist_0.</summary>
    public static CartridgeRoomState DeadScientist0 { get; } =
        new(nameof(DeadScientist0), 0xCDBFC9, 0x0F, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistState0, Enemies.RoomEnemyPopulationDefinitions.DeadScientist0, Enemies.RoomEnemyGraphicsSetDefinitions.DeadScientist0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DeadScientistState01, null, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_DeadScientist_1.</summary>
    public static CartridgeRoomState DeadScientist1 { get; } =
        new(nameof(DeadScientist1), 0xCDBFC9, 0x10, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistState1, Enemies.RoomEnemyPopulationDefinitions.DeadScientist1, Enemies.RoomEnemyGraphicsSetDefinitions.DeadScientist1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DeadScientistState01, null, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_58Escape_0.</summary>
    public static CartridgeRoomState Room58Escape0 { get; } =
        new(nameof(Room58Escape0), 0xCDC330, 0x0F, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Room58EscapeState0, Enemies.RoomEnemyPopulationDefinitions.Room58Escape0, Enemies.RoomEnemyGraphicsSetDefinitions.Room58Escape0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Room58EscapeState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_58Escape_1.</summary>
    public static CartridgeRoomState Room58Escape1 { get; } =
        new(nameof(Room58Escape1), 0xCDC330, 0x10, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Room58EscapeState1, Enemies.RoomEnemyPopulationDefinitions.Room58Escape1, Enemies.RoomEnemyGraphicsSetDefinitions.Room58Escape1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Room58EscapeState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgramDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_CeresRidley_0.</summary>
    public static CartridgeRoomState CeresRidley0 { get; } =
        new(nameof(CeresRidley0), 0xCDC43F, 0x13, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Silenced), SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyState0, Enemies.RoomEnemyPopulationDefinitions.CeresRidley0, Enemies.RoomEnemyGraphicsSetDefinitions.CeresRidley0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.HandleCeresRidleyGetawayCutscene, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CeresRidleyState01, null, RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze);


    /// <summary>The disassembly's RoomState_CeresRidley_1.</summary>
    public static CartridgeRoomState CeresRidley1 { get; } =
        new(nameof(CeresRidley1), 0xCDC43F, 0x14, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyState1, Enemies.RoomEnemyPopulationDefinitions.CeresRidley1, Enemies.RoomEnemyGraphicsSetDefinitions.CeresRidley1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.HandleCeresRidleyGetawayCutscene, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CeresRidleyState01, null, RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze);
}
