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
private static int Main()
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
Console.WriteLine("Verifying translated Super Metroid routines...");

VerifyRandomNumberGeneratorExhaustively();
VerifyKnownRandomSequence();
VerifyTimedHeldInputTimeline();
VerifyEventBitfield();
VerifyBossBitfield();
VerifyMultiplicationExhaustively();
VerifySmCompressionFormat();
VerifyVramWriteQueue();
VerifyEscapeTimerBcd();
VerifyEscapeTimerStateMachine();
VerifyControllerInputLatch();
VerifyGameOptionsRomDataCatalog();
VerifyControllerBindingsAndOptionsSubmenus();
VerifyGameOverRomData();
VerifyTitleSequenceRomData();
VerifyStrictFailureBoundaries();
VerifyReserveAutoRecovery();
VerifyDoorOpeningTrajectories();
VerifyCreditsObjectInterpreter();
VerifyEndingCreditsState();
VerifyGenericGamepadInput();
VerifyDemoInputObject();
VerifyAttractDemoScene();
VerifyAttractDemoControllerOverride();
VerifyFrameRuntime();
VerifyGameTimeState();
VerifySuperMetroidAddressSpace();
VerifyCartridgeAudioQueues();
VerifyManagedSnesDsp();
VerifyTypedNativeWords();
VerifyProductionMagicNumberAudit();
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
VerifySamusXray();
VerifySamusDeathSequence();
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
VerifyCrampedAerialLandingPoseCollision();
VerifySamusSpaceJumpAndScrewAttack();
VerifySamusLiquidPhysics();
VerifySamusAtmosphericEffects();
VerifySamusAerialTurnsAndWallJump();
VerifySamusKnockbackAndDamageBoost();
VerifySamusGrappleSwingAndRelease();
VerifyBreakableGrapplePlms();
VerifyPermanentCollectibles();
VerifyEnemyDrops();
VerifySamusPostureMovement();
VerifySamusPowerBeamProjectiles();
VerifySamusMorphBallMovement();
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
VerifyMovedSamusCameraTracking();
VerifyBackgroundScrollState();
VerifyLevelBlockTilemapExpansion();
VerifyRoomLevelData();
VerifyCartridgeRoomStateSelection();
VerifyRoomMainCodeCatalog();
VerifyRoomSetupCodeCatalog();
VerifyRoomAssetRomData();
VerifyAreaMapAssets();
VerifyBackgroundTilemapStreamer();
VerifyFourBitBackgroundRendering();
VerifyLoRomCrossBankCompressedData();
VerifyMode7Rendering();
VerifyLayerCompositorBackdrop();
VerifyBgPriorityPlaneRendering();
VerifyLibraryBackgroundLoader();
VerifyGameConfigurationIni();
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
VerifyPowerBombRuntimeRendererIntegration();
VerifyRoomFxRomData();
VerifyScrollingSkyState();
VerifyEnemyAiCodePointerCatalog();
VerifyEnemyInstructionCodePointerCatalogs();
VerifyEnemyRomTablePointerCatalog();
VerifyPaletteFxInstructionCodeCatalogs();
VerifyAnimatedTileInstructionCodeCatalog();
VerifyEnemyProjectileCodePointerCatalog();
VerifyRoomEnemyLoading();
VerifyEnemyProjectileCollisionLifecycle();
VerifyRipperEnemy();
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
