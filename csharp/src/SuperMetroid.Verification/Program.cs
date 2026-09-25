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
if (args is ["--collectible-visuals"])
{
    VerifyCollectibleVisuals();
    return 0;
}
if (args is ["--sand-animated-tiles"])
{
    VerifySandAnimatedTiles();
    return 0;
}
if (args is ["--room-fx-animated-tiles"])
{
    VerifyRoomFxAnimatedTileMechanicsDefinitions();
    VerifyRoomFxAnimatedTileArtwork();
    return 0;
}
if (args is ["--room-fx-layer3-tilemaps"])
{
    VerifyRoomFxLayer3Tilemaps();
    return 0;
}
if (args is ["--room-fx-palette-blends"])
{
    VerifyRoomFxPaletteBlends();
    return 0;
}
if (args is ["--power-bomb-fixed-colors"])
{
    VerifyPowerBombFixedColors();
    return 0;
}
if (args is ["--samus-visor-colors"])
{
    VerifySamusVisorColors();
    return 0;
}
if (args is ["--samus-hurt-colors"])
{
    VerifySamusHurtColors();
    return 0;
}
if (args is ["--samus-hyper-beam-colors"])
{
    VerifySamusHyperBeamColors();
    return 0;
}
if (args is ["--game-options-language-palettes"])
{
    VerifyGameOptionsLanguagePalettes();
    return 0;
}
if (args is ["--game-options-cursor-phases"])
{
    VerifyGameOptionsCursorPhases();
    return 0;
}
if (args is ["--normal-suit-palette-pointers"])
{
    VerifyNormalSuitPalettePointers();
    return 0;
}
if (args is ["--speed-boost-palette-pointers"])
{
    VerifySpeedBoostPalettePointers();
    return 0;
}
if (args is ["--full-body-palette-pointer-lists"])
{
    VerifyFullBodyPalettePointerLists();
    return 0;
}
if (args is ["--spc-sound-library-2-pointers"])
{
    VerifySpcSoundLibrary2Pointers();
    return 0;
}
if (args is ["--room-fx-retail-inventory"])
{
    VerifyRetailRoomFxInventory();
    return 0;
}
if (args is ["--enemy-projectile-instruction-mechanics"])
{
    VerifyEnemyProjectileInstructionMechanicsDefinitions();
    return 0;
}
if (args is ["--enemy-pickup-instruction-mechanics"])
{
    VerifyEnemyPickupInstructionProgramDefinitions();
    return 0;
}
if (args is ["--enemy-death-instruction-mechanics"])
{
    VerifyEnemyDeathInstructionProgramDefinitions();
    return 0;
}
if (args is ["--shaktool-projectile-instruction-mechanics"])
{
    VerifyShaktoolProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--chozo-tourian-dust-instruction-mechanics"])
{
    VerifyChozoTourianDustInstructionProgramDefinitions();
    return 0;
}
if (args is ["--tourian-statue-projectile-instruction-mechanics"])
{
    VerifyTourianStatueProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--spore-spawn-projectile-instruction-mechanics"])
{
    VerifySporeSpawnProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--botwoon-projectile-instruction-mechanics"])
{
    VerifyBotwoonProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-landing-dust-instruction-mechanics"])
{
    VerifyTorizoLandingDustInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-explosive-swipe-instruction-mechanics"])
{
    VerifyTorizoExplosiveSwipeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--bomb-torizo-drool-instruction-mechanics"])
{
    VerifyBombTorizoDroolInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-explosion-instruction-mechanics"])
{
    VerifyTorizoExplosionInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-chozo-orb-instruction-mechanics"])
{
    VerifyTorizoChozoOrbInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-sonic-boom-instruction-mechanics"])
{
    VerifyTorizoSonicBoomInstructionProgramDefinitions();
    return 0;
}
if (args is ["--bomb-torizo-statue-instruction-mechanics"])
{
    VerifyBombTorizoStatueInstructionProgramDefinitions();
    return 0;
}
if (args is ["--golden-torizo-egg-instruction-mechanics"])
{
    VerifyGoldenTorizoEggInstructionProgramDefinitions();
    return 0;
}
if (args is ["--golden-torizo-super-missile-instruction-mechanics"])
{
    VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--golden-torizo-eye-beam-instruction-mechanics"])
{
    VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions();
    return 0;
}
if (args is ["--enemy-projectile-instruction-owner-coverage"])
{
    VerifyEnemyProjectileInstructionOwnerCoverage();
    return 0;
}
if (args is ["--enemy-instruction-owner-coverage"])
{
    VerifyEnemyInstructionOwnerCoverage();
    return 0;
}
if (args is ["--mother-brain-body-instruction-mechanics"])
{
    VerifyMotherBrainBodyInstructionPrograms();
    return 0;
}
if (args is ["--mother-brain-room-palette-mechanics"])
{
    VerifyMotherBrainRoomPaletteProgramDefinitions();
    return 0;
}
if (args is ["--gunship-instruction-mechanics"])
{
    VerifyGunshipInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ridley-instruction-mechanics"])
{
    VerifyRidleyInstructionProgramDefinitions();
    return 0;
}
if (args is ["--draygon-instruction-mechanics"])
{
    VerifyDraygonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--walking-space-pirate-instruction-mechanics"])
{
    VerifyWalkingSpacePirateInstructionProgramDefinitions();
    return 0;
}
if (args is ["--wall-space-pirate-instruction-mechanics"])
{
    VerifyWallSpacePirateInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ninja-space-pirate-instruction-mechanics"])
{
    VerifyNinjaSpacePirateInstructionProgramDefinitions();
    return 0;
}
if (args is ["--cacatac-projectile-instruction-mechanics"])
{
    VerifyCacatacProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--falling-spark-instruction-mechanics"])
{
    VerifyFallingSparkInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fune-namihe-fireball-instruction-mechanics"])
{
    VerifyFuneNamiheFireballInstructionProgramDefinitions();
    return 0;
}
if (args is ["--magdollite-lava-instruction-mechanics"])
{
    VerifyMagdolliteLavaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dragon-fireball-instruction-mechanics"])
{
    VerifyDragonFireballInstructionProgramDefinitions();
    return 0;
}
if (args is ["--eye-door-projectile-instruction-mechanics"])
{
    VerifyEyeDoorProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--eye-door-sweat-instruction-mechanics"])
{
    VerifyEyeDoorSweatInstructionProgramDefinitions();
    return 0;
}
if (args is ["--skree-metaree-particle-instruction-mechanics"])
{
    VerifySkreeMetareeParticleInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-rock-projectile-instruction-mechanics"])
{
    VerifyKraidRockProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fake-kraid-projectile-instruction-mechanics"])
{
    VerifyFakeKraidProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--alcoon-fireball-instruction-mechanics"])
{
    VerifyAlcoonFireballInstructionProgramDefinitions();
    return 0;
}
if (args is ["--work-robot-laser-instruction-mechanics"])
{
    VerifyWorkRobotLaserInstructionProgramDefinitions();
    return 0;
}
if (args is ["--powamp-spike-instruction-mechanics"])
{
    VerifyPowampSpikeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--polyp-rock-instruction-mechanics"])
{
    VerifyPolypRockInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kihunter-acid-spit-instruction-mechanics"])
{
    VerifyKiHunterAcidSpitInstructionProgramDefinitions();
    return 0;
}
if (args is ["--stoke-projectile-instruction-mechanics"])
{
    VerifyStokeProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--nuclear-waffle-projectile-instruction-mechanics"])
{
    VerifyNuclearWaffleProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kago-bug-projectile-instruction-mechanics"])
{
    VerifyKagoBugProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--yapping-maw-body-projectile-instruction-mechanics"])
{
    VerifyYappingMawBodyProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--crocomire-projectile-instruction-mechanics"])
{
    VerifyCrocomireProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--phantoon-projectile-instruction-mechanics"])
{
    VerifyPhantoonProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--draygon-projectile-instruction-mechanics"])
{
    VerifyDraygonProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-debris-instruction-mechanics"])
{
    VerifyCeresFallingDebrisInstructionProgramDefinitions();
    return 0;
}
if (args is ["--save-station-electricity-instruction-mechanics"])
{
    VerifySaveStationElectricityInstructionProgramDefinitions();
    return 0;
}
if (args is ["--downward-gate-projectile-instruction-mechanics"])
{
    VerifyDownwardGateProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--noob-tube-projectile-instruction-mechanics"])
{
    VerifyNoobTubeProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mother-brain-top-tube-instruction-mechanics"])
{
    VerifyMotherBrainTopTubeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mother-brain-glass-instruction-mechanics"])
{
    VerifyMotherBrainGlassInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mother-brain-turret-instruction-mechanics"])
{
    VerifyMotherBrainTurretInstructionProgramDefinitions();
    return 0;
}
if (args is ["--gunship-dust-instruction-mechanics"])
{
    VerifyGunshipDustInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-ridley-projectile-instruction-mechanics"])
{
    VerifyCeresRidleyProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--space-pirate-projectile-instruction-mechanics"])
{
    VerifySpacePirateProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--eye-door-plms"])
{
    VerifyEyeDoorPlms();
    return 0;
}
if (args is ["--cacatac-instruction-mechanics"])
{
    VerifyCacatacInstructionProgramDefinitions();
    return 0;
}
if (args is ["--magdollite-instruction-mechanics"])
{
    VerifyMagdolliteInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kihunter-instruction-mechanics"])
{
    VerifyKiHunterInstructionProgramDefinitions();
    return 0;
}
if (args is ["--owtch-instruction-mechanics"])
{
    VerifyOwtchInstructionProgramDefinitions();
    return 0;
}
if (args is ["--stoke-instruction-mechanics"])
{
    VerifyStokeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ripper-instruction-mechanics"])
{
    VerifyRipperInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kzan-instruction-mechanics"])
{
    VerifyKzanInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fly-instruction-mechanics"])
{
    VerifyFlyInstructionProgramDefinitions();
    return 0;
}
if (args is ["--bull-instruction-mechanics"])
{
    VerifyBullInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kago-instruction-mechanics"])
{
    VerifyKagoInstructionProgramDefinitions();
    return 0;
}
if (args is ["--horizontal-shutter-instruction-mechanics"])
{
    VerifyHorizontalShutterInstructionProgramDefinitions();
    return 0;
}
if (args is ["--growing-shutter-instruction-mechanics"])
{
    VerifyGrowingShutterInstructionProgramDefinitions();
    return 0;
}
if (args is ["--vertical-shutter-instruction-mechanics"])
{
    VerifyVerticalShutterInstructionProgramDefinitions();
    return 0;
}
if (args is ["--choot-instruction-mechanics"])
{
    VerifyChootInstructionProgramDefinitions();
    return 0;
}
if (args is ["--norfair-lava-jumper-instruction-mechanics"])
{
    VerifyNorfairLavaJumperInstructionProgramDefinitions();
    return 0;
}
if (args is ["--beetom-instruction-mechanics"])
{
    VerifyBeetomInstructionProgramDefinitions();
    return 0;
}
if (args is ["--alcoon-instruction-mechanics"])
{
    VerifyAlcoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--multiviola-instruction-mechanics"])
{
    VerifyMultiviolaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--polyp-instruction-mechanics"])
{
    VerifyPolypInstructionProgramDefinitions();
    return 0;
}
if (args is ["--powamp-instruction-mechanics"])
{
    VerifyPowampInstructionProgramDefinitions();
    return 0;
}
if (args is ["--wrecked-ship-ghost-instruction-mechanics"])
{
    VerifyWreckedShipGhostInstructionProgramDefinitions();
    return 0;
}
if (args is ["--puyo-instruction-mechanics"])
{
    VerifyPuyoInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dead-torizo-instruction-mechanics"])
{
    VerifyDeadTorizoInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dead-sidehopper-instruction-mechanics"])
{
    VerifyDeadSidehopperInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dead-tourian-corpse-instruction-mechanics"])
{
    VerifyDeadTourianCorpseInstructionProgramDefinitions();
    return 0;
}
if (args is ["--shitroid-instruction-mechanics"])
{
    VerifyShitroidInstructionProgramDefinitions();
    return 0;
}
if (args is ["--rio-instruction-mechanics"])
{
    VerifyRioInstructionProgramDefinitions();
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
if (args is ["--skree-metaree-instruction-mechanics"])
{
    VerifySkreeMetareeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--zoa-instruction-mechanics"])
{
    VerifyZoaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dragon-instruction-mechanics"])
{
    VerifyDragonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--brinstar-pipe-bug-instruction-mechanics"])
{
    VerifyBrinstarPipeBugInstructionProgramDefinitions();
    return 0;
}
if (args is ["--norfair-pipe-bug-instruction-mechanics"])
{
    VerifyNorfairPipeBugInstructionProgramDefinitions();
    return 0;
}
if (args is ["--yellow-pipe-bug-instruction-mechanics"])
{
    VerifyYellowPipeBugInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-steam-instruction-mechanics"])
{
    VerifyCeresSteamInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-door-instruction-mechanics"])
{
    VerifyCeresDoorInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fake-kraid-instruction-mechanics"])
{
    VerifyFakeKraidInstructionProgramDefinitions();
    return 0;
}
if (args is ["--chozo-statue-instruction-mechanics"])
{
    VerifyChozoStatueInstructionProgramDefinitions();
    return 0;
}
if (args is ["--crocomire-tongue-instruction-mechanics"])
{
    VerifyCrocomireTongueInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mother-brain-baby-instruction-mechanics"])
{
    VerifyMotherBrainBabyInstructionProgramDefinitions();
    return 0;
}
if (args is ["--botwoon-instruction-mechanics"])
{
    VerifyBotwoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-lint-instruction-mechanics"])
{
    VerifyKraidLintInstructionProgramDefinitions();
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
if (args is ["--shaktool-instruction-mechanics"])
{
    VerifyShaktoolInstructionProgramDefinitions();
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
if (args is ["--speed-booster-block-plms"])
{
    VerifySpeedBoosterCollisionBlocks();
    return 0;
}
if (args is ["--maridia-elevatube-plm"])
{
    VerifyMaridiaElevatubePlm();
    return 0;
}
if (args is ["--tourian-access-plms"])
{
    VerifyCompiledTourianAccessPlmPrograms();
    return 0;
}
if (args is ["--tourian-access-visuals"])
{
    VerifyTourianAccessVisuals();
    return 0;
}
if (args is ["--speed-booster-visuals"])
{
    VerifySpeedBoosterVisuals();
    return 0;
}
if (args is ["--maridia-elevatube-visuals"])
{
    VerifyMaridiaElevatubeVisuals();
    return 0;
}
if (args is ["--spore-spawn-ceiling-plms"])
{
    VerifyCompiledSporeSpawnCeilingPlms();
    return 0;
}
if (args is ["--spore-spawn-ceiling-visuals"])
{
    VerifySporeSpawnCeilingVisuals();
    return 0;
}
if (args is ["--botwoon-wall-plms"])
{
    VerifyCompiledBotwoonWallPlms();
    return 0;
}
if (args is ["--botwoon-wall-visuals"])
{
    VerifyBotwoonWallVisuals();
    return 0;
}
if (args is ["--kraid-room-plms"])
{
    VerifyCompiledKraidRoomPlms();
    return 0;
}
if (args is ["--crocomire-arena-plms"])
{
    VerifyCompiledCrocomireArenaPlms();
    return 0;
}
if (args is ["--mother-brain-fake-death-plms"])
{
    VerifyCompiledMotherBrainFakeDeathPlms();
    return 0;
}
if (args is ["--mother-brain-fake-death-visuals"])
{
    VerifyMotherBrainFakeDeathVisuals();
    return 0;
}
if (args is ["--crocomire-arena-visuals"])
{
    VerifyCrocomireArenaVisuals();
    return 0;
}
if (args is ["--kraid-room-visuals"])
{
    VerifyKraidRoomVisuals();
    return 0;
}
if (args is ["--door-closing-definitions"])
{
    VerifyDoorClosingPlmDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--blue-door-plm-draws"])
{
    VerifyBlueDoorPlmDrawDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--colored-door-plm-draws"])
{
    VerifyColoredDoorPlmDrawDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grey-door-plm-draws"])
{
    VerifyGreyDoorPlmDrawDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--eye-door-plm-draws"])
{
    VerifyEyeDoorPlmDrawDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--mother-brain-glass-plm-draws"])
{
    VerifyMotherBrainGlassPlmDrawDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--draygon-cannon-plm-program"])
{
    VerifyDraygonCannonPlmProgram();
    return 0;
}
if (args is ["--bomb-torizo-hand-plm-program"])
{
    VerifyBombTorizoHandPlm();
    return 0;
}
if (args is ["--noob-tube-plm-draws"])
{
    VerifyNoobTubePlmDrawDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--noob-tube-plm-program"])
{
    VerifyNoobTubePlm();
    return 0;
}
if (args is ["--shot-block-plm-programs"])
{
    VerifyShotBlockPlmPrograms();
    return 0;
}
if (args is ["--grapple-block-programs"])
{
    VerifyGrappleBlockPrograms();
    return 0;
}
if (args is ["--bomb-block-programs"])
{
    VerifyBombBlockPrograms();
    return 0;
}
if (args is ["--contact-crumble-programs"])
{
    VerifyContactCrumblePrograms();
    return 0;
}
if (args is ["--station-animation-programs"])
{
    VerifyStationAnimationProgramDefinitions();
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
if (args is ["--escape-etecoon-instruction-mechanics"])
{
    VerifyEscapeEtecoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--escape-dachora-instruction-mechanics"])
{
    VerifyEscapeDachoraInstructionProgramDefinitions();
    return 0;
}
if (args is ["--crocomire-instruction-mechanics"])
{
    VerifyCrocomireInstructionProgramDefinitions();
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
if (args is ["--room-plm-populations"])
{
    VerifyCompiledRoomPlmPopulationDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
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
if (args is ["--etecoon-instruction-program-definitions"])
{
    VerifyEtecoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--elevator-instruction-program-definitions"])
{
    VerifyElevatorInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mochtroid-instruction-program-definitions"])
{
    VerifyMochtroidInstructionProgramDefinitions();
    return 0;
}
if (args is ["--platform-instruction-program-definitions"])
{
    VerifyPlatformInstructionProgramDefinitions();
    return 0;
}
if (args is ["--hopper-instruction-program-definitions"])
{
    VerifyHopperAnimationDefinitions(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--hzoomer-instruction-program-definitions"])
{
    VerifyHZoomerInstructionProgramDefinitions();
    return 0;
}
if (args is ["--sciser-instruction-program-definitions"])
{
    VerifySciserInstructionProgramDefinitions();
    return 0;
}
if (args is ["--zero-instruction-program-definitions"])
{
    VerifyZeroInstructionProgramDefinitions();
    return 0;
}
if (args is ["--viola-instruction-program-definitions"])
{
    VerifyViolaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--shared-crawler-instruction-program-definitions"])
{
    VerifySharedCrawlerInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dachora-instruction-program-definitions"])
{
    VerifyDachoraInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fireflea-instruction-program-definitions"])
{
    VerifyFirefleaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--zebetite-instruction-program-definitions"])
{
    VerifyZebetiteInstructionProgramDefinitions();
    return 0;
}
if (args is ["--evir-instruction-program-definitions"])
{
    VerifyEvirInstructionProgramDefinitions();
    return 0;
}
if (args is ["--morph-ball-eye-instruction-program-definitions"])
{
    VerifyMorphBallEyeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--yapping-maw-instruction-program-definitions"])
{
    VerifyYappingMawInstructionProgramDefinitions();
    return 0;
}
if (args is ["--metroid-instruction-program-definitions"])
{
    VerifyMetroidInstructionProgramDefinitions();
    return 0;
}
if (args is ["--norfair-rio-instruction-program-definitions"])
{
    VerifyNorfairRioInstructionProgramDefinitions();
    return 0;
}
if (args is ["--lower-norfair-rio-instruction-program-definitions"])
{
    VerifyLowerNorfairRioInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mama-turtle-instruction-program-definitions"])
{
    VerifyMamaTurtleInstructionProgramDefinitions();
    return 0;
}
if (args is ["--tourian-entrance-statue-instruction-program-definitions"])
{
    VerifyTourianEntranceStatueInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-nail-instruction-program-definitions"])
{
    VerifyKraidNailInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-arm-instruction-program-definitions"])
{
    VerifyKraidArmInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-foot-instruction-program-definitions"])
{
    VerifyKraidFootInstructionProgramDefinitions();
    return 0;
}
if (args is ["--phantoon-instruction-program-definitions"])
{
    VerifyPhantoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--work-robot-instruction-program-definitions"])
{
    VerifyWorkRobotInstructionProgramDefinitions();
    return 0;
}
if (args is ["--yard-instruction-program-definitions"])
{
    VerifyYardInstructionProgramDefinitions();
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
if (args is ["--room-tileset-definitions"])
{
    VerifyRoomAssetRomData();
    return 0;
}
if (args is ["--room-character-atlases"])
{
    VerifyRoomCharacterAtlases();
    return 0;
}
if (args is ["--enemy-tile-artwork"])
{
    VerifyEnemyTileArtwork();
    VerifyRoomEnemyLoading();
    return 0;
}
if (args is ["--enemy-visual-selector-inventory"])
{
    InspectEnemyVisualSelectors();
    return 0;
}
if (args is ["--generate-enemy-visual-selectors"])
{
    InspectEnemyVisualSelectors(generateCatalog: true);
    return 0;
}
if (args is ["--verify-enemy-visual-selectors"])
{
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--generate-space-pirate-collision"])
{
    GenerateSpacePirateCollisionDefinitions();
    return 0;
}
if (args is ["--generate-room-level-stream-corpus"])
{
    GenerateRoomLevelStreamCorpus();
    return 0;
}
if (args is ["--verify-space-pirate-collision"])
{
    VerifySpacePirateCollisionDefinitions();
    return 0;
}
if (args is ["--projectile-frame-bindings"])
{
    VerifyProjectileFrameBindings(
        SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--intro-cinematic-artwork", var introBackgroundRom])
{
    VerifyIntroCinematicArtwork(introBackgroundRom);
    return 0;
}
if (args is ["--room-character-installation", var roomCharacterRom])
{
    VerifyRoomArtworkInstallation(roomCharacterRom);
    return 0;
}
if (args is ["--room-artwork-installation", var roomArtworkRom])
{
    VerifyRoomArtworkInstallation(roomArtworkRom);
    return 0;
}
if (args is ["--room-static-palettes"])
{
    VerifyRoomStaticPaletteExtraction();
    return 0;
}
if (args is ["--room-metatiles"])
{
    VerifyRoomMetatileExtraction();
    return 0;
}
if (args is ["--library-background-inventory"])
{
    VerifyLibraryBackgroundSourceInventory();
    return 0;
}
if (args is ["--room-background-tilemaps"])
{
    VerifyRoomBackgroundTilemapExtraction();
    return 0;
}
if (args is ["--room-sky-tilemaps"])
{
    VerifyScrollingSkyState();
    VerifyRoomSkyTilemaps();
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
if (args is ["--enemy-definitions"])
{
    VerifyCompiledEnemyDefinitions();
    return 0;
}
if (args is ["--enemy-room-lists"])
{
    VerifyCompiledEnemyRoomLists();
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
if (args is ["--title-instruction-definitions"])
{
    VerifyTitleGradientTables(SuperMetroidAddressSpace.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc")));
    VerifyTitleSequenceRomData();
    return 0;
}
if (args.Length > 1 || (args.Length == 1 && args[0] != "--render-contract"))
    throw new ArgumentException("Usage: SuperMetroid.Verification [--render-contract | --title-instruction-definitions | --phantoon-position | --samus-grapple | --samus-xray | --grapple-doors | --grapple-sounds | --grapple-spin | --grapple-gates | --grapple-enemy-death | --shutter-riding | --shutter-embedding]");
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
VerifyBombBlockPrograms();
VerifyContactCrumblePrograms();
VerifyStationAnimationProgramDefinitions();
VerifyPermanentCollectibles();
VerifyCollectibleVisuals();
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
VerifyShotBlockPlmPrograms();
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
VerifyMaridiaElevatubePlm();
VerifyCompiledTourianAccessPlmPrograms();
VerifyTourianAccessVisuals();
VerifySpeedBoosterVisuals();
VerifyMaridiaElevatubeVisuals();
VerifyCompiledSporeSpawnCeilingPlms();
VerifyCompiledBotwoonWallPlms();
VerifyBotwoonWallVisuals();
VerifyCompiledKraidRoomPlms();
VerifyCompiledCrocomireArenaPlms();
VerifyCompiledMotherBrainFakeDeathPlms();
VerifyMotherBrainFakeDeathVisuals();
VerifyCrocomireArenaVisuals();
VerifyKraidRoomVisuals();
VerifySporeSpawnCeilingVisuals();
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
VerifyMotherBrainEscapeGateCompiledDefinitions(
    SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyEscapeGateVisuals(
    SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyCompiledRoomPlmPopulationDefinitions(
    SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
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
VerifyCompiledEnemyDefinitions();
VerifyCompiledEnemyRoomLists();
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
VerifyIntroCinematicArtwork(Path.GetFullPath("Super Metroid.smc"));
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
VerifyPowerBombFixedColors();
VerifySamusVisorColors();
VerifySamusHurtColors();
VerifySamusHyperBeamColors();
VerifySpcSoundLibrary2Pointers();
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
VerifyEnemyTileArtwork();
VerifyCompiledEnemyVisualSelectors();
VerifySpacePirateCollisionDefinitions();
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
