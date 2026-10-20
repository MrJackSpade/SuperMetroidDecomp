using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Focused cartridge comparisons for aerial motion, jump poses, liquid drag, and
/// atmosphere effects that share Samus's movement state.
/// </summary>
internal static class SamusAerialAndLiquidsVerification
{

/// <summary>Aerial, Space Jump, Screw Attack, liquid, and atmosphere verification.</summary>
internal static void VerifySamusAerialMovement()
{
    TestAddressSpace bus = CreateSamusAerialMovementCartridge();

    // Type two resolves the compiled native $9F6D entry. With no direction input the
    // movement handler still clears its tentative acceleration after the calculation.

    const int width = 8;
    const int height = 8;
    var foreground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        foreground[6 * width + x] = 0x8000; // ordinary solid floor begins at Y=96
    RoomLevelData level = CreateRoom(new(width, height), foreground, new byte[foreground.Length]);

    var samus = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 77)), // radius 19 will place jump-pose feet at floor Y=96
    };
    samus.PoseMetadata.RefreshCollisionRadii();
    samus.PoseTransitions.ApplyOrdinaryJumpTransition(SamusPoseId.NeutralJumpTransitionRightPose);
    AssertEqual(21, samus.Kinematics.Radius.Y, "jump pose commit retains standing radius until alpha");
    AssertEqual(0x0004, samus.Kinematics.YSpeed.Whole, "jump reads initial whole Y speed");
    AssertEqual(0xe000, samus.Kinematics.YSpeed.Fraction, "jump reads initial fractional Y speed");
    AssertEqual(SamusVerticalDirection.Up, samus.Kinematics.YDirection, "jump begins upward");

    // The $4B transition movement deliberately does not consume the initialized 4.E000.
    samus.PoseMetadata.RefreshCollisionRadii();
    AssertEqual(19, samus.Kinematics.Radius.Y, "next alpha publishes neutral jump radius");
    AerialMovementResult transitionFrame = SamusAerialMovement.StepNormalJump(
level, samus, SnesButton.A, nmiFrameCounter: 0);
    AssertEqual(77, samus.Position.Y.Whole, "neutral-jump transition preserves Y");
    samus.Presentation.AnimateNoFx(new SamusAnimationStep { Random = new CartridgeRandom() });
    AssertTrue(samus.PoseTransitions.ApplyPendingVerifiedAnimationTransition(), "FD installs neutral jump pose");
    AssertEqual(SamusPoseId.NeutralJumpRightPose, samus.Pose, "FD target pose");

    // The first real airborne frame moves by OLD 4.E000, then stores 4.C400 after gravity.
    AerialMovementResult firstRise = SamusAerialMovement.StepNormalJump(
level, samus, SnesButton.A, nmiFrameCounter: 1);
    AssertEqual(72, samus.Position.Y.Whole, "first rise old-speed whole displacement");
    AssertEqual(0x2000, samus.Kinematics.Position.Y.Fraction, "first rise old-speed fraction");
    AssertEqual(0x0004, samus.Kinematics.YSpeed.Whole, "first rise stored whole speed");
    AssertEqual(0xc400, samus.Kinematics.YSpeed.Fraction, "first rise subtracts gravity afterward");

    // Releasing jump cuts velocity before the common routine copies it. The frame has no
    // displacement, changes direction to down, and only primes 0.1C00 for the next frame.
    ushort releaseY = samus.Position.Y.Whole;
    ushort releaseSubY = samus.Kinematics.Position.Y.Fraction;
    SamusAerialMovement.StepNormalJump(level, samus, controllerInput: 0, nmiFrameCounter: 2);
    AssertEqual(SamusVerticalDirection.Down, samus.Kinematics.YDirection, "jump release starts falling");
    AssertEqual(releaseY, samus.Position.Y.Whole, "jump release stationary whole Y frame");
    AssertEqual(releaseSubY, samus.Kinematics.Position.Y.Fraction, "jump release stationary fractional Y frame");
    AssertEqual(0x1c00, samus.Kinematics.YSpeed.Fraction, "jump release primes falling gravity");

    // Continue the native recurrence until the solid floor clips a downward displacement.
    // This is bounded well above the roughly 50 frames needed by the synthetic room.
    AerialMovementResult result = default;
    StepUntil(
        () => result.Landed,
        frame => result = SamusAerialMovement.StepNormalJump(
level, samus, 0, unchecked((ushort)(frame + 3))),
        maximumFrames: 197,
        context: "neutral jump solid-floor landing");
    AssertEqual(77, samus.Position.Y.Whole, "aerial radius rests at floor before pose expansion");

    samus.PoseTransitions.ApplyAerialLanding(wasSpinning: false);
    AssertEqual(SamusPoseId.NormalLandingRightPose, samus.Pose, "normal right-facing landing pose");
    AssertEqual(75, samus.Position.Y.Whole, "landing radius expansion keeps feet fixed");
    AssertEqual(SamusVerticalDirection.None, samus.Kinematics.YDirection, "landing clears vertical direction");

    // The compiled retail $A4 stream is 05,02,F8,01. Five plus two ticks
    // reach $F8 at byte index two; its operand returns to $01.
    for (int tick = 0; tick < 7; tick++)
        samus.Presentation.AnimateNoFx(new SamusAnimationStep { Random = new CartridgeRandom() });
    AssertTrue(samus.PoseTransitions.ApplyPendingVerifiedAnimationTransition(), "landing F8 transition applies");
    AssertEqual(SamusPoseId.FacingRightNormalPose, samus.Pose, "landing animation returns to standing");

    // Aerial mode two accelerates because $90:9B22 tests only bit zero. This guards the
    // counterintuitive distinction from the grounded deceleration-allowed routine.
    var aerialSpeed = new SamusHorizontalSpeedState { AccelerationMode = 2 };
    aerialSpeed.SelectNormalAirSpeedTable();
    AerialBaseSpeedResult accelerated =
        aerialSpeed.CalculateBaseSpeedDecelerationDisallowed(movementType: SamusMovementType.NormalJumping);
    AssertEqual(0x0000c000u, accelerated.Speed, "aerial mode two uses native normal-jump acceleration");
    AssertTrue(!accelerated.ReachedMaximum, "aerial sub-cap call clears carry");

    // Mirror the launch through the left-facing spin-jump route. This checks that mode two
    // uses the pose direction normally, and that the type-three speed entry (not running's
    // type-one entry) supplies the in-air cap/acceleration.
    SeedLeftSpinJumpPoses(bus);
    var spinLeft = new SamusState
    {
        Pose = SamusPoseId.MovingLeftNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 77)),
    };
    spinLeft.HorizontalSpeed.BaseSpeed = spinLeft.HorizontalSpeed.BaseSpeed.WithWhole(1);
    spinLeft.PoseTransitions.ApplyOrdinaryJumpTransition(SamusPoseId.SpinJumpLeftPose);
    int spinStartX = (SnesSixteenSixteen.FromWhole(spinLeft.Position.X.Whole)) | spinLeft.Kinematics.Position.X.Fraction;
    SamusAerialMovement.StepSpinJump(

        level,
        spinLeft,
        (SnesButton.Left | SnesButton.A),
        nmiFrameCounter: 0);
    AssertEqual(-0x00016000,
        ((SnesSixteenSixteen.FromWhole(spinLeft.Position.X.Whole)) | spinLeft.Kinematics.Position.X.Fraction) - spinStartX,
        "left spin jump reaches native cap");
    AssertEqual(2, spinLeft.HorizontalSpeed.AccelerationMode, "spin jump selects aerial mode two");
    AssertEqual(46, spinLeft.Position.X.Whole, "left spin jump whole X");
    AssertEqual(0xa000, spinLeft.Kinematics.Position.X.Fraction, "left spin jump fractional X");

    // Turning input handler `$91:8142` uses the ordinary transition table. The reported
    // retail match `$26 -> $19` is therefore a real spin-jump launch, not an unsupported
    // turning-only side effect. Lock down both mirrors so the runtime cannot regress to a
    // an open-dispatch crash when Jump is pressed during either one-frame ground turn.
    var turnJumpRight = new SamusState { Pose = SamusPoseId.TurningLeftToRightPose };
    turnJumpRight.PoseTransitions.ApplyOrdinaryJumpTransition(SamusPoseId.SpinJumpRightPose);
    AssertEqual(SamusPoseId.SpinJumpRightPose, turnJumpRight.Pose,
        "left-to-right ground turn accepts spin jump");
    AssertEqual(SamusVerticalDirection.Up, turnJumpRight.Kinematics.YDirection,
        "left-to-right turn jump launches upward");

    var turnJumpLeft = new SamusState { Pose = SamusPoseId.TurningRightToLeftPose };
    turnJumpLeft.PoseTransitions.ApplyOrdinaryJumpTransition(SamusPoseId.SpinJumpLeftPose);
    AssertEqual(SamusPoseId.SpinJumpLeftPose, turnJumpLeft.Pose,
        "right-to-left ground turn accepts spin jump");
    AssertEqual(SamusVerticalDirection.Up, turnJumpLeft.Kinematics.YDirection,
        "right-to-left turn jump launches upward");

    // Releasing horizontal direction before the fresh Jump edge selects the neutral-jump
    // records `$4B/$4C`, not spin `$19/$1A`. These are the exact mirrors exercised by a
    // player who turns in place and immediately jumps to aim a diagonal shot.
    var neutralTurnJumpRight = new SamusState { Pose = SamusPoseId.TurningLeftToRightPose };
    neutralTurnJumpRight.PoseTransitions.ApplyOrdinaryJumpTransition(
        SamusPoseId.NeutralJumpTransitionRightPose);
    AssertEqual(SamusPoseId.NeutralJumpTransitionRightPose, neutralTurnJumpRight.Pose,
        "left-to-right ground turn accepts neutral jump");
    AssertEqual(SamusVerticalDirection.Up, neutralTurnJumpRight.Kinematics.YDirection,
        "left-to-right neutral turn jump launches upward");

    var neutralTurnJumpLeft = new SamusState { Pose = SamusPoseId.TurningRightToLeftPose };
    neutralTurnJumpLeft.PoseTransitions.ApplyOrdinaryJumpTransition(
        SamusPoseId.NeutralJumpTransitionLeftPose);
    AssertEqual(SamusPoseId.NeutralJumpTransitionLeftPose, neutralTurnJumpLeft.Pose,
        "right-to-left ground turn accepts neutral jump");
    AssertEqual(SamusVerticalDirection.Up, neutralTurnJumpLeft.Kinematics.YDirection,
        "right-to-left neutral turn jump launches upward");

    // Aimed standing turns do not become inert until their three-frame stream reaches
    // `$F8`. They keep using `$91:8142`, so Jump may interrupt every one of the six records
    // with the same spin/neutral pair as `$25/$26`. Exercise both outputs for both facings;
    // `$8D -> $1A` is the live Climb route which first exposed this omitted family.
    (SamusPoseId Source, SamusPoseId SpinTarget, SamusPoseId NeutralTarget)[] aimedTurnJumps =
    [
        (SamusPoseId.TurningRightToLeftAimUpPose,
            SamusPoseId.SpinJumpLeftPose, SamusPoseId.NeutralJumpTransitionLeftPose),
        (SamusPoseId.TurningLeftToRightAimUpPose,
            SamusPoseId.SpinJumpRightPose, SamusPoseId.NeutralJumpTransitionRightPose),
        (SamusPoseId.TurningRightToLeftAimDiagonalDownPose,
            SamusPoseId.SpinJumpLeftPose, SamusPoseId.NeutralJumpTransitionLeftPose),
        (SamusPoseId.TurningLeftToRightAimDiagonalDownPose,
            SamusPoseId.SpinJumpRightPose, SamusPoseId.NeutralJumpTransitionRightPose),
        (SamusPoseId.TurningRightToLeftAimDiagonalUpPose,
            SamusPoseId.SpinJumpLeftPose, SamusPoseId.NeutralJumpTransitionLeftPose),
        (SamusPoseId.TurningLeftToRightAimDiagonalUpPose,
            SamusPoseId.SpinJumpRightPose, SamusPoseId.NeutralJumpTransitionRightPose),
    ];
    foreach ((SamusPoseId source, SamusPoseId spinTarget, SamusPoseId neutralTarget) in aimedTurnJumps)
    {
        var spin = new SamusState { Pose = source };
        spin.PoseTransitions.ApplyOrdinaryJumpTransition(spinTarget);
        AssertEqual(spinTarget, spin.Pose,
            $"aimed standing turn ${(int)source:X2} accepts facing spin jump");
        AssertEqual(SamusVerticalDirection.Up, spin.Kinematics.YDirection,
            $"aimed standing turn ${(int)source:X2} spin launches upward");

        var neutral = new SamusState { Pose = source };
        neutral.PoseTransitions.ApplyOrdinaryJumpTransition(neutralTarget);
        AssertEqual(neutralTarget, neutral.Pose,
            $"aimed standing turn ${(int)source:X2} accepts facing neutral jump");
    }
    AssertThrows<InvalidOperationException>(
        () => new SamusState { Pose = SamusPoseId.TurningRightToLeftCrouchingPose }
            .PoseTransitions.ApplyOrdinaryJumpTransition(SamusPoseId.SpinJumpLeftPose),
        "crouched turn remains outside direct ordinary-jump initializer");

    // Walking off a ledge is the movement-type-six entry point. $91:E8F2 selects pose $2A
    // from the old left-facing direction and command five begins with a stationary falling
    // frame before gravity produces displacement. Landing from type six uses normal $A5.
    SeedLedgeFallPosesAndTypeSixSpeed(bus);
    var fallLeft = new SamusState
    {
        Pose = SamusPoseId.FacingLeftNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 77)),
    };
    fallLeft.PoseMetadata.RefreshCollisionRadii();
    fallLeft.PoseTransitions.ApplyWalkedOffFloorTransition(level, SamusPoseId.FallingLeftPose);
    AssertEqual(SamusPoseId.FallingLeftPose, fallLeft.Pose, "walk-off chooses left falling pose");
    AssertEqual(21, fallLeft.Kinematics.Radius.Y, "walk-off commit retains standing radius");
    fallLeft.PoseMetadata.RefreshCollisionRadii();
    AssertEqual(19, fallLeft.Kinematics.Radius.Y, "next alpha publishes falling radius");
    AerialMovementResult firstFall = SamusAerialMovement.StepFalling(
level, fallLeft, controllerInput: 0, nmiFrameCounter: 0);
    AssertEqual(77, fallLeft.Position.Y.Whole, "walk-off starts with stationary fall frame");
    AssertEqual(0x1c00, fallLeft.Kinematics.YSpeed.Fraction, "first fall frame primes gravity");
    AerialMovementResult fallResult = default;
    StepUntil(
        () => fallResult.Landed,
        frame => fallResult = SamusAerialMovement.StepFalling(
level, fallLeft, 0, unchecked((ushort)(frame + 1))),
        maximumFrames: 199,
        context: "left falling pose floor landing");
    fallLeft.PoseTransitions.ApplyAerialLanding(wasSpinning: false);
    AssertEqual(SamusPoseId.NormalLandingLeftPose, fallLeft.Pose, "left fall selects normal landing pose");

    // The standing transition table is shared by `$A4/$A5`, so Jump may interrupt normal
    // landing before its `$F8` fallback executes. Exercise both mirrors through the same
    // public initializer used by the runtime; this locks down actual jump velocity/radius
    // setup rather than merely accepting the target pose byte in the dispatcher.
    var interruptRightLanding = new SamusState { Pose = SamusPoseId.NormalLandingRightPose };
    interruptRightLanding.PoseTransitions.ApplyOrdinaryJumpTransition(
        SamusPoseId.NeutralJumpTransitionRightPose);
    AssertEqual(SamusPoseId.NeutralJumpTransitionRightPose, interruptRightLanding.Pose,
        "right normal landing accepts fresh jump");
    AssertEqual(SamusVerticalDirection.Up, interruptRightLanding.Kinematics.YDirection,
        "right landing jump starts upward");

    var interruptLeftLanding = new SamusState { Pose = SamusPoseId.NormalLandingLeftPose };
    interruptLeftLanding.PoseTransitions.ApplyOrdinaryJumpTransition(
        SamusPoseId.NeutralJumpTransitionLeftPose);
    AssertEqual(SamusPoseId.NeutralJumpTransitionLeftPose, interruptLeftLanding.Pose,
        "left normal landing accepts fresh jump");
    AssertEqual(SamusVerticalDirection.Up, interruptLeftLanding.Kinematics.YDirection,
        "left landing jump starts upward");

    var interruptRightSpinLanding = new SamusState
    {
        Pose = SamusPoseId.SpinLandingRightPose,
    };
    interruptRightSpinLanding.PoseTransitions.ApplyOrdinaryJumpTransition(
        SamusPoseId.NeutralJumpTransitionRightPose);
    AssertEqual(SamusPoseId.NeutralJumpTransitionRightPose, interruptRightSpinLanding.Pose,
        "right spin landing accepts fresh neutral jump");
    AssertEqual(SamusVerticalDirection.Up, interruptRightSpinLanding.Kinematics.YDirection,
        "right spin-landing jump starts upward");

    var interruptLeftSpinLanding = new SamusState
    {
        Pose = SamusPoseId.SpinLandingLeftPose,
    };
    interruptLeftSpinLanding.PoseTransitions.ApplyOrdinaryJumpTransition(
        SamusPoseId.NeutralJumpTransitionLeftPose);
    AssertEqual(SamusPoseId.NeutralJumpTransitionLeftPose, interruptLeftSpinLanding.Pose,
        "left spin landing accepts fresh neutral jump");
    AssertEqual(SamusVerticalDirection.Up, interruptLeftSpinLanding.Kinematics.YDirection,
        "left spin-landing jump starts upward");

    // The engine alternates $94:959E left-to-right and $94:95F5 right-to-left vertical
    // scans on successive NMIs. Their $1A counters run in opposite numerical directions,
    // but the physical result must be identical. Exercise every square-slope shape,
    // orientation, and grazed column with exactly one candidate block; this is the case
    // that previously made a fall beside a wall report landing every other frame.
    for (int shape = 0; shape < 5; shape++)
    {
        for (int orientation = 0; orientation < 4; orientation++)
        {
            for (int solidBlockX = 1; solidBlockX <= 4; solidBlockX++)
            {
                var squareForeground = new ushort[width * height];
                var squareBehavior = new byte[width * height];
                squareForeground[3 * width + solidBlockX] = 0x1000;
                squareBehavior[3 * width + solidBlockX] = unchecked((byte)(
                    shape | (orientation << 6)));
                RoomLevelData squareRoom = CreateRoom(new(width, height),
                    squareForeground,
                    squareBehavior);

                for (ushort x = 20; x <= 68; x += 4)
                {
                    SamusKinematicsState leftToRight = new()
                    {
                        Position = new(new(x, 0), new(36, 0)),
                        Radius = new(12, 12),
                    };
                    SamusKinematicsState rightToLeft = new()
                    {
                        Position = new(new(x, 0), new(36, 0)),
                        Radius = new(12, 12),
                    };
                    BlockMoveResult ltr = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = squareRoom }, leftToRight, new VerticalBlockMove { Displacement = 1 << 16, ScanLeftToRight = true });
                    BlockMoveResult rtl = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = squareRoom }, rightToLeft, new VerticalBlockMove { Displacement = 1 << 16, ScanLeftToRight = false });
                    AssertEqual(ltr.Collided, rtl.Collided,
                        $"square slope {shape}/{orientation} X={x} collision scan parity");
                    AssertEqual(ltr.AcceptedDisplacement, rtl.AcceptedDisplacement,
                        $"square slope {shape}/{orientation} X={x} displacement scan parity");
                }
            }
        }
    }

    Console.WriteLine("  Samus aerial: FD launch, exact 16.16 arc, jump cut, floor landing, radius, F8, and alternating square-slope scans agree.");
}

