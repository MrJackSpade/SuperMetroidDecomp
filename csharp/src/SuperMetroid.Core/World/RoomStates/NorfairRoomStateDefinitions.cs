using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The initialization settings of each Norfair room state, named for their rooms.</summary>
[NumberedIdentities("Each numbered state is one state of its room, chosen by its own event condition; the number tells the states apart and no caller indexes them.")]
public static class NorfairRoomStateDefinitions
{

    /// <summary>The Ice Beam Acid room's state.</summary>
    public static RoomStateSettings IceBeamAcid { get; } =
        new(nameof(IceBeamAcid), RoomLevelDefinitions.IceBeamAcid, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamAcid, RoomEnemyPopulationDefinitions.IceBeamAcid, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.IceBeamAcid, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurpleBright, RoomSetupBehavior.None);


    /// <summary>The Cathedral room's state.</summary>
    public static RoomStateSettings Cathedral { get; } =
        new(nameof(Cathedral), RoomLevelDefinitions.Cathedral, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Cathedral, RoomEnemyPopulationDefinitions.Cathedral, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Cathedral, null, RoomSetupBehavior.None);


    /// <summary>The Cathedral Entrance room's state.</summary>
    public static RoomStateSettings CathedralEntrance { get; } =
        new(nameof(CathedralEntrance), RoomLevelDefinitions.CathedralEntrance, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CathedralEntrance, RoomEnemyPopulationDefinitions.CathedralEntrance, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CathedralEntrance, null, RoomSetupBehavior.None);


    /// <summary>The Business Center room's state.</summary>
    public static RoomStateSettings BusinessCenter { get; } =
        new(nameof(BusinessCenter), RoomLevelDefinitions.BusinessCenter, RoomGraphicsSet.UnheatedNorfair, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.BusinessCenter, RoomEnemyPopulationDefinitions.BusinessCenter, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BusinessCenter, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernVertical, RoomSetupBehavior.None);


    /// <summary>The Ice Beam Gate room's state.</summary>
    public static RoomStateSettings IceBeamGate { get; } =
        new(nameof(IceBeamGate), RoomLevelDefinitions.IceBeamGate, RoomGraphicsSet.UnheatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamGate, RoomEnemyPopulationDefinitions.IceBeamGate, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.IceBeamGate, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.IceBeamGate, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurple, RoomSetupBehavior.None);


    /// <summary>The Ice Beam Tutorial room's state.</summary>
    public static RoomStateSettings IceBeamTutorial { get; } =
        new(nameof(IceBeamTutorial), RoomLevelDefinitions.IceBeamTutorial, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamTutorial, RoomEnemyPopulationDefinitions.IceBeamTutorial, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.IceBeamTutorial, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurpleBright, RoomSetupBehavior.None);


    /// <summary>The Ice Beam room's state.</summary>
    public static RoomStateSettings IceBeam { get; } =
        new(nameof(IceBeam), RoomLevelDefinitions.IceBeam, RoomGraphicsSet.UnheatedNorfair, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeam, RoomEnemyPopulationDefinitions.IceBeam, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.IceBeam, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurple, RoomSetupBehavior.None);


    /// <summary>The Ice Beam Snake room's state.</summary>
    public static RoomStateSettings IceBeamSnake { get; } =
        new(nameof(IceBeamSnake), RoomLevelDefinitions.IceBeamSnake, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.IceBeamSnake, RoomEnemyPopulationDefinitions.IceBeamSnake, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.IceBeamSnake, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.IceBeamSnake, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurple, RoomSetupBehavior.None);


    /// <summary>The Crumble Shaft room's state.</summary>
    public static RoomStateSettings CrumbleShaft { get; } =
        new(nameof(CrumbleShaft), RoomLevelDefinitions.CrumbleShaft, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrumbleShaft, RoomEnemyPopulationDefinitions.CrumbleShaft, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrumbleShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernVertical, RoomSetupBehavior.None);


    /// <summary>The Crocomire Speedway room's state.</summary>
    public static RoomStateSettings CrocomireSpeedway { get; } =
        new(nameof(CrocomireSpeedway), RoomLevelDefinitions.CrocomireSpeedway, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSpeedway, RoomEnemyPopulationDefinitions.CrocomireSpeedway, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrocomireSpeedway, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrocomireSpeedway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurple, RoomSetupBehavior.None);


