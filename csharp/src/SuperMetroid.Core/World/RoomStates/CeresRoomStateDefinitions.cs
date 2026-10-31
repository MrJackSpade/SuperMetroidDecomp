using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Ceres room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class CeresRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_CeresElev_0.</summary>
    public static RoomStateSettings CeresElev0 { get; } =
        new(nameof(CeresElev0), RoomLevelDefinitions.CeresElev, 17, new RoomMusic(MusicDataSet.Ceres, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElevState01, RoomEnemyPopulationDefinitions.CeresElev0, Enemies.RoomEnemyGraphicsSetDefinitions.CeresElev0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.CeresElevatorShaft, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CeresElevState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresElevatorState01, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_CeresElev_1.</summary>
    public static RoomStateSettings CeresElev1 { get; } =
        new(nameof(CeresElev1), RoomLevelDefinitions.CeresElev, 18, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElevState01, RoomEnemyPopulationDefinitions.CeresElev1, Enemies.RoomEnemyGraphicsSetDefinitions.CeresElev1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.CeresElevatorShaft, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CeresElevState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresElevatorState01, RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze);


    /// <summary>The disassembly's RoomState_FallingTile_0.</summary>
    public static RoomStateSettings FallingTile0 { get; } =
        new(nameof(FallingTile0), RoomLevelDefinitions.FallingTile, 15, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileState0, RoomEnemyPopulationDefinitions.FallingTile0, Enemies.RoomEnemyGraphicsSetDefinitions.FallingTile0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FallingTileState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_FallingTile_1.</summary>
    public static RoomStateSettings FallingTile1 { get; } =
        new(nameof(FallingTile1), RoomLevelDefinitions.FallingTile, 16, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileState1, RoomEnemyPopulationDefinitions.FallingTile1, Enemies.RoomEnemyGraphicsSetDefinitions.FallingTile1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FallingTileState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_MagnetStairs_0.</summary>
    public static RoomStateSettings MagnetStairs0 { get; } =
        new(nameof(MagnetStairs0), RoomLevelDefinitions.MagnetStairs, 15, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsState0, RoomEnemyPopulationDefinitions.MagnetStairs0, Enemies.RoomEnemyGraphicsSetDefinitions.MagnetStairs0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(1, 65), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MagnetStairsState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_MagnetStairs_1.</summary>
    public static RoomStateSettings MagnetStairs1 { get; } =
        new(nameof(MagnetStairs1), RoomLevelDefinitions.MagnetStairs, 16, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsState1, RoomEnemyPopulationDefinitions.MagnetStairs1, Enemies.RoomEnemyGraphicsSetDefinitions.MagnetStairs1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(1, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MagnetStairsState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_DeadScientist_0.</summary>
    public static RoomStateSettings DeadScientist0 { get; } =
        new(nameof(DeadScientist0), RoomLevelDefinitions.DeadScientist, 15, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistState0, RoomEnemyPopulationDefinitions.DeadScientist0, Enemies.RoomEnemyGraphicsSetDefinitions.DeadScientist0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DeadScientistState01, null, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_DeadScientist_1.</summary>
    public static RoomStateSettings DeadScientist1 { get; } =
        new(nameof(DeadScientist1), RoomLevelDefinitions.DeadScientist, 16, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistState1, RoomEnemyPopulationDefinitions.DeadScientist1, Enemies.RoomEnemyGraphicsSetDefinitions.DeadScientist1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DeadScientistState01, null, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_58Escape_0.</summary>
    public static RoomStateSettings Room58Escape0 { get; } =
        new(nameof(Room58Escape0), RoomLevelDefinitions.Room58Escape, 15, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Room58EscapeState0, RoomEnemyPopulationDefinitions.Room58Escape0, Enemies.RoomEnemyGraphicsSetDefinitions.Room58Escape0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Room58EscapeState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_58Escape_1.</summary>
    public static RoomStateSettings Room58Escape1 { get; } =
        new(nameof(Room58Escape1), RoomLevelDefinitions.Room58Escape, 16, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Room58EscapeState1, RoomEnemyPopulationDefinitions.Room58Escape1, Enemies.RoomEnemyGraphicsSetDefinitions.Room58Escape1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Room58EscapeState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDEStatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The disassembly's RoomState_CeresRidley_0.</summary>
    public static RoomStateSettings CeresRidley0 { get; } =
        new(nameof(CeresRidley0), RoomLevelDefinitions.CeresRidley, 19, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Silenced), SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyState0, RoomEnemyPopulationDefinitions.CeresRidley0, Enemies.RoomEnemyGraphicsSetDefinitions.CeresRidley0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.HandleCeresRidleyGetawayCutscene, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CeresRidleyState01, null, RoomSetupCallback.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze);


    /// <summary>The disassembly's RoomState_CeresRidley_1.</summary>
    public static RoomStateSettings CeresRidley1 { get; } =
        new(nameof(CeresRidley1), RoomLevelDefinitions.CeresRidley, 20, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyState1, RoomEnemyPopulationDefinitions.CeresRidley1, Enemies.RoomEnemyGraphicsSetDefinitions.CeresRidley1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.HandleCeresRidleyGetawayCutscene, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.CeresRidleyState01, null, RoomSetupCallback.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze);
}