/// <summary>
/// Exercises the equipment-aware spin initializer, every edge of the dry-air Space Jump
/// velocity window, Screw Attack contact/wall frames, and both bank-$91/bank-$9B palette
/// indirections. These checks deliberately keep Space Jump physics independent from the
/// visible Screw Attack pose used when both retail item bits are equipped.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifySamusSpaceJumpAndScrewAttack()
{
    TestAddressSpace bus = CreateSpaceJumpAndScrewAttackCartridge();

    RoomLevelData empty = CreateEmptyRoom(new(16, 16));

    // Exercise the real F624 pose initializer with the same constructed cartridge as
    // the movement tests below. Environment and cinematic gates apply before queuing,
    // not as a host-side attempt to guess whether an audible spin should be playing.
    foreach (bool cinematic in new[] { false, true })
    foreach (bool submerged in new[] { false, true })
    foreach (bool gravity in new[] { false, true })
    {
        var ordinary = new SamusState
        {
            Pose = SamusPoseId.MovingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            EquippedItems = gravity ? SamusEquipmentFlags.GravitySuit : 0,
        };
        ordinary.LiquidPhysics.FxYPosition = submerged ? (ushort)0 : ushort.MaxValue;
        ordinary.LiquidPhysics.CinematicFunctionActive = cinematic;
        ordinary.PoseTransitions.ApplyOrdinaryJumpTransition(SamusPoseId.SpinJumpRightPose);
        bool expectedSound = !cinematic && (!submerged || gravity);
        AssertEqual(expectedSound ? 1 : 0, ordinary.LiquidPhysics.SoundRequests.Count,
            $"ordinary spin sound cinematic={cinematic}, submerged={submerged}, gravity={gravity}");
        if (expectedSound)
            AssertEqual(new SamusSoundRequest(SoundEffectLibrary1Sounds.SpinJump, 6),
                ordinary.LiquidPhysics.SoundRequests[0], "ordinary spin uses native max-six queue");
        ordinary.LiquidPhysics.BeginFrameSoundRequests();
        ordinary.PoseTransitions.ApplySpinJumpDirectionTransition(SamusPoseId.SpinJumpLeftPose);
        AssertEqual(0, ordinary.LiquidPhysics.SoundRequests.Count, "ordinary reversal suppresses restart");
    }

    foreach (SamusMovementType previous in new[] { SamusMovementType.SpinJumping, SamusMovementType.WallJumping, SamusMovementType.Running })
    foreach (SamusPoseId currentPose in new[] { SamusPoseId.ScrewAttackRightPose, SamusPoseId.WallJumpRightPose, SamusPoseId.NormalJumpGunExtendedRightPose })
    {
        var endingSpin = new SamusState { Pose = currentPose };
        SamusPostDrawAudio.Step(endingSpin, previous, 0);
        bool endsSpin = previous != SamusMovementType.Running && currentPose == SamusPoseId.NormalJumpGunExtendedRightPose;
        AssertEqual(endsSpin ? 1 : 0, endingSpin.LiquidPhysics.SoundRequests.Count, "post-draw spin exit classification");
        if (endsSpin)
            AssertEqual(new SamusSoundRequest(SoundEffectLibrary1Sounds.StopSpinJump, 15),
                endingSpin.LiquidPhysics.SoundRequests[0], "post-draw stop uses native max-fifteen queue");
    }
    var resumeCharge = new SamusState
    {
        Pose = SamusPoseId.NormalJumpGunExtendedRightPose,
        ProjectileFlareCounter = SamusProjectileRomData.Beams.ChargeSoundStartCounter,
    };
    SamusPostDrawAudio.Step(resumeCharge, SamusMovementType.SpinJumping, SnesButton.X);
    AssertEqual(ResumeChargingBeamSoundLatch.ResumeNextPass, resumeCharge.ResumeChargingBeamSound, "spin exit defers charge resume");
    AssertEqual(1, resumeCharge.LiquidPhysics.SoundRequests.Count, "only stop is queued on spin exit");
    resumeCharge.LiquidPhysics.BeginFrameSoundRequests();
    SamusPostDrawAudio.Step(resumeCharge, SamusMovementType.NormalJumping, SnesButton.X);
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x41), 9),
        resumeCharge.LiquidPhysics.SoundRequests.Single(), "next frame resumes charge after stop");
    resumeCharge.LiquidPhysics.BeginFrameSoundRequests();
    resumeCharge.ResumeChargingBeamSound = ResumeChargingBeamSoundLatch.SignedLatch;
    SamusPostDrawAudio.Step(resumeCharge, SamusMovementType.SpinJumping, SnesButton.X);
    AssertEqual(0, resumeCharge.LiquidPhysics.SoundRequests.Count, "negative charge latch bypasses audio work this frame");
    AssertEqual(ResumeChargingBeamSoundLatch.ResumeNextPass, resumeCharge.ResumeChargingBeamSound, "negative charge latch rearms next-frame resume");

    var spaceLaunch = new SamusState
    {
        Pose = SamusPoseId.MovingRightNormalPose,
        EquippedItems = (SamusEquipmentFlags)0x0200,
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
    };
    spaceLaunch.PoseTransitions.ApplyOrdinaryJumpTransition(SamusPoseId.SpinJumpRightPose);
    AssertEqual(SamusPoseId.SpaceJumpRightPose, spaceLaunch.Pose,
        "Space Jump substitutes right spin pose");
    AssertEqual(1, spaceLaunch.LiquidPhysics.SoundRequests.Count, "Space Jump entry publishes its start sound");
    AssertEqual(SoundEffectLibrary1Sounds.SpaceJump, spaceLaunch.LiquidPhysics.SoundRequests[0].SoundEffect,
        "Space Jump entry uses the cartridge sequence");
    spaceLaunch.LiquidPhysics.BeginFrameSoundRequests();
    spaceLaunch.PoseTransitions.ApplySpinJumpDirectionTransition(SamusPoseId.SpinJumpLeftPose);
    AssertEqual(1, spaceLaunch.LiquidPhysics.SoundRequests.Count, "Space Jump reversal republishes its sound");

    var screwLaunch = new SamusState
    {
        Pose = SamusPoseId.MovingLeftNormalPose,
        EquippedItems = (SamusEquipmentFlags)0x0208,
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
    };
    screwLaunch.PoseTransitions.ApplyOrdinaryJumpTransition(SamusPoseId.SpinJumpLeftPose);
    AssertEqual(SamusPoseId.ScrewAttackLeftPose, screwLaunch.Pose,
        "Screw Attack takes priority over Space Jump pose");
    AssertEqual(1, screwLaunch.LiquidPhysics.SoundRequests.Count, "Screw Attack entry publishes one sound");
    AssertEqual(SoundEffectLibrary1Sounds.ScrewAttack, screwLaunch.LiquidPhysics.SoundRequests[0].SoundEffect,
        "Screw Attack sound takes priority over Space Jump");
    screwLaunch.LiquidPhysics.BeginFrameSoundRequests();

    // `$81/$82` have their own retail input tables, so an opposite-direction match may
    // publish the specialized target directly rather than generic `$19/$1A`. The common
    // F624 initializer must accept that record, preserve Screw's equipment priority, and
    // still start a direction change at animation frame one.
    screwLaunch.PoseTransitions.ApplySpinJumpDirectionTransition(SamusPoseId.ScrewAttackRightPose);
    AssertEqual(SamusPoseId.ScrewAttackRightPose, screwLaunch.Pose,
        "direct Screw table target preserves equipped art");
    AssertEqual(1, screwLaunch.AnimationFrame,
        "direct Screw direction transition starts at frame one");
    AssertEqual(0, screwLaunch.LiquidPhysics.SoundRequests.Count,
        "Screw Attack reversal does not restart its sound");

    // Fire from a spin uses `$19/$1B/$81 -> $13`, expanding radius 12 -> 19 through
    // changed-pose collision but preserving the live jump arc. `$91:F543` also selects
    // mode two from residual extra-run speed and publishes the target's shot direction.
    var spinFire = new SamusState
    {
        Pose = SamusPoseId.ScrewAttackRightPose,
        EquippedItems = SamusEquipmentFlags.ScrewAttack,
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        Kinematics =
        {
            Radius = new(5, 12),
            YDirection = SamusVerticalDirection.Up,
            YSpeed = new SnesUnsignedSixteenSixteen(3, 0x4567),
        },
    };
    spinFire.HorizontalSpeed.ExtraRunSpeed = spinFire.HorizontalSpeed.ExtraRunSpeed.WithFraction(1);
    AssertTrue(spinFire.TryApplySpinToNormalJumpFireTransition(
        empty,
        SamusPoseId.NormalJumpGunExtendedRightPose,
        nmiFrameCounter: 0,
        controllerNewInput: SnesButton.X),
        "spin Fire body expansion fits empty room");
    AssertEqual(SamusPoseId.NormalJumpGunExtendedRightPose, spinFire.Pose,
        "spin Fire selects gun-extended normal jump");
    AssertEqual(12, spinFire.Kinematics.Radius.Y,
        "spin Fire retains source radius for the transition update");
    spinFire.PoseMetadata.RefreshCollisionRadii();
    AssertEqual(19, spinFire.Kinematics.Radius.Y,
        "next alpha publishes normal-jump radius");
    AssertEqual(3, spinFire.Kinematics.YSpeed.Whole,
        "spin Fire preserves whole vertical speed");
    AssertEqual(0x4567, spinFire.Kinematics.YSpeed.Fraction,
        "spin Fire preserves fractional vertical speed");
    AssertEqual(2, spinFire.HorizontalSpeed.AccelerationMode,
        "spin Fire retains extra-run aerial acceleration");
    AssertEqual(0x8002, spinFire.PoseTransitionShotDirection,
        "spin Fire publishes target shot direction");
    AssertTrue(spinFire.HorizontalSpeed.NormalSuitPaletteRestoreRequested,
        "leaving Screw Attack requests normal suit palette");

    // `$91:A9EC` is the retail `$83` wall-jump input table. Its Shot record is literally
    // `$0000,$0040,$0013`; `$91:F404` then sends that target through the same changed-pose,
    // type-two initializer used by a spinning source. Prove the wall route independently so
    // the exhaustive runtime dispatcher cannot silently regress to spin-only admission.
    var wallFire = new SamusState
    {
        Pose = SamusPoseId.WallJumpRightPose,
        EquippedItems = SamusEquipmentFlags.ScrewAttack,
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        Kinematics =
        {
            Radius = new(5, 19),
            YDirection = SamusVerticalDirection.Up,
            YSpeed = new SnesUnsignedSixteenSixteen(4, 0x2345),
        },
    };
    AssertTrue(wallFire.PoseTransitions.TryApplySpinOrWallJumpToNormalJumpTransition(

        empty,
        SamusPoseId.NormalJumpGunExtendedRightPose,
        nmiFrameCounter: 0,
        controllerNewInput: SnesButton.X),
        "wall-jump Shot body expansion fits empty room");
    AssertEqual(SamusPoseId.NormalJumpGunExtendedRightPose, wallFire.Pose,
        "wall-jump Shot selects cartridge target $13");
    AssertEqual(19, wallFire.Kinematics.Radius.Y,
        "wall-jump Shot expands to normal-jump radius");
    AssertEqual(4, wallFire.Kinematics.YSpeed.Whole,
        "wall-jump Shot preserves whole vertical speed");
    AssertEqual(0x2345, wallFire.Kinematics.YSpeed.Fraction,
        "wall-jump Shot preserves fractional vertical speed");
    AssertEqual(0x8002, wallFire.PoseTransitionShotDirection,
        "wall-jump Shot publishes target shot direction");
    AssertTrue(wallFire.HorizontalSpeed.NormalSuitPaletteRestoreRequested,
        "leaving Screw wall-jump requests normal suit palette");

    // Down during a left spin selects compact normal-jump pose `$18`. It enters the same
    // `$91:F543` initializer as Fire but shrinks radius 12 -> 10, preserves the running jump
    // arc, and does not invent the `$8000` projectile bridge without a fresh Shoot edge.
    var spinAimDown = new SamusState
    {
        Pose = SamusPoseId.SpinJumpLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        Kinematics =
        {
            Radius = new(5, 12),
            YDirection = SamusVerticalDirection.Up,
            YSpeed = new SnesUnsignedSixteenSixteen(2, 0x3456),
        },
    };
    AssertTrue(spinAimDown.PoseTransitions.TryApplySpinOrWallJumpToNormalJumpTransition(

        empty,
        SamusPoseId.NormalJumpAimDownLeftPose,
        nmiFrameCounter: 0,
        controllerNewInput: 0),
        "spin Down body contraction fits empty room");
    AssertEqual(SamusPoseId.NormalJumpAimDownLeftPose, spinAimDown.Pose,
        "spin Down selects compact left normal jump");
    AssertEqual(12, spinAimDown.Kinematics.Radius.Y,
        "spin Down retains source radius for the transition update");
    spinAimDown.PoseMetadata.RefreshCollisionRadii();
    AssertEqual(10, spinAimDown.Kinematics.Radius.Y,
        "next alpha publishes straight-down radius");
    AssertEqual(128, spinAimDown.Position.Y.Whole,
        "spin Down contraction does not move the body center");
    AssertEqual(2, spinAimDown.Kinematics.YSpeed.Whole,
        "spin Down preserves whole vertical speed");
    AssertEqual(0x3456, spinAimDown.Kinematics.YSpeed.Fraction,
        "spin Down preserves fractional vertical speed");
    AssertEqual(0, spinAimDown.PoseTransitionShotDirection,
        "spin Down without Shoot does not publish a projectile bridge");

    static SamusState CreateFallingSpin(SamusPoseId pose, ushort items, ushort speed, ushort subspeed) => new()
    {
        Pose = pose,
        EquippedItems = (SamusEquipmentFlags)items,
        Position = new(new(128, 0), new(128, 0)),
        Kinematics =
        {
            YDirection = SamusVerticalDirection.Down,
            YSpeed = new SnesUnsignedSixteenSixteen(speed, subspeed),
            YAcceleration = new SnesUnsignedSixteenSixteen(0, 0x2800),
            Radius = new(5, 12),
        },
    };

    // `$0280` is inclusive. A fresh edge restarts at 4.E000, moves upward by that OLD
    // magnitude, then stores 4.C400 after jump setup refreshes native NTSC gravity.
    SamusState minimum = CreateFallingSpin(
        SamusPoseId.SpaceJumpRightPose, 0x0200, speed: 2, subspeed: 0x8000);
    SamusAerialMovement.StepSpinJump(

        empty,
        minimum,
        SnesButton.A,
        nmiFrameCounter: 0,
        controllerNewInput: SnesButton.A);
    AssertEqual(SamusVerticalDirection.Up, minimum.Kinematics.YDirection, "Space Jump minimum velocity restarts upward");
    AssertEqual(4, minimum.Kinematics.YSpeed.Whole, "Space Jump restart whole speed");
    AssertEqual(0xc400, minimum.Kinematics.YSpeed.Fraction, "Space Jump restart applies refreshed native gravity after movement");
    AssertEqual(123, minimum.Position.Y.Whole, "Space Jump restart moves by old 4.E000 magnitude");

    SamusState below = CreateFallingSpin(
        SamusPoseId.SpaceJumpRightPose, 0x0200, speed: 2, subspeed: 0x7fff);
    SamusAerialMovement.StepSpinJump(
empty, below, SnesButton.A, 0, SnesButton.A);
    AssertEqual(SamusVerticalDirection.Down, below.Kinematics.YDirection, "Space Jump rejects velocity $027F");

    SamusState maximum = CreateFallingSpin(
        SamusPoseId.SpaceJumpRightPose, 0x0200, speed: 5, subspeed: 0);
    SamusAerialMovement.StepSpinJump(
empty, maximum, SnesButton.A, 0, SnesButton.A);
    AssertEqual(SamusVerticalDirection.Down, maximum.Kinematics.YDirection, "Space Jump maximum $0500 is exclusive");

    SamusState heldOnly = CreateFallingSpin(
        SamusPoseId.SpaceJumpRightPose, 0x0200, speed: 3, subspeed: 0);
    SamusAerialMovement.StepSpinJump(empty, heldOnly, SnesButton.A, 0);
    AssertEqual(SamusVerticalDirection.Down, heldOnly.Kinematics.YDirection, "Space Jump requires a fresh Jump edge");

    // Both item bits retain Space Jump physics while Screw Attack owns art and damage.
    SamusState screwRepeat = CreateFallingSpin(
        SamusPoseId.ScrewAttackRightPose, 0x0208, speed: 3, subspeed: 0);
    SamusAerialMovement.StepSpinJump(
empty, screwRepeat, SnesButton.A, 0, SnesButton.A);
    AssertEqual(SamusVerticalDirection.Up, screwRepeat.Kinematics.YDirection,
        "Screw Attack with Space Jump repeats upward");
    AssertEqual(SamusContactDamageIndex.ScrewAttack, screwRepeat.HorizontalSpeed.ContactDamageIndex,
        "Screw Attack republishes contact damage index three");

    // `$84:CE91-$CEA3` explicitly admits poses `$81/$82`, independently of the boost-stage
    // branch above them. Place a BTS-4 permanent 1x1 block under a falling Screw body so the
    // shared vertical movement must allocate the room PLM and continue through new air.
    var screwBombForeground = new ushort[16 * 16];
    var screwBombBts = new byte[screwBombForeground.Length];
    int screwBombIndex = 8 * 16 + 8;
    screwBombForeground[screwBombIndex] = 0xf123;
    screwBombBts[screwBombIndex] = 4;
    RoomLevelData screwBombLevel = CreateRoom(new(16, 16), screwBombForeground, screwBombBts);
    var screwBombPlms = PlmFixtures.Unbound();
    SamusState screwBombSamus = CreateFallingSpin(
        SamusPoseId.ScrewAttackRightPose, 0x0008, speed: 3, subspeed: 0);
    ushort screwBombY = screwBombSamus.Position.Y.Whole;
    AerialMovementResult screwBombFrame = SamusAerialMovement.StepSpinJump(

        screwBombLevel,
        screwBombSamus,
        controllerInput: 0,
        nmiFrameCounter: 0,
        controllerNewInput: 0,
        plms: screwBombPlms);
    AssertTrue(screwBombSamus.Position.Y.Whole != screwBombY,
        "Screw collision-bomb setup returns carry clear and preserves vertical travel");
    AssertTrue(screwBombPlms.PopulationSlots.Any(slot => slot.BlockIndex == screwBombIndex),
        "Screw vertical collision reacts at the exact BTS-4 block");
    AssertEqual(0x0058,
        screwBombLevel.GetCollisionBlockByIndex(screwBombIndex).PackedWord.Raw,
        "Screw setup installs CE83's replacement visual before the PLM handler");
    AssertEqual(1, screwBombPlms.SlotTable.ActiveCount,
        "Screw pose installs the BTS-4 bank-$84 lifecycle in the room owner");

    // A fully charged beam makes ordinary/Space-Jump spin damaging only in dry physics.
    // Full submersion suppresses that contact mode and, on animation frames zero/eight's
    // final tick, emits the literal library-one sound $2F instead.
    SamusState chargedSpin = CreateFallingSpin(
        SamusPoseId.SpaceJumpRightPose, 0x0200, speed: 3, subspeed: 0);
    chargedSpin.ProjectileFlareCounter = 0x003c;
    SamusAerialMovement.StepSpinJump(empty, chargedSpin, 0, 0, 0);
    AssertEqual(SamusContactDamageIndex.PseudoScrew, chargedSpin.HorizontalSpeed.ContactDamageIndex,
        "fully charged dry spin publishes contact damage index four");

    SamusState submergedSpin = CreateFallingSpin(
        SamusPoseId.SpaceJumpRightPose, 0x0200, speed: 3, subspeed: 0);
    submergedSpin.ProjectileFlareCounter = 0x003c;
    submergedSpin.Presentation.InitializeAnimation(initialFrame: 0);
    for (int tick = 0; tick < 3; tick++)
        submergedSpin.Presentation.AnimateNoFx(new SamusAnimationStep { Random = new CartridgeRandom() });
    submergedSpin.LiquidPhysics.ConfigureWater(surfaceY: 100);
    submergedSpin.LiquidPhysics.BeginFrameSoundRequests();
    SamusAerialMovement.StepSpinJump(empty, submergedSpin, 0, 0, 0);
    AssertEqual(SamusContactDamageIndex.None, submergedSpin.HorizontalSpeed.ContactDamageIndex,
        "full liquid physics suppresses charged-spin contact damage");
    AssertEqual(1, submergedSpin.LiquidPhysics.SoundRequests.Count,
        "underwater Space Jump sound frame publishes once");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x2f), 6), submergedSpin.LiquidPhysics.SoundRequests[0],
        "underwater Space Jump uses library-one sound $2F max six");

    screwRepeat.AnimationFrame = 4;
    screwRepeat.PoseTransitions.ApplyWallContactAnimationRewind();
    AssertEqual(0x1a, screwRepeat.AnimationFrame,
        "early Screw wall contact rewinds to frame 26");
    minimum.AnimationFrame = 4;
    minimum.PoseTransitions.ApplyWallContactAnimationRewind();
    AssertEqual(0x0a, minimum.AnimationFrame,
        "Space Jump wall contact uses ordinary frame 10 rewind");

    // The compiled Power Suit Screw list visits four distinct shades in the
    // native 0,1,2,3,2,1 order before wrapping to phase zero.
    SeedSamusPaletteWord(bus, 0x9400, 0x0111);
    for (int shade = 0; shade < 4; shade++)
    {
        AssertTrue(SamusPaletteRomData.FullBodyCycles.TryScrewAttackPalettePointer(
            0, unchecked((ushort)(shade * 2)), out ushort pointer),
            $"compiled Power Screw shade {shade} exists");
        SeedSamusPaletteWord(bus, pointer, unchecked((ushort)(0x1200 + shade)));
    }

    var suitColors = SamusSuitColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus)));
    var cycleColors = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorExtractor.Extract(bus)));
    var palettes = new SamusHorizontalSpeedState();
    var cgram = new SnesCgram();
    AssertTrue(palettes.UpdateSpeedBoosterPalette(cgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 0x1b, EquippedItems = (SamusEquipmentFlags)0x0008 }, suitColors: suitColors, cycleColors: cycleColors),
        "ending Screw frame copies normal suit palette");
    AssertEqual(0x0111, cgram.Colors[192], "ending Screw frame normal palette");
    for (int frame = 0; frame < 6; frame++)
    {
        AssertTrue(palettes.UpdateSpeedBoosterPalette(cgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 1, EquippedItems = (SamusEquipmentFlags)0x0008 }, suitColors: suitColors, cycleColors: cycleColors),
            $"Screw palette frame {frame} copies");
        AssertEqual(unchecked((ushort)(0x1200 + Math.Min(frame, 6 - frame))), cgram.Colors[192],
            $"Screw palette frame {frame} ROM color");
    }
    AssertEqual(0, palettes.SpecialPaletteFrame, "six Screw palettes wrap to offset zero");

    // A Screw landing requests the same normal palette reload performed by `$91:F433`.
    screwRepeat.PoseTransitions.ApplyAerialLanding(wasSpinning: true);
    AssertEqual(SamusPoseId.SpinLandingRightPose, screwRepeat.Pose, "Screw Attack lands through spin landing");
    AssertTrue(screwRepeat.HorizontalSpeed.NormalSuitPaletteRestoreRequested,
        "Screw Attack landing requests normal palette restore");

    Console.WriteLine(
        "  Space Jump/Screw Attack: pose priority, wall-jump Shot exit, repeat window, collision PLMs, charged/Screw damage, underwater sound, palette cycle, and landing agree.");
}

