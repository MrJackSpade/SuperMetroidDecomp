using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Enemies.Kago;
using SuperMetroid.Verification.Enemies.YappingMaws;
using SuperMetroid.Verification.Enemies.Shaktool;
using SuperMetroid.Verification.Enemies.Chozo;
using SuperMetroid.Verification.Enemies.Deaths;
using SuperMetroid.Verification.Enemies.Magdollite;
using SuperMetroid.Verification.Enemies.FuneNamihe;
using SuperMetroid.Verification.Enemies.Alcoon;
using SuperMetroid.Verification.Enemies.WorkRobots;
using SuperMetroid.Verification.Enemies.KiHunters;
using SuperMetroid.Verification.Enemies.FallingSparks;
using SuperMetroid.Verification.Enemies.Puyo;
using SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Verification.Enemies.Powamp;
using SuperMetroid.Verification.Enemies.Boyon;
using SuperMetroid.Verification.Enemies.Skultera;
using SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Enemies.Skree;
using SuperMetroid.Verification.Enemies.MorphBallEye;
using SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Enemies.Cacatac;
using SuperMetroid.Verification.Enemies.Stoke;
using SuperMetroid.Verification.Enemies.Owtch;
using SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Enemies.Hoppers;
using SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Enemies.LavaJumpers;
using SuperMetroid.Verification.Enemies.Waver;
using SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Enemies.Choot;
using SuperMetroid.Verification.Enemies.Boulder;
using SuperMetroid.Verification.Enemies.Bull;
using SuperMetroid.Verification.Enemies.Atomic;
using SuperMetroid.Verification.Enemies.Fireflea;
using SuperMetroid.Verification.Enemies.WreckedShip;
using SuperMetroid.Verification.Enemies.Sbug;
using SuperMetroid.Verification.Enemies.Polyp;
using SuperMetroid.Verification.Enemies.Nuclear;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Movement.AbsoluteTangentDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Alcoon.AlcoonInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Atomic.AtomicMovementDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.BeamCallbackTablesVerification;
using static SuperMetroid.Verification.Enemies.Beetom.BeetomInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.BlueDoorPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.BombSpreadLaunchDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoAttackDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoDroolSineVerification;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoMovementDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoStatueFragmentDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonHealthPaletteDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonNavigationDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonSpeedsVerification;
using static SuperMetroid.Verification.Enemies.Boulder.BoulderBounceDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Boyon.BoyonSpeedDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Bull.BullMovementDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Cacatac.CacatacMovementDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Cacatac.CacatacProjectileDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresDoorInitializationDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresDoorAnimationsVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresBabyAnimationsVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresDoorQuakeDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresFallingDebrisAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.CeresRidleyEyeFadeDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.CeresRidleyGetawayDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyAnimationsVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresSteamDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresSteamAnimationsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ChargeFlareDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ChargeFlarePlacementVerification;
using static SuperMetroid.Verification.Enemies.Choot.ChootPatternDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatuePlmDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.ColoredDoorPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.CompiledProjectileMathVerification;
using static SuperMetroid.Verification.Enemies.Crawlers.CrawlerAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Crawlers.CrawlerSpeedsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireBridgeFragmentDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireMeltingDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireRumbleDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Abilities.CrystalFlashPaletteTimingDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Animals.DachoraInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadSidehopperCorpseDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadSidehopperLaunchDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTorizoCorpseDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTourianCorpseDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.DownwardGatePlmsVerification;
using static SuperMetroid.Verification.Enemies.Dragon.DragonAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonBurialEvirDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonHealthPaletteDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidAnimationsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonIntroDanceDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Elevator.ElevatorInputDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyBreakableTerrainDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDeathExplosionDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropChanceDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyFireballLaunchDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyInstructionOwnerCoverageVerification;
using static SuperMetroid.Verification.Enemies.EnemyLinearSpeedsVerification;
using static SuperMetroid.Verification.Enemies.EnemyPickupDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Projectiles.EnemyProjectileDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Projectiles.EnemyProjectileInstructionOwnerCoverageVerification;
using static SuperMetroid.Verification.Enemies.EnemyQuadraticSpeedsVerification;
using static SuperMetroid.Verification.Enemies.EnemyVulnerabilityDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Animals.EscapeDachoraAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Animals.EscapeEtecoonAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Animals.EscapeEtecoonDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Animals.EtecoonInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Evir.EvirInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.EyeDoorPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidProjectileDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidProjectileInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.FallingSparks.FallingSparkLaunchDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.FamilyTrigonometryVerification;
using static SuperMetroid.Verification.Enemies.Fireflea.FirefleaMovementDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Flies.FlyAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Flies.FlyBehaviorVerification;
using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using static SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleBodyPlacementVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleCompiledMathVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleConnectionDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleFiringDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.GreyDoorPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Shutters.GrowingShutterDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Gunship.GunshipDustDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Gunship.GunshipDustAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Gunship.GunshipAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Gunship.GunshipMotionDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.HalfWaveAlgorithmsVerification;
using static SuperMetroid.Verification.Enemies.Hibashi.HibashiDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Hoppers.HopperAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroEggMotionDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Kago.KagoAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Kago.KagoBehaviorVerification;
using static SuperMetroid.Verification.Enemies.KiHunters.KiHunterInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.KiHunters.KiHunterMotionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidBodyContourVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidCeilingRockPositionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidHeadInstructionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidMouthHitboxesVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidMovementChoicesVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNailBounceVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNailContourVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNailSiblingVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRockLaunchDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidSinkScheduleVerification;
using static SuperMetroid.Verification.Enemies.Kzan.KzanAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Kzan.KzanBehaviorVerification;
using static SuperMetroid.Verification.Enemies.Rios.LowerNorfairRioInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Magdollite.MagdolliteInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.MamaTurtles.MamaTurtleInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.MaridiaSnails.MaridiaLargeSnailInstructionDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Metroids.MetroidBehaviorDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.MiscDustProjectileDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainBabyMetroidDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainDeathExplosionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainDoorFragmentDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainGlassPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainGlassShardDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainNeckSineVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainTopTubeInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainTurretDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.SpacePirates.NinjaSpacePirateInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.NoobTubePlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.LavaJumpers.NorfairLavaJumpDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Rios.NorfairRioInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Nuclear.NuclearWaffleDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Owtch.OwtchMovementDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonCasualFlamesVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonDeathExplosionsVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonFlameSpawnsVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonMotionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonPathVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonPatternsVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonSoundDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonTimersVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonWaveMathVerification;
using static SuperMetroid.Verification.Enemies.PipeBugs.PipeBugAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Platforms.PlatformInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Polyp.PolypLaunchDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Poses.PoseCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Poses.PoseDispatchDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Poses.PoseInputDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Poses.PoseProjectileOriginVerification;
using static SuperMetroid.Verification.Samus.Projectiles.PowerBombCallbackDefinitionsVerification;
using static SuperMetroid.Verification.Rendering.PowerBombShapeDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileCompositionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileDamageVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileFrameBindingsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileInstructionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileOriginsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileRadiiVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileVisualPartsVerification;
using static SuperMetroid.Verification.Enemies.Puyo.PuyoHopDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Puyo.PuyoInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Fx.QuicksandVerification;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyAttackChoicesVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyClawOffsetsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyDeathAccelerationVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyExplosionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyInertiaVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyMovementTargetsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyNeutralTailVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyPogoDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Rios.RioInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Rios.RioLaunchDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Ripper.RipperAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Ripper.RipperBehaviorVerification;
using static SuperMetroid.Verification.Fx.RoomPaletteFxDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationVerification;
using static SuperMetroid.Verification.Fx.RoomShakeDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.SpriteObjects.RoomSpriteObjectDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Movement.RunningCadenceVerification;
using static SuperMetroid.Verification.Samus.Drawing.SamusArmCannonDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Drawing.SamusAtmosphericAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Drawing.SamusAtmosphericEffectDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Movement.SamusBallBounceDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Death.SamusDeathExplosionTimingDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Hud.SamusHudDefinitionsVerification;
using static SuperMetroid.Verification.Samus.SamusImpulseDefinitionsVerification;
using static SuperMetroid.Verification.Samus.SamusIndexedSpeedsVerification;
using static SuperMetroid.Verification.Samus.Movement.SamusStandaloneSpeedsVerification;
using static SuperMetroid.Verification.Samus.Movement.SamusVerticalDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.SaveStationAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Sbug.SbugMovementDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Shaktool.ShaktoolInstructionDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Shaktool.ShaktoolSegmentDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using static SuperMetroid.Verification.Enemies.ShotCallbackDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Skree.SkreeMetareeAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SlopeSpeedDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.SpacePirates.SpacePirateProjectileInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Sparks.SparkMovementDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnProjectileDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoProjectileAnimationsVerification;
using static SuperMetroid.Verification.Samus.Movement.SquareSlopeDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.StationAccessPlmDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.StatueWalkDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.SuitPickupBeamCurveDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteSchedulingVerification;
using static SuperMetroid.Verification.Samus.Movement.SurfaceMotionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoInitializationDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoRandomizedProjectileDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianAccessPlmDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianEntranceStatueInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianStatueProjectileInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianStatueUnlockDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.SpacePirates.WalkingSpacePirateInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.SpacePirates.WallSpacePirateInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Waver.WaverAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotPaletteTimingDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.YappingMaws.YappingMawInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Yard.YardDirectionDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Yard.YardInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Yard.YardTurnDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Escape.ZebesEscapeExplosionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.ZebetiteDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Zoa.ZoaAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Zoa.ZoaAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Zoa.ZoaSpeedDefinitionsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Spatial;

