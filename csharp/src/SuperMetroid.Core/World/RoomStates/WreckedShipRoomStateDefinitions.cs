using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Wrecked Ship room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class WreckedShipRoomStateDefinitions
{

    /// <summary>The Bowling Alley room's state 0.</summary>
    public static RoomStateSettings BowlingAlleyDefault { get; } =
        new(nameof(BowlingAlleyDefault), RoomLevelDefinitions.BowlingAlley, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyDefault, RoomEnemyPopulationDefinitions.BowlingAlleyDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BowlingAlleyDefault, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Bowling Alley room's state 1.</summary>
    public static RoomStateSettings BowlingAlleyWhenAreaBossDead { get; } =
        new(nameof(BowlingAlleyWhenAreaBossDead), RoomLevelDefinitions.BowlingAlley, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BowlingAlleyWhenAreaBossDead, RoomEnemyPopulationDefinitions.BowlingAlleyWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BowlingAlley, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BowlingAlleyWhenAreaBossDead, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Wrecked Ship Entrance room's state 0.</summary>
    public static RoomStateSettings WreckedShipEntranceDefault { get; } =
        new(nameof(WreckedShipEntranceDefault), RoomLevelDefinitions.WreckedShipEntrance, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceDefault, RoomEnemyPopulationDefinitions.WreckedShipEntranceDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEntranceDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45EntranceHall, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Wrecked Ship Entrance room's state 1.</summary>
    public static RoomStateSettings WreckedShipEntranceWhenAreaBossDead { get; } =
        new(nameof(WreckedShipEntranceWhenAreaBossDead), RoomLevelDefinitions.WreckedShipEntrance, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipEntranceWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipEntranceWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipEntranceWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45EntranceHall, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Attic room's state 0.</summary>
    public static RoomStateSettings AtticDefault { get; } =
        new(nameof(AtticDefault), RoomLevelDefinitions.AtticDefault, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.AtticDefault, RoomEnemyPopulationDefinitions.AtticDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 255), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Attic, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AtticDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45ExperimentRoom, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Attic room's state 1.</summary>
    public static RoomStateSettings AtticWhenAreaBossDead { get; } =
        new(nameof(AtticWhenAreaBossDead), RoomLevelDefinitions.AtticWhenAreaBossDead, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.AtticWhenAreaBossDead, RoomEnemyPopulationDefinitions.AtticWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 255), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Attic, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AtticWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45ExperimentRoom, RoomSetupCallback.OrdinaryReturn);


    /// <summary>Wrecked Ship's Assembly Line room's state 0.</summary>
    public static RoomStateSettings AssemblyLineDefault { get; } =
        new(nameof(AssemblyLineDefault), RoomLevelDefinitions.AssemblyLine, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineDefault, RoomEnemyPopulationDefinitions.AssemblyLineDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AssemblyLineDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVentsVertical, RoomSetupCallback.OrdinaryReturn);


    /// <summary>Wrecked Ship's Assembly Line room's state 1.</summary>
    public static RoomStateSettings AssemblyLineWhenAreaBossDead { get; } =
        new(nameof(AssemblyLineWhenAreaBossDead), RoomLevelDefinitions.AssemblyLine, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AssemblyLineWhenAreaBossDead, RoomEnemyPopulationDefinitions.AssemblyLineWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AssemblyLineWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVentsVertical, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Wrecked Ship Main Shaft room's state 0.</summary>
    public static RoomStateSettings WreckedShipMainShaftDefault { get; } =
        new(nameof(WreckedShipMainShaftDefault), RoomLevelDefinitions.WreckedShipMainShaftDefault, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftDefault, RoomEnemyPopulationDefinitions.WreckedShipMainShaftDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Wrecked Ship Main Shaft room's state 1.</summary>
    public static RoomStateSettings WreckedShipMainShaftWhenAreaBossDead { get; } =
        new(nameof(WreckedShipMainShaftWhenAreaBossDead), RoomLevelDefinitions.WreckedShipMainShaftWhenAreaBossDead, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMainShaftWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipMainShaftWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Spiky Death room's state 0.</summary>
    public static RoomStateSettings SpikyDeathDefault { get; } =
        new(nameof(SpikyDeathDefault), RoomLevelDefinitions.SpikyDeath, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathDefault, RoomEnemyPopulationDefinitions.SpikyDeathDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpikyDeathDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Spiky Death room's state 1.</summary>
    public static RoomStateSettings SpikyDeathWhenAreaBossDead { get; } =
        new(nameof(SpikyDeathWhenAreaBossDead), RoomLevelDefinitions.SpikyDeath, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyDeathWhenAreaBossDead, RoomEnemyPopulationDefinitions.SpikyDeathWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpikyDeathWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Electric Death room's state 0.</summary>
    public static RoomStateSettings ElectricDeathDefault { get; } =
        new(nameof(ElectricDeathDefault), RoomLevelDefinitions.ElectricDeath, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathDefault, RoomEnemyPopulationDefinitions.ElectricDeathDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElectricDeathDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Electric Death room's state 1.</summary>
    public static RoomStateSettings ElectricDeathWhenAreaBossDead { get; } =
        new(nameof(ElectricDeathWhenAreaBossDead), RoomLevelDefinitions.ElectricDeath, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.ElectricDeathWhenAreaBossDead, RoomEnemyPopulationDefinitions.ElectricDeathWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ElectricDeath, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ElectricDeathWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Wrecked Ship E Tank room's state 0.</summary>
    public static RoomStateSettings WreckedShipETankDefault { get; } =
        new(nameof(WreckedShipETankDefault), RoomLevelDefinitions.WreckedShipETank, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankDefault, RoomEnemyPopulationDefinitions.WreckedShipETankDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipETankDefault, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Wrecked Ship E Tank room's state 1.</summary>
    public static RoomStateSettings WreckedShipETankWhenAreaBossDead { get; } =
        new(nameof(WreckedShipETankWhenAreaBossDead), RoomLevelDefinitions.WreckedShipETank, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipETankWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipETankWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipETankWhenAreaBossDead, null, RoomSetupCallback.OrdinaryReturn);


    /// <summary>The Basement room's state 0.</summary>
    public static RoomStateSettings BasementDefault { get; } =
        new(nameof(BasementDefault), RoomLevelDefinitions.Basement, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BasementDefault, RoomEnemyPopulationDefinitions.BasementDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Basement, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BasementDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile);


    /// <summary>The Basement room's state 1.</summary>
    public static RoomStateSettings BasementWhenAreaBossDead { get; } =
        new(nameof(BasementWhenAreaBossDead), RoomLevelDefinitions.Basement, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BasementWhenAreaBossDead, RoomEnemyPopulationDefinitions.BasementWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Basement, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BasementWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45ColumnsTubes, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile);


    /// <summary>The Wrecked Ship Map room's state 0.</summary>
    public static RoomStateSettings WreckedShipMapDefault { get; } =
        new(nameof(WreckedShipMapDefault), RoomLevelDefinitions.GreenBrinstarMap, RoomGraphicsSet.WreckedShipMapPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapDefault, RoomEnemyPopulationDefinitions.WreckedShipMapDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMapDefault, null, RoomSetupCallback.BossRoomReturn);


    /// <summary>The Wrecked Ship Map room's state 1.</summary>
    public static RoomStateSettings WreckedShipMapWhenAreaBossDead { get; } =
        new(nameof(WreckedShipMapWhenAreaBossDead), RoomLevelDefinitions.GreenBrinstarMap, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WreckedShipMapWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipMapWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipMapWhenAreaBossDead, null, RoomSetupCallback.BossRoomReturn);


    /// <summary>The Phantoon room's state 0.</summary>
    public static RoomStateSettings PhantoonDefault { get; } =
        new(nameof(PhantoonDefault), RoomLevelDefinitions.Phantoon, RoomGraphicsSet.WreckedShipPowerOff, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonDefault, RoomEnemyPopulationDefinitions.PhantoonDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Phantoon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.PhantoonDefault, RoomSetupCallback.BossRoomReturn);


    /// <summary>The Phantoon room's state 1.</summary>
    public static RoomStateSettings PhantoonWhenAreaBossDead { get; } =
        new(nameof(PhantoonWhenAreaBossDead), RoomLevelDefinitions.Phantoon, RoomGraphicsSet.WreckedShipPowerOn, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.PhantoonWhenAreaBossDead, RoomEnemyPopulationDefinitions.PhantoonWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Phantoon, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Phantoon, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.DraygonOrPhantoonWhenAreaBossDead, RoomSetupCallback.BossRoomReturn);


    /// <summary>The Sponge Bath room's state 0.</summary>
    public static RoomStateSettings SpongeBathDefault { get; } =
        new(nameof(SpongeBathDefault), RoomLevelDefinitions.SpongeBath, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathDefault, RoomEnemyPopulationDefinitions.SpongeBathDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpongeBathDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The Sponge Bath room's state 1.</summary>
    public static RoomStateSettings SpongeBathWhenAreaBossDead { get; } =
        new(nameof(SpongeBathWhenAreaBossDead), RoomLevelDefinitions.SpongeBath, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpongeBathWhenAreaBossDead, RoomEnemyPopulationDefinitions.SpongeBathWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SpongeBath, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpongeBathWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The WS West Super room's state 0.</summary>
    public static RoomStateSettings WSWestSuperDefault { get; } =
        new(nameof(WSWestSuperDefault), RoomLevelDefinitions.WSWestSuper, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperDefault, RoomEnemyPopulationDefinitions.WSWestSuperDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WSWestSuperDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The WS West Super room's state 1.</summary>
    public static RoomStateSettings WSWestSuperWhenAreaBossDead { get; } =
        new(nameof(WSWestSuperWhenAreaBossDead), RoomLevelDefinitions.WSWestSuper, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WSWestSuperWhenAreaBossDead, RoomEnemyPopulationDefinitions.WSWestSuperWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSWestSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WSWestSuperWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The WS East Super room's state 0.</summary>
    public static RoomStateSettings WSEastSuperDefault { get; } =
        new(nameof(WSEastSuperDefault), RoomLevelDefinitions.WSEastSuperDefault, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWSEastSuperOrWreckedShipSaveDefault, RoomEnemyPopulationDefinitions.WSEastSuperDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WSEastSuperDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVentsHorizontal, RoomSetupCallback.BossRoomReturn);


    /// <summary>The WS East Super room's state 1.</summary>
    public static RoomStateSettings WSEastSuperWhenAreaBossDead { get; } =
        new(nameof(WSEastSuperWhenAreaBossDead), RoomLevelDefinitions.WSEastSuperWhenAreaBossDead, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WSEastSuperWhenAreaBossDead, RoomEnemyPopulationDefinitions.WSEastSuperWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WSEastSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WSEastSuperWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The Gravity Suit room's state 0.</summary>
    public static RoomStateSettings GravitySuitDefault { get; } =
        new(nameof(GravitySuitDefault), RoomLevelDefinitions.GravitySuit, RoomGraphicsSet.WreckedShipPowerOff, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWSEastSuperOrWreckedShipSaveDefault, RoomEnemyPopulationDefinitions.GravitySuitDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GravitySuitOrWreckedShipSaveDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The Gravity Suit room's state 1.</summary>
    public static RoomStateSettings GravitySuitWhenAreaBossDead { get; } =
        new(nameof(GravitySuitWhenAreaBossDead), RoomLevelDefinitions.GravitySuit, RoomGraphicsSet.WreckedShipPowerOn, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWreckedShipSaveWhenAreaBossDead, RoomEnemyPopulationDefinitions.GravitySuitWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GravitySuitWhenAreaBossDead, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.WreckedShip45PanelsVents, RoomSetupCallback.BossRoomReturn);


    /// <summary>The Wrecked Ship Save room's state 0.</summary>
    public static RoomStateSettings WreckedShipSaveDefault { get; } =
        new(nameof(WreckedShipSaveDefault), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.WreckedShipMapPowerOff, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWSEastSuperOrWreckedShipSaveDefault, RoomEnemyPopulationDefinitions.WreckedShipSaveDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GravitySuitOrWreckedShipSaveDefault, null, RoomSetupCallback.None);


    /// <summary>The Wrecked Ship Save room's state 1.</summary>
    public static RoomStateSettings WreckedShipSaveWhenAreaBossDead { get; } =
        new(nameof(WreckedShipSaveWhenAreaBossDead), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.MapStatues, new RoomMusic(MusicDataSet.WreckedShip, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GravitySuitOrWreckedShipSaveWhenAreaBossDead, RoomEnemyPopulationDefinitions.WreckedShipSaveWhenAreaBossDead, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WreckedShipSaveWhenAreaBossDead, null, RoomSetupCallback.BossRoomReturn);
}
