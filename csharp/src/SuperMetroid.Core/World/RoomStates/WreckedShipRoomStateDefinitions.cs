using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Wrecked Ship room state, named for their rooms.</summary>
public static partial class WreckedShipRoomStateDefinitions
{

    /// <summary>The Bowling Alley room's default state.</summary>
    public static RoomStateSettings BowlingAlleyDefault { get; } =
        new("BowlingAlleyDefault", RoomLevelDefinitions.BowlingAlley, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyDefault, RoomEnemyPopulationDefinitions.BowlingAlleyDefault, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BowlingAlleyDefault, null, RoomSetupBehavior.None);


    /// <summary>The Bowling Alley room's state once the area's boss is dead.</summary>
    public static RoomStateSettings BowlingAlleyWhenAreaBossDead { get; } =
        new("BowlingAlleyWhenAreaBossDead", RoomLevelDefinitions.BowlingAlley, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyWhenAreaBossDead, RoomEnemyPopulationDefinitions.BowlingAlleyWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BowlingAlleyWhenAreaBossDead, null, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Entrance room's default state.</summary>
    public static RoomStateSettings WreckedShipEntranceDefault { get; } =
        new("WreckedShipEntranceDefault", RoomLevelDefinitions.WreckedShipEntrance, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceDefault, RoomEnemyPopulationDefinitions.WreckedShipEntranceDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEntranceDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipEntranceHall, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Entrance room's state once the area's boss is dead.</summary>
    public static RoomStateSettings WreckedShipEntranceWhenAreaBossDead { get; } =
        new("WreckedShipEntranceWhenAreaBossDead", RoomLevelDefinitions.WreckedShipEntrance, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipEntranceWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEntranceWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipEntranceHall, RoomSetupBehavior.None);


    /// <summary>The Attic room's default state.</summary>
    public static RoomStateSettings AtticDefault { get; } =
        new("AtticDefault", RoomLevelDefinitions.AtticDefault, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.AtticDefault, RoomEnemyPopulationDefinitions.AtticDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(127, 128)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Attic, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AtticDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipExperimentRoom, RoomSetupBehavior.None);


    /// <summary>The Attic room's state once the area's boss is dead.</summary>
    public static RoomStateSettings AtticWhenAreaBossDead { get; } =
        new("AtticWhenAreaBossDead", RoomLevelDefinitions.AtticWhenAreaBossDead, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.AtticWhenAreaBossDead, RoomEnemyPopulationDefinitions.AtticWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(127, 128)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Attic, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AtticWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipExperimentRoom, RoomSetupBehavior.None);


    /// <summary>The Assembly Line room's default state.</summary>
    public static RoomStateSettings AssemblyLineDefault { get; } =
        new("AssemblyLineDefault", RoomLevelDefinitions.AssemblyLine, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineDefault, RoomEnemyPopulationDefinitions.AssemblyLineDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AssemblyLineDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsVertical, RoomSetupBehavior.None);


    /// <summary>The Assembly Line room's state once the area's boss is dead.</summary>
    public static RoomStateSettings AssemblyLineWhenAreaBossDead { get; } =
        new("AssemblyLineWhenAreaBossDead", RoomLevelDefinitions.AssemblyLine, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineWhenAreaBossDead, RoomEnemyPopulationDefinitions.AssemblyLineWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AssemblyLineWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsVertical, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Main Shaft room's default state.</summary>
    public static RoomStateSettings WreckedShipMainShaftDefault { get; } =
        new("WreckedShipMainShaftDefault", RoomLevelDefinitions.WreckedShipMainShaftDefault, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftDefault, RoomEnemyPopulationDefinitions.WreckedShipMainShaftDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Main Shaft room's state once the area's boss is dead.</summary>
    public static RoomStateSettings WreckedShipMainShaftWhenAreaBossDead { get; } =
        new("WreckedShipMainShaftWhenAreaBossDead", RoomLevelDefinitions.WreckedShipMainShaftWhenAreaBossDead, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipMainShaftWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The Spiky Death room's default state.</summary>
    public static RoomStateSettings SpikyDeathDefault { get; } =
        new("SpikyDeathDefault", RoomLevelDefinitions.SpikyDeath, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathDefault, RoomEnemyPopulationDefinitions.SpikyDeathDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpikyDeathDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipColumnsTubes, RoomSetupBehavior.None);


    /// <summary>The Spiky Death room's state once the area's boss is dead.</summary>
    public static RoomStateSettings SpikyDeathWhenAreaBossDead { get; } =
        new("SpikyDeathWhenAreaBossDead", RoomLevelDefinitions.SpikyDeath, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathWhenAreaBossDead, RoomEnemyPopulationDefinitions.SpikyDeathWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpikyDeathWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipColumnsTubes, RoomSetupBehavior.None);


    /// <summary>The Electric Death room's default state.</summary>
    public static RoomStateSettings ElectricDeathDefault { get; } =
        new("ElectricDeathDefault", RoomLevelDefinitions.ElectricDeath, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathDefault, RoomEnemyPopulationDefinitions.ElectricDeathDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElectricDeathDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The Electric Death room's state once the area's boss is dead.</summary>
    public static RoomStateSettings ElectricDeathWhenAreaBossDead { get; } =
        new("ElectricDeathWhenAreaBossDead", RoomLevelDefinitions.ElectricDeath, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathWhenAreaBossDead, RoomEnemyPopulationDefinitions.ElectricDeathWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElectricDeathWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Energy Tank room's default state.</summary>
    public static RoomStateSettings WreckedShipEnergyTankDefault { get; } =
        new("WreckedShipEnergyTankDefault", RoomLevelDefinitions.WreckedShipEnergyTank, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEnergyTankDefault, RoomEnemyPopulationDefinitions.WreckedShipEnergyTankDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEnergyTankDefault, null, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Energy Tank room's state once the area's boss is dead.</summary>
    public static RoomStateSettings WreckedShipEnergyTankWhenAreaBossDead { get; } =
        new("WreckedShipEnergyTankWhenAreaBossDead", RoomLevelDefinitions.WreckedShipEnergyTank, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEnergyTankWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipEnergyTankWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEnergyTankWhenAreaBossDead, null, RoomSetupBehavior.None);


    /// <summary>The Basement room's default state.</summary>
    public static RoomStateSettings BasementDefault { get; } =
        new("BasementDefault", RoomLevelDefinitions.Basement, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.BasementDefault, RoomEnemyPopulationDefinitions.BasementDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Basement, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BasementDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipColumnsTubes, RoomSetupBehavior.HoldPrePhantoonProjectilePlace);


    /// <summary>The Basement room's state once the area's boss is dead.</summary>
    public static RoomStateSettings BasementWhenAreaBossDead { get; } =
        new("BasementWhenAreaBossDead", RoomLevelDefinitions.Basement, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.BasementWhenAreaBossDead, RoomEnemyPopulationDefinitions.BasementWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Basement, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BasementWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipColumnsTubes, RoomSetupBehavior.HoldPrePhantoonProjectilePlace);


    /// <summary>The Wrecked Ship Map room's default state.</summary>
    public static RoomStateSettings WreckedShipMapDefault { get; } =
        new("WreckedShipMapDefault", RoomLevelDefinitions.MapStationRoomDoorOnRight, RoomGraphicsSet.WreckedShipMapPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapDefault, RoomEnemyPopulationDefinitions.WreckedShipMapDefault, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMapDefault, null, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Map room's state once the area's boss is dead.</summary>
    public static RoomStateSettings WreckedShipMapWhenAreaBossDead { get; } =
        new("WreckedShipMapWhenAreaBossDead", RoomLevelDefinitions.MapStationRoomDoorOnRight, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipMapWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMapWhenAreaBossDead, null, RoomSetupBehavior.None);


    /// <summary>The Phantoon room's default state.</summary>
    public static RoomStateSettings PhantoonDefault { get; } =
        new("PhantoonDefault", RoomLevelDefinitions.Phantoon, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.KraidAndPhantoonBattles, RoomMusicTrack.Plays(MusicTrack.BossRoomTension)), SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonDefault, RoomEnemyPopulationDefinitions.PhantoonDefault, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Phantoon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BossBody, RoomSetupBehavior.None);


    /// <summary>The Phantoon room's state once the area's boss is dead.</summary>
    public static RoomStateSettings PhantoonWhenAreaBossDead { get; } =
        new("PhantoonWhenAreaBossDead", RoomLevelDefinitions.Phantoon, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonWhenAreaBossDead, RoomEnemyPopulationDefinitions.PhantoonWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Phantoon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Cleared, RoomSetupBehavior.None);


    /// <summary>The Sponge Bath room's default state.</summary>
    public static RoomStateSettings SpongeBathDefault { get; } =
        new("SpongeBathDefault", RoomLevelDefinitions.SpongeBath, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathDefault, RoomEnemyPopulationDefinitions.SpongeBathDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpongeBathDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Sponge Bath room's state once the area's boss is dead.</summary>
    public static RoomStateSettings SpongeBathWhenAreaBossDead { get; } =
        new("SpongeBathWhenAreaBossDead", RoomLevelDefinitions.SpongeBath, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathWhenAreaBossDead, RoomEnemyPopulationDefinitions.SpongeBathWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpongeBathWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship West Super room's default state.</summary>
    public static RoomStateSettings WreckedShipWestSuperDefault { get; } =
        new("WreckedShipWestSuperDefault", RoomLevelDefinitions.WreckedShipWestSuper, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipWestSuperDefault, RoomEnemyPopulationDefinitions.WreckedShipWestSuperDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipWestSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipWestSuperDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship West Super room's state once the area's boss is dead.</summary>
    public static RoomStateSettings WreckedShipWestSuperWhenAreaBossDead { get; } =
        new("WreckedShipWestSuperWhenAreaBossDead", RoomLevelDefinitions.WreckedShipWestSuper, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipWestSuperWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipWestSuperWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipWestSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipWestSuperWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship East Super room's default state.</summary>
    public static RoomStateSettings WreckedShipEastSuperDefault { get; } =
        new("WreckedShipEastSuperDefault", RoomLevelDefinitions.WreckedShipEastSuperDefault, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipUntinted, RoomEnemyPopulationDefinitions.WreckedShipEastSuperDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipEastSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEastSuperDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVentsHorizontal, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship East Super room's state once the area's boss is dead.</summary>
    public static RoomStateSettings WreckedShipEastSuperWhenAreaBossDead { get; } =
        new("WreckedShipEastSuperWhenAreaBossDead", RoomLevelDefinitions.WreckedShipEastSuperWhenAreaBossDead, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEastSuperWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipEastSuperWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipEastSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEastSuperWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Gravity Suit room's default state.</summary>
    public static RoomStateSettings GravitySuitDefault { get; } =
        new("GravitySuitDefault", RoomLevelDefinitions.GravitySuit, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipUntinted, RoomEnemyPopulationDefinitions.GravitySuitDefault, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GravitySuitAndWreckedShipSavePoweredDown, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Gravity Suit room's state once the area's boss is dead.</summary>
    public static RoomStateSettings GravitySuitWhenAreaBossDead { get; } =
        new("GravitySuitWhenAreaBossDead", RoomLevelDefinitions.GravitySuit, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipGreenLights, RoomEnemyPopulationDefinitions.GravitySuitWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GravitySuitWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShipPanelsVents, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Save room's default state.</summary>
    public static RoomStateSettings WreckedShipSaveDefault { get; } =
        new("WreckedShipSaveDefault", RoomLevelDefinitions.SaveRoomDoorOnLeft, RoomGraphicsSet.WreckedShipMapPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredDown)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipUntinted, RoomEnemyPopulationDefinitions.WreckedShipSaveDefault, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GravitySuitAndWreckedShipSavePoweredDown, null, RoomSetupBehavior.None);


    /// <summary>The Wrecked Ship Save room's state once the area's boss is dead.</summary>
    public static RoomStateSettings WreckedShipSaveWhenAreaBossDead { get; } =
        new("WreckedShipSaveWhenAreaBossDead", RoomLevelDefinitions.SaveRoomDoorOnLeft, RoomGraphicsSet.MapStatues, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.WreckedShipPoweredUp)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipGreenLights, RoomEnemyPopulationDefinitions.WreckedShipSaveWhenAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipSaveWhenAreaBossDead, null, RoomSetupBehavior.None);
}