/// <summary>
/// Exercises the deliberately different top/bottom/bottom-minus-one liquid boundaries,
/// all three ROM jump/gravity/X-table selections, Gravity Suit bypass, submerged dash
/// behavior, and the water-specific Space Jump velocity window.
/// </summary>
/// <summary>Writes one word of the bank-$9B Samus palette data at <paramref name="pointer"/>.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$9B Samus palette word")]
static void SeedSamusPaletteWord(TestAddressSpace bus, ushort pointer, ushort color) =>
    WriteTestWord(bus, SamusPaletteRomData.Banks.Palette | pointer, color);

/// <summary>
/// Seeds the three media's standalone grapple-release speed records ($90:9F31/$9F3D/$9F49):
/// all zero except each record's fractional deceleration word at +10.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the grapple-release speed records")]
static void SeedGrappleReleaseSpeedRecords(TestAddressSpace bus, ushort[] grappleReleaseDeceleration)
{
    int[] grappleReleaseRecords = [0x909f31, 0x909f3d, 0x909f49];
    for (int medium = 0; medium < grappleReleaseRecords.Length; medium++)
    {
        int address = grappleReleaseRecords[medium];
        for (int byteOffset = 0; byteOffset < SpeedTableEntryConstants.ByteCount; byteOffset += 2)
            WriteTestWord(bus, address + byteOffset, 0);
        WriteTestWord(bus, address + 10, grappleReleaseDeceleration[medium]);
    }
}

