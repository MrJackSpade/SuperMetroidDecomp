using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
private static int Main(string[] args)
{
// This is deliberately a plain console executable rather than an xUnit/MSTest project.
// It keeps the reverse-engineering workspace dependency-free and makes every check easy
// to step through in Visual Studio. A failed check throws immediately with concrete state.
// Windows otherwise turns an unhandled CLR assertion into a modal "unknown software
// exception" dialog. That is actively hostile to an automated verifier: the useful stack
// trace belongs in this console and a dialog must never steal focus or stall the process.
if (OperatingSystem.IsWindows())
    NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

try
{
VerifyDebuggerVersionCompatibility();
VerifyCpuOperandOpenBus();
if (args is ["--enemy-angle-division"])
{
    VerifyEnemyAngleDivision();
    return 0;
}
if (args is ["--area-animated-tile-definitions"])
{
    VerifyAreaAnimatedTileObjectDefinitions();
    return 0;
}
if (args is ["--enemy-projectile-instruction-mechanics"])
{
    VerifyEnemyProjectileInstructionMechanicsDefinitions();
    return 0;
}
if (args is ["--spore-spawn-instruction-mechanics"])
{
    VerifySporeSpawnInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-baby-instruction-mechanics"])
{
    VerifyCeresBabyInstructionProgramDefinitions();
    return 0;
}
if (args is ["--rinka-instruction-mechanics"])
{
    VerifyRinkaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fune-namihe-instruction-mechanics"])
{
    VerifyFuneNamiheInstructionProgramDefinitions();
    return 0;
}
if (args is ["--atomic-instruction-mechanics"])
{
    VerifyAtomicMovementDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--sbug-instruction-mechanics"])
{
    VerifySbugMovementDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--spark-instruction-mechanics"])
{
    VerifySparkMovementDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--nuclear-waffle-instruction-mechanics"])
{
    VerifyNuclearWaffleDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--hibashi-instruction-mechanics"])
{
    VerifyHibashiDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--blue-brinstar-face-block-instruction-mechanics"])
{
    VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions();
    return 0;
}
if (args is ["--boulder-instruction-mechanics"])
{
    VerifyBoulderInstructionProgramDefinitions();
    return 0;
}
if (args is ["--boyon-instruction-mechanics"])
{
    VerifyBoyonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--skultera-instruction-mechanics"])
{
    VerifySkulteraInstructionProgramDefinitions();
    return 0;
}
if (args is ["--waver-instruction-mechanics"])
{
    VerifyWaverInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-elevator-arrival-definitions"])
{
    VerifyCeresElevatorArrivalGraphicsIndex();
    return 0;
}
if (args is ["--draygon-eye-effects"])
{
    VerifyDraygonEyeEffects();
    return 0;
}
if (args is ["--acid-statue-first-entry"])
{
    VerifyAcidStatueFirstEntry();
    return 0;
}
if (args is ["--door-alignment"])
{
    VerifyDoorAlignmentParity();
    return 0;
}
if (args is ["--spark-crash-alignment"])
{
    VerifySparkCrashAlignment();
    return 0;
}
if (args is ["--gate-jump-traces"])
{
    VerifyGateJumpTraces();
    return 0;
}
if (args is ["--gate-beam-collision"])
{
    VerifyKronicGateBeamCollision();
    return 0;
}
if (args is ["--right-facing-gate-glitch"])
{
    VerifyRightFacingGateGlitches();
    return 0;
}
if (args is ["--green-hill-gate-glitch"])
{
    VerifyGreenHillGrappleSpeedGateGlitch();
    return 0;
}
if (args is ["--gmode-gate-glitch"])
{
    VerifyGModeGateGlitch();
    return 0;
}
if (args is ["--frozen-gate-glitch"])
{
    VerifyFrozenEnemyGateGlitch();
    return 0;
}
if (args is ["--mochtroid-botwoon-clip"])
{
    VerifyMochtroidBotwoonPipeClip();
    return 0;
}
if (args is ["--red-tower-hero"])
{
    VerifyControlledRedTowerHeroShot();
    return 0;
}
if (args is ["--hero-shot-runtime"])
{
    VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv");
    return 0;
}
if (args is ["--hero-shot-runtime", var nativeHeroRuntimeTrace])
{
    VerifyHeroShotRuntimeCamera(nativeHeroRuntimeTrace);
    return 0;
}
if (args is ["--missile-edge", var missileEdgeTrace])
{
    VerifyMissileImpactCameraEdge(missileEdgeTrace);
    return 0;
}
if (args is ["--wrap-shots", var nativeWrapTrace])
{
    VerifyWrapShotTrace(nativeWrapTrace);
    return 0;
}
if (args is ["--ceiling-wrap", var nativeCeilingTrace])
{
    VerifyCeilingWrapPlmTrace(nativeCeilingTrace);
    return 0;
}
if (args is ["--ceiling-wrap-room"])
{
    VerifyFrogSpeedwayPoolCollision();
    return 0;
}
if (args is ["--ceiling-wrap-runtime", var nativeFrogTrace])
{
    VerifyFrogSpeedwayRuntimeTrace(nativeFrogTrace);
    return 0;
}
if (args is ["--ceiling-wrap-success", var nativeFrogSuccess, var frogDash])
{
    VerifyFrogSpeedwayRuntimeTrace(nativeFrogSuccess, 11, bool.Parse(frogDash));
    return 0;
}
if (args is ["--wrap-shot-rooms"])
{
    VerifyRetailWrapShotDoors();
    return 0;
}
if (args is ["--wrap-shot-enemies"])
{
    VerifyWrapShotEnemySeparation();
    return 0;
}
if (args is ["--wrap-shot-widths", var nativeWrapWidths])
{
    VerifyWrapShotWidths(nativeWrapWidths);
    return 0;
}
if (args is ["--hero-shots"])
{
    VerifyHeroShotCameraLifetime();
    return 0;
}
if (args is ["--hero-shots", var nativeHeroTrace])
{
    VerifyHeroShotCameraLifetime(nativeHeroTrace);
    return 0;
}
if (args is ["--projectile-inheritance-probe"])
{
    ProbeProjectileVelocityInheritance();
    return 0;
}
if (args is ["--samus-physics"])
{
    VerifySamusPhysicsBatch();
    return 0;
}
if (args is ["--samus-projectiles"])
{
    VerifySamusPowerBeamProjectiles();
    VerifyBeamSpeedRows();
    VerifyProjectileCooldowns();
    VerifyHeroShotCameraLifetime("csharp/test-fixtures/movement-release/hero-shot-411.csv");
    VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv");
    VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv");
    VerifyControlledRedTowerHeroShot();
    return 0;
}
if (args is ["--projectile-cooldowns"])
{
    VerifyProjectileCooldowns();
    return 0;
}
if (args is ["--projectile-motion"])
{
    VerifyBeamSpeedRows();
    return 0;
}
if (args is ["--botwoon-plm-identity"])
{
    VerifyBotwoonPlmIdentity();
    return 0;
}
if (args is ["--compiled-enemy-sine"])
{
    VerifyCompiledEnemyTrigonometry();
    return 0;
}
if (args is ["--downward-gate-definitions"])
{
    VerifyDownwardGateShotBlockDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--speed-booster-escape-definitions"])
{
    VerifySpeedBoosterEscapeStageDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--door-closing-definitions"])
{
    VerifyDoorClosingPlmDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--arm-cannon-definitions"])
{
    VerifySamusArmCannonDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--tourian-access-definitions"])
{
    VerifyTourianAccessPlmDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--chozo-plm-definitions"])
{
    VerifyChozoStatuePlmDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--samus-eater-plm-definitions"])
{
    VerifySamusEaterPlmDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--station-access-plm-definitions"])
{
    VerifyStationAccessPlmDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--quicksand-definitions"])
{
    VerifyQuicksandDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifyQuicksand();
    return 0;
}
if (args is ["--save-station-animation-definitions"])
{
    VerifySaveStationAnimationDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--enemy-drop-chance-definitions"])
{
    VerifyEnemyDropChanceDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--enemy-vulnerability-definitions"])
{
    VerifyEnemyVulnerabilityDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--escape-etecoon-definitions"])
{
    VerifyEscapeEtecoonDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--yard-turn-definitions"])
{
    VerifyYardTurnDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--ridley-explosion-definitions"])
{
    VerifyRidleyExplosionDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--choot-pattern-definitions"])
{
    VerifyChootPatternDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--enemy-instruction-selectors"])
{
    VerifyEnemyRomTablePointerCatalog();
    return 0;
}
if (args is ["--kraid-head-instruction-definitions"])
{
    VerifyKraidHeadInstructionDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--magic-number-audit"])
{
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--explored-map-packing-definitions"])
{
    VerifyExploredMapPackingDefinitions();
    return 0;
}
if (args is ["--enemy-death-explosion-definitions"])
{
    VerifyEnemyDeathExplosionDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--ceres-door-initialization-definitions"])
{
    VerifyCeresDoorInitializationDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--botwoon-instruction-definitions"])
{
    VerifyBotwoonInstructionDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--botwoon-navigation-definitions"])
{
    VerifyBotwoonNavigationDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--enemy-projectile-definitions"])
{
    VerifyEnemyProjectileDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--room-palette-fx-definitions"])
{
    VerifyRoomPaletteFxDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--ceres-ridley-eye-fade-definitions"])
{
    VerifyCeresRidleyEyeFadeDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--crystal-flash-palette-timing-definitions"])
{
    VerifyCrystalFlashPaletteTimingDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--work-robot-palette-timing-definitions"])
{
    VerifyWorkRobotPaletteTimingDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--samus-death-explosion-timing-definitions"])
{
    VerifySamusDeathExplosionTimingDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifySamusDeathSequence();
    return 0;
}
if (args is ["--hyper-beam-palette-fx-program-definitions"])
{
    VerifyHyperBeamPaletteFxProgramDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifySamusDrainedController();
    return 0;
}
if (args is ["--suit-pickup-beam-curve-definitions"])
{
    VerifySuitPickupBeamCurveDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifyPermanentCollectibles();
    return 0;
}
if (args is ["--palette-fx-instruction-codes"])
{
    VerifyPaletteFxInstructionCodeCatalogs();
    return 0;
}
if (args is ["--enemy-drops"])
{
    VerifyEnemyDrops();
    return 0;
}
if (args is ["--room-sprite-object-definitions"])
{
    VerifyRoomSpriteObjectDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--dead-sidehopper-corpse-definitions"])
{
    VerifyDeadSidehopperCorpseDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--dead-tourian-corpse-definitions"])
{
    VerifyDeadTourianCorpseDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--dead-torizo-corpse-definitions"])
{
    VerifyDeadTorizoCorpseDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--gunship-motion-definitions"])
{
    VerifyGunshipMotionDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifyPostCeresGunshipLanding();
    return 0;
}
if (args is ["--remaining-signed-sine-consumers"])
{
    SuperMetroidAddressSpace rom =
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    VerifyBombTorizoDroolSine(rom);
    VerifyMotherBrainNeckSine(rom);
    return 0;
}
if (args is ["--crocomire-bridge-fragment-definitions"])
{
    VerifyCrocomireBridgeFragmentDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--crocomire-melting-definitions"])
{
    VerifyCrocomireMeltingDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--draygon-intro-dance-definitions"])
{
    VerifyDraygonIntroDanceDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--phantoon-sound-definitions"])
{
    VerifyPhantoonSoundDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--baby-metroid-route-definitions"])
{
    VerifyBabyMetroidCutsceneEntrance();
    return 0;
}
if (args is ["--mother-brain-contact-hitboxes"])
{
    VerifyMotherBrainContactHitboxes();
    return 0;
}
if (args is ["--mother-brain-turret-definitions"])
{
    VerifyMotherBrainTurretDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--mother-brain-glass-shard-definitions"])
{
    VerifyMotherBrainGlassShardDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--mama-turtle-shell-contour"])
{
    VerifyMamaTurtleShellContourDefinitions();
    return 0;
}
if (args is ["--maridia-large-snail-instruction-definitions"])
{
    VerifyMaridiaLargeSnailInstructionDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--mama-turtle-enemy-definitions"])
{
    VerifyMamaTurtleEnemyDefinitions();
    return 0;
}
if (args is ["--pose-dispatch-definitions"])
{
    var poseRom = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    VerifyPoseDispatchDefinitions(poseRom);
    VerifyPoseCollisionDefinitions(poseRom);
    VerifyPoseProjectileOrigin(poseRom);
    VerifySamusHudDefinitions(poseRom);
    return 0;
}
if (args is ["--pose-input-definitions"])
{
    VerifyPoseInputDefinitions(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-rope-geometry"])
{
    VerifyGrappleRopeGeometry(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-sprite-artwork"])
{
    VerifyGrappleSpriteArtwork(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-tile-artwork"])
{
    VerifyGrappleTileArtwork(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-flare-placement"])
{
    VerifyGrappleFlarePlacement(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-swing-frames"])
{
    VerifyGrappleBodyPlacement(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--stock-attract-scenes"])
{
    VerifyStockAttractScenes();
    return 0;
}
if (args is ["--native-boss-markers", var bossTrace])
{
    VerifyNativeBossMarkers(bossTrace);
    return 0;
}
if (args is ["--pause-boss-markers"])
{
    VerifyPauseBossMarkers();
    return 0;
}
if (args is ["--crystal-palette-native", var paletteRom, var paletteTrace])
{
    VerifyCrystalPaletteNative(paletteRom, paletteTrace);
    return 0;
}
if (args is ["--crystal-window-native", var windowRom, var windowTrace])
{
    VerifyCrystalWindowNative(windowRom, windowTrace);
    return 0;
}
if (args is ["--crystal-flash-contact-native", var contactRom, var contactTrace])
{
    VerifyCrystalFlashContactNative(contactRom, contactTrace);
    return 0;
}
if (args is ["--crystal-flash-runtime"])
{
    VerifyCrystalFlashRuntime();
    return 0;
}
if (args is ["--crystal-flash-lifetime", var lifetimeRom, var lifetimeTrace])
{
    VerifyCrystalFlashLifetime(lifetimeRom, lifetimeTrace);
    return 0;
}
if (args is ["--crystal-flash-native", var crystalRom, var crystalTrace])
{
    VerifyCrystalFlashCleanup(crystalRom, crystalTrace);
    return 0;
}
if (args is ["--crystal-flash"])
{
    VerifySamusCrystalFlash();
    return 0;
}
if (args is ["--plasma-penetration"])
{
    VerifyPlasmaEnemyPenetration();
    return 0;
}
if (args is ["--pcm-loop-entry"])
{
    VerifyManagedDspUsesIndependentLoopEntry();
    return 0;
}
if (args is ["--audio-bank-transition", var audioDirectory])
{
    VerifyAudioBankTransition(audioDirectory);
    return 0;
}
if (args is ["--wall-spread-trace", var wallTrace])
{
    VerifyAerialSpreadTransitions(outputPath: wallTrace, wallRoute: true);
    return 0;
}
if (args is ["--wall-spread-transition", var wallNative])
{
    VerifyAerialSpreadTransitions(tracePath: wallNative, wallRoute: true);
    return 0;
}
if (args is ["--wall-spread-transition"])
{
    VerifyAerialSpreadTransitions(wallRoute: true);
    return 0;
}
if (args is ["--aerial-spread-trace", var aerialTrace])
{
    VerifyAerialSpreadTransitions(outputPath: aerialTrace);
    return 0;
}
if (args is ["--aerial-spread-transition"])
{
    VerifyAerialSpreadTransitions();
    return 0;
}
if (args is ["--aerial-spread-transition", var nativeAerialTrace])
{
    VerifyAerialSpreadTransitions(nativeAerialTrace);
    return 0;
}
if (args is ["--grounded-spread-transition"])
{
    VerifyGroundedSpreadTransition();
    return 0;
}
if (args is ["--grounded-spread-transition", var transitionTrace])
{
    VerifyGroundedSpreadTransition(transitionTrace);
    return 0;
}
if (args is ["--grounded-bomb-spread-native", var spreadTrace])
{
    VerifyGroundedBombSpreadNative(spreadTrace);
    return 0;
}
if (args is ["--grounded-bomb-spread"])
{
    VerifyGroundedBombSpread();
    return 0;
}
if (args is ["--map-installation", var installationRom])
{
    VerifyMapInstallation(installationRom);
    return 0;
}
if (args is ["--map-presentation"])
{
    VerifyMapPresentation();
    return 0;
}
if (args is ["--load-station-definitions"])
{
    VerifyCompiledLoadStationDefinitions();
    return 0;
}
if (args is ["--room-header-definitions"])
{
    VerifyCompiledRoomHeaderDefinitions();
    return 0;
}
if (args is ["--room-state-payloads"])
{
    VerifyCompiledRoomStateDefinitions();
    return 0;
}
if (args is ["--door-definitions"])
{
    VerifyCompiledDoorDefinitions();
    return 0;
}
if (args is ["--room-scroll-definitions"])
{
    VerifyCompiledRoomScrollDefinitions();
    return 0;
}
if (args is ["--room-callback-definitions"])
{
    VerifyCompiledRoomCallbackDefinitions();
    return 0;
}
if (args is ["--room-definition-integration"])
{
    VerifyCompiledRoomDefinitionIntegration();
    return 0;
}
if (args is ["--room-state-definitions"])
{
    VerifyCompiledRoomStateSelectionDefinitions();
    return 0;
}
if (args is ["--gameplay-message-titles", var messageTitleRom])
{
    VerifyGameplayMessageTitles(messageTitleRom);
    return 0;
}
if (args is ["--gameplay-message-panels", var messagePanelRom])
{
    VerifyGameplayMessagePanels(messagePanelRom);
    return 0;
}
if (args is ["--gameplay-message-notices", var messageNoticeRom])
{
    VerifyGameplayMessageNotices(messageNoticeRom);
    return 0;
}
if (args is ["--gameplay-message-definitions"])
{
    VerifyGameplayMessageDefinitions();
    return 0;
}
if (args is ["--escape-typewriter-presentation", var escapeTextRom])
{
    VerifyEscapeTypewriterPresentation(escapeTextRom);
    return 0;
}
if (args is ["--intro-narration-presentation", var narrationRom])
{
    VerifyIntroNarrationPresentation(narrationRom);
    return 0;
}
if (args is ["--ending-text-presentation", var endingTextRom])
{
    VerifyEndingTextPresentation(endingTextRom);
    return 0;
}
if (args is ["--credits-presentation", var creditsRom])
{
    VerifyCreditsPresentation(creditsRom);
    return 0;
}
if (args is ["--pause-reserve-hud"])
{
    VerifyPauseReserveHud();
    return 0;
}
if (args is ["--reserve-auto-frontend"])
{
    VerifyReserveAutoFrontend();
    return 0;
}
if (args is ["--health-warning"])
{
    VerifyHealthWarning();
    return 0;
}
if (args is ["--health-warning-native", var healthWarningTrace])
{
    VerifyHealthWarningNative(healthWarningTrace);
    return 0;
}
if (args is ["--reserve-native-trace", var reserveTrace])
{
    VerifyReserveNativeTrace(reserveTrace);
    return 0;
}
if (args is ["--reserve-mode", var reserveModeTrace])
{
    VerifyReserveMode(reserveModeTrace);
    return 0;
}
if (args is ["--cinematic-flash", var cinematicFlashTrace])
{
    VerifyCinematicCrystalFlash(cinematicFlashTrace);
    return 0;
}
if (args is ["--pause-reserve-manual"])
{
    VerifyPauseReserveManual();
    return 0;
}
if (args is ["--pause-reserve-arrow"])
{
    VerifyPauseReserveArrow();
    return 0;
}
if (args is ["--pause-reserve-native"])
{
    VerifyPauseReserveNativePixels();
    return 0;
}
if (args is ["--pause-reserve-tanks"])
{
    VerifyPauseReserveTanks();
    return 0;
}
if (args is ["--draygon-tilemap-production"])
{
    VerifyDraygonTilemapProduction();
    return 0;
}
if (args is ["--projectile-runtime-phase"])
{
    VerifyProjectileRuntimePhase();
    return 0;
}
if (args is ["--projectile-contact-phase"] or ["--projectile-contact-damage"])
{
    VerifyProjectileContactPhase(args[0] == "--projectile-contact-phase");
    return 0;
}
if (args is ["--enemy-contact-phase"])
{
    VerifyRipperEnemy(verifyDeferredContact: true);
    return 0;
}
if (args is ["--ripper-enemy"])
{
    VerifyRipperEnemy();
    return 0;
}
if (args is ["--ceres-door-boss"])
{
    VerifyCeresDoorBossBranch();
    return 0;
}
if (args is ["--x-plasma-timers"])
{
    VerifyRipperEnemy(verifyXrayTimers: true);
    return 0;
}
if (args is ["--native-x-plasma-timers", var timerTrace])
{
    VerifyRipperEnemy(verifyXrayTimers: true, nativeXrayTimerTrace: timerTrace);
    return 0;
}
if (args is ["--window-pixels"])
{
    VerifyWindowPixels();
    VerifyGameplaySnapshots();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--hardware-windows"])
{
    VerifyHardwareWindows();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--power-bomb-fuse"])
{
    VerifyPowerBombFuse();
    VerifyPowerBombBoundary();
    VerifyPowerBombRuntimeRendererIntegration();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--beam-callback-tables"])
{
    VerifyBeamCallbackTables();
    return 0;
}
if (args is ["--beam-speed-rows"])
{
    VerifyBeamSpeedRows();
    VerifySamusPowerBeamProjectiles();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--chainsaw-firing"])
{
    VerifyChainsawFiring();
    return 0;
}
if (args is ["--spacetime-beam"])
{
    VerifySpacetimeBeam();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--murder-beam"])
{
    VerifyMurderBeam();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--invalid-beam-graphics"])
{
    VerifyInvalidBeamGraphics();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--invalid-beam-selection"])
{
    VerifyPauseMenuEquipmentInteraction();
    VerifyInvalidBeamSelection();
    return 0;
}
if (args is ["--spin-entry-audio"])
{
    VerifySamusSpaceJumpAndScrewAttack();
    VerifySamusAtmosphericEffects();
    return 0;
}
if (args is ["--boost-floor-scroll"])
{
    VerifyBoostFloorScroll();
    return 0;
}
if (args is ["--xray-controls"])
{
    VerifyXrayControls();
    return 0;
}
if (args is ["--elevatube-scrolling"])
{
    VerifyElevatubeScrolling();
    return 0;
}
if (args is ["--maridia-puyo-pile"])
{
    VerifyMaridiaPuyoPile();
    return 0;
}
if (args is ["--moat-first-entry"])
{
    VerifyMoatFirstEntry();
    return 0;
}
if (args is ["--pillar-first-entry"])
{
    VerifyPillarFirstEntry();
    return 0;
}
if (args is ["--ridley-acid"])
{
    VerifyRidleyAcid();
    return 0;
}
if (args is ["--treadmill-visual"])
{
    VerifyTreadmillVisual();
    return 0;
}
if (args is ["--metroid-bomb-placement"])
{
    VerifyMetroidBombPlacement();
    return 0;
}
if (args is ["--fireflea-eye"])
{
    VerifyFirefleaEye();
    return 0;
}
if (args is ["--yard-trajectories"])
{
    VerifyYardTrajectories();
    return 0;
}
if (args is ["--statue-splash"])
{
    VerifyStatueSplash();
    return 0;
}
if (args is ["--projectile-quake"])
{
    VerifyProjectileQuake();
    return 0;
}
if (args is ["--pause-reserve-labels"])
{
    VerifyPauseReserveLabels();
    return 0;
}
if (args is ["--draygon-goop-drops"])
{
    VerifyDraygonGoopDrops();
    return 0;
}
if (args is ["--ending-takeoff-wrap"] or ["--ending-takeoff-math"])
{
    VerifyEndingTakeoffColorMath(args[0] == "--ending-takeoff-wrap");
    return 0;
}
if (args is ["--ending-planet-boundary"])
{
    VerifyEndingPlanetBoundary();
    return 0;
}
if (args is ["--ending-native-ppu"] or ["--ending-native-offset-check"])
{
    VerifyEndingNativePpu(args[0] == "--ending-native-offset-check");
    return 0;
}
if (args is ["--ending-native-finale"])
{
    for (int frame = 512; frame <= 800; frame += 16) VerifyEndingNativePpu(frame: frame);
    return 0;
}
if (args is ["--ending-native-burst"])
{
    for (int frame = 400; frame <= 464; frame += 16) VerifyEndingNativePpu(frame: frame);
    return 0;
}
if (args is ["--ending-reward-definitions"])
{
    VerifyEndingRewardGesture();
    return 0;
}
if (args is ["--intro-mother-brain-definitions"])
{
    VerifyIntroMotherBrainDefinitions();
    return 0;
}
if (args is ["--intro-rinka-definitions"])
{
    VerifyIntroRinkaDefinitions();
    return 0;
}
if (args is ["--intro-baby-actor-definitions"])
{
    VerifyIntroBabyActorDefinitions();
    return 0;
}
if (args is ["--intro-egg-effect-definitions"])
{
    VerifyIntroEggEffectDefinitions();
    return 0;
}
if (args is ["--ceres-explosion-definitions"])
{
    VerifyCeresExplosionDefinitions();
    return 0;
}
if (args is ["--ceres-flight-actor-definitions"])
{
    VerifyCeresFlightActorDefinitions();
    return 0;
}
if (args is ["--ceres-destruction-actor-definitions"])
{
    VerifyCeresDestructionActorDefinitions();
    return 0;
}
if (args.Contains("--ending-dma"))
{
    VerifyEndingDma();
    VerifyEndingRenderSnapshots();
    return 0;
}
if (args.Length == 2 && args[0] == "--mother-brain-health")
{
    VerifyMotherBrainHealthPalette(args[1]);
    return 0;
}
if (args.Length == 2 && args[0] == "--escape-animals")
{
    VerifyEscapeAnimalBlocks(args[1]);
    VerifyEscapeRoomEffects(args[1]);
    return 0;
}
if (args.Contains("--escape-timer"))
{
    VerifyEscapeTimerBcd();
    VerifyEscapeTimerStateMachine();
    VerifyEscapeTimerFloor();
    VerifyControllerInputRecording();
    return 0;
}
VerifyAndroidHostPolicies();
VerifyBoostFloorScroll();
VerifyXrayControls();
VerifyIntroPoseHistory();
VerifyCinematicTextGlow();
VerifyProjectileContactPhase(verifyPhase: true);
VerifyProjectileRuntimePhase();
VerifyMorphedSpikeRelease();
VerifySpikeShinesparkSuit();
VerifyReserveMode("csharp/test-fixtures/movement-release/reserve-mode-433.csv");
VerifyCinematicCrystalFlash("csharp/test-fixtures/movement-release/cinematic-flash-432.csv");
VerifyElevatubeScrolling();
VerifyIniEditing();
VerifyBackgroundSampler();
if (args is ["--ini-edit"]) return 0;
if (args is ["--ceres-ridley-room-entry"])
{
    VerifyCeresRidleyRoomEntry();
    return 0;
}
if (args is ["--mother-brain"])
{
    VerifyMotherBrainBeamWindow();
    VerifyMotherBrainDeathHandoff();
    VerifySamusDrainedController();
    VerifyMotherBrainRainbowBeamSamusMovement();
    VerifyMotherBrainRainbowBeamAttackSequence();
    VerifyEnemyProjectileInstructionMechanicsDefinitions();
    VerifyMotherBrainBombProjectiles();
    VerifyMotherBrainProjectileRendering();
    VerifyMiscDustProjectiles();
    VerifyMotherBrainEscapeDoorParticles();
    VerifyBabyMetroidCutsceneEntrance();
    return 0;
}
if (args is ["--grapple-movement"])
{
    VerifySamusGrappleRomData();
    VerifySamusGrappleSwingAndRelease();
    return 0;
}
if (args is ["--shinespark"])
{
    VerifySamusStoredShineAndShinespark();
    Console.WriteLine("PASS shinespark: native movement, energy cutoff, invincibility, and crash lifecycle.");
    return 0;
}
if (args is ["--file-select-sound"])
{
    VerifyFileSelectSound();
    return 0;
}
if (args is ["--enemy-contact-death"])
{
    VerifyContactDeathStopsEnemyDispatch();
    return 0;
}
if (args is ["--android-host"])
    return 0;
if (args is ["--audio-queues"])
{
    VerifyCartridgeAudioQueues();
    return 0;
}
if (args is ["--audio-instruments"])
{
    VerifyEditableAudioInstruments();
    return 0;
}
if (args is ["--audio-overrides"])
{
    VerifyPersistentAudioOverrides();
    return 0;
}
if (args is ["--audio-sfx-programs"])
{
    VerifyEditableSoundEffectPrograms();
    return 0;
}
if (args is ["--audio-music-programs"])
{
    VerifyEditableMusicPrograms();
    return 0;
}
if (args is ["--shutter-native-arc"])
{
    AuditMorphShutterApproaches(reproduceOnly: true, exportNativeArc: true);
    return 0;
}
if (args is ["--bomb-wall"])
{
    VerifyBombJumpWallContact();
    return 0;
}
if (args is ["--shutter-morph-repro"])
{
    AuditMorphShutterApproaches(reproduceOnly: true);
    return 0;
}
if (args is ["--shutter-morph-approaches"])
{
    AuditMorphShutterApproaches();
    return 0;
}
if (args is ["--shutter-repeat"])
{
    AuditRepeatedShutterBombs();
    return 0;
}
if (args is ["--boost-floor-audit"])
{
    AuditBoostFloor();
    return 0;
}
if (args is ["--ocean-sky"])
{
    VerifyOceanSky();
    return 0;
}
if (args is ["--shutter-riding"])
{
    VerifyShutterRiding();
    return 0;
}
if (args is ["--shutter-embedding"])
{
    VerifyShutterEmbedding();
    return 0;
}
if (args is ["--xray-input"])
{
    VerifyXrayInput();
    return 0;
}
if (args is ["--xray-setup"])
{
    VerifyXraySetupBuffers();
    return 0;
}
if (args is ["--fireflea-fx"])
{
    VerifyFirefleaFx();
    return 0;
}
if (args is ["--xray-window-geometry"])
{
    VerifyXrayWindowGeometry();
    return 0;
}
if (args is ["--xray-reveal"])
{
    VerifyXrayRoomDisplayRules();
    VerifyXrayRevealTable();
    VerifyXrayExtensions();
    VerifyXrayTilemap();
    VerifyXrayOverlays();
    return 0;
}
if (args is ["--grapple-enemy-death"])
{
    VerifyGrappleEnemyDeath();
    return 0;
}
if (args is ["--samus-grapple"])
{
    VerifySamusGrappleSwingAndRelease();
    return 0;
}
if (args is ["--samus-xray"])
{
    VerifySamusXray();
    return 0;
}
if (args is ["--plm-draw-clone"])
{
    VerifyPlmDrawClone();
    return 0;
}
if (args is ["--grapple-resident-trigger"])
{
    VerifyNoobTubePlm();
    VerifyPermanentCollectibles();
    return 0;
}
if (args is ["--lower-norfair-hand"])
{
    VerifyLowerNorfairHand();
    return 0;
}
if (args is ["--draygon-defeated-room"])
{
    VerifyDraygonDefeatedRoom();
    return 0;
}
if (args is ["--grapple-gates"])
{
    VerifyGrappleGreenGateVisibility();
    return 0;
}
if (args is ["--grapple-spin"])
{
    VerifyGrappleSpinInput();
    return 0;
}
if (args is ["--grapple-sounds"])
{
    VerifyGrappleSounds();
    return 0;
}
if (args is ["--grapple-doors"])
{
    VerifyGrappleBlueDoors();
    VerifyGrapplePoseRefire();
    return 0;
}
if (args is ["--phantoon-position"])
{
    VerifyPhantoonPosition();
    return 0;
}
if (args.Length > 1 || (args.Length == 1 && args[0] != "--render-contract"))
    throw new ArgumentException("Usage: SuperMetroid.Verification [--render-contract | --phantoon-position | --samus-grapple | --samus-xray | --grapple-doors | --grapple-sounds | --grapple-spin | --grapple-gates | --grapple-enemy-death | --shutter-riding | --shutter-embedding]");
Console.WriteLine("Verifying translated Super Metroid routines...");
VerifyPlmDrawClone();
VerifyGameConfigurationIni();
VerifyViewportTileRowParity();
VerifyPpuMemorySnapshotOwnership();
VerifyTitleRenderSnapshots();
VerifyPauseRenderSnapshots();
VerifyPauseBossMarkers();
VerifyFileMenuRenderSnapshots();
VerifyRenderFrameHandoff();
VerifyRenderPresentationGate();
VerifyRenderPacketCodec();
VerifyFrontendRenderCapture();
VerifyCinematicRenderSnapshots();
VerifyGameplaySnapshots();
VerifyWindowPixels();
VerifyColorWindowSnapshots();
VerifyMessageSnapshots();
VerifyGameplayMessageDefinitions();
VerifyEyeWindowSnapshots();
VerifyRoomFxSnapshots();
VerifyGameplayCaptureIntegration();
VerifyMode7GameplaySnapshots();
VerifyEndingRenderSnapshots();
VerifyFileMapSnapshots();
VerifyAttractCapture();

// This portable gate retains every snapshot, codec, publication, scene-capture and
// software parity check above. It deliberately excludes unrelated gameplay audits,
// and never loads the Windows desktop or a graphics backend.
if (args.Length == 1)
{
    Console.WriteLine($"Portable render contract passed on {RuntimeInformation.OSDescription}; {RuntimeInformation.FrameworkDescription}.");
    return 0;
}

VerifyRandomNumberGeneratorExhaustively();
SaveLoadRandomAudit.Run(Path.GetFullPath("Super Metroid.smc"));
        VerifySandAnimatedTiles();
        VerifyQuicksand();
        VerifyTreadmillPhysics();
        VerifyPhantoonPosition();
        VerifyPausePaletteSound();
VerifyKnownRandomSequence();
VerifyTimedHeldInputTimeline();
VerifyEventBitfield();
VerifyBossBitfield();
VerifyMultiplicationExhaustively();
VerifySmCompressionFormat();
VerifyVramWriteQueue();
VerifyEscapeTimerBcd();
VerifyEscapeTimerStateMachine();
VerifyEscapeTimerFloor();
VerifyControllerInputLatch();
VerifyGameOptionsRomDataCatalog();
VerifyControllerBindingsAndOptionsSubmenus();
VerifyGameOverRomData();
VerifyTitleSequenceRomData();
VerifyStrictFailureBoundaries();
VerifyReserveAutoRecovery();
VerifyHealthWarning();
VerifyReserveAutoFrontend();
VerifyPauseReserveManual();
VerifyPauseReserveArrow();
VerifyPauseReserveTanks();
VerifyPauseReserveHud();
VerifyDoorAlignmentParity();
VerifyDoorOpeningTrajectories();
VerifyCreditsObjectInterpreter();
VerifyEndingCreditsState();
VerifyGenericGamepadInput();
VerifyDemoInputObject();
VerifyAttractDemoScene();
VerifyAttractDemoControllerOverride();
VerifyGrappleDemoTrajectory();
VerifyFrameRuntime();
VerifyGameTimeState();
VerifySuperMetroidAddressSpace();
VerifyCartridgeAudioQueues();
VerifyManagedSnesDsp();
VerifyTypedNativeWords();
VerifyProductionMagicNumberAudit();
VerifyPhantoonWaveLifecycle();
VerifyBackgroundMosaicSampling();
VerifyOamSpritemapPacking();
VerifyCeresElevatorArrivalGraphicsIndex();
VerifySamusRenderingSlice();
VerifySamusMovementRomData();
VerifySamusRenderingRomData();
VerifySamusPaletteRomData();
VerifySamusSpecialSequenceRomData();
VerifySamusArmCannon();
VerifySamusProjectileRomData();
VerifySamusGrappleRomData();
VerifySamusXrayRomData();
VerifySamusHudSelection();
VerifySamusVisorPalette();
VerifySamusHurtFlashPalette();
VerifySamusPoseTransitionMatching();
VerifySamusHorizontalSpeed();
VerifySamusExtraDisplacement();
VerifySamusStoredShineAndShinespark();
VerifySamusCrystalFlash();
VerifyCrystalFlashRuntime();
VerifySamusXray();
VerifySamusDeathSequence();
VerifyMotherBrainBeamWindow();
VerifyMotherBrainDeathHandoff();
VerifySamusDrainedController();
VerifySamusGrabbedByDraygon();
VerifyMotherBrainRainbowBeamSamusMovement();
VerifyMotherBrainRainbowBeamAttackSequence();
VerifyEnemyProjectileInstructionMechanicsDefinitions();
VerifyMotherBrainBombProjectiles();
VerifyMotherBrainProjectileRendering();
VerifyMiscDustProjectiles();
VerifyMotherBrainEscapeDoorParticles();
VerifyBabyMetroidCutsceneEntrance();
VerifySamusSolidEnemyCollision();
VerifySamusAerialMovement();
VerifyZeroDistanceJumpContact();
VerifyShinesparkEnemyStop();
VerifyKnockbackHorizontalStop();
VerifyRetailFallingSpeedRecurrence();
VerifyCrampedAerialLandingPoseCollision();
VerifySamusSpaceJumpAndScrewAttack();
VerifySamusLiquidPhysics();
VerifySamusAtmosphericEffects();
VerifySamusAerialTurnsAndWallJump();
VerifySamusPoseHistory();
VerifyWallJumpDust();
VerifyCeresHazeLifecycle();
VerifyCeresRidleyWallImpact();
VerifySamusKnockbackAndDamageBoost();
VerifySamusGrappleSwingAndRelease();
VerifyGrappleBlueDoors();
VerifyGrappleSounds();
VerifyGrapplePoseRefire();
VerifyGrappleSpinInput();
VerifyGrappleGreenGateVisibility();
VerifyGrappleEnemyDeath();
VerifyShutterRiding();
VerifyXrayInput();
VerifyXrayWindowGeometry();
VerifyXraySetupBuffers();
VerifyFirefleaFx();
VerifyBombJumpWallContact();
VerifyXrayRoomDisplayRules();
VerifyXrayRevealTable();
VerifyXrayExtensions();
VerifyXrayTilemap();
VerifyXrayOverlays();
VerifyBreakableGrapplePlms();
VerifyPermanentCollectibles();
VerifyEnemyDrops();
VerifySamusPostureMovement();
VerifySamusPowerBeamProjectiles();
ProbeProjectileVelocityInheritance();
VerifyProjectileCooldowns();
VerifyWrapShotTrace("csharp/test-fixtures/movement-release/wrap-shot-409.csv");
VerifyRetailWrapShotDoors();
VerifyWrapShotEnemySeparation();
VerifyWrapShotWidths("csharp/test-fixtures/movement-release/wrap-width-409.csv");
VerifyCeilingWrapPlmTrace("csharp/test-fixtures/movement-release/ceiling-plm-410.csv");
VerifyKronicGateBeamCollision();
VerifyGateJumpTraces();
VerifyRightFacingGateGlitches();
VerifyGreenHillGrappleSpeedGateGlitch();
VerifyGModeGateGlitch();
VerifyFrozenEnemyGateGlitch();
VerifyMochtroidBotwoonPipeClip();
VerifySparkCrashAlignment();
VerifyEnemyAngleDivision();
VerifyDraygonEyeEffects();
VerifyDraygonTilemapProduction();
VerifyFrogSpeedwayPoolCollision();
VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv");
VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv", 9);
VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-run-410.csv", 11);
VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-walk-410.csv", 11, false);
VerifyHeroShotCameraLifetime("csharp/test-fixtures/movement-release/hero-shot-411.csv");
VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv");
VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv");
VerifyControlledRedTowerHeroShot();
VerifyBeamSpeedRows();
VerifyBeamCallbackTables();
VerifySamusMorphBallMovement();
VerifyCompactWalkOffCollision();
VerifyPauseMomentumReconciliation();
VerifyBombChargeRejection();
VerifyGroundedBombSpread();
VerifyGroundedSpreadTransition();
VerifyAerialSpreadTransitions();
VerifyAerialSpreadTransitions(wallRoute: true);
VerifySamusStandingAimMovement();
VerifySamusAimedAerialMovement();
VerifySamusGunExtendedMovement();
VerifySamusSlopePhysics();
VerifySamusBlockCollision();
VerifySpeedBoosterCollisionBlocks();
VerifySamusGroundedMovement();
VerifySamusGroundedReversal();
VerifySamusMoonwalking();
VerifySamusRanIntoWall();
VerifyObjRendering();
VerifyHudStateAndBg3Rendering();
VerifyDebugRoomCamera();
VerifyRoomScrollGridAndBoundaryCamera();
VerifyRoomScrollPlms();
VerifyRoomPlmHeaderCatalog();
VerifyRoomPlmInstructionListCatalog();
VerifySequentialRoomPlmPopulationLoader();
VerifyNoobTubePlm();
VerifyDownwardGatePlms();
VerifyEyeDoorPlms();
VerifyDraygonCannonPlms();
VerifyBombTorizoHandPlm();
VerifyPauseMenuEquipmentInteraction();
VerifyInvalidBeamSelection();
VerifyInvalidBeamGraphics();
VerifyMovedSamusCameraTracking();
VerifyBackgroundScrollState();
VerifyLevelBlockTilemapExpansion();
VerifyRoomLevelData();
VerifyCartridgeRoomStateSelection();
VerifyCompiledRoomHeaderDefinitions();
VerifyCompiledRoomStateDefinitions();
VerifyCompiledRoomStateSelectionDefinitions();
VerifyCompiledLoadStationDefinitions();
VerifyCompiledDoorDefinitions();
VerifyCompiledRoomScrollDefinitions();
VerifyCompiledRoomCallbackDefinitions();
VerifyCompiledRoomDefinitionIntegration();
VerifyRoomMainCodeCatalog();
VerifyRoomSetupCodeCatalog();
VerifyRoomAssetRomData();
VerifyAreaMapAssets();
VerifyMapPresentation();
VerifyBackgroundTilemapStreamer();
VerifyFourBitBackgroundRendering();
VerifyLoRomCrossBankCompressedData();
VerifyMode7Rendering();
VerifyLayerCompositorBackdrop();
VerifyBgPriorityPlaneRendering();
VerifyLibraryBackgroundLoader();
VerifyControllerInputRecording();
VerifySaveRamLayout();
VerifyExploredMapPackingDefinitions();
VerifyGameSaveJsonPersistence();
VerifyFileSelectFreshSaveTilemap();
VerifyFileSelectMapWindow();
VerifySavedGameLoadAppearance();
VerifyIntroCinematicRomData();
VerifyIntroGameplayFlashbackVerticalScroll();
VerifyIntroMotherBrainDefinitions();
VerifyIntroRinkaDefinitions();
VerifyIntroBabyActorDefinitions();
VerifyIntroEggEffectDefinitions();
VerifyCeresExplosionDefinitions();
VerifyCeresFlightActorDefinitions();
VerifyCeresDestructionActorDefinitions();
VerifyCinematicPaletteFader();
VerifyHostRoomViewportAlignment();
VerifyPowerBombColorMathWindow();
VerifyHardwareWindows();
VerifyChainsawFiring();
VerifySpacetimeBeam();
VerifyMurderBeam();
VerifyPowerBombRuntimeRendererIntegration();
VerifyPowerBombFuse();
VerifyPowerBombBoundary();
VerifyRoomFxRomData();
VerifyScrollingSkyState();
VerifyOceanSky();
AuditBoostFloor();
VerifyEnemyAiCodePointerCatalog();
VerifyEnemyInstructionCodePointerCatalogs();
VerifyEnemyRomTablePointerCatalog();
VerifyMotherBrainContactHitboxes();
VerifyMamaTurtleShellContourDefinitions();
VerifyMamaTurtleEnemyDefinitions();
VerifyPaletteFxInstructionCodeCatalogs();
VerifyAnimatedTileInstructionCodeCatalog();
VerifyEnemyProjectileCodePointerCatalog();
VerifyRoomEnemyLoading();
VerifyBotwoonPlmIdentity();
VerifyCompiledEnemyTrigonometry();
VerifyRidleyExplosionDefinitions(
    SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyCrocomireMeltingDefinitions(
    SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyDraygonIntroDanceDefinitions(
    SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyEnemyProjectileCollisionLifecycle();
VerifyRipperEnemy();
VerifyRipperEnemy(verifyXrayTimers: true);
VerifyPostCeresGunshipLanding();
VerifyCeresElevatorPlatformAnimation();
VerifyCeresDoorBossBranch();
VerifyCeresRidleyRoomEntry();
VerifyCeresEscapeHandoff();
VerifyCeresDestructionCinematic();

Console.WriteLine("All bank $80 verification checks passed.");
return 0;
}
catch (Exception exception)
{
    // This is deliberately handled here, at the process boundary. Assertions still stop the
    // verifier immediately, but Windows never receives an unhandled CLR exception that it can
    // turn into a focus-stealing dialog. ToString() retains the type, message, inner exception,
    // and complete stack trace in the terminal where the failure is actually actionable.
    Console.Error.WriteLine(exception);
    return 1;
}

}

}
