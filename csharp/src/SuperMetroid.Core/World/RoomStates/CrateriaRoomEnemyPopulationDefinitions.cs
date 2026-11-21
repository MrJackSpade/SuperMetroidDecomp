using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The enemy placement lists of the Crateria rooms.</summary>
internal static class CrateriaRoomEnemyPopulationDefinitions
{
    /// <summary>The face blocks of the Final Missile Bombway room, which it places alike in its default state and once Zebes is awake.</summary>
    private static readonly RoomEnemyPlacement[] FinalMissileBombwayFaceBlocks =
    [
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(256, 72)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(128, 72)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(384, 72)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(128, 184)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(256, 184)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(384, 184)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
    ];

    /// <summary>The enemy population of the East Ocean room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition EastOcean { get; } = new("EastOcean",
    [
        new(EnemyDefinitionId.Choot, EnemyPlacing.At(new(336, 1356)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChootSpawnSettings(FallingPath: ChootFallingPath.VeryWide, PatternLoops: 3, JumpDelay: 16)),
        new(EnemyDefinitionId.Choot, EnemyPlacing.At(new(400, 1292)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChootSpawnSettings(FallingPath: ChootFallingPath.VeryWide, PatternLoops: 3, JumpDelay: 16)),
        new(EnemyDefinitionId.Choot, EnemyPlacing.At(new(532, 1340)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChootSpawnSettings(FallingPath: ChootFallingPath.VeryWide, PatternLoops: 3, JumpDelay: 16)),
        new(EnemyDefinitionId.Choot, EnemyPlacing.At(new(1480, 1470)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChootSpawnSettings(FallingPath: ChootFallingPath.VeryWide, PatternLoops: 3, JumpDelay: 16)),
        new(EnemyDefinitionId.Choot, EnemyPlacing.At(new(176, 1308)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChootSpawnSettings(FallingPath: ChootFallingPath.VeryWide, PatternLoops: 3, JumpDelay: 16)),
        new(EnemyDefinitionId.Skultera, EnemyPlacing.At(new(252, 1379)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Left, Radius: 16, AngleStep: RotationAngle.FromSteps(2))),
        new(EnemyDefinitionId.Skultera, EnemyPlacing.At(new(469, 1393)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Right, Radius: 16, AngleStep: RotationAngle.FromSteps(2))),
        new(EnemyDefinitionId.Skultera, EnemyPlacing.At(new(788, 1415)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Left, Radius: 16, AngleStep: RotationAngle.FromSteps(2))),
        new(EnemyDefinitionId.Skultera, EnemyPlacing.At(new(964, 1387)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Right, Radius: 16, AngleStep: RotationAngle.FromSteps(2))),
        new(EnemyDefinitionId.Skultera, EnemyPlacing.At(new(1237, 1399)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Right, Radius: 16, AngleStep: RotationAngle.FromSteps(2))),
        new(EnemyDefinitionId.SuspensorPlatform, EnemyPlacing.At(new(1216, 1264)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 40)),
        new(EnemyDefinitionId.SuspensorPlatform, EnemyPlacing.At(new(960, 1264)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 40)),
        new(EnemyDefinitionId.SuspensorPlatform, EnemyPlacing.At(new(712, 1264)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 40)),
    ], 10);

    /// <summary>The enemy population of the Crateria Power Bombs room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CrateriaPowerBombs { get; } = new("CrateriaPowerBombs",
    [
        new(EnemyDefinitionId.Alcoon, EnemyPlacing.At(new(136, 216)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Alcoon, EnemyPlacing.At(new(264, 216)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Alcoon, EnemyPlacing.At(new(376, 216)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 3);

    /// <summary>The enemy population of the Terminator room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Terminator { get; } = new("Terminator",
    [
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(790, 348)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(531, 480)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(258, 610)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(888, 304)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(1176, 160)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(745, 470)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(519, 586)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(376, 560)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(664, 416)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
    ], 9);

    /// <summary>The enemy population of the Final Missile Bombway room once Zebes is awake.</summary>
    internal static PlacedRoomEnemyPopulationDefinition FinalMissileBombwayWhenZebesAwake { get; } = new("FinalMissileBombwayWhenZebesAwake", FinalMissileBombwayFaceBlocks, 0);

    /// <summary>The enemy population of the Final Missile Bombway room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition FinalMissileBombwayDefault { get; } = new("FinalMissileBombwayDefault", FinalMissileBombwayFaceBlocks, 0);

    /// <summary>The enemy population of the Parlor room once Zebes is awake.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ParlorWhenZebesAwake { get; } = new("ParlorWhenZebesAwake",
    [
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(1176, 64)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(1055, 136)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(1096, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Skree, EnemyPlacing.At(new(766, 59)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Skree, EnemyPlacing.At(new(702, 91)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Skree, EnemyPlacing.At(new(638, 59)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(416, 376)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(440, 552)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(296, 1080)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnFloor, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(468, 1080)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(384, 1032)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(360, 808)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnCeiling, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(490, 746)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnRightWall, BlockSide: CrawlerSurfaceOrientation.OnFloor, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(456, 632)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(385, 277)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(941, 488)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Left)),
    ], 16);

    /// <summary>The enemy population of the Flyway room while the Zebes timebomb is not set.</summary>
    internal static PlacedRoomEnemyPopulationDefinition FlywayBeforeTimebomb { get; } = new("FlywayBeforeTimebomb",
    [
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(237, 97)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(350, 92)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(283, 96)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(306, 73)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(314, 101)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(234, 66)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(458, 107)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(256, 118)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(410, 88)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(400, 126)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(383, 62)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(450, 65)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Pit room once Samus has the Morph Ball and Missiles.</summary>
    internal static PlacedRoomEnemyPopulationDefinition PitWithMorphBallAndMissiles { get; } = new("PitWithMorphBallAndMissiles",
    [
        new(EnemyDefinitionId.PirateGreyWalking, EnemyPlacing.At(new(616, 112)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(717, 63)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 32)),
        new(EnemyDefinitionId.PirateGreyWalking, EnemyPlacing.At(new(505, 112)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateGreyWalking, EnemyPlacing.At(new(376, 112)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateGreyWalking, EnemyPlacing.At(new(104, 128)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 32)),
    ], 5);

    /// <summary>The enemy population of the Gauntlet Entrance room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition GauntletEntrance { get; } = new("GauntletEntrance",
    [
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(1032, 56)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 112, Attack: VerticalDirection.Down)),
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(760, 200)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 112, Attack: VerticalDirection.Up)),
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(504, 56)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 112, Attack: VerticalDirection.Down)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(881, 101)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(570, 125)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(358, 119)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(264, 200)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 112, Attack: VerticalDirection.Up)),
    ], 3);

    /// <summary>The enemy population of the Bomb Torizo room while the Zebes timebomb is not set.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BombTorizoBeforeTimebomb { get; } = new("BombTorizoBeforeTimebomb",
    [
        new(EnemyDefinitionId.BombTorizo, EnemyPlacing.At(new(219, 179)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Green Pirates Shaft room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition GreenPiratesShaft { get; } = new("GreenPiratesShaft",
    [
        new(EnemyDefinitionId.PirateGreenWalking, EnemyPlacing.At(new(128, 1232)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 24)),
        new(EnemyDefinitionId.PirateGreenWalking, EnemyPlacing.At(new(141, 1312)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 24)),
        new(EnemyDefinitionId.PirateGreenWalking, EnemyPlacing.At(new(139, 1392)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 24)),
        new(EnemyDefinitionId.PirateGreenWalking, EnemyPlacing.At(new(132, 1472)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 24)),
        new(EnemyDefinitionId.PirateGreenWalking, EnemyPlacing.At(new(150, 1552)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 24)),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(184, 984)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(72, 984)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 7);

    /// <summary>The enemy population of the Green Brinstar Elevator room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition GreenBrinstarElevator { get; } = new("GreenBrinstarElevator",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(128, 162)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Down, ArrivalY: 320)),
    ], 0);

    /// <summary>The enemy population of the Final Missile room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition FinalMissileDefault { get; } = new("FinalMissileDefault",
    [
        new(EnemyDefinitionId.MorphBallEye, EnemyPlacing.At(new(50, 120)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MorphBallEyeMountSpawnSettings(Side: MorphBallEyeMountSide.Left)),
        new(EnemyDefinitionId.MorphBallEye, EnemyPlacing.At(new(50, 120)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MorphBallEyeBodySpawnSettings(Facing: HorizontalDirection.Right)),
    ], 0);

    /// <summary>The enemy population of the Crateria Map room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition CrateriaMap { get; } = new("CrateriaMap");

    /// <summary>The enemy population of the Final Missile room once Zebes is awake.</summary>
    internal static EmptyRoomEnemyPopulationDefinition FinalMissileWhenZebesAwake { get; } = new("FinalMissileWhenZebesAwake");

    /// <summary>The enemy population of the Climb room once the Zebes timebomb is set.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ClimbWhenZebesTimebombSet { get; } = new("ClimbWhenZebesTimebombSet",
    [
        new(EnemyDefinitionId.PirateSilverNinja, EnemyPlacing.At(new(128, 2176)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, new NinjaSpacePirateSpawnSettings(StartPost: NinjaSpacePirateStartPost.Right, PostDistance: 0)),
        new(EnemyDefinitionId.PirateSilverNinja, EnemyPlacing.At(new(192, 2176)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, new NinjaSpacePirateSpawnSettings(StartPost: NinjaSpacePirateStartPost.Right, PostDistance: 0)),
    ], 2);

    /// <summary>The enemy population of the Crateria Save room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition CrateriaSave { get; } = new("CrateriaSave");

    /// <summary>The enemy population of the Forgotten Highway Elbow room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition ForgottenHighwayElbow { get; } = new("ForgottenHighwayElbow");

    /// <summary>The enemy population of the Moat room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition Moat { get; } = new("Moat");

    /// <summary>The enemy population of the Climb room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ClimbDefault { get; } = new("ClimbDefault",
    [
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(276, 76)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(80), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(272, 88)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(144), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(269, 114)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(172), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(491, 150)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(499, 154)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(195), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(277, 294)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(146), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(276, 291)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(96), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(273, 296)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(156), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(494, 535)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(240), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(278, 1721)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(188), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
    ], 0);

    /// <summary>The enemy population of the West Ocean room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition WestOcean { get; } = new("WestOcean",
    [
        new(EnemyDefinitionId.Skultera, EnemyPlacing.At(new(1188, 1392)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Right, Radius: 16, AngleStep: RotationAngle.FromSteps(2))),
        new(EnemyDefinitionId.Skultera, EnemyPlacing.At(new(971, 1402)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Left, Radius: 16, AngleStep: RotationAngle.FromSteps(2))),
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(632, 704)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
        new(EnemyDefinitionId.GrappleRipper, EnemyPlacing.At(new(1424, 1072)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrappleRipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Right, MinimumX: 1376, MaximumX: 1472)),
        new(EnemyDefinitionId.Tripper, EnemyPlacing.At(new(336, 216)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromWholePixels(1), VerticalAccelerationUpdates: 0)),
        new(EnemyDefinitionId.Tripper, EnemyPlacing.At(new(904, 88)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromWholePixels(1), VerticalAccelerationUpdates: 0)),
        new(EnemyDefinitionId.Tripper, EnemyPlacing.At(new(880, 352)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Right, Speed: SignedSixteenSixteen.FromWholePixels(1), VerticalAccelerationUpdates: 0)),
    ], 7);

    /// <summary>The enemy population of the Parlor room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ParlorDefault { get; } = new("ParlorDefault",
    [
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(1214, 192)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(170), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(1188, 201)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(181), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(1211, 207)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(1252, 65)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(72), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(1109, 42)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(96), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(1087, 46)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(108), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(1135, 39)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(120), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(984, 195)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(202), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(978, 201)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(987, 206)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(226), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(283, 188)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(160), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(274, 196)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(171), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(300, 553)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(124), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(292, 557)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(148), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(323, 575)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(132), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(319, 703)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(161), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(325, 698)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(145), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(317, 707)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(140), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(269, 731)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(172), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(421, 1248)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(232), ActivationRadius: 32, Behavior: SbugActivationBehavior.MoveForward)),
    ], 0);

    /// <summary>The enemy population of the Landing Site room while the Zebes timebomb is not set.</summary>
    internal static PlacedRoomEnemyPopulationDefinition LandingSiteBeforeTimebomb { get; } = new("LandingSiteBeforeTimebomb",
    [
        new(EnemyDefinitionId.ShipTop, EnemyPlacing.At(new(1152, 1144)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.ShipBottomEntrance, EnemyPlacing.At(new(1152, 1144)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new GunshipSpawnSettings(Part: GunshipBottomPiece.Hull)),
        new(EnemyDefinitionId.ShipBottomEntrance, EnemyPlacing.At(new(1152, 1144)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new GunshipSpawnSettings(Part: GunshipBottomPiece.EntrancePad)),
    ], 0);

    /// <summary>The enemy population of the Crateria Super room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CrateriaSuper { get; } = new("CrateriaSuper",
    [
        new(EnemyDefinitionId.Boyon, EnemyPlacing.At(new(600, 1960)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 64, TriggerDistance: 32)),
        new(EnemyDefinitionId.Boyon, EnemyPlacing.At(new(632, 1960)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 64, TriggerDistance: 32)),
        new(EnemyDefinitionId.Boyon, EnemyPlacing.At(new(664, 1960)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 64, TriggerDistance: 32)),
        new(EnemyDefinitionId.Boyon, EnemyPlacing.At(new(696, 1960)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 64, TriggerDistance: 32)),
    ], 0);

    /// <summary>The enemy population of the Pre Bowling room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition PreBowling { get; } = new("PreBowling",
    [
        new(EnemyDefinitionId.OrangeZoomer, EnemyPlacing.At(new(128, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
    ], 1);

    /// <summary>The enemy population of the Climb room once Zebes is awake.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ClimbWhenZebesAwake { get; } = new("ClimbWhenZebesAwake",
    [
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(304, 216)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(464, 296)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(304, 472)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(304, 824)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(464, 984)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(304, 1208)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(464, 1448)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(301, 1688)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(464, 632)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(464, 1800)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateGreyWall, EnemyPlacing.At(new(304, 1992)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, JumpSpeed: WallSpacePirateJumpSpeed.Slow, JumpSpan: 160)),
    ], 11);

    /// <summary>The enemy population of the Bowling Alley Path room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BowlingAlleyPath { get; } = new("BowlingAlleyPath",
    [
        new(EnemyDefinitionId.Choot, EnemyPlacing.At(new(112, 204)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChootSpawnSettings(FallingPath: ChootFallingPath.VeryWide, PatternLoops: 4, JumpDelay: 0)),
        new(EnemyDefinitionId.Choot, EnemyPlacing.At(new(224, 204)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChootSpawnSettings(FallingPath: ChootFallingPath.VeryWide, PatternLoops: 4, JumpDelay: 0)),
        new(EnemyDefinitionId.Choot, EnemyPlacing.At(new(336, 204)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChootSpawnSettings(FallingPath: ChootFallingPath.VeryWide, PatternLoops: 4, JumpDelay: 0)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(170, 93)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(233, 117)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(310, 99)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 6);

    /// <summary>The enemy population of the Red Brinstar Elevator room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition RedBrinstarElevator { get; } = new("RedBrinstarElevator",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(128, 194)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Down, ArrivalY: 320)),
    ], 0);

    /// <summary>The enemy population of the Gauntlet Energy Tank room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition GauntletEnergyTank { get; } = new("GauntletEnergyTank",
    [
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(136, 56)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 112, Attack: VerticalDirection.Down)),
        new(EnemyDefinitionId.Zebbo, EnemyPlacing.At(new(136, 184)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
    ], 1);

    /// <summary>The enemy population of the Pit room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition PitDefault { get; } = new("PitDefault",
    [
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(107, 56)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(40), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(112, 52)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(120), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(90, 45)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(124), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(115, 173)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(148), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(122, 183)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(196), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(108, 195)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(124), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(127, 178)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(194), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(189, 202)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(193), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(713, 166)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(240), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(716, 171)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(236), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
    ], 0);

    /// <summary>The enemy population of the Crab Maze room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CrabMaze { get; } = new("CrabMaze",
    [
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(632, 172)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(21, 152)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(209, 200)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(514, 104)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(286, 472)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixels(1))),
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(280, 344)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(178, 440)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(680, 452)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixels(1))),
    ], 8);

