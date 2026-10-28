using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidFixtureHeadVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Rendering.RenderingAndRoomsVerification;
using static SuperMetroid.Verification.Hardware.SignedSineFixtureVerification;
using static SuperMetroid.Verification.Harness.SyntheticSamusAnimationDelaysVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Baby Metroid's entrance, first Mother Brain latch, and ensuing balance recovery.</summary>
internal static class BabyMetroidVerification
{

/// <summary>Compares the cutscene's timed phases, fixed-point positions, Samus pose, and Mother Brain handoff.</summary>
internal static void VerifyBabyMetroidCutsceneEntrance()
{
    VerifyBabyMetroidRouteDefinitions();
    var bus = new TestAddressSpace();

    // `$A0:B443` is trunc(sin(i*pi/128)*256), stored as a sign-extended word. Seed every
    // possible eight-bit index because the curve visits non-cardinal angles. The first
    // flight assertion below is a literal retail-ROM coordinate/velocity witness, so a
    // future accidental rounding change cannot make production and fixture drift together.
    var sineSamples = new SineSample[256];
    for (int angle = 0; angle < 256; angle++)
        sineSamples[angle] = new SineSample((byte)angle, unchecked((short)(Math.Sin(angle * Math.PI / 128.0) * 256.0)));
    SeedSignedSineTable(bus, sineSamples);

    // The drain producer alternates four retail walk speeds in both directions, then runs
    // the fast crouch. Seed mechanically equivalent bytecode at the literal list addresses;
    // private-ROM integration below separately proves those addresses against cartridge data.
    SeedMotherBrainWalkProgram(bus, 0x9730, duration: 2, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x976a, duration: 4, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x97a4, duration: 6, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x97de, duration: 8, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x9818, duration: 10, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x988c, duration: 2, forward: false);
    SeedMotherBrainWalkProgram(bus, 0x98c6, duration: 4, forward: false);
    SeedMotherBrainWalkProgram(bus, 0x9900, duration: 6, forward: false);
    SeedMotherBrainWalkProgram(bus, 0x9852, duration: 8, forward: false);
    SeedMotherBrainWalkProgram(bus, 0x993a, duration: 10, forward: false);
    SeedMotherBrainCrouchFastProgram(bus);
    // Controller one reads the current `$E9` direction byte, then rebinds the new `$EB`
    // animation pointer without refreshing radii. These are the only Samus ROM fields the
    // entrance consumes; the dedicated drained-controller suite proves their animation.
    WritePoseDefinition(bus, (int)SamusPoseId.DrainedCrouchingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.DrainedStandingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    SeedSyntheticAnimationDelay(bus, SamusPoseId.DrainedStandingLeftPose, 0xc100, 0x10);

    var samus = new SamusState
    {
        Pose = SamusPoseId.DrainedCrouchingLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(0x00ca, 0x00c0)),
        // These are the private-ROM runner's post-rainbow values. The Baby must add one
        // point on each `$CA7A` call and stop exactly at 899 rather than using a host fill.
        Health = 200,
        MaxHealth = 899,
        ReserveEnergy = 7,
        MaxReserveEnergy = 99,
    };
    // The real route reached `$E9` through controller zero, which had already loaded the
    // pose radius. Controller one/four intentionally do not refresh it, so initialize that
    // pre-existing WRAM state once rather than widening touch collision in production code.
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    var motherBrain = new MotherBrainRainbowBeamAttackSequence
    {
        State = { BrainPosition = new(0x0040, 0x0060) },
    };
    motherBrain.State.Body.Position = new(0x0040, 0x0064);
    motherBrain.StartAttackCycle(); // Establishes the pre-existing enabled neck flag.

    var baby = new BabyMetroidCutsceneState();
    baby.Initialize(previousOccupant: FixedPosition<RoomPixels>.Zero);
    AssertEqual(0x3800, baby.Properties.ToWord(), "Baby population/init property OR");
    AssertEqual(false, baby.HitFlashPaletteShown, "Baby cutscene shows its own palette, not the hit flash");
    // The Baby's graphics are its tile base alone: its parts carry their own palette rows and priorities, so the
    // palette and priority fields the cartridge's graphics word also held are not state the Baby keeps (#1422).
    AssertEqual(160, baby.TileBase, "Baby draws from transferred tile 160");
    AssertEqual(MotherBrainBabyProgram.Initial, baby.Program, "Baby initial program");
    // Whole/subpixel coordinates in native `$0F7A/$0F7C/$0F7E/$0F80` order.
    FixedPosition<RoomPixels> BabyPoint() => baby.Position;
    static FixedPosition<RoomPixels> ExpectedBabyPoint(
        ushort xPosition, ushort xSubposition, ushort yPosition, ushort ySubposition) =>
        FixedPosition<RoomPixels>.FromWords(xPosition, xSubposition, yPosition, ySubposition);

    AssertEqual(ExpectedBabyPoint(0x0140, 0, 0x0060, 0),
        BabyPoint(),
        "Baby initialization overwrites population coordinates");

    // `$F8` reaches zero without expiring. This is 248 visibly stationary calls, not an
    // approximate four-second host delay.
    for (int call = 1; call <= 248; call++)
        baby.Step(samus, motherBrain, HeadOf(motherBrain));
    AssertEqual(BabyMetroidCutscenePhase.DashOntoScreen, baby.Phase,
        "Baby dash delay retains function at timer zero");
    AssertEqual(0, baby.FunctionTimer.RawValue, "Baby dash delay exact zero boundary");
    AssertEqual(new Position16<RoomPixels>(0x0140, 0x0060), baby.Position.Whole, "Baby remains still through call 248");

    baby.Step(samus, motherBrain, HeadOf(motherBrain));
    AssertEqual(BabyMetroidCutscenePhase.CurveTowardMotherBrainHead, baby.Phase,
        "Baby call 249 falls through into curve function");
    AssertEqual(SuperMetroid.Core.Spatial.RotationAngle.FromRaw(0xd680), baby.Angle, "Baby first curve angle");
    AssertEqual(0x0a00, baby.Speed, "Baby first curve speed");
    AssertEqual(0xf772, baby.Velocity.X.RawValue, "Baby first ROM sine X velocity");
    AssertEqual(0x051e, baby.Velocity.Y.RawValue, "Baby first ROM cosine Y velocity");
    AssertEqual(ExpectedBabyPoint(0x0137, 0x7200, 0x0065, 0x1e00),
        BabyPoint(),
        "Baby first curve fixed-point displacement");

    BabyMetroidCutsceneStepResult latch;
    bool sawBodyStumbleRequest = false;
    bool sawMotherBrainInterrupt = false;
    bool sawLatchSound = false;
    int calls = 249;
    while (baby.Phase != BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse &&
           calls < 700)
    {
        SamusPoseId samusPoseBeforeStep = samus.Pose;
        latch = baby.Step(samus, motherBrain, HeadOf(motherBrain));
        calls++;
        sawBodyStumbleRequest |= latch.BodyStumbleRequested;
        sawMotherBrainInterrupt |= motherBrain.State.Phase ==
            MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback;
        sawLatchSound |= latch.LatchSoundQueued;

        if (calls == 259)
        {
            AssertEqual(BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace, baby.Phase,
                "Baby curve timer expires on call 259");
            AssertEqual(ExpectedBabyPoint(0x00da, 0x0c00, 0x0086, 0x7a00),
                BabyPoint(),
                "Baby curve endpoint from retail ROM");
        }
        else if (calls == 269)
        {
            AssertEqual(BabyMetroidCutscenePhase.LatchOntoMotherBrain, baby.Phase,
                "Baby face timer expires on call 269");
            AssertTrue(samusPoseBeforeStep != samus.Pose,
                "Baby face completion calls drained controller one");
            AssertEqual(SamusPoseId.DrainedStandingLeftPose, samus.Pose,
                "Baby face completion installs left drained standing pose");
            AssertEqual(ExpectedBabyPoint(0x008c, 0xc400, 0x004b, 0x6300),
                BabyPoint(),
                "Baby face endpoint from retail ROM");
        }
    }

    // GradduallyAccelerateTowardsPoint ($A9:F46B) runs as its ADC/SBC carry chain (#1269);
    // the earlier signed-integer approximation reached the pin three calls later.
    AssertEqual(422, calls, "Baby entrance reaches exact head pin on call 422");
    AssertEqual(BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse, baby.Phase,
        "Baby enters corpse-state wait after pin");
    AssertTrue(sawBodyStumbleRequest, "Baby requests Mother Brain fast backward stumble");
    AssertTrue(sawMotherBrainInterrupt, "Baby overwrites Mother Brain final-beam function");
    AssertTrue(sawLatchSound, "Baby latch queues sound library one effect $40");
    AssertEqual(MotherBrainBabyProgram.DrainingMotherBrain, baby.Program, "Baby pin installs draining animation");
    AssertEqual(ExpectedBabyPoint(0x0040, 0xc500, 0x0048, 0xf000),
        BabyPoint(),
        "Baby pin changes whole coordinates but retains native subpositions");
    AssertEqual(MotherBrainBodyProgram.WalkBackwardReallyFast, motherBrain.State.Body.Program,
        "Baby stumble uses animation-delay index two");
    AssertEqual(MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback,
        motherBrain.State.Phase,
        "Baby installs Mother Brain $BE38 for the following actor turn");

    MotherBrainRainbowBeamAttackStepResult takenAback = motherBrain.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
    AssertEqual(MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance,
        takenAback.PhaseAfter,
        "Mother Brain taken-aback setup falls through into regain balance");
    AssertEqual(0x002f, motherBrain.State.FunctionTimer.RawValue,
        "Mother Brain first regain call decrements $30 to $2F");
    AssertEqual(MotherBrainForm.DrainedByBabyMetroid, motherBrain.State.Body.Form, "Baby drain changes Mother Brain to her drained form");
    AssertEqual(MotherBrainNeckMovement.Raise, motherBrain.State.LowerNeckMovementIndex,
        "Mother Brain taken-aback lower neck index");
    AssertEqual(MotherBrainNeckMovement.Raise, motherBrain.State.UpperNeckMovementIndex,
        "Mother Brain taken-aback upper neck index");
    AssertEqual(0x0700, motherBrain.State.NeckAngleDelta,
        "Mother Brain taken-aback neck delta");

    int regainCalls = 1;
    while (motherBrain.State.Phase == MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance)
    {
        motherBrain.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        regainCalls++;
    }
    AssertEqual(49, regainCalls, "Mother Brain regain balance includes setup fallthrough call");
    AssertEqual(MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam,
        motherBrain.State.Phase,
        "Mother Brain advances to painful firing function");
    AssertEqual(MotherBrainNeckMovement.BobDown, motherBrain.State.LowerNeckMovementIndex,
        "Mother Brain drained firing lower neck index");
    AssertEqual(MotherBrainNeckMovement.BobUp, motherBrain.State.UpperNeckMovementIndex,
        "Mother Brain drained firing upper neck index");

    Console.WriteLine(
        "  Baby Metroid: entrance, first latch and Mother Brain regain balance agree.");
}

