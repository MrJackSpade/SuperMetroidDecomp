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
    movement.RainbowBeamAngle = SnesAngle.Zero;
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
    movement.RainbowBeamAngle = SnesAngle.HalfTurn;
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
    VerifyMotherBrainBodyInstructionPrograms();
    var bus = new TestAddressSpace();
    // Retail $A9:8FE5 transfers start at Baby graphics +$400, not its base address.
    bus.WriteBytes(0xa98fe5,
        [0x00, 0x02, 0x00, 0x88, 0xb1, 0x00, 0x7c,
         0x00, 0x02, 0x00, 0x8a, 0xb1, 0x00, 0x7d,
         0x00, 0x02, 0x00, 0x8c, 0xb1, 0x00, 0x7e,
         0x00, 0x02, 0x00, 0x8e, 0xb1, 0x00, 0x7f, 0x00, 0x00]);

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
    WritePoseDefinition(bus, SamusPoseIds.KnockbackLeftPose,
        [0x04, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.DrainedCrouchingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.KnockbackLeftPose * 2, 0xc020);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.DrainedCrouchingLeftPose * 2, 0xb268);
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
    AssertEqual(SamusPoseIds.KnockbackLeftPose, samus.Pose,
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
    AssertEqual(SamusPoseIds.DrainedCrouchingLeftPose, samus.Pose,
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

    Console.WriteLine("  Mother Brain actor: rainbow drain, ammo depletion and HUD selection agree.");
}

/// <summary>
/// Runs the complete `$A9:C710-$C8E1` entrance from initialization through head pinning.
/// The hard-coded milestone coordinates came from the private retail-ROM runner, while the
/// synthetic sine fixture is generated independently from the table's documented definition.
/// This combination catches timer, angle, multiplication, subposition, collision, and
/// cross-enemy ordering regressions without making the ordinary verifier depend on a ROM.
/// </summary>
}