    /// <summary>The enemy population of the Crateria Tube room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition CrateriaTube { get; } = new("CrateriaTube");

    /// <summary>The enemy population of the Forgotten Highway Kagos room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ForgottenHighwayKagos { get; } = new("ForgottenHighwayKagos",
    [
        new(EnemyDefinitionId.Kago, EnemyPlacing.At(new(80, 832)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KagoSpawnSettings(HitCount: 10)),
        new(EnemyDefinitionId.Kago, EnemyPlacing.At(new(168, 512)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KagoSpawnSettings(HitCount: 10)),
    ], 2);

    /// <summary>The enemy population of the Elevator to Blue Brinstar room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ElevatorToBlueBrinstar { get; } = new("ElevatorToBlueBrinstar",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(128, 162)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Down, ArrivalY: 320)),
    ], 0);

    /// <summary>The enemy population of the Forgotten Highway Elevator room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ForgottenHighwayElevator { get; } = new("ForgottenHighwayElevator",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(128, 192)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Down, ArrivalY: 320)),
    ], 0);

    /// <summary>The enemy population of the Pre Map Flyway room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition PreMapFlyway { get; } = new("PreMapFlyway",
    [
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(332, 110)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(251, 89)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Mellow, EnemyPlacing.At(new(423, 90)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Rio, EnemyPlacing.At(new(331, 69)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 1);

    /// <summary>The enemy population of the Lower Mushrooms room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition LowerMushrooms { get; } = new("LowerMushrooms",
    [
        new(EnemyDefinitionId.Kago, EnemyPlacing.At(new(256, 160)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KagoSpawnSettings(HitCount: 10)),
        new(EnemyDefinitionId.Kago, EnemyPlacing.At(new(528, 176)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KagoSpawnSettings(HitCount: 10)),
        new(EnemyDefinitionId.ArmoredZoomer, EnemyPlacing.At(new(760, 112)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.ArmoredZoomer, EnemyPlacing.At(new(424, 112)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
    ], 4);

    /// <summary>The enemy population of the Landing Site room once the Zebes timebomb is set.</summary>
    internal static PlacedRoomEnemyPopulationDefinition LandingSiteWhenZebesTimebombSet { get; } = new("LandingSiteWhenZebesTimebombSet",
    [
        new(EnemyDefinitionId.ShipTop, EnemyPlacing.At(new(1152, 1144)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.ShipBottomEntrance, EnemyPlacing.At(new(1152, 1144)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new GunshipSpawnSettings(Part: GunshipBottomPiece.Hull)),
        new(EnemyDefinitionId.ShipBottomEntrance, EnemyPlacing.At(new(1152, 1144)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new GunshipSpawnSettings(Part: GunshipBottomPiece.EntrancePad)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(81, 1194)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(128, 1212)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(161, 1232)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(229, 1239)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(347, 1239)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(413, 1237)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(448, 1225)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(546, 1224)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(579, 1220)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(636, 1230)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(791, 1239)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(831, 1225)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(876, 1206)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(912, 1193)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(985, 1207)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1050, 1241)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1180, 1240)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1273, 1226)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1317, 1211)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1367, 1225)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1478, 1236)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1572, 1236)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
    ], 0);

    /// <summary>The enemy population of the Parlor room once the Zebes timebomb is set.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ParlorWhenZebesTimebombSet { get; } = new("ParlorWhenZebesTimebombSet",
    [
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(325, 181)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(441, 186)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(478, 169)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(551, 177)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(573, 185)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(623, 192)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(689, 208)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(721, 200)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(779, 192)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(815, 160)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(862, 136)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(900, 119)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(988, 121)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1022, 142)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1055, 161)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1116, 196)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1151, 179)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1194, 174)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, EnemyPlacing.At(new(1228, 166)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
    ], 0);

    /// <summary>The enemy population of the Bomb Torizo room once the Zebes timebomb is set.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BombTorizoWhenZebesTimebombSet { get; } = new("BombTorizoWhenZebesTimebombSet",
    [
        new(EnemyDefinitionId.DachoraEscape, EnemyPlacing.At(new(224, 184)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.EtecoonEscape, EnemyPlacing.At(new(224, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EscapeEtecoonSpawnSettings(Role: EscapeEtecoonRole.LeftWalker)),
        new(EnemyDefinitionId.EtecoonEscape, EnemyPlacing.At(new(224, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EscapeEtecoonSpawnSettings(Role: EscapeEtecoonRole.RightWalker)),
        new(EnemyDefinitionId.EtecoonEscape, EnemyPlacing.At(new(224, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new EscapeEtecoonSpawnSettings(Role: EscapeEtecoonRole.WaitForEscapeEvent)),
    ], 4);

    /// <summary>The enemy population of the Flyway room once the Zebes timebomb is set.</summary>
    internal static EmptyRoomEnemyPopulationDefinition FlywayWhenZebesTimebombSet { get; } = new("FlywayWhenZebesTimebombSet");

    /// <summary>The enemy population of the Crateria Kihunter room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CrateriaKihunter { get; } = new("CrateriaKihunter",
    [
        new(EnemyDefinitionId.KihunterGreen, EnemyPlacing.At(new(612, 115)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterGreenWings, EnemyPlacing.At(new(612, 115)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterGreen, EnemyPlacing.At(new(168, 135)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterGreenWings, EnemyPlacing.At(new(168, 135)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(408, 472)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Sciser, EnemyPlacing.At(new(357, 538)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
    ], 4);

    /// <summary>The enemy population of the Statues room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Statues { get; } = new("Statues",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(128, 432)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Down, ArrivalY: 576)),
        new(EnemyDefinitionId.TourianStatue, EnemyPlacing.At(new(120, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new TourianEntranceStatueSpawnSettings(Role: TourianEntranceStatueRole.SpawnsDecorations)),
        new(EnemyDefinitionId.TourianStatue, EnemyPlacing.At(new(142, 85)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new TourianEntranceStatueSpawnSettings(Role: TourianEntranceStatueRole.Inert)),
        new(EnemyDefinitionId.TourianStatue, EnemyPlacing.At(new(132, 136)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new TourianEntranceStatueSpawnSettings(Role: TourianEntranceStatueRole.Inert)),
    ], 0);

    /// <summary>The enemy population of the Statues Hallway room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition StatuesHallway { get; } = new("StatuesHallway");

    /// <summary>The Crateria rooms' lists, in their authored order.</summary>
    internal static RoomEnemyPopulationDefinition[] Lists { get; } =
    [
        EastOcean,
        CrateriaPowerBombs,
        Terminator,
        FinalMissileBombwayWhenZebesAwake,
        FinalMissileBombwayDefault,
        ParlorWhenZebesAwake,
        FlywayBeforeTimebomb,
        PitWithMorphBallAndMissiles,
        GauntletEntrance,
        BombTorizoBeforeTimebomb,
        GreenPiratesShaft,
        GreenBrinstarElevator,
        FinalMissileDefault,
        CrateriaMap,
        FinalMissileWhenZebesAwake,
        ClimbWhenZebesTimebombSet,
        CrateriaSave,
        ForgottenHighwayElbow,
        Moat,
        ClimbDefault,
        WestOcean,
        ParlorDefault,
        LandingSiteBeforeTimebomb,
        CrateriaSuper,
        PreBowling,
        ClimbWhenZebesAwake,
        BowlingAlleyPath,
        RedBrinstarElevator,
        GauntletEnergyTank,
        PitDefault,
        CrabMaze,
        CrateriaTube,
        ForgottenHighwayKagos,
        ElevatorToBlueBrinstar,
        ForgottenHighwayElevator,
        PreMapFlyway,
        LowerMushrooms,
        LandingSiteWhenZebesTimebombSet,
        ParlorWhenZebesTimebombSet,
        BombTorizoWhenZebesTimebombSet,
        FlywayWhenZebesTimebombSet,
        CrateriaKihunter,
        Statues,
        StatuesHallway,
    ];
}
