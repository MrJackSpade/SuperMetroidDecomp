using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Wrecked Ship room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class WreckedShipRoomStateDefinitions
{

    /// <summary>The Bowling Alley room's state 0.</summary>
    public static RoomStateSettings BowlingAlleyDefault { get; } =
        new(nameof(BowlingAlleyDefault), RoomLevelDefinitions.BowlingAlley, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyDefault, RoomEnemyPopulationDefinitions.BowlingAlleyDefault, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BowlingAlleyDefault, null, RoomSetupBehavior.None);


    /// <summary>The Bowling Alley room's state 1.</summary>
    public static RoomStateSettings BowlingAlleyWhenAreaBossDead { get; } =
        new(nameof(BowlingAlleyWhenAreaBossDead), RoomLevelDefinitions.BowlingAlley, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyWhenAreaBossDead, RoomEnemyPopulationDefinitions.BowlingAlleyWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BowlingAlleyWhenAreaBossDead, null, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Entrance room's state 0.</summary>
    public static RoomStateSettings WreckedShipEntranceDefault { get; } =
        new(nameof(WreckedShipEntranceDefault), RoomLevelDefinitions.WreckedShipEntrance, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceDefault, RoomEnemyPopulationDefinitions.WreckedShipEntranceDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEntranceDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipEntranceHall, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Entrance room's state 1.</summary>
    public static RoomStateSettings WreckedShipEntranceWhenAreaBossDead { get; } =
        new(nameof(WreckedShipEntranceWhenAreaBossDead), RoomLevelDefinitions.WreckedShipEntrance, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipEntranceWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEntranceWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipEntranceHall, RoomSetupBehavior.None);


    /// <summary>The Attic room's state 0.</summary>
    public static RoomStateSettings AtticDefault { get; } =
        new(nameof(AtticDefault), RoomLevelDefinitions.AtticDefault, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.AtticDefault, RoomEnemyPopulationDefinitions.AtticDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(254), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Attic, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AtticDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipExperimentRoom, RoomSetupBehavior.None);


    /// <summary>The Attic room's state 1.</summary>
    public static RoomStateSettings AtticWhenAreaBossDead { get; } =
        new(nameof(AtticWhenAreaBossDead), RoomLevelDefinitions.AtticWhenAreaBossDead, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.AtticWhenAreaBossDead, RoomEnemyPopulationDefinitions.AtticWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(254), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Attic, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AtticWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipExperimentRoom, RoomSetupBehavior.None);


    /// <summary>Wrecked Ship's Assembly Line room's state 0.</summary>
    public static RoomStateSettings AssemblyLineDefault { get; } =
        new(nameof(AssemblyLineDefault), RoomLevelDefinitions.AssemblyLine, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineDefault, RoomEnemyPopulationDefinitions.AssemblyLineDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AssemblyLineDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsVertical, RoomSetupBehavior.None);


    /// <summary>Wrecked Ship's Assembly Line room's state 1.</summary>
    public static RoomStateSettings AssemblyLineWhenAreaBossDead { get; } =
        new(nameof(AssemblyLineWhenAreaBossDead), RoomLevelDefinitions.AssemblyLine, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineWhenAreaBossDead, RoomEnemyPopulationDefinitions.AssemblyLineWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AssemblyLineWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsVertical, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Main Shaft room's state 0.</summary>
    public static RoomStateSettings WreckedShipMainShaftDefault { get; } =
        new(nameof(WreckedShipMainShaftDefault), RoomLevelDefinitions.WreckedShipMainShaftDefault, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftDefault, RoomEnemyPopulationDefinitions.WreckedShipMainShaftDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Main Shaft room's state 1.</summary>
    public static RoomStateSettings WreckedShipMainShaftWhenAreaBossDead { get; } =
        new(nameof(WreckedShipMainShaftWhenAreaBossDead), RoomLevelDefinitions.WreckedShipMainShaftWhenAreaBossDead, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipMainShaftWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The Spiky Death room's state 0.</summary>
    public static RoomStateSettings SpikyDeathDefault { get; } =
        new(nameof(SpikyDeathDefault), RoomLevelDefinitions.SpikyDeath, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathDefault, RoomEnemyPopulationDefinitions.SpikyDeathDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpikyDeathDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipColumnsTubes, RoomSetupBehavior.None);


    /// <summary>The Spiky Death room's state 1.</summary>
    public static RoomStateSettings SpikyDeathWhenAreaBossDead { get; } =
        new(nameof(SpikyDeathWhenAreaBossDead), RoomLevelDefinitions.SpikyDeath, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathWhenAreaBossDead, RoomEnemyPopulationDefinitions.SpikyDeathWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpikyDeathWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipColumnsTubes, RoomSetupBehavior.None);


    /// <summary>The Electric Death room's state 0.</summary>
    public static RoomStateSettings ElectricDeathDefault { get; } =
        new(nameof(ElectricDeathDefault), RoomLevelDefinitions.ElectricDeath, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathDefault, RoomEnemyPopulationDefinitions.ElectricDeathDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElectricDeathDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The Electric Death room's state 1.</summary>
    public static RoomStateSettings ElectricDeathWhenAreaBossDead { get; } =
        new(nameof(ElectricDeathWhenAreaBossDead), RoomLevelDefinitions.ElectricDeath, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathWhenAreaBossDead, RoomEnemyPopulationDefinitions.ElectricDeathWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElectricDeathWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship E Tank room's state 0.</summary>
    public static RoomStateSettings WreckedShipETankDefault { get; } =
        new(nameof(WreckedShipETankDefault), RoomLevelDefinitions.WreckedShipETank, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankDefault, RoomEnemyPopulationDefinitions.WreckedShipETankDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipETankDefault, null, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship E Tank room's state 1.</summary>
    public static RoomStateSettings WreckedShipETankWhenAreaBossDead { get; } =
        new(nameof(WreckedShipETankWhenAreaBossDead), RoomLevelDefinitions.WreckedShipETank, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipETankWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipETankWhenAreaBossDead, null, RoomSetupBehavior.None);


    /// <summary>The Basement room's state 0.</summary>
    public static RoomStateSettings BasementDefault { get; } =
        new(nameof(BasementDefault), RoomLevelDefinitions.Basement, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.BasementDefault, RoomEnemyPopulationDefinitions.BasementDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Basement, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BasementDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipColumnsTubes, RoomSetupBehavior.SpawnPrePhantoonRoomEnemyProjectile);


    /// <summary>The Basement room's state 1.</summary>
    public static RoomStateSettings BasementWhenAreaBossDead { get; } =
        new(nameof(BasementWhenAreaBossDead), RoomLevelDefinitions.Basement, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.BasementWhenAreaBossDead, RoomEnemyPopulationDefinitions.BasementWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Basement, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BasementWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipColumnsTubes, RoomSetupBehavior.SpawnPrePhantoonRoomEnemyProjectile);


    /// <summary>The Wrecked Ship Map room's state 0.</summary>
    public static RoomStateSettings WreckedShipMapDefault { get; } =
        new(nameof(WreckedShipMapDefault), RoomLevelDefinitions.GreenBrinstarMap, RoomGraphicsSet.WreckedShipMapPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapDefault, RoomEnemyPopulationDefinitions.WreckedShipMapDefault, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMapDefault, null, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Map room's state 1.</summary>
    public static RoomStateSettings WreckedShipMapWhenAreaBossDead { get; } =
        new(nameof(WreckedShipMapWhenAreaBossDead), RoomLevelDefinitions.GreenBrinstarMap, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipMapWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMapWhenAreaBossDead, null, RoomSetupBehavior.None);


    /// <summary>The Phantoon room's state 0.</summary>
    public static RoomStateSettings PhantoonDefault { get; } =
        new(nameof(PhantoonDefault), RoomLevelDefinitions.Phantoon, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.KraidAndPhantoonBattles, RoomMusicTrack.Plays(MusicTrack.BossRoomTension)), SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonDefault, RoomEnemyPopulationDefinitions.PhantoonDefault, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Phantoon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.PhantoonDefault, RoomSetupBehavior.None);


    /// <summary>The Phantoon room's state 1.</summary>
    public static RoomStateSettings PhantoonWhenAreaBossDead { get; } =
        new(nameof(PhantoonWhenAreaBossDead), RoomLevelDefinitions.Phantoon, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonWhenAreaBossDead, RoomEnemyPopulationDefinitions.PhantoonWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Phantoon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.DraygonOrPhantoonWhenAreaBossDead, RoomSetupBehavior.None);


    /// <summary>The Sponge Bath room's state 0.</summary>
    public static RoomStateSettings SpongeBathDefault { get; } =
        new(nameof(SpongeBathDefault), RoomLevelDefinitions.SpongeBath, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathDefault, RoomEnemyPopulationDefinitions.SpongeBathDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpongeBathDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Sponge Bath room's state 1.</summary>
    public static RoomStateSettings SpongeBathWhenAreaBossDead { get; } =
        new(nameof(SpongeBathWhenAreaBossDead), RoomLevelDefinitions.SpongeBath, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathWhenAreaBossDead, RoomEnemyPopulationDefinitions.SpongeBathWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpongeBathWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The WS West Super room's state 0.</summary>
    public static RoomStateSettings WSWestSuperDefault { get; } =
        new(nameof(WSWestSuperDefault), RoomLevelDefinitions.WSWestSuper, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperDefault, RoomEnemyPopulationDefinitions.WSWestSuperDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WSWestSuperDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The WS West Super room's state 1.</summary>
    public static RoomStateSettings WSWestSuperWhenAreaBossDead { get; } =
        new(nameof(WSWestSuperWhenAreaBossDead), RoomLevelDefinitions.WSWestSuper, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperWhenAreaBossDead, RoomEnemyPopulationDefinitions.WSWestSuperWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WSWestSuperWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The WS East Super room's state 0.</summary>
    public static RoomStateSettings WSEastSuperDefault { get; } =
        new(nameof(WSEastSuperDefault), RoomLevelDefinitions.WSEastSuperDefault, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWSEastSuperOrWreckedShipSaveDefault, RoomEnemyPopulationDefinitions.WSEastSuperDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WSEastSuperDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The WS East Super room's state 1.</summary>
    public static RoomStateSettings WSEastSuperWhenAreaBossDead { get; } =
        new(nameof(WSEastSuperWhenAreaBossDead), RoomLevelDefinitions.WSEastSuperWhenAreaBossDead, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WSEastSuperWhenAreaBossDead, RoomEnemyPopulationDefinitions.WSEastSuperWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WSEastSuperWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Gravity Suit room's state 0.</summary>
    public static RoomStateSettings GravitySuitDefault { get; } =
        new(nameof(GravitySuitDefault), RoomLevelDefinitions.GravitySuit, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWSEastSuperOrWreckedShipSaveDefault, RoomEnemyPopulationDefinitions.GravitySuitDefault, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GravitySuitOrWreckedShipSaveDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Gravity Suit room's state 1.</summary>
    public static RoomStateSettings GravitySuitWhenAreaBossDead { get; } =
        new(nameof(GravitySuitWhenAreaBossDead), RoomLevelDefinitions.GravitySuit, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWreckedShipSaveWhenAreaBossDead, RoomEnemyPopulationDefinitions.GravitySuitWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GravitySuitWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Save room's state 0.</summary>
    public static RoomStateSettings WreckedShipSaveDefault { get; } =
        new(nameof(WreckedShipSaveDefault), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.WreckedShipMapPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWSEastSuperOrWreckedShipSaveDefault, RoomEnemyPopulationDefinitions.WreckedShipSaveDefault, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GravitySuitOrWreckedShipSaveDefault, null, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Save room's state 1.</summary>
    public static RoomStateSettings WreckedShipSaveWhenAreaBossDead { get; } =
        new(nameof(WreckedShipSaveWhenAreaBossDead), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.MapStatues, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWreckedShipSaveWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipSaveWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipSaveWhenAreaBossDead, null, RoomSetupBehavior.None);
}