/// <summary>Checks Samus's velocity, pose, and effects across water and lava boundaries.</summary>
internal static void VerifySamusLiquidPhysics()
{
    TestAddressSpace bus = CreateSamusLiquidPhysicsCartridge();

    var sample = new SamusState { Pose = SamusPoseId.SpinJumpRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(64, 100)) };
    sample.Kinematics.Radius = sample.Kinematics.Radius.WithY(12); // top 88, exclusive bottom 112, occupied bottom pixel 111
    sample.LiquidPhysics.ConfigureWater(surfaceY: 110);
    AssertEqual(SamusLiquidMedium.Water,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "water below the occupied bottom pixel affects movement");
    AssertTrue(sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "palette liquid gate sees water above the bottom boundary");
    AssertTrue(!sample.LiquidPhysics.IsTopBoundarySubmerged(sample),
        "partially submerged body leaves top above water");

    sample.LiquidPhysics.ConfigureWater(surfaceY: 111);
    AssertEqual(SamusLiquidMedium.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "occupied bottom pixel equal to surface remains dry, matching native DEC");
    sample.LiquidPhysics.ConfigureWater(surfaceY: 112);
    AssertEqual(SamusLiquidMedium.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "liquid equality is not submerged");
    AssertTrue(!sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "palette liquid gate treats surface equality as dry");
    sample.LiquidPhysics.ConfigureWater(surfaceY: 110, liquidOptions: RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave);
    AssertEqual(SamusLiquidMedium.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "water option bit two disables physics");
    AssertTrue(!sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "palette liquid gate also honors disabled-water option bit");

    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 110);
    AssertEqual(SamusLiquidMedium.LavaOrAcid,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "negative general FX Y selects lava/acid surface");
    AssertTrue(sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "palette liquid gate falls through negative FX Y to lava/acid surface");
    sample.EquippedItems = SamusEquipmentFlags.GravitySuit;
    AssertEqual(SamusLiquidMedium.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "Gravity Suit bypasses liquid movement physics");
    AssertTrue(sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "raw palette boundary probe remains submerged before its Gravity exemption");

    // Normal and Hi-Jump launch tables are orthogonal to medium selection. Gravity Suit
    // forces the air entry even while the raw water surface still contains Samus's feet.
    sample.EquippedItems = 0;
    sample.LiquidPhysics.ConfigureWater(surfaceY: 110);
    SamusAerialMovement.InitializeJump(sample);
    AssertEqual(1, sample.Kinematics.YSpeed.Whole, "water normal-jump whole speed");
    AssertEqual(0xc000, sample.Kinematics.YSpeed.Fraction, "water normal-jump fraction");
    AssertEqual(0x0800, sample.Kinematics.YAcceleration.Fraction, "water gravity fraction");

    sample.EquippedItems = (SamusEquipmentFlags)0x0100;
    SamusAerialMovement.InitializeJump(sample);
    AssertEqual(2, sample.Kinematics.YSpeed.Whole, "water Hi-Jump whole speed");
    AssertEqual(0x8000, sample.Kinematics.YSpeed.Fraction, "water Hi-Jump fraction");

    sample.EquippedItems = (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots);
    SamusAerialMovement.InitializeJump(sample);
    AssertEqual(6, sample.Kinematics.YSpeed.Whole, "Gravity Suit forces air Hi-Jump entry");
    AssertEqual(0x1c00, sample.Kinematics.YAcceleration.Fraction,
        "Gravity Suit forces air acceleration");

    sample.EquippedItems = 0;
    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 110);
    SamusAerialMovement.InitializeJump(sample);
    AssertEqual(2, sample.Kinematics.YSpeed.Whole, "lava normal-jump whole speed");
    AssertEqual(0x0900, sample.Kinematics.YAcceleration.Fraction, "lava gravity fraction");

    var speed = sample.HorizontalSpeed;
    speed.SelectEnvironmentSpeedTable(SamusLiquidMedium.Air);
    AssertEqual(SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable,
        speed.ActiveSpeedTableBaseAddress, "air X table base");
    speed.SelectEnvironmentSpeedTable(SamusLiquidMedium.Water);
    AssertEqual(SamusMovementRomData.HorizontalMotion.WaterSpeedTable,
        speed.ActiveSpeedTableBaseAddress, "water X table base");
    speed.SelectEnvironmentSpeedTable(SamusLiquidMedium.LavaOrAcid);
    AssertEqual(SamusMovementRomData.HorizontalMotion.LavaAcidSpeedTable,
        speed.ActiveSpeedTableBaseAddress, "lava X table base");

    // Submersion reaches `$90:9808` before the running/Dash test. It cannot establish new
    // momentum, but a pre-existing momentum flag preserves the accumulated pair exactly.
    var submergedDash = new SamusHorizontalSpeedState();
    submergedDash.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: SnesButton.B,
        speedBoosterEquipped: false,
        liquidImpeded: true);
    AssertTrue(!submergedDash.HasRunningMomentum, "submerged Dash cannot establish momentum");
    AssertEqual(0, submergedDash.ExtraRunSpeed.Fraction, "submerged no-momentum Dash stays zero");
    submergedDash.HandleExtraRunSpeed(SamusMovementType.Running, SnesButton.B, false);
    int carriedFraction = submergedDash.ExtraRunSpeed.Fraction;
    submergedDash.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: SnesButton.B,
        speedBoosterEquipped: false,
        liquidImpeded: true);
    AssertTrue(submergedDash.HasRunningMomentum, "existing Dash momentum survives submersion");
    AssertEqual(carriedFraction, submergedDash.ExtraRunSpeed.Fraction,
        "submersion freezes rather than clears existing extra speed");

    // Pose-change animation samples Y+radius-1. Continuous FX animation samples the full
    // bottom boundary and updates remembered `$0AD2`; prove both edges independently.
    sample.EquippedItems = 0;
    sample.XSpeedDivisor = 7;
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111);
    AssertEqual(7, sample.LiquidPhysics.DeterminePoseChangeAnimationBuffer(sample),
        "pose-change surface equality uses speed divisor");
    sample.LiquidPhysics.ConfigureWater(surfaceY: 110);
    AssertEqual(3, sample.LiquidPhysics.DeterminePoseChangeAnimationBuffer(sample),
        "pose-change water delay below bottom-minus-one");
    sample.LiquidPhysics.PrepareAnimationFrame(sample, new CartridgeRandom());
    AssertEqual(3, sample.AnimationFrameBuffer, "continuous water animation delay");
    AssertEqual(SamusLiquidMedium.Water, sample.LiquidPhysics.LiquidPhysicsType,
        "continuous water animation remembers medium");
    sample.EquippedItems = SamusEquipmentFlags.GravitySuit;
    sample.LiquidPhysics.PrepareAnimationFrame(sample, new CartridgeRandom());
    AssertEqual(0, sample.AnimationFrameBuffer, "Gravity Suit cancels submerged frame delay");

    // Lava's FX handler performs the retail speed-boost cancellation before checking
    // Gravity Suit; acid enters the shared delay/damage tail without touching momentum.
    sample.EquippedItems = SamusEquipmentFlags.GravitySuit;
    sample.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: SnesButton.B,
        speedBoosterEquipped: false);
    sample.HorizontalSpeed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0401);
    sample.HorizontalSpeed.ExtraRunSpeed = new SnesUnsignedSixteenSixteen(3, 0x4000);
    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 110);
    sample.LiquidPhysics.PrepareAnimationFrame(sample, new CartridgeRandom());
    AssertTrue(!sample.HorizontalSpeed.HasRunningMomentum,
        "lava cancels momentum even with Gravity Suit");
    AssertEqual(0, sample.HorizontalSpeed.SpeedBoostCounter.RawValue,
        "lava clears speed-boost timer/counter");
    AssertEqual(0, sample.HorizontalSpeed.ExtraRunSpeed.Whole,
        "lava explicitly clears extra whole speed");
    AssertEqual(0, sample.HorizontalSpeed.ExtraRunSpeed.Fraction,
        "lava explicitly clears extra fractional speed");

    sample.EquippedItems = 0;
    sample.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: SnesButton.B,
        speedBoosterEquipped: false);
    sample.HorizontalSpeed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0201);
    sample.HorizontalSpeed.ExtraRunSpeed = sample.HorizontalSpeed.ExtraRunSpeed.WithWhole(1);
    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 110, acid: true);
    sample.LiquidPhysics.PrepareAnimationFrame(sample, new CartridgeRandom());
    AssertTrue(sample.HorizontalSpeed.HasRunningMomentum, "acid preserves running momentum");
    AssertEqual(0x0201, sample.HorizontalSpeed.SpeedBoostCounter.RawValue,
        "acid preserves speed-boost timer/counter");
    AssertEqual(1, sample.HorizontalSpeed.ExtraRunSpeed.Whole,
        "acid preserves extra run speed");

    // `$9B:C4BE` has its own intentionally narrow definition of grapple liquid physics.
    // It ignores option bit two, samples only general FX Y, and clears on release-function
    // entry so the just-finished swing retains its prior flag for exactly one handler call.
    sample.EquippedItems = 0;
    sample.Grapple.Phase = GrapplePhase.ConnectedSwinging;
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111, liquidOptions: RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave);
    SamusGrappleMovement.RefreshLiquidPhysicsFlag(sample);
    AssertTrue(sample.Grapple.Submerged, "grapple liquid flag ignores water option bit two");
    sample.Grapple.Phase = GrapplePhase.ReleaseFromSwing;
    SamusGrappleMovement.RefreshLiquidPhysicsFlag(sample);
    AssertTrue(!sample.Grapple.Submerged, "grapple release function clears liquid flag");

    // All three native standalone release records are identical. Mode two subtracts
    // the pinned fractional deceleration from 2.0000 regardless of the medium.
    ushort[] grappleReleaseDeceleration = [0x1000, 0x1000, 0x1000];
    SeedGrappleReleaseSpeedRecords(bus, grappleReleaseDeceleration);

    for (int medium = 0; medium < 3; medium++)
    {
        RoomLevelData releaseRoom = CreateEmptyRoom(new(16, 16));
        var released = new SamusState
        {
            Pose = SamusPoseId.NormalJumpForwardRightPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            Kinematics =
            {
                Radius = new(5, 12),
                YDirection = SamusVerticalDirection.Up,
                YSpeed = new SnesUnsignedSixteenSixteen(1, 0),
            },
        };
        WritePoseDefinition(bus, (int)released.Pose, [8, 2, 0xff, 0xff, 0, 0, 12, 0]);
        released.HorizontalSpeed.BaseSpeed = released.HorizontalSpeed.BaseSpeed.WithWhole(2);
        released.Grapple.ReleasedMovementActive = true;
        if (medium == (int)SamusLiquidMedium.Water)
            released.LiquidPhysics.ConfigureWater(surfaceY: 127);
        else if (medium == (int)SamusLiquidMedium.LavaOrAcid)
            released.LiquidPhysics.ConfigureLavaAcid(surfaceY: 127);
        SamusAerialMovement.ConfigureEnvironmentGravity(released);
        SamusAerialMovement.StepReleasedFromGrapple(

            releaseRoom,
            released,
            controllerInput: 0,
            nmiFrameCounter: 0);
        AssertEqual(unchecked((ushort)(0 - grappleReleaseDeceleration[medium])),
            released.HorizontalSpeed.BaseSpeed.Fraction,
            $"grapple release medium {medium} standalone X record");
        AssertEqual(1, released.HorizontalSpeed.BaseSpeed.Whole,
            $"grapple release medium {medium} borrow into whole X speed");
        AssertTrue(released.Grapple.ReleasedMovementActive,
            $"grapple release medium {medium} handler survives before apex/collision");
    }

    // #1269: a release that starts downward (zero angular velocity at the swing's bottom)
    // makes $90:E61B publish "falling" (2) without any collision. $90:94BF reads that
    // nonzero word and restores the normal handler after this single pass.
    var fallingRelease = new SamusState
    {
        Pose = SamusPoseId.NormalJumpForwardRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        Kinematics = { Radius = new(5, 12), YDirection = SamusVerticalDirection.Down },
    };
    fallingRelease.Grapple.ReleasedMovementActive = true;
    SamusAerialMovement.ConfigureEnvironmentGravity(fallingRelease);
    var fallingResult = SamusAerialMovement.StepReleasedFromGrapple(
CreateEmptyRoom(new(16, 16)), fallingRelease, controllerInput: 0, nmiFrameCounter: 0);
    AssertEqual(SamusVerticalCollisionResult.Falling, fallingRelease.SolidVerticalCollisionResult,
        "downward release publishes the falling result");
    AssertTrue(!fallingRelease.Grapple.ReleasedMovementActive,
        "a falling release restores the normal handler after one pass");

    // Give the spin routine authentic metadata plus zero horizontal records in all three
    // tables. Water's remembered medium lowers only the inclusive minimum from $0280 to $0080.
    SeedSpaceJumpSpinPoseAndZeroSpinSpeeds(bus);
    RoomLevelData empty = CreateEmptyRoom(new(16, 16));

    var partialWaterSpaceJump = new SamusState
    {
        Pose = SamusPoseId.SpaceJumpRightPose,
        EquippedItems = (SamusEquipmentFlags)0x0200,
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        Kinematics =
        {
            Radius = new(5, 12),
            YDirection = SamusVerticalDirection.Down,
            YSpeed = new SnesUnsignedSixteenSixteen(0, 0x8000),
        },
    };
    partialWaterSpaceJump.LiquidPhysics.ConfigureWater(surfaceY: 128);
    partialWaterSpaceJump.LiquidPhysics.InitializeRememberedMedium(partialWaterSpaceJump);
    SamusAerialMovement.ConfigureEnvironmentGravity(partialWaterSpaceJump);
    SamusAerialMovement.StepSpinJump(

        empty,
        partialWaterSpaceJump,
        SnesButton.A,
        nmiFrameCounter: 0,
        controllerNewInput: SnesButton.A);
    AssertEqual(SamusVerticalDirection.Up, partialWaterSpaceJump.Kinematics.YDirection,
        "partially submerged Space Jump accepts water minimum $0080");
    AssertEqual(1, partialWaterSpaceJump.Kinematics.YSpeed.Whole,
        "water Space Jump reloads water launch whole speed");

    var fullySubmergedScrew = new SamusState
    {
        Pose = SamusPoseId.ScrewAttackRightPose,
        EquippedItems = (SamusEquipmentFlags)0x0208,
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        Kinematics =
        {
            Radius = new(5, 12),
            YDirection = SamusVerticalDirection.Down,
            YSpeed = new SnesUnsignedSixteenSixteen(3, 0),
        },
    };
    WritePoseDefinition(bus, (int)SamusPoseId.ScrewAttackRightPose,
        [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    fullySubmergedScrew.LiquidPhysics.ConfigureWater(surfaceY: 100);
    fullySubmergedScrew.LiquidPhysics.InitializeRememberedMedium(fullySubmergedScrew);
    SamusAerialMovement.ConfigureEnvironmentGravity(fullySubmergedScrew);
    SamusAerialMovement.StepSpinJump(

        empty,
        fullySubmergedScrew,
        SnesButton.A,
        nmiFrameCounter: 0,
        controllerNewInput: SnesButton.A);
    AssertEqual(SamusVerticalDirection.Down, fullySubmergedScrew.Kinematics.YDirection,
        "fully submerged non-Gravity Space Jump cannot restart");
    AssertEqual(SamusContactDamageIndex.None, fullySubmergedScrew.HorizontalSpeed.ContactDamageIndex,
        "fully submerged Screw Attack does not publish contact damage");

    Console.WriteLine(
        "  Samus liquids: boundaries, palette probe, ROM tables, gravity, Dash/lava cancellation, grapple, animation, Space Jump, and Gravity Suit agree.");
}

/// <summary>
/// Checks the producer, packed-slot interpreter, OAM path, sound publications, and periodic
/// fixed-point damage translated from `$90:8000-$82DB/$8A4C-$8C1E/$A3E5/$E9CE`.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
internal static void VerifySamusAtmosphericEffects()
{
    TestAddressSpace bus = CreateSamusAtmosphericEffectsCartridge();
    var atmosphericArt = new SamusAtmosphericArtworkCatalog(
        new ushort[SamusMovementRomData.Environment.DirectAtmosphericFrameCount],
        [0x2a48, 0x2a49, 0, 0]);

    var samus = new SamusState
    {
        Pose = SamusPoseId.MovingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
        // This atmosphere fixture deliberately uses a twelve-pixel test body.
        // Authored collision radii are compiled now; do not fake a ROM override
        // for running pose $09, whose actual native radius is twenty-one.
        Kinematics = { Radius = new(5, 12) },
    };
    samus.Presentation.InitializeAnimation();

    // Movement type one is a diving splash according to the compiled `$81A4` policy. Keep NMI
    // away from the 128-frame bubble cadence so entry has exactly one sound request.
    samus.LiquidPhysics.ConfigureWater(surfaceY: 110);
    samus.LiquidPhysics.PrepareAnimationFrame(samus, new CartridgeRandom(), nmiFrameCounter: 1);
    SamusAtmosphericEffectSlot entrySplash = samus.LiquidPhysics.AtmosphericEffects.Slots[0];
    AssertEqual(SamusAtmosphericEffectType.DivingSplash, entrySplash.Type, "water entry selects diving-splash type");
    AssertEqual(2, entrySplash.AnimationTimer.RawValue, "diving splash initial timer");
    AssertEqual(100, entrySplash.Position.X, "diving splash Samus X");
    AssertEqual(110, entrySplash.Position.Y, "diving splash surface Y");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0d), 6),
        samus.LiquidPhysics.SoundRequests.Single(), "water-entry library-two sound");

    // Leaving water produces sound `$0E` and replaces the slot through the same movement-
    // type table. A spin/wall-jump-only `$30` request is correctly absent for type one.
    samus.LiquidPhysics.ConfigureWater(surfaceY: 112);
    samus.LiquidPhysics.PrepareAnimationFrame(samus, new CartridgeRandom(), nmiFrameCounter: 2);
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0e), 6),
        samus.LiquidPhysics.SoundRequests.Single(), "water-exit library-two sound");

    // Bubble admission samples top-24, every 128th accepted NMI, and slot two availability.
    // Re-enter on frame one first so the cadence call below is an already-submerged pass.
    samus.LiquidPhysics.ConfigureWater(surfaceY: 50);
    samus.LiquidPhysics.PrepareAnimationFrame(samus, new CartridgeRandom(), nmiFrameCounter: 1);
    // The power-on word, $0061.
    var bubbleRandom = new CartridgeRandom();
    samus.LiquidPhysics.PrepareAnimationFrame(samus, bubbleRandom, nmiFrameCounter: 128);
    SamusAtmosphericEffectSlot bubble = samus.LiquidPhysics.AtmosphericEffects.Slots[2];
    AssertEqual(SamusAtmosphericEffectType.Bubbles, bubble.Type, "128-frame submerged cadence creates bubbles");
    AssertEqual(94, bubble.Position.Y, "bubble origin is Samus top plus six");
    AssertTrue(samus.LiquidPhysics.SoundRequests.Any(request =>
        request.SoundEffect.Library == SoundEffectLibrary.Library2 &&
        request.SoundEffect.Value is 0x0f or 0x11),
        "bubble RNG publishes one of the two native sounds");

    // Surface spray uses four type-four slots and exact asymmetric X positions. A half-unit
    // lava rate then borrows from fractional health on the same `$E9CE` consumer call.
    samus.LiquidPhysics.ConfigureLavaAcid(surfaceY: 110);
    samus.Health = 99;
    samus.SubunitHealth = 0;
    samus.EquippedItems = 0;
    samus.LiquidPhysics.PrepareAnimationFrame(samus, new CartridgeRandom(), nmiFrameCounter: 1);
    AssertTrue(samus.LiquidPhysics.AtmosphericEffects.Slots.All(slot => slot.Type == SamusAtmosphericEffectType.LavaSurfaceDamage),
        "partial lava submersion fills four surface-spray slots");
    AssertEqual(0x8000, samus.LiquidPhysics.PeriodicSubDamage,
        "lava fractional damage accumulates from ROM");
    samus.LiquidPhysics.ApplyPeriodicDamage(samus, timeIsFrozen: false);
    AssertEqual(98, samus.Health, "fractional lava subtraction borrows one energy");
    AssertEqual(0x8000, samus.SubunitHealth, "fractional energy retains wrapped half");
    AssertEqual(0, samus.LiquidPhysics.PeriodicSubDamage,
        "periodic consumer clears fractional accumulator");
    VerifyPeriodicDamageFractionalBorrow();

    // Type-four slot zero begins with timer two. First update decrements/draws frame zero and
    // applies +1/-1 motion before clipping. The next call reloads ROM timer zero, advances to
    // frame one, and consumes its distinct `$2A49` attribute word.
    var effects = new SamusAtmosphericEffectsState();
    effects.SetSlot(0, type: 4, animationFrame: 0, animationTimer: 2, anchor: new(104, 104));
    var oamFrame = new SpriteFrameBuffer();
    oamFrame.BeginFrame();
    OamBuffer oam;
    effects.UpdateAndDraw(oamFrame, camera: new(0, 0), fxYPosition: 0,
        directArtwork: atmosphericArt, standardObjects: SpriteFrameTestArtwork.StandardObjects);
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(1, effects.Slots[0].AnimationTimer.RawValue, "atmospheric positive timer decrements");
    AssertEqual(105, effects.Slots[0].Position.X, "lava slot zero drifts right");
    AssertEqual(103, effects.Slots[0].Position.Y, "lava spray rises one pixel");
    AssertEqual(0x048, oam.GetEntry(0).TileNumber, "lava frame zero direct OAM tile");
    AssertEqual(5, oam.GetEntry(0).Palette, "lava direct OAM retains ROM palette");

    oamFrame.BeginFrame();
    effects.UpdateAndDraw(oamFrame, camera: new(0, 0), fxYPosition: 0,
        directArtwork: atmosphericArt, standardObjects: SpriteFrameTestArtwork.StandardObjects);
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(1, effects.Slots[0].AnimationFrame,
        "timer zero reloads old frame then advances packed word");
    AssertEqual(2, effects.Slots[0].AnimationTimer.RawValue,
        "frame advance reloads literal ROM duration");
    AssertEqual(0x049, oam.GetEntry(0).TileNumber, "lava frame one direct OAM tile");

    // `$8002` is not a large positive delay: one call reaches `$8001` and does nothing;
    // the next reaches `$8000`, reloads the current ROM duration, moves, and draws.
    effects.Clear();
    effects.SetSlot(1, type: 4, animationFrame: 0, animationTimer: 0x8002, anchor: new(100, 100));
    oamFrame.BeginFrame();
    effects.UpdateAndDraw(oamFrame, new(0, 0), 0, directArtwork: atmosphericArt, standardObjects: SpriteFrameTestArtwork.StandardObjects);
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(100, effects.Slots[1].Position.X, "$8001 suppresses atmospheric motion");
    AssertEqual(0, (oam.LastFinalizedSpriteCount * 4), "$8001 suppresses atmospheric drawing");
    effects.UpdateAndDraw(oamFrame, new(0, 0), 0, directArtwork: atmosphericArt, standardObjects: SpriteFrameTestArtwork.StandardObjects);
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(101, effects.Slots[1].Position.X, "$8000 reload call begins motion");
    AssertEqual(4, (oam.LastFinalizedSpriteCount * 4), "$8000 reload call emits one OAM record");

    // Running frame two with stage four produces dry-room type-seven dust. The first slot
    // is delayed and the second immediate, exactly like `$90:EE64-$EEE3`.
    var runner = new SamusState
    {
        Pose = SamusPoseId.MovingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
    };
    runner.PoseMetadata.RefreshCollisionRadii();
    runner.Presentation.InitializeAnimation(initialFrame: 2);
    // The native foot-contact gate checks the final tick of this frame. The
    // compiled running stream initializes frame two at two ticks, so stage its
    // final tick explicitly instead of relying on the old synthetic ROM delay.
    runner.Presentation.SetAnimationFrameFromSpecialHandler(frame: 2, timer: 1);
    runner.HorizontalSpeed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0400);
    runner.LiquidPhysics.PrepareAnimationFrame(runner, new CartridgeRandom(), nmiFrameCounter: 1);
    AssertEqual(SamusAtmosphericEffectType.AlternateDust, runner.LiquidPhysics.AtmosphericEffects.Slots[0].Type,
        "boost-stage-four foot contact creates dust");
    AssertEqual(0x8002,
        runner.LiquidPhysics.AtmosphericEffects.Slots[0].AnimationTimer.RawValue,
        "first foot effect uses delayed-start sentinel");
    AssertEqual(3,
        runner.LiquidPhysics.AtmosphericEffects.Slots[1].AnimationTimer.RawValue,
        "second foot effect starts immediately");

    // $90:A40E reads $0A68 (stored-shine timer), not the boost palette timer
    // at $0AD0. Boost buildup must still publish footsteps into the real queue.
    runner.HorizontalSpeed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0301);
    runner.HorizontalSpeed.SpecialPaletteTimer = NativeCountdown.Start(1);
    runner.LiquidPhysics.PrepareAnimationFrame(runner, new CartridgeRandom(), nmiFrameCounter: 1);
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 6), 6),
        runner.LiquidPhysics.SoundRequests.Single(), "boost palette timer does not suppress footsteps");
    // #1386: $90:A409 reads the live boss ID; the same footstep in a boss room queues no sound.
    runner.LiquidPhysics.PrepareAnimationFrame(runner, new CartridgeRandom(), nmiFrameCounter: 1, bossPresent: true);
    AssertEqual(0, runner.LiquidPhysics.SoundRequests.Count, "a nonzero boss ID suppresses footsteps");
    AssertTrue(runner.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(0x0400)), "seed stored shine sound gate");
    runner.HorizontalSpeed.SpecialPaletteTimer = NativeCountdown.Start(0);
    runner.LiquidPhysics.PrepareAnimationFrame(runner, new CartridgeRandom(), nmiFrameCounter: 1);
    AssertEqual(0, runner.LiquidPhysics.SoundRequests.Count, "stored shine timer suppresses footsteps");

    // `$91:F046` runs before animation and before landing clears Y velocity. Norfair selects
    // type-six dust in slots two/three; spin termination precedes the soft-impact request in
    // the native sound queues. The source pose is deliberately ordinary spin rather than a
    // landing pose because `$0A20` has not been replaced yet at this collision seam.
    var landing = new SamusState
    {
        Pose = SamusPoseId.SpinJumpRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
    };
    WritePoseDefinition(bus, 0x19, [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    landing.PoseMetadata.RefreshCollisionRadii();
    landing.Kinematics.YSpeed = new SnesUnsignedSixteenSixteen(4, 1);
    landing.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Norfair, 0);
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        landing, previousMovementType: SamusMovementType.SpinJumping, previousPose: SamusPoseId.SpinJumpRightPose,
        landing.Kinematics.YSpeed);
    AssertEqual(2, landing.LiquidPhysics.SoundRequests.Count,
        "spin landing publishes termination and impact sounds");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x32), 6),
        landing.LiquidPhysics.SoundRequests[0], "ordinary spin termination sound");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x05), 6),
        landing.LiquidPhysics.SoundRequests[1], "sub-five soft landing sound");
    AssertEqual(SamusAtmosphericEffectType.Dust, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "Norfair landing creates right dust slot");
    AssertEqual(SamusAtmosphericEffectType.Dust, landing.LiquidPhysics.AtmosphericEffects.Slots[3].Type,
        "Norfair landing creates left dust slot");
    AssertEqual(108, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Position.X,
        "landing dust right X offset");
    AssertEqual(92, landing.LiquidPhysics.AtmosphericEffects.Slots[3].Position.X,
        "landing dust left X offset");
    AssertEqual(111, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Position.Y,
        "landing dust uses pose-defined inclusive bottom pixel");

    // Whole speed five changes only the impact sound to hard `$04`; Screw Attack changes
    // only the preceding library-one termination to `$34`. A cinematic suppresses both
    // sounds but Norfair's graphics handler still creates dust, exactly as the separate
    // native checks dictate.
    landing.Kinematics.YSpeed = landing.Kinematics.YSpeed.WithWhole(5);
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        landing, previousMovementType: SamusMovementType.WallJumping, previousPose: SamusPoseId.ScrewAttackRightPose,
        landing.Kinematics.YSpeed);
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x34), 6),
        landing.LiquidPhysics.SoundRequests[0], "Screw Attack termination sound");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x04), 6),
        landing.LiquidPhysics.SoundRequests[1], "speed-five hard landing sound");
    // #1389: `$91:F07D` compares signed, so a whole speed of $8005 or more is below five; a sub-five
    // whole speed with no sub-speed returns before sound and graphics, as zero does.
    foreach (ushort whole in new ushort[] { 2, 0x8005 })
    {
        landing.LiquidPhysics.BeginFrameSoundRequests();
        landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
            landing, previousMovementType: SamusMovementType.Falling, previousPose: SamusPoseId.FallingRightPose,
            new SnesUnsignedSixteenSixteen(whole, 0));
        AssertEqual(0, landing.LiquidPhysics.SoundRequests.Count, $"whole speed ${whole:X4} with no sub-speed lands silently");
    }
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        landing, previousMovementType: SamusMovementType.Falling, previousPose: SamusPoseId.FallingRightPose,
        new SnesUnsignedSixteenSixteen(0x8005, 1));
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x05), 6),
        landing.LiquidPhysics.SoundRequests.Single(), "whole speed $8005 is below five as a signed word");
    landing.LiquidPhysics.CinematicFunctionActive = true;
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        landing, previousMovementType: SamusMovementType.SpinJumping, previousPose: SamusPoseId.SpinJumpRightPose,
        landing.Kinematics.YSpeed);
    AssertEqual(0, landing.LiquidPhysics.SoundRequests.Count,
        "cinematic landing suppresses both sound libraries");
    AssertEqual(SamusAtmosphericEffectType.Dust, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "Norfair cinematic still dispatches landing dust");
    landing.LiquidPhysics.CinematicFunctionActive = false;

    landing.LiquidPhysics.ConfigureWater(surfaceY: 111);
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        landing, previousMovementType: SamusMovementType.Falling, previousPose: SamusPoseId.FallingRightPose,
        landing.Kinematics.YSpeed);
    AssertEqual(SamusAtmosphericEffectType.Dust, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "liquid exactly at the inclusive bottom does not suppress landing dust");

    // Active liquid returns without touching the landing slots. This is intentionally not
    // deletion: seed an unrelated type-seven record and prove its packed word survives.
    landing.LiquidPhysics.AtmosphericEffects.SetSlot(
        2, type: 7, animationFrame: 2, animationTimer: 9, anchor: new(77, 88));
    landing.LiquidPhysics.ConfigureWater(surfaceY: 110);
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        landing, previousMovementType: SamusMovementType.Falling, previousPose: SamusPoseId.FallingRightPose,
        landing.Kinematics.YSpeed);
    AssertEqual(SamusAtmosphericEffectType.AlternateDust, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "submerged landing leaves prior atmospheric slot untouched");
    AssertEqual(9, landing.LiquidPhysics.AtmosphericEffects.Slots[2].AnimationTimer.RawValue,
        "submerged return does not clear stale timer");

    // Ceres/debug deletion writes only frame/type. Position and timer are observable stale
    // WRAM. Zero vertical speed returns even earlier and therefore does not delete anything.
    landing.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Ceres, 0);
    landing.LiquidPhysics.FxYPosition = ushort.MaxValue;
    landing.Kinematics.YSpeed = landing.Kinematics.YSpeed.WithWhole(1);
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        landing, previousMovementType: SamusMovementType.Falling, previousPose: SamusPoseId.FallingRightPose,
        landing.Kinematics.YSpeed);
    AssertEqual(0, landing.LiquidPhysics.AtmosphericEffects.Slots[2].FrameAndType,
        "Ceres deletes landing packed word");
    AssertEqual(9, landing.LiquidPhysics.AtmosphericEffects.Slots[2].AnimationTimer.RawValue,
        "Ceres delete preserves atmospheric timer");
    AssertEqual(77, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Position.X,
        "Ceres delete preserves atmospheric X");
    landing.LiquidPhysics.AtmosphericEffects.SetSlot(
        2, type: 7, animationFrame: 0, animationTimer: 3, anchor: new(1, 2));
    landing.Kinematics.YSpeed = default;
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        landing, previousMovementType: SamusMovementType.Falling, previousPose: SamusPoseId.FallingRightPose,
        landing.Kinematics.YSpeed);
    AssertEqual(SamusAtmosphericEffectType.AlternateDust, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "zero-speed grounding returns before graphics dispatch");

    // Crateria reads its literal inline room byte. Landing Site flag one requires exact FX
    // type `$000A`; with a dry surface it creates type-one splashes at +4/-3 and bottom-4.
    SeedLandingSiteInlineRoomByte(bus);
    landing.LiquidPhysics.RoomIdentity = RoomIdentities.LandingSite;
    landing.LiquidPhysics.FxType = RoomFxType.Rain;
    landing.LiquidPhysics.FxYPosition = ushort.MaxValue;
    landing.Kinematics.YSpeed = landing.Kinematics.YSpeed.WithFraction(1);
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        landing, previousMovementType: SamusMovementType.Falling, previousPose: SamusPoseId.FallingRightPose,
        landing.Kinematics.YSpeed);
    AssertEqual(SamusAtmosphericEffectType.FootstepSplash, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "Landing Site type-A FX selects splash");
    AssertEqual(104, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Position.X,
        "landing splash right X offset");
    AssertEqual(97, landing.LiquidPhysics.AtmosphericEffects.Slots[3].Position.X,
        "landing splash left X offset");
    AssertEqual(107, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Position.Y,
        "landing splash rises four pixels above feet");

    Console.WriteLine(
        "  Samus atmosphere: liquid FX, footsteps, landing impact, packed timers, sound, and OAM agree.");
}