/// <summary>Registers compiled enemy math and instruction checks against pinned cartridge data.</summary>
internal static class EnemyTrigonometryVerification
{
    /// <summary>
    /// The compiled enemy math, projectile and instruction-program family. Each suite loads its own
    /// retail ROM, so the full run schedules them independently; the sine products run last.
    /// </summary>
    internal static RegisteredSuite[] CompiledEnemyTrigonometrySuites() =>
    [
        new(nameof(VerifyRidleyPowerBombNeutralTail), () => VerifyRidleyPowerBombNeutralTail()),
        new(nameof(VerifyEightBitHalfWaveAlgorithm), () => VerifyEightBitHalfWaveAlgorithm(LoadRepositoryRom())),
        new(nameof(VerifyUnsignedHalfWaveAlgorithm), () => VerifyUnsignedHalfWaveAlgorithm(LoadRepositoryRom())),
        new(nameof(VerifyCompiledSignedTrigonometry), () => VerifyCompiledSignedTrigonometry(LoadRepositoryRom())),
        new(nameof(VerifyBombTorizoDroolSine), () => VerifyBombTorizoDroolSine(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainNeckSine), () => VerifyMotherBrainNeckSine(LoadRepositoryRom())),
        new(nameof(VerifyCompiledGrappleMath), () => VerifyCompiledGrappleMath(LoadRepositoryRom())),
        new(nameof(VerifyCompiledProjectileMath), () => VerifyCompiledProjectileMath(LoadRepositoryRom())),
        new(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables()),
        new(nameof(VerifyCompiledFamilyTrigonometry), () => VerifyCompiledFamilyTrigonometry(LoadRepositoryRom())),
        new(nameof(VerifyPhantoonWaveMath), () => VerifyPhantoonWaveMath(LoadRepositoryRom())),
        new(nameof(VerifyCompiledLinearEnemySpeeds), () => VerifyCompiledLinearEnemySpeeds(LoadRepositoryRom())),
        new(nameof(VerifyFirefleaMovementDefinitions), () => VerifyFirefleaMovementDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCacatacMovementDefinitions), () => VerifyCacatacMovementDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCacatacProjectileDefinitions), () => VerifyCacatacProjectileDefinitions(LoadRepositoryRom())),
        new(nameof(CacatacProjectileAnimationsVerification.VerifyCacatacProjectileAnimations), () => CacatacProjectileAnimationsVerification.VerifyCacatacProjectileAnimations(LoadRepositoryRom())),
        new(nameof(CacatacProjectileBehaviorVerification.VerifyCacatacProjectileBehavior), () => CacatacProjectileBehaviorVerification.VerifyCacatacProjectileBehavior(LoadRepositoryRom())),
        new(nameof(FallingSparkAnimationsVerification.VerifyFallingSparkAnimations), () => FallingSparkAnimationsVerification.VerifyFallingSparkAnimations(LoadRepositoryRom())),
        new(nameof(FallingSparkBehaviorVerification.VerifyFallingSparkBehavior), () => FallingSparkBehaviorVerification.VerifyFallingSparkBehavior(LoadRepositoryRom())),
        new(nameof(FuneNamiheFireballAnimationsVerification.VerifyFuneNamiheFireballAnimations), () => FuneNamiheFireballAnimationsVerification.VerifyFuneNamiheFireballAnimations(LoadRepositoryRom())),
        new(nameof(FuneNamiheFireballBehaviorVerification.VerifyFuneNamiheFireballBehavior), () => FuneNamiheFireballBehaviorVerification.VerifyFuneNamiheFireballBehavior(LoadRepositoryRom())),
        new(nameof(MagdolliteLavaAnimationsVerification.VerifyMagdolliteLavaAnimations), () => MagdolliteLavaAnimationsVerification.VerifyMagdolliteLavaAnimations(LoadRepositoryRom())),
        new(nameof(MagdolliteLavaBehaviorVerification.VerifyMagdolliteLavaBehavior), () => MagdolliteLavaBehaviorVerification.VerifyMagdolliteLavaBehavior(LoadRepositoryRom())),
        new(nameof(DragonFireballAnimationsVerification.VerifyDragonFireballAnimations), () => DragonFireballAnimationsVerification.VerifyDragonFireballAnimations(LoadRepositoryRom())),
        new(nameof(DragonFireballBehaviorVerification.VerifyDragonFireballBehavior), () => DragonFireballBehaviorVerification.VerifyDragonFireballBehavior(LoadRepositoryRom())),
        new(nameof(EyeDoorProjectileAnimationsVerification.VerifyEyeDoorProjectileAnimations), () => EyeDoorProjectileAnimationsVerification.VerifyEyeDoorProjectileAnimations(LoadRepositoryRom())),
        new(nameof(EyeDoorProjectileBehaviorVerification.VerifyEyeDoorProjectileBehavior), () => EyeDoorProjectileBehaviorVerification.VerifyEyeDoorProjectileBehavior(LoadRepositoryRom())),
        new(nameof(EyeDoorSweatAnimationsVerification.VerifyEyeDoorSweatAnimations), () => EyeDoorSweatAnimationsVerification.VerifyEyeDoorSweatAnimations(LoadRepositoryRom())),
        new(nameof(EyeDoorSweatBehaviorVerification.VerifyEyeDoorSweatBehavior), () => EyeDoorSweatBehaviorVerification.VerifyEyeDoorSweatBehavior(LoadRepositoryRom())),
        new(nameof(SkreeMetareeParticleAnimationsVerification.VerifySkreeMetareeParticleAnimations), () => SkreeMetareeParticleAnimationsVerification.VerifySkreeMetareeParticleAnimations(LoadRepositoryRom())),
        new(nameof(SkreeMetareeParticleBehaviorVerification.VerifySkreeMetareeParticleBehavior), () => SkreeMetareeParticleBehaviorVerification.VerifySkreeMetareeParticleBehavior(LoadRepositoryRom())),
        new(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(AlcoonFireballAnimationsVerification.VerifyAlcoonFireballAnimations), () => AlcoonFireballAnimationsVerification.VerifyAlcoonFireballAnimations(LoadRepositoryRom())),
        new(nameof(AlcoonFireballBehaviorVerification.VerifyAlcoonFireballBehavior), () => AlcoonFireballBehaviorVerification.VerifyAlcoonFireballBehavior(LoadRepositoryRom())),
        new(nameof(WorkRobotLaserAnimationsVerification.VerifyWorkRobotLaserAnimations), () => WorkRobotLaserAnimationsVerification.VerifyWorkRobotLaserAnimations(LoadRepositoryRom())),
        new(nameof(WorkRobotLaserBehaviorVerification.VerifyWorkRobotLaserBehavior), () => WorkRobotLaserBehaviorVerification.VerifyWorkRobotLaserBehavior(LoadRepositoryRom())),
        new(nameof(PowampSpikeAnimationsVerification.VerifyPowampSpikeAnimations), () => PowampSpikeAnimationsVerification.VerifyPowampSpikeAnimations(LoadRepositoryRom())),
        new(nameof(PowampSpikeBehaviorVerification.VerifyPowampSpikeBehavior), () => PowampSpikeBehaviorVerification.VerifyPowampSpikeBehavior(LoadRepositoryRom())),
        new(nameof(PolypRockAnimationsVerification.VerifyPolypRockAnimations), () => PolypRockAnimationsVerification.VerifyPolypRockAnimations(LoadRepositoryRom())),
        new(nameof(PolypRockBehaviorVerification.VerifyPolypRockBehavior), () => PolypRockBehaviorVerification.VerifyPolypRockBehavior(LoadRepositoryRom())),
        new(nameof(KiHunterAcidSpitAnimationsVerification.VerifyKiHunterAcidSpitAnimations), () => KiHunterAcidSpitAnimationsVerification.VerifyKiHunterAcidSpitAnimations(LoadRepositoryRom())),
        new(nameof(KiHunterAcidSpitBehaviorVerification.VerifyKiHunterAcidSpitBehavior), () => KiHunterAcidSpitBehaviorVerification.VerifyKiHunterAcidSpitBehavior(LoadRepositoryRom())),
        new(nameof(StokeProjectileAnimationsVerification.VerifyStokeProjectileAnimations), () => StokeProjectileAnimationsVerification.VerifyStokeProjectileAnimations(LoadRepositoryRom())),
        new(nameof(StokeProjectileBehaviorVerification.VerifyStokeProjectileBehavior), () => StokeProjectileBehaviorVerification.VerifyStokeProjectileBehavior(LoadRepositoryRom())),
        new(nameof(NuclearWaffleProjectileAnimationsVerification.VerifyNuclearWaffleProjectileAnimations), () => NuclearWaffleProjectileAnimationsVerification.VerifyNuclearWaffleProjectileAnimations(LoadRepositoryRom())),
        new(nameof(NuclearWaffleProjectileBehaviorVerification.VerifyNuclearWaffleProjectileBehavior), () => NuclearWaffleProjectileBehaviorVerification.VerifyNuclearWaffleProjectileBehavior(LoadRepositoryRom())),
        new(nameof(KagoBugProjectileAnimationsVerification.VerifyKagoBugProjectileAnimations), () => KagoBugProjectileAnimationsVerification.VerifyKagoBugProjectileAnimations(LoadRepositoryRom())),
        new(nameof(KagoBugProjectileBehaviorVerification.VerifyKagoBugProjectileBehavior), () => KagoBugProjectileBehaviorVerification.VerifyKagoBugProjectileBehavior(LoadRepositoryRom())),
        new(nameof(SuperMetroid.Verification.Enemies.SpacePirates.SpacePirateProjectileAnimationsVerification.VerifySpacePirateProjectileAnimations), () => SuperMetroid.Verification.Enemies.SpacePirates.SpacePirateProjectileAnimationsVerification.VerifySpacePirateProjectileAnimations(LoadRepositoryRom())),
        new(nameof(YappingMawBodyProjectileAnimationsVerification.VerifyYappingMawBodyProjectileAnimations), () => YappingMawBodyProjectileAnimationsVerification.VerifyYappingMawBodyProjectileAnimations(LoadRepositoryRom())),
        new(nameof(YappingMawBodyProjectileBehaviorVerification.VerifyYappingMawBodyProjectileBehavior), () => YappingMawBodyProjectileBehaviorVerification.VerifyYappingMawBodyProjectileBehavior(LoadRepositoryRom())),
        new(nameof(VerifyCrocomireProjectileAnimations), () => VerifyCrocomireProjectileAnimations(LoadRepositoryRom())),
        new(nameof(VerifyPhantoonFlameAnimations), () => VerifyPhantoonFlameAnimations(LoadRepositoryRom())),
        new(nameof(VerifyDraygonProjectileAnimations), () => VerifyDraygonProjectileAnimations(LoadRepositoryRom())),
        new(nameof(VerifyRidleyAnimations), () => VerifyRidleyAnimations(LoadRepositoryRom())),
        new(nameof(VerifySpacePirateProjectileInstructionProgramDefinitions), () => VerifySpacePirateProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCeresFallingDebrisAnimations), () => VerifyCeresFallingDebrisAnimations(LoadRepositoryRom())),
        new(nameof(SaveStationElectricityAnimationsVerification.VerifySaveStationElectricityAnimations), () => SaveStationElectricityAnimationsVerification.VerifySaveStationElectricityAnimations(LoadRepositoryRom())),
        new(nameof(SaveStationElectricityBehaviorVerification.VerifySaveStationElectricityBehavior), () => SaveStationElectricityBehaviorVerification.VerifySaveStationElectricityBehavior(LoadRepositoryRom())),
        new(nameof(EnemyDeathProjectileAnimationsVerification.VerifyEnemyDeathProjectileAnimations), () => EnemyDeathProjectileAnimationsVerification.VerifyEnemyDeathProjectileAnimations(LoadRepositoryRom())),
        new(nameof(EnemyDeathProjectileBehaviorVerification.VerifyEnemyDeathProjectileBehavior), EnemyDeathProjectileBehaviorVerification.VerifyEnemyDeathProjectileBehavior),
        new(nameof(ShaktoolProjectileAnimationsVerification.VerifyShaktoolProjectileAnimations), () => ShaktoolProjectileAnimationsVerification.VerifyShaktoolProjectileAnimations(LoadRepositoryRom())),
        new(nameof(ShaktoolProjectileBehaviorVerification.VerifyShaktoolProjectileBehavior), () => ShaktoolProjectileBehaviorVerification.VerifyShaktoolProjectileBehavior(LoadRepositoryRom())),
        new(nameof(ChozoTourianDustAnimationsVerification.VerifyChozoTourianDustAnimations), () => ChozoTourianDustAnimationsVerification.VerifyChozoTourianDustAnimations(LoadRepositoryRom())),
        new(nameof(ChozoTourianDustBehaviorVerification.VerifyChozoTourianDustBehavior), () => ChozoTourianDustBehaviorVerification.VerifyChozoTourianDustBehavior(LoadRepositoryRom())),
        new(nameof(VerifyTourianStatueProjectileInstructionProgramDefinitions), () => VerifyTourianStatueProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySporeSpawnAnimations), () => VerifySporeSpawnAnimations(LoadRepositoryRom())),
        new(nameof(VerifyTorizoAnimations), () => VerifyTorizoAnimations(LoadRepositoryRom())),
        new(nameof(VerifyTorizoProjectileAnimations), () => VerifyTorizoProjectileAnimations(LoadRepositoryRom())),
        new(nameof(VerifyBotwoonProjectileAnimations), () => VerifyBotwoonProjectileAnimations(LoadRepositoryRom())),
        new(nameof(VerifyEnemyProjectileInstructionOwnerCoverage), () => VerifyEnemyProjectileInstructionOwnerCoverage()),
        new(nameof(VerifyGunshipAnimations), () => VerifyGunshipAnimations(LoadRepositoryRom())),
        new(nameof(VerifyDraygonAnimations), () => VerifyDraygonAnimations(LoadRepositoryRom())),
        new(nameof(VerifyKraidPartAnimations), () => VerifyKraidPartAnimations(LoadRepositoryRom())),
        new(nameof(VerifyKraidRockAnimations), () => VerifyKraidRockAnimations(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainAnimations), () => VerifyMotherBrainAnimations(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainProjectileAnimations), () => VerifyMotherBrainProjectileAnimations(LoadRepositoryRom())),
        new(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyWallSpacePirateInstructionProgramDefinitions), () => VerifyWallSpacePirateInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNinjaSpacePirateInstructionProgramDefinitions), () => VerifyNinjaSpacePirateInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyInstructionOwnerCoverage), () => VerifyEnemyInstructionOwnerCoverage()),
        new(nameof(DownwardGateProjectileAnimationsVerification.VerifyDownwardGateProjectileAnimations), () => DownwardGateProjectileAnimationsVerification.VerifyDownwardGateProjectileAnimations(LoadRepositoryRom())),
        new(nameof(DownwardGateProjectileBehaviorVerification.VerifyDownwardGateProjectileBehavior), () => DownwardGateProjectileBehaviorVerification.VerifyDownwardGateProjectileBehavior(LoadRepositoryRom())),
        new(nameof(NoobTubeProjectileAnimationsVerification.VerifyNoobTubeProjectileAnimations), () => NoobTubeProjectileAnimationsVerification.VerifyNoobTubeProjectileAnimations(LoadRepositoryRom())),
        new(nameof(NoobTubeProjectileBehaviorVerification.VerifyNoobTubeProjectileBehavior), () => NoobTubeProjectileBehaviorVerification.VerifyNoobTubeProjectileBehavior(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainTopTubeInstructionProgramDefinitions), () => VerifyMotherBrainTopTubeInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(SuperMetroid.Verification.Areas.Tourian.TourianStatueProjectileAnimationsVerification.VerifyTourianStatueProjectileAnimations), () => SuperMetroid.Verification.Areas.Tourian.TourianStatueProjectileAnimationsVerification.VerifyTourianStatueProjectileAnimations(LoadRepositoryRom())),
        new(nameof(SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainTopTubeAnimationsVerification.VerifyMotherBrainTopTubeAnimations), () => SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainTopTubeAnimationsVerification.VerifyMotherBrainTopTubeAnimations(LoadRepositoryRom())),
        new(nameof(VerifyGunshipDustAnimations), () => VerifyGunshipDustAnimations(LoadRepositoryRom())),
        new(nameof(CacatacAnimationsVerification.VerifyCacatacAnimations), () => CacatacAnimationsVerification.VerifyCacatacAnimations(LoadRepositoryRom())),
        new(nameof(CacatacBehaviorVerification.VerifyCacatacBehavior), () => CacatacBehaviorVerification.VerifyCacatacBehavior()),
        new(nameof(VerifyMagdolliteInstructionProgramDefinitions), () => VerifyMagdolliteInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(OwtchAnimationsVerification.VerifyOwtchAnimations), () => OwtchAnimationsVerification.VerifyOwtchAnimations(LoadRepositoryRom())),
        new(nameof(OwtchBehaviorVerification.VerifyOwtchBehavior), () => OwtchBehaviorVerification.VerifyOwtchBehavior()),
        new(nameof(StokeAnimationsVerification.VerifyStokeAnimations), () => StokeAnimationsVerification.VerifyStokeAnimations(LoadRepositoryRom())),
        new(nameof(StokeBehaviorVerification.VerifyStokeBehavior), () => StokeBehaviorVerification.VerifyStokeBehavior()),
        new(nameof(VerifyRipperAnimations), () => VerifyRipperAnimations(LoadRepositoryRom())),
        new(nameof(VerifyRipperBehavior), () => VerifyRipperBehavior(LoadRepositoryRom())),
        new(nameof(VerifyKzanAnimations), () => VerifyKzanAnimations(LoadRepositoryRom())),
        new(nameof(VerifyKzanBehavior), () => VerifyKzanBehavior()),
        new(nameof(VerifyFlyAnimations), () => VerifyFlyAnimations(LoadRepositoryRom())),
        new(nameof(VerifyFlyBehavior), () => VerifyFlyBehavior()),
        new(nameof(BullAnimationsVerification.VerifyBullAnimations), () => BullAnimationsVerification.VerifyBullAnimations(LoadRepositoryRom())),
        new(nameof(BullBehaviorVerification.VerifyBullBehavior), () => BullBehaviorVerification.VerifyBullBehavior()),
        new(nameof(VerifyKagoAnimations), () => VerifyKagoAnimations(LoadRepositoryRom())),
        new(nameof(VerifyKagoBehavior), () => VerifyKagoBehavior()),
        new(nameof(ShutterAnimationsVerification.VerifyShutterAnimations), () => ShutterAnimationsVerification.VerifyShutterAnimations(LoadRepositoryRom())),
        new(nameof(ShutterBehaviorVerification.VerifyShutterBehavior), () => ShutterBehaviorVerification.VerifyShutterBehavior()),
        new(nameof(ShutterBehaviorVerification.VerifyShutterExportFrames), () => ShutterBehaviorVerification.VerifyShutterExportFrames(LoadRepositoryRom())),
        new(nameof(ShutterBehaviorVerification.VerifyVerticalShutterInitialFunctionSelection), () => ShutterBehaviorVerification.VerifyVerticalShutterInitialFunctionSelection(LoadRepositoryRom())),
        new(nameof(ShutterBehaviorVerification.VerifyHorizontalShutterInitialFunctionSelection), () => ShutterBehaviorVerification.VerifyHorizontalShutterInitialFunctionSelection(LoadRepositoryRom())),
        new(nameof(ChootAnimationsVerification.VerifyChootAnimations), () => ChootAnimationsVerification.VerifyChootAnimations(LoadRepositoryRom())),
        new(nameof(ChootBehaviorVerification.VerifyChootBehavior), () => ChootBehaviorVerification.VerifyChootBehavior(null)),
        new(nameof(NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations), () => NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations(LoadRepositoryRom())),
        new(nameof(NorfairLavaJumperBehaviorVerification.VerifyNorfairLavaJumperBehavior), () => NorfairLavaJumperBehaviorVerification.VerifyNorfairLavaJumperBehavior()),
        new(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(MultiviolaAnimationsVerification.VerifyMultiviolaAnimations), () => MultiviolaAnimationsVerification.VerifyMultiviolaAnimations(LoadRepositoryRom())),
        new(nameof(MultiviolaBehaviorVerification.VerifyMultiviolaBehavior), () => MultiviolaBehaviorVerification.VerifyMultiviolaBehavior()),
        new(nameof(PolypAnimationsVerification.VerifyPolypAnimations), () => PolypAnimationsVerification.VerifyPolypAnimations(LoadRepositoryRom())),
        new(nameof(PolypBehaviorVerification.VerifyPolypBehavior), () => PolypBehaviorVerification.VerifyPolypBehavior()),
        new(nameof(PowampAnimationsVerification.VerifyPowampAnimations), () => PowampAnimationsVerification.VerifyPowampAnimations(LoadRepositoryRom())),
        new(nameof(PowampBehaviorVerification.VerifyPowampBehavior), () => PowampBehaviorVerification.VerifyPowampBehavior(LoadRepositoryRom())),
        new(nameof(WreckedShipGhostAnimationsVerification.VerifyWreckedShipGhostAnimations), () => WreckedShipGhostAnimationsVerification.VerifyWreckedShipGhostAnimations(LoadRepositoryRom())),
        new(nameof(WreckedShipGhostBehaviorVerification.VerifyWreckedShipGhostBehavior), () => WreckedShipGhostBehaviorVerification.VerifyWreckedShipGhostBehavior()),
        new(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions()),
        new(nameof(PuyoAnimationsVerification.VerifyPuyoAnimations), () => PuyoAnimationsVerification.VerifyPuyoAnimations(LoadRepositoryRom())),
        new(nameof(DeadTorizoAnimationsVerification.VerifyDeadTorizoAnimations), () => DeadTorizoAnimationsVerification.VerifyDeadTorizoAnimations(LoadRepositoryRom())),
        new(nameof(DeadTorizoBehaviorVerification.VerifyDeadTorizoBehavior), () => DeadTorizoBehaviorVerification.VerifyDeadTorizoBehavior()),
        new(nameof(DeadTourianAnimationsVerification.VerifyDeadTourianAnimations), () => DeadTourianAnimationsVerification.VerifyDeadTourianAnimations(LoadRepositoryRom())),
        new(nameof(DeadTourianBehaviorVerification.VerifyDeadSidehopperBehavior), () => DeadTourianBehaviorVerification.VerifyDeadSidehopperBehavior()),
        new(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyAtomicMovementDefinitions), () => VerifyAtomicMovementDefinitions(LoadRepositoryRom())),
        new(nameof(AtomicAnimationsVerification.VerifyAtomicAnimations), () => AtomicAnimationsVerification.VerifyAtomicAnimations(LoadRepositoryRom())),
        new(nameof(VerifySbugMovementDefinitions), () => VerifySbugMovementDefinitions(LoadRepositoryRom())),
        new(nameof(SbugAnimationsVerification.VerifySbugAnimations), () => SbugAnimationsVerification.VerifySbugAnimations(LoadRepositoryRom())),
        new(nameof(VerifySparkMovementDefinitions), () => VerifySparkMovementDefinitions(LoadRepositoryRom())),
        new(nameof(SparkAnimationsVerification.VerifySparkAnimations), () => SparkAnimationsVerification.VerifySparkAnimations(LoadRepositoryRom())),
        new(nameof(VerifyElevatorInputDefinitions), () => VerifyElevatorInputDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyOwtchMovementDefinitions), () => VerifyOwtchMovementDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNuclearWaffleDefinitions), () => VerifyNuclearWaffleDefinitions(LoadRepositoryRom())),
        new(nameof(NuclearWaffleAnimationsVerification.VerifyNuclearWaffleAnimations), () => NuclearWaffleAnimationsVerification.VerifyNuclearWaffleAnimations(LoadRepositoryRom())),
        new(nameof(VerifyHibashiDefinitions), () => VerifyHibashiDefinitions(LoadRepositoryRom())),
        new(nameof(BlueBrinstarFaceBlockAnimationsVerification.VerifyBlueBrinstarFaceBlockAnimations), () => BlueBrinstarFaceBlockAnimationsVerification.VerifyBlueBrinstarFaceBlockAnimations(LoadRepositoryRom())),
        new(nameof(BlueBrinstarFaceBlockBehaviorVerification.VerifyBlueBrinstarFaceBlockBehavior), () => BlueBrinstarFaceBlockBehaviorVerification.VerifyBlueBrinstarFaceBlockBehavior()),
        new(nameof(VerifyFuneNamiheDefinitions), () => VerifyFuneNamiheDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMiscDustProjectileDefinitions), () => VerifyMiscDustProjectileDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(HopperAnimationsVerification.VerifyHopperAnimations), () => HopperAnimationsVerification.VerifyHopperAnimations(LoadRepositoryRom())),
        new(nameof(VerifyChootPatternDefinitions), () => VerifyChootPatternDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCrawlerAnimationDefinitions), () => VerifyCrawlerAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(CrawlerAnimationsVerification.VerifyCrawlerAnimations), () => CrawlerAnimationsVerification.VerifyCrawlerAnimations(LoadRepositoryRom())),
        new(nameof(CrawlerBehaviorVerification.VerifyCrawlerBehavior), () => CrawlerBehaviorVerification.VerifyCrawlerBehavior()),
        new(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(FirefleaAnimationsVerification.VerifyFirefleaAnimations), () => FirefleaAnimationsVerification.VerifyFirefleaAnimations(LoadRepositoryRom())),
        new(nameof(FirefleaBehaviorVerification.VerifyFirefleaBehavior), () => FirefleaBehaviorVerification.VerifyFirefleaBehavior()),
        new(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(MorphBallEyeAnimationsVerification.VerifyMorphBallEyeAnimations), () => MorphBallEyeAnimationsVerification.VerifyMorphBallEyeAnimations(LoadRepositoryRom())),
        new(nameof(MorphBallEyeBehaviorVerification.VerifyMorphBallEyeBehavior), () => MorphBallEyeBehaviorVerification.VerifyMorphBallEyeBehavior()),
        new(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(MetroidAnimationsVerification.VerifyMetroidAnimations), () => MetroidAnimationsVerification.VerifyMetroidAnimations(LoadRepositoryRom())),
        new(nameof(MetroidBehaviorVerification.VerifyMetroidBehavior), () => MetroidBehaviorVerification.VerifyMetroidBehavior()),
        new(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPhantoonPartAnimations), () => VerifyPhantoonPartAnimations(LoadRepositoryRom())),
        new(nameof(VerifyWorkRobotInstructionProgramDefinitions), () => VerifyWorkRobotInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNorfairLavaJumpDefinitions), () => VerifyNorfairLavaJumpDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCompiledQuadraticEnemySpeeds), () => VerifyCompiledQuadraticEnemySpeeds(LoadRepositoryRom())),
        new(nameof(VerifyCompiledBullMovement), () => VerifyCompiledBullMovement(LoadRepositoryRom())),
        new(nameof(VerifyPowerBombCallbackDefinitions), () => VerifyPowerBombCallbackDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyShotCallbackDefinitions), () => VerifyShotCallbackDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPuyoHops), () => VerifyCompiledPuyoHops(LoadRepositoryRom())),
        new(nameof(VerifyCompiledBotwoonSpeeds), () => VerifyCompiledBotwoonSpeeds(LoadRepositoryRom())),
        new(nameof(VerifyBotwoonNavigationDefinitions), () => VerifyBotwoonNavigationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBotwoonAnimationSelection), () => VerifyBotwoonAnimationSelection(LoadRepositoryRom())),
        new(nameof(VerifyBotwoonHeadAnimations), () => VerifyBotwoonHeadAnimations(LoadRepositoryRom())),
        new(nameof(VerifyBotwoonHealthPaletteDefinitions), () => VerifyBotwoonHealthPaletteDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCompiledCrawlerSpeeds), () => VerifyCompiledCrawlerSpeeds(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPolypLaunchDefinitions), () => VerifyCompiledPolypLaunchDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyShaktoolSegmentDefinitions), () => VerifyShaktoolSegmentDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyShaktoolInstructionDefinitions), () => VerifyShaktoolInstructionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyShaktoolInstructionProgramDefinitions), () => VerifyShaktoolInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySporeSpawnProjectileDefinitions), () => VerifySporeSpawnProjectileDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySamusAtmosphericEffectDefinitions), () => VerifySamusAtmosphericEffectDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySamusAtmosphericAnimationDefinitions), () => VerifySamusAtmosphericAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCrystalFlashPaletteTimingDefinitions), () => VerifyCrystalFlashPaletteTimingDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyWorkRobotPaletteTimingDefinitions), () => VerifyWorkRobotPaletteTimingDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySamusDeathExplosionTimingDefinitions), () => VerifySamusDeathExplosionTimingDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySuitPickupBeamCurveDefinitions), () => VerifySuitPickupBeamCurveDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyQuicksandDefinitions), () => VerifyQuicksandDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySaveStationAnimationDefinitions), () => VerifySaveStationAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyZebesEscapeExplosionDefinitions), () => VerifyZebesEscapeExplosionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMetroidBehaviorDefinitions), () => VerifyMetroidBehaviorDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainDoorFragmentDefinitions), () => VerifyMotherBrainDoorFragmentDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainTurretDefinitions), () => VerifyMotherBrainTurretDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainGlassShardDefinitions), () => VerifyMotherBrainGlassShardDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTourianStatueUnlockDefinitions), () => VerifyTourianStatueUnlockDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTourianAccessPlmDefinitions), () => VerifyTourianAccessPlmDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyChozoStatuePlmDefinitions), () => VerifyChozoStatuePlmDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySamusEaterPlmDefinitions), () => VerifySamusEaterPlmDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyStationAccessPlmDefinitions), () => VerifyStationAccessPlmDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMaridiaLargeSnailInstructionDefinitions), () => VerifyMaridiaLargeSnailInstructionDefinitions(LoadRepositoryRom())),
        new(nameof(MaridiaLargeSnailAnimationsVerification.VerifyMaridiaLargeSnailAnimations), () => MaridiaLargeSnailAnimationsVerification.VerifyMaridiaLargeSnailAnimations(LoadRepositoryRom())),
        new(nameof(VerifyEtecoonInstructionProgramDefinitions), () => VerifyEtecoonInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(ElevatorAnimationsVerification.VerifyElevatorAnimations), () => ElevatorAnimationsVerification.VerifyElevatorAnimations(LoadRepositoryRom())),
        new(nameof(ElevatorBehaviorVerification.VerifyElevatorBehavior), () => ElevatorBehaviorVerification.VerifyElevatorBehavior()),
        new(nameof(MochtroidAnimationsVerification.VerifyMochtroidAnimations), () => MochtroidAnimationsVerification.VerifyMochtroidAnimations(LoadRepositoryRom())),
        new(nameof(MochtroidBehaviorVerification.VerifyMochtroidBehavior), () => MochtroidBehaviorVerification.VerifyMochtroidBehavior()),
        new(nameof(VerifyPlatformInstructionProgramDefinitions), () => VerifyPlatformInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyBreakableTerrainDefinitions), () => VerifyEnemyBreakableTerrainDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainDeathExplosionDefinitions), () => VerifyMotherBrainDeathExplosionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyWaverAnimationDefinitions), () => VerifyWaverAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(WaverAnimationsVerification.VerifyWaverAnimations), () => WaverAnimationsVerification.VerifyWaverAnimations(LoadRepositoryRom())),
        new(nameof(WaverBehaviorVerification.VerifyWaverBehavior), () => WaverBehaviorVerification.VerifyWaverBehavior()),
        new(nameof(VerifySkreeMetareeAnimationDefinitions), () => VerifySkreeMetareeAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(SkreeMetareeAnimationsVerification.VerifySkreeMetareeAnimations), () => SkreeMetareeAnimationsVerification.VerifySkreeMetareeAnimations(LoadRepositoryRom())),
        new(nameof(SkreeMetareeBehaviorVerification.VerifySkreeMetareeBehavior), () => SkreeMetareeBehaviorVerification.VerifySkreeMetareeBehavior()),
        new(nameof(VerifyZoaAnimationDefinitions), () => VerifyZoaAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyZoaAnimations), () => VerifyZoaAnimations(LoadRepositoryRom())),
        new(nameof(VerifyDragonAnimationDefinitions), () => VerifyDragonAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(DragonAnimationsVerification.VerifyDragonAnimations), () => DragonAnimationsVerification.VerifyDragonAnimations(LoadRepositoryRom())),
        new(nameof(DragonBehaviorVerification.VerifyDragonBehavior), () => DragonBehaviorVerification.VerifyDragonBehavior()),
        new(nameof(VerifyEnemyPickupDefinitions), () => VerifyEnemyPickupDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyDropChanceDefinitions), () => VerifyEnemyDropChanceDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyVulnerabilityDefinitions), () => VerifyEnemyVulnerabilityDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDownwardGateShotBlockDefinitions), () => VerifyDownwardGateShotBlockDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySpeedBoosterEscapeStageDefinitions), () => VerifySpeedBoosterEscapeStageDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDoorClosingPlmDefinitions), () => VerifyDoorClosingPlmDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBlueDoorPlmDrawDefinitions), () => VerifyBlueDoorPlmDrawDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyColoredDoorPlmDrawDefinitions), () => VerifyColoredDoorPlmDrawDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyGreyDoorPlmDrawDefinitions), () => VerifyGreyDoorPlmDrawDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEyeDoorPlmDrawDefinitions), () => VerifyEyeDoorPlmDrawDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainGlassPlmDrawDefinitions), () => VerifyMotherBrainGlassPlmDrawDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNoobTubePlmDrawDefinitions), () => VerifyNoobTubePlmDrawDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySamusArmCannonDefinitions), () => VerifySamusArmCannonDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyDeathExplosionDefinitions), () => VerifyEnemyDeathExplosionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyProjectileDefinitions), () => VerifyEnemyProjectileDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyRoomPaletteFxDefinitions), () => VerifyRoomPaletteFxDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyRoomSpriteObjectDefinitions), () => VerifyRoomSpriteObjectDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCeresSteamDefinitions), () => VerifyCeresSteamDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCeresSteamAnimations), () => VerifyCeresSteamAnimations(LoadRepositoryRom())),
        new(nameof(VerifyCeresDoorAnimations), () => VerifyCeresDoorAnimations(LoadRepositoryRom())),
        new(nameof(VerifyCeresBabyAnimations), () => VerifyCeresBabyAnimations(LoadRepositoryRom())),
        new(nameof(VerifyPipeBugAnimationDefinitions), () => VerifyPipeBugAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(PipeBugAnimationsVerification.VerifyPipeBugAnimations), () => PipeBugAnimationsVerification.VerifyPipeBugAnimations(LoadRepositoryRom())),
        new(nameof(PipeBugBehaviorVerification.VerifyPipeBugBehavior), () => PipeBugBehaviorVerification.VerifyPipeBugBehavior()),
        new(nameof(VerifyDraygonBurialEvirDefinitions), () => VerifyDraygonBurialEvirDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDraygonHealthPaletteDefinitions), () => VerifyDraygonHealthPaletteDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFakeKraidProjectileDefinitions), () => VerifyFakeKraidProjectileDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCrocomireTongueAnimations), () => VerifyCrocomireTongueAnimations(LoadRepositoryRom())),
        new(nameof(VerifyCrocomireBodyAnimations), () => VerifyCrocomireBodyAnimations(LoadRepositoryRom())),
        new(nameof(VerifyEscapeEtecoonDefinitions), () => VerifyEscapeEtecoonDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEscapeEtecoonAnimations), () => VerifyEscapeEtecoonAnimations(LoadRepositoryRom())),
        new(nameof(VerifyEscapeEtecoonProgramBehavior), () => VerifyEscapeEtecoonProgramBehavior(LoadRepositoryRom())),
        new(nameof(VerifyEscapeDachoraAnimations), () => VerifyEscapeDachoraAnimations(LoadRepositoryRom())),
        new(nameof(VerifyZebetiteDefinitions), () => VerifyZebetiteDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainBabyMetroidDefinitions), () => VerifyMotherBrainBabyMetroidDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyYardDirectionDefinitions), () => VerifyYardDirectionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyYardTurnDefinitions), () => VerifyYardTurnDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBombTorizoStatueFragmentDefinitions), () => VerifyBombTorizoStatueFragmentDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTorizoRandomizedProjectileDefinitions), () => VerifyTorizoRandomizedProjectileDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBombTorizoAttackDefinitions), () => VerifyBombTorizoAttackDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBombTorizoMovementDefinitions), () => VerifyBombTorizoMovementDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCrocomireRumbleDefinitions), () => VerifyCrocomireRumbleDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCrocomireBridgeFragmentDefinitions), () => VerifyCrocomireBridgeFragmentDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyRoomShakeDefinitions), () => VerifyRoomShakeDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCeresDoorQuakeDefinitions), () => VerifyCeresDoorQuakeDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCeresDoorInitializationDefinitions), () => VerifyCeresDoorInitializationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCeresRidleyEyeFadeDefinitions), () => VerifyCeresRidleyEyeFadeDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCompiledCeresRidleyGetaway), () => VerifyCompiledCeresRidleyGetaway(LoadRepositoryRom())),
        new(nameof(VerifyRidleyExplosionDefinitions), () => VerifyRidleyExplosionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCrocomireMeltingDefinitions), () => VerifyCrocomireMeltingDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDraygonIntroDanceDefinitions), () => VerifyDraygonIntroDanceDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCompiledBoyonSpeeds), () => VerifyCompiledBoyonSpeeds(LoadRepositoryRom())),
        new(nameof(BoyonAnimationsVerification.VerifyBoyonAnimations), () => BoyonAnimationsVerification.VerifyBoyonAnimations(LoadRepositoryRom())),
        new(nameof(BoyonBehaviorVerification.VerifyBoyonBehavior), () => BoyonBehaviorVerification.VerifyBoyonBehavior()),
        new(nameof(SkulteraAnimationsVerification.VerifySkulteraAnimations), () => SkulteraAnimationsVerification.VerifySkulteraAnimations(LoadRepositoryRom())),
        new(nameof(SkulteraBehaviorVerification.VerifySkulteraBehavior), () => SkulteraBehaviorVerification.VerifySkulteraBehavior()),
        new(nameof(VerifyCompiledSurfaceMotion), () => VerifyCompiledSurfaceMotion(LoadRepositoryRom())),
        new(nameof(VerifyCompiledEnemyFireballLaunches), () => VerifyCompiledEnemyFireballLaunches(LoadRepositoryRom())),
        new(nameof(VerifyCompiledRioLaunches), () => VerifyCompiledRioLaunches(LoadRepositoryRom())),
        new(nameof(VerifyCompiledBoulderBounces), () => VerifyCompiledBoulderBounces(LoadRepositoryRom())),
        new(nameof(BoulderAnimationsVerification.VerifyBoulderAnimations), () => BoulderAnimationsVerification.VerifyBoulderAnimations(LoadRepositoryRom())),
        new(nameof(BoulderBehaviorVerification.VerifyBoulderBehavior), () => BoulderBehaviorVerification.VerifyBoulderBehavior()),
        new(nameof(VerifyCompiledZoaSpeeds), () => VerifyCompiledZoaSpeeds(LoadRepositoryRom())),
        new(nameof(VerifyCompiledGrowingShutters), () => VerifyCompiledGrowingShutters(LoadRepositoryRom())),
        new(nameof(VerifyCompiledIntroEggMotion), () => VerifyCompiledIntroEggMotion(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPowerBombShape), () => VerifyCompiledPowerBombShape(LoadRepositoryRom())),
        new(nameof(VerifyCompiledAbsoluteTangent), () => VerifyCompiledAbsoluteTangent(LoadRepositoryRom())),
        new(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(LoadRepositoryRom())),
        new(nameof(VerifyCompiledRidleyInertia), () => VerifyCompiledRidleyInertia(LoadRepositoryRom())),
        new(nameof(VerifyRidleyDeathAcceleration), () => VerifyRidleyDeathAcceleration(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPhantoonMotion), () => VerifyCompiledPhantoonMotion(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPhantoonFlameSpawns), () => VerifyCompiledPhantoonFlameSpawns(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPhantoonTimers), () => VerifyCompiledPhantoonTimers(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPhantoonCasualFlames), () => VerifyCompiledPhantoonCasualFlames(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPhantoonPatterns), () => VerifyCompiledPhantoonPatterns(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPhantoonPath), () => VerifyCompiledPhantoonPath(LoadRepositoryRom())),
        new(nameof(VerifyCompiledPhantoonDeathExplosions), () => VerifyCompiledPhantoonDeathExplosions(LoadRepositoryRom())),
        new(nameof(VerifyPhantoonSoundDefinitions), () => VerifyPhantoonSoundDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCompiledSlopeHeights), () => VerifyCompiledSlopeHeights(LoadRepositoryRom())),
        new(nameof(VerifyCompiledSlopeSpeeds), () => VerifyCompiledSlopeSpeeds(LoadRepositoryRom())),
        new(nameof(VerifyCompiledSquareSlopes), () => VerifyCompiledSquareSlopes(LoadRepositoryRom())),
        new(nameof(VerifyCompiledTorizoInitialization), () => VerifyCompiledTorizoInitialization(LoadRepositoryRom())),
        new(nameof(VerifyCompiledGunshipDust), () => VerifyCompiledGunshipDust(LoadRepositoryRom())),
        new(nameof(VerifyGunshipMotionDefinitions), () => VerifyGunshipMotionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCompiledRidleyPogo), () => VerifyCompiledRidleyPogo(LoadRepositoryRom())),
        new(nameof(VerifyRidleyMovementTargets), () => VerifyRidleyMovementTargets(LoadRepositoryRom())),
        new(nameof(VerifyRidleyAttackChoices), () => VerifyRidleyAttackChoices(LoadRepositoryRom())),
        new(nameof(VerifyRidleyClawOffsets), () => VerifyRidleyClawOffsets(LoadRepositoryRom())),
        new(nameof(VerifyFallingSparkLaunchDefinitions), () => VerifyFallingSparkLaunchDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDeadSidehopperLaunchDefinitions), () => VerifyDeadSidehopperLaunchDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDeadSidehopperCorpseDefinitions), () => VerifyDeadSidehopperCorpseDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDeadTourianCorpseDefinitions), () => VerifyDeadTourianCorpseDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDeadTorizoCorpseDefinitions), () => VerifyDeadTorizoCorpseDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKiHunterMotionDefinitions), () => VerifyKiHunterMotionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKraidRockLaunchDefinitions), () => VerifyKraidRockLaunchDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKraidMovementChoices), () => VerifyKraidMovementChoices(LoadRepositoryRom())),
        new(nameof(VerifyKraidBodyContour), () => VerifyKraidBodyContour(LoadRepositoryRom())),
        new(nameof(VerifyKraidNailSibling), () => VerifyKraidNailSibling(LoadRepositoryRom())),
        new(nameof(VerifyKraidNailBounce), () => VerifyKraidNailBounce()),
        new(nameof(VerifyKraidCeilingRockPositions), () => VerifyKraidCeilingRockPositions(LoadRepositoryRom())),
        new(nameof(VerifyKraidSinkSchedule), () => VerifyKraidSinkSchedule(LoadRepositoryRom())),
        new(nameof(VerifyKraidHeadInstructionDefinitions), () => VerifyKraidHeadInstructionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKraidMouthHitboxes), () => VerifyKraidMouthHitboxes(LoadRepositoryRom())),
        new(nameof(VerifyKraidNailContour), () => VerifyKraidNailContour(LoadRepositoryRom())),
        new(nameof(VerifySamusVerticalDefinitions), () => VerifySamusVerticalDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySamusImpulseDefinitions), () => VerifySamusImpulseDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySamusBallBounceDefinitions), () => VerifySamusBallBounceDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySamusStandaloneSpeeds), () => VerifySamusStandaloneSpeeds(LoadRepositoryRom())),
        new(nameof(VerifySamusIndexedSpeeds), () => VerifySamusIndexedSpeeds(LoadRepositoryRom())),
        new(nameof(VerifyGrappleFiringDefinitions), () => VerifyGrappleFiringDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyGrappleConnectionDefinitions), () => VerifyGrappleConnectionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySamusHudDefinitions), () => VerifySamusHudDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyGrappleBodyPlacement), () => VerifyGrappleBodyPlacement(LoadRepositoryRom())),
        new(nameof(VerifyRunningCadence), () => VerifyRunningCadence(LoadRepositoryRom())),
        new(nameof(VerifyPoseProjectileOrigin), () => VerifyPoseProjectileOrigin(LoadRepositoryRom())),
        new(nameof(VerifyPoseCollisionDefinitions), () => VerifyPoseCollisionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPoseDispatchDefinitions), () => VerifyPoseDispatchDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPoseInputDefinitions), () => VerifyPoseInputDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBombSpreadLaunchDefinitions), () => VerifyBombSpreadLaunchDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyProjectileDamage), () => VerifyProjectileDamage(LoadRepositoryRom())),
        new(nameof(VerifyProjectileRadii), () => VerifyProjectileRadii(LoadRepositoryRom())),
        new(nameof(VerifyProjectileInstructions), () => VerifyProjectileInstructions(LoadRepositoryRom())),
        new(nameof(VerifyProjectileFrameBindings), () => VerifyProjectileFrameBindings(LoadRepositoryRom())),
        new(nameof(VerifyChargeFlareDefinitions), () => VerifyChargeFlareDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyChargeFlarePlacement), () => VerifyChargeFlarePlacement(LoadRepositoryRom())),
        new(nameof(VerifyProjectileVisualParts), () => VerifyProjectileVisualParts()),
        new(nameof(VerifyProjectileCompositions), () => VerifyProjectileCompositions(LoadRepositoryRom())),
        new(nameof(VerifyProjectileOrigins), () => VerifyProjectileOrigins(LoadRepositoryRom())),
        new(nameof(VerifyCompiledEnemySine), () => VerifyCompiledEnemySine()),
    ];

    /// <summary>Runs the compiled enemy math and projectile suites serially in registry order.</summary>
    internal static void VerifyCompiledEnemyTrigonometry() => RunSuitesSerially(CompiledEnemyTrigonometrySuites());

    /// <summary>Checks byte and unsigned sine products, phase shifts, truncation, and Sbug vectors against retail tables.</summary>
    private static void VerifyCompiledEnemySine()
    {
        var rom = LoadRepositoryRom();
        var bytes = new byte[128];
        var words = new ushort[128];
        for (int i = 0; i < 128; i++)
        {
            bytes[i] = rom.ReadCartridgeByte(EnemyMathReferenceData.ByteSine + i);
            int address = EnemyMathReferenceData.UnsignedSine + i * 2;
            words[i] = rom.ReadCartridgeWord(address);
        }

        var pixel = new Func<ushort, ushort, int>((angle, radius) => EnemyMath.ReadEightBitSineProduct(new ByteAngle(unchecked((byte)angle)), radius));
        var fixedProduct = new Func<ushort, ushort, SignedSixteenSixteen>((angle, radius) => EnemyMath.ReadEightBitSineFixedProduct(new ByteAngle(unchecked((byte)angle)), radius));
        var cosine = new Func<ushort, ushort, int>((angle, radius) => EnemyMath.ReadEightBitCosineProduct(new ByteAngle(unchecked((byte)angle)), radius));
        var negative = new Func<ushort, ushort, int>((angle, radius) => EnemyMath.ReadEightBitNegativeSineProduct(new ByteAngle(unchecked((byte)angle)), radius));
        var fixedCosine = new Func<ushort, ushort, SignedSixteenSixteen>((angle, radius) => EnemyMath.ReadEightBitCosineFixedProduct(new ByteAngle(unchecked((byte)angle)), radius));
        var fixedNegative = new Func<ushort, ushort, SignedSixteenSixteen>((angle, radius) => EnemyMath.ReadEightBitNegativeSineFixedProduct(new ByteAngle(unchecked((byte)angle)), radius));
        var sbugSigned = new Func<byte, byte, int, SignedSixteenSixteen>((angle, speed, phase) => SbugBehavior.CalculateSignedSbugComponent(new ByteAngle(angle), speed, phase));
        var sbugUnsigned = new Func<byte, byte, int, UnsignedSixteenSixteen>((angle, speed, phase) => SbugBehavior.CalculateUnsignedSbugMagnitude(new ByteAngle(angle), speed, phase));
        var unsigned = new Func<ushort, ushort, ushort, int>((angle, magnitude, offset) => EnemyMath.ReadUnsignedSineMagnitudeProduct(new ByteAngle(unchecked((byte)angle)), magnitude, offset));

        int ExpectedPixel(int angle, int radius)
        {
            angle &= 255;
            int magnitude = EightEight.Multiply(bytes[angle & 127], unchecked((byte)radius));
            return angle < 128 ? magnitude : -magnitude;
        }
        for (int angle = 0; angle < 256; angle++)
        for (int radius = 0; radius < 256; radius++)
        {
            // Poison high bytes to prove native byte truncation remains in the
            // production helpers. The fractional negation deliberately has no carry.
            ushort a = (ushort)(0xff00 | angle), r = (ushort)(0xab00 | radius);
            int product = bytes[angle & 127] * radius;
            var expected = SignedSixteenSixteen.FromParts((short)(angle < 128 ? EightEight.WholeOf(product) : -EightEight.WholeOf(product)),
                unchecked((ushort)(angle < 128 ? SixteenSixteen.FromEightEight(product) : -SixteenSixteen.FromEightEight(product))));
            if (pixel(a, r) != ExpectedPixel(angle, radius) || fixedProduct(a, r) != expected ||
                cosine(a, r) != ExpectedPixel(angle + 64, radius) || negative(a, r) != ExpectedPixel(angle + 128, radius))
                throw new InvalidDataException($"Compiled byte sine result differs at angle={angle}, radius={radius}.");
            foreach (int phase in new[] { 64, 128 })
            {
                int shiftedAngle = unchecked((byte)(angle + phase));
                int shiftedProduct = bytes[shiftedAngle & 127] * radius;
                short whole = (short)(shiftedAngle < 128 ? EightEight.WholeOf(shiftedProduct) : -EightEight.WholeOf(shiftedProduct));
                ushort fraction = unchecked((ushort)(shiftedAngle < 128 ? SixteenSixteen.FromEightEight(shiftedProduct) : -SixteenSixteen.FromEightEight(shiftedProduct)));
                var fixedActual = phase == 64 ? fixedCosine(a, r) : fixedNegative(a, r);
                var signedActual = sbugSigned((byte)angle, (byte)radius, phase);
                var unsignedActual = sbugUnsigned((byte)angle, (byte)radius, phase);
                uint magnitude = words[shiftedAngle & 127] * (uint)radius;
                if (fixedActual != SignedSixteenSixteen.FromParts(whole, fraction) || signedActual != SignedSixteenSixteen.FromParts(whole, fraction) ||
                    unsignedActual.RawValue != magnitude)
                    throw new InvalidDataException($"Compiled vector differs at angle={angle}, radius={radius}, phase={phase}.");
            }
        }
        for (int index = 0; index < 128; index++)
        for (int magnitude = 0; magnitude <= ushort.MaxValue; magnitude++)
        {
            int expected = unchecked((int)((uint)words[index] * magnitude));
            if (unsigned((ushort)(0xff80 | index), (ushort)magnitude, 128) != expected ||
                unsigned((ushort)(0xffc0 + index), (ushort)magnitude, 64) != expected)
                throw new InvalidDataException($"Compiled unsigned sine differs at index={index}, magnitude={magnitude}.");
        }
        Console.WriteLine("Compiled enemy sine: 256 exact samples, 65,536 byte inputs including both Sbug vector phases, and 8,388,608 unsigned products (two wrapped offsets) pass without any production bus dependency.");
    }

    /// <summary>Checks signed sine and cinematic samples, then every tide phase and option combination against retail words.</summary>
    internal static void VerifyCompiledSignedTrigonometry(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        short Reference(int index)
        {
            int address = EnemyMathReferenceData.SignedNegativeCosine + index * 2;
            return unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        }
        Func<byte, short>[] cinematicReaders =
            [sample => FloorRotationMatrix.ReadSine(new ByteAngle(sample)), sample => EndingCreditsPresentation.ReadSine(new ByteAngle(sample))];
        for (int index = 0; index < 320; index++)
        {
            short expected = Reference(index);
            AssertEqual(expected, Trigonometry.SignedNegativeCosineWord(index), "all 320 native prefix/full-wave words");
            if (index >= 64)
            {
                byte angle = (byte)(index - 64);
                AssertEqual(expected, Trigonometry.SignedSine(new ByteAngle(angle)), "all signed sine quadrants match cartridge");
                foreach (var reader in cinematicReaders)
                    AssertEqual(expected, reader(angle), "cinematic matrix sample matches cartridge without bus");
            }
        }
        foreach (int invalid in new[] { -1, 320, int.MinValue, int.MaxValue })
        {
            bool rejected = false;
            try { _ = Trigonometry.SignedNegativeCosineWord(invalid); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            AssertTrue(rejected, "signed table does not silently wrap invalid prefix indexes");
        }
        Console.WriteLine("Compiled signed sine: all 320 native words and three cinematic readers match, including +/-256 peaks and prefix bounds.");
        if (definitionsOnly) return;
        var tideSession = new RoomFxSession();
        var tide = new RoomLayer3FxState(tideSession);
        var step = new Action(tide.StepLiquidTide);
        foreach (RoomLiquidOptions option in new[] { RoomLiquidOptions.None, RoomLiquidOptions.LargeTide,
            RoomLiquidOptions.SmallTide, RoomLiquidOptions.SmallTide | RoomLiquidOptions.LargeTide })
        {
            tide.LiquidOptions = option;
            for (int phase = 0; phase <= ushort.MaxValue; phase++)
            {
                tide.TidePhase = (ushort)phase;
                tide.TideFixedOffset = 12345;
                step();
                short sample = Reference(RotationAngle.FromRaw(unchecked((ushort)phase)).TableIndex);
                bool small = (option & RoomLiquidOptions.SmallTide) != 0;
                bool still = option == RoomLiquidOptions.None;
                int scale = still ? 0 : small ? 8 : 32;
                int delta = still ? 0 : small ? (sample >= 0 ? 288 : 192) : (sample >= 0 ? 224 : 128);
                if (tide.TideFixedOffset != SixteenSixteen.FromEightEight(sample * scale) ||
                    tide.TidePhase != unchecked((ushort)(phase + delta)))
                    throw new InvalidDataException($"Native tide differs at options={option}, phase={phase:X4}.");
            }
        }
        Console.WriteLine("Compiled tide: all 65,536 phases in four option combinations preserve exact offset, phase advance and small-tide precedence without a bus.");
    }

    /// <summary>Runs the registered compiled enemy trigonometry family.</summary>
    [VerificationCommand("--compiled-enemy-sine")]
    internal static int CompiledEnemySineCommand()
    {
        Suite(nameof(VerifyCompiledEnemyTrigonometry), () => VerifyCompiledEnemyTrigonometry());
        return 0;
    }

    /// <summary>Runs signed sine and tide phase checks using a retail cartridge oracle.</summary>
    [VerificationCommand("--liquid-tide-phase")]
    internal static int LiquidTidePhaseCommand()
    {
        var tideOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCompiledSignedTrigonometry), () => VerifyCompiledSignedTrigonometry(tideOracle));
        return 0;
    }
}
