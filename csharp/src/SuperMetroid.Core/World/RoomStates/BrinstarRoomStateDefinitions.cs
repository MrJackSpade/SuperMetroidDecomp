using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Brinstar room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class BrinstarRoomStateDefinitions
{

    /// <summary>The Green Brinstar Main Shaft room's state.</summary>
    public static RoomStateSettings GreenBrinstarMainShaft { get; } =
        new(nameof(GreenBrinstarMainShaft), RoomLevelDefinitions.GreenBrinstarMainShaft, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMainShaft, RoomEnemyPopulationDefinitions.GreenBrinstarMainShaft, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarMainShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6VerticalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn Super room's state.</summary>
    public static RoomStateSettings SporeSpawnSuper { get; } =
        new(nameof(SporeSpawnSuper), RoomLevelDefinitions.SporeSpawnSuper, 6, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnSuper, RoomEnemyPopulationDefinitions.SporeSpawnSuper, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SporeSpawnSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawnSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Brinstar Pre Map room's state.</summary>
    public static RoomStateSettings BrinstarPreMap { get; } =
        new(nameof(BrinstarPreMap), RoomLevelDefinitions.BrinstarPreMap, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarPreMap, RoomEnemyPopulationDefinitions.BrinstarPreMap, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BrinstarPreMap, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6MediumHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Early Supers room's state.</summary>
    public static RoomStateSettings EarlySupers { get; } =
        new(nameof(EarlySupers), RoomLevelDefinitions.EarlySupers, 6, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.EarlySupers, RoomEnemyPopulationDefinitions.EarlySupers, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EarlySupers, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EarlySupers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Brinstar Reserve Tank room's state.</summary>
    public static RoomStateSettings BrinstarReserveTank { get; } =
        new(nameof(BrinstarReserveTank), RoomLevelDefinitions.BrinstarReserveTank, 6, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarReserveTank, RoomEnemyPopulationDefinitions.BrinstarReserveTank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BrinstarReserveTank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BrinstarReserveTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6LargeHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Map room's state.</summary>
    public static RoomStateSettings GreenBrinstarMap { get; } =
        new(nameof(GreenBrinstarMap), RoomLevelDefinitions.GreenBrinstarMap, 21, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMap, RoomEnemyPopulationDefinitions.GreenBrinstarMap, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarMap, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Firefleas room's state.</summary>
    public static RoomStateSettings GreenBrinstarFirefleas { get; } =
        new(nameof(GreenBrinstarFirefleas), RoomLevelDefinitions.GreenBrinstarFirefleas, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarFirefleas, RoomEnemyPopulationDefinitions.GreenBrinstarFirefleas, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Missile Refill room's state.</summary>
    public static RoomStateSettings GreenBrinstarMissileRefill { get; } =
        new(nameof(GreenBrinstarMissileRefill), RoomLevelDefinitions.GreenBrinstarMissileRefill, 24, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMissileRefill, RoomEnemyPopulationDefinitions.GreenBrinstarMissileRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarMissileRefill, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarMissileRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Dachora room's state.</summary>
    public static RoomStateSettings Dachora { get; } =
        new(nameof(Dachora), RoomLevelDefinitions.Dachora, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Dachora, RoomEnemyPopulationDefinitions.Dachora, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Dachora, null, RoomMainCallback.ScrollScreenRightInDachoraRoom, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Dachora, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Big Pink room's state.</summary>
    public static RoomStateSettings BigPink { get; } =
        new(nameof(BigPink), RoomLevelDefinitions.BigPink, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BigPink, RoomEnemyPopulationDefinitions.BigPink, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigPink, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigPink, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6DarkPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn Kihunters room's state.</summary>
    public static RoomStateSettings SporeSpawnKihunters { get; } =
        new(nameof(SporeSpawnKihunters), RoomLevelDefinitions.SporeSpawnKihunters, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnKihunters, RoomEnemyPopulationDefinitions.SporeSpawnKihunters, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawnKihunters, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn room's state 0.</summary>
    public static RoomStateSettings SporeSpawnDefault { get; } =
        new(nameof(SporeSpawnDefault), RoomLevelDefinitions.SporeSpawn, 6, new RoomMusic(MusicDataSet.MiniBossFight, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawn, RoomEnemyPopulationDefinitions.SporeSpawn, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawn, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn room's state 1.</summary>
    public static RoomStateSettings SporeSpawnWhenAreaMiniBossDead { get; } =
        new(nameof(SporeSpawnWhenAreaMiniBossDead), RoomLevelDefinitions.SporeSpawn, 6, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawn, RoomEnemyPopulationDefinitions.SporeSpawn, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawn, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Pink Brinstar Power Bombs room's state.</summary>
    public static RoomStateSettings PinkBrinstarPowerBombs { get; } =
        new(nameof(PinkBrinstarPowerBombs), RoomLevelDefinitions.PinkBrinstarPowerBombs, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarPowerBombs, RoomEnemyPopulationDefinitions.PinkBrinstarPowerBombs, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PinkBrinstarPowerBombs, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PinkBrinstarPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Hill Zone room's state.</summary>
    public static RoomStateSettings GreenHillZone { get; } =
        new(nameof(GreenHillZone), RoomLevelDefinitions.GreenHillZone, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenHillZone, RoomEnemyPopulationDefinitions.GreenHillZone, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenHillZone, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenHillZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SpheresPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Morph Ball room's state 0.</summary>
    public static RoomStateSettings MorphBallDefault { get; } =
        new(nameof(MorphBallDefault), RoomLevelDefinitions.MorphBall, 6, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.ThirdSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.MorphBallDefault, RoomEnemyPopulationDefinitions.MorphBallDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MorphBallDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Morph Ball room's state 1.</summary>
    public static RoomStateSettings MorphBallWhenZebesAwake { get; } =
        new(nameof(MorphBallWhenZebesAwake), RoomLevelDefinitions.MorphBall, 6, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedPlain, RoomEnemyPopulationDefinitions.MorphBallWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MorphBallWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Construction Zone room's state 0.</summary>
    public static RoomStateSettings ConstructionZoneDefault { get; } =
        new(nameof(ConstructionZoneDefault), RoomLevelDefinitions.ConstructionZone, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ConstructionZoneDefault, RoomEnemyPopulationDefinitions.ConstructionZoneDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ConstructionZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Construction Zone room's state 1.</summary>
    public static RoomStateSettings ConstructionZoneWhenZebesAwake { get; } =
        new(nameof(ConstructionZoneWhenZebesAwake), RoomLevelDefinitions.ConstructionZone, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedPlain, RoomEnemyPopulationDefinitions.ConstructionZoneWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ConstructionZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Blue Brinstar E Tank room's state 0.</summary>
    public static RoomStateSettings BlueBrinstarETankDefault { get; } =
        new(nameof(BlueBrinstarETankDefault), RoomLevelDefinitions.BlueBrinstarETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarETankDefault, RoomEnemyPopulationDefinitions.BlueBrinstarETankDefault, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueBrinstarETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Blue Brinstar E Tank room's state 1.</summary>
    public static RoomStateSettings BlueBrinstarETankWhenZebesAwake { get; } =
        new(nameof(BlueBrinstarETankWhenZebesAwake), RoomLevelDefinitions.BlueBrinstarETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedPlain, RoomEnemyPopulationDefinitions.BlueBrinstarETankWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueBrinstarETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The Noob Bridge room's state.</summary>
    public static RoomStateSettings NoobBridge { get; } =
        new(nameof(NoobBridge), RoomLevelDefinitions.NoobBridge, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.NoobBridge, RoomEnemyPopulationDefinitions.NoobBridge, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.NoobBridge, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Beetoms room's state.</summary>
    public static RoomStateSettings GreenBrinstarBeetoms { get; } =
        new(nameof(GreenBrinstarBeetoms), RoomLevelDefinitions.GreenBrinstarBeetoms, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarBeetoms, RoomEnemyPopulationDefinitions.GreenBrinstarBeetoms, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarBeetoms, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarBeetoms, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Etecoon E Tank room's state.</summary>
    public static RoomStateSettings EtecoonETank { get; } =
        new(nameof(EtecoonETank), RoomLevelDefinitions.EtecoonETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonETank, RoomEnemyPopulationDefinitions.EtecoonETank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EtecoonETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EtecoonETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Etecoon Super room's state.</summary>
    public static RoomStateSettings EtecoonSuper { get; } =
        new(nameof(EtecoonSuper), RoomLevelDefinitions.EtecoonSuper, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSuper, RoomEnemyPopulationDefinitions.EtecoonSuper, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EtecoonSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EtecoonSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Dachora Energy Refill room's state.</summary>
    public static RoomStateSettings DachoraEnergyRefill { get; } =
        new(nameof(DachoraEnergyRefill), RoomLevelDefinitions.DachoraEnergyRefill, 23, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DachoraEnergyRefill, RoomEnemyPopulationDefinitions.DachoraEnergyRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DachoraEnergyRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Spore Spawn Farming room's state.</summary>
    public static RoomStateSettings SporeSpawnFarming { get; } =
        new(nameof(SporeSpawnFarming), RoomLevelDefinitions.SporeSpawnFarming, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnFarming, RoomEnemyPopulationDefinitions.SporeSpawnFarming, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SporeSpawnFarming, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawnFarming, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Waterway E Tank room's state.</summary>
    public static RoomStateSettings WaterwayETank { get; } =
        new(nameof(WaterwayETank), RoomLevelDefinitions.WaterwayETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WaterwayETank, RoomEnemyPopulationDefinitions.WaterwayETank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WaterwayETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WaterwayETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The First Missile room's state.</summary>
    public static RoomStateSettings FirstMissile { get; } =
        new(nameof(FirstMissile), RoomLevelDefinitions.FirstMissile, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FirstMissile, RoomEnemyPopulationDefinitions.FirstMissile, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FirstMissile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Pink Brinstar Hoppers room's state.</summary>
    public static RoomStateSettings PinkBrinstarHoppers { get; } =
        new(nameof(PinkBrinstarHoppers), RoomLevelDefinitions.PinkBrinstarHoppers, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarHoppers, RoomEnemyPopulationDefinitions.PinkBrinstarHoppers, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PinkBrinstarHoppers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Hopper E Tank room's state.</summary>
    public static RoomStateSettings HopperETank { get; } =
        new(nameof(HopperETank), RoomLevelDefinitions.HopperETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.HopperETank, RoomEnemyPopulationDefinitions.HopperETank, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.HopperETank, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Big Pink Save Room room's state.</summary>
    public static RoomStateSettings BigPinkSaveRoom { get; } =
        new(nameof(BigPinkSaveRoom), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BigPinkSaveRoom, RoomEnemyPopulationDefinitions.BigPinkSaveRoom, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigPinkSaveRoom, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Blue Brinstar Boulders room's state.</summary>
    public static RoomStateSettings BlueBrinstarBoulders { get; } =
        new(nameof(BlueBrinstarBoulders), RoomLevelDefinitions.BlueBrinstarBoulders, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarBoulders, RoomEnemyPopulationDefinitions.BlueBrinstarBoulders, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueBrinstarBoulders, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Billy Mays room's state.</summary>
    public static RoomStateSettings BillyMays { get; } =
        new(nameof(BillyMays), RoomLevelDefinitions.BillyMays, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BillyMays, RoomEnemyPopulationDefinitions.BillyMays, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BillyMays, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Green Brinstar Save room's state.</summary>
    public static RoomStateSettings GreenBrinstarSave { get; } =
        new(nameof(GreenBrinstarSave), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarSave, RoomEnemyPopulationDefinitions.GreenBrinstarSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Etecoon Save room's state.</summary>
    public static RoomStateSettings EtecoonSave { get; } =
        new(nameof(EtecoonSave), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSave, RoomEnemyPopulationDefinitions.EtecoonSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EtecoonSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The Red Tower room's state.</summary>
    public static RoomStateSettings RedTower { get; } =
        new(nameof(RedTower), RoomLevelDefinitions.RedTower, 7, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedTower, RoomEnemyPopulationDefinitions.RedTower, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedTower, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedTower, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7WideVerticalTowerBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Red Brinstar Firefleas room's state.</summary>
    public static RoomStateSettings RedBrinstarFirefleas { get; } =
        new(nameof(RedBrinstarFirefleas), RoomLevelDefinitions.RedBrinstarFirefleas, 7, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarFirefleas, RoomEnemyPopulationDefinitions.RedBrinstarFirefleas, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedBrinstarFirefleas, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Xray Scope room's state.</summary>
    public static RoomStateSettings XrayScope { get; } =
        new(nameof(XrayScope), RoomLevelDefinitions.XrayScope, 7, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.XrayScope, RoomEnemyPopulationDefinitions.XrayScope, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.XrayScope, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7MechanicalRoomSpikeFloor, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Hellway room's state.</summary>
    public static RoomStateSettings Hellway { get; } =
        new(nameof(Hellway), RoomLevelDefinitions.Hellway, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Hellway, RoomEnemyPopulationDefinitions.Hellway, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Hellway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7VerticalTower, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Caterpillar room's state.</summary>
    public static RoomStateSettings Caterpillar { get; } =
        new(nameof(Caterpillar), RoomLevelDefinitions.Caterpillar, 7, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Caterpillar, RoomEnemyPopulationDefinitions.Caterpillar, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Caterpillar, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Caterpillar, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Beta Power Bomb room's state.</summary>
    public static RoomStateSettings BetaPowerBomb { get; } =
        new(nameof(BetaPowerBomb), RoomLevelDefinitions.BetaPowerBomb, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BetaPowerBomb, RoomEnemyPopulationDefinitions.BetaPowerBomb, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BetaPowerBomb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BetaPowerBomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Alpha Power Bomb room's state.</summary>
    public static RoomStateSettings AlphaPowerBomb { get; } =
        new(nameof(AlphaPowerBomb), RoomLevelDefinitions.AlphaPowerBomb, 7, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.AlphaPowerBomb, RoomEnemyPopulationDefinitions.AlphaPowerBomb, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AlphaPowerBomb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AlphaPowerBomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Skree Boost room's state.</summary>
    public static RoomStateSettings SkreeBoost { get; } =
        new(nameof(SkreeBoost), RoomLevelDefinitions.SkreeBoost, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SkreeBoost, RoomEnemyPopulationDefinitions.SkreeBoost, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SkreeBoost, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Below Spazer room's state.</summary>
    public static RoomStateSettings BelowSpazer { get; } =
        new(nameof(BelowSpazer), RoomLevelDefinitions.BelowSpazer, 7, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BelowSpazer, RoomEnemyPopulationDefinitions.BelowSpazer, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BelowSpazer, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BelowSpazer, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7VerticalBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Spazer room's state.</summary>
    public static RoomStateSettings Spazer { get; } =
        new(nameof(Spazer), RoomLevelDefinitions.Spazer, 7, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Spazer, RoomEnemyPopulationDefinitions.Spazer, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Spazer, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Spazer, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Warehouse Zeela room's state.</summary>
    public static RoomStateSettings WarehouseZeela { get; } =
        new(nameof(WarehouseZeela), RoomLevelDefinitions.WarehouseZeela, 7, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedBeaconFlashing, RoomEnemyPopulationDefinitions.WarehouseZeela, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseZeela, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseZeela, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Warehouse E Tank room's state.</summary>
    public static RoomStateSettings WarehouseETank { get; } =
        new(nameof(WarehouseETank), RoomLevelDefinitions.WarehouseETank, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SharedBeaconFlashing, RoomEnemyPopulationDefinitions.WarehouseETank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseETank, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Warehouse Kihunter room's state.</summary>
    public static RoomStateSettings WarehouseKihunter { get; } =
        new(nameof(WarehouseKihunter), RoomLevelDefinitions.WarehouseKihunter, 7, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.SharedBeaconFlashing, RoomEnemyPopulationDefinitions.WarehouseKihunter, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseKihunter, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseKihunter, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Mini Kraid room's state 0.</summary>
    public static RoomStateSettings MiniKraidDefault { get; } =
        new(nameof(MiniKraidDefault), RoomLevelDefinitions.MiniKraid, 7, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraid, RoomEnemyPopulationDefinitions.MiniKraid, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Mini Kraid room's state 1.</summary>
    public static RoomStateSettings MiniKraidWhenAreaBossDead { get; } =
        new(nameof(MiniKraidWhenAreaBossDead), RoomLevelDefinitions.MiniKraid, 7, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraid, RoomEnemyPopulationDefinitions.MiniKraid, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Kraid Eye Door room's state.</summary>
    public static RoomStateSettings KraidEyeDoor { get; } =
        new(nameof(KraidEyeDoor), RoomLevelDefinitions.KraidEyeDoor, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidEyeDoor, RoomEnemyPopulationDefinitions.KraidEyeDoor, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.KraidEyeDoor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.KraidEyeDoor, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Kraid room's state 0.</summary>
    public static RoomStateSettings KraidDefault { get; } =
        new(nameof(KraidDefault), RoomLevelDefinitions.Kraid, 26, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Kraid, RoomEnemyPopulationDefinitions.Kraid, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Kraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar26KraidUpperLower, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Kraid room's state 1.</summary>
    public static RoomStateSettings KraidWhenAreaBossDead { get; } =
        new(nameof(KraidWhenAreaBossDead), RoomLevelDefinitions.Kraid, 26, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Kraid, RoomEnemyPopulationDefinitions.Kraid, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Kraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.KraidDead, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Red Tower Refill room's state.</summary>
    public static RoomStateSettings RedTowerRefill { get; } =
        new(nameof(RedTowerRefill), RoomLevelDefinitions.DachoraEnergyRefill, 23, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RedTowerRefill, RoomEnemyPopulationDefinitions.RedTowerRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedTowerRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Kraid Refill room's state.</summary>
    public static RoomStateSettings KraidRefill { get; } =
        new(nameof(KraidRefill), RoomLevelDefinitions.KraidRefill, 24, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidRefill, RoomEnemyPopulationDefinitions.KraidRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.KraidRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The Warehouse Entrance room's state.</summary>
    public static RoomStateSettings WarehouseEntrance { get; } =
        new(nameof(WarehouseEntrance), RoomLevelDefinitions.WarehouseEntrance, 7, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseEntrance, RoomEnemyPopulationDefinitions.WarehouseEntrance, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseEntrance, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7MechanicalRoom, RoomSetupCallback.SharedReturn);


    /// <summary>The Varia Suit room's state.</summary>
    public static RoomStateSettings VariaSuit { get; } =
        new(nameof(VariaSuit), RoomLevelDefinitions.VariaSuit, 7, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariaSuit, RoomEnemyPopulationDefinitions.VariaSuit, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.VariaSuit, null, RoomSetupCallback.SharedReturn);


    /// <summary>The Warehouse Save room's state.</summary>
    public static RoomStateSettings WarehouseSave { get; } =
        new(nameof(WarehouseSave), RoomLevelDefinitions.WarehouseSave, 25, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseSave, RoomEnemyPopulationDefinitions.WarehouseSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseSave, null, RoomSetupCallback.SharedReturn);


    /// <summary>The Red Brinstar Save room's state.</summary>
    public static RoomStateSettings RedBrinstarSave { get; } =
        new(nameof(RedBrinstarSave), RoomLevelDefinitions.WarehouseSave, 25, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarSave, RoomEnemyPopulationDefinitions.RedBrinstarSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedBrinstarSave, null, RoomSetupCallback.SharedReturn);
}
