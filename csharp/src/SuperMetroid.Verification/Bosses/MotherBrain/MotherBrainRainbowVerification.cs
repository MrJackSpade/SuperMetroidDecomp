using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.SignedSineFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Constructed-cartridge checks for Mother Brain's rainbow-beam Samus movement and full drain attack chain.</summary>
internal static class MotherBrainRainbowVerification
{

/// <summary>Checks recoil, floor and ceiling clamps, 8.8 sine movement and previous-position publication through the rainbow-beam Samus controller.</summary>
internal static void VerifyMotherBrainRainbowBeamSamusMovement()
{
    var bus = new TestAddressSpace();

    // `$86:C272` uses table index angle+$40. Give angle zero a literal +$0100 entry and
    // angle $80 a literal -$0100 entry so the expected $10.00 components are unambiguous.
    SeedSignedSineTable(bus, [(0x40, 0x0100), (0xc0, unchecked((short)0xff00))]);

    // The helpers add a signed 8.8 velocity to the whole Y word and only the high
    // subposition byte, so the applied vertical velocity is the delta of that 8.8 view.
    static ushort YEightEight(SamusState state) =>
        unchecked((ushort)((SnesEightEight.FromWhole(state.Position.Y.Whole)) | SnesLittleEndian.HighByte(state.Kinematics.Position.Y.Fraction)));

    var movement = new MotherBrainRainbowBeamSamusMovement();
    var samus = new SamusState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
    };

