using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileDrawEntriesVerification;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SyntheticSamusCartridgeVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Audio;

/// <summary>Constructed cartridge checks for ball movement, bombs, blast reactions, and tight passages.</summary>
internal static class SamusMorphBallVerification
{

/// <summary>Checks morph entry, bomb spread and jump timing, reaction PLMs, bounce, and tunnel collision.</summary>
internal static void VerifySamusMorphBallMovement()
{
    var bus = new TestAddressSpace();

    // Pose definitions `$91:B711-$91:B819/$91:B831`. Only fields consumed by the current
    // slice are populated, but the complete eight-byte records make direction, movement,
    // fallback, graphics offset, and collision radius independently observable.
    void WritePose(SamusPoseId pose, byte[] definition) =>
        WritePoseDefinition(bus, SamusPoseNumbers.Number(pose), definition);
    WritePose(SamusPoseId.CrouchingRightPose, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusPoseId.CrouchingLeftPose, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusPoseId.MorphBallGroundRightPose, [0x08, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.MorphBallMovingRightPose, [0x08, 0x04, 0x1d, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.MorphBallMovingLeftPose, [0x04, 0x04, 0x41, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.MorphBallFallingRightPose, [0x08, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.MorphBallFallingLeftPose, [0x04, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.MorphingTransitionRightPose, [0x08, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.MorphingTransitionLeftPose, [0x04, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.UnmorphingTransitionRightPose, [0x08, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusPoseId.UnmorphingTransitionLeftPose, [0x04, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusPoseId.MorphBallGroundLeftPose, [0x04, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.SpringBallGroundRightPose, [0x08, 0x11, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.SpringBallGroundLeftPose, [0x04, 0x11, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.SpringBallMovingRightPose, [0x08, 0x11, 0x79, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.SpringBallMovingLeftPose, [0x04, 0x11, 0x7a, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.SpringBallFallingRightPose, [0x08, 0x13, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.SpringBallFallingLeftPose, [0x04, 0x13, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.SpringBallJumpRightPose, [0x08, 0x12, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.SpringBallJumpLeftPose, [0x04, 0x12, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusPoseId.NormalJumpForwardRightPose, [0x08, 0x02, 0x4d, 0x02, 0x00, 0x00, 0x13, 0x00]);
    WritePose(SamusPoseId.NormalJumpForwardLeftPose, [0x04, 0x02, 0x4e, 0x07, 0x00, 0x00, 0x13, 0x00]);
    WritePose(SamusPoseId.FallingAimUpRightPose, [0x08, 0x06, 0x29, 0x00, 0x00, 0x00, 0x13, 0x00]);
    WritePose(SamusPoseId.FallingAimUpLeftPose, [0x04, 0x06, 0x2a, 0x05, 0x00, 0x00, 0x13, 0x00]);
    WritePose(SamusPoseId.FallingAimDownLeftPose, [0x04, 0x06, 0x2a, 0x05, 0x08, 0x00, 0x0a, 0x00]);
    WritePose(SamusPoseId.SpinJumpLeftPose, [0x04, 0x03, 0xff, 0xff, 0x00, 0x00, 0x13, 0x00]);

    // Pose metadata remains explicit fixture data. Animation pointers, frame
    // delays, and F9/FD operands are compiled cartridge mechanics: the retail
    // $37/$38 morph transitions spend three ticks in each of two visible frames.

    // Horizontal motion uses compiled native rows; only pose metadata is synthetic.
    SyntheticSamusCartridge.WriteDryAirJumpSpeed(bus, whole: 0x0004, subspeed: 0xe000);
    SyntheticSamusCartridge.WriteDryAirGravity(bus, whole: 0x0000, subspeed: 0x2800);
    // Pinned NTSC fractional bounce speed.
    SyntheticSamusCartridge.WriteBallBounceSpeed(bus, whole: 0x0001, subspeed: 0x0000);

    // Bomb jumps do not use the normal movement-type speed table. `$90:8EF4` passes
    // the standalone `$90:9F25` record directly to `$90:9A7E`, while `$90:9A2C`
    // reads the dry-air bomb-jump magnitude from `$90:9EF5/$90:9EFB`.
    SyntheticSamusCartridge.WriteBombJumpSpeedEntry(bus, [
        0x00, 0x00, 0x00, 0x30, // native diagonal acceleration 0.3000
        0x03, 0x00, 0x00, 0x00, // native diagonal maximum 3.0000
        0x00, 0x00, 0x00, 0x08, // native post-apex deceleration 0.0800
    ]);
    SyntheticSamusCartridge.WriteDryAirBombJumpSpeed(bus, whole: 0x0002, subspeed: 0xc000);

    const int width = 8;
    const int height = 8;
    var floorBlocks = new RoomTerrainBlock[width * height];
    for (int x = 0; x < width; x++)
        floorBlocks[4 * width + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData floor = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(floorBlocks, new RoomBlockBehavior[floorBlocks.Length], new RoomBlockAppearance[floorBlocks.Length]), RoomLevelData.NothingBeyondGrid(), []);

    var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.CrouchingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    SamusPresentation.InitializeAnimation(samus);

    SamusPosturePoseTransitions.ApplyMorphTransition(samus, 
floor, SamusPoseId.MorphingTransitionRightPose, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(SamusPoseId.CrouchingRightPose, samus.Pose, "rejected morph retains crouch");
    AssertEqual(48, samus.Position.Y.Whole, "rejected morph retains center Y");

    samus.EquippedItems = SamusEquipmentFlags.MorphBall;
    SamusPosturePoseTransitions.ApplyMorphTransition(samus, 
floor, SamusPoseId.MorphingTransitionRightPose, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(7, samus.Kinematics.Radius.Y, "morph transition radius from ROM");
    AssertEqual(57, samus.Position.Y.Whole, "command seven moves center down nine");

    // The same `$91:F7CE` initializer is selected by aerial input tables. `$2E -> $38`
    // is the controller route used to aim at Parlor's floor hatch: its radius-ten body
    // still receives the table's nine-pixel request, retains falling momentum mode, and clears the distinct
    // bomb-spread timeout without touching beam charge. A spin source additionally forces
    // acceleration mode two exactly as the initializer's previous-type-three branch does.
    var fallingMorph = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.FallingAimDownLeftPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 40)),
        BombSpreadCharge = UpdateCountdownAccess.AtAge(SuperMetroid.Core.Samus.Projectiles.MorphBallBombDefinitions.DownChargeTimeoutUpdates, 52),
    }.WithBeamChargeCount(0x003c);
    SamusPoseMetadata.RefreshCollisionRadii(fallingMorph);
    fallingMorph.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
    SamusPosturePoseTransitions.ApplyMorphTransition(fallingMorph, 
floor, SamusPoseId.MorphingTransitionLeftPose, frameCounter: 1, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(49, fallingMorph.Position.Y.Whole,
        "compact falling morph uses command-seven table displacement, not radius difference");
    var clippedMorph = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.FallingAimDownLeftPose, EquippedItems = SamusEquipmentFlags.MorphBall,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 54)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(clippedMorph);
    SamusPosturePoseTransitions.ApplyMorphTransition(clippedMorph, floor,
        SamusPoseId.MorphingTransitionLeftPose, 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(57, clippedMorph.Position.Y.Whole,
        "command-seven nine-pixel request clips to three pixels at the floor");
    AssertEqual(SamusAccelerationMode.Turning, fallingMorph.HorizontalSpeed.AccelerationMode,
        "non-spin aerial morph retains acceleration mode");
    AssertEqual(0, fallingMorph.BombSpreadCharge.Age,
        "morph initializer clears bomb-spread timeout");
    AssertEqual(0x003c, fallingMorph.BeamCharge.UpdatesHeld,
        "morph initializer does not clear arm-cannon charge counter");

    var spinMorph = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.SpinJumpLeftPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 40)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(spinMorph);
    SamusPosturePoseTransitions.ApplyMorphTransition(spinMorph, 
floor, SamusPoseId.MorphingTransitionLeftPose, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(SamusAccelerationMode.Decelerating, spinMorph.HorizontalSpeed.AccelerationMode,
        "spin-source morph forces native aerial deceleration mode two");

    // The compiled retail transition is 03,03,F9: six animation ticks
    // reach the frame-two branch while retaining its native F9 operands.
    for (int tick = 0; tick < 6; tick++)
    {
        samus.LiquidPhysics.BeginFrameSoundRequests();
        SamusPresentation.Animate(samus, new SamusAnimationStep { Random = new SharedRandom() });
    }
    AssertEqual(SamusPoseId.MorphBallGroundRightPose, (samus.CuePose.Named ?? throw new InvalidOperationException("Verification requires samus.CuePose.Named.")),
        "F9 selects no-spring grounded endpoint");
    AssertTrue(SamusPoseTransitions.ApplyPendingAnimationTransition(samus), "grounded F9 endpoint applies");
    AssertEqual(SamusPoseId.MorphBallGroundRightPose, samus.Pose, "morph entry reaches stable ball");

    // Changing to the moving record preserves the shared rolling frame/timer. Movement
    // type four uses native 0.C000 acceleration and remains floor-constrained.
    ushort timerBeforeRoll = samus.AnimationFrameTimer.RawValue;
    SamusPosturePoseTransitions.ApplyMorphBallPoseChange(samus, SamusPoseId.MorphBallMovingRightPose);
    AssertEqual(timerBeforeRoll, samus.AnimationFrameTimer.RawValue, "ball direction change preserves timer");
    MorphBallMovementResult rolling = SamusMorphBallMovement.StepGrounded(
floor, samus, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertTrue(rolling.Vertical.Collided, "rolling ball retains floor contact");
    AssertEqual(0xc000, samus.HorizontalSpeed.BaseSpeed.Fraction, "type-four acceleration uses native definition");

    SamusPosturePoseTransitions.ApplyMorphBallPoseChange(samus, SamusPoseId.MorphBallMovingLeftPose);
    AssertEqual(SamusAccelerationMode.Turning, samus.HorizontalSpeed.AccelerationMode, "ball reversal selects momentum mode one");
    AssertEqual(0xc000, samus.HorizontalSpeed.BaseSpeed.Fraction, "ball reversal preserves base magnitude");

    // Remove the floor and execute the stable grounded handler. Its failed +1 probe drives
    // the explicit `$1D/$41 -> $31/$32` walk-off transition and starts downward gravity.
    RoomLevelData empty = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new RoomTerrainBlock[width * height], new RoomBlockBehavior[width * height], new RoomBlockAppearance[width * height]), RoomLevelData.NothingBeyondGrid(), []);
    SamusPosturePoseTransitions.ApplyMorphBallPoseChange(samus, SamusPoseId.MorphBallGroundLeftPose);
    samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
    MorphBallMovementResult unsupported = SamusMorphBallMovement.StepGrounded(
empty, samus, frameCounter: 1, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertTrue(!unsupported.Vertical.Collided, "grounded ball detects missing floor");
    SamusPosturePoseTransitions.ApplyMorphBallWalkOff(samus);
    AssertEqual(SamusPoseId.MorphBallFallingLeftPose, samus.Pose, "left ball walk-off endpoint");
    AssertEqual(SamusVerticalDirection.Down, samus.Kinematics.VerticalVelocity.Direction, "ball walk-off starts falling");

    // None of the four native ball wrappers has a separate Speed-Booster branch. Their
    // horizontal helpers all end at `$90:E4E6`, which adds the inherited `$0B42.$0B44`
    // extra-run component to the wrapper's selected base magnitude. These fixtures retain
    // a deliberately fractional inherited component so a future integer-only shortcut
    // cannot accidentally pass. The individual wrappers still own their native teardown:
    // falling clears momentum only when no direction is held and mode is zero; a stable
    // grounded ball clears it after the no-speed vertical path; the moving and airborne
    // routes below retain it.
    var boostedRoll = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallMovingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(32, 32)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(boostedRoll);
    boostedRoll.HorizontalSpeed.HasRunningMomentum = true;
    boostedRoll.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(1, 0x8000);
    SamusMorphBallMovement.StepGrounded(empty, boostedRoll, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(34, boostedRoll.Position.X.Whole,
        "grounded moving ball adds inherited 1.8000 extra speed to native 0.C000 base");
    AssertEqual(0x4000, boostedRoll.Kinematics.Position.X.Fraction,
        "grounded moving ball preserves exact fractional carry from total speed");
    AssertEqual(1, boostedRoll.HorizontalSpeed.ExtraRunSpeed.Whole,
        "moving ball retains inherited extra-run whole word");
    AssertEqual(0x8000, boostedRoll.HorizontalSpeed.ExtraRunSpeed.Fraction,
        "moving ball retains inherited extra-run fractional word");

    var boostedFall = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallFallingLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(32, 32)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(boostedFall);
    boostedFall.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(1, 0x8000);
    boostedFall.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(boostedFall.Kinematics.VerticalVelocity.Speed, SamusVerticalDirection.Down);
    SamusMorphBallMovement.StepFalling(

        empty,
        boostedFall,
        controllerInput: GameplayAction.Left,
        frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(29, boostedFall.Position.X.Whole,
        "held-left falling ball subtracts inherited 1.8000 extra speed");
    AssertEqual(0xc000, boostedFall.Kinematics.Position.X.Fraction,
        "falling-left subtraction retains fractional position");
    AssertEqual(1, boostedFall.HorizontalSpeed.ExtraRunSpeed.Whole,
        "direction-held falling ball retains extra-run momentum");

    var opposedDirectionFall = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallFallingLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(32, 32)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(opposedDirectionFall);
    opposedDirectionFall.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(1, 0x8000);
    opposedDirectionFall.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(opposedDirectionFall.Kinematics.VerticalVelocity.Speed, SamusVerticalDirection.Down);
    SamusMorphBallMovement.StepFalling(

        empty,
        opposedDirectionFall,
        controllerInput: (GameplayAction.Left | GameplayAction.Right),
        frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(boostedFall.Kinematics.Position.X, opposedDirectionFall.Kinematics.Position.X,
        "Left+Right falling ball follows its pose direction instead of cancelling motion");
    AssertEqual(boostedFall.HorizontalSpeed.BaseSpeed.RawValue,
        opposedDirectionFall.HorizontalSpeed.BaseSpeed.RawValue,
        "Left+Right falling ball retains the same native acceleration as Left-only");

    var boostedSpring = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.SpringBallJumpRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(32, 32)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(boostedSpring);
    boostedSpring.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(1, 0x8000);
    boostedSpring.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(boostedSpring.Kinematics.VerticalVelocity.Speed, SamusVerticalDirection.Down);
    SamusMorphBallMovement.StepSpringBallInAir(

        empty,
        boostedSpring,
        controllerInput: GameplayAction.Right,
        frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(34, boostedSpring.Position.X.Whole,
        "Spring-Ball air handler adds inherited extra-run speed through shared jump movement");
    AssertEqual(0x4000, boostedSpring.Kinematics.Position.X.Fraction,
        "Spring-Ball air handler retains inherited fractional displacement");

    var boostedTransition = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphingTransitionRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(32, 32)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(boostedTransition);
    boostedTransition.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(1, 0x8000);
    SamusMorphBallMovement.StepTransition(

        empty,
        boostedTransition,
        frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(33, boostedTransition.Position.X.Whole,
        "morph transition moves by inherited extra-run speed with zero base");
    AssertEqual(0x8000, boostedTransition.Kinematics.Position.X.Fraction,
        "morph transition retains inherited fractional displacement");

    // `$90:DFB5-$DFEB` is a complete movement-type dispatcher, not a morphed-only
    // convenience. Standing/crouching share the frozen-time gate and otherwise select the
    // same `$51/$52` forward-jump bodies as running/falling/moonwalk/wall/grapple. Starting
    // from literal crouch also exercises `$91:FDAE`'s radius expansion and `$91:FC7D`'s
    // additional ten-pixel stable-crouch adjustment before command three arms the jump.
    var frozenCrouchBombJump = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.CrouchingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(frozenCrouchBombJump);
    SamusPresentation.InitializeAnimation(frozenCrouchBombJump);
    SamusPoseMetadata.PublishBombJumpDirection(frozenCrouchBombJump, SamusBombJumpDirection.Straight);
    AssertTrue(!SamusPoseMetadata.TrySetupPublishedBombJump(frozenCrouchBombJump, 

            floor,
            timeIsFrozen: true,
            frameCounter: 0, controllerNewInput: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled),
        "frozen standing/crouching setup rejects bomb jump");
    AssertEqual(null, frozenCrouchBombJump.BombJump,
        "frozen setup clears complete published direction word");
    AssertEqual(SamusPoseId.CrouchingRightPose, frozenCrouchBombJump.Pose,
        "frozen setup retains crouching pose");

    var crouchBombJump = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.CrouchingLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(crouchBombJump);
    SamusPresentation.InitializeAnimation(crouchBombJump);
    SamusPoseMetadata.PublishBombJumpDirection(crouchBombJump, SamusBombJumpDirection.Left);
    AssertTrue(SamusPoseMetadata.TrySetupPublishedBombJump(crouchBombJump, 

            floor,
            timeIsFrozen: false,
            frameCounter: 1, controllerNewInput: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled),
        "unfrozen crouch follows humanoid bomb-jump setup");
    AssertEqual(SamusPoseId.NormalJumpForwardLeftPose, crouchBombJump.Pose,
        "literal left pose direction selects bomb-jump body $52");
    AssertEqual(19, crouchBombJump.Kinematics.Radius.Y,
        "crouch bomb jump installs forward-jump radius from ROM");
    AssertEqual(45, crouchBombJump.Position.Y.Whole,
        "crouch bomb jump aligns feet without the ordinary jump-transition lift");
    AssertEqual(SamusBombJump.FromBlast(SamusBombJumpDirection.Left).Arm(), crouchBombJump.BombJump,
        "humanoid setup executes command three after pose initialization");

    var airborneBombRejection = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.NormalJumpForwardRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 32)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(airborneBombRejection);
    SamusPresentation.InitializeAnimation(airborneBombRejection);
    SamusPoseMetadata.PublishBombJumpDirection(airborneBombRejection, SamusBombJumpDirection.Right);
    AssertTrue(!SamusPoseMetadata.TrySetupPublishedBombJump(airborneBombRejection, 

            empty,
            timeIsFrozen: false,
            frameCounter: 0, controllerNewInput: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled),
        "normal-jump movement type uses native carry-clear setup entry");
    AssertEqual(null, airborneBombRejection.BombJump,
        "carry-clear bomb-jump family consumes and clears published direction");
    AssertTrue(airborneBombRejection.BombJumpPhase != SamusBombJumpPhase.Starting,
        "carry-clear bomb-jump family does not install special movement");

    // Put the airborne body three pixels above the floor and give it a hard downward
    // magnitude. The collision launches bounce one with the two constants at `$90:9EB5`.
    samus.Position = samus.Position.WithWhole(new(48, 55));
    samus.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(new UnsignedSixteenSixteen(3, 0), SamusVerticalDirection.Down);
    samus.Kinematics.YAcceleration = default;
    MorphBallMovementResult hardLanding = SamusMorphBallMovement.StepFalling(
floor, samus, controllerInput: 0, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertTrue(hardLanding.Landed, "hard ball fall collides with floor");
    samus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(2, 0x8000);
    samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
    SamusPosturePoseTransitions.ApplyMorphBallLanding(samus);
    AssertEqual(0x00028000u, samus.HorizontalSpeed.BaseSpeed.RawValue, "first rebound retains horizontal momentum");
    AssertEqual(SamusBallBounce.MorphBallFirst, samus.MorphBallBounceState, "first rebound state");
    AssertEqual(1, samus.Kinematics.VerticalVelocity.Speed.Whole, "first rebound whole speed from ROM");
    AssertEqual(0, samus.Kinematics.VerticalVelocity.Speed.Fraction, "first rebound subspeed matches pinned NTSC ROM");
    AssertEqual(SamusVerticalDirection.Up, samus.Kinematics.VerticalVelocity.Direction, "first rebound moves upward");

    // Isolate the two later collision handlers at the exact floor boundary. State one
    // launches the smaller second rebound; state two finally installs stable ground art.
    samus.Position = samus.Position.WithWholeY(57);
    samus.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(new UnsignedSixteenSixteen(1, 0), SamusVerticalDirection.Down);
    SamusPosturePoseTransitions.ApplyMorphBallLanding(samus);
    AssertEqual(0x00028000u, samus.HorizontalSpeed.BaseSpeed.RawValue, "second rebound retains horizontal momentum");
    AssertEqual(SamusBallBounce.MorphBallSecond, samus.MorphBallBounceState, "second rebound state");
    AssertEqual(0, samus.Kinematics.VerticalVelocity.Speed.Whole, "second rebound decrements whole constant");

    samus.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(samus.Kinematics.VerticalVelocity.Speed.WithWhole(1), SamusVerticalDirection.Down);
    SamusPosturePoseTransitions.ApplyMorphBallLanding(samus);
    AssertEqual(0u, samus.HorizontalSpeed.BaseSpeed.RawValue, "final ball landing clears both base-speed words");
    AssertEqual(SamusAccelerationMode.Accelerating, samus.HorizontalSpeed.AccelerationMode, "final ball landing clears acceleration mode");
    AssertEqual(SamusPoseId.MorphBallGroundLeftPose, samus.Pose, "bounce recovery uses facing-left ground pose");
    AssertEqual((SamusBallBounce?)null, samus.MorphBallBounceState, "grounding clears bounce state");

    // Unmorphing against only the floor succeeds and moves center up nine, preserving the
    // target body's bottom boundary. The live radius changes in the next alpha pass.
    // A ceiling in row two makes both initial expansion probes collide;
    // radius-seven `$91:FFA7` must retain the ball instead of selecting crouch.
    SamusPosturePoseTransitions.ApplyMorphTransition(samus, 
floor, SamusPoseId.UnmorphingTransitionLeftPose, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(7, samus.Kinematics.Radius.Y, "unmorph retains old live radius through beta");
    AssertEqual(48, samus.Position.Y.Whole, "unmorph expansion keeps bottom boundary");
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    AssertEqual(16, samus.Kinematics.Radius.Y, "following alpha publishes unmorph radius");

    var tunnelBlocks = (RoomTerrainBlock[])floorBlocks.Clone();
    for (int x = 0; x < width; x++)
        tunnelBlocks[2 * width + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData tunnel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(tunnelBlocks, new RoomBlockBehavior[tunnelBlocks.Length], new RoomBlockAppearance[tunnelBlocks.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var boxedBall = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 57)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(boxedBall);
    SamusPresentation.InitializeAnimation(boxedBall);
    SamusPosturePoseTransitions.ApplyMorphTransition(boxedBall, 
tunnel, SamusPoseId.UnmorphingTransitionRightPose, frameCounter: 1, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(SamusPoseId.MorphBallGroundRightPose, boxedBall.Pose, "boxed unmorph retains ball pose");
    AssertEqual(7, boxedBall.Kinematics.Radius.Y, "boxed unmorph retains ball radius");
    AssertEqual(57, boxedBall.Position.Y.Whole, "boxed unmorph retains center");

    // `$3D/$3E` do not become inert after unmorph entry. Their input tables can select a
    // type-six pose before `$FD` supplies the ordinary crouch endpoint. Exercise both
    // facings against an empty room and retain distinctive 16.16 speed words so this test
    // detects an accidental call to a fresh-jump or grounded initializer.
    foreach ((SamusPoseId sourcePose, SamusPoseId targetPose) in new[]
    {
        new SamusMorphBallVerifySamusMorphBallMovementCase(SamusPoseId.UnmorphingTransitionRightPose, SamusPoseId.FallingAimUpRightPose),
        new SamusMorphBallVerifySamusMorphBallMovementCase(SamusPoseId.UnmorphingTransitionLeftPose, SamusPoseId.FallingAimUpLeftPose),
    })
    {
        var fallingUnmorph = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = sourcePose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(fallingUnmorph);
        SamusPresentation.InitializeAnimation(fallingUnmorph);
        fallingUnmorph.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(2, 0x3456);
        fallingUnmorph.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(new UnsignedSixteenSixteen(2, 0x8000), SamusVerticalDirection.Down);

        SamusPosturePoseTransitions.ApplyUnmorphToFallingTransition(fallingUnmorph, 
empty, targetPose, frameCounter: 3, roomObjects: SamusRoomObjectFixtures.Uninstalled);
        AssertEqual(targetPose, fallingUnmorph.Pose,
            $"unmorph ${(int)SamusPoseNumbers.Number(sourcePose):X2} installs falling target");
        AssertEqual(19, fallingUnmorph.Kinematics.Radius.Y,
            $"unmorph ${(int)SamusPoseNumbers.Number(sourcePose):X2} reads falling radius from ROM");
        AssertEqual(2, fallingUnmorph.HorizontalSpeed.BaseSpeed.Whole,
            $"unmorph ${(int)SamusPoseNumbers.Number(sourcePose):X2} preserves horizontal whole speed");
        AssertEqual(0x3456, fallingUnmorph.HorizontalSpeed.BaseSpeed.Fraction,
            $"unmorph ${(int)SamusPoseNumbers.Number(sourcePose):X2} preserves horizontal subspeed");
        AssertEqual(SamusVerticalDirection.Down, fallingUnmorph.Kinematics.VerticalVelocity.Direction,
            $"unmorph ${(int)SamusPoseNumbers.Number(sourcePose):X2} preserves falling direction");
        AssertEqual(2, fallingUnmorph.Kinematics.VerticalVelocity.Speed.Whole,
            $"unmorph ${(int)SamusPoseNumbers.Number(sourcePose):X2} preserves falling whole speed");
        AssertEqual(0x8000, fallingUnmorph.Kinematics.VerticalVelocity.Speed.Fraction,
            $"unmorph ${(int)SamusPoseNumbers.Number(sourcePose):X2} preserves falling subspeed");
    }

    // Re-run entry with a nonzero vertical word to prove F9 uses its airborne operand, not
    // current collision radius or pose name. Spring Ball remains unequipped, selecting $31.
    var airborneEntry = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.CrouchingRightPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(airborneEntry);
    SamusPresentation.InitializeAnimation(airborneEntry);
    SamusPosturePoseTransitions.ApplyMorphTransition(airborneEntry, 
floor, SamusPoseId.MorphingTransitionRightPose, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    airborneEntry.Kinematics.VerticalVelocity = airborneEntry.Kinematics.VerticalVelocity.WithSpeed(airborneEntry.Kinematics.VerticalVelocity.Speed.WithFraction(1));
    for (int tick = 0; tick < 6; tick++)
    {
        airborneEntry.LiquidPhysics.BeginFrameSoundRequests();
        SamusPresentation.Animate(airborneEntry, new SamusAnimationStep { Random = new SharedRandom() });
    }
    AssertEqual(SamusPoseId.MorphBallFallingRightPose, (airborneEntry.CuePose.Named ?? throw new InvalidOperationException("Verification requires airborneEntry.CuePose.Named.")),
        "F9 nonzero Y subspeed selects airborne endpoint");

    // Repeat entry with Spring Ball bit `$0002`. F9 must use its equipped grounded operand,
    // then movement type `$11` reads its own speed record rather than ordinary type four.
    var spring = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.CrouchingRightPose,
        EquippedItems = (SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall),
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(spring);
    SamusPresentation.InitializeAnimation(spring);
    SamusPosturePoseTransitions.ApplyMorphTransition(spring, 
floor, SamusPoseId.MorphingTransitionRightPose, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    for (int tick = 0; tick < 6; tick++)
    {
        spring.LiquidPhysics.BeginFrameSoundRequests();
        SamusPresentation.Animate(spring, new SamusAnimationStep { Random = new SharedRandom() });
    }
    AssertEqual(SamusPoseId.SpringBallGroundRightPose, (spring.CuePose.Named ?? throw new InvalidOperationException("Verification requires spring.CuePose.Named.")),
        "F9 equipped endpoint selects Spring Ball ground");
    AssertTrue(SamusPoseTransitions.ApplyPendingAnimationTransition(spring), "Spring Ball F9 applies");
    SamusPosturePoseTransitions.ApplyMorphBallPoseChange(spring, SamusPoseId.SpringBallMovingRightPose);
    MorphBallMovementResult springRoll = SamusMorphBallMovement.StepGrounded(
floor, spring, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertTrue(springRoll.Vertical.Collided, "Spring Ball roll retains floor contact");
    AssertEqual(0xc000, spring.HorizontalSpeed.BaseSpeed.Fraction,
        "type-$11 acceleration uses its ROM record");

    // `$79 -> $7F` initializes the literal dry-air 4.E000 jump. Releasing Jump on its
    // first movement frame invokes the shared variable-height cutoff before displacement.
    SamusPosturePoseTransitions.ApplyMorphBallPoseChange(spring, SamusPoseId.SpringBallGroundRightPose);
    SamusPosturePoseTransitions.ApplySpringBallJump(spring, SamusPoseId.SpringBallJumpRightPose);
    AssertEqual(4, spring.Kinematics.VerticalVelocity.Speed.Whole, "Spring Ball launch whole speed");
    AssertEqual(0xe000, spring.Kinematics.VerticalVelocity.Speed.Fraction, "Spring Ball launch subspeed");
    SamusMorphBallMovement.StepSpringBallInAir(
empty, spring, controllerInput: 0, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(SamusVerticalDirection.Down, spring.Kinematics.VerticalVelocity.Direction, "released Spring Ball jump cuts upward arc");

    // The no-Jump hard impact stores the distinctive `$0601` state. A held-Jump impact
    // instead clears that state and immediately relaunches through Make_Samus_Jump.
    SamusPosturePoseTransitions.ApplyMorphBallPoseChange(spring, SamusPoseId.SpringBallFallingRightPose);
    spring.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(new UnsignedSixteenSixteen(3, 0), SamusVerticalDirection.Down);
    SamusPosturePoseTransitions.ApplySpringBallLanding(spring, controllerInput: 0);
    AssertEqual(SamusBallBounce.SpringBallFirst, spring.MorphBallBounceState, "Spring Ball first bounce state");
    var springLandingAnimation = new AnimationResult(spring.AnimationNumber, spring.AnimationFrameTimer.RawValue);
    SamusPosturePoseTransitions.ApplySpringBallLanding(spring, GameplayAction.Jump);
    AssertEqual((SamusBallBounce?)null, spring.MorphBallBounceState, "held-Jump relaunch clears bounce state");
    AssertEqual(SamusPoseId.SpringBallFallingRightPose, spring.Pose, "held-Jump landing retains its existing airborne pose");
    AssertEqual(springLandingAnimation, new AnimationResult(spring.AnimationNumber, spring.AnimationFrameTimer.RawValue),
        "held-Jump landing does not restart the rolling animation");

    // F1FC/F25E branch on the sign of the wrapping word subtraction speed-3.
    // Include negative Moonfall speed and the subtraction's signed-wrap boundary,
    // not just an ordinary positive landing where unsigned comparison looks equivalent.
    foreach ((ushort impactSpeed, bool rebounds) in new SamusMorphBallVerifySamusMorphBallMovementCase2[]
        { new SamusMorphBallVerifySamusMorphBallMovementCase2(2, false), new SamusMorphBallVerifySamusMorphBallMovementCase2(3, true), new SamusMorphBallVerifySamusMorphBallMovementCase2(0xffff, false), new SamusMorphBallVerifySamusMorphBallMovementCase2(0x8002, true), new SamusMorphBallVerifySamusMorphBallMovementCase2(0x8003, false) })
    foreach (bool springEquipped in new[] { false, true })
    {
        var impact = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = springEquipped ? SamusPoseId.SpringBallFallingRightPose : SamusPoseId.MorphBallFallingRightPose,
            EquippedItems = (SamusEquipmentFlags.MorphBall | (springEquipped ? SamusEquipmentFlags.SpringBall : 0)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(impact);
        SamusPresentation.InitializeAnimation(impact);
        impact.Kinematics.VerticalVelocity = impact.Kinematics.VerticalVelocity.WithSpeed(impact.Kinematics.VerticalVelocity.Speed.WithWhole(impactSpeed));
        impact.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(1, 0x8000);
        impact.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
        if (springEquipped)
            SamusPosturePoseTransitions.ApplySpringBallLanding(impact, controllerInput: 0);
        else
            SamusPosturePoseTransitions.ApplyMorphBallLanding(impact);
        AssertEqual(rebounds
            ? springEquipped ? SamusBallBounce.SpringBallFirst : SamusBallBounce.MorphBallFirst
            : (SamusBallBounce?)null, impact.MorphBallBounceState,
            "signed threshold preserves the selected bounce family");
        AssertEqual(rebounds ? 0x00018000u : 0u, impact.HorizontalSpeed.BaseSpeed.RawValue,
            "Only settled ball landing clears both base-speed words");
        AssertEqual(rebounds ? SamusAccelerationMode.Decelerating : SamusAccelerationMode.Accelerating, impact.HorizontalSpeed.AccelerationMode,
            "Both ball families share settled-landing acceleration cleanup");
    }

    // Production follows the cartridge pointers these fixtures install; no test-facing
    // constructor injects damage, radii, durations, or animation endpoints into a slot.
    InstallMorphBombProjectileFixture(bus);
    InstallPowerBombFixedColorFixture(bus);
    InstallBombReactionObjectFixture(bus);

    var noBombItemSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 57)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(noBombItemSamus);
    var noBombItemSystem = CreateBombFixture();
    noBombItemSystem.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, noBombItemSamus, GameplayAction.Shoot, GameplayAction.Shoot);
    AssertTrue(noBombItemSystem.BombCounter == 0 && noBombItemSystem.Bombs.All(slot => !slot.IsActive),
        "bomb item bit gates placement");

    var bombProjectileSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 57)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(bombProjectileSamus);
    var bombs = CreateBombFixture();
    bombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, bombProjectileSamus, GameplayAction.Shoot, GameplayAction.Shoot);
    AssertTrue(bombs.Bombs[0].IsActive && bombs.Bombs.Skip(1).All(slot => !slot.IsActive),
        "first normal bomb uses physical slot zero");
    AssertEqual(1, bombs.BombCounter, "placement increments native bomb counter");
    AssertEqual(0x0010, bombs.CooldownTimer, "normal bomb loads cooldown table entry five");
    AssertEqual(59, bombs.Bombs[0].Fuse.UpdatesLeft, "placement frame immediately decrements timer 60 to 59");
    AssertEqual(0x001e, bombs.Bombs[0].HeldBomb().Damage, "bomb damage follows bank-$93 data pointer");
    AssertEqual(0xad45, bombs.Bombs[0].SpritemapPointer, "first instruction selects ROM bomb spritemap");
    ProjectileDrawArtwork bombArtwork = InstalledFixtures.ProjectileArtwork();
    AssertProjectileEntry(
        DrawnEntries(frame => bombs.Draw(frame, cameraPosition: new(0, 0), bombArtwork)),
        new SpriteSource(SpriteSourceKind.SamusBomb), ProjectileSheetFrames.ProjectileFrame(ProjectileSpriteNative.Projectile(bombs.Bombs[0].SpritemapPointer)), SamusBeamCombinations.Of(bombProjectileSamus.EquippedBeams),
        new Position16<ScreenPixels>(bombs.Bombs[0].Position.X.Whole, bombs.Bombs[0].Position.Y.Whole), "Bomb");
    AssertEqual(4, bombs.Bombs[0].Radius.X, "first instruction publishes ROM X radius");
    AssertEqual(4, bombs.Bombs[0].Radius.Y, "first instruction publishes ROM Y radius");

    // A new edge during the active low-byte cooldown is rejected without incrementing the
    // aggregate. Release is a separate frame so ControllerInputState-like edge semantics
    // are represented explicitly in this direct subsystem test.
    bombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, bombProjectileSamus, 0, 0);
    bombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, bombProjectileSamus, GameplayAction.Shoot, GameplayAction.Shoot);
    AssertTrue(bombs.Bombs.Skip(1).All(slot => !slot.IsActive),
        "active bomb cooldown rejects another edge");
    AssertEqual(1, bombs.BombCounter, "rejected edge does not alter bomb counter");

    // Run to timer nine, then prove bank-$A0's three X comparisons at timer eight using
    // three independent slots/lifecycles. Distances remain inside the strict radius sum.
    static MakeDirectionFixtureResult MakeDirectionFixture(
        TestAddressSpace fixtureBus,
        RoomLevelData fixtureFloor)
    {
        var fixtureSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            Position = FixedPosition<RoomPixels>.FromWhole(new(48, 57)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(fixtureSamus);
        var fixtureSystem = CreateBombFixture();
        fixtureSystem.StepFrame(new SamusTerrain { Level = fixtureFloor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, fixtureSamus, GameplayAction.Shoot, GameplayAction.Shoot);
        while (fixtureSystem.Bombs[0].Fuse.UpdatesLeft > 9)
            fixtureSystem.StepFrame(new SamusTerrain { Level = fixtureFloor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, fixtureSamus, 0, 0);
        return new MakeDirectionFixtureResult(fixtureSystem, fixtureSamus);
    }

    foreach ((ushort samusX, SamusBombJumpDirection expectedDirection) in new SamusMorphBallVerifySamusMorphBallMovementCase3[]
    {
        new SamusMorphBallVerifySamusMorphBallMovementCase3(47, SamusBombJumpDirection.Left),
        new SamusMorphBallVerifySamusMorphBallMovementCase3(48, SamusBombJumpDirection.Straight),
        new SamusMorphBallVerifySamusMorphBallMovementCase3(49, SamusBombJumpDirection.Right),
    })
    {
        (SamusBombProjectileSystem directionSystem, SamusState directionSamus) =
            MakeDirectionFixture(bus, floor);
        directionSamus.Position = directionSamus.Position.WithWholeX(samusX);
        directionSystem.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, directionSamus, 0, 0);
        AssertEqual(SamusBombJump.FromBlast(expectedDirection), directionSamus.BombJump,
            "bank-$A0 publishes the direction as a request pose handling has not accepted");
        AssertTrue(directionSamus.BombJumpPhase != SamusBombJumpPhase.Starting,
            "timer-eight overlap does not start movement in same frame");
    }

    // Continue the original straight fixture through timer zero. It must enter fast art at
    // fifteen, publish direction at eight, emit a five-block no-op reaction cross at zero,
    // animate all five explosion records, execute delete, and decrement BombCounter.
    while (bombs.Bombs[0].Fuse.UpdatesLeft > 15)
        bombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, bombProjectileSamus, 0, 0);
    AssertTrue(bombs.Bombs[0].InstructionPointer >= 0x9fe3,
        "timer fifteen advances the live instruction pointer into fast animation");
    while (bombs.Bombs[0].Fuse.UpdatesLeft > 8)
        bombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, bombProjectileSamus, 0, 0);
    AssertEqual(SamusBombJump.FromBlast(SamusBombJumpDirection.Straight), bombProjectileSamus.BombJump,
        "same-X timer-eight overlap publishes straight direction");
    AssertTrue(SamusPoseMetadata.TrySetupPublishedBombJump(bombProjectileSamus, 

            floor,
            timeIsFrozen: false,
            frameCounter: 0, controllerNewInput: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled),
        "following alpha consumes published morphed bomb jump");
    AssertEqual(SamusBombJump.FromBlast(SamusBombJumpDirection.Straight).Arm(), bombProjectileSamus.BombJump,
        "morphed setup accepts the request on the following frame");

    while (!bombs.Bombs[0].IsExploding)
    {
        // Each pass is an update of its own; the bombs' record keeps only its requests.
        bombs.AudioRequests.BeginRecordedUpdate();
        bombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, bombProjectileSamus, 0, 0);
    }
    AssertTrue(bombs.Bombs[0].IsExploding, "timer zero selects bomb explosion list");
    AssertEqual(SamusBombProjectileSystem.NormalBombType, bombs.Bombs[0].HeldBomb().Type, "the exploding bomb stays a bomb");
    AssertTrue(bombs.Bombs[0].HasReactedToTerrain, "first explosion pass marks block cross handled");
    AssertEqual(RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock), floor.GetCollisionBlockByIndex(4 * width + 3).Terrain,
        "bottom reaction leaves the fixture solid floor intact without inventing a PLM");
    AssertEqual(1, bombs.AudioRequests.SoundRequests.Count,
        "normal bomb fuse expiry queues exactly one sound request");
    AssertEqual(
        new SoundRequest(EnemySound.BombExplosion, SoundRequestPriority.Normal),
        bombs.AudioRequests.SoundRequests[0],
        "normal bomb fuse expiry requests the bomb explosion at normal priority");

    for (int tick = 0; tick < 20 && bombs.BombCounter != 0; tick++)
        bombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, bombProjectileSamus, 0, 0);
    AssertEqual(0, bombs.BombCounter, "explosion delete decrements bomb counter");
    AssertTrue(!bombs.Bombs[0].IsActive, "delete opcode clears complete bomb slot");

    // A charged beam carried into Morph Ball is not a sixth projectile family. `$90:C0AB`
    // waits while Shoot+Down remain held, then `$90:D849` fills every ordinary bomb slot
    // from the ROM table as soon as Down is released. The standard bomb interpreter runs
    // later in that same frame, so fuses and positions below include their first tick.
    var spreadSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        Position = FixedPosition<RoomPixels>.FromWhole(new(80, 80)),
    }.WithBeamChargeCount(BeamChargeDefinitions.FullyChargedCounter);
    SamusPoseMetadata.RefreshCollisionRadii(spreadSamus);
    var spreadBombs = CreateBombFixture();
    spreadBombs.StepFrame(new SamusTerrain { Level = empty, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, spreadSamus, (GameplayAction.Shoot | GameplayAction.Down), 0);
    AssertTrue(spreadBombs.Bombs.All(slot => !slot.IsActive),
        "held Down charges bomb spread without spawning");
    AssertEqual(1, spreadSamus.BombSpreadCharge.Age,
        "held Down advances native bomb-spread timeout");

    spreadBombs.AudioRequests.BeginRecordedUpdate();
    bool spreadReleasedCharge = spreadBombs.StepFrameConsumingCharge(new SamusTerrain { Level = empty, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, spreadSamus, GameplayAction.Shoot, 0);
    AssertTrue(spreadBombs.Bombs.All(slot => slot.IsActive && slot.IsBombSpread),
        "releasing Down while holding charged Shoot starts bomb spread");
    AssertTrue(spreadReleasedCharge,
        "bomb spread publishes charge teardown to shared beam owner");
    AssertEqual(1, spreadBombs.AudioRequests.SoundRequests.Count,
        "bomb spread queues one charge-cancellation sound request");
    AssertEqual(
        new SoundRequest(SamusSound.Silence, SoundRequestPriority.High),
        spreadBombs.AudioRequests.SoundRequests[0],
        "bomb spread queues cartridge library-one sound two with Max9");
    AssertEqual(5, spreadBombs.BombCounter,
        "bomb spread occupies all five physical bomb slots");
    AssertEqual(0x0010, spreadBombs.CooldownTimer,
        "bomb spread selects non-beam cooldown entry five");
    AssertEqual(0, spreadSamus.BeamCharge.UpdatesHeld,
        "bomb spread clears mirrored flare counter");
    AssertEqual(0, spreadSamus.BombSpreadCharge.Age,
        "bomb spread clears its timeout counter");

    // `$90:BF9D` gives selected HUD item three priority over `$90:C0AB`. A charge
    // carried through the morph transition therefore cannot start BombSpread while
    // Power Bombs remain selected: held Shoot without a new edge leaves both projectile
    // families untouched, exactly as the cartridge does. This is intentionally distinct
    // from selecting no HUD item and releasing Down in the spread test above.
    var selectedPowerBombSpreadSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        SelectedHudItem = HudItem.PowerBomb,
        PowerBombs = 2,
        Position = FixedPosition<RoomPixels>.FromWhole(new(80, 80)),
    }.WithBeamChargeCount(BeamChargeDefinitions.FullyChargedCounter);
    SamusPoseMetadata.RefreshCollisionRadii(selectedPowerBombSpreadSamus);
    var selectedPowerBombSpreadBombs = CreateBombFixture();
    selectedPowerBombSpreadBombs.StepFrame(new SamusTerrain { Level = empty, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, selectedPowerBombSpreadSamus, GameplayAction.Shoot, 0);
    AssertTrue(selectedPowerBombSpreadBombs.Bombs.All(slot => !slot.IsBombSpread),
        "selected Power Bomb bypasses charged regular-bomb spread");
    AssertTrue(selectedPowerBombSpreadBombs.Bombs.All(slot => !slot.IsActive),
        "held Shoot without a new edge does not place the selected Power Bomb");
    AssertEqual(0, selectedPowerBombSpreadBombs.BombCounter,
        "selected Power Bomb charge state leaves all five bomb slots unused");
    AssertEqual(2, selectedPowerBombSpreadSamus.PowerBombs,
        "selected Power Bomb charge state does not consume ammunition without a new edge");

    ushort[] expectedSpreadTimers = [119, 109, 99, 109, 119];
    ushort[] expectedSpreadX = [79, 79, 80, 80, 81];
    ushort[] expectedSpreadXSub = [0, 0x8000, 0, 0x8000, 0];
    ushort[] expectedSpreadY = [80, 79, 78, 79, 80];
    ushort[] expectedSpreadYSub = [0, 0, 0x8000, 0, 0];
    for (int index = 0; index < MorphBallBombDefinitions.PlaceCount; index++)
    {
        SamusBomb slot = spreadBombs.Bombs[index];
        AssertTrue(slot.IsBombSpread, $"spread slot {index} selects spread pre-instruction");
        AssertEqual(SamusBombProjectileSystem.NormalBombType, slot.HeldBomb().Type, $"spread slot {index} holds a bomb");
        AssertEqual(expectedSpreadTimers[index], slot.Fuse.UpdatesLeft,
            $"spread slot {index} consumes first ROM fuse tick");
        AssertEqual(expectedSpreadX[index], slot.Position.X.Whole,
            $"spread slot {index} applies encoded X velocity");
        AssertEqual(expectedSpreadXSub[index], slot.Position.X.Fraction,
            $"spread slot {index} preserves fractional X motion");
        AssertEqual(expectedSpreadY[index], slot.Position.Y.Whole,
            $"spread slot {index} applies encoded Y velocity");
        AssertEqual(expectedSpreadYSub[index], slot.Position.Y.Fraction,
            $"spread slot {index} preserves fractional Y motion");
    }

    // Force one already-launched spread bomb's next point step through the synthetic
    // floor. The real pre-instruction must undo penetration and restore its original
    // negative bounce velocity rather than freezing or reusing generic Samus collision.
    SamusBomb bouncingSpread = spreadBombs.Bombs[2];
    bouncingSpread.Position = bouncingSpread.Position.WithWholeX(48).WithFractionX(0).WithWholeY(63).WithFractionY(0);
    var bouncingFlight = new BombSpreadFlight(SignedSixteenSixteen.Zero, SignedSixteenSixteen.FromParts(-2, 0x8000))
    {
        VerticalVelocity = SignedSixteenSixteen.FromParts(1, 0),
    };
    bouncingSpread.SpreadFlight = bouncingFlight;
    spreadBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, spreadSamus, 0, 0);
    AssertEqual(63, bouncingSpread.Position.Y.Whole,
        "spread-bomb floor collision undoes penetrating Y movement");
    AssertEqual(0xfffe, bouncingFlight.VerticalVelocity.UnsignedWholePixels,
        "falling spread bomb restores launch velocity on floor bounce");
    AssertEqual(0x8000, bouncingFlight.VerticalVelocity.Fraction,
        "falling spread bomb restores ROM fractional launch speed");

    // The Down timeout is intentionally quantized by bits six/seven. `$BF -> $C0` waits
    // one final frame; the next held frame forces the same five-slot launch.
    var timeoutSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        Position = FixedPosition<RoomPixels>.FromWhole(new(80, 80)),
        BombSpreadCharge = UpdateCountdownAccess.AtAge(SuperMetroid.Core.Samus.Projectiles.MorphBallBombDefinitions.DownChargeTimeoutUpdates, 0x00bf),
    }.WithBeamChargeCount(BeamChargeDefinitions.FullyChargedCounter);
    SamusPoseMetadata.RefreshCollisionRadii(timeoutSamus);
    var timeoutBombs = CreateBombFixture();
    timeoutBombs.StepFrame(new SamusTerrain { Level = empty, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, timeoutSamus, (GameplayAction.Shoot | GameplayAction.Down), 0);
    AssertTrue(timeoutBombs.Bombs.All(slot => !slot.IsActive),
        "timeout `$BF` advances to `$C0` before forced launch");
    AssertEqual(0x00c0, timeoutSamus.BombSpreadCharge.Age,
        "last held charging frame reaches timeout threshold");
    timeoutBombs.StepFrame(new SamusTerrain { Level = empty, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, timeoutSamus, (GameplayAction.Shoot | GameplayAction.Down), 0);
    AssertTrue(timeoutBombs.Bombs.All(slot => slot.IsActive && slot.IsBombSpread),
        "timeout `$C0` forces launch even while Down remains held");

    // Releasing Shoot in ball form cancels an inherited flare without placing a normal
    // bomb. Runtime consumes this publication to clear actual beam-flare animation state,
    // restore the suit palette, and queue the same library-one cancellation sound.
    var cancelledSpreadSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
    }.WithBeamChargeCount(BeamChargeDefinitions.FullyChargedCounter);
    var cancelledSpreadBombs = CreateBombFixture();
    AssertTrue(cancelledSpreadBombs.StepFrameConsumingCharge(new SamusTerrain { Level = empty, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, cancelledSpreadSamus, 0, 0),
        "released Shoot publishes carried-charge cancellation");
    AssertTrue(cancelledSpreadBombs.Bombs.All(slot => !slot.IsActive),
        "charge cancellation does not place an ordinary bomb");

    // Selected HUD item three takes the power-bomb branch even without the normal Bomb
    // item bit. Placement consumes one round, locks `$0CEA`, initializes type `$0300` from
    // the literal bank-$93 record, and installs cooldown table entry three (`$28`).
    var powerBombSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        SelectedHudItem = HudItem.PowerBomb,
        PowerBombs = 2,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(powerBombSamus);
    var powerBombs = CreateBombFixture();
    powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, GameplayAction.Shoot, GameplayAction.Shoot);
    AssertTrue(powerBombs.Bombs[0].IsActive && powerBombs.Bombs.Skip(1).All(slot => !slot.IsActive),
        "selected power bomb uses first physical bomb slot");
    AssertEqual(1, powerBombSamus.PowerBombs,
        "power-bomb placement decrements ammo exactly once");
    AssertEqual(HudItem.PowerBomb, powerBombSamus.SelectedHudItem,
        "remaining power-bomb ammo retains HUD selection");
    AssertEqual(0x0028, powerBombs.CooldownTimer,
        "power bomb loads non-beam cooldown table entry three");
    AssertEqual(SamusBombProjectileSystem.PowerBombType, powerBombs.Bombs[0].HeldBomb().Type,
        "the placed projectile is a Power Bomb");
    AssertEqual(0x00c8, powerBombs.Bombs[0].HeldBomb().Damage,
        "power-bomb damage follows bank-$93 type-three data");
    AssertEqual(0xab97, powerBombs.Bombs[0].SpritemapPointer,
        "power-bomb placement selects first retail slow-list spritemap");
    AssertTrue(powerBombs.PowerBombExplosion.IsArmed,
        "placement sets negative native power-bomb flag");
    AssertTrue(!powerBombs.PowerBombExplosion.IsActive,
        "bank-$88 explosion waits for projectile fuse");

    // `$90:C157` shares the normal bomb's 60->15 timing and adds `$1C` to the live
    // instruction pointer. A second selected-item edge is rejected by the armed flag
    // before helper two can perturb either aggregate counter.
    powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0);
    powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, GameplayAction.Shoot, GameplayAction.Shoot);
    AssertTrue(powerBombs.Bombs.Skip(1).All(slot => !slot.IsActive),
        "negative power-bomb flag rejects a second placement");
    AssertEqual(1, powerBombs.BombCounter,
        "armed rejection preserves bomb aggregate");
    while (powerBombs.Bombs[0].Fuse.UpdatesLeft > 15)
        powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0);
    AssertTrue(powerBombs.Bombs[0].InstructionPointer >= 0x9fa3,
        "power-bomb timer fifteen enters retail fast list");
    OamBuffer armedPowerBombOam = SpriteFrameBuffer.DrawnOam(frame =>
        powerBombs.Draw(frame, cameraPosition: new(0, 0), InstalledFixtures.ProjectileArtwork()));
    AssertTrue(armedPowerBombOam.LastFinalizedSpriteCount > 0,
        "armed Power Bomb draws its fast-list spritemap before detonation");

    while (!powerBombs.PowerBombExplosion.IsActive)
    {
        powerBombs.AudioRequests.BeginRecordedUpdate();
        powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0);
    }
    AssertEqual(0, powerBombs.Bombs[0].Fuse.UpdatesLeft,
        "the fuse has run out once the explosion starts");
    AssertEqual(PowerBombExplosionPhase.PendingPreExplosionSetup,
        powerBombs.PowerBombExplosion.Blast.Phase,
        "fuse expiry allocates HDMA list without executing setup");
    AssertEqual(PowerBombRadius.Zero, powerBombs.PowerBombExplosion.Blast.PreExplosionRadius,
        "pre-explosion radius is not initialized during allocation");
    AssertTrue(powerBombs.PowerBombExplosion.IsActive,
        "normal explosion becomes active at its spawn");
    AssertEqual(
        new SoundRequest(SamusSound.PowerBombExplosion, SoundRequestPriority.Essential),
        powerBombs.AudioRequests.SoundRequests.Single(),
        "power-bomb spawn queues cartridge library-one explosion sound");
    OamBuffer detonatedPowerBombOam = SpriteFrameBuffer.DrawnOam(frame =>
        powerBombs.Draw(frame, cameraPosition: new(0, 0), InstalledFixtures.ProjectileArtwork()));
    AssertEqual(0, detonatedPowerBombOam.LastFinalizedSpriteCount,
        "detonation-frame zero variable suppresses the placed Power Bomb sprite");

    powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0);
    AssertEqual(PowerBombRadius.FromWholePixels(4), powerBombs.PowerBombExplosion.Blast.PreExplosionRadius,
        "first HDMA pass initializes the 4.00-pixel radius without expanding it");
    AssertEqual(PowerBombExplosionPhase.Inactive, powerBombs.PowerBombExplosion.Blast.RenderedPhase,
        "setup pass has not generated an explosion window");

    // The following HDMA pass runs the new pre-instruction. White pre-flash grows 4.00 by 48.00,
    // then the still-zero damaging radius scans the one-block rectangle's four duplicated
    // corners in bank-$94 top/left/bottom/right order.
    powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0);
    AssertEqual(PowerBombRadius.FromWholePixels(52), powerBombs.PowerBombExplosion.Blast.PreExplosionRadius,
        "first white pre-explosion frame applies $3000 speed");
    AssertEqual(PowerBombRadius.FromPixels(47, 128), powerBombs.PowerBombExplosion.Blast.RadiusSpeed,
        "white pre-explosion subtracts $0080 acceleration");

    int whiteFlashFrames = 1;
    while (powerBombs.PowerBombExplosion.Blast.Phase == PowerBombExplosionPhase.PreExplosionWhite)
    {
        powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0);
        whiteFlashFrames++;
        AssertTrue(whiteFlashFrames < 32, "white pre-explosion phase terminates");
    }
    AssertEqual(PowerBombExplosionPhase.PreExplosionYellow,
        powerBombs.PowerBombExplosion.Blast.Phase,
        "white threshold advances to yellow shape phase");
    AssertTrue(powerBombs.PowerBombExplosion.Blast.PreExplosionRadius >= PowerBombRadius.FromWholePixels(146),
        "white phase crosses its 146-pixel radius threshold");
    AssertEqual<int?>(null, powerBombs.PowerBombExplosion.Blast.RenderedOvalRadius,
        "yellow pre-explosion shows its first oval on its next update");

    StepFrames(4, _ => powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0));
    AssertEqual(PowerBombExplosionPhase.ExplosionYellow,
        powerBombs.PowerBombExplosion.Blast.Phase,
        "four 192-byte yellow shapes advance to damaging explosion");
    AssertEqual(PowerBombRadius.FromWholePixels(4), powerBombs.PowerBombExplosion.Blast.ExplosionRadius,
        "damaging yellow explosion restarts at 4.00 pixels");
    AssertEqual(PowerBombRadius.Zero, powerBombs.PowerBombExplosion.Blast.RadiusSpeed,
        "damaging yellow explosion restarts with zero speed");

    int yellowExplosionFrames = 0;
    while (powerBombs.PowerBombExplosion.Blast.Phase == PowerBombExplosionPhase.ExplosionYellow)
    {
        powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0);
        yellowExplosionFrames++;
        AssertTrue(yellowExplosionFrames < 128, "yellow explosion reaches $8600 threshold");
    }
    AssertEqual(PowerBombExplosionPhase.ExplosionWhite,
        powerBombs.PowerBombExplosion.Blast.Phase,
        "yellow radius threshold advances to white shape phase");
    AssertTrue(powerBombs.PowerBombExplosion.Blast.ExplosionRadius >= PowerBombRadius.FromWholePixels(134),
        "yellow explosion crosses its 134-pixel threshold");
    AssertEqual<int?>(null, powerBombs.PowerBombExplosion.Blast.RenderedOvalRadius,
        "white explosion shows its first oval on its next update");

    int whiteExplosionFrames = 0;
    while (powerBombs.PowerBombExplosion.Blast.Phase == PowerBombExplosionPhase.ExplosionWhite)
    {
        powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0);
        whiteExplosionFrames++;
        AssertTrue(whiteExplosionFrames < 32, "white explosion shape sequence terminates");
    }
    AssertEqual(17, whiteExplosionFrames,
        "$9246-$9F06 white phase contains seventeen 192-byte shapes");
    AssertEqual(PowerBombExplosionPhase.Afterglow,
        powerBombs.PowerBombExplosion.Blast.Phase,
        "white shapes advance to stage-five afterglow");

    // Moving one pixel guarantees cleanup cannot begin Crystal Flash. The 32-step byte
    // counter performs 31 consecutive fades, then `$88:8B4E` clears status/radii and
    // releases the flag; the same projectile pass sees flag zero and deletes the slot.
    powerBombSamus.Position += new Offset16<RoomPixels>(1, 0);
    int afterglowFrames = 0;
    while (powerBombs.PowerBombExplosion.IsActive)
    {
        powerBombs.StepFrame(new SamusTerrain { Level = floor, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, powerBombSamus, 0, 0);
        afterglowFrames++;
        AssertTrue(afterglowFrames < 160, "power-bomb afterglow reaches cleanup");
    }
    AssertEqual(32, afterglowFrames,
        "afterglow retains the underflowed timer high byte across its low-byte reload");
    AssertTrue(!powerBombs.Bombs[0].IsActive,
        "cleanup frame deletes released power-bomb projectile");
    AssertEqual(0, powerBombs.BombCounter,
        "power-bomb cleanup decrements shared bomb counter");
    AssertTrue(!powerBombs.PowerBombExplosion.IsArmed,
        "failed Crystal Flash cleanup releases power-bomb flag");
    AssertEqual(PowerBombRadius.Zero, powerBombs.PowerBombExplosion.Blast.ExplosionRadius,
        "cleanup clears damaging radius");

    // Put the explosion center on a type-$5 horizontal extension whose signed BTS $FF
    // redirects one column left to a BTS-zero type-$F parent. `$94:9CF4` visits center
    // before left, so the extension must spawn exactly one PLM and synchronously turn the
    // parent into temporary type-$8 terrain; the later left-arm visit sees that mutation.
    var reactionWords = new RoomTerrainBlock[width * height];
    var reactionBts = new RoomBlockBehavior[reactionWords.Length];
    const int reactionParentIndex = 3 * width + 2;
    const int reactionExtensionIndex = 3 * width + 3;
    reactionWords[reactionParentIndex] = RoomTerrainBlock.Of(801, BlockMirroring.None, RoomCollisionType.BombableBlock);
    reactionBts[reactionParentIndex] = RoomBlockBehaviorCodec.Decode(0);
    reactionWords[reactionExtensionIndex] = RoomTerrainBlock.Of(88, BlockMirroring.None, RoomCollisionType.HorizontalExtension);
    reactionBts[reactionExtensionIndex] = RoomBlockBehaviorCodec.Decode(0xff);
    RoomLevelData reactionLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(reactionWords, reactionBts, new RoomBlockAppearance[reactionWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var reactionObjects = RoomObjectFixtures.Unbound();
    var reactionBombs = CreateBombFixture();
    var reactionSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(reactionSamus);
    reactionBombs.StepFrame(new SamusTerrain { Level = reactionLevel, RoomObjects = reactionObjects }, reactionSamus, GameplayAction.Shoot, GameplayAction.Shoot);

    while (!reactionBombs.Bombs[0].IsExploding)
    {
        reactionBombs.StepFrame(new SamusTerrain { Level = reactionLevel, RoomObjects = reactionObjects }, reactionSamus, 0, 0);
    }

    AssertEqual(RoomCollisionType.HorizontalExtension,
        reactionLevel.GetCollisionBlockByIndex(reactionExtensionIndex).CollisionType,
        "bomb cross leaves the visited horizontal extension in place");
    AssertEqual(RoomBlockBehaviorCodec.Decode(0xff),
        reactionLevel.GetCollisionBlockByIndex(reactionExtensionIndex).Behavior,
        "bomb cross preserves the extension's signed redirect BTS");
    AssertEqual(RoomCollisionType.SolidBlock,
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).CollisionType,
        "later left-arm reaction observes the synchronously mutated parent");
    AssertTrue(reactionObjects.PopulationPlaces.Any(place => place.Block == reactionLevel.BlockAt(reactionParentIndex)),
        "extension redirect installs its reaction PLM on the parent block");
    AssertEqual(1, reactionObjects.Places.ActiveCount,
        "extension and later parent visit produce one native reaction PLM");
    AssertEqual(RoomTerrainBlock.Of(0x058, BlockMirroring.None, RoomCollisionType.SolidBlock),
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).Terrain,
        "CEDA keeps a type-F bomb block temporarily solid through movement beta");

    // PLM_Handler runs after movement beta in the same gameplay frame. Normal-bomb setup
    // began at `$CC3F`, so this first handler pass draws air `$0053` without queueing the
    // reaction head's sound $0A. The bomb explosion itself remains the sound owner.
    reactionObjects.Step(reactionLevel);
    AssertEqual(RoomTerrainBlock.Of(0x053, BlockMirroring.None, RoomCollisionType.Air),
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).Terrain,
        "same-frame PLM pass draws the first bomb-block air frame");
    AssertEqual(0, reactionObjects.Sounds.SoundRequests.Count,
        "normal bomb setup skips reaction PLM sound $0A");

    // Walk the exact forward, 384-frame blank hold, reverse, restore, and delayed-delete
    // timeline. The restored word is deliberately `$F058`, not the original `$F321`:
    // setup CEDA synthesized PLM_Vars by replacing all twelve low bits with `$058`.
    StepFrames(12, _ => reactionObjects.Step(reactionLevel));
    AssertEqual(new RoomTerrainBlock(RoomBlockAppearance.Nothing, RoomCollisionType.Air),
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).Terrain,
        "bomb reaction reaches blank air after three four-frame transitions");
    StepFrames(384 + 12, _ =>
        reactionObjects.Step(reactionLevel));
    AssertEqual(RoomTerrainBlock.Of(0x058, BlockMirroring.None, RoomCollisionType.BombableBlock),
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).Terrain,
        "bomb reaction restores synthesized type-F parent after native hold");
    AssertEqual(1, reactionObjects.Places.ActiveCount,
        "DrawPLMBlock retains reaction PLM through its timer-one restore pass");
    reactionObjects.Step(reactionLevel);
    AssertEqual(0, reactionObjects.Places.ActiveCount,
        "bomb reaction PLM deletes on the handler pass after restoration");

    // Exercise the public bomb pipeline as well as the setup methods below. A BTS-eight
    // type-$C parent remains unchanged during CF2E setup, then the same gameplay frame's
    // PLM pass reveals `$C057`. This catches an omitted/wrong collision-type dispatcher
    // branch even if the isolated bank-$84 setup tests continue to pass.
    var integratedRevealWords = new RoomTerrainBlock[width * height];
    var integratedRevealBts = new RoomBlockBehavior[integratedRevealWords.Length];
    const int integratedRevealIndex = 3 * width + 3;
    integratedRevealWords[integratedRevealIndex] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock);
    integratedRevealBts[integratedRevealIndex] = RoomBlockBehaviorCodec.Decode(8);
    RoomLevelData integratedRevealLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(integratedRevealWords, integratedRevealBts, new RoomBlockAppearance[integratedRevealWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var integratedRevealObjects = RoomObjectFixtures.Unbound();
    var integratedRevealBombs = CreateBombFixture();
    var integratedRevealSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(integratedRevealSamus);
    integratedRevealBombs.StepFrame(new SamusTerrain { Level = integratedRevealLevel, RoomObjects = integratedRevealObjects }, integratedRevealSamus, GameplayAction.Shoot, GameplayAction.Shoot);
    while (!integratedRevealBombs.Bombs[0].IsExploding)
    {
        integratedRevealBombs.StepFrame(new SamusTerrain { Level = integratedRevealLevel, RoomObjects = integratedRevealObjects }, integratedRevealSamus, 0, 0);
    }
    AssertEqual(RoomCollisionType.ShootableBlock,
        integratedRevealLevel.GetCollisionBlockByIndex(integratedRevealIndex).CollisionType,
        "bomb dispatcher visits center shootable-solid parent");
    AssertTrue(integratedRevealObjects.PopulationPlaces.Any(place => place.Block == integratedRevealLevel.BlockAt(integratedRevealIndex)),
        "bomb dispatcher installs its reveal PLM on the center parent");
    AssertEqual(1, integratedRevealObjects.Places.ActiveCount,
        "bomb dispatcher installs shootable reveal PLM");
    AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        integratedRevealLevel.GetCollisionBlockByIndex(integratedRevealIndex).Terrain,
        "CF2E leaves required-weapon parent unchanged through movement beta");
    integratedRevealObjects.Step(integratedRevealLevel);
    AssertEqual(RoomTerrainBlock.Of(0x057, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        integratedRevealLevel.GetCollisionBlockByIndex(integratedRevealIndex).Terrain,
        "same-frame PLM pass reveals required power-bomb block");

    // The player's recording that reached room `$8F:9B9D` detonated a normal bomb beside
    // a right-facing blue cap (type C/BTS $41). `$94:9EA6` sends that private BTS directly
    // to Setup_BlueDoor; it is not an out-of-range ordinary shot-block table index. Drive
    // the complete bomb fuse/cross path so this regression cannot pass through a direct
    // PLM helper while gameplay still throws before reaching it.
    var bombDoorWords = new RoomTerrainBlock[width * height];
    var bombDoorBts = new RoomBlockBehavior[bombDoorWords.Length];
    const int bombDoorIndex = 3 * width + 3;
    bombDoorWords[bombDoorIndex] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock);
    bombDoorBts[bombDoorIndex] = RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right));
    RoomLevelData bombDoorLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(bombDoorWords, bombDoorBts, new RoomBlockAppearance[bombDoorWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var bombDoorObjects = RoomObjectFixtures.Unbound();
    var bombDoorBombs = CreateBombFixture();
    var bombDoorSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(bombDoorSamus);
    bombDoorBombs.StepFrame(new SamusTerrain { Level = bombDoorLevel, RoomObjects = bombDoorObjects }, bombDoorSamus, GameplayAction.Shoot, GameplayAction.Shoot);
    while (!bombDoorBombs.Bombs[0].IsExploding)
    {
        bombDoorBombs.StepFrame(new SamusTerrain { Level = bombDoorLevel, RoomObjects = bombDoorObjects }, bombDoorSamus, 0, 0);
    }
    AssertEqual(1, bombDoorObjects.Places.ActiveCount,
        "normal bomb allocates one blue-door opening PLM");
    AssertEqual(RoomCollisionType.SolidBlock,
        bombDoorLevel.GetCollisionBlockByIndex(bombDoorIndex).CollisionType,
        "normal bomb synchronously runs blue-door setup");

    RoomLevelData powerBombDoorLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(bombDoorWords, bombDoorBts, new RoomBlockAppearance[bombDoorWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var powerBombDoorObjects = RoomObjectFixtures.Unbound();
    powerBombDoorObjects.DoorObjects.SpawnBlueDoorOpening(
            powerBombDoorLevel,
            powerBombDoorLevel.BlockAt(bombDoorIndex),
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right)),
            BlockStrike.By(SamusBombProjectileSystem.PowerBombType));
    AssertEqual(RoomCollisionType.ShootableBlock,
        powerBombDoorLevel.GetCollisionBlockByIndex(bombDoorIndex).CollisionType,
        "rejected power bomb leaves blue-door cap collision intact");

    // Shootable reaction setup `$84:CE6B` is easy to get subtly wrong because it does not
    // preserve the original low twelve bits. Prove that a type-C/BTS-zero parent becomes
    // temporary `$8052`, begins at the ROM's `$0053` air frame, queues sound `$0A` through
    // the max-one opcode, and ultimately restores synthesized `$C052` rather than `$C321`.
    var shotWords = new RoomTerrainBlock[width * height];
    const int shotIndex = 3 * width + 3;
    shotWords[shotIndex] = RoomTerrainBlock.Of(801, BlockMirroring.None, RoomCollisionType.ShootableBlock);
    RoomLevelData shotLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(shotWords, new RoomBlockBehavior[shotWords.Length], new RoomBlockAppearance[shotWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var shotObjects = RoomObjectFixtures.Unbound();
    AssertTrue(
        shotObjects.TrySpawnBombedShootableBlock(shotLevel, shotLevel.BlockAt(shotIndex), 0, SamusProjectileType.Of(SamusProjectileFamily.Bomb)),
        "normal bomb allocates respawning shot-block PLM");
    AssertEqual(RoomTerrainBlock.Of(0x052, BlockMirroring.None, RoomCollisionType.SolidBlock),
        shotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "CE6B installs synthesized temporary shot-block word");
    shotObjects.Step(shotLevel);
    AssertEqual(RoomTerrainBlock.Of(0x053, BlockMirroring.None, RoomCollisionType.Air),
        shotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "respawning shot block begins with retail air frame");
    AssertEqual(1, shotObjects.Sounds.SoundRequests.Count,
        "shot-block head queues one sound request");
    AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, 0x0a), shotObjects.Sounds.SoundRequests[0].SoundEffect,
        "shot-block head queues crumble sound $0A");
    AssertEqual(1, shotObjects.Sounds.SoundRequests[0].Priority.Requests,
        "$84:8C79 uses sound-library-two maximum one");
    StepFrames(12, _ => shotObjects.Step(shotLevel));
    AssertEqual(new RoomTerrainBlock(RoomBlockAppearance.Nothing, RoomCollisionType.Air),
        shotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "respawning shot block reaches its blank hold word");
    StepFrames(384 + 12, _ =>
        shotObjects.Step(shotLevel));
    AssertEqual(RoomTerrainBlock.Of(0x052, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        shotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "respawning shot block restores CE6B's synthesized parent");
    shotObjects.Step(shotLevel);
    AssertEqual(0, shotObjects.Places.ActiveCount,
        "respawning shot block deletes one handler pass after restoration");

    // `$84:D084` is the respawning power-bomb block. Its CF2E setup accepts family `$0300`,
    // synthesizes `$C057`, and executes `$CB94`; the latter begins with the direct `$8C7C`
    // max-one sound opcode rather than ordinary shot block `$8C79`. Following the entire
    // reverse animation proves RestoreLevelWord is the setup-produced `$C057`, never the
    // fixture's original visual block `$C321`.
    RoomLevelData powerBombShotLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(shotWords, new RoomBlockBehavior[shotWords.Length], new RoomBlockAppearance[shotWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var powerBombShotObjects = RoomObjectFixtures.Unbound();
    AssertTrue(
        powerBombShotObjects.TrySpawnProjectileShotBlock(
            powerBombShotLevel,
            powerBombShotLevel.BlockAt(shotIndex),
            behavior: 8,
            strike: BlockStrike.By(SamusProjectileType.Of(SamusProjectileFamily.PowerBomb)),
            solidBlock: true),
        "power bomb allocates BTS-eight respawning block PLM");
    AssertEqual(RoomTerrainBlock.Of(0x057, BlockMirroring.None, RoomCollisionType.SolidBlock),
        powerBombShotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "CF2E installs synthesized temporary power-bomb word");
    powerBombShotObjects.Step(powerBombShotLevel);
    AssertEqual(RoomTerrainBlock.Of(0x053, BlockMirroring.None, RoomCollisionType.Air),
        powerBombShotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "power-bomb block begins with retail air frame");
    AssertEqual(new SoundRequest(EnemySound.BlockCrumble, SoundRequestPriority.WhenIdle), powerBombShotObjects.Sounds.SoundRequests[0],
        "$8C7C queues power-bomb breakup sound with maximum one");
    StepFrames(12 + 384 + 12, _ =>
        powerBombShotObjects.Step(powerBombShotLevel));
    AssertEqual(RoomTerrainBlock.Of(0x057, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        powerBombShotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "respawning power-bomb block restores synthesized $C057 parent");
    powerBombShotObjects.Step(powerBombShotLevel);
    AssertEqual(0, powerBombShotObjects.Places.ActiveCount,
        "respawning power-bomb block deletes after restoration");

    // Issue #286 reached this same BTS-eight setup through the integrated expanding
    // power-bomb boundary scan. The old call site incorrectly routed every type-$4/$C
    // collision through the legacy normal-bomb-only helper, even though the shared
    // projectile setup above already models CF2E's accepted family `$0300`. Drive the
    // actual placement and fuse so a direct helper test cannot conceal that mismatch.
    var integratedPowerBombWords = new RoomTerrainBlock[width * height];
    var integratedPowerBombBts = new RoomBlockBehavior[integratedPowerBombWords.Length];
    integratedPowerBombWords[shotIndex] = RoomTerrainBlock.Of(801, BlockMirroring.None, RoomCollisionType.ShootableBlock);
    integratedPowerBombBts[shotIndex] = RoomBlockBehaviorCodec.Decode(8);
    RoomLevelData integratedPowerBombLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(integratedPowerBombWords, integratedPowerBombBts, new RoomBlockAppearance[integratedPowerBombWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var integratedPowerBombObjects = RoomObjectFixtures.Unbound();
    var integratedPowerBombs = CreateBombFixture();
    var integratedPowerBombSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        SelectedHudItem = HudItem.PowerBomb,
        PowerBombs = 1,
        MaxPowerBombs = 5,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(integratedPowerBombSamus);
    integratedPowerBombs.StepFrame(new SamusTerrain { Level = integratedPowerBombLevel, RoomObjects = integratedPowerBombObjects }, integratedPowerBombSamus, GameplayAction.Shoot, GameplayAction.Shoot);
    while (!integratedPowerBombs.PowerBombExplosion.IsActive)
    {
        integratedPowerBombs.StepFrame(new SamusTerrain { Level = integratedPowerBombLevel, RoomObjects = integratedPowerBombObjects }, integratedPowerBombSamus, 0, 0);
    }
    AssertEqual(RoomTerrainBlock.Of(0x321, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        integratedPowerBombLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "power-bomb fuse frame leaves gated block intact before radius scan");
    integratedPowerBombs.StepFrame(new SamusTerrain { Level = integratedPowerBombLevel, RoomObjects = integratedPowerBombObjects }, integratedPowerBombSamus, 0, 0);
    AssertTrue(integratedPowerBombObjects.PopulationPlaces.Any(place => place.Block == integratedPowerBombLevel.BlockAt(shotIndex)),
        "expanding power bomb visits BTS-eight shootable block");
    AssertEqual(RoomTerrainBlock.Of(0x057, BlockMirroring.None, RoomCollisionType.SolidBlock),
        integratedPowerBombLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "integrated CF2E setup installs temporary power-bomb block word");
    AssertEqual(1, integratedPowerBombObjects.Places.ActiveCount,
        "integrated power-bomb radius allocates the gated block PLM exactly once");
    integratedPowerBombObjects.Step(integratedPowerBombLevel);
    AssertEqual(RoomTerrainBlock.Of(0x053, BlockMirroring.None, RoomCollisionType.Air),
        integratedPowerBombLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "integrated power-bomb reaction draws the first cleared-air frame");

    // Issue #298 reached the deliberate seventeenth `$94:9EA6` table entry through the
    // same expanding-radius dispatcher. Keep this separate from the direct PLM test below:
    // the production boundary collector used to reject BTS $10 before it could call the
    // correctly translated `$84:B974` one-frame owner.
    var collisionProbeWords = new RoomTerrainBlock[width * height];
    var collisionProbeBts = new RoomBlockBehavior[collisionProbeWords.Length];
    collisionProbeWords[shotIndex] = RoomTerrainBlock.Of(801, BlockMirroring.None, RoomCollisionType.ShootableBlock);
    collisionProbeBts[shotIndex] = RoomBlockBehaviorCodec.Decode(0x10);
    RoomLevelData collisionProbeLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(collisionProbeWords, collisionProbeBts, new RoomBlockAppearance[collisionProbeWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var collisionProbeObjects = RoomObjectFixtures.Unbound();
    RunPowerBombToInitialBoundary(
            collisionProbeLevel,
            collisionProbeObjects,
            position: new(48, 48));
    AssertTrue(collisionProbeObjects.PopulationPlaces.Any(place => place.Block == collisionProbeLevel.BlockAt(shotIndex)),
        "expanding power bomb visits native BTS-ten collision-probe entry");
    AssertEqual(4, collisionProbeObjects.Places.ActiveCount,
        "zero-radius border visits its center four times and allocates four B974 PLMs");
    AssertEqual(RoomTerrainBlock.Of(0x321, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        collisionProbeLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "B974 boundary reaction leaves terrain unchanged");

    // `$84:D08C` is the corresponding Super Missile block. CF67 accepts family `$0200`,
    // synthesizes `$C09F`, and retains `$CB71`'s max-six sound plus the same 384-frame
    // blank hold. An ordinary missile must fail that setup without touching terrain or
    // leaving an apparently occupied host PLM slot.
    RoomLevelData superShotLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(shotWords, new RoomBlockBehavior[shotWords.Length], new RoomBlockAppearance[shotWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var superShotObjects = RoomObjectFixtures.Unbound();
    AssertTrue(!superShotObjects.TrySpawnProjectileShotBlock(
            superShotLevel,
            superShotLevel.BlockAt(shotIndex),
            behavior: 10,
            strike: BlockStrike.By(SamusProjectileType.Of(SamusProjectileFamily.Missile)),
            solidBlock: true),
        "ordinary missile is rejected by Super Missile block setup");
    AssertEqual(RoomTerrainBlock.Of(0x321, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        superShotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "rejected missile leaves Super Missile block word untouched");
    AssertEqual(0, superShotObjects.Places.ActiveCount,
        "rejected missile leaves no live PLM header");
    AssertTrue(superShotObjects.TrySpawnProjectileShotBlock(
            superShotLevel,
            superShotLevel.BlockAt(shotIndex),
            behavior: 10,
            strike: BlockStrike.By(SamusProjectileType.Of(SamusProjectileFamily.SuperMissile)),
            solidBlock: true),
        "Super Missile allocates BTS-A respawning block PLM");
    AssertEqual(RoomTerrainBlock.Of(0x09f, BlockMirroring.None, RoomCollisionType.SolidBlock),
        superShotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "CF67 installs synthesized temporary Super Missile word");
    superShotObjects.Step(superShotLevel);
    AssertEqual(new SoundRequest(EnemySound.BlockCrumble, SoundRequestPriority.Normal), superShotObjects.Sounds.SoundRequests[0],
        "$CB71 queues Super Missile breakup sound with maximum six");
    StepFrames(12 + 384 + 12, _ =>
        superShotObjects.Step(superShotLevel));
    AssertEqual(RoomTerrainBlock.Of(0x09f, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        superShotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "respawning Super Missile block restores synthesized $C09F parent");
    superShotObjects.Step(superShotLevel);
    AssertEqual(0, superShotObjects.Places.ActiveCount,
        "respawning Super Missile block deletes after restoration");

    // BTS nine and B select the two permanent lists. Run both beyond their final timer so
    // `$CC20`'s asymmetric 3/2/1 timing and `$CC0B`'s ordinary 4/4/4 timing must each parse,
    // publish blank `$00FF`, and delete without executing DrawPLMBlock.
    foreach ((byte behavior, ushort projectileType, string familyName) in new[]
    {
        new VerifySamusMorphBallMovementCase(9, 0x0300, "power-bomb"),
        new VerifySamusMorphBallMovementCase(11, 0x0200, "Super-Missile"),
    })
    {
        RoomLevelData permanentWeaponLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(shotWords, new RoomBlockBehavior[shotWords.Length], new RoomBlockAppearance[shotWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
        var permanentWeaponObjects = RoomObjectFixtures.Unbound();
        AssertTrue(permanentWeaponObjects.TrySpawnProjectileShotBlock(
                permanentWeaponLevel,
                permanentWeaponLevel.BlockAt(shotIndex),
                behavior,
                BlockStrike.By(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(projectileType)),
                solidBlock: true),
            $"{familyName} allocates permanent weapon-gated block PLM");
        for (int frame = 0; frame < 20; frame++)
        {
            permanentWeaponObjects.Step(permanentWeaponLevel);
        }
        AssertEqual(new RoomTerrainBlock(RoomBlockAppearance.Nothing, RoomCollisionType.Air),
            permanentWeaponLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
            $"permanent {familyName} block ends on retail blank word");
        AssertEqual(0, permanentWeaponObjects.Places.ActiveCount,
            $"permanent {familyName} block deletes without restoration");
    }

    // The final four nonnegative table entries all point to PLMEntries_nothing. Negative BTS
    // has a collision-nibble asymmetry instead: type C allocates an area-table no-op, while
    // type four returns before Spawn_PLM. These slots are visually invisible, but retaining
    // their one-handler lifetime prevents the finite 40-slot pool from behaving differently.
    RoomLevelData noOpShotLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(shotWords, new RoomBlockBehavior[shotWords.Length], new RoomBlockAppearance[shotWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var noOpShotObjects = RoomObjectFixtures.Unbound();
    AssertTrue(noOpShotObjects.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            noOpShotLevel.BlockAt(shotIndex),
            behavior: 12,
            BlockStrike.GrappleTouch.Instance,
            solidBlock: true),
        "BTS-C allocates retail no-op shot PLM");
    AssertTrue(noOpShotObjects.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            noOpShotLevel.BlockAt(shotIndex),
            behavior: 0x80,
            BlockStrike.GrappleTouch.Instance,
            solidBlock: true),
        "negative type-C BTS allocates area-table no-op shot PLM");
    AssertTrue(!noOpShotObjects.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            noOpShotLevel.BlockAt(shotIndex),
            behavior: 0x80,
            BlockStrike.GrappleTouch.Instance,
            solidBlock: false),
        "negative type-four BTS exits before shot PLM allocation");
    AssertTrue(noOpShotObjects.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            noOpShotLevel.BlockAt(shotIndex),
            behavior: 0x10,
            strike: BlockStrike.By(SamusProjectileType.Of(SamusProjectileFamily.PowerBomb)),
            solidBlock: true),
        "BTS-ten allocates the native B974 collision-probe PLM");
    AssertEqual(3, noOpShotObjects.Places.ActiveCount,
        "the three solid/no-op reactions occupy native slots");
    AssertEqual(RoomTerrainBlock.Of(0x321, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        noOpShotLevel.GetCollisionBlockByIndex(shotIndex).Terrain,
        "all no-op shot reactions preserve terrain");
    noOpShotObjects.Step(noOpShotLevel);
    AssertEqual(0, noOpShotObjects.Places.ActiveCount,
        "no-op shot PLMs delete on their first handler pass");

    // Normal bombs striking BTS 8 and A do not break their weapon-gated blocks. CF2E/CF67
    // redirect the PLM pointer to one-frame diagnostic reveals. The complete level word is
    // drawn by the normal bank-$84 draw parser and remains after the slot deletes.
    var revealWords = new RoomTerrainBlock[width * height];
    const int powerRevealIndex = 2 * width + 2;
    const int superRevealIndex = 2 * width + 4;
    const int areaNoOpIndex = 4 * width + 2;
    revealWords[powerRevealIndex] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableAir);
    revealWords[superRevealIndex] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock);
    revealWords[areaNoOpIndex] = RoomTerrainBlock.Of(546, BlockMirroring.None, RoomCollisionType.ShootableBlock);
    RoomLevelData revealLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(revealWords, new RoomBlockBehavior[revealWords.Length], new RoomBlockAppearance[revealWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var revealObjects = RoomObjectFixtures.Unbound();
    AssertTrue(
        revealObjects.TrySpawnBombedShootableBlock(revealLevel, revealLevel.BlockAt(powerRevealIndex), 8, SamusProjectileType.Of(SamusProjectileFamily.Bomb)),
        "normal bomb allocates power-bomb reveal PLM");
    AssertTrue(
        revealObjects.TrySpawnBombedShootableBlock(revealLevel, revealLevel.BlockAt(superRevealIndex), 10, SamusProjectileType.Of(SamusProjectileFamily.Bomb)),
        "normal bomb allocates super-missile reveal PLM");
    AssertTrue(
        revealObjects.TrySpawnBombedShootableBlock(revealLevel, revealLevel.BlockAt(areaNoOpIndex), 0x80, SamusProjectileType.Of(SamusProjectileFamily.Bomb)),
        "negative type-C BTS allocates area-table no-op PLM");
    revealObjects.Step(revealLevel);
    AssertEqual(RoomTerrainBlock.Of(0x057, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        revealLevel.GetCollisionBlockByIndex(powerRevealIndex).Terrain,
        "normal bomb reveals visible power-bomb block word");
    AssertEqual(RoomTerrainBlock.Of(0x09f, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        revealLevel.GetCollisionBlockByIndex(superRevealIndex).Terrain,
        "normal bomb reveals visible super-missile block word");
    AssertEqual(0, revealObjects.Sounds.SoundRequests.Count,
        "weapon-required reveal lists do not queue shot-break sound");
    AssertEqual(RoomTerrainBlock.Of(0x222, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        revealLevel.GetCollisionBlockByIndex(areaNoOpIndex).Terrain,
        "area-dependent shootable no-op leaves terrain unchanged");
    AssertEqual(2, revealObjects.Places.ActiveCount,
        "area no-op deletes while both one-frame reveal PLMs remain timed");
    revealObjects.Step(revealLevel);
    AssertEqual(0, revealObjects.Places.ActiveCount,
        "weapon-required reveal PLMs delete after their one-frame draw");

    // `$94:9DA4` chooses a two-by-two crumble reveal for BTS three. The draw record is two
    // horizontal rows with a signed `(0,+1)` continuation; asserting all four words guards
    // both the special dispatch and the generic multi-record parser. Brinstar's negative
    // BTS `$82` instead selects the area-specific speed-block reveal at `$84:C928`.
    var specialWords = new RoomTerrainBlock[width * height];
    const int crumbleIndex = 2 * width + 2;
    const int speedIndex = 5 * width + 5;
    specialWords[crumbleIndex] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SpecialBlock);
    specialWords[speedIndex] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SpecialBlock);
    RoomLevelData specialLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(specialWords, new RoomBlockBehavior[specialWords.Length], new RoomBlockAppearance[specialWords.Length]), RoomLevelData.NothingBeyondGrid(), []);
    // #320: the cartridge's normal table extends through BTS $4F, not $0F.
    // Exercise actual bomb collision dispatch and assert the resulting terrain.
    for (byte behavior = 16; behavior < 80; behavior++)
    {
        var words = new RoomTerrainBlock[width * height];
        var behaviors = new RoomBlockBehavior[words.Length];
        words[crumbleIndex] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SpecialBlock);
        behaviors[crumbleIndex] = RoomBlockBehaviorCodec.Decode(behavior);
        var tableLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(words, behaviors, new RoomBlockAppearance[words.Length]), RoomLevelData.NothingBeyondGrid(), []);
        var tableObjects = RoomObjectFixtures.Unbound();
        SamusBombProjectileSystem.ReactToBombedBlock(tableLevel,
            new(2, 2), tableObjects, SamusProjectileType.Of(SamusProjectileFamily.Bomb));
        tableObjects.Step(tableLevel);
        AssertEqual(behavior is >= 0x1a and <= 0x1d ? RoomTerrainBlock.Of(0x0b6, BlockMirroring.None, RoomCollisionType.SpecialBlock) : RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.SpecialBlock),
            tableLevel.GetCollisionBlockByIndex(crumbleIndex).Terrain,
            $"extended bomb-special BTS {behavior:X2} native reveal/no-op terrain");
    }
        var specialObjects = RoomObjectFixtures.Unbound();
    AssertTrue(
        specialObjects.SpawnBombedSpecialBlock(
            specialLevel, specialLevel.BlockAt(crumbleIndex), 3, AreaId.Crateria, SamusProjectileType.Of(SamusProjectileFamily.Bomb)),
        "normal bomb allocates two-by-two crumble reveal");
    AssertTrue(
        specialObjects.SpawnBombedSpecialBlock(
            specialLevel, specialLevel.BlockAt(speedIndex), 0x82, AreaId.Brinstar, SamusProjectileType.Of(SamusProjectileFamily.Bomb)),
        "Brinstar negative BTS two allocates speed-block reveal");
    AssertTrue(
        !specialObjects.SpawnBombedSpecialBlock(
            specialLevel, specialLevel.BlockAt(crumbleIndex), 3, AreaId.Crateria, SamusProjectileType.Of(SamusProjectileFamily.PowerBomb)),
        "Power Bomb visits special block but CFA0 synchronously clears its PLM header");
    AssertEqual(2, specialObjects.Places.ActiveCount,
        "rejected Power Bomb special reveal leaves no active PLM slot");
    specialObjects.Step(specialLevel);
    AssertEqual(RoomTerrainBlock.Of(0x0bc, BlockMirroring.None, RoomCollisionType.SpecialBlock),
        specialLevel.GetCollisionBlockByIndex(crumbleIndex).Terrain,
        "crumble reveal writes parent word");
    AssertEqual(RoomTerrainBlock.Of(0x0bc, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
        specialLevel.GetCollisionBlockByIndex(crumbleIndex + 1).Terrain,
        "crumble reveal writes right extension");
    AssertEqual(RoomTerrainBlock.Of(0x0bc, BlockMirroring.None, RoomCollisionType.VerticalExtension),
        specialLevel.GetCollisionBlockByIndex(crumbleIndex + width).Terrain,
        "crumble reveal writes lower vertical extension");
    AssertEqual(RoomTerrainBlock.Of(0x0bc, BlockMirroring.None, RoomCollisionType.VerticalExtension),
        specialLevel.GetCollisionBlockByIndex(crumbleIndex + width + 1).Terrain,
        "crumble reveal writes lower-right vertical extension");
    AssertEqual(RoomTerrainBlock.Of(0x0b6, BlockMirroring.None, RoomCollisionType.SpecialBlock),
        specialLevel.GetCollisionBlockByIndex(speedIndex).Terrain,
        "Brinstar area table reveals speed-booster block");
    specialObjects.Step(specialLevel);
    AssertEqual(0, specialObjects.Places.ActiveCount,
        "special reveal PLMs delete after their one-frame draw");

    // Bank `$A0:97E2-$A0:984E` decides direction from bomb-versus-Samus X. Its bank-$91
    // command-three handoff must retain the stable ball pose and arm `$0801-$0803`; it
    // must also reject non-ball callers instead of silently inventing a normal jump.
    var bombJump = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(bombJump);
    SamusPresentation.InitializeAnimation(bombJump);
    bombJump.Kinematics.YAcceleration = new UnsignedSixteenSixteen(0, 0x4000);
    AssertThrows<ArgumentOutOfRangeException>(
        () => bombJump.RequestMorphedBombJump((SamusBombJumpDirection)(-1)),
        "an undefined bomb-jump direction rejected");

    bombJump.RequestMorphedBombJump(SamusBombJumpDirection.Right);
    AssertEqual(SamusBombJump.FromBlast(SamusBombJumpDirection.Right).Arm(), bombJump.BombJump, "right bomb jump accepted");
    ushort startX = bombJump.Position.X.Whole;
    ushort startY = bombJump.Position.Y.Whole;
    SamusBombJumpMovement.Start(bombJump);
    AssertTrue(bombJump.BombJumpPhase == SamusBombJumpPhase.Rising,
        "bomb-jump start handler activates the special handler");
    AssertEqual(startX, bombJump.Position.X.Whole, "bomb-jump start frame has no horizontal displacement");
    AssertEqual(startY, bombJump.Position.Y.Whole, "bomb-jump start frame has no vertical displacement");
    AssertEqual(2, bombJump.Kinematics.VerticalVelocity.Speed.Whole, "bomb-jump whole speed comes from $90:9EF5");
    AssertEqual(0xc000, bombJump.Kinematics.VerticalVelocity.Speed.Fraction, "bomb-jump subspeed comes from $90:9EFB");
    AssertEqual(SamusVerticalDirection.Up, bombJump.Kinematics.VerticalVelocity.Direction, "bomb jump starts upward");

    // The first diagonal handler frame accelerates by the native 0.3000 record,
    // moves right fractionally, moves upward by the pre-gravity 2.C000 magnitude, then stores
    // the reduced 2.8000 magnitude for the following frame.
    SamusBombJumpMovement.Step(
empty, bombJump, frameCounter: 0, RoomObjectFixtures.Unbound());
    AssertTrue(bombJump.BombJumpPhase == SamusBombJumpPhase.Rising, "unobstructed diagonal bomb jump remains active");
    AssertEqual(startX, bombJump.Position.X.Whole, "right bomb jump retains whole X on first fractional step");
    AssertEqual(0x3000, bombJump.Kinematics.Position.X.Fraction, "right bomb jump uses native $90:9F25 fractional displacement");
    AssertEqual(2, bombJump.Kinematics.VerticalVelocity.Speed.Whole, "bomb-jump gravity stores next whole speed");
    AssertEqual(0x8000, bombJump.Kinematics.VerticalVelocity.Speed.Fraction, "bomb-jump gravity stores next subspeed");

    // `$90:8F1B` interprets a wrapped whole word as signed underflow. It switches to down,
    // installs diagonal deceleration mode two, and `$90:E032` relinquishes control before
    // making another vertical move. The normal type-four ball handler owns the descent.
    bombJump.Kinematics.VerticalVelocity = bombJump.Kinematics.VerticalVelocity.WithSpeed(new UnsignedSixteenSixteen(0xffff, 0));
    SamusBombJumpMovement.Step(
empty, bombJump, frameCounter: 1, RoomObjectFixtures.Unbound());
    AssertTrue(bombJump.BombJumpPhase != SamusBombJumpPhase.Rising, "signed bomb-jump apex ends special handler");
    AssertEqual(null, bombJump.BombJump, "bomb-jump apex drops the request");
    AssertEqual(SamusVerticalDirection.Down, bombJump.Kinematics.VerticalVelocity.Direction, "bomb-jump apex hands off downward direction");
    AssertEqual(SamusAccelerationMode.Decelerating, bombJump.HorizontalSpeed.AccelerationMode, "diagonal apex selects mode two");

    // Direction two deliberately omits horizontal calculation. A ceiling collision ends
    // the handler on that same frame and zeros vertical magnitude before normal movement
    // resumes, matching `$90:E077-$90:E094`.
    var ceilingBlocks = new RoomTerrainBlock[width * height];
    for (int x = 0; x < width; x++)
        ceilingBlocks[2 * width + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData ceiling = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(ceilingBlocks, new RoomBlockBehavior[ceilingBlocks.Length], new RoomBlockAppearance[ceilingBlocks.Length]), RoomLevelData.NothingBeyondGrid(), []);
    var straightBombJump = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundLeftPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 56)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(straightBombJump);
    SamusPresentation.InitializeAnimation(straightBombJump);
    straightBombJump.Kinematics.YAcceleration = new UnsignedSixteenSixteen(0, 0x4000);
    straightBombJump.RequestMorphedBombJump(SamusBombJumpDirection.Straight);
    SamusBombJumpMovement.Start(straightBombJump);
    ushort straightXSubposition = straightBombJump.Kinematics.Position.X.Fraction;
    BombJumpMovementResult ceilingHit = SamusBombJumpMovement.Step(
ceiling, straightBombJump, frameCounter: 0, RoomObjectFixtures.Unbound());
    AssertTrue(straightBombJump.BombJumpPhase != SamusBombJumpPhase.Rising && ceilingHit.Vertical is { Collided: true },
        "straight bomb jump terminates on ceiling");
    AssertEqual(straightXSubposition, straightBombJump.Kinematics.Position.X.Fraction,
        "straight bomb jump performs no horizontal move");
    AssertEqual(48, straightBombJump.Position.X.Whole, "straight bomb jump preserves X");
    AssertEqual(0, straightBombJump.Kinematics.VerticalVelocity.Speed.Whole, "ceiling hit clears vertical speed");

    // A grounded pose is intentional after the native special handler: morphed setup
    // preserved it. Prove that its normal falling collision can enter the ordinary bounce
    // state instead of throwing solely because the art still names a grounded ball.
    straightBombJump.Position = straightBombJump.Position.WithWholeY(55);
    straightBombJump.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(new UnsignedSixteenSixteen(3, 0), SamusVerticalDirection.Down);
    straightBombJump.Kinematics.YAcceleration = default;
    MorphBallMovementResult bombLanding = SamusMorphBallMovement.StepGrounded(
floor, straightBombJump, frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertTrue(bombLanding.Landed, "post-bomb-jump grounded art collides with floor");
    SamusPosturePoseTransitions.ApplyMorphBallLanding(straightBombJump);
    AssertEqual(SamusBallBounce.MorphBallFirst, straightBombJump.MorphBallBounceState, "post-bomb-jump landing enters bounce one");

    Console.WriteLine("  Morph Ball: entry, bomb spread, bomb jump, reaction PLMs, bounce, and tunnel collision agree.");
}

/// <summary>Advances one planted power bomb through fuse expiry to its first center-block boundary scan.</summary>
private static void RunPowerBombToInitialBoundary(
    RoomLevelData level,
    RoomObjectSystem roomObjects,
    Position16<RoomPixels> position)
{
    var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallGroundRightPose,
        EquippedItems = SamusEquipmentFlags.MorphBall,
        SelectedHudItem = HudItem.PowerBomb,
        PowerBombs = 1,
        MaxPowerBombs = 5,
        Position = FixedPosition<RoomPixels>.FromWhole(position),
    };
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    var bombs = CreateBombFixture();
    bombs.StepFrame(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, GameplayAction.Shoot, GameplayAction.Shoot);

    while (!bombs.PowerBombExplosion.IsActive)
        bombs.StepFrame(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, 0, 0);

    // `$90:C157` first turns the fuse-expiration sentinel into zero. The following frame
    // is the first `$94:A06A` boundary scan, initially the single center block.
    bombs.StepFrame(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, 0, 0);
}

/// <summary>
/// Seeds the bank-$93 normal-bomb and power-bomb projectile records and instruction lists
/// and the bank-$90 bomb-spread launch table consumed by the Morph Ball bomb verifier.
/// </summary>
static void InstallMorphBombProjectileFixture(TestAddressSpace bus)
{
    // Bank-$93 projectile fixtures copied byte-for-byte from the normal-bomb pointer/data
    // records and its slow, fast, and explosion instruction lists. The production code
    // must follow these pointers; no test-facing constructor is allowed to inject damage,
    // radii, frame durations, or animation endpoints directly into a slot.
    WriteTestWord(bus, 0x9383fb, 0x8675); // Non-beam type five -> normal bomb data.
    bus.WriteBytes(0x938675, [0x1e, 0x00, 0xbf, 0x9f]);
    // `$90:D8CF-$D8F6` is the complete five-slot bomb-spread launch table. Keeping the
    // encoded direction/magnitude words intact exercises the same XBA/sign decode as ROM.
    bus.WriteBytes(0x90d8cf, [
        0x78, 0x00, 0x6e, 0x00, 0x64, 0x00, 0x6e, 0x00, 0x78, 0x00,
        0x00, 0x81, 0x80, 0x80, 0x00, 0x00, 0x80, 0x00, 0x00, 0x01,
        0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00,
    ]);
    WriteTestWord(bus, 0x938683, 0xa06b); // Bomb-explosion instruction pointer.
    bus.WriteBytes(0x939fbf, [
        0x05, 0x00, 0x45, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x4c, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x53, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x5a, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xbf, 0x9f,
    ]);
    bus.WriteBytes(0x939fe3, [
        0x01, 0x00, 0x45, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x4c, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x53, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x5a, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xe3, 0x9f,
    ]);
    bus.WriteBytes(0x93a06b, [
        0x02, 0x00, 0x3e, 0xa8, 0x08, 0x08, 0x00, 0x00,
        0x02, 0x00, 0x54, 0xa8, 0x0c, 0x0c, 0x00, 0x00,
        0x02, 0x00, 0x6a, 0xa8, 0x10, 0x10, 0x00, 0x00,
        0x02, 0x00, 0x80, 0xa8, 0x10, 0x10, 0x00, 0x00,
        0x02, 0x00, 0x96, 0xa8, 0x10, 0x10, 0x00, 0x00,
        0x2f, 0x82,
    ]);

    // Power-bomb type three follows its own literal pointer/data record and three-frame
    // slow/fast loops. These bytes are ROM `$93:83F7/$8671/$9F87-$9FBE`; in particular,
    // damage `$00C8` and pointer `$9F87` are not host-selected stand-ins.
    WriteTestWord(bus, 0x9383f7, 0x8671);
    bus.WriteBytes(0x938671, [0xc8, 0x00, 0x87, 0x9f]);
    bus.WriteBytes(0x939f87, [
        0x05, 0x00, 0x97, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x9e, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0xa5, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0x87, 0x9f,
    ]);
    bus.WriteBytes(0x939fa3, [
        0x01, 0x00, 0x97, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x9e, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0xa5, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xa3, 0x9f,
    ]);
    bus.WriteBytes(0x93ab97, [
        0x01, 0x00, 0xfc, 0x01, 0xfc, 0x26, 0x3a,
        0x01, 0x00, 0xfc, 0x01, 0xfc, 0x27, 0x3a,
        0x01, 0x00, 0xfc, 0x01, 0xfc, 0x7b, 0x3a,
    ]);
}

/// <summary>Seeds the retail bank-$88 power-bomb fixed-color tables indexed by radius.</summary>
static void InstallPowerBombFixedColorFixture(TestAddressSpace bus)
{
    // `$88:9079` and `$88:8D85` are the fixed-color tables indexed by the high byte of
    // the two authentic 8.8 radii. Keeping the retail bytes makes phase/color assertions
    // detect an incorrect radius update as well as a merely incorrect phase enum.
    bus.WriteBytes(0x889079, [
        0x10, 0x10, 0x10, 0x04, 0x04, 0x04, 0x06, 0x06, 0x06,
        0x08, 0x08, 0x08, 0x0a, 0x0a, 0x0a, 0x0c, 0x0c, 0x0c,
        0x0e, 0x0e, 0x0a, 0x10, 0x10, 0x08, 0x12, 0x12, 0x08,
        0x14, 0x14, 0x08, 0x16, 0x16, 0x08, 0x18, 0x18, 0x08,
        0x1a, 0x1a, 0x0a, 0x18, 0x18, 0x08, 0x16, 0x16, 0x06,
        0x14, 0x14, 0x04,
    ]);
    bus.WriteBytes(0x888d85, [
        0x0e, 0x0e, 0x0a, 0x0f, 0x0f, 0x09, 0x10, 0x10, 0x08,
        0x11, 0x11, 0x07, 0x12, 0x12, 0x06, 0x13, 0x13, 0x05,
        0x14, 0x14, 0x04, 0x15, 0x15, 0x03, 0x16, 0x16, 0x02,
        0x17, 0x17, 0x01, 0x18, 0x18, 0x00, 0x19, 0x19, 0x00,
        0x1a, 0x1a, 0x00, 0x1a, 0x1a, 0x00, 0x1a, 0x1a, 0x1a,
        0x1a, 0x1a, 0x1a, 0x1b, 0x1b, 0x1b,
    ]);
}

/// <summary>
/// Seeds the retail bank-$84 bomb-reaction PLM instruction lists and draw lists exercised by
/// the bombable-terrain integration.
/// </summary>
static void InstallBombReactionObjectFixture(TestAddressSpace bus)
{
    // The bombable-terrain integration below intentionally supplies the literal bank-$84
    // BTS-zero reaction head and complete shared 1x1 respawn tail. `$84:CEDA` advances a
    // normal bomb's live pointer by three, so the leading `$8C46,$0A` sound instruction is
    // present in ROM but must be skipped. This makes a mistaken collision-list substitution
    // or invented host-side timer immediately observable.
    bus.WriteBytes(0x84cc3c, [
        0x46, 0x8c, 0x0a,       // Reaction-only sound $0A, skipped by normal bomb setup.
        0x04, 0x00, 0x45, 0xa3, // Four frames: visual block $053.
        0x04, 0x00, 0x4b, 0xa3, // Four frames: visual block $054.
        0x04, 0x00, 0x51, 0xa3, // Four frames: visual block $055.
        0x80, 0x01, 0x57, 0xa3, // 384 frames: blank-air visual block $0FF.
        0x04, 0x00, 0x51, 0xa3, // Reverse animation: $055.
        0x04, 0x00, 0x4b, 0xa3, // Reverse animation: $054.
        0x04, 0x00, 0x45, 0xa3, // Reverse animation: $053.
        0x17, 0x8b,             // DrawPLMBlock restores PLM_Vars and seeds timer one.
        0xbc, 0x86,             // Delete on the following PLM handler pass.
    ]);
    bus.WriteBytes(0x84a345, [0x01, 0x00, 0x53, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a34b, [0x01, 0x00, 0x54, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a351, [0x01, 0x00, 0x55, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a357, [0x01, 0x00, 0xff, 0x00, 0x00, 0x00]);

    // Additional bomb-reaction lists used later in this method. These bytes are literal
    // transcriptions of `$84:CADF`, `$84:C91C`, `$84:C922`, `$84:C8FE`, and `$84:C928`.
    // Keeping the instruction words in the sparse bus means the production interpreter—not
    // a test-only animation shortcut—still owns every duration, draw pointer, and deletion.
    bus.WriteBytes(0x84cadf, [
        0x79, 0x8c, 0x0a,       // Queue sound library 2, maximum 1: sound $0A.
        0x04, 0x00, 0x45, 0xa3, // Forward shot-break frames `$053,$054,$055`.
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x80, 0x01, 0x57, 0xa3, // Blank `$0FF` hold for exactly 384 frames.
        0x04, 0x00, 0x51, 0xa3, // Reverse frames before restoring PLM_Vars.
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x45, 0xa3,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);

    // `$84:D084/$D08C` retain these complete power-bomb and Super-Missile respawning
    // programs after their weapon-family setup succeeds. They differ only in the sound
    // opcode's native entry point: `$8C7C` and `$8C10` both consume the odd byte `$0A`, but
    // queue at maximum one and maximum six respectively. The shared draw pointers below are
    // cartridge-authored shot-break art, and DrawPLMBlock restores synthesized `$x057/$x09F`.
    bus.WriteBytes(0x84cb71, [
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0x45, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x80, 0x01, 0x57, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x45, 0xa3,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cb94, [
        0x7c, 0x8c, 0x0a,
        0x04, 0x00, 0x45, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x80, 0x01, 0x57, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x45, 0xa3,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);

    // The adjacent `$D090/$D088` headers select the permanent variants. Their shortened
    // 4/4/4 or 3/2/1 forward animation ends on one blank frame and deletes without executing
    // DrawPLMBlock, so the cleared terrain never grows back.
    bus.WriteBytes(0x84cc0b, [
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0x45, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x01, 0x00, 0x57, 0xa3,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cc20, [
        0x7c, 0x8c, 0x0a,
        0x03, 0x00, 0x45, 0xa3,
        0x02, 0x00, 0x4b, 0xa3,
        0x01, 0x00, 0x51, 0xa3,
        0x01, 0x00, 0x57, 0xa3,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84c91c, [0x01, 0x00, 0xe7, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84c922, [0x01, 0x00, 0xed, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84c8fe, [0x01, 0x00, 0xb1, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84c928, [0x01, 0x00, 0xf3, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84aae3, [0xbc, 0x86]); // PLMEntries_nothing: delete immediately.

    // The reveal draw data is likewise retail-authored. `$A4B1` contains two horizontal
    // two-word records joined by signed relative offset `(0,+1)`; the other three are
    // ordinary single-record lists terminated by a zero offset pair.
    bus.WriteBytes(0x84a4b1, [
        0x02, 0x00, 0xbc, 0xb0, 0xbc, 0x50, 0x00, 0x01,
        0x02, 0x00, 0xbc, 0xd0, 0xbc, 0xd0, 0x00, 0x00,
    ]);
    bus.WriteBytes(0x84a4e7, [0x01, 0x00, 0x57, 0xc0, 0x00, 0x00]);
    bus.WriteBytes(0x84a4ed, [0x01, 0x00, 0x9f, 0xc0, 0x00, 0x00]);
    bus.WriteBytes(0x84a4f3, [0x01, 0x00, 0xb6, 0xb0, 0x00, 0x00]);
}

    /// <summary>Runs the Morph Ball movement and bomb-reaction fixture.</summary>
    [VerificationCommand("--samus-morph-ball")]
    internal static int SamusMorphBallCommand()
    {
        Suite(nameof(VerifySamusMorphBallMovement), () => VerifySamusMorphBallMovement());
        return 0;
    }
}
