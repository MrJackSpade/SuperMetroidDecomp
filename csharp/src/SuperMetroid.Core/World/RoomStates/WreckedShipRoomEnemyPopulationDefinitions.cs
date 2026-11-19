using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The enemy placement lists of the Wrecked Ship rooms.</summary>
internal static class WreckedShipRoomEnemyPopulationDefinitions
{
    /// <summary>The enemy population of the Sponge Bath room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition SpongeBathWhenAreaBossDead { get; } = new("SpongeBathWhenAreaBossDead",
    [
        new(EnemyDefinitionId.Bull, new(240, 136), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new BullSpawnSettings(Intervals: new BullSpeedIntervals(6, 2), MaximumSpeed: SignedEightEight.FromPixelFraction(1791, 256))),
    ], 1);

    /// <summary>The enemy population of the Wrecked Ship Energy Tank room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipEnergyTankWhenAreaBossDead { get; } = new("WreckedShipEnergyTankWhenAreaBossDead",
    [
        new(EnemyDefinitionId.Skultera, new(173, 374), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Right, Radius: 16, AngleStep: RotationAngle.FromSteps(2))),
        new(EnemyDefinitionId.Skultera, new(576, 300), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Left, Radius: 16, AngleStep: RotationAngle.FromSteps(2))),
        new(EnemyDefinitionId.SuspensorPlatform, new(280, 168), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 32)),
        new(EnemyDefinitionId.SuspensorPlatform, new(384, 168), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 32)),
        new(EnemyDefinitionId.SuspensorPlatform, new(496, 168), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new PlatformSpawnSettings(Movement: HorizontalDirection.Left, Speed: SignedSixteenSixteen.Zero, VerticalAccelerationUpdates: 32)),
    ], 2);

    /// <summary>The enemy population of the Wrecked Ship Main Shaft room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipMainShaftDefault { get; } = new("WreckedShipMainShaftDefault",
    [
        new(EnemyDefinitionId.Coven, new(56, 984), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Sbug, new(1090, 842), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(112), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1101, 849), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(124), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1224, 972), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(204), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1221, 979), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(216), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1230, 985), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1227, 1227), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(32), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1228, 1238), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(255), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1220, 1233), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(10), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1246, 1214), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(935, 1313), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(122), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(953, 1324), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(126), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1220, 637), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(252), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1226, 650), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(8), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1243, 632), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(224), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1061, 528), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(132), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1060, 518), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(120), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(325, 698), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(145), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(317, 707), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(140), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(269, 731), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(172), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(429, 1256), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(232), ActivationRadius: 32, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.KzanTop, new(608, 1368), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.Zero, FallWait: 0, FallDistance: 0)),
        new(EnemyDefinitionId.KzanBottom, new(608, 1376), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(448, 1368), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.Zero, FallWait: 0, FallDistance: 0)),
        new(EnemyDefinitionId.KzanBottom, new(448, 1376), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(288, 1368), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.Zero, FallWait: 0, FallDistance: 0)),
        new(EnemyDefinitionId.KzanBottom, new(288, 1376), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(128, 1368), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.Zero, FallWait: 0, FallDistance: 0)),
        new(EnemyDefinitionId.KzanBottom, new(128, 1376), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Atomic, new(1072, 2000), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(1232, 2000), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
    ], 3);

    /// <summary>The enemy population of the Bowling Alley room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition BowlingAlleyDefault { get; } = new("BowlingAlleyDefault",
    [
        new(EnemyDefinitionId.Chozo, new(1224, 394), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChozoStatueSpawnSettings(Site: ChozoStatueSite.WreckedShip)),
        new(EnemyDefinitionId.Sbug, new(1227, 393), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(194), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1228, 380), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(220), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1221, 388), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(222), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1233, 401), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(196), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1228, 413), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(216), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(38, 466), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(157, 490), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(140), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(206, 466), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(152), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(211, 463), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(144), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(37, 432), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(45, 441), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(152), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(48, 447), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(146), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(482, 474), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(184), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(477, 483), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(504, 483), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(224), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(629, 481), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(861, 466), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(818, 487), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(863, 473), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(804, 482), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(196), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
    ], 0);

    /// <summary>The enemy population of the Attic room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition AtticWhenAreaBossDead { get; } = new("AtticWhenAreaBossDead",
    [
        new(EnemyDefinitionId.KihunterYellow, new(1049, 200), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Wingless)),
        new(EnemyDefinitionId.KihunterYellowWings, new(1049, 200), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterYellow, new(511, 142), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterYellowWings, new(511, 142), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterYellow, new(1493, 118), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterYellowWings, new(1493, 118), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.Atomic, new(1423, 90), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpLeftTurning, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Atomic, new(850, 97), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpLeftTurning, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Atomic, new(556, 96), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpLeftTurning, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Atomic, new(147, 124), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.KihunterYellow, new(1256, 200), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Wingless)),
        new(EnemyDefinitionId.KihunterYellowWings, new(1256, 200), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.Spark, new(224, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 144)),
        new(EnemyDefinitionId.Spark, new(304, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 80)),
        new(EnemyDefinitionId.Spark, new(656, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 32)),
        new(EnemyDefinitionId.Spark, new(752, 104), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 128)),
        new(EnemyDefinitionId.Spark, new(880, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 48)),
        new(EnemyDefinitionId.Spark, new(1072, 136), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 128)),
        new(EnemyDefinitionId.Spark, new(1232, 136), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 112)),
        new(EnemyDefinitionId.Spark, new(1488, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 16)),
        new(EnemyDefinitionId.Spark, new(1584, 120), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 96)),
    ], 8);

    /// <summary>The enemy population of the Basement room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition BasementWhenAreaBossDead { get; } = new("BasementWhenAreaBossDead",
    [
        new(EnemyDefinitionId.WorkRobot, new(77, 176), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
        new(EnemyDefinitionId.WorkRobot, new(880, 176), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
        new(EnemyDefinitionId.Atomic, new(152, 77), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Atomic, new(638, 168), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurningBack, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Atomic, new(852, 73), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurningBack, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Spark, new(1145, 106), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 32)),
    ], 5);

    /// <summary>The enemy population of the Wrecked Ship Map room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipMapWhenAreaBossDead { get; } = new("WreckedShipMapWhenAreaBossDead",
    [
    ], 0);

    /// <summary>The enemy population of the Wrecked Ship West Super room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipWestSuperDefault { get; } = new("WreckedShipWestSuperDefault",
    [
    ], 0);

    /// <summary>The enemy population of the Spiky Death room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition SpikyDeathDefault { get; } = new("SpikyDeathDefault",
    [
    ], 0);

    /// <summary>The enemy population of the Sponge Bath room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition SpongeBathDefault { get; } = new("SpongeBathDefault",
    [
    ], 0);

    /// <summary>The enemy population of the Wrecked Ship Entrance room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipEntranceWhenAreaBossDead { get; } = new("WreckedShipEntranceWhenAreaBossDead",
    [
    ], 0);

    /// <summary>The enemy population of the Electric Death room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition ElectricDeathDefault { get; } = new("ElectricDeathDefault",
    [
    ], 0);

    /// <summary>The enemy population of the Bowling Alley room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition BowlingAlleyWhenAreaBossDead { get; } = new("BowlingAlleyWhenAreaBossDead",
    [
        new(EnemyDefinitionId.Chozo, new(1224, 394), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ChozoStatueSpawnSettings(Site: ChozoStatueSite.WreckedShip)),
        new(EnemyDefinitionId.WorkRobot, new(752, 624), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
        new(EnemyDefinitionId.WorkRobot, new(880, 624), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
    ], 2);

    /// <summary>The enemy population of Wrecked Ship's Assembly Line room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition AssemblyLineDefault { get; } = new("AssemblyLineDefault",
    [
    ], 0);

    /// <summary>The enemy population of the Phantoon room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition PhantoonWhenAreaBossDead { get; } = new("PhantoonWhenAreaBossDead",
    [
    ], 0);

    /// <summary>The enemy population of the Wrecked Ship Energy Tank room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipEnergyTankDefault { get; } = new("WreckedShipEnergyTankDefault",
    [
    ], 0);

    /// <summary>The enemy population of the Gravity Suit room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition GravitySuitDefault { get; } = new("GravitySuitDefault",
    [
    ], 0);

    /// <summary>The enemy population of the Wrecked Ship Map room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipMapDefault { get; } = new("WreckedShipMapDefault",
    [
        new(EnemyDefinitionId.Coven, new(136, 136), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Sbug, new(88, 184), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(184), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(80, 168), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(196), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(77, 207), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(250), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(205, 184), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(176), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(200, 176), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(184), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(208, 184), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(139, 39), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(120), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(141, 8), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(138), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
    ], 1);

    /// <summary>The enemy population of the Gravity Suit room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition GravitySuitWhenAreaBossDead { get; } = new("GravitySuitWhenAreaBossDead",
    [
    ], 0);

    /// <summary>The enemy population of the Basement room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition BasementDefault { get; } = new("BasementDefault",
    [
        new(EnemyDefinitionId.Sbug, new(875, 187), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(194), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(880, 184), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(220), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(880, 188), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(222), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(872, 195), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(196), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(882, 201), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(216), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(78, 170), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(81, 173), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(140), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(72, 200), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(112), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(82, 194), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(144), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(41, 170), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(45, 185), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(152), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(48, 191), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(146), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(482, 218), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(184), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(477, 227), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(504, 227), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(224), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1011, 175), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1007, 179), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1002, 203), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(972, 222), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(974, 222), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(196), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.UnpoweredWorkRobot, new(77, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerLeaningLeft)),
        new(EnemyDefinitionId.UnpoweredWorkRobot, new(880, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
    ], 2);

    /// <summary>The enemy population of the Wrecked Ship Entrance room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipEntranceDefault { get; } = new("WreckedShipEntranceDefault",
    [
        new(EnemyDefinitionId.Sbug, new(116, 64), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(24), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(124, 189), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(224), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(150, 197), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(184), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(115, 74), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(120), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(186, 66), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(48), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(180, 200), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(152), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(65, 73), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(120), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(130, 188), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(216), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(68, 75), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(88), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(599, 65), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(24), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(283, 188), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(232), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(274, 196), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(248), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(301, 542), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(124), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(293, 550), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(148), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(323, 575), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(132), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(319, 703), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(161), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(325, 698), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(145), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(317, 707), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(140), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(269, 731), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(172), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(429, 1256), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(232), ActivationRadius: 32, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(609, 66), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(80), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(659, 55), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(72), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(634, 181), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(204), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(651, 190), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(221), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(609, 203), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1001, 179), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(216), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(985, 192), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(216), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1003, 196), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(216), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(990, 81), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(125), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1007, 76), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(60), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(875, 75), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(112), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(791, 176), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(248), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
    ], 0);

    /// <summary>The enemy population of Wrecked Ship's Assembly Line room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition AssemblyLineWhenAreaBossDead { get; } = new("AssemblyLineWhenAreaBossDead",
    [
        new(EnemyDefinitionId.Spark, new(464, 93), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 152)),
        new(EnemyDefinitionId.Spark, new(624, 91), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 64)),
        new(EnemyDefinitionId.Spark, new(144, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 160)),
        new(EnemyDefinitionId.Spark, new(240, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 48)),
        new(EnemyDefinitionId.Spark, new(301, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 16)),
        new(EnemyDefinitionId.Spark, new(400, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 128)),
        new(EnemyDefinitionId.Spark, new(560, 88), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 40)),
        new(EnemyDefinitionId.Spark, new(728, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 32)),
        new(EnemyDefinitionId.WorkRobot, new(592, 128), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
        new(EnemyDefinitionId.WorkRobot, new(272, 128), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
        new(EnemyDefinitionId.WorkRobot, new(432, 128), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
    ], 3);

    /// <summary>The enemy population of the Wrecked Ship Save room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipSaveWhenAreaBossDead { get; } = new("WreckedShipSaveWhenAreaBossDead",
    [
    ], 0);

    /// <summary>The enemy population of the Wrecked Ship West Super room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipWestSuperWhenAreaBossDead { get; } = new("WreckedShipWestSuperWhenAreaBossDead",
    [
        new(EnemyDefinitionId.Spark, new(154, 58), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 48)),
        new(EnemyDefinitionId.Spark, new(52, 59), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 32)),
        new(EnemyDefinitionId.Spark, new(151, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 40)),
        new(EnemyDefinitionId.Spark, new(103, 200), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 48)),
        new(EnemyDefinitionId.Spark, new(178, 200), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 24)),
    ], 0);

    /// <summary>The enemy population of the Attic room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition AtticDefault { get; } = new("AtticDefault",
    [
        new(EnemyDefinitionId.Coven, new(727, 152), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Sbug, new(1705, 186), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(170), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1711, 189), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(212), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForwardThenWait)),
        new(EnemyDefinitionId.Sbug, new(1709, 197), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1721, 178), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(240), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(40, 185), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(116), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(37, 190), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForwardThenWait)),
        new(EnemyDefinitionId.Sbug, new(27, 198), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(160), ActivationRadius: 160, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(51, 82), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(120), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(40, 67), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(80), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForwardThenWait)),
        new(EnemyDefinitionId.Sbug, new(1038, 230), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(130), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(286, 234), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(132), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(276, 230), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(130), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(26, 73), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(152), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForwardThenWait)),
        new(EnemyDefinitionId.Sbug, new(1073, 233), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(184), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1085, 237), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(186), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForwardThenWait)),
        new(EnemyDefinitionId.Sbug, new(1475, 227), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 1, InitialAngle: RotationAngle.FromSteps(196), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForwardThenWait)),
        new(EnemyDefinitionId.Sbug, new(1490, 215), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(184), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(423, 148), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(64), ActivationRadius: 64, Behavior: SbugActivationBehavior.MoveForwardThenWait)),
        new(EnemyDefinitionId.Sbug, new(1749, 197), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(224), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Atomic, new(224, 78), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(752, 94), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(1584, 110), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(1488, 190), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(656, 190), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(304, 190), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(880, 222), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(1232, 126), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(1072, 126), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
    ], 10);

    /// <summary>The enemy population of the Wrecked Ship East Super room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipEastSuperDefault { get; } = new("WreckedShipEastSuperDefault",
    [
        new(EnemyDefinitionId.Coven, new(136, 136), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Sbug, new(847, 154), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(194), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(255, 233), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(252), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(852, 166), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(222), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(852, 164), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(196), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(839, 166), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(216), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(373, 209), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(81, 173), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(140), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(72, 200), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(112), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(734, 153), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(48), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(715, 143), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(184), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(45, 185), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(152), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(48, 191), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(146), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(713, 138), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(184), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(739, 115), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(168), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(743, 170), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(224), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1011, 175), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 4, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1007, 179), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(1002, 203), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(388, 216), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(240, 229), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(130), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.UnpoweredWorkRobot, new(376, 144), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
        new(EnemyDefinitionId.UnpoweredWorkRobot, new(708, 144), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerLeaningRight)),
        new(EnemyDefinitionId.Atomic, new(160, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(288, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpLeftTurning, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(464, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurningBack, Speed: SignedSixteenSixteen.Zero)),
        new(EnemyDefinitionId.Atomic, new(592, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpLeftTurningBack, Speed: SignedSixteenSixteen.Zero)),
    ], 7);

    /// <summary>The enemy population of the Wrecked Ship Save room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipSaveDefault { get; } = new("WreckedShipSaveDefault",
    [
        new(EnemyDefinitionId.Coven, new(136, 136), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Sbug, new(131, 186), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(176), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(123, 181), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(202), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(142, 192), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(250), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(116, 196), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(131), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(185, 46), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(123), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(193, 53), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(224), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(186, 54), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(248), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(203, 64), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(24), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(197, 70), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(253), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, new(51, 171), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(136), ActivationRadius: 48, Behavior: SbugActivationBehavior.ZigZag)),
        new(EnemyDefinitionId.Sbug, new(56, 172), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(144), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
    ], 1);

    /// <summary>The enemy population of the Electric Death room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition ElectricDeathWhenAreaBossDead { get; } = new("ElectricDeathWhenAreaBossDead",
    [
        new(EnemyDefinitionId.Spark, new(104, 600), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 128)),
        new(EnemyDefinitionId.Spark, new(42, 551), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 128)),
        new(EnemyDefinitionId.Spark, new(200, 477), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 16)),
        new(EnemyDefinitionId.Spark, new(134, 394), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 144)),
        new(EnemyDefinitionId.Spark, new(79, 51), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 144)),
        new(EnemyDefinitionId.Spark, new(53, 218), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 16)),
        new(EnemyDefinitionId.Spark, new(92, 152), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 160)),
    ], 0);

    /// <summary>The enemy population of the Spiky Death room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition SpikyDeathWhenAreaBossDead { get; } = new("SpikyDeathWhenAreaBossDead",
    [
        new(EnemyDefinitionId.KzanTop, new(80, 96), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(4), FallWait: 16, FallDistance: 104)),
        new(EnemyDefinitionId.KzanBottom, new(80, 104), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(160, 96), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(4), FallWait: 12, FallDistance: 104)),
        new(EnemyDefinitionId.KzanBottom, new(160, 104), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(256, 96), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(4), FallWait: 17, FallDistance: 104)),
        new(EnemyDefinitionId.KzanBottom, new(256, 104), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(352, 96), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(4), FallWait: 10, FallDistance: 104)),
        new(EnemyDefinitionId.KzanBottom, new(352, 104), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(432, 96), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(4), FallWait: 20, FallDistance: 104)),
        new(EnemyDefinitionId.KzanBottom, new(432, 104), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Wrecked Ship East Super room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipEastSuperWhenAreaBossDead { get; } = new("WreckedShipEastSuperWhenAreaBossDead",
    [
        new(EnemyDefinitionId.WorkRobot, new(376, 128), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
        new(EnemyDefinitionId.WorkRobot, new(704, 128), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WorkRobotSpawnSettings(Pose: WorkRobotSegment.NoPowerNeutral)),
        new(EnemyDefinitionId.Bull, new(224, 120), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new BullSpawnSettings(Intervals: new BullSpeedIntervals(6, 2), MaximumSpeed: SignedEightEight.FromPixelFraction(1791, 256))),
        new(EnemyDefinitionId.Bull, new(528, 117), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new BullSpawnSettings(Intervals: new BullSpeedIntervals(6, 2), MaximumSpeed: SignedEightEight.FromPixelFraction(1791, 256))),
        new(EnemyDefinitionId.Spark, new(160, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 128)),
        new(EnemyDefinitionId.Spark, new(288, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 16)),
        new(EnemyDefinitionId.Spark, new(464, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 160)),
        new(EnemyDefinitionId.Spark, new(592, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 128)),
    ], 4);

    /// <summary>The enemy population of the Phantoon room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition PhantoonDefault { get; } = new("PhantoonDefault",
    [
        new(EnemyDefinitionId.PhantoonBody, new(128, 96), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.PhantoonEye, new(128, 96), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.PhantoonTentacles, new(128, 96), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.PhantoonMouth, new(128, 96), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Wrecked Ship Main Shaft room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition WreckedShipMainShaftWhenAreaBossDead { get; } = new("WreckedShipMainShaftWhenAreaBossDead",
    [
        new(EnemyDefinitionId.Spark, new(1176, 680), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 48)),
        new(EnemyDefinitionId.Spark, new(1136, 857), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 40)),
        new(EnemyDefinitionId.Spark, new(1056, 1164), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 32)),
        new(EnemyDefinitionId.Spark, new(1224, 1288), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 32)),
        new(EnemyDefinitionId.Spark, new(1160, 1468), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 24)),
        new(EnemyDefinitionId.Spark, new(1129, 1818), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.AlwaysActive, BaseActionTime: 0)),
        new(EnemyDefinitionId.Spark, new(1048, 1928), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Emitter, BaseActionTime: 48)),
        new(EnemyDefinitionId.Atomic, new(1144, 594), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurning, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Atomic, new(1237, 683), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpLeftTurning, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Atomic, new(1216, 1287), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurningBack, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Atomic, new(1062, 1465), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new AtomicSpawnSettings(Spin: AtomicSegment.UpRightTurningBack, Speed: SignedSixteenSixteen.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.KzanTop, new(608, 1368), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(2), FallWait: 48, FallDistance: 80)),
        new(EnemyDefinitionId.KzanBottom, new(608, 1376), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(448, 1368), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(2), FallWait: 48, FallDistance: 80)),
        new(EnemyDefinitionId.KzanBottom, new(448, 1376), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(288, 1368), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(2), FallWait: 48, FallDistance: 80)),
        new(EnemyDefinitionId.KzanBottom, new(288, 1376), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KzanTop, new(128, 1368), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromWholePixels(2), FallWait: 48, FallDistance: 80)),
        new(EnemyDefinitionId.KzanBottom, new(128, 1376), EnemyContact.None, EnemyProcessing.None, EnemyPresence.Hidden, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Spark, new(1232, 1992), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 48)),
        new(EnemyDefinitionId.Spark, new(1072, 1992), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SparkSpawnSettings(Start: SparkMovementDefinitions.Intermittent, BaseActionTime: 32)),
    ], 4);

    /// <summary>The Wrecked Ship rooms' lists, in their authored order.</summary>
    internal static RoomEnemyPopulationDefinition[] Lists { get; } =
    [
        SpongeBathWhenAreaBossDead,
        WreckedShipEnergyTankWhenAreaBossDead,
        WreckedShipMainShaftDefault,
        BowlingAlleyDefault,
        AtticWhenAreaBossDead,
        BasementWhenAreaBossDead,
        WreckedShipMapWhenAreaBossDead,
        WreckedShipWestSuperDefault,
        SpikyDeathDefault,
        SpongeBathDefault,
        WreckedShipEntranceWhenAreaBossDead,
        ElectricDeathDefault,
        BowlingAlleyWhenAreaBossDead,
        AssemblyLineDefault,
        PhantoonWhenAreaBossDead,
        WreckedShipEnergyTankDefault,
        GravitySuitDefault,
        WreckedShipMapDefault,
        GravitySuitWhenAreaBossDead,
        BasementDefault,
        WreckedShipEntranceDefault,
        AssemblyLineWhenAreaBossDead,
        WreckedShipSaveWhenAreaBossDead,
        WreckedShipWestSuperWhenAreaBossDead,
        AtticDefault,
        WreckedShipEastSuperDefault,
        WreckedShipSaveDefault,
        ElectricDeathWhenAreaBossDead,
        SpikyDeathWhenAreaBossDead,
        WreckedShipEastSuperWhenAreaBossDead,
        PhantoonDefault,
        WreckedShipMainShaftWhenAreaBossDead,
    ];
}
