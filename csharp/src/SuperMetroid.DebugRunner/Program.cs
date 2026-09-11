using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;
using SuperMetroid.SourceAudit;

// This runner is an intentionally thin debugger host, not a claim that the full game has
// already been ported. Its job is to exercise every translated frame boundary against the
// user's private ROM while presenting stable, obvious breakpoint locations.
// Keep faults in the CLI. Without this process policy Windows can display a modal "unknown
// software exception" dialog for an unhandled debugger assertion, steal desktop focus, and
// leave the build output locked until somebody dismisses it.
if (OperatingSystem.IsWindows())
    NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

try
{
if (args.Length == 2 && args[0] == "--diagonal-spark-input-audit")
    return DiagonalSparkInputAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--diagonal-spark-native")
    return DiagonalSparkInputAudit.CompareNative(args[1], args[2]);
if (args.Length == 3 && args[0] == "--suit-acquisition-audit")
    return SuitAcquisitionAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--suit-pickup-audio-audit")
    return SuitPickupAudioAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--magdollite-runtime-audit")
    return MagdolliteRuntimeAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--shine-charge-audit")
    return ShineChargeAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spark-aerial-window-audit")
    return SparkAerialAudit.Run(args[1], args[2], window: true);
if (args.Length == 3 && args[0] == "--spark-ground-restrictions-audit")
    return SparkWindowAudit.Run(args[1], args[2], groundRestrictions: true);
if (args.Length == 2 && args[0] == "--spark-invincibility-audit")
    return SparkInvincibilityAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--spark-surface-audit")
    return SparkSurfaceAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spark-sand-entry-audit")
    return SparkSandAudit.Run(args[1], args[2], entry: true);
if (args.Length == 3 && args[0] == "--spark-sand-audit")
    return SparkSandAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spark-corrosive-audit")
    return SparkCorrosiveAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spark-water-audit")
    return SparkEnergyAudit.Run(args[1], args[2], water: true);
if (args.Length == 3 && args[0] == "--spark-energy-audit")
    return SparkEnergyAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spark-restrictions-audit")
    return SparkAerialAudit.Run(args[1], args[2], restrictions: true);
if (args.Length == 3 && args[0] == "--spark-aerial-audit")
    return SparkAerialAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spark-tap-window-audit")
    return SparkWindowAudit.Run(args[1], args[2], tap: true);
if (args.Length == 3 && args[0] == "--spark-window-audit")
    return SparkWindowAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--shine-storage-audit")
    return ShineStorageAudit.Run(args[1], args[2]);
if (args.Length > 0 && args[0] == "assets")
    return AssetCommands.Run(args[1..]);
if (args.Length == 2 && args[0] == "--xray-stored-shine-audit")
    return XrayStoredShineAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--spark-runtime-sequence-audit")
    return SparkSequenceAudit.Run(args[1], args[2], runtimeFrames: true);
if (args.Length == 3 && args[0] == "--spark-sequence-audit")
    return SparkSequenceAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spark-reset-audit")
    return SparkOwnershipAudit.Run(args[1], args[2], reset: true);
if (args.Length == 3 && args[0] == "--spark-reentry-audit")
    return SparkOwnershipAudit.Run(args[1], args[2], reentry: true);
if (args.Length == 3 && args[0] == "--spark-ownership-audit")
    return SparkOwnershipAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spark-departure-audit")
    return SparkDepartureAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--spark-capacity-input-audit")
    return SparkCapacityInputAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--spark-retained-audit")
    return SparkRetainedAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--plasma-repeat-audit")
    return PlasmaRepeatAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spazer-contact-audit")
    return ComboContactAudit.Run(args[1], args[2], spazerAges: true);
if (args.Length == 3 && args[0] == "--wave-patterns-audit")
    return ComboMotionAudit.Run(args[1], args[2], SamusBeamFlags.Wave, wavePatterns: true);
if (args.Length == 2 && args[0] == "--phantoon-plasma-audit")
    return PhantoonPlasmaAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--phantoon-plasma-release")
    return PhantoonPlasmaAudit.RunRelease(args[1]);
if (args.Length == 2 && args[0] == "--draygon-plasma-release")
    return DraygonPlasmaAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--native-phantoon-plasma-audit")
    return PhantoonPlasmaAudit.RunNative(args[1], args[2]);
if (args.Length == 3 && args[0] == "--wave-phantoon-audit")
    return WavePhantoonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--ice-thaw-audit")
    return IceThawAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--ice-contact-audit")
    return IceContactAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--frozen-ai-audit")
    return FrozenAiAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--combo-input-sequence-audit")
    return ComboInputSequenceAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--combo-grapple-audit")
    return ComboGrappleAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--bomb-admission-audit")
    return BombAdmissionAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--missile-admission-audit")
    return MissileAdmissionAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--combo-draw-audit")
    return ComboDrawAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--combo-contact-audit")
    return ComboContactAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spazer-combo-motion-audit")
    return ComboMotionAudit.Run(args[1], args[2], SamusBeamFlags.Spazer);
if (args.Length == 3 && args[0] == "--ice-combo-motion-audit")
    return ComboMotionAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--wave-combo-motion-audit")
    return ComboMotionAudit.Run(args[1], args[2], SamusBeamFlags.Wave);
if (args.Length == 3 && args[0] == "--plasma-combo-motion-audit")
    return ComboMotionAudit.Run(args[1], args[2], SamusBeamFlags.Plasma);
if (args.Length == 3 && args[0] == "--combo-allocation-audit")
    return ComboAllocationAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--combo-activation-input-audit")
    return ComboActivationInputAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--pseudo-screw-extended-reset-audit")
    return PseudoScrewResetAudit.Run(args[1], args[2], extended: true);
if (args.Length == 3 && args[0] == "--pseudo-screw-reset-audit")
    return PseudoScrewResetAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--pseudo-screw-liquid-audit")
    return PseudoScrewLiquidAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--elevator-grab-timeline-audit")
    return ElevatorGrabTimelineAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--elevator-grab-comparison-audit")
    return ElevatorGrabComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--yard-native-comparison-audit")
    return YardNativeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--pause-charge-comparison-audit")
    return PauseChargeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--pause-fade-comparison-audit")
    return PauseFadeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--soft-unmorph-charge-comparison-audit")
    return SoftUnmorphChargeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--continuous-walljump-comparison-audit")
    return ContinuousWalljumpComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--xray-charge-comparison-audit")
    return XrayChargeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--charge-equipment-comparison-audit")
    return ChargeEquipmentComparisonAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--pause-charge-carry-audit")
    return PauseChargeCarryAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--charged-walljump-comparison-audit")
    return ChargedWalljumpComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--moonfall-comparison-audit")
    return MoonfallComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--kago-contact-audit")
    return KagoContactAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--kago-solidity-audit")
    return KagoSolidityAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--kago-kzan-audit")
    return KagoKzanAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--kago-kamer-audit")
    return KagoKamerAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--quick-charge-audit")
    return QuickChargeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--short-charge-audit")
    return ShortChargeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--waterball-audit")
    return WaterballComparisonAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--pseudo-screw-contact-audit")
    return PseudoScrewContactAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--pseudo-screw-projectile-audit")
    return PseudoScrewProjectileAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--pseudo-screw-walljump-audit")
    return PseudoScrewWalljumpAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--moving-door-audit")
    return MovingDoorComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--moving-item-audit")
    return MovingItemComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--station-probe-audit")
    return StationProbeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--save-probe-audit")
    return SaveProbeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--sand-probe-audit")
    return SandProbeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--pose-sand-audit")
    return PoseSandComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--wall-sand-audit")
    return WallSandComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--hand-probe-audit")
    return HandProbeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--ceiling-door-audit")
    return CeilingDoorComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--downback-door-audit")
    return DownbackDoorComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--pose-trigger-audit")
    return PoseTriggerComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--pose-crumble-audit")
    return PoseCrumbleComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--remote-item-audit")
    return RemoteItemComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--item-acquisition-audit")
    return ItemAcquisitionComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--remote-door-audit")
    return RemoteDoorComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--corner-jump-audit")
    return CornerJumpComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--downback-audit")
    return DownbackComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--gap-skip-audit")
    return GapSkipComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--ledge-grab-audit")
    return LedgeGrabComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--edge-boost-audit")
    return EdgeBoostComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--crouch-jump-audit")
    return CrouchJumpComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--kago-passage-audit")
    return KagoPassageAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--quick-drop-bomb-audit")
    return QuickDropBombAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--quick-drop-crumble-audit")
    return QuickDropCrumbleAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--quick-drop-timeline-audit")
    return QuickDropTimelineAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--quick-drop-ceiling-audit")
    return QuickDropCeilingAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--stop-on-dime-comparison-audit")
    return StopOnDimeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--arm-pump-comparison-audit")
    return ArmPumpComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--moonwalk-comparison-audit")
    return MoonwalkComparisonAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--moonwalk-options-audit")
    return MoonwalkOptionsAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--options-heading-audit")
    return OptionsHeadingAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--intro-text-capture-audit")
    return IntroTextCaptureAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--xray-first-use-audit")
    return XrayFirstUseAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--grapple-jump-aim-audit")
    return GrappleJumpAimAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--crocomire-attack-entry-audit")
    return CrocomireAudit.RunNaturalAttackEntry(args[1], args[2]);
if (args.Length == 2 && args[0] == "--crocomire-power-bomb-audit")
    return CrocomireAudit.RunPowerBombTrajectory(args[1]);
if (args.Length == 2 && args[0] == "--file-select-native-audit")
    return NativeAudioCorpusAudit.Run("standalone-assets/audio", args[1], "Super Metroid.smc", fileSelectOnly: true);
if (args.Length == 4 && args[0] == "--recorded-native-audio-audit")
    return NativeAudioCorpusAudit.Run("standalone-assets/audio", args[3], args[1], args[2]);
if (args.Length == 4 && args[0] == "--recorded-native-audio-survey")
    return NativeAudioCorpusAudit.Run("standalone-assets/audio", args[3], args[1], args[2], survey: true);
if (args.Length == 5 && args[0] == "--capture-recorded-pause-audio")
    return NativeAudioCorpusAudit.Run("standalone-assets/audio", args[3], args[1], args[2], survey: true, captureDirectory: args[4]);
if (args.Length == 3 && args[0] == "--quicksand-comparison-audit")
    return QuicksandComparisonAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--missile-reuse-audit")
    return MissileReuseAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--pause-map-centering-audit")
    return PauseMapCenteringAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--pause-map-scrolling-audit")
    return PauseMapScrollingAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--minimap-blink-audit")
    return MinimapBlinkAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--gunship-recharge-audit")
    return GunshipRechargeAudit.Run(args[1]);

if (args.Length == 2 && args[0] == "--gunship-save-audit")
    return GunshipSaveAudit.Run(args[1]);

if (args.Length == 2 && args[0] == "--forward-facing-projectile-audit")
    return ForwardFacingProjectileAudit.Run(args[1]);

if (args.Length == 2 && args[0] == "--crocomire-bg2-audit")
    return CrocomireBg2Audit.Run(args[1]);

if (args.Length == 2 && args[0] == "--grapple-hud-audit")
    return GrappleHudAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--grapple-demo-trace")
    return GrappleDemoTrace.Run(args[1]);

if (args.Length == 2 && args[0] == "--saved-room-music-audit")
    return SavedRoomMusicAudit.Run(args[1]);

if (args.Length == 2 && args[0] == "--wall-jump-spin-audit")
    return WallJumpSpinAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--deleted-squeept-parent-audit")
    return NorfairLavaJumpingEnemyAudit.Run(args[1], deletedParentOnly: true);
if (args.Length == 2 && args[0] == "--sand-physics-audit")
    return SandRoomAudit.Run(args[1], verifyPhysics: true);
if (args.Length == 2 && args[0] == "--sand-room-audit")
    return SandRoomAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--native-audio-corpus-audit")
    return NativeAudioCorpusAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--dsp-source-transition-audit")
    return DspSourceTransitionAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--health-warning-native-audio-audit")
    return NativeAudioCorpusAudit.Run(args[1], args[2], healthWarningOnly: true);
if (args.Length == 3 && args[0] == "--item-message-audio-audit")
    return ItemMessageAudioAudit.Run(args[1], args[2]);
if (args.Length == 5 && args[0] is "--sound-native-audio-audit" or "--sound-native-audio-survey")
    return NativeAudioCorpusAudit.Run(args[1], args[2], survey: args[0] == "--sound-native-audio-survey", soundOnly: new SoundEffectId(
        SoundEffectLibraries.FromCartridge(byte.Parse(args[3]), "diagnostic command line"),
        byte.Parse(args[4], System.Globalization.NumberStyles.HexNumber)));
if (args.Length == 4 && args[0] == "--missile-impact-native-audio-audit")
    return NativeAudioCorpusAudit.Run(args[1], args[2], romPath: args[3], missileImpactOnly: true);
if (args.Length == 5 && args[0] == "--ridley-death-native-audio-audit")
    return NativeAudioCorpusAudit.Run(args[1], args[2], romPath: args[3], ridleyTracePath: args[4]);
if (args.Length == 5 && args[0] == "--dsp-sample-comparison-audit")
    return DspSampleComparisonAudit.Run(args[1], Convert.ToInt32(args[2], 16), Convert.ToByte(args[3], 16), args[4]);
if (args.Length == 4 && args[0] == "--spc-tick-comparison-audit")
    return SpcTickComparisonAudit.Run(args[1], Convert.ToInt32(args[2], 16), args[3]);
if (args.Length == 4 && args[0] == "--ridley-spc-tick-comparison-audit")
    return SpcTickComparisonAudit.Run(args[1], Convert.ToInt32(args[2], 16), args[3], expectedTicks: 6000);
if (args.Length == 3 && args[0] == "--spc-cancellation-audit")
    return SpcCancellationAudit.Run(args[1], Convert.ToInt32(args[2], 16));
if (args.Length == 3 && args[0] == "--spin-door-native-comparison")
    return DoorExitMomentumAudit.Run(args[1], SpinDoorFixtureDefinitions.SourceRoom,
        SpinDoorFixtureDefinitions.DestinationRoom, SpinDoorFixtureDefinitions.SourceX,
        SpinDoorFixtureDefinitions.SourceY, spinJump: true, nativeTrace: args[2]);
if (args.Length is 6 or 9 or 10 && args[0] is "--door-exit-momentum-audit" or "--spin-door-exit-momentum-audit")
    return DoorExitMomentumAudit.Run(args[1], Convert.ToUInt16(args[2], 16),
        Convert.ToUInt16(args[3], 16), Convert.ToUInt16(args[4], 16), Convert.ToUInt16(args[5], 16),
        args.Length >= 9 ? Convert.ToUInt16(args[6], 16) : (ushort)0,
        args.Length >= 9 ? Convert.ToUInt16(args[7], 16) : (ushort)0,
        args.Length >= 9 ? Convert.ToUInt16(args[8], 16) : (ushort)0,
        args.Length == 10 ? args[9] : null,
        spinJump: args[0] == "--spin-door-exit-momentum-audit");
if (args.Length == 2 && args[0] == "--short-tap-audit")
    return ShortTapAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--short-tap-comparison-audit")
    return ShortTapComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--spinjump-comparison-audit")
    return SpinjumpComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--walljump-comparison-audit")
    return WallJumpComparisonAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--elevator-spinjump-audit")
    return ElevatorSpinjumpAudit.Run(args[1]);
if (args.Length is 3 or 4 && args[0] == "--elevator-top-edge-audit")
    return ElevatorTopEdgeAudit.Run(args[1], args[2], args.Length == 4 ? args[3] : null);
if (args.Length == 2 && args[0] == "--elevator-charge-audit")
    return ElevatorChargeAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--door-charge-audit")
    return DoorChargeAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--missile-crawler-detach-audit")
    return MissileCrawlerDetachmentAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--crawler-quake-native-audit")
    return CrawlerQuakeNativeAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--yard-quake-native-audit")
    return YardQuakeNativeAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--missile-yard-detach-audit")
    return MissileYardDetachmentAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--spore-glow-audit")
    return SporeSpawnGlowAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--floor-maw-contact-audit")
    return YappingMawAudit.RunFloorContact(args[1]);
if (args.Length == 2 && args[0] == "--samus-eater-audit")
    return SamusEaterAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--samus-eater-audit")
    return SamusEaterAudit.Run(args[1], args[2]);

if (args.Length == 2 && args[0] == "--missile-impact-shake-audit")
    return MissileImpactShakeAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--phantoon-materialization-audit")
    return PhantoonMaterializationAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--phantoon-transparency-audit")
    return PhantoonTransparencyAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--phantoon-fade-comparison-audit")
    return PhantoonFadeComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--phantoon-death-visual-audit")
    return PhantoonDeathVisualAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--phantoon-hit-fades-audit")
    return PhantoonHitFadeAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--phantoon-flame-region-audit")
    return PhantoonFlameRegionAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--phantoon-rain-timing-audit")
    return PhantoonRainTimingAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--phantoon-attack-population-audit")
    return PhantoonAttackPopulationAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--phantoon-flame-coordinate-audit")
    return PhantoonFlameCoordinateAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--phantoon-rage-wave-audit")
    return PhantoonRageWaveAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--phantoon-flame-collision-audit")
    return PhantoonFlameCollisionAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--phantoon-wave-comparison-audit")
    return PhantoonWaveComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--elevator-draw-native-comparison")
    return ElevatorDrawNativeComparison.Run(args[1], args[2]);
if (args.Length is 3 or 4 && args[0] == "--green-elevator-top-edge-audit")
    return ElevatorTopEdgeAudit.Run(args[1], args[2], greenBrinstar: true,
        nativeHandoffCsv: args.Length == 4 ? args[3] : null);
if (args.Length == 2 && args[0] == "--elevator-frontend-handoff-audit")
    return ElevatorFrontendHandoffAudit.Run(args[1]);
if (args.Length == 3 && args[0] == "--elevator-spinjump-audit")
    return ElevatorSpinjumpAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--elevator-spinjump-compare")
    return ElevatorSpinjumpAudit.Compare(args[1]);
if (args.Length == 3 && args[0] == "--elevator-spinjump-preheld-audit")
    return ElevatorSpinjumpAudit.Run(args[1], args[2], directionHeldDuringArrival: true);
if (args.Length == 2 && args[0] == "--elevator-spinjump-preheld-compare")
    return ElevatorSpinjumpAudit.Compare(args[1], preheld: true);
if (args.Length == 3 && args[0] == "--jump-turn-comparison-audit")
    return JumpTurnComparisonAudit.Run(args[1], args[2]);
if (args.Length == 2 && args[0] == "--running-release-audit")
    return RunningReleaseAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--walk-off-momentum-audit")
    return WalkOffMomentumAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--attract-demo-frontend-audit")
    return AttractDemoFrontendAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--attract-demo-data-audit")
    return AttractDemoDataAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--title-console-palette-audit")
    return TitleConsolePaletteAudit.Run(args[1]);
if (args.Length == 2 && args[0] == "--underwater-turn-probe")
    return UnderwaterTurnProbe.Run(args[1]);
if (args.Length is 4 or 5 && args[0] == "--export-replay-sram")
    return ReplaySaveRamExporter.Run(args[1], args[2], args[3], args.Length == 5 ? int.Parse(args[4]) : null);
if (args.Length <= 2 && args.Length >= 1 && args[0] == "--magic-number-audit")
{
    bool printBaseline = args.Length == 2 && args[1] == "--print-baseline";
    string sourceAuditRoot = args.Length == 2 && !printBaseline
        ? Path.GetFullPath(args[1].Trim('"'))
        : ProductionMagicNumberAudit.FindRepositoryRoot();
    MagicNumberAuditResult audit = ProductionMagicNumberAudit.Run(sourceAuditRoot);
    if (printBaseline)
    {
        Console.WriteLine(ProductionMagicNumberAudit.CreateBaselinePayload(audit.CurrentFindings));
        return 0;
    }
    foreach (MagicNumberFinding finding in audit.NewFindings)
        Console.Error.WriteLine(finding.Diagnostic);
    if (!audit.Passed)
    {
        Console.Error.WriteLine(
            $"Magic-number audit failed with {audit.NewFindings.Count} new finding(s); " +
            $"reviewed baseline={audit.BaselineCount}, current debt={audit.CurrentFindingCount}.");
        return 1;
    }

    Console.WriteLine(
        $"Magic-number audit passed: current debt={audit.CurrentFindingCount}, " +
        $"reviewed baseline={audit.BaselineCount}, retired baseline entries={audit.RetiredBaselineEntries}.");
    return 0;
}

if (args.Length == 3 && args[0] == "--damageboost-comparison-audit")
    return DamageBoostComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--hurt-bomb-comparison-audit")
    return HurtBombComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--shot-bomb-comparison-audit")
    return ShotBombComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--live-hurt-bomb-comparison-audit")
    return LiveHurtBombComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--bomb-chain-comparison-audit")
    return BombChainComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--repeated-bomb-chain-comparison-audit")
    return BombChainComparisonAudit.Run(args[1], args[2], BombChainAuditScenario.Repeated);
if (args.Length == 3 && args[0] == "--triple-bomb-chain-comparison-audit")
    return BombChainComparisonAudit.Run(args[1], args[2], BombChainAuditScenario.Triple);
if (args.Length == 3 && args[0] == "--horizontal-bomb-chain-comparison-audit")
    return BombChainComparisonAudit.Run(args[1], args[2], BombChainAuditScenario.Steering);
if (args.Length == 3 && args[0] == "--ladder-bomb-chain-comparison-audit")
    return BombChainComparisonAudit.Run(args[1], args[2], BombChainAuditScenario.Ladder);
if (args.Length == 3 && args[0] == "--ceiling-steering-bomb-chain-comparison-audit")
    return BombChainComparisonAudit.Run(args[1], args[2], BombChainAuditScenario.CeilingSteering);
if (args.Length == 3 && args[0] == "--morph-bounce-comparison-audit")
    return MorphBounceComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--morph-timing-comparison-audit")
    return MorphBounceComparisonAudit.Run(args[1], args[2], MorphBounceAuditScenario.FallingMorphTiming);
if (args.Length == 3 && args[0] == "--run-jump-morph-comparison-audit")
    return MorphBounceComparisonAudit.Run(args[1], args[2], MorphBounceAuditScenario.RunJumpMorph);
if (args.Length == 3 && args[0] == "--crouch-lock-comparison-audit")
    return CrouchLockComparisonAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--temporary-blue-comparison-audit")
    return MorphBounceComparisonAudit.Run(args[1], args[2], MorphBounceAuditScenario.TemporaryBlue);

if (args.Length == 3 && args[0] == "--speedball-family-comparison-audit")
    return SpeedballBlockComparisonAudit.Run(args[1], args[2], families: true);

if (args.Length == 3 && args[0] == "--speedball-comparison-audit")
    return MorphBounceComparisonAudit.Run(args[1], args[2], MorphBounceAuditScenario.Speedball);

if (args.Length == 3 && args[0] == "--mockball-comparison-audit")
    return MorphBounceComparisonAudit.Run(args[1], args[2], MorphBounceAuditScenario.Mockball);
if (args.Length == 3 && args[0] == "--speedball-block-comparison-audit")
    return SpeedballBlockComparisonAudit.Run(args[1], args[2]);

if (args.Length == 3 && args[0] == "--speedboost-animation-audit")
    return SpeedBoostAnimationAudit.Run(args[1], args[2]);

if (args.Length == 3 && args[0] == "--damageboost-hurt-prefix-audit")
    return DamageBoostComparisonAudit.Run(args[1], args[2], hurtPrefixOnly: true);

if (args.Length >= 2 && args[0] == "--door-setup-callback-audit")
{
    string doorSetupRomPath = string.Join(' ', args[1..]).Trim('"');
    return DoorSetupCallbackAudit.Run(doorSetupRomPath);
}

if (args.Length >= 2 && args[0] == "--power-bomb-runtime-audit")
{
    string powerBombRomPath = string.Join(' ', args[1..]).Trim('"');
    return PowerBombRuntimeAudit.Run(powerBombRomPath);
}

if (args.Length >= 2 && args[0] == "--blue-brinstar-door-audit")
{
    string blueDoorRomPath = string.Join(' ', args[1..]).Trim('"');
    return BlueBrinstarDoorAudit.Run(blueDoorRomPath);
}
if (args.Length == 2 && args[0] == "--blue-door-timing-audit")
    return BlueBrinstarDoorAudit.RunTimingSweep(args[1]);
if (args.Length == 2 && args[0] == "--blue-door-contact-audit")
    return BlueBrinstarDoorAudit.RunContactSweep(args[1]);

if (args.Length >= 2 && args[0] == "--spike-hazard-audit")
{
    string spikeHazardRomPath = string.Join(' ', args[1..]).Trim('"');
    return SpikeHazardAudit.Run(spikeHazardRomPath);
}

if (args.Length >= 3 && args[0] == "--input-replay-audit")
{
    return InputReplayAudit.Run(
        args[1].Trim('"'),
        string.Join(' ', args[2..]).Trim('"'));
}
if (args.Length == 3 && args[0] == "--moving-missile-explosion-audit")
    return InputReplayAudit.Run(args[1], args[2], enforceStationaryMissileExplosions: true);

if (args.Length == 5 && args[0] == "--input-replay-trace")
{
    return InputReplayAudit.Run(
        args[1].Trim('"'),
        args[2].Trim('"'),
        traceStartFrame: int.Parse(args[3]),
        traceEndFrame: int.Parse(args[4]));
}
if (args.Length == 6 && args[0] == "--input-replay-capture")
    return InputReplayAudit.Run(args[1], args[2], traceStartFrame: int.Parse(args[3]),
        traceEndFrame: int.Parse(args[4]), frameCaptureDirectory: args[5]);

if (args.Length == 4 && args[0] == "--start-input-visual-audit")
{
    return StartInputVisualAudit.Run(
        args[1].Trim('"'),
        args[2].Trim('"'),
        args[3].Trim('"'));
}

if (args.Length == 3 && args[0] == "--file-select-data-audit")
{
    return FileSelectDataManagementAudit.Run(
        args[1].Trim('"'),
        args[2].Trim('"'));
}

if (args.Length >= 2 && args[0] == "--ceres-controller-route-audit")
{
    string ceresControllerRomPath = string.Join(' ', args[1..]).Trim('"');
    return CeresControllerRouteAudit.Run(ceresControllerRomPath);
}

if (args.Length >= 2 && args[0] == "--ceres-elevator-climb-focus")
{
    string ceresElevatorRomPath = string.Join(' ', args[1..]).Trim('"');
    return CeresControllerRouteAudit.RunElevatorClimbFocus(ceresElevatorRomPath);
}

if (args.Length >= 2 && args[0] == "--ceres-elevator-arrival-audit")
{
    string ceresArrivalRomPath = string.Join(' ', args[1..]).Trim('"');
    return CeresElevatorArrivalAudit.Run(ceresArrivalRomPath);
}

// Configuration regression kept as its own small audit rather than adding another branch
// to the already deep cinematic capture tree below.
if (args.Length >= 3 && args[0] == "--frontend-skip-intro-capture")
{
    string skipIntroRomPath = string.Join(' ', args[1..^1]).Trim('"');
    string skipIntroOutputPath = args[^1].Trim('"');
    return FrontendSkipIntroAudit.Run(skipIntroRomPath, skipIntroOutputPath);
}

if (args.Length == 3 && args[0] == "--ceres-explosion-timing-audit")
    return CeresExplosionTimingAudit.Run(args[1], args[2]);

if (args.Length == 3 && args[0] == "--ceres-zoom-timing-audit")
    return CeresZoomTimingAudit.Run(args[1], args[2]);

if (args.Length == 3 && args[0] == "--ceres-actor-native-audit")
    return CeresActorNativeAudit.Run(args[1], args[2]);

if (args.Length == 3 && args[0] == "--ceres-scene-native-audit")
    return CeresSceneNativeAudit.Run(args[1], args[2]);

if (args.Length == 3 && args[0] == "--zebes-ship-visibility-audit")
    return ZebesShipVisibilityAudit.Run(args[1], args[2]);

if (args.Length == 3 && args[0] == "--intro-hurt-palette-audit")
    return IntroHurtPaletteAudit.Run(args[1], args[2]);

if (args.Length is 3 or 4 && args[0] == "--intro-return-jump-audit")
    return IntroReturnJumpAudit.Run(args[1], args[2], args.Length == 4 ? args[3] : null);

if (args.Length >= 3 && args[0] == "--ceres-destruction-audit")
{
    string destructionRomPath = string.Join(' ', args[1..^1]).Trim('"');
    string destructionOutputDirectory = args[^1].Trim('"');
    return CeresDestructionAudit.Run(destructionRomPath, destructionOutputDirectory);
}

if (args.Length >= 2 && args[0] == "--early-route-audit")
{
    string earlyRouteRomPath = string.Join(' ', args[1..]).Trim('"');
    return EarlyRouteAudit.Run(earlyRouteRomPath);
}

if (args.Length >= 3 && args[0] == "--pause-map-audit")
{
    string pauseMapRomPath = string.Join(' ', args[1..^1]).Trim('"');
    string pauseMapOutputPath = args[^1].Trim('"');
    return PauseMapAudit.Run(pauseMapRomPath, pauseMapOutputPath);
}

if (args.Length >= 2 && args[0] == "--pause-message-gate-audit")
{
    string pauseMessageRomPath = string.Join(' ', args[1..]).Trim('"');
    return PauseMessageGateAudit.Run(pauseMessageRomPath);
}

if (args.Length >= 2 && args[0] == "--map-station-pause-gate-audit")
{
    string mapStationRomPath = string.Join(' ', args[1..]).Trim('"');
    return MapStationPauseGateAudit.Run(mapStationRomPath);
}

if (args.Length >= 3 && args[0] == "--save-station-audit")
{
    string saveStationRomPath = string.Join(' ', args[1..^1]).Trim('"');
    string saveStationOutputDirectory = args[^1].Trim('"');
    return SaveStationAudit.Run(saveStationRomPath, saveStationOutputDirectory);
}

if (args.Length == 2 && args[0] == "--save-station-activation-audit")
    return SaveStationActivationAudit.Run(args[1]);

if (args.Length == 4 && args[0] == "--save-station-recording-audit")
{
    return SaveStationAudit.RunRecording(
        args[1].Trim('"'),
        args[2].Trim('"'),
        args[3].Trim('"'));
}

if (args.Length >= 2 && args[0] == "--early-controller-route-audit")
{
    string controllerRouteRomPath = string.Join(' ', args[1..]).Trim('"');
    return EarlyControllerRouteAudit.Run(controllerRouteRomPath);
}

if (args.Length >= 2 && args[0] == "--door-transition-visual-audit")
{
    string doorTransitionRomPath = string.Join(' ', args[1..]).Trim('"');
    return EarlyControllerRouteAudit.RunDoorTransitionVisualAudit(doorTransitionRomPath);
}

if (args.Length == 4 && args[0] == "--warehouse-save-exit-audit")
{
    return WarehouseSaveExitAudit.Run(
        args[1].Trim('"'),
        args[2].Trim('"'),
        args[3].Trim('"'));
}

if (args.Length >= 2 && args[0] == "--construction-zone-door-ghost-audit")
{
    string doorGhostRomPath = string.Join(' ', args[1..]).Trim('"');
    return EarlyControllerRouteAudit.RunConstructionZoneDoorGhostAudit(doorGhostRomPath);
}

if (args.Length >= 2 && args[0] == "--vertical-room-entry-audit")
{
    string verticalEntryRomPath = args.Length == 2
        ? args[1].Trim('"')
        : string.Join(' ', args[1..^1]).Trim('"');
    string? captureDirectory = args.Length == 2 ? null : args[^1].Trim('"');
    return EarlyControllerRouteAudit.RunVerticalRoomEntryAudit(
        verticalEntryRomPath,
        captureDirectory);
}

if (args.Length >= 2 && args[0] == "--green-brinstar-elevator-audit")
{
    string elevatorRomPath = args[1].Trim('"');
    string? captureDirectory = args.Length >= 3 ? args[2].Trim('"') : null;
    return EarlyControllerRouteAudit.RunGreenBrinstarElevatorAudit(
        elevatorRomPath,
        captureDirectory);
}

if (args.Length >= 2 && args[0] == "--parlor-scroll-pose-probe-audit")
{
    string scrollPoseRomPath = string.Join(' ', args[1..]).Trim('"');
    return EarlyControllerRouteAudit.RunParlorScrollPoseProbeAudit(scrollPoseRomPath);
}

if (args.Length is 2 or 3 && args[0] == "--blue-hopper-audit")
{
    string hopperRomPath = args[1].Trim('"');
    string? hopperCaptureDirectory = args.Length == 3 ? args[2].Trim('"') : null;
    return HopperAudit.Run(hopperRomPath, hopperCaptureDirectory);
}

if (args.Length == 4 && args[0] == "--hopper-recording-audit")
{
    return HopperAudit.RunRecording(
        args[1].Trim('"'),
        args[2].Trim('"'),
        args[3].Trim('"'));
}

if (args.Length >= 2 && args[0] == "--water-fx-audit")
{
    string waterFxRomPath = string.Join(' ', args[1..]).Trim('"');
    return WaterFxAudit.Run(waterFxRomPath);
}

if (args.Length >= 2 && args[0] == "--map-cross-view-audit")
{
    string mapRomPath = string.Join(' ', args[1..]).Trim('"');
    return MapCrossViewAudit.Run(mapRomPath);
}

if (args.Length >= 2 && args[0] == "--crumble-block-audit")
{
    string crumbleRomPath = string.Join(' ', args[1..]).Trim('"');
    return CrumbleBlockAudit.Run(crumbleRomPath);
}

if (args.Length >= 2 && args[0] == "--alpha-power-bomb-boyon-audit")
{
    string boyonRomPath = string.Join(' ', args[1..]).Trim('"');
    return BoyonAudit.Run(boyonRomPath);
}

if (args.Length >= 2 && args[0] == "--stoke-audit")
{
    string stokeRomPath = string.Join(' ', args[1..]).Trim('"');
    return StokeAudit.Run(stokeRomPath);
}

if (args.Length >= 2 && args[0] == "--mama-turtle-audit")
{
    string turtleRomPath = string.Join(' ', args[1..]).Trim('"');
    return MamaTurtleAudit.Run(turtleRomPath);
}

if (args.Length >= 2 && args[0] == "--waterway-puyo-audit")
{
    string puyoRomPath = string.Join(' ', args[1..]).Trim('"');
    return PuyoAudit.Run(puyoRomPath);
}

if (args.Length >= 2 && args[0] == "--noob-bridge-cacatac-audit")
{
    string cacatacRomPath = string.Join(' ', args[1..]).Trim('"');
    return CacatacAudit.Run(cacatacRomPath);
}

if (args.Length >= 2 && args[0] == "--pseudo-plasma-owtch-audit")
{
    string owtchRomPath = string.Join(' ', args[1..]).Trim('"');
    return OwtchAudit.Run(owtchRomPath);
}

if (args.Length >= 2 && args[0] == "--single-chamber-multiviola-audit")
{
    string multiviolaRomPath = string.Join(' ', args[1..]).Trim('"');
    return MultiviolaAudit.Run(multiviolaRomPath);
}

if (args.Length >= 2 && args[0] == "--volcano-enemy-audit")
{
    string volcanoRomPath = string.Join(' ', args[1..]).Trim('"');
    return VolcanoEnemyAudit.Run(volcanoRomPath);
}

if (args.Length >= 2 && args[0] == "--lava-dive-namihe-audit")
{
    string namiheRomPath = string.Join(' ', args[1..]).Trim('"');
    return NamiheAudit.Run(namiheRomPath);
}

if (args.Length >= 2 && args[0] == "--forgotten-highway-kago-audit")
{
    string kagoRomPath = string.Join(' ', args[1..]).Trim('"');
    return KagoAudit.Run(kagoRomPath);
}

if (args.Length >= 2 && args[0] == "--magdollite-tunnel-audit")
{
    string magdolliteRomPath = string.Join(' ', args[1..]).Trim('"');
    return MagdolliteAudit.Run(magdolliteRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-coverage-audit")
{
    string coverageRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyCoverageAudit.Run(coverageRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-collectible-audit")
{
    string collectibleRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailCollectibleAudit.Run(collectibleRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-plm-population-audit")
{
    string plmRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailPlmPopulationAudit.Run(plmRomPath);
}

if (args.Length >= 2 && args[0] == "--out-of-bounds-plm-audit")
{
    string outOfBoundsPlmRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailPlmPopulationAudit.AuditOutOfBoundsSetup(outOfBoundsPlmRomPath);
}

if (args.Length >= 2 && args[0] == "--speed-booster-escape-plm-audit")
{
    string speedBoosterEscapeRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailPlmPopulationAudit.AuditSpeedBoosterEscape(speedBoosterEscapeRomPath);
}

if (args.Length >= 2 && args[0] == "--wrecked-ship-attic-plm-audit")
{
    string wreckedShipAtticRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailPlmPopulationAudit.AuditWreckedShipAttic(wreckedShipAtticRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-scroll-owner-audit")
{
    string scrollOwnerRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailPlmPopulationAudit.LocateScrollOwners(scrollOwnerRomPath, 1172, 1062, 940);
}

if (args.Length >= 2 && args[0] == "--retail-scroll-ownership-audit")
{
    string scrollOwnershipRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailPlmPopulationAudit.AuditScrollOwnership(scrollOwnershipRomPath);
}

if (args.Length >= 2 && args[0] == "--metroids-cleared-plm-audit")
{
    string metroidsClearedRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailPlmPopulationAudit.AuditMetroidsClearedStates(metroidsClearedRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-execution-audit")
{
    string executionRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.Run(executionRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-lifecycle-audit")
{
    string lifecycleRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.RunLifecycle(lifecycleRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-extended-lifecycle-audit")
{
    string extendedLifecycleRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.RunExtendedLifecycle(extendedLifecycleRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-directional-lifecycle-audit")
{
    string directionalLifecycleRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.RunDirectionalLifecycle(directionalLifecycleRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-power-bomb-audit")
{
    string powerBombAuditRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.RunPowerBombCombat(powerBombAuditRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-touch-audit")
{
    string touchAuditRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.RunTouchCombat(touchAuditRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-grapple-audit")
{
    string grappleAuditRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.RunGrappleCombat(grappleAuditRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-projectile-audit")
{
    string projectileAuditRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.RunProjectileCombat(projectileAuditRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-normal-bomb-audit")
{
    string normalBombAuditRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.RunNormalBombCombat(normalBombAuditRomPath);
}

if (args.Length >= 2 && args[0] == "--retail-enemy-attack-audit")
{
    string attackAuditRomPath = string.Join(' ', args[1..]).Trim('"');
    return RetailEnemyExecutionAudit.RunEnemyAttackCombat(attackAuditRomPath);
}

if (args.Length >= 3 && args[0] == "--retail-enemy-projectile-definition-audit")
{
    string definitionText = args[1].TrimStart('$');
    if (!ushort.TryParse(
            definitionText,
            System.Globalization.NumberStyles.AllowHexSpecifier,
            provider: null,
            out ushort projectileDefinition))
    {
        throw new ArgumentException(
            $"Projectile audit definition '{args[1]}' is not a four-digit hexadecimal word.");
    }
    string projectileAuditRomPath = string.Join(' ', args[2..]).Trim('"');
    return RetailEnemyExecutionAudit.RunProjectileCombat(
        projectileAuditRomPath,
        projectileDefinition);
}

if (args.Length >= 2 && args[0] == "--dead-torizo-audit")
{
    string deadTorizoRomPath = string.Join(' ', args[1..]).Trim('"');
    return DeadTorizoAudit.Run(deadTorizoRomPath);
}

if (args.Length >= 2 && args[0] == "--shitroid-audit")
{
    string shitroidRomPath = string.Join(' ', args[1..]).Trim('"');
    return ShitroidAudit.Run(shitroidRomPath);
}

if (args.Length >= 2 && args[0] == "--hibashi-audit")
{
    string hibashiRomPath = string.Join(' ', args[1..]).Trim('"');
    return HibashiAudit.Run(hibashiRomPath);
}

if (args.Length >= 2 && args[0] == "--rinka-audit")
{
    string rinkaRomPath = string.Join(' ', args[1..]).Trim('"');
    return RinkaAudit.Run(rinkaRomPath);
}

if (args.Length >= 2 && args[0] == "--rio-audit")
{
    string rioRomPath = string.Join(' ', args[1..]).Trim('"');
    return RioAudit.Run(rioRomPath);
}

if (args.Length >= 2 && args[0] == "--norfair-lava-jumper-audit")
{
    string lavaJumperRomPath = string.Join(' ', args[1..]).Trim('"');
    return NorfairLavaJumpingEnemyAudit.Run(lavaJumperRomPath);
}

if (args.Length >= 2 && args[0] == "--norfair-rio-audit")
{
    string norfairRioRomPath = string.Join(' ', args[1..]).Trim('"');
    return NorfairRioAudit.Run(norfairRioRomPath);
}

if (args.Length >= 2 && args[0] == "--lower-norfair-rio-audit")
{
    string lowerNorfairRioRomPath = string.Join(' ', args[1..]).Trim('"');
    return LowerNorfairRioAudit.Run(lowerNorfairRioRomPath);
}

if (args.Length >= 2 && args[0] == "--maridia-large-snail-audit")
{
    string maridiaLargeSnailRomPath = string.Join(' ', args[1..]).Trim('"');
    return MaridiaLargeSnailAudit.Run(maridiaLargeSnailRomPath);
}

if (args.Length >= 2 && args[0] == "--gripper-ripper2-audit")
{
    string gripperRipper2RomPath = string.Join(' ', args[1..]).Trim('"');
    return GRipperRipper2Audit.Run(gripperRipper2RomPath);
}

if (args.Length >= 2 && args[0] == "--dragon-audit")
{
    string dragonRomPath = string.Join(' ', args[1..]).Trim('"');
    return DragonAudit.Run(dragonRomPath);
}

if (args.Length >= 2 && args[0] == "--shutter-audit")
{
    string shutterRomPath = string.Join(' ', args[1..]).Trim('"');
    return ShutterAudit.Run(shutterRomPath);
}

if (args.Length >= 2 && args[0] == "--elevator-audit")
{
    string elevatorRomPath = string.Join(' ', args[1..]).Trim('"');
    return ElevatorAudit.Run(elevatorRomPath);
}

// #485 characterization, not proof of the reported defect: the native focused
// sequence also misses one centered bomb during the dry-floor bomb-jump arc.
if (args.Length == 3 && args[0] == "--metroid-controller-comparison-audit")
    return MetroidAudit.CompareControllerBombs(args[1], args[2]);
if (args.Length == 2 && args[0] == "--metroid-runtime-bomb-audit")
    return MetroidAudit.VerifyRuntimePlacedBomb(args[1]);
if (args.Length >= 2 && args[0] == "--metroid-audit")
{
    string metroidRomPath = string.Join(' ', args[1..]).Trim('"');
    return MetroidAudit.Run(metroidRomPath);
}

if (args.Length >= 2 && args[0] == "--boulder-audit")
{
    string boulderRomPath = string.Join(' ', args[1..]).Trim('"');
    return BoulderAudit.Run(boulderRomPath);
}

if (args.Length >= 2 && args[0] == "--zebetite-audit")
{
    string zebetiteRomPath = string.Join(' ', args[1..]).Trim('"');
    return ZebetiteAudit.Run(zebetiteRomPath);
}

if (args.Length >= 2 && args[0] == "--etecoon-audit")
{
    string etecoonRomPath = string.Join(' ', args[1..]).Trim('"');
    return EtecoonAudit.Run(etecoonRomPath);
}

if (args.Length >= 2 && args[0] == "--dachora-audit")
{
    string dachoraRomPath = string.Join(' ', args[1..]).Trim('"');
    return DachoraAudit.Run(dachoraRomPath);
}

if (args.Length >= 2 && args[0] == "--evir-audit")
{
    string evirRomPath = string.Join(' ', args[1..]).Trim('"');
    return EvirAudit.Run(evirRomPath);
}

if (args.Length >= 2 && args[0] == "--morph-ball-eye-audit")
{
    string eyeRomPath = string.Join(' ', args[1..]).Trim('"');
    return MorphBallEyeAudit.Run(eyeRomPath);
}

if (args is ["--eye-window-native-compare", var eyeWindowRom, var eyeWindowCsv])
    return MorphBallEyeAudit.CompareNativeWindows(eyeWindowRom, eyeWindowCsv);

if (args.Length >= 2 && args[0] == "--wrecked-ship-ghost-audit")
{
    string ghostRomPath = string.Join(' ', args[1..]).Trim('"');
    return WreckedShipGhostAudit.Run(ghostRomPath);
}

if (args.Length >= 2 && args[0] == "--yapping-maw-runtime-contact-audit")
    return YappingMawAudit.RunRuntimeContact(Path.GetFullPath(args[1]));
if (args.Length == 2 && args[0] == "--yapping-maw-audio-audit")
    return YappingMawAudit.RunAudio(Path.GetFullPath(args[1]));

if (args.Length >= 2 && args[0] is "--yapping-maw-audit" or "--yapping-maw-history-audit")
{
    string yappingMawRomPath = string.Join(' ', args[1..]).Trim('"');
    return YappingMawAudit.Run(yappingMawRomPath, historyOnly: args[0] == "--yapping-maw-history-audit");
}

if (args.Length >= 2 && args[0] == "--blue-brinstar-face-block-audit")
{
    string faceBlockRomPath = string.Join(' ', args[1..]).Trim('"');
    return BlueBrinstarFaceBlockAudit.Run(faceBlockRomPath);
}

if (args.Length >= 2 && args[0] == "--ki-hunter-audit")
{
    string kiHunterRomPath = string.Join(' ', args[1..]).Trim('"');
    return KiHunterAudit.Run(kiHunterRomPath);
}

if (args.Length >= 2 && args[0] == "--heat-room-audit")
{
    string heatRoomRomPath = string.Join(' ', args[1..]).Trim('"');
    return HeatRoomAudit.Run(heatRoomRomPath);
}

if (args.Length >= 2 && args[0] == "--norfair-barrier-collision-audit")
{
    string norfairBarrierRomPath = args[^1].Trim('"');
    string? norfairBarrierRecordingPath = args.Length >= 3 ? args[1].Trim('"') : null;
    return NorfairBarrierCollisionAudit.Run(
        norfairBarrierRomPath,
        norfairBarrierRecordingPath);
}

if (args.Length >= 2 && args[0] == "--breakable-block-initial-state-audit")
{
    string breakableBlockRomPath = string.Join(' ', args[1..]).Trim('"');
    return BreakableBlockInitialStateAudit.Run(breakableBlockRomPath);
}

if (args.Length == 4 && args[0] == "--load-appearance-artifact-audit")
{
    return LoadAppearanceArtifactAudit.Run(
        args[1].Trim('"'),
        args[2].Trim('"'),
        args[3].Trim('"'));
}

if (args.Length == 2 && args[0] == "--load-appearance-artifact-audit")
    return LoadAppearanceArtifactAudit.RunRegression(args[1].Trim('"'));

if (args.Length >= 2 && args[0] == "--pipe-bug-audit")
{
    string pipeBugRomPath = string.Join(' ', args[1..]).Trim('"');
    return PipeBugAudit.Run(pipeBugRomPath);
}

if (args.Length >= 2 && args[0] == "--botwoon-x-plasma-controls")
    return BotwoonAudit.RunXPlasmaControls(string.Join(' ', args[1..]).Trim('"'));
if (args.Length >= 2 && args[0] == "--botwoon-x-plasma-fired")
    return BotwoonAudit.RunFiredXPlasma(string.Join(' ', args[1..]).Trim('"'));
if (args.Length >= 2 && args[0] == "--botwoon-hyper-audit")
    return BotwoonAudit.RunHyper(string.Join(' ', args[1..]).Trim('"'));
if (args.Length >= 2 && args[0] == "--botwoon-x-plasma")
    return BotwoonAudit.RunXPlasma(string.Join(' ', args[1..]).Trim('"'));
if (args.Length >= 2 && args[0] == "--botwoon-audit")
{
    string botwoonRomPath = string.Join(' ', args[1..]).Trim('"');
    return BotwoonAudit.Run(botwoonRomPath);
}

if (args.Length >= 2 && args[0] == "--spore-spawn-audit")
{
    string sporeSpawnRomPath = string.Join(' ', args[1..]).Trim('"');
    return SporeSpawnAudit.Run(sporeSpawnRomPath);
}

if (args.Length >= 2 && args[0] == "--ceres-steam-audit")
{
    string ceresSteamRomPath = string.Join(' ', args[1..]).Trim('"');
    return CeresSteamAudit.Run(ceresSteamRomPath);
}

if (args.Length >= 2 && args[0] == "--ceres-door-audit")
{
    string ceresDoorRomPath = string.Join(' ', args[1..]).Trim('"');
    return CeresDoorAudit.Run(ceresDoorRomPath);
}

if (args.Length == 3 && args[0] == "--norfair-ridley-death-audio-trace")
    return NorfairRidleyAudit.Run(args[1], args[2]);
if (args.Length >= 2 && args[0] == "--norfair-ridley-audit")
{
    string ridleyRomPath = string.Join(' ', args[1..]).Trim('"');
    return NorfairRidleyAudit.Run(ridleyRomPath);
}

if (args.Length is 2 or 3 && args[0] == "--kraid-lint-contact-audit")
    return KraidLintContactAudit.Run(args[1], args.Length == 3 ? args[2] : null);

if (args.Length >= 2 && args[0] == "--kraid-audit")
{
    string kraidRomPath = string.Join(' ', args[1..]).Trim('"');
    return KraidAudit.Run(kraidRomPath);
}

if (args.Length is 3 or 4 && args[0] == "--kraid-death-capture")
{
    int result = KraidAudit.Run(args[1], args[2]);
    if (result == 0 && args.Length == 4)
        KraidDeathCapture.VerifyNativeArmTrace(args[2], args[3]);
    return result;
}
if (args.Length == 3 && args[0] == "--kraid-floor-death-capture")
    return KraidAudit.Run(args[1], args[2], observeFloor: true);

if (args.Length is 3 or 4 && args[0] == "--fake-kraid-cadence")
    return FakeKraidCadenceAudit.Run(args[1], args[2], args.Length == 4 ? args[3] : null);

if (args.Length == 2 && args[0] == "--kraid-camera-audit")
    return KraidCameraAudit.Run(args[1]);

if (args.Length == 3 && args[0] == "--kraid-rise-capture")
{
    return KraidAudit.CaptureRise(
        args[1].Trim('"'),
        args[2].Trim('"'));
}

if (args.Length >= 2 && args[0] == "--phantoon-audit")
{
    string phantoonRomPath = string.Join(' ', args[1..]).Trim('"');
    return PhantoonAudit.Run(phantoonRomPath);
}

if (args.Length >= 2 && args[0] == "--draygon-audit")
{
    string draygonRomPath = string.Join(' ', args[1..]).Trim('"');
    return DraygonAudit.Run(draygonRomPath);
}

if (args.Length >= 2 && args[0] == "--mother-brain-room-audit")
    return MotherBrainRoomAudit.Run(args[1]);

if (args.Length == 3 && args[0] == "--mother-brain-recording-audit")
    return MotherBrainRecordingAudit.Run(args[1], args[2]);
if (args.Length == 3 && args[0] == "--mother-brain-beam-recording-audit")
    return MotherBrainRecordingAudit.Run(args[1], args[2], verifyBeam: true);
if (args.Length == 3 && args[0] == "--mother-brain-death-recording-audit")
    return MotherBrainRecordingAudit.Run(args[1], args[2], verifyDeath: true);

if (args.Length >= 2 && args[0] == "--mother-brain-audit")
{
    string motherBrainRomPath = string.Join(' ', args[1..]).Trim('"');
    return MotherBrainAudit.Run(motherBrainRomPath);
}

if (args.Length >= 2 && args[0] == "--mother-brain-glass-projectile-audit")
{
    string glassProjectileRomPath = string.Join(' ', args[1..]).Trim('"');
    return MotherBrainGlassProjectileAudit.Run(glassProjectileRomPath);
}

if (args.Length >= 2 && args[0] == "--bomb-torizo-audit")
{
    string bombTorizoRomPath = string.Join(' ', args[1..]).Trim('"');
    return BombTorizoAudit.Run(bombTorizoRomPath);
}

if (args.Length >= 2 && args[0] == "--golden-torizo-audit")
{
    string goldenTorizoRomPath = string.Join(' ', args[1..]).Trim('"');
    return GoldenTorizoAudit.Run(goldenTorizoRomPath);
}

if (args.Length >= 2 && args[0] == "--tourian-palette-fx-audit")
    return TourianPaletteFxAudit.Run(args[1]);

if (args.Length >= 2 && args[0] == "--tourian-entrance-statue-audit")
{
    string statueRomPath = string.Join(' ', args[1..]).Trim('"');
    return TourianEntranceStatueAudit.Run(statueRomPath);
}

if (args.Length >= 2 && args[0] == "--shaktool-audit")
{
    string shaktoolRomPath = string.Join(' ', args[1..]).Trim('"');
    return ShaktoolAudit.Run(shaktoolRomPath);
}

if (args.Length >= 2 && args[0] == "--chozo-statue-audit")
{
    string chozoStatueRomPath = string.Join(' ', args[1..]).Trim('"');
    return ChozoStatueAudit.Run(chozoStatueRomPath);
}

if (args.Length >= 2 && args[0] == "--escape-animals-audit")
{
    string escapeAnimalsRomPath = string.Join(' ', args[1..]).Trim('"');
    return EscapeAnimalsAudit.Run(escapeAnimalsRomPath);
}

if (args.Length >= 2 && args[0] == "--crocomire-audit")
{
    string crocomireRomPath = string.Join(' ', args[1..]).Trim('"');
    return CrocomireAudit.Run(crocomireRomPath);
}

if (args.Length >= 2 && args[0] == "--butterfly-zoa-audit")
{
    string zoaRomPath = string.Join(' ', args[1..]).Trim('"');
    return ZoaAudit.Run(zoaRomPath);
}

if (args.Length >= 2 && args[0] == "--shared-crawler-audit")
{
    string crawlerRomPath = string.Join(' ', args[1..]).Trim('"');
    return SharedCrawlerAudit.Run(crawlerRomPath);
}

if (args.Length >= 2 && args[0] == "--pre-bowling-hzoomer-audit")
{
    string hzoomerRomPath = string.Join(' ', args[1..]).Trim('"');
    return HZoomerAudit.Run(hzoomerRomPath);
}

if (args.Length >= 2 && args[0] == "--aqueduct-yard-audit")
{
    string yardRomPath = string.Join(' ', args[1..]).Trim('"');
    return YardAudit.Run(yardRomPath);
}

if (args.Length >= 2 && args[0] == "--terminator-waver-audit")
{
    string waverRomPath = string.Join(' ', args[1..]).Trim('"');
    return WaverAudit.Run(waverRomPath);
}

if (args.Length >= 2 && args[0] == "--dachora-metaree-audit")
{
    string metareeRomPath = string.Join(' ', args[1..]).Trim('"');
    return MetareeAudit.Run(metareeRomPath);
}

if (args.Length >= 2 && args[0] == "--green-brinstar-fireflea-audit")
{
    string firefleaRomPath = string.Join(' ', args[1..]).Trim('"');
    return FirefleaAudit.Run(firefleaRomPath);
}

if (args.Length >= 2 && args[0] == "--main-street-skultera-audit")
{
    string skulteraRomPath = string.Join(' ', args[1..]).Trim('"');
    return SkulteraAudit.Run(skulteraRomPath);
}

if (args.Length >= 2 && args[0] == "--bowling-alley-choot-audit")
{
    string chootRomPath = string.Join(' ', args[1..]).Trim('"');
    return ChootAudit.Run(chootRomPath);
}

if (args.Length >= 2 && args[0] == "--ocean-platform-audit")
{
    string platformRomPath = string.Join(' ', args[1..]).Trim('"');
    return PlatformAudit.Run(platformRomPath);
}

if (args.Length >= 2 && args[0] == "--crateria-power-bombs-alcoon-audit")
{
    string alcoonRomPath = string.Join(' ', args[1..]).Trim('"');
    return AlcoonAudit.Run(alcoonRomPath);
}

if (args.Length >= 2 && args[0] == "--green-brinstar-beetom-audit")
{
    string beetomRomPath = string.Join(' ', args[1..]).Trim('"');
    return BeetomAudit.Run(beetomRomPath);
}

if (args.Length >= 2 && args[0] == "--mt-everest-powamp-audit")
{
    string powampRomPath = string.Join(' ', args[1..]).Trim('"');
    return PowampAudit.Run(powampRomPath);
}

if (args.Length >= 2 && args[0] == "--wrecked-ship-work-robot-audit")
{
    string workRobotRomPath = string.Join(' ', args[1..]).Trim('"');
    return WorkRobotAudit.Run(workRobotRomPath);
}

if (args.Length >= 2 && args[0] == "--sponge-bath-bull-audit")
{
    string bullRomPath = string.Join(' ', args[1..]).Trim('"');
    return BullAudit.Run(bullRomPath);
}

if (args.Length >= 2 && args[0] == "--wrecked-ship-atomic-audit")
{
    string atomicRomPath = string.Join(' ', args[1..]).Trim('"');
    return AtomicAudit.Run(atomicRomPath);
}

if (args.Length >= 2 && args[0] == "--wrecked-ship-spark-audit")
{
    string sparkRomPath = string.Join(' ', args[1..]).Trim('"');
    return SparkAudit.Run(sparkRomPath);
}

if (args.Length >= 2 && args[0] == "--wrecked-ship-kzan-audit")
{
    string kzanRomPath = string.Join(' ', args[1..]).Trim('"');
    return KzanAudit.Run(kzanRomPath);
}

if (args.Length >= 2 && args[0] == "--norfair-nuclear-waffle-audit")
{
    string nuclearWaffleRomPath = string.Join(' ', args[1..]).Trim('"');
    return NuclearWaffleAudit.Run(nuclearWaffleRomPath);
}

if (args.Length >= 2 && args[0] == "--brinstar-fake-kraid-audit")
{
    string fakeKraidRomPath = string.Join(' ', args[1..]).Trim('"');
    return FakeKraidAudit.Run(fakeKraidRomPath);
}

if (args.Length >= 2 && args[0] == "--brinstar-walking-pirate-audit")
{
    string walkingPirateRomPath = string.Join(' ', args[1..]).Trim('"');
    return WalkingSpacePirateAudit.Run(walkingPirateRomPath);
}

// Exercises the complete bank-$B2 wall Pirate family against the Climb's untouched eleven
// actors plus the Pit's real bit-15-clear fast variant.
if (args.Length >= 2 && args[0] == "--crateria-wall-pirate-audit")
{
    string wallPirateRomPath = string.Join(' ', args[1..]).Trim('"');
    return WallSpacePirateAudit.Run(wallPirateRomPath);
}

// The Metal Pirates room contains exactly two untouched gold ninja Pirates facing inward,
// making both directions and all post-relative branches independently reproducible.
if (args.Length >= 2 && args[0] == "--norfair-ninja-pirate-audit")
{
    string ninjaPirateRomPath = string.Join(' ', args[1..]).Trim('"');
    return NinjaSpacePirateAudit.Run(ninjaPirateRomPath);
}

if (args.Length >= 2 && args[0] == "--parlor-awake-audit")
{
    string parlorRomPath = string.Join(' ', args[1..]).Trim('"');
    return SkreeAudit.Run(parlorRomPath);
}

if (args.Length >= 2 && args[0] == "--ripper-audit")
{
    string ripperRomPath = string.Join(' ', args[1..]).Trim('"');
    return RipperAudit.Run(ripperRomPath);
}

// The scoped audit covers all three bank-$A2 variants rather than leaving Mellow's former
// inline Flyway-only probe inside this already-large command dispatcher.
if (args.Length >= 2 && args[0] == "--flyway-audit")
{
    string flywayRomPath = string.Join(' ', args[1..]).Trim('"');
    return FlyFamilyAudit.Run(flywayRomPath);
}

// Climb is the first retail room whose population consists entirely of Sbugs. Keeping the
// audit on that unmodified population proves that the translated AI consumes the cartridge's
// real parameters, trigonometry tables, instruction lists, enemy header, and graphics.
if (args.Length >= 2 && args[0] == "--climb-sbug-audit")
{
    string climbRomPath = string.Join(' ', args[1..]).Trim('"');
    SuperMetroidAddressSpace climbBus = SuperMetroidAddressSpace.LoadRetailRom(climbRomPath);
    CartridgeRoomHeader climbRoom = CartridgeRoomHeader.Load(climbBus, 0x96ba);
    CartridgeRoomAssets climbAssets = CartridgeRoomAssets.Load(climbBus, climbRoom);
    var climbVram = new SnesVram();
    var climbCgram = new SnesCgram();
    climbAssets.LoadGraphics(climbVram, climbCgram);
    var climbEnemies = new RoomEnemySystem();
    var climbRandom = new Bank80SystemState();
    climbEnemies.Load(
        climbBus,
        climbRoom.State.EnemyPopulationPointer,
        climbRoom.State.EnemyTilesetPointer,
        climbVram,
        climbCgram,
        climbRandom.NextRandom,
        climbRandom.SetRandomNumber);

    const ushort sbugDefinition = 0xd87f;
    if (climbRoom.State.Pointer != 0x96d1 || climbEnemies.EnemyCount != 10 ||
        climbEnemies.Slots.Take(10).Any(slot => slot.EnemyDefinitionPointer != sbugDefinition) ||
        climbEnemies.SbugStates.Take(10).Any(state => state is null))
    {
        throw new InvalidDataException(
            $"Climb selected state ${climbRoom.State.Pointer:X4} with " +
            $"{climbEnemies.EnemyCount} non-uniform or uninitialized Sbugs.");
    }

    RoomEnemySlot auditedSbug = climbEnemies.Slots[0];
    SbugEnemyState auditedState = climbEnemies.SbugStates[0]
        ?? throw new InvalidDataException("Climb slot zero did not create Sbug state.");
    if (auditedSbug.XPosition != 0x0114 || auditedSbug.YPosition != 0x004c ||
        auditedSbug.Parameter1 != 0x5003 || auditedSbug.Parameter2 != 0x0050 ||
        auditedState.Function != SbugEnemyFunction.WaitForSamus)
    {
        throw new InvalidDataException(
            $"First Climb Sbug disagrees with population/initialization data: " +
            $"position=(${auditedSbug.XPosition:X4},${auditedSbug.YPosition:X4}), " +
            $"parameters=${auditedSbug.Parameter1:X4}/${auditedSbug.Parameter2:X4}, " +
            $"function=$A3:{(ushort)auditedState.Function:X4}.");
    }

    var climbSamus = new SamusState
    {
        Health = 99,
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 0,
        YPosition = 0,
    };
    climbSamus.RefreshCollisionRadii(climbBus);
    climbSamus.InitializeAnimation(climbBus);

    ushort waitingX = auditedSbug.XPosition;
    ushort waitingY = auditedSbug.YPosition;
    var sbugAnimationMaps = new HashSet<ushort>();
    for (int frame = 0; frame < 20; frame++)
    {
        climbEnemies.StepFrame(
            cameraX: 0x0100,
            cameraY: 0,
            timeIsFrozen: false,
            climbSamus,
            level: climbAssets.LevelData);
        sbugAnimationMaps.Add(auditedSbug.SpritemapPointer);
    }
    if (sbugAnimationMaps.Count != 3 || auditedSbug.XPosition != waitingX ||
        auditedSbug.YPosition != waitingY ||
        auditedState.Function != SbugEnemyFunction.WaitForSamus)
    {
        throw new InvalidDataException(
            $"Waiting Sbug animation/position failed: maps={sbugAnimationMaps.Count}, " +
            $"position=({waitingX},{waitingY})->" +
            $"({auditedSbug.XPosition},{auditedSbug.YPosition}), " +
            $"function=$A3:{(ushort)auditedState.Function:X4}.");
    }

    // Exercise every entry in the native seven-word activation table. Retail Climb uses
    // index zero, but the remaining behaviors are part of the same shipped enemy AI and
    // are retained for other population/state combinations and ROM-derived fixtures.
    SbugEnemyFunction[] activationFunctions =
    [
        SbugEnemyFunction.ActivateMoveForward,
        SbugEnemyFunction.ActivateZigZag,
        SbugEnemyFunction.ActivateMoveTowardSamus,
        SbugEnemyFunction.ActivateRandomUntilCollision,
        SbugEnemyFunction.ActivateRandomAndReverseWhenFar,
        SbugEnemyFunction.ActivateMoveForwardThenWait,
        SbugEnemyFunction.ActivateMoveAwayFromSamus,
    ];
    SbugEnemyFunction[] activeFunctions =
    [
        SbugEnemyFunction.MoveForward,
        SbugEnemyFunction.ZigZag,
        SbugEnemyFunction.MoveTowardSamus,
        SbugEnemyFunction.ChooseRandomDirectionUntilCollision,
        SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar,
        SbugEnemyFunction.MoveForwardThenWait,
        SbugEnemyFunction.MoveAwayFromSamus,
    ];
    for (int behavior = 0; behavior < activationFunctions.Length; behavior++)
    {
        auditedSbug.Parameter2 = unchecked((ushort)((behavior << 8) | 0x50));
        auditedState.Function = SbugEnemyFunction.WaitForSamus;
        climbSamus.XPosition = auditedSbug.XPosition;
        climbSamus.YPosition = auditedSbug.YPosition;
        climbEnemies.StepFrame(0x0100, 0, false, climbSamus, level: climbAssets.LevelData);
        if (auditedState.Function != activationFunctions[behavior])
        {
            throw new InvalidDataException(
                $"Sbug behavior {behavior} selected $A3:{(ushort)auditedState.Function:X4}, " +
                $"expected $A3:{(ushort)activationFunctions[behavior]:X4}.");
        }

        climbRandom.SetRandomNumber(0x1234);
        climbEnemies.StepFrame(0x0100, 0, false, climbSamus, level: climbAssets.LevelData);
        if (auditedState.Function != activeFunctions[behavior])
        {
            throw new InvalidDataException(
                $"Sbug behavior {behavior} activated $A3:{(ushort)auditedState.Function:X4}, " +
                $"expected $A3:{(ushort)activeFunctions[behavior]:X4}.");
        }
        if (behavior is 3 or 4 && climbRandom.RandomNumber != 0x000b)
        {
            throw new InvalidDataException(
                $"Sbug behavior {behavior} wrote RNG ${climbRandom.RandomNumber:X4}, expected $000B.");
        }
    }

    // Return to the actual population behavior and prove that its activation consumes one
    // frame before movement begins, exactly as the native indirect-dispatch state machine.
    auditedSbug.Parameter2 = 0x0050;
    auditedState.Function = SbugEnemyFunction.WaitForSamus;
    auditedSbug.XPosition = waitingX;
    auditedSbug.YPosition = waitingY;
    auditedSbug.XSubposition = 0;
    auditedSbug.YSubposition = 0;
    climbSamus.XPosition = auditedSbug.XPosition;
    climbSamus.YPosition = auditedSbug.YPosition;
    climbEnemies.StepFrame(0x0100, 0, false, climbSamus, level: climbAssets.LevelData);
    climbEnemies.StepFrame(0x0100, 0, false, climbSamus, level: climbAssets.LevelData);
    if (auditedSbug.XPosition != waitingX || auditedSbug.YPosition != waitingY ||
        auditedState.Function != SbugEnemyFunction.MoveForward)
    {
        throw new InvalidDataException("Sbug moved during its two-frame activation transition.");
    }
    climbEnemies.StepFrame(0x0100, 0, false, climbSamus, level: climbAssets.LevelData);
    if (auditedSbug.XPosition == waitingX && auditedSbug.YPosition == waitingY)
        throw new InvalidDataException("Activated Sbug did not apply its ROM-derived forward velocity.");

    var climbOam = new OamBuffer();
    climbOam.BeginFrame();
    climbEnemies.DrawLayers(climbOam, 0x0100, 0, 0, 7);
    climbOam.FinalizeFrame();
    if (climbOam.LastFinalizedSpriteCount == 0)
        throw new InvalidDataException("Climb's live Sbug spritemaps emitted no enemy OBJ.");

    // Every retail Climb instance sets $0400, intentionally excluding ordinary Samus and
    // projectile collision. Clear only that population flag for a focused proof that the
    // Sbug header is nevertheless wired to the common 40-damage touch handler and its ROM
    // vulnerability table. The gameplay population remains untouched outside this audit.
    if (!auditedSbug.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
        throw new InvalidDataException("Retail Climb Sbug unexpectedly allows collision.");
    auditedSbug.Properties = auditedSbug.Properties.Without(EnemyProperties.IgnoreSamusCollision);
    climbEnemies.StepFrame(0x0100, 0, false, climbSamus, level: climbAssets.LevelData);
    climbSamus.XPosition = auditedSbug.XPosition;
    climbSamus.YPosition = auditedSbug.YPosition;
    climbSamus.Health = 99;
    climbSamus.InvincibilityTimer = 0;
    if (!climbEnemies.ResolveOrdinarySamusContact(climbSamus, 0) ||
        climbSamus.Health != 59 || !climbSamus.KnockbackActive)
    {
        throw new InvalidDataException(
            $"Sbug common contact failed: health={climbSamus.Health}, " +
            $"knockback={climbSamus.KnockbackActive}.");
    }

    var sbugProjectiles = new SamusProjectileSystem();
    var sbugBombs = new SamusBombProjectileSystem();
    SamusProjectileSlot sbugShot = sbugProjectiles.Slots[0];
    sbugShot.ClearFields();
    sbugShot.Type = 0;
    sbugShot.Damage = 20;
    sbugShot.Direction = 2;
    sbugShot.XPosition = auditedSbug.XPosition;
    sbugShot.YPosition = auditedSbug.YPosition;
    sbugShot.XRadius = 4;
    sbugShot.YRadius = 4;
    sbugShot.InstructionPointer = 0x9000;
    sbugShot.InstructionTimer = 1;
    ushort sbugHealth = auditedSbug.Health;
    if (climbEnemies.ResolveOrdinaryProjectileHits(climbBus, sbugProjectiles, sbugBombs) != 1 ||
        auditedSbug.Health != sbugHealth)
    {
        throw new InvalidDataException(
            $"Sbug indestructible vulnerability failed: health={sbugHealth}->{auditedSbug.Health}.");
    }

    Console.WriteLine(
        $"Climb Sbug audit passed: ten retail actors loaded, three ROM maps animated, " +
        $"all seven activation modes dispatched, movement/contact/vulnerability agreed, and " +
        $"{climbOam.LastFinalizedSpriteCount} OBJ pieces rendered.");
    return 0;
}

// Colosseum's default state is a clean retail fixture containing eight Mochtroids and no
// other enemy family. It therefore proves the custom steering/attachment/touch behavior
// without replacing unsupported neighboring actors with host-authored placeholders.
if (args.Length >= 2 && args[0] == "--colosseum-mochtroid-audit")
{
    string colosseumRomPath = string.Join(' ', args[1..]).Trim('"');
    SuperMetroidAddressSpace colosseumBus =
        SuperMetroidAddressSpace.LoadRetailRom(colosseumRomPath);
    CartridgeRoomHeader colosseumRoom = CartridgeRoomHeader.Load(colosseumBus, 0xd72a);
    CartridgeRoomAssets colosseumAssets = CartridgeRoomAssets.Load(colosseumBus, colosseumRoom);
    var colosseumVram = new SnesVram();
    var colosseumCgram = new SnesCgram();
    colosseumAssets.LoadGraphics(colosseumVram, colosseumCgram);
    var colosseumEnemies = new RoomEnemySystem();
    var colosseumRandom = new Bank80SystemState();
    colosseumEnemies.Load(
        colosseumBus,
        colosseumRoom.State.EnemyPopulationPointer,
        colosseumRoom.State.EnemyTilesetPointer,
        colosseumVram,
        colosseumCgram,
        colosseumRandom.NextRandom,
        colosseumRandom.SetRandomNumber);

    const ushort mochtroidDefinition = 0xd8ff;
    if (colosseumRoom.State.Pointer != 0xd737 || colosseumEnemies.EnemyCount != 8 ||
        colosseumEnemies.Slots.Take(8).Any(
            slot => slot.EnemyDefinitionPointer != mochtroidDefinition) ||
        colosseumEnemies.MochtroidStates.Take(8).Any(state => state is null))
    {
        throw new InvalidDataException(
            $"Colosseum selected state ${colosseumRoom.State.Pointer:X4} with " +
            $"{colosseumEnemies.EnemyCount} non-uniform or uninitialized Mochtroids.");
    }

    RoomEnemySlot auditedMochtroid = colosseumEnemies.Slots[0];
    MochtroidEnemyState auditedMochtroidState = colosseumEnemies.MochtroidStates[0]
        ?? throw new InvalidDataException("Colosseum slot zero did not create Mochtroid state.");
    if (auditedMochtroid.XPosition != 0x0080 || auditedMochtroid.YPosition != 0x0078 ||
        auditedMochtroid.Layer != 2 ||
        auditedMochtroidState.InstalledInstructionList != 0xa745 ||
        auditedMochtroidState.MovementMode != MochtroidMovementMode.NotTouchingSamus)
    {
        throw new InvalidDataException(
            $"First Colosseum Mochtroid initialization disagrees with ROM data: " +
            $"position=(${auditedMochtroid.XPosition:X4},${auditedMochtroid.YPosition:X4}), " +
            $"layer={auditedMochtroid.Layer}, list=$A3:" +
            $"{auditedMochtroidState.InstalledInstructionList:X4}, " +
            $"mode={(ushort)auditedMochtroidState.MovementMode}.");
    }

    var colosseumSamus = new SamusState
    {
        Health = 999,
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 0x0180,
        YPosition = 0x0078,
    };
    colosseumSamus.RefreshCollisionRadii(colosseumBus);
    colosseumSamus.InitializeAnimation(colosseumBus);

    // A -256-pixel signed X delta produces +0.4000 on the first proportional-attraction
    // call: (-256 >> 2) << 8 is -$4000, which the AI subtracts from zero velocity.
    colosseumEnemies.StepFrame(
        0,
        0,
        false,
        colosseumSamus,
        level: colosseumAssets.LevelData);
    if (auditedMochtroid.XPosition != 0x0080 || auditedMochtroid.XSubposition != 0x4000 ||
        auditedMochtroidState.XVelocity != 0 ||
        auditedMochtroidState.XSubvelocity != 0x4000)
    {
        throw new InvalidDataException(
            $"Mochtroid first steering step was " +
            $"${auditedMochtroid.XPosition:X4}.${auditedMochtroid.XSubposition:X4} with " +
            $"velocity {auditedMochtroidState.XVelocity}." +
            $"{auditedMochtroidState.XSubvelocity:X4}, expected $0080.4000.");
    }

    var mochtroidIdleMaps = new HashSet<ushort> { auditedMochtroid.SpritemapPointer };
    for (int frame = 1; frame < 56; frame++)
    {
        colosseumEnemies.StepFrame(
            0,
            0,
            false,
            colosseumSamus,
            level: colosseumAssets.LevelData);
        mochtroidIdleMaps.Add(auditedMochtroid.SpritemapPointer);
    }
    if (mochtroidIdleMaps.Count != 3 || auditedMochtroid.XPosition <= 0x0080)
    {
        throw new InvalidDataException(
            $"Mochtroid free flight/idle animation failed: maps={mochtroidIdleMaps.Count}, " +
            $"X=${auditedMochtroid.XPosition:X4}.");
    }

    var colosseumOam = new OamBuffer();
    ushort mochtroidCameraX = auditedMochtroid.XPosition > 0x0080
        ? unchecked((ushort)(auditedMochtroid.XPosition - 0x0080))
        : (ushort)0;
    ushort mochtroidCameraY = auditedMochtroid.YPosition > 0x0080
        ? unchecked((ushort)(auditedMochtroid.YPosition - 0x0080))
        : (ushort)0;
    colosseumEnemies.StepFrame(
        mochtroidCameraX,
        mochtroidCameraY,
        false,
        colosseumSamus,
        level: colosseumAssets.LevelData);
    colosseumOam.BeginFrame();
    colosseumEnemies.DrawLayers(
        colosseumOam,
        mochtroidCameraX,
        mochtroidCameraY,
        0,
        7);
    colosseumOam.FinalizeFrame();
    if (colosseumOam.LastFinalizedSpriteCount == 0)
        throw new InvalidDataException("Colosseum's live Mochtroid spritemaps emitted no enemy OBJ.");

    // Collision runs after EnemyMain. Re-overlap the moving actor once per frame so touch AI
    // republishes attached mode exactly as the retail collision scheduler does. Calls 1..79
    // do no damage; call 80 applies the header's 90 damage and immediately clears both timers.
    ushort attachedStartHealth = colosseumSamus.Health;
    var mochtroidAttachedMaps = new HashSet<ushort>();
    for (int contact = 1; contact <= 80; contact++)
    {
        ushort contactCameraX = auditedMochtroid.XPosition > 0x0080
            ? unchecked((ushort)(auditedMochtroid.XPosition - 0x0080))
            : (ushort)0;
        ushort contactCameraY = auditedMochtroid.YPosition > 0x0080
            ? unchecked((ushort)(auditedMochtroid.YPosition - 0x0080))
            : (ushort)0;
        colosseumEnemies.StepFrame(
            contactCameraX,
            contactCameraY,
            false,
            colosseumSamus,
            level: colosseumAssets.LevelData);
        colosseumSamus.XPosition = auditedMochtroid.XPosition;
        colosseumSamus.YPosition = auditedMochtroid.YPosition;
        if (!colosseumEnemies.ResolveOrdinarySamusContact(colosseumSamus, 0))
            throw new InvalidDataException($"Mochtroid lost overlap on contact {contact}.");
        // Contact changes the instruction pointer after this frame's enemy-instruction
        // pass. The first overlap therefore still displays the final idle map; sample the
        // attached list beginning on the following native frame.
        if (contact > 1)
            mochtroidAttachedMaps.Add(auditedMochtroid.SpritemapPointer);
        ushort expectedHealth = contact == 80
            ? unchecked((ushort)(attachedStartHealth - 90))
            : attachedStartHealth;
        if (colosseumSamus.Health != expectedHealth)
        {
            throw new InvalidDataException(
                $"Mochtroid contact {contact} changed health to {colosseumSamus.Health}, " +
                $"expected {expectedHealth}.");
        }
    }
    if (auditedMochtroidState.AttachmentDamageTimer != 0 ||
        auditedMochtroidState.InstalledInstructionList != 0xa759 ||
        mochtroidAttachedMaps.Count != 3 ||
        colosseumSamus.InvincibilityTimer != 0 || colosseumSamus.KnockbackTimer != 0 ||
        colosseumSamus.KnockbackActive)
    {
        throw new InvalidDataException(
            $"Mochtroid attachment state failed: timer={auditedMochtroidState.AttachmentDamageTimer}, " +
            $"list=$A3:{auditedMochtroidState.InstalledInstructionList:X4}, " +
            $"maps={mochtroidAttachedMaps.Count}, invinc={colosseumSamus.InvincibilityTimer}, " +
            $"knockback={colosseumSamus.KnockbackTimer}/{colosseumSamus.KnockbackActive}.");
    }

    // The custom $A3:A9A8 shot entry is a thin wrapper around normal shot AI. Use a real
    // power-beam collision to prove it consumes the default vulnerability and 100-health
    // header before testing the newly shared Screw-Attack contact branch.
    var mochtroidProjectiles = new SamusProjectileSystem();
    var mochtroidBombs = new SamusBombProjectileSystem();
    SamusProjectileSlot mochtroidShot = mochtroidProjectiles.Slots[0];
    mochtroidShot.ClearFields();
    mochtroidShot.Type = 0;
    mochtroidShot.Damage = 20;
    mochtroidShot.Direction = 2;
    mochtroidShot.XPosition = auditedMochtroid.XPosition;
    mochtroidShot.YPosition = auditedMochtroid.YPosition;
    mochtroidShot.XRadius = 4;
    mochtroidShot.YRadius = 4;
    mochtroidShot.InstructionPointer = 0x9000;
    mochtroidShot.InstructionTimer = 1;
    if (colosseumEnemies.ResolveOrdinaryProjectileHits(
            colosseumBus,
            mochtroidProjectiles,
            mochtroidBombs) != 1 || auditedMochtroid.Health != 80)
    {
        throw new InvalidDataException(
            $"Mochtroid common shot wrapper left health {auditedMochtroid.Health}, expected 80.");
    }

    auditedMochtroid.InvincibilityTimer = 0;
    colosseumSamus.HorizontalSpeed.ContactDamageIndex = 3;
    colosseumSamus.XPosition = auditedMochtroid.XPosition;
    colosseumSamus.YPosition = auditedMochtroid.YPosition;
    if (!colosseumEnemies.ResolveOrdinarySamusContact(colosseumSamus, 0) ||
        auditedMochtroid.Health != 0 ||
        auditedMochtroid.EnemyDefinitionPointer != 0 || colosseumEnemies.EnemiesKilled != 1)
    {
        throw new InvalidDataException(
            $"Screw-Attack contact failed: health={auditedMochtroid.Health}, " +
            $"deleted={auditedMochtroid.Properties.HasAny(EnemyProperties.Deleted)}.");
    }

    Console.WriteLine(
        $"Colosseum Mochtroid audit passed: eight retail actors loaded, proportional and " +
        $"attached flight advanced, both three-map ROM animations rendered, the 80-contact " +
        $"drain dealt 90 damage, beam/Screw damage resolved, and " +
        $"{colosseumOam.LastFinalizedSpriteCount} OBJ pieces rendered.");
    return 0;
}

// Measures the exact hot path used by ordinary gameplay's four BG-relative OBJ priority
// insertions. The legacy side intentionally calls the public filtered renderer four times;
// the resolved side uses the single OAM raster consumed by the optimized compositor. This
// stays as a CLI audit so future correctness work can detect an accidental return to four
// tile decodes per sprite without relying on subjective desktop smoothness.
if (args.Length >= 2 && args[0] == "--obj-render-benchmark")
{
    string benchmarkRomPath = string.Join(' ', args[1..]).Trim('"');
    SuperMetroidAddressSpace benchmarkBus =
        SuperMetroidAddressSpace.LoadRetailRom(benchmarkRomPath);
    CartridgeRoomHeader benchmarkRoom = CartridgeRoomHeader.Load(benchmarkBus, 0xe0b5);
    var benchmarkRuntime = new SuperMetroidRuntime(benchmarkBus);
    benchmarkRuntime.InitializeHud(HudSnapshot.CeresDebug);
    benchmarkRuntime.InitializeStartingCeresRoom();
    benchmarkRuntime.InitializeCeresStartSamus();
    benchmarkRuntime.LoadCartridgeRoomForDebug(benchmarkRoom.Pointer);
    benchmarkRuntime.Samus!.InputLocked = true;
    benchmarkRuntime.StepFrame(0);
    benchmarkRuntime.StepFrame(0);

    // Before measuring, compare the optimized single-scan compositor with the deliberately
    // slow, independently assembled priority-plane reference that production used before
    // this optimization. The retail Ridley room exercises BG12NBA=$66, both BG priorities,
    // and the cross-priority Samus/Ridley OAM overlap in one deterministic frame.
    Rgba32[] optimizedOrdinaryFrame = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
        benchmarkRuntime.Vram,
        benchmarkRuntime.Cgram,
        benchmarkRuntime.Oam,
        benchmarkRuntime.BackgroundScroll.Bg1HorizontalScroll,
        benchmarkRuntime.BackgroundScroll.Bg1VerticalScroll,
        benchmarkRuntime.BackgroundScroll.Bg2HorizontalScroll,
        benchmarkRuntime.BackgroundScroll.Bg2VerticalScroll,
        bg1CharacterBaseWord: 0x6000,
        bg2CharacterBaseWord: 0x6000);
    Rgba32[] referenceOrdinaryFrame = RenderOrdinaryGameplayReference(
        benchmarkRuntime.Vram,
        benchmarkRuntime.Cgram,
        benchmarkRuntime.Oam,
        benchmarkRuntime.BackgroundScroll.Bg1HorizontalScroll,
        benchmarkRuntime.BackgroundScroll.Bg1VerticalScroll,
        benchmarkRuntime.BackgroundScroll.Bg2HorizontalScroll,
        benchmarkRuntime.BackgroundScroll.Bg2VerticalScroll,
        bg1CharacterBaseWord: 0x6000,
        bg2CharacterBaseWord: 0x6000);
    int ordinaryMismatch = optimizedOrdinaryFrame.AsSpan().SequenceEqual(referenceOrdinaryFrame)
        ? -1
        : Enumerable.Range(0, optimizedOrdinaryFrame.Length)
            .First(pixel => optimizedOrdinaryFrame[pixel] != referenceOrdinaryFrame[pixel]);
    if (ordinaryMismatch >= 0)
    {
        throw new InvalidDataException(
            $"Optimized Mode-1 compositor diverged from the priority-plane reference at " +
            $"({ordinaryMismatch % 256},{ordinaryMismatch / 256}): " +
            $"optimized={optimizedOrdinaryFrame[ordinaryMismatch]}, " +
            $"reference={referenceOrdinaryFrame[ordinaryMismatch]}.");
    }

    const int benchmarkIterations = 250;
    const byte benchmarkObsel = 0x03;

    // Warm both JIT paths and ROM/VRAM cache lines before collecting time or allocation
    // counts. Consuming one field prevents an optimizing runtime from proving the raster
    // result dead if this benchmark is ever compiled with more aggressive whole-program
    // optimization.
    int benchmarkChecksum = 0;
    for (int priority = 0; priority < 4; priority++)
    {
        benchmarkChecksum ^= SnesObjRenderer.Render(
            benchmarkRuntime.Oam,
            benchmarkRuntime.Vram,
            benchmarkRuntime.Cgram,
            benchmarkObsel,
            priority: priority)[0].A;
    }
    benchmarkChecksum ^= SnesObjRenderer.RenderResolved(
        benchmarkRuntime.Oam,
        benchmarkRuntime.Vram,
        benchmarkRuntime.Cgram,
        benchmarkObsel).Priorities[0];

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    long legacyAllocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    long legacyStarted = System.Diagnostics.Stopwatch.GetTimestamp();
    for (int iteration = 0; iteration < benchmarkIterations; iteration++)
    {
        for (int priority = 0; priority < 4; priority++)
        {
            Rgba32[] plane = SnesObjRenderer.Render(
                benchmarkRuntime.Oam,
                benchmarkRuntime.Vram,
                benchmarkRuntime.Cgram,
                benchmarkObsel,
                priority: priority);
            benchmarkChecksum ^= plane[(iteration * 257 + priority) % plane.Length].A;
        }
    }
    TimeSpan legacyElapsed = System.Diagnostics.Stopwatch.GetElapsedTime(legacyStarted);
    long legacyAllocated = GC.GetAllocatedBytesForCurrentThread() - legacyAllocatedBefore;

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    long resolvedAllocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    long resolvedStarted = System.Diagnostics.Stopwatch.GetTimestamp();
    for (int iteration = 0; iteration < benchmarkIterations; iteration++)
    {
        ResolvedObjFrame resolved = SnesObjRenderer.RenderResolved(
            benchmarkRuntime.Oam,
            benchmarkRuntime.Vram,
            benchmarkRuntime.Cgram,
            benchmarkObsel);
        int sample = (iteration * 257) % resolved.Pixels.Length;
        benchmarkChecksum ^= resolved.Pixels[sample].A ^ resolved.Priorities[sample];
    }
    TimeSpan resolvedElapsed = System.Diagnostics.Stopwatch.GetElapsedTime(resolvedStarted);
    long resolvedAllocated = GC.GetAllocatedBytesForCurrentThread() - resolvedAllocatedBefore;

    // Measure the whole ordinary-room compositor as a second number. This separates the
    // now-cheap OBJ raster from BG tilemap decoding, HUD composition, and frame-buffer
    // allocation, making the next optimization target obvious instead of speculative.
    SuperMetroidRuntimeFrameRenderer.Render(benchmarkRuntime);
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    long frameAllocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    long frameStarted = System.Diagnostics.Stopwatch.GetTimestamp();
    for (int iteration = 0; iteration < benchmarkIterations; iteration++)
    {
        Rgba32[] frame = SuperMetroidRuntimeFrameRenderer.Render(benchmarkRuntime);
        benchmarkChecksum ^= frame[(iteration * 257) % frame.Length].A;
    }
    TimeSpan frameElapsed = System.Diagnostics.Stopwatch.GetElapsedTime(frameStarted);
    long frameAllocated = GC.GetAllocatedBytesForCurrentThread() - frameAllocatedBefore;

    // Ceres' elevator shaft uses the separate Mode-7-plus-HUD/OBJ path. Measure it beside
    // ordinary Mode 1 so a fast Ridley-room benchmark cannot conceal the exact slowdown a
    // player sees immediately after leaving the frontend menus.
    const short identityScale = 0x0100;
    const short ceresShaftCenterX = 0x0080;
    const short ceresShaftCenterY = 0x03f0;
    SnesGameplayFrameRenderer.RenderHudMode7AndObjs(
        benchmarkRuntime.Vram,
        benchmarkRuntime.Cgram,
        benchmarkRuntime.Oam,
        identityScale,
        matrixB: 0,
        matrixC: 0,
        matrixD: identityScale,
        ceresShaftCenterX,
        ceresShaftCenterY,
        horizontalOffset: 0,
        verticalOffset: 0);
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    long mode7AllocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    long mode7Started = System.Diagnostics.Stopwatch.GetTimestamp();
    for (int iteration = 0; iteration < benchmarkIterations; iteration++)
    {
        Rgba32[] frame = SnesGameplayFrameRenderer.RenderHudMode7AndObjs(
            benchmarkRuntime.Vram,
            benchmarkRuntime.Cgram,
            benchmarkRuntime.Oam,
            identityScale,
            matrixB: 0,
            matrixC: 0,
            matrixD: identityScale,
            ceresShaftCenterX,
            ceresShaftCenterY,
            horizontalOffset: 0,
            verticalOffset: 0);
        benchmarkChecksum ^= frame[(iteration * 257) % frame.Length].A;
    }
    TimeSpan mode7Elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(mode7Started);
    long mode7Allocated = GC.GetAllocatedBytesForCurrentThread() - mode7AllocatedBefore;

    Console.WriteLine(
        $"OBJ raster benchmark ({benchmarkIterations} Ceres frames, " +
        $"{benchmarkRuntime.Oam.LastFinalizedSpriteCount} finalized sprites, " +
        $"pixel-exact reference parity):");
    Console.WriteLine(
        $"  four filtered passes: {legacyElapsed.TotalMilliseconds:F1} ms, " +
        $"{legacyAllocated / 1024.0 / 1024.0:F1} MiB allocated");
    Console.WriteLine(
        $"  one resolved pass:    {resolvedElapsed.TotalMilliseconds:F1} ms, " +
        $"{resolvedAllocated / 1024.0 / 1024.0:F1} MiB allocated");
    Console.WriteLine(
        $"  improvement: {legacyElapsed.TotalMilliseconds / resolvedElapsed.TotalMilliseconds:F2}x time, " +
        $"{legacyAllocated / (double)resolvedAllocated:F2}x allocation; checksum {benchmarkChecksum}.");
    Console.WriteLine(
        $"  complete PPU frame: {frameElapsed.TotalMilliseconds / benchmarkIterations:F2} ms/frame, " +
        $"{benchmarkIterations / frameElapsed.TotalSeconds:F1} fps, " +
        $"{frameAllocated / 1024.0 / benchmarkIterations:F1} KiB/frame allocated.");
    Console.WriteLine(
        $"  Ceres Mode 7 frame: {mode7Elapsed.TotalMilliseconds / benchmarkIterations:F2} ms/frame, " +
        $"{benchmarkIterations / mode7Elapsed.TotalSeconds:F1} fps, " +
        $"{mode7Allocated / 1024.0 / benchmarkIterations:F1} KiB/frame allocated.");
    return 0;
}

// Exercise the reported title/cinematic parity boundaries without opening a window. This
// keeps transform direction, retail title dwell, caret motion, and both Rinka spawn waves
// independently debuggable from the much larger playable-room audit.
if (args.Length >= 2 && args[0] == "--frontend-parity-audit")
{
    string frontendRomPath = string.Join(' ', args[1..]).Trim('"');
    SuperMetroidAddressSpace frontendBus = SuperMetroidAddressSpace.LoadRetailRom(frontendRomPath);
    var titleAudit = new TitleSequenceState(frontendBus);
    Rgba32[] yearFrame = titleAudit.Render();
    if (yearFrame[0] != new Rgba32(0, 0, 0, 255))
    {
        throw new InvalidDataException(
            $"1994 card enabled a non-OBJ layer; top-left backdrop was {yearFrame[0]} instead of black.");
    }
    bool sawNintendoPan = false;
    bool sawZoomOut = false;
    ushort previousScale = 0;
    short previousOffset = 0;
    for (int frame = 0; frame < 8192 && titleAudit.Phase != TitleSequencePhase.TitleScreen; frame++)
    {
        TitleSequencePhase beforePhase = titleAudit.Phase;
        previousScale = titleAudit.Mode7MatrixScale;
        previousOffset = titleAudit.Mode7HorizontalOffset;
        titleAudit.Step(0);
        if (beforePhase == TitleSequencePhase.SceneZeroPan &&
            titleAudit.Mode7HorizontalOffset < previousOffset)
        {
            sawNintendoPan = true;
        }
        if (beforePhase == TitleSequencePhase.SceneThreeZoom &&
            titleAudit.Mode7MatrixScale > previousScale)
        {
            sawZoomOut = true;
        }
    }
    if (titleAudit.Phase != TitleSequencePhase.TitleScreen ||
        titleAudit.TitleScreenFramesRemaining != 900 ||
        !sawNintendoPan ||
        !sawZoomOut)
    {
        throw new InvalidDataException(
            $"Frontend title parity failed: phase={titleAudit.Phase}, " +
            $"timer={titleAudit.TitleScreenFramesRemaining}, pan={sawNintendoPan}, zoomOut={sawZoomOut}.");
    }

    var introAudit = new IntroCinematicState(frontendBus);
    int narrationFrames = 0;
    var pageOneCaretPositions = new HashSet<(ushort X, ushort Y)>();
    while (introAudit.Phase != IntroCinematicPhase.PageOneAwaitingInput && narrationFrames < 8192)
    {
        introAudit.Step(0);
        if (introAudit.Phase is IntroCinematicPhase.PageOneText or
            IntroCinematicPhase.PageOneAwaitingInput)
        {
            pageOneCaretPositions.Add((introAudit.IntroCaretX, introAudit.IntroCaretY));
        }
        narrationFrames++;
    }
    if (introAudit.Phase != IntroCinematicPhase.PageOneAwaitingInput ||
        introAudit.IntroCaretX != 8 ||
        introAudit.IntroCaretY != 136 ||
        pageOneCaretPositions.Count < 8)
    {
        throw new InvalidDataException(
            $"Intro page one did not advance its retail caret: phase={introAudit.Phase}, " +
            $"caret=({introAudit.IntroCaretX},{introAudit.IntroCaretY}), " +
            $"distinctPositions={pageOneCaretPositions.Count}.");
    }

    introAudit.Step((ushort)SnesButton.A);
    if (introAudit.IntroCaretY != 0x00f8)
        throw new InvalidDataException($"Intro caret remained at Y=${introAudit.IntroCaretY:X4} during crossfade.");

    bool sawRinkaHit = false;
    bool sawRinkaHurtPose = false;
    bool sawRinkaKnockbackMotion = false;
    bool sawRinkaRise = false;
    bool sawPostKnockbackFall = false;
    bool sawFloorRecovery = false;
    var hurtAnimationFrames = new HashSet<ushort>();
    ushort previousFlashbackSamusX = introAudit.FlashbackSamusX;
    ushort previousFlashbackSamusY = introAudit.FlashbackSamusY;
    int flashbackFrames = 0;
    while (flashbackFrames < 1024 &&
        (!sawRinkaHit || !sawRinkaHurtPose || !sawRinkaKnockbackMotion ||
            !sawRinkaRise || !sawPostKnockbackFall || !sawFloorRecovery ||
            hurtAnimationFrames.Count < 2 || introAudit.SpawnedIntroRinkaCount < 4 ||
            introAudit.MotherBrainHitCount < 4))
    {
        introAudit.Step(0);
        sawRinkaHit |= introAudit.FlashbackSamusInvincibilityTimer != 0;
        sawRinkaHurtPose |= introAudit.FlashbackSamusPose == SamusPoseIds.KnockbackLeftPose;
        sawRinkaKnockbackMotion |= introAudit.FlashbackSamusKnockbackActive &&
            introAudit.FlashbackSamusX != previousFlashbackSamusX;
        sawRinkaRise |= introAudit.FlashbackSamusKnockbackActive &&
            introAudit.FlashbackSamusY < previousFlashbackSamusY;
        sawPostKnockbackFall |= !introAudit.FlashbackSamusKnockbackActive &&
            introAudit.FlashbackSamusPose == SamusPoseIds.FallingLeftPose &&
            introAudit.FlashbackSamusY > previousFlashbackSamusY;
        sawFloorRecovery |= sawRinkaHit &&
            !introAudit.FlashbackSamusKnockbackActive &&
            introAudit.FlashbackSamusY == 115 &&
            introAudit.FlashbackSamusPose is not (
                SamusPoseIds.KnockbackLeftPose or SamusPoseIds.FallingLeftPose);
        if (introAudit.FlashbackSamusPose == SamusPoseIds.KnockbackLeftPose)
            hurtAnimationFrames.Add(introAudit.FlashbackSamusAnimationFrame);
        previousFlashbackSamusX = introAudit.FlashbackSamusX;
        previousFlashbackSamusY = introAudit.FlashbackSamusY;
        flashbackFrames++;
    }
    if (!sawRinkaHit || !sawRinkaHurtPose || !sawRinkaKnockbackMotion ||
        !sawRinkaRise || !sawPostKnockbackFall || !sawFloorRecovery ||
        hurtAnimationFrames.Count < 2 || introAudit.SpawnedIntroRinkaCount != 4 ||
        introAudit.MotherBrainHitCount != 4)
    {
        throw new InvalidDataException(
            $"Intro Rinka audit stopped after {flashbackFrames} frames with " +
            $"spawned={introAudit.SpawnedIntroRinkaCount}, hit={sawRinkaHit}, " +
            $"hurtPose={sawRinkaHurtPose}, hurtFrames={hurtAnimationFrames.Count}, " +
            $"knockbackMotion={sawRinkaKnockbackMotion}, rise={sawRinkaRise}, " +
            $"fall={sawPostKnockbackFall}, floor={sawFloorRecovery}, " +
            $"Samus=({introAudit.FlashbackSamusX},{introAudit.FlashbackSamusY})/" +
            $"${introAudit.FlashbackSamusPose:X2}, MotherBrainHits={introAudit.MotherBrainHitCount}.");
    }

    var ceresStartAudit = new SuperMetroidRuntime(frontendBus);
    ceresStartAudit.InitializeHud(HudSnapshot.CeresDebug);
    ceresStartAudit.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
    ceresStartAudit.InitializeStartingCeresRoom();
    ceresStartAudit.InitializeCeresStartSamus();
    byte expectedMapX = ceresStartAudit.ActiveRoom!.MapX;
    byte expectedMapY = unchecked((byte)(ceresStartAudit.ActiveRoom.MapY + 1));
    if (ceresStartAudit.Hud.MinimapCenterX != expectedMapX ||
        ceresStartAudit.Hud.MinimapCenterY != expectedMapY)
    {
        throw new InvalidDataException(
            $"Locked Ceres arrival published minimap " +
            $"({ceresStartAudit.Hud.MinimapCenterX},{ceresStartAudit.Hud.MinimapCenterY}) " +
            $"instead of ({expectedMapX},{expectedMapY}).");
    }

    var hazeAudit = new Rgba32[SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
    Array.Fill(hazeAudit, new Rgba32(0, 0, 0, 255));
    SnesGameplayFrameRenderer.ApplyCeresHaze(hazeAudit, ridleyIsDead: false);
    // The retail HDMA table holds component zero for the first 64 scanlines.
    // The first nonzero band starts below that, not immediately below the HUD.
    if (hazeAudit[0].B != 0 ||
        hazeAudit[32 * SnesGameplayFrameRenderer.Width].B != 0 ||
        hazeAudit[63 * SnesGameplayFrameRenderer.Width].B != 0 ||
        hazeAudit[64 * SnesGameplayFrameRenderer.Width].B == 0 ||
        hazeAudit[223 * SnesGameplayFrameRenderer.Width].B <=
            hazeAudit[64 * SnesGameplayFrameRenderer.Width].B)
    {
        throw new InvalidDataException("Ceres haze did not preserve BG3 HUD and increase blue toward the floor.");
    }

    Console.WriteLine(
        $"Frontend parity audit passed: black 1994 card, Nintendo pan, title zoom-out, " +
        $"900-frame NTSC dwell, " +
        $"{pageOneCaretPositions.Count} caret positions then Y=$F8, four Rinkas, " +
        $"Samus hurt/rise/fall/floor recovery and four later Mother Brain hits after " +
        $"{flashbackFrames} flashback frames, " +
        $"arrival minimap ({expectedMapX},{expectedMapY}), and ROM-selected Ceres haze.");
    return 0;
}

// Load Ceres Ridley's room directly from its retail bank-$8F header, then run the complete
// deterministic dispatcher/palette reveal slice. This is separate from screenshot capture: it
// gives regressions in $E13F initialization, instruction dispatch, or palette timing a fast
// ROM-backed command with concrete debugger-visible state.
if (args.Length >= 2 && args[0] == "--ceres-ridley-audit")
{
    string ridleyRomPath = string.Join(' ', args[1..]).Trim('"');
    SuperMetroidAddressSpace ridleyBus = SuperMetroidAddressSpace.LoadRetailRom(ridleyRomPath);
    CartridgeRoomHeader ridleyRoom = CartridgeRoomHeader.Load(ridleyBus, 0xe0b5);
    var ridleyEnemies = new RoomEnemySystem();
    var ridleyVram = new SnesVram();
    var ridleyCgram = new SnesCgram();
    CartridgeRoomAssets retailRidleyAssets = CartridgeRoomAssets.Load(ridleyBus, ridleyRoom);
    // StartGameplay loads room characters first, then lets enemy graphics replace their
    // reserved OBJ region. This ordering is also what makes the later Mode-7 getaway's
    // interleaved view of the same VRAM deterministic.
    retailRidleyAssets.LoadGraphics(ridleyVram, ridleyCgram);
    ridleyEnemies.Load(
        ridleyBus,
        ridleyRoom.State.EnemyPopulationPointer,
        ridleyRoom.State.EnemyTilesetPointer,
        ridleyVram,
        ridleyCgram,
        // Low nibble zero selects the first literal $A6:A743 fireball route after hover.
        () => 0x1230,
        readRandomNumber: () => 0x1230);

    RoomEnemySlot ridleySlot = ridleyEnemies.Slots[0];
    RidleyEnemyState ridleyState = ridleyEnemies.CeresRidley
        ?? throw new InvalidDataException("Retail room $E0B5 did not initialize enemy $E13F.");
    if (ridleySlot.EnemyDefinitionPointer != 0xe13f ||
        ridleySlot.Definition.InitializationAiPointer != 0xa0f5 ||
        ridleySlot.Definition.MainAiPointer != 0xa288)
    {
        throw new InvalidDataException(
            $"Retail room $E0B5 selected enemy ${ridleySlot.EnemyDefinitionPointer:X4}, " +
            $"init ${ridleySlot.Definition.InitializationAiPointer:X4}, " +
            $"main ${ridleySlot.Definition.MainAiPointer:X4}.");
    }

    // `$A6:A2DF` branches directly to the Baby-and-door routine while Ridley's animation
    // word is zero. Prove both halves of that scheduling contract before advancing reveal:
    // the immediate EnemyMain phase must emit the Baby, and the not-yet-installed deferred
    // hook must be inert. A test that starts only after battle entry misses the exact visual
    // ordering regression where the Baby first appears together with Ridley.
    var preRevealOam = new OamBuffer();
    preRevealOam.BeginFrame();
    ridleyEnemies.DrawCeresRidleyImmediateBabyAndDoor(preRevealOam, cameraX: 0, cameraY: 0);
    int preRevealImmediateBytes = preRevealOam.NextByteOffset;
    ridleyEnemies.DrawCeresRidleyPostEnemyHook(preRevealOam, cameraX: 0, cameraY: 0);
    if (ridleyState.MovementAnimationEnabled != 0 ||
        ridleyState.BabyCurrentSpritemap == 0 ||
        preRevealImmediateBytes == 0 ||
        preRevealOam.NextByteOffset != preRevealImmediateBytes)
    {
        throw new InvalidDataException(
            $"Ceres Baby pre-reveal scheduling failed: movement={ridleyState.MovementAnimationEnabled}, " +
            $"map=${ridleyState.BabyCurrentSpritemap:X4}, immediate={preRevealImmediateBytes}, " +
            $"afterDeferred={preRevealOam.NextByteOffset}.");
    }

    // 1 entry call + 512 remaining delay calls + 65 eye-table calls + 32 body-fade calls.
    for (int frame = 0; frame < 610; frame++)
        ridleyEnemies.StepFrame(cameraX: 0, cameraY: 0, timeIsFrozen: false);
    if (ridleyState.Function != RidleyAiFunction.WaitBeforeRoar ||
        ridleyState.FunctionTimer != 4)
    {
        throw new InvalidDataException(
            $"Retail Ceres Ridley reveal ended at $A6:{(ushort)ridleyState.Function:X4}, " +
            $"timer ${ridleyState.FunctionTimer:X4} instead of $A455/$0004.");
    }

    // Continue through the cartridge's roar/liftoff lists until $A6:A6E8 publishes the
    // active fight. Supplying a real Samus object matters: the dispatcher reads her energy
    // every frame and must not be audited through a host-only null shortcut.
    var auditSamus = new SamusState { Health = 99 };
    int battleEntryFrames = 0;
    while (ridleyState.Function != RidleyAiFunction.CeresHovering && battleEntryFrames < 1024)
    {
        ridleyEnemies.StepFrame(0, 0, timeIsFrozen: false, auditSamus);
        battleEntryFrames++;
    }
    if (ridleyState.Function != RidleyAiFunction.CeresHovering || ridleyState.FightMode != 1)
    {
        throw new InvalidDataException(
            $"Retail Ceres Ridley never entered battle; stopped at " +
            $"$A6:{(ushort)ridleyState.Function:X4} after {battleEntryFrames} frames.");
    }

    // DrawLayers must prepend the seven tail pieces and current wing map before the common
    // extended body. Checking both the solved segment chain and its OAM output catches a
    // future regression that leaves the Baby actor working while silently dropping the two
    // special draw calls again.
    if (ridleyState.TailSegments.Length != 7 ||
        ridleyState.TailSegments.Select(segment => segment.XPosition).Distinct().Count() < 3)
    {
        throw new InvalidDataException("Ceres Ridley neutral tail did not solve seven world-space segments.");
    }
    var ridleyCompositeOam = new OamBuffer();
    ridleyCompositeOam.BeginFrame();
    ridleyEnemies.DrawLayers(ridleyCompositeOam, cameraX: 0, cameraY: 0, 0, 7);
    int compositeByteOffsetBeforePostHook = ridleyCompositeOam.NextByteOffset;
    ridleyEnemies.DrawCeresRidleyPostEnemyHook(ridleyCompositeOam, cameraX: 0, cameraY: 0);
    if (ridleyCompositeOam.NextByteOffset < 12 * 4)
    {
        throw new InvalidDataException(
            $"Ceres Ridley composite emitted only {ridleyCompositeOam.NextByteOffset / 4} OBJ entries.");
    }
    if (ridleyState.BabyCurrentSpritemap == 0 ||
        ridleyCompositeOam.NextByteOffset <= compositeByteOffsetBeforePostHook)
    {
        throw new InvalidDataException(
            $"Ceres Baby post-enemy hook emitted no OBJ: map=${ridleyState.BabyCurrentSpritemap:X4}, " +
            $"OAM={compositeByteOffsetBeforePostHook}->{ridleyCompositeOam.NextByteOffset}.");
    }

    // Exercise the actual $A6:E73A mouth animation before shortening the battle with the
    // scripted 100-hit condition. It must produce $86:9642 actors, collide with the retail
    // room plane, damage Samus, and bloom into the directional afterburn definitions.
    RoomLevelData retailRidleyLevel = retailRidleyAssets.LevelData;
    var ridleyInitialScroll = new BackgroundScrollState
    {
        Layer2ScrollX = ridleyRoom.State.Layer2ScrollX,
        Layer2ScrollY = ridleyRoom.State.Layer2ScrollY,
    };
    BackgroundTilemapStreamer ridleyInitialStreamer =
        retailRidleyLevel.CreateBackgroundStreamer(sizeOfBg2: 0x0800);
    IReadOnlyList<BackgroundUpdateRequest> ridleyInitialRequests =
        ridleyInitialScroll.BuildInitialViewportRequests();
    foreach (BackgroundUpdateRequest request in ridleyInitialRequests)
        ridleyInitialStreamer.Build(request)?.ExecuteTo(ridleyVram);
    if (ridleyInitialRequests.Count != 34)
    {
        throw new InvalidDataException(
            $"Retail Ridley room initial viewport produced {ridleyInitialRequests.Count} requests, expected 34.");
    }

    auditSamus.Health = 999;
    auditSamus.XPosition = 0x0080;
    auditSamus.YPosition = 0x0064;
    bool sawFireball = false;
    bool sawAfterburn = false;
    bool sawSamusDamage = false;
    bool sawNativeVelocityRange = false;
    int fireballAuditFrames = 0;
    while (!(sawFireball && sawAfterburn && sawSamusDamage && sawNativeVelocityRange) &&
        fireballAuditFrames < 2048)
    {
        ushort healthBefore = auditSamus.Health;
        ridleyEnemies.StepFrame(0, 0, timeIsFrozen: false, auditSamus);
        ridleyEnemies.StepEnemyProjectiles(retailRidleyLevel, auditSamus);
        sawFireball |= ridleyEnemies.EnemyProjectiles.Any(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CeresRidleyFireball);
        sawAfterburn |= ridleyEnemies.EnemyProjectiles.Any(
            projectile => projectile.Kind is not (
                RoomEnemyProjectileKind.None or RoomEnemyProjectileKind.CeresRidleyFireball));
        sawSamusDamage |= auditSamus.Health < healthBefore;
        if (ridleyEnemies.EnemyProjectiles.Any(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CeresRidleyFireball))
        {
            // Speed $0500 multiplied by a signed unit sine can never exceed $0500.
            // This catches both an incorrect table bank and accidental double $40 angle
            // bias; either mistake still creates moving actors but sends them far offscreen.
            int absoluteXVelocity = Math.Abs((int)(short)ridleyState.FireballXVelocity);
            int absoluteYVelocity = Math.Abs((int)(short)ridleyState.FireballYVelocity);
            sawNativeVelocityRange = absoluteXVelocity <= 0x0500 &&
                absoluteYVelocity <= 0x0500;
        }
        fireballAuditFrames++;
    }
    if (!sawFireball || !sawAfterburn || !sawSamusDamage || !sawNativeVelocityRange)
    {
        throw new InvalidDataException(
            $"Retail fireball audit stopped after {fireballAuditFrames} frames: " +
            $"fireball={sawFireball}, afterburn={sawAfterburn}, damage={sawSamusDamage}, " +
            $"nativeVelocity={sawNativeVelocityRange}, " +
            $"AI=$A6:{(ushort)ridleyState.Function:X4}, " +
            $"velocity=({(short)ridleyState.FireballXVelocity},{(short)ridleyState.FireballYVelocity}), " +
            $"live={string.Join(';', ridleyEnemies.EnemyProjectiles.Where(p => p.IsActive).Select(p => $"{p.Kind}@{p.XPosition},{p.YPosition}"))}.");
    }

    // A projectile hit must enter the shared bank-$90 hurt state, not merely subtract
    // energy and set invincibility. The old partial translation made the OBJ flicker hide
    // an unchanged normal pose, which looked like Samus vanished on contact.
    if (!auditSamus.KnockbackActive ||
        auditSamus.Pose is not (SamusPoseIds.KnockbackRightPose or SamusPoseIds.KnockbackLeftPose) ||
        auditSamus.HurtFlashCounter == 0 ||
        auditSamus.Kinematics.YSpeed != 5 ||
        auditSamus.Kinematics.YSubspeed != 0)
    {
        throw new InvalidDataException(
            $"Ridley fireball damage omitted the shared hurt transition: " +
            $"active={auditSamus.KnockbackActive}, pose=${auditSamus.Pose:X2}, " +
            $"flash={auditSamus.HurtFlashCounter}, " +
            $"Y={auditSamus.Kinematics.YSpeed:X4}.{auditSamus.Kinematics.YSubspeed:X4}.");
    }

    var hurtOam = new OamBuffer();
    hurtOam.BeginFrame();
    if (!auditSamus.Draw(
            ridleyBus,
            hurtOam,
            layer1X: 0,
            layer1Y: 0,
            nmiFrameCounter: 1) ||
        hurtOam.NextByteOffset == 0)
    {
        throw new InvalidDataException(
            $"Ridley fireball hurt pose ${auditSamus.Pose:X2} emitted no Samus OBJ entries.");
    }

    // This isolated enemy audit does not run Samus's alpha/beta handlers, so explicitly
    // return the fixture to a normal cartridge pose before testing the independent beam
    // producer below. PlayableGame advances the same state through KnockbackMovement.Step.
    ResetRidleyAuditSamusAfterHurt(ridleyBus, auditSamus);

    // Exercise the active extended-spritemap frame through $A0:9A5A's exact component and
    // hitbox lists. Ridley's origin lies inside his torso frame at this point.
    auditSamus.Health = 999;
    auditSamus.XPosition = ridleySlot.XPosition;
    auditSamus.YPosition = ridleySlot.YPosition;
    if (!ridleyEnemies.ResolveCeresRidleySamusContact(auditSamus, controllerInput: 0) ||
        auditSamus.Health != 994 || !auditSamus.KnockbackActive ||
        auditSamus.Kinematics.YSpeed != 5 || auditSamus.Kinematics.YSubspeed != 0)
    {
        throw new InvalidDataException(
            $"Ceres Ridley extended body contact failed: health={auditSamus.Health}, " +
            $"hurt={auditSamus.KnockbackActive}, pose=${auditSamus.Pose:X2}.");
    }
    ResetRidleyAuditSamusAfterHurt(ridleyBus, auditSamus);

    // Ridley_Func_127 at $A6:DFD9 gives the final tail entry its own 14-by-14
    // contact rectangle and applies the Ceres tail damage word ($000F). This is
    // deliberately separate from the extended body-map audit above: the tail is
    // drawn by custom OAM code and therefore never appears in $A0:9A5A's list.
    RidleyTailSegment tailTip = ridleyState.TailSegments[^1];
    auditSamus.Health = 999;
    auditSamus.XPosition = tailTip.XPosition;
    auditSamus.YPosition = tailTip.YPosition;
    if (!ridleyEnemies.ResolveCeresRidleySamusContact(auditSamus, controllerInput: 0) ||
        auditSamus.Health != 984 || !auditSamus.KnockbackActive)
    {
        throw new InvalidDataException(
            $"Ceres Ridley tail-tip contact failed: health={auditSamus.Health}, " +
            $"hurt={auditSamus.KnockbackActive}, pose=${auditSamus.Pose:X2}, " +
            $"tip=({tailTip.XPosition},{tailTip.YPosition}).");
    }
    ResetRidleyAuditSamusAfterHurt(ridleyBus, auditSamus);

    // Repeat body contact through SuperMetroidRuntime itself. The isolated checks above
    // validate bank-$A0 geometry, but previously let a missing scheduler call, hurt-handler
    // dispatch, or Samus draw silently escape. Direct room loading is an internal debugger
    // seam; every frame below still runs the production NMI, enemy, collision, movement,
    // palette, OAM, and finalization order used by the desktop game.
    var liveRidleyRuntime = new SuperMetroidRuntime(ridleyBus);
    liveRidleyRuntime.InitializeHud(HudSnapshot.CeresDebug);
    liveRidleyRuntime.InitializeStartingCeresRoom();
    liveRidleyRuntime.InitializeCeresStartSamus();
    liveRidleyRuntime.LoadCartridgeRoomForDebug(ridleyRoom.Pointer);
    SamusState liveSamus = liveRidleyRuntime.Samus!;
    RoomEnemySlot liveRidleySlot = liveRidleyRuntime.Enemies.Slots[0];
    RidleyEnemyState liveRidleyState = liveRidleyRuntime.Enemies.CeresRidley
        ?? throw new InvalidDataException("Runtime Ridley room omitted its Ceres actor.");

    // Reproduce the pre-reveal overlap that exposed the software-PPU bug. Ceres Ridley's
    // bank-$A0 header places him on enemy layer five and his bank-$A6 maps use OBJ3; Samus
    // is emitted earlier by the layer-three insertion and her bank-$92 maps use OBJ2. The
    // SNES first resolves the two opaque OBJ pixels by OAM number, then compares the winner
    // with BG priority. Rendering OBJ3 as an independent plane used to resurrect Ridley's
    // later pixel and make Samus disappear behind the still-black body.
    liveSamus.Pose = SamusPoseIds.FacingRightNormalPose;
    liveSamus.XPosition = liveRidleySlot.XPosition;
    liveSamus.YPosition = liveRidleySlot.YPosition;
    liveSamus.Health = 999;
    liveSamus.InvincibilityTimer = 0;
    liveSamus.InputLocked = true;
    liveSamus.RefreshCollisionRadii(ridleyBus);
    liveSamus.InitializeAnimation(ridleyBus);
    // Two frames admit the initial enemy instruction map and the selected Samus tiles
    // through their real main-loop/NMI producer-consumer boundary.
    liveRidleyRuntime.StepFrame(0);
    liveRidleyRuntime.StepFrame(0);

    var preRevealRidleyOnlyOam = new OamBuffer();
    preRevealRidleyOnlyOam.BeginFrame();
    liveRidleyRuntime.Enemies.DrawLayers(
        preRevealRidleyOnlyOam,
        liveRidleyRuntime.Camera!.XPosition,
        liveRidleyRuntime.Camera.YPosition,
        firstLayer: 5,
        lastLayer: 5);
    preRevealRidleyOnlyOam.FinalizeFrame();

    var preRevealSamusOnlyOam = new OamBuffer();
    preRevealSamusOnlyOam.BeginFrame();
    liveSamus.Draw(
        ridleyBus,
        preRevealSamusOnlyOam,
        liveRidleyRuntime.Camera.XPosition,
        liveRidleyRuntime.Camera.YPosition,
        liveRidleyRuntime.NmiFrameCounter);
    preRevealSamusOnlyOam.FinalizeFrame();

    Rgba32[] preRevealRidleyOnly = SnesObjRenderer.Render(
        preRevealRidleyOnlyOam,
        liveRidleyRuntime.Vram,
        liveRidleyRuntime.Cgram,
        obsel: 0x03,
        priority: 3);
    Rgba32[] preRevealSamusOnly = SnesObjRenderer.Render(
        preRevealSamusOnlyOam,
        liveRidleyRuntime.Vram,
        liveRidleyRuntime.Cgram,
        obsel: 0x03,
        priority: 2);
    Rgba32[] preRevealCombinedObj2 = SnesObjRenderer.Render(
        liveRidleyRuntime.Oam,
        liveRidleyRuntime.Vram,
        liveRidleyRuntime.Cgram,
        obsel: 0x03,
        priority: 2);
    Rgba32[] preRevealCombinedObj3 = SnesObjRenderer.Render(
        liveRidleyRuntime.Oam,
        liveRidleyRuntime.Vram,
        liveRidleyRuntime.Cgram,
        obsel: 0x03,
        priority: 3);

    int preRevealOverlapPixel = -1;
    for (int pixel = 32 * 256; pixel < preRevealSamusOnly.Length; pixel++)
    {
        if (preRevealSamusOnly[pixel].A != 0 && preRevealRidleyOnly[pixel].A != 0)
        {
            preRevealOverlapPixel = pixel;
            break;
        }
    }
    if (preRevealOverlapPixel < 0 ||
        preRevealCombinedObj2[preRevealOverlapPixel] != preRevealSamusOnly[preRevealOverlapPixel] ||
        preRevealCombinedObj3[preRevealOverlapPixel].A != 0)
    {
        throw new InvalidDataException(
            $"Pre-reveal Samus/Ridley OBJ ownership failed at pixel {preRevealOverlapPixel}: " +
            $"Samus={((preRevealOverlapPixel >= 0) ? preRevealSamusOnly[preRevealOverlapPixel] : default)}, " +
            $"Ridley={((preRevealOverlapPixel >= 0) ? preRevealRidleyOnly[preRevealOverlapPixel] : default)}, " +
            $"combined2={((preRevealOverlapPixel >= 0) ? preRevealCombinedObj2[preRevealOverlapPixel] : default)}, " +
            $"combined3={((preRevealOverlapPixel >= 0) ? preRevealCombinedObj3[preRevealOverlapPixel] : default)}.");
    }

    // Move the audit actor out of the arena while the literal 512-frame reveal delay runs;
    // this keeps the later battle-contact assertion independent of the overlap probe.
    liveSamus.Pose = SamusPoseIds.ForwardFacingPowerSuitPose;
    liveSamus.XPosition = 0x0010;
    liveSamus.YPosition = 0x0010;
    liveSamus.InvincibilityTimer = ushort.MaxValue;
    liveSamus.RefreshCollisionRadii(ridleyBus);
    liveSamus.InitializeAnimation(ridleyBus);
    int liveBattleEntryFrames = 0;
    while (liveRidleyState.Function != RidleyAiFunction.CeresHovering &&
        liveBattleEntryFrames < 2048)
    {
        liveRidleyRuntime.StepFrame(0);
        liveBattleEntryFrames++;
    }
    if (liveRidleyState.Function != RidleyAiFunction.CeresHovering)
    {
        throw new InvalidDataException(
            $"Runtime Ceres Ridley never entered battle after {liveBattleEntryFrames} frames.");
    }

    liveSamus.Pose = SamusPoseIds.FacingRightNormalPose;
    liveSamus.XPosition = liveRidleySlot.XPosition;
    liveSamus.YPosition = liveRidleySlot.YPosition;
    liveSamus.Health = 999;
    liveSamus.InvincibilityTimer = 0;
    liveSamus.InputLocked = false;
    liveSamus.RefreshCollisionRadii(ridleyBus);
    liveSamus.InitializeAnimation(ridleyBus);
    ushort liveContactStartY = liveSamus.YPosition;
    ushort hudOnesBeforeContact = liveRidleyRuntime.Hud.Tiles[0x8e / 2];
    liveRidleyRuntime.StepFrame(0);
    int liveContactOamCount = liveRidleyRuntime.Oam.LastFinalizedSpriteCount;
    if (liveSamus.Health != 994 || !liveSamus.KnockbackActive ||
        liveSamus.Pose is not (SamusPoseIds.KnockbackRightPose or SamusPoseIds.KnockbackLeftPose) ||
        !liveRidleyRuntime.LastSamusBodyDrawn ||
        liveRidleyRuntime.Hud.Tiles[0x8e / 2] == hudOnesBeforeContact)
    {
        throw new InvalidDataException(
            $"Runtime Ceres Ridley contact failed: health={liveSamus.Health}, " +
            $"hurt={liveSamus.KnockbackActive}, pose=${liveSamus.Pose:X2}, " +
            $"body={liveRidleyRuntime.LastSamusBodyDrawn}, " +
            $"HUD=${hudOnesBeforeContact:X4}->${liveRidleyRuntime.Hud.Tiles[0x8e / 2]:X4}.");
    }

    // `$18AA=5` forces the body visible throughout the special knockback handler. Verify
    // every remaining owned frame, not just its initial state, and bound the five-pixel
    // launch so a bad ROM table cannot masquerade as successful OAM emission off-screen.
    for (int hurtFrame = 1; hurtFrame < 5; hurtFrame++)
    {
        liveRidleyRuntime.StepFrame(0);
        if (!liveRidleyRuntime.LastSamusBodyDrawn ||
            Math.Abs(unchecked((short)(liveSamus.YPosition - liveContactStartY))) > 32)
        {
            throw new InvalidDataException(
                $"Runtime hurt frame {hurtFrame} lost Samus: body={liveRidleyRuntime.LastSamusBodyDrawn}, " +
                $"Y=${liveContactStartY:X4}->${liveSamus.YPosition:X4}, " +
                $"timer={liveSamus.KnockbackTimer}.");
        }
    }

    auditSamus.Health = 99;
    auditSamus.InvincibilityTimer = 0;

    // Reach the 100-hit escape condition through both physical projectile owners that call
    // `$A6:DF8A`: one exploding normal bomb followed by 99 actual power-beam slots. This
    // deliberately avoids assigning HitCounter from the audit. Bank $A0 must select the
    // current extended rectangle, mark the bomb, and dispatch the same Ceres callback used
    // by the ordinary-shot owner before the beam loop proves its independent impact path.
    const int auditRoomWidth = 32;
    const int auditRoomHeight = 16;
    var auditAir = new RoomLevelData(
        auditRoomWidth,
        auditRoomHeight,
        new ushort[auditRoomWidth * auditRoomHeight],
        new byte[auditRoomWidth * auditRoomHeight],
        new ushort[auditRoomWidth * auditRoomHeight],
        new byte[8]);
    var auditBombs = new SamusBombProjectileSystem();
    var auditProjectiles = new SamusProjectileSystem();
    ushort healthBeforeNormalBomb = ridleySlot.Health;
    ushort hitCounterBeforeNormalBomb = ridleyState.HitCounter;
    ushort flashBeforeNormalBomb = ridleySlot.FlashTimer;
    ushort expectedNormalBombFlash =
        flashBeforeNormalBomb != 0 && (flashBeforeNormalBomb & 1) != 0
            ? (ushort)14
            : (ushort)13;
    SamusBombProjectileSlot normalBomb =
        EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
            auditBombs,
            ridleySlot.XPosition,
            ridleySlot.YPosition);
    int normalBombHits = ridleyEnemies.ResolveOrdinaryBombHits(
        auditBombs,
        auditProjectiles,
        auditSamus);
    if (normalBombHits != 1 || (normalBomb.Direction & 0x0010) == 0 ||
        ridleySlot.Health != healthBeforeNormalBomb ||
        ridleyState.HitCounter != unchecked((ushort)(hitCounterBeforeNormalBomb + 1)) ||
        ridleySlot.FlashTimer != expectedNormalBombFlash ||
        ridleySlot.Properties.HasAny(EnemyProperties.Deleted))
    {
        throw new InvalidDataException(
            $"Ceres Ridley normal-bomb callback mismatch: hits={normalBombHits}, " +
            $"direction=${normalBomb.Direction:X4}, health=" +
            $"{healthBeforeNormalBomb}->{ridleySlot.Health}, counter=" +
            $"{hitCounterBeforeNormalBomb}->{ridleyState.HitCounter}, flash=" +
            $"{flashBeforeNormalBomb}->{ridleySlot.FlashTimer}/{expectedNormalBombFlash}, " +
            $"properties=${ridleySlot.Properties:X4}.");
    }

    for (int hit = 0; hit < 99; hit++)
    {
        auditSamus.XPosition = ridleySlot.XPosition;
        auditSamus.YPosition = ridleySlot.YPosition;
        auditBombs.StepFrame(ridleyBus, auditAir, auditSamus, 0, 0);
        SamusProjectileFrameResult fired = auditProjectiles.StepFrame(
            ridleyBus,
            auditAir,
            auditSamus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            auditBombs);
        if (fired.FiredSlot is not int firedSlot ||
            ridleyEnemies.ResolveCeresRidleyProjectileHits(
                ridleyBus, auditProjectiles, auditBombs) != 1)
        {
            throw new InvalidDataException(
                $"Retail Ceres Ridley shot {hit + 1} did not produce exactly one hit.");
        }

        int explosionFrames = 0;
        while (auditProjectiles.Slots[firedSlot].IsActive && explosionFrames < 64)
        {
            auditBombs.StepFrame(ridleyBus, auditAir, auditSamus, 0, 0);
            auditProjectiles.StepFrame(
                ridleyBus, auditAir, auditSamus, 0, 0, 0, 0, auditBombs);
            explosionFrames++;
        }
        if (auditProjectiles.Slots[firedSlot].IsActive)
            throw new InvalidDataException("Retail power-beam explosion did not delete within 64 frames.");

        ridleyEnemies.StepFrame(0, 0, timeIsFrozen: false, auditSamus);
    }
    if (ridleyState.HitCounter != 100)
        throw new InvalidDataException($"Retail Ceres hit counter ended at {ridleyState.HitCounter}.");

    int retreatFrames = 0;
    while (ridleyEnemies.CeresStatus != 1 && retreatFrames < 1024)
    {
        ridleyEnemies.StepFrame(0, 0, timeIsFrozen: false, auditSamus);
        retreatFrames++;
    }
    if (ridleyEnemies.CeresStatus != 1 ||
        ridleyState.Function != RidleyAiFunction.CeresInactive)
    {
        throw new InvalidDataException(
            $"Retail Ceres Ridley retreat did not publish status one; stopped at " +
            $"$A6:{(ushort)ridleyState.Function:X4}/status={ridleyEnemies.CeresStatus} " +
            $"after {retreatFrames} frames.");
    }
    RoomEnemySlot[] getawayWalls = ridleyEnemies.Slots
        .Where(slot => slot.EnemyDefinitionPointer == 0xe23f && slot.Parameter1 is 5 or 6)
        .ToArray();
    if (getawayWalls.Length != 2 ||
        getawayWalls.Select(wall => wall.Parameter1).Distinct().Count() != 2)
    {
        throw new InvalidDataException(
            $"Ceres getaway spawned {getawayWalls.Length} Mode-7 walls instead of native variants five/six.");
    }

    // Continue room main through the retail zoom table's $FFFF terminator and verify the
    // explicit return to ordinary mode-nine rendering state.
    byte[] vramBeforeMode7Animation = ridleyVram.Bytes.ToArray();
    bool sawRotatedMatrix = false;
    bool sawSamusPushOwnership = false;
    bool sawAnimatedMode7Map = false;
    bool sawVisibleMode7Getaway = false;
    bool sawBothMode7WallsDrawn = false;
    int mode7Frames = 0;
    while (ridleyState.Mode7Active && mode7Frames < 512)
    {
        ridleyEnemies.StepFrame(0, 0, timeIsFrozen: false, auditSamus);
        ridleyEnemies.StepEnemyProjectiles(retailRidleyLevel, auditSamus);
        sawRotatedMatrix |= ridleyState.Mode7MatrixB != 0 &&
            ridleyState.Mode7MatrixC != 0;
        sawSamusPushOwnership |= auditSamus.InputLocked;
        sawAnimatedMode7Map |= !ridleyVram.Bytes.SequenceEqual(vramBeforeMode7Animation);
        if (!sawVisibleMode7Getaway)
        {
            var getawayOam = new OamBuffer();
            getawayOam.BeginFrame();
            ridleyEnemies.DrawLayers(getawayOam, cameraX: 0, cameraY: 0, 0, 7);
            // The two literal wall maps contain 19 and 13 entries. This assertion is
            // intentionally about emitted OAM, not merely the existence of logical slots.
            sawBothMode7WallsDrawn |= getawayOam.NextByteOffset >= (19 + 13) * 4;
            getawayOam.FinalizeFrame();
            Rgba32[] mode7Frame = SnesGameplayFrameRenderer.RenderHudCeresRidleyGetawayAndObjs(
                ridleyVram,
                ridleyCgram,
                getawayOam,
                unchecked((short)ridleyState.Mode7MatrixA),
                unchecked((short)ridleyState.Mode7MatrixB),
                unchecked((short)ridleyState.Mode7MatrixC),
                unchecked((short)ridleyState.Mode7MatrixD),
                unchecked((short)ridleyState.Mode7CenterX),
                unchecked((short)ridleyState.Mode7CenterY),
                unchecked((short)ridleyState.Mode7HorizontalOffset),
                unchecked((short)ridleyState.Mode7VerticalOffset),
                ridleyInitialScroll.Bg2HorizontalScroll,
                ridleyInitialScroll.Bg2VerticalScroll,
                bg2CharacterBaseWord: 0x6000);
            sawVisibleMode7Getaway = mode7Frame
                .Skip(SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.HudHeight)
                .Distinct()
                .Take(2)
                .Count() == 2;
        }
        mode7Frames++;
    }
    if (!ridleyState.Mode7Finished || ridleyState.Mode7Active ||
        ridleyState.Mode7MatrixA != 0 || ridleyState.Mode7HorizontalOffset != 0 ||
        !sawRotatedMatrix || !sawSamusPushOwnership || !sawAnimatedMode7Map ||
        !sawVisibleMode7Getaway || !sawBothMode7WallsDrawn ||
        !auditSamus.InputLocked)
    {
        throw new InvalidDataException(
            $"Retail Mode-7 getaway failed to restore mode nine after {mode7Frames} frames: " +
            $"active={ridleyState.Mode7Active}, finished={ridleyState.Mode7Finished}, " +
            $"rotated={sawRotatedMatrix}, pushedSamus={sawSamusPushOwnership}, " +
            $"animatedMap={sawAnimatedMode7Map}, visible={sawVisibleMode7Getaway}, " +
            $"wallsDrawn={sawBothMode7WallsDrawn}, " +
            $"inputLocked={auditSamus.InputLocked}, " +
            $"A=${ridleyState.Mode7MatrixA:X4}, X=${ridleyState.Mode7HorizontalOffset:X4}.");
    }

    // The direct enemy audit intentionally does not run bank-$90 Samus movement, so its
    // push handler remains latched here. Continue the actor-owned $A6:C04E sequence with
    // the same native queue the runtime supplies and prove all fifteen retail DMA records,
    // the 128-frame English warning hold, and the status-two publication.
    var ceresEscapeWrites = new VramWriteQueue();
    int warningSetupFrames = 0;
    while (ridleyState.FunctionTimer != 6 && warningSetupFrames < 16)
    {
        ridleyEnemies.StepFrame(
            0,
            0,
            timeIsFrozen: false,
            auditSamus,
            vramWriteQueue: ceresEscapeWrites);
        warningSetupFrames++;
    }
    if (ceresEscapeWrites.Entries.Count != 15 ||
        warningSetupFrames != 14 ||
        ridleyState.FunctionTimer != 6 ||
        ridleyState.CeresEscapeTextDelayTimer != 128)
    {
        throw new InvalidDataException(
            $"Retail Ceres warning setup queued {ceresEscapeWrites.Entries.Count}/15 records, " +
            $"frames={warningSetupFrames}/14, phase=${ridleyState.FunctionTimer:X4}, " +
            $"hold={ridleyState.CeresEscapeTextDelayTimer}/128.");
    }
    for (int warningFrame = 0; warningFrame < 127; warningFrame++)
    {
        ridleyEnemies.StepFrame(
            0,
            0,
            timeIsFrozen: false,
            auditSamus,
            vramWriteQueue: ceresEscapeWrites);
        if (ridleyEnemies.CeresEscapeStartedThisFrame)
            throw new InvalidDataException($"Retail Ceres escape started early on warning frame {warningFrame}.");
    }
    ridleyEnemies.StepFrame(
        0,
        0,
        timeIsFrozen: false,
        auditSamus,
        vramWriteQueue: ceresEscapeWrites);
    if (!ridleyEnemies.CeresEscapeStartedThisFrame ||
        ridleyEnemies.CeresStatus != 2 ||
        ridleyState.Function != RidleyAiFunction.CeresSelfDestructPaletteOnly)
    {
        throw new InvalidDataException(
            $"Retail Ceres escape handoff failed: event={ridleyEnemies.CeresEscapeStartedThisFrame}, " +
            $"status=${ridleyEnemies.CeresStatus:X4}, function=$A6:{(ushort)ridleyState.Function:X4}.");
    }

    // $A6:DFB2 is shared with the power-bomb-immune Norfair boss, but Ceres Ridley's
    // vulnerability byte reaches it. Keep that fresh-load proof out of this already-large
    // battle fixture so its common-damage timers cannot perturb the Mode-7 sequence above.
    CeresRidleyProjectileAudit.Verify(ridleyBus, ridleyRoom, retailRidleyAssets);
    CeresRidleyPowerBombAudit.Verify(ridleyBus, ridleyRoom, retailRidleyAssets);

    Console.WriteLine(
        $"Ceres Ridley audit passed: room $8F:{ridleyRoom.Pointer:X4}, " +
        $"state $8F:{ridleyRoom.State.Pointer:X4}, " +
        $"population $A1:{ridleyRoom.State.EnemyPopulationPointer:X4}, " +
        $"enemy ${ridleySlot.EnemyDefinitionPointer:X4}, reveal $A6:A455/$0004, " +
        $"pre-reveal Samus-owned overlap " +
        $"({preRevealOverlapPixel % 256},{preRevealOverlapPixel / 256}), " +
        $"battle in {battleEntryFrames} frames, all seven fireball/afterburn definitions in " +
        $"{fireballAuditFrames} frames, runtime hurt OAM {liveContactOamCount}/128, " +
        $"one retail normal-bomb plus 99 beam hits and shared power-bomb damage, " +
        $"escape handoff in {retreatFrames} frames, Mode 7 restored in {mode7Frames} frames, " +
        $"fifteen warning DMAs over {warningSetupFrames} frames and 128-frame self-destruct hold verified.");
    return 0;
}

/// <summary>
/// Slow, explicit Mode-1 gameplay compositor retained only as a pixel-parity oracle for the
/// optimized software PPU benchmark. It intentionally allocates every old priority plane.
/// </summary>
static Rgba32[] RenderOrdinaryGameplayReference(
    SnesVram vram,
    SnesCgram cgram,
    OamBuffer oam,
    ushort bg1HorizontalScroll,
    ushort bg1VerticalScroll,
    ushort bg2HorizontalScroll,
    ushort bg2VerticalScroll,
    ushort bg1CharacterBaseWord,
    ushort bg2CharacterBaseWord)
{
    const int width = SnesGameplayFrameRenderer.Width;
    const int height = SnesGameplayFrameRenderer.Height;
    const int hudHeight = SnesGameplayFrameRenderer.HudHeight;
    var output = new Rgba32[width * height];
    Array.Fill(output, cgram.GetRgba(0));

    void CompositeObj(int priority)
    {
        Rgba32[] plane = SnesObjRenderer.Render(
            oam,
            vram,
            cgram,
            obsel: 0x03,
            width,
            height,
            priority);
        for (int pixel = hudHeight * width; pixel < output.Length; pixel++)
        {
            if (plane[pixel].A != 0)
                output[pixel] = plane[pixel];
        }
    }

    void CompositeBg(
        ushort tilemapBaseWord,
        ushort characterBaseWord,
        ushort horizontalScroll,
        ushort verticalScroll,
        bool priority)
    {
        Rgba32[] plane = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord,
            characterBaseWord,
            horizontalScroll,
            unchecked((ushort)(verticalScroll + hudHeight)),
            width,
            height - hudHeight,
            priority: priority);
        int destinationBase = hudHeight * width;
        for (int pixel = 0; pixel < plane.Length; pixel++)
        {
            if (plane[pixel].A != 0)
                output[destinationBase + pixel] = plane[pixel];
        }
    }

    // This is the literal BGMODE=$09 back-to-front ladder. Keeping the oracle phrased as
    // separate planes makes it structurally independent from the optimized rank selection.
    CompositeObj(priority: 0);
    CompositeObj(priority: 1);
    CompositeBg(0x4800, bg2CharacterBaseWord, bg2HorizontalScroll, bg2VerticalScroll, priority: false);
    CompositeBg(0x5000, bg1CharacterBaseWord, bg1HorizontalScroll, bg1VerticalScroll, priority: false);
    CompositeObj(priority: 2);
    CompositeBg(0x4800, bg2CharacterBaseWord, bg2HorizontalScroll, bg2VerticalScroll, priority: true);
    CompositeBg(0x5000, bg1CharacterBaseWord, bg1HorizontalScroll, bg1VerticalScroll, priority: true);
    CompositeObj(priority: 3);

    Rgba32[] hud = SnesBgTilemapRenderer.Render2Bpp(
        vram,
        cgram,
        tilemapBaseWord: 0x5800,
        characterBaseWord: 0x4000,
        rowCount: 4);
    hud.CopyTo(output, 0);
    return output;
}

static void ResetRidleyAuditSamusAfterHurt(
    ISnesAddressSpace bus,
    SamusState samus)
{
    samus.KnockbackActive = false;
    samus.KnockbackDirection = 0;
    samus.KnockbackTimer = 0;
    samus.InvincibilityTimer = 0;
    samus.Pose = SamusPoseIds.FacingRightNormalPose;
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);
}

// This milestone capture exercises the actual new-game load-station/room pipeline without
// requiring the still-in-progress intro object interpreter to reach its final state first.
// It is diagnostic entry only: the Playable dispatcher will own this same runtime once the
// cinematic transitions to state $06.
if (args.Length >= 3 && args[0] == "--ceres-room-capture")
{
    string ceresRomPath = string.Join(' ', args[1..^1]).Trim('"');
    string ceresOutputPath = args[^1].Trim('"');
    SuperMetroidAddressSpace ceresBus = SuperMetroidAddressSpace.LoadRetailRom(ceresRomPath);
    var ceresRuntime = new SuperMetroidRuntime(ceresBus);

    // Native loading initializes standard HUD/OBJ art before loading the destination room.
    // Drain that first NMI now, then let room/enemy uploads overwrite their reserved OBJ
    // ranges in the same order as StartGameplay_Async.
    ceresRuntime.InitializeHud(HudSnapshot.CeresDebug);
    ceresRuntime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
    InitialViewportResult viewport = ceresRuntime.InitializeStartingCeresRoom();
    ceresRuntime.InitializeCeresStartSamus();
    CartridgeDoorHeader door = ceresRuntime.ActiveDoor!;

    // Keep a phase-separated image beside the requested capture. Room graphics and the
    // initial 17x2 viewport are already resident here, but no queued enemy/Samus transfer
    // has reached VRAM. Comparing this image with the post-NMI result makes an overlapping
    // DMA visually attributable instead of leaving one opaque "Ceres is striped" symptom.
    string ceresPreNmiPath = Path.Combine(
        Path.GetDirectoryName(ceresOutputPath) ?? string.Empty,
        $"{Path.GetFileNameWithoutExtension(ceresOutputPath)}.pre-nmi{Path.GetExtension(ceresOutputPath)}");
    Rgba32[] ceresPreNmiFrame = SuperMetroidRuntimeFrameRenderer.Render(ceresRuntime);
    PngWriter.WriteRgba(ceresPreNmiPath, 256, 224, ceresPreNmiFrame);

    // Sixty wait frames plus seventy-two one-pixel descent frames reproduce the complete
    // bank-$86 arrival. One additional frame publishes the completed OAM through NMI so
    // the requested PNG represents the actual first controllable Ceres frame.
    for (int arrivalFrame = 0; arrivalFrame < 132; arrivalFrame++)
    {
        ceresRuntime.StepFrame(0);

        // Preserve a frame while both bank-$86 elevator projectiles are visible. The final
        // controllable capture cannot catch a wrong level-data-concealer palette because
        // that object deletes itself precisely when Samus lands.
        if (arrivalFrame == 0)
        {
            string arrivalPath = Path.Combine(
                Path.GetDirectoryName(ceresOutputPath) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(ceresOutputPath)}.arrival{Path.GetExtension(ceresOutputPath)}");
            Rgba32[] arrivalPixels = SuperMetroidRuntimeFrameRenderer.Render(ceresRuntime);
            EnsureOpaqueFrame(arrivalPixels, "Ceres elevator arrival");
            PngWriter.WriteRgba(arrivalPath, 256, 224, arrivalPixels);
        }
    }
    if (ceresRuntime.CeresElevatorArrival is not { IsComplete: true } ||
        ceresRuntime.Samus?.YPosition != 72 ||
        !ceresRuntime.GroundedSamusMovementEnabled)
    {
        throw new InvalidOperationException(
            "Ceres elevator did not finish at native Samus Y=$0048 with controls unlocked.");
    }

    CeresProjectileAudit.AssertPowerBeamGraphics(ceresBus, ceresRuntime);

    CartridgeRoomHeader room = ceresRuntime.ActiveRoom!;
    LoadStationEntry station = ceresRuntime.ActiveLoadStation!;
    CartridgeRoomAssets assets = ceresRuntime.ActiveRoomAssets!;
    Rgba32[] ceresFrame = SuperMetroidRuntimeFrameRenderer.Render(ceresRuntime);
    EnsureOpaqueFrame(ceresFrame, "Ceres gameplay");
    PngWriter.WriteRgba(ceresOutputPath, 256, 224, ceresFrame);

    // Both bank-$86 arrival projectiles are gone at this point. The platform must still
    // alternate because Ceres-door variant two owns $A6:F8F1 independently. Cross one
    // complete bit-one phase and compare the literal four map bytes written at word $060E;
    // this is a retail-ROM end-to-end guard against the exact “flashes only while moving”
    // regression that a projectile-only screenshot cannot detect.
    byte[] landedPlatformFrame = Enumerable.Range(0, 4)
        .Select(index => ceresRuntime.Vram.ReadByte((0x060e + index) * 2))
        .ToArray();
    ceresRuntime.StepFrame(0);
    ceresRuntime.StepFrame(0);
    byte[] nextLandedPlatformFrame = Enumerable.Range(0, 4)
        .Select(index => ceresRuntime.Vram.ReadByte((0x060e + index) * 2))
        .ToArray();
    if (landedPlatformFrame.AsSpan().SequenceEqual(nextLandedPlatformFrame))
    {
        throw new InvalidDataException(
            $"Landed Ceres platform stopped animating at Mode-7 word $060E: " +
            $"{Convert.ToHexString(landedPlatformFrame)}.");
    }
    Console.WriteLine(
        $"Ceres station area={station.RequestedAreaIndex} index={station.StationIndex} " +
        $"room=$8F:{room.Pointer:X4} state=$8F:{room.State.Pointer:X4} " +
        $"door=$83:{door.Pointer:X4}/setup=$8F:{door.SetupCodePointer:X4} " +
        $"camera=({station.CameraX:X4},{station.CameraY:X4}) " +
        $"Samus=({station.SamusX:X4},{station.SamusY:X4}) " +
        $"tileset=${room.State.GraphicsSet:X2} layer2=({room.State.Layer2ScrollX:X2},{room.State.Layer2ScrollY:X2}) " +
        $"background=$8F:{room.State.BackgroundDataPointer:X4} setup=$8F:{room.State.SetupCodePointer:X4} " +
        $"viewport={viewport.UpdateRequestCount}/{viewport.DmaSegmentCount}.");
    Console.WriteLine(
        $"Tileset table=$8F:{assets.Tileset.Pointer:X4} " +
        $"defs=${assets.Tileset.BlockDefinitionsAddress:X6} chars=${assets.Tileset.CharacterAddress:X6} " +
        $"palette=${assets.Tileset.PaletteAddress:X6} " +
        $"blocks=${assets.LevelData.BlockDefinitions.Length:X} " +
        $"room-chars=${assets.RoomCharacters.Length:X} CRE-chars=${assets.CreCharacters.Length:X} " +
        $"palette=${assets.PaletteBytes.Length:X} level={assets.LevelData.WidthInBlocks}x{assets.LevelData.HeightInBlocks} blocks.");
    Console.Write("Ceres first level words:");
    foreach (ushort word in assets.LevelData.ForegroundEntries.Span[..16])
        Console.Write($" ${word:X4}");
    Console.WriteLine();
    Console.Write("Ceres first BG1 VRAM words:");
    for (int wordIndex = 0; wordIndex < 16; wordIndex++)
        Console.Write($" ${ceresRuntime.Vram.ReadWord(0x5000 + wordIndex):X4}");
    Console.WriteLine();
    foreach (ushort definitionPointer in new ushort[] { 0xa387, 0xa395 })
    {
        int definitionAddress = 0x860000 | definitionPointer;
        ushort initialization = RomDataReader.ReadWordFixedBank(ceresBus, definitionAddress);
        ushort preInstruction = RomDataReader.ReadWordFixedBank(ceresBus, definitionAddress + 2);
        ushort instructionList = RomDataReader.ReadWordFixedBank(ceresBus, definitionAddress + 4);
        Console.WriteLine(
            $"Ceres eproj $86:{definitionPointer:X4}: init=${initialization:X4} " +
            $"pre=${preInstruction:X4} list=${instructionList:X4} " +
            $"radius=${RomDataReader.ReadWordFixedBank(ceresBus, definitionAddress + 6):X4} " +
            $"properties=${RomDataReader.ReadWordFixedBank(ceresBus, definitionAddress + 8):X4}.");
        Console.Write("  list words:");
        for (int wordIndex = 0; wordIndex < 8; wordIndex++)
        {
            ushort word = RomDataReader.ReadWordFixedBank(
                ceresBus,
                0x860000 | ((instructionList + wordIndex * 2) & 0xffff));
            Console.Write($" ${word:X4}");
        }
        Console.WriteLine();
    }
    Console.WriteLine($"Captured pre-NMI Ceres room to {Path.GetFullPath(ceresPreNmiPath)}.");
    Console.WriteLine($"Captured cartridge-backed starting Ceres room to {Path.GetFullPath(ceresOutputPath)}.");

    CeresProjectileAudit.RunPowerBeamLifetime(ceresRuntime);

    // Reproduce the reported block-639/BTS-$00 path against a separate runtime so this
    // room-capture mode still exports its documented elevator-arrival frame. A one-pixel
    // high probe isolates the exact door row from adjacent cap/terrain rows while retaining
    // the real bank-$94 horizontal dispatcher, room door table, bank-$83 header, and full
    // destination loader. This fails if type $9 is merely treated as air or solid.
    var doorProbeRuntime = new SuperMetroidRuntime(ceresBus);
    doorProbeRuntime.InitializeHud(HudSnapshot.CeresDebug);
    doorProbeRuntime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
    doorProbeRuntime.InitializeStartingCeresRoom();
    doorProbeRuntime.InitializeCeresStartSamus();
    RoomLevelData probeLevel = doorProbeRuntime.LevelData!;
    const int reportedDoorBlockIndex = 639;
    RoomCollisionBlock reportedDoorBlock = probeLevel.GetCollisionBlockByIndex(reportedDoorBlockIndex);
    if (reportedDoorBlock.CollisionType != RoomCollisionType.DoorBlock ||
        reportedDoorBlock.Behavior != 0)
    {
        throw new InvalidDataException(
            $"Expected reported Ceres block 639 to remain type $9/BTS $00, got " +
            $"${(byte)reportedDoorBlock.CollisionType:X1}/${reportedDoorBlock.Behavior:X2}.");
    }

    int reportedDoorX = reportedDoorBlockIndex % probeLevel.WidthInBlocks;
    int reportedDoorY = reportedDoorBlockIndex / probeLevel.WidthInBlocks;
    SamusState doorProbeSamus = doorProbeRuntime.Samus!;
    doorProbeSamus.Pose = SamusPoseIds.MovingRightNormalPose;
    doorProbeSamus.InitializeAnimation(ceresBus);
    doorProbeSamus.Kinematics.XPosition = unchecked((ushort)(reportedDoorX * 16 - 8));
    doorProbeSamus.Kinematics.YPosition = unchecked((ushort)(reportedDoorY * 16 + 8));
    doorProbeSamus.Kinematics.XRadius = 8;
    doorProbeSamus.Kinematics.YRadius = 1;
    SamusBlockCollision.MoveHorizontal(
        ceresBus,
        probeLevel,
        doorProbeSamus.Kinematics,
        displacement: 1 << 16);
    CartridgeDoorHeader selectedDoor = probeLevel.PendingDoorTransition
        ?? throw new InvalidOperationException(
            "Reported Ceres type-$9 collision did not publish its door transition.");
    ushort sourceRoomPointer = doorProbeRuntime.ActiveRoom!.Pointer;
    doorProbeRuntime.LoadPendingDoorDestination();
    CartridgeRoomHeader destinationRoom = doorProbeRuntime.ActiveRoom!;
    CartridgeRoomAssets destinationAssets = doorProbeRuntime.ActiveRoomAssets!;
    ushort destinationRoomPointer = destinationRoom.Pointer;
    if (destinationRoomPointer == sourceRoomPointer)
    {
        throw new InvalidOperationException(
            $"Reported Ceres door $83:{selectedDoor.Pointer:X4} reloaded source room " +
            $"$8F:{sourceRoomPointer:X4}.");
    }
    for (int tilemapWord = 0; tilemapWord < 0x0400; tilemapWord++)
    {
        ushort firstPage = doorProbeRuntime.Vram.ReadWord(0x4800 + tilemapWord);
        ushort secondPage = doorProbeRuntime.Vram.ReadWord(0x4c00 + tilemapWord);
        if (firstPage != secondPage)
        {
            throw new InvalidDataException(
                $"Ceres $8F:{destinationRoomPointer:X4} library BG differs between " +
                $"VRAM pages at word ${tilemapWord:X3}: ${firstPage:X4}/${secondPage:X4}.");
        }
    }
    for (int destinationFrame = 0; destinationFrame < 4; destinationFrame++)
        doorProbeRuntime.StepFrame(0);
    Rgba32[] destinationPixels = SuperMetroidRuntimeFrameRenderer.Render(doorProbeRuntime);
    EnsureOpaqueFrame(destinationPixels, "Ceres door destination");
    string destinationPath = Path.Combine(
        Path.GetDirectoryName(ceresOutputPath) ?? string.Empty,
        $"{Path.GetFileNameWithoutExtension(ceresOutputPath)}.door-destination" +
        Path.GetExtension(ceresOutputPath));
    PngWriter.WriteRgba(destinationPath, 256, 224, destinationPixels);
    Console.WriteLine(
        $"Ceres block 639 door probe: $8F:{sourceRoomPointer:X4} via " +
        $"$83:{selectedDoor.Pointer:X4} -> $8F:{destinationRoomPointer:X4}; " +
        $"state=$8F:{destinationRoom.State.Pointer:X4}, " +
        $"screens={destinationRoom.WidthInScreens}x{destinationRoom.HeightInScreens}, " +
        $"camera=(${doorProbeRuntime.Camera!.XPosition:X4},${doorProbeRuntime.Camera.YPosition:X4}), " +
        $"Samus=(${doorProbeRuntime.Samus!.XPosition:X4},${doorProbeRuntime.Samus.YPosition:X4}), " +
        $"tileset=${destinationRoom.State.GraphicsSet:X2}, " +
        $"layer2=(${destinationRoom.State.Layer2ScrollX:X2},${destinationRoom.State.Layer2ScrollY:X2}).");
    Console.WriteLine(
        $"Destination assets: definitions=${destinationAssets.LevelData.BlockDefinitions.Length:X}, " +
        $"characters=${destinationAssets.RoomCharacters.Length:X}, " +
        $"definition-source=${destinationAssets.Tileset.BlockDefinitionsAddress:X6}, " +
        $"character-source=${destinationAssets.Tileset.CharacterAddress:X6}.");
    Console.Write("Destination first level words:");
    foreach (ushort word in destinationAssets.LevelData.ForegroundEntries.Span[..16])
        Console.Write($" ${word:X4}");
    Console.WriteLine();
    Console.WriteLine($"Captured first Ceres door destination to {Path.GetFullPath(destinationPath)}.");
    return 0;
}

static void EnsureOpaqueFrame(ReadOnlySpan<Rgba32> pixels, string frameName)
{
    for (int pixel = 0; pixel < pixels.Length; pixel++)
    {
        if (pixels[pixel].A != byte.MaxValue)
        {
            throw new InvalidDataException(
                $"{frameName} exported alpha ${pixels[pixel].A:X2} at pixel {pixel}; " +
                "palette-index-zero keys must be resolved against the SNES backdrop.");
        }
    }
}

// Front-end capture is a deliberately small alternate entry point for visual regression.
// It runs before the gameplay runner's large option parser so title/menu work does not need
// fake room initialization merely to produce a PNG. The frame itself still comes only from
// the cartridge-backed dispatcher used by the WinForms Playable tab.
if (args.Length >= 3 && args[0] is
    "--frontend-capture" or "--frontend-title-capture" or
    "--frontend-options-capture" or "--frontend-intro-capture" or
    "--frontend-mother-brain-capture" or "--frontend-mother-brain-action-capture" or
    "--frontend-mother-brain-explosion-capture" or "--frontend-page-two-capture" or
    "--frontend-baby-discovery-capture" or "--frontend-egg-hatching-capture" or
    "--frontend-egg-particles-capture" or "--frontend-page-three-capture" or
    "--frontend-delivery-capture" or "--frontend-page-four-capture" or
    "--frontend-examination-capture" or "--frontend-page-five-capture" or
    "--frontend-page-six-capture" or "--frontend-ceres-approach-capture" or
    "--frontend-ceres-rear-view-capture" or "--frontend-ceres-title-capture" or
    "--frontend-playable-ceres-capture")
{
    bool captureTitle = args[0] == "--frontend-title-capture";
    bool captureOptions = args[0] == "--frontend-options-capture";
    bool captureIntro = args[0] is
        "--frontend-intro-capture" or
        "--frontend-mother-brain-capture" or
        "--frontend-mother-brain-action-capture" or
        "--frontend-mother-brain-explosion-capture" or
        "--frontend-page-two-capture" or
        "--frontend-baby-discovery-capture" or
        "--frontend-egg-hatching-capture" or
        "--frontend-egg-particles-capture" or
        "--frontend-page-three-capture" or
        "--frontend-delivery-capture" or
        "--frontend-page-four-capture";
    bool captureMotherBrain = args[0] is
        "--frontend-mother-brain-capture" or
        "--frontend-mother-brain-action-capture" or
        "--frontend-mother-brain-explosion-capture" or
        "--frontend-page-two-capture" or
        "--frontend-baby-discovery-capture" or
        "--frontend-egg-hatching-capture" or
        "--frontend-egg-particles-capture" or
        "--frontend-page-three-capture" or
        "--frontend-delivery-capture" or
        "--frontend-page-four-capture";
    bool captureMotherBrainAction = args[0] is
        "--frontend-mother-brain-action-capture" or
        "--frontend-mother-brain-explosion-capture" or
        "--frontend-page-two-capture" or
        "--frontend-baby-discovery-capture" or
        "--frontend-egg-hatching-capture" or
        "--frontend-egg-particles-capture" or
        "--frontend-page-three-capture" or
        "--frontend-delivery-capture" or
        "--frontend-page-four-capture";
    bool captureMotherBrainExplosion = args[0] == "--frontend-mother-brain-explosion-capture";
    bool capturePageTwo = args[0] is
        "--frontend-page-two-capture" or
        "--frontend-baby-discovery-capture" or
        "--frontend-egg-hatching-capture" or
        "--frontend-egg-particles-capture" or
        "--frontend-page-three-capture" or
        "--frontend-delivery-capture" or
        "--frontend-page-four-capture";
    bool captureBabyDiscovery = args[0] is
        "--frontend-baby-discovery-capture" or
        "--frontend-egg-hatching-capture" or
        "--frontend-egg-particles-capture" or
        "--frontend-page-three-capture" or
        "--frontend-delivery-capture" or
        "--frontend-page-four-capture";
    bool captureEggHatching = args[0] is
        "--frontend-egg-hatching-capture" or
        "--frontend-egg-particles-capture" or
        "--frontend-page-three-capture" or
        "--frontend-delivery-capture" or
        "--frontend-page-four-capture";
    bool captureEggParticles = args[0] is
        "--frontend-egg-particles-capture" or
        "--frontend-page-three-capture" or
        "--frontend-delivery-capture" or
        "--frontend-page-four-capture";
    bool capturePageThree = args[0] is
        "--frontend-page-three-capture" or
        "--frontend-delivery-capture" or
        "--frontend-page-four-capture";
    bool captureDelivery = args[0] is
        "--frontend-delivery-capture" or
        "--frontend-page-four-capture";
    bool capturePageFour = args[0] == "--frontend-page-four-capture";
    bool captureExamination = args[0] is
        "--frontend-examination-capture" or
        "--frontend-page-five-capture" or
        "--frontend-page-six-capture" or
        "--frontend-ceres-approach-capture" or
        "--frontend-ceres-rear-view-capture" or
        "--frontend-ceres-title-capture" or
        "--frontend-playable-ceres-capture";
    bool capturePageFive = args[0] is
        "--frontend-page-five-capture" or
        "--frontend-page-six-capture" or
        "--frontend-ceres-approach-capture" or
        "--frontend-ceres-rear-view-capture" or
        "--frontend-ceres-title-capture" or
        "--frontend-playable-ceres-capture";
    bool capturePageSix = args[0] is
        "--frontend-page-six-capture" or
        "--frontend-ceres-approach-capture" or
        "--frontend-ceres-rear-view-capture" or
        "--frontend-ceres-title-capture" or
        "--frontend-playable-ceres-capture";
    bool captureCeresApproach = args[0] is
        "--frontend-ceres-approach-capture" or
        "--frontend-ceres-rear-view-capture" or
        "--frontend-ceres-title-capture" or
        "--frontend-playable-ceres-capture";
    bool captureCeresRearView = args[0] is
        "--frontend-ceres-rear-view-capture" or
        "--frontend-ceres-title-capture" or
        "--frontend-playable-ceres-capture";
    bool captureCeresTitle = args[0] is
        "--frontend-ceres-title-capture" or
        "--frontend-playable-ceres-capture";
    bool capturePlayableCeres = args[0] == "--frontend-playable-ceres-capture";
    if (captureExamination)
    {
        // Deeper captures traverse every earlier state through the same dispatcher path.
        captureIntro = true;
        captureMotherBrain = true;
        captureMotherBrainAction = true;
        capturePageTwo = true;
        captureBabyDiscovery = true;
        captureEggHatching = true;
        captureEggParticles = true;
        capturePageThree = true;
        captureDelivery = true;
        capturePageFour = true;
    }
    // Rejoining the middle tokens also tolerates minimal Windows command hosts that strip
    // quotes around the conventional "Super Metroid.smc" filename before `dotnet run`.
    string frontendRomPath = string.Join(' ', args[1..^1]).Trim('"');
    SuperMetroidAddressSpace frontendBus = SuperMetroidAddressSpace.LoadRetailRom(frontendRomPath);
    var frontend = new SuperMetroidGame(frontendBus);
    FrontendFrame frontendFrame = frontend.Step(0);

    // A fresh Start edge skips the opening montage, release allows the fast transition to
    // finish, then a second edge starts file select. Bounds make a broken transition fail in
    // the CLI rather than spin forever or quietly capture the wrong screen.
    frontendFrame = frontend.Step((ushort)SnesButton.Start);
    for (int frame = 0; frame < 120 && frontendFrame.Phase != nameof(TitleSequencePhase.TitleScreen); frame++)
        frontendFrame = frontend.Step(0);
    if (frontendFrame.Phase != nameof(TitleSequencePhase.TitleScreen))
        throw new InvalidOperationException("Title skip did not reach TitleScreen within 120 frames.");

    if (!captureTitle)
    {
        frontendFrame = frontend.Step((ushort)SnesButton.Start);
        for (int frame = 0; frame < 120 && frontendFrame.GameState != SuperMetroidGameState.FileSelectMenus; frame++)
            frontendFrame = frontend.Step(0);
        for (int frame = 0; frame < 16; frame++)
            frontendFrame = frontend.Step(0);

        if (captureOptions || captureIntro)
        {
            frontendFrame = frontend.Step((ushort)SnesButton.A);
            for (int frame = 0; frame < 180 && frontendFrame.GameState != SuperMetroidGameState.GameOptionsMenu; frame++)
                frontendFrame = frontend.Step(0);
            if (frontendFrame.GameState != SuperMetroidGameState.GameOptionsMenu)
                throw new InvalidOperationException("Fresh slot did not reach GameOptionsMenu within 180 frames.");
            for (int frame = 0; frame < 16; frame++)
                frontendFrame = frontend.Step(0);

            if (captureIntro)
            {
                frontendFrame = frontend.Step((ushort)SnesButton.A);
                for (int frame = 0; frame < 120 && frontendFrame.GameState != SuperMetroidGameState.IntroCinematic; frame++)
                    frontendFrame = frontend.Step(0);
                if (frontendFrame.GameState != SuperMetroidGameState.IntroCinematic)
                    throw new InvalidOperationException("Options did not reach IntroCinematic within 120 frames.");
                for (int frame = 0; frame < 1_200 && frontendFrame.Phase != nameof(IntroCinematicPhase.PageOneText); frame++)
                    frontendFrame = frontend.Step(0);
                if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageOneText))
                    throw new InvalidOperationException("Intro did not reach the first illustrated text page within 1,200 frames.");
                for (int frame = 0; frame < 1_000 && frontendFrame.Phase == nameof(IntroCinematicPhase.PageOneText); frame++)
                    frontendFrame = frontend.Step(0);
                if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageOneAwaitingInput))
                    throw new InvalidOperationException("Intro page-one text script did not reach its native input wait within 1,000 frames.");

                if (captureMotherBrain)
                {
                    // One fresh key edge invokes $8B:AEB8. The transition routine then
                    // observes counters 127 through zero and completes on the decrement
                    // from zero to $FFFF, which requires exactly 128 subsequent frames.
                    frontendFrame = frontend.Step((ushort)SnesButton.A);
                    if (frontendFrame.Phase != nameof(IntroCinematicPhase.MotherBrainCrossfade))
                        throw new InvalidOperationException("Page-one input did not start the Mother Brain crossfade.");
                    for (int frame = 0; frame < 128; frame++)
                        frontendFrame = frontend.Step(0);
                    if (frontendFrame.Phase != nameof(IntroCinematicPhase.MotherBrainFlashback))
                        throw new InvalidOperationException("Mother Brain crossfade did not complete after its native 128-frame counter.");

                    if (captureMotherBrainAction)
                    {
                        // Do not estimate where the fourth shot ought to land. Run the live
                        // ROM-authored demo until the cinematic actor itself records all four
                        // impacts. The 720-frame ceiling is comfortably beyond both adjacent
                        // demo lists and turns a broken producer/collision seam into a useful
                        // CLI failure instead of an infinite visual-capture loop.
                        for (int frame = 0; frame < 720 && frontend.IntroMotherBrainHitCount < 4; frame++)
                            frontendFrame = frontend.Step(0);
                        if (frontend.IntroMotherBrainHitCount != 4)
                        {
                            throw new InvalidOperationException(
                                $"Mother Brain accepted {frontend.IntroMotherBrainHitCount} of four scripted missile hits; " +
                                $"{frontend.IntroActiveProjectileCount} ordinary projectile slots remain active.");
                        }
                        if (frontend.IntroMotherBrainExplosionCount != 8)
                        {
                            throw new InvalidOperationException(
                                $"The fourth hit spawned {frontend.IntroMotherBrainExplosionCount} of eight native explosion actors.");
                        }

                        if (captureMotherBrainExplosion)
                        {
                            // Forty-eight subsequent object-handler calls expose all three
                            // small actors and four of the five deliberately staggered big
                            // actors. This catches list decoding, blank-frame looping, and
                            // OAM insertion while remaining well before page two at frame 128.
                            for (int frame = 0; frame < 48; frame++)
                                frontendFrame = frontend.Step(0);
                            if (frontend.IntroMotherBrainExplosionCount != 8)
                                throw new InvalidOperationException("A Mother Brain explosion actor vanished before page two began.");
                        }

                        if (capturePageTwo)
                        {
                            // The fourth-hit state lasts 128 frames, then page two's reverse
                            // palette transition lasts another 128. Use named phases instead
                            // of baking that sum into the assertion so either handoff reports
                            // its own failure point in the debugger.
                            for (int frame = 0; frame < 400 &&
                                frontendFrame.Phase != nameof(IntroCinematicPhase.PageTwoText); frame++)
                            {
                                frontendFrame = frontend.Step(0);
                            }
                            if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageTwoText))
                                throw new InvalidOperationException("Mother Brain did not crossfade into intro page two within 400 frames.");

                            for (int frame = 0; frame < 1_200 &&
                                frontendFrame.Phase == nameof(IntroCinematicPhase.PageTwoText); frame++)
                            {
                                frontendFrame = frontend.Step(0);
                            }
                            if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageTwoAwaitingInput))
                                throw new InvalidOperationException("Intro page-two text did not reach its ROM-authored input wait.");

                            if (captureBabyDiscovery)
                            {
                                frontendFrame = frontend.Step((ushort)SnesButton.A);
                                if (frontendFrame.Phase != nameof(IntroCinematicPhase.BabyDiscoveryCrossfade))
                                    throw new InvalidOperationException("Page-two input did not set up the SR388 discovery crossfade.");
                                for (int frame = 0; frame < 128; frame++)
                                    frontendFrame = frontend.Step(0);
                                if (frontendFrame.Phase != nameof(IntroCinematicPhase.BabyDiscovery))
                                    throw new InvalidOperationException("SR388 discovery crossfade did not complete after 128 frames.");
                                if (frontend.IntroBabyDiscoverySamusX >= 0x0178)
                                    throw new InvalidOperationException("The SR388 ROM demo did not move Samus left during its crossfade.");

                                if (captureEggHatching)
                                {
                                    for (int frame = 0; frame < 240 &&
                                        !frontend.IntroBabyDiscoveryEggHatchingStarted; frame++)
                                    {
                                        frontendFrame = frontend.Step(0);
                                    }
                                    if (!frontend.IntroBabyDiscoveryEggHatchingStarted)
                                    {
                                        throw new InvalidOperationException(
                                            $"Samus stopped at world X ${frontend.IntroBabyDiscoverySamusX:X4} without triggering the egg.");
                                    }
                                    for (int frame = 0; frame < 16; frame++)
                                        frontendFrame = frontend.Step(0);

                                    if (captureEggParticles)
                                    {
                                        // Continue until opcode $A918—not a host frame
                                        // estimate—has actually allocated all six slots.
                                        for (int frame = 0; frame < 320 &&
                                            frontend.IntroBabyDiscoveryEggParticleCount == 0; frame++)
                                        {
                                            frontendFrame = frontend.Step(0);
                                        }
                                        if (frontend.IntroBabyDiscoveryEggParticleCount != 6)
                                        {
                                            throw new InvalidOperationException(
                                                $"Egg burst produced {frontend.IntroBabyDiscoveryEggParticleCount} of six shell fragments.");
                                        }

                                        // Eight gravity updates separate the pieces clearly
                                        // while all six are still above the $A8 deletion line.
                                        for (int frame = 0; frame < 8; frame++)
                                            frontendFrame = frontend.Step(0);

                                        if (capturePageThree)
                                        {
                                            // The open-shell record lasts $140 frames before
                                            // $B33E requests page three; its reverse palette
                                            // crossfade then runs the shared 128-frame counter.
                                            for (int frame = 0; frame < 720 &&
                                                frontendFrame.Phase != nameof(IntroCinematicPhase.PageThreeText); frame++)
                                            {
                                                frontendFrame = frontend.Step(0);
                                            }
                                            if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageThreeText))
                                                throw new InvalidOperationException("The hatched egg did not crossfade into intro page three.");

                                            for (int frame = 0; frame < 1_600 &&
                                                frontendFrame.Phase == nameof(IntroCinematicPhase.PageThreeText); frame++)
                                            {
                                                frontendFrame = frontend.Step(0);
                                            }
                                            if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageThreeAwaitingInput))
                                                throw new InvalidOperationException("Intro page-three text did not reach its ROM-authored input wait.");

                                            if (captureDelivery)
                                            {
                                                frontendFrame = frontend.Step((ushort)SnesButton.A);
                                                if (frontendFrame.Phase != nameof(IntroCinematicPhase.BabyMetroidDeliveryCrossfade))
                                                    throw new InvalidOperationException("Page-three input did not start the Ceres delivery crossfade.");
                                                for (int frame = 0; frame < 128; frame++)
                                                    frontendFrame = frontend.Step(0);
                                                if (frontendFrame.Phase != nameof(IntroCinematicPhase.BabyMetroidDelivery))
                                                    throw new InvalidOperationException("The Ceres delivery crossfade did not complete after 128 frames.");

                                                if (capturePageFour)
                                                {
                                                    for (int frame = 0; frame < 500 &&
                                                        frontendFrame.Phase != nameof(IntroCinematicPhase.PageFourText); frame++)
                                                    {
                                                        frontendFrame = frontend.Step(0);
                                                    }
                                                    if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageFourText))
                                                        throw new InvalidOperationException("The delivery actor did not crossfade into intro page four.");

                                                    for (int frame = 0; frame < 1_600 &&
                                                        frontendFrame.Phase == nameof(IntroCinematicPhase.PageFourText); frame++)
                                                    {
                                                        frontendFrame = frontend.Step(0);
                                                    }
                                                    if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageFourAwaitingInput))
                                                        throw new InvalidOperationException("Intro page-four text did not reach its ROM-authored input wait.");

                                                    if (captureExamination)
                                                    {
                                                        frontendFrame = frontend.Step((ushort)SnesButton.A);
                                                        if (frontendFrame.Phase != nameof(IntroCinematicPhase.BabyMetroidExaminationCrossfade))
                                                            throw new InvalidOperationException("Page-four input did not start the examination crossfade.");
                                                        for (int frame = 0; frame < 128; frame++)
                                                            frontendFrame = frontend.Step(0);
                                                        if (frontendFrame.Phase != nameof(IntroCinematicPhase.BabyMetroidExamination))
                                                            throw new InvalidOperationException("The examination crossfade did not complete after 128 frames.");

                                                        if (capturePageFive)
                                                        {
                                                            for (int frame = 0; frame < 500 &&
                                                                frontendFrame.Phase != nameof(IntroCinematicPhase.PageFiveText); frame++)
                                                            {
                                                                frontendFrame = frontend.Step(0);
                                                            }
                                                            if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageFiveText))
                                                                throw new InvalidOperationException("The examination actor did not crossfade into intro page five.");

                                                            for (int frame = 0; frame < 1_600 &&
                                                                frontendFrame.Phase == nameof(IntroCinematicPhase.PageFiveText); frame++)
                                                            {
                                                                frontendFrame = frontend.Step(0);
                                                            }
                                                            if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageFiveAwaitingInput))
                                                                throw new InvalidOperationException("Intro page-five text did not reach its ROM-authored input wait.");

                                                            if (capturePageSix)
                                                            {
                                                                frontendFrame = frontend.Step((ushort)SnesButton.A);
                                                                if (frontendFrame.Phase != nameof(IntroCinematicPhase.PageSixText))
                                                                    throw new InvalidOperationException("Page-five input did not start final intro page six.");
                                                                for (int frame = 0; frame < 800 &&
                                                                    frontendFrame.Phase == nameof(IntroCinematicPhase.PageSixText); frame++)
                                                                {
                                                                    frontendFrame = frontend.Step(0);
                                                                }
                                                                if (frontendFrame.Phase != nameof(IntroCinematicPhase.IntroFadeOut))
                                                                    throw new InvalidOperationException("Intro page six did not reach its automatic finish opcode.");

                                                                if (captureCeresApproach)
                                                                {
                                                                    // Follow the live narration fade into $8B:BCA0. Once the
                                                                    // outer dispatcher reports CeresFlight, fourteen native
                                                                    // music-delay frames plus eight zoom frames put the front
                                                                    // view visibly in motion without crossing into the rear map.
                                                                    for (int frame = 0; frame < 64 &&
                                                                        frontendFrame.Phase != nameof(IntroCinematicPhase.CeresFlight); frame++)
                                                                    {
                                                                        frontendFrame = frontend.Step(0);
                                                                    }
                                                                    if (frontendFrame.Phase != nameof(IntroCinematicPhase.CeresFlight))
                                                                        throw new InvalidOperationException("The final narration fade did not install the Ceres flight state.");
                                                                    // The front capture stops eight zoom calls after display
                                                                    // enable. The rear capture crosses the remaining front-map
                                                                    // counter and then runs long enough for the 31-step fixed-
                                                                    // white flash to reach black around the Ceres/asteroid shot.
                                                                    int ceresCaptureFrames = captureCeresRearView ? 84 : 22;
                                                                    for (int frame = 0; frame < ceresCaptureFrames; frame++)
                                                                        frontendFrame = frontend.Step(0);
                                                                    if (frontendFrame.Phase != nameof(IntroCinematicPhase.CeresFlight))
                                                                        throw new InvalidOperationException("The Ceres front-view zoom ended before its ROM counter allowed.");

                                                                    if (captureCeresTitle)
                                                                    {
                                                                        // The outer game state stays $1E throughout the flight.
                                                                        // Wait on the translated inner function, then let `$8C:D629`
                                                                        // publish the remaining ten English letters at $10 frames each.
                                                                        for (int frame = 0; frame < 500 &&
                                                                            frontend.IntroCeresFlightPhaseName != "SpaceColonyTitle"; frame++)
                                                                        {
                                                                            frontendFrame = frontend.Step(0);
                                                                        }
                                                                        if (frontend.IntroCeresFlightPhaseName != "SpaceColonyTitle")
                                                                            throw new InvalidOperationException("The Ceres rear-view zoom did not reach the SPACE COLONY title.");
                                                                        for (int frame = 0; frame < 0x10 * 10; frame++)
                                                                            frontendFrame = frontend.Step(0);

                                                                        if (capturePlayableCeres)
                                                                        {
                                                                            // Finish the title hold/fade, execute state $1F,
                                                                            // and allow both bank-$86 elevator actors to unlock
                                                                            // the ordinary state-$08 controller before capture.
                                                                            for (int frame = 0; frame < 500 &&
                                                                                frontendFrame.GameState != SuperMetroidGameState.MainGameplay; frame++)
                                                                            {
                                                                                frontendFrame = frontend.Step(0);
                                                                            }
                                                                            if (frontendFrame.GameState != SuperMetroidGameState.MainGameplay)
                                                                                throw new InvalidOperationException("The complete frontend path did not unlock playable Ceres within 500 frames.");

                                                                            // A state-$08 label alone is not evidence of playability.
                                                                            // Hold the real SNES Right bit through the shared runtime and
                                                                            // require cartridge collision/movement to change world X.
                                                                            ushort controllableStartX = frontend.GameplaySamusX;
                                                                            // Twelve frames are long enough to clear the front-view turn
                                                                            // and produce real X displacement, but keep Samus on the narrow
                                                                            // elevator platform so the following reversal does not become a
                                                                            // simultaneous walked-off-floor test.
                                                                            for (int frame = 0; frame < 12; frame++)
                                                                            {
                                                                                frontendFrame = frontend.Step((ushort)SnesButton.Right);
                                                                            }
                                                                            if (!frontend.GameplayMovementEnabled ||
                                                                                frontend.GameplaySamusX == controllableStartX)
                                                                            {
                                                                                throw new InvalidOperationException(
                                                                                    $"Playable Ceres did not move Samus from X=${controllableStartX:X4}; " +
                                                                                    $"current=(${frontend.GameplaySamusX:X4},${frontend.GameplaySamusY:X4}).");
                                                                            }

                                                                            ushort afterRightX = frontend.GameplaySamusX;

                                                                            // Reversal is a separate native path: running-right first
                                                                            // carries its old momentum through `$25`, then `$F8` installs
                                                                            // left-facing standing and held Left begins pose `$0A`. Give the
                                                                            // real animation/momentum enough frames to cross back past the
                                                                            // Right-only endpoint, and reject a renderer-only pose change.
                                                                            for (int frame = 0; frame < 48; frame++)
                                                                            {
                                                                                frontendFrame = frontend.Step((ushort)SnesButton.Left);
                                                                            }
                                                                            if (frontend.GameplaySamusX >= afterRightX)
                                                                            {
                                                                                throw new InvalidOperationException(
                                                                                    $"Playable Ceres reversal did not move left from X=${afterRightX:X4}; " +
                                                                                    $"current X=${frontend.GameplaySamusX:X4}, pose=${frontend.GameplaySamusPose:X2}.");
                                                                            }

                                                                            // Release Left so running momentum reaches its ROM no-input
                                                                            // fallback, then send a fresh Jump edge. Track the entire early
                                                                            // arc instead of sampling one guessed frame: at least one frame
                                                                            // must put Samus's world center above the grounded baseline.
                                                                            for (int frame = 0; frame < 24; frame++)
                                                                                frontendFrame = frontend.Step(0);
                                                                            ushort groundedY = frontend.GameplaySamusY;
                                                                            ushort minimumJumpY = groundedY;
                                                                            for (int frame = 0; frame < 24; frame++)
                                                                            {
                                                                                frontendFrame = frontend.Step((ushort)SnesButton.A);
                                                                                minimumJumpY = Math.Min(minimumJumpY, frontend.GameplaySamusY);
                                                                            }
                                                                            if (minimumJumpY >= groundedY)
                                                                            {
                                                                                throw new InvalidOperationException(
                                                                                    $"Playable Ceres jump never rose above Y=${groundedY:X4}; " +
                                                                                    $"minimum=${minimumJumpY:X4}, pose=${frontend.GameplaySamusPose:X2}.");
                                                                            }

                                                                            // Let the arc return to the left ledge, then walk toward the
                                                                            // central shaft only until gravity has committed Samus to the
                                                                            // drop. Releasing Right at that point prevents air control from
                                                                            // carrying her across to the matching ledge on the other side.
                                                                            for (int frame = 0; frame < 180; frame++)
                                                                                frontendFrame = frontend.Step(0);
                                                                            ushort ledgeY = frontend.GameplaySamusY;
                                                                            bool leftLedgeCleared = false;
                                                                            for (int frame = 0; frame < 120; frame++)
                                                                            {
                                                                                frontendFrame = frontend.Step((ushort)SnesButton.Right);
                                                                                if (frontend.GameplaySamusY > ledgeY + 8)
                                                                                {
                                                                                    leftLedgeCleared = true;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (!leftLedgeCleared)
                                                                            {
                                                                                throw new InvalidOperationException(
                                                                                    $"Playable Ceres did not leave the lower ledge at Y=${ledgeY:X4}; " +
                                                                                    $"current=(${frontend.GameplaySamusX:X4},${frontend.GameplaySamusY:X4}).");
                                                                            }

                                                                            // `$80:A9DE/$80:AB78` suppress ordinary row/column producers
                                                                            // while Ceres's Mode-7 IRQ flag is active. Continue across many
                                                                            // 16-pixel camera boundaries and prove that every one remains
                                                                            // suppressed; this is the same gate that prevents the eventual
                                                                            // bottom-edge source-row-$002F request reported by the desktop
                                                                            // host. Also prove live M7 scroll follows the camera used by OAM.
                                                                            ushort fallStartY = frontend.GameplaySamusY;
                                                                            ushort fallStartCameraY = frontend.GameplayCameraY;
                                                                            ushort maximumFallY = fallStartY;
                                                                            ushort maximumCameraY = fallStartCameraY;
                                                                            for (int frame = 0; frame < 360; frame++)
                                                                            {
                                                                                frontendFrame = frontend.Step(0);
                                                                                maximumFallY = Math.Max(maximumFallY, frontend.GameplaySamusY);
                                                                                maximumCameraY = Math.Max(maximumCameraY, frontend.GameplayCameraY);
                                                                                if (frontend.GameplayBackgroundUpdateCount != 0)
                                                                                {
                                                                                    throw new InvalidOperationException(
                                                                                        "Ceres Mode 7 emitted an ordinary BG tilemap update while falling.");
                                                                                }
                                                                                if (frontend.GameplayBg1VerticalScroll != frontend.GameplayCameraY)
                                                                                {
                                                                                    throw new InvalidOperationException(
                                                                                        $"Ceres M7VOFS ${frontend.GameplayBg1VerticalScroll:X4} " +
                                                                                        $"diverged from camera Y ${frontend.GameplayCameraY:X4}.");
                                                                                }
                                                                            }
                                                                            if (maximumFallY <= fallStartY || maximumCameraY <= fallStartCameraY)
                                                                            {
                                                                                throw new InvalidOperationException(
                                                                                    $"Playable Ceres fall did not advance world and camera Y: " +
                                                                                    $"Samus ${fallStartY:X4}->${maximumFallY:X4}, " +
                                                                                    $"camera ${fallStartCameraY:X4}->${maximumCameraY:X4}.");
                                                                            }

                                                                            // Fire the desktop host's documented S/SNES-X action through
                                                                            // the same dispatcher used by interactive play. A new slot is
                                                                            // stronger evidence than a cannon/body animation: it proves
                                                                            // keyboard-bit semantics reached `$90:B80D/$90:B986` and that
                                                                            // the bank-$93 projectile object survived its creation frame.
                                                                            frontendFrame = frontend.Step((ushort)SnesButton.X);
                                                                            if (frontend.GameplayLastFiredProjectileSlot is null ||
                                                                                frontend.GameplayProjectileCount == 0)
                                                                            {
                                                                                throw new InvalidOperationException(
                                                                                    "Playable Ceres Shoot did not allocate a power-beam projectile.");
                                                                            }
                                                                            int firedSlot = frontend.GameplayLastFiredProjectileSlot.Value;
                                                                            // Power/ice/wave slot zero intentionally flickers on odd NMIs.
                                                                            // Four releases leave this deterministic capture on its visible
                                                                            // phase while still keeping the shot near Samus for inspection.
                                                                            for (int frame = 0; frame < 4; frame++)
                                                                                frontendFrame = frontend.Step(0);

                                                                            Console.WriteLine(
                                                                                $"Playable input smoke: X ${controllableStartX:X4} -> " +
                                                                                $"${afterRightX:X4} -> ${frontend.GameplaySamusX:X4}; " +
                                                                                $"jump Y ${groundedY:X4} -> ${minimumJumpY:X4}; " +
                                                                                $"fall Y ${fallStartY:X4} -> ${maximumFallY:X4}; " +
                                                                                $"camera Y ${fallStartCameraY:X4} -> ${maximumCameraY:X4}; " +
                                                                                $"beam slot {firedSlot}, count {frontend.GameplayProjectileCount}; " +
                                                                                $"pose ${frontend.GameplaySamusPose:X2}.");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    string frontendOutputPath = args[^1].Trim('"');
    EnsureOpaqueFrame(frontendFrame.Pixels, $"{frontendFrame.GameState}/{frontendFrame.Phase}");
    PngWriter.WriteRgba(frontendOutputPath, FrontendFrame.Width, FrontendFrame.Height, frontendFrame.Pixels);
    Console.WriteLine($"Captured {frontendFrame.GameState}/{frontendFrame.Phase} to {Path.GetFullPath(frontendOutputPath)}.");
    return 0;
}

DebugRunnerOptions options = DebugRunnerOptions.Parse(args);
SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(options.RomPath);
var runtime = new SuperMetroidRuntime(bus);
runtime.MoonwalkEnabled = options.MoonwalkScript;

Console.WriteLine($"Loaded {Path.GetFullPath(options.RomPath)} ({bus.Rom.Length:N0} bytes).");
Console.WriteLine($"Stepping {options.FrameCount:N0} translated frames using {options.TimerScenario} timer startup.");
if (options.ReversalScript)
{
    Console.WriteLine(
        "Input script: Start, release, Right for 60 frames, Left for 60, Right for 60, then release.");
}
else if (options.MoonwalkScript)
{
    Console.WriteLine(
        "Input script: enable Moonwalk, walk backward right-facing through neutral/up/down aim, release, re-enter, then execute the $BF -> $1A jump route.");
}
else if (options.RanIntoWallScript)
{
    Console.WriteLine(
        "Input script: press into a ROM-authored solid wall, change wall-stop aim up/down, release to neutral, then jump away through $4B.");
}
else if (options.ShinesparkScript)
{
    Console.WriteLine(
        "Input script: equip Speed Booster, charge stage four on ROM-authored terrain, crouch to store shine, jump into $C7 windup, then launch right through $C9.");
}
else if (options.SpeedBoosterScript)
{
    Console.WriteLine(
        "Input script: equip Speed Booster, hold Right+Dash through ROM-authored acceleration stages, jump with the accumulated boost bonus, then observe both cancellation echoes return to Samus.");
}
else if (options.ScrewAttackScript && options.WaterSpaceJumpScript)
{
    Console.WriteLine(
        "Input script: combine the water and Screw routes, reach ROM frame 27 while the bottom boundary remains submerged, and prove the normal suit palette plus both cycle words stay frozen.");
}
else if (options.ScrewAttackScript)
{
    Console.WriteLine(
        "Input script: equip Space Jump and Screw Attack, enter ROM pose $81, then issue fresh Jump edges only inside the native falling-speed window while observing contact damage and the late-frame palette cycle.");
}
else if (options.WaterSpaceJumpScript)
{
    Console.WriteLine(
        "Input script: host-place a water FX surface through Samus's center, equip Space Jump, launch with ROM water physics, then pulse Jump inside the native $0080..$04FF partial-submersion window.");
}
else if (options.SpaceJumpScript)
{
    Console.WriteLine(
        "Input script: equip Space Jump, enter ROM pose $1B, then issue fresh Jump edges only while the live falling speed is inside the native $0280..$04FF window.");
}
else if (options.RunScript)
{
    Console.WriteLine(
        "Input script: hold Right+Dash through the ordinary 2.0000 cap, carry that exact extra component into a spin jump, release Jump, then coast to landing.");
}
else if (options.JumpScript)
{
    Console.WriteLine(
        "Input script: Start, short neutral jump, run right, full spin jump, then release.");
}
else if (options.LandingImpactScript)
{
    Console.WriteLine(
        "Input script: run the ordinary short-jump route while host-selecting Norfair's native landing-FX area handler; terrain, collision, velocity, particles, sound IDs, art, and OAM remain ROM-backed.");
}
else if (options.PostureScript)
{
    Console.WriteLine(
        "Input script: crouch/stand right, turn left, crouch/stand left, then release.");
}
else if (options.AimScript)
{
    Console.WriteLine(
        "Input script: straight/up-diagonal/down-diagonal aim right, turn left, then repeat.");
}
else if (options.AimRunScript)
{
    Console.WriteLine(
        "Input script: aimed running up/down right, turn, aimed running up/down left, then release.");
}
else if (options.AimAirScript)
{
    Console.WriteLine(
        "Input script: aimed up/down normal jumps right, turn left, repeat, then release.");
}
else if (options.GunExtendedScript)
{
    Console.WriteLine(
        "Input script: fire live power beams while running through $0B, during a neutral jump through $13/$E6, then across the documented walk-off seam through $67/$E6.");
}
else if (options.ChargeBeamScript)
{
    Console.WriteLine(
        "Input script: equip Charge Beam, hold Shoot through the native 60-frame arming threshold and three-component muzzle flare, then release one charged power shot.");
}
else if (options.HyperBeamScript)
{
    Console.WriteLine(
        "Input script: install the retail endgame Hyper Beam state, fire one fresh Shoot edge, and follow its native flare, Wave motion, and bank-$93 projectile art.");
}
else if (options.MissileScript)
{
    Console.WriteLine(
        "Input script: select HUD item one, grant ten debugger missiles, fire one fresh Shoot edge, and follow the retail projectile art, acceleration, and exhaust trail.");
}
else if (options.SuperMissileScript)
{
    Console.WriteLine(
        "Input script: select HUD item two, grant ten debugger Super Missiles, fire one fresh Shoot edge, and follow its visible owner plus invisible linked collision slot.");
}
else if (options.AerialTurnScript)
{
    Console.WriteLine(
        "Input script: neutral jump right, reverse left in mid-air, retain momentum through the ROM turn animation, then land.");
}
else if (options.CompactAirScript)
{
    Console.WriteLine(
        "Input script: enter/exit compact straight-down jump right, land compact, turn left, then mirror it.");
}
else if (options.AimCrouchScript)
{
    Console.WriteLine(
        "Input script: aimed crouch/live aim/stand right, turn left, repeat, then release.");
}
else if (options.AimTurnScript)
{
    Console.WriteLine(
        "Input script: diagonal-up, straight-up, and diagonal-down grounded aim turns in both directions.");
}
else if (options.CrouchTurnScript)
{
    Console.WriteLine(
        "Input script: ordinary, straight-up, diagonal-up, and diagonal-down crouched turns in both directions.");
}
else if (options.CrouchJumpScript)
{
    Console.WriteLine(
        "Input script: direct crouch exit, ordinary crouch jump, aimed crouch jump, then direct exit again.");
}
else if (options.MorphBallScript)
{
    Console.WriteLine(
        "Input script: crouch/morph, roll right, reverse left, stop, then unmorph; Morph Ball item bit is host-enabled.");
}
else if (options.SpringBallScript)
{
    Console.WriteLine(
        "Input script: crouch into equipped Spring Ball, roll, powered jump, release, and land.");
}
else if (options.BombJumpScript)
{
    Console.WriteLine(
        "Input script: crouch/morph, place a real normal bomb, then follow its ROM countdown, overlap, explosion, and straight bomb-jump arc.");
}
else if (options.PowerBombScript)
{
    Console.WriteLine(
        "Input script: select ten Power Bombs, crouch/morph, fire one fresh Shoot edge, then follow the retail fuse, bank-$88 radius phases, terrain border scan, and color-math window.");
}
else if (options.KnockbackScript)
{
    Console.WriteLine(
        "Input script: host-inject one enemy-side hit result, run native knockback, press Left+Jump for the retail damage boost, then hold that chord through its arc.");
}
else if (options.MorphKnockbackScript)
{
    Console.WriteLine(
        "Input script: crouch/morph, host-inject one enemy-side hit result, preserve real Morph Ball pose/animation through native knockback, then fall and land normally.");
}
else if (options.GrappleFireScript)
{
    Console.WriteLine(
        "Input script: select grapple, fire right using ROM pose tables, hold through live room collision/cutoff, then observe queued cancellation.");
}
else if (options.GrappleScript)
{
    Console.WriteLine(
        "Input script: host-publish one connected grapple anchor, pump the ROM pendulum with Left/Right while holding Shoot, then release into native $51/$52 velocity.");
}
else if (options.CrystalFlashScript)
{
    Console.WriteLine(
        "Input script: invoke the translated centred power-bomb-cleanup entry with exact Down+L+R+Shoot input, then execute ROM poses $D3/$01 through all three native Crystal Flash handlers.");
}
else if (options.XrayScript)
{
    Console.WriteLine(
        "Input script: select equipped X-ray, execute all eight bank-$88 setup stages, hold Dash through native widening, aim upward, then turn through ROM poses $D5/$25/$D6 while the visor palette cycles.");
}
else if (options.VisorScript)
{
    Console.WriteLine(
        "Palette script: publish layer-blending configuration $28, then run the native five-call visor cycle while ordinary movement and drawing continue.");
}
else if (options.DeathScript)
{
    Console.WriteLine(
        "Game-state script: enter bank-$9B after the fatal-damage music wait, animate pose $D7, transfer five death-tile segments, flash for 60 calls, then render the native suit explosion and room whiteout.");
}
else if (options.MotherBrainRainbowScript)
{
    Console.WriteLine(
        "Actor script: execute `$A9:B8EB-$BFCF/$C710-$CABC` with retail Mother Brain body/neck bytecode, forced drained-Samus handlers, live Baby tile DMA, sine/table flight, drain/corpse handshake, release, Samus latch, and healing.");
}
else if (options.DrainedSamusScript)
{
    Console.WriteLine(
        "Actor script: publish the real Mother Brain/Baby Metroid drained-controller calls, execute ROM poses $E8/$EA/$E8/$01, and use $F7's installed vertical handler against live terrain.");
}
else if (options.DraygonGrabScript)
{
    Console.WriteLine(
        "Actor script: enter the real right-facing Draygon grab, exercise ROM poses $EC-$F0, reach 60 counted D-pad patterns, then execute $90:E2DE release to $01.");
}
else if (options.ExtraDisplacementScript)
{
    Console.WriteLine(
        "Producer script: publish persistent $0B56-$0B5C X/Y displacement, move standing Samus right/up/down through live terrain, then clear all four words.");
}
else if (options.ElevatorScript)
{
    Console.WriteLine(
        "Movement script: host-publish nonzero elevator status, then execute forward-facing `$00`'s native one-pixel `$94:9763` terrain scan until real Landing Site floor collision.");
}
else if (options.ForwardFacingScript)
{
    Console.WriteLine(
        "Pose script: invoke the equipment-selected $91:E3F6 forward-facing setup and render the real $00 power-suit body plus its raw $3821 chest-cover OBJ.");
}

// Copy the first 16 bytes at the reset bank into an otherwise-unused VRAM diagnostic page
// through the same queue/NMI path used by room and sprite uploads. Word $7800 stays clear
// of BG data, the standard $6000-$76FF sprite upload, and the timer at $7E00.
runtime.VramWrites.Enqueue(
    sizeInBytes: 16,
    sourceAddress: 0x808000,
    encodedVramDestination: 0x7800);

// Populate the exact BG3 HUD graphics/tilemap VRAM regions as another real NMI workload.
// The current snapshot represents Ceres-era 99 energy and no collected ammunition.
runtime.LoadUpperCrateriaBackgroundPalette();
runtime.InitializeHud(HudSnapshot.CeresDebug);

// These are the exact first two entries of the Ceres escape-timer transfer table at
// $A6:C4CB. They land at the OBJ addresses selected by gameplay's OBSEL=$03.
// The gunship route is normal Landing Site gameplay, not either escape. Keeping the timer
// tile upload would be harmless by itself, but its independently started actor was also
// being drawn over the ship in earlier captures and made a bad visual test look plausible.
if (!options.GunshipScript)
    runtime.QueueEscapeTimerSpriteTiles();

// Establish the room before frame one. The command-line runner used to do this after the
// stepping loop, which meant its requested frames could advance the timer but could not run
// Landing Site's room-main sky uploads. The 9x5 dimensions come directly from
// RoomHeader_LandingSite ($8F:91F8); no extracted PNG or raw-asset directory participates in
// this live path.
// The gunship route is explicitly placed on Landing Site's bottom row. Door $83:896A is
// an actual entry into that row and its command-E record installs sky page $8A:D180. Other
// debug routes can choose non-door camera coordinates after this call, so keep their prior
// cutscene page until their entry-page selection is modeled rather than guessing by script.
runtime.InitializeLandingSiteCamera(
    options.GunshipScript ? (ushort)0x896a : LandingSiteRomData.LandingCutsceneDoorPointer);
ScrollBoundaryCamera camera = runtime.Camera!;

// The optional grounded scenario must choose its camera before the native initial viewport
// fill. Its world X and desired screen Y are explicitly host-authored; the returned floor,
// slope height, resting Y, pose data, and every subsequent movement value are ROM-backed.
DebugGroundedSamusPlacement? groundedPlacement = null;
DebugRanIntoWallSamusPlacement? wallPlacement = null;
if (options.GroundedRun)
{
    if (options.GrappleScript)
    {
        groundedPlacement = runtime.InitializeDebugGrappleSwing();
    }
    else if (options.RanIntoWallScript || options.ScrewAttackScript || options.SuperMissileScript)
    {
        // The core scans the decompressed room for an ordinary type-$8 corner. Keeping
        // the returned coordinates here makes the host-authored placement as inspectable
        // as the cartridge-authored blocks that the live one-pixel probe will consume.
        wallPlacement = runtime.InitializeDebugRanIntoWallSamus();
        groundedPlacement = wallPlacement.Value.Grounded;

        if (options.ScrewAttackScript)
        {
            // `$90:9D96` exposes Screw Attack frames 26/27 only when the spinning body
            // actually reaches a wall. Begin three blocks left of the wall selected above:
            // enough runway to establish `$09`, but close enough for the airborne body to
            // contact the same cartridge-authored type-$8 column before landing. Rebuild
            // the diagnostic placement record so later floor/map probes remain truthful.
            ushort screwStartX = unchecked((ushort)(runtime.Samus!.XPosition - 48));
            runtime.Samus.XPosition = screwStartX;
            int screwFloorBlockX = screwStartX >> 4;
            DebugGroundedSamusPlacement original = groundedPlacement.Value;
            groundedPlacement = original with
            {
                XPosition = screwStartX,
                BlockX = screwFloorBlockX,
                FloorBlock = runtime.LevelData!.GetCollisionBlock(
                    screwFloorBlockX,
                    original.BlockY),
            };
        }
        else if (options.SuperMissileScript)
        {
            // `$90:AFE5` needs enough unobstructed distance to cross the ten-pixel threshold
            // that activates `$90:B00E`'s linked gap probe. Begin six blocks left of the
            // already-inspected ordinary-solid wall while retaining its ROM-authored floor.
            // This host placement replaces no collision: every crossed air/slope block and
            // the eventual impact block still come from Landing Site's decompressed level.
            ushort missileStartX = unchecked((ushort)(runtime.Samus!.XPosition - 96));
            runtime.Samus.XPosition = missileStartX;
            int missileFloorBlockX = missileStartX >> 4;
            DebugGroundedSamusPlacement original = groundedPlacement.Value;
            groundedPlacement = original with
            {
                XPosition = missileStartX,
                BlockX = missileFloorBlockX,
                FloorBlock = runtime.LevelData!.GetCollisionBlock(
                    missileFloorBlockX,
                    original.BlockY),
            };

        }
    }
    else
    {
        groundedPlacement = runtime.InitializeDebugGroundedSamus();
    }
}

if (options.GroundedRun)
{
    if (options.HyperBeamScript)
    {
        // `$91:E5F0` is the actual Mother Brain phase-three reward handler. Calling the
        // translated routine here installs equipment `$1009`, requests the matching beam
        // tile/palette upload, sets `$0A76 = $8000`, and records the still-explicit palette-
        // FX seam. The debugger does not manufacture a special projectile or its constants.
        runtime.Samus!.Drained.EnableHyperBeam(runtime.Samus);
    }
    else
    {
        // Save loading and pause equipment are outside this debugger route. Publish only the
        // validated low-nibble combination before `$90:AC8D` queues its matching tile/palette
        // transfer below. Every live projectile table lookup still consumes the private ROM.
        runtime.Samus!.EquippedBeams = unchecked((ushort)(
            (runtime.Samus.EquippedBeams & 0xfff0) | options.BeamType));
    }
}

if (options.SpeedBoosterScript || options.ShinesparkScript)
{
    // The ordinary debug placement reaches Landing Site's right-side type-$F door before
    // 112 hexadecimal `.1000` additions can reach 7.0000. Move only this diagnostic route
    // 256 pixels left along the same ROM-authored floor; collision and all subsequent
    // motion remain native-data driven, while the separate type-$F dispatcher stays honest.
    runtime.Samus!.XPosition = unchecked((ushort)(runtime.Samus.XPosition - 256));
}

if (options.GunshipScript)
{
    // `$A1:883D` places the ship at X=$0480. The ordinary grounded-debug placement chosen
    // above is one block column to its left and had already centered the camera on that
    // unrelated stimulus, leaving every gunship OBJ 32 pixels right of its true viewport
    // location. Select the ship-centred Landing Site camera before initial BG streaming;
    // the exact actor coordinates are still read back from the loaded slot below.
    camera.SetPosition(x: 0x0400, y: 0x0400);
}
InitialViewportResult initialViewport = runtime.InitializeLandingSiteViewport();

// Validate the complete room-owned chain before a frame has a chance to mutate it. These
// values are not fixture constants copied into RoomEnemySystem: the runtime reached them by
// following Landing Site's default $8F room state, its $A1 population, the matching $B4
// graphics set, both $A0 definitions, and the definitions' bank-$A2 initialization AI.
if (runtime.LandingSiteEntry!.RoomStatePointer != 0x9213 ||
    runtime.LandingSiteEntry.EnemyPopulationPointer != 0x883d ||
    runtime.LandingSiteEntry.EnemyTilesetPointer != 0x8193)
{
    throw new InvalidOperationException(
        "Landing Site default room state did not resolve the retail enemy pointers.");
}
if (runtime.Enemies.EnemyCount != 3 ||
    runtime.Enemies.FirstFreeEnemyIndex != 0x00c0 ||
    runtime.Enemies.DeathQuota != 0 ||
    runtime.Enemies.GraphicsSet.Count != 2)
{
    throw new InvalidOperationException(
        "Landing Site enemy population/graphics terminators produced unexpected counts.");
}
RoomEnemySlot gunshipTop = runtime.Enemies.Slots[0];
RoomEnemySlot gunshipBottom = runtime.Enemies.Slots[1];
RoomEnemySlot gunshipPad = runtime.Enemies.Slots[2];
if (gunshipTop.EnemyDefinitionPointer != 0xd07f ||
    gunshipBottom.EnemyDefinitionPointer != 0xd0bf ||
    gunshipPad.EnemyDefinitionPointer != 0xd0bf ||
    gunshipTop.XPosition != 0x0480 || gunshipTop.YPosition != 0x045f ||
    gunshipBottom.YPosition != 0x0487 || gunshipPad.YPosition != 0x045e ||
    gunshipTop.CurrentInstruction != 0xa616 ||
    gunshipBottom.CurrentInstruction != 0xa61c ||
    gunshipPad.CurrentInstruction != 0xa60e ||
    gunshipTop.VramTilesIndex != 0 ||
    gunshipBottom.VramTilesIndex != 0 ||
    gunshipPad.VramTilesIndex != 0 ||
    gunshipTop.PaletteIndex != 0x0e00 ||
    gunshipBottom.PaletteIndex != 0x0e00 ||
    gunshipPad.PaletteIndex != 0x0e00)
{
    throw new InvalidOperationException(
        "Landing Site gunship slots disagree with the retail initialization result.");
}
if (runtime.Vram.ReadByte(0xe000) != bus.ReadByte(0xadb600) ||
    runtime.Vram.ReadByte(0xefff) != bus.ReadByte(0xadc5ff) ||
    runtime.Vram.ReadByte(0xf000) != bus.ReadByte(0xadb600) ||
    runtime.Vram.ReadByte(0xf1ff) != bus.ReadByte(0xadb7ff))
{
    throw new InvalidOperationException(
        "Landing Site enemy tile loads disagree with the definitions' ROM source slices.");
}
Console.WriteLine(
    "Loaded Landing Site enemies from $A1:883D: gunship top $D07F plus two " +
    "bottom/pad $D0BF slots; graphics $B4:8193 now occupy VRAM $E000-$F1FF.");

// Cross-room proof: Parlor's post-Ceres state is a dense nineteen-slot population of steam
// actors at $A1:8DA0. It exercises fallback graphics indexes, RNG-consuming initialization,
// actor-specific branch instructions, property-driven visibility, and extended spritemaps.
// Use independent PPU/RNG state so this diagnostic cannot perturb the live Landing Site run.
var parlorEnemies = new RoomEnemySystem();
var parlorVram = new SnesVram();
var parlorCgram = new SnesCgram();
var parlorRandom = new Bank80SystemState();

// Steam deliberately has a zero-byte enemy-graphics allocation and therefore draws from
// the standard sprite sheet installed during $82:82E2. Reproduce that earlier game-start
// DMA here; otherwise the actor state and OAM would be exact while its diagnostic PNG was
// transparently decoding an empty private VRAM buffer. The default palette at $9A:8000 is
// loaded for the same reason. RoomEnemySystem.Load then applies Parlor's enemy transfers on
// top, preserving the native ordering in which room-specific art wins any overlap.
parlorVram.ExecuteQueuedWrite(
    bus,
    sourceAddress: 0x9ad200,
    sizeInBytes: 0x2e00,
    encodedDestination: 0x6000);
parlorCgram.LoadFromBus(bus, sourceAddress: 0x9a8000);
parlorEnemies.Load(
    bus,
    populationPointer: 0x8da0,
    tilesetPointer: 0x8295,
    parlorVram,
    parlorCgram,
    parlorRandom.NextRandom);
if (parlorEnemies.EnemyCount != 19 ||
    parlorEnemies.FirstFreeEnemyIndex != 19 * RoomEnemySystem.NativeSlotSize ||
    parlorEnemies.GraphicsSet.Count != 1 ||
    parlorEnemies.GraphicsSet[0].DefinitionPointer != 0xd87f)
{
    throw new InvalidOperationException(
        "Parlor escape-state enemy population/graphics set did not load exactly.");
}
RoomEnemySlot firstSteam = parlorEnemies.Slots[0];
if (firstSteam.EnemyDefinitionPointer != 0xe1ff ||
    firstSteam.CurrentInstruction != 0xf04d ||
    firstSteam.VariableA != 0xeff4 ||
    firstSteam.VariableD != 23 ||
    firstSteam.VramTilesIndex != 0 ||
    firstSteam.PaletteIndex != 0x0a00 ||
    firstSteam.ExtraProperties != 0x0004)
{
    throw new InvalidOperationException(
        "First Parlor steam slot disagrees with $A6:EFB1 initialization and RNG seed $0061: " +
        $"def=${firstSteam.EnemyDefinitionPointer:X4}, list=${firstSteam.CurrentInstruction:X4}, " +
        $"func=${firstSteam.VariableA:X4}, delay={firstSteam.VariableD}, " +
        $"tile=${firstSteam.VramTilesIndex:X4}, palette=${firstSteam.PaletteIndex:X4}, " +
        $"extra=${firstSteam.ExtraProperties:X4}.");
}
for (int steamFrame = 0; steamFrame < 24; steamFrame++)
    parlorEnemies.StepFrame(cameraX: 0x0100, cameraY: 0, timeIsFrozen: false);
if (firstSteam.Health != 0x7fff ||
    firstSteam.Properties != 0x2000 ||
    firstSteam.SpritemapPointer != 0xf142 ||
    firstSteam.CurrentInstruction != 0xf065 ||
    firstSteam.InstructionTimer != 3)
{
    throw new InvalidOperationException(
        "First Parlor steam did not emerge on its exact randomized instruction-list frame.");
}
var parlorOam = new OamBuffer();
parlorOam.BeginFrame();
parlorEnemies.DrawLayers(parlorOam, cameraX: 0x0100, cameraY: 0, firstLayer: 5, lastLayer: 5);
parlorOam.FinalizeFrame();
if (parlorOam.LastFinalizedSpriteCount == 0)
    throw new InvalidOperationException("Visible Parlor steam emitted no extended-spritemap OAM.");

// Keep a standalone visual artifact beside the requested gameplay capture. It contains
// only the authentic steam OBJ layer (transparent elsewhere), which makes incorrect tile
// indexes, palettes, clipping, or extended-spritemap offsets immediately obvious without
// needing a full Parlor background renderer first.
Rgba32[] parlorSteamFrame = SnesObjRenderer.Render(
    parlorOam,
    parlorVram,
    parlorCgram,
    obsel: 0x03,
    width: 256,
    height: 224);
string parlorSteamOutputPath = Path.Combine(
    Path.GetDirectoryName(Path.GetFullPath(options.OutputPath))!,
    "ParlorSteamFrame.png");
PngWriter.WriteRgba(parlorSteamOutputPath, 256, 224, parlorSteamFrame);
Console.WriteLine(
    $"Cross-room enemy proof: Parlor $A1:8DA0 loaded 19 steam slots; first RNG delay " +
    $"23 reached extended map $A6:F142 on frame 24 and emitted " +
    $"{parlorOam.LastFinalizedSpriteCount} OBJ piece(s) to {Path.GetFullPath(parlorSteamOutputPath)}.");

// Two loader-only probes deliberately avoid claiming that unrelated actor AI is playable.
// Crateria Map's real $A1:85A9 list is empty, while Mother Brain's $A0:EC3F header exercises
// the boss-ID and high-health fields that neither Landing Site nor Parlor happen to contain.
var emptyRoomEnemies = new RoomEnemySystem();
var emptyRoomVram = new SnesVram();
var emptyRoomCgram = new SnesCgram();
emptyRoomVram.LoadBytes(0xe000, new byte[] { 0x5a });
emptyRoomCgram.SetColor(128, 0x4567);
emptyRoomEnemies.Load(
    bus,
    populationPointer: 0x85a9,
    tilesetPointer: 0x8193,
    emptyRoomVram,
    emptyRoomCgram,
    parlorRandom.NextRandom);
if (emptyRoomEnemies.EnemyCount != 0 ||
    emptyRoomEnemies.GraphicsSet.Count != 0 ||
    emptyRoomVram.ReadByte(0xe000) != 0x5a ||
    emptyRoomCgram.Colors[128] != 0x4567)
{
    throw new InvalidOperationException(
        "Crateria Map's empty population unexpectedly processed enemy graphics or slots.");
}
RoomEnemyDefinition motherBrainHeader = RoomEnemySystem.ReadDefinition(bus, 0xec3f);
if (motherBrainHeader.TileDataSize != 0x1000 ||
    motherBrainHeader.Health != 18000 ||
    motherBrainHeader.Damage != 120 ||
    motherBrainHeader.Bank != 0xa9 ||
    motherBrainHeader.BossId != 0x000a ||
    motherBrainHeader.InitializationAiPointer != 0x8705 ||
    motherBrainHeader.PartCount != 1 ||
    motherBrainHeader.Layer != 5 ||
    motherBrainHeader.NamePointer != 0)
{
    throw new InvalidOperationException(
        "Mother Brain's complete $A0:EC3F header did not parse at its native offsets.");
}
Console.WriteLine(
    "Loader-only enemy proof: empty Crateria Map skipped $B4 data, and Mother Brain " +
    "$A0:EC3F parsed health 18000 with boss ID $000A without dispatching unsupported AI.");

if (options.GunshipScript)
{
    // Place an ordinary movement-type-zero body inside $A2:A9BD's exact 16-by-64 entry
    // rectangle. This is the only host stimulus; the Down predicate, forward-facing lock,
    // pad bytecode, 144-call waits, vertical motion, restoration, and exit all remain the
    // production actor path driven by the private ROM.
    runtime.Samus!.XPosition = gunshipTop.XPosition;
    runtime.Samus.YPosition = unchecked((ushort)(gunshipTop.YPosition - 32));
    runtime.Samus.Health = 91;
    runtime.Samus.MaxHealth = 99;
    runtime.Samus.Missiles = 0;
    runtime.Samus.MaxMissiles = 4;
    runtime.Samus.SuperMissiles = 0;
    runtime.Samus.MaxSuperMissiles = 2;
    runtime.Samus.PowerBombs = 0;
    runtime.Samus.MaxPowerBombs = 2;
}

// The cutscene door does not define a normal-gameplay Samus spawn. In the default scenario,
// introduce stationary pose $01 at a clearly documented host point so the native palette,
// animation-definition, tile-DMA, screen-position, split-spritemap, and OAM paths can be
// stepped without pretending that the cinematic spawned gameplay Samus.
if (!options.GroundedRun)
    runtime.InitializeDebugStandingSamus();

ushort? elevatorStartY = null;
ushort? elevatorStopY = null;
if (options.ForwardFacingScript || options.ElevatorScript)
{
    // Landing Site's gameplay-debug placement is host-authored because the cinematic door
    // does not spawn normal Samus. From that documented seam onward this invokes the exact
    // equipment selection, radius, delay program, speed clears, graphics definitions,
    // spritemaps, and raw chest-cover OAM record used by retail pose `$00`.
    runtime.Samus!.ApplyForwardFacingPoseSetup(bus);
    runtime.Samus.PrimeGraphics(bus);

    if (options.ElevatorScript)
    {
        // Landing Site has no active elevator actor in this debug room. Publish only the
        // actor-owned status word and place the already grounded body eight pixels above
        // the same cartridge-authored floor. `$90:A392-$A3A8`, `$94:9763`, collision,
        // camera, art, and OAM remain live; this does not fabricate a moving platform.
        runtime.Samus.YPosition = unchecked((ushort)(runtime.Samus.YPosition - 8));
        runtime.Samus.Kinematics.YSubposition = 0;
        elevatorStartY = runtime.Samus.YPosition;
        DebugGroundedSamusPlacement elevatorPlacement = groundedPlacement ??
            throw new InvalidOperationException(
                "Elevator script requires the inspected Landing Site floor placement.");
        elevatorStopY = unchecked((ushort)(
            elevatorPlacement.BlockY * 16 + elevatorPlacement.FloorHeight -
            runtime.Samus.Kinematics.YRadius));
        runtime.ElevatorStatus = 1;
    }
}

MotherBrainRainbowBeamAttackSequence? rainbowAttack = null;
BabyMetroidCutsceneState? cutsceneBaby = null;
MotherBrainEnemyProjectileSystem? motherBrainProjectiles = null;

if (options.MorphBallScript || options.BombJumpScript || options.PowerBombScript || options.MorphKnockbackScript)
{
    // The Landing Site debugger spawn has no save-file inventory. The ordinary route needs
    // Morph Ball `$0004`; the bomb route additionally needs Bombs `$1000`. These are the
    // only host grants: placement/countdown/instructions/overlap/movement remain ROM-backed.
    runtime.Samus!.EquippedItems = runtime.Samus.EquippedItems.With(
        options.BombJumpScript
            ? SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs
            : SamusEquipmentFlags.MorphBall);
    if (options.PowerBombScript)
    {
        // Save/pause loading is the explicit host seam. The selected item and reserve are
        // ordinary Samus words; every producer/lifecycle value after this grant is native.
        runtime.Samus.PowerBombs = 10;
        runtime.Samus.SelectedHudItem = 3;
    }
}
else if (options.SpringBallScript)
{
    // As with ordinary Morph Ball, inventory is explicit debugger stimulus. Grant both
    // Morph Ball `$0004` and Spring Ball `$0002`; F9 must choose its equipped operands.
    runtime.Samus!.EquippedItems = runtime.Samus.EquippedItems.With(
        SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpringBall);
}
else if (options.SpeedBoosterScript || options.ShinesparkScript)
{
    // The debug spawn has no save inventory. Grant only retail Speed Booster bit `$2000`;
    // every counter, delay list, velocity, transition, and collision remains ROM-driven.
    runtime.Samus!.EquippedItems = runtime.Samus.EquippedItems.With(
        SamusEquipmentFlags.SpeedBooster);
}
else if (options.SpaceJumpScript || options.WaterSpaceJumpScript || options.ScrewAttackScript)
{
    // Landing Site's debugger spawn deliberately begins without save-file inventory.
    // Space Jump is bit `$0200`; the Screw route adds `$0008`. Granting both on the latter
    // route is important: it makes the real `$91:F624` priority rule choose Screw art while
    // `$90:A436` independently continues to permit Space Jump's repeated-jump physics.
    runtime.Samus!.EquippedItems = runtime.Samus.EquippedItems.With(
        options.ScrewAttackScript
            ? SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.ScrewAttack
            : SamusEquipmentFlags.SpaceJump);
}
else if (options.CrystalFlashScript)
{
    // The runner has no save-file loader, and this focused script intentionally skips the
    // several-second translated fuse/explosion wait. Publish the inventory and invoke the
    // same bank-$88 cleanup entry reached by ordinary power-bomb play. TryBegin below still
    // performs the native exact-input, velocity, energy, reserve, ammo, direction, and pose
    // checks before any scripted handler is allowed to become active.
    runtime.Samus!.Health = 1;
    runtime.Samus.MaxHealth = 99;
    runtime.Samus.ReserveEnergy = 0;
    runtime.Samus.MaxReserveEnergy = 0;
    runtime.Samus.Missiles = 10;
    runtime.Samus.SuperMissiles = 10;
    runtime.Samus.PowerBombs = 10;
    const ushort crystalFlashChord =
        (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
    if (!runtime.TryBeginCrystalFlashFromPowerBombCleanup(crystalFlashChord))
        throw new InvalidOperationException("Real-ROM Crystal Flash initiation rejected its canonical fixture.");
}
else if (options.XrayScript)
{
    // Landing Site's debug spawn has no save-file inventory or general HUD-item cursor.
    // Grant exactly X-ray bit `$8000`, then enter through the narrow runtime seam which
    // represents the HUD having already selected the scope. `$91:E16D` still owns every
    // velocity, pose-family, power-bomb, bomb-count, cooldown, and prior-movement gate;
    // no host-authored X-ray pose, angle, animation frame, or beam state is installed here.
    runtime.Samus!.EquippedItems = runtime.Samus.EquippedItems.With(
        SamusEquipmentFlags.XrayScope);
    if (!runtime.TryBeginXrayFromSelectedHudItem())
        throw new InvalidOperationException("Real-ROM X-ray initiation rejected its canonical grounded fixture.");
}
else if (options.DeathScript)
{
    // Fatal-damage detection, state-$14 blackout, and the state-$15 music-queue wait are
    // outer game-state producers. Enter at their exact cleared-queue seam; `$9B:B3A7` still
    // reads the live pose movement type/direction and owns every subsequent art/timer word.
    SamusDeathSequenceStartResult deathStart = runtime.BeginDeathSequenceAfterMusicWait();
    Console.WriteLine(
        $"Death setup: source type=${(byte)deathStart.SourceMovementType:X2}, " +
        $"pose=${deathStart.DeathPose:X2}, frame={deathStart.InitialFrame}, " +
        $"screen=({deathStart.ScreenX},{deathStart.ScreenY}), " +
        $"spinSfx={deathStart.SpinJumpSoundRequested}.");
}
else if (options.MotherBrainRainbowScript)
{
    // The Landing Site supplies a real ROM/PPU/runtime host, not Mother Brain's room spawn.
    // Put only the encounter-local actor coordinates and inventory at their documented seam;
    // every wait, list opcode, pose, resource tick, and forced displacement remains translated.
    runtime.Samus!.Health = 800;
    runtime.Samus.MaxHealth = 899;
    runtime.Samus.Missiles = 80;
    runtime.Samus.SuperMissiles = 80;
    runtime.Samus.PowerBombs = 400;
    runtime.Samus.XPosition = 220;
    runtime.Samus.YPosition = 124;
    runtime.Samus.Kinematics.XSubposition = 0;
    runtime.Samus.Kinematics.YSubposition = 0;

    rainbowAttack = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 64,
        BrainYPosition = 96,
    };
    rainbowAttack.Body.XPosition = 64;
    rainbowAttack.Body.YPosition = 100;
    // Native brain-slot initialization `$A9:8705` creates the 48-entry rot table and
    // extracts the right-hand corpse graphics frame immediately, long before the death AI
    // consumes it. Do the same here against the live ROM/WRAM bus so late debugger stepping
    // observes real data and the normal VRAM queue can read its `$7E:9000` working buffer.
    rainbowAttack.InitializeCorpseRotting(bus);
    rainbowAttack.StartAttackCycle();
    motherBrainProjectiles = new MotherBrainEnemyProjectileSystem();
}
else if (options.DrainedSamusScript)
{
    // The complete bank-$A9 boss actor does not exist yet. Lift the normal grounded debug
    // placement by two blocks, then publish exactly controller function zero. Pose metadata,
    // animation bytecode, gravity, collision, spritemaps, tile DMA, and later controller
    // calls all remain live ROM data; only the missing actor's call timing is host-authored.
    runtime.Samus!.YPosition = unchecked((ushort)(runtime.Samus.YPosition - 32));
    runtime.Samus.Drained.LetFall(bus, runtime.Samus);
}
else if (options.DraygonGrabScript)
{
    // A live Draygon enemy actor is not part of Landing Site. Host-publish only its body
    // coordinate at the exact `$A5:94A9` seam, choosing it so the first application keeps
    // the grounded debug placement unchanged. Every pose, delay, tile definition, sprite-
    // map, input transition, escape count, release side effect, and later camera read comes
    // from translated logic plus this private ROM.
    runtime.Samus!.DraygonGrabbed.Begin(bus, runtime.Samus, draygonFacingRight: true);
    runtime.Samus.DraygonGrabbed.ApplyOwnerPosition(
        runtime.Samus,
        unchecked((ushort)(runtime.Samus.XPosition - 8)),
        unchecked((ushort)(runtime.Samus.YPosition - 0x28)),
        draygonFacingRight: true);
}

if (options.WaterSpaceJumpScript)
{
    // Landing Site has scrolling-sky FX rather than water. This route supplies only the
    // missing room-FX words at their normal producer/consumer seam; surface comparisons,
    // launch/gravity/X tables, animation delay, transition records, collision, and art all
    // remain live cartridge data. The Space-Jump-only route places the surface at center Y
    // to exercise `$90:A436`'s distinct top- and bottom-boundary branches. When this option
    // is deliberately combined with the Screw route, raise that same room-owned surface by
    // sixteen pixels so Samus's bottom remains submerged when the ROM animation reaches
    // frame 27; no pose, animation, palette, velocity, or physics word is host-authored.
    ushort debugWaterSurface = options.ScrewAttackScript
        ? unchecked((ushort)(runtime.Samus!.YPosition - 16))
        : runtime.Samus!.YPosition;
    runtime.Samus.LiquidPhysics.ConfigureWater(debugWaterSurface);
    runtime.Samus.LiquidPhysics.InitializeRememberedMedium(runtime.Samus);
    Console.WriteLine(
        $"Debug water FX stimulus: surface Y=${debugWaterSurface:X4}, " +
        $"top=${runtime.Samus.Kinematics.TopBoundary:X4}, " +
        $"bottom=${runtime.Samus.Kinematics.BottomBoundary:X4}, " +
        $"remembered medium={runtime.Samus.LiquidPhysics.LiquidPhysicsType}.");
}

if (options.LandingImpactScript)
{
    // Landing Site itself deliberately deletes landing particles unless its FX type is the
    // special `$000A` record. The reusable debugger has not loaded a Norfair room yet, so
    // publish only area byte two at this documented room-metadata seam. `$91:F0AE` then
    // selects the retail Norfair handler; the live Landing Site fall/collision still supplies
    // the exact impact position and velocity, and bank `$90` supplies particle art/OAM.
    runtime.Samus!.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Norfair, 0);
}

if (options.GrappleFireScript)
    runtime.EnableDebugGrappleItemSelection();

if (options.ChargeBeamScript)
{
    // Landing Site's diagnostic spawn has no save-file inventory. Grant only the retail
    // Charge Beam equipment bit `$1000`; `$90:B80D/$BAFC`, the ROM delay lists, bank-$93
    // projectile records, tile DMA, OAM, and the release threshold remain live cartridge
    // behavior. No charge counter, flare frame, projectile type, or damage is host-written.
    runtime.Samus!.EquippedBeams |= 0x1000;
}

if (options.MissileScript)
{
    // Save-file loading and pause-screen item selection are not translated. Publish only
    // their gameplay results: a finite ammo word and HUD selection one. `$90:BE62`, the
    // shared allocation/cooldown words, `$93:8641` data, `$90:AF68` acceleration, bank-$90
    // trail lists, collision, and OAM all remain live private-cartridge behavior.
    runtime.Samus!.Missiles = 10;
    runtime.Samus.SelectedHudItem = 1;
}

if (options.SuperMissileScript)
{
    // The inventory/menu seam is the same as ordinary missiles. No link is injected here:
    // the translated first alpha pass must allocate `$90:BF46` itself from a real free slot.
    runtime.Samus!.SuperMissiles = 10;
    runtime.Samus.SelectedHudItem = 2;
}

if (options.VisorScript)
{
    // Landing Site normally uses default blending configuration two and therefore resets
    // `$0A72/$0A73` instead of cycling. Publish only the room/HDMA-owned configuration word
    // `$28`; the timer, table offsets, colors, movement, DMA, OAM, and rendering stay native.
    runtime.LayerBlendingDefaultConfig = LayerBlendingConfiguration.VisorBackdrop28;
}

Console.WriteLine(
    $"Loaded Landing Site scrolls $8F:9283 -> $7E:CD20: " +
    $"{Convert.ToHexString(camera.Scrolls.Storage[..camera.Scrolls.LogicalCellCount])}.");
Console.WriteLine(
    $"Bank $80 BG bookkeeping selected {initialViewport.UpdateRequestCount} initial column update request(s) " +
    $"at camera ({camera.XPosition},{camera.YPosition}).");
Console.WriteLine(
    $"Door $83:{runtime.LandingSiteEntry!.DoorPointer:X4} selected sky " +
    $"${runtime.LandingSiteEntry.SkySourceAddress >> 16:X2}:" +
    $"{runtime.LandingSiteEntry.SkySourceAddress & 0xffff:X4} -> " +
    $"VRAM ${runtime.LandingSiteEntry.SkyVramDestination:X4}, " +
    $"${runtime.LandingSiteEntry.SkyByteCount:X4} bytes.");
Console.WriteLine(
    $"Expanded those requests from real cartridge level data and executed " +
    $"{initialViewport.DmaSegmentCount} initial BG1 DMA segment(s); bank $88 also queued " +
    $"the first four circular BG2 sky rows for frame one's NMI.");
Console.WriteLine(
    $"Debug Samus uses cartridge pose ${runtime.Samus!.Pose:X2}, frame {runtime.Samus.AnimationFrame}, " +
    $"world position ({runtime.Samus.XPosition},{runtime.Samus.YPosition}); " +
    (groundedPlacement is DebugGroundedSamusPlacement placement
        ? $"floor=({placement.BlockX},{placement.BlockY}) type=${(byte)placement.FloorBlock.CollisionType:X1}/" +
          $"BTS ${placement.FloorBlock.Behavior:X2}, height={placement.FloorHeight}; only X/screen framing are host-selected."
        : "only that placement is host-selected."));
if (wallPlacement is DebugRanIntoWallSamusPlacement wallDiagnostic)
{
    if (options.SuperMissileScript)
    {
        Console.WriteLine(
            $"Super Missile regression starts six blocks left of ROM wall column " +
            $"${wallDiagnostic.WallBlockX:X2}, rows " +
            $"${wallDiagnostic.WallTopBlockY:X2}-${wallDiagnostic.WallBottomBlockY:X2}; " +
            "the intervening terrain and impact remain cartridge-authored.");
    }
    else
    {
        Console.WriteLine(
            $"Wall regression uses ROM column ${wallDiagnostic.WallBlockX:X2}, rows " +
            $"${wallDiagnostic.WallTopBlockY:X2}-${wallDiagnostic.WallBottomBlockY:X2}; " +
            $"standing center X=${wallDiagnostic.Grounded.XPosition:X4} is exactly one " +
            "prospective running pixel from collision.");
    }
}
if (options.GrappleScript || options.GrappleFireScript)
{
    // Inventory/PLM placement is not inferred from graphics. Inventory-like BTS type $E
    // is the only bank-$94 block family that can return a grapple connection, so list the
    // real room coordinates before running the script. This diagnostic is also useful when
    // choosing a future non-host-authored grapple firing route.
    var grappleBlocks = new List<string>();
    for (int blockY = 0; blockY < runtime.LevelData!.HeightInBlocks; blockY++)
    {
        for (int blockX = 0; blockX < runtime.LevelData.WidthInBlocks; blockX++)
        {
            RoomCollisionBlock block = runtime.LevelData.GetCollisionBlock(blockX, blockY);
            if (block.CollisionType == RoomCollisionType.GrappleBlock)
                grappleBlocks.Add($"({blockX:X2},{blockY:X2}):{block.Behavior:X2}");
        }
    }
    Console.WriteLine(
        $"Landing Site grapple blocks (X,Y:BTS): " +
        (grappleBlocks.Count == 0 ? "none" : string.Join(' ', grappleBlocks)));
    Console.WriteLine(
        $"Grapple state: anchor=({runtime.Samus.Grapple.AnchorX},{runtime.Samus.Grapple.AnchorY}), " +
        $"beamStart=({runtime.Samus.Grapple.BeamStartX},{runtime.Samus.Grapple.BeamStartY}), " +
        $"length={runtime.Samus.Grapple.RopeLength}, angle=${runtime.Samus.Grapple.Angle:X4}, " +
        $"angularVelocity=${unchecked((ushort)runtime.Samus.Grapple.AngularVelocity):X4}.");
}

// Resolve this through the same two-stage pointer calculation used by live movement. This
// line is deliberately ROM-backed evidence, not a hard-coded description: ordinary air's
// base $9F55 plus running movement type 1 selects the 12-byte entry at $90:9F61.
SamusHorizontalSpeedState horizontalSpeed = runtime.Samus.HorizontalSpeed;
horizontalSpeed.SelectEnvironmentSpeedTable(
    runtime.Samus.LiquidPhysics.DetermineMovementMedium(runtime.Samus));
int runningSpeedAddress = horizontalSpeed.ResolveEntryAddress(movementType: SamusMovementType.Running);
SpeedTableEntry runningSpeed = horizontalSpeed.ReadEntry(bus, movementType: SamusMovementType.Running);
Console.WriteLine(
    $"Running speed table ${runningSpeedAddress >> 16:X2}:{runningSpeedAddress & 0xffff:X4}: " +
    $"accel {runningSpeed.Acceleration:X4}.{runningSpeed.AccelerationSubspeed:X4}, " +
    $"max {runningSpeed.MaximumSpeed:X4}.{runningSpeed.MaximumSubspeed:X4}, " +
    $"decel {runningSpeed.Deceleration:X4}.{runningSpeed.DecelerationSubspeed:X4}.");

// Pose parameters are eight-byte records at $91:B629. Offset six is y_radius, which bank
// $94 uses to choose every 16-pixel row crossed by horizontal collision and the bottom
// probe used by BlockInsideDetection. Log the real BG1/BTS pair at both points so the next
// collision slice begins with visible cartridge evidence.
int poseDefinitionAddress = 0x91b629 + runtime.Samus.Pose * 8;
byte samusYRadius = bus.ReadByte(poseDefinitionAddress + 6);
RoomCollisionBlock centerBlock = runtime.LevelData!.GetCollisionBlockAtPixel(
    runtime.Samus.XPosition,
    runtime.Samus.YPosition);
RoomCollisionBlock bottomBlock = runtime.LevelData.GetCollisionBlockAtPixel(
    runtime.Samus.XPosition,
    unchecked((ushort)(runtime.Samus.YPosition + samusYRadius - 1)));
Console.WriteLine(
    $"Debug Samus collision probes: radiusY={samusYRadius}; " +
    $"center index={centerBlock.Index} level=${centerBlock.LevelWord:X4} BTS=${centerBlock.Behavior:X2} type=${(byte)centerBlock.CollisionType:X1}; " +
    $"bottom index={bottomBlock.Index} level=${bottomBlock.LevelWord:X4} BTS=${bottomBlock.Behavior:X2} type=${(byte)bottomBlock.CollisionType:X1}.");

if (groundedPlacement is DebugGroundedSamusPlacement groundedDiagnostic)
{
    // Landing Site contains collision-authored surfaces whose graphics are transparent in
    // the static room composition. List every lower non-air candidate at the selected X so
    // a grounded preview can deliberately choose a surface with visible terrain instead of
    // silently assuming the first collision block must have artwork.
    var floorCandidates = new List<string>();
    for (int blockY = groundedDiagnostic.BlockY;
         blockY < runtime.LevelData.HeightInBlocks;
         blockY++)
    {
        RoomCollisionBlock candidate = runtime.LevelData.GetCollisionBlock(
            groundedDiagnostic.BlockX,
            blockY);
        if (candidate.CollisionType != RoomCollisionType.Air)
        {
            floorCandidates.Add(
                $"{blockY:X2}:{(byte)candidate.CollisionType:X1}/{candidate.Behavior:X2}/" +
                $"{candidate.LevelWord:X4}");
        }
    }
    Console.WriteLine(
        $"Non-air candidates below grounded X (Y:type/BTS/level): " +
        string.Join(' ', floorCandidates));
}

// This is analysis output, not collision behavior: locate the next non-air dispatcher row
// below the host-selected stimulus. It makes an accidental floating spawn immediately
// obvious and gives the grounding port an exact target block/index/BTS triple.
for (int blockY = (runtime.Samus.YPosition + samusYRadius - 1) >> 4;
     blockY < runtime.LevelData.HeightInBlocks;
     blockY++)
{
    RoomCollisionBlock candidate = runtime.LevelData.GetCollisionBlock(
        runtime.Samus.XPosition >> 4,
        blockY);
    if (candidate.CollisionType == RoomCollisionType.Air)
        continue;

    Console.WriteLine(
        $"First non-air block below debug Samus: block=({runtime.Samus.XPosition >> 4},{blockY}) " +
        $"index={candidate.Index} topY={blockY * 16} level=${candidate.LevelWord:X4} " +
        $"BTS=${candidate.Behavior:X2} type=${(byte)candidate.CollisionType:X1}.");

    // A compact neighboring-row dump reveals whether the candidate is an isolated special
    // block or part of a continuous slope/solid running lane. Each token is X:type/BTS.
    int firstDiagnosticX = Math.Max(0, (runtime.Samus.XPosition >> 4) - 8);
    int lastDiagnosticX = Math.Min(runtime.LevelData.WidthInBlocks - 1, firstDiagnosticX + 16);
    var rowTokens = new List<string>();
    for (int blockX = firstDiagnosticX; blockX <= lastDiagnosticX; blockX++)
    {
        RoomCollisionBlock neighbor = runtime.LevelData.GetCollisionBlock(blockX, blockY);
        rowTokens.Add($"{blockX:X2}:{(byte)neighbor.CollisionType:X1}/{neighbor.Behavior:X2}");
    }
    Console.WriteLine($"Collision row {blockY:X2} around stimulus: {string.Join(' ', rowTokens)}");

    // For a non-square floor, the height-table sample is measured downward from the
    // block's top. Derive the exact center Y that places Samus's bottom on that surface,
    // then execute a +1.0 native grounding probe and a separate +1.0 horizontal scan.
    // These copies do not relocate the visible debug stimulus.
    if (candidate.CollisionType == RoomCollisionType.Slope &&
        (candidate.Behavior & 0x1f) >= 5 &&
        (candidate.Behavior & 0x80) == 0)
    {
        byte floorHeight = SamusSlopePhysics.ReadAlignmentHeight(
            bus,
            candidate.Behavior,
            runtime.Samus.XPosition);
        ushort restingCenterY = unchecked((ushort)(blockY * 16 + floorHeight - samusYRadius));

        var verticalProbe = new SamusKinematicsState
        {
            XPosition = runtime.Samus.XPosition,
            YPosition = restingCenterY,
            XRadius = runtime.Samus.Kinematics.XRadius,
            YRadius = samusYRadius,
        };
        BlockMoveResult grounding = SamusBlockCollision.MoveVertical(
            bus,
            runtime.LevelData,
            verticalProbe,
            displacement: 0x00010000,
            scanLeftToRight: (runtime.NmiFrameCounter & 1) == 0);

        var horizontalProbe = new SamusKinematicsState
        {
            XPosition = runtime.Samus.XPosition,
            YPosition = restingCenterY,
            XRadius = runtime.Samus.Kinematics.XRadius,
            YRadius = samusYRadius,
        };
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            runtime.LevelData,
            horizontalProbe,
            displacement: 0x00010000);

        Console.WriteLine(
            $"ROM floor probe: height={floorHeight}, restingY={restingCenterY}; " +
            $"down +1.0 => accepted=${grounding.AcceptedDisplacement:X8}, collision={grounding.Collided}; " +
            $"right +1.0 => accepted=${horizontal.AcceptedDisplacement:X8}, " +
            $"position={horizontalProbe.XPosition:X4}.{horizontalProbe.XSubposition:X4}, " +
            $"Y={horizontalProbe.YPosition}.");
    }
    break;
}

// Ordinary timer diagnostics still choose a host scenario at startup. The Mother Brain
// route must not do that: `$A9:B309` owns its real status-$0002 request near the very end of
// the death sequence, and prestarting it here would let the timer expire during the fight.
if (!options.MotherBrainRainbowScript && !options.GunshipScript)
{
    if (options.TimerScenario == TimerScenario.Ceres)
        runtime.EscapeTimer.RequestCeresStart();
    else
        runtime.EscapeTimer.RequestMotherBrainStart();
}

EscapeTimerState priorState = runtime.EscapeTimer.State;
bool priorEscapeTimerExpired = false;
ushort priorSamusFrame = runtime.Samus!.AnimationFrame;
byte priorSamusPose = runtime.Samus.Pose;
uint priorSamusX = runtime.Samus.Kinematics.XFixed;
uint priorSamusY = runtime.Samus.Kinematics.YFixed;
ushort? priorProspectivePose = null;
ushort? priorFallbackPose = null;
byte? priorWallCollisionPose = null;
bool observedBombJumpStart = false;
bool observedBombJumpEnd = false;
bool observedBombJumpRise = false;
bool observedBombPlacement = false;
bool observedBombExplosion = false;
bool observedBombDeletion = false;
bool observedStraightBombOverlap = false;
int observedPowerBeamShots = 0;
bool observedPowerBeamArt = false;
bool observedPowerBeamExplosion = false;
bool observedSelectedBeamType = false;
ushort maximumObservedCharge = 0;
bool observedChargedShot = false;
bool observedChargedTrail = false;
ushort observedChargedShotDamage = 0;
ushort observedChargedShotSound = 0;
var observedLiveChargeBodyPalettes = new HashSet<int>();
int observedOrdinaryChargedWhitePaletteCalls = 0;
bool observedOrdinaryChargedSuitRestore = false;
bool observedHyperBeamShot = false;
bool observedHyperBeamArt = false;
bool observedHyperBeamFlare = false;
ushort observedHyperBeamType = 0;
ushort observedHyperBeamDamage = 0;
ushort observedHyperBeamSound = 0;
var observedHyperBeamPaletteFrames = new HashSet<int>();
var observedHyperBeamBodyPalettes = new HashSet<int>();
int observedHyperBeamBodyPaletteHolds = 0;
bool observedHyperBeamBodySuitRestore = false;
bool observedMissileShot = false;
bool observedMissileArt = false;
bool observedMissileTrail = false;
ushort observedMissileDamage = 0;
ushort observedMissileSound = 0;
var observedArmCannonFrames = new HashSet<ushort>();
bool observedArmCannonSprite = false;
bool observedArmCannonTileDma = false;
var observedVisorPaletteOffsets = new HashSet<byte>();
bool observedSuperMissileShot = false;
bool observedSuperMissileArt = false;
bool observedSuperMissileTrail = false;
bool observedSuperMissileLink = false;
bool observedSuperMissileImpact = false;
bool observedSuperMissileQuake = false;
ushort observedSuperMissileDamage = 0;
ushort observedSuperMissileSound = 0;
bool observedKnockbackMovement = false;
bool observedDamageBoostMovement = false;
int observedHurtFlashPaletteCalls = 0;
int observedHurtSuitRestoreCalls = 0;
bool observedHurtImpactSound = false;
bool observedMorphedKnockbackPosePreserved = false;
bool observedMorphedKnockbackAnimationPreserved = false;
bool observedMorphedKnockbackDirectionRule = false;
bool observedMorphedKnockbackCompletion = false;
bool observedMorphedKnockbackFalling = false;
bool observedMorphedKnockbackLanding = false;
ushort morphKnockbackGroundY = 0;
bool observedGrappleSwing = false;
bool observedGrappleReleaseQueue = false;
bool observedGrappleRelease = false;
bool observedGrappleReleaseMovement = false;
bool observedGrappleTerrainCollision = false;
bool observedGrappleFire = false;
bool observedGrappleFireCancelQueue = false;
bool observedGrappleFireCancel = false;
bool observedGrappleDrawHandler = false;
bool observedGrappleBeamDrawPath = false;
bool observedGrappleFlare = false;
bool observedGrappleTeardownDrawFallback = false;
bool observedBlockedRanIntoWallProbe = false;
bool observedDashMomentum = false;
bool observedDashAerialCarry = false;
uint maximumObservedExtraRunSpeed = 0;
byte maximumObservedSpeedBoostStage = 0;
bool observedSpeedBoostEcho = false;
bool observedSpeedBoostContactDamage = false;
bool observedSpeedBoostFootDust = false;
bool observedSpeedBoostDeparture = false;
bool observedSpeedBoostDepartureFinished = false;
bool previousSpeedBoostDeparture = false;
bool observedStoredShine = false;
bool observedShinesparkWindup = false;
bool observedDirectionalShinespark = false;
bool observedShinesparkMovement = false;
bool observedShinesparkPalette = false;
bool observedShinesparkCrashOrbit = false;
bool observedShinesparkCrashEchoCircle = false;
bool observedShinesparkCrashFinish = false;
bool observedShinesparkCrashDrawingHandler = false;
bool observedReleasedShinesparkEcho = false;
int priorReleasedShinesparkEchoCount = 0;
int observedSpaceJumpRestarts = 0;
bool observedScrewAttackContactDamage = false;
bool observedScrewAttackPaletteCycle = false;
bool observedSubmergedScrewPaletteFreeze = false;
bool observedCrystalFlashDrain = false;
bool observedCrystalFlashFinish = false;
bool observedCrystalFlashCompletion = false;
bool observedCrystalFlashWindowExpansion = false;
bool observedCrystalFlashWindowAfterglow = false;
bool observedCrystalFlashPalette = false;
var observedXrayPhases = new HashSet<XrayBeamPhase>();
var observedXrayAnimationFrames = new HashSet<ushort>();
bool observedXrayAim = false;
bool observedXrayTurnStart = false;
bool observedXrayTurnCompletion = false;
bool observedXrayLeftStablePose = false;
var observedDeathPhases = new HashSet<SamusDeathSequencePhase>();
var observedDeathSegments = new HashSet<byte>();
var observedDeathExplosionSpritemaps = new HashSet<ushort>();
bool observedDeathWhiteout = false;
bool observedDeathCompletion = false;
bool observedDrainedFallingHandler = false;
bool observedDrainedLanding = false;
bool observedDrainedStanding = false;
bool observedDrainedCrouching = false;
bool observedDrainedRelease = false;
bool observedDrainedHyperBeam = false;
bool observedDraygonAimUp = false;
bool observedDraygonFiring = false;
bool observedDraygonAimDown = false;
bool observedDraygonMoving = false;
bool observedDraygonNeutralFallback = false;
bool observedDraygonRelease = false;
bool observedExternalXMovement = false;
bool observedExternalUpMovement = false;
bool observedExternalDownCollision = false;
bool observedExternalWordsCleared = false;
bool observedGunshipExit = false;
bool issuedDrainedStandingCommand = false;
bool issuedDrainedCrouchingCommand = false;
bool issuedDrainedReleaseCommand = false;
bool issuedDrainedHyperBeamCommand = false;
int issuedSpaceJumpPulses = 0;
bool spaceJumpPulseMayBeIssued = true;
var observedRainbowPhases = new HashSet<MotherBrainRainbowBeamAttackPhase>();
var observedPhaseThreeAttacks = new HashSet<MotherBrainPhase3AttackKind>();
bool observedPhaseThreeForwardMovement = false;
var observedBabyPhases = new HashSet<BabyMetroidCutscenePhase>();
var observedBabyTileTransfers = new List<MotherBrainSpriteTileTransferRequest>();
var observedMotherBrainCorpseTileTransfers = new List<MotherBrainSpriteTileTransferRequest>();
var observedAttackTileTransfers = new List<MotherBrainSpriteTileTransferRequest>();
var observedBabyDeathPalettes = new List<BabyMetroidPaletteTransferRequest>();
var observedPhaseThreeBackgroundPalettes = new List<MotherBrainBackgroundPaletteTransferRequest>();
int observedBabyDeathExplosions = 0;
int allocatedBabyDeathExplosions = 0;
bool observedBabyPhaseThreeHandoff = false;
bool observedBabySpawnRequest = false;
bool observedFinalBeamSound = false;
bool observedBabyMotherBrainInterrupt = false;
bool observedBabyCeilingTableInstall = false;
bool observedBabyHealingCompletion = false;
int observedBabyLatchOntoSamusFrame = 0;
int observedBabyHealSamusFrame = 0;
int observedBabyHealingCompletionFrame = 0;
ushort observedBabyHealingCompletionHealth = 0;
ushort observedBabyHealingCompletionReserveEnergy = 0;
// Preserve each ROM-record boundary as a fixed-point witness. Merely reaching `$CA66`
// would not detect a carry bug that happened to converge on the same broad target rectangle.
var observedBabyRouteFrames = new Dictionary<ushort, int>();
var observedBabyRoutePoints = new Dictionary<ushort, BabyMetroidCutscenePoint>();
MotherBrainRainbowBeamAttackPhase previousRainbowPhase =
    rainbowAttack?.Phase ?? MotherBrainRainbowBeamAttackPhase.Inactive;
BabyMetroidCutscenePhase previousBabyPhase = BabyMetroidCutscenePhase.Inactive;
ushort previousBabyMovementTablePointer = 0;
// Keep the actual post-frame poses, rather than assuming the requested inputs succeeded.
// The dedicated ROM regression below fails unless both compact bodies and both native
// ordinary-landing records were genuinely installed by the translated frame pipeline.
var observedSamusPoses = new HashSet<byte> { runtime.Samus.Pose };
bool observedLandingImpactDust = false;
bool observedLandingImpactSound = false;
// `$90:EB86` is an NMI-parity display handler rather than an animation-frame toggle.
// Record both outcomes from the live frame pipeline so an even-only final PNG cannot hide
// a regression that accidentally draws Samus on every elevator frame.
bool observedElevatorVisibleEvenFrame = false;
bool observedElevatorHiddenOddFrame = false;
ushort extraDisplacementStartX = runtime.Samus.XPosition;
ushort extraDisplacementStartY = runtime.Samus.YPosition;
for (int frameIndex = 0; frameIndex < options.FrameCount; frameIndex++)
{
    if (options.ExtraDisplacementScript)
    {
        // The room has no translated enemy/PLM that publishes these four WRAM words yet.
        // Supply only that producer boundary. The consumer lifetime, signed 16.16 math,
        // block clipping, slope response, camera, animation, DMA, and rendering all continue
        // through the ordinary private-ROM frame pipeline.
        SamusKinematicsState kinematics = runtime.Samus!.Kinematics;
        if (frameIndex < 32)
        {
            kinematics.ExtraXDisplacement = 1;
            kinematics.ExtraXSubdisplacement = 0;
            kinematics.ExtraYDisplacement = 0;
            kinematics.ExtraYSubdisplacement = 0;
        }
        else if (frameIndex < 48)
        {
            kinematics.ExtraXDisplacement = 0;
            kinematics.ExtraXSubdisplacement = 0;
            kinematics.ExtraYDisplacement = 0xffff;
            kinematics.ExtraYSubdisplacement = 0x8000; // -0.8000
        }
        else if (frameIndex < 64)
        {
            kinematics.ExtraXDisplacement = 0;
            kinematics.ExtraXSubdisplacement = 0;
            kinematics.ExtraYDisplacement = 0;
            kinematics.ExtraYSubdisplacement = 0x8000; // +0.8000; `$923F` requests +1.8000
        }
        else
        {
            kinematics.ExtraXDisplacement = 0;
            kinematics.ExtraXSubdisplacement = 0;
            kinematics.ExtraYDisplacement = 0;
            kinematics.ExtraYSubdisplacement = 0;
        }
    }

    if (options.DraygonGrabScript && runtime.Samus!.DraygonGrabbed.IsActive)
    {
        // EnemyMain runs before Samus's draw and calls `$A5:94A9` after updating Draygon.
        // This asset/handler route deliberately holds that absent enemy actor still; the
        // repeated call proves no ordinary Samus physics drifts away from the supplied claw
        // coordinate. It is an explicit fixed actor stimulus, not a fabricated boss flight.
        runtime.Samus.DraygonGrabbed.ApplyOwnerPosition(
            runtime.Samus,
            runtime.Samus.DraygonGrabbed.OwnerXPosition,
            runtime.Samus.DraygonGrabbed.OwnerYPosition,
            draygonFacingRight: true);
    }

    // The default script presses Start for one frame, releases it, then holds Right. The
    // explicit reversal script is a deterministic real-ROM regression route: enough time
    // to accelerate right, complete $25 toward the left, then complete $26 back right.
    // These are only controller samples; all pose choices still come from bank-$91 tables.
    bool specialSpinRoute = options.SpaceJumpScript ||
        options.WaterSpaceJumpScript ||
        options.ScrewAttackScript;

    // Unlike the fixed-input posture routes below, Space Jump's legal repeat instant is a
    // function of the live 16.16 vertical velocity. Derive the unaligned 8.8 magnitude in
    // the same way as `$90:A436`: high byte of subspeed below the integer speed. A pulse is
    // emitted for exactly one frame, so each accepted attempt has the required new-A edge.
    // The first held interval is still an ordinary grounded running jump; only later pulses
    // are velocity-gated. This keeps the harness deterministic without faking any movement.
    uint liveVerticalMagnitude8Point8 =
        ((uint)runtime.Samus.Kinematics.YSpeed << 8) |
        ((uint)runtime.Samus.Kinematics.YSubspeed >> 8);
    uint liveSpaceJumpMinimum = runtime.Samus.LiquidPhysics.LiquidPhysicsType !=
        SamusLiquidPhysicsState.Air
            ? 0x0080u
            : 0x0280u;
    bool liveSpaceJumpWindow =
        runtime.Samus.Kinematics.YDirection == 2 &&
        liveVerticalMagnitude8Point8 >= liveSpaceJumpMinimum &&
        liveVerticalMagnitude8Point8 < 0x0500;
    bool screwBodyReachedRightWall =
        options.ScrewAttackScript &&
        wallPlacement is DebugRanIntoWallSamusPlacement screwWall &&
        runtime.Samus.XPosition >= screwWall.Grounded.XPosition - 8;
    ushort specialSpinDirection = screwBodyReachedRightWall
        ? (ushort)SnesButton.Left
        : (ushort)SnesButton.Right;
    ushort specialSpinInput = frameIndex switch
    {
        0 => (ushort)SnesButton.Start,
        >= 2 and < 12 => specialSpinDirection,
        >= 12 and < 24 => (ushort)(specialSpinDirection | (ushort)SnesButton.A),
        _ when liveSpaceJumpWindow && spaceJumpPulseMayBeIssued && issuedSpaceJumpPulses < 2
            => (ushort)(specialSpinDirection | (ushort)SnesButton.A),
        _ => specialSpinDirection,
    };
    if (specialSpinRoute && frameIndex >= 24)
    {
        bool jumpPressedThisFrame = (specialSpinInput & (ushort)SnesButton.A) != 0;
        if (jumpPressedThisFrame)
        {
            issuedSpaceJumpPulses++;
            spaceJumpPulseMayBeIssued = false;
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: issued Space Jump pulse {issuedSpaceJumpPulses} " +
                $"at falling magnitude ${liveVerticalMagnitude8Point8:X4}.");
        }
        else
        {
            // One released sample is sufficient to make the following A sample a fresh
            // edge, exactly as the controller new-input word on the SNES would require.
            spaceJumpPulseMayBeIssued = true;
        }
    }

    ushort yDirectionBeforeFrame = runtime.Samus.Kinematics.YDirection;
    ushort controllerInput = specialSpinRoute
        ? specialSpinInput
        : options.ExtraDisplacementScript
        ? (ushort)0
        : options.DraygonGrabScript
        ? frameIndex switch
        {
            // `$91:AE56` maps the default shoulder bindings to up/down aim and Shoot to
            // firing. A direction selects the six-frame moving/struggling body `$F0`.
            >= 16 and < 32 => (ushort)SnesButton.R,
            >= 32 and < 48 => (ushort)SnesButton.X,
            >= 48 and < 64 => (ushort)SnesButton.L,
            >= 64 and < 96 => (ushort)SnesButton.Right,

            // Zero input must use pose-definition byte two to return `$F0 -> $EC`.
            // The earlier Right edge that entered `$F0` already contributed one count to
            // `$90:E2A1`. Alternate Up/Down for the remaining 59 distinct samples so the
            // sixtieth call releases Samus, then leave subsequent frames genuinely blank.
            // That last detail keeps the route from immediately applying ordinary pose-$01
            // controller transitions after it has proved the native release state.
            >= 112 and < 171 => (frameIndex & 1) == 0
                ? (ushort)SnesButton.Up
                : (ushort)SnesButton.Down,
            _ => (ushort)0,
        }
        : options.XrayScript
        ? frameIndex switch
        {
            // X-ray setup began before frame one. Dash/B remains held through eight bank-
            // `$88` setup calls, state zero, and all 27 widening calls. The beam therefore
            // reaches the literal 10.0000 clamp without the host assigning any width word.
            < 36 => (ushort)SnesButton.B,

            // Once full, fourteen Up samples rotate the center angle from `$40` toward the
            // native upper clamp. This must drive `$90:E94F` across real art boundaries.
            >= 36 and < 50 => (ushort)(SnesButton.B | SnesButton.Up),

            // Left is opposite the original right-facing body. `$91:FCAF` mirrors the
            // angle through `$0100-angle`, installs turn pose `$25`, waits for its ROM delay
            // list to reach frame two/timer one, and finally selects stable X-ray pose `$D6`.
            _ => (ushort)(SnesButton.B | SnesButton.Left),
        }
        : options.DeathScript || options.CrystalFlashScript || options.DrainedSamusScript ||
          options.MotherBrainRainbowScript
        ? (ushort)0
        : options.GrappleFireScript
        ? frameIndex < 16 ? (ushort)SnesButton.X : (ushort)0
        : options.GrappleScript
        ? frameIndex switch
        {
            // X is the runtime's default Shoot binding. Pump left through the first half,
            // right through the second, then release so the two-frame $C79D/$CB8B seam is
            // visible in both logs and debugger watches.
            < 45 => (ushort)(SnesButton.X | SnesButton.Left),
            < 90 => (ushort)(SnesButton.X | SnesButton.Right),
            _ => (ushort)0,
        }
        : options.KnockbackScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // Frame 20 below injects the only missing producer: the one-bit enemy-side
            // collision result. Leave this sample empty so `$53` gets one visible native
            // hurt frame. On frame 21, Left+Jump is canonical `$0280` and matches the
            // literal `$91:A8E4 -> $50` record in the cartridge.
            >= 21 and < 46 => (ushort)(SnesButton.Left | SnesButton.A),

            // `$50` is movement type `$19`, whose dispatcher entry calls the ordinary
            // jumping routine. Keeping `$0280` held selects `$50`'s self-record and proves
            // variable-height movement; release later lets it descend and land normally.
            _ => (ushort)0,
        }
        : options.MoonwalkScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // Standing-right `$01` plus backward Left and Shoot proposes `$4A`. The host
            // option merely admits that cartridge candidate; type `$10` owns every step.
            >= 2 and < 30 => (ushort)(SnesButton.Left | SnesButton.X),

            // `$91:A8AC` changes only the moonwalk art/shot direction while the same
            // backward direction stays held. R selects `$76`; L then selects `$78`.
            >= 30 and < 50 => (ushort)(SnesButton.Left | SnesButton.X | SnesButton.R),
            >= 50 and < 70 => (ushort)(SnesButton.Left | SnesButton.X | SnesButton.L),

            // Zero input proves definition fallbacks `$78 -> $07 -> $01`. Re-enter the
            // neutral family, then replace Shoot with Jump while still moving backward.
            >= 80 and < 100 => (ushort)(SnesButton.Left | SnesButton.X),
            >= 100 and < 122 => (ushort)(SnesButton.Left | SnesButton.A),
            _ => (ushort)0,
        }
        : options.RanIntoWallScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // Standing `$01` proposes running `$09`. The host placed the current body on
            // the last safe pixel, so `$91:EADE`'s real +1.0000 block move must reject it
            // and use `$09`'s shot direction two to select neutral wall pose `$89`.
            >= 2 and < 12 => (ushort)SnesButton.Right,

            // Preserve forward Right while changing the actual controller shoulder. The
            // unchanged `$91:AA38` records propose running aim `$0F/$11`; the same block
            // probe then maps their shot directions one/three to wall aim `$CF/$D1`.
            >= 12 and < 22 => (ushort)(SnesButton.Right | SnesButton.R),
            >= 22 and < 32 => (ushort)(SnesButton.Right | SnesButton.L),

            // Releasing every button exercises pose-definition fallback `$D1 -> $89`.
            // A fresh Jump edge then takes the literal wall table's `$89 -> $4B` route;
            // command `$FF` completes the normal `$4B -> $4D` jump handoff.
            >= 42 and < 52 => (ushort)SnesButton.A,
            _ => (ushort)0,
        }
        : options.ShinesparkScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // This is the same natural stage-four route used by --speed-booster-script.
            // The host grants only inventory; `$90:973E`, the ROM delay lists, and the
            // translated block dispatcher own every acceleration and accepted pixel.
            >= 2 and < 118 => (ushort)(SnesButton.Right | SnesButton.B),

            // A fresh Down edge while stage four is still published selects the retail
            // running-to-crouch record. `$91:F7B0` must observe that stage before the
            // posture transition clears normal running momentum and store 180 frames.
            >= 118 and < 128 => (ushort)SnesButton.Down,

            // Jump from the settled crouch. The ordinary `$4B` transition must finish
            // through command `$FF` into `$4D`; only there may stored shine replace the
            // final pose with windup `$C7`, precisely matching `$90:CFFA`'s native seam.
            >= 138 and < 154 => (ushort)SnesButton.A,

            // Windup direction is chosen from a newly pressed direction. Delay Right
            // until after several held-still windup frames so this cannot accidentally
            // pass by merely preserving the direction from the charging run.
            >= 154 and < 190 => (ushort)(SnesButton.A | SnesButton.Right),
            _ => (ushort)0,
        }
        : options.SpeedBoosterScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,
            >= 2 and < 118 => (ushort)(SnesButton.Right | SnesButton.B),
            >= 118 and < 138 => (ushort)(SnesButton.Right | SnesButton.B | SnesButton.A),
            >= 138 and < 166 => (ushort)SnesButton.Right,
            _ => (ushort)0,
        }
        : options.RunScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // The standing frame first lets the unchanged bank-$91 transition table
            // install `$09`. Every subsequent frame reaches `$90:973E` with movement type
            // one and canonical Dash/B `$8000`; C# contributes no velocity constant here.
            >= 2 and < 38 => (ushort)(SnesButton.Right | SnesButton.B),

            // A fresh Jump edge selects retail spin pose `$19`. B remains held briefly,
            // but type three must take `$90:9808` and retain rather than increment 2.0000.
            >= 38 and < 52 => (ushort)(SnesButton.Right | SnesButton.B | SnesButton.A),

            // Releasing B proves momentum, not held input, owns the airborne extra pair.
            // Releasing A later exercises the existing variable-height cutoff and descent.
            >= 52 and < 68 => (ushort)(SnesButton.Right | SnesButton.A),
            >= 68 and < 88 => (ushort)SnesButton.Right,
            _ => (ushort)0,
        }
        : options.ReversalScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,
            >= 2 and < 62 => (ushort)SnesButton.Right,
            >= 62 and < 122 => (ushort)SnesButton.Left,
            >= 122 and < 182 => (ushort)SnesButton.Right,
            _ => (ushort)0,
        }
        : options.JumpScript || options.LandingImpactScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 12 => (ushort)SnesButton.A,
                >= 50 and < 90 => (ushort)SnesButton.Right,
                >= 90 and < 125 => (ushort)(SnesButton.Right | SnesButton.A),
                >= 125 and < 155 => (ushort)SnesButton.Right,
                _ => (ushort)0,
            }
        : options.PostureScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 10 => (ushort)SnesButton.Down,
                20 => (ushort)SnesButton.Up,
                >= 40 and < 65 => (ushort)SnesButton.Left,
                >= 75 and < 83 => (ushort)SnesButton.Down,
                95 => (ushort)SnesButton.Up,
                _ => (ushort)0,
            }
        : options.AimScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 10 => (ushort)SnesButton.Up,
                >= 20 and < 28 => (ushort)SnesButton.R,
                >= 38 and < 46 => (ushort)SnesButton.L,
                >= 60 and < 85 => (ushort)SnesButton.Left,
                >= 100 and < 108 => (ushort)SnesButton.Up,
                >= 118 and < 126 => (ushort)SnesButton.R,
                >= 136 and < 144 => (ushort)SnesButton.L,
                _ => (ushort)0,
            }
        : options.AimRunScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 33 => (ushort)(SnesButton.Right | SnesButton.R),
                >= 33 and < 41 => (ushort)SnesButton.R,
                >= 50 and < 81 => (ushort)(SnesButton.Right | SnesButton.L),
                >= 105 and < 136 => (ushort)SnesButton.Left,
                >= 150 and < 181 => (ushort)(SnesButton.Left | SnesButton.R),
                >= 181 and < 189 => (ushort)SnesButton.R,
                >= 195 and < 226 => (ushort)(SnesButton.Left | SnesButton.L),
                _ => (ushort)0,
            }
        : options.GunExtendedScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // `$91:A1F8` sees held Shot+Right and selects the real type-one `$0B`
                // body. Releasing both inputs then lets running's native momentum command
                // decelerate all the way back to standing `$01`.
                >= 2 and < 28 => (ushort)(SnesButton.Right | SnesButton.X),

                // A fresh Jump edge begins `$4B`; held Shot selects `$13` from that
                // transition table, and retaining X through impact makes `$91:E99B`
                // choose firing landing `$E6` rather than ordinary `$A4`.
                >= 58 and < 70 => (ushort)(SnesButton.A | SnesButton.X),
                >= 70 and < 108 => (ushort)SnesButton.X,

                // The diagnostic walk-off stimulus below runs just before frame 111.
                // Holding X lets `$29`'s literal table select falling fire pose `$67`,
                // then keeps the same horizontal firing direction through landing.
                >= 110 and < 160 => (ushort)SnesButton.X,
                _ => (ushort)0,
            }
        : options.ChargeBeamScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // Begin with a fresh Shot edge after dismissing the timer state. Sixty-five
                // consecutive held samples cross `$90:B856`'s threshold without relying on
                // a host counter; the first blank sample is the real charged-shot trigger.
                >= 2 and < 67 => (ushort)SnesButton.X,
                _ => (ushort)0,
            }
        : options.HyperBeamScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // Hyper Beam is fired by held Shoot in `$90:BCBE`, but the normal projectile
                // cooldown still prevents repeats. One sample keeps this proof isolated and
                // leaves the following frames free to expose native Wave movement and art.
                2 => (ushort)SnesButton.X,
                _ => (ushort)0,
            }
        : options.MissileScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // `$90:BE62` is edge-triggered through `$8F`; one isolated X sample after
                // timer dismissal proves a held desktop button is not being turned into a
                // fabricated auto-fire stream.
                2 => (ushort)SnesButton.X,
                _ => (ushort)0,
            }
        : options.SuperMissileScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                2 => (ushort)SnesButton.X,
                _ => (ushort)0,
            }
        : options.AimAirScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 43 => (ushort)(SnesButton.A | SnesButton.R),
                >= 43 and < 76 => (ushort)SnesButton.R,
                >= 85 and < 126 => (ushort)(SnesButton.A | SnesButton.L),
                >= 126 and < 159 => (ushort)SnesButton.L,
                >= 170 and < 196 => (ushort)SnesButton.Left,
                >= 210 and < 251 => (ushort)(SnesButton.A | SnesButton.R),
                >= 251 and < 286 => (ushort)SnesButton.R,
                >= 295 and < 336 => (ushort)(SnesButton.A | SnesButton.L),
                >= 336 and < 371 => (ushort)SnesButton.L,
                _ => (ushort)0,
            }
        : options.AerialTurnScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // A starts `$4B->$4D`. On the next interval Left is the opposite-facing
                // held bit, so `$91:A2F6` publishes generic `$2F`; `$91:F952` then chooses
                // the exact unaimed record and preserves old momentum during reversal.
                >= 2 and < 10 => (ushort)SnesButton.A,
                >= 10 and < 48 => (ushort)(SnesButton.Left | SnesButton.A),
                >= 48 and < 80 => (ushort)SnesButton.Left,
                _ => (ushort)0,
            }
        : options.CompactAirScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // Begin a right-facing diagonal-up normal jump. Down then selects compact
                // `$17`; R expands back to `$69`; the second Down is retained through the
                // descent so shot direction four must land through ordinary `$A4`.
                >= 2 and < 10 => (ushort)(SnesButton.A | SnesButton.R),
                >= 10 and < 20 => (ushort)(SnesButton.A | SnesButton.Down),
                >= 20 and < 22 => (ushort)(SnesButton.A | SnesButton.R),
                >= 22 and < 82 => (ushort)(SnesButton.A | SnesButton.Down),

                // Complete the grounded turn before starting the mirrored jump. Its Down
                // route is `$18`, shot direction five, and therefore landing pose `$A5`.
                >= 100 and < 126 => (ushort)SnesButton.Left,
                >= 140 and < 148 => (ushort)(SnesButton.A | SnesButton.R),
                >= 148 and < 158 => (ushort)(SnesButton.A | SnesButton.Down),
                >= 158 and < 166 => (ushort)(SnesButton.A | SnesButton.R),
                >= 166 and < 220 => (ushort)(SnesButton.A | SnesButton.Down),
                _ => (ushort)0,
            }
        : options.AimCrouchScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 12 => (ushort)(SnesButton.Down | SnesButton.R),
                >= 12 and < 22 => (ushort)SnesButton.R,
                >= 22 and < 32 => (ushort)SnesButton.L,
                >= 32 and < 42 => (ushort)(SnesButton.R | SnesButton.L),
                >= 60 and < 68 => (ushort)(SnesButton.Up | SnesButton.R),
                >= 90 and < 116 => (ushort)SnesButton.Left,
                >= 130 and < 140 => (ushort)(SnesButton.Down | SnesButton.R),
                >= 140 and < 150 => (ushort)SnesButton.R,
                >= 150 and < 160 => (ushort)SnesButton.L,
                >= 160 and < 170 => (ushort)(SnesButton.R | SnesButton.L),
                >= 190 and < 198 => (ushort)(SnesButton.Up | SnesButton.L),
                _ => (ushort)0,
            }
        : options.AimTurnScript
            ? frameIndex switch
            {
                // Start only dismisses the debug runner's timer state; every later pose
                // still comes from the retail bank-$91 input tables and turn selector.
                0 => (ushort)SnesButton.Start,

                // R alone selects diagonal-up aim. Adding the opposite direction requests
                // generic `$25/$26`; `$91:F8D3` must replace those with `$9C/$9D`.
                >= 2 and < 20 => (ushort)SnesButton.R,
                >= 20 and < 32 => (ushort)(SnesButton.Left | SnesButton.R),
                >= 32 and < 40 => (ushort)SnesButton.R,
                >= 40 and < 52 => (ushort)(SnesButton.Right | SnesButton.R),

                // Up selects straight-up aim. The same generic reversal pair must become
                // `$8B/$8C`, then command `$F8` must land on standing poses `$04/$03`.
                >= 52 and < 78 => (ushort)SnesButton.Up,
                >= 78 and < 90 => (ushort)(SnesButton.Left | SnesButton.Up),
                >= 90 and < 98 => (ushort)SnesButton.Up,
                >= 98 and < 110 => (ushort)(SnesButton.Right | SnesButton.Up),

                // L alone selects diagonal-down aim. These last two reversals exercise
                // `$8D/$8E` and their destinations `$08/$07`.
                >= 110 and < 136 => (ushort)SnesButton.L,
                >= 136 and < 148 => (ushort)(SnesButton.Left | SnesButton.L),
                >= 148 and < 156 => (ushort)SnesButton.L,
                >= 156 and < 168 => (ushort)(SnesButton.Right | SnesButton.L),
                _ => (ushort)0,
            }
        : options.CrouchTurnScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // A single Down edge enters the ordinary crouch. Opposite directions then
                // make `$91:F8D3` select the crouching table's `$43/$44` entries.
                >= 2 and < 10 => (ushort)SnesButton.Down,
                20 => (ushort)SnesButton.Left,
                40 => (ushort)SnesButton.Right,

                // Both shoulders select straight-up crouched aim `$85/$86`.
                >= 60 and < 78 => (ushort)(SnesButton.R | SnesButton.L),
                78 => (ushort)(SnesButton.Left | SnesButton.R | SnesButton.L),
                >= 79 and < 98 => (ushort)(SnesButton.R | SnesButton.L),
                98 => (ushort)(SnesButton.Right | SnesButton.R | SnesButton.L),
                >= 99 and < 110 => (ushort)(SnesButton.R | SnesButton.L),

                // R alone selects diagonal-up crouched aim `$71/$72`.
                >= 110 and < 136 => (ushort)SnesButton.R,
                136 => (ushort)(SnesButton.Left | SnesButton.R),
                >= 137 and < 156 => (ushort)SnesButton.R,
                156 => (ushort)(SnesButton.Right | SnesButton.R),
                >= 157 and < 168 => (ushort)SnesButton.R,

                // L alone selects diagonal-down crouched aim `$73/$74`.
                >= 168 and < 194 => (ushort)SnesButton.L,
                194 => (ushort)(SnesButton.Left | SnesButton.L),
                >= 195 and < 214 => (ushort)SnesButton.L,
                214 => (ushort)(SnesButton.Right | SnesButton.L),
                >= 215 and < 226 => (ushort)SnesButton.L,
                _ => (ushort)0,
            }
        : options.CrouchJumpScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // Enter stable `$27`, then release Down while retaining the facing
                // direction. `$91:A6A0` must install `$01` directly (there is no `$3B`).
                >= 2 and < 10 => (ushort)SnesButton.Down,
                20 => (ushort)SnesButton.Right,

                // Enter ordinary crouch again and press a fresh jump edge. The real table
                // selects `$4B`; `$91:FC8A` supplies its ordinary-crouch ten-pixel lift.
                >= 30 and < 38 => (ushort)SnesButton.Down,
                >= 48 and < 60 => (ushort)SnesButton.A,

                // After the first landing, enter diagonal-up aimed crouch `$71` and jump
                // while R remains held. This intentionally exercises the same `$4B` table
                // result without the literal `$27/$28`-only `$91:FC8A` adjustment.
                >= 120 and < 130 => (ushort)(SnesButton.Down | SnesButton.R),
                >= 130 and < 140 => (ushort)SnesButton.R,
                >= 140 and < 152 => (ushort)(SnesButton.A | SnesButton.R),

                // A final stable crouch/direct exit makes the immediate `$01` seam visible
                // at the end of the trace and in the output PNG.
                >= 220 and < 228 => (ushort)SnesButton.Down,
                238 => (ushort)SnesButton.Right,
                _ => (ushort)0,
            }
        : options.MorphBallScript || options.BombJumpScript || options.PowerBombScript || options.MorphKnockbackScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // The first Down edge selects `$35`, which reaches stable crouch `$27`.
                // Super Metroid does *not* morph from that continuously-held input: its
                // crouching table requires a second newly-pressed Down edge. Release for
                // two frames, press Down again, and let `$37`'s `$F9` choose grounded `$1D`.
                >= 2 and < 8 => (ushort)SnesButton.Down,
                >= 10 and < 20 => (ushort)SnesButton.Down,

                // The bomb-jump route stops moving here and presses the default Shoot/X
                // binding once. Everything after this edge is produced by the translated
                // five-slot projectile lifecycle and cartridge instruction data.
                25 when options.BombJumpScript => (ushort)SnesButton.X,
                25 when options.PowerBombScript => (ushort)SnesButton.X,
                >= 25 and < 80 when options.MorphBallScript => (ushort)SnesButton.Right,
                >= 80 and < 140 when options.MorphBallScript => (ushort)SnesButton.Left,

                // Let command one decelerate to definition fallback `$41`, then Up starts
                // `$3E`; its `$FD $28` endpoint proves the expanded body fits the terrain.
                >= 175 and < 185 when options.MorphBallScript => (ushort)SnesButton.Up,
                _ => (ushort)0,
            }
        : options.SpringBallScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 8 => (ushort)SnesButton.Down,
                >= 10 and < 20 => (ushort)SnesButton.Down,
                >= 25 and < 55 => (ushort)SnesButton.Right,

                // A fresh Jump edge from `$79/$7B` selects `$7F`. Hold briefly for a
                // visible ascent, then release to exercise the native variable-height cut.
                >= 60 and < 72 => (ushort)SnesButton.A,
                _ => (ushort)0,
            }
        : frameIndex switch
        {
            0 => (ushort)SnesButton.Start,
            >= 2 when frameIndex - 2 < options.RightFrameCount => (ushort)SnesButton.Right,
            _ => (ushort)0,
        };

    if (options.GunshipScript)
        controllerInput = frameIndex == 0 ? (ushort)SnesButton.Down : (ushort)0;

    if (options.KnockbackScript && frameIndex == 20)
    {
        // No enemy subsystem exists in the playable slice yet. This is therefore an
        // explicit debugger stimulus at exactly bank-$A0's producer/consumer seam: one
        // means the damage source was left of Samus, so physical knockback moves right.
        // Start() reads pose direction, movement type, speeds, radii, and animation from
        // the cartridge and installs the same `$90:DF38` handler as command one.
        SamusKnockbackMovement.Start(
            bus,
            runtime.Samus,
            controllerInput: 0,
            knockbackXDirection: 1);
        observedSamusPoses.Add(runtime.Samus.Pose);
        Console.WriteLine(
            $"frame {frameIndex + 1,4}: injected bank-$A0 knockback X direction 1; " +
            $"pose=${runtime.Samus.Pose:X2}, direction={runtime.Samus.KnockbackDirection}, " +
            $"timer={runtime.Samus.KnockbackTimer}.");
    }

    if (options.MorphKnockbackScript && frameIndex == 25)
    {
        // As in the humanoid diagnostic, this publishes only bank-$A0's untranslated
        // damage-source side. Use leftward X side zero against right-facing `$1D`, and pass
        // held-forward Right directly to command one: `$91:EE27` must still choose vertical
        // direction two from pose facing while `$90:8EDF` independently moves left.
        if (runtime.Samus.Pose != SamusPoseIds.MorphBallGroundRightPose)
        {
            throw new InvalidOperationException(
                $"Morphed knockback stimulus expected stable pose $1D, not ${runtime.Samus.Pose:X2}.");
        }

        byte poseBeforeHit = runtime.Samus.Pose;
        ushort animationFrameBeforeHit = runtime.Samus.AnimationFrame;
        ushort animationTimerBeforeHit = runtime.Samus.AnimationFrameTimer;
        morphKnockbackGroundY = runtime.Samus.YPosition;
        SamusKnockbackMovement.Start(
            bus,
            runtime.Samus,
            controllerInput: (ushort)SnesButton.Right,
            knockbackXDirection: 0);
        observedMorphedKnockbackPosePreserved = runtime.Samus.Pose == poseBeforeHit;
        observedMorphedKnockbackAnimationPreserved =
            runtime.Samus.AnimationFrame == animationFrameBeforeHit &&
            runtime.Samus.AnimationFrameTimer == animationTimerBeforeHit;
        observedMorphedKnockbackDirectionRule =
            runtime.Samus.KnockbackDirection == 2 &&
            runtime.Samus.KnockbackXDirection == 0;
        observedSamusPoses.Add(runtime.Samus.Pose);
        Console.WriteLine(
            $"frame {frameIndex + 1,4}: injected bank-$A0 X side 0 into Morph Ball; " +
            $"pose=${runtime.Samus.Pose:X2}, animation=" +
            $"{runtime.Samus.AnimationFrame}/{runtime.Samus.AnimationFrameTimer}, " +
            $"direction={runtime.Samus.KnockbackDirection}, timer={runtime.Samus.KnockbackTimer}.");
    }

    if (options.GunExtendedScript && frameIndex == 110)
    {
        // Landing Site's convenient debug floor has no nearby safe ledge. Publish only the
        // already-translated `$91:E8F2` walk-off producer seam: lift the host diagnostic body
        // two blocks, then invoke the same collision-command-five transition a missing floor
        // would have selected. Gravity, `$29 -> $67` input matching, terrain collision,
        // `$91:E99B` held-Shot landing selection, animation, tile DMA, and art stay native.
        if (!SamusState.IsRightFacingStandingPose(runtime.Samus.Pose))
        {
            throw new InvalidOperationException(
                $"Gun-extension walk-off stimulus expected right-facing standing, not ${runtime.Samus.Pose:X2}.");
        }

        runtime.Samus.YPosition = unchecked((ushort)(runtime.Samus.YPosition - 32));
        byte fallingPose = runtime.Samus.SelectFallingPoseForCurrentAim(bus);
        runtime.Samus.ApplyWalkedOffFloorTransition(bus, runtime.LevelData!, fallingPose);
        observedSamusPoses.Add(runtime.Samus.Pose);
        Console.WriteLine(
            $"frame {frameIndex + 1,4}: published `$91:E8F2` walk-off at Y={runtime.Samus.YPosition}; " +
            $"pose=${runtime.Samus.Pose:X2}, next held Shot must select $67.");
    }

    if (options.DrainedSamusScript)
    {
        // These four calls are the exact A9 actor-to-bank-91 interface. Conditions keep the
        // runner deterministic even if room collision changes the fall duration; the lower
        // bounds retain visible time in every stable ROM animation before the next command.
        if (!issuedDrainedStandingCommand && frameIndex >= 50 &&
            runtime.Samus.Drained.Phase == DrainedSamusPhase.OnFloor)
        {
            runtime.Samus.Drained.PutStanding(bus, runtime.Samus);
            issuedDrainedStandingCommand = true;
            Console.WriteLine($"frame {frameIndex + 1,4}: actor called drained controller 1 (standing).");
        }
        else if (!issuedDrainedCrouchingCommand && frameIndex >= 90 &&
                 runtime.Samus.Drained.Phase == DrainedSamusPhase.Standing)
        {
            runtime.Samus.Drained.PutCrouchingOrFalling(bus, runtime.Samus);
            issuedDrainedCrouchingCommand = true;
            Console.WriteLine($"frame {frameIndex + 1,4}: actor called drained controller 4 (crouching/falling).");
        }
        else if (!issuedDrainedReleaseCommand && frameIndex >= 120 &&
                 runtime.Samus.Drained.Phase == DrainedSamusPhase.Crouching)
        {
            runtime.Samus.Drained.Release(bus, runtime.Samus);
            issuedDrainedReleaseCommand = true;
            Console.WriteLine($"frame {frameIndex + 1,4}: actor called drained controller 2 (release).");
        }
        else if (!issuedDrainedHyperBeamCommand && frameIndex >= 150)
        {
            runtime.Samus.Drained.EnableHyperBeam(runtime.Samus);
            issuedDrainedHyperBeamCommand = true;
            Console.WriteLine($"frame {frameIndex + 1,4}: actor called drained controller 3 (hyper beam).");
        }
    }

    MotherBrainForcedSamusMovementResult? rainbowSamusMovement = null;
    if (rainbowAttack is not null)
    {
        // The retail outer frame loop calls `$80:8111` exactly once before dispatching
        // gameplay state. Earlier versions of this isolated encounter accidentally left
        // `$05E5` frozen at its reset seed, preventing `$C15C`'s sign-bit attack gate from
        // ever firing. Advancing the translated LFSR here supplies that missing global
        // producer without substituting a hand-authored boss random sequence.
        ushort motherBrainRandom = runtime.System.NextRandom();

        // Enemy AI executes before the ordinary enemy-instruction stage. Keep those calls
        // separate so `$B92B` can observe the pose/X values published by the previous frame's
        // retail `$A9:993A` opcode, just as the SNES scheduler does.
        MotherBrainRainbowBeamAttackStepResult actorResult = rainbowAttack.Step(
            bus,
            runtime.Samus,
            enemyFrameCounter: unchecked((ushort)frameIndex),
            mainEnemyExecutionCounter: unchecked((ushort)frameIndex),
            randomNumberSeed: motherBrainRandom,
            // Death explosions call the global generator once per simultaneous projectile.
            // Supplying the live owner keeps every later actor's random stream synchronized.
            nextRandomNumber: runtime.System.NextRandom);
        rainbowSamusMovement = actorResult.Movement;
        observedRainbowPhases.Add(actorResult.PhaseBefore);
        observedRainbowPhases.Add(actorResult.PhaseAfter);

        if (actorResult.SpriteTileTransfer is { } actorTiles)
        {
            // Feed either actor-owned transfer list into the ordinary WRAM queue before NMI.
            // `$8FE5` loads the Baby; `$9003` later replaces six pages with corpse graphics.
            runtime.VramWrites.Enqueue(
                actorTiles.Size,
                checked((int)actorTiles.SourceAddress),
                actorTiles.VramDestination);
            bool corpseTiles = actorResult.PhaseBefore ==
                MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles;
            if (corpseTiles)
                observedMotherBrainCorpseTileTransfers.Add(actorTiles);
            else
                observedBabyTileTransfers.Add(actorTiles);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: {(corpseTiles ? "Mother Brain corpse" : "Baby")} " +
                $"tile transfer {actorTiles.EntryIndex}: ${actorTiles.SourceAddress:X6} -> " +
                $"VRAM ${actorTiles.VramDestination:X4}, ${actorTiles.Size:X4} bytes.");
        }
        observedBabySpawnRequest |= actorResult.BabySpawnRequested;
        observedFinalBeamSound |= actorResult.FinalBeamSoundQueued;
        if (actorResult.Phase3Attack is { } phaseThreeAttack)
        {
            observedPhaseThreeAttacks.Add(phaseThreeAttack);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: phase-three attack {phaseThreeAttack}; " +
                $"RNG=${motherBrainRandom:X4}, head=${rainbowAttack.HeadInstructionList:X4}, " +
                $"walk={rainbowAttack.Phase3WalkingPhase}/" +
                $"${rainbowAttack.Phase3WalkCounter:X4}.");
        }
        foreach (MotherBrainDeathExplosionRequest deathExplosion in actorResult.DeathExplosions)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain death explosion pattern " +
                $"{deathExplosion.PatternIndex} at ({deathExplosion.XPosition}," +
                $"{deathExplosion.YPosition}), offset ({deathExplosion.XOffset}," +
                $"{deathExplosion.YOffset}), parameter {deathExplosion.ProjectileParameter}, " +
                $"SFX ${deathExplosion.SoundEffect:X2}.");
        }
        foreach (MotherBrainSpriteTileTransferRequest transfer in
                 actorResult.CorpseRottingVramTransfers)
        {
            // Unlike the earlier six frame-spread corpse loads, `$A9:E1F4` appends all six
            // changing WRAM slices every active rotting frame. The standard NMI consumer
            // therefore sees precisely the same sources and encoded VRAM destinations.
            runtime.VramWrites.Enqueue(
                transfer.Size,
                checked((int)transfer.SourceAddress),
                transfer.VramDestination);
        }
        foreach (MotherBrainSpriteTileTransferRequest transfer in
                 actorResult.EscapeSequenceTileTransfers)
        {
            runtime.VramWrites.Enqueue(
                transfer.Size,
                checked((int)transfer.SourceAddress),
                transfer.VramDestination);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape-sequence tile transfer " +
                $"${transfer.SourceAddress:X6} -> VRAM ${transfer.VramDestination:X4}, " +
                $"${transfer.Size:X4} bytes.");
        }
        foreach (MotherBrainCorpseDustRequest dust in actorResult.CorpseDustRequests)
        {
            // `$A9:E23A` performs this allocation from the body enemy's instruction
            // interpreter. Enemy projectiles have not run yet, so placing the new slot in
            // the shared pool here lets the later high-to-low projectile pass consume its
            // first animation frame on the native spawn frame.
            int? slot = motherBrainProjectiles!.SpawnMiscDust(
                bus,
                dust.XPosition,
                dust.YPosition,
                dust.ProjectileParameter);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain corpse row {dust.EntryIndex} " +
                $"finished at dust ({dust.XPosition},{dust.YPosition}), " +
                $"parameter ${dust.ProjectileParameter:X4}, " +
                $"slot={slot?.ToString() ?? "full"}" +
                (dust.SoundEffectQueued ? $", SFX ${dust.SoundEffect:X2}." : "."));
        }
        if (actorResult.MusicStopQueued || actorResult.EscapeMusicQueued)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain corpse finished; queued music " +
                $"${(actorResult.MusicStopQueued ? 0x0000 : 0xffff):X4} then " +
                $"${(actorResult.EscapeMusicQueued ? 0xff24 : 0xffff):X4}.");
        }
        if (actorResult.EscapeTypewriterSetupRequested)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape start initialized; palette copy=" +
                $"{actorResult.ExplodedDoorPaletteRequested}, music7=" +
                $"{actorResult.EscapeMusicTrackQueued}, palette FX=" +
                $"{string.Join(',', actorResult.EscapePaletteFxRequests.Select(x => $"${x:X4}"))}.");
        }
        if (actorResult.EscapeDoorExplosion is { } doorExplosion)
        {
            // The door's periodic producer is body AI as well. These room coordinates are
            // deliberately near X zero; the generic dust pre-instruction, rather than the
            // runner, decides whether the origin lies inside the current layer-1 window.
            int? slot = motherBrainProjectiles!.SpawnMiscDust(
                bus,
                doorExplosion.XPosition,
                doorExplosion.YPosition,
                doorExplosion.ProjectileParameter);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape-door dust pattern " +
                $"{doorExplosion.PatternIndex} at ({doorExplosion.XPosition}," +
                $"{doorExplosion.YPosition}), parameter ${doorExplosion.ProjectileParameter:X4}, " +
                $"slot={slot?.ToString() ?? "full"}, SFX ${doorExplosion.SoundEffect:X2}.");
        }
        if (actorResult.TimeBombSetSubtitleSpawnRequested)
        {
            int? slot = motherBrainProjectiles!.SpawnTimeBombSetSubtitle();
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: alternate time-bomb subtitle allocated " +
                $"slot {slot?.ToString() ?? "full"}.");
        }
        if (actorResult.MotherBrainEscapeTimerStartRequested)
        {
            // This is the concrete TimerStatus `$0002` consumer. Boss-area bits remain a
            // logged request because this isolated route is hosted in Landing Site and must
            // not corrupt Crateria's area byte while pretending it is Tourian.
            runtime.EscapeTimer.RequestMotherBrainStart();
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: enabled Samus timer handling and requested " +
                $"Mother Brain escape timer; bossBit={actorResult.MotherBrainBossBitRequested}, " +
                $"event0E={actorResult.ZebesTimebombEventRequested}.");
        }
        foreach (MotherBrainEscapeDoorParticleSpawnRequest particleRequest in
                 actorResult.EscapeDoorParticleSpawns)
        {
            int? slot = motherBrainProjectiles!.SpawnEscapeDoorParticle(particleRequest);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape-door fragment " +
                $"{particleRequest.Parameter} allocated slot {slot?.ToString() ?? "full"}.");
        }
        if (actorResult.EscapeDoorPlm is { } escapeDoorPlm)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: requested escape-door PLM " +
                $"${escapeDoorPlm.PlmEntry:X4} at block " +
                $"({escapeDoorPlm.BlockX},{escapeDoorPlm.BlockY}).");
        }

        if (actorResult.BabySpawnRequested)
        {
            // Native `$A9:BE1B` spawns population record `$BE28`, whose initialization AI
            // overwrites the record's coordinates and installs `$C7CC`. The runner creates
            // exactly that translated enemy once; its later movement reads the supplied
            // cartridge's real `$A0:B443` sine table on every curved-flight call.
            if (cutsceneBaby is not null)
                throw new InvalidOperationException("Mother Brain requested the cutscene Baby more than once.");
            cutsceneBaby = new BabyMetroidCutsceneState();
            cutsceneBaby.Initialize();
            previousBabyPhase = cutsceneBaby.Phase;
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: initialized cutscene Baby at " +
                $"({cutsceneBaby.XPosition},{cutsceneBaby.YPosition}); " +
                $"phase={cutsceneBaby.Phase}, timer=${cutsceneBaby.FunctionTimer:X4}.");
        }

        MotherBrainBodyAnimationStepResult bodyResult = rainbowAttack.Body.Step(bus);
        observedPhaseThreeForwardMovement |=
            (actorResult.PhaseBefore is MotherBrainRainbowBeamAttackPhase.Phase3FightingMain or
                MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown) &&
            unchecked((short)(bodyResult.XAfter - bodyResult.XBefore)) > 0;

        // Mother Brain's brain is the next occupied enemy slot after her body. Its `$91B8`
        // handler advances the two neck angles before the later cutscene-Baby slot polls the
        // corpse flag. This ordering is observable when `$BF56` waits for both raises to end.
        rainbowAttack.StepNeckMovement(bus, runtime.Samus);

        // Mother Brain's head animation is part of the brain enemy slot and therefore runs
        // after its main/neck AI but before the later Baby slot. It must continue after that
        // cutscene enemy deletes itself: phase-three's `$9F00` list owns the bomb attack.
        // Head opcodes only allocate here; the gameplay projectile pass below advances every
        // newly occupied bank-$86 slot after all enemy slots have finished.
        MotherBrainHeadAnimationStepResult headResult = rainbowAttack.StepHeadAnimation(
            bus,
            runtime.Samus,
            cutsceneBaby,
            runtime.System.RandomNumber);
        if (headResult.OnionRingSpawn is { } ringRequest)
        {
            int? slot = motherBrainProjectiles!.Spawn(bus, rainbowAttack, ringRequest);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain head spawned onion ring " +
                $"angle=${ringRequest.Angle:X2}, slot={slot?.ToString() ?? "full"}, " +
                $"head=${headResult.InstructionPointerAfter:X4}.");
        }
        if (headResult.BombSpawn is { } bombRequest)
        {
            int? slot = motherBrainProjectiles!.SpawnBomb(rainbowAttack, bombRequest);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain head spawned bomb " +
                $"afterburn={bombRequest.AfterburnCount}, slot={slot?.ToString() ?? "full"}, " +
                $"activeBombs={rainbowAttack.BombCounter}, " +
                $"head=${headResult.InstructionPointerAfter:X4}.");
        }
        if (headResult.PurpleBreathBigSpawnRequested)
        {
            int? slot = motherBrainProjectiles!.SpawnPurpleBreathBig(rainbowAttack);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain head spawned large purple breath " +
                $"slot={slot?.ToString() ?? "full"}.");
        }
        if (rainbowAttack.Phase != previousRainbowPhase)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: rainbow actor {previousRainbowPhase} -> " +
                $"{rainbowAttack.Phase}; timer=${rainbowAttack.FunctionTimer:X4}, " +
                $"body=({rainbowAttack.Body.XPosition},{rainbowAttack.Body.YPosition})/" +
                $"pose {rainbowAttack.Body.Pose}, head=${rainbowAttack.HeadInstructionList:X4}, " +
                $"width=${rainbowAttack.AngularWidth:X4}.");
            previousRainbowPhase = rainbowAttack.Phase;
        }
        if (bodyResult.FootstepRequested)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain body opcode footstep at " +
                $"({bodyResult.XAfter},{bodyResult.YAfter}); earthquake " +
                $"{bodyResult.EarthquakeType}/{bodyResult.EarthquakeTimer}.");
        }

        if (cutsceneBaby is { IsDeleted: false })
        {
            // Body slot zero executes before the later spawned enemy slot. This placement
            // also means the spawn frame can run the newly initialized Baby once, matching
            // the native increasing-slot enemy loop rather than adding an invented delay.
            // Blue-ring projectiles run after enemies, so hits from the preceding frame are
            // consumed here. Native `$A9:C6C8` clears the shared request word and queues one
            // cry even when several rings incremented it before this enemy turn.
            int pendingBabyCries = motherBrainProjectiles!.ConsumePendingBabyCries();
            if (pendingBabyCries != 0)
            {
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby consumes Mother Brain ring cry request " +
                    $"({pendingBabyCries} hit{(pendingBabyCries == 1 ? string.Empty : "s")}).");
            }
            BabyMetroidCutsceneStepResult babyResult = cutsceneBaby.Step(
                bus,
                runtime.Samus,
                rainbowAttack,
                layer1X: 0,
                layer1Y: 0,
                enemyFrameCounter: unchecked((ushort)frameIndex),
                randomNumber: runtime.System.RandomNumber);
            observedBabyPhases.Add(babyResult.PhaseBefore);
            observedBabyPhases.Add(babyResult.PhaseAfter);
            observedBabyMotherBrainInterrupt |= babyResult.MotherBrainInterrupted;
            observedBabyCeilingTableInstall |=
                babyResult.PhaseAfter == BabyMetroidCutscenePhase.MoveToSamus &&
                babyResult.MovementTablePointer == BabyMetroidCutsceneState.CeilingToSamusMovementTable;
            if (babyResult.HealingCompleted)
            {
                observedBabyHealingCompletion = true;
                observedBabyHealingCompletionFrame = frameIndex + 1;
                observedBabyHealingCompletionHealth = runtime.Samus.Health;
                observedBabyHealingCompletionReserveEnergy = runtime.Samus.ReserveEnergy;
            }
            observedBabyPhaseThreeHandoff |= babyResult.PhaseThreeHandoff;
            if (babyResult.PhaseThreeHandoff)
            {
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby deleted itself, restored Hyper Beam, " +
                    "and handed Mother Brain to phase-three recovery.");
            }

            foreach (BabyMetroidReleaseDustRequest dust in babyResult.ReleaseDustClouds)
            {
                // The Baby occupies a later enemy slot than Mother Brain. Its three helper
                // calls execute in list order and each allocator resumes at slot seventeen,
                // exactly like `$A9:C98C-C9C2`; all successful allocations are therefore
                // present before this frame's shared enemy-projectile pass begins.
                int? slot = motherBrainProjectiles!.SpawnMiscDust(
                    bus,
                    dust.XPosition,
                    dust.YPosition,
                    dust.ProjectileParameter);
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby release dust at " +
                    $"({dust.XPosition},{dust.YPosition}), parameter " +
                    $"${dust.ProjectileParameter:X4}, slot={slot?.ToString() ?? "full"}.");
            }

            if (babyResult.DeathExplosion is { } deathExplosion)
            {
                observedBabyDeathExplosions++;
                int? slot = motherBrainProjectiles!.SpawnMiscDust(
                    bus,
                    deathExplosion.XPosition,
                    deathExplosion.YPosition,
                    deathExplosion.ProjectileParameter);
                if (slot is not null)
                    allocatedBabyDeathExplosions++;
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby death explosion pattern " +
                    $"{deathExplosion.PatternIndex} at ({deathExplosion.XPosition}," +
                    $"{deathExplosion.YPosition}); projectile parameter " +
                    $"${deathExplosion.ProjectileParameter:X4}, " +
                    $"slot={slot?.ToString() ?? "full"}, SFX ${deathExplosion.SoundEffect:X2}.");
            }

            if (babyResult.BabyPaletteTransfer is { } babyPalette)
            {
                // The native destination `$01E2` is a byte offset into the 512-byte
                // palette buffer; SnesCgram accepts an actual colour number, hence `/2`.
                runtime.Cgram.LoadFromBus(
                    bus,
                    checked((int)babyPalette.SourceAddress),
                    babyPalette.ColorCount,
                    babyPalette.DestinationColorIndex / 2);
                observedBabyDeathPalettes.Add(babyPalette);
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby black-fade palette " +
                    $"{babyPalette.PaletteIndex} from ${babyPalette.SourceAddress:X6}.");
            }

            if (babyResult.AttackTileTransfer is { } attackTiles)
            {
                // The same ordinary pre-NMI queue used for the earlier Baby graphics now
                // restores Mother Brain's four attack rows from bank `$B7`.
                runtime.VramWrites.Enqueue(
                    attackTiles.Size,
                    checked((int)attackTiles.SourceAddress),
                    attackTiles.VramDestination);
                observedAttackTileTransfers.Add(attackTiles);
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Mother Brain attack tile transfer " +
                    $"{attackTiles.EntryIndex}: ${attackTiles.SourceAddress:X6} -> " +
                    $"VRAM ${attackTiles.VramDestination:X4}.");
            }

            if (babyResult.BackgroundPaletteTransfer is { } backgroundPalette)
            {
                // `$AD:F24B` writes fourteen background-palette-three colours followed by
                // fourteen background-palette-five colours from consecutive source words.
                int source = checked((int)backgroundPalette.SourceAddress);
                int colors = backgroundPalette.ColorsPerDestination;
                runtime.Cgram.LoadFromBus(
                    bus,
                    source,
                    colors,
                    backgroundPalette.FirstDestinationColorIndex / 2);
                runtime.Cgram.LoadFromBus(
                    bus,
                    source + colors * 2,
                    colors,
                    backgroundPalette.SecondDestinationColorIndex / 2);
                observedPhaseThreeBackgroundPalettes.Add(backgroundPalette);
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: phase-three room-light palette " +
                    $"{backgroundPalette.PaletteIndex} from ${backgroundPalette.SourceAddress:X6}.");
            }

            if (cutsceneBaby.Phase != previousBabyPhase)
            {
                if (cutsceneBaby.Phase == BabyMetroidCutscenePhase.LatchOntoSamus)
                    observedBabyLatchOntoSamusFrame = frameIndex + 1;
                if (cutsceneBaby.Phase == BabyMetroidCutscenePhase.HealSamusToFullHealth)
                    observedBabyHealSamusFrame = frameIndex + 1;
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby actor {previousBabyPhase} -> " +
                    $"{cutsceneBaby.Phase}; position=({cutsceneBaby.XPosition:X4}." +
                    $"{cutsceneBaby.XSubposition:X4},{cutsceneBaby.YPosition:X4}." +
                    $"{cutsceneBaby.YSubposition:X4}), velocity=" +
                    $"({cutsceneBaby.XVelocity:X4},{cutsceneBaby.YVelocity:X4}), " +
                    $"angle/speed=${cutsceneBaby.Angle:X4}/${cutsceneBaby.Speed:X4}.");
                previousBabyPhase = cutsceneBaby.Phase;
            }
            if (cutsceneBaby.MovementTablePointer != previousBabyMovementTablePointer)
            {
                // The route advances through overlapping eight-byte ROM records. Logging
                // the literal pointer makes each leg independently breakpointable and also
                // exposes the final `$CA5C + 8 == $CA64` function-pointer overlay.
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby movement table " +
                    $"${previousBabyMovementTablePointer:X4} -> " +
                    $"${cutsceneBaby.MovementTablePointer:X4}; position=" +
                    $"({cutsceneBaby.XPosition:X4}.{cutsceneBaby.XSubposition:X4}," +
                    $"{cutsceneBaby.YPosition:X4}.{cutsceneBaby.YSubposition:X4}).");
                observedBabyRouteFrames[cutsceneBaby.MovementTablePointer] = frameIndex + 1;
                observedBabyRoutePoints[cutsceneBaby.MovementTablePointer] = babyResult.After;
                previousBabyMovementTablePointer = cutsceneBaby.MovementTablePointer;
            }
        }

        // GameState_8 runs EprojRunAll after EnemyMain. This ordering lets a ring spawned by
        // the head above receive delay call one and animation frame one immediately, while
        // any collision it produces is not visible to the Baby's `$CABD` until next frame.
        MotherBrainEnemyProjectileFrameResult ringResult = motherBrainProjectiles!.StepFrame(
            bus,
            rainbowAttack,
            cutsceneBaby,
            runtime.Samus,
            layer1X: 0,
            samusBombs: runtime.BombProjectiles);
        foreach (MotherBrainOnionRingEvent ringEvent in ringResult.Events)
        {
            string target = ringEvent.Collision switch
            {
                MotherBrainOnionRingCollisionKind.BabyMetroid or
                    MotherBrainOnionRingCollisionKind.DeletedAfterBabyDeath => "Baby",
                MotherBrainOnionRingCollisionKind.Samus => "Samus",
                _ => "target",
            };
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: onion ring slot {ringEvent.SlotIndex} " +
                $"{ringEvent.Collision} at ({ringEvent.XPosition},{ringEvent.YPosition}); " +
                $"{target} HP ${ringEvent.TargetHealthBefore:X4}->" +
                $"${ringEvent.TargetHealthAfter:X4}.");
        }
        foreach (MotherBrainEscapeDoorParticleDustRequest dust in
                 ringResult.EscapeDoorDustRequests)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape-door fragment slot " +
                $"{dust.SourceSlotIndex} expired at ({dust.XPosition},{dust.YPosition}); " +
                $"spawn misc dust parameter ${dust.ProjectileParameter:X4}.");
        }
        foreach (MotherBrainBombEvent bombEvent in ringResult.BombEvents)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain bomb slot {bombEvent.SlotIndex} " +
                $"{bombEvent.Kind} at ({bombEvent.XPosition},{bombEvent.YPosition}); " +
                $"bounceOffset=${bombEvent.BounceTableOffset:X2}, " +
                $"afterburn={(bombEvent.AfterburnCount?.ToString() ?? "none")}, " +
                $"dust=${bombEvent.DustParameter:X2}, drops={bombEvent.EnemyDropRequested}, " +
                $"SFX={(bombEvent.QueuedSoundLibraryThree is { } sound ? $"${sound:X2}" : "none")}.");
        }

        // The enemy graphics hook runs after actor processing. Its shake countdown is not a
        // body-AI timer: decrementing here lets the following `$BE96` call perform the exact
        // zero-to-fifty refresh instead of leaving the head frozen on one shake-table entry.
        rainbowAttack.StepBrainShakeForDraw();
    }

    // This isolated phase-three script keeps Mother Brain's native room-space coordinates,
    // whose layer-1 origin is (0,0), while the reusable Landing Site runtime has its own
    // camera at X=$0400. Inject both enemy-projectile passes with the actor's coordinate
    // system instead of incorrectly subtracting the unrelated playable-room camera.
    Action<OamBuffer>? drawHighPriorityEnemyProjectiles = motherBrainProjectiles is null
        ? null
        : oam => motherBrainProjectiles.DrawHighPriority(bus, oam, layer1X: 0, layer1Y: 0);
    Action<OamBuffer>? drawLowPriorityEnemyProjectiles = motherBrainProjectiles is null
        ? null
        : oam => motherBrainProjectiles.DrawLowPriority(bus, oam, layer1X: 0, layer1Y: 0);
    RuntimeFrameResult result = runtime.StepFrame(
        controllerInput,
        drawHighPriorityEnemyProjectiles,
        drawLowPriorityEnemyProjectiles);

    if (options.GunshipScript && runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.None)
    {
        GunshipFrameEvent gunshipEvent = runtime.Enemies.LastGunshipEvent;
        int expectedEventFrame = gunshipEvent switch
        {
            GunshipFrameEvent.EntryStarted => 1,
            GunshipFrameEvent.EntryPadClosing => 170,
            GunshipFrameEvent.SavePromptRequested => 318,
            GunshipFrameEvent.ExitPadClosing => 487,
            GunshipFrameEvent.ExitCompleted => 631,
            _ => frameIndex + 1,
        };
        if (frameIndex + 1 != expectedEventFrame)
        {
            throw new InvalidOperationException(
                $"Gunship {gunshipEvent} occurred on frame {frameIndex + 1}, " +
                $"not private-ROM frame {expectedEventFrame}.");
        }
        Console.WriteLine(
            $"frame {frameIndex + 1,4}: gunship {gunshipEvent}; " +
            $"function=$A2:{gunshipTop.VariableF:X4}, pad=${gunshipPad.SpritemapPointer:X4}, " +
            $"Samus=({runtime.Samus.XPosition:X4},{runtime.Samus.YPosition:X4}).");
        if (gunshipEvent == GunshipFrameEvent.SavePromptRequested)
        {
            // The focused script chooses No at message box $1C. Both retail answers run
            // the same opening/raising/closing animation; only Yes requests the still-
            // external SRAM persistence operation.
            if (runtime.Samus.Health != runtime.Samus.MaxHealth ||
                runtime.Samus.Missiles != runtime.Samus.MaxMissiles ||
                runtime.Samus.SuperMissiles != runtime.Samus.MaxSuperMissiles ||
                runtime.Samus.PowerBombs != runtime.Samus.MaxPowerBombs)
            {
                throw new InvalidOperationException(
                    "Gunship requested its save prompt before restoring every resource.");
            }
            runtime.Enemies.AnswerGunshipSavePrompt(save: false);
        }
        else if (gunshipEvent == GunshipFrameEvent.ExitCompleted)
        {
            observedGunshipExit = true;
        }
    }

    if (frameIndex == 0 && options.GroundedRun)
    {
        if (options.GunshipScript)
        {
            // These assertions deliberately run after accepted NMI, not immediately after
            // RoomEnemySystem.Load. The latter used to pass while the deferred standard OBJ
            // and BG3 uploads subsequently erased both room-owned regions. Lock the first
            // and last bytes of each gunship allocation plus both ends of door $896A's sky
            // page so future queue-order regressions fail before producing another bad PNG.
            if (runtime.Vram.ReadByte(0xe000) != bus.ReadByte(0xadb600) ||
                runtime.Vram.ReadByte(0xefff) != bus.ReadByte(0xadc5ff) ||
                runtime.Vram.ReadByte(0xf000) != bus.ReadByte(0xadb600) ||
                runtime.Vram.ReadByte(0xf1ff) != bus.ReadByte(0xadb7ff) ||
                runtime.Vram.ReadWord(0x4800) != unchecked((ushort)(
                    bus.ReadByte(0x8ad180) | (bus.ReadByte(0x8ad181) << 8))) ||
                runtime.Vram.ReadWord(0x4bff) != unchecked((ushort)(
                    bus.ReadByte(0x8ad97e) | (bus.ReadByte(0x8ad97f) << 8))))
            {
                throw new InvalidOperationException(
                    "First NMI overwrote room-owned gunship graphics or scrolling-sky tilemap data.");
            }
        }

        // The first active call proves three independent operations happened in native
        // order: $A759 bobbed every component down one pixel from ROM table $A2:A7CF,
        // property $2000 ran each instruction list, and each duration/map pair replaced
        // the loader's temporary empty $804D spritemap.
        if (gunshipTop.YPosition != 0x0460 ||
            gunshipBottom.YPosition != 0x0488 ||
            gunshipPad.YPosition != 0x045f ||
            gunshipTop.SpritemapPointer != 0xad81 ||
            gunshipBottom.SpritemapPointer != 0xaddd ||
            gunshipPad.SpritemapPointer != 0xafdd ||
            gunshipTop.InstructionTimer != 1 ||
            gunshipBottom.InstructionTimer != 1 ||
            gunshipPad.InstructionTimer != (options.GunshipScript
                ? unchecked((ushort)(bus.ReadByte(0xa2a5be) | (bus.ReadByte(0xa2a5bf) << 8)))
                : (ushort)8))
        {
            throw new InvalidOperationException(
                "Gunship frame one disagrees with native bob/instruction-list state.");
        }

        if (motherBrainProjectiles is null && !runtime.Samus.DeathSequence.IsActive)
        {
            // Gunship layer two is the first normal actor layer. Recompute its first object
            // directly from the ROM's five-byte $A2:AD81 entry and the post-scroll camera;
            // this independently checks the base-tile/palette arithmetic and OAM packing.
            ushort encodedX = unchecked((ushort)(
                bus.ReadByte(0xa2ad83) | (bus.ReadByte(0xa2ad84) << 8)));
            byte encodedY = bus.ReadByte(0xa2ad85);
            ushort sourceAttributes = unchecked((ushort)(
                bus.ReadByte(0xa2ad86) | (bus.ReadByte(0xa2ad87) << 8)));
            ushort originX = unchecked((ushort)(gunshipTop.XPosition - runtime.Camera!.XPosition));
            ushort originY = unchecked((ushort)(gunshipTop.YPosition - runtime.Camera.YPosition));
            ushort expectedAttributes = unchecked((ushort)(
                sourceAttributes + gunshipTop.VramTilesIndex));
            expectedAttributes |= gunshipTop.PaletteIndex;
            OamEntry firstEnemyObject = runtime.Oam.GetEntry(0);
            if (firstEnemyObject.X != ((originX + encodedX) & 0x01ff) ||
                firstEnemyObject.Y != unchecked((byte)(originY + encodedY)) ||
                firstEnemyObject.TileNumber != (expectedAttributes & 0x01ff) ||
                firstEnemyObject.Palette != ((expectedAttributes >> 9) & 7) ||
                firstEnemyObject.Priority != ((expectedAttributes >> 12) & 3) ||
                firstEnemyObject.IsLarge != ((encodedX & 0x8000) != 0))
            {
                throw new InvalidOperationException(
                    "Gunship's first ROM spritemap object was not packed into OAM exactly.");
            }
        }

        Console.WriteLine(
            "frame    1: gunship bob $A2:A7CF applied; maps=$AD81/$ADDD/$AFDD; " +
            $"OAM sprites={runtime.Oam.LastFinalizedSpriteCount}.");
    }
    observedSamusPoses.Add(runtime.Samus.Pose);
    if (options.ElevatorScript)
    {
        if ((result.FrameNumber & 1) == 0)
            observedElevatorVisibleEvenFrame |= runtime.LastSamusBodyDrawn;
        else
            observedElevatorHiddenOddFrame |= !runtime.LastSamusBodyDrawn;

        // Elevator display `$90:EB86` never reaches either arm-cannon drawing mode. The
        // cover state update still ran before dispatch, but no separate OBJ or tile-$1F
        // transfer may leak into this handler's deliberately minimal body presentation.
        if (runtime.LastArmCannonDraw.SpriteWritten ||
            runtime.LastArmCannonDraw.TileUploadQueued)
        {
            throw new InvalidOperationException(
                $"Elevator frame {result.FrameNumber} incorrectly drew or uploaded the independent arm cannon.");
        }
    }
    if (options.VisorScript &&
        runtime.LastVisorPaletteStep is { Action: SamusVisorPaletteAction.ColorWritten } visor)
    {
        // The host supplies only configuration `$28`. Re-read the selected word from this
        // private cartridge and compare the final live CGRAM slot after the complete palette
        // pipeline, catching both bad table routing and a later accidental overwrite.
        byte sourceOffset = visor.SourceByteOffset ??
            throw new InvalidOperationException("Visor write omitted its source byte offset.");
        int sourceAddress = 0x9ba3c0 + sourceOffset;
        ushort expectedColor = unchecked((ushort)(
            bus.ReadByte(sourceAddress) | (bus.ReadByte(sourceAddress + 1) << 8)));
        if (visor.WrittenColor != expectedColor || runtime.Cgram.Colors[196] != expectedColor)
        {
            throw new InvalidOperationException(
                $"Visor offset ${sourceOffset:X2} wrote " +
                $"${visor.WrittenColor.GetValueOrDefault():X4}/" +
                $"${runtime.Cgram.Colors[196]:X4}; ROM says ${expectedColor:X4}.");
        }
        observedVisorPaletteOffsets.Add(sourceOffset);
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: visor table +${sourceOffset:X2} -> " +
            $"CGRAM 196 ${expectedColor:X4}; packed=" +
            $"${runtime.Samus.VisorPalette.PackedTimerIndex:X4}.");
    }
    if (options.MissileScript)
    {
        // `$90:C5C4-$C790` is deliberately validated from the live cartridge rather than
        // by accepting the typed result at face value. Recompute the pose selector,
        // attribute word, and frame tile through their original pointer tables, then prove
        // that the runtime queued the exact 32-byte transfer consumed by the next NMI.
        observedArmCannonFrames.Add(runtime.Samus.ArmCannon.Frame);
        SamusArmCannonDrawResult cannon = runtime.LastArmCannonDraw;
        if (cannon.TileUploadQueued)
        {
            int posePointerCell = 0x90c7df + runtime.Samus.Pose * 2;
            ushort drawingData = unchecked((ushort)(
                bus.ReadByte(posePointerCell) | (bus.ReadByte(posePointerCell + 1) << 8)));
            int drawingAddress = 0x900000 | drawingData;
            byte firstSelector = bus.ReadByte(drawingAddress);
            byte expectedSelector = (firstSelector & 0x80) != 0 &&
                runtime.Samus.AnimationFrame != 0
                    ? unchecked((byte)(bus.ReadByte(
                        0x900000 | unchecked((ushort)(drawingData + 2))) & 0x7f))
                    : unchecked((byte)(firstSelector & 0x7f));
            int attributeCell = 0x90c791 + expectedSelector * 2;
            ushort expectedAttributes = unchecked((ushort)(
                bus.ReadByte(attributeCell) | (bus.ReadByte(attributeCell + 1) << 8)));
            int listPointerCell = 0x90c7a5 + expectedSelector * 2;
            ushort listPointer = unchecked((ushort)(
                bus.ReadByte(listPointerCell) | (bus.ReadByte(listPointerCell + 1) << 8)));
            int tileCell = 0x900000 | unchecked((ushort)(
                listPointer + runtime.Samus.ArmCannon.Frame * 2));
            ushort expectedTileSource = unchecked((ushort)(
                bus.ReadByte(tileCell) | (bus.ReadByte(
                    0x900000 | unchecked((ushort)(tileCell + 1))) << 8)));

            if (cannon.DirectionSelector != expectedSelector ||
                cannon.Attributes != expectedAttributes ||
                cannon.TileSource != expectedTileSource)
            {
                throw new InvalidOperationException(
                    $"Arm-cannon frame {cannon.Frame} disagrees with ROM: " +
                    $"selector {cannon.DirectionSelector}/{expectedSelector}, " +
                    $"attributes ${cannon.Attributes:X4}/${expectedAttributes:X4}, " +
                    $"tile ${cannon.TileSource:X4}/${expectedTileSource:X4}.");
            }

            var expectedDma = new VramWriteEntry(
                0x20,
                0x9a0000 | expectedTileSource,
                0x61f0);
            if (!runtime.VramWrites.Entries.Contains(expectedDma))
            {
                throw new InvalidOperationException(
                    $"Arm-cannon frame {cannon.Frame} omitted ROM tile DMA " +
                    $"$9A:{expectedTileSource:X4} -> VRAM $61F0.");
            }

            observedArmCannonSprite |= cannon.SpriteWritten;
            observedArmCannonTileDma = true;
        }
    }
    if (runtime.LastHyperBeamPaletteFxStep is { PaletteWritten: true } hyperPalette)
    {
        // Do not merely trust the specialized interpreter's frame index. Compare the live
        // CGRAM words with this cartridge's `$8D:D904` record after the complete runtime
        // call; this catches a wrong destination, wrong stride, or later palette overwrite.
        int recordAddress = 0x8dd904 + hyperPalette.FrameIndex * 20;
        for (int color = 0; color < HyperBeamPaletteFxState.ColorsPerFrame; color++)
        {
            int source = recordAddress + 2 + color * 2;
            ushort expectedColor = unchecked((ushort)(
                bus.ReadByte(source) | (bus.ReadByte(source + 1) << 8)));
            ushort actualColor = runtime.Cgram.Colors[0xe1 + color];
            if (actualColor != expectedColor)
            {
                throw new InvalidOperationException(
                    $"Hyper Beam palette frame {hyperPalette.FrameIndex}, color {color} " +
                    $"was ${actualColor:X4}; ROM `$8D:{source & 0xffff:X4}` says ${expectedColor:X4}.");
            }
        }

        observedHyperBeamPaletteFrames.Add(hyperPalette.FrameIndex);
    }
    SamusBeamChargePaletteStepResult bodyPalette = runtime.LastBeamChargePaletteStep;
    if (bodyPalette.Action == SamusBeamChargePaletteAction.ChargeCycle)
    {
        // Validate both levels of `$91:D7D5` indirection against the private ROM, then
        // compare the complete post-frame CGRAM palette. This proves the viewer is showing
        // cartridge-authored charge colors rather than merely advancing a host counter.
        int paletteIndex = bodyPalette.ChargePaletteIndex ??
            throw new InvalidOperationException("Live charge palette write omitted its table index.");
        ushort suitOffset = runtime.Samus!.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? (ushort)4
            : runtime.Samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? (ushort)2
                : (ushort)0;
        int listCell = 0x91d7d5 + suitOffset;
        ushort listPointer = unchecked((ushort)(
            bus.ReadByte(listCell) | (bus.ReadByte(listCell + 1) << 8)));
        int paletteCell = 0x910000 | unchecked((ushort)(listPointer + paletteIndex * 2));
        ushort expectedPointer = unchecked((ushort)(
            bus.ReadByte(paletteCell) | (bus.ReadByte(paletteCell + 1) << 8)));
        if (bodyPalette.PalettePointer != expectedPointer)
        {
            throw new InvalidOperationException(
                $"Charge body palette {paletteIndex} used pointer ${bodyPalette.PalettePointer:X4}; " +
                $"ROM table contains ${expectedPointer:X4}.");
        }
        for (int color = 0; color < 16; color++)
        {
            int source = 0x9b0000 | unchecked((ushort)(expectedPointer + color * 2));
            ushort expectedColor = unchecked((ushort)(
                bus.ReadByte(source) | (bus.ReadByte(source + 1) << 8)));
            if (runtime.Cgram.Colors[192 + color] != expectedColor)
            {
                throw new InvalidOperationException(
                    $"Charge body palette {paletteIndex} color {color} differs from ROM.");
            }
        }
        observedLiveChargeBodyPalettes.Add(paletteIndex);
    }
    else if (bodyPalette.Action == SamusBeamChargePaletteAction.OrdinaryWhite)
    {
        // `$91:D7A4` writes `$03FF` backward over visible colors 15..1. Validate the final
        // live CGRAM image after all later special-palette dispatch, not merely the typed
        // branch result, so an ordering regression cannot pass this real-ROM route.
        for (int color = 1; color < 16; color++)
        {
            if (runtime.Cgram.Colors[192 + color] != 0x03ff)
            {
                throw new InvalidOperationException(
                    $"Charged-shot body glow wrote ${runtime.Cgram.Colors[192 + color]:X4} " +
                    $"to Samus color {color}, expected $03FF.");
            }
        }
        observedOrdinaryChargedWhitePaletteCalls++;
    }
    else if (bodyPalette.Action == SamusBeamChargePaletteAction.HyperPalette)
    {
        int paletteIndex = bodyPalette.HyperPaletteIndex ??
            throw new InvalidOperationException("Hyper body palette write omitted its table index.");
        int pointerCell = 0x91d829 + (bodyPalette.TimerBefore & 0x001e);
        ushort expectedPointer = unchecked((ushort)(
            bus.ReadByte(pointerCell) | (bus.ReadByte(pointerCell + 1) << 8)));
        if (bodyPalette.PalettePointer != expectedPointer)
        {
            throw new InvalidOperationException(
                $"Hyper body palette {paletteIndex} used pointer ${bodyPalette.PalettePointer:X4}, " +
                $"ROM table contains ${expectedPointer:X4}.");
        }
        for (int color = 0; color < 16; color++)
        {
            int source = 0x9b0000 | unchecked((ushort)(expectedPointer + color * 2));
            ushort expectedColor = unchecked((ushort)(
                bus.ReadByte(source) | (bus.ReadByte(source + 1) << 8)));
            if (runtime.Cgram.Colors[192 + color] != expectedColor)
            {
                throw new InvalidOperationException(
                    $"Hyper body palette {paletteIndex} color {color} differs from ROM.");
            }
        }
        observedHyperBeamBodyPalettes.Add(paletteIndex);
    }
    else if (bodyPalette.Action == SamusBeamChargePaletteAction.HyperHold)
    {
        observedHyperBeamBodyPaletteHolds++;
    }
    else if (bodyPalette.Action == SamusBeamChargePaletteAction.RestoredNormalSuit)
    {
        if (bodyPalette.TimerBefore == 1)
            observedOrdinaryChargedSuitRestore = true;
        else if (bodyPalette.TimerBefore == 0x8000)
            observedHyperBeamBodySuitRestore = true;
    }
    SamusHurtFlashPaletteStepResult hurtPalette = runtime.LastHurtFlashPaletteStep;
    if (options.KnockbackScript &&
        hurtPalette.Action is SamusHurtFlashPaletteAction.HurtFlash or
            SamusHurtFlashPaletteAction.NormalSuitRestore)
    {
        // Compare the final live CGRAM image with the private cartridge after every one of
        // the six visible writes. This checks the final-priority runtime placement as well
        // as the typed branch result: a later special handler overwriting these colors
        // would fail even if the hurt routine itself selected the correct pointer.
        int expectedPaletteAddress;
        if (hurtPalette.Action == SamusHurtFlashPaletteAction.HurtFlash)
        {
            observedHurtFlashPaletteCalls++;
            expectedPaletteAddress = 0x9ba380;
        }
        else
        {
            observedHurtSuitRestoreCalls++;
            ushort suitOffset = runtime.Samus!.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? (ushort)4
                : runtime.Samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? (ushort)2
                    : (ushort)0;
            int pointerCell = 0x91d727 + suitOffset;
            ushort pointer = unchecked((ushort)(
                bus.ReadByte(pointerCell) | (bus.ReadByte(pointerCell + 1) << 8)));
            expectedPaletteAddress = 0x9b0000 | pointer;
        }

        if (hurtPalette.PaletteAddress != expectedPaletteAddress)
        {
            throw new InvalidOperationException(
                $"Hurt counter {hurtPalette.CounterBefore} selected " +
                $"${hurtPalette.PaletteAddress:X6}; ROM route requires ${expectedPaletteAddress:X6}.");
        }
        for (int color = 0; color < 16; color++)
        {
            int source = expectedPaletteAddress + color * 2;
            ushort expectedColor = unchecked((ushort)(
                bus.ReadByte(source) | (bus.ReadByte(source + 1) << 8)));
            if (runtime.Cgram.Colors[192 + color] != expectedColor)
            {
                throw new InvalidOperationException(
                    $"Hurt counter {hurtPalette.CounterBefore} color {color} differs from ROM.");
            }
        }
    }
    observedHurtImpactSound |= options.KnockbackScript &&
        runtime.Samus!.LiquidPhysics.SoundRequests.Any(
            request => request == new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x35), 6));
    if (options.LandingImpactScript &&
        (runtime.LastAerialSamusMovement is { Landed: true } ||
         runtime.LastMorphBallMovement is { Landed: true }))
    {
        // Sample after the draw pass: a surviving type-six word proves the landing producer
        // ran before animation and `$90:8A4C` consumed/drew it without deleting the slot.
        observedLandingImpactDust |=
            runtime.Samus.LiquidPhysics.AtmosphericEffects.Slots[2].Type == 6 &&
            runtime.Samus.LiquidPhysics.AtmosphericEffects.Slots[3].Type == 6;
        observedLandingImpactSound |= runtime.Samus.LiquidPhysics.SoundRequests.Any(
            request => request == new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x05), 6));
    }
    if (specialSpinRoute &&
        yDirectionBeforeFrame == 2 &&
        runtime.Samus.Kinematics.YDirection == 1)
    {
        observedSpaceJumpRestarts++;
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: accepted Space Jump restart " +
            $"{observedSpaceJumpRestarts}; pose=${runtime.Samus.Pose:X2}, " +
            $"velocity={runtime.Samus.Kinematics.YSpeed:X4}." +
            $"{runtime.Samus.Kinematics.YSubspeed:X4}.");
    }
    observedScrewAttackContactDamage |=
        runtime.Samus.HorizontalSpeed.ContactDamageIndex == 3;
    observedScrewAttackPaletteCycle |=
        options.ScrewAttackScript &&
        SamusState.IsScrewAttackPose(runtime.Samus.Pose) &&
        runtime.Samus.AnimationFrame >= 27 &&
        runtime.Samus.HorizontalSpeed.SpecialPaletteFrame != 0;
    bool submergedLateScrewFrame =
        options.ScrewAttackScript &&
        options.WaterSpaceJumpScript &&
        SamusState.IsScrewAttackPose(runtime.Samus.Pose) &&
        runtime.Samus.AnimationFrame >= 27 &&
        runtime.Samus.LiquidPhysics.IsBottomBoundarySubmerged(runtime.Samus);
    if (submergedLateScrewFrame)
    {
        // `$91:D9B2-$D9D8` must return before touching either shared palette word. Compare
        // all sixteen live colors against the cartridge's ordinary Power Suit palette as
        // independent evidence: merely seeing index zero could hide an incorrect CGRAM
        // write followed by a host reset. This combined script grants no Varia/Gravity bit,
        // so native suit-table offset zero is the exact expected source.
        if (runtime.Samus.HorizontalSpeed.SpecialPaletteFrame != 0 ||
            runtime.Samus.HorizontalSpeed.SpecialPaletteTimer != 0)
        {
            throw new InvalidOperationException(
                $"Submerged Screw palette mutated timer/index at frame {result.FrameNumber}: " +
                $"timer=${runtime.Samus.HorizontalSpeed.SpecialPaletteTimer:X4}, " +
                $"index=${runtime.Samus.HorizontalSpeed.SpecialPaletteFrame:X4}.");
        }

        ushort normalPowerPalette = unchecked((ushort)(
            bus.ReadByte(0x91d727) |
            (bus.ReadByte(0x91d728) << 8)));
        for (int colorIndex = 0; colorIndex < 16; colorIndex++)
        {
            int source = 0x9b0000 | unchecked((ushort)(normalPowerPalette + colorIndex * 2));
            ushort expectedColor = unchecked((ushort)(
                bus.ReadByte(source) |
                (bus.ReadByte((source & 0xff0000) | ((source + 1) & 0xffff)) << 8)));
            ushort actualColor = runtime.Cgram.Colors[192 + colorIndex];
            if (actualColor != expectedColor)
            {
                throw new InvalidOperationException(
                    $"Submerged Screw palette color {colorIndex} disagrees with " +
                    $"$9B:{unchecked((ushort)(normalPowerPalette + colorIndex * 2)):X4}; " +
                    $"expected ${expectedColor:X4}, got ${actualColor:X4}.");
            }
        }
        observedSubmergedScrewPaletteFreeze = true;
    }
    observedCrystalFlashDrain |= runtime.LastCrystalFlashMovement is
        { PhaseAfterStep: CrystalFlashPhase.DrainingAmmo };
    observedCrystalFlashFinish |= runtime.LastCrystalFlashMovement is
        { PhaseAfterStep: CrystalFlashPhase.Finishing };
    observedCrystalFlashCompletion |= runtime.LastCrystalFlashMovement is
        { Completed: true };
    observedCrystalFlashWindowExpansion |=
        runtime.BombProjectiles.PowerBombExplosion.Phase ==
            PowerBombExplosionPhase.CrystalFlashExplosion;
    observedCrystalFlashWindowAfterglow |=
        runtime.BombProjectiles.PowerBombExplosion.Phase ==
            PowerBombExplosionPhase.CrystalFlashAfterglow;
    observedCrystalFlashPalette |=
        runtime.Samus.CrystalFlash.SpecialPaletteType == 7 &&
        runtime.Cgram.Colors[0xe0] != 0;
    if (runtime.LastXrayBeamStep is { } xrayBeamStep)
    {
        observedXrayPhases.Add(xrayBeamStep.PhaseAfterStep);

        // Report only meaningful state boundaries. The record remains debugger-visible on
        // every frame, but a 15-frame timer/angle trace should not bury other diagnostics.
        if (xrayBeamStep.PhaseAtStart != xrayBeamStep.PhaseAfterStep ||
            xrayBeamStep.SetupStage == 8)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: X-ray {xrayBeamStep.PhaseAtStart} -> " +
                $"{xrayBeamStep.PhaseAfterStep}; setup={xrayBeamStep.SetupStage}, " +
                $"angle=${xrayBeamStep.AngleAfterStep.TableIndex:X2}, width={xrayBeamStep.WidthAfterStep}.");
        }

        observedXrayAim |= xrayBeamStep.AngleAfterStep != SnesAngle.QuarterTurn &&
            xrayBeamStep.AngleAfterStep != SnesAngle.ThreeQuarterTurn;
    }
    if (runtime.LastXrayAnimationFrame is ushort xrayAnimationFrame)
        observedXrayAnimationFrames.Add(xrayAnimationFrame);
    observedXrayTurnStart |= runtime.LastXrayPoseInput is { StartedTurn: true };
    observedXrayTurnCompletion |= runtime.LastXrayPoseInput is { CompletedTurn: true };
    observedXrayLeftStablePose |=
        runtime.Samus.Pose == SamusPoseIds.XrayingStandingLeftPose;
    if (runtime.LastDeathSequenceStep is { } deathStep)
    {
        observedDeathPhases.Add(deathStep.PhaseAfterStep);
        if (deathStep.QueuedSegment is byte segment)
            observedDeathSegments.Add(segment);
        if (deathStep.ExplosionSpritemapIndex is ushort spritemap)
            observedDeathExplosionSpritemaps.Add(spritemap);
        observedDeathWhiteout |= deathStep.WhiteoutChanged;
        observedDeathCompletion |= deathStep.Completed;

        if (deathStep.PhaseAtStart != deathStep.PhaseAfterStep ||
            deathStep.QueuedSegment is not null)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: death {deathStep.PhaseAtStart} -> " +
                $"{deathStep.PhaseAfterStep}; index={deathStep.IndexAfterStep}, " +
                $"timer={deathStep.TimerAfterStep}, shade={deathStep.CounterAfterStep}, " +
                $"segment={deathStep.QueuedSegment?.ToString() ?? "none"}, " +
                $"spritemap={deathStep.ExplosionSpritemapIndex?.ToString("X3") ?? "none"}.");
        }
    }
    observedDrainedFallingHandler |= runtime.LastDrainedSamusMovement is not null;
    observedDrainedLanding |= runtime.LastDrainedSamusMovement is { Landed: true };
    observedDrainedStanding |= runtime.Samus.Pose is
        SamusPoseIds.DrainedStandingRightPose or SamusPoseIds.DrainedStandingLeftPose;
    observedDrainedCrouching |= issuedDrainedCrouchingCommand &&
        (runtime.Samus.Pose is
            SamusPoseIds.DrainedCrouchingRightPose or SamusPoseIds.DrainedCrouchingLeftPose);
    observedDrainedRelease |= issuedDrainedReleaseCommand &&
        runtime.Samus.Drained.Phase == DrainedSamusPhase.Inactive;
    observedDrainedHyperBeam |= runtime.Samus.HyperBeam == 0x8000;
    observedDraygonAimUp |= runtime.Samus.Pose == SamusPoseIds.DraygonGrabbedAimUpRightPose;
    observedDraygonFiring |= runtime.Samus.Pose == SamusPoseIds.DraygonGrabbedFiringRightPose;
    observedDraygonAimDown |= runtime.Samus.Pose == SamusPoseIds.DraygonGrabbedAimDownRightPose;
    observedDraygonMoving |= runtime.Samus.Pose == SamusPoseIds.DraygonGrabbedMovingRightPose;
    observedDraygonNeutralFallback |= frameIndex >= 96 && frameIndex < 112 &&
        runtime.Samus.Pose == SamusPoseIds.DraygonGrabbedNeutralRightPose;
    observedDraygonRelease |= !runtime.Samus.DraygonGrabbed.IsActive &&
        runtime.Samus.Pose == SamusPoseIds.FacingRightNormalPose;
    observedExternalXMovement |= options.ExtraDisplacementScript &&
        frameIndex == 31 &&
        runtime.Samus.XPosition == unchecked((ushort)(extraDisplacementStartX + 32));
    observedExternalUpMovement |= options.ExtraDisplacementScript &&
        frameIndex >= 32 && frameIndex < 48 &&
        runtime.Samus.YPosition < extraDisplacementStartY;
    observedExternalDownCollision |= options.ExtraDisplacementScript &&
        frameIndex >= 48 && frameIndex < 64 &&
        runtime.LastGroundedSamusMovement is { Vertical.Collided: true };
    observedExternalWordsCleared |= options.ExtraDisplacementScript &&
        frameIndex >= 64 &&
        runtime.Samus.Kinematics.ExtraXFixed == 0 &&
        runtime.Samus.Kinematics.ExtraYFixed == 0;
    observedKnockbackMovement |= runtime.LastKnockbackMovement is not null;
    observedDamageBoostMovement |= runtime.LastAerialSamusMovement is not null &&
        runtime.Samus.ReadMovementType(bus) == SamusMovementType.DamageBoost;
    observedMorphedKnockbackCompletion |= options.MorphKnockbackScript &&
        runtime.LastKnockbackMovement is { Ended: true } &&
        runtime.Samus.Pose == SamusPoseIds.MorphBallGroundRightPose &&
        !runtime.Samus.KnockbackActive &&
        runtime.Samus.Kinematics.YSpeed == 0 &&
        runtime.Samus.Kinematics.YSubspeed == 0 &&
        runtime.Samus.Kinematics.YDirection == 2;
    observedMorphedKnockbackFalling |= options.MorphKnockbackScript &&
        runtime.Samus.Pose == SamusPoseIds.MorphBallFallingRightPose &&
        runtime.Samus.YPosition < morphKnockbackGroundY;
    observedMorphedKnockbackLanding |= options.MorphKnockbackScript &&
        runtime.LastMorphBallMovement is { Landed: true } &&
        runtime.Samus.Pose == SamusPoseIds.MorphBallGroundRightPose;
    observedGrappleSwing |= runtime.LastGrappleMovement is
        { Phase: GrapplePhase.ConnectedSwinging };
    observedGrappleReleaseQueue |= runtime.LastGrappleMovement is { ReleaseQueued: true };
    observedGrappleRelease |= runtime.LastGrappleMovement is { Released: true };
    // `$90:946E` is independent of the beam function. LastAerialSamusMovement proves beta
    // movement actually ran rather than merely observing `$9B:CB8B`'s jump-pose cleanup.
    observedGrappleReleaseMovement |=
        observedGrappleReleaseQueue && runtime.LastAerialSamusMovement is not null;
    if (!observedGrappleTerrainCollision &&
        runtime.LastGrappleMovement is { TerrainCollided: true } grappleTerrain)
    {
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: grapple body collision at radial point " +
            $"{grappleTerrain.CollisionDistanceFromFeet}/6; safe angle=" +
            $"${runtime.Samus.Grapple.Angle:X4}, reflected velocity=" +
            $"${unchecked((ushort)runtime.Samus.Grapple.AngularVelocity):X4}, " +
            $"kickTimer={runtime.Samus.Grapple.CollisionBounceTimer}.");
    }
    observedGrappleTerrainCollision |= runtime.LastGrappleMovement is { TerrainCollided: true };
    observedGrappleFire |= runtime.LastGrappleMovement is { Fired: true };
    observedGrappleFireCancelQueue |= runtime.LastGrappleMovement is { CancelQueued: true };
    observedGrappleFireCancel |= runtime.LastGrappleMovement is { Cancelled: true };
    observedGrappleDrawHandler |= runtime.LastGrappleDrawingHandlerActive;
    observedGrappleBeamDrawPath |= runtime.LastGrappleBeamSpecificDrawingPath;
    observedGrappleFlare |= runtime.LastGrappleFlareDrawn;
    observedGrappleTeardownDrawFallback |=
        runtime.LastGrappleDrawingHandlerActive &&
        !runtime.LastGrappleBeamSpecificDrawingPath;
    observedBlockedRanIntoWallProbe |= runtime.LastRanIntoWallProbe is { Collided: true };
    observedBombJumpStart |= runtime.LastBombJumpMovement is { Started: true };
    observedBombJumpEnd |= runtime.LastBombJumpMovement is { Ended: true };
    observedBombJumpRise |= runtime.LastBombJumpMovement is { Started: false, Ended: false };
    observedBombPlacement |= runtime.BombProjectiles.LastFrameResult.PlacedSlot is not null;
    observedBombExplosion |= runtime.BombProjectiles.LastFrameResult.ExplosionStarted;
    observedBombDeletion |= runtime.BombProjectiles.LastFrameResult.ProjectileDeleted;
    observedStraightBombOverlap |=
        runtime.BombProjectiles.LastFrameResult.PublishedBombJumpDirection == 2;
    // These are sampled only after the complete frame/NMI handoff. A shot count therefore
    // proves the live bank-$90 producer allocated a real ordinary slot; a nonzero spritemap
    // proves bank `$93:81E9` consumed the cartridge's direction-specific instruction list.
    // The collision flag is diagnostic because a short capture may legitimately stop while
    // the projectile is still travelling and some long shots leave the 320-pixel kill box.
    if (runtime.Projectiles.LastFrameResult.FiredSlot is not null)
        observedPowerBeamShots++;
    maximumObservedCharge = Math.Max(maximumObservedCharge, runtime.Projectiles.FlareCounter);
    observedChargedTrail |= runtime.Projectiles.ActiveTrailCount != 0;
    if (runtime.Projectiles.LastFrameResult.FiredSlot is int firedSlot)
    {
        SamusProjectileSlot firedProjectile = runtime.Projectiles.Slots[firedSlot];
        if (firedProjectile.PreInstruction == SamusProjectilePreInstruction.HyperBeam)
        {
            // Preserve the actual values published by `$90:BCD1`; the final assertion below
            // compares them with the retail literals instead of accepting generic beam state.
            observedHyperBeamShot = true;
            observedHyperBeamType = firedProjectile.Type;
            observedHyperBeamDamage = firedProjectile.Damage;
            observedHyperBeamSound = runtime.Projectiles.LastFrameResult.QueuedSoundEffect?.Value ?? 0;
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: Hyper Beam slot {firedSlot}, " +
                $"type=${firedProjectile.Type:X4}, damage=${firedProjectile.Damage:X4}, " +
                $"sound=${observedHyperBeamSound:X2}.");
        }
        if (firedProjectile.PackedType.Family == SamusProjectileFamily.Beam &&
            (firedProjectile.Type & 0x000f) == options.BeamType)
        {
            observedSelectedBeamType = true;
        }
        if (firedProjectile.PackedType.Family == SamusProjectileFamily.SuperMissile)
        {
            observedSuperMissileShot = true;
            observedSuperMissileDamage = firedProjectile.Damage;
            observedSuperMissileSound = runtime.Projectiles.LastFrameResult.QueuedSoundEffect?.Value ?? 0;
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: super missile slot {firedSlot}, " +
                $"type=${firedProjectile.Type:X4}, damage=${firedProjectile.Damage:X4}, " +
                $"sound=${observedSuperMissileSound:X2}, ammo={runtime.Samus.SuperMissiles}.");
        }
        if (firedProjectile.PackedType.Family == SamusProjectileFamily.Missile)
        {
            observedMissileShot = true;
            observedMissileDamage = firedProjectile.Damage;
            observedMissileSound = runtime.Projectiles.LastFrameResult.QueuedSoundEffect?.Value ?? 0;
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: missile slot {firedSlot}, " +
                $"type=${firedProjectile.Type:X4}, damage=${firedProjectile.Damage:X4}, " +
                $"sound=${observedMissileSound:X2}, ammo={runtime.Samus.Missiles}.");
        }
        if ((firedProjectile.Type & 0x0010) != 0)
        {
            observedChargedShot = true;
            observedChargedShotDamage = firedProjectile.Damage;
            observedChargedShotSound = runtime.Projectiles.LastFrameResult.QueuedSoundEffect?.Value ?? 0;
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: charged beam slot {firedSlot}, " +
                $"type=${firedProjectile.Type:X4}, damage=${firedProjectile.Damage:X4}, " +
                $"sound=${observedChargedShotSound:X2}.");
        }
    }
    observedPowerBeamArt |= runtime.Projectiles.Slots.Any(
        slot => slot.IsActive && slot.SpritemapPointer != 0);
    observedHyperBeamArt |= runtime.Projectiles.Slots.Any(
        slot => slot.IsActive &&
            slot.PreInstruction == SamusProjectilePreInstruction.HyperBeam &&
            slot.SpritemapPointer != 0);
    observedHyperBeamFlare |=
        options.HyperBeamScript && runtime.Projectiles.FlareCounter == 0x8000;
    observedMissileArt |= runtime.Projectiles.Slots.Any(
        slot => slot.IsActive &&
            slot.PackedType.Family == SamusProjectileFamily.Missile &&
            slot.SpritemapPointer != 0);
    observedMissileTrail |= options.MissileScript && runtime.Projectiles.ActiveTrailCount != 0;
    observedSuperMissileArt |= runtime.Projectiles.Slots.Any(
        slot => slot.IsActive &&
            slot.PreInstruction == SamusProjectilePreInstruction.SuperMissile &&
            slot.SpritemapPointer != 0);
    observedSuperMissileLink |= runtime.Projectiles.Slots.Any(
        slot => slot.IsActive &&
            slot.PreInstruction == SamusProjectilePreInstruction.SuperMissileLink);
    observedSuperMissileTrail |=
        options.SuperMissileScript && runtime.Projectiles.ActiveTrailCount != 0;
    observedSuperMissileImpact |=
        options.SuperMissileScript &&
        runtime.Projectiles.LastFrameResult.CollisionStartedExplosion;
    observedSuperMissileQuake |=
        options.SuperMissileScript &&
        runtime.Projectiles.EarthquakeType == 20 &&
        runtime.Projectiles.EarthquakeTimer == 30;
    observedPowerBeamExplosion |=
        runtime.Projectiles.LastFrameResult.CollisionStartedExplosion;
    uint currentExtraRunSpeed =
        ((uint)runtime.Samus.HorizontalSpeed.ExtraRunSpeed << 16) |
        runtime.Samus.HorizontalSpeed.ExtraRunSubspeed;
    maximumObservedExtraRunSpeed = Math.Max(maximumObservedExtraRunSpeed, currentExtraRunSpeed);
    maximumObservedSpeedBoostStage = Math.Max(
        maximumObservedSpeedBoostStage,
        unchecked((byte)(runtime.Samus.HorizontalSpeed.SpeedBoostCounter >> 8)));
    observedSpeedBoostEcho |= runtime.Samus.HorizontalSpeed.EchoSoundRequested;
    observedSpeedBoostContactDamage |= runtime.Samus.HorizontalSpeed.ContactDamageIndex == 1;
    observedSpeedBoostFootDust |= runtime.Samus.LiquidPhysics.AtmosphericEffects.Slots.Any(
        slot => slot.Type == 7);
    bool currentSpeedBoostDeparture =
        (runtime.Samus.HorizontalSpeed.SpeedEchoIndex & 0x8000) != 0;
    observedSpeedBoostDeparture |= currentSpeedBoostDeparture;
    observedSpeedBoostDepartureFinished |=
        previousSpeedBoostDeparture && !currentSpeedBoostDeparture;
    if (currentSpeedBoostDeparture != previousSpeedBoostDeparture)
    {
        Console.WriteLine(
            $"frame {frameIndex + 1,4}: ordinary Speed Booster departure=" +
            $"{currentSpeedBoostDeparture}; " +
            $"slot0=({runtime.Samus.HorizontalSpeed.FirstSpeedEchoXPosition:X4}," +
            $"{runtime.Samus.HorizontalSpeed.FirstSpeedEchoYPosition:X4})/" +
            $"v{runtime.Samus.HorizontalSpeed.FirstSpeedEchoXSpeed:X4}, " +
            $"slot1=({runtime.Samus.HorizontalSpeed.SecondSpeedEchoXPosition:X4}," +
            $"{runtime.Samus.HorizontalSpeed.SecondSpeedEchoYPosition:X4})/" +
            $"v{runtime.Samus.HorizontalSpeed.SecondSpeedEchoXSpeed:X4}.");
    }
    previousSpeedBoostDeparture = currentSpeedBoostDeparture;
    observedStoredShine |= runtime.Samus.Shinespark.Phase == ShinesparkPhase.Stored;
    observedShinesparkWindup |= runtime.Samus.Shinespark.Phase == ShinesparkPhase.Windup;
    observedDirectionalShinespark |= runtime.Samus.Shinespark.Phase is
        ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal;
    observedShinesparkMovement |= runtime.LastShinesparkMovement is
        { PhaseAtStart: ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal };
    observedShinesparkPalette |= runtime.Samus.Shinespark.PaletteType is 1 or 6;
    observedShinesparkCrashOrbit |= runtime.Samus.Shinespark.Phase == ShinesparkPhase.Crash;
    observedShinesparkCrashEchoCircle |=
        runtime.Samus.Shinespark.Phase == ShinesparkPhase.CrashEchoCircle;
    // CrashFinish is an installed one-frame handler: its Step call restores standing and
    // publishes Inactive before this observer runs. Use the movement result as the native
    // handler-execution witness instead of hoping to sample a transient host enum value.
    observedShinesparkCrashFinish |=
        runtime.LastShinesparkMovement is { CrashSequenceFinished: true };
    if (runtime.LastShinesparkCrashDrawingHandlerActive)
    {
        observedShinesparkCrashDrawingHandler = true;
        // `$90:EBF3` never reaches either arm-cannon priority branch. Checking the typed
        // draw result on every observed crash frame catches both a leaked OBJ and the less
        // visible tile-$1F DMA side effect even when the final screenshot happens to overlap.
        if (runtime.LastArmCannonDraw.SpriteWritten ||
            runtime.LastArmCannonDraw.TileUploadQueued)
        {
            throw new InvalidOperationException(
                $"Shinespark crash frame {result.FrameNumber} leaked ordinary arm-cannon drawing.");
        }
    }
    observedReleasedShinesparkEcho |= runtime.Samus.Shinespark.ReleasedCrashEchoCount != 0;
    if (runtime.Samus.Shinespark.ReleasedCrashEchoCount != priorReleasedShinesparkEchoCount)
    {
        ShinesparkReleasedEcho firstReleased = runtime.Samus.Shinespark.FirstReleasedCrashEcho;
        ShinesparkReleasedEcho secondReleased = runtime.Samus.Shinespark.SecondReleasedCrashEcho;
        ShinesparkReleasedEchoClear? releasedClear =
            runtime.Samus.Shinespark.LastReleasedCrashEchoClear;
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: released shinespark echoes=" +
            $"{runtime.Samus.Shinespark.ReleasedCrashEchoCount}; " +
            $"slot3={(firstReleased.Active ? $"${firstReleased.Angle:X2}/r{firstReleased.Radius}/({firstReleased.XPosition:X4},{firstReleased.YPosition:X4})" : "clear")}, " +
            $"slot4={(secondReleased.Active ? $"${secondReleased.Angle:X2}/r{secondReleased.Radius}/({secondReleased.XPosition:X4},{secondReleased.YPosition:X4})" : "clear")}" +
            (releasedClear is { } clear
                ? $"; last clear=slot{clear.NativeSlot}/{clear.Axis}/r{clear.Radius}/" +
                  $"world({clear.XPosition:X4},{clear.YPosition:X4})/" +
                  $"screen({clear.ScreenX},{clear.ScreenY?.ToString() ?? "not sampled"})"
                : string.Empty));
        priorReleasedShinesparkEchoCount = runtime.Samus.Shinespark.ReleasedCrashEchoCount;
    }
    observedDashMomentum |= runtime.Samus.HorizontalSpeed.HasRunningMomentum;
    observedDashAerialCarry |= runtime.Samus.HorizontalSpeed.HasRunningMomentum &&
        currentExtraRunSpeed != 0 &&
        runtime.Samus.ReadMovementType(bus) is
            SamusMovementType.NormalJumping or
            SamusMovementType.SpinJumping or
            SamusMovementType.Falling;

    // Put a breakpoint here to inspect the complete runtime after any chosen frame. The
    // NoInlining attribute below keeps this method as a reliable stack frame in Debug and
    // Release builds instead of letting the JIT dissolve the hook into this loop.
    FrameBreakpoint(runtime, result);

    if (result.EscapeTimerState != priorState ||
        result.EscapeTimerExpired != priorEscapeTimerExpired)
    {
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: timer={result.EscapeTimerState,-27} " +
            $"time={runtime.EscapeTimer.MinutesBcd:X2}:{runtime.EscapeTimer.SecondsBcd:X2}.{runtime.EscapeTimer.CentisecondsBcd:X2} " +
            $"position=({runtime.EscapeTimer.XPixel},{runtime.EscapeTimer.YPixel}) " +
            $"expired={result.EscapeTimerExpired}");
        priorState = result.EscapeTimerState;
        priorEscapeTimerExpired = result.EscapeTimerExpired;
    }

    if (runtime.Samus.AnimationFrame != priorSamusFrame)
    {
        // The Mother Brain trace already logs every actor, head opcode, projectile hit,
        // and route boundary. Drained Samus loops a four-frame animation hundreds of times;
        // suppressing only that redundant presentation log keeps the causal trace readable.
        if (!options.MotherBrainRainbowScript)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: Samus animation {priorSamusFrame} -> " +
                $"{runtime.Samus.AnimationFrame}; timer={runtime.Samus.AnimationFrameTimer}; " +
                $"command={(runtime.Samus.LastAnimationDelayCommand is byte command ? $"${command:X2}" : "none")}; " +
                $"next definitions=${runtime.Samus.TileTransfers.TopDefinitionAddress:X6}/" +
                $"${runtime.Samus.TileTransfers.BottomDefinitionAddress:X6}");
        }
        priorSamusFrame = runtime.Samus.AnimationFrame;
    }

    if (runtime.Samus.Pose != priorSamusPose)
    {
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: applied Samus pose ${priorSamusPose:X2} -> " +
            $"${runtime.Samus.Pose:X2} after movement/animation; " +
            $"frame={runtime.Samus.AnimationFrame}, timer={runtime.Samus.AnimationFrameTimer}");
        priorSamusPose = runtime.Samus.Pose;
        priorSamusFrame = runtime.Samus.AnimationFrame;
    }

    if (runtime.Samus.Kinematics.XFixed != priorSamusX)
    {
        BlockMoveResult? horizontal = runtime.LastGroundedSamusMovement?.Horizontal ??
            runtime.LastAerialSamusMovement?.Horizontal ??
            runtime.LastMorphBallMovement?.Horizontal ??
            runtime.LastBombJumpMovement?.Horizontal ??
            runtime.LastKnockbackMovement?.Horizontal ??
            runtime.LastShinesparkMovement?.Horizontal;
        if (horizontal is null && rainbowSamusMovement is null &&
            runtime.LastGrappleMovement is null &&
            runtime.LastShinesparkMovement is null)
            throw new InvalidOperationException("Samus X changed without a translated movement result.");
        string vertical = runtime.LastGroundedSamusMovement is GroundedMovementResult groundedMovement
            ? $"ground=${groundedMovement.Vertical.AcceptedDisplacement:X8}/collision={groundedMovement.Vertical.Collided}"
            : runtime.LastAerialSamusMovement?.Vertical is BlockMoveResult aerialVertical
                ? $"airY=${aerialVertical.AcceptedDisplacement:X8}/collision={aerialVertical.Collided}"
                : runtime.LastMorphBallMovement is MorphBallMovementResult morphMovement
                    ? $"morphY=${morphMovement.Vertical.AcceptedDisplacement:X8}/collision={morphMovement.Vertical.Collided}"
                    : runtime.LastBombJumpMovement?.Vertical is BlockMoveResult bombVertical
                        ? $"bombY=${bombVertical.AcceptedDisplacement:X8}/collision={bombVertical.Collided}"
                    : runtime.LastKnockbackMovement?.Vertical is BlockMoveResult knockbackVertical
                        ? $"hurtY=${knockbackVertical.AcceptedDisplacement:X8}/collision={knockbackVertical.Collided}"
                    : "airY=transition";
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: Samus X={runtime.Samus.XPosition:X4}." +
            $"{runtime.Samus.Kinematics.XSubposition:X4}; " +
            $"base={runtime.Samus.HorizontalSpeed.BaseSpeed:X4}." +
            $"{runtime.Samus.HorizontalSpeed.BaseSubspeed:X4}, " +
            $"extra={runtime.Samus.HorizontalSpeed.ExtraRunSpeed:X4}." +
            $"{runtime.Samus.HorizontalSpeed.ExtraRunSubspeed:X4}, " +
            (horizontal is BlockMoveResult moved
                ? $"horizontal=${moved.AcceptedDisplacement:X8}, {vertical}"
                : rainbowSamusMovement is MotherBrainForcedSamusMovementResult forced
                    ? $"Mother Brain forced velocity=${forced.XVelocity:X4}/" +
                      $"${forced.YVelocity:X4}, carry={forced.NativeCarry}"
                : $"grapple angle=${runtime.Samus.Grapple.Angle:X4}, " +
                  $"velocity=${unchecked((ushort)runtime.Samus.Grapple.AngularVelocity):X4}, " +
                  $"beamStart=({runtime.Samus.Grapple.BeamStartX:X4},{runtime.Samus.Grapple.BeamStartY:X4})"));
        priorSamusX = runtime.Samus.Kinematics.XFixed;
    }

    if (runtime.Samus.Kinematics.YFixed != priorSamusY)
    {
        if (!options.MotherBrainRainbowScript)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: Samus Y={runtime.Samus.YPosition:X4}." +
                $"{runtime.Samus.Kinematics.YSubposition:X4}; " +
                $"velocity={runtime.Samus.Kinematics.YSpeed:X4}." +
                $"{runtime.Samus.Kinematics.YSubspeed:X4}, " +
                $"direction={runtime.Samus.Kinematics.YDirection}, " +
                $"landed={runtime.LastAerialSamusMovement?.Landed ?? runtime.LastMorphBallMovement?.Landed ?? false}, " +
                $"ceiling={runtime.LastAerialSamusMovement?.HitCeiling ?? runtime.LastMorphBallMovement?.HitCeiling ?? false}, " +
                $"bombActive={runtime.Samus.BombJumpActive}");
        }
        priorSamusY = runtime.Samus.Kinematics.YFixed;
    }

    ushort? prospectivePose = runtime.ProspectiveSamusPose?.ProspectivePose;
    if (prospectivePose != priorProspectivePose)
    {
        if (runtime.ProspectiveSamusPose is SamusPoseTransition transition)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: input matched ${transition.EntryAddress:X6}; " +
                $"prospective pose=${transition.ProspectivePose:X2} " +
                $"(required new=${transition.RequiredNewInput:X4}, held=${transition.RequiredHeldInput:X4}); " +
                (SamusState.IsDraygonGrabbedPose(
                     checked((byte)transition.ProspectivePose)) ||
                 (runtime.GroundedSamusMovementEnabled &&
                 transition.ProspectivePose is
                     SamusPoseIds.FacingRightNormalPose or
                     SamusPoseIds.FacingLeftNormalPose or
                     SamusPoseIds.MovingRightNormalPose or
                     SamusPoseIds.MovingLeftNormalPose or
                     SamusPoseIds.MovingRightGunExtendedPose or
                     SamusPoseIds.MovingLeftGunExtendedPose or
                     SamusPoseIds.TurningRightToLeftPose or
                     SamusPoseIds.TurningLeftToRightPose or
                     SamusPoseIds.TurningRightToLeftAimUpPose or
                     SamusPoseIds.TurningLeftToRightAimUpPose or
                     SamusPoseIds.TurningRightToLeftAimDiagonalUpPose or
                     SamusPoseIds.TurningLeftToRightAimDiagonalUpPose or
                     SamusPoseIds.TurningRightToLeftAimDiagonalDownPose or
                     SamusPoseIds.TurningLeftToRightAimDiagonalDownPose or
                     SamusPoseIds.TurningRightToLeftCrouchingPose or
                     SamusPoseIds.TurningLeftToRightCrouchingPose or
                     SamusPoseIds.TurningRightToLeftCrouchingAimUpPose or
                     SamusPoseIds.TurningLeftToRightCrouchingAimUpPose or
                     SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalUpPose or
                     SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalUpPose or
                     SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalDownPose or
                     SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalDownPose or
                     SamusPoseIds.NeutralJumpTransitionRightPose or
                     SamusPoseIds.NeutralJumpTransitionLeftPose or
                     SamusPoseIds.SpinJumpRightPose or
                     SamusPoseIds.SpinJumpLeftPose or
                     SamusPoseIds.SpaceJumpRightPose or
                     SamusPoseIds.SpaceJumpLeftPose or
                     SamusPoseIds.ScrewAttackRightPose or
                     SamusPoseIds.ScrewAttackLeftPose or
                     SamusPoseIds.CrouchingTransitionRightPose or
                     SamusPoseIds.CrouchingTransitionLeftPose or
                     SamusPoseIds.StandingTransitionRightPose or
                     SamusPoseIds.StandingTransitionLeftPose or
                     SamusPoseIds.MorphBallGroundRightPose or
                     SamusPoseIds.MorphBallGroundLeftPose or
                     SamusPoseIds.MorphBallMovingRightPose or
                     SamusPoseIds.MorphBallMovingLeftPose or
                     SamusPoseIds.MorphBallFallingRightPose or
                     SamusPoseIds.MorphBallFallingLeftPose or
                     SamusPoseIds.MorphingTransitionRightPose or
                     SamusPoseIds.MorphingTransitionLeftPose or
                     SamusPoseIds.UnmorphingTransitionRightPose or
                     SamusPoseIds.UnmorphingTransitionLeftPose or
                     SamusPoseIds.SpringBallGroundRightPose or
                     SamusPoseIds.SpringBallGroundLeftPose or
                     SamusPoseIds.SpringBallMovingRightPose or
                     SamusPoseIds.SpringBallMovingLeftPose or
                     SamusPoseIds.SpringBallFallingRightPose or
                     SamusPoseIds.SpringBallFallingLeftPose or
                     SamusPoseIds.SpringBallJumpRightPose or
                     SamusPoseIds.SpringBallJumpLeftPose or
                     SamusPoseIds.CrouchingRightPose or
                     SamusPoseIds.CrouchingLeftPose or
                     SamusPoseIds.StandingAimUpRightPose or
                     SamusPoseIds.StandingAimUpLeftPose or
                     SamusPoseIds.StandingAimDiagonalUpRightPose or
                     SamusPoseIds.StandingAimDiagonalUpLeftPose or
                     SamusPoseIds.StandingAimDiagonalDownRightPose or
                     SamusPoseIds.StandingAimDiagonalDownLeftPose or
                     SamusPoseIds.RunningAimUpRightPose or
                     SamusPoseIds.RunningAimUpLeftPose or
                     SamusPoseIds.RunningAimDiagonalUpRightPose or
                     SamusPoseIds.RunningAimDiagonalUpLeftPose or
                     SamusPoseIds.RunningAimDiagonalDownRightPose or
                     SamusPoseIds.RunningAimDiagonalDownLeftPose or
                     SamusPoseIds.NormalJumpForwardRightPose or
                     SamusPoseIds.NormalJumpForwardLeftPose or
                     SamusPoseIds.NormalJumpGunExtendedRightPose or
                     SamusPoseIds.NormalJumpGunExtendedLeftPose or
                     SamusPoseIds.NormalJumpAimUpRightPose or
                     SamusPoseIds.NormalJumpAimUpLeftPose or
                     SamusPoseIds.NormalJumpTransitionAimUpRightPose or
                     SamusPoseIds.NormalJumpTransitionAimUpLeftPose or
                     SamusPoseIds.NormalJumpTransitionAimDiagonalUpRightPose or
                     SamusPoseIds.NormalJumpTransitionAimDiagonalUpLeftPose or
                     SamusPoseIds.NormalJumpTransitionAimDiagonalDownRightPose or
                     SamusPoseIds.NormalJumpTransitionAimDiagonalDownLeftPose or
                     SamusPoseIds.NormalJumpAimDiagonalUpRightPose or
                     SamusPoseIds.NormalJumpAimDiagonalUpLeftPose or
                     SamusPoseIds.NormalJumpAimDiagonalDownRightPose or
                     SamusPoseIds.NormalJumpAimDiagonalDownLeftPose or
                     SamusPoseIds.NormalJumpAimDownRightPose or
                     SamusPoseIds.NormalJumpAimDownLeftPose or
                     SamusPoseIds.DamageBoostRightPose or
                     SamusPoseIds.DamageBoostLeftPose or
                     SamusPoseIds.FallingAimUpRightPose or
                     SamusPoseIds.FallingAimUpLeftPose or
                     SamusPoseIds.FallingAimDiagonalUpRightPose or
                     SamusPoseIds.FallingAimDiagonalUpLeftPose or
                     SamusPoseIds.FallingAimDiagonalDownRightPose or
                     SamusPoseIds.FallingAimDiagonalDownLeftPose or
                     SamusPoseIds.FallingAimDownRightPose or
                     SamusPoseIds.FallingAimDownLeftPose or
                     SamusPoseIds.FallingGunExtendedRightPose or
                     SamusPoseIds.FallingGunExtendedLeftPose or
                     SamusPoseIds.CrouchingAimUpRightPose or
                     SamusPoseIds.CrouchingAimUpLeftPose or
                     SamusPoseIds.CrouchingAimDiagonalUpRightPose or
                     SamusPoseIds.CrouchingAimDiagonalUpLeftPose or
                     SamusPoseIds.CrouchingAimDiagonalDownRightPose or
                     SamusPoseIds.CrouchingAimDiagonalDownLeftPose or
                     SamusPoseIds.LandingAimUpRightPose or
                     SamusPoseIds.LandingAimUpLeftPose or
                     SamusPoseIds.LandingAimDiagonalUpRightPose or
                     SamusPoseIds.LandingAimDiagonalUpLeftPose or
                     SamusPoseIds.LandingAimDiagonalDownRightPose or
                     SamusPoseIds.LandingAimDiagonalDownLeftPose or
                     SamusPoseIds.FiringLandingRightPose or
                     SamusPoseIds.FiringLandingLeftPose or
                     SamusPoseIds.CrouchingTransitionAimUpRightPose or
                     SamusPoseIds.CrouchingTransitionAimUpLeftPose or
                     SamusPoseIds.CrouchingTransitionAimDiagonalUpRightPose or
                     SamusPoseIds.CrouchingTransitionAimDiagonalUpLeftPose or
                     SamusPoseIds.CrouchingTransitionAimDiagonalDownRightPose or
                     SamusPoseIds.CrouchingTransitionAimDiagonalDownLeftPose or
                     SamusPoseIds.StandingTransitionAimUpRightPose or
                     SamusPoseIds.StandingTransitionAimUpLeftPose or
                     SamusPoseIds.StandingTransitionAimDiagonalUpRightPose or
                     SamusPoseIds.StandingTransitionAimDiagonalUpLeftPose or
                     SamusPoseIds.StandingTransitionAimDiagonalDownRightPose or
                     SamusPoseIds.StandingTransitionAimDiagonalDownLeftPose or
                     SamusPoseIds.MoonwalkFacingLeftPose or
                     SamusPoseIds.MoonwalkFacingRightPose or
                     SamusPoseIds.MoonwalkAimUpRightPose or
                     SamusPoseIds.MoonwalkAimUpLeftPose or
                     SamusPoseIds.MoonwalkAimDownRightPose or
                     SamusPoseIds.MoonwalkAimDownLeftPose or
                     SamusPoseIds.MoonwalkTurnJumpLeftPose or
                     SamusPoseIds.MoonwalkTurnJumpRightPose or
                     SamusPoseIds.MoonwalkTurnJumpAimUpLeftPose or
                     SamusPoseIds.MoonwalkTurnJumpAimUpRightPose or
                     SamusPoseIds.MoonwalkTurnJumpAimDownLeftPose or
                     SamusPoseIds.MoonwalkTurnJumpAimDownRightPose or
                     SamusPoseIds.RanIntoWallRightPose or
                     SamusPoseIds.RanIntoWallLeftPose or
                     SamusPoseIds.RanIntoWallAimUpRightPose or
                     SamusPoseIds.RanIntoWallAimUpLeftPose or
                     SamusPoseIds.RanIntoWallAimDownRightPose or
                     SamusPoseIds.RanIntoWallAimDownLeftPose or
                     SamusPoseIds.ShinesparkWindupRightPose or
                     SamusPoseIds.ShinesparkWindupLeftPose or
                     SamusPoseIds.ShinesparkHorizontalRightPose or
                     SamusPoseIds.ShinesparkHorizontalLeftPose or
                     SamusPoseIds.ShinesparkVerticalRightPose or
                     SamusPoseIds.ShinesparkVerticalLeftPose or
                     SamusPoseIds.ShinesparkDiagonalRightPose or
                     SamusPoseIds.ShinesparkDiagonalLeftPose or
                     SamusPoseIds.TurningRightToLeftJumpPose or
                     SamusPoseIds.TurningLeftToRightJumpPose or
                     SamusPoseIds.TurningRightToLeftFallingPose or
                     SamusPoseIds.TurningLeftToRightFallingPose)
                    ? "applied at a verified translated pose-transition seam"
                    : "not applied because its movement/transition side effects are not translated"));
        }
        priorProspectivePose = prospectivePose;
    }

    if (runtime.ProspectiveSamusFallbackPose != priorFallbackPose)
    {
        if (runtime.ProspectiveSamusFallbackPose is ushort fallback)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: no-button fallback selected pose ${fallback:X2}; " +
                $"base={runtime.Samus.HorizontalSpeed.BaseSpeed:X4}." +
                $"{runtime.Samus.HorizontalSpeed.BaseSubspeed:X4}, " +
                $"accelMode={runtime.Samus.HorizontalSpeed.AccelerationMode}");
        }
        priorFallbackPose = runtime.ProspectiveSamusFallbackPose;
    }

    if (runtime.ProspectiveSamusWallCollisionPose != priorWallCollisionPose)
    {
        if (runtime.ProspectiveSamusWallCollisionPose is byte wallPose)
        {
            BlockMoveResult? probe = runtime.LastRanIntoWallProbe;
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: `$91:EADE` selected wall pose ${wallPose:X2}; " +
                (probe is { } onePixel
                    ? $"one-pixel block probe collision={onePixel.Collided}, " +
                      $"accepted=${onePixel.AcceptedDisplacement:X8}."
                    : "current running X speed had already been killed by collision."));
        }
        priorWallCollisionPose = runtime.ProspectiveSamusWallCollisionPose;
    }
}

if (options.ElevatorScript)
{
    ushort startY = elevatorStartY ?? throw new InvalidOperationException(
        "Elevator script did not record its host-authored starting position.");
    ushort stopY = elevatorStopY ?? throw new InvalidOperationException(
        "Elevator script did not derive its ROM-backed floor stop.");
    int clearPixels = stopY - startY;
    if (clearPixels < 0)
        throw new InvalidOperationException("Elevator stimulus began below its derived floor stop.");
    ushort expectedY = unchecked((ushort)(startY + Math.Min(options.FrameCount, clearPixels)));
    if (runtime.Samus!.Pose != SamusPoseIds.ForwardFacingPowerSuitPose)
    {
        throw new InvalidOperationException(
            $"Elevator ROM script left required forward-facing pose $00 for ${runtime.Samus.Pose:X2}.");
    }
    if (runtime.Samus.YPosition != expectedY)
    {
        throw new InvalidOperationException(
            $"Elevator ROM script expected Y=${expectedY:X4} after {options.FrameCount} frame(s), " +
            $"but reached ${runtime.Samus.YPosition:X4}.");
    }
    if (options.FrameCount >= 2 &&
        (!observedElevatorVisibleEvenFrame || !observedElevatorHiddenOddFrame))
    {
        throw new InvalidOperationException(
            "Elevator display handler did not alternate an odd hidden frame and an even visible frame.");
    }
    Console.WriteLine(
        $"Forward-facing elevator route validated {Math.Min(options.FrameCount, clearPixels)} accepted one-pixel move(s) " +
        $"and real floor clipping at Y=${runtime.Samus.YPosition:X4}; " +
        $"display blink odd-hidden/even-visible=" +
        $"{observedElevatorHiddenOddFrame}/{observedElevatorVisibleEvenFrame}.");
}

if (options.AerialTurnScript)
{
    byte[] requiredAerialTurnRoute = [
        SamusPoseIds.NeutralJumpTransitionRightPose,
        SamusPoseIds.NeutralJumpRightPose,
        SamusPoseIds.TurningRightToLeftJumpPose,
        SamusPoseIds.NormalJumpForwardLeftPose,
        SamusPoseIds.NormalLandingLeftPose,
    ];
    foreach (byte requiredPose in requiredAerialTurnRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
            throw new InvalidOperationException(
                $"Aerial-turn ROM script did not observe required pose ${requiredPose:X2}.");
    }
    Console.WriteLine(
        $"Aerial-turn ROM route validated {requiredAerialTurnRoute.Length} deterministic pose milestones.");
}

if (options.CompactAirScript)
{
    // This is deliberately a real-ROM assertion: input records, pose definitions, delay
    // lists, collision geometry, and landing-table results all came from the user's image.
    // Shorter captures are useful for freezing the airborne artwork, so each documented
    // milestone validates everything the deterministic timeline has reached by that frame.
    var requiredCompactRoute = new List<byte>
    {
        SamusPoseIds.NormalJumpAimDownRightPose,
    };
    if (options.FrameCount >= 32)
        requiredCompactRoute.Add(SamusPoseIds.NormalLandingRightPose);
    if (options.FrameCount >= 149)
        requiredCompactRoute.Add(SamusPoseIds.NormalJumpAimDownLeftPose);
    if (options.FrameCount >= 235)
        requiredCompactRoute.Add(SamusPoseIds.NormalLandingLeftPose);
    foreach (byte requiredPose in requiredCompactRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
        {
            throw new InvalidOperationException(
                $"Compact-air ROM script did not observe required pose ${requiredPose:X2}.");
        }
    }

    Console.WriteLine(
        $"Compact-air ROM route validated {requiredCompactRoute.Count} deterministic pose milestone(s).");
}

if (options.MorphBallScript)
{
    // These milestones come from the user's ROM tables and are deliberately stricter than
    // “the runner did not throw.” Keep the list frame-count-aware so a short run can freeze
    // a useful ball-art diagnostic without weakening the complete 220-frame route. Each
    // threshold is the first accepted NMI on which the full private-ROM run observed it.
    var requiredMorphRoute = new List<byte> {
        SamusPoseIds.MorphingTransitionRightPose,
    };
    if (options.FrameCount >= 17)
        requiredMorphRoute.Add(SamusPoseIds.MorphBallGroundRightPose);
    if (options.FrameCount >= 26)
        requiredMorphRoute.Add(SamusPoseIds.MorphBallMovingRightPose);
    if (options.FrameCount >= 81)
        requiredMorphRoute.Add(SamusPoseIds.MorphBallMovingLeftPose);
    if (options.FrameCount >= 149)
        requiredMorphRoute.Add(SamusPoseIds.MorphBallGroundLeftPose);
    if (options.FrameCount >= 176)
        requiredMorphRoute.Add(SamusPoseIds.UnmorphingTransitionLeftPose);
    if (options.FrameCount >= 182)
        requiredMorphRoute.Add(SamusPoseIds.CrouchingLeftPose);
    foreach (byte requiredPose in requiredMorphRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
        {
            throw new InvalidOperationException(
                $"Morph-Ball ROM script did not observe required pose ${requiredPose:X2}.");
        }
    }

    Console.WriteLine(
        $"Morph-Ball ROM route validated {requiredMorphRoute.Count} deterministic pose milestones.");
}

if (options.SpringBallScript)
{
    byte[] requiredSpringRoute = [
        SamusPoseIds.MorphingTransitionRightPose,
        SamusPoseIds.SpringBallGroundRightPose,
        SamusPoseIds.SpringBallMovingRightPose,
        SamusPoseIds.SpringBallJumpRightPose,
    ];
    foreach (byte requiredPose in requiredSpringRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
            throw new InvalidOperationException(
                $"Spring-Ball ROM script did not observe required pose ${requiredPose:X2}.");
    }
    Console.WriteLine(
        $"Spring-Ball ROM route validated {requiredSpringRoute.Length} deterministic pose milestones.");
}

if (options.BombJumpScript)
{
    // The route now proves the complete producer-to-consumer chain, not merely the special
    // movement handler: controller edge, slot allocation, timer-eight bank-$A0 overlap,
    // next-frame `$E025`, explosion instruction list/delete, rise, and termination.
    if (options.FrameCount >= 17 &&
        !observedSamusPoses.Contains(SamusPoseIds.MorphBallGroundRightPose))
        throw new InvalidOperationException("Bomb-jump ROM script never reached stable Morph Ball pose $1D.");

    // Short runs are intentionally supported as art/timing captures. Each threshold is
    // the first accepted NMI where the full real-ROM route can have observed that phase.
    bool missedReachedPhase =
        (options.FrameCount >= 26 && !observedBombPlacement) ||
        (options.FrameCount >= 77 && !observedStraightBombOverlap) ||
        (options.FrameCount >= 78 && !observedBombJumpStart) ||
        (options.FrameCount >= 79 && !observedBombJumpRise) ||
        (options.FrameCount >= 85 && !observedBombExplosion) ||
        (options.FrameCount >= 95 && !observedBombDeletion) ||
        (options.FrameCount >= 105 && !observedBombJumpEnd);
    if (missedReachedPhase)
    {
        throw new InvalidOperationException(
            $"Bomb route missed a phase: placed={observedBombPlacement}, " +
            $"overlap={observedStraightBombOverlap}, explosion={observedBombExplosion}, " +
            $"deleted={observedBombDeletion}, start={observedBombJumpStart}, " +
            $"rise={observedBombJumpRise}, end={observedBombJumpEnd}.");
    }
    Console.WriteLine(
        $"Bomb-jump ROM route validated every milestone reachable within {options.FrameCount} frame(s).");
}

if (options.PowerBombScript)
{
    SamusPowerBombExplosionState powerBomb = runtime.BombProjectiles.PowerBombExplosion;
    if (options.FrameCount >= 17 &&
        !observedSamusPoses.Contains(SamusPoseIds.MorphBallGroundRightPose))
    {
        throw new InvalidOperationException(
            "Power-bomb ROM script never reached stable Morph Ball pose $1D.");
    }
    if (options.FrameCount >= 26 &&
        (runtime.Samus!.PowerBombs != 9 ||
         runtime.BombProjectiles.Slots[0].Damage != 0x00c8 ||
         runtime.BombProjectiles.Slots[0].PackedType.Family !=
            SamusProjectileFamily.PowerBomb ||
         !powerBomb.IsArmed))
    {
        throw new InvalidOperationException(
            $"Power-bomb producer mismatch: ammo={runtime.Samus!.PowerBombs}/9, " +
            $"type=${runtime.BombProjectiles.Slots[0].Type:X4}/0300, " +
            $"damage=${runtime.BombProjectiles.Slots[0].Damage:X4}/00C8, " +
            $"armed={powerBomb.IsArmed}.");
    }
    if (options.FrameCount >= 86 && !powerBomb.IsActive)
    {
        throw new InvalidOperationException(
            "Power-bomb fuse expired without starting bank-$88 explosion status $8000.");
    }

    Console.WriteLine(
        $"Power-bomb ROM route: ammo={runtime.Samus!.PowerBombs}, " +
        $"slotType=${runtime.BombProjectiles.Slots[0].Type:X4}, " +
        $"damage=${runtime.BombProjectiles.Slots[0].Damage:X4}, " +
        $"phase={powerBomb.Phase}, preRadius=${powerBomb.PreExplosionRadius:X4}, " +
        $"radius=${powerBomb.ExplosionRadius:X4}.");
}

if (options.LandingImpactScript)
{
    if (options.FrameCount >= 28 && !observedLandingImpactDust)
        throw new InvalidOperationException("Landing-impact route never retained both native type-six dust slots through draw.");
    if (options.FrameCount >= 28 && !observedLandingImpactSound)
        throw new InvalidOperationException("Landing-impact route never published the native library-three soft-impact sound `$05`.");
    Console.WriteLine(
        $"Landing-impact ROM route: dust={observedLandingImpactDust}, " +
        $"softSound={observedLandingImpactSound}, " +
        $"slot2=${runtime.Samus!.LiquidPhysics.AtmosphericEffects.Slots[2].FrameAndType:X4}, " +
        $"slot3=${runtime.Samus.LiquidPhysics.AtmosphericEffects.Slots[3].FrameAndType:X4}.");
}

if (options.KnockbackScript)
{
    // The only host-authored fact is which side the not-yet-translated enemy occupied.
    // A short 21-frame capture is intentionally allowed to stop on the first white flash.
    // Longer captures require the later damage-boost pose/handler and all six palette calls.
    var requiredKnockbackRoute = new List<byte> { SamusPoseIds.KnockbackRightPose };
    if (options.FrameCount >= 22)
        requiredKnockbackRoute.Add(SamusPoseIds.DamageBoostRightPose);
    foreach (byte requiredPose in requiredKnockbackRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
            throw new InvalidOperationException(
                $"Knockback ROM script did not observe required pose ${requiredPose:X2}.");
    }
    if (!observedKnockbackMovement ||
        (options.FrameCount >= 22 && !observedDamageBoostMovement))
    {
        throw new InvalidOperationException(
            $"Knockback route missed a handler: hurt={observedKnockbackMovement}, " +
            $"damageBoost={observedDamageBoostMovement}.");
    }
    if (options.FrameCount >= 26 &&
        (observedHurtFlashPaletteCalls != 3 ||
         observedHurtSuitRestoreCalls != 3 ||
         !observedHurtImpactSound))
    {
        throw new InvalidOperationException(
            "Knockback hurt palette/audio cadence differed from ROM: " +
            $"flash={observedHurtFlashPaletteCalls}/3, " +
            $"restore={observedHurtSuitRestoreCalls}/3, " +
            $"impactSfx={observedHurtImpactSound}.");
    }
    Console.WriteLine(options.FrameCount >= 26
        ? "Knockback ROM route validated hurt movement, 3+3 body-palette flashes, impact SFX, the Left+Jump damage-boost chord, and type-$19 jump movement."
        : "Knockback ROM capture validated the translated route through its requested frame boundary.");
}

if (options.GunExtendedScript && options.FrameCount >= 150)
{
    // Every member is recorded only after a complete StepFrame. `$29` is the single
    // documented host-published walk-off input; `$0B/$13/$67/$E6` must all be selected by
    // unchanged ROM transition/delay data and live controller/collision state.
    byte[] requiredGunExtendedRoute =
    [
        SamusPoseIds.MovingRightGunExtendedPose,
        SamusPoseIds.NormalJumpGunExtendedRightPose,
        SamusPoseIds.FallingGunExtendedRightPose,
        SamusPoseIds.FiringLandingRightPose,
    ];
    foreach (byte requiredPose in requiredGunExtendedRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
        {
            throw new InvalidOperationException(
                $"Horizontal-fire ROM script did not observe required pose ${requiredPose:X2}.");
        }
    }

    Console.WriteLine(
        "Horizontal-fire ROM route validated running $0B, neutral-jump $13, falling $67, and held-Shot landing $E6.");
}

if (options.GunExtendedScript && options.FrameCount >= 3)
{
    // Pose milestones alone could pass if projectile production were accidentally removed.
    // Require both allocation and decoded bank-$93 art so this route guards the newly joined
    // producer -> instruction interpreter -> OAM chain on the private retail cartridge.
    if (observedPowerBeamShots == 0 || !observedPowerBeamArt || !observedSelectedBeamType)
    {
        throw new InvalidOperationException(
            $"Beam route missed live projectile state: type=${options.BeamType:X1}, " +
            $"shots={observedPowerBeamShots}, art={observedPowerBeamArt}, " +
            $"selectedType={observedSelectedBeamType}, collision={observedPowerBeamExplosion}.");
    }

    Console.WriteLine(
        $"Beam type ${options.BeamType:X1} ROM route fired {observedPowerBeamShots} shot(s), decoded bank-$93 art, " +
        $"and observedCollision={observedPowerBeamExplosion}.");
}

if (options.ChargeBeamScript)
{
    // Counter 15 is the first visible main flare and 60 is the charged-release threshold.
    // Requiring the latter on a long capture proves the real frame loop did not merely draw
    // a transient spark. The charged-family bit, nonzero ROM damage, and `$17` sound then
    // prove release selected `$90:B986/$93:83D9`, not a second ordinary power shot.
    if (options.FrameCount >= 62 && maximumObservedCharge < 60)
    {
        throw new InvalidOperationException(
            $"Charge Beam route reached only {maximumObservedCharge}/60 held frames.");
    }
    int chargedSoundAddress = 0x90c2a7 + options.BeamType * 2;
    ushort expectedChargedSound = unchecked((ushort)(
        bus.ReadByte(chargedSoundAddress) | (bus.ReadByte(chargedSoundAddress + 1) << 8)));
    if (options.FrameCount >= 68 &&
        (!observedChargedShot ||
         !observedSelectedBeamType ||
         observedChargedShotDamage == 0 ||
         observedChargedShotSound != expectedChargedSound))
    {
        throw new InvalidOperationException(
            $"Charge Beam type ${options.BeamType:X1} release mismatch: " +
            $"charged={observedChargedShot}, selectedType={observedSelectedBeamType}, " +
            $"damage=${observedChargedShotDamage:X4}, " +
            $"sound=${observedChargedShotSound:X2}/${expectedChargedSound:X2}.");
    }
    if (options.FrameCount >= 71 && !observedChargedTrail)
    {
        throw new InvalidOperationException(
            "Charge Beam release did not allocate its native bank-$90 projectile trail.");
    }
    if (options.FrameCount >= 71 &&
        (observedOrdinaryChargedWhitePaletteCalls != 3 ||
         !observedOrdinaryChargedSuitRestore))
    {
        throw new InvalidOperationException(
            $"Charge Beam body palette cadence mismatch: white calls=" +
            $"{observedOrdinaryChargedWhitePaletteCalls}/3, " +
            $"suitRestore={observedOrdinaryChargedSuitRestore}.");
    }
    if (options.FrameCount >= 67 && observedLiveChargeBodyPalettes.Count != 6)
    {
        throw new InvalidOperationException(
            $"Charge Beam live body cycle visited {observedLiveChargeBodyPalettes.Count}/6 " +
            "ROM palettes before release.");
    }

    Console.WriteLine(
        $"Charge Beam type ${options.BeamType:X1} ROM route reached {maximumObservedCharge}/60, " +
        $"chargedShot={observedChargedShot}, damage=${observedChargedShotDamage:X4}, " +
        $"sound=${observedChargedShotSound:X2}, trail={observedChargedTrail}, " +
        $"chargeCycle={observedLiveChargeBodyPalettes.Count}/6, " +
        $"bodyGlow={observedOrdinaryChargedWhitePaletteCalls}/3+restore.");
}

if (options.HyperBeamScript)
{
    // `$90:BCD1` does not use the ordinary charge-release family even though type `$9018`
    // carries bit `$0010`. Require every literal written by that routine, decoded ROM art,
    // and its special `$8000` flare sentinel. Hyper's `$90:B159` movement intentionally
    // falls into Wave motion without allocating the ordinary Wave trail, so a trail is a bug.
    if (options.FrameCount >= 3 &&
        (!observedHyperBeamShot ||
         observedHyperBeamType != 0x9018 ||
         observedHyperBeamDamage != 0x03e8 ||
         observedHyperBeamSound != 0x001f ||
         !observedHyperBeamFlare))
    {
        throw new InvalidOperationException(
            $"Hyper Beam fire mismatch: fired={observedHyperBeamShot}, " +
            $"type=${observedHyperBeamType:X4}/9018, " +
            $"damage=${observedHyperBeamDamage:X4}/03E8, " +
            $"sound=${observedHyperBeamSound:X2}/1F, flare={observedHyperBeamFlare}.");
    }
    if (options.FrameCount >= 4 && !observedHyperBeamArt)
        throw new InvalidOperationException("Hyper Beam never decoded its native bank-$93 art.");
    if (runtime.Projectiles.ActiveTrailCount != 0)
        throw new InvalidOperationException("Hyper Beam incorrectly allocated an ordinary Wave trail.");

    if (options.FrameCount >= 23 &&
        (observedHyperBeamBodyPalettes.Count != 10 ||
         observedHyperBeamBodyPaletteHolds != 10 ||
         !observedHyperBeamBodySuitRestore))
    {
        throw new InvalidOperationException(
            $"Hyper Beam body palette cadence mismatch: palettes=" +
            $"{observedHyperBeamBodyPalettes.Count}/10, " +
            $"holds={observedHyperBeamBodyPaletteHolds}/10, " +
            $"suitRestore={observedHyperBeamBodySuitRestore}.");
    }

    // Every ROM record lasts two handler calls. Short captures must visit every record
    // they had time to reach; captures of nineteen calls or more must cover all ten before
    // the following odd call executes the terminal goto and starts the next cycle.
    int expectedPaletteFrameCount = Math.Min(
        HyperBeamPaletteFxState.FrameCount,
        (options.FrameCount + 1) / 2);
    if (observedHyperBeamPaletteFrames.Count != expectedPaletteFrameCount)
    {
        throw new InvalidOperationException(
            $"Hyper Beam palette-FX visited {observedHyperBeamPaletteFrames.Count} distinct " +
            $"ROM frames; {expectedPaletteFrameCount} were expected in {options.FrameCount} calls.");
    }

    Console.WriteLine(
        $"Hyper Beam ROM route fired type ${observedHyperBeamType:X4}, " +
        $"damage=${observedHyperBeamDamage:X4}, sound=${observedHyperBeamSound:X2}, " +
        $"art={observedHyperBeamArt}, flare={observedHyperBeamFlare}, trail=false; " +
        $"paletteFX={observedHyperBeamPaletteFrames.Count}/10 ROM frames, " +
        $"bodyGlow={observedHyperBeamBodyPalettes.Count}/10+restore.");
}

if (options.VisorScript)
{
    if (options.FrameCount >= 11 &&
        (!observedVisorPaletteOffsets.Contains(6) ||
         !observedVisorPaletteOffsets.Contains(8) ||
         !observedVisorPaletteOffsets.Contains(10)))
    {
        throw new InvalidOperationException(
            $"Visor ROM route missed its three room-cycle colors: " +
            $"offsets=[{string.Join(',', observedVisorPaletteOffsets.Order())}].");
    }
    Console.WriteLine(
        $"Visor ROM route observed offsets " +
        $"[{string.Join(',', observedVisorPaletteOffsets.Order())}] from $9B:A3C0.");
}

if (options.MissileScript)
{
    // A seven-frame run covers Start dismissal, the isolated fire edge, and the fourth
    // alpha pass that allocates exhaust. Requiring native type `$8100`, nonzero ROM damage,
    // sound three, decoded bank-$93 art, exactly one consumed round, and a live bank-$90
    // trail makes this proof span every subsystem visible in the resulting PNG.
    if (options.FrameCount >= 3 &&
        (!observedMissileShot || observedMissileDamage == 0 || observedMissileSound != 3))
    {
        throw new InvalidOperationException(
            $"Missile producer mismatch: fired={observedMissileShot}, " +
            $"damage=${observedMissileDamage:X4}, sound=${observedMissileSound:X2}.");
    }
    if (options.FrameCount >= 4 && !observedMissileArt)
        throw new InvalidOperationException("Missile route never decoded live bank-$93 art.");
    if (options.FrameCount >= 7 && !observedMissileTrail)
        throw new InvalidOperationException("Missile route never allocated bank-$90 exhaust.");
    if (options.FrameCount >= 3 && runtime.Samus!.Missiles != 9)
    {
        throw new InvalidOperationException(
            $"Missile route expected one round consumed from ten; remaining={runtime.Samus.Missiles}.");
    }
    if (options.FrameCount >= 4 &&
        (!observedArmCannonFrames.Contains(1) ||
         !observedArmCannonFrames.Contains(2) ||
         !observedArmCannonFrames.Contains(3) ||
         !observedArmCannonSprite ||
         !observedArmCannonTileDma))
    {
        throw new InvalidOperationException(
            $"Missile arm-cannon route missed native cover state: " +
            $"frames=[{string.Join(',', observedArmCannonFrames.Order())}], " +
            $"sprite={observedArmCannonSprite}, DMA={observedArmCannonTileDma}.");
    }

    Console.WriteLine(
        $"Missile ROM route fired={observedMissileShot}, damage=${observedMissileDamage:X4}, " +
        $"sound=${observedMissileSound:X2}, art={observedMissileArt}, " +
        $"trail={observedMissileTrail}, armFrames=[{string.Join(',', observedArmCannonFrames.Order())}], " +
        $"armSprite={observedArmCannonSprite}, armDMA={observedArmCannonTileDma}, " +
        $"ammo={runtime.Samus!.Missiles}.");
}

if (options.SuperMissileScript)
{
    if (options.FrameCount >= 3 &&
        (!observedSuperMissileShot ||
         observedSuperMissileDamage == 0 ||
         observedSuperMissileSound != 4 ||
         !observedSuperMissileLink))
    {
        throw new InvalidOperationException(
            $"Super Missile producer mismatch: fired={observedSuperMissileShot}, " +
            $"damage=${observedSuperMissileDamage:X4}, sound=${observedSuperMissileSound:X2}, " +
            $"link={observedSuperMissileLink}.");
    }
    if (options.FrameCount >= 4 && !observedSuperMissileArt)
        throw new InvalidOperationException("Super Missile route never decoded owner bank-$93 art.");
    if (options.FrameCount >= 7 && !observedSuperMissileTrail)
        throw new InvalidOperationException("Super Missile route never allocated two-count exhaust.");
    if (options.FrameCount >= 20 && (!observedSuperMissileImpact || !observedSuperMissileQuake))
    {
        throw new InvalidOperationException(
            $"Super Missile wall route did not complete its native impact: " +
            $"explosion={observedSuperMissileImpact}, quake={observedSuperMissileQuake}.");
    }
    if (options.FrameCount >= 3 && runtime.Samus!.SuperMissiles != 9)
    {
        throw new InvalidOperationException(
            $"Super Missile route expected one round consumed from ten; remaining={runtime.Samus.SuperMissiles}.");
    }

    Console.WriteLine(
        $"Super Missile ROM route fired={observedSuperMissileShot}, " +
        $"damage=${observedSuperMissileDamage:X4}, sound=${observedSuperMissileSound:X2}, " +
        $"art={observedSuperMissileArt}, link={observedSuperMissileLink}, " +
        $"trail={observedSuperMissileTrail}, impact={observedSuperMissileImpact}, " +
        $"quake={observedSuperMissileQuake}, ammo={runtime.Samus!.SuperMissiles}.");
}

if (options.MorphKnockbackScript)
{
    // Short runs remain useful as authentic airborne-art captures, but every milestone that
    // the requested frame count could have reached is mandatory. No host-selected target
    // pose can satisfy these checks: `$37/$F9`, `$DF15`, `$EE27`, `$F31D`, `$31`, and the
    // eventual `$1D` landing all execute through the live cartridge-backed runtime.
    if (options.FrameCount >= 17 &&
        !observedSamusPoses.Contains(SamusPoseIds.MorphBallGroundRightPose))
    {
        throw new InvalidOperationException(
            "Morphed-knockback ROM script never reached stable Morph Ball pose $1D.");
    }
    if (options.FrameCount >= 26 &&
        (!observedMorphedKnockbackPosePreserved ||
         !observedMorphedKnockbackAnimationPreserved ||
         !observedMorphedKnockbackDirectionRule ||
         !observedKnockbackMovement))
    {
        throw new InvalidOperationException(
            $"Morphed-knockback start mismatch: pose={observedMorphedKnockbackPosePreserved}, " +
            $"animation={observedMorphedKnockbackAnimationPreserved}, " +
            $"direction={observedMorphedKnockbackDirectionRule}, " +
            $"movement={observedKnockbackMovement}.");
    }
    if (options.FrameCount >= 31 && !observedMorphedKnockbackCompletion)
    {
        throw new InvalidOperationException(
            "Morphed-knockback route did not execute same-pose command-one cleanup.");
    }
    if (options.FrameCount >= 32 && !observedMorphedKnockbackFalling)
    {
        throw new InvalidOperationException(
            "Morphed-knockback route did not hand elevated $1D into falling pose $31.");
    }
    if (options.FrameCount >= 54 && !observedMorphedKnockbackLanding)
    {
        throw new InvalidOperationException(
            "Morphed-knockback route did not return through real floor collision to $1D.");
    }

    Console.WriteLine(
        $"Morphed-knockback ROM route validated every milestone reachable within {options.FrameCount} frame(s).");
}

if (options.MoonwalkScript)
{
    // These are not host-selected poses. Every member must have been observed after the
    // unchanged retail transition matcher consumed the scripted chords. `$BF` is the
    // grounded turn art and `$1A` proves its input/animation handoff created a real jump.
    byte[] requiredMoonwalkRoute =
    [
        SamusPoseIds.MoonwalkFacingRightPose,
        SamusPoseIds.MoonwalkAimUpRightPose,
        SamusPoseIds.MoonwalkAimDownRightPose,
        SamusPoseIds.MoonwalkTurnJumpLeftPose,
        SamusPoseIds.SpinJumpLeftPose,
    ];
    foreach (byte requiredPose in requiredMoonwalkRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
        {
            throw new InvalidOperationException(
                $"Moonwalk ROM script did not observe required pose ${requiredPose:X2}.");
        }
    }
    Console.WriteLine(
        "Moonwalk ROM route validated stable neutral/up/down movement, zero-input fallback, grounded $BF turn art, and $1A jump handoff.");
}

if (options.RanIntoWallScript)
{
    // Placement alone cannot satisfy these checks: the observed set is populated only
    // after complete runtime frames. Thus each wall pose proves controller matching,
    // prospective-pose filtering, bank-$94 collision, and the ten-way selector together.
    byte[] requiredWallRoute =
    [
        SamusPoseIds.RanIntoWallRightPose,
        SamusPoseIds.RanIntoWallAimUpRightPose,
        SamusPoseIds.RanIntoWallAimDownRightPose,
        SamusPoseIds.NeutralJumpTransitionRightPose,
        SamusPoseIds.NeutralJumpRightPose,
    ];
    foreach (byte requiredPose in requiredWallRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
        {
            throw new InvalidOperationException(
                $"Ran-into-wall ROM script did not observe required pose ${requiredPose:X2}.");
        }
    }
    if (!observedBlockedRanIntoWallProbe)
    {
        throw new InvalidOperationException(
            "Ran-into-wall ROM script never observed a blocked one-pixel probe.");
    }
    Console.WriteLine(
        "Ran-into-wall ROM route validated neutral/up/down wall art, zero-input fallback, and the $89 -> $4B -> $4D jump exit.");
}

if (options.RunScript)
{
    // These are post-frame observations from the complete ROM-backed runtime. The checks
    // jointly prove transition-table admission, exact `$90:973E` accumulation/cap, and the
    // type-three `$90:9808` retention route; a host-authored displacement cannot satisfy
    // the state assertions by merely moving Samus farther.
    if (!observedSamusPoses.Contains(SamusPoseIds.MovingRightNormalPose))
        throw new InvalidOperationException("Dash ROM script never entered running pose $09.");
    if (!observedDashMomentum)
        throw new InvalidOperationException("Dash ROM script never established momentum flag $0B3C.");
    if (options.FrameCount >= 38 && maximumObservedExtraRunSpeed != 0x00020000u)
    {
        throw new InvalidOperationException(
            $"Dash ROM script expected maximum extra speed 2.0000, observed ${maximumObservedExtraRunSpeed:X8}.");
    }
    if (options.FrameCount >= 40 &&
        (!observedSamusPoses.Contains(SamusPoseIds.SpinJumpRightPose) || !observedDashAerialCarry))
    {
        throw new InvalidOperationException(
            "Dash ROM script did not retain the accumulated extra component through spin pose $19.");
    }
    Console.WriteLine(
        $"Dash ROM route validated momentum, ordinary cap ${maximumObservedExtraRunSpeed >> 16:X4}." +
        $"{maximumObservedExtraRunSpeed & 0xffff:X4}, shared running animation timing, and spin-jump carry.");
}

if (options.SpaceJumpScript || options.WaterSpaceJumpScript || options.ScrewAttackScript)
{
    byte requiredSpinPose = options.ScrewAttackScript
        ? SamusPoseIds.ScrewAttackRightPose
        : SamusPoseIds.SpaceJumpRightPose;
    if (options.FrameCount >= 15 && !observedSamusPoses.Contains(requiredSpinPose))
    {
        throw new InvalidOperationException(
            $"Special-spin ROM script never installed required pose ${requiredSpinPose:X2}.");
    }
    if (options.FrameCount >= (options.ScrewAttackScript ? 48 : 43) &&
        observedSpaceJumpRestarts == 0)
    {
        throw new InvalidOperationException(
            "Special-spin ROM script reached the repeat interval but never restarted upward.");
    }
    if (options.ScrewAttackScript &&
        options.FrameCount >= 15 &&
        !observedScrewAttackContactDamage)
    {
        throw new InvalidOperationException(
            "Screw Attack ROM script never published native contact-damage index 3.");
    }
    bool submergedScrewRoute = options.ScrewAttackScript && options.WaterSpaceJumpScript;
    if (options.ScrewAttackScript &&
        options.FrameCount >= 34 &&
        !submergedScrewRoute &&
        !observedScrewAttackPaletteCycle)
    {
        throw new InvalidOperationException(
            "Screw Attack ROM script never reached the frame-27 palette cycle.");
    }
    if (submergedScrewRoute &&
        options.FrameCount >= 34 &&
        !observedSubmergedScrewPaletteFreeze)
    {
        throw new InvalidOperationException(
            "Submerged Screw ROM script never proved the frame-27 palette freeze.");
    }
    Console.WriteLine(
        $"Special-spin ROM route validated pose ${requiredSpinPose:X2}, " +
        $"{observedSpaceJumpRestarts} accepted repeat(s)" +
        (options.ScrewAttackScript
            ? submergedScrewRoute
                ? $", contact damage 3, and submerged palette freeze={observedSubmergedScrewPaletteFreeze}."
                : $", contact damage 3, and palette cycle={observedScrewAttackPaletteCycle}."
            : "."));
}

if (options.SpeedBoosterScript)
{
    if (!observedSamusPoses.Contains(SamusPoseIds.MovingRightNormalPose) || !observedDashMomentum)
        throw new InvalidOperationException("Speed Booster ROM script never established running momentum.");
    if (maximumObservedSpeedBoostStage == 0 && options.FrameCount >= 30)
        throw new InvalidOperationException("Speed Booster ROM script never loaded a staged counter.");
    if (options.FrameCount >= 103 &&
        (!observedSpeedBoostEcho || !observedSpeedBoostContactDamage || maximumObservedSpeedBoostStage != 4))
    {
        throw new InvalidOperationException(
            $"Speed Booster ROM script expected stage four with echo/contact; observed stage {maximumObservedSpeedBoostStage}, " +
            $"echo={observedSpeedBoostEcho}, contact={observedSpeedBoostContactDamage}.");
    }
    if (options.FrameCount >= 116 && maximumObservedExtraRunSpeed != 0x00070000u)
    {
        throw new InvalidOperationException(
            $"Speed Booster ROM script expected the 7.0000 cap, observed ${maximumObservedExtraRunSpeed:X8}.");
    }
    if (options.FrameCount >= 110 && !observedSpeedBoostFootDust)
    {
        throw new InvalidOperationException(
            "Speed Booster ROM script reached stage four without producing native type-seven foot dust.");
    }
    if (options.FrameCount >= 190 &&
        (!observedSpeedBoostDeparture || !observedSpeedBoostDepartureFinished))
    {
        throw new InvalidOperationException(
            "Speed Booster ROM script did not observe both the ordinary cancellation-echo departure and its completion.");
    }
    Console.WriteLine(
        $"Speed Booster ROM route observed maximum extra speed ${maximumObservedExtraRunSpeed >> 16:X4}." +
        $"{maximumObservedExtraRunSpeed & 0xffff:X4}, stage {maximumObservedSpeedBoostStage}, " +
        $"echo={observedSpeedBoostEcho}, contact={observedSpeedBoostContactDamage}, " +
        $"footDust={observedSpeedBoostFootDust}, " +
        $"departure={observedSpeedBoostDeparture}/{observedSpeedBoostDepartureFinished}.");
}

if (options.ShinesparkScript)
{
    // These assertions deliberately observe states after full StepFrame calls. A direct
    // test helper that invoked SamusShinesparkState would miss the point of this route:
    // real bank-$91 input records, delayed animation commands, palette priority, room
    // collision, and runtime handler dispatch must all cooperate in their normal order.
    if (!observedStoredShine)
        throw new InvalidOperationException("Shinespark ROM script never stored a stage-four shine while crouching.");
    if (!observedShinesparkPalette)
        throw new InvalidOperationException("Shinespark ROM script never installed a stored/active shine palette handler.");
    if (options.FrameCount >= 155 && !observedShinesparkWindup)
        throw new InvalidOperationException("Shinespark ROM script never reached windup pose $C7/$C8.");
    if (options.FrameCount >= 160 && (!observedDirectionalShinespark || !observedShinesparkMovement))
    {
        throw new InvalidOperationException(
            "Shinespark ROM script never launched a directional spark through the live movement handler.");
    }
    if (options.FrameCount >= 220 && !observedShinesparkCrashOrbit)
        throw new InvalidOperationException("Shinespark ROM script never entered the collision crash orbit.");
    if (options.FrameCount >= 220 && !observedShinesparkCrashDrawingHandler)
        throw new InvalidOperationException("Shinespark crash never installed its dedicated draw handler.");
    if (options.FrameCount >= 260 && !observedShinesparkCrashEchoCircle)
        throw new InvalidOperationException("Shinespark ROM script never entered the 30-frame crash echo circle.");
    if (options.FrameCount >= 291 &&
        (!observedShinesparkCrashFinish || !observedReleasedShinesparkEcho))
    {
        throw new InvalidOperationException(
            "Shinespark ROM script never completed crash into the departing projectile echoes.");
    }
    Console.WriteLine(
        $"Shinespark ROM route observed stored={observedStoredShine}, windup={observedShinesparkWindup}, " +
        $"directional={observedDirectionalShinespark}, movement={observedShinesparkMovement}, " +
        $"palette={observedShinesparkPalette}, crash={observedShinesparkCrashOrbit}, " +
        $"crashDraw={observedShinesparkCrashDrawingHandler}, " +
        $"circle={observedShinesparkCrashEchoCircle}, finish={observedShinesparkCrashFinish}, " +
        $"releasedEcho={observedReleasedShinesparkEcho}.");
}

if (options.GrappleScript)
{
    if (!observedGrappleSwing)
        throw new InvalidOperationException("Grapple ROM script never executed connected swinging.");
    if (!observedGrappleTerrainCollision)
        throw new InvalidOperationException("Grapple ROM script never exercised connected terrain collision.");
    if (!observedGrappleDrawHandler || !observedGrappleBeamDrawPath || !observedGrappleFlare)
    {
        throw new InvalidOperationException(
            "Grapple ROM script never rendered the complete $90:EB86 flare/body/rope path.");
    }
    if (options.FrameCount >= 91 && !observedGrappleReleaseQueue)
        throw new InvalidOperationException("Grapple ROM script never queued release at $9B:C79D.");
    if (options.FrameCount >= 91 && !observedGrappleTeardownDrawFallback)
    {
        throw new InvalidOperationException(
            "Grapple release frame never took $90:EB86's body-and-echo fallback.");
    }
    if (options.FrameCount >= 92 && !observedGrappleRelease)
        throw new InvalidOperationException("Grapple ROM script never completed release at $9B:CB8B.");
    if (options.FrameCount >= 91 && !observedGrappleReleaseMovement)
        throw new InvalidOperationException("Grapple ROM script never executed release movement handler $90:946E.");
    Console.WriteLine(
        $"Grapple ROM route validated connected pendulum stepping and six-point terrain reflection" +
        (options.FrameCount >= 92
            ? ", queued release, persistent $90:946E motion, and jump-pose handoff."
            : "."));
}

if (options.CrystalFlashScript)
{
    if (!observedSamusPoses.Contains(SamusPoseIds.CrystalFlashRightPose))
        throw new InvalidOperationException("Crystal Flash ROM route never rendered pose $D3.");
    if (options.FrameCount >= 11 && !observedCrystalFlashDrain)
        throw new InvalidOperationException("Crystal Flash ROM route never installed ammo handler $90:D6CE.");
    if (options.FrameCount >= 11 && !observedCrystalFlashWindowExpansion)
        throw new InvalidOperationException("Crystal Flash ROM route never spawned bank-$88 bubble expansion.");
    if (options.FrameCount >= 30 && !observedCrystalFlashWindowAfterglow)
        throw new InvalidOperationException("Crystal Flash ROM route never reached bank-$88 bubble afterglow.");
    if (!observedCrystalFlashPalette)
        throw new InvalidOperationException("Crystal Flash ROM route never copied its bank-$9B sprite palette.");
    if (options.FrameCount >= 249 && !observedCrystalFlashFinish)
        throw new InvalidOperationException("Crystal Flash ROM route never consumed all three ammo families.");
    if (options.FrameCount >= 265 && !observedCrystalFlashCompletion)
        throw new InvalidOperationException("Crystal Flash ROM route never returned to normal movement.");
    Console.WriteLine(
        $"Crystal Flash ROM route observed drain={observedCrystalFlashDrain}, " +
        $"window={observedCrystalFlashWindowExpansion}/{observedCrystalFlashWindowAfterglow}, " +
        $"palette={observedCrystalFlashPalette}, finish={observedCrystalFlashFinish}, " +
        $"complete={observedCrystalFlashCompletion}, " +
        $"energy={runtime.Samus.Health}, ammo={runtime.Samus.Missiles}/" +
        $"{runtime.Samus.SuperMissiles}/{runtime.Samus.PowerBombs}.");
}

if (options.XrayScript)
{
    // Keep short captures useful while making every reached timeline boundary strict. The
    // thresholds are counts of accepted NMI/runtime calls, not guesses about wall-clock time.
    if (options.FrameCount >= 8 && runtime.Samus!.Xray.SetupStage != 0)
        throw new InvalidOperationException("X-ray ROM route did not finish all eight setup calls.");
    if (options.FrameCount >= 9 && !observedXrayPhases.Contains(XrayBeamPhase.Widening))
        throw new InvalidOperationException("X-ray ROM route never entered widening state one.");
    if (options.FrameCount >= 36 && !observedXrayPhases.Contains(XrayBeamPhase.Full))
        throw new InvalidOperationException("X-ray ROM route never reached the native ten-unit full beam.");
    if (options.FrameCount >= 37 && !observedXrayAim)
        throw new InvalidOperationException("X-ray ROM route never changed its center angle while full.");
    if (options.FrameCount >= 51 && !observedXrayTurnStart)
        throw new InvalidOperationException("X-ray ROM route never installed standing turn pose $25.");
    if (options.FrameCount >= 80 && (!observedXrayTurnCompletion || !observedXrayLeftStablePose))
    {
        throw new InvalidOperationException(
            $"X-ray ROM route did not complete $D5->$25->$D6; pose=${runtime.Samus!.Pose:X2}, " +
            $"frame={runtime.Samus.AnimationFrame}, timer={runtime.Samus.AnimationFrameTimer}.");
    }

    Console.WriteLine(
        $"X-ray ROM route validated setup/full={observedXrayPhases.Contains(XrayBeamPhase.Full)}, " +
        $"aim={observedXrayAim}, turn={observedXrayTurnStart}/{observedXrayTurnCompletion}, " +
        $"pose=${runtime.Samus!.Pose:X2}, angle=${runtime.Samus.Xray.Angle.TableIndex:X2}, " +
        $"art=[{string.Join(',', observedXrayAnimationFrames.Order())}].");
}

if (options.DeathScript)
{
    if (runtime.Samus!.Pose != SamusPoseIds.DeathSequenceRightPose)
        throw new InvalidOperationException($"Death route left required pose $D7 for ${runtime.Samus.Pose:X2}.");
    if (options.FrameCount >= 16 && !observedDeathPhases.Contains(SamusDeathSequencePhase.Flashing))
        throw new InvalidOperationException("Death route did not finish its sixteen-call preflash.");
    if (options.FrameCount >= 76 &&
        (!observedDeathPhases.Contains(SamusDeathSequencePhase.SuitExplosion) ||
         observedDeathSegments.Count != 5))
    {
        throw new InvalidOperationException(
            $"Death route did not reach suit explosion with five tile segments; " +
            $"phase={runtime.Samus.DeathSequence.Phase}, segments={observedDeathSegments.Count}.");
    }
    if (options.FrameCount >= 97 && !observedDeathWhiteout)
        throw new InvalidOperationException("Death route never began the room-palette whiteout.");
    if (options.FrameCount >= 211 && !observedDeathCompletion)
        throw new InvalidOperationException("Death route did not reach terminal explosion index nine.");

    Console.WriteLine(
        $"Death ROM route validated phases=[{string.Join(',', observedDeathPhases.Order())}], " +
        $"segments=[{string.Join(',', observedDeathSegments.Order())}], " +
        $"explosionMaps={observedDeathExplosionSpritemaps.Count}, " +
        $"whiteout={observedDeathWhiteout}, complete={observedDeathCompletion}.");
}

if (options.MotherBrainRainbowScript)
{
    if (rainbowAttack is null)
        throw new InvalidOperationException("Mother Brain rainbow route was not initialized.");
    if (options.FrameCount >= 258 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.RetractNeck))
    {
        throw new InvalidOperationException(
            "Mother Brain rainbow ROM route never reached the retracting body walk.");
    }
    if (options.FrameCount >= 571 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall))
    {
        throw new InvalidOperationException(
            "Mother Brain rainbow ROM route never completed charge into the active beam.");
    }
    if (options.FrameCount >= 880 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.LetSamusFall))
    {
        throw new InvalidOperationException(
            "Mother Brain rainbow ROM route never drained and released Samus.");
    }
    if (options.FrameCount >= 1100 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.FinishSamusOff))
    {
        throw new InvalidOperationException(
            $"Mother Brain rainbow ROM route did not enter finish-off AI; phase={rainbowAttack.Phase}.");
    }
    if (options.FrameCount >= 1200 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam))
    {
        throw new InvalidOperationException(
            $"Mother Brain rainbow ROM route did not finish its body walk/stand delay; phase={rainbowAttack.Phase}.");
    }
    if (options.FrameCount >= 1450 &&
        (observedBabyTileTransfers.Count != 4 || !observedBabySpawnRequest))
    {
        throw new InvalidOperationException(
            $"Mother Brain rainbow ROM route did not DMA/spawn Baby exactly once; " +
            $"transfers={observedBabyTileTransfers.Count}, spawn={observedBabySpawnRequest}, " +
            $"phase={rainbowAttack.Phase}.");
    }
    if (options.FrameCount >= 1700 &&
        (!observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding) ||
         !observedFinalBeamSound))
    {
        throw new InvalidOperationException(
            $"Mother Brain rainbow ROM route did not reach final-beam hold; " +
            $"sound={observedFinalBeamSound}, phase={rainbowAttack.Phase}.");
    }
    if (options.FrameCount >= 2050 &&
        (cutsceneBaby is null ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.LatchOntoMotherBrain) ||
         !observedBabyMotherBrainInterrupt))
    {
        throw new InvalidOperationException(
            $"Cutscene Baby did not complete its ROM-backed entrance/latch by the " +
            $"regression boundary; phase={cutsceneBaby?.Phase.ToString() ?? "not spawned"}, " +
            $"interrupted={observedBabyMotherBrainInterrupt}.");
    }
    if (options.FrameCount >= 3340 &&
        (rainbowAttack.Phase2CorpseState == 0 || rainbowAttack.BrainHealth != 0x8ca0))
    {
        throw new InvalidOperationException(
            $"Mother Brain did not reach the ROM-backed corpse/revival producer; " +
            $"phase={rainbowAttack.Phase}, corpse={rainbowAttack.Phase2CorpseState}, " +
            $"brainHealth=${rainbowAttack.BrainHealth:X4}.");
    }
    if (options.FrameCount >= 3477 &&
        (cutsceneBaby is null ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.MoveToTheCeiling) ||
         !observedBabyCeilingTableInstall))
    {
        throw new InvalidOperationException(
            $"Cutscene Baby did not complete drain release and ceiling retreat; " +
            $"phase={cutsceneBaby?.Phase.ToString() ?? "not spawned"}, " +
            $"movementTable=${cutsceneBaby?.MovementTablePointer:X4}.");
    }
    if (options.FrameCount >= 3819)
    {
        // These values come from the private retail-ROM run itself after correcting the
        // header `$24/$24` words to the literal hitbox radii loaded by `$A0:8AFF/$8B05`.
        // Each point retains every earlier 8.8 carry and catches either a radius regression
        // or a movement-helper rounding error even when the route eventually converges.
        (ushort Pointer, int Frame, BabyMetroidCutscenePoint Point)[] expectedRoute =
        [
            (0xca2c, 3524, new(0x007e, 0x2800, 0x0051, 0x4100)),
            (0xca34, 3591, new(0x010d, 0x9600, 0x008e, 0x5f00)),
            (0xca3c, 3688, new(0x00e6, 0x2600, 0x004d, 0x3500)),
            (0xca44, 3689, new(0x00e4, 0xaa00, 0x004c, 0x2700)),
            (0xca4c, 3768, new(0x00c6, 0x6600, 0x0059, 0x3c00)),
            (0xca54, 3788, new(0x00ca, 0x9400, 0x0069, 0x1d00)),
            (0xca5c, 3805, new(0x00ce, 0x0200, 0x0079, 0xb500)),
        ];
        foreach ((ushort pointer, int frame, BabyMetroidCutscenePoint point) in expectedRoute)
        {
            bool sawFrame = observedBabyRouteFrames.TryGetValue(pointer, out int actualFrame);
            bool sawPoint = observedBabyRoutePoints.TryGetValue(
                pointer,
                out BabyMetroidCutscenePoint actualPoint);
            if (!sawFrame ||
                actualFrame != frame ||
                !sawPoint ||
                actualPoint != point)
            {
                throw new InvalidOperationException(
                    $"Baby ROM route witness ${pointer:X4} differed: expected frame {frame} " +
                    $"at {point}, got frame {actualFrame} at {actualPoint}.");
            }
        }
        if (observedBabyLatchOntoSamusFrame != 3819)
        {
            throw new InvalidOperationException(
                $"Baby did not install gradual Samus pursuit `$CA66` on frame 3819; " +
                $"observed {observedBabyLatchOntoSamusFrame}.");
        }
    }
    if (options.FrameCount >= 3837 && observedBabyHealSamusFrame != 3837)
    {
        throw new InvalidOperationException(
            $"Baby generic touch AI did not latch on frame 3837; " +
            $"observed {observedBabyHealSamusFrame}.");
    }
    if (options.FrameCount >= 4536 &&
        (cutsceneBaby is null ||
         !observedBabyHealingCompletion ||
         observedBabyHealingCompletionFrame != 4536 ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.IdleUntilNoHealth) ||
         observedBabyHealingCompletionHealth != runtime.Samus.MaxHealth ||
         observedBabyHealingCompletionReserveEnergy != runtime.Samus.MaxReserveEnergy))
    {
        throw new InvalidOperationException(
            $"Baby did not complete the ROM-backed 699-call heal on frame 4536; " +
            $"completion={observedBabyHealingCompletion}/" +
            $"{observedBabyHealingCompletionFrame}, phase=" +
            $"{cutsceneBaby?.Phase.ToString() ?? "not spawned"}, completion energy=" +
            $"{observedBabyHealingCompletionHealth}/{runtime.Samus.MaxHealth}, reserves=" +
            $"{observedBabyHealingCompletionReserveEnergy}/{runtime.Samus.MaxReserveEnergy}.");
    }
    if (options.FrameCount >= 5703 &&
        (cutsceneBaby is null ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.FinalCharge) ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.DeathSequence) ||
         cutsceneBaby.Health != 0))
    {
        throw new InvalidOperationException(
            $"Mother Brain's ROM-backed ring volleys/final charge did not reach the " +
            $"Baby death-sequence seam; phase={cutsceneBaby?.Phase.ToString() ?? "not spawned"}, " +
            $"health=${cutsceneBaby?.Health:X4}.");
    }
    if (options.FrameCount >= 6168 &&
        (cutsceneBaby is null ||
         !cutsceneBaby.IsDeleted ||
         !observedBabyPhaseThreeHandoff ||
         observedBabyDeathExplosions != 30 ||
         allocatedBabyDeathExplosions != 30 ||
         observedBabyDeathPalettes.Count != 6 ||
         observedAttackTileTransfers.Count != 4 ||
         observedPhaseThreeBackgroundPalettes.Count != 7 ||
         runtime.Samus.HyperBeam != 0x8000 ||
         runtime.Samus.Drained.RainbowPaletteEnabled))
    {
        throw new InvalidOperationException(
            $"Baby death/recovery producer differed at frame 6168: " +
            $"deleted={cutsceneBaby?.IsDeleted}, handoff={observedBabyPhaseThreeHandoff}, " +
            $"explosions={observedBabyDeathExplosions}/{allocatedBabyDeathExplosions} allocated, " +
            $"black palettes=" +
            $"{observedBabyDeathPalettes.Count}, attack DMA={observedAttackTileTransfers.Count}, " +
            $"room palettes={observedPhaseThreeBackgroundPalettes.Count}, " +
            $"hyper=${runtime.Samus.HyperBeam:X4}, " +
            $"rainbow={runtime.Samus.Drained.RainbowPaletteEnabled}.");
    }
    if (options.FrameCount >= 6202 &&
        (!observedRainbowPhases.Contains(
             MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown) ||
         !observedPhaseThreeAttacks.Contains(MotherBrainPhase3AttackKind.FourOnionRings)))
    {
        throw new InvalidOperationException(
            $"Mother Brain did not finish recovery and select the deterministic first " +
            $"phase-three attack on frame 6202; phase={rainbowAttack.Phase}, attacks=" +
            $"{string.Join(",", observedPhaseThreeAttacks)}.");
    }
    if (options.FrameCount >= 6272 &&
        !observedPhaseThreeAttacks.Contains(MotherBrainPhase3AttackKind.Bomb))
    {
        throw new InvalidOperationException(
            $"Mother Brain did not select the deterministic phase-three bomb at the " +
            $"post-cooldown RNG boundary; phase={rainbowAttack.Phase}, attacks=" +
            $"{string.Join(",", observedPhaseThreeAttacks)}.");
    }
    if (options.FrameCount >= 6500 && !observedPhaseThreeForwardMovement)
    {
        throw new InvalidOperationException(
            $"Mother Brain's phase-three walking scheduler never produced positive body " +
            $"movement; body X={rainbowAttack.Body.XPosition}, walk=" +
            $"{rainbowAttack.Phase3WalkingPhase}/${rainbowAttack.Phase3WalkCounter:X4}.");
    }
    if (options.FrameCount >= 1450)
    {
        // The actor request alone is not enough evidence: prove the ordinary NMI queue
        // copied every byte from each real LoROM source into the encoded VRAM word address.
        // Before death these rows must contain the Baby source. Once `$CCC0` deliberately
        // overwrites them, only the later bank-$B7 assertion below describes final VRAM.
        foreach (MotherBrainSpriteTileTransferRequest transfer in
                 observedAttackTileTransfers.Count == 0
                     ? observedBabyTileTransfers
                     : [])
        {
            int vramByteAddress = transfer.VramDestination * 2;
            for (int byteOffset = 0; byteOffset < transfer.Size; byteOffset++)
            {
                byte expected = bus.ReadByte(checked((int)transfer.SourceAddress) + byteOffset);
                byte actual = runtime.Vram.ReadByte(vramByteAddress + byteOffset);
                if (actual != expected)
                {
                    throw new InvalidOperationException(
                        $"Baby tile DMA entry {transfer.EntryIndex} differs at byte " +
                        $"${byteOffset:X4}: expected ${expected:X2}, got ${actual:X2}.");
                }
            }
        }

        foreach (MotherBrainSpriteTileTransferRequest transfer in observedAttackTileTransfers)
        {
            // The death sequence overwrites exactly the same VRAM rows. Prove the final
            // bytes came from bank `$B7`, not merely that four requests were observed.
            int vramByteAddress = transfer.VramDestination * 2;
            for (int byteOffset = 0; byteOffset < transfer.Size; byteOffset++)
            {
                byte expected = bus.ReadByte(checked((int)transfer.SourceAddress) + byteOffset);
                byte actual = runtime.Vram.ReadByte(vramByteAddress + byteOffset);
                if (actual != expected)
                {
                    throw new InvalidOperationException(
                        $"Attack tile DMA entry {transfer.EntryIndex} differs at byte " +
                        $"${byteOffset:X4}: expected ${expected:X2}, got ${actual:X2}.");
                }
            }
        }
    }
    Console.WriteLine(
        $"Mother Brain rainbow ROM route ended at {rainbowAttack.Phase}; " +
        $"body=({rainbowAttack.Body.XPosition},{rainbowAttack.Body.YPosition})/" +
        $"pose {rainbowAttack.Body.Pose}, Samus=({runtime.Samus.XPosition}," +
        $"{runtime.Samus.YPosition}) energy={runtime.Samus.Health}, " +
        $"ammo={runtime.Samus.Missiles}/{runtime.Samus.SuperMissiles}/" +
        $"{runtime.Samus.PowerBombs}, Baby DMA/spawn=" +
        $"{observedBabyTileTransfers.Count}/{observedBabySpawnRequest}, Baby AI=" +
        $"{cutsceneBaby?.Phase.ToString() ?? "not spawned"}/" +
        $"({cutsceneBaby?.XPosition:X4},{cutsceneBaby?.YPosition:X4}), phase-three attacks=" +
        $"{string.Join("/", observedPhaseThreeAttacks)}, forward=" +
        $"{observedPhaseThreeForwardMovement}.");
}

if (options.DrainedSamusScript)
{
    if (options.FrameCount >= 20 && !observedDrainedFallingHandler)
        throw new InvalidOperationException("Drained ROM script never executed `$F7` handler $90:94CB.");
    if (options.FrameCount >= 50 && !observedDrainedLanding)
        throw new InvalidOperationException("Drained ROM script never collided with the live room floor.");
    if (options.FrameCount >= 91 && (!observedDrainedStanding || !observedDrainedCrouching))
        throw new InvalidOperationException("Drained ROM script missed controller-one/four poses $EA/$E8.");
    if (options.FrameCount >= 150 && !observedDrainedRelease)
        throw new InvalidOperationException("Drained ROM script never completed its `$FD,$01` release.");
    if (options.FrameCount >= 151 && !observedDrainedHyperBeam)
        throw new InvalidOperationException("Drained ROM script never installed hyper beam word $8000.");
    Console.WriteLine(
        $"Drained Samus ROM route observed falling={observedDrainedFallingHandler}, " +
        $"landing={observedDrainedLanding}, standing={observedDrainedStanding}, " +
        $"crouching={observedDrainedCrouching}, release={observedDrainedRelease}, " +
        $"hyper={observedDrainedHyperBeam}.");
}

if (options.DraygonGrabScript)
{
    if (options.FrameCount >= 17 && !observedDraygonAimUp)
        throw new InvalidOperationException("Draygon ROM route never reached aim-up pose $ED.");
    if (options.FrameCount >= 33 && !observedDraygonFiring)
        throw new InvalidOperationException("Draygon ROM route never reached firing pose $EE.");
    if (options.FrameCount >= 49 && !observedDraygonAimDown)
        throw new InvalidOperationException("Draygon ROM route never reached aim-down pose $EF.");
    if (options.FrameCount >= 65 && !observedDraygonMoving)
        throw new InvalidOperationException("Draygon ROM route never reached moving pose $F0.");
    if (options.FrameCount >= 97 && !observedDraygonNeutralFallback)
        throw new InvalidOperationException("Draygon ROM route never applied $F0 -> $EC no-input fallback.");
    if (options.FrameCount >= 171 && !observedDraygonRelease)
        throw new InvalidOperationException("Draygon ROM route did not release on its sixtieth counted D-pad pattern.");
    Console.WriteLine(
        $"Draygon ROM route validated every pose/escape milestone reachable within " +
        $"{options.FrameCount} frame(s); escape count=" +
        $"{runtime.Samus!.DraygonGrabbed.EscapeButtonCounter}, " +
        $"owner release={runtime.Samus.DraygonGrabbed.ReleasePublishedToOwner}.");
}

if (options.ExtraDisplacementScript)
{
    if (options.FrameCount >= 32 && !observedExternalXMovement)
        throw new InvalidOperationException("External-displacement ROM route did not apply 32 persistent +1.0000 X samples.");
    if (options.FrameCount >= 48 && !observedExternalUpMovement)
        throw new InvalidOperationException("External-displacement ROM route did not move upward from signed -0.8000 Y.");
    if (options.FrameCount >= 64 && !observedExternalDownCollision)
        throw new InvalidOperationException("External-displacement ROM route did not clip its biased positive Y movement against the live floor.");
    if (options.FrameCount >= 65 && !observedExternalWordsCleared)
        throw new InvalidOperationException("External-displacement producer words did not remain clear after frame 64.");
    Console.WriteLine(
        $"External-displacement ROM route validated X={observedExternalXMovement}, " +
        $"up={observedExternalUpMovement}, floor={observedExternalDownCollision}, " +
        $"clear={observedExternalWordsCleared}; final=({runtime.Samus!.XPosition}," +
        $"{runtime.Samus.YPosition}).");
}

if (options.GrappleFireScript)
{
    if (!observedGrappleFire)
        throw new InvalidOperationException("Grapple-fire ROM script never initialized or extended a beam.");
    if (!observedGrappleDrawHandler || !observedGrappleBeamDrawPath || !observedGrappleFlare)
    {
        throw new InvalidOperationException(
            "Grapple-fire ROM script never rendered the complete $90:EB86 flare/body/rope path.");
    }
    if (options.FrameCount >= 13 && !observedGrappleFireCancelQueue)
        throw new InvalidOperationException("Grapple-fire ROM script never queued collision/range cancellation.");
    if (options.FrameCount >= 13 && !observedGrappleTeardownDrawFallback)
    {
        throw new InvalidOperationException(
            "Grapple cancellation frame never took $90:EB86's body-and-echo fallback.");
    }
    if (options.FrameCount >= 14 && !observedGrappleFireCancel)
        throw new InvalidOperationException("Grapple-fire ROM script never completed queued cancellation.");
    Console.WriteLine(
        $"Grapple-fire ROM route validated pose-table initialization and every firing/cancellation milestone reachable within {options.FrameCount} frame(s).");
}

if (options.GunshipScript)
{
    if (options.FrameCount >= 640 && !observedGunshipExit)
        throw new InvalidOperationException("Gunship script did not finish its native exit wait.");
    Console.WriteLine(
        $"Gunship ROM route: function=$A2:{gunshipTop.VariableF:X4}, " +
        $"prompt={runtime.Enemies.GunshipSavePromptPending}, " +
        $"inputLocked={runtime.Samus!.InputLocked}, exit={observedGunshipExit}.");
}

Console.WriteLine(
    $"Finished at accepted NMI {runtime.NmiFrameCounter}; " +
    $"timer {runtime.EscapeTimer.MinutesBcd:X2}:{runtime.EscapeTimer.SecondsBcd:X2}.{runtime.EscapeTimer.CentisecondsBcd:X2}; " +
    $"camera=({camera.XPosition:X4}.{camera.XSubposition:X4},{camera.YPosition:X4}.{camera.YSubposition:X4}); " +
    $"minimap=({runtime.Hud.MinimapCenterX},{runtime.Hud.MinimapCenterY}); " +
    $"projectileTrails={runtime.Projectiles.ActiveTrailCount}; " +
    $"OAM staged/displayed sprites={runtime.Oam.LastFinalizedSpriteCount}/{runtime.DisplayedOam.LastFinalizedSpriteCount}; " +
    $"VRAM[$F000..$F00F] = {Convert.ToHexString(runtime.Vram.Bytes[0xf000..0xf010])}.");
Console.WriteLine(
    $"BG2 map row heads $4800/$4880/$48E0/$4900/$4A00 = " +
    $"${runtime.Vram.ReadWord(0x4800):X4}/${runtime.Vram.ReadWord(0x4880):X4}/" +
    $"${runtime.Vram.ReadWord(0x48e0):X4}/${runtime.Vram.ReadWord(0x4900):X4}/" +
    $"${runtime.Vram.ReadWord(0x4a00):X4}.");

OamEntry firstSamusSprite = runtime.DisplayedOam.GetEntry(0);
Console.WriteLine(
    $"First staged gameplay OBJ (bombs precede Samus while active): X={firstSamusSprite.X}, Y={firstSamusSprite.Y}, " +
    $"tile=${firstSamusSprite.TileNumber:X3}, palette={firstSamusSprite.Palette}, " +
    $"priority={firstSamusSprite.Priority}, large={firstSamusSprite.IsLarge}. " +
    $"definitions=${runtime.Samus!.TileTransfers.TopDefinitionAddress:X6}/" +
    $"${runtime.Samus.TileTransfers.BottomDefinitionAddress:X6}.");

// Convert the same finalized OAM, VRAM, and CGRAM buffers a real NMI would send to the PPU
// into a transparent desktop image. This is intentionally after the breakpoint loop so a
// developer can compare the PNG against those three live hardware-model objects.
Rgba32[] objFrame = SnesObjRenderer.Render(runtime.DisplayedOam, runtime.Vram, runtime.Cgram, obsel: 0x03);
string objectOutputPath = Path.Combine(
    Path.GetDirectoryName(Path.GetFullPath(options.OutputPath))!,
    Path.GetFileNameWithoutExtension(options.OutputPath) + ".objects.png");
PngWriter.WriteRgba(objectOutputPath, width: 256, height: 224, objFrame);

// Render transparent diagnostics from the exact VRAM state left by the final accepted NMI.
// Physical scanline 32 is the first visible gameplay line. BG scroll registers continue
// counting behind the HUD, so desktop row zero of each 192-line diagnostic samples VOFS+32.
ushort gameplayBg1Y = unchecked((ushort)(runtime.BackgroundScroll.Bg1VerticalScroll + SnesGameplayFrameRenderer.HudHeight));
ushort gameplayBg2Y = unchecked((ushort)(runtime.ScrollingSky!.VerticalScroll + SnesGameplayFrameRenderer.HudHeight));
ushort[] skyHorizontalScrolls = runtime.ScrollingSky.BuildGameplayHorizontalScrolls(camera.YPosition);

Rgba32[] liveBg1 = SnesBgTilemapRenderer.Render4BppViewport(
    runtime.Vram,
    runtime.Cgram,
    tilemapBaseWord: 0x5000,
    characterBaseWord: 0,
    runtime.BackgroundScroll.Bg1HorizontalScroll,
    gameplayBg1Y,
    width: 256,
    height: 192);

// Do not make a developer infer whether a transparent PNG means valid empty space or a
// broken producer. BG1's two horizontal $400-word screens occupy $5000-$57FF; counting
// their populated words separately from the renderer's opaque pixels localizes failures to
// either streaming/map state or character/palette sampling without inventing any fix.
int populatedBg1MapWords = 0;
for (int word = 0x5000; word < 0x5800; word++)
{
    if (runtime.Vram.ReadWord((ushort)word) != 0)
        populatedBg1MapWords++;
}
int opaqueBg1Pixels = 0;
foreach (Rgba32 pixel in liveBg1)
{
    if (pixel.A != 0)
        opaqueBg1Pixels++;
}
Console.WriteLine(
    $"Live BG1 diagnostics: {populatedBg1MapWords:N0}/2,048 populated tilemap words; " +
    $"{opaqueBg1Pixels:N0}/49,152 opaque viewport pixels.");

// Decode the map address for the first physical gameplay pixel exactly as the desktop PPU
// renderer does. Printing the selected entry, character byte address, and nonzero plane
// bytes makes a wrong VRAM unit/base visible in one debugger run.
int diagnosticWorldX = runtime.BackgroundScroll.Bg1HorizontalScroll & 0x01ff;
int diagnosticWorldY = gameplayBg1Y & 0x00ff;
int diagnosticTileX = diagnosticWorldX >> 3;
int diagnosticTileY = diagnosticWorldY >> 3;
int diagnosticScreenColumn = diagnosticTileX >> 5;
ushort diagnosticMapWord = (ushort)(
    0x5000 +
    diagnosticScreenColumn * 0x0400 +
    (diagnosticTileY & 31) * 32 +
    (diagnosticTileX & 31));
ushort diagnosticEntry = runtime.Vram.ReadWord(diagnosticMapWord);
int diagnosticCharacterByte = (diagnosticEntry & 0x03ff) * 32;
int diagnosticCharacterNonzeroBytes = 0;
for (int index = 0; index < 32; index++)
{
    if (runtime.Vram.ReadByte(diagnosticCharacterByte + index) != 0)
        diagnosticCharacterNonzeroBytes++;
}
Console.WriteLine(
    $"First BG1 sample: scroll=({diagnosticWorldX},{diagnosticWorldY}), " +
    $"map=${diagnosticMapWord:X4}, entry=${diagnosticEntry:X4}, " +
    $"character byte=${diagnosticCharacterByte:X4} " +
    $"({diagnosticCharacterNonzeroBytes}/32 nonzero plane bytes).");

// The grounded stimulus supplies an especially useful second probe: Samus's bottom is
// resting on a collision-authored floor, so the corresponding visual tile should not be
// guessed from an arbitrary corner of the viewport.
if (groundedPlacement is DebugGroundedSamusPlacement grounded)
{
    int floorTileX = (grounded.XPosition & 0x01ff) >> 3;
    int floorWorldY = grounded.BlockY * 16;
    int floorTileY = (floorWorldY & 0x00ff) >> 3;
    int floorScreenColumn = floorTileX >> 5;
    ushort floorMapWord = (ushort)(
        0x5000 +
        floorScreenColumn * 0x0400 +
        (floorTileY & 31) * 32 +
        (floorTileX & 31));
    ushort floorEntry = runtime.Vram.ReadWord(floorMapWord);
    int floorCharacterByte = (floorEntry & 0x03ff) * 32;
    int floorCharacterNonzeroBytes = 0;
    for (int index = 0; index < 32; index++)
    {
        if (runtime.Vram.ReadByte(floorCharacterByte + index) != 0)
            floorCharacterNonzeroBytes++;
    }
    Console.WriteLine(
        $"Grounded-floor BG1 sample: world=({grounded.XPosition},{floorWorldY}), " +
        $"map=${floorMapWord:X4}, entry=${floorEntry:X4}, " +
        $"character byte=${floorCharacterByte:X4} " +
        $"({floorCharacterNonzeroBytes}/32 nonzero plane bytes).");
}
string bg1OutputPath = Path.Combine(
    Path.GetDirectoryName(Path.GetFullPath(options.OutputPath))!,
    Path.GetFileNameWithoutExtension(options.OutputPath) + ".background1.png");
PngWriter.WriteRgba(bg1OutputPath, 256, 192, liveBg1);
Console.WriteLine($"Wrote transparent live BG1 layer to {Path.GetFullPath(bg1OutputPath)}.");

Rgba32[] liveBg2 = SnesBgTilemapRenderer.Render4BppViewport(
    runtime.Vram,
    runtime.Cgram,
    tilemapBaseWord: 0x4800,
    characterBaseWord: 0,
    horizontalScroll: 0,
    verticalScroll: gameplayBg2Y,
    width: 256,
    height: 192,
    tilemapWidthInTiles: 32,
    tilemapHeightInTiles: 64,
    horizontalScrollByLine: skyHorizontalScrolls);
string bg2OutputPath = Path.Combine(
    Path.GetDirectoryName(Path.GetFullPath(options.OutputPath))!,
    Path.GetFileNameWithoutExtension(options.OutputPath) + ".background2.png");
PngWriter.WriteRgba(bg2OutputPath, 256, 192, liveBg2);
Console.WriteLine($"Wrote live bank-$88 scrolling-sky BG2 layer to {Path.GetFullPath(bg2OutputPath)}.");

Rgba32[] gameplayFrame = SnesGameplayFrameRenderer.RenderHudLiveBackgroundsAndObjs(
    runtime.Vram,
    runtime.Cgram,
    runtime.DisplayedOam,
    runtime.BackgroundScroll.Bg1HorizontalScroll,
    runtime.BackgroundScroll.Bg1VerticalScroll,
    runtime.ScrollingSky.VerticalScroll,
    skyHorizontalScrolls);
SnesGameplayFrameRenderer.ApplyPowerBombColorMath(
    gameplayFrame,
    bus,
    runtime.BombProjectiles.PowerBombExplosion,
    camera.XPosition,
    camera.YPosition);
// X-ray's bank-$91 boundary rays and bank-$88 outside-window half color math run after
// ordinary layer/OBJ composition, just like the two physical WH2/WH3 HDMA channels.
SnesGameplayFrameRenderer.ApplyXrayWindowColorMath(
    gameplayFrame,
    bus,
    runtime.Samus!.Xray,
    runtime.Samus,
    camera.XPosition,
    camera.YPosition);
PngWriter.WriteRgba(options.OutputPath, width: 256, height: 224, gameplayFrame);
Console.WriteLine($"Wrote ROM-backed HUD/OBJ frame to {Path.GetFullPath(options.OutputPath)}.");
Console.WriteLine($"Wrote transparent OBJ layer to {Path.GetFullPath(objectOutputPath)}.");
return 0;
}
catch (Exception exception)
{
    // Keep a bad ROM path, failed assertion, or unfinished translation in the debugger's
    // console. An explicit failing exit code preserves automation semantics without allowing
    // the CLR/Windows error reporter to display a modal "unknown software exception" box.
    Console.Error.WriteLine(exception);
    return 1;
}

[MethodImpl(MethodImplOptions.NoInlining)]
static void FrameBreakpoint(SuperMetroidRuntime runtime, RuntimeFrameResult result)
{
    // Intentionally empty. Both parameters remain debugger-visible and give a breakpoint a
    // stable name that will survive as the main runtime grows around it.
    _ = runtime;
    _ = result;
}

/// <summary>Command-line choices kept explicit so invalid debug sessions fail helpfully.</summary>
readonly record struct DebugRunnerOptions(
    string RomPath,
    int FrameCount,
    TimerScenario TimerScenario,
    string OutputPath,
    bool GroundedRun,
    int RightFrameCount,
    bool ReversalScript,
    bool MoonwalkScript,
    bool RanIntoWallScript,
    bool RunScript,
    bool SpeedBoosterScript,
    bool ShinesparkScript,
    bool SpaceJumpScript,
    bool WaterSpaceJumpScript,
    bool ScrewAttackScript,
    bool JumpScript,
    bool LandingImpactScript,
    bool PostureScript,
    bool AimScript,
    bool AimRunScript,
    bool AimAirScript,
    bool GunExtendedScript,
    bool ChargeBeamScript,
    bool HyperBeamScript,
    bool MissileScript,
    bool SuperMissileScript,
    bool AerialTurnScript,
    bool CompactAirScript,
    bool AimCrouchScript,
    bool AimTurnScript,
    bool CrouchTurnScript,
    bool CrouchJumpScript,
    bool MorphBallScript,
    bool SpringBallScript,
    bool BombJumpScript,
    bool PowerBombScript,
    bool KnockbackScript,
    bool MorphKnockbackScript,
    bool GrappleScript,
    bool GrappleFireScript,
    bool CrystalFlashScript,
    bool XrayScript,
    bool VisorScript,
    bool DeathScript,
    bool MotherBrainRainbowScript,
    bool DrainedSamusScript,
    bool DraygonGrabScript,
    bool ExtraDisplacementScript,
    bool ElevatorScript,
    bool ForwardFacingScript,
    bool GunshipScript,
    ushort BeamType)
{
    public static DebugRunnerOptions Parse(string[] arguments)
    {
        string? romPath = null;
        int frameCount = 240;
        TimerScenario timerScenario = TimerScenario.Ceres;
        string? outputPath = null;
        bool groundedRun = false;
        int rightFrameCount = int.MaxValue;
        bool reversalScript = false;
        bool moonwalkScript = false;
        bool ranIntoWallScript = false;
        bool runScript = false;
        bool speedBoosterScript = false;
        bool shinesparkScript = false;
        bool spaceJumpScript = false;
        bool waterSpaceJumpScript = false;
        bool screwAttackScript = false;
        bool jumpScript = false;
        bool landingImpactScript = false;
        bool postureScript = false;
        bool aimScript = false;
        bool aimRunScript = false;
        bool aimAirScript = false;
        bool gunExtendedScript = false;
        bool chargeBeamScript = false;
        bool hyperBeamScript = false;
        bool missileScript = false;
        bool superMissileScript = false;
        bool aerialTurnScript = false;
        bool compactAirScript = false;
        bool aimCrouchScript = false;
        bool aimTurnScript = false;
        bool crouchTurnScript = false;
        bool crouchJumpScript = false;
        bool morphBallScript = false;
        bool springBallScript = false;
        bool bombJumpScript = false;
        bool powerBombScript = false;
        bool knockbackScript = false;
        bool morphKnockbackScript = false;
        bool grappleScript = false;
        bool grappleFireScript = false;
        bool crystalFlashScript = false;
        bool xrayScript = false;
        bool visorScript = false;
        bool deathScript = false;
        bool motherBrainRainbowScript = false;
        bool drainedSamusScript = false;
        bool draygonGrabScript = false;
        bool extraDisplacementScript = false;
        bool elevatorScript = false;
        bool forwardFacingScript = false;
        bool gunshipScript = false;
        ushort beamType = 0;

        for (int index = 0; index < arguments.Length; index++)
        {
            string argument = arguments[index];
            switch (argument)
            {
                case "--frames":
                    frameCount = ParsePositiveInt(ReadValue(arguments, ref index, argument), argument);
                    break;

                case "--timer":
                    string scenario = ReadValue(arguments, ref index, argument);
                    timerScenario = scenario.ToLowerInvariant() switch
                    {
                        "ceres" => TimerScenario.Ceres,
                        "mother-brain" => TimerScenario.MotherBrain,
                        _ => throw new ArgumentException("--timer must be 'ceres' or 'mother-brain'."),
                    };
                    break;

                case "--output":
                    outputPath = ReadValue(arguments, ref index, argument);
                    break;

                case "--beam-type":
                    // The equipment table has entries zero through eleven. Values twelve
                    // through fifteen represent the forbidden Spazer+Plasma combinations
                    // and would index beyond retail data, so fail at the CLI boundary.
                    string beamTypeText = ReadValue(arguments, ref index, argument);
                    if (!ushort.TryParse(beamTypeText, out beamType) || beamType >= 12)
                    {
                        throw new ArgumentException(
                            $"--beam-type requires an integer from 0 through 11, not '{beamTypeText}'.");
                    }
                    break;

                case "--grounded-run":
                    groundedRun = true;
                    break;

                case "--right-frames":
                    rightFrameCount = ParsePositiveInt(
                        ReadValue(arguments, ref index, argument),
                        argument);
                    break;

                case "--reversal-script":
                    reversalScript = true;
                    groundedRun = true;
                    break;

                case "--moonwalk-script":
                    moonwalkScript = true;
                    groundedRun = true;
                    break;

                case "--ran-into-wall-script":
                    ranIntoWallScript = true;
                    groundedRun = true;
                    break;

                case "--run-script":
                    runScript = true;
                    groundedRun = true;
                    break;

                case "--speed-booster-script":
                    speedBoosterScript = true;
                    groundedRun = true;
                    break;

                case "--shinespark-script":
                    shinesparkScript = true;
                    groundedRun = true;
                    break;

                case "--space-jump-script":
                    spaceJumpScript = true;
                    groundedRun = true;
                    break;

                case "--water-space-jump-script":
                    waterSpaceJumpScript = true;
                    groundedRun = true;
                    break;

                case "--screw-attack-script":
                    screwAttackScript = true;
                    groundedRun = true;
                    break;

                case "--jump-script":
                    jumpScript = true;
                    groundedRun = true;
                    break;

                case "--landing-impact-script":
                    landingImpactScript = true;
                    groundedRun = true;
                    break;

                case "--posture-script":
                    postureScript = true;
                    groundedRun = true;
                    break;

                case "--aim-script":
                    aimScript = true;
                    groundedRun = true;
                    break;

                case "--aim-run-script":
                    aimRunScript = true;
                    groundedRun = true;
                    break;

                case "--aim-air-script":
                    aimAirScript = true;
                    groundedRun = true;
                    break;

                case "--gun-extended-script":
                    gunExtendedScript = true;
                    groundedRun = true;
                    break;

                case "--charge-beam-script":
                    chargeBeamScript = true;
                    groundedRun = true;
                    break;

                case "--hyper-beam-script":
                    hyperBeamScript = true;
                    groundedRun = true;
                    break;

                case "--missile-script":
                    missileScript = true;
                    groundedRun = true;
                    break;

                case "--super-missile-script":
                    superMissileScript = true;
                    groundedRun = true;
                    break;

                case "--aerial-turn-script":
                    aerialTurnScript = true;
                    groundedRun = true;
                    break;

                case "--compact-air-script":
                    compactAirScript = true;
                    groundedRun = true;
                    break;

                case "--aim-crouch-script":
                    aimCrouchScript = true;
                    groundedRun = true;
                    break;

                case "--aim-turn-script":
                    aimTurnScript = true;
                    groundedRun = true;
                    break;

                case "--crouch-turn-script":
                    crouchTurnScript = true;
                    groundedRun = true;
                    break;

                case "--crouch-jump-script":
                    crouchJumpScript = true;
                    groundedRun = true;
                    break;

                case "--morph-ball-script":
                    morphBallScript = true;
                    groundedRun = true;
                    break;

                case "--spring-ball-script":
                    springBallScript = true;
                    groundedRun = true;
                    break;

                case "--bomb-jump-script":
                    bombJumpScript = true;
                    groundedRun = true;
                    break;

                case "--power-bomb-script":
                    powerBombScript = true;
                    groundedRun = true;
                    break;

                case "--knockback-script":
                    knockbackScript = true;
                    groundedRun = true;
                    break;

                case "--morph-knockback-script":
                    morphKnockbackScript = true;
                    groundedRun = true;
                    break;

                case "--grapple-script":
                    grappleScript = true;
                    groundedRun = true;
                    break;

                case "--grapple-fire-script":
                    grappleFireScript = true;
                    groundedRun = true;
                    break;

                case "--crystal-flash-script":
                    crystalFlashScript = true;
                    groundedRun = true;
                    break;

                case "--xray-script":
                    xrayScript = true;
                    groundedRun = true;
                    break;

                case "--visor-script":
                    visorScript = true;
                    groundedRun = true;
                    break;

                case "--death-script":
                    deathScript = true;
                    groundedRun = true;
                    break;

                case "--mother-brain-rainbow-script":
                    motherBrainRainbowScript = true;
                    break;

                case "--drained-samus-script":
                    drainedSamusScript = true;
                    groundedRun = true;
                    break;

                case "--draygon-grab-script":
                    draygonGrabScript = true;
                    groundedRun = true;
                    break;

                case "--extra-displacement-script":
                    extraDisplacementScript = true;
                    groundedRun = true;
                    break;

                case "--elevator-script":
                    elevatorScript = true;
                    groundedRun = true;
                    break;

                case "--forward-facing-script":
                    forwardFacingScript = true;
                    break;

                case "--gunship-script":
                    gunshipScript = true;
                    groundedRun = true;
                    break;

                default:
                    if (argument.StartsWith('-'))
                        throw new ArgumentException($"Unknown option '{argument}'.");
                    if (romPath is not null)
                        throw new ArgumentException("Specify exactly one ROM path.");
                    romPath = argument;
                    break;
            }
        }

        if (romPath is null)
        {
            throw new ArgumentException(
                "ROM path is required. Example: dotnet run --project src/SuperMetroid.DebugRunner -- " +
                "../Super Metroid.smc --frames 240 --timer ceres");
        }

        outputPath ??= Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(romPath))!,
            "standalone-assets",
            "runtime",
            deathScript ? "DeathFrame.png" :
            xrayScript ? "XrayFrame.png" :
            visorScript ? "VisorFrame.png" :
            hyperBeamScript ? "HyperBeamFrame.png" :
            missileScript ? "MissileFrame.png" :
            superMissileScript ? "SuperMissileFrame.png" :
            powerBombScript ? "PowerBombFrame.png" : "EscapeTimerFrame.png");

        // The frame runtime reads compressed room data, graphics, palette, door metadata,
        // and library-background tilemaps directly from the ROM. It deliberately has no
        // extracted-assets argument; PNG/raw folders are outputs and diagnostics, not an
        // alternative source of gameplay truth.
        return new DebugRunnerOptions(
            romPath,
            frameCount,
            timerScenario,
            outputPath,
            groundedRun,
            rightFrameCount,
            reversalScript,
            moonwalkScript,
            ranIntoWallScript,
            runScript,
            speedBoosterScript,
            shinesparkScript,
            spaceJumpScript,
            waterSpaceJumpScript,
            screwAttackScript,
            jumpScript,
            landingImpactScript,
            postureScript,
            aimScript,
            aimRunScript,
            aimAirScript,
            gunExtendedScript,
            chargeBeamScript,
            hyperBeamScript,
            missileScript,
            superMissileScript,
            aerialTurnScript,
            compactAirScript,
            aimCrouchScript,
            aimTurnScript,
            crouchTurnScript,
            crouchJumpScript,
            morphBallScript,
            springBallScript,
            bombJumpScript,
            powerBombScript,
            knockbackScript,
            morphKnockbackScript,
            grappleScript,
            grappleFireScript,
            crystalFlashScript,
            xrayScript,
            visorScript,
            deathScript,
            motherBrainRainbowScript,
            drainedSamusScript,
            draygonGrabScript,
            extraDisplacementScript,
            elevatorScript,
            forwardFacingScript,
            gunshipScript,
            beamType);
    }

    private static string ReadValue(string[] arguments, ref int index, string option)
    {
        if (++index >= arguments.Length)
            throw new ArgumentException($"{option} requires a value.");
        return arguments[index];
    }

    private static int ParsePositiveInt(string value, string option)
    {
        if (!int.TryParse(value, out int parsed) || parsed <= 0)
            throw new ArgumentException($"{option} requires a positive integer, not '{value}'.");
        return parsed;
    }
}

enum TimerScenario
{
    Ceres,
    MotherBrain,
}

/// <summary>
/// Makes the command-line debugger genuinely non-interactive on Windows. The CLR retains
/// its ordinary stderr stack trace and exit code; only OS-owned modal error boxes are barred.
/// </summary>
static partial class NativeConsoleProcess
{
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
