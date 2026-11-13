using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Brinstar room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class BrinstarRoomStateDefinitions
{

    /// <summary>The Green Brinstar Main Shaft room's state.</summary>
    public static RoomStateSettings GreenBrinstarMainShaft { get; } =
        new(nameof(GreenBrinstarMainShaft), RoomLevelDefinitions.GreenBrinstarMainShaft, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMainShaft, RoomEnemyPopulationDefinitions.GreenBrinstarMainShaft, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarMainShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6VerticalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn Super room's state.</summary>
    public static RoomStateSettings SporeSpawnSuper { get; } =
        new(nameof(SporeSpawnSuper), RoomLevelDefinitions.SporeSpawnSuper, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnSuper, RoomEnemyPopulationDefinitions.SporeSpawnSuper, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SporeSpawnSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawnSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Brinstar Pre Map room's state.</summary>
    public static RoomStateSettings BrinstarPreMap { get; } =
        new(nameof(BrinstarPreMap), RoomLevelDefinitions.BrinstarPreMap, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarPreMap, RoomEnemyPopulationDefinitions.BrinstarPreMap, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BrinstarPreMap, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6MediumHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Early Supers room's state.</summary>
    public static RoomStateSettings EarlySupers { get; } =
        new(nameof(EarlySupers), RoomLevelDefinitions.EarlySupers, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.EarlySupers, RoomEnemyPopulationDefinitions.EarlySupers, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EarlySupers, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EarlySupers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Brinstar Reserve Tank room's state.</summary>
    public static RoomStateSettings BrinstarReserveTank { get; } =
        new(nameof(BrinstarReserveTank), RoomLevelDefinitions.BrinstarReserveTank, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarReserveTank, RoomEnemyPopulationDefinitions.BrinstarReserveTank, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BrinstarReserveTank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BrinstarReserveTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6LargeHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Map room's state.</summary>
    public static RoomStateSettings GreenBrinstarMap { get; } =
        new(nameof(GreenBrinstarMap), RoomLevelDefinitions.GreenBrinstarMap, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMap, RoomEnemyPopulationDefinitions.GreenBrinstarMap, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarMap, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Firefleas room's state.</summary>
    public static RoomStateSettings GreenBrinstarFirefleas { get; } =
        new(nameof(GreenBrinstarFirefleas), RoomLevelDefinitions.GreenBrinstarFirefleas, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarFirefleas, RoomEnemyPopulationDefinitions.GreenBrinstarFirefleas, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Missile Refill room's state.</summary>
    public static RoomStateSettings GreenBrinstarMissileRefill { get; } =
        new(nameof(GreenBrinstarMissileRefill), RoomLevelDefinitions.GreenBrinstarMissileRefill, RoomGraphicsSet.YellowRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMissileRefill, RoomEnemyPopulationDefinitions.GreenBrinstarMissileRefill, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarMissileRefill, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarMissileRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Dachora room's state.</summary>
    public static RoomStateSettings Dachora { get; } =
        new(nameof(Dachora), RoomLevelDefinitions.Dachora, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Dachora, RoomEnemyPopulationDefinitions.Dachora, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Dachora, null, RoomMainCallback.ScrollScreenRightInDachoraRoom, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Dachora, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Big Pink room's state.</summary>
    public static RoomStateSettings BigPink { get; } =
        new(nameof(BigPink), RoomLevelDefinitions.BigPink, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BigPink, RoomEnemyPopulationDefinitions.BigPink, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigPink, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigPink, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6DarkPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn Kihunters room's state.</summary>
    public static RoomStateSettings SporeSpawnKihunters { get; } =
        new(nameof(SporeSpawnKihunters), RoomLevelDefinitions.SporeSpawnKihunters, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnKihunters, RoomEnemyPopulationDefinitions.SporeSpawnKihunters, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawnKihunters, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn room's state 0.</summary>
    public static RoomStateSettings SporeSpawnDefault { get; } =
        new(nameof(SporeSpawnDefault), RoomLevelDefinitions.SporeSpawn, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.MiniBossFight, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawn, RoomEnemyPopulationDefinitions.SporeSpawn, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawn, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn room's state 1.</summary>
    public static RoomStateSettings SporeSpawnWhenAreaMiniBossDead { get; } =
        new(nameof(SporeSpawnWhenAreaMiniBossDead), RoomLevelDefinitions.SporeSpawn, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawn, RoomEnemyPopulationDefinitions.SporeSpawn, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawn, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Pink Brinstar Power Bombs room's state.</summary>
    public static RoomStateSettings PinkBrinstarPowerBombs { get; } =
        new(nameof(PinkBrinstarPowerBombs), RoomLevelDefinitions.PinkBrinstarPowerBombs, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarPowerBombs, RoomEnemyPopulationDefinitions.PinkBrinstarPowerBombs, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PinkBrinstarPowerBombs, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PinkBrinstarPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Hill Zone room's state.</summary>
    public static RoomStateSettings GreenHillZone { get; } =
        new(nameof(GreenHillZone), RoomLevelDefinitions.GreenHillZone, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenHillZone, RoomEnemyPopulationDefinitions.GreenHillZone, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenHillZone, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenHillZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SpheresPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Morph Ball room's state 0.</summary>
    public static RoomStateSettings MorphBallDefault { get; } =
        new(nameof(MorphBallDefault), RoomLevelDefinitions.MorphBall, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.ThirdSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.MorphBallDefault, RoomEnemyPopulationDefinitions.MorphBallDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MorphBallDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Morph Ball room's state 1.</summary>
    public static RoomStateSettings MorphBallWhenZebesAwake { get; } =
        new(nameof(MorphBallWhenZebesAwake), RoomLevelDefinitions.MorphBall, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedPlain, RoomEnemyPopulationDefinitions.MorphBallWhenZebesAwake, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MorphBallWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Construction Zone room's state 0.</summary>
    public static RoomStateSettings ConstructionZoneDefault { get; } =
        new(nameof(ConstructionZoneDefault), RoomLevelDefinitions.ConstructionZone, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ConstructionZoneDefault, RoomEnemyPopulationDefinitions.ConstructionZoneDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ConstructionZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Construction Zone room's state 1.</summary>
    public static RoomStateSettings ConstructionZoneWhenZebesAwake { get; } =
        new(nameof(ConstructionZoneWhenZebesAwake), RoomLevelDefinitions.ConstructionZone, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedPlain, RoomEnemyPopulationDefinitions.ConstructionZoneWhenZebesAwake, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ConstructionZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Blue Brinstar E Tank room's state 0.</summary>
    public static RoomStateSettings BlueBrinstarETankDefault { get; } =
        new(nameof(BlueBrinstarETankDefault), RoomLevelDefinitions.BlueBrinstarETank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarETankDefault, RoomEnemyPopulationDefinitions.BlueBrinstarETankDefault, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueBrinstarETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Blue Brinstar E Tank room's state 1.</summary>
    public static RoomStateSettings BlueBrinstarETankWhenZebesAwake { get; } =
        new(nameof(BlueBrinstarETankWhenZebesAwake), RoomLevelDefinitions.BlueBrinstarETank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedPlain, RoomEnemyPopulationDefinitions.BlueBrinstarETankWhenZebesAwake, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueBrinstarETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Noob Bridge room's state.</summary>
    public static RoomStateSettings NoobBridge { get; } =
        new(nameof(NoobBridge), RoomLevelDefinitions.NoobBridge, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.NoobBridge, RoomEnemyPopulationDefinitions.NoobBridge, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.NoobBridge, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Beetoms room's state.</summary>
    public static RoomStateSettings GreenBrinstarBeetoms { get; } =
        new(nameof(GreenBrinstarBeetoms), RoomLevelDefinitions.GreenBrinstarBeetoms, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarBeetoms, RoomEnemyPopulationDefinitions.GreenBrinstarBeetoms, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarBeetoms, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarBeetoms, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Etecoon E Tank room's state.</summary>
    public static RoomStateSettings EtecoonETank { get; } =
        new(nameof(EtecoonETank), RoomLevelDefinitions.EtecoonETank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonETank, RoomEnemyPopulationDefinitions.EtecoonETank, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EtecoonETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EtecoonETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Etecoon Super room's state.</summary>
    public static RoomStateSettings EtecoonSuper { get; } =
        new(nameof(EtecoonSuper), RoomLevelDefinitions.EtecoonSuper, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSuper, RoomEnemyPopulationDefinitions.EtecoonSuper, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EtecoonSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EtecoonSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Dachora Energy Refill room's state.</summary>
    public static RoomStateSettings DachoraEnergyRefill { get; } =
        new(nameof(DachoraEnergyRefill), RoomLevelDefinitions.DachoraEnergyRefill, RoomGraphicsSet.BlueRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DachoraEnergyRefill, RoomEnemyPopulationDefinitions.DachoraEnergyRefill, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DachoraEnergyRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn Farming room's state.</summary>
    public static RoomStateSettings SporeSpawnFarming { get; } =
        new(nameof(SporeSpawnFarming), RoomLevelDefinitions.SporeSpawnFarming, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnFarming, RoomEnemyPopulationDefinitions.SporeSpawnFarming, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SporeSpawnFarming, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawnFarming, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Waterway E Tank room's state.</summary>
    public static RoomStateSettings WaterwayETank { get; } =
        new(nameof(WaterwayETank), RoomLevelDefinitions.WaterwayETank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WaterwayETank, RoomEnemyPopulationDefinitions.WaterwayETank, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WaterwayETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WaterwayETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The First Missile room's state.</summary>
    public static RoomStateSettings FirstMissile { get; } =
        new(nameof(FirstMissile), RoomLevelDefinitions.FirstMissile, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FirstMissile, RoomEnemyPopulationDefinitions.FirstMissile, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FirstMissile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Pink Brinstar Hoppers room's state.</summary>
    public static RoomStateSettings PinkBrinstarHoppers { get; } =
        new(nameof(PinkBrinstarHoppers), RoomLevelDefinitions.PinkBrinstarHoppers, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarHoppers, RoomEnemyPopulationDefinitions.PinkBrinstarHoppers, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PinkBrinstarHoppers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Hopper E Tank room's state.</summary>
    public static RoomStateSettings HopperETank { get; } =
        new(nameof(HopperETank), RoomLevelDefinitions.HopperETank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.HopperETank, RoomEnemyPopulationDefinitions.HopperETank, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.HopperETank, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Big Pink Save Room room's state.</summary>
    public static RoomStateSettings BigPinkSaveRoom { get; } =
        new(nameof(BigPinkSaveRoom), RoomLevelDefinitions.CrateriaSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BigPinkSaveRoom, RoomEnemyPopulationDefinitions.BigPinkSaveRoom, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigPinkSaveRoom, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Blue Brinstar Boulders room's state.</summary>
    public static RoomStateSettings BlueBrinstarBoulders { get; } =
        new(nameof(BlueBrinstarBoulders), RoomLevelDefinitions.BlueBrinstarBoulders, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarBoulders, RoomEnemyPopulationDefinitions.BlueBrinstarBoulders, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueBrinstarBoulders, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Billy Mays room's state.</summary>
    public static RoomStateSettings BillyMays { get; } =
        new(nameof(BillyMays), RoomLevelDefinitions.BillyMays, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BillyMays, RoomEnemyPopulationDefinitions.BillyMays, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BillyMays, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Save room's state.</summary>
    public static RoomStateSettings GreenBrinstarSave { get; } =
        new(nameof(GreenBrinstarSave), RoomLevelDefinitions.CrateriaSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarSave, RoomEnemyPopulationDefinitions.GreenBrinstarSave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Etecoon Save room's state.</summary>
    public static RoomStateSettings EtecoonSave { get; } =
        new(nameof(EtecoonSave), RoomLevelDefinitions.CrateriaSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSave, RoomEnemyPopulationDefinitions.EtecoonSave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EtecoonSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Red Tower room's state.</summary>
    public static RoomStateSettings RedTower { get; } =
        new(nameof(RedTower), RoomLevelDefinitions.RedTower, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedTower, RoomEnemyPopulationDefinitions.RedTower, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedTower, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedTower, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7WideVerticalTowerBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Red Brinstar Firefleas room's state.</summary>
    public static RoomStateSettings RedBrinstarFirefleas { get; } =
        new(nameof(RedBrinstarFirefleas), RoomLevelDefinitions.RedBrinstarFirefleas, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarFirefleas, RoomEnemyPopulationDefinitions.RedBrinstarFirefleas, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedBrinstarFirefleas, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Xray Scope room's state.</summary>
    public static RoomStateSettings XrayScope { get; } =
        new(nameof(XrayScope), RoomLevelDefinitions.XrayScope, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.XrayScope, RoomEnemyPopulationDefinitions.XrayScope, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.XrayScope, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7MechanicalRoomSpikeFloor, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Hellway room's state.</summary>
    public static RoomStateSettings Hellway { get; } =
        new(nameof(Hellway), RoomLevelDefinitions.Hellway, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Hellway, RoomEnemyPopulationDefinitions.Hellway, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Hellway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7VerticalTower, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Caterpillar room's state.</summary>
    public static RoomStateSettings Caterpillar { get; } =
        new(nameof(Caterpillar), RoomLevelDefinitions.Caterpillar, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Caterpillar, RoomEnemyPopulationDefinitions.Caterpillar, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Caterpillar, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Caterpillar, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Beta Power Bomb room's state.</summary>
    public static RoomStateSettings BetaPowerBomb { get; } =
        new(nameof(BetaPowerBomb), RoomLevelDefinitions.BetaPowerBomb, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BetaPowerBomb, RoomEnemyPopulationDefinitions.BetaPowerBomb, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BetaPowerBomb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BetaPowerBomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Alpha Power Bomb room's state.</summary>
    public static RoomStateSettings AlphaPowerBomb { get; } =
        new(nameof(AlphaPowerBomb), RoomLevelDefinitions.AlphaPowerBomb, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.AlphaPowerBomb, RoomEnemyPopulationDefinitions.AlphaPowerBomb, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AlphaPowerBomb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AlphaPowerBomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Skree Boost room's state.</summary>
    public static RoomStateSettings SkreeBoost { get; } =
        new(nameof(SkreeBoost), RoomLevelDefinitions.SkreeBoost, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SkreeBoost, RoomEnemyPopulationDefinitions.SkreeBoost, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SkreeBoost, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Below Spazer room's state.</summary>
    public static RoomStateSettings BelowSpazer { get; } =
        new(nameof(BelowSpazer), RoomLevelDefinitions.BelowSpazer, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BelowSpazer, RoomEnemyPopulationDefinitions.BelowSpazer, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BelowSpazer, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BelowSpazer, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7VerticalBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Spazer room's state.</summary>
    public static RoomStateSettings Spazer { get; } =
        new(nameof(Spazer), RoomLevelDefinitions.Spazer, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Spazer, RoomEnemyPopulationDefinitions.Spazer, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Spazer, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Spazer, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Warehouse Zeela room's state.</summary>
    public static RoomStateSettings WarehouseZeela { get; } =
        new(nameof(WarehouseZeela), RoomLevelDefinitions.WarehouseZeela, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedBeaconFlashing, RoomEnemyPopulationDefinitions.WarehouseZeela, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseZeela, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseZeela, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Warehouse E Tank room's state.</summary>
    public static RoomStateSettings WarehouseETank { get; } =
        new(nameof(WarehouseETank), RoomLevelDefinitions.WarehouseETank, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedBeaconFlashing, RoomEnemyPopulationDefinitions.WarehouseETank, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseETank, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Warehouse Kihunter room's state.</summary>
    public static RoomStateSettings WarehouseKihunter { get; } =
        new(nameof(WarehouseKihunter), RoomLevelDefinitions.WarehouseKihunter, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedBeaconFlashing, RoomEnemyPopulationDefinitions.WarehouseKihunter, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseKihunter, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseKihunter, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Mini Kraid room's state 0.</summary>
    public static RoomStateSettings MiniKraidDefault { get; } =
        new(nameof(MiniKraidDefault), RoomLevelDefinitions.MiniKraid, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraid, RoomEnemyPopulationDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Mini Kraid room's state 1.</summary>
    public static RoomStateSettings MiniKraidWhenAreaBossDead { get; } =
        new(nameof(MiniKraidWhenAreaBossDead), RoomLevelDefinitions.MiniKraid, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraid, RoomEnemyPopulationDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Kraid Eye Door room's state.</summary>
    public static RoomStateSettings KraidEyeDoor { get; } =
        new(nameof(KraidEyeDoor), RoomLevelDefinitions.KraidEyeDoor, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidEyeDoor, RoomEnemyPopulationDefinitions.KraidEyeDoor, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.KraidEyeDoor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.KraidEyeDoor, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Kraid room's state 0.</summary>
    public static RoomStateSettings KraidDefault { get; } =
        new(nameof(KraidDefault), RoomLevelDefinitions.Kraid, RoomGraphicsSet.Kraid, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Kraid, RoomEnemyPopulationDefinitions.Kraid, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Kraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar26KraidUpperLower, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Kraid room's state 1.</summary>
    public static RoomStateSettings KraidWhenAreaBossDead { get; } =
        new(nameof(KraidWhenAreaBossDead), RoomLevelDefinitions.Kraid, RoomGraphicsSet.Kraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Kraid, RoomEnemyPopulationDefinitions.Kraid, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Kraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.KraidDead, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Red Tower Refill room's state.</summary>
    public static RoomStateSettings RedTowerRefill { get; } =
        new(nameof(RedTowerRefill), RoomLevelDefinitions.DachoraEnergyRefill, RoomGraphicsSet.BlueRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RedTowerRefill, RoomEnemyPopulationDefinitions.RedTowerRefill, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedTowerRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Kraid Refill room's state.</summary>
    public static RoomStateSettings KraidRefill { get; } =
        new(nameof(KraidRefill), RoomLevelDefinitions.KraidRefill, RoomGraphicsSet.YellowRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidRefill, RoomEnemyPopulationDefinitions.KraidRefill, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.KraidRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Warehouse Entrance room's state.</summary>
    public static RoomStateSettings WarehouseEntrance { get; } =
        new(nameof(WarehouseEntrance), RoomLevelDefinitions.WarehouseEntrance, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseEntrance, RoomEnemyPopulationDefinitions.WarehouseEntrance, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseEntrance, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7MechanicalRoom, RoomSetupCallback.SharedReturn);


    /// <summary>The Varia Suit room's state.</summary>
    public static RoomStateSettings VariaSuit { get; } =
        new(nameof(VariaSuit), RoomLevelDefinitions.VariaSuit, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariaSuit, RoomEnemyPopulationDefinitions.VariaSuit, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.VariaSuit, null, RoomSetupCallback.SharedReturn);


    /// <summary>The Warehouse Save room's state.</summary>
    public static RoomStateSettings WarehouseSave { get; } =
        new(nameof(WarehouseSave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseSave, RoomEnemyPopulationDefinitions.WarehouseSave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseSave, null, RoomSetupCallback.SharedReturn);


    /// <summary>The Red Brinstar Save room's state.</summary>
    public static RoomStateSettings RedBrinstarSave { get; } =
        new(nameof(RedBrinstarSave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarSave, RoomEnemyPopulationDefinitions.RedBrinstarSave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedBrinstarSave, null, RoomSetupCallback.SharedReturn);
}