    /// <summary>The Crocomire room's state 0.</summary>
    public static RoomStateSettings CrocomireDefault { get; } =
        new(nameof(CrocomireDefault), RoomLevelDefinitions.Crocomire, RoomGraphicsSet.Crocomire, new RoomMusic(MusicDataSet.KraidAndPhantoonBattles, RoomMusicTrack.Plays(MusicTrack.KraidAndPhantoonBattle)), SuperMetroid.Core.Fx.RoomFxDefinitions.Crocomire, RoomEnemyPopulationDefinitions.Crocomire, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainBehavior.CrocomireRoomShaking, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Crocomire, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrocomireDefault, RoomSetupBehavior.None);


    /// <summary>The Crocomire room's state 1.</summary>
    public static RoomStateSettings CrocomireWhenAreaMiniBossDead { get; } =
        new(nameof(CrocomireWhenAreaMiniBossDead), RoomLevelDefinitions.Crocomire, RoomGraphicsSet.Crocomire, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Crocomire, RoomEnemyPopulationDefinitions.Crocomire, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Crocomire, null, RoomMainBehavior.CrocomireRoomShaking, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Crocomire, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.CrocomireWhenAreaMiniBossDead, RoomSetupBehavior.None);


    /// <summary>The Hi Jump Boots room's state.</summary>
    public static RoomStateSettings HiJumpBoots { get; } =
        new(nameof(HiJumpBoots), RoomLevelDefinitions.HiJumpBoots, RoomGraphicsSet.UnheatedNorfair, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpBoots, RoomEnemyPopulationDefinitions.HiJumpBoots, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.HiJumpBoots, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurpleBright, RoomSetupBehavior.None);


    /// <summary>The Crocomire Escape room's state.</summary>
    public static RoomStateSettings CrocomireEscape { get; } =
        new(nameof(CrocomireEscape), RoomLevelDefinitions.CrocomireEscape, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireEscape, RoomEnemyPopulationDefinitions.CrocomireEscape, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.CrocomireEscape, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrocomireEscape, null, RoomSetupBehavior.None);


    /// <summary>The Hi Jump E Tank room's state.</summary>
    public static RoomStateSettings HiJumpETank { get; } =
        new(nameof(HiJumpETank), RoomLevelDefinitions.HiJumpETank, RoomGraphicsSet.UnheatedNorfair, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.HiJumpETank, RoomEnemyPopulationDefinitions.HiJumpETank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.HiJumpETank, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.HiJumpETank, null, RoomSetupBehavior.None);


    /// <summary>The Post Croc Farming room's state.</summary>
    public static RoomStateSettings PostCrocFarming { get; } =
        new(nameof(PostCrocFarming), RoomLevelDefinitions.PostCrocFarming, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocFarming, RoomEnemyPopulationDefinitions.PostCrocFarming, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PostCrocFarming, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PostCrocFarming, null, RoomSetupBehavior.None);


