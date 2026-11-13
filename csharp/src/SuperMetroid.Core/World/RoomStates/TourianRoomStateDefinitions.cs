using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Tourian room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class TourianRoomStateDefinitions
{

    /// <summary>The Tourian First room's state.</summary>
    public static RoomStateSettings TourianFirst { get; } =
        new(nameof(TourianFirst), RoomLevelDefinitions.TourianFirst, RoomGraphicsSet.Tourian, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.TourianFirst, RoomEnemyPopulationDefinitions.TourianFirst, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianFirst, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianFirst, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupBehavior.SetCollectedMap);


    /// <summary>The Metroids room's state 10.</summary>
    public static RoomStateSettings Metroids1Default { get; } =
        new(nameof(Metroids1Default), RoomLevelDefinitions.Metroids1, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1, RoomEnemyPopulationDefinitions.Metroids1Default, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 11.</summary>
    public static RoomStateSettings Metroids1WhenFirstMetroidHallCleared { get; } =
        new(nameof(Metroids1WhenFirstMetroidHallCleared), RoomLevelDefinitions.Metroids1, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids1, RoomEnemyPopulationDefinitions.Metroids1WhenFirstMetroidHallCleared, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids1, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 20.</summary>
    public static RoomStateSettings Metroids2Default { get; } =
        new(nameof(Metroids2Default), RoomLevelDefinitions.Metroids2, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2, RoomEnemyPopulationDefinitions.Metroids2Default, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 21.</summary>
    public static RoomStateSettings Metroids2WhenFirstMetroidShaftCleared { get; } =
        new(nameof(Metroids2WhenFirstMetroidShaftCleared), RoomLevelDefinitions.Metroids2, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids2, RoomEnemyPopulationDefinitions.Metroids2WhenFirstMetroidShaftCleared, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids2, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 30.</summary>
    public static RoomStateSettings Metroids3Default { get; } =
        new(nameof(Metroids3Default), RoomLevelDefinitions.Metroids3, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3, RoomEnemyPopulationDefinitions.Metroids3Default, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 31.</summary>
    public static RoomStateSettings Metroids3WhenSecondMetroidHallCleared { get; } =
        new(nameof(Metroids3WhenSecondMetroidHallCleared), RoomLevelDefinitions.Metroids3, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids3, RoomEnemyPopulationDefinitions.Metroids3WhenSecondMetroidHallCleared, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids3, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoomLetterbox, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 40.</summary>
    public static RoomStateSettings Metroids4Default { get; } =
        new(nameof(Metroids4Default), RoomLevelDefinitions.Metroids4, RoomGraphicsSet.Tourian, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4, RoomEnemyPopulationDefinitions.Metroids4Default, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids4, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Metroids room's state 41.</summary>
    public static RoomStateSettings Metroids4WhenSecondMetroidShaftCleared { get; } =
        new(nameof(Metroids4WhenSecondMetroidShaftCleared), RoomLevelDefinitions.Metroids4, RoomGraphicsSet.Tourian, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Metroids4, RoomEnemyPopulationDefinitions.Metroids4WhenSecondMetroidShaftCleared, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Metroids4, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Metroids4, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDMechanicalRoom, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Blue Hopper room's state 0.</summary>
    public static RoomStateSettings BlueHopperDefault { get; } =
        new(nameof(BlueHopperDefault), RoomLevelDefinitions.BlueHopper, RoomGraphicsSet.Tourian, new RoomMusic(MusicDataSet.BabyMetroidBossFight2, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopper, RoomEnemyPopulationDefinitions.BlueHopper, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueHopper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Blue Hopper room's state 1.</summary>
    public static RoomStateSettings BlueHopperWhenShitroidEncounterMusicSkipped { get; } =
        new(nameof(BlueHopperWhenShitroidEncounterMusicSkipped), RoomLevelDefinitions.BlueHopper, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BlueHopper, RoomEnemyPopulationDefinitions.BlueHopper, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BlueHopper, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BlueHopper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Dust Torizo room's state 0.</summary>
    public static RoomStateSettings DustTorizoDefault { get; } =
        new(nameof(DustTorizoDefault), RoomLevelDefinitions.DustTorizo, RoomGraphicsSet.Tourian, new RoomMusic(MusicDataSet.BabyMetroidBossFight2, RoomMusicTrack.Plays(MusicTrack.SecondSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizo, RoomEnemyPopulationDefinitions.DustTorizo, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DustTorizo, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Dust Torizo room's state 1.</summary>
    public static RoomStateSettings DustTorizoWhenShitroidEncounterMusicSkipped { get; } =
        new(nameof(DustTorizoWhenShitroidEncounterMusicSkipped), RoomLevelDefinitions.DustTorizo, RoomGraphicsSet.Tourian, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.DustTorizo, RoomEnemyPopulationDefinitions.DustTorizo, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.DustTorizo, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DustTorizo, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Big Boy room's state 0.</summary>
    public static RoomStateSettings BigBoyDefault { get; } =
        new(nameof(BigBoyDefault), RoomLevelDefinitions.BigBoy, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoy, RoomEnemyPopulationDefinitions.BigBoy, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigBoy, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Big Boy room's state 1.</summary>
    public static RoomStateSettings BigBoyWhenShitroidEncounterMusicSkipped { get; } =
        new(nameof(BigBoyWhenShitroidEncounterMusicSkipped), RoomLevelDefinitions.BigBoy, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BigBoy, RoomEnemyPopulationDefinitions.BigBoy, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BigBoy, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BigBoy, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Seaweed room's state.</summary>
    public static RoomStateSettings Seaweed { get; } =
        new(nameof(Seaweed), RoomLevelDefinitions.Seaweed, RoomGraphicsSet.Tourian, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.Seaweed, RoomEnemyPopulationDefinitions.Seaweed, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Seaweed, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Seaweed, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Tourian Recharge room's state.</summary>
    public static RoomStateSettings TourianRecharge { get; } =
        new(nameof(TourianRecharge), RoomLevelDefinitions.TourianRecharge, RoomGraphicsSet.YellowRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianRecharge, RoomEnemyPopulationDefinitions.TourianRecharge, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianRecharge, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianRecharge, null, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Mother Brain room's state 0.</summary>
    public static RoomStateSettings MotherBrainDefault { get; } =
        new(nameof(MotherBrainDefault), RoomLevelDefinitions.MotherBrain, RoomGraphicsSet.MotherBrain, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainDefaultOrWhenMotherBrainGlassDestroyed, RoomEnemyPopulationDefinitions.MotherBrain01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MotherBrain, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianEPipeRoomDark, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Mother Brain room's state 1.</summary>
    public static RoomStateSettings MotherBrainWhenMotherBrainGlassDestroyed { get; } =
        new(nameof(MotherBrainWhenMotherBrainGlassDestroyed), RoomLevelDefinitions.MotherBrain, RoomGraphicsSet.MotherBrain, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainDefaultOrWhenMotherBrainGlassDestroyed, RoomEnemyPopulationDefinitions.MotherBrain01, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MotherBrain, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianEPipeRoomDark, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Mother Brain room's state 2.</summary>
    public static RoomStateSettings MotherBrainWhenMainAreaBossDead { get; } =
        new(nameof(MotherBrainWhenMainAreaBossDead), RoomLevelDefinitions.MotherBrain, RoomGraphicsSet.MotherBrain, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainWhenMainAreaBossDead, RoomEnemyPopulationDefinitions.DraygonWhenAreaBossDeadOrMotherBrainWhenMainAreaBossDead, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrain, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MotherBrain, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianEPipeRoomDark, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Tourian Eye Door room's state.</summary>
    public static RoomStateSettings TourianEyeDoor { get; } =
        new(nameof(TourianEyeDoor), RoomLevelDefinitions.TourianEyeDoor, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEyeDoor, RoomEnemyPopulationDefinitions.TourianEyeDoor, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEyeDoor, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEyeDoor, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDVerticalStructures, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Rinka Shaft room's state.</summary>
    public static RoomStateSettings RinkaShaft { get; } =
        new(nameof(RinkaShaft), RoomLevelDefinitions.RinkaShaft, RoomGraphicsSet.Tourian, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.RinkaShaft, RoomEnemyPopulationDefinitions.RinkaShaft, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RinkaShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RinkaShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Mother Brain Save room's state.</summary>
    public static RoomStateSettings MotherBrainSave { get; } =
        new(nameof(MotherBrainSave), RoomLevelDefinitions.CrateriaSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.MotherBrainSave, RoomEnemyPopulationDefinitions.MotherBrainSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MotherBrainSave, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MotherBrainSave, null, RoomSetupBehavior.ReturnBeforeZebesTimebombSetup);


    /// <summary>The Tourian Escape room's state 1.</summary>
    public static RoomStateSettings TourianEscape1 { get; } =
        new(nameof(TourianEscape1), RoomLevelDefinitions.TourianEscape1, RoomGraphicsSet.Tourian, new RoomMusic(MusicDataSet.BossFight1, RoomMusicTrack.Plays(MusicTrack.ThirdSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape1, RoomEnemyPopulationDefinitions.TourianEscape1, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape1, null, RoomMainBehavior.GenerateRandomExplosionEveryFourthFrame, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEscape1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoomLetterbox, RoomSetupBehavior.SetZebesTimebombEventAndLightHorizontalShaking);


    /// <summary>The Tourian Escape room's state 2.</summary>
    public static RoomStateSettings TourianEscape2 { get; } =
        new(nameof(TourianEscape2), RoomLevelDefinitions.TourianEscape2, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape2, RoomEnemyPopulationDefinitions.TourianEscape2, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape2, null, RoomMainBehavior.ShakeScreenLightHorizontalAndMediumDiagonal, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEscape2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoomVerticalSeparation, RoomSetupBehavior.SetLightHorizontalRoomShaking);


    /// <summary>The Tourian Escape room's state 3.</summary>
    public static RoomStateSettings TourianEscape3 { get; } =
        new(nameof(TourianEscape3), RoomLevelDefinitions.TourianEscape3, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.TourianEscape3, RoomEnemyPopulationDefinitions.TourianEscape3, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape3, null, RoomMainBehavior.GenerateRandomExplosionEveryFourthFrame, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEscape3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoom, RoomSetupBehavior.SetMediumHorizontalRoomShaking);


    /// <summary>The Tourian Escape room's state 4.</summary>
    public static RoomStateSettings TourianEscape4 { get; } =
        new(nameof(TourianEscape4), RoomLevelDefinitions.TourianEscape4, RoomGraphicsSet.Tourian, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.ToruianEscaper4, RoomEnemyPopulationDefinitions.TourianEscape4, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.TourianEscape4, null, RoomMainBehavior.ShakeScreenMediumHorizontalAndStrongDiagonal, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.TourianEscape4, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.TourianDPipeRoom, RoomSetupBehavior.SetupEscapeRoom4ObjectAndMediumHorizontalShaking);


    /// <summary>The Upper Tourian Save room's state.</summary>
    public static RoomStateSettings UpperTourianSave { get; } =
        new(nameof(UpperTourianSave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.Tourian, RoomMusicTrack.Plays(MusicTrack.FirstSetSong)), SuperMetroid.Core.Fx.RoomFxDefinitions.UpperTourianSave, RoomEnemyPopulationDefinitions.UpperTourianSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.UpperTourianSave, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.UpperTourianSave, null, RoomSetupBehavior.None);
}