    // Begin-fall seeds -$01.00 X and zero Y. The first call changes those to -$00.FE and
    // +$00.18. Adding fractional FE to an existing FF must carry into the signed whole
    // delta: -1+1 is zero, while the untouched low subposition bytes remain AA/BB.
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0xffaa).WithFractionY(0xf0bb);
    movement.BeginFallingAfterRainbowBeam();
    MotherBrainForcedSamusMovementResult first =
        movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual(new EightEightVector<RoomPixels>(unchecked((short)0xff02), 0x0018), movement.CustomVelocity,
        "rainbow fall first eased X and accelerated Y velocity");
    AssertEqual(100, samus.Position.X.Whole, "rainbow fall signed X plus fractional carry");
    AssertEqual(0x01aa, samus.Kinematics.Position.X.Fraction, "rainbow fall preserves X low sub-byte");
    AssertEqual(101, samus.Position.Y.Whole, "rainbow fall Y fractional carry");
    AssertEqual(0x08bb, samus.Kinematics.Position.Y.Fraction, "rainbow fall preserves Y low sub-byte");
    AssertTrue(!first.ReachedVerticalBoundary && samus.Position.Y.Whole is > 0x0030 and < 0x00c0,
        "unclamped first fall returns clear vertical carry");

    // Exactly 127 more +$0002 updates carry the negative X word to zero. Native clamps it
    // there instead of allowing a positive recoil; subsequent calls must remain zero.
    for (int call = 1; call < 128; call++)
        movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual(0, movement.CustomVelocity.X.SignedRawValue, "rainbow fall X easing clamps at zero");
    movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual(0, movement.CustomVelocity.X.SignedRawValue, "rainbow fall X easing stays zero");
    AssertEqual(0x00c0, samus.Position.Y.Whole, "rainbow fall clamps at arena floor Y $C0");
    AssertEqual(0, samus.Kinematics.Position.Y.Fraction, "arena floor clamp clears Y subposition");

    // At/below $7C, `$A9:BBCF` chooses +$00.40. It is not a snap: this deliberately
    // crosses from $7C.D0 to $7D.10 through the eight-bit fractional carry.
    samus.Position = samus.Position.WithWholeY(0x007c);
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0xd055);
    ushort middleDownBefore = YEightEight(samus);
    MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus);
    AssertEqual(0x0040, unchecked((ushort)(YEightEight(samus) - middleDownBefore)),
        "middle-wall below target velocity");
    AssertEqual(0x007d, samus.Position.Y.Whole, "middle-wall downward whole carry");
    AssertEqual(0x1055, samus.Kinematics.Position.Y.Fraction, "middle-wall downward fraction");

    // Above $7C, two's-complement $FFC0 moves upward. $7D.10 + (-$00.40) becomes $7C.D0.
    ushort middleUpBefore = YEightEight(samus);
    MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus);
    AssertEqual(0xffc0, unchecked((ushort)(YEightEight(samus) - middleUpBefore)),
        "middle-wall above target velocity");
    AssertEqual(0x007c, samus.Position.Y.Whole, "middle-wall upward whole borrow");
    AssertEqual(0xd055, samus.Kinematics.Position.Y.Fraction, "middle-wall upward fraction");

    // Angle zero reads +$0100 at index $40. Speed $1000 * sine $0100 >> 8 therefore
    // produces +$1000 on Y, while the independent horizontal helper also adds $10 pixels.
    samus.Position = samus.Position.WithWholeX(100);
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0x0022);
    samus.Position = samus.Position.WithWholeY(100);
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0x0033);
    movement.RainbowBeamAngle = SnesAngle.Zero;
    ushort beamBefore = YEightEight(samus);
    MotherBrainForcedSamusMovementResult beam = movement.MoveTowardWall(samus);
    AssertEqual(0x1000, unchecked((ushort)(YEightEight(samus) - beamBefore)),
        "rainbow beam table-derived Y velocity");
    AssertEqual(116, samus.Position.X.Whole, "rainbow beam horizontal $10.00 step");
    AssertEqual(116, samus.Position.Y.Whole, "rainbow beam vertical $10.00 step");
    AssertTrue(!beam.ReachedVerticalBoundary, "rainbow beam caller clears vertical-helper carry");

    // Reaching X $EB returns set carry and skips vertical calculation entirely.
    samus.Position = samus.Position.WithWholeX(0x00e0);
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0x7777);
    samus.Position = samus.Position.WithWholeY(100);
    MotherBrainForcedSamusMovementResult wall = movement.MoveTowardWall(samus);
    AssertTrue(samus.Position.X.Whole == 0x00eb && wall.ReachedVerticalBoundary, "rainbow beam wall clamp returns carry");
    AssertEqual(0x00eb, samus.Position.X.Whole, "rainbow beam hardcoded wall X $EB");
    AssertEqual(0, samus.Kinematics.Position.X.Fraction, "rainbow wall clamp clears X subposition");
    AssertEqual(100, samus.Position.Y.Whole, "rainbow wall clamp skips vertical movement");

    // A negative table component below Y $30 proves the separate ceiling clamp and its
    // subposition clear. MoveTowardWall then clears native carry because X did not clamp.
    samus.Position = samus.Position.WithWholeX(100);
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0);
    samus.Position = samus.Position.WithWholeY(0x0030);
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0x9999);
    movement.RainbowBeamAngle = SnesAngle.HalfTurn;
    MotherBrainForcedSamusMovementResult ceiling = movement.MoveTowardWall(samus);
    AssertTrue(samus.Position.Y.Whole == 0x0030 && samus.Kinematics.Position.Y.Fraction == 0 && !ceiling.ReachedVerticalBoundary,
        "rainbow ceiling clamp is hidden from caller carry");
    AssertEqual(0x0030, samus.Position.Y.Whole, "rainbow hardcoded ceiling Y $30");
    AssertEqual(0, samus.Kinematics.Position.Y.Fraction, "rainbow ceiling clears Y subposition");

    // #1269: $A9:BBFD/$BC3F mirror every Samus store into the previous-position words, so the
    // camera sees no movement. In the 100% movie Samus falls from Y $C3.FFFF into the $C0
    // floor clamp; native's previous Y fraction is zeroed with her own.
    samus.Position = samus.Position.WithWholeX(100);
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0x1234);
    samus.Position = samus.Position.WithWholeY(0x00c3);
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0xffff);
    var frameStart = samus.Position;
    // The earlier clamps above queued their own writes; each frame's scroll applies them.
    samus.ApplyPreviousPositionWrites(frameStart);
    movement.BeginFallingAfterRainbowBeam();
    movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual(0x00c0, samus.Position.Y.Whole, "the fall reaches the floor clamp");
    FixedPosition<RoomPixels> checkpoint = samus.ApplyPreviousPositionWrites(frameStart);
    AssertEqual(samus.Position, checkpoint,
        "the forced move leaves the camera checkpoint at Samus's new position");

    Console.WriteLine("  Mother Brain: rainbow-beam forced 8.8 movement and arena clamps agree.");
}

