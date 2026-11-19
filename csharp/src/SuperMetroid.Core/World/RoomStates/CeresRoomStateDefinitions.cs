using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Ceres room state, named for their rooms.</summary>
public static class CeresRoomStateDefinitions
{

    /// <summary>The Ceres Elevator room's default state.</summary>
    public static RoomStateSettings CeresElevatorDefault { get; } =
        new(nameof(CeresElevatorDefault), RoomLevelDefinitions.CeresElevator, RoomGraphicsSet.BlueCeresElevator, new RoomMusic(MusicDataSet.Ceres, RoomMusicTrack.Plays(MusicTrack.CeresStation)), SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElevator, RoomEnemyPopulationDefinitions.CeresElevatorDefault, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.CeresElevatorShaft, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresElevator, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Cleared, RoomSetupBehavior.SpawnCeresHaze) { TurningFloor = RoomTurningFloor.ElevatorShaft };


    /// <summary>The Ceres Elevator room's state once the area's boss is dead.</summary>
    public static RoomStateSettings CeresElevatorWhenAreaBossDead { get; } =
        new(nameof(CeresElevatorWhenAreaBossDead), RoomLevelDefinitions.CeresElevator, RoomGraphicsSet.WhiteCeresElevator, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresElevator, RoomEnemyPopulationDefinitions.CeresElevatorWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.CeresElevatorShaft, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresElevator, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Cleared, RoomSetupBehavior.SealCeresElevatorDoorAndSpawnHaze) { TurningFloor = RoomTurningFloor.ElevatorShaft };


    /// <summary>The Falling Tile room's default state.</summary>
    public static RoomStateSettings FallingTileDefault { get; } =
        new(nameof(FallingTileDefault), RoomLevelDefinitions.FallingTile, RoomGraphicsSet.BlueCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileDefault, RoomEnemyPopulationDefinitions.FallingTileDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.SpawnCeresPreElevatorHallFallingDebris, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FallingTile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresCapsuleHall, RoomSetupBehavior.SpawnCeresHaze);


    /// <summary>The Falling Tile room's state once the area's boss is dead.</summary>
    public static RoomStateSettings FallingTileWhenAreaBossDead { get; } =
        new(nameof(FallingTileWhenAreaBossDead), RoomLevelDefinitions.FallingTile, RoomGraphicsSet.WhiteCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FallingTileWhenAreaBossDead, RoomEnemyPopulationDefinitions.FallingTileWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.SpawnCeresPreElevatorHallFallingDebris, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FallingTile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresCapsuleHall, RoomSetupBehavior.SpawnCeresHaze);


    /// <summary>The Magnet Stairs room's default state.</summary>
    public static RoomStateSettings MagnetStairsDefault { get; } =
        new(nameof(MagnetStairsDefault), RoomLevelDefinitions.MagnetStairs, RoomGraphicsSet.BlueCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsDefault, RoomEnemyPopulationDefinitions.MagnetStairsDefault, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MagnetStairs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresCapsuleHall, RoomSetupBehavior.SpawnCeresHaze);


    /// <summary>The Magnet Stairs room's state once the area's boss is dead.</summary>
    public static RoomStateSettings MagnetStairsWhenAreaBossDead { get; } =
        new(nameof(MagnetStairsWhenAreaBossDead), RoomLevelDefinitions.MagnetStairs, RoomGraphicsSet.WhiteCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagnetStairsWhenAreaBossDead, RoomEnemyPopulationDefinitions.MagnetStairsWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MagnetStairs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresCapsuleHall, RoomSetupBehavior.SpawnCeresHaze);


    /// <summary>The Dead Scientist room's default state.</summary>
    public static RoomStateSettings DeadScientistDefault { get; } =
        new(nameof(DeadScientistDefault), RoomLevelDefinitions.DeadScientist, RoomGraphicsSet.BlueCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistDefault, RoomEnemyPopulationDefinitions.DeadScientistDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DeadScientist, null, RoomSetupBehavior.SpawnCeresHaze);


    /// <summary>The Dead Scientist room's state once the area's boss is dead.</summary>
    public static RoomStateSettings DeadScientistWhenAreaBossDead { get; } =
        new(nameof(DeadScientistWhenAreaBossDead), RoomLevelDefinitions.DeadScientist, RoomGraphicsSet.WhiteCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DeadScientistWhenAreaBossDead, RoomEnemyPopulationDefinitions.DeadScientistWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DeadScientist, null, RoomSetupBehavior.SpawnCeresHaze);


    /// <summary>The Ceres Ridley hallway's default state.</summary>
    public static RoomStateSettings CeresRidleyHallwayDefault { get; } =
        new(nameof(CeresRidleyHallwayDefault), RoomLevelDefinitions.CeresRidleyHallway, RoomGraphicsSet.BlueCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyHallwayDefault, RoomEnemyPopulationDefinitions.CeresRidleyHallwayDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresRidleyHallway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresCapsuleHall, RoomSetupBehavior.SpawnCeresHaze);


    /// <summary>The Ceres Ridley hallway's state once the area's boss is dead.</summary>
    public static RoomStateSettings CeresRidleyHallwayWhenAreaBossDead { get; } =
        new(nameof(CeresRidleyHallwayWhenAreaBossDead), RoomLevelDefinitions.CeresRidleyHallway, RoomGraphicsSet.WhiteCeres, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyHallwayWhenAreaBossDead, RoomEnemyPopulationDefinitions.CeresRidleyHallwayWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresRidleyHallway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CeresCapsuleHall, RoomSetupBehavior.SpawnCeresHaze);


    /// <summary>The Ceres Ridley room's default state.</summary>
    public static RoomStateSettings CeresRidleyDefault { get; } =
        new(nameof(CeresRidleyDefault), RoomLevelDefinitions.CeresRidley, RoomGraphicsSet.BlueCeresRidley, new RoomMusic(MusicDataSet.RidleyAndDraygonBattles, RoomMusicTrack.Silenced), SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyDefault, RoomEnemyPopulationDefinitions.CeresRidleyDefault, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.CeresRidleyGetaway, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresRidley, null, RoomSetupBehavior.SpawnCeresHaze) { TurningFloor = RoomTurningFloor.RidleyGetaway };


    /// <summary>The Ceres Ridley room's state once the area's boss is dead.</summary>
    public static RoomStateSettings CeresRidleyWhenAreaBossDead { get; } =
        new(nameof(CeresRidleyWhenAreaBossDead), RoomLevelDefinitions.CeresRidley, RoomGraphicsSet.WhiteCeresRidley, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CeresRidleyWhenAreaBossDead, RoomEnemyPopulationDefinitions.CeresRidleyWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.CeresRidleyGetaway, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CeresRidley, null, RoomSetupBehavior.SpawnCeresHaze);
}