    /// <summary>The Post Croc Save room's state.</summary>
    public static RoomStateSettings PostCrocSave { get; } =
        new(nameof(PostCrocSave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocSave, RoomEnemyPopulationDefinitions.PostCrocSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PostCrocSave, null, RoomSetupBehavior.None);


    /// <summary>The Post Croc Power Bombs room's state.</summary>
    public static RoomStateSettings PostCrocPowerBombs { get; } =
        new(nameof(PostCrocPowerBombs), RoomLevelDefinitions.PostCrocPowerBombs, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocPowerBombs, RoomEnemyPopulationDefinitions.PostCrocPowerBombs, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PostCrocPowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Post Croc Shaft room's state.</summary>
    public static RoomStateSettings PostCrocShaft { get; } =
        new(nameof(PostCrocShaft), RoomLevelDefinitions.PostCrocShaft, RoomGraphicsSet.UnheatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocShaft, RoomEnemyPopulationDefinitions.PostCrocShaft, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PostCrocShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PostCrocShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernVertical, RoomSetupBehavior.None);


    /// <summary>The Post Croc Missile room's state.</summary>
    public static RoomStateSettings PostCrocMissile { get; } =
        new(nameof(PostCrocMissile), RoomLevelDefinitions.PostCrocMissile, RoomGraphicsSet.UnheatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocMissile, RoomEnemyPopulationDefinitions.PostCrocMissile, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PostCrocMissile, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Grapple Tutorial room's state 3.</summary>
    public static RoomStateSettings GrappleTutorial3 { get; } =
        new(nameof(GrappleTutorial3), RoomLevelDefinitions.GrappleTutorial3, RoomGraphicsSet.UnheatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial3, RoomEnemyPopulationDefinitions.GrappleTutorial3, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GrappleTutorial3, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurple, RoomSetupBehavior.None);


    /// <summary>The Post Croc Jump room's state.</summary>
    public static RoomStateSettings PostCrocJump { get; } =
        new(nameof(PostCrocJump), RoomLevelDefinitions.PostCrocJump, RoomGraphicsSet.UnheatedNorfair, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.PostCrocJump, RoomEnemyPopulationDefinitions.PostCrocJump, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PostCrocJump, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PostCrocJump, null, RoomSetupBehavior.None);


    /// <summary>The Grapple Tutorial room's state 2.</summary>
    public static RoomStateSettings GrappleTutorial2 { get; } =
        new(nameof(GrappleTutorial2), RoomLevelDefinitions.GrappleTutorial2, RoomGraphicsSet.UnheatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial2, RoomEnemyPopulationDefinitions.GrappleTutorial2, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GrappleTutorial2, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GrappleTutorial2, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernVertical, RoomSetupBehavior.None);


    /// <summary>The Grapple Tutorial room's state 1.</summary>
    public static RoomStateSettings GrappleTutorial1 { get; } =
        new(nameof(GrappleTutorial1), RoomLevelDefinitions.GrappleTutorial1, RoomGraphicsSet.UnheatedNorfair, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleTutorial1, RoomEnemyPopulationDefinitions.GrappleTutorial1, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GrappleTutorial1, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Grapple Beam room's state.</summary>
    public static RoomStateSettings GrappleBeam { get; } =
        new(nameof(GrappleBeam), RoomLevelDefinitions.GrappleBeam, RoomGraphicsSet.UnheatedNorfair, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.GrappleBeam, RoomEnemyPopulationDefinitions.GrappleBeam, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GrappleBeam, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GrappleBeam, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernVertical, RoomSetupBehavior.None);


    /// <summary>The Norfair Reserve Tank room's state.</summary>
    public static RoomStateSettings NorfairReserveTank { get; } =
        new(nameof(NorfairReserveTank), RoomLevelDefinitions.NorfairReserveTank, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairReserveTank, RoomEnemyPopulationDefinitions.NorfairReserveTank, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.NorfairReserveTank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairPurplePatches, RoomSetupBehavior.None);


    /// <summary>The Green Bubbles Missiles room's state.</summary>
    public static RoomStateSettings GreenBubblesMissiles { get; } =
        new(nameof(GreenBubblesMissiles), RoomLevelDefinitions.GreenBubblesMissiles, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.GreenBubblesMissiles, RoomEnemyPopulationDefinitions.GreenBubblesMissiles, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GreenBubblesMissiles, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GreenBubblesMissiles, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairBubbles, RoomSetupBehavior.None);


    /// <summary>The Bubble Mountain room's state.</summary>
    public static RoomStateSettings BubbleMountain { get; } =
        new(nameof(BubbleMountain), RoomLevelDefinitions.BubbleMountain, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountain, RoomEnemyPopulationDefinitions.BubbleMountain, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BubbleMountain, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BubbleMountain, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairBubbles, RoomSetupBehavior.None);


    /// <summary>The Speed Booster Hall room's state.</summary>
    public static RoomStateSettings SpeedBoosterHall { get; } =
        new(nameof(SpeedBoosterHall), RoomLevelDefinitions.SpeedBoosterHall, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBoosterHall, RoomEnemyPopulationDefinitions.SpeedBoosterHall, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpeedBoosterHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurple, RoomSetupBehavior.None);


    /// <summary>The Speed Booster room's state.</summary>
    public static RoomStateSettings SpeedBooster { get; } =
        new(nameof(SpeedBooster), RoomLevelDefinitions.SpeedBooster, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.SpeedBooster, RoomEnemyPopulationDefinitions.SpeedBooster, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpeedBooster, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurpleBright, RoomSetupBehavior.None);


    /// <summary>The Single Chamber room's state.</summary>
    public static RoomStateSettings SingleChamber { get; } =
        new(nameof(SingleChamber), RoomLevelDefinitions.SingleChamber, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.SingleChamber, RoomEnemyPopulationDefinitions.SingleChamber, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.SingleChamber, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SingleChamber, null, RoomSetupBehavior.None);


    /// <summary>The Double Chamber room's state.</summary>
    public static RoomStateSettings DoubleChamber { get; } =
        new(nameof(DoubleChamber), RoomLevelDefinitions.DoubleChamber, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.DoubleChamber, RoomEnemyPopulationDefinitions.DoubleChamber, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.DoubleChamber, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.DoubleChamber, null, RoomSetupBehavior.None);


    /// <summary>The Wave Beam room's state.</summary>
    public static RoomStateSettings WaveBeam { get; } =
        new(nameof(WaveBeam), RoomLevelDefinitions.WaveBeam, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.WaveBeam, RoomEnemyPopulationDefinitions.WaveBeam, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WaveBeam, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Spiky Platforms Tunnel room's state.</summary>
    public static RoomStateSettings SpikyPlatformsTunnel { get; } =
        new(nameof(SpikyPlatformsTunnel), RoomLevelDefinitions.SpikyPlatformsTunnel, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyPlatformsTunnel, RoomEnemyPopulationDefinitions.SpikyPlatformsTunnel, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpikyPlatformsTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Volcano room's state.</summary>
    public static RoomStateSettings Volcano { get; } =
        new(nameof(Volcano), RoomLevelDefinitions.Volcano, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Volcano, RoomEnemyPopulationDefinitions.Volcano, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Volcano, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Volcano, null, RoomSetupBehavior.None);


    /// <summary>The Kronic Boost room's state.</summary>
    public static RoomStateSettings KronicBoost { get; } =
        new(nameof(KronicBoost), RoomLevelDefinitions.KronicBoost, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.KronicBoost, RoomEnemyPopulationDefinitions.KronicBoost, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.KronicBoost, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.KronicBoost, null, RoomSetupBehavior.None);


    /// <summary>The Magdollite Tunnel room's state.</summary>
    public static RoomStateSettings MagdolliteTunnel { get; } =
        new(nameof(MagdolliteTunnel), RoomLevelDefinitions.MagdolliteTunnel, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MagdolliteTunnel, RoomEnemyPopulationDefinitions.MagdolliteTunnel, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MagdolliteTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Purple Shaft room's state.</summary>
    public static RoomStateSettings PurpleShaft { get; } =
        new(nameof(PurpleShaft), RoomLevelDefinitions.PurpleShaft, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleShaft, RoomEnemyPopulationDefinitions.PurpleShaft, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.PurpleShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PurpleShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairPurplePatches, RoomSetupBehavior.None);


    /// <summary>The Lava Dive room's state.</summary>
    public static RoomStateSettings LavaDive { get; } =
        new(nameof(LavaDive), RoomLevelDefinitions.LavaDive, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.LavaDive, RoomEnemyPopulationDefinitions.LavaDive, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LavaDive, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurple, RoomSetupBehavior.None);


    /// <summary>The Lower Norfair Elev room's state.</summary>
    public static RoomStateSettings LowerNorfairElev { get; } =
        new(nameof(LowerNorfairElev), RoomLevelDefinitions.LowerNorfairElev, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.LowerNorfairElev, RoomEnemyPopulationDefinitions.LowerNorfairElev, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LowerNorfairElev, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LowerNorfairElev, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Upper Norfair Farming room's state.</summary>
    public static RoomStateSettings UpperNorfairFarming { get; } =
        new(nameof(UpperNorfairFarming), RoomLevelDefinitions.UpperNorfairFarming, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairFarming, RoomEnemyPopulationDefinitions.UpperNorfairFarming, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.UpperNorfairFarming, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.UpperNorfairFarming, null, RoomSetupBehavior.None);


    /// <summary>The Rising Tide room's state.</summary>
    public static RoomStateSettings RisingTide { get; } =
        new(nameof(RisingTide), RoomLevelDefinitions.RisingTide, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RisingTide, RoomEnemyPopulationDefinitions.RisingTide, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RisingTide, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Acid Snakes Tunnel room's state.</summary>
    public static RoomStateSettings AcidSnakesTunnel { get; } =
        new(nameof(AcidSnakesTunnel), RoomLevelDefinitions.AcidSnakesTunnel, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.AcidSnakesTunnel, RoomEnemyPopulationDefinitions.AcidSnakesTunnel, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AcidSnakesTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Spiky Acid Snakes Tunnel room's state.</summary>
    public static RoomStateSettings SpikyAcidSnakesTunnel { get; } =
        new(nameof(SpikyAcidSnakesTunnel), RoomLevelDefinitions.SpikyAcidSnakesTunnel, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.SpikyAcidSnakesTunnel, RoomEnemyPopulationDefinitions.SpikyAcidSnakesTunnel, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.SpikyAcidSnakesTunnel, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernStalactites, RoomSetupBehavior.None);


    /// <summary>The Upper Norfair Refill room's state.</summary>
    public static RoomStateSettings UpperNorfairRefill { get; } =
        new(nameof(UpperNorfairRefill), RoomLevelDefinitions.UpperNorfairRefill, RoomGraphicsSet.BlueRefill, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.UpperNorfairRefill, RoomEnemyPopulationDefinitions.UpperNorfairRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.UpperNorfairRefill, null, RoomSetupBehavior.None);


    /// <summary>The Purple Farming room's state.</summary>
    public static RoomStateSettings PurpleFarming { get; } =
        new(nameof(PurpleFarming), RoomLevelDefinitions.PurpleFarming, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.PurpleFarming, RoomEnemyPopulationDefinitions.PurpleFarming, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.PurpleFarming, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairPurplePatches, RoomSetupBehavior.None);


    /// <summary>The Bat Cave room's state.</summary>
    public static RoomStateSettings BatCave { get; } =
        new(nameof(BatCave), RoomLevelDefinitions.BatCave, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.BatCave, RoomEnemyPopulationDefinitions.BatCave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.BatCave, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BatCave, null, RoomSetupBehavior.None);


    /// <summary>The Norfair Map room's state.</summary>
    public static RoomStateSettings NorfairMap { get; } =
        new(nameof(NorfairMap), RoomLevelDefinitions.GreenBrinstarMap, RoomGraphicsSet.MapStatues, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.NorfairMap, RoomEnemyPopulationDefinitions.NorfairMap, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.NorfairMap, null, RoomSetupBehavior.None);


    /// <summary>The Bubble Mountain Save room's state.</summary>
    public static RoomStateSettings BubbleMountainSave { get; } =
        new(nameof(BubbleMountainSave), RoomLevelDefinitions.CrateriaSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.BubbleMountainSave, RoomEnemyPopulationDefinitions.BubbleMountainSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.BubbleMountainSave, null, RoomSetupBehavior.None);


    /// <summary>The Frog Speedway room's state.</summary>
    public static RoomStateSettings FrogSpeedway { get; } =
        new(nameof(FrogSpeedway), RoomLevelDefinitions.FrogSpeedway, RoomGraphicsSet.UnheatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSpeedway, RoomEnemyPopulationDefinitions.FrogSpeedway, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FrogSpeedway, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FrogSpeedway, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurpleBright, RoomSetupBehavior.None);


    /// <summary>The Red Pirate Shaft room's state.</summary>
    public static RoomStateSettings RedPirateShaft { get; } =
        new(nameof(RedPirateShaft), RoomLevelDefinitions.RedPirateShaft, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RedPirateShaft, RoomEnemyPopulationDefinitions.RedPirateShaft, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedPirateShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedPirateShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernVertical, RoomSetupBehavior.None);


    /// <summary>The Frog Save room's state.</summary>
    public static RoomStateSettings FrogSave { get; } =
        new(nameof(FrogSave), RoomLevelDefinitions.FrogSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.FrogSave, RoomEnemyPopulationDefinitions.FrogSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FrogSave, null, RoomSetupBehavior.None);


    /// <summary>The Crocomire Save room's state.</summary>
    public static RoomStateSettings CrocomireSave { get; } =
        new(nameof(CrocomireSave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.CrocomireSave, RoomEnemyPopulationDefinitions.CrocomireSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.CrocomireSave, null, RoomSetupBehavior.None);


    /// <summary>The LN Elev Save room's state.</summary>
    public static RoomStateSettings LNElevSave { get; } =
        new(nameof(LNElevSave), RoomLevelDefinitions.CrateriaSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.UpperNorfair, RoomMusicTrack.Plays(MusicTrack.UpperNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.LNElevSave, RoomEnemyPopulationDefinitions.LNElevSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LNElevSave, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LNElevSave, null, RoomSetupBehavior.None);


    /// <summary>The Acid Statue room's state.</summary>
    public static RoomStateSettings AcidStatue { get; } =
        new(nameof(AcidStatue), RoomLevelDefinitions.AcidStatue, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(MusicTrack.LowerNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.AcidStatue, RoomEnemyPopulationDefinitions.AcidStatue, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.AcidStatue, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.AcidStatue, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernRoomPillars, RoomSetupBehavior.None);


    /// <summary>The Main Hall room's state.</summary>
    public static RoomStateSettings MainHall { get; } =
        new(nameof(MainHall), RoomLevelDefinitions.MainHall, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(MusicTrack.LowerNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.MainHall, RoomEnemyPopulationDefinitions.MainHall, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MainHall, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MainHall, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernVerticalRuins, RoomSetupBehavior.None);


    /// <summary>The Golden Torizo room's state 0.</summary>
    public static RoomStateSettings GoldenTorizoDefault { get; } =
        new(nameof(GoldenTorizoDefault), RoomLevelDefinitions.GoldenTorizo, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.RidleyAndDraygonBattles, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizo, RoomEnemyPopulationDefinitions.GoldenTorizo, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GoldenTorizo, null, RoomSetupBehavior.None);


    /// <summary>The Golden Torizo room's state 1.</summary>
    public static RoomStateSettings GoldenTorizoWhenAreaTorizoDead { get; } =
        new(nameof(GoldenTorizoWhenAreaTorizoDead), RoomLevelDefinitions.GoldenTorizo, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.GoldenTorizo, RoomEnemyPopulationDefinitions.GoldenTorizo, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.GoldenTorizo, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GoldenTorizo, null, RoomSetupBehavior.None);


    /// <summary>The Fast Ripper room's state.</summary>
    public static RoomStateSettings FastRipper { get; } =
        new(nameof(FastRipper), RoomLevelDefinitions.FastRipper, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(MusicTrack.LowerNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.FastRipper, RoomEnemyPopulationDefinitions.FastRipper, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FastRipper, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernVerticalRuins, RoomSetupBehavior.None);


    /// <summary>The GT Energy Refill room's state.</summary>
    public static RoomStateSettings GTEnergyRefill { get; } =
        new(nameof(GTEnergyRefill), RoomLevelDefinitions.GTEnergyRefill, RoomGraphicsSet.BlueRefill, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.GTEnergyRefill, RoomEnemyPopulationDefinitions.GTEnergyRefill, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.GTEnergyRefill, null, RoomSetupBehavior.None);


    /// <summary>The Ridley room's state 0.</summary>
    public static RoomStateSettings RidleyDefault { get; } =
        new(nameof(RidleyDefault), RoomLevelDefinitions.Ridley, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.RidleyAndDraygonBattles, RoomMusicTrack.Plays(MusicTrack.StatueHall)), SuperMetroid.Core.Fx.RoomFxDefinitions.Ridley, RoomEnemyPopulationDefinitions.Ridley, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainBehavior.RidleyRoomShaking, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Ridley, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernRoomPillars, RoomSetupBehavior.None);


    /// <summary>The Ridley room's state 1.</summary>
    public static RoomStateSettings RidleyWhenAreaBossDead { get; } =
        new(nameof(RidleyWhenAreaBossDead), RoomLevelDefinitions.Ridley, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.Ridley, RoomEnemyPopulationDefinitions.Ridley, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Ridley, null, RoomMainBehavior.RidleyRoomShaking, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Ridley, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernRoomPillars, RoomSetupBehavior.None);


    /// <summary>The LN Farming room's state.</summary>
    public static RoomStateSettings LNFarming { get; } =
        new(nameof(LNFarming), RoomLevelDefinitions.LNFarming, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(MusicTrack.LowerNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.LNFarming, RoomEnemyPopulationDefinitions.LNFarming, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LNFarming, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairRoomStatues, RoomSetupBehavior.None);


    /// <summary>The Fast Pillars Setup room's state.</summary>
    public static RoomStateSettings FastPillarsSetup { get; } =
        new(nameof(FastPillarsSetup), RoomLevelDefinitions.FastPillarsSetup, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.FastPillarsSetup, RoomEnemyPopulationDefinitions.FastPillarsSetup, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.FastPillarsSetup, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.FastPillarsSetup, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairVerticalPatternBrick, RoomSetupBehavior.None);


    /// <summary>The Mickey Mouse room's state.</summary>
    public static RoomStateSettings MickeyMouse { get; } =
        new(nameof(MickeyMouse), RoomLevelDefinitions.MickeyMouse, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MickeyMouse, RoomEnemyPopulationDefinitions.MickeyMouse, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.MickeyMouse, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MickeyMouse, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairPurplePatches, RoomSetupBehavior.None);


    /// <summary>The Pillar room's state.</summary>
    public static RoomStateSettings Pillar { get; } =
        new(nameof(Pillar), RoomLevelDefinitions.Pillar, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Pillar, RoomEnemyPopulationDefinitions.Pillar, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Pillar, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernVerticalRuins, RoomSetupBehavior.None);


    /// <summary>The Plowerhouse room's state.</summary>
    public static RoomStateSettings Plowerhouse { get; } =
        new(nameof(Plowerhouse), RoomLevelDefinitions.Plowerhouse, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Plowerhouse, RoomEnemyPopulationDefinitions.Plowerhouse, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Plowerhouse, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairRoomStatues, RoomSetupBehavior.None);


    /// <summary>The Worst Room In The Game room's state.</summary>
    public static RoomStateSettings WorstRoomInTheGame { get; } =
        new(nameof(WorstRoomInTheGame), RoomLevelDefinitions.WorstRoomInTheGame, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.WorstRoomInTheGame, RoomEnemyPopulationDefinitions.WorstRoomInTheGame, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.WorstRoomInTheGame, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.WorstRoomInTheGame, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernHorizontalRuins, RoomSetupBehavior.None);


    /// <summary>The Amphitheatre room's state.</summary>
    public static RoomStateSettings Amphitheatre { get; } =
        new(nameof(Amphitheatre), RoomLevelDefinitions.Amphitheatre, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Amphitheatre, RoomEnemyPopulationDefinitions.Amphitheatre, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Amphitheatre, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurple, RoomSetupBehavior.None);


    /// <summary>The LN Spring Ball Maze room's state.</summary>
    public static RoomStateSettings LNSpringBallMaze { get; } =
        new(nameof(LNSpringBallMaze), RoomLevelDefinitions.LNSpringBallMaze, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.PlaysAreaTheme), SuperMetroid.Core.Fx.RoomFxDefinitions.LNSpringballMaze, RoomEnemyPopulationDefinitions.LNSpringBallMaze, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LNSpringBallMaze, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LNSpringBallMaze, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairPurplePatches, RoomSetupBehavior.None);


    /// <summary>The LN Escape Power Bombs room's state.</summary>
    public static RoomStateSettings LNEscapePowerBombs { get; } =
        new(nameof(LNEscapePowerBombs), RoomLevelDefinitions.LNEscapePowerBombs, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.LNEscapePowerBombs, RoomEnemyPopulationDefinitions.LNEscapePowerBombs, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LNEscapePowerBombs, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernRoomPillars, RoomSetupBehavior.None);


    /// <summary>The Red Kihunter Shaft room's state.</summary>
    public static RoomStateSettings RedKihunterShaft { get; } =
        new(nameof(RedKihunterShaft), RoomLevelDefinitions.RedKihunterShaft, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RedKihunterShaft, RoomEnemyPopulationDefinitions.RedKihunterShaft, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.RedKihunterShaft, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RedKihunterShaft, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairSmallPatternBrownPurple, RoomSetupBehavior.None);


    /// <summary>The Wasteland room's state.</summary>
    public static RoomStateSettings Wasteland { get; } =
        new(nameof(Wasteland), RoomLevelDefinitions.Wasteland, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.Wasteland, RoomEnemyPopulationDefinitions.Wasteland, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Wasteland, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.Wasteland, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernRoomPillars, RoomSetupBehavior.None);


    /// <summary>The Metal Pirates room's state.</summary>
    public static RoomStateSettings MetalPirates { get; } =
        new(nameof(MetalPirates), RoomLevelDefinitions.MetalPirates, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.MetalPirates, RoomEnemyPopulationDefinitions.MetalPirates, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.MetalPirates, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairRoomStatues, RoomSetupBehavior.None);


    /// <summary>The Three Musketeers room's state.</summary>
    public static RoomStateSettings ThreeMusketeers { get; } =
        new(nameof(ThreeMusketeers), RoomLevelDefinitions.ThreeMusketeers, RoomGraphicsSet.HeatedNorfair, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(MusicTrack.LowerNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.ThreeMusketeers, RoomEnemyPopulationDefinitions.ThreeMusketeers, new BackgroundScrollModes(BackgroundAxisScroll.Parallax(192), BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.ThreeMusketeers, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ThreeMusketeers, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairPurplePatches, RoomSetupBehavior.None);


    /// <summary>The Ridley E Tank room's state.</summary>
    public static RoomStateSettings RidleyETank { get; } =
        new(nameof(RidleyETank), RoomLevelDefinitions.RidleyETank, RoomGraphicsSet.HeatedNorfair, RoomMusic.Inherited, SuperMetroid.Core.Fx.RoomFxDefinitions.RidleyETank, RoomEnemyPopulationDefinitions.RidleyETank, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.RidleyETank, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernRoomPillars, RoomSetupBehavior.None);


    /// <summary>The Screw Attack room's state.</summary>
    public static RoomStateSettings ScrewAttack { get; } =
        new(nameof(ScrewAttack), RoomLevelDefinitions.ScrewAttack, RoomGraphicsSet.HeatedNorfair, new RoomMusic(null, RoomMusicTrack.Plays(MusicTrack.Elevator)), SuperMetroid.Core.Fx.RoomFxDefinitions.ScrewAttack, RoomEnemyPopulationDefinitions.ScrewAttack, new BackgroundScrollModes(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Parallax(192), RoomBackgroundSource.Placed), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.ScrewAttack, SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundDefinitions.NorfairCavernRoomPillars, RoomSetupBehavior.None);


    /// <summary>The LN Fireflea room's state.</summary>
    public static RoomStateSettings LNFireflea { get; } =
        new(nameof(LNFireflea), RoomLevelDefinitions.LNFireflea, RoomGraphicsSet.UnheatedNorfair, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(MusicTrack.LowerNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.LNFireflea, RoomEnemyPopulationDefinitions.LNFireflea, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.LNFireflea, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LNFireflea, null, RoomSetupBehavior.None);


    /// <summary>The LN Save room's state.</summary>
    public static RoomStateSettings LNSave { get; } =
        new(nameof(LNSave), RoomLevelDefinitions.WarehouseSave, RoomGraphicsSet.SaveStation, new RoomMusic(MusicDataSet.LowerNorfair, RoomMusicTrack.Plays(MusicTrack.LowerNorfair)), SuperMetroid.Core.Fx.RoomFxDefinitions.LNSave, RoomEnemyPopulationDefinitions.LNSave, new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level), SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, RoomMainBehavior.None, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulationDefinitions.LNSave, null, RoomSetupBehavior.None);
}