/// <summary>
/// Drives the complete active `$A9:B983-$BB2D` body-function chain. Long loops are valuable
/// here: the attack's 300 drain calls and 129 decision-timer calls expose off-by-one mistakes
/// that isolated helper checks cannot see.
/// </summary>
internal static void VerifyMotherBrainRainbowBeamAttackSequence()
{

    var samus = new SamusState
    {
        Health = 999,
        Missiles = 80,
        SuperMissiles = 80,
        PowerBombs = 400,
        Position = FixedPosition<RoomPixels>.FromWhole(new(220, 124)),
    };
    var attack = new MotherBrainRainbowBeamAttackSequence
    {
        BrainPosition = new(64, 96),
    };
    attack.Body.Position = new(64, 100);

    attack.StartActiveBeam(samus);
    AssertEqual(MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall, attack.Phase,
        "active rainbow start installs wall-motion function");
    AssertEqual(SamusPoseId.KnockbackLeftPose, samus.Pose,
        "active rainbow start runs native drained setup command");
    AssertEqual(DrainedGetUpHandler.AbleToStand, samus.Drained.GetUpHandler,
        "energy 999 selects command five able handler");
    AssertTrue(samus.Control.AlphaLocked, "rainbow start locks Samus input handlers");
    AssertTrue(attack.HdmaActive, "rainbow start requests active HDMA beam");
    AssertEqual(0x0200, attack.AngularWidth, "rainbow start width");

    MotherBrainRainbowBeamAttackStepResult wall = attack.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 2, MainEnemyExecutionCounter = 1 });
    AssertEqual(0x00eb, samus.Position.X.Whole, "actor sequence moves Samus to hardcoded wall");
    AssertEqual(MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay, attack.Phase,
        "wall carry installs one-frame-delay function");
    AssertTrue(wall.SoundQueued && wall.PaletteRequested,
        "wall function runs sound and bit-one palette cadence");
    AssertEqual(0x0380, attack.AngularWidth, "wall function widens before aiming");
    AssertTrue(wall.Explosion is { Offset: { X: 6, Y: 2 } },
        "zero explosion timer increments to literal offset record one");

    MotherBrainRainbowBeamAttackStepResult delay = attack.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 2 });
    AssertEqual(MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus, attack.Phase,
        "zero delay timer underflows and schedules drain initializer");
    AssertEqual(8, attack.EarthquakeType, "delay underflow selects earthquake type eight");
    AssertEqual(NativeCountdown.Start(8), attack.EarthquakeTimer, "delay underflow seeds eight-frame earthquake");

    int drainCalls = 0;
    int queuedBeamSounds = (wall.SoundQueued ? 1 : 0) + (delay.SoundQueued ? 1 : 0);
    int explosions = wall.Explosion is null ? 0 : 1;
    while (attack.Phase is MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus or
           MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus)
    {
        MotherBrainRainbowBeamAttackStepResult drain = attack.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = unchecked((ushort)drainCalls), MainEnemyExecutionCounter = unchecked((ushort)drainCalls) });
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
    AssertEqual(MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring, attack.Phase,
        "drain timer underflow installs finish-firing function");

    int narrowingCalls = 0;
    bool lockedBeforeNarrowing = samus.Control.AlphaLocked;
    while (attack.Phase == MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring)
    {
        attack.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        narrowingCalls++;
    }
    AssertEqual(7, narrowingCalls, "$0C00 beam narrows below $0200 in seven calls");
    AssertEqual(0x0200, attack.AngularWidth, "beam shutdown pins angular width floor");
    AssertTrue(lockedBeforeNarrowing && !samus.Control.AlphaLocked, "beam shutdown runs Samus command one");
    AssertTrue(!attack.HdmaActive, "beam shutdown disables its HDMA channel");
    AssertEqual(NativeCountdown.Start(8), attack.SamusProjectileCooldownTimer,
        "beam shutdown reloads Samus projectile cooldown");

    int fallingCalls = 0;
    while (attack.Phase is MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall or
           MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand)
    {
        MotherBrainRainbowBeamAttackStepResult falling = attack.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        fallingCalls++;
        AssertTrue(falling.Movement is not null,
            $"custom post-beam fall call {fallingCalls} owns Samus coordinates");
    }
    AssertEqual(MotherBrainBodyFunction.SecondPhaseRainbowLowerHead, attack.Phase,
        "custom falling carry installs lower-head function");
    AssertEqual(0x00c0, samus.Position.Y.Whole, "custom falling reaches hardcoded floor $C0");
    AssertEqual(SamusPoseId.DrainedCrouchingLeftPose, samus.Pose,
        "let-fall controller selects left drained pose from forced $54 direction");

    attack.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
    AssertEqual(MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction, attack.Phase,
        "lower-head function installs decision timer");
    AssertEqual(0x0080, attack.FunctionTimer.RawValue, "lower-head function seeds $80 timer");

    int decisionCalls = 0;
    while (attack.Phase == MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction)
    {
        attack.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        decisionCalls++;
    }
    AssertEqual(129, decisionCalls, "$80 decision timer expires on 129th DEC/BPL call");
    AssertEqual(MotherBrainBodyFunction.SecondPhaseFinishSamusOff, attack.Phase,
        "post-drain health below $190 chooses finish-Samus chain");
    AssertEqual(MotherBrainBodyProgram.WalkForwardReallySlow, attack.Body.Program,
        "low-health decision immediately requests the native forward body walk");
    MotherBrainHeadProgram? headBeforeFinishThreshold = attack.HeadProgram;
    MotherBrainRainbowBeamAttackStepResult finishThreshold = attack.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
    AssertEqual(MotherBrainBodyFunction.SecondPhaseFinishSamusOff, finishThreshold.PhaseAfter,
        "399 energy remains above no-suit finish threshold 340");
    AssertEqual(headBeforeFinishThreshold, attack.HeadProgram,
        "RNG zero takes finish-off no-attack branch");

    // Boundary 700 uses command five; 699 uses `$18`. The second fixture also proves that
    // `$A9:C4E8` loads literal zero even when a different HUD item made the prior CMP fail.
    var low = new SamusState
    {
        Health = 699,
        Missiles = 1,
        SuperMissiles = 1,
        PowerBombs = 1,
        SelectedHudItem = HudItem.SuperMissile,
        AutoCancelHudItemIndex = HudItem.XRay,
        Position = FixedPosition<RoomPixels>.FromWhole(new(220, 124)),
    };
    var lowAttack = new MotherBrainRainbowBeamAttackSequence();
    lowAttack.StartActiveBeam(low);
    AssertEqual(DrainedGetUpHandler.UnableToStand, low.Drained.GetUpHandler,
        "energy 699 selects command $18 unable handler");
    lowAttack.Step(low, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 1 });
    lowAttack.Step(low, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 2 });
    MotherBrainRainbowBeamAttackStepResult firstDrain = lowAttack.Step(low, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
    AssertEqual(MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus, firstDrain.PhaseAfter,
        "drain initializer falls through into first resource tick");
    AssertEqual(0, low.Missiles,
        "depleted missiles clear even when a different HUD item is selected");
    AssertEqual(0, low.SuperMissiles,
        "selected supers clear HUD item and reach zero");
    AssertEqual(0, low.PowerBombs,
        "power bombs reach the shared literal-zero reset regardless of HUD selection");
    AssertEqual(HudItem.Beam, low.SelectedHudItem, "selected depleted item clears HUD selection");
    AssertEqual(HudItem.Beam, low.AutoCancelHudItemIndex, "ammo depletion resets auto-cancel index");

    Console.WriteLine("  Mother Brain actor: rainbow drain, ammo depletion and HUD selection agree.");
}

}