// Verifies the cartridge's complete ten-way jump/fall turn selectors and the block-only
// wall-jump route. Every expectation below is a literal bank-$90/$91 table value; the test
// intentionally does not calculate a mirrored target from facing.

/// <summary>Synthetic cartridge with the aerial route's pose records, animation streams and dry-air jump words.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for Samus aerial movement poses and jump words")]
static TestAddressSpace CreateSamusAerialMovementCartridge()
{
    var bus = new TestAddressSpace();

    // Minimal literal pose records for the verified right-facing route. Byte one is the
    // movement dispatcher index, byte four is the signed graphics offset, and byte six is
    // the collision radius. All values mirror the retail definitions.
    bus.WriteBytes(0x91b881, [0x08, 0x02, 0xff, 0x02, 0x03, 0x00, 0x13, 0x00]); // $4B
    bus.WriteBytes(0x91b889, [0x04, 0x02, 0xff, 0x07, 0x03, 0x00, 0x13, 0x00]); // $4C
    bus.WriteBytes(0x91b891, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $4D
    bus.WriteBytes(0x91bb49, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]); // $A4
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01

    // Animation streams are byte-indexed. $4B spends one frame as a transition then FD
    // publishes $4D. $4D's first two frames support the rising-frame hold assertion below.
    // $A4 reaches F8 and publishes standing-right pose $01.
    WriteTestWord(bus, 0x91b0a6, 0xc100); // pose $4B pointer
    WriteTestWord(bus, 0x91b0a8, 0xc108); // pose $4C pointer
    WriteTestWord(bus, 0x91b0aa, 0xc110); // pose $4D pointer
    WriteTestWord(bus, 0x91b158, 0xc120); // pose $A4 pointer
    WriteTestWord(bus, 0x91b012, 0xc130); // pose $01 pointer
    bus.WriteBytes(0x91c100, [0x01, 0xfd, 0x4d]);
    bus.WriteBytes(0x91c108, [0x01, 0xfd, 0x4e]);
    bus.WriteBytes(0x91c110, [0x02, 0x03, 0x03, 0x03, 0x03, 0x50, 0xfe, 0x01]);
    bus.WriteBytes(0x91c120, [0x04, 0x02, 0xf8, 0x01]);
    bus.WriteBytes(0x91c130, [0x0a]);

    // Reference words use the pinned NTSC revision; production now compiles these definitions.
    WriteTestWord(bus, 0x909eb9, 0x0004);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x1c00);
    WriteTestWord(bus, 0x909ea7, 0x0000);
    return bus;
}