    /// <summary>
    /// The slowing Samus rainbow accumulates 768/65536 of a palette frame per call and advances
    /// a frame exactly when the cartridge's 16-bit accumulator would carry, keeping the remainder.
    /// </summary>
    internal static void VerifySamusRainbowFractionCarry()
    {
        AssertEqual(new SamusRainbowFractionStep(768, false), BabyMetroidCutsceneState.AdvanceSamusRainbowFraction(0),
            "one call from a cleared accumulator adds 768 without completing a frame");
        int fraction = 0;
        for (int call = 1; call <= 85; call++)
        {
            (int next, bool completed) = BabyMetroidCutsceneState.AdvanceSamusRainbowFraction(fraction);
            AssertTrue(!completed, $"call {call} stays below a whole frame");
            fraction = next;
        }
        AssertEqual(65280, fraction, "85 calls accumulate 65280 of the 65536 a frame needs");
        AssertEqual(new SamusRainbowFractionStep(512, true), BabyMetroidCutsceneState.AdvanceSamusRainbowFraction(fraction),
            "the 86th call carries into a new frame and keeps the 512 remainder");
        AssertEqual(new SamusRainbowFractionStep(767, true), BabyMetroidCutsceneState.AdvanceSamusRainbowFraction(65535),
            "the largest fraction carries and keeps the wrapped remainder");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BabyMetroidCutsceneState.AdvanceSamusRainbowFraction(65536),
            "a fraction that is already a whole frame is rejected");
        Console.WriteLine("  Baby Metroid: the slowing Samus rainbow carries exactly at whole frames.");
    }

    /// <summary>Runs the slowing Samus rainbow accumulator check.</summary>
    [VerificationCommand("--baby-metroid-rainbow-fraction")]
    internal static int BabyMetroidRainbowFractionCommand()
    {
        Suite(nameof(VerifySamusRainbowFractionCarry), () => VerifySamusRainbowFractionCarry());
        return 0;
    }

    /// <summary>Runs the Baby Metroid entrance and route-definition check.</summary>
    [VerificationCommand("--baby-metroid-route-definitions")]
    internal static int BabyMetroidRouteDefinitionsCommand()
    {
        Suite(nameof(VerifyBabyMetroidCutsceneEntrance), () => VerifyBabyMetroidCutsceneEntrance());
        return 0;
    }
}
