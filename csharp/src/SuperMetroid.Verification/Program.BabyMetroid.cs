using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>Baby Metroid cutscene verification and its private fixtures.</summary>
static void VerifyBabyMetroidCutsceneEntrance()
{
    var bus = new TestAddressSpace();

    // `$A0:B443` is trunc(sin(i*pi/128)*256), stored as a sign-extended word. Seed every
    // possible eight-bit index because the curve visits non-cardinal angles. The first
    // flight assertion below is a literal retail-ROM coordinate/velocity witness, so a
    // future accidental rounding change cannot make production and fixture drift together.
    for (int angle = 0; angle < 256; angle++)
    {
        short sine = unchecked((short)(Math.Sin(angle * Math.PI / 128.0) * 256.0));
        WriteTestWord(bus, 0xa0b443 + angle * 2, unchecked((ushort)sine));
    }

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
    SeedBabyCeilingToSamusRoute(bus);

    // Controller one reads the current `$E9` direction byte, then rebinds the new `$EB`
    // animation pointer without refreshing radii. These are the only Samus ROM fields the
    // entrance consumes; the dedicated drained-controller suite proves their animation.
    WritePoseDefinition(bus, SamusState.DrainedCrouchingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusState.DrainedStandingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedStandingLeftPose * 2, 0xc100);
    bus.WriteByte(0x91c100, 0x10);

    var samus = new SamusState
    {
        Pose = SamusState.DrainedCrouchingLeftPose,
        XPosition = 0x00ca,
        YPosition = 0x00c0,
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
    samus.RefreshCollisionRadii(bus);
    var motherBrain = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 0x0040,
        BrainYPosition = 0x0060,
    };
    motherBrain.Body.XPosition = 0x0040;
    motherBrain.Body.YPosition = 0x0064;
    motherBrain.StartAttackCycle(); // Establishes the pre-existing enabled neck flag.

    var baby = new BabyMetroidCutsceneState();
    baby.Initialize();
    AssertEqual((ushort)0x3800, baby.Properties, "Baby population/init property OR");
    AssertEqual((ushort)0x0e00, baby.Palette, "Baby cutscene palette");
    AssertEqual((ushort)0x00a0, baby.GraphicsOffset, "Baby transferred-tile offset");
    AssertEqual(BabyMetroidCutsceneState.InitialInstructionList, baby.InstructionList,
        "Baby initial instruction list");
    AssertEqual(new BabyMetroidCutscenePoint(0x0140, 0, 0x0060, 0),
        new BabyMetroidCutscenePoint(
            baby.XPosition, baby.XSubposition, baby.YPosition, baby.YSubposition),
        "Baby initialization overwrites population coordinates");

    // `$F8` reaches zero without expiring. This is 248 visibly stationary calls, not an
    // approximate four-second host delay.
    for (int call = 1; call <= 248; call++)
        baby.Step(bus, samus, motherBrain);
    AssertEqual(BabyMetroidCutscenePhase.DashOntoScreen, baby.Phase,
        "Baby dash delay retains function at timer zero");
    AssertEqual((ushort)0, baby.FunctionTimer, "Baby dash delay exact zero boundary");
    AssertEqual((ushort)0x0140, baby.XPosition, "Baby remains still through call 248");
    AssertEqual((ushort)0x0060, baby.YPosition, "Baby Y remains still through call 248");

    BabyMetroidCutsceneStepResult firstCurve = baby.Step(bus, samus, motherBrain);
    AssertEqual(BabyMetroidCutscenePhase.CurveTowardMotherBrainHead, baby.Phase,
        "Baby call 249 falls through into curve function");
    AssertEqual((ushort)0xd680, baby.Angle, "Baby first curve angle");
    AssertEqual((ushort)0x0a00, baby.Speed, "Baby first curve speed");
    AssertEqual((ushort)0xf772, firstCurve.XVelocity, "Baby first ROM sine X velocity");
    AssertEqual((ushort)0x051e, firstCurve.YVelocity, "Baby first ROM cosine Y velocity");
    AssertEqual(new BabyMetroidCutscenePoint(0x0137, 0x7200, 0x0065, 0x1e00),
        firstCurve.After,
        "Baby first curve fixed-point displacement");

    BabyMetroidCutsceneStepResult latch = default;
    bool sawBodyStumbleRequest = false;
    bool sawMotherBrainInterrupt = false;
    bool sawLatchSound = false;
    int calls = 249;
    while (baby.Phase != BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse &&
           calls < 700)
    {
        latch = baby.Step(bus, samus, motherBrain);
        calls++;
        sawBodyStumbleRequest |= latch.BodyStumbleRequested;
        sawMotherBrainInterrupt |= latch.MotherBrainInterrupted;
        sawLatchSound |= latch.LatchSoundQueued;

        if (calls == 259)
        {
            AssertEqual(BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace, baby.Phase,
                "Baby curve timer expires on call 259");
            AssertEqual(new BabyMetroidCutscenePoint(0x00da, 0x0c00, 0x0086, 0x7a00),
                latch.After,
                "Baby curve endpoint from retail ROM");
        }
        else if (calls == 269)
        {
            AssertEqual(BabyMetroidCutscenePhase.LatchOntoMotherBrain, baby.Phase,
                "Baby face timer expires on call 269");
            AssertTrue(latch.SamusStandingRequested,
                "Baby face completion calls drained controller one");
            AssertEqual(SamusState.DrainedStandingLeftPose, samus.Pose,
                "Baby face completion installs left drained standing pose");
            AssertEqual(new BabyMetroidCutscenePoint(0x008c, 0xc400, 0x004b, 0x6300),
                latch.After,
                "Baby face endpoint from retail ROM");
        }
    }

    AssertEqual(425, calls, "Baby entrance reaches exact head pin on call 425");
    AssertEqual(BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse, baby.Phase,
        "Baby enters corpse-state wait after pin");
    AssertTrue(sawBodyStumbleRequest, "Baby requests Mother Brain fast backward stumble");
    AssertTrue(sawMotherBrainInterrupt, "Baby overwrites Mother Brain final-beam function");
    AssertTrue(sawLatchSound, "Baby latch queues sound library one effect $40");
    AssertEqual(BabyMetroidCutsceneState.DrainingMotherBrainInstructionList,
        baby.InstructionList,
        "Baby pin installs draining animation");
    AssertEqual(new BabyMetroidCutscenePoint(0x0040, 0x1400, 0x0048, 0xd200),
        latch.After,
        "Baby pin changes whole coordinates but retains native subpositions");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardReallyFastInstructionList,
        motherBrain.Body.InstructionPointer,
        "Baby stumble uses animation-delay index two");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback,
        motherBrain.Phase,
        "Baby installs Mother Brain $BE38 for the following actor turn");

    MotherBrainRainbowBeamAttackStepResult takenAback = motherBrain.Step(
        bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance,
        takenAback.PhaseAfter,
        "Mother Brain taken-aback setup falls through into regain balance");
    AssertEqual((ushort)0x002f, motherBrain.FunctionTimer,
        "Mother Brain first regain call decrements $30 to $2F");
    AssertEqual((ushort)3, motherBrain.Body.Form, "Baby drain changes Mother Brain form to three");
    AssertEqual((ushort)8, motherBrain.LowerNeckMovementIndex,
        "Mother Brain taken-aback lower neck index");
    AssertEqual((ushort)8, motherBrain.UpperNeckMovementIndex,
        "Mother Brain taken-aback upper neck index");
    AssertEqual((ushort)0x0700, motherBrain.NeckAngleDelta,
        "Mother Brain taken-aback neck delta");

    int regainCalls = 1;
    while (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance)
    {
        motherBrain.Step(bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
        regainCalls++;
    }
    AssertEqual(49, regainCalls, "Mother Brain regain balance includes setup fallthrough call");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam,
        motherBrain.Phase,
        "Mother Brain advances to painful firing function");
    AssertEqual((ushort)2, motherBrain.LowerNeckMovementIndex,
        "Mother Brain drained firing lower neck index");
    AssertEqual((ushort)4, motherBrain.UpperNeckMovementIndex,
        "Mother Brain drained firing upper neck index");

    int drainFrame = 0;
    int beamRunOutFrame = 0;
    int lowPowerFrame = 0;
    int greyStartFrame = 0;
    int corpseFrame = 0;
    int stopDrainingFrame = 0;
    int letGoFrame = 0;
    int dustFrame = 0;
    int ceilingFrame = 0;
    // Capture the actual three allocation records, not merely the transition edge. This
    // independently locks the signed offset arithmetic and native helper-call order.
    var releaseDustClouds = new List<BabyMetroidReleaseDustRequest>(capacity: 3);
    bool sawSamusCrouch = false;
    while (baby.Phase != BabyMetroidCutscenePhase.MoveToSamus)
    {
        MotherBrainRainbowBeamAttackPhase mbBefore = motherBrain.Phase;
        BabyMetroidCutscenePhase babyBefore = baby.Phase;
        motherBrain.Step(
            bus,
            samus,
            enemyFrameCounter: unchecked((ushort)drainFrame),
            mainEnemyExecutionCounter: unchecked((ushort)drainFrame));
        motherBrain.Body.Step(bus);
        motherBrain.StepNeckMovement(bus, samus);
        BabyMetroidCutsceneStepResult babyDrain = baby.Step(
            bus,
            samus,
            motherBrain,
            enemyFrameCounter: unchecked((ushort)drainFrame));
        motherBrain.StepBrainShakeForDraw();
        drainFrame++;

        if (mbBefore != motherBrain.Phase)
        {
            if (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut)
                beamRunOutFrame = drainFrame;
            if (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode)
                lowPowerFrame = drainFrame;
            if (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey)
                greyStartFrame = drainFrame;
            if (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey)
                corpseFrame = drainFrame;
        }
        if (babyBefore != baby.Phase)
        {
            if (baby.Phase == BabyMetroidCutscenePhase.StopDraining)
                stopDrainingFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds)
                letGoFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.MoveToTheCeiling)
                dustFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.MoveToSamus)
                ceilingFrame = drainFrame;
        }
        releaseDustClouds.AddRange(babyDrain.ReleaseDustClouds);
        sawSamusCrouch |= babyDrain.SamusCrouchingRequested;
        AssertTrue(drainFrame < 2500,
            $"Baby drain/release reaches ceiling; MB={motherBrain.Phase}/stage{motherBrain.PainfulWalkingStage}/" +
            $"body({motherBrain.Body.XPosition},{motherBrain.Body.YPosition}) pose{motherBrain.Body.Pose}, " +
            $"neck={motherBrain.LowerNeckMovementIndex}/{motherBrain.UpperNeckMovementIndex}, Baby={baby.Phase}");
    }

    // Hard boundaries from the independent bytecode fixture. These counts begin with the
    // first `$BE96` call after the already-proven 49-call regain phase.
    AssertEqual(593, beamRunOutFrame, "painful stage six ends rainbow beam");
    AssertEqual(977, lowPowerFrame, "painful stage eight enters low-power mode");
    AssertEqual(1439, greyStartFrame, "neck raise and crouch delay reach grey transition");
    AssertEqual(1591, corpseFrame, "nine grey-table probes publish corpse state");
    AssertEqual(corpseFrame, stopDrainingFrame,
        "later Baby slot observes corpse flag on its publication frame");
    AssertEqual(1656, letGoFrame, "stop-draining `$40` expires after 65 calls");
    AssertEqual(1689, dustFrame, "let-go `$20` requests dust on call 33");
    AssertEqual(1703, ceilingFrame, "gradual ceiling acceleration reaches collision rectangle");
    AssertEqual((ushort)40, motherBrain.Body.XPosition, "corpse body rests at rear X");
    AssertEqual((ushort)138, motherBrain.Body.YPosition, "fast crouch lowers body by 38 pixels");
    AssertEqual((ushort)81, motherBrain.BrainXPosition, "neck geometry publishes corpse brain X");
    AssertEqual((ushort)78, motherBrain.BrainYPosition, "neck geometry publishes corpse brain Y");
    AssertEqual((ushort)81, baby.XPosition, "ceiling handoff Baby X");
    AssertEqual((ushort)40, baby.YPosition, "ceiling handoff Baby Y after common mover");
    AssertEqual((ushort)0x0000, baby.XVelocity, "ceiling handoff X velocity");
    AssertEqual((ushort)0xff78, baby.YVelocity,
        "synthetic ceiling handoff preserves its independently accumulated Y velocity");
    AssertEqual(3, releaseDustClouds.Count,
        "Baby release requests exactly three Mother Brain head dust clouds");
    AssertEqual(new BabyMetroidReleaseDustRequest(65, 70, 9), releaseDustClouds[0],
        "Baby release first dust uses brain offset (-16,-8)");
    AssertEqual(new BabyMetroidReleaseDustRequest(81, 62, 9), releaseDustClouds[1],
        "Baby release second dust uses brain offset (0,-16)");
    AssertEqual(new BabyMetroidReleaseDustRequest(97, 70, 9), releaseDustClouds[2],
        "Baby release third dust uses brain offset (+16,-8)");
    AssertTrue(sawSamusCrouch, "Baby ceiling collision calls drained controller four");
    AssertEqual(BabyMetroidCutsceneState.CeilingToSamusMovementTable,
        baby.MovementTablePointer,
        "Baby ceiling collision installs `$CA24` movement table");
    AssertEqual((ushort)0x8ca0, motherBrain.BrainHealth,
        "Mother Brain grey completion rewrites brain health to 36,000");
    AssertEqual((ushort)1, motherBrain.Phase2CorpseState,
        "Mother Brain grey completion publishes corpse state one");
    AssertEqual(SamusState.DrainedCrouchingLeftPose, samus.Pose,
        "ceiling collision installs left drained crouching pose");

    // Continue in native enemy-slot order through the eight ROM route records, generic
    // touch AI, and 699 one-point healing calls. This deliberately remains an integrated
    // synthetic fixture: its earlier entrance used a stationary neck target, so its inherited
    // subpixels are not falsely presented as coordinates captured from the full retail run.
    var routePointerFrames = new Dictionary<ushort, int>();
    var routePointerPoints = new Dictionary<ushort, BabyMetroidCutscenePoint>();
    int latchOntoSamusFrame = 0;
    int healSamusFrame = 0;
    int healingCompleteFrame = 0;
    bool sawSamusTouch = false;
    bool sawAmbientCryThreshold = false;
    while (baby.Phase != BabyMetroidCutscenePhase.IdleUntilNoHealth)
    {
        ushort pointerBefore = baby.MovementTablePointer;
        BabyMetroidCutscenePhase phaseBefore = baby.Phase;
        motherBrain.Step(
            bus,
            samus,
            enemyFrameCounter: unchecked((ushort)drainFrame),
            mainEnemyExecutionCounter: unchecked((ushort)drainFrame));
        motherBrain.Body.Step(bus);
        motherBrain.StepNeckMovement(bus, samus);
        BabyMetroidCutsceneStepResult routeStep = baby.Step(
            bus,
            samus,
            motherBrain,
            enemyFrameCounter: unchecked((ushort)drainFrame),
            // `$FA0` proves the inclusive random-cry threshold without influencing motion.
            randomNumber: 0x0fa0);
        motherBrain.StepBrainShakeForDraw();
        drainFrame++;

        sawSamusTouch |= routeStep.SamusTouchCollision;
        sawAmbientCryThreshold |= routeStep.AmbientCrySoundQueued;
        if (pointerBefore != baby.MovementTablePointer)
        {
            routePointerFrames[baby.MovementTablePointer] = drainFrame;
            routePointerPoints[baby.MovementTablePointer] = routeStep.After;
        }
        if (phaseBefore != baby.Phase)
        {
            if (baby.Phase == BabyMetroidCutscenePhase.LatchOntoSamus)
                latchOntoSamusFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.HealSamusToFullHealth)
                healSamusFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.IdleUntilNoHealth)
            {
                healingCompleteFrame = drainFrame;
                AssertTrue(routeStep.HealingCompleted,
                    "final one-point heal publishes completion on the transition call");
            }
        }
        AssertTrue(drainFrame < 3200,
            $"Baby route/heal reaches idle state; phase={baby.Phase}, pointer=${baby.MovementTablePointer:X4}");
    }

    // These are the deterministic witnesses produced by this fixture's own inherited
    // fixed-point state. The DebugRunner separately locks the retail-ROM witnesses; keeping
    // both sets makes any accidental dependence on a fabricated initial subposition visible.
    AssertEqual(1781, routePointerFrames[0xca2c], "route reaches `$CA2C` record");
    AssertEqual(1847, routePointerFrames[0xca34], "route reaches `$CA34` record");
    AssertEqual(1946, routePointerFrames[0xca3c], "route reaches `$CA3C` record");
    AssertEqual(1947, routePointerFrames[0xca44], "overlapping route advances again on next call");
    AssertEqual(2027, routePointerFrames[0xca4c], "route reaches `$CA4C` record");
    AssertEqual(2046, routePointerFrames[0xca54], "route reaches `$CA54` record");
    AssertEqual(2063, routePointerFrames[0xca5c], "route reaches final `$CA5C` record");
    AssertEqual(new BabyMetroidCutscenePoint(0x007e, 0xac00, 0x0051, 0x9700),
        routePointerPoints[0xca2c], "first route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x010b, 0xaf00, 0x008d, 0xc700),
        routePointerPoints[0xca34], "second route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00e7, 0x1b00, 0x004b, 0x0200),
        routePointerPoints[0xca3c], "third route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00e5, 0xa400, 0x0049, 0xf400),
        routePointerPoints[0xca44], "fourth route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00c7, 0x7900, 0x0059, 0x4b00),
        routePointerPoints[0xca4c], "fifth route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00cb, 0x1c00, 0x0069, 0x0000),
        routePointerPoints[0xca54], "sixth route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00cd, 0x8d00, 0x007a, 0x0f00),
        routePointerPoints[0xca5c], "seventh route-leg endpoint");
    AssertEqual(2077, latchOntoSamusFrame,
        "final route record overlays +8 with signed `$CA66` function pointer");
    AssertEqual(2095, healSamusFrame,
        "post-main enemy touch reaches `$CF03` latch target");
    AssertEqual(2794, healingCompleteFrame,
        "699 one-point heals reach 899 energy");
    AssertTrue(sawSamusTouch, "generic collision dispatches Baby `$CF03` touch AI");
    AssertTrue(sawAmbientCryThreshold, "route accepts random cry threshold `$FA0`");
    AssertTrue(!baby.CrySoundEnabled, "route/heal keeps ordinary cry request clear");
    AssertTrue(baby.HealthBasedPaletteEnabled, "route enables health-based Baby palette");
    AssertEqual((ushort)899, samus.Health, "Baby healing clamps at maximum energy");
    AssertEqual((ushort)99, samus.ReserveEnergy, "healing completion fills reserve energy");
    AssertEqual((ushort)3200, baby.Health, "healing does not invent Mother Brain damage");

    // Drive the same actor into `$CABD`'s zero-health transition without fabricating a
    // function pointer. One saturating hit represents the already-verified ring collision
    // producer; all following coordinates, timers, and cross-actor writes remain Baby AI.
    BabyMetroidOnionRingHitResult ordinaryFatal = baby.ApplyMotherBrainOnionRingHit(3200);
    AssertTrue(ordinaryFatal.Applied && ordinaryFatal.HealthAfter == 0,
        "ordinary murder volley can saturate Baby health to zero");
    baby.Step(bus, samus, motherBrain);
    AssertEqual(BabyMetroidCutscenePhase.ReleaseSamus, baby.Phase,
        "zero-health idle call installs release function");
    AssertEqual((ushort)0x0140, baby.Health,
        "ordinary zero-health transition restores $140 for flight");

    // The fixed target chain takes a deterministic but fixture-dependent number of calls.
    // Bound it, then use the real `$86:C381` damage path for the intended 79->0 final hit.
    int finalRouteCalls = 0;
    while (baby.Phase != BabyMetroidCutscenePhase.FinalCharge)
    {
        baby.Step(bus, samus, motherBrain, enemyFrameCounter: unchecked((ushort)finalRouteCalls));
        finalRouteCalls++;
        AssertTrue(finalRouteCalls < 1000,
            $"Baby reaches final charge; current phase={baby.Phase}");
    }
    AssertEqual((ushort)0x004f, baby.Health,
        "final-charge staging point assigns literal 79 health");
    BabyMetroidOnionRingHitResult finalFatal = baby.ApplyMotherBrainOnionRingHit();
    AssertTrue(finalFatal.Applied && finalFatal.HealthAfter == 0,
        "one final 80-damage ring saturates 79 health");

    while (baby.Phase != BabyMetroidCutscenePhase.DeathSequence)
    {
        baby.Step(bus, samus, motherBrain, enemyFrameCounter: unchecked((ushort)finalRouteCalls));
        finalRouteCalls++;
        AssertTrue(finalRouteCalls < 1200,
            $"fatal shake/theme delays reach death sequence; current phase={baby.Phase}");
    }
    AssertEqual((ushort)28, samus.AnimationFrame,
        "prepare-Hyper expiry executes Samus command $19");
    AssertEqual((ushort)1, samus.AnimationFrameTimer,
        "Samus command $19 freezes animation with timer one");
    AssertEqual(BabyMetroidSamusRainbowPhase.ActivateWhenEnemyIsLow,
        baby.SamusRainbowPhase,
        "prepare-Hyper expiry installs low-enemy rainbow handler");

    var blackPalettes = new List<BabyMetroidPaletteTransferRequest>();
    var deathExplosions = new List<BabyMetroidDeathExplosionRequest>();
    int deathCalls = 0;
    while (baby.Phase == BabyMetroidCutscenePhase.DeathSequence)
    {
        BabyMetroidCutsceneStepResult babyDeath = baby.Step(
            bus,
            samus,
            motherBrain,
            enemyFrameCounter: unchecked((ushort)deathCalls));
        deathCalls++;
        if (babyDeath.DeathExplosion is { } deathExplosion)
            deathExplosions.Add(deathExplosion);
        if (babyDeath.BabyPaletteTransfer is { } blackPalette)
            blackPalettes.Add(blackPalette);
        AssertTrue(deathCalls < 500, "Baby black fade reaches unload phase");
    }
    AssertEqual(BabyMetroidCutscenePhase.UnloadTiles, baby.Phase,
        "seventh black-table probe installs unload function");
    AssertEqual(6, blackPalettes.Count, "black fade publishes six palette records");
    uint[] expectedBlackSources = [
        0xade90c, 0xade928, 0xade944, 0xade960, 0xade97c, 0xade998,
    ];
    for (int index = 0; index < expectedBlackSources.Length; index++)
    {
        AssertEqual((ushort)(index + 1), blackPalettes[index].PaletteIndex,
            $"black palette {index + 1} index");
        AssertEqual(expectedBlackSources[index], blackPalettes[index].SourceAddress,
            $"black palette {index + 1} source");
        AssertEqual((ushort)0x01e2, blackPalettes[index].DestinationColorIndex,
            $"black palette {index + 1} destination");
    }
    AssertTrue(deathExplosions.Count > 1, "death sequence emits repeating dust explosions");
    AssertEqual((ushort)1, deathExplosions[0].PatternIndex,
        "cleared death pattern increments before first lookup");
    AssertEqual((ushort)2, deathExplosions[1].PatternIndex,
        "death explosions advance to the following table pair");
    AssertEqual(36, (int)deathExplosions[1].XPosition - deathExplosions[0].XPosition,
        "death explosion entries one/two retain their -20/+16 X offsets");
    AssertTrue(baby.IsInvisible, "black-fade completion sets enemy invisibility property");

    var attackTransfers = new List<MotherBrainSpriteTileTransferRequest>();
    int unloadCalls = 0;
    BabyMetroidCutsceneStepResult fourthTransfer = default;
    while (baby.Phase == BabyMetroidCutscenePhase.UnloadTiles)
    {
        fourthTransfer = baby.Step(bus, samus, motherBrain);
        unloadCalls++;
        if (fourthTransfer.AttackTileTransfer is { } transfer)
            attackTransfers.Add(transfer);
        AssertTrue(unloadCalls < 200, "Baby unload wait reaches all four attack DMAs");
    }
    AssertEqual(132, unloadCalls,
        "unload waits 129 calls then publishes four consecutive transfers");
    AssertEqual(4, attackTransfers.Count, "attack graphics have four DMA records");
    uint[] expectedAttackSources = [0xb7a000, 0xb7a200, 0xb7a400, 0xb7a600];
    ushort[] expectedAttackDestinations = [0x7c00, 0x7d00, 0x7e00, 0x7f00];
    for (int index = 0; index < attackTransfers.Count; index++)
    {
        AssertEqual(expectedAttackSources[index], attackTransfers[index].SourceAddress,
            $"attack DMA {index} source");
        AssertEqual(expectedAttackDestinations[index], attackTransfers[index].VramDestination,
            $"attack DMA {index} destination");
        AssertEqual((ushort)0x0200, attackTransfers[index].Size,
            $"attack DMA {index} size");
    }
    AssertEqual(BabyMetroidCutscenePhase.LetSamusRainbowSomeMore, baby.Phase,
        "fourth attack DMA observes zero terminator");
    AssertEqual((ushort)0x00af, baby.FunctionTimer,
        "fourth attack DMA falls through and decrements new $B0 timer");

    var roomPalettes = new List<MotherBrainBackgroundPaletteTransferRequest>();
    int rainbowDelayCalls = 0;
    BabyMetroidCutsceneStepResult firstRoomPalette = default;
    while (baby.Phase == BabyMetroidCutscenePhase.LetSamusRainbowSomeMore)
    {
        firstRoomPalette = baby.Step(bus, samus, motherBrain);
        rainbowDelayCalls++;
        if (firstRoomPalette.BackgroundPaletteTransfer is { } palette)
            roomPalettes.Add(palette);
    }
    AssertEqual(176, rainbowDelayCalls,
        "post-DMA `$AF..0` wait expires after 176 additional calls");
    AssertEqual(BabyMetroidCutscenePhase.FinalCutscene, baby.Phase,
        "rainbow delay falls through into final room-light function");
    AssertEqual(1, roomPalettes.Count,
        "final-cutscene fallthrough publishes room palette zero immediately");

    int finalPaletteCalls = 1;
    while (!baby.IsDeleted)
    {
        BabyMetroidCutsceneStepResult finalPalette = baby.Step(bus, samus, motherBrain);
        finalPaletteCalls++;
        if (finalPalette.BackgroundPaletteTransfer is { } palette)
            roomPalettes.Add(palette);
        AssertTrue(finalPaletteCalls < 20, "phase-three light table reaches zero terminator");
    }
    AssertEqual(8, finalPaletteCalls,
        "room-light table publishes seven palettes then completes on entry seven");
    AssertEqual(7, roomPalettes.Count, "phase-three light restore has seven records");
    for (int index = 0; index < roomPalettes.Count; index++)
    {
        AssertEqual((ushort)index, roomPalettes[index].PaletteIndex,
            $"room-light palette {index} index");
        AssertEqual(unchecked((uint)(0xadf3d3 - index * 0x38)), roomPalettes[index].SourceAddress,
            $"room-light palette {index} reverse source");
    }
    AssertTrue(!samus.Drained.RainbowPaletteEnabled,
        "final cutscene executes command $17 rainbow disable");
    AssertEqual((ushort)13, samus.AnimationFrame,
        "command $17 starts drained Samus standing animation at frame 13");
    AssertEqual((ushort)0x8000, samus.HyperBeam,
        "drained controller three grants Hyper Beam");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance,
        motherBrain.Phase,
        "Baby deletion installs Mother Brain `$C1CF` for following actor call");

    MotherBrainRainbowBeamAttackStepResult recovery = motherBrain.Step(bus, samus, 0, 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting,
        recovery.PhaseAfter,
        "phase-three recovery publishes form and setup timer");
    AssertEqual((ushort)4, motherBrain.Body.Form, "phase-three recovery sets body form four");
    AssertEqual((ushort)0x0020, motherBrain.FunctionTimer,
        "phase-three recovery loads literal $20 setup wait");
    int setupCalls = 0;
    while (motherBrain.Phase ==
           MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting)
    {
        motherBrain.Step(bus, samus, 0, 0);
        setupCalls++;
    }
    AssertEqual(33, setupCalls, "phase-three `$20` wait expires on wrapped call 33");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3FightingMain, motherBrain.Phase,
        "recovery reaches genuine `$C209` phase-three combat seam");

    // Exercise the new combat scheduler with its real body bytecode rather than changing
    // X directly. `$C1F0` falls into main on setup call 33, so that same call must initialize
    // the neck and request the first fourteen-pixel retreat.
    var phase3Samus = new SamusState { XPosition = 224, YPosition = 120 };
    var phase3 = new MotherBrainRainbowBeamAttackSequence();
    phase3.Body.XPosition = 0x0070;
    phase3.Body.YPosition = 0x0064;
    phase3.BeginPhase3RecoveryFromBabyCutscene();
    phase3.Step(bus, phase3Samus, 0, 0);
    for (int call = 0; call < 33; call++)
        phase3.Step(bus, phase3Samus, 0, 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3FightingMain, phase3.Phase,
        "phase-three fixture reaches main after exact setup wait");
    AssertEqual(MotherBrainPhase3NeckPhase.Inactive, phase3.Phase3NeckPhase,
        "same-call normal neck initializer reduces to its RTS state");
    AssertEqual((ushort)0x0080, phase3.NeckAngleDelta,
        "normal neck initializer selects delta $80");
    AssertEqual(MotherBrainPhase3WalkingPhase.RetreatQuickly, phase3.Phase3WalkingPhase,
        "zero walk credit falls through to quick retreat");
    AssertEqual((ushort)0x0062, phase3.Phase3TargetXPosition,
        "quick retreat target is body X minus fourteen");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardReallyFastInstructionList,
        phase3.Body.InstructionPointer,
        "quick retreat selects delay-index-two body bytecode");

    int quickWalkCalls = 0;
    while (!phase3.Body.Sleeping)
    {
        phase3.Body.Step(bus);
        quickWalkCalls++;
        AssertTrue(quickWalkCalls < 40, "phase-three quick retreat bytecode sleeps");
    }
    AssertEqual((ushort)0x0058, phase3.Body.XPosition,
        "quick retreat completes its native net-minus-24 animation");
    MotherBrainRainbowBeamAttackStepResult quickReached =
        phase3.Step(bus, phase3Samus, 0, 0);
    AssertTrue(!quickReached.BodyWalkRequested,
        "quick-retreat target call changes scheduler without same-call slow request");
    AssertEqual(MotherBrainPhase3WalkingPhase.RetreatSlowly, phase3.Phase3WalkingPhase,
        "quick retreat hands off to slow retreat");
    AssertEqual((ushort)0x004a, phase3.Phase3TargetXPosition,
        "slow retreat chooses a fresh fourteen-pixel target");

    MotherBrainRainbowBeamAttackStepResult slowRequest =
        phase3.Step(bus, phase3Samus, 0, 0);
    AssertTrue(slowRequest.BodyWalkRequested, "following call requests slow retreat");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardFastInstructionList,
        phase3.Body.InstructionPointer,
        "slow retreat selects delay-index-four body bytecode");
    int slowWalkCalls = 0;
    while (!phase3.Body.Sleeping)
    {
        phase3.Body.Step(bus);
        slowWalkCalls++;
        AssertTrue(slowWalkCalls < 80, "phase-three slow retreat bytecode sleeps");
    }
    AssertEqual((ushort)0x0040, phase3.Body.XPosition,
        "slow retreat completes another native net-minus-24 animation");
    phase3.Step(bus, phase3Samus, 0, 0);
    AssertEqual(MotherBrainPhase3WalkingPhase.TryToInchForward, phase3.Phase3WalkingPhase,
        "slow-retreat target restores inch-forward scheduler");
    AssertEqual((ushort)0x0040, phase3.Phase3WalkCounter,
        "slow-retreat completion loads literal $40 walk credit");
    AssertEqual((ushort)0x0041, phase3.Phase3TargetXPosition,
        "inch-forward handoff records current X plus one");

    // Five calls reach `$E0`; the sixth reaches `$100` and emits a one-pixel forward walk.
    for (int call = 0; call < 5; call++)
    {
        MotherBrainRainbowBeamAttackStepResult accumulating =
            phase3.Step(bus, phase3Samus, 0, 0);
        AssertTrue(!accumulating.BodyWalkRequested,
            $"walk-credit accumulation call {call} does not move before $100");
    }
    AssertEqual((ushort)0x00e0, phase3.Phase3WalkCounter,
        "five standing calls accumulate walk credit to $E0");
    MotherBrainRainbowBeamAttackStepResult inch = phase3.Step(bus, phase3Samus, 0, 0);
    AssertTrue(inch.BodyWalkRequested, "$100 walk credit requests one-pixel forward target");
    AssertEqual((ushort)0x0100, phase3.Phase3WalkCounter,
        "inch-forward request preserves accumulated credit");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingForwardFastInstructionList,
        phase3.Body.InstructionPointer,
        "RNG bit one clear chooses phase-three delay index four");

    // Begin the visible body instruction before applying a Hyper Beam reaction. This keeps
    // the next AI call in native order: neck recoil still runs, walking is pose-gated, and
    // the negative `$0100-$010A` subtraction clamps the walk counter to zero.
    phase3.Body.Step(bus);
    AssertEqual((ushort)1, phase3.Body.Pose, "forward bytecode publishes walking pose");
    phase3.ApplyPhase2Or3ShotReaction(MotherBrainProjectileType.Beam);
    AssertEqual((ushort)0, phase3.Phase3WalkCounter,
        "Hyper Beam underflow clamps walk counter to zero");
    AssertEqual(MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil, phase3.Phase3NeckPhase,
        "Hyper Beam underflow installs recoil setup");
    MotherBrainRainbowBeamAttackStepResult recoilSetup =
        phase3.Step(bus, phase3Samus, 0, 0, randomNumberSeed: 0xffff);
    AssertEqual(MotherBrainPhase3NeckPhase.HyperBeamRecoil, phase3.Phase3NeckPhase,
        "recoil setup falls into recoil timer");
    AssertEqual((ushort)0x000a, phase3.Phase3NeckFunctionTimer,
        "recoil setup decrements freshly loaded $0B to $0A");
    AssertEqual((ushort)1, phase3.Phase3DisableAttacks,
        "Hyper Beam recoil disables attack selection");
    AssertEqual<MotherBrainPhase3AttackKind?>(null, recoilSetup.Phase3Attack,
        "negative RNG cannot attack while recoil disable is set");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadHyperBeamRecoilInstructionList,
        phase3.HeadInstructionList,
        "recoil installs exact `$9BE7` head animation");
    AssertEqual((ushort)0x0900, phase3.NeckAngleDelta, "Hyper Beam recoil neck delta");
    AssertEqual((ushort)8, phase3.LowerNeckMovementIndex, "Hyper Beam lower recoil index");
    AssertEqual((ushort)8, phase3.UpperNeckMovementIndex, "Hyper Beam upper recoil index");
    AssertEqual((ushort)0x0032, phase3.BrainMainShakeTimer,
        "Hyper Beam recoil seeds brain shake timer fifty");

    int recoilTimerCalls = 0;
    while (phase3.Phase3NeckPhase == MotherBrainPhase3NeckPhase.HyperBeamRecoil)
    {
        phase3.Step(bus, phase3Samus, 0, 0);
        recoilTimerCalls++;
    }
    AssertEqual(11, recoilTimerCalls,
        "remaining `$0A` recoil timer expires only after zero underflows");
    AssertEqual(MotherBrainPhase3NeckPhase.SetupRecoilRecovery, phase3.Phase3NeckPhase,
        "recoil expiration defers recovery setup to following call");
    AssertEqual((ushort)0, phase3.Phase3DisableAttacks,
        "recoil expiration reenables attacks before recovery setup");

    phase3.Step(bus, phase3Samus, 0, 0);
    AssertEqual(MotherBrainPhase3NeckPhase.RecoilRecovery, phase3.Phase3NeckPhase,
        "recovery setup falls through into its timer");
    AssertEqual((ushort)0x000f, phase3.Phase3NeckFunctionTimer,
        "recovery setup decrements freshly loaded $10 to $0F");
    int recoveryTimerCalls = 0;
    while (phase3.Phase3NeckPhase == MotherBrainPhase3NeckPhase.RecoilRecovery)
    {
        phase3.Step(bus, phase3Samus, 0, 0);
        recoveryTimerCalls++;
    }
    AssertEqual(16, recoveryTimerCalls,
        "remaining `$0F` recovery timer expires only after zero underflows");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingFourOnionRingsPhase3InstructionList,
        phase3.HeadInstructionList,
        "recovery expiration installs phase-three four-ring head list");
    AssertEqual(MotherBrainPhase3NeckPhase.Normal, phase3.Phase3NeckPhase,
        "recovery expiration defers normal-neck initializer");
    phase3.Step(bus, phase3Samus, 0, 0);
    AssertEqual(MotherBrainPhase3NeckPhase.Inactive, phase3.Phase3NeckPhase,
        "following call consumes one-shot normal-neck initializer");

    // Attack selection is independent of the movement request produced earlier in the same
    // AI call. A fresh fixture stays standing because this direct test intentionally does
    // not advance its newly requested body list until after observing both producers.
    var phase3Attack = new MotherBrainRainbowBeamAttackSequence();
    phase3Attack.Body.XPosition = 0x0070;
    phase3Attack.BeginPhase3RecoveryFromBabyCutscene();
    phase3Attack.Step(bus, phase3Samus, 0, 0);
    for (int call = 0; call < 32; call++)
        phase3Attack.Step(bus, phase3Samus, 0, 0);
    MotherBrainRainbowBeamAttackStepResult bombSelection =
        phase3Attack.Step(bus, phase3Samus, 0, 0, randomNumberSeed: 0x8000);
    AssertTrue(bombSelection.BodyWalkRequested,
        "phase-three setup fallthrough can request movement and attack together");
    AssertEqual(MotherBrainPhase3AttackKind.Bomb, bombSelection.Phase3Attack,
        "negative RNG with low byte below $80 selects bomb");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingBombPhase3InstructionList,
        phase3Attack.HeadInstructionList, "phase-three bomb installs exact `$9F00` list");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown,
        phase3Attack.Phase, "phase-three attack installs cooldown function");
    AssertEqual((ushort)0x0040, phase3Attack.FunctionTimer,
        "phase-three attack cooldown starts at literal $40");

    int cooldownCalls = 0;
    while (phase3Attack.Phase ==
           MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown)
    {
        phase3Attack.Step(bus, phase3Samus, 0, 0);
        cooldownCalls++;
    }
    AssertEqual(65, cooldownCalls, "$40 attack cooldown expires on underflow call 65");
    MotherBrainRainbowBeamAttackStepResult ringsSelection =
        phase3Attack.Step(bus, phase3Samus, 0, 0, randomNumberSeed: 0x8080);
    AssertEqual(MotherBrainPhase3AttackKind.FourOnionRings, ringsSelection.Phase3Attack,
        "negative RNG with low byte exactly $80 selects four rings");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingFourOnionRingsPhase3InstructionList,
        phase3Attack.HeadInstructionList, "phase-three rings install exact `$9DBB` list");

    // Drive a separate healthy phase-three actor into zero health through the public generic-
    // damage boundary. The combat function only installs `$AEE1`; it must not perform any of
    // the death function's property writes or movement on that same AI call.
    var death = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 0x0050,
        BrainYPosition = 0x0060,
    };
    death.Body.XPosition = 0x0040;
    death.Body.YPosition = 0x0064;
    death.InitializeCorpseRotting(bus);

    // Head initialization creates entries `(47,0)` through `(0,94)` in native WRAM.
    // Check every entry, not merely the endpoints, because one reversed loop direction
    // changes both the visible dissolve order and its 118-call completion time.
    for (int entryIndex = 0; entryIndex < MotherBrainCorpseRottingState.EntryCount; entryIndex++)
    {
        AssertEqual(
            new MotherBrainCorpseRotEntry(
                YOffset: unchecked((short)(47 - entryIndex)),
                Timer: unchecked((ushort)(entryIndex * 2))),
            death.CorpseRotting.ReadEntry(bus, entryIndex),
            $"corpse rot-table entry {entryIndex}");
    }

    // Verify all copied bytes plus the deliberately untouched `$20`-byte gaps in the first
    // four `$E0`-byte rows. This catches the tempting but wrong six-uniform-row extraction.
    for (int row = 0; row < 6; row++)
    {
        int copiedLength = row < 4 ? 0x00c0 : 0x00e0;
        int sourceOffset = 0x00c0 + row * 0x0200;
        int destinationOffset = row * 0x00e0;
        for (int byteIndex = 0; byteIndex < copiedLength; byteIndex++)
        {
            AssertEqual(
                bus.ReadByte(0xb7ce00 + sourceOffset + byteIndex),
                bus.ReadByte(
                    MotherBrainCorpseRottingState.GraphicsBufferAddress +
                    destinationOffset + byteIndex),
                $"corpse graphics extraction row {row} byte ${byteIndex:X2}");
        }
        for (int byteIndex = copiedLength; byteIndex < 0x00e0; byteIndex++)
        {
            AssertEqual(
                (byte)0,
                bus.ReadByte(
                    MotherBrainCorpseRottingState.GraphicsBufferAddress +
                    destinationOffset + byteIndex),
                $"corpse graphics untouched gap row {row} byte ${byteIndex:X2}");
        }
    }
    death.BeginPhase3RecoveryFromBabyCutscene();
    death.Step(bus, phase3Samus, 0, 0);
    for (int call = 0; call < 33; call++)
        death.Step(bus, phase3Samus, 0, 0);
    death.ApplyCalculatedBrainDamage(0x0bb8);
    MotherBrainRainbowBeamAttackStepResult deathHandoff =
        death.Step(bus, phase3Samus, 0, 0);
    AssertEqual((ushort)0, death.BrainHealth, "calculated damage saturates brain health at zero");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom,
        deathHandoff.PhaseAfter, "zero-health combat call installs `$AEE1` without fallthrough");
    AssertTrue(death.HitboxesEnabled,
        "death handoff defers `$AEE1` hitbox clear to following actor call");

    var deathRandom = new Bank80SystemState(0x0061);
    MotherBrainRainbowBeamAttackStepResult firstDeathMove = death.Step(
        bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
    AssertTrue(firstDeathMove.BodyWalkRequested, "death requests medium backward body list");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardMediumInstructionList,
        death.Body.InstructionPointer, "death retreat selects exact `$9900` list");
    AssertEqual((ushort)0x0400, death.BodyProperties, "death sets raw body property `$0400`");
    AssertEqual((ushort)0x0400, death.BrainProperties, "death sets raw brain property `$0400`");
    AssertTrue(!death.HitboxesEnabled, "death entry disables shared hitboxes");

    // The medium backward program has the same net -24 motion as every admitted walk list.
    // Run its real command stream to sleeping rather than assigning the `$28` destination.
    int deathRetreatBodyCalls = 0;
    while (!death.Body.Sleeping)
    {
        death.Body.Step(bus);
        deathRetreatBodyCalls++;
        AssertTrue(deathRetreatBodyCalls < 100, "death retreat body bytecode sleeps");
    }
    AssertEqual((ushort)0x0028, death.Body.XPosition, "death retreat reaches back-room X `$28`");

    MotherBrainRainbowBeamAttackStepResult firstSmokyBatch = death.Step(
        bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding,
        death.Phase, "back-room carry falls through into smoky idle");
    AssertEqual((ushort)0x007f, death.FunctionTimer,
        "same-call smoky idle decrements freshly loaded `$80`");
    AssertEqual((ushort)0x0010, death.DeathExplosionIntervalTimer,
        "zero death-explosion timer emits immediately and reloads smoky interval `$10`");
    AssertEqual((ushort)6, death.DeathExplosionIndex,
        "zero death-explosion index wraps backward to record six");
    AssertEqual(2, firstSmokyBatch.DeathExplosions.Count,
        "smoky generator emits two simultaneous projectiles");
    AssertEqual(new MotherBrainDeathExplosionRequest(
            PatternIndex: 6,
            XOffset: 0x000a,
            YOffset: -0x001f,
            XPosition: 0x0032,
            YPosition: 0x0045,
            ProjectileParameter: 1,
            SoundEffect: 0x0013),
        firstSmokyBatch.DeathExplosions[0],
        "first smoky projectile uses record-six pair zero and smoke parameter");
    AssertEqual((short)-0x0014, firstSmokyBatch.DeathExplosions[1].XOffset,
        "second smoky projectile advances to record-six pair one");

    int smokyIdleCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        smokyIdleCalls++;
        AssertTrue(smokyIdleCalls < 200, "smoky idle timer reaches stumble");
    }
    AssertEqual(128, smokyIdleCalls,
        "remaining `$7F` smoky-idle timer expires only after zero underflows");

    // `$AF21` can require multiple complete forward programs because the carry test uses
    // strict signed overshoot. Advance the actor and instruction stages in native order.
    int stumbleCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStumbleToMiddleOfRoom)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        death.Body.Step(bus);
        stumbleCalls++;
        AssertTrue(stumbleCalls < 200, "death stumble reaches middle-room target");
    }
    AssertEqual((ushort)0x006d, death.Body.XPosition,
        "third really-fast program reports carry on its mid-list +15px crossing");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadDyingDroolInstructionList,
        death.HeadInstructionList, "stumble completion installs dying-drool head list");
    AssertEqual((ushort)0x0020, death.FunctionTimer,
        "stumble completion loads brain-effects delay `$20`");

    int disableEffectsCalls = 0;
    MotherBrainRainbowBeamAttackStepResult disableResult = default;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects)
    {
        disableResult = death.Step(
            bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        // Enemy instruction processing remains independent after body AI changes function;
        // finish the still-visible third walk exactly as the following native slot stage does.
        death.Body.Step(bus);
        disableEffectsCalls++;
        AssertTrue(disableEffectsCalls < 50, "brain-effects timer reaches body fade");
    }
    AssertEqual(33, disableEffectsCalls, "brain-effects `$20` timer expires on call 33");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody,
        death.Phase, "disable/setup functions fall through into first body-fade call");
    AssertEqual((ushort)1, death.GreyTransitionCounter,
        "same-call fade performs black palette record zero");
    AssertEqual((ushort)0x0010, death.FunctionTimer,
        "same-call fade reloads palette cadence `$10`");
    AssertTrue(disableResult.PaletteRequested,
        "disable/fade fallthrough exposes palette-copy work");
    AssertTrue(!death.DroolGenerationEnabled && !death.SmallPurpleBreathGenerationEnabled &&
               !death.BrainPaletteHandlingEnabled && !death.HealthBasedPaletteHandlingEnabled,
        "death disables all four brain-effect producers");
    AssertEqual((ushort)0x0e00, death.BrainPaletteIndex,
        "death forces sprite palette-seven index `$0E00`");
    AssertEqual((ushort)0x0070, death.Body.XPosition,
        "in-flight third walk finishes to `$70` during brain-effects delay");

    int fadeBodyCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        fadeBodyCalls++;
        AssertTrue(fadeBodyCalls < 350, "body palette reaches black terminator");
    }
    AssertEqual(272, fadeBodyCalls,
        "remaining sixteen black-table probes use exact seventeen-call cadence");
    AssertEqual((ushort)17, death.GreyTransitionCounter,
        "black fade consumes sixteen records plus null entry");
    AssertTrue(death.EnemyBg2TilemapClearRequested,
        "black terminator requests native `$02C6..0` BG2 clear");
    AssertEqual((ushort)0x0500, death.BodyProperties,
        "black terminator preserves `$0400`, sets `$0100`, and clears `$2000`");

    int finalExplosionCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFinalFewExplosions)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        finalExplosionCalls++;
    }
    AssertEqual(17, finalExplosionCalls,
        "final-explosion `$10` timer expires only after zero underflows");

    MotherBrainRainbowBeamAttackStepResult firstFall = death.Step(
        bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround,
        firstFall.PhaseAfter, "decapitation falls through into brain motion");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadDecapitatedInstructionList,
        death.HeadInstructionList, "decapitation installs exact `$9C29` head list");
    AssertTrue(death.BrainDrawSetupRequested,
        "decapitation publishes separate brain draw-setup request");
    AssertEqual((ushort)0x0020, death.FunctionTimer,
        "first 8.8 falling call accelerates from zero to `$0020`");
    AssertEqual((ushort)0x0060, death.BrainYPosition,
        "first falling velocity has zero whole-pixel displacement");

    int remainingFallCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        remainingFallCalls++;
        AssertTrue(remainingFallCalls < 100, "8.8 brain fall reaches floor");
    }
    AssertEqual(42, remainingFallCalls,
        "brain reaches `$C4` after 43 total 8.8 integration calls");
    AssertEqual((ushort)0x00c4, death.BrainYPosition, "brain fall clamps to floor Y `$C4`");
    AssertEqual((ushort)2, death.EarthquakeType, "brain floor hit requests earthquake type two");
    AssertEqual((ushort)20, death.EarthquakeTimer, "brain floor hit requests twenty frames");
    AssertEqual((ushort)0x0100, death.FunctionTimer,
        "brain floor hit seeds corpse-load scratch timer `$0100`");

    var corpseTransfers = new List<MotherBrainSpriteTileTransferRequest>();
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles)
    {
        MotherBrainRainbowBeamAttackStepResult transfer = death.Step(
            bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        AssertTrue(transfer.SpriteTileTransfer is not null, "corpse-load call emits one DMA record");
        corpseTransfers.Add(transfer.SpriteTileTransfer!.Value);
    }
    AssertEqual(6, corpseTransfers.Count, "corpse transfer list has six records");
    for (int index = 0; index < corpseTransfers.Count; index++)
    {
        AssertEqual(unchecked((uint)(0xb7ce00 + index * 0x200)),
            corpseTransfers[index].SourceAddress, $"corpse DMA {index} source");
        AssertEqual(unchecked((ushort)(0x7a00 + index * 0x100)),
            corpseTransfers[index].VramDestination, $"corpse DMA {index} destination");
        AssertEqual((ushort)0x01c0, corpseTransfers[index].Size,
            $"corpse DMA {index} skips two source rows");
    }
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey,
        death.Phase, "sixth corpse DMA observes terminator on same call");

    int greySetupCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        greySetupCalls++;
    }
    AssertEqual(33, greySetupCalls, "corpse-grey setup `$20` expires on call 33");
    int greyFadeCalls = 0;
    int greyPaletteCopies = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey)
    {
        MotherBrainRainbowBeamAttackStepResult grey = death.Step(
            bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        if (grey.PaletteRequested)
            greyPaletteCopies++;
        greyFadeCalls++;
        AssertTrue(greyFadeCalls < 180, "corpse-grey table reaches terminator");
    }
    AssertEqual(137, greyFadeCalls, "eight grey records and terminator use native cadence");
    AssertEqual(8, greyPaletteCopies, "real-death grey transition copies eight palettes");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadCorpseInstructionList,
        death.HeadInstructionList, "grey terminator installs exact `$9D25` corpse list");
    AssertEqual((ushort)0x0100, death.FunctionTimer, "corpse tip-over delay starts at `$100`");

    int corpseTipCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseTipsOver)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        corpseTipCalls++;
    }
    AssertEqual(257, corpseTipCalls, "corpse `$100` display delay expires on call 257");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway,
        death.Phase, "tip-over reaches shared corpse-rotting engine");

    // `$DB12` processes all 48 entries on every call. The staggered timers finish exactly
    // one entry on each of 48 irregularly spaced calls; carry stays set for the first 117
    // calls and the final entry returns carry clear on call 118 without queuing VRAM work.
    int corpseRottingCalls = 0;
    int corpseDustCount = 0;
    MotherBrainCorpseDustRequest? firstCorpseDust = null;
    MotherBrainCorpseDustRequest? lastCorpseDust = null;
    MotherBrainRainbowBeamAttackStepResult corpseFinished = default;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway)
    {
        corpseRottingCalls++;
        MotherBrainRainbowBeamAttackStepResult rot = death.Step(
            bus,
            phase3Samus,
            enemyFrameCounter: 0,
            mainEnemyExecutionCounter: unchecked((ushort)(corpseRottingCalls - 1)),
            randomNumberSeed: 0x1234,
            nextRandomNumber: deathRandom.NextRandom);

        foreach (MotherBrainCorpseDustRequest dust in rot.CorpseDustRequests)
        {
            firstCorpseDust ??= dust;
            lastCorpseDust = dust;
            corpseDustCount++;
        }

        if (corpseRottingCalls < 118)
        {
            AssertEqual(6, rot.CorpseRottingVramTransfers.Count,
                $"active corpse rot call {corpseRottingCalls} queues all six DMA records");
            ushort[] sizes = [0x0060, 0x00a0, 0x00c0, 0x00c0, 0x00e0, 0x00e0];
            uint[] sources = [0x7e9040, 0x7e9100, 0x7e91c0, 0x7e92a0, 0x7e9380, 0x7e9460];
            ushort[] destinations = [0x7a80, 0x7b70, 0x7c60, 0x7d60, 0x7e60, 0x7f60];
            for (int transferIndex = 0; transferIndex < 6; transferIndex++)
            {
                AssertEqual(
                    new MotherBrainSpriteTileTransferRequest(
                        unchecked((ushort)transferIndex),
                        sizes[transferIndex],
                        sources[transferIndex],
                        destinations[transferIndex]),
                    rot.CorpseRottingVramTransfers[transferIndex],
                    $"corpse rot DMA {transferIndex} on call {corpseRottingCalls}");
            }
            AssertTrue(!rot.MusicStopQueued && !rot.EscapeMusicQueued,
                "active corpse rot does not queue escape music early");
        }
        else
        {
            corpseFinished = rot;
            AssertEqual(0, rot.CorpseRottingVramTransfers.Count,
                "carry-clear corpse completion skips `$E1F4` VRAM records");
        }

        AssertTrue(corpseRottingCalls <= 118, "corpse rotting reaches final table entry");
    }
    AssertEqual(118, corpseRottingCalls, "48 staggered corpse rows finish on exact call 118");
    AssertEqual((uint)118, death.CorpseRotting.ProcessCallCount,
        "corpse processor publishes native call count");
    AssertEqual(48, corpseDustCount, "every rot entry runs Mother Brain's dust hook once");
    AssertEqual((uint)48, death.CorpseRotting.FinishedEntryCount,
        "corpse processor publishes all finished entries");
    AssertEqual(
        new MotherBrainCorpseDustRequest(0, 0x0054, 0x00d4, 0x000a, true, 0x0010),
        firstCorpseDust!.Value,
        "entry zero finishes on call one using sampled RNG and execution-counter SFX gate");
    AssertEqual(
        new MotherBrainCorpseDustRequest(47, 0x0054, 0x00d4, 0x000a, false, 0x0010),
        lastCorpseDust!.Value,
        "entry 47 finishes last without advancing sampled RNG");
    AssertTrue(corpseFinished.MusicStopQueued && corpseFinished.EscapeMusicQueued,
        "rotting completion queues `$0000` then `$FF24`");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay,
        death.Phase, "rotting completion installs `$B211` delay function");
    AssertEqual((ushort)0x0013, death.FunctionTimer,
        "completion falls through and decrements freshly loaded `$14`");
    AssertEqual((ushort)0x0500, death.BrainProperties,
        "completion preserves `$0400`, sets `$0100`, and clears `$2000`");
    AssertEqual((ushort)0, death.BrainProperties2,
        "completion clears brain property word two");

    // All eligible bitplane rows must eventually be moved out and cleared. Check the entire
    // `$540`-byte staging buffer so a missing column gate or one-plane clear cannot hide.
    for (int byteOffset = 0; byteOffset < MotherBrainCorpseRottingState.GraphicsBufferSize; byteOffset++)
    {
        AssertEqual(
            (byte)0,
            bus.ReadByte(MotherBrainCorpseRottingState.GraphicsBufferAddress + byteOffset),
            $"fully rotted corpse graphics byte ${byteOffset:X3}");
    }

    int postRotDelayCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay)
    {
        death.Step(bus, phase3Samus, 0, 0);
        postRotDelayCalls++;
        AssertTrue(postRotDelayCalls <= 20, "post-rot `$14` delay reaches escape tile load");
    }
    AssertEqual(20, postRotDelayCalls,
        "same-call first decrement leaves exactly twenty later delay calls");
    AssertEqual((ushort)0, death.BrainXPosition, "post-rot delay parks brain X at zero");
    AssertEqual((ushort)0, death.BrainYPosition, "post-rot delay parks brain Y at zero");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadEscapeTimerTiles,
        death.Phase, "post-rot delay reaches explicit escape-timer tile seam");

    MotherBrainSpriteTileTransferRequest[] expectedEscapeTimerTransfers =
    [
        new(0, 0x0200, 0xb0c000, 0x7e00),
        new(1, 0x0120, 0xb0c200, 0x7f00),
        new(2, 0x0200, 0xb7da00, 0x7820),
        new(3, 0x0200, 0xb7dc00, 0x7920),
        new(4, 0x0200, 0xb7de00, 0x7a20),
        new(5, 0x0200, 0xb7e000, 0x7b20),
        new(6, 0x0100, 0xb7e200, 0x7c20),
    ];
    for (int transferIndex = 0; transferIndex < expectedEscapeTimerTransfers.Length; transferIndex++)
    {
        MotherBrainRainbowBeamAttackStepResult escapeTiles =
            death.Step(bus, phase3Samus, 0, 0);
        AssertEqual(expectedEscapeTimerTransfers[transferIndex],
            escapeTiles.EscapeSequenceTileTransfers[0],
            $"escape timer tile DMA {transferIndex}");
        if (transferIndex < expectedEscapeTimerTransfers.Length - 1)
        {
            AssertEqual(1, escapeTiles.EscapeSequenceTileTransfers.Count,
                "nonfinal escape-timer call emits exactly one record");
        }
        else
        {
            // `$B26A` falls through: final timer text and first exploded-door page share
            // one AI call even though both use the same global transfer-list cursor.
            AssertEqual(2, escapeTiles.EscapeSequenceTileTransfers.Count,
                "final escape-timer call also emits first exploded-door record");
            AssertEqual(new MotherBrainSpriteTileTransferRequest(0, 0x0200, 0xabf400, 0x7000),
                escapeTiles.EscapeSequenceTileTransfers[1],
                "same-call first exploded-door DMA");
        }
    }
    AssertEqual((ushort)7, death.EscapeTimerTileTransferIndex,
        "escape-timer list consumes seven NTSC records");
    AssertEqual((ushort)1, death.ExplodedDoorTileTransferIndex,
        "escape-timer fallthrough consumes exploded-door record zero");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape,
        death.Phase, "first door record leaves `$B26D` active");

    MotherBrainRainbowBeamAttackStepResult escapeStarted =
        death.Step(bus, phase3Samus, 0, 0);
    AssertEqual(1, escapeStarted.EscapeSequenceTileTransfers.Count,
        "second start-escape call emits one door record");
    AssertEqual(new MotherBrainSpriteTileTransferRequest(1, 0x0200, 0xabf600, 0x7100),
        escapeStarted.EscapeSequenceTileTransfers[0],
        "second exploded-door DMA");
    AssertTrue(escapeStarted.ExplodedDoorPaletteRequested,
        "door-list terminator requests fourteen-color exploded-door palette copy");
    AssertTrue(escapeStarted.EscapeMusicTrackQueued,
        "door-list terminator queues escape music track seven");
    AssertEqual((ushort)5, death.EarthquakeType, "escape start selects earthquake type five");
    AssertEqual((ushort)0xffff, death.EarthquakeTimer,
        "escape start holds earthquake with `$FFFF`");
    AssertEqual(4, escapeStarted.EscapePaletteFxRequests.Count,
        "escape start spawns all four Tourian red-flash palette objects");
    AssertEqual((ushort)0xffc9, escapeStarted.EscapePaletteFxRequests[0],
        "escape palette FX begins with shutter-red object");
    AssertEqual((ushort)0xffd5, escapeStarted.EscapePaletteFxRequests[3],
        "escape palette FX ends with Arkanoid/red-orb object");
    AssertTrue(!death.MotherBrainUnpauseHookEnabled,
        "escape typewriter disables Mother Brain unpause hook");
    AssertTrue(escapeStarted.EscapeTypewriterSetupRequested,
        "escape start requests native Zebes typewriter setup");
    AssertEqual((ushort)0x0020, death.FunctionTimer,
        "escape text handoff loads `$20` subtitle/typewriter timer");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText,
        death.Phase, "default NTSC text selection reaches explicit `$B2E3` seam");

    MotherBrainRainbowBeamAttackStepResult typing = death.Step(bus, phase3Samus, 0, 0);
    AssertTrue(typing.TypewriterStepRequested,
        "`$B2E3` requests one external typewriter step on every call");
    AssertEqual<ushort?>(0x2610, typing.TypewriterTextPointer,
        "`$B2E3` publishes exact Zebes escape text-list pointer");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText,
        death.Phase, "clear typewriter carry keeps `$B2E3` active");

    MotherBrainRainbowBeamAttackStepResult typewriterComplete = death.Step(
        bus,
        phase3Samus,
        0,
        0,
        typewriterFinished: true);
    AssertTrue(typewriterComplete.TypewriterStepRequested,
        "completion call still records the `$2610` typewriter invocation");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer,
        death.Phase, "typewriter carry installs door-explosion countdown");
    AssertEqual((ushort)0x0020, death.FunctionTimer,
        "typewriter completion reloads exact `$20` door timer");

    // `$B346` advances global RNG only when its shared interval underflows. Alternate the
    // two sides of the literal `$4000` comparison, then verify the four-position descending
    // cycle independently from however much of the earlier death interval remained.
    int escapeRngCalls = 0;
    int doorTimerCalls = 0;
    var emittedDoorExplosions = new List<MotherBrainEscapeDoorExplosionRequest>();
    MotherBrainRainbowBeamAttackStepResult doorTimerResult = default;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer)
    {
        doorTimerResult = death.Step(
            bus,
            phase3Samus,
            0,
            0,
            nextRandomNumber: () =>
            {
                ushort sampled = (escapeRngCalls & 1) == 0 ? (ushort)0x3fff : (ushort)0x4000;
                escapeRngCalls++;
                return sampled;
            });
        if (doorTimerResult.EscapeDoorExplosion is { } emitted)
            emittedDoorExplosions.Add(emitted);
        doorTimerCalls++;
        AssertTrue(doorTimerCalls <= 33, "door timer reaches underflow within 33 calls");
    }
    AssertEqual(33, doorTimerCalls, "`$20` door countdown accepts zero and expires on call 33");
    AssertEqual(escapeRngCalls, emittedDoorExplosions.Count,
        "door producer advances RNG exactly once per emitted projectile");
    AssertTrue(emittedDoorExplosions.Count >= 6,
        "33-call door countdown exposes repeated five-call explosion cadence");
    for (int explosionIndex = 0; explosionIndex < emittedDoorExplosions.Count; explosionIndex++)
    {
        MotherBrainEscapeDoorExplosionRequest emitted = emittedDoorExplosions[explosionIndex];
        ushort expectedPattern = unchecked((ushort)(
            (emittedDoorExplosions[0].PatternIndex - explosionIndex) & 3));
        AssertEqual(expectedPattern, emitted.PatternIndex,
            $"door explosion {explosionIndex} cycles 3,2,1,0");
        AssertEqual(explosionIndex % 2 == 0 ? (ushort)0x000c : (ushort)0x0003,
            emitted.ProjectileParameter,
            $"door explosion {explosionIndex} honors `$4000` RNG boundary");
        AssertEqual((ushort)0x0024, emitted.SoundEffect,
            $"door explosion {explosionIndex} queues sound `$24`");
    }
    AssertTrue(doorTimerResult.TimerHandlingEnableRequested,
        "timer expiry publishes Samus command `$0F`");
    AssertTrue(doorTimerResult.MotherBrainEscapeTimerStartRequested,
        "timer expiry publishes TimerStatus `$0002`");
    AssertTrue(doorTimerResult.MotherBrainBossBitRequested,
        "timer expiry publishes current-area mini-boss bit `$02`");
    AssertTrue(doorTimerResult.ZebesTimebombEventRequested,
        "timer expiry publishes event `$0E`");
    AssertEqual((ushort)0, death.DeathExplosionIntervalTimer,
        "timer expiry clears reused explosion interval");
    AssertEqual((ushort)0, death.EscapeDoorIndex,
        "timer expiry clears door explosion index");

    MotherBrainRainbowBeamAttackStepResult blownDoor = death.Step(bus, phase3Samus, 0, 0);
    AssertEqual(8, blownDoor.EscapeDoorParticleSpawns.Count,
        "door blow-up attempts all eight particle allocations in one call");
    for (ushort parameter = 0; parameter < 8; parameter++)
    {
        AssertEqual(new MotherBrainEscapeDoorParticleSpawnRequest(parameter),
            blownDoor.EscapeDoorParticleSpawns[parameter],
            $"door fragment spawn parameter {parameter}");
    }
    AssertEqual(new MotherBrainEscapeDoorPlmRequest(0x00, 0x06, 0xb677),
        blownDoor.EscapeDoorPlm!.Value,
        "door blow-up requests native hardcoded PLM location and entry");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceKeepEarthquakeGoing,
        death.Phase, "door blow-up reaches final Mother Brain body function");

    MotherBrainRainbowBeamAttackStepResult nonzeroQuake = death.Step(
        bus, phase3Samus, 0, 0, globalEarthquakeTimer: 1);
    AssertTrue(!nonzeroQuake.EarthquakeTimerRefreshed,
        "final body function leaves nonzero global earthquake timer alone");
    AssertEqual((ushort)1, death.EarthquakeTimer,
        "final body function mirrors nonzero global quake sample");
    MotherBrainRainbowBeamAttackStepResult zeroQuake = death.Step(
        bus, phase3Samus, 0, 0, globalEarthquakeTimer: 0);
    AssertTrue(zeroQuake.EarthquakeTimerRefreshed,
        "final body function changes visible zero to `$FFFF`");
    AssertEqual((ushort)0xffff, death.EarthquakeTimer,
        "final body function holds earthquake indefinitely");

    Console.WriteLine(
        "  Baby Metroid: death movement, corpse rotting, escape DMA, timer handoff, and door blow-up agree.");
}

/// <summary>
/// Verifies `$A9:9F00`'s phase-three head bytecode and `$86:CB59`'s complete bomb lifecycle:
/// initialization, 8.8 motion, nine bounce-table stages, animation, both deletion paths,
/// afterburn/dust/sound requests, and the Mother Brain body-owned active-bomb counter.
/// </summary>
}
