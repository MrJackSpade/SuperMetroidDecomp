using static Program;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;
using static AbsoluteTangentDefinitionsVerification;
using static AlcoonFireballInstructionProgramDefinitionsVerification;
using static AlcoonInstructionProgramDefinitionsVerification;
using static AtomicMovementDefinitionsVerification;
using static BeamCallbackTablesVerification;
using static BeetomInstructionProgramDefinitionsVerification;
using static BlueBrinstarFaceBlockInstructionProgramDefinitionsVerification;
using static BlueDoorPlmDrawDefinitionsVerification;
using static BombSpreadLaunchDefinitionsVerification;
using static BombTorizoAttackDefinitionsVerification;
using static BombTorizoDroolInstructionProgramDefinitionsVerification;
using static BombTorizoDroolSineVerification;
using static BombTorizoMovementDefinitionsVerification;
using static BombTorizoStatueFragmentDefinitionsVerification;
using static BombTorizoStatueInstructionProgramDefinitionsVerification;
using static BotwoonAnimationsVerification;
using static BotwoonHealthPaletteDefinitionsVerification;
using static BotwoonNavigationDefinitionsVerification;
using static BotwoonSpeedsVerification;
using static BoulderBounceDefinitionsVerification;
using static BoulderInstructionProgramDefinitionsVerification;
using static BoyonInstructionProgramDefinitionsVerification;
using static BoyonSpeedDefinitionsVerification;
using static BrinstarPipeBugInstructionProgramDefinitionsVerification;
using static BullInstructionProgramDefinitionsVerification;
using static BullMovementDefinitionsVerification;
using static CacatacInstructionProgramDefinitionsVerification;
using static CacatacMovementDefinitionsVerification;
using static CacatacProjectileDefinitionsVerification;
using static CacatacProjectileInstructionProgramDefinitionsVerification;
using static CeresDoorInitializationDefinitionsVerification;
using static CeresDoorInstructionProgramDefinitionsVerification;
using static CeresDoorQuakeDefinitionsVerification;
using static CeresFallingDebrisInstructionProgramDefinitionsVerification;
using static CeresRidleyEyeFadeDefinitionsVerification;
using static CeresRidleyGetawayDefinitionsVerification;
using static CeresRidleyProjectileInstructionProgramDefinitionsVerification;
using static CeresSteamDefinitionsVerification;
using static CeresSteamInstructionProgramDefinitionsVerification;
using static ChargeFlareDefinitionsVerification;
using static ChargeFlarePlacementVerification;
using static ChootInstructionProgramDefinitionsVerification;
using static ChootPatternDefinitionsVerification;
using static ChozoStatueInstructionProgramDefinitionsVerification;
using static ChozoStatuePlmDefinitionsVerification;
using static ChozoTourianDustInstructionProgramDefinitionsVerification;
using static ColoredDoorPlmDrawDefinitionsVerification;
using static CompiledProjectileMathVerification;
using static CrawlerAnimationDefinitionsVerification;
using static CrawlerSpeedsVerification;
using static CrocomireBridgeFragmentDefinitionsVerification;
using static CrocomireAnimationsVerification;
using static CrocomireMeltingDefinitionsVerification;
using static CrocomireRumbleDefinitionsVerification;
using static CrystalFlashPaletteTimingDefinitionsVerification;
using static DachoraInstructionProgramDefinitionsVerification;
using static DeadSidehopperCorpseDefinitionsVerification;
using static DeadSidehopperInstructionProgramDefinitionsVerification;
using static DeadSidehopperLaunchDefinitionsVerification;
using static DeadTorizoCorpseDefinitionsVerification;
using static DeadTorizoInstructionProgramDefinitionsVerification;
using static DeadTourianCorpseDefinitionsVerification;
using static DeadTourianCorpseInstructionProgramDefinitionsVerification;
using static DownwardGatePlmsVerification;
using static DownwardGateProjectileInstructionProgramDefinitionsVerification;
using static DragonAnimationDefinitionsVerification;
using static DragonFireballInstructionProgramDefinitionsVerification;
using static DragonInstructionProgramDefinitionsVerification;
using static DraygonBurialEvirDefinitionsVerification;
using static DraygonHealthPaletteDefinitionsVerification;
using static DraygonInstructionProgramDefinitionsVerification;
using static DraygonIntroDanceDefinitionsVerification;
using static DraygonProjectileInstructionProgramDefinitionsVerification;
using static ElevatorInputDefinitionsVerification;
using static ElevatorInstructionProgramDefinitionsVerification;
using static EnemyBreakableTerrainDefinitionsVerification;
using static EnemyDeathExplosionDefinitionsVerification;
using static EnemyDeathInstructionProgramDefinitionsVerification;
using static EnemyDropChanceDefinitionsVerification;
using static EnemyFireballLaunchDefinitionsVerification;
using static EnemyInstructionOwnerCoverageVerification;
using static EnemyLinearSpeedsVerification;
using static EnemyPickupDefinitionsVerification;
using static EnemyPickupInstructionProgramDefinitionsVerification;
using static EnemyProjectileDefinitionsVerification;
using static EnemyProjectileInstructionOwnerCoverageVerification;
using static EnemyQuadraticSpeedsVerification;
using static EnemyVulnerabilityDefinitionsVerification;
using static EscapeDachoraInstructionProgramDefinitionsVerification;
using static EscapeEtecoonDefinitionsVerification;
using static EtecoonInstructionProgramDefinitionsVerification;
using static EvirInstructionProgramDefinitionsVerification;
using static EyeDoorPlmDrawDefinitionsVerification;
using static EyeDoorProjectileInstructionProgramDefinitionsVerification;
using static EyeDoorSweatInstructionProgramDefinitionsVerification;
using static FakeKraidInstructionProgramDefinitionsVerification;
using static FakeKraidProjectileDefinitionsVerification;
using static FakeKraidProjectileInstructionProgramDefinitionsVerification;
using static FallingSparkInstructionProgramDefinitionsVerification;
using static FallingSparkLaunchDefinitionsVerification;
using static FamilyTrigonometryVerification;
using static FirefleaInstructionProgramDefinitionsVerification;
using static FirefleaMovementDefinitionsVerification;
using static FlyInstructionProgramDefinitionsVerification;
using static FullRunSuitesVerification;
using static FuneNamiheDefinitionsVerification;
using static FuneNamiheFireballInstructionProgramDefinitionsVerification;
using static FuneNamiheInstructionProgramDefinitionsVerification;
using static GoldenTorizoEggInstructionProgramDefinitionsVerification;
using static GoldenTorizoEyeBeamInstructionProgramDefinitionsVerification;
using static GoldenTorizoSuperMissileInstructionProgramDefinitionsVerification;
using static GrappleBodyPlacementVerification;
using static GrappleCompiledMathVerification;
using static GrappleConnectionDefinitionsVerification;
using static GrappleFiringDefinitionsVerification;
using static GreyDoorPlmDrawDefinitionsVerification;
using static GrowingShutterDefinitionsVerification;
using static GrowingShutterInstructionProgramDefinitionsVerification;
using static GunshipDustDefinitionsVerification;
using static GunshipDustInstructionProgramDefinitionsVerification;
using static GunshipInstructionProgramDefinitionsVerification;
using static GunshipMotionDefinitionsVerification;
using static HZoomerInstructionProgramDefinitionsVerification;
using static HalfWaveAlgorithmsVerification;
using static HibashiDefinitionsVerification;
using static HopperAnimationDefinitionsVerification;
using static HorizontalShutterInstructionProgramDefinitionsVerification;
using static HyperBeamPaletteFxProgramDefinitionsVerification;
using static IntroEggMotionDefinitionsVerification;
using static KagoBugProjectileInstructionProgramDefinitionsVerification;
using static KagoInstructionProgramDefinitionsVerification;
using static KiHunterAcidSpitInstructionProgramDefinitionsVerification;
using static KiHunterInstructionProgramDefinitionsVerification;
using static KiHunterMotionDefinitionsVerification;
using static KraidArmInstructionProgramDefinitionsVerification;
using static KraidBodyContourVerification;
using static KraidCeilingRockPositionsVerification;
using static KraidFootInstructionProgramDefinitionsVerification;
using static KraidHeadInstructionsVerification;
using static KraidLintInstructionProgramDefinitionsVerification;
using static KraidMouthHitboxesVerification;
using static KraidMovementChoicesVerification;
using static KraidNailBounceVerification;
using static KraidNailContourVerification;
using static KraidNailInstructionProgramDefinitionsVerification;
using static KraidNailSiblingVerification;
using static KraidRockLaunchDefinitionsVerification;
using static KraidRockProjectileInstructionProgramDefinitionsVerification;
using static KraidSinkScheduleVerification;
using static KzanInstructionProgramDefinitionsVerification;
using static LowerNorfairRioInstructionProgramDefinitionsVerification;
using static MagdolliteInstructionProgramDefinitionsVerification;
using static MagdolliteLavaInstructionProgramDefinitionsVerification;
using static MamaTurtleInstructionProgramDefinitionsVerification;
using static MaridiaLargeSnailInstructionDefinitionsVerification;
using static MetroidBehaviorDefinitionsVerification;
using static MetroidInstructionProgramDefinitionsVerification;
using static MiscDustProjectileDefinitionsVerification;
using static MochtroidInstructionProgramDefinitionsVerification;
using static MorphBallEyeInstructionProgramDefinitionsVerification;
using static MotherBrainBabyInstructionProgramDefinitionsVerification;
using static MotherBrainBabyMetroidDefinitionsVerification;
using static MotherBrainBodyInstructionProgramsVerification;
using static MotherBrainDeathExplosionDefinitionsVerification;
using static MotherBrainDoorFragmentDefinitionsVerification;
using static MotherBrainGlassInstructionProgramDefinitionsVerification;
using static MotherBrainGlassPlmDrawDefinitionsVerification;
using static MotherBrainGlassShardDefinitionsVerification;
using static MotherBrainNeckSineVerification;
using static MotherBrainRoomPaletteProgramDefinitionsVerification;
using static MotherBrainTopTubeInstructionProgramDefinitionsVerification;
using static MotherBrainTurretDefinitionsVerification;
using static MotherBrainTurretInstructionProgramDefinitionsVerification;
using static MultiviolaInstructionProgramDefinitionsVerification;
using static NinjaSpacePirateInstructionProgramDefinitionsVerification;
using static NoobTubePlmDrawDefinitionsVerification;
using static NoobTubeProjectileInstructionProgramDefinitionsVerification;
using static NorfairLavaJumpDefinitionsVerification;
using static NorfairLavaJumperInstructionProgramDefinitionsVerification;
using static NorfairPipeBugInstructionProgramDefinitionsVerification;
using static NorfairRioInstructionProgramDefinitionsVerification;
using static NuclearWaffleDefinitionsVerification;
using static NuclearWaffleProjectileInstructionProgramDefinitionsVerification;
using static OwtchInstructionProgramDefinitionsVerification;
using static OwtchMovementDefinitionsVerification;
using static PhantoonCasualFlamesVerification;
using static PhantoonDeathExplosionsVerification;
using static PhantoonFlameSpawnsVerification;
using static PhantoonInstructionProgramDefinitionsVerification;
using static PhantoonMotionDefinitionsVerification;
using static PhantoonPathVerification;
using static PhantoonPatternsVerification;
using static PhantoonProjectileInstructionProgramDefinitionsVerification;
using static PhantoonSoundDefinitionsVerification;
using static PhantoonTimersVerification;
using static PhantoonWaveMathVerification;
using static PipeBugAnimationDefinitionsVerification;
using static PlatformInstructionProgramDefinitionsVerification;
using static PolypInstructionProgramDefinitionsVerification;
using static PolypLaunchDefinitionsVerification;
using static PolypRockInstructionProgramDefinitionsVerification;
using static PoseCollisionDefinitionsVerification;
using static PoseDispatchDefinitionsVerification;
using static PoseInputDefinitionsVerification;
using static PoseProjectileOriginVerification;
using static PowampInstructionProgramDefinitionsVerification;
using static PowampSpikeInstructionProgramDefinitionsVerification;
using static PowerBombCallbackDefinitionsVerification;
using static PowerBombShapeDefinitionsVerification;
using static ProjectileCompositionsVerification;
using static ProjectileDamageVerification;
using static ProjectileFrameBindingsVerification;
using static ProjectileInstructionsVerification;
using static ProjectileOriginsVerification;
using static ProjectileRadiiVerification;
using static ProjectileVisualPartsVerification;
using static PuyoHopDefinitionsVerification;
using static PuyoInstructionProgramDefinitionsVerification;
using static QuicksandVerification;
using static RepositoryInputsVerification;
using static RidleyAttackChoicesVerification;
using static RidleyClawOffsetsVerification;
using static RidleyDeathAccelerationVerification;
using static RidleyExplosionDefinitionsVerification;
using static RidleyInertiaVerification;
using static RidleyInstructionProgramDefinitionsVerification;
using static RidleyMovementTargetsVerification;
using static RidleyNeutralTailVerification;
using static RidleyPogoDefinitionsVerification;
using static RioInstructionProgramDefinitionsVerification;
using static RioLaunchDefinitionsVerification;
using static RipperInstructionProgramDefinitionsVerification;
using static RoomPaletteFxDefinitionsVerification;
using static RoomPlmPopulationVerification;
using static RoomShakeDefinitionsVerification;
using static RoomSpriteObjectDefinitionsVerification;
using static RunningCadenceVerification;
using static SamusArmCannonDefinitionsVerification;
using static SamusAtmosphericAnimationDefinitionsVerification;
using static SamusAtmosphericEffectDefinitionsVerification;
using static SamusBallBounceDefinitionsVerification;
using static SamusDeathExplosionTimingDefinitionsVerification;
using static SamusEaterPlmDefinitionsVerification;
using static SamusHudDefinitionsVerification;
using static SamusImpulseDefinitionsVerification;
using static SamusIndexedSpeedsVerification;
using static SamusStandaloneSpeedsVerification;
using static SamusVerticalDefinitionsVerification;
using static SaveStationAnimationDefinitionsVerification;
using static SaveStationElectricityInstructionProgramDefinitionsVerification;
using static SbugMovementDefinitionsVerification;
using static SciserInstructionProgramDefinitionsVerification;
using static ShaktoolInstructionDefinitionsVerification;
using static ShaktoolProjectileInstructionProgramDefinitionsVerification;
using static ShaktoolSegmentDefinitionsVerification;
using static SharedCrawlerInstructionProgramDefinitionsVerification;
using static SharedSlopeHeightsVerification;
using static ShitroidInstructionProgramDefinitionsVerification;
using static ShotCallbackDefinitionsVerification;
using static SkreeMetareeAnimationDefinitionsVerification;
using static SkreeMetareeInstructionProgramDefinitionsVerification;
using static SkreeMetareeParticleInstructionProgramDefinitionsVerification;
using static SkulteraInstructionProgramDefinitionsVerification;
using static SlopeSpeedDefinitionsVerification;
using static SpacePirateProjectileInstructionProgramDefinitionsVerification;
using static SparkMovementDefinitionsVerification;
using static SporeSpawnProjectileDefinitionsVerification;
using static SporeSpawnProjectileInstructionProgramDefinitionsVerification;
using static SquareSlopeDefinitionsVerification;
using static StationAccessPlmDefinitionsVerification;
using static StatueWalkDefinitionsVerification;
using static StokeInstructionProgramDefinitionsVerification;
using static StokeProjectileInstructionProgramDefinitionsVerification;
using static SuitPickupBeamCurveDefinitionsVerification;
using static SuiteSchedulingVerification;
using static SurfaceMotionDefinitionsVerification;
using static TorizoChozoOrbInstructionProgramDefinitionsVerification;
using static TorizoExplosionInstructionProgramDefinitionsVerification;
using static TorizoExplosiveSwipeInstructionProgramDefinitionsVerification;
using static TorizoInitializationDefinitionsVerification;
using static TorizoLandingDustInstructionProgramDefinitionsVerification;
using static TorizoRandomizedProjectileDefinitionsVerification;
using static TorizoSonicBoomInstructionProgramDefinitionsVerification;
using static TourianAccessPlmDefinitionsVerification;
using static TourianEntranceStatueInstructionProgramDefinitionsVerification;
using static TourianStatueProjectileInstructionProgramDefinitionsVerification;
using static TourianStatueUnlockDefinitionsVerification;
using static VerticalShutterInstructionProgramDefinitionsVerification;
using static ViolaInstructionProgramDefinitionsVerification;
using static WalkingSpacePirateInstructionProgramDefinitionsVerification;
using static WallSpacePirateInstructionProgramDefinitionsVerification;
using static WaverAnimationDefinitionsVerification;
using static WaverInstructionProgramDefinitionsVerification;
using static WorkRobotInstructionProgramDefinitionsVerification;
using static WorkRobotLaserInstructionProgramDefinitionsVerification;
using static WorkRobotPaletteTimingDefinitionsVerification;
using static WreckedShipGhostInstructionProgramDefinitionsVerification;
using static YappingMawBodyProjectileInstructionProgramDefinitionsVerification;
using static YappingMawInstructionProgramDefinitionsVerification;
using static YardDirectionDefinitionsVerification;
using static YardInstructionProgramDefinitionsVerification;
using static YardTurnDefinitionsVerification;
using static YellowPipeBugInstructionProgramDefinitionsVerification;
using static ZebesEscapeExplosionDefinitionsVerification;
using static ZebetiteDefinitionsVerification;
using static ZebetiteInstructionProgramDefinitionsVerification;
using static ZeroInstructionProgramDefinitionsVerification;
using static ZoaAnimationDefinitionsVerification;
using static ZoaInstructionProgramDefinitionsVerification;
using static ZoaSpeedDefinitionsVerification;

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
        new(nameof(VerifyCacatacProjectileInstructionProgramDefinitions), () => VerifyCacatacProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFallingSparkInstructionProgramDefinitions), () => VerifyFallingSparkInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFuneNamiheFireballInstructionProgramDefinitions), () => VerifyFuneNamiheFireballInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMagdolliteLavaInstructionProgramDefinitions), () => VerifyMagdolliteLavaInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDragonFireballInstructionProgramDefinitions), () => VerifyDragonFireballInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEyeDoorProjectileInstructionProgramDefinitions), () => VerifyEyeDoorProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEyeDoorSweatInstructionProgramDefinitions), () => VerifyEyeDoorSweatInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySkreeMetareeParticleInstructionProgramDefinitions), () => VerifySkreeMetareeParticleInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKraidRockProjectileInstructionProgramDefinitions), () => VerifyKraidRockProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyAlcoonFireballInstructionProgramDefinitions), () => VerifyAlcoonFireballInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyWorkRobotLaserInstructionProgramDefinitions), () => VerifyWorkRobotLaserInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPowampSpikeInstructionProgramDefinitions), () => VerifyPowampSpikeInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPolypRockInstructionProgramDefinitions), () => VerifyPolypRockInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKiHunterAcidSpitInstructionProgramDefinitions), () => VerifyKiHunterAcidSpitInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyStokeProjectileInstructionProgramDefinitions), () => VerifyStokeProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNuclearWaffleProjectileInstructionProgramDefinitions), () => VerifyNuclearWaffleProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKagoBugProjectileInstructionProgramDefinitions), () => VerifyKagoBugProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyYappingMawBodyProjectileInstructionProgramDefinitions), () => VerifyYappingMawBodyProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCrocomireProjectileAnimations), () => VerifyCrocomireProjectileAnimations(LoadRepositoryRom())),
        new(nameof(VerifyPhantoonProjectileInstructionProgramDefinitions), () => VerifyPhantoonProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDraygonProjectileInstructionProgramDefinitions), () => VerifyDraygonProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCeresRidleyProjectileInstructionProgramDefinitions), () => VerifyCeresRidleyProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySpacePirateProjectileInstructionProgramDefinitions), () => VerifySpacePirateProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCeresFallingDebrisInstructionProgramDefinitions), () => VerifyCeresFallingDebrisInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySaveStationElectricityInstructionProgramDefinitions), () => VerifySaveStationElectricityInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyPickupInstructionProgramDefinitions), () => VerifyEnemyPickupInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyDeathInstructionProgramDefinitions), () => VerifyEnemyDeathInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyShaktoolProjectileInstructionProgramDefinitions), () => VerifyShaktoolProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyChozoTourianDustInstructionProgramDefinitions), () => VerifyChozoTourianDustInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTourianStatueProjectileInstructionProgramDefinitions), () => VerifyTourianStatueProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySporeSpawnProjectileInstructionProgramDefinitions), () => VerifySporeSpawnProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBotwoonProjectileAnimations), () => VerifyBotwoonProjectileAnimations(LoadRepositoryRom())),
        new(nameof(VerifyTorizoLandingDustInstructionProgramDefinitions), () => VerifyTorizoLandingDustInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTorizoExplosiveSwipeInstructionProgramDefinitions), () => VerifyTorizoExplosiveSwipeInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBombTorizoDroolInstructionProgramDefinitions), () => VerifyBombTorizoDroolInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTorizoExplosionInstructionProgramDefinitions), () => VerifyTorizoExplosionInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTorizoChozoOrbInstructionProgramDefinitions), () => VerifyTorizoChozoOrbInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTorizoSonicBoomInstructionProgramDefinitions), () => VerifyTorizoSonicBoomInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBombTorizoStatueInstructionProgramDefinitions), () => VerifyBombTorizoStatueInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyGoldenTorizoEggInstructionProgramDefinitions), () => VerifyGoldenTorizoEggInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions), () => VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions), () => VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyProjectileInstructionOwnerCoverage), () => VerifyEnemyProjectileInstructionOwnerCoverage()),
        new(nameof(VerifyGunshipInstructionProgramDefinitions), () => VerifyGunshipInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyRidleyInstructionProgramDefinitions), () => VerifyRidleyInstructionProgramDefinitions()),
        new(nameof(VerifyDraygonInstructionProgramDefinitions), () => VerifyDraygonInstructionProgramDefinitions()),
        new(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyWallSpacePirateInstructionProgramDefinitions), () => VerifyWallSpacePirateInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNinjaSpacePirateInstructionProgramDefinitions), () => VerifyNinjaSpacePirateInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainBodyInstructionPrograms), () => VerifyMotherBrainBodyInstructionPrograms()),
        new(nameof(VerifyMotherBrainRoomPaletteProgramDefinitions), () => VerifyMotherBrainRoomPaletteProgramDefinitions()),
        new(nameof(VerifyEnemyInstructionOwnerCoverage), () => VerifyEnemyInstructionOwnerCoverage()),
        new(nameof(VerifyDownwardGateProjectileInstructionProgramDefinitions), () => VerifyDownwardGateProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNoobTubeProjectileInstructionProgramDefinitions), () => VerifyNoobTubeProjectileInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainTopTubeInstructionProgramDefinitions), () => VerifyMotherBrainTopTubeInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainGlassInstructionProgramDefinitions), () => VerifyMotherBrainGlassInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainTurretInstructionProgramDefinitions), () => VerifyMotherBrainTurretInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyGunshipDustInstructionProgramDefinitions), () => VerifyGunshipDustInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCacatacInstructionProgramDefinitions), () => VerifyCacatacInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMagdolliteInstructionProgramDefinitions), () => VerifyMagdolliteInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyOwtchInstructionProgramDefinitions), () => VerifyOwtchInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyStokeInstructionProgramDefinitions), () => VerifyStokeInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyRipperInstructionProgramDefinitions), () => VerifyRipperInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFlyInstructionProgramDefinitions), () => VerifyFlyInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBullInstructionProgramDefinitions), () => VerifyBullInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKagoInstructionProgramDefinitions), () => VerifyKagoInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyHorizontalShutterInstructionProgramDefinitions), () => VerifyHorizontalShutterInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyGrowingShutterInstructionProgramDefinitions), () => VerifyGrowingShutterInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyVerticalShutterInstructionProgramDefinitions), () => VerifyVerticalShutterInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyChootInstructionProgramDefinitions), () => VerifyChootInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNorfairLavaJumperInstructionProgramDefinitions), () => VerifyNorfairLavaJumperInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMultiviolaInstructionProgramDefinitions), () => VerifyMultiviolaInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPowampInstructionProgramDefinitions), () => VerifyPowampInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyWreckedShipGhostInstructionProgramDefinitions), () => VerifyWreckedShipGhostInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDeadTorizoInstructionProgramDefinitions), () => VerifyDeadTorizoInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDeadSidehopperInstructionProgramDefinitions), () => VerifyDeadSidehopperInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDeadTourianCorpseInstructionProgramDefinitions), () => VerifyDeadTourianCorpseInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyShitroidInstructionProgramDefinitions), () => VerifyShitroidInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyAtomicMovementDefinitions), () => VerifyAtomicMovementDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySbugMovementDefinitions), () => VerifySbugMovementDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySparkMovementDefinitions), () => VerifySparkMovementDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyElevatorInputDefinitions), () => VerifyElevatorInputDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyOwtchMovementDefinitions), () => VerifyOwtchMovementDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNuclearWaffleDefinitions), () => VerifyNuclearWaffleDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyHibashiDefinitions), () => VerifyHibashiDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions), () => VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFuneNamiheDefinitions), () => VerifyFuneNamiheDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMiscDustProjectileDefinitions), () => VerifyMiscDustProjectileDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyChootPatternDefinitions), () => VerifyChootPatternDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCrawlerAnimationDefinitions), () => VerifyCrawlerAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyHZoomerInstructionProgramDefinitions), () => VerifyHZoomerInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySciserInstructionProgramDefinitions), () => VerifySciserInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyZeroInstructionProgramDefinitions), () => VerifyZeroInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyViolaInstructionProgramDefinitions), () => VerifyViolaInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySharedCrawlerInstructionProgramDefinitions), () => VerifySharedCrawlerInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFirefleaInstructionProgramDefinitions), () => VerifyFirefleaInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyZebetiteInstructionProgramDefinitions), () => VerifyZebetiteInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMorphBallEyeInstructionProgramDefinitions), () => VerifyMorphBallEyeInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMetroidInstructionProgramDefinitions), () => VerifyMetroidInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKraidNailInstructionProgramDefinitions), () => VerifyKraidNailInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKraidArmInstructionProgramDefinitions), () => VerifyKraidArmInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKraidFootInstructionProgramDefinitions), () => VerifyKraidFootInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyKraidLintInstructionProgramDefinitions), () => VerifyKraidLintInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPhantoonInstructionProgramDefinitions), () => VerifyPhantoonInstructionProgramDefinitions(LoadRepositoryRom())),
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
        new(nameof(VerifyHyperBeamPaletteFxProgramDefinitions), () => VerifyHyperBeamPaletteFxProgramDefinitions(LoadRepositoryRom())),
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
        new(nameof(VerifyEtecoonInstructionProgramDefinitions), () => VerifyEtecoonInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyElevatorInstructionProgramDefinitions), () => VerifyElevatorInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPlatformInstructionProgramDefinitions), () => VerifyPlatformInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEnemyBreakableTerrainDefinitions), () => VerifyEnemyBreakableTerrainDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainDeathExplosionDefinitions), () => VerifyMotherBrainDeathExplosionDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyWaverAnimationDefinitions), () => VerifyWaverAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyWaverInstructionProgramDefinitions), () => VerifyWaverInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySkreeMetareeAnimationDefinitions), () => VerifySkreeMetareeAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySkreeMetareeInstructionProgramDefinitions), () => VerifySkreeMetareeInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyZoaAnimationDefinitions), () => VerifyZoaAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyZoaInstructionProgramDefinitions), () => VerifyZoaInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDragonAnimationDefinitions), () => VerifyDragonAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDragonInstructionProgramDefinitions), () => VerifyDragonInstructionProgramDefinitions(LoadRepositoryRom())),
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
        new(nameof(VerifyCeresSteamInstructionProgramDefinitions), () => VerifyCeresSteamInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCeresDoorInstructionProgramDefinitions), () => VerifyCeresDoorInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyPipeBugAnimationDefinitions), () => VerifyPipeBugAnimationDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyBrinstarPipeBugInstructionProgramDefinitions), () => VerifyBrinstarPipeBugInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyNorfairPipeBugInstructionProgramDefinitions), () => VerifyNorfairPipeBugInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyYellowPipeBugInstructionProgramDefinitions), () => VerifyYellowPipeBugInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDraygonBurialEvirDefinitions), () => VerifyDraygonBurialEvirDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyDraygonHealthPaletteDefinitions), () => VerifyDraygonHealthPaletteDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFakeKraidProjectileDefinitions), () => VerifyFakeKraidProjectileDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCrocomireTongueAnimations), () => VerifyCrocomireTongueAnimations(LoadRepositoryRom())),
        new(nameof(VerifyCrocomireBodyAnimations), () => VerifyCrocomireBodyAnimations(LoadRepositoryRom())),
        new(nameof(VerifyMotherBrainBabyInstructionProgramDefinitions), () => VerifyMotherBrainBabyInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEscapeEtecoonDefinitions), () => VerifyEscapeEtecoonDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEscapeEtecoonInstructionProgramDefinitions), () => VerifyEscapeEtecoonInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyEscapeDachoraInstructionProgramDefinitions), () => VerifyEscapeDachoraInstructionProgramDefinitions(LoadRepositoryRom())),
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
        new(nameof(VerifyBoyonInstructionProgramDefinitions), () => VerifyBoyonInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifySkulteraInstructionProgramDefinitions), () => VerifySkulteraInstructionProgramDefinitions(LoadRepositoryRom())),
        new(nameof(VerifyCompiledSurfaceMotion), () => VerifyCompiledSurfaceMotion(LoadRepositoryRom())),
        new(nameof(VerifyCompiledEnemyFireballLaunches), () => VerifyCompiledEnemyFireballLaunches(LoadRepositoryRom())),
        new(nameof(VerifyCompiledRioLaunches), () => VerifyCompiledRioLaunches(LoadRepositoryRom())),
        new(nameof(VerifyCompiledBoulderBounces), () => VerifyCompiledBoulderBounces(LoadRepositoryRom())),
        new(nameof(VerifyBoulderInstructionProgramDefinitions), () => VerifyBoulderInstructionProgramDefinitions(LoadRepositoryRom())),
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
        new(nameof(VerifyKraidNailBounce), () => VerifyKraidNailBounce(LoadRepositoryRom())),
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
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled enemy sine")]
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

        var pixel = new Func<ushort, ushort, int>(EnemyMath.ReadEightBitSineProduct);
        var fixedProduct = new Func<ushort, ushort, (short Whole, ushort Fraction)>(EnemyMath.ReadEightBitSineFixedProduct);
        var cosine = new Func<ushort, ushort, int>(EnemyMath.ReadEightBitCosineProduct);
        var negative = new Func<ushort, ushort, int>(EnemyMath.ReadEightBitNegativeSineProduct);
        var fixedCosine = new Func<ushort, ushort, (short Whole, ushort Fraction)>(EnemyMath.ReadEightBitCosineFixedProduct);
        var fixedNegative = new Func<ushort, ushort, (short Whole, ushort Fraction)>(EnemyMath.ReadEightBitNegativeSineFixedProduct);
        var sbugSigned = new Func<byte, byte, int, SbugVelocityWords>(SbugBehavior.CalculateSignedSbugComponent);
        var sbugUnsigned = new Func<byte, byte, int, SbugVelocityWords>(SbugBehavior.CalculateUnsignedSbugMagnitude);
        var unsigned = new Func<ushort, ushort, ushort, int>(EnemyMath.ReadUnsignedSineMagnitudeProduct);

        int ExpectedPixel(int angle, int radius)
        {
            angle &= 255;
            int magnitude = SnesEightEight.Multiply(bytes[angle & 127], unchecked((byte)radius));
            return angle < 128 ? magnitude : -magnitude;
        }
        for (int angle = 0; angle < 256; angle++)
        for (int radius = 0; radius < 256; radius++)
        {
            // Poison high bytes to prove native byte truncation remains in the
            // production helpers. The fractional negation deliberately has no carry.
            ushort a = (ushort)(0xff00 | angle), r = (ushort)(0xab00 | radius);
            int product = bytes[angle & 127] * radius;
            var expected = ((short)(angle < 128 ? SnesEightEight.WholeOf(product) : -SnesEightEight.WholeOf(product)),
                unchecked((ushort)(angle < 128 ? SnesSixteenSixteen.FromEightEight(product) : -SnesSixteenSixteen.FromEightEight(product))));
            if (pixel(a, r) != ExpectedPixel(angle, radius) || fixedProduct(a, r) != expected ||
                cosine(a, r) != ExpectedPixel(angle + 64, radius) || negative(a, r) != ExpectedPixel(angle + 128, radius))
                throw new InvalidDataException($"Compiled byte sine result differs at angle={angle}, radius={radius}.");
            foreach (int phase in new[] { 64, 128 })
            {
                int shiftedAngle = unchecked((byte)(angle + phase));
                int shiftedProduct = bytes[shiftedAngle & 127] * radius;
                short whole = (short)(shiftedAngle < 128 ? SnesEightEight.WholeOf(shiftedProduct) : -SnesEightEight.WholeOf(shiftedProduct));
                ushort fraction = unchecked((ushort)(shiftedAngle < 128 ? SnesSixteenSixteen.FromEightEight(shiftedProduct) : -SnesSixteenSixteen.FromEightEight(shiftedProduct)));
                var fixedActual = phase == 64 ? fixedCosine(a, r) : fixedNegative(a, r);
                var signedActual = sbugSigned((byte)angle, (byte)radius, phase);
                var unsignedActual = sbugUnsigned((byte)angle, (byte)radius, phase);
                uint magnitude = (uint)words[shiftedAngle & 127] * (uint)radius;
                if (fixedActual != (whole, fraction) || signedActual != new SbugVelocityWords(unchecked((ushort)whole), fraction) ||
                    unsignedActual.RawFixed != magnitude)
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
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled signed trigonometry")]
    internal static void VerifyCompiledSignedTrigonometry(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        short Reference(int index)
        {
            int address = EnemyMathReferenceData.SignedNegativeCosine + index * 2;
            return unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        }
        Func<byte, short>[] cinematicReaders =
            [CeresDestructionCinematicState.ReadSine, EndingCreditsPresentation.ReadSine, IntroCeresFlightState.ReadSine];
        for (int index = 0; index < 320; index++)
        {
            short expected = Reference(index);
            AssertEqual(expected, EnemyTrigonometryTables.SignedNegativeCosineWord(index), "all 320 native prefix/full-wave words");
            if (index >= 64)
            {
                byte angle = (byte)(index - 64);
                AssertEqual(expected, EnemyTrigonometryTables.SignedSine(angle), "all signed sine quadrants match cartridge");
                foreach (var reader in cinematicReaders)
                    AssertEqual(expected, reader(angle), "cinematic matrix sample matches cartridge without bus");
            }
        }
        foreach (int invalid in new[] { -1, 320, int.MinValue, int.MaxValue })
        {
            bool rejected = false;
            try { _ = EnemyTrigonometryTables.SignedNegativeCosineWord(invalid); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            AssertTrue(rejected, "signed table does not silently wrap invalid prefix indexes");
        }
        Console.WriteLine("Compiled signed sine: all 320 native words and three cinematic readers match, including +/-256 peaks and prefix bounds.");
        if (definitionsOnly) return;
        var tide = new RoomLayer3FxState();
        var step = new Action(tide.StepLiquidTide);
        foreach (ushort option in new ushort[] { 0, 0x40, 0x80, 0xc0 })
        {
            tide.LiquidOptions = option;
            for (int phase = 0; phase <= ushort.MaxValue; phase++)
            {
                tide.tidePhase = (ushort)phase;
                tide.tideFixedOffset = 12345;
                step();
                short sample = Reference(SnesAngle.FromRaw(unchecked((ushort)phase)).TableIndex);
                bool small = (option & 0x80) != 0;
                int scale = option == 0 ? 0 : small ? 8 : 32;
                int delta = option == 0 ? 0 : small ? (sample >= 0 ? 288 : 192) : (sample >= 0 ? 224 : 128);
                if (tide.tideFixedOffset != SnesSixteenSixteen.FromEightEight(sample * scale) ||
                    tide.tidePhase != unchecked((ushort)(phase + delta)))
                    throw new InvalidDataException($"Native tide differs at options={option:X2}, phase={phase:X4}.");
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

/// <summary>Pinned cartridge addresses used as independent enemy trigonometry oracles.</summary>
internal static class EnemyMathReferenceData
{
    /// <summary>Pinned $A0:B443-$B642 sign-extended 8.8 sine words.</summary>
    public const int SignedSine = 0xa0b443;
    /// <summary>Pinned $A0:B1C3 signed 16-bit sine/cosine quadrants.</summary>
    public const int SignedSixteenBitSine = 0xa0b1c3;
    /// <summary>Pinned $AA:E03D Shaktool negative-cosine prefix and sine quadrants.</summary>
    public const int ShaktoolOrbit = 0xaae03d;
    /// <summary>Pinned $A0:B3C3 negative-cosine prefix followed by the full signed sine wave.</summary>
    public const int SignedNegativeCosine = 0xa0b3c3;
    /// <summary>Pinned $A0:B143 positive byte sine/cosine sample range.</summary>
    public const int ByteSine = 0xa0b143;
    /// <summary>Pinned $A0:B7EE UnsignedSineTable.</summary>
    public const int UnsignedSine = 0xa0b7ee;
}