/// <summary>Seeds left-facing spin-jump poses and animation streams in the fixture bus.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the spin-jump $19/$1A pose records and streams")]
static void SeedLeftSpinJumpPoses(TestAddressSpace bus)
{
    bus.WriteBytes(0x91b6f1, [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]); // $19
    bus.WriteBytes(0x91b6f9, [0x04, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]); // $1A
    WriteTestWord(bus, 0x91b042, 0xc138);
    WriteTestWord(bus, 0x91b044, 0xc140);
    bus.WriteBytes(0x91c138, [0x03, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03]);
    bus.WriteBytes(0x91c140, [0x03, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0xfe, 0x08]);
}

/// <summary>Seeds ledge-fall pose records, animations, and the type-six speed entry.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the ledge-fall poses, streams and type-six speed entry")]
static void SeedLedgeFallPosesAndTypeSixSpeed(TestAddressSpace bus)
{
    bus.WriteBytes(0x91b779, [0x04, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]); // $2A
    bus.WriteBytes(0x91bb51, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]); // $A5
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]); // $02
    WriteTestWord(bus, 0x91b064, 0xc150);
    WriteTestWord(bus, 0x91b15a, 0xc160);
    WriteTestWord(bus, 0x91b014, 0xc170);
    bus.WriteBytes(0x91c150, [0x05, 0x04, 0x04, 0xfe, 0x01, 0x06, 0x10, 0xfe, 0x01]);
    bus.WriteBytes(0x91c160, [0x04, 0x02, 0xf8, 0x02]);
    bus.WriteBytes(0x91c170, [0x0a]);
    WriteTestWord(bus, 0x909f9d, 0x0000); // type-six entry at $9F55 + 6*12
    WriteTestWord(bus, 0x909f9f, 0x1000);
    WriteTestWord(bus, 0x909fa1, 0x0001);
    WriteTestWord(bus, 0x909fa3, 0x0000);
    WriteTestWord(bus, 0x909fa5, 0x0000);
    WriteTestWord(bus, 0x909fa7, 0x1000);
}

