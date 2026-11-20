using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The enemy placement lists of the Norfair rooms.</summary>
[NumberedIdentities("Each numbered member is the enemy population of a room whose own name carries the number (Grapple Tutorial Room 1-3, Metroid Room 1-4, Tourian Escape Room 1-4); nothing indexes the populations by number.")]
internal static class NorfairRoomEnemyPopulationDefinitions
{
    /// <summary>The enemy population of the Grapple Tutorial 3 room.</summary>
    internal static RoomEnemyPopulationDefinition GrappleTutorial3 { get; } = new("GrappleTutorial3",
    [
        new(EnemyDefinitionId.Gamet, new(208, 360), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugLeaderSpawnSettings(TriggerWidth: 48, Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(208, 360), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(208, 360), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(208, 360), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(208, 360), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Puyo, new(461, 332), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new PuyoSpawnSettings(HopCooldown: 0, HopDistance: 0)),
        new(EnemyDefinitionId.Puyo, new(442, 276), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new PuyoSpawnSettings(HopCooldown: 0, HopDistance: 0)),
        new(EnemyDefinitionId.Puyo, new(520, 418), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new PuyoSpawnSettings(HopCooldown: 0, HopDistance: 0)),
        new(EnemyDefinitionId.Puyo, new(572, 423), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new PuyoSpawnSettings(HopCooldown: 0, HopDistance: 0)),
        new(EnemyDefinitionId.Puyo, new(624, 417), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new PuyoSpawnSettings(HopCooldown: 0, HopDistance: 0)),
    ], 6);

    /// <summary>The enemy population of the Post-Crocomire Shaft room.</summary>
    internal static RoomEnemyPopulationDefinition PostCrocomireShaft { get; } = new("PostCrocomireShaft",
    [
        new(EnemyDefinitionId.Viola, new(119, 840), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(96, 936), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(147, 1016), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(125, 1144), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
    ], 4);

    /// <summary>The enemy population of the Metal Pirates room.</summary>
    internal static RoomEnemyPopulationDefinition MetalPirates { get; } = new("MetalPirates",
    [
        new(EnemyDefinitionId.PirateGoldNinja, new(232, 176), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, new NinjaSpacePirateSpawnSettings(StartPost: NinjaSpacePirateStartPost.Left, PostDistance: 192)),
        new(EnemyDefinitionId.PirateGoldNinja, new(536, 176), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, new NinjaSpacePirateSpawnSettings(StartPost: NinjaSpacePirateStartPost.Right, PostDistance: 192)),
    ], 2);

    /// <summary>The enemy population of the Acid Statue room.</summary>
    internal static RoomEnemyPopulationDefinition AcidStatue { get; } = new("AcidStatue",
    [
        new(EnemyDefinitionId.Chozo, new(44, 154), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChozoStatueSpawnSettings(Site: ChozoStatueSite.LowerNorfair)),
        new(EnemyDefinitionId.Magdollite, new(472, 728), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteHeadSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(2), Range: 240)),
        new(EnemyDefinitionId.Magdollite, new(472, 728), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteBodySpawnSettings()),
        new(EnemyDefinitionId.Magdollite, new(472, 728), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteOverlaySpawnSettings()),
        new(EnemyDefinitionId.Holtz, new(512, 616), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Holtz, new(512, 616), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Holtz, new(432, 592), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Holtz, new(432, 592), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Holtz, new(596, 589), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Holtz, new(596, 589), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
    ], 4);

    /// <summary>The enemy population of the Purple Farming room.</summary>
    internal static RoomEnemyPopulationDefinition PurpleFarming { get; } = new("PurpleFarming",
    [
        new(EnemyDefinitionId.Gamet, new(128, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugLeaderSpawnSettings(TriggerWidth: 48, Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(128, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(128, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(128, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(128, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
    ], 1);

    /// <summary>The enemy population of the Ice Beam Gate room.</summary>
    internal static RoomEnemyPopulationDefinition IceBeamGate { get; } = new("IceBeamGate",
    [
        new(EnemyDefinitionId.ShutterGrowing, new(1032, 600), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrowingShutterSpawnSettings(Wait: GrowingShutterWait.SamusNear, Growth: VerticalDirection.Down, ActivationDistance: 240, ActivationWait: 0, GrowthSpeed: SignedSixteenSixteen.FromPixelFraction(7, 8))),
        new(EnemyDefinitionId.ShutterGrowing, new(1544, 600), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrowingShutterSpawnSettings(Wait: GrowingShutterWait.SamusNear, Growth: VerticalDirection.Down, ActivationDistance: 104, ActivationWait: 0, GrowthSpeed: SignedSixteenSixteen.FromPixelFraction(17, 16))),
        new(EnemyDefinitionId.Dessgeega, new(260, 943), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Dessgeega, new(405, 863), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.Dessgeega, new(546, 946), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Mella, new(313, 884), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mella, new(406, 935), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mella, new(508, 881), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mella, new(596, 889), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mella, new(641, 926), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mella, new(176, 866), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Sova, new(1480, 568), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixels(1))),
    ], 4);

    /// <summary>The enemy population of the Spiky Acid Snakes Tunnel room.</summary>
    internal static RoomEnemyPopulationDefinition SpikyAcidSnakesTunnel { get; } = new("SpikyAcidSnakesTunnel",
    [
        new(EnemyDefinitionId.YappingMaw, new(392, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 128, Attack: VerticalDirection.Up)),
        new(EnemyDefinitionId.YappingMaw, new(537, 215), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 128, Attack: VerticalDirection.Up)),
        new(EnemyDefinitionId.YappingMaw, new(760, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 128, Attack: VerticalDirection.Up)),
    ], 0);

    /// <summary>The enemy population of the Red Kihunter Shaft room.</summary>
    internal static RoomEnemyPopulationDefinition RedKihunterShaft { get; } = new("RedKihunterShaft",
    [
        new(EnemyDefinitionId.KihunterRed, new(185, 470), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterRedWings, new(185, 470), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterRed, new(77, 570), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterRedWings, new(77, 570), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterRed, new(135, 754), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterRedWings, new(135, 754), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
    ], 3);

    /// <summary>The enemy population of the Fast Ripper room.</summary>
    internal static RoomEnemyPopulationDefinition FastRipper { get; } = new("FastRipper",
    [
        new(EnemyDefinitionId.JetRipper, new(472, 120), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.JetRipper, new(472, 88), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.JetRipper, new(616, 104), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.JetRipper, new(616, 136), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.JetRipper, new(616, 72), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.JetRipper, new(344, 152), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4), Direction: HorizontalDirection.Left)),
    ], 6);

    /// <summary>The enemy population of the Wave Beam room.</summary>
    internal static RoomEnemyPopulationDefinition WaveBeam { get; } = new("WaveBeam",
    [
    ], 0);

    /// <summary>The enemy population of the Fast Pillars Setup room.</summary>
    internal static RoomEnemyPopulationDefinition FastPillarsSetup { get; } = new("FastPillarsSetup",
    [
        new(EnemyDefinitionId.PirateGoldWall, new(209, 312), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGoldWall, new(45, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGoldWalking, new(176, 384), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 160)),
        new(EnemyDefinitionId.Viola, new(120, 567), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Viola, new(136, 567), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
    ], 5);

    /// <summary>The enemy population of the Lower Norfair Elevator room.</summary>
    internal static RoomEnemyPopulationDefinition LowerNorfairElevator { get; } = new("LowerNorfairElevator",
    [
        new(EnemyDefinitionId.Elevator, new(128, 160), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Down, ArrivalY: 320)),
    ], 0);

    /// <summary>The enemy population of the Lower Norfair Save room.</summary>
    internal static RoomEnemyPopulationDefinition LowerNorfairSave { get; } = new("LowerNorfairSave",
    [
    ], 0);

    /// <summary>The enemy population of the Upper Norfair Refill room.</summary>
    internal static RoomEnemyPopulationDefinition UpperNorfairRefill { get; } = new("UpperNorfairRefill",
    [
    ], 0);

    /// <summary>The enemy population of the Norfair Map room.</summary>
    internal static RoomEnemyPopulationDefinition NorfairMap { get; } = new("NorfairMap",
    [
    ], 0);

    /// <summary>The enemy population of the Mickey Mouse room.</summary>
    internal static RoomEnemyPopulationDefinition MickeyMouse { get; } = new("MickeyMouse",
    [
        new(EnemyDefinitionId.DessgeegaLarge, new(960, 907), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.DessgeegaLarge, new(855, 906), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.DessgeegaLarge, new(433, 907), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.DessgeegaLarge, new(261, 896), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.DessgeegaLarge, new(618, 897), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.Multiviola, new(149, 887), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(14), Speed: 6)),
        new(EnemyDefinitionId.Multiviola, new(353, 912), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(168), Speed: 6)),
        new(EnemyDefinitionId.Multiviola, new(918, 790), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(0), Speed: 7)),
        new(EnemyDefinitionId.Multiviola, new(841, 763), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(80), Speed: 7)),
        new(EnemyDefinitionId.Multiviola, new(883, 681), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(200), Speed: 6)),
        new(EnemyDefinitionId.Multiviola, new(856, 568), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(240), Speed: 6)),
        new(EnemyDefinitionId.Multiviola, new(971, 532), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(72), Speed: 5)),
    ], 5);

    /// <summary>The enemy population of the Golden Torizo Energy Refill room.</summary>
    internal static RoomEnemyPopulationDefinition GoldenTorizoEnergyRefill { get; } = new("GoldenTorizoEnergyRefill",
    [
    ], 0);

    /// <summary>The enemy population of the Ridley room.</summary>
    internal static RoomEnemyPopulationDefinition Ridley { get; } = new("Ridley",
    [
        new(EnemyDefinitionId.Ridley, OffRoomPlaces.Placed(new Offset16<RoomPixels>(48, -16)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Post-Crocomire Power Bombs room.</summary>
    internal static RoomEnemyPopulationDefinition PostCrocomirePowerBombs { get; } = new("PostCrocomirePowerBombs",
    [
    ], 0);

    /// <summary>The enemy population of the Ice Beam room.</summary>
    internal static RoomEnemyPopulationDefinition IceBeam { get; } = new("IceBeam",
    [
    ], 0);

    /// <summary>The enemy population of the Hi-Jump Boots room.</summary>
    internal static RoomEnemyPopulationDefinition HiJumpBoots { get; } = new("HiJumpBoots",
    [
    ], 0);

    /// <summary>The enemy population of the Post-Crocomire Save room.</summary>
    internal static RoomEnemyPopulationDefinition PostCrocomireSave { get; } = new("PostCrocomireSave",
    [
    ], 0);

    /// <summary>The enemy population of the Frog Speedway room.</summary>
    internal static RoomEnemyPopulationDefinition FrogSpeedway { get; } = new("FrogSpeedway",
    [
        new(EnemyDefinitionId.Beetom, new(696, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, new(744, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, new(784, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, new(912, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, new(1024, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, new(1120, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 6);

    /// <summary>The enemy population of the Rising Tide room.</summary>
    internal static RoomEnemyPopulationDefinition RisingTide { get; } = new("RisingTide",
    [
        new(EnemyDefinitionId.Squeept, new(232, 240), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Squeept, new(232, 240), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Sova, new(488, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(1000, 168), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sova, new(376, 160), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sova, new(712, 171), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnRightWall, BlockSide: CrawlerSurfaceOrientation.OnCeiling, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(648, 168), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnFloor, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Dragon, new(568, 240), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(568, 240), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(824, 240), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(824, 240), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(1128, 240), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(1128, 240), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(952, 240), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(952, 240), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Squeept, new(360, 240), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Squeept, new(360, 240), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
    ], 11);

    /// <summary>The enemy population of the Post-Crocomire Farming room.</summary>
    internal static RoomEnemyPopulationDefinition PostCrocomireFarming { get; } = new("PostCrocomireFarming",
    [
        new(EnemyDefinitionId.Gamet, new(304, 456), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugLeaderSpawnSettings(TriggerWidth: 48, Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(304, 456), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(304, 456), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(304, 456), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(304, 456), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.GrappleRipper, new(256, 80), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrappleRipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Right, MinimumX: 232, MaximumX: 304)),
        new(EnemyDefinitionId.SuspensorPlatform, new(344, 328), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 40)),
    ], 2);

    /// <summary>The enemy population of the Worst Room in the Game.</summary>
    internal static RoomEnemyPopulationDefinition WorstRoomInTheGame { get; } = new("WorstRoomInTheGame",
    [
        new(EnemyDefinitionId.Namihe, new(218, 1399), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Left, LaunchSpeed: SignedEightEight.FromPixels(2), YProximity: 64)),
        new(EnemyDefinitionId.PirateGoldWall, new(208, 1272), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGoldWall, new(48, 1168), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGoldWall, new(208, 1088), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 160)),
        new(EnemyDefinitionId.Namihe, new(39, 800), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Right, LaunchSpeed: SignedEightEight.FromPixels(2), YProximity: 64)),
        new(EnemyDefinitionId.PirateGoldWalking, new(48, 688), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 1)),
        new(EnemyDefinitionId.Namihe, new(218, 904), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Left, LaunchSpeed: SignedEightEight.FromPixels(2), YProximity: 64)),
        new(EnemyDefinitionId.PirateGoldWalking, new(208, 736), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 0)),
        new(EnemyDefinitionId.Namihe, new(218, 576), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Left, LaunchSpeed: SignedEightEight.FromPixels(2), YProximity: 64)),
        new(EnemyDefinitionId.Namihe, new(39, 480), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Right, LaunchSpeed: SignedEightEight.FromPixels(2), YProximity: 64)),
        new(EnemyDefinitionId.PirateGoldWalking, new(128, 368), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
    ], 11);

    /// <summary>The enemy population of the Double Chamber room.</summary>
    internal static RoomEnemyPopulationDefinition DoubleChamber { get; } = new("DoubleChamber",
    [
        new(EnemyDefinitionId.Fune, new(48, 208), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Right, CooldownTime: 80, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(448, 216), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 80, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Kamer, new(136, 168), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.HorizontalProximity, TravelDistance: 64, ProximityDistance: 32, WaitTime: 0, RestAfterMovingUp: ShutterRest.For(64), RestAfterMovingDown: ShutterRest.For(64))),
        new(EnemyDefinitionId.Kamer, new(208, 232), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, TriggerMode: ShutterTriggerMode.HorizontalProximity, TravelDistance: 48, ProximityDistance: 48, WaitTime: 0, RestAfterMovingUp: ShutterRest.For(128), RestAfterMovingDown: ShutterRest.For(128))),
        new(EnemyDefinitionId.Kamer, new(272, 168), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.HorizontalProximity, TravelDistance: 96, ProximityDistance: 48, WaitTime: 0, RestAfterMovingUp: ShutterRest.For(32), RestAfterMovingDown: ShutterRest.For(32))),
        new(EnemyDefinitionId.Kamer, new(376, 168), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.HorizontalProximity, TravelDistance: 80, ProximityDistance: 48, WaitTime: 0, RestAfterMovingUp: ShutterRest.For(48), RestAfterMovingDown: ShutterRest.For(48))),
        new(EnemyDefinitionId.Kago, new(752, 464), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KagoSpawnSettings(HitCount: 10)),
        new(EnemyDefinitionId.GrappleRipper, new(744, 240), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new GrappleRipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Right, MinimumX: 664, MaximumX: 824)),
    ], 4);

    /// <summary>The enemy population of the Grapple Beam room.</summary>
    internal static RoomEnemyPopulationDefinition GrappleBeam { get; } = new("GrappleBeam",
    [
    ], 0);

    /// <summary>The enemy population of the Red Pirate Shaft room.</summary>
    internal static RoomEnemyPopulationDefinition RedPirateShaft { get; } = new("RedPirateShaft",
    [
        new(EnemyDefinitionId.PirateRedWalking, new(80, 512), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 8)),
        new(EnemyDefinitionId.PirateRedWalking, new(176, 384), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 8)),
        new(EnemyDefinitionId.PirateRedWalking, new(80, 256), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 8)),
        new(EnemyDefinitionId.ArmoredZoomer, new(128, 616), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.ArmoredZoomer, new(128, 472), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.ArmoredZoomer, new(128, 344), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.ArmoredZoomer, new(128, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
    ], 7);

    /// <summary>The enemy population of the Crocomire Speedway room.</summary>
    internal static RoomEnemyPopulationDefinition CrocomireSpeedway { get; } = new("CrocomireSpeedway",
    [
        new(EnemyDefinitionId.PirateRedWalking, new(2232, 624), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 0)),
        new(EnemyDefinitionId.Multiviola, new(1043, 204), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(240), Speed: 3)),
        new(EnemyDefinitionId.PirateRedWalking, new(756, 176), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 48)),
        new(EnemyDefinitionId.Multiviola, new(1165, 278), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(48), Speed: 3)),
        new(EnemyDefinitionId.PirateRedWalking, new(922, 224), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 32)),
        new(EnemyDefinitionId.Multiviola, new(1292, 344), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(104), Speed: 3)),
        new(EnemyDefinitionId.Multiviola, new(1455, 456), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(208), Speed: 3)),
        new(EnemyDefinitionId.PirateRedWalking, new(2555, 624), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 0)),
        new(EnemyDefinitionId.PirateRedWalking, new(1767, 656), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 16)),
        new(EnemyDefinitionId.Cacatac, new(3160, 483), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Left, Start: CacatacStart.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 16, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Cacatac, new(3240, 483), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Left, Start: CacatacStart.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 16, Speed: SignedSixteenSixteen.Zero)),
    ], 7);

    /// <summary>The enemy population of the Plowerhouse room.</summary>
    internal static RoomEnemyPopulationDefinition Plowerhouse { get; } = new("Plowerhouse",
    [
        new(EnemyDefinitionId.Holtz, new(600, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Holtz, new(600, 88), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Holtz, new(648, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Holtz, new(648, 88), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Holtz, new(280, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Holtz, new(280, 88), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Holtz, new(456, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Holtz, new(456, 88), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Holtz, new(104, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Holtz, new(104, 88), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Holtz, new(152, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Holtz, new(152, 88), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Zebbo, new(192, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, new(384, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, new(576, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
    ], 9);

    /// <summary>The enemy population of the Lower Norfair Fireflea room.</summary>
    internal static RoomEnemyPopulationDefinition LowerNorfairFireflea { get; } = new("LowerNorfairFireflea",
    [
        new(EnemyDefinitionId.Fune, new(496, 720), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 1, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(272, 488), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Right, CooldownTime: 1, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(496, 432), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 1, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(464, 216), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 1, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(496, 896), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 1, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Boulder, new(336, 304), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoulderSpawnSettings(TriggerHeight: 80, TriggerReach: 128, Start: BoulderStart.RollsAtOnce.Instance, Roll: BoulderRoll.Right)),
        new(EnemyDefinitionId.Boulder, new(440, 464), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new BoulderSpawnSettings(TriggerHeight: 80, TriggerReach: 128, Start: BoulderStart.RollsAtOnce.Instance, Roll: BoulderRoll.Left)),
        new(EnemyDefinitionId.Boulder, new(296, 608), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new BoulderSpawnSettings(TriggerHeight: 80, TriggerReach: 128, Start: BoulderStart.RollsAtOnce.Instance, Roll: BoulderRoll.Right)),
        new(EnemyDefinitionId.Fireflea, new(342, 1368), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromSteps(-2)), Radius: 48)),
        new(EnemyDefinitionId.Fireflea, new(456, 1336), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromStepFraction(3, 2)), Radius: 48)),
        new(EnemyDefinitionId.Fireflea, new(568, 1368), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromSteps(-2)), Radius: 48)),
        new(EnemyDefinitionId.Fireflea, new(680, 1336), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromStepFraction(3, 2)), Radius: 48)),
        new(EnemyDefinitionId.Fireflea, new(368, 1142), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromSteps(-2)), Radius: 64)),
    ], 10);

    /// <summary>The enemy population of the Magdollite Tunnel room.</summary>
    internal static RoomEnemyPopulationDefinition MagdolliteTunnel { get; } = new("MagdolliteTunnel",
    [
        new(EnemyDefinitionId.Magdollite, new(248, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteHeadSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(29, 8), Range: 96)),
        new(EnemyDefinitionId.Magdollite, new(248, 184), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteBodySpawnSettings()),
        new(EnemyDefinitionId.Magdollite, new(248, 184), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteOverlaySpawnSettings()),
        new(EnemyDefinitionId.Magdollite, new(488, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteHeadSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(29, 8), Range: 96)),
        new(EnemyDefinitionId.Magdollite, new(488, 184), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteBodySpawnSettings()),
        new(EnemyDefinitionId.Magdollite, new(488, 184), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteOverlaySpawnSettings()),
        new(EnemyDefinitionId.Magdollite, new(584, 184), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteHeadSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(29, 8), Range: 96)),
        new(EnemyDefinitionId.Magdollite, new(584, 184), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteBodySpawnSettings()),
        new(EnemyDefinitionId.Magdollite, new(584, 184), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MagdolliteOverlaySpawnSettings()),
        new(EnemyDefinitionId.Multiviola, new(120, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(96), Speed: 2)),
        new(EnemyDefinitionId.Multiviola, new(572, 93), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(48), Speed: 2)),
    ], 3);

    /// <summary>The enemy population of the Lava Dive room.</summary>
    internal static RoomEnemyPopulationDefinition LavaDive { get; } = new("LavaDive",
    [
        new(EnemyDefinitionId.Namihe, new(496, 264), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Left, LaunchSpeed: SignedEightEight.FromPixelFraction(3, 2), YProximity: 128)),
        new(EnemyDefinitionId.Namihe, new(288, 464), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Right, LaunchSpeed: SignedEightEight.FromPixelFraction(3, 2), YProximity: 128)),
        new(EnemyDefinitionId.Namihe, new(288, 584), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Right, LaunchSpeed: SignedEightEight.FromPixelFraction(3, 2), YProximity: 128)),
        new(EnemyDefinitionId.Namihe, new(639, 568), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Left, LaunchSpeed: SignedEightEight.FromPixelFraction(3, 2), YProximity: 128)),
        new(EnemyDefinitionId.Namihe, new(912, 280), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Left, LaunchSpeed: SignedEightEight.FromPixelFraction(3, 2), YProximity: 128)),
        new(EnemyDefinitionId.Namihe, new(832, 408), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new NamiheSpawnSettings(Facing: HorizontalDirection.Left, LaunchSpeed: SignedEightEight.FromPixelFraction(3, 2), YProximity: 128)),
    ], 6);

    /// <summary>The enemy population of the Pillar room.</summary>
    internal static RoomEnemyPopulationDefinition Pillar { get; } = new("Pillar",
    [
        new(EnemyDefinitionId.NuclearWaffle, new(400, 208), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new NuclearWaffleSpawnSettings(AngularSpeed: AngularSpeed.FromSteps(1), OrbitRadius: 64, Direction: NuclearWaffleSweepDirection.Forward, WaitTime: 32)),
        new(EnemyDefinitionId.NuclearWaffle, new(720, 208), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new NuclearWaffleSpawnSettings(AngularSpeed: AngularSpeed.FromSteps(1), OrbitRadius: 64, Direction: NuclearWaffleSweepDirection.Forward, WaitTime: 32)),
    ], 0);

    /// <summary>The enemy population of the Crocomire Escape room.</summary>
    internal static RoomEnemyPopulationDefinition CrocomireEscape { get; } = new("CrocomireEscape",
    [
        new(EnemyDefinitionId.Dragon, new(896, 488), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(896, 488), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Geruta, new(918, 127), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Geruta, new(918, 127), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Dragon, new(704, 488), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(704, 488), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(512, 488), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(512, 488), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(320, 488), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(320, 488), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(128, 488), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(128, 488), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
    ], 6);

    /// <summary>The enemy population of the Lower Norfair Spring Ball Maze room.</summary>
    internal static RoomEnemyPopulationDefinition LowerNorfairSpringBallMaze { get; } = new("LowerNorfairSpringBallMaze",
    [
        new(EnemyDefinitionId.Alcoon, new(256, 512), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Alcoon, new(337, 512), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Alcoon, new(96, 512), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Alcoon, new(408, 216), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Alcoon, new(1019, 1225), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 5);

    /// <summary>The enemy population of the Lower Norfair Escape Power Bombs room.</summary>
    internal static RoomEnemyPopulationDefinition LowerNorfairEscapePowerBombs { get; } = new("LowerNorfairEscapePowerBombs",
    [
    ], 0);

    /// <summary>The enemy population of the Purple Shaft room.</summary>
    internal static RoomEnemyPopulationDefinition PurpleShaft { get; } = new("PurpleShaft",
    [
    ], 0);

    /// <summary>The enemy population of the Ridley Energy Tank room.</summary>
    internal static RoomEnemyPopulationDefinition RidleyEnergyTank { get; } = new("RidleyEnergyTank",
    [
    ], 0);

    /// <summary>The enemy population of the Screw Attack room.</summary>
    internal static RoomEnemyPopulationDefinition ScrewAttack { get; } = new("ScrewAttack",
    [
    ], 0);

    /// <summary>The enemy population of the Green Bubbles Missiles room.</summary>
    internal static RoomEnemyPopulationDefinition GreenBubblesMissiles { get; } = new("GreenBubblesMissiles",
    [
        new(EnemyDefinitionId.ShutterShootable, new(296, 256), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, TriggerMode: ShutterTriggerMode.OneShotReaction, TravelDistance: 64, ProximityDistance: 0, WaitTime: 0, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterShootable, new(296, 256), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, TriggerMode: ShutterTriggerMode.OneShotReaction, TravelDistance: 128, ProximityDistance: 0, WaitTime: 0, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.Geruta, new(388, 96), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Geruta, new(388, 96), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
    ], 1);

    /// <summary>The enemy population of the Amphitheatre room.</summary>
    internal static RoomEnemyPopulationDefinition Amphitheatre { get; } = new("Amphitheatre",
    [
        new(EnemyDefinitionId.PirateGoldWalking, new(448, 208), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateGoldWalking, new(704, 176), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 8)),
        new(EnemyDefinitionId.PirateGoldWalking, new(696, 560), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 8)),
        new(EnemyDefinitionId.PirateGoldWalking, new(344, 288), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 8)),
        new(EnemyDefinitionId.PirateGoldWalking, new(464, 1088), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateGoldWalking, new(197, 1104), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 160)),
        new(EnemyDefinitionId.PirateGoldWalking, new(893, 925), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateGoldWalking, new(960, 784), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateGoldWalking, new(568, 416), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 8)),
    ], 9);

    /// <summary>The enemy population of the Cathedral Entrance room.</summary>
    internal static RoomEnemyPopulationDefinition CathedralEntrance { get; } = new("CathedralEntrance",
    [
        new(EnemyDefinitionId.Sova, new(40, 256), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixels(1))),
        new(EnemyDefinitionId.Sova, new(24, 312), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixels(1))),
        new(EnemyDefinitionId.Sova, new(248, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixels(1))),
        new(EnemyDefinitionId.Sova, new(176, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixels(1))),
        new(EnemyDefinitionId.Dessgeega, new(392, 415), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Dessgeega, new(559, 414), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
    ], 6);

    /// <summary>The enemy population of the Main Hall room.</summary>
    internal static RoomEnemyPopulationDefinition MainHall { get; } = new("MainHall",
    [
        new(EnemyDefinitionId.Elevator, new(1152, 674), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Up, ArrivalY: 24)),
        new(EnemyDefinitionId.Dragon, new(720, 760), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(720, 760), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(816, 760), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(816, 760), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Hibashi, new(407, 696), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiFlameSpawnSettings(InactiveTime: 80)),
        new(EnemyDefinitionId.Hibashi, new(407, 696), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiCollisionPartSpawnSettings()),
        new(EnemyDefinitionId.Hibashi, new(279, 680), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiFlameSpawnSettings(InactiveTime: 96)),
        new(EnemyDefinitionId.Hibashi, new(279, 680), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiCollisionPartSpawnSettings()),
        new(EnemyDefinitionId.Hibashi, new(151, 648), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiFlameSpawnSettings(InactiveTime: 96)),
        new(EnemyDefinitionId.Hibashi, new(151, 648), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiCollisionPartSpawnSettings()),
        new(EnemyDefinitionId.Hibashi, new(535, 696), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiFlameSpawnSettings(InactiveTime: 80)),
        new(EnemyDefinitionId.Hibashi, new(535, 696), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiCollisionPartSpawnSettings()),
        new(EnemyDefinitionId.Dragon, new(568, 760), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(568, 760), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(312, 760), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(312, 760), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(440, 760), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(440, 760), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
    ], 5);

    /// <summary>The enemy population of the Post-Crocomire Jump room.</summary>
    internal static RoomEnemyPopulationDefinition PostCrocomireJump { get; } = new("PostCrocomireJump",
    [
        new(EnemyDefinitionId.Mella, new(1475, 595), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mella, new(1433, 616), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mella, new(1521, 626), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mella, new(1362, 604), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mella, new(1403, 637), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.GrappleRipper, new(209, 155), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new GrappleRipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Right, MinimumX: 177, MaximumX: 273)),
        new(EnemyDefinitionId.GrappleRipper, new(408, 104), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrappleRipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(2), Direction: HorizontalDirection.Right, MinimumX: 360, MaximumX: 456)),
        new(EnemyDefinitionId.GrappleRipper, new(552, 152), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrappleRipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(2), Direction: HorizontalDirection.Right, MinimumX: 528, MaximumX: 616)),
        new(EnemyDefinitionId.GrappleRipper, new(864, 88), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrappleRipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Right, MinimumX: 768, MaximumX: 960)),
        new(EnemyDefinitionId.SuspensorPlatform, new(176, 664), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 40)),
        new(EnemyDefinitionId.SuspensorPlatform, new(272, 664), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 40)),
        new(EnemyDefinitionId.SuspensorPlatform, new(368, 664), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 40)),
        new(EnemyDefinitionId.SuspensorPlatform, new(464, 664), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 40)),
    ], 4);

    /// <summary>The enemy population of the Frog Save room.</summary>
    internal static RoomEnemyPopulationDefinition FrogSave { get; } = new("FrogSave",
    [
    ], 0);

    /// <summary>The enemy population of the Crocomire Save room.</summary>
    internal static RoomEnemyPopulationDefinition CrocomireSave { get; } = new("CrocomireSave",
    [
    ], 0);

    /// <summary>The enemy population of the Crumble Shaft room.</summary>
    internal static RoomEnemyPopulationDefinition CrumbleShaft { get; } = new("CrumbleShaft",
    [
        new(EnemyDefinitionId.Sova, new(120, 262), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(56, 312), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(192, 472), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(128, 598), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(128, 758), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(192, 838), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
    ], 6);

    /// <summary>The enemy population of the Wasteland room.</summary>
    internal static RoomEnemyPopulationDefinition Wasteland { get; } = new("Wasteland",
    [
        new(EnemyDefinitionId.Hibashi, new(1472, 224), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiFlameSpawnSettings(InactiveTime: 8)),
        new(EnemyDefinitionId.Hibashi, new(1472, 224), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiCollisionPartSpawnSettings()),
        new(EnemyDefinitionId.DessgeegaLarge, new(648, 90), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.DessgeegaLarge, new(552, 168), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.DessgeegaLarge, new(752, 168), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.DessgeegaLarge, new(312, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.Hibashi, new(1344, 224), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiFlameSpawnSettings(InactiveTime: 0)),
        new(EnemyDefinitionId.Hibashi, new(1344, 224), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiCollisionPartSpawnSettings()),
        new(EnemyDefinitionId.ShutterShootable, new(1320, 224), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, TriggerMode: ShutterTriggerMode.OneShotReaction, TravelDistance: 32, ProximityDistance: 0, WaitTime: 0, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.KzanTop, new(384, 604), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(4), FallWait: 112, FallDistance: 80)),
        new(EnemyDefinitionId.KzanBottom, new(384, 612), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(384, 348), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(4), FallWait: 112, FallDistance: 128)),
        new(EnemyDefinitionId.KzanBottom, new(384, 356), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
    ], 4);

    /// <summary>The enemy population of the Acid Snakes Tunnel room.</summary>
    internal static RoomEnemyPopulationDefinition AcidSnakesTunnel { get; } = new("AcidSnakesTunnel",
    [
        new(EnemyDefinitionId.Gamet, new(272, 232), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugLeaderSpawnSettings(TriggerWidth: 48, Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(272, 232), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(272, 232), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(272, 232), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(272, 232), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Dragon, new(512, 240), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(512, 240), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(752, 240), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(752, 240), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
    ], 3);

    /// <summary>The enemy population of the Grapple Tutorial 1 room.</summary>
    internal static RoomEnemyPopulationDefinition GrappleTutorial1 { get; } = new("GrappleTutorial1",
    [
    ], 0);

    /// <summary>The enemy population of the Bubble Mountain Save room.</summary>
    internal static RoomEnemyPopulationDefinition BubbleMountainSave { get; } = new("BubbleMountainSave",
    [
    ], 0);

    /// <summary>The enemy population of the Hi-Jump Energy Tank room.</summary>
    internal static RoomEnemyPopulationDefinition HiJumpEnergyTank { get; } = new("HiJumpEnergyTank",
    [
        new(EnemyDefinitionId.Sova, new(375, 181), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(5, 4))),
    ], 1);

    /// <summary>The enemy population of the Bubble Mountain room.</summary>
    internal static RoomEnemyPopulationDefinition BubbleMountain { get; } = new("BubbleMountain",
    [
        new(EnemyDefinitionId.JetRipper, new(376, 880), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(2), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Sova, new(280, 896), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnRightWall, BlockSide: CrawlerSurfaceOrientation.OnCeiling, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sova, new(337, 730), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sova, new(72, 809), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Waver, new(185, 440), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Waver, new(448, 544), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Waver, new(147, 222), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Cacatac, new(268, 149), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Left, Start: CacatacStart.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 16, Speed: SignedSixteenSixteen.Zero)),
    ], 8);

    /// <summary>The enemy population of the Ice Beam Acid room.</summary>
    internal static RoomEnemyPopulationDefinition IceBeamAcid { get; } = new("IceBeamAcid",
    [
        new(EnemyDefinitionId.Tripper, new(400, 168), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromWholePixels(1), VerticalAccelerationUpdates: 16)),
        new(EnemyDefinitionId.Tripper, new(256, 168), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromWholePixels(1), VerticalAccelerationUpdates: 16)),
        new(EnemyDefinitionId.Tripper, new(112, 168), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromWholePixels(1), VerticalAccelerationUpdates: 16)),
    ], 3);

    /// <summary>The enemy population of the Ice Beam Tutorial room.</summary>
    internal static RoomEnemyPopulationDefinition IceBeamTutorial { get; } = new("IceBeamTutorial",
    [
        new(EnemyDefinitionId.Boyon, new(120, 217), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 64, TriggerDistance: 64)),
        new(EnemyDefinitionId.Boyon, new(168, 217), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 64, TriggerDistance: 64)),
        new(EnemyDefinitionId.Boyon, new(216, 217), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 80, TriggerDistance: 64)),
        new(EnemyDefinitionId.JetRipper, new(344, 168), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(3, 2), Direction: HorizontalDirection.Left)),
    ], 1);

    /// <summary>The enemy population of the Post-Crocomire Missile room.</summary>
    internal static RoomEnemyPopulationDefinition PostCrocomireMissile { get; } = new("PostCrocomireMissile",
    [
        new(EnemyDefinitionId.Metaree, new(280, 56), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Metaree, new(248, 64), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Gamet, new(688, 224), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugLeaderSpawnSettings(TriggerWidth: 48, Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(688, 224), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(688, 224), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(688, 224), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
        new(EnemyDefinitionId.Gamet, new(688, 224), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(3))),
    ], 3);

    /// <summary>The enemy population of the Cathedral room.</summary>
    internal static RoomEnemyPopulationDefinition Cathedral { get; } = new("Cathedral",
    [
        new(EnemyDefinitionId.Geruta, new(161, 83), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Geruta, new(161, 83), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Geruta, new(478, 321), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Geruta, new(478, 321), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Geruta, new(548, 125), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Geruta, new(548, 125), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Sova, new(320, 229), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sova, new(110, 278), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sova, new(216, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnCeiling, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sova, new(668, 244), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
    ], 7);

    /// <summary>The enemy population of the Norfair Reserve Tank room.</summary>
    internal static RoomEnemyPopulationDefinition NorfairReserveTank { get; } = new("NorfairReserveTank",
    [
        new(EnemyDefinitionId.Dragon, new(344, 248), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(344, 248), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(216, 248), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(216, 248), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Dragon, new(152, 248), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Body)),
        new(EnemyDefinitionId.Dragon, new(152, 248), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DragonSpawnSettings(Part: DragonPart.Wing)),
        new(EnemyDefinitionId.Sova, new(120, 69), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(7, 4))),
        new(EnemyDefinitionId.Sova, new(248, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(7, 4))),
        new(EnemyDefinitionId.Sova, new(391, 58), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(7, 4))),
    ], 6);

    /// <summary>The enemy population of the Grapple Tutorial 2 room.</summary>
    internal static RoomEnemyPopulationDefinition GrappleTutorial2 { get; } = new("GrappleTutorial2",
    [
        new(EnemyDefinitionId.Fune, new(240, 656), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 32, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(16, 432), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Right, CooldownTime: 32, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(240, 528), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 32, LaunchSpeed: SignedEightEight.FromPixels(2))),
    ], 3);

    /// <summary>The enemy population of the Ice Beam Snake room.</summary>
    internal static RoomEnemyPopulationDefinition IceBeamSnake { get; } = new("IceBeamSnake",
    [
        new(EnemyDefinitionId.Sova, new(344, 440), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(408, 440), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(376, 440), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Fune, new(176, 452), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 80, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(32, 256), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Right, CooldownTime: 80, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(176, 324), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 80, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(32, 656), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Right, CooldownTime: 80, LaunchSpeed: SignedEightEight.FromPixels(2))),
    ], 7);

    /// <summary>The enemy population of the Golden Torizo room.</summary>
    internal static RoomEnemyPopulationDefinition GoldenTorizo { get; } = new("GoldenTorizo",
    [
        new(EnemyDefinitionId.GoldenTorizo, new(128, 384), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Spiky Platforms Tunnel room.</summary>
    internal static RoomEnemyPopulationDefinition SpikyPlatformsTunnel { get; } = new("SpikyPlatformsTunnel",
    [
        new(EnemyDefinitionId.Tripper, new(640, 168), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Right, Speed: SignedSixteenSixteen.FromPixelFraction(3, 2), VerticalAccelerationUpdates: 0)),
        new(EnemyDefinitionId.Tripper, new(384, 168), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromPixelFraction(3, 2), VerticalAccelerationUpdates: 7)),
        new(EnemyDefinitionId.ShutterShootable, new(512, 8), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.Immediate, TravelDistance: 64, ProximityDistance: 0, WaitTime: 0, RestAfterMovingUp: ShutterRest.For(48), RestAfterMovingDown: ShutterRest.For(48))),
    ], 2);

    /// <summary>The enemy population of the Speed Booster room.</summary>
    internal static RoomEnemyPopulationDefinition SpeedBooster { get; } = new("SpeedBooster",
    [
    ], 0);

    /// <summary>The enemy population of the Lower Norfair Farming room.</summary>
    internal static RoomEnemyPopulationDefinition LowerNorfairFarming { get; } = new("LowerNorfairFarming",
    [
        new(EnemyDefinitionId.Viola, new(552, 136), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(568, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(360, 136), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(360, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(216, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zebbo, new(664, 240), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, new(600, 240), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, new(520, 240), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, new(456, 240), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, new(328, 240), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, new(248, 240), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
    ], 11);

    /// <summary>The enemy population of the Single Chamber room.</summary>
    internal static RoomEnemyPopulationDefinition SingleChamber { get; } = new("SingleChamber",
    [
        new(EnemyDefinitionId.Multiviola, new(60, 854), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(208), Speed: 2)),
        new(EnemyDefinitionId.Multiviola, new(153, 483), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(88), Speed: 2)),
        new(EnemyDefinitionId.Multiviola, new(207, 210), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MultiviolaSpawnSettings(Heading: new TrigonometryAngle(136), Speed: 2)),
        new(EnemyDefinitionId.Alcoon, new(128, 824), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Alcoon, new(128, 568), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Alcoon, new(216, 312), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Alcoon, new(136, 312), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 4);

    /// <summary>The enemy population of the Speed Booster Hall room.</summary>
    internal static RoomEnemyPopulationDefinition SpeedBoosterHall { get; } = new("SpeedBoosterHall",
    [
        new(EnemyDefinitionId.Geruta, new(904, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Geruta, new(904, 296), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Geruta, new(1160, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Geruta, new(1160, 296), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Geruta, new(1580, 392), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Leader)),
        new(EnemyDefinitionId.Geruta, new(1580, 392), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new EnemyFollowerSpawnSettings(Role: EnemyFollowerRole.Follower)),
        new(EnemyDefinitionId.Metaree, new(288, 100), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Metaree, new(240, 86), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 5);

    /// <summary>The enemy population of the Bat Cave room.</summary>
    internal static RoomEnemyPopulationDefinition BatCave { get; } = new("BatCave",
    [
        new(EnemyDefinitionId.Gamet, new(96, 176), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugLeaderSpawnSettings(TriggerWidth: 32, Speed: SignedSixteenSixteen.FromWholePixels(2))),
        new(EnemyDefinitionId.Gamet, new(96, 176), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(2))),
        new(EnemyDefinitionId.Gamet, new(96, 176), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(2))),
        new(EnemyDefinitionId.Gamet, new(96, 176), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(2))),
        new(EnemyDefinitionId.Gamet, new(96, 176), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(2))),
        new(EnemyDefinitionId.Skree, new(200, 328), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Skree, new(168, 320), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Skree, new(128, 312), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 4);

    /// <summary>The enemy population of the Kronic Boost room.</summary>
    internal static RoomEnemyPopulationDefinition KronicBoost { get; } = new("KronicBoost",
    [
        new(EnemyDefinitionId.Viola, new(392, 168), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(352, 456), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(416, 520), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Viola, new(384, 600), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ViolaSpawnSettings(BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
    ], 4);

    /// <summary>The enemy population of the Business Center room.</summary>
    internal static RoomEnemyPopulationDefinition BusinessCenter { get; } = new("BusinessCenter",
    [
        new(EnemyDefinitionId.Elevator, new(128, 706), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Up, ArrivalY: 24)),
        new(EnemyDefinitionId.Sova, new(123, 856), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(144, 1000), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(97, 1080), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(160, 1304), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sova, new(96, 1352), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
        new(EnemyDefinitionId.Sova, new(129, 1512), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
    ], 6);

    /// <summary>The enemy population of the Three Musketeers room.</summary>
    internal static RoomEnemyPopulationDefinition ThreeMusketeers { get; } = new("ThreeMusketeers",
    [
        new(EnemyDefinitionId.Hibashi, new(663, 640), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiFlameSpawnSettings(InactiveTime: 48)),
        new(EnemyDefinitionId.Hibashi, new(663, 640), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiCollisionPartSpawnSettings()),
        new(EnemyDefinitionId.Hibashi, new(823, 640), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiFlameSpawnSettings(InactiveTime: 48)),
        new(EnemyDefinitionId.Hibashi, new(823, 640), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden, false, new HibashiCollisionPartSpawnSettings()),
        new(EnemyDefinitionId.KihunterRed, new(341, 498), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterRedWings, new(341, 498), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterRed, new(357, 372), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterRedWings, new(357, 372), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterRed, new(330, 264), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterRedWings, new(330, 264), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.ShutterShootable, new(744, 602), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.OneShotReaction, TravelDistance: 64, ProximityDistance: 0, WaitTime: 0, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterShootable, new(904, 570), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.OneShotReaction, TravelDistance: 64, ProximityDistance: 0, WaitTime: 0, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
    ], 3);

    /// <summary>The enemy population of the Crocomire room.</summary>
    internal static RoomEnemyPopulationDefinition Crocomire { get; } = new("Crocomire",
    [
        new(EnemyDefinitionId.Crocomire, new(1152, 120), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.CrocomireTongue, new(1152, 120), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Lower Norfair Elevator Save room.</summary>
    internal static RoomEnemyPopulationDefinition LowerNorfairElevatorSave { get; } = new("LowerNorfairElevatorSave",
    [
    ], 0);

    /// <summary>The enemy population of the Volcano room.</summary>
    internal static RoomEnemyPopulationDefinition Volcano { get; } = new("Volcano",
    [
        new(EnemyDefinitionId.Fune, new(464, 640), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 128, LaunchSpeed: SignedEightEight.FromPixelFraction(3, 2))),
        new(EnemyDefinitionId.Fune, new(736, 528), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 128, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(544, 456), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Right, CooldownTime: 128, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(736, 376), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 128, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(544, 296), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Right, CooldownTime: 128, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Fune, new(736, 216), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 128, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Polyp, new(248, 712), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Polyp, new(128, 712), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Polyp, new(136, 712), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Polyp, new(264, 712), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
    ], 10);

    /// <summary>The enemy population of the Upper Norfair Farming room.</summary>
    internal static RoomEnemyPopulationDefinition UpperNorfairFarming { get; } = new("UpperNorfairFarming",
    [
        new(EnemyDefinitionId.Fune, new(496, 400), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: 32, LaunchSpeed: SignedEightEight.FromPixels(2))),
        new(EnemyDefinitionId.Gamet, new(304, 448), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugLeaderSpawnSettings(TriggerWidth: 80, Speed: SignedSixteenSixteen.FromWholePixels(4))),
        new(EnemyDefinitionId.Gamet, new(304, 448), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4))),
        new(EnemyDefinitionId.Gamet, new(304, 448), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4))),
        new(EnemyDefinitionId.Gamet, new(304, 448), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4))),
        new(EnemyDefinitionId.Gamet, new(304, 448), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new NorfairPipeBugFollowerSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(4))),
    ], 2);

    /// <summary>The Norfair rooms' lists, in their authored order.</summary>
    internal static RoomEnemyPopulationDefinition[] Lists { get; } =
    [
        GrappleTutorial3,
        PostCrocomireShaft,
        MetalPirates,
        AcidStatue,
        PurpleFarming,
        IceBeamGate,
        SpikyAcidSnakesTunnel,
        RedKihunterShaft,
        FastRipper,
        WaveBeam,
        FastPillarsSetup,
        LowerNorfairElevator,
        LowerNorfairSave,
        UpperNorfairRefill,
        NorfairMap,
        MickeyMouse,
        GoldenTorizoEnergyRefill,
        Ridley,
        PostCrocomirePowerBombs,
        IceBeam,
        HiJumpBoots,
        PostCrocomireSave,
        FrogSpeedway,
        RisingTide,
        PostCrocomireFarming,
        WorstRoomInTheGame,
        DoubleChamber,
        GrappleBeam,
        RedPirateShaft,
        CrocomireSpeedway,
        Plowerhouse,
        LowerNorfairFireflea,
        MagdolliteTunnel,
        LavaDive,
        Pillar,
        CrocomireEscape,
        LowerNorfairSpringBallMaze,
        LowerNorfairEscapePowerBombs,
        PurpleShaft,
        RidleyEnergyTank,
        ScrewAttack,
        GreenBubblesMissiles,
        Amphitheatre,
        CathedralEntrance,
        MainHall,
        PostCrocomireJump,
        FrogSave,
        CrocomireSave,
        CrumbleShaft,
        Wasteland,
        AcidSnakesTunnel,
        GrappleTutorial1,
        BubbleMountainSave,
        HiJumpEnergyTank,
        BubbleMountain,
        IceBeamAcid,
        IceBeamTutorial,
        PostCrocomireMissile,
        Cathedral,
        NorfairReserveTank,
        GrappleTutorial2,
        IceBeamSnake,
        GoldenTorizo,
        SpikyPlatformsTunnel,
        SpeedBooster,
        LowerNorfairFarming,
        SingleChamber,
        SpeedBoosterHall,
        BatCave,
        KronicBoost,
        BusinessCenter,
        ThreeMusketeers,
        Crocomire,
        LowerNorfairElevatorSave,
        Volcano,
        UpperNorfairFarming,
    ];
}
