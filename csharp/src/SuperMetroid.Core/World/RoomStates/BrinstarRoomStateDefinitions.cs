using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Brinstar room state, named by the disassembly's RoomState_ labels.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class BrinstarRoomStateDefinitions
{

    /// <summary>The disassembly's RoomState_GreenBrinstarMainShaft.</summary>
    public static RoomStateSettings GreenBrinstarMainShaft { get; } =
        new(nameof(GreenBrinstarMainShaft), RoomLevelDefinitions.GreenBrinstarMainShaft, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMainShaft, RoomEnemyPopulationDefinitions.GreenBrinstarMainShaft, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMainShaft, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 192), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarMainShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBrinstarMainShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6VerticalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawnSuper.</summary>
    public static RoomStateSettings SporeSpawnSuper { get; } =
        new(nameof(SporeSpawnSuper), RoomLevelDefinitions.SporeSpawnSuper, 6, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnSuper, RoomEnemyPopulationDefinitions.SporeSpawnSuper, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnSuper, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SporeSpawnSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SporeSpawnSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BrinstarPreMap.</summary>
    public static RoomStateSettings BrinstarPreMap { get; } =
        new(nameof(BrinstarPreMap), RoomLevelDefinitions.BrinstarPreMap, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarPreMap, RoomEnemyPopulationDefinitions.BrinstarPreMap, Enemies.RoomEnemyGraphicsSetDefinitions.BrinstarPreMap, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BrinstarPreMap, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6MediumHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_EarlySupers.</summary>
    public static RoomStateSettings EarlySupers { get; } =
        new(nameof(EarlySupers), RoomLevelDefinitions.EarlySupers, 6, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.EarlySupers, RoomEnemyPopulationDefinitions.EarlySupers, Enemies.RoomEnemyGraphicsSetDefinitions.EarlySupers, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EarlySupers, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EarlySupers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BrinstarReserveTank.</summary>
    public static RoomStateSettings BrinstarReserveTank { get; } =
        new(nameof(BrinstarReserveTank), RoomLevelDefinitions.BrinstarReserveTank, 6, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.BrinstarReserveTank, RoomEnemyPopulationDefinitions.BrinstarReserveTank, Enemies.RoomEnemyGraphicsSetDefinitions.BrinstarReserveTank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BrinstarReserveTank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BrinstarReserveTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6LargeHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarMap.</summary>
    public static RoomStateSettings GreenBrinstarMap { get; } =
        new(nameof(GreenBrinstarMap), RoomLevelDefinitions.GreenBrinstarMap, 21, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMap, RoomEnemyPopulationDefinitions.GreenBrinstarMap, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMap, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBrinstarMap, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarFirefleas.</summary>
    public static RoomStateSettings GreenBrinstarFirefleas { get; } =
        new(nameof(GreenBrinstarFirefleas), RoomLevelDefinitions.GreenBrinstarFirefleas, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarFirefleas, RoomEnemyPopulationDefinitions.GreenBrinstarFirefleas, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarFirefleas, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarMissileRefill.</summary>
    public static RoomStateSettings GreenBrinstarMissileRefill { get; } =
        new(nameof(GreenBrinstarMissileRefill), RoomLevelDefinitions.GreenBrinstarMissileRefill, 24, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarMissileRefill, RoomEnemyPopulationDefinitions.GreenBrinstarMissileRefill, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMissileRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarMissileRefill, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBrinstarMissileRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_Dachora.</summary>
    public static RoomStateSettings Dachora { get; } =
        new(nameof(Dachora), RoomLevelDefinitions.Dachora, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Dachora, RoomEnemyPopulationDefinitions.Dachora, Enemies.RoomEnemyGraphicsSetDefinitions.Dachora, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Dachora, null, RoomMainCallback.ScrollScreenRightInDachoraRoom, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Dachora, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BigPink.</summary>
    public static RoomStateSettings BigPink { get; } =
        new(nameof(BigPink), RoomLevelDefinitions.BigPink, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BigPink, RoomEnemyPopulationDefinitions.BigPink, Enemies.RoomEnemyGraphicsSetDefinitions.BigPink, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigPink, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BigPink, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6DarkPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawnKihunters.</summary>
    public static RoomStateSettings SporeSpawnKihunters { get; } =
        new(nameof(SporeSpawnKihunters), RoomLevelDefinitions.SporeSpawnKihunters, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnKihunters, RoomEnemyPopulationDefinitions.SporeSpawnKihunters, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnKihunters, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SporeSpawnKihunters, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawn_0.</summary>
    public static RoomStateSettings SporeSpawn0 { get; } =
        new(nameof(SporeSpawn0), RoomLevelDefinitions.SporeSpawn, 6, new RoomMusic(MusicDataSet.MiniBossFight, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnState01, RoomEnemyPopulationDefinitions.SporeSpawn, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawn, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SporeSpawnState01, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawn_1.</summary>
    public static RoomStateSettings SporeSpawn1 { get; } =
        new(nameof(SporeSpawn1), RoomLevelDefinitions.SporeSpawn, 6, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnState01, RoomEnemyPopulationDefinitions.SporeSpawn, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawn, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AllGreen, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SporeSpawnState01, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_PinkBrinstarPowerBombs.</summary>
    public static RoomStateSettings PinkBrinstarPowerBombs { get; } =
        new(nameof(PinkBrinstarPowerBombs), RoomLevelDefinitions.PinkBrinstarPowerBombs, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarPowerBombs, RoomEnemyPopulationDefinitions.PinkBrinstarPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.PinkBrinstarPowerBombs, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PinkBrinstarPowerBombs, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PinkBrinstarPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenHillZone.</summary>
    public static RoomStateSettings GreenHillZone { get; } =
        new(nameof(GreenHillZone), RoomLevelDefinitions.GreenHillZone, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenHillZone, RoomEnemyPopulationDefinitions.GreenHillZone, Enemies.RoomEnemyGraphicsSetDefinitions.GreenHillZone, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenHillZone, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenHillZone, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SpheresPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_MorphBall_0.</summary>
    public static RoomStateSettings MorphBall0 { get; } =
        new(nameof(MorphBall0), RoomLevelDefinitions.MorphBall, 6, new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(7)), SuperMetroid.Core.Fx.RoomFxDefinitions.MorphBallState0, RoomEnemyPopulationDefinitions.MorphBall0, Enemies.RoomEnemyGraphicsSetDefinitions.MorphBall0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MorphBallState0, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_MorphBall_1.</summary>
    public static RoomStateSettings MorphBall1 { get; } =
        new(nameof(MorphBall1), RoomLevelDefinitions.MorphBall, 6, new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381F4, RoomEnemyPopulationDefinitions.MorphBall1, Enemies.RoomEnemyGraphicsSetDefinitions.MorphBall1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MorphBall, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MorphBallState1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_ConstructionZone_0.</summary>
    public static RoomStateSettings ConstructionZone0 { get; } =
        new(nameof(ConstructionZone0), RoomLevelDefinitions.ConstructionZone, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ConstructionZoneState0, RoomEnemyPopulationDefinitions.ConstructionZone0, Enemies.RoomEnemyGraphicsSetDefinitions.ConstructionZone0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ConstructionZoneState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_ConstructionZone_1.</summary>
    public static RoomStateSettings ConstructionZone1 { get; } =
        new(nameof(ConstructionZone1), RoomLevelDefinitions.ConstructionZone, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381F4, RoomEnemyPopulationDefinitions.ConstructionZone1, Enemies.RoomEnemyGraphicsSetDefinitions.ConstructionZone1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ConstructionZone, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.ConstructionZoneState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_BlueBrinstarETank_0.</summary>
    public static RoomStateSettings BlueBrinstarETank0 { get; } =
        new(nameof(BlueBrinstarETank0), RoomLevelDefinitions.BlueBrinstarETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarETankState0, RoomEnemyPopulationDefinitions.BlueBrinstarETank0, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank0, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BlueBrinstarETankState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BlueBrinstarETank_1.</summary>
    public static RoomStateSettings BlueBrinstarETank1 { get; } =
        new(nameof(BlueBrinstarETank1), RoomLevelDefinitions.BlueBrinstarETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8381F4, RoomEnemyPopulationDefinitions.BlueBrinstarETank1, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank1, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueBrinstarETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BlueBrinstarETankState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeEscapeSkySetup);


    /// <summary>The disassembly's RoomState_NoobBridge.</summary>
    public static RoomStateSettings NoobBridge { get; } =
        new(nameof(NoobBridge), RoomLevelDefinitions.NoobBridge, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.NoobBridge, RoomEnemyPopulationDefinitions.NoobBridge, Enemies.RoomEnemyGraphicsSetDefinitions.NoobBridge, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.NoobBridge, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarBeetoms.</summary>
    public static RoomStateSettings GreenBrinstarBeetoms { get; } =
        new(nameof(GreenBrinstarBeetoms), RoomLevelDefinitions.GreenBrinstarBeetoms, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarBeetoms, RoomEnemyPopulationDefinitions.GreenBrinstarBeetoms, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarBeetoms, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBrinstarBeetoms, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBrinstarBeetoms, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_EtecoonETank.</summary>
    public static RoomStateSettings EtecoonETank { get; } =
        new(nameof(EtecoonETank), RoomLevelDefinitions.EtecoonETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonETank, RoomEnemyPopulationDefinitions.EtecoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EtecoonETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EtecoonETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_EtecoonSuper.</summary>
    public static RoomStateSettings EtecoonSuper { get; } =
        new(nameof(EtecoonSuper), RoomLevelDefinitions.EtecoonSuper, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSuper, RoomEnemyPopulationDefinitions.EtecoonSuper, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonSuper, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.EtecoonSuper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EtecoonSuper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6HorizontalGlowPatches, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_DachoraEnergyRefill.</summary>
    public static RoomStateSettings DachoraEnergyRefill { get; } =
        new(nameof(DachoraEnergyRefill), RoomLevelDefinitions.DachoraEnergyRefill, 23, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.DachoraEnergyRefill, RoomEnemyPopulationDefinitions.DachoraEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.DachoraEnergyRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.DachoraEnergyRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_SporeSpawnFarming.</summary>
    public static RoomStateSettings SporeSpawnFarming { get; } =
        new(nameof(SporeSpawnFarming), RoomLevelDefinitions.SporeSpawnFarming, 6, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.SporeSpawnFarming, RoomEnemyPopulationDefinitions.SporeSpawnFarming, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnFarming, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SporeSpawnFarming, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SporeSpawnFarming, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_WaterwayETank.</summary>
    public static RoomStateSettings WaterwayETank { get; } =
        new(nameof(WaterwayETank), RoomLevelDefinitions.WaterwayETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WaterwayETank, RoomEnemyPopulationDefinitions.WaterwayETank, Enemies.RoomEnemyGraphicsSetDefinitions.WaterwayETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WaterwayETank, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WaterwayETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_FirstMissile.</summary>
    public static RoomStateSettings FirstMissile { get; } =
        new(nameof(FirstMissile), RoomLevelDefinitions.FirstMissile, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FirstMissile, RoomEnemyPopulationDefinitions.FirstMissile, Enemies.RoomEnemyGraphicsSetDefinitions.FirstMissile, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.FirstMissile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_PinkBrinstarHoppers.</summary>
    public static RoomStateSettings PinkBrinstarHoppers { get; } =
        new(nameof(PinkBrinstarHoppers), RoomLevelDefinitions.PinkBrinstarHoppers, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PinkBrinstarHoppers, RoomEnemyPopulationDefinitions.PinkBrinstarHoppers, Enemies.RoomEnemyGraphicsSetDefinitions.PinkBrinstarHoppers, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.PinkBrinstarHoppers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6ThinHorizontalPattern, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_HopperETank.</summary>
    public static RoomStateSettings HopperETank { get; } =
        new(nameof(HopperETank), RoomLevelDefinitions.HopperETank, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.HopperETank, RoomEnemyPopulationDefinitions.HopperETank, Enemies.RoomEnemyGraphicsSetDefinitions.HopperETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.HopperETank, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BigPinkSaveRoom.</summary>
    public static RoomStateSettings BigPinkSaveRoom { get; } =
        new(nameof(BigPinkSaveRoom), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BigPinkSaveRoom, RoomEnemyPopulationDefinitions.BigPinkSaveRoom, Enemies.RoomEnemyGraphicsSetDefinitions.BigPinkSaveRoom, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BigPinkSaveRoom, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BlueBrinstarBoulders.</summary>
    public static RoomStateSettings BlueBrinstarBoulders { get; } =
        new(nameof(BlueBrinstarBoulders), RoomLevelDefinitions.BlueBrinstarBoulders, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueBrinstarBoulders, RoomEnemyPopulationDefinitions.BlueBrinstarBoulders, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarBoulders, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BlueBrinstarBoulders, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_BillyMays.</summary>
    public static RoomStateSettings BillyMays { get; } =
        new(nameof(BillyMays), RoomLevelDefinitions.BillyMays, 6, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BillyMays, RoomEnemyPopulationDefinitions.BillyMays, Enemies.RoomEnemyGraphicsSetDefinitions.BillyMays, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BillyMays, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar6SmallPatternVariety0, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_GreenBrinstarSave.</summary>
    public static RoomStateSettings GreenBrinstarSave { get; } =
        new(nameof(GreenBrinstarSave), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBrinstarSave, RoomEnemyPopulationDefinitions.GreenBrinstarSave, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.GreenBrinstarSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_EtecoonSave.</summary>
    public static RoomStateSettings EtecoonSave { get; } =
        new(nameof(EtecoonSave), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.GreenBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.EtecoonSave, RoomEnemyPopulationDefinitions.EtecoonSave, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.EtecoonSave, null, RoomSetupCallback.ReturnBeforeStatueSetupA);


    /// <summary>The disassembly's RoomState_RedTower.</summary>
    public static RoomStateSettings RedTower { get; } =
        new(nameof(RedTower), RoomLevelDefinitions.RedTower, 7, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedTower, RoomEnemyPopulationDefinitions.RedTower, Enemies.RoomEnemyGraphicsSetDefinitions.RedTower, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedTower, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedTower, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7WideVerticalTowerBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_RedBrinstarFirefleas.</summary>
    public static RoomStateSettings RedBrinstarFirefleas { get; } =
        new(nameof(RedBrinstarFirefleas), RoomLevelDefinitions.RedBrinstarFirefleas, 7, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarFirefleas, RoomEnemyPopulationDefinitions.RedBrinstarFirefleas, Enemies.RoomEnemyGraphicsSetDefinitions.RedBrinstarFirefleas, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedBrinstarFirefleas, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedBrinstarFirefleas, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_XrayScope.</summary>
    public static RoomStateSettings XrayScope { get; } =
        new(nameof(XrayScope), RoomLevelDefinitions.XrayScope, 7, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.XrayScope, RoomEnemyPopulationDefinitions.XrayScope, Enemies.RoomEnemyGraphicsSetDefinitions.XrayScope, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.XrayScope, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7MechanicalRoomSpikeFloor, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Hellway.</summary>
    public static RoomStateSettings Hellway { get; } =
        new(nameof(Hellway), RoomLevelDefinitions.Hellway, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Hellway, RoomEnemyPopulationDefinitions.Hellway, Enemies.RoomEnemyGraphicsSetDefinitions.Hellway, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Hellway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7VerticalTower, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Caterpillar.</summary>
    public static RoomStateSettings Caterpillar { get; } =
        new(nameof(Caterpillar), RoomLevelDefinitions.Caterpillar, 7, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.Caterpillar, RoomEnemyPopulationDefinitions.Caterpillar, Enemies.RoomEnemyGraphicsSetDefinitions.Caterpillar, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Caterpillar, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Caterpillar, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_BetaPowerBomb.</summary>
    public static RoomStateSettings BetaPowerBomb { get; } =
        new(nameof(BetaPowerBomb), RoomLevelDefinitions.BetaPowerBomb, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BetaPowerBomb, RoomEnemyPopulationDefinitions.BetaPowerBomb, Enemies.RoomEnemyGraphicsSetDefinitions.BetaPowerBomb, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BetaPowerBomb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BetaPowerBomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_AlphaPowerBomb.</summary>
    public static RoomStateSettings AlphaPowerBomb { get; } =
        new(nameof(AlphaPowerBomb), RoomLevelDefinitions.AlphaPowerBomb, 7, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.AlphaPowerBomb, RoomEnemyPopulationDefinitions.AlphaPowerBomb, Enemies.RoomEnemyGraphicsSetDefinitions.AlphaPowerBomb, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AlphaPowerBomb, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.AlphaPowerBomb, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_SkreeBoost.</summary>
    public static RoomStateSettings SkreeBoost { get; } =
        new(nameof(SkreeBoost), RoomLevelDefinitions.SkreeBoost, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SkreeBoost, RoomEnemyPopulationDefinitions.SkreeBoost, Enemies.RoomEnemyGraphicsSetDefinitions.SkreeBoost, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.SkreeBoost, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrick, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_BelowSpazer.</summary>
    public static RoomStateSettings BelowSpazer { get; } =
        new(nameof(BelowSpazer), RoomLevelDefinitions.BelowSpazer, 7, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.BelowSpazer, RoomEnemyPopulationDefinitions.BelowSpazer, Enemies.RoomEnemyGraphicsSetDefinitions.BelowSpazer, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BelowSpazer, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.BelowSpazer, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7VerticalBrick0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Spazer.</summary>
    public static RoomStateSettings Spazer { get; } =
        new(nameof(Spazer), RoomLevelDefinitions.Spazer, 7, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.Spazer, RoomEnemyPopulationDefinitions.Spazer, Enemies.RoomEnemyGraphicsSetDefinitions.Spazer, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Spazer, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Spazer, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7BlueGridBlocks, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_WarehouseZeela.</summary>
    public static RoomStateSettings WarehouseZeela { get; } =
        new(nameof(WarehouseZeela), RoomLevelDefinitions.WarehouseZeela, 7, new RoomMusic(null, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8383D2, RoomEnemyPopulationDefinitions.WarehouseZeela, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseZeela, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseZeela, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WarehouseZeela, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_WarehouseETank.</summary>
    public static RoomStateSettings WarehouseETank { get; } =
        new(nameof(WarehouseETank), RoomLevelDefinitions.WarehouseETank, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8383D2, RoomEnemyPopulationDefinitions.WarehouseETank, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseETank, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WarehouseETank, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_WarehouseKihunter.</summary>
    public static RoomStateSettings WarehouseKihunter { get; } =
        new(nameof(WarehouseKihunter), RoomLevelDefinitions.WarehouseKihunter, 7, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariousRooms8383D2, RoomEnemyPopulationDefinitions.WarehouseKihunter, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseKihunter, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(192, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseKihunter, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WarehouseKihunter, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_MiniKraid_0.</summary>
    public static RoomStateSettings MiniKraid0 { get; } =
        new(nameof(MiniKraid0), RoomLevelDefinitions.MiniKraid, 7, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraidState01, RoomEnemyPopulationDefinitions.MiniKraid, Enemies.RoomEnemyGraphicsSetDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MiniKraidState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_MiniKraid_1.</summary>
    public static RoomStateSettings MiniKraid1 { get; } =
        new(nameof(MiniKraid1), RoomLevelDefinitions.MiniKraid, 7, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.MiniKraidState01, RoomEnemyPopulationDefinitions.MiniKraid, Enemies.RoomEnemyGraphicsSetDefinitions.MiniKraid, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.MiniKraidState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7NarrowVerticalTowerBrickVines0, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_KraidEyeDoor.</summary>
    public static RoomStateSettings KraidEyeDoor { get; } =
        new(nameof(KraidEyeDoor), RoomLevelDefinitions.KraidEyeDoor, 7, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidEyeDoor, RoomEnemyPopulationDefinitions.KraidEyeDoor, Enemies.RoomEnemyGraphicsSetDefinitions.KraidEyeDoor, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.KraidEyeDoor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.KraidEyeDoor, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Kraid_0.</summary>
    public static RoomStateSettings Kraid0 { get; } =
        new(nameof(Kraid0), RoomLevelDefinitions.Kraid, 26, new RoomMusic(MusicDataSet.BossFight2, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState0, RoomEnemyPopulationDefinitions.Kraid, Enemies.RoomEnemyGraphicsSetDefinitions.Kraid, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.KraidState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar1AKraidUpperLower, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_Kraid_1.</summary>
    public static RoomStateSettings Kraid1 { get; } =
        new(nameof(Kraid1), RoomLevelDefinitions.Kraid, 26, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidState0, RoomEnemyPopulationDefinitions.Kraid, Enemies.RoomEnemyGraphicsSetDefinitions.Kraid, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(1, 1), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Kraid, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.KraidState01, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.KraidDead, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_RedTowerRefill.</summary>
    public static RoomStateSettings RedTowerRefill { get; } =
        new(nameof(RedTowerRefill), RoomLevelDefinitions.DachoraEnergyRefill, 23, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RedTowerRefill, RoomEnemyPopulationDefinitions.RedTowerRefill, Enemies.RoomEnemyGraphicsSetDefinitions.RedTowerRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedTowerRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_KraidRefill.</summary>
    public static RoomStateSettings KraidRefill { get; } =
        new(nameof(KraidRefill), RoomLevelDefinitions.KraidRefill, 24, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KraidRefill, RoomEnemyPopulationDefinitions.KraidRefill, Enemies.RoomEnemyGraphicsSetDefinitions.KraidRefill, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.KraidRefill, null, RoomSetupCallback.ReturnBeforeStatueSetupB);


    /// <summary>The disassembly's RoomState_WarehouseEntrance.</summary>
    public static RoomStateSettings WarehouseEntrance { get; } =
        new(nameof(WarehouseEntrance), RoomLevelDefinitions.WarehouseEntrance, 7, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseEntrance, RoomEnemyPopulationDefinitions.WarehouseEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseEntrance, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WarehouseEntrance, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WarehouseEntrance, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.Brinstar7MechanicalRoom, RoomSetupCallback.SharedReturn);


    /// <summary>The disassembly's RoomState_VariaSuit.</summary>
    public static RoomStateSettings VariaSuit { get; } =
        new(nameof(VariaSuit), RoomLevelDefinitions.VariaSuit, 7, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.VariaSuit, RoomEnemyPopulationDefinitions.VariaSuit, Enemies.RoomEnemyGraphicsSetDefinitions.VariaSuit, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.VariaSuit, null, RoomSetupCallback.SharedReturn);


    /// <summary>The disassembly's RoomState_WarehouseSave.</summary>
    public static RoomStateSettings WarehouseSave { get; } =
        new(nameof(WarehouseSave), RoomLevelDefinitions.WarehouseSave, 25, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.WarehouseSave, RoomEnemyPopulationDefinitions.WarehouseSave, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.WarehouseSave, null, RoomSetupCallback.SharedReturn);


    /// <summary>The disassembly's RoomState_RedBrinstarSave.</summary>
    public static RoomStateSettings RedBrinstarSave { get; } =
        new(nameof(RedBrinstarSave), RoomLevelDefinitions.WarehouseSave, 25, new RoomMusic(MusicDataSet.RedBrinstar, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.RedBrinstarSave, RoomEnemyPopulationDefinitions.RedBrinstarSave, Enemies.RoomEnemyGraphicsSetDefinitions.RedBrinstarSave, SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.RedBrinstarSave, null, RoomSetupCallback.SharedReturn);
}
