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
if (args is ["--hero-shots"])
{
    VerifyHeroShotCameraLifetime();
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
if (args is ["--pause-reserve-tanks"])
{
    VerifyPauseReserveTanks();
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
VerifyElevatubeScrolling();
VerifyIniEditing();
VerifyBackgroundSampler();
if (args is ["--ini-edit"]) return 0;
if (args is ["--mother-brain"])
{
    VerifyMotherBrainBeamWindow();
    VerifyMotherBrainDeathHandoff();
    VerifySamusDrainedController();
    VerifyMotherBrainRainbowBeamSamusMovement();
    VerifyMotherBrainRainbowBeamAttackSequence();
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
    throw new ArgumentException("Usage: SuperMetroid.Verification [--render-contract | --phantoon-position | --grapple-doors | --grapple-sounds | --grapple-spin | --grapple-gates | --grapple-enemy-death | --shutter-riding | --shutter-embedding]");
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
VerifyMotherBrainBombProjectiles();
VerifyMotherBrainProjectileRendering();
VerifyMiscDustProjectiles();
VerifyMotherBrainEscapeDoorParticles();
VerifyBabyMetroidCutsceneEntrance();
VerifySamusSolidEnemyCollision();
VerifySamusAerialMovement();
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
VerifyGameSaveJsonPersistence();
VerifyFileSelectFreshSaveTilemap();
VerifyFileSelectMapWindow();
VerifySavedGameLoadAppearance();
VerifyIntroCinematicRomData();
VerifyIntroGameplayFlashbackVerticalScroll();
VerifyCinematicPaletteFader();
VerifyHostRoomViewportAlignment();
VerifyPowerBombColorMathWindow();
VerifyHardwareWindows();
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
VerifyPaletteFxInstructionCodeCatalogs();
VerifyAnimatedTileInstructionCodeCatalog();
VerifyEnemyProjectileCodePointerCatalog();
VerifyRoomEnemyLoading();
VerifyBotwoonPlmIdentity();
VerifyCompiledEnemyTrigonometry();
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
