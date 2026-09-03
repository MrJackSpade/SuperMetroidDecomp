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

/// <summary>Mother Brain rainbow-beam movement and attack-sequence verification.</summary>
static void VerifyMotherBrainRainbowBeamSamusMovement()
{
    var bus = new TestAddressSpace();

    // `$86:C272` uses table index angle+$40. Give angle zero a literal +$0100 entry and
    // angle $80 a literal -$0100 entry so the expected $10.00 components are unambiguous.
    WriteTestWord(bus, 0xa0b443 + 0x40 * 2, 0x0100);
    WriteTestWord(bus, 0xa0b443 + 0xc0 * 2, 0xff00);

    var movement = new MotherBrainRainbowBeamSamusMovement();
    var samus = new SamusState
    {
        XPosition = 100,
        YPosition = 100,
    };

    // Begin-fall seeds -$01.00 X and zero Y. The first call changes those to -$00.FE and
    // +$00.18. Adding fractional FE to an existing FF must carry into the signed whole
    // delta: -1+1 is zero, while the untouched low subposition bytes remain AA/BB.
    samus.Kinematics.XSubposition = 0xffaa;
    samus.Kinematics.YSubposition = 0xf0bb;
    movement.BeginFallingAfterRainbowBeam();
    MotherBrainForcedSamusMovementResult first =
        movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual(0xff02, movement.CustomXVelocity, "rainbow fall first eased X velocity");
    AssertEqual(0x0018, movement.CustomYVelocity, "rainbow fall first accelerated Y velocity");
    AssertEqual(100, samus.XPosition, "rainbow fall signed X plus fractional carry");
    AssertEqual(0x01aa, samus.Kinematics.XSubposition, "rainbow fall preserves X low sub-byte");
    AssertEqual(101, samus.YPosition, "rainbow fall Y fractional carry");
    AssertEqual(0x08bb, samus.Kinematics.YSubposition, "rainbow fall preserves Y low sub-byte");
    AssertEqual(first.After, first.CameraPreviousPosition,
        "forced movement publishes new position as camera previous");
    AssertTrue(!first.NativeCarry && !first.ReachedVerticalBoundary,
        "unclamped first fall returns clear vertical carry");

    // Exactly 127 more +$0002 updates carry the negative X word to zero. Native clamps it
    // there instead of allowing a positive recoil; subsequent calls must remain zero.
    for (int call = 1; call < 128; call++)
        movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual(0, movement.CustomXVelocity, "rainbow fall X easing clamps at zero");
    movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual(0, movement.CustomXVelocity, "rainbow fall X easing stays zero");
    AssertEqual(0x00c0, samus.YPosition, "rainbow fall clamps at arena floor Y $C0");
    AssertEqual(0, samus.Kinematics.YSubposition, "arena floor clamp clears Y subposition");

    // At/below $7C, `$A9:BBCF` chooses +$00.40. It is not a snap: this deliberately
    // crosses from $7C.D0 to $7D.10 through the eight-bit fractional carry.
    samus.YPosition = 0x007c;
    samus.Kinematics.YSubposition = 0xd055;
    MotherBrainForcedSamusMovementResult middleDown = MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus);
    AssertEqual(0x0040, middleDown.YVelocity, "middle-wall below target velocity");
    AssertEqual(0x007d, samus.YPosition, "middle-wall downward whole carry");
    AssertEqual(0x1055, samus.Kinematics.YSubposition, "middle-wall downward fraction");

    // Above $7C, two's-complement $FFC0 moves upward. $7D.10 + (-$00.40) becomes $7C.D0.
    MotherBrainForcedSamusMovementResult middleUp = MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus);
    AssertEqual(0xffc0, middleUp.YVelocity, "middle-wall above target velocity");
    AssertEqual(0x007c, samus.YPosition, "middle-wall upward whole borrow");
    AssertEqual(0xd055, samus.Kinematics.YSubposition, "middle-wall upward fraction");

    // Angle zero reads +$0100 at index $40. Speed $1000 * sine $0100 >> 8 therefore
    // produces +$1000 on Y, while the independent horizontal helper also adds $10 pixels.
    samus.XPosition = 100;
    samus.Kinematics.XSubposition = 0x0022;
    samus.YPosition = 100;
    samus.Kinematics.YSubposition = 0x0033;
    movement.RainbowBeamAngle = 0;
    MotherBrainForcedSamusMovementResult beam = movement.MoveTowardWall(bus, samus);
    AssertEqual(0x1000, beam.YVelocity, "rainbow beam table-derived Y velocity");
    AssertEqual(116, samus.XPosition, "rainbow beam horizontal $10.00 step");
    AssertEqual(116, samus.YPosition, "rainbow beam vertical $10.00 step");
    AssertTrue(!beam.NativeCarry, "rainbow beam caller clears vertical-helper carry");

    // Reaching X $EB returns set carry and skips vertical calculation entirely.
    samus.XPosition = 0x00e0;
    samus.Kinematics.XSubposition = 0x7777;
    samus.YPosition = 100;
    MotherBrainForcedSamusMovementResult wall = movement.MoveTowardWall(bus, samus);
    AssertTrue(wall.ReachedWall && wall.NativeCarry, "rainbow beam wall clamp returns carry");
    AssertEqual(0x00eb, samus.XPosition, "rainbow beam hardcoded wall X $EB");
    AssertEqual(0, samus.Kinematics.XSubposition, "rainbow wall clamp clears X subposition");
    AssertEqual(100, samus.YPosition, "rainbow wall clamp skips vertical movement");

    // A negative table component below Y $30 proves the separate ceiling clamp and its
    // subposition clear. MoveTowardWall then clears native carry because X did not clamp.
    samus.XPosition = 100;
    samus.Kinematics.XSubposition = 0;
    samus.YPosition = 0x0030;
    samus.Kinematics.YSubposition = 0x9999;
    movement.RainbowBeamAngle = 0x80;
    MotherBrainForcedSamusMovementResult ceiling = movement.MoveTowardWall(bus, samus);
    AssertTrue(ceiling.ReachedVerticalBoundary && !ceiling.NativeCarry,
        "rainbow ceiling clamp is hidden from caller carry");
    AssertEqual(0x0030, samus.YPosition, "rainbow hardcoded ceiling Y $30");
    AssertEqual(0, samus.Kinematics.YSubposition, "rainbow ceiling clears Y subposition");

    Console.WriteLine("  Mother Brain: rainbow-beam forced 8.8 movement and arena clamps agree.");
}

