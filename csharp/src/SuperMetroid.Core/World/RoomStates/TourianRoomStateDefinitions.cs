using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Tourian room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class TourianRoomStateDefinitions
{

    /// <summary>The Tourian First room's state.</summary>
    public static RoomStateSettings TourianFirst { get; } =
        new(nameof(TourianFirst), RoomLevelDefinitions.TourianFirst, 13, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.TourianFirst, RoomEnemyPopulationDefinitions.TourianFirst, Enemies.RoomEnemyGraphicsSetDefinitions.TourianFirst, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(1, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianFirst, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianFirst, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.SetCollectedMap);


    /// <summary>The Metroids room's state 10.</summary>
    public static RoomStateSettings Metroids1Default { get; } =
        new(nameof(Metroids1Default), RoomLevelDefinitions.Metroids1, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1, RoomEnemyPopulationDefinitions.Metroids1Default, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids1Default, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 11.</summary>
    public static RoomStateSettings Metroids1WhenFirstMetroidHallCleared { get; } =
        new(nameof(Metroids1WhenFirstMetroidHallCleared), RoomLevelDefinitions.Metroids1, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1, RoomEnemyPopulationDefinitions.Metroids1WhenFirstMetroidHallCleared, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids1WhenFirstMetroidHallCleared, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 20.</summary>
    public static RoomStateSettings Metroids2Default { get; } =
        new(nameof(Metroids2Default), RoomLevelDefinitions.Metroids2, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2, RoomEnemyPopulationDefinitions.Metroids2Default, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids2Default, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 21.</summary>
    public static RoomStateSettings Metroids2WhenFirstMetroidShaftCleared { get; } =
        new(nameof(Metroids2WhenFirstMetroidShaftCleared), RoomLevelDefinitions.Metroids2, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2, RoomEnemyPopulationDefinitions.Metroids2WhenFirstMetroidShaftCleared, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids2WhenFirstMetroidShaftCleared, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 30.</summary>
    public static RoomStateSettings Metroids3Default { get; } =
        new(nameof(Metroids3Default), RoomLevelDefinitions.Metroids3, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3, RoomEnemyPopulationDefinitions.Metroids3Default, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids3Default, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 31.</summary>
    public static RoomStateSettings Metroids3WhenSecondMetroidHallCleared { get; } =
        new(nameof(Metroids3WhenSecondMetroidHallCleared), RoomLevelDefinitions.Metroids3, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3, RoomEnemyPopulationDefinitions.Metroids3WhenSecondMetroidHallCleared, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids3WhenSecondMetroidHallCleared, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 40.</summary>
    public static RoomStateSettings Metroids4Default { get; } =
        new(nameof(Metroids4Default), RoomLevelDefinitions.Metroids4, 13, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4, RoomEnemyPopulationDefinitions.Metroids4Default, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids4Default, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids4, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 41.</summary>
    public static RoomStateSettings Metroids4WhenSecondMetroidShaftCleared { get; } =
        new(nameof(Metroids4WhenSecondMetroidShaftCleared), RoomLevelDefinitions.Metroids4, 13, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4, RoomEnemyPopulationDefinitions.Metroids4WhenSecondMetroidShaftCleared, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids4WhenSecondMetroidShaftCleared, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids4, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Blue Hopper room's state 0.</summary>
    public static RoomStateSettings BlueHopperDefault { get; } =
        new(nameof(BlueHopperDefault), RoomLevelDefinitions.BlueHopper, 13, new RoomMusic(MusicDataSet.BabyMetroidBossFight2, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopper, RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueHopper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Blue Hopper room's state 1.</summary>
    public static RoomStateSettings BlueHopperWhenUnused14 { get; } =
        new(nameof(BlueHopperWhenUnused14), RoomLevelDefinitions.BlueHopper, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopper, RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueHopper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Dust Torizo room's state 0.</summary>
    public static RoomStateSettings DustTorizoDefault { get; } =
        new(nameof(DustTorizoDefault), RoomLevelDefinitions.DustTorizo, 13, new RoomMusic(MusicDataSet.BabyMetroidBossFight2, RoomMusicTrack.Plays(6)), SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizo, RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DustTorizo, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Dust Torizo room's state 1.</summary>
    public static RoomStateSettings DustTorizoWhenUnused14 { get; } =
        new(nameof(DustTorizoWhenUnused14), RoomLevelDefinitions.DustTorizo, 13, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizo, RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DustTorizo, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Big Boy room's state 0.</summary>
    public static RoomStateSettings BigBoyDefault { get; } =
        new(nameof(BigBoyDefault), RoomLevelDefinitions.BigBoy, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoy, RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigBoy, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Big Boy room's state 1.</summary>
    public static RoomStateSettings BigBoyWhenUnused14 { get; } =
        new(nameof(BigBoyWhenUnused14), RoomLevelDefinitions.BigBoy, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoy, RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigBoy, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Seaweed room's state.</summary>
    public static RoomStateSettings Seaweed { get; } =
        new(nameof(Seaweed), RoomLevelDefinitions.Seaweed, 13, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.Seaweed, RoomEnemyPopulationDefinitions.Seaweed, Enemies.RoomEnemyGraphicsSetDefinitions.Seaweed, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Seaweed, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Seaweed, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Tourian Recharge room's state.</summary>
    public static RoomStateSettings TourianRecharge { get; } =
        new(nameof(TourianRecharge), RoomLevelDefinitions.TourianRecharge, 24, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianRecharge, RoomEnemyPopulationDefinitions.TourianRecharge, Enemies.RoomEnemyGraphicsSetDefinitions.TourianRecharge, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianRecharge, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianRecharge, null, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Mother Brain room's state 0.</summary>
    public static RoomStateSettings MotherBrainDefault { get; } =
        new(nameof(MotherBrainDefault), RoomLevelDefinitions.MotherBrain, 14, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainDefaultOrWhenMotherBrainGlassDestroyed, RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MotherBrain, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Mother Brain room's state 1.</summary>
    public static RoomStateSettings MotherBrainWhenMotherBrainGlassDestroyed { get; } =
        new(nameof(MotherBrainWhenMotherBrainGlassDestroyed), RoomLevelDefinitions.MotherBrain, 14, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainDefaultOrWhenMotherBrainGlassDestroyed, RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MotherBrain, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Mother Brain room's state 2.</summary>
    public static RoomStateSettings MotherBrainWhenMainAreaBossDead { get; } =
        new(nameof(MotherBrainWhenMainAreaBossDead), RoomLevelDefinitions.MotherBrain, 14, new RoomMusic(null, RoomMusicTrack.Plays(3)), SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainWhenMainAreaBossDead, RoomEnemyPopulationDefinitions.DraygonWhenAreaBossDeadOrMotherBrainWhenMainAreaBossDead, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MotherBrain, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianEPipeRoomDark, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Tourian Eye Door room's state.</summary>
    public static RoomStateSettings TourianEyeDoor { get; } =
        new(nameof(TourianEyeDoor), RoomLevelDefinitions.TourianEyeDoor, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEyeDoor, RoomEnemyPopulationDefinitions.TourianEyeDoor, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEyeDoor, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEyeDoor, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEyeDoor, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Rinka Shaft room's state.</summary>
    public static RoomStateSettings RinkaShaft { get; } =
        new(nameof(RinkaShaft), RoomLevelDefinitions.RinkaShaft, 13, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.RinkaShaft, RoomEnemyPopulationDefinitions.RinkaShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RinkaShaft, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RinkaShaft, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RinkaShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Mother Brain Save room's state.</summary>
    public static RoomStateSettings MotherBrainSave { get; } =
        new(nameof(MotherBrainSave), RoomLevelDefinitions.CrateriaSave, 25, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainSave, RoomEnemyPopulationDefinitions.MotherBrainSave, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrainSave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrainSave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MotherBrainSave, null, RoomSetupCallback.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Tourian Escape room's state 1.</summary>
    public static RoomStateSettings TourianEscape1 { get; } =
        new(nameof(TourianEscape1), RoomLevelDefinitions.TourianEscape1, 13, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(7)), SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape1, RoomEnemyPopulationDefinitions.TourianEscape1, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape1, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape1, null, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEscape1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoomLetterbox, RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking);


    /// <summary>The Tourian Escape room's state 2.</summary>
    public static RoomStateSettings TourianEscape2 { get; } =
        new(nameof(TourianEscape2), RoomLevelDefinitions.TourianEscape2, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape2, RoomEnemyPopulationDefinitions.TourianEscape2, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape2, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape2, null, RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEscape2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupCallback.SetLightHorizontalRoomShaking);


    /// <summary>The Tourian Escape room's state 3.</summary>
    public static RoomStateSettings TourianEscape3 { get; } =
        new(nameof(TourianEscape3), RoomLevelDefinitions.TourianEscape3, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape3, RoomEnemyPopulationDefinitions.TourianEscape3, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape3, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape3, null, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEscape3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoom, RoomSetupCallback.SetMediumHorizontalRoomShaking);


    /// <summary>The Tourian Escape room's state 4.</summary>
    public static RoomStateSettings TourianEscape4 { get; } =
        new(nameof(TourianEscape4), RoomLevelDefinitions.TourianEscape4, 13, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ToruianEscaper4, RoomEnemyPopulationDefinitions.TourianEscape4, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape4, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(193, 193), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape4, null, RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEscape4, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoom, RoomSetupCallback.SetupEscapeRoom4ObjectAndMediumHorizontalShaking);


    /// <summary>The Upper Tourian Save room's state.</summary>
    public static RoomStateSettings UpperTourianSave { get; } =
        new(nameof(UpperTourianSave), RoomLevelDefinitions.WarehouseSave, 25, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(5)), SuperMetroid.Core.Fx.RoomFxDefinitions.UpperTourianSave, RoomEnemyPopulationDefinitions.UpperTourianSave, Enemies.RoomEnemyGraphicsSetDefinitions.UpperTourianSave, SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes.FromHeader(0, 0), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.UpperTourianSave, null, RoomMainCallback.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.UpperTourianSave, null, RoomSetupCallback.None);
}