/// <summary>Synthetic cartridge with the spin, running and landing pose records, their streams, and dry-air words.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for Space Jump and Screw Attack poses")]
static TestAddressSpace CreateSpaceJumpAndScrewAttackCartridge()
{
    var bus = new TestAddressSpace();

    // All six stable spin records share movement type three and radius twelve. The generic
    // `$19/$1A` records are transition-table outputs; the four equipment records are the
    // actual bodies selected by `$91:F624` after that table lookup.
    foreach ((SamusPoseId pose, byte direction) in new (SamusPoseId, byte)[]
    {
        (SamusPoseId.SpinJumpRightPose, 8),
        (SamusPoseId.SpinJumpLeftPose, 4),
        (SamusPoseId.SpaceJumpRightPose, 8),
        (SamusPoseId.SpaceJumpLeftPose, 4),
        (SamusPoseId.ScrewAttackRightPose, 8),
        (SamusPoseId.ScrewAttackLeftPose, 4),
    })
    {
        WritePoseDefinition(bus, (int)pose, [direction, 3, 0xff, 0xff, 0, 0, 12, 0]);
        ushort stream = unchecked((ushort)(0xc000 + (int)pose * 0x20));
        WriteTestWord(bus, 0x91b010 + (int)pose * 2, stream);
        for (int frame = 0; frame < 31; frame++)
            WriteTestByte(bus, 0x910000 | unchecked((ushort)(stream + frame)), 4);
    }

    // Running sources and spin-landing endpoints are sufficient to prove equipment
    // substitution and Screw palette restoration without mocking pose metadata in code.
    WritePoseDefinition(bus, (int)SamusPoseId.MovingRightNormalPose,
        [8, 1, 0xff, 2, 0, 0, 21, 0]);
    WritePoseDefinition(bus, (int)SamusPoseId.MovingLeftNormalPose,
        [4, 1, 0xff, 7, 0, 0, 21, 0]);
    WritePoseDefinition(bus, (int)SamusPoseId.SpinLandingRightPose,
        [8, 0, 0xff, 2, 0, 0, 21, 0]);
    WritePoseDefinition(bus, (int)SamusPoseId.SpinLandingLeftPose,
        [4, 0, 0xff, 7, 0, 0, 21, 0]);
    WritePoseDefinition(bus, (int)SamusPoseId.NormalJumpGunExtendedRightPose,
        [8, 2, 0xff, 2, 0, 0, 19, 0]);
    WritePoseDefinition(bus, (int)SamusPoseId.WallJumpRightPose,
        [8, 0x14, (byte)SamusPoseId.SpinJumpRightPose, 0xff, 8, 0, 19, 0]);
    WritePoseDefinition(bus, (int)SamusPoseId.NormalJumpAimDownLeftPose,
        [4, 2, 0xff, 5, 0, 0, 10, 0]);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.SpinLandingRightPose * 2, 0xc800);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.SpinLandingLeftPose * 2, 0xc810);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.NormalJumpGunExtendedRightPose * 2, 0xc820);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.WallJumpRightPose * 2, 0xc828);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.NormalJumpAimDownLeftPose * 2, 0xc830);
    WriteTestByte(bus, 0x91c800, 4);
    WriteTestByte(bus, 0x91c810, 4);
    WriteTestByte(bus, 0x91c820, 4);
    WriteTestByte(bus, 0x91c828, 4);
    WriteTestByte(bus, 0x91c830, 4);

    // Dry-air Samus_InitJump and gravity words. The type-three horizontal record is zeroed
    // intentionally so the assertions isolate the vertical 8.8 gate from X acceleration.
    WriteTestWord(bus, 0x909eb9, 4);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0);
    for (int byteOffset = 0; byteOffset < SpeedTableEntryConstants.ByteCount; byteOffset += 2)
        WriteTestWord(bus, 0x909f79 + byteOffset, 0);
    return bus;
}

