using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Brinstar room state, named for their rooms.</summary>
public static partial class BrinstarRoomStateDefinitions
{

    /// <summary>The Green Brinstar Main Shaft room's state.</summary>
    public static RoomStateSettings GreenBrinstarMainShaft { get; } =
        new("GreenBrinstarMainShaft", RoomLevelDefinitions.GreenBrinstarMainShaft, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.GreenBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMainShaft, RoomEnemyPopulationDefinitions.GreenBrinstarMainShaft, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarMainShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarVerticalGlowPatches, RoomSetupBehavior.None);


    /// <summary>The Spore Spawn Super room's state.</summary>
    public static RoomStateSettings SporeSpawnSuper { get; } =
        new("SporeSpawnSuper", RoomLevelDefinitions.SporeSpawnSuper, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnSuper, RoomEnemyPopulationDefinitions.SporeSpawnSuper, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SporeSpawnSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawnSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Brinstar Pre Map room's state.</summary>
    public static RoomStateSettings BrinstarPreMap { get; } =
        new("BrinstarPreMap", RoomLevelDefinitions.BrinstarPreMap, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarPreMap, RoomEnemyPopulationDefinitions.BrinstarPreMap, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BrinstarPreMap, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarMediumHorizontalPattern, RoomSetupBehavior.None);


    /// <summary>The Early Supers room's state.</summary>
    public static RoomStateSettings EarlySupers { get; } =
        new("EarlySupers", RoomLevelDefinitions.EarlySupers, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.EarlySupers, RoomEnemyPopulationDefinitions.EarlySupers, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EarlySupers, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EarlySupers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarThinHorizontalPattern, RoomSetupBehavior.None);


    /// <summary>The Brinstar Reserve Tank room's state.</summary>
    public static RoomStateSettings BrinstarReserveTank { get; } =
        new("BrinstarReserveTank", RoomLevelDefinitions.BrinstarReserveTank, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarReserveTank, RoomEnemyPopulationDefinitions.BrinstarReserveTank, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BrinstarReserveTank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BrinstarReserveTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarLargeHorizontalPattern, RoomSetupBehavior.None);


    /// <summary>The Green Brinstar Map room's state.</summary>
    public static RoomStateSettings GreenBrinstarMap { get; } =
        new("GreenBrinstarMap", RoomLevelDefinitions.MapStationRoomDoorOnRight, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMap, RoomEnemyPopulationDefinitions.GreenBrinstarMap, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarMap, null, RoomSetupBehavior.None);


    /// <summary>The Green Brinstar Firefleas room's state.</summary>
    public static RoomStateSettings GreenBrinstarFirefleas { get; } =
        new("GreenBrinstarFirefleas", RoomLevelDefinitions.GreenBrinstarFirefleas, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarFirefleas, RoomEnemyPopulationDefinitions.GreenBrinstarFirefleas, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarFirefleas, null, RoomSetupBehavior.None);


    /// <summary>The Green Brinstar Missile Refill room's state.</summary>
    public static RoomStateSettings GreenBrinstarMissileRefill { get; } =
        new("GreenBrinstarMissileRefill", RoomLevelDefinitions.GreenBrinstarMissileRefill, RoomGraphicsSet.YellowRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMissileRefill, RoomEnemyPopulationDefinitions.GreenBrinstarMissileRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarMissileRefill, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarMissileRefill, null, RoomSetupBehavior.None);


    /// <summary>The Dachora room's state.</summary>
    public static RoomStateSettings Dachora { get; } =
        new("Dachora", RoomLevelDefinitions.Dachora, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Dachora, RoomEnemyPopulationDefinitions.Dachora, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Dachora, null, RoomMainBehavior.DachoraShaftCamera, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Dachora, null, RoomSetupBehavior.None);


    /// <summary>The Big Pink room's state.</summary>
    public static RoomStateSettings BigPink { get; } =
        new("BigPink", RoomLevelDefinitions.BigPink, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.GreenBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.BigPink, RoomEnemyPopulationDefinitions.BigPink, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigPink, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigPink, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarDarkPattern, RoomSetupBehavior.None);


    /// <summary>The Spore Spawn Kihunters room's state.</summary>
    public static RoomStateSettings SporeSpawnKihunters { get; } =
        new("SporeSpawnKihunters", RoomLevelDefinitions.SporeSpawnKihunters, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.GreenBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnKihunters, RoomEnemyPopulationDefinitions.SporeSpawnKihunters, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawnKihunters, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarHorizontalGlowPatches, RoomSetupBehavior.None);


    /// <summary>The Spore Spawn room's default state.</summary>
    public static RoomStateSettings SporeSpawnDefault { get; } =
        new("SporeSpawnDefault", RoomLevelDefinitions.SporeSpawn, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.MiniBossFight, RoomMusicTrack.Plays(MusicTrack.MiniBossBattle)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawn, RoomEnemyPopulationDefinitions.SporeSpawn, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllOpen, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawn, null, RoomSetupBehavior.None);


    /// <summary>The Spore Spawn room's state once the area's mini-boss is dead.</summary>
    public static RoomStateSettings SporeSpawnWhenAreaMiniBossDead { get; } =
        new("SporeSpawnWhenAreaMiniBossDead", RoomLevelDefinitions.SporeSpawn, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawn, RoomEnemyPopulationDefinitions.SporeSpawn, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllOpen, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawn, null, RoomSetupBehavior.None);


    /// <summary>The Pink Brinstar Power Bombs room's state.</summary>
    public static RoomStateSettings PinkBrinstarPowerBombs { get; } =
        new("PinkBrinstarPowerBombs", RoomLevelDefinitions.PinkBrinstarPowerBombs, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarPowerBombs, RoomEnemyPopulationDefinitions.PinkBrinstarPowerBombs, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PinkBrinstarPowerBombs, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PinkBrinstarPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarThinHorizontalPattern, RoomSetupBehavior.None);


    /// <summary>The Green Hill Zone room's state.</summary>
    public static RoomStateSettings GreenHillZone { get; } =
        new("GreenHillZone", RoomLevelDefinitions.GreenHillZone, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.GreenBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenHillZone, RoomEnemyPopulationDefinitions.GreenHillZone, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenHillZone, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenHillZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarSpheresPattern, RoomSetupBehavior.None);


    /// <summary>The Morph Ball room's default state.</summary>
    public static RoomStateSettings MorphBallDefault { get; } =
        new("MorphBallDefault", RoomLevelDefinitions.MorphBall, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(MusicTrack.BlueBrinstarArrival)), SuperMetroid.Core.Fx.RoomFxDefinitions.MorphBallDefault, RoomEnemyPopulationDefinitions.MorphBallDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MorphBallDefault, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarVariedSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Morph Ball room's state once Zebes is awake.</summary>
    public static RoomStateSettings MorphBallWhenZebesAwake { get; } =
        new("MorphBallWhenZebesAwake", RoomLevelDefinitions.MorphBall, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(MusicTrack.LowerCrateria)), SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarUntinted, RoomEnemyPopulationDefinitions.MorphBallWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MorphBallWhenZebesAwake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarVariedSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Construction Zone room's default state.</summary>
    public static RoomStateSettings ConstructionZoneDefault { get; } =
        new("ConstructionZoneDefault", RoomLevelDefinitions.ConstructionZone, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ConstructionZoneDefault, RoomEnemyPopulationDefinitions.ConstructionZoneDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ConstructionZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Construction Zone room's state once Zebes is awake.</summary>
    public static RoomStateSettings ConstructionZoneWhenZebesAwake { get; } =
        new("ConstructionZoneWhenZebesAwake", RoomLevelDefinitions.ConstructionZone, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarUntinted, RoomEnemyPopulationDefinitions.ConstructionZoneWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ConstructionZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Blue Brinstar Energy Tank room's default state.</summary>
    public static RoomStateSettings BlueBrinstarEnergyTankDefault { get; } =
        new("BlueBrinstarEnergyTankDefault", RoomLevelDefinitions.BlueBrinstarEnergyTank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarEnergyTankDefault, RoomEnemyPopulationDefinitions.BlueBrinstarEnergyTankDefault, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueBrinstarEnergyTank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueBrinstarEnergyTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarVariedSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Blue Brinstar Energy Tank room's state once Zebes is awake.</summary>
    public static RoomStateSettings BlueBrinstarEnergyTankWhenZebesAwake { get; } =
        new("BlueBrinstarEnergyTankWhenZebesAwake", RoomLevelDefinitions.BlueBrinstarEnergyTank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarUntinted, RoomEnemyPopulationDefinitions.BlueBrinstarEnergyTankWhenZebesAwake, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueBrinstarEnergyTank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueBrinstarEnergyTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarVariedSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Noob Bridge room's state.</summary>
    public static RoomStateSettings NoobBridge { get; } =
        new("NoobBridge", RoomLevelDefinitions.NoobBridge, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.GreenBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.NoobBridge, RoomEnemyPopulationDefinitions.NoobBridge, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.NoobBridge, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarHorizontalGlowPatches, RoomSetupBehavior.None);


    /// <summary>The Green Brinstar Beetoms room's state.</summary>
    public static RoomStateSettings GreenBrinstarBeetoms { get; } =
        new("GreenBrinstarBeetoms", RoomLevelDefinitions.GreenBrinstarBeetoms, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarBeetoms, RoomEnemyPopulationDefinitions.GreenBrinstarBeetoms, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarBeetoms, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarBeetoms, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarHorizontalGlowPatches, RoomSetupBehavior.None);


    /// <summary>The Etecoon Energy Tank room's state.</summary>
    public static RoomStateSettings EtecoonEnergyTank { get; } =
        new("EtecoonEnergyTank", RoomLevelDefinitions.EtecoonEnergyTank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonEnergyTank, RoomEnemyPopulationDefinitions.EtecoonEnergyTank, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EtecoonEnergyTank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EtecoonEnergyTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarHorizontalGlowPatches, RoomSetupBehavior.None);


    /// <summary>The Etecoon Super room's state.</summary>
    public static RoomStateSettings EtecoonSuper { get; } =
        new("EtecoonSuper", RoomLevelDefinitions.EtecoonSuper, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSuper, RoomEnemyPopulationDefinitions.EtecoonSuper, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EtecoonSuper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EtecoonSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarHorizontalGlowPatches, RoomSetupBehavior.None);


    /// <summary>The Dachora Energy Refill room's state.</summary>
    public static RoomStateSettings DachoraEnergyRefill { get; } =
        new("DachoraEnergyRefill", RoomLevelDefinitions.EnergyRefillRoomDoorOnRight, RoomGraphicsSet.BlueRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DachoraEnergyRefill, RoomEnemyPopulationDefinitions.DachoraEnergyRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DachoraEnergyRefill, null, RoomSetupBehavior.None);


    /// <summary>The Spore Spawn Farming room's state.</summary>
    public static RoomStateSettings SporeSpawnFarming { get; } =
        new("SporeSpawnFarming", RoomLevelDefinitions.SporeSpawnFarming, RoomGraphicsSet.GreenBlueBrinstar, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.GreenBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnFarming, RoomEnemyPopulationDefinitions.SporeSpawnFarming, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SporeSpawnFarming, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SporeSpawnFarming, null, RoomSetupBehavior.None);


    /// <summary>The Waterway Energy Tank room's state.</summary>
    public static RoomStateSettings WaterwayEnergyTank { get; } =
        new("WaterwayEnergyTank", RoomLevelDefinitions.WaterwayEnergyTank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WaterwayEnergyTank, RoomEnemyPopulationDefinitions.WaterwayEnergyTank, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WaterwayEnergyTank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WaterwayEnergyTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarThinHorizontalPattern, RoomSetupBehavior.None);


    /// <summary>The First Missile room's state.</summary>
    public static RoomStateSettings FirstMissile { get; } =
        new("FirstMissile", RoomLevelDefinitions.FirstMissile, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FirstMissile, RoomEnemyPopulationDefinitions.FirstMissile, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FirstMissile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Pink Brinstar Hoppers room's state.</summary>
    public static RoomStateSettings PinkBrinstarHoppers { get; } =
        new("PinkBrinstarHoppers", RoomLevelDefinitions.PinkBrinstarHoppers, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarHoppers, RoomEnemyPopulationDefinitions.PinkBrinstarHoppers, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PinkBrinstarHoppers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarThinHorizontalPattern, RoomSetupBehavior.None);


    /// <summary>The Hopper Energy Tank room's state.</summary>
    public static RoomStateSettings HopperEnergyTank { get; } =
        new("HopperEnergyTank", RoomLevelDefinitions.HopperEnergyTank, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.HopperEnergyTank, RoomEnemyPopulationDefinitions.HopperEnergyTank, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.HopperEnergyTank, null, RoomSetupBehavior.None);


    /// <summary>The Big Pink Save Room's state.</summary>
    public static RoomStateSettings BigPinkSaveRoom { get; } =
        new("BigPinkSaveRoom", RoomLevelDefinitions.SaveRoomDoorOnRight, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.GreenBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.BigPinkSaveRoom, RoomEnemyPopulationDefinitions.BigPinkSaveRoom, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigPinkSaveRoom, null, RoomSetupBehavior.None);


    /// <summary>The Blue Brinstar Boulders room's state.</summary>
    public static RoomStateSettings BlueBrinstarBoulders { get; } =
        new("BlueBrinstarBoulders", RoomLevelDefinitions.BlueBrinstarBoulders, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarBoulders, RoomEnemyPopulationDefinitions.BlueBrinstarBoulders, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueBrinstarBoulders, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarVariedSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Billy Mays room's state.</summary>
    public static RoomStateSettings BillyMays { get; } =
        new("BillyMays", RoomLevelDefinitions.BillyMays, RoomGraphicsSet.GreenBlueBrinstar, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BillyMays, RoomEnemyPopulationDefinitions.BillyMays, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BillyMays, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarVariedSmallPattern, RoomSetupBehavior.None);


    /// <summary>The Green Brinstar Save room's state.</summary>
    public static RoomStateSettings GreenBrinstarSave { get; } =
        new("GreenBrinstarSave", RoomLevelDefinitions.SaveRoomDoorOnRight, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.GreenBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarSave, RoomEnemyPopulationDefinitions.GreenBrinstarSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBrinstarSave, null, RoomSetupBehavior.None);


    /// <summary>The Etecoon Save room's state.</summary>
    public static RoomStateSettings EtecoonSave { get; } =
        new("EtecoonSave", RoomLevelDefinitions.SaveRoomDoorOnRight, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(MusicTrack.GreenBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSave, RoomEnemyPopulationDefinitions.EtecoonSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.EtecoonSave, null, RoomSetupBehavior.None);


    /// <summary>The Red Tower room's state.</summary>
    public static RoomStateSettings RedTower { get; } =
        new("RedTower", RoomLevelDefinitions.RedTower, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.RedBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedTower, RoomEnemyPopulationDefinitions.RedTower, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedTower, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedTower, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarWideVerticalTowerBrick, RoomSetupBehavior.None);


    /// <summary>The Red Brinstar Firefleas room's state.</summary>
    public static RoomStateSettings RedBrinstarFirefleas { get; } =
        new("RedBrinstarFirefleas", RoomLevelDefinitions.RedBrinstarFirefleas, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarFirefleas, RoomEnemyPopulationDefinitions.RedBrinstarFirefleas, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedBrinstarFirefleas, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedBrinstarFirefleas, null, RoomSetupBehavior.None);


    /// <summary>The Xray Scope room's state.</summary>
    public static RoomStateSettings XrayScope { get; } =
        new("XrayScope", RoomLevelDefinitions.XrayScope, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.XrayScope, RoomEnemyPopulationDefinitions.XrayScope, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.XrayScope, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarMechanicalRoomSpikeFloor, RoomSetupBehavior.None);


    /// <summary>The Hellway room's state.</summary>
    public static RoomStateSettings Hellway { get; } =
        new("Hellway", RoomLevelDefinitions.Hellway, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Hellway, RoomEnemyPopulationDefinitions.Hellway, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Hellway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarVerticalTower, RoomSetupBehavior.None);


    /// <summary>The Caterpillar room's state.</summary>
    public static RoomStateSettings Caterpillar { get; } =
        new("Caterpillar", RoomLevelDefinitions.Caterpillar, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.RedBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.Caterpillar, RoomEnemyPopulationDefinitions.Caterpillar, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Caterpillar, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Caterpillar, null, RoomSetupBehavior.None);


    /// <summary>The Beta Power Bomb room's state.</summary>
    public static RoomStateSettings BetaPowerBomb { get; } =
        new("BetaPowerBomb", RoomLevelDefinitions.BetaPowerBomb, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BetaPowerBomb, RoomEnemyPopulationDefinitions.BetaPowerBomb, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BetaPowerBomb, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BetaPowerBomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarNarrowVerticalTowerBrick, RoomSetupBehavior.None);


    /// <summary>The Alpha Power Bomb room's state.</summary>
    public static RoomStateSettings AlphaPowerBomb { get; } =
        new("AlphaPowerBomb", RoomLevelDefinitions.AlphaPowerBomb, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.AlphaPowerBomb, RoomEnemyPopulationDefinitions.AlphaPowerBomb, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AlphaPowerBomb, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AlphaPowerBomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarBlueGridBlocks, RoomSetupBehavior.None);


    /// <summary>The Skree Boost room's state.</summary>
    public static RoomStateSettings SkreeBoost { get; } =
        new("SkreeBoost", RoomLevelDefinitions.SkreeBoost, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SkreeBoost, RoomEnemyPopulationDefinitions.SkreeBoost, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SkreeBoost, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarNarrowVerticalTowerBrick, RoomSetupBehavior.None);


    /// <summary>The Below Spazer room's state.</summary>
    public static RoomStateSettings BelowSpazer { get; } =
        new("BelowSpazer", RoomLevelDefinitions.BelowSpazer, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.BelowSpazer, RoomEnemyPopulationDefinitions.BelowSpazer, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BelowSpazer, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BelowSpazer, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarVerticalBrick, RoomSetupBehavior.None);


    /// <summary>The Spazer room's state.</summary>
    public static RoomStateSettings Spazer { get; } =
        new("Spazer", RoomLevelDefinitions.Spazer, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Spazer, RoomEnemyPopulationDefinitions.Spazer, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Spazer, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Spazer, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarBlueGridBlocks, RoomSetupBehavior.None);


    /// <summary>The Warehouse Zeela room's state.</summary>
    public static RoomStateSettings WarehouseZeela { get; } =
        new("WarehouseZeela", RoomLevelDefinitions.WarehouseZeela, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.BeaconFlashing, RoomEnemyPopulationDefinitions.WarehouseZeela, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseZeela, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseZeela, null, RoomSetupBehavior.None);


    /// <summary>The Warehouse Energy Tank room's state.</summary>
    public static RoomStateSettings WarehouseEnergyTank { get; } =
        new("WarehouseEnergyTank", RoomLevelDefinitions.WarehouseEnergyTank, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BeaconFlashing, RoomEnemyPopulationDefinitions.WarehouseEnergyTank, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseEnergyTank, null, RoomSetupBehavior.None);


    /// <summary>The Warehouse Kihunter room's state.</summary>
    public static RoomStateSettings WarehouseKihunter { get; } =
        new("WarehouseKihunter", RoomLevelDefinitions.WarehouseKihunter, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.RedBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.BeaconFlashing, RoomEnemyPopulationDefinitions.WarehouseKihunter, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseKihunter, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseKihunter, null, RoomSetupBehavior.None);


    /// <summary>The Mini Kraid room's default state.</summary>
    public static RoomStateSettings MiniKraidDefault { get; } =
        new("MiniKraidDefault", RoomLevelDefinitions.MiniKraid, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.KraidAndPhantoonBattles, RoomMusicTrack.Plays(MusicTrack.BossRoomTension)), SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraid, RoomEnemyPopulationDefinitions.MiniKraid, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarNarrowVerticalTowerBrickVines, RoomSetupBehavior.None);


    /// <summary>The Mini Kraid room's state once the area's boss is dead.</summary>
    public static RoomStateSettings MiniKraidWhenAreaBossDead { get; } =
        new("MiniKraidWhenAreaBossDead", RoomLevelDefinitions.MiniKraid, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.KraidAndPhantoonBattles, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraid, RoomEnemyPopulationDefinitions.MiniKraid, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarNarrowVerticalTowerBrickVines, RoomSetupBehavior.None);


    /// <summary>The Kraid Eye Door room's state.</summary>
    public static RoomStateSettings KraidEyeDoor { get; } =
        new("KraidEyeDoor", RoomLevelDefinitions.KraidEyeDoor, RoomGraphicsSet.RedBrinstarKraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidEyeDoor, RoomEnemyPopulationDefinitions.KraidEyeDoor, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.KraidEyeDoor, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.KraidEyeDoor, null, RoomSetupBehavior.None);


    /// <summary>The Kraid room's default state.</summary>
    public static RoomStateSettings KraidDefault { get; } =
        new("KraidDefault", RoomLevelDefinitions.Kraid, RoomGraphicsSet.Kraid, new RoomMusic(MusicDataSet.KraidAndPhantoonBattles, RoomMusicTrack.Plays(MusicTrack.BossRoomTension)), SuperMetroid.Core.Fx.RoomFxDefinitions.Kraid, RoomEnemyPopulationDefinitions.Kraid, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Kraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.KraidAlive, RoomSetupBehavior.None);


    /// <summary>The Kraid room's state once the area's boss is dead.</summary>
    public static RoomStateSettings KraidWhenAreaBossDead { get; } =
        new("KraidWhenAreaBossDead", RoomLevelDefinitions.Kraid, RoomGraphicsSet.Kraid, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Kraid, RoomEnemyPopulationDefinitions.Kraid, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Kraid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Cleared, RoomSetupBehavior.None);


    /// <summary>The Red Tower Refill room's state.</summary>
    public static RoomStateSettings RedTowerRefill { get; } =
        new("RedTowerRefill", RoomLevelDefinitions.EnergyRefillRoomDoorOnRight, RoomGraphicsSet.BlueRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RedTowerRefill, RoomEnemyPopulationDefinitions.RedTowerRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedTowerRefill, null, RoomSetupBehavior.None);


    /// <summary>The Kraid Refill room's state.</summary>
    public static RoomStateSettings KraidRefill { get; } =
        new("KraidRefill", RoomLevelDefinitions.KraidRefill, RoomGraphicsSet.YellowRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidRefill, RoomEnemyPopulationDefinitions.KraidRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.KraidRefill, null, RoomSetupBehavior.None);


    /// <summary>The Warehouse Entrance room's state.</summary>
    public static RoomStateSettings WarehouseEntrance { get; } =
        new("WarehouseEntrance", RoomLevelDefinitions.WarehouseEntrance, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseEntrance, RoomEnemyPopulationDefinitions.WarehouseEntrance, new BackgroundScrollModes(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(3, 4)), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseEntrance, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.BrinstarMechanicalRoom, RoomSetupBehavior.None);


    /// <summary>The Varia Suit room's state.</summary>
    public static RoomStateSettings VariaSuit { get; } =
        new("VariaSuit", RoomLevelDefinitions.VariaSuit, RoomGraphicsSet.RedBrinstarKraid, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariaSuit, RoomEnemyPopulationDefinitions.VariaSuit, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.VariaSuit, null, RoomSetupBehavior.None);


    /// <summary>The Warehouse Save room's state.</summary>
    public static RoomStateSettings WarehouseSave { get; } =
        new("WarehouseSave", RoomLevelDefinitions.SaveRoomDoorOnLeft, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.RedBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseSave, RoomEnemyPopulationDefinitions.WarehouseSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WarehouseSave, null, RoomSetupBehavior.None);


    /// <summary>The Red Brinstar Save room's state.</summary>
    public static RoomStateSettings RedBrinstarSave { get; } =
        new("RedBrinstarSave", RoomLevelDefinitions.SaveRoomDoorOnLeft, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(MusicTrack.RedBrinstar)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarSave, RoomEnemyPopulationDefinitions.RedBrinstarSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedBrinstarSave, null, RoomSetupBehavior.None);
}
