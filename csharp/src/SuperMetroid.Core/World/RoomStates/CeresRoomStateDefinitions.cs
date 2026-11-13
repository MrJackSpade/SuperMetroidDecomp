using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Ceres room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class CeresRoomStateDefinitions
{

    /// <summary>The Ceres Elev room's state 0.</summary>
    public static RoomStateSettings CeresElevDefault { get; } =
        new(nameof(CeresElevDefault), RoomLevelDefinitions.CeresElev, RoomGraphicsSet.BlueCeresElevator, new RoomMusic(MusicDataSet.Ceres, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElev, RoomEnemyPopulationDefinitions.CeresElevDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.CeresElevatorShaft, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresElev, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The Ceres Elev room's state 1.</summary>
    public static RoomStateSettings CeresElevWhenAreaBossDead { get; } =
        new(nameof(CeresElevWhenAreaBossDead), RoomLevelDefinitions.CeresElev, RoomGraphicsSet.WhiteCeresElevator, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElev, RoomEnemyPopulationDefinitions.CeresElevWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.CeresElevatorShaft, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresElev, RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze);


    /// <summary>The Falling Tile room's state 0.</summary>
    public static RoomStateSettings FallingTileDefault { get; } =
        new(nameof(FallingTileDefault), RoomLevelDefinitions.FallingTile, RoomGraphicsSet.BlueCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileDefault, RoomEnemyPopulationDefinitions.FallingTileDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FallingTile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Tourian13And14StatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The Falling Tile room's state 1.</summary>
    public static RoomStateSettings FallingTileWhenAreaBossDead { get; } =
        new(nameof(FallingTileWhenAreaBossDead), RoomLevelDefinitions.FallingTile, RoomGraphicsSet.WhiteCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileWhenAreaBossDead, RoomEnemyPopulationDefinitions.FallingTileWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FallingTile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Tourian13And14StatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The Magnet Stairs room's state 0.</summary>
    public static RoomStateSettings MagnetStairsDefault { get; } =
        new(nameof(MagnetStairsDefault), RoomLevelDefinitions.MagnetStairs, RoomGraphicsSet.BlueCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsDefault, RoomEnemyPopulationDefinitions.MagnetStairsDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 65), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MagnetStairs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Tourian13And14StatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The Magnet Stairs room's state 1.</summary>
    public static RoomStateSettings MagnetStairsWhenAreaBossDead { get; } =
        new(nameof(MagnetStairsWhenAreaBossDead), RoomLevelDefinitions.MagnetStairs, RoomGraphicsSet.WhiteCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsWhenAreaBossDead, RoomEnemyPopulationDefinitions.MagnetStairsWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MagnetStairs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Tourian13And14StatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The Dead Scientist room's state 0.</summary>
    public static RoomStateSettings DeadScientistDefault { get; } =
        new(nameof(DeadScientistDefault), RoomLevelDefinitions.DeadScientist, RoomGraphicsSet.BlueCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistDefault, RoomEnemyPopulationDefinitions.DeadScientistDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DeadScientist, null, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The Dead Scientist room's state 1.</summary>
    public static RoomStateSettings DeadScientistWhenAreaBossDead { get; } =
        new(nameof(DeadScientistWhenAreaBossDead), RoomLevelDefinitions.DeadScientist, RoomGraphicsSet.WhiteCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistWhenAreaBossDead, RoomEnemyPopulationDefinitions.DeadScientistWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DeadScientist, null, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The Room Escape room's state 0.</summary>
    public static RoomStateSettings Room58EscapeDefault { get; } =
        new(nameof(Room58EscapeDefault), RoomLevelDefinitions.Room58Escape, RoomGraphicsSet.BlueCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Room58EscapeDefault, RoomEnemyPopulationDefinitions.Room58EscapeDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Room58Escape, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Tourian13And14StatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The Room Escape room's state 1.</summary>
    public static RoomStateSettings Room58EscapeWhenAreaBossDead { get; } =
        new(nameof(Room58EscapeWhenAreaBossDead), RoomLevelDefinitions.Room58Escape, RoomGraphicsSet.WhiteCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Room58EscapeWhenAreaBossDead, RoomEnemyPopulationDefinitions.Room58EscapeWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.Return, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Room58Escape, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Tourian13And14StatueHall, RoomSetupCallback.SpawnCeresHaze);


    /// <summary>The Ceres Ridley room's state 0.</summary>
    public static RoomStateSettings CeresRidleyDefault { get; } =
        new(nameof(CeresRidleyDefault), RoomLevelDefinitions.CeresRidley, RoomGraphicsSet.BlueCeresRidley, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Silenced), SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyDefault, RoomEnemyPopulationDefinitions.CeresRidleyDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.HandleCeresRidleyGetawayCutscene, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresRidley, null, RoomSetupCallback.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze);


    /// <summary>The Ceres Ridley room's state 1.</summary>
    public static RoomStateSettings CeresRidleyWhenAreaBossDead { get; } =
        new(nameof(CeresRidleyWhenAreaBossDead), RoomLevelDefinitions.CeresRidley, RoomGraphicsSet.WhiteCeresRidley, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyWhenAreaBossDead, RoomEnemyPopulationDefinitions.CeresRidleyWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.HandleCeresRidleyGetawayCutscene, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresRidley, null, RoomSetupCallback.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze);
}