/// <summary>Synthetic cartridge with the three media's adjacent jump/gravity word tables read by `$90:98BC/$90:9C5B`.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for Samus liquid jump and gravity tables")]
static TestAddressSpace CreateSamusLiquidPhysicsCartridge()
{
    var bus = new TestAddressSpace();

    // Seed exactly the adjacent word tables read by `$90:98BC/$90:9C5B`. Distinct values
    // make a wrong byte offset or accidental host constant immediately observable.
    ushort[] launchWhole = [4, 1, 2];
    ushort[] launchFraction = [0xe000, 0xc000, 0xc000];
    ushort[] hiWhole = [6, 2, 3];
    ushort[] hiFraction = [0, 0x8000, 0x8000];
    ushort[] gravityFraction = [0x1c00, 0x0800, 0x0900];
    for (int medium = 0; medium < 3; medium++)
    {
        WriteTestWord(bus, 0x909eb9 + medium * 2, launchWhole[medium]);
        WriteTestWord(bus, 0x909ebf + medium * 2, launchFraction[medium]);
        WriteTestWord(bus, 0x909ec5 + medium * 2, hiWhole[medium]);
        WriteTestWord(bus, 0x909ecb + medium * 2, hiFraction[medium]);
        WriteTestWord(bus, 0x909ea1 + medium * 2, gravityFraction[medium]);
        WriteTestWord(bus, 0x909ea7 + medium * 2, 0);
    }
    return bus;
}

/// <summary>Seeds the Space Jump spin pose and zeroed spin speed records.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Space Jump spin pose and zeroed spin speed records")]
static void SeedSpaceJumpSpinPoseAndZeroSpinSpeeds(TestAddressSpace bus)
{
    WritePoseDefinition(bus, (int)SamusPoseId.SpaceJumpRightPose,
        [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.SpaceJumpRightPose * 2, 0xc000);
    for (int frame = 0; frame < 32; frame++)
        WriteTestByte(bus, 0x91c000 + frame, 4);
    foreach (int tableBase in new[] { 0x909f55, 0x90a08d, 0x90a1dd })
    {
        int spinEntry = tableBase + 3 * SpeedTableEntryConstants.ByteCount;
        for (int byteOffset = 0; byteOffset < SpeedTableEntryConstants.ByteCount; byteOffset += 2)
            WriteTestWord(bus, spinEntry + byteOffset, 0);
    }
}

/// <summary>Synthetic cartridge with running pose $09 and the bank-$90 atmospheric records exercised by the effects test.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for Samus atmospheric effects")]
static TestAddressSpace CreateSamusAtmosphericEffectsCartridge()
{
    var bus = new TestAddressSpace();

    // Use native running pose `$09` for the movement type and X direction used by
    // the producer branches. Its delay metadata also lets the footstep fixture land exactly
    // on running frame two with timer one, matching `$90:A3EE-$A401`.
    WritePoseDefinition(bus, (int)SamusPoseId.MovingRightNormalPose,
        [8, 1, 0xff, 0, 0, 0, 12, 0]);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.MovingRightNormalPose * 2, 0xc000);
    bus.WriteBytes(0x91c000, [1, 1, 1, 1]);

    // Seed only the bank-$90 table records exercised below. Distinct attributes and timers
    // prove the interpreter follows ROM pointers rather than a duplicated C# animation list.
    WriteTestWord(bus, 0x908b93 + 4 * 2, 0x9000);
    WriteTestWord(bus, 0x909000, 2);
    WriteTestWord(bus, 0x909002, 3);
    WriteTestWord(bus, 0x908bef + 4 * 2, 4);
    WriteTestWord(bus, 0x908bff + 4 * 2, 0x9100);
    WriteTestWord(bus, 0x909100, 0x2a48);
    WriteTestWord(bus, 0x909102, 0x2a49);
    return bus;
}

/// <summary>Crateria's literal inline Landing Site flag byte at $91:F0F3.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Landing Site inline room flag")]
static void SeedLandingSiteInlineRoomByte(TestAddressSpace bus)
{
    WriteTestByte(bus, 0x91f0f3, 1);
}

    /// <summary>Runs the Space Jump and Screw Attack fixture directly.</summary>
    [VerificationCommand("--space-screw-fixture")]
    internal static int SpaceScrewFixtureCommand()
    {
     VerifySamusSpaceJumpAndScrewAttack(); return 0; 
    }

    /// <summary>Checks liquid drag and buoyancy in Samus's movement fixture.</summary>
    [VerificationCommand("--samus-liquid-physics")]
    internal static int SamusLiquidPhysicsCommand()
    {
     VerifySamusLiquidPhysics(); return 0; 
    }

    /// <summary>Runs the aerial movement and jump-pose fixture.</summary>
    [VerificationCommand("--samus-aerial-movement")]
    internal static int SamusAerialMovementCommand()
    {
        Suite(nameof(VerifySamusAerialMovement), () => VerifySamusAerialMovement());
        return 0;
    }

    /// <summary>
    /// Confirms the periodic-damage consumer's fractional-energy borrow through <c>ApplyPeriodicDamage</c>: a fraction
    /// that covers the damage is reduced without a borrow, one that does not wraps to the remainder and costs one
    /// more whole unit, and a borrow or whole damage that would take energy below zero clamps energy and its
    /// fraction to zero.
    /// </summary>
    private static void VerifyPeriodicDamageFractionalBorrow()
    {
        const int QuarterUnit = 16384;
        const int HalfUnit = 32768;
        const int ThreeQuarterUnits = 49152;
        var samus = new SamusState { EquippedItems = 0 };

        samus.Health = 50;
        samus.SubunitHealth = ThreeQuarterUnits;
        samus.LiquidPhysics.AccumulatePeriodicDamage(HalfUnit, 1);
        samus.LiquidPhysics.ApplyPeriodicDamage(samus, timeIsFrozen: false);
        AssertEqual(49, samus.Health, "a covered fraction takes only the whole damage");
        AssertEqual(QuarterUnit, samus.SubunitHealth, "a covered fraction keeps its remainder without borrowing");

        samus.Health = 50;
        samus.SubunitHealth = QuarterUnit;
        samus.LiquidPhysics.AccumulatePeriodicDamage(HalfUnit, 1);
        samus.LiquidPhysics.ApplyPeriodicDamage(samus, timeIsFrozen: false);
        AssertEqual(48, samus.Health, "an uncovered fraction borrows one more whole unit");
        AssertEqual(ThreeQuarterUnits, samus.SubunitHealth, "an uncovered fraction wraps to the remainder");

        samus.Health = 0;
        samus.SubunitHealth = 0;
        samus.LiquidPhysics.AccumulatePeriodicDamage(HalfUnit, 0);
        samus.LiquidPhysics.ApplyPeriodicDamage(samus, timeIsFrozen: false);
        AssertEqual(0, samus.Health, "a borrow from zero energy clamps energy to zero");
        AssertEqual(0, samus.SubunitHealth, "a fatal borrow clears the fraction instead of wrapping it");

        samus.Health = 1;
        samus.SubunitHealth = HalfUnit;
        samus.LiquidPhysics.AccumulatePeriodicDamage(0, 2);
        samus.LiquidPhysics.ApplyPeriodicDamage(samus, timeIsFrozen: false);
        AssertEqual(0, samus.Health, "whole damage beyond the remaining energy clamps energy to zero");
        AssertEqual(0, samus.SubunitHealth, "fatal whole damage clears the fraction too");
    }

    /// <summary>Checks movement effects from the room's atmosphere.</summary>
    [VerificationCommand("--samus-atmospheric-effects")]
    internal static int SamusAtmosphericEffectsCommand()
    {
        Suite(nameof(VerifySamusAtmosphericEffects), () => VerifySamusAtmosphericEffects());
        return 0;
    }

    /// <summary>Checks Space Jump, Screw Attack, and atmosphere sound requests at spin entry.</summary>
    [VerificationCommand("--spin-entry-audio")]
    internal static int SpinEntryAudioCommand()
    {
        Suite(nameof(VerifySamusSpaceJumpAndScrewAttack), () => VerifySamusSpaceJumpAndScrewAttack());
        Suite(nameof(VerifySamusAtmosphericEffects), () => VerifySamusAtmosphericEffects());
        return 0;
    }
}