/// <summary>
/// Drives the complete active `$A9:B983-$BB2D` body-function chain. Long loops are valuable
/// here: the attack's 300 drain calls and 129 decision-timer calls expose off-by-one mistakes
/// that isolated helper checks cannot see.
/// </summary>
static void VerifyMotherBrainRainbowBeamAttackSequence()
{
    var bus = new TestAddressSpace();

    // `$B7:CE00` contains two side-by-side corpse frames in the retail ROM. Give every byte
    // in the complete source window a deterministic, nonzero-heavy identity pattern so the
    // shared graphics initializer must select the six exact right-frame slices; sparse zero
    // memory would let a wrong source, length, or destination pass accidentally.
    for (int byteOffset = 0; byteOffset < 0x0c00; byteOffset++)
    {
        bus.WriteByte(
            0xb7ce00 + byteOffset,
            unchecked((byte)(byteOffset * 37 + 0x5a)));
    }

    // The active chain needs only command-five/$18's forced `$54` pose and controller-zero's
    // later `$E9` pose. These bytes are the retail direction/type/radius metadata and minimal
    // byte-indexed animation streams already proven by the dedicated drained-controller test.
    WritePoseDefinition(bus, SamusState.KnockbackLeftPose,
        [0x04, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusState.DrainedCrouchingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusState.KnockbackLeftPose * 2, 0xc020);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedCrouchingLeftPose * 2, 0xb268);
    bus.WriteBytes(0x91c020, [0x01, 0xfe, 0x01]);
    bus.WriteBytes(0x91b268, [0x02, 0x02, 0x10, 0xf7, 0x01]);

    // Exact `$A9:9818` and `$A9:993A` command/duration shapes. Spritemap operands are
    // deliberately small sentinels because this check targets the enemy interpreter; the
    // private-ROM regression separately reads the retail extended-spritemap pointers.
    ushort[] forwardReallySlow =
    [
        0x9708, 0x000a, 0x1000,
        0x95fc, 0x000a, 0x1001,
        0x960c, 0x000a, 0x1002,
        0x961c, 0x000a, 0x1003,
        0x9622, 0x000a, 0x1004,
        0x9638, 0x000a, 0x1005,
        0x9648, 0x000a, 0x1006,
        0x9658, 0x000a, 0x1007,
        0x9668, 0x9700, 0x000a, 0x1008, 0x812f,
    ];
    ushort[] backwardReallySlow =
    [
        0x9708, 0x000a, 0x1100,
        0x96f0, 0x000a, 0x1101,
        0x96e0, 0x000a, 0x1102,
        0x96d0, 0x000a, 0x1103,
        0x96ba, 0x000a, 0x1104,
        0x96aa, 0x000a, 0x1105,
        0x96a4, 0x000a, 0x1106,
        0x9694, 0x000a, 0x1107,
        0x967e, 0x9700, 0x000a, 0x1108, 0x812f,
    ];
    for (int index = 0; index < forwardReallySlow.Length; index++)
    {
        WriteTestWord(bus, 0xa99818 + index * 2, forwardReallySlow[index]);
        WriteTestWord(bus, 0xa9993a + index * 2, backwardReallySlow[index]);
    }

    // Exact command/duration topology for the three posture programs reached by the
    // finish-off loop. As above, only spritemap operands are synthetic sentinels.
    ushort[] standUpFast =
    [
        0x9718, 0x0008, 0x1200,
        0x95b6, 0x0008, 0x1201,
        0x95c0, 0x0008, 0x1202,
        0x95ca, 0x0008, 0x1203,
        0x9700, 0x812f,
    ];
    ushort[] standUpAfterLeaning =
    [
        0x9718, 0x0008, 0x1210,
        0x95ca, 0x0008, 0x1211,
        0x9700, 0x812f,
    ];
    ushort[] leanDown =
    [
        0x9718, 0x0008, 0x1220,
        0x95de, 0x9728, 0x0008, 0x1221,
        0x812f,
    ];
    WriteTestWords(bus, 0xa999c6, standUpFast);
    WriteTestWords(bus, 0xa999e2, standUpAfterLeaning);
    WriteTestWords(bus, 0xa999f2, leanDown);

    var samus = new SamusState
    {
        Health = 999,
        Missiles = 80,
        SuperMissiles = 80,
        PowerBombs = 400,
        XPosition = 220,
        YPosition = 124,
    };
    var attack = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 64,
        BrainYPosition = 96,
    };
    attack.Body.XPosition = 64;
    attack.Body.YPosition = 100;

    attack.StartActiveBeam(bus, samus);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall, attack.Phase,
        "active rainbow start installs wall-motion function");
    AssertEqual(SamusState.KnockbackLeftPose, samus.Pose,
        "active rainbow start runs native drained setup command");
    AssertEqual(DrainedGetUpHandler.AbleToStand, samus.Drained.GetUpHandler,
        "energy 999 selects command five able handler");
    AssertTrue(samus.InputLocked, "rainbow start locks Samus input handlers");
    AssertTrue(attack.HdmaActive, "rainbow start requests active HDMA beam");
    AssertEqual(0x0200, attack.AngularWidth, "rainbow start width");

    MotherBrainRainbowBeamAttackStepResult wall = attack.Step(
        bus, samus, enemyFrameCounter: 2, mainEnemyExecutionCounter: 1);
    AssertEqual(0x00eb, samus.XPosition, "actor sequence moves Samus to hardcoded wall");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.OneFrameDelay, attack.Phase,
        "wall carry installs one-frame-delay function");
    AssertTrue(wall.SoundQueued && wall.PaletteRequested,
        "wall function runs sound and bit-one palette cadence");
    AssertEqual(0x0380, wall.AngularWidth, "wall function widens before aiming");
    AssertTrue(wall.Explosion is { XOffset: 6, YOffset: 2, SoundEffect: 0x24 },
        "zero explosion timer increments to literal offset record one");

    MotherBrainRainbowBeamAttackStepResult delay = attack.Step(
        bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 2);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.StartDrainingSamus, attack.Phase,
        "zero delay timer underflows and schedules drain initializer");
    AssertEqual(8, delay.EarthquakeType, "delay underflow selects earthquake type eight");
    AssertEqual(8, delay.EarthquakeTimer, "delay underflow seeds eight-frame earthquake");

    int drainCalls = 0;
    int queuedBeamSounds = (wall.SoundQueued ? 1 : 0) + (delay.SoundQueued ? 1 : 0);
    int explosions = wall.Explosion is null ? 0 : 1;
    while (attack.Phase is MotherBrainRainbowBeamAttackPhase.StartDrainingSamus or
           MotherBrainRainbowBeamAttackPhase.DrainingSamus)
    {
        MotherBrainRainbowBeamAttackStepResult drain = attack.Step(
            bus,
            samus,
            enemyFrameCounter: unchecked((ushort)drainCalls),
            mainEnemyExecutionCounter: unchecked((ushort)drainCalls));
        drainCalls++;
        if (drain.SoundQueued)
            queuedBeamSounds++;
        if (drain.Explosion is not null)
            explosions++;
    }

    AssertEqual(300, drainCalls, "$012B drain timer includes fallthrough call and expires after 300");
    AssertEqual(399, samus.Health, "300 no-Varia rainbow hits subtract two each");
    AssertEqual(5, samus.Missiles, "missiles decrement every fourth enemy pass");
    AssertEqual(5, samus.SuperMissiles, "supers share every-fourth cadence");
    AssertEqual(100, samus.PowerBombs, "power bombs decrement every drain call");
    AssertEqual(7, queuedBeamSounds, "count six queues seven rainbow beam sound attempts");
    AssertTrue(explosions > 1, "explosion timer continues across wall and drain phases");
    AssertEqual(0x0c00, attack.AngularWidth, "drain widening clamps at $0C00");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishFiring, attack.Phase,
        "drain timer underflow installs finish-firing function");

    int narrowingCalls = 0;
    bool observedUnlock = false;
    while (attack.Phase == MotherBrainRainbowBeamAttackPhase.FinishFiring)
    {
        MotherBrainRainbowBeamAttackStepResult narrowing = attack.Step(
            bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
        narrowingCalls++;
        observedUnlock |= narrowing.UnlockedSamus;
    }
    AssertEqual(7, narrowingCalls, "$0C00 beam narrows below $0200 in seven calls");
    AssertEqual(0x0200, attack.AngularWidth, "beam shutdown pins angular width floor");
    AssertTrue(observedUnlock && !samus.InputLocked, "beam shutdown runs Samus command one");
    AssertTrue(!attack.HdmaActive, "beam shutdown disables its HDMA channel");
    AssertEqual(8, attack.SamusProjectileCooldownTimer,
        "beam shutdown reloads Samus projectile cooldown");

    int fallingCalls = 0;
    while (attack.Phase is MotherBrainRainbowBeamAttackPhase.LetSamusFall or
           MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand)
    {
        MotherBrainRainbowBeamAttackStepResult falling = attack.Step(
            bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
        fallingCalls++;
        AssertTrue(falling.Movement is not null,
            $"custom post-beam fall call {fallingCalls} owns Samus coordinates");
    }
    AssertEqual(MotherBrainRainbowBeamAttackPhase.LowerHead, attack.Phase,
        "custom falling carry installs lower-head function");
    AssertEqual(0x00c0, samus.YPosition, "custom falling reaches hardcoded floor $C0");
    AssertEqual(SamusState.DrainedCrouchingLeftPose, samus.Pose,
        "let-fall controller selects left drained pose from forced $54 direction");

    attack.Step(bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DecideNextAction, attack.Phase,
        "lower-head function installs decision timer");
    AssertEqual(0x0080, attack.FunctionTimer, "lower-head function seeds $80 timer");

    int decisionCalls = 0;
    while (attack.Phase == MotherBrainRainbowBeamAttackPhase.DecideNextAction)
    {
        attack.Step(bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
        decisionCalls++;
    }
    AssertEqual(129, decisionCalls, "$80 decision timer expires on 129th DEC/BPL call");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishSamusOff, attack.Phase,
        "post-drain health below $190 chooses finish-Samus chain");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingForwardReallySlowInstructionList,
        attack.Body.InstructionPointer,
        "low-health decision immediately installs native forward body walk");
    AssertEqual(1, attack.Body.InstructionTimer,
        "low-health decision makes forward walk eligible in same enemy frame");
    MotherBrainRainbowBeamAttackStepResult finishThreshold = attack.Step(
        bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishSamusOff, finishThreshold.PhaseAfter,
        "399 energy remains above no-suit finish threshold 340");
    AssertEqual<MotherBrainFinishOffAttackKind?>(null, finishThreshold.FinishOffAttack,
        "RNG zero takes finish-off no-attack branch");

    // Boundary 700 uses command five; 699 uses `$18`. The second fixture also proves that
    // `$A9:C4E8` loads literal zero even when a different HUD item made the prior CMP fail.
    var low = new SamusState
    {
        Health = 699,
        Missiles = 1,
        SuperMissiles = 1,
        PowerBombs = 1,
        SelectedHudItem = 2,
        AutoCancelHudItemIndex = 9,
        XPosition = 220,
        YPosition = 124,
    };
    var lowAttack = new MotherBrainRainbowBeamAttackSequence();
    lowAttack.StartActiveBeam(bus, low);
    AssertEqual(DrainedGetUpHandler.UnableToStand, low.Drained.GetUpHandler,
        "energy 699 selects command $18 unable handler");
    lowAttack.Step(bus, low, enemyFrameCounter: 0, mainEnemyExecutionCounter: 1);
    lowAttack.Step(bus, low, enemyFrameCounter: 0, mainEnemyExecutionCounter: 2);
    MotherBrainRainbowBeamAttackStepResult firstDrain = lowAttack.Step(
        bus, low, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DrainingSamus, firstDrain.PhaseAfter,
        "drain initializer falls through into first resource tick");
    AssertEqual(0, low.Missiles,
        "depleted missiles clear even when a different HUD item is selected");
    AssertEqual(0, low.SuperMissiles,
        "selected supers clear HUD item and reach zero");
    AssertEqual(0, low.PowerBombs,
        "power bombs reach the shared literal-zero reset regardless of HUD selection");
    AssertEqual(0, low.SelectedHudItem, "selected depleted item clears HUD selection");
    AssertEqual(0, low.AutoCancelHudItemIndex, "ammo depletion resets auto-cancel index");

    // Walk programs execute in the ordinary enemy-instruction stage after AI. A complete
    // really-slow list contains nine ten-frame spritemaps and reaches sleep on call 91.
    var backwardBody = new MotherBrainBodyAnimationState
    {
        XPosition = 64,
        YPosition = 100,
        Form = 3,
    };
    backwardBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardReallySlowInstructionList);
    int backwardAnimationCalls = 0;
    int backwardFootsteps = 0;
    while (!backwardBody.Sleeping)
    {
        MotherBrainBodyAnimationStepResult bodyStep = backwardBody.Step(bus);
        backwardAnimationCalls++;
        if (bodyStep.FootstepRequested)
        {
            backwardFootsteps++;
            AssertTrue(bodyStep.FootstepSoundRequested,
                "form-three backward footstep retains otherwise-silent sound request");
            AssertEqual(1, bodyStep.EarthquakeType, "backward footstep earthquake type");
            AssertEqual(4, bodyStep.EarthquakeTimer, "backward footstep earthquake timer");
        }
        AssertTrue(backwardAnimationCalls < 100, "backward walk reaches common sleep");
    }
    AssertEqual(91, backwardAnimationCalls, "nine ten-frame backward records then sleep");
    AssertEqual(2, backwardFootsteps, "backward walk executes two footstep opcodes");
    AssertEqual(40, backwardBody.XPosition, "backward walk literal net X delta -24");
    AssertEqual(100, backwardBody.YPosition, "backward walk literal Y deltas cancel");
    AssertEqual(0, backwardBody.Pose, "backward walk restores standing pose");
    AssertEqual(0xfffa, backwardBody.Bg2XScroll,
        "backward walk keeps BG2 X at $22 minus body X");
    AssertEqual(0, backwardBody.Bg2YScroll,
        "backward walk inverse BG2 Y deltas cancel");
    AssertEqual(0x9972, backwardBody.InstructionPointer,
        "backward common sleep pins its own opcode address");

    var forwardBody = new MotherBrainBodyAnimationState
    {
        XPosition = 64,
        YPosition = 100,
        Form = 2,
    };
    forwardBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyWalkingForwardReallySlowInstructionList);
    int forwardAnimationCalls = 0;
    int forwardFootsteps = 0;
    while (!forwardBody.Sleeping)
    {
        MotherBrainBodyAnimationStepResult bodyStep = forwardBody.Step(bus);
        forwardAnimationCalls++;
        if (bodyStep.FootstepRequested)
        {
            forwardFootsteps++;
            AssertTrue(!bodyStep.FootstepSoundRequested,
                "non-form-three forward footstep suppresses sound request");
        }
        AssertTrue(forwardAnimationCalls < 100, "forward walk reaches common sleep");
    }
    AssertEqual(91, forwardAnimationCalls, "nine ten-frame forward records then sleep");
    AssertEqual(2, forwardFootsteps, "forward walk executes two footstep opcodes");
    AssertEqual(88, forwardBody.XPosition, "forward walk literal net X delta +24");
    AssertEqual(100, forwardBody.YPosition, "forward walk literal Y deltas cancel");
    AssertEqual(0, forwardBody.Pose, "forward walk restores standing pose");
    AssertEqual(0x9850, forwardBody.InstructionPointer,
        "forward common sleep pins its own opcode address");

    // The finish-off loop can idle Mother Brain into a lean and must later wait for the
    // corresponding stand-up bytecode. Verify both posture directions and the longer fast
    // crouch recovery independently so their body-world/BG2 compensation cannot regress.
    var postureBody = new MotherBrainBodyAnimationState
    {
        XPosition = 64,
        YPosition = 100,
    };
    postureBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyLeaningDownInstructionList);
    int leanCalls = 0;
    while (!postureBody.Sleeping)
    {
        postureBody.Step(bus);
        leanCalls++;
        AssertTrue(leanCalls < 30, "lean-down animation reaches sleep");
    }
    AssertEqual(17, leanCalls, "two eight-frame lean records then sleep");
    AssertEqual(6, postureBody.Pose, "lean-down list publishes pose six");
    AssertEqual(112, postureBody.YPosition, "lean-down list moves body down twelve");
    AssertEqual(0xfff4, postureBody.Bg2YScroll,
        "lean-down body Y movement is cancelled in BG2");
    AssertEqual(0xffe6, postureBody.Bg2XScroll,
        "lean-down opcode applies temporary left-four BG2 compensation");

    postureBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyStandingUpAfterLeaningDownInstructionList);
    int leanStandCalls = 0;
    while (!postureBody.Sleeping)
    {
        postureBody.Step(bus);
        leanStandCalls++;
        AssertTrue(leanStandCalls < 30, "lean stand-up animation reaches sleep");
    }
    AssertEqual(17, leanStandCalls, "lean recovery has two eight-frame records");
    AssertEqual(0, postureBody.Pose, "lean recovery restores standing pose");
    AssertEqual(100, postureBody.YPosition, "lean recovery reverses twelve-pixel drop");
    AssertEqual(0, postureBody.Bg2YScroll, "lean recovery reverses BG2 Y offset");
    AssertEqual(0xffe0, postureBody.Bg2XScroll,
        "lean recovery finishes with right-two BG2 compensation");

    var crouchedBody = new MotherBrainBodyAnimationState
    {
        XPosition = 64,
        YPosition = 138,
        Pose = 3,
    };
    crouchedBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyStandingUpAfterCrouchingFastInstructionList);
    int crouchStandCalls = 0;
    while (!crouchedBody.Sleeping)
    {
        crouchedBody.Step(bus);
        crouchStandCalls++;
        AssertTrue(crouchStandCalls < 50, "fast crouch stand-up reaches sleep");
    }
    AssertEqual(33, crouchStandCalls, "four eight-frame fast stand records then sleep");
    AssertEqual(0, crouchedBody.Pose, "fast crouch recovery restores standing pose");
    AssertEqual(100, crouchedBody.YPosition, "fast crouch recovery moves body up 38");
    AssertEqual(38, crouchedBody.Bg2YScroll,
        "fast crouch recovery applies inverse 38-pixel BG2 Y movement");

    WriteTestWord(bus, 0xa99000, 0xffff);
    var unknownBody = new MotherBrainBodyAnimationState();
    unknownBody.SetInstructionList(0x9000);
    AssertThrows<InvalidOperationException>(() => unknownBody.Step(bus),
        "unknown Mother Brain animation command is an explicit translation seam");

    // Now drive the entire `$B8EB-$B983` repeat cycle with the body interpreter after each
    // AI call. This validates both independent timers and their three native fallthroughs.
    var repeatSamus = new SamusState
    {
        Health = 800,
        XPosition = 220,
        YPosition = 124,
    };
    var repeatAttack = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 64,
        BrainYPosition = 96,
    };
    repeatAttack.Body.XPosition = 64;
    repeatAttack.Body.YPosition = 100;
    repeatAttack.StartAttackCycle();
    AssertEqual(MotherBrainRainbowBeamAttackPhase.StartCharging, repeatAttack.Phase,
        "repeat setup installs first charge wait");
    AssertEqual(0x0100, repeatAttack.FunctionTimer, "repeat first wait starts at $100");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadNeutralPhase2InstructionList,
        repeatAttack.HeadInstructionList, "repeat setup selects neutral phase-two head art");

    int firstChargeCalls = 0;
    while (repeatAttack.Phase == MotherBrainRainbowBeamAttackPhase.StartCharging)
    {
        repeatAttack.Step(bus, repeatSamus, 0, 0);
        repeatAttack.Body.Step(bus);
        firstChargeCalls++;
    }
    AssertEqual(257, firstChargeCalls, "$100 first charge wait expires on DEC call 257");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.RetractNeck, repeatAttack.Phase,
        "first wait falls through into retracting walk");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadChargingRainbowInstructionList,
        repeatAttack.HeadInstructionList, "first wait selects charging head program");
    AssertEqual(1, repeatAttack.Body.Pose,
        "same-frame enemy stage begins the requested backward body animation");

    int retractCalls = 0;
    while (repeatAttack.Phase == MotherBrainRainbowBeamAttackPhase.RetractNeck)
    {
        repeatAttack.Step(bus, repeatSamus, 0, 0);
        repeatAttack.Body.Step(bus);
        retractCalls++;
        AssertTrue(retractCalls < 100, "retracting walk reaches X $28");
    }
    AssertEqual(41, retractCalls,
        "AI advances after the -15 opcode crosses hard X $30 boundary");
    AssertEqual(46, repeatAttack.Body.XPosition,
        "retract AI handoff occurs at overshot X $2E before walk animation settles");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.WaitForCharge, repeatAttack.Phase,
        "retract target falls through into second charge wait");
    AssertEqual(0x0050, repeatAttack.NeckAngleDelta, "retract neck NTSC delta");
    AssertEqual(8, repeatAttack.LowerNeckMovementIndex, "retract lower neck index");
    AssertEqual(6, repeatAttack.UpperNeckMovementIndex, "retract upper neck index");
    AssertEqual(0x00ff, repeatAttack.FunctionTimer,
        "second wait is decremented once by retract fallthrough");

    int secondChargeCalls = 0;
    MotherBrainRainbowBeamAttackStepResult chargeCompletion = default;
    while (repeatAttack.Phase == MotherBrainRainbowBeamAttackPhase.WaitForCharge)
    {
        chargeCompletion = repeatAttack.Step(bus, repeatSamus, 0, 0);
        repeatAttack.Body.Step(bus);
        secondChargeCalls++;
    }
    AssertEqual(256, secondChargeCalls, "remaining second charge wait calls");
    AssertTrue(chargeCompletion.ChargeSoundQueued,
        "second charge underflow queues sound-library-two effect $71");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.StartFiring, repeatAttack.Phase,
        "second wait falls through neck-down setup and first firing call");
    AssertEqual(8, repeatAttack.SamusProjectileCooldownTimer,
        "neck-down setup writes projectile cooldown eight");
    AssertEqual(6, repeatAttack.LowerNeckMovementIndex, "firing lower neck index");
    AssertEqual(6, repeatAttack.UpperNeckMovementIndex, "firing upper neck index");
    AssertEqual(0x0500, repeatAttack.NeckAngleDelta, "firing NTSC neck delta");
    AssertEqual(0x0180, repeatAttack.AngularWidth,
        "fallthrough firing call widens prior zero width");
    AssertEqual(0x000f, repeatAttack.FunctionTimer,
        "fallthrough firing call decrements regional timer 16");

    MotherBrainRainbowBeamAttackStepResult frozenCharge = repeatAttack.Step(
        bus, repeatSamus, 0, 0, powerBombActive: true);
    AssertEqual(0x000f, frozenCharge.FunctionTimer,
        "active power bomb freezes firing countdown");
    AssertEqual(0x0300, frozenCharge.AngularWidth,
        "active power bomb does not freeze beam aiming/width growth");

    int firingCalls = 0;
    while (repeatAttack.Phase == MotherBrainRainbowBeamAttackPhase.StartFiring)
    {
        repeatAttack.Step(bus, repeatSamus, 0, 0);
        repeatAttack.Body.Step(bus);
        firingCalls++;
    }
    AssertEqual(16, firingCalls, "timer $000F reaches active beam on 16th unfrozen call");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall, repeatAttack.Phase,
        "start-firing underflow executes `$B983` active setup in same call");
    AssertEqual(0x0200, repeatAttack.AngularWidth,
        "active setup resets prefire width to $0200");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadFiringRainbowInstructionList,
        repeatAttack.HeadInstructionList, "active setup selects firing head program");
    AssertTrue(repeatSamus.InputLocked, "repeat cycle ends by locking Samus through command five");

    var exactThreshold = new SamusState
    {
        Health = 700,
        EquippedItems = 1, // Varia carry makes each of the 300 hits subtract one.
        XPosition = 220,
        YPosition = 124,
    };
    var thresholdAttack = new MotherBrainRainbowBeamAttackSequence();
    thresholdAttack.StartActiveBeam(bus, exactThreshold);
    AssertEqual(DrainedGetUpHandler.AbleToStand, exactThreshold.Drained.GetUpHandler,
        "energy exactly $02BC takes native BPL able branch");

    int thresholdRouteCalls = 0;
    while (thresholdAttack.Phase != MotherBrainRainbowBeamAttackPhase.DecideNextAction)
    {
        thresholdAttack.Step(
            bus,
            exactThreshold,
            enemyFrameCounter: unchecked((ushort)thresholdRouteCalls),
            mainEnemyExecutionCounter: unchecked((ushort)thresholdRouteCalls));
        thresholdRouteCalls++;
        AssertTrue(thresholdRouteCalls < 500, "exact-threshold route reaches decision timer");
    }
    while (thresholdAttack.Phase == MotherBrainRainbowBeamAttackPhase.DecideNextAction)
        thresholdAttack.Step(bus, exactThreshold, 0, 0);
    AssertEqual(400, exactThreshold.Health,
        "700 with Varia reaches exact repeat/finish boundary after 300 hits");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.RepeatAttack, thresholdAttack.Phase,
        "health exactly $0190 selects repeat attack");
    thresholdAttack.Step(bus, exactThreshold, 0, 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.StartCharging, thresholdAttack.Phase,
        "repeat pointer executes neck-extension setup on following AI call");
    AssertEqual(0x0100, thresholdAttack.FunctionTimer,
        "repeated neck-extension setup reloads first charge timer");

    // `$BD45`'s first threshold is suit-dependent and inclusive. Check every retail suit
    // path at the exact boundary and one energy above it before exercising RNG selection.
    foreach ((ushort items, ushort threshold, string suitName) in new[]
    {
        ((ushort)0x0000, (ushort)340, "Power"),
        ((ushort)0x0001, (ushort)180, "Varia"),
        ((ushort)0x0020, (ushort)100, "Gravity"),
    })
    {
        var atBoundary = new SamusState { Health = threshold, EquippedItems = items };
        var boundaryFinish = new MotherBrainRainbowBeamAttackSequence();
        boundaryFinish.Body.XPosition = 64;
        boundaryFinish.Body.YPosition = 100;
        boundaryFinish.StartFinishOffSequence();
        boundaryFinish.Step(bus, atBoundary, 0, 0);
        AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishStandUp, boundaryFinish.Phase,
            $"{suitName} exact finish-off threshold takes BPL done branch");

        var aboveBoundary = new SamusState
        {
            Health = unchecked((ushort)(threshold + 1)),
            EquippedItems = items,
        };
        var aboveFinish = new MotherBrainRainbowBeamAttackSequence();
        aboveFinish.Body.XPosition = 64;
        aboveFinish.Body.YPosition = 100;
        aboveFinish.StartFinishOffSequence();
        aboveFinish.Step(bus, aboveBoundary, 1, 0, randomNumberSeed: 0);
        AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishSamusOff, aboveFinish.Phase,
            $"{suitName} threshold plus one remains in attack loop");
    }

    var onionFinishSamus = new SamusState { Health = 341 };
    var onionFinish = new MotherBrainRainbowBeamAttackSequence();
    onionFinish.Body.XPosition = 64;
    onionFinish.StartFinishOffSequence();
    MotherBrainRainbowBeamAttackStepResult onionAttack = onionFinish.Step(
        bus, onionFinishSamus, 1, 0, randomNumberSeed: 0x0fef);
    AssertEqual(MotherBrainFinishOffAttackKind.TwoOnionRings, onionAttack.FinishOffAttack,
        "RNG $FEF selects two onion rings at upper boundary minus one");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingTwoOnionRingsPhase2InstructionList,
        onionFinish.HeadInstructionList, "onion-ring selection installs retail head list");

    var bombFinish = new MotherBrainRainbowBeamAttackSequence();
    bombFinish.Body.XPosition = 64;
    bombFinish.StartFinishOffSequence();
    MotherBrainRainbowBeamAttackStepResult bombAttack = bombFinish.Step(
        bus, new SamusState { Health = 341 }, 1, 0, randomNumberSeed: 0x0ff0);
    AssertEqual(MotherBrainFinishOffAttackKind.Bomb, bombAttack.FinishOffAttack,
        "RNG $FF0 selects bomb at exact BCS boundary");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingBombPhase2InstructionList,
        bombFinish.HeadInstructionList, "bomb selection installs retail head list");

    var idleFinish = new MotherBrainRainbowBeamAttackSequence();
    idleFinish.Body.XPosition = 64;
    idleFinish.Body.YPosition = 100;
    idleFinish.StartFinishOffSequence();
    MotherBrainRainbowBeamAttackStepResult idlePosture = idleFinish.Step(
        bus,
        new SamusState { Health = 341 },
        enemyFrameCounter: 0,
        mainEnemyExecutionCounter: 0,
        randomNumberSeed: 0x09c0);
    AssertTrue(idlePosture.BodyPostureRequested,
        "no-attack frame with low byte $C0 requests posture change");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyLeaningDownInstructionList,
        idleFinish.Body.InstructionPointer, "standing posture helper requests lean bytecode");

    // Drive the complete low-health handoff. The initial forward walk runs in the separate
    // enemy-instruction stage; only after it restores pose zero can `$C670` report carry.
    var finalSamus = new SamusState { Health = 340 };
    var finalAttack = new MotherBrainRainbowBeamAttackSequence();
    finalAttack.Body.XPosition = 64;
    finalAttack.Body.YPosition = 100;
    finalAttack.StartFinishOffSequence();
    finalAttack.Body.Step(bus); // Same-frame enemy stage following `$BB1A`'s list request.
    finalAttack.Step(bus, finalSamus, 0, 0);
    finalAttack.Body.Step(bus);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishStandUp, finalAttack.Phase,
        "low-health finish loop installs stand-up body function");

    int standWaitCalls = 0;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.FinishStandUp)
    {
        finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        standWaitCalls++;
        AssertTrue(standWaitCalls < 120, "finish-off stand-up waits for forward walk pose zero");
    }
    AssertEqual(MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone, finalAttack.Phase,
        "standing carry enters admire delay");
    AssertEqual(0x000f, finalAttack.FunctionTimer,
        "stand-up fallthrough immediately decrements admire timer");
    AssertEqual(88, finalAttack.Body.XPosition,
        "initial finish-off walk completes one literal really-slow program");

    int admireCalls = 0;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone)
    {
        finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        admireCalls++;
    }
    AssertEqual(16, admireCalls, "remaining admire delay underflows after sixteen calls");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadStretchingPhase2InstructionList,
        finalAttack.HeadInstructionList, "admire expiry installs stretching head animation");
    AssertEqual(0x0100, finalAttack.FunctionTimer,
        "admire expiry loads 256-count final charge");

    int finalChargeCalls = 0;
    MotherBrainRainbowBeamAttackStepResult firstTile = default;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam)
    {
        firstTile = finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        finalChargeCalls++;
    }
    AssertEqual(257, finalChargeCalls, "final charge `$100` expires on DEC call 257");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadChargingRainbowInstructionList,
        finalAttack.HeadInstructionList, "final charge expiry installs charging head list");
    AssertTrue(firstTile.SpriteTileTransfer is
        { EntryIndex: 0, Size: 0x0200, SourceAddress: 0xb18400, VramDestination: 0x7c00 },
        "charge underflow falls through into first Baby tile transfer");

    var transfers = new List<MotherBrainSpriteTileTransferRequest>
    {
        firstTile.SpriteTileTransfer!.Value,
    };
    MotherBrainRainbowBeamAttackStepResult spawnCall = firstTile;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles)
    {
        spawnCall = finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        if (spawnCall.SpriteTileTransfer is { } transfer)
            transfers.Add(transfer);
    }
    AssertEqual(4, transfers.Count, "Baby graphics list emits four frame-spread transfers");
    AssertEqual(new MotherBrainSpriteTileTransferRequest(1, 0x0200, 0xb18600, 0x7d00),
        transfers[1], "Baby transfer entry one");
    AssertEqual(new MotherBrainSpriteTileTransferRequest(2, 0x0200, 0xb18800, 0x7e00),
        transfers[2], "Baby transfer entry two");
    AssertEqual(new MotherBrainSpriteTileTransferRequest(3, 0x0200, 0xb18a00, 0x7f00),
        transfers[3], "Baby transfer entry three");
    AssertTrue(spawnCall.BabySpawnRequested && finalAttack.BabyMetroidSpawned,
        "terminating transfer entry retracts head and requests Baby spawn");
    AssertEqual(0x0050, finalAttack.NeckAngleDelta,
        "Baby spawn call uses NTSC head-retraction delta");
    AssertEqual(0x0100, finalAttack.FunctionTimer,
        "Baby spawn call loads final-beam wait");

    int finalBeamCalls = 0;
    MotherBrainRainbowBeamAttackStepResult finalShot = default;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam)
    {
        finalShot = finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        finalBeamCalls++;
    }
    AssertEqual(257, finalBeamCalls, "final-beam wait `$100` expires on DEC call 257");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding, finalAttack.Phase,
        "final shot installs self-return holding function");
    AssertTrue(finalShot.FinalBeamSoundQueued,
        "final shot queues sound-library-two effect $71");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadFiringRainbowInstructionList,
        finalAttack.HeadInstructionList, "final shot installs firing head animation");
    AssertEqual(6, finalAttack.LowerNeckMovementIndex,
        "final shot lower neck index");
    AssertEqual(6, finalAttack.UpperNeckMovementIndex,
        "final shot upper neck index");
    AssertEqual(0x0500, finalAttack.NeckAngleDelta,
        "final shot NTSC neck delta");

    Console.WriteLine("  Mother Brain actor: ROM posture/walk bytecode, repeat/active/final rainbow chain, thresholds, VRAM, and Baby spawn agree.");
}

/// <summary>
/// Runs the complete `$A9:C710-$C8E1` entrance from initialization through head pinning.
/// The hard-coded milestone coordinates came from the private retail-ROM runner, while the
/// synthetic sine fixture is generated independently from the table's documented definition.
/// This combination catches timer, angle, multiplication, subposition, collision, and
/// cross-enemy ordering regressions without making the ordinary verifier depend on a ROM.
/// </summary>
}
