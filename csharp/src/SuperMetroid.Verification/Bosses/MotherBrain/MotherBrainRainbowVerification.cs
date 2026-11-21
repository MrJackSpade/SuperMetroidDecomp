using SuperMetroid.Core.Bosses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Hardware.SignedSineFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Constructed-cartridge checks for Mother Brain's rainbow-beam Samus movement and full drain attack chain.</summary>
internal static class MotherBrainRainbowVerification
{

/// <summary>The picture her brain shows, as a draw with time frozen shows it without playing the brain on.</summary>
private static EnemyFrameId? FrozenBrainPicture(MotherBrainBrainAnimation brain) =>
    brain.AdvanceAtDraw(timeIsFrozen: true, cue => throw new InvalidOperationException($"A frozen draw played the brain's {cue}."));

/// <summary>Checks recoil, floor and ceiling clamps, 8.8 sine movement and previous-position publication through the rainbow-beam Samus controller.</summary>
internal static void VerifyMotherBrainRainbowBeamSamusMovement()
{
    var bus = new TestAddressSpace();

    // `$86:C272` uses table index angle+$40. Give angle zero a literal +$0100 entry and
    // angle $80 a literal -$0100 entry so the expected $10.00 components are unambiguous.
    SeedSignedSineTable(bus, [new SineSample(0x40, 0x0100), new SineSample(0xc0, unchecked((short)0xff00))]);

    // The helpers add a signed 8.8 velocity to the whole Y word and only the high
    // subposition byte, so the applied vertical velocity is the delta of that 8.8 view.
    static ushort YEightEight(SamusState state) =>
        unchecked((ushort)((EightEightOracle.FromWhole(state.Position.Y.Whole)) | LittleEndianBytes.HighByte(state.Kinematics.Position.Y.Fraction)));

    var movement = new MotherBrainRainbowBeamSamusMovement();
    var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
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
    AssertEqual(new PixelVelocity<RoomPixels>(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-254), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24)), movement.CustomVelocity,
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
    AssertEqual(0, movement.CustomVelocity.X.InTwoHundredFiftySixthsOfPixel, "rainbow fall X easing clamps at zero");
    movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual(0, movement.CustomVelocity.X.InTwoHundredFiftySixthsOfPixel, "rainbow fall X easing stays zero");
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
    movement.RainbowBeamAngle = RotationAngle.Zero;
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
    movement.RainbowBeamAngle = RotationAngle.HalfTurn;
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

    // #1470: a push inside the arena hides only its coarse sub-pixel movement. The camera's record keeps its own fine
    // sub-pixel, so fine movement Samus made since the record was taken still reaches the camera's speed.
    samus.Position = samus.Position.WithWholeY(0x007c);
    samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0xd055);
    FixedPosition<RoomPixels> recorded = samus.Position.WithFractionY(0xd0aa);
    MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus);
    AssertEqual(0x1055, samus.Kinematics.Position.Y.Fraction, "the push moves Samus's coarse sub-pixel");
    AssertEqual(recorded.WithWholeY(0x007d).WithFractionY(0x10aa),
        samus.ApplyPreviousPositionWrites(recorded),
        "the camera's record takes the push's whole pixel and coarse sub-pixel and keeps its own fine sub-pixel");
    AssertEqual(recorded, samus.ApplyPreviousPositionWrites(recorded), "the hidden push is consumed once");

    Console.WriteLine("  Mother Brain: rainbow-beam forced 8.8 movement and arena clamps agree.");
}

/// <summary>
/// Drives the complete active `$A9:B983-$BB2D` body-function chain. Long loops are valuable
/// here: the attack's 300 drain calls and 129 decision-timer calls expose off-by-one mistakes
/// that isolated helper checks cannot see.
/// </summary>
internal static void VerifyMotherBrainRainbowBeamAttackSequence()
{
    var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Health = 999,
        Missiles = 80,
        SuperMissiles = 80,
        PowerBombs = 400,
        Position = FixedPosition<RoomPixels>.FromWhole(new(220, 124)),
    };
    var attack = MotherBrainLateFightFixtures.NewSequence();
    attack.State.MoveBrainTo(new(64, 96));
    attack.Encounter.Body.Position = FixedPosition<RoomPixels>.FromWhole(new(64, 100));

    attack.RainbowAttack.StartActiveBeam(samus);
    AssertEqual(MotherBrainRainbowPhase.MoveSamusTowardWall, attack.Encounter.RainbowPhase,
        "active rainbow start installs wall-motion function");
    AssertEqual(SamusPoseId.KnockbackLeftPose, samus.Pose,
        "active rainbow start runs native drained setup command");
    AssertEqual(DrainedGetUpCheck.AbleToStand, samus.Drained.GetUpCheck,
        "energy 999 selects command five able handler");
    AssertTrue(samus.Control.InputLocked, "rainbow start locks Samus input handlers");
    AssertTrue(attack.RainbowAttack.BeamActive, "rainbow start shows the beam");
    AssertEqual(RotationAngle.FromSteps(2), attack.RainbowAttack.AngularWidth, "rainbow start width");

    MotherBrainRainbowBeamAttackStepResult wall = attack.Step(samus, MotherBrainLateFightFixtures.QuietFrame(2, 1));
    AssertEqual(0x00eb, samus.Position.X.Whole, "actor sequence moves Samus to hardcoded wall");
    AssertEqual(MotherBrainRainbowPhase.PinSamusToWall, attack.Encounter.RainbowPhase,
        "wall carry installs one-frame-delay function");
    AssertTrue(wall.Starts(MotherBrainStepEffect.RainbowBeamSound) && wall.Starts(MotherBrainStepEffect.RainbowColours),
        "wall function runs sound and bit-one palette cadence");
    AssertEqual(RotationAngle.FromSteps(3, 128), attack.RainbowAttack.AngularWidth, "wall function widens before aiming");
    AssertTrue(wall.Explosion is { Offset: { X: 6, Y: 2 } },
        "zero explosion timer increments to literal offset record one");

    MotherBrainRainbowBeamAttackStepResult delay = attack.Step(samus, MotherBrainLateFightFixtures.QuietFrame(0, 2));
    AssertEqual(MotherBrainRainbowPhase.StartDrainingSamus, attack.Encounter.RainbowPhase,
        "zero delay timer underflows and schedules drain initializer");
    AssertEqual<MotherBrainEarthquakeRequest?>(new(MotherBrainDeathDefinitions.RainbowBeamEarthquake, new MotherBrainEarthquakeLength.Lasting(8)), delay.Earthquake,
        "delay underflow starts the rainbow beam's strong diagonal foreground earthquake for eight frames");

    int drainCalls = 0;
    int queuedBeamSounds = (wall.Starts(MotherBrainStepEffect.RainbowBeamSound) ? 1 : 0) + (delay.Starts(MotherBrainStepEffect.RainbowBeamSound) ? 1 : 0);
    // The beam's explosions burst at the cartridge's offsets from its second on, wrapping to its first after its last (#2258).
    Offset16<RoomPixels>[] cartridgeOffsets = [new(-8, -7), new(6, 2), new(-4, 5), new(2, -4), new(3, 6), new(-6, -2), new(8, -6), new(0, 7)];
    var burstOffsets = new List<Offset16<RoomPixels>>();
    if (wall.Explosion is { } firstExplosion)
        burstOffsets.Add(firstExplosion.Offset);
    int explosions = wall.Explosion is null ? 0 : 1;
    while (attack.Encounter.RainbowPhase is MotherBrainRainbowPhase.StartDrainingSamus or
           MotherBrainRainbowPhase.DrainingSamus)
    {
        MotherBrainRainbowBeamAttackStepResult drain = attack.Step(samus, MotherBrainLateFightFixtures.QuietFrame(unchecked((ushort)drainCalls), unchecked((ushort)drainCalls)));
        drainCalls++;
        if (drain.Starts(MotherBrainStepEffect.RainbowBeamSound))
            queuedBeamSounds++;
        if (drain.Explosion is { } explosion)
        {
            explosions++;
            burstOffsets.Add(explosion.Offset);
        }
    }

    AssertEqual(300, drainCalls, "$012B drain timer includes fallthrough call and expires after 300");
    AssertEqual(399, samus.Health, "300 no-Varia rainbow hits subtract two each");
    AssertEqual(5, samus.Missiles, "missiles decrement every fourth enemy pass");
    AssertEqual(5, samus.SuperMissiles, "supers share every-fourth cadence");
    AssertEqual(100, samus.PowerBombs, "power bombs decrement every drain call");
    AssertEqual(7, queuedBeamSounds, "count six queues seven rainbow beam sound attempts");
    AssertTrue(explosions > 1, "explosion timer continues across wall and drain phases");
    AssertTrue(burstOffsets.Count > cartridgeOffsets.Length, "the beam's explosions run past the last offset");
    for (int burst = 0; burst < burstOffsets.Count; burst++)
        AssertEqual(cartridgeOffsets[(burst + 1) % cartridgeOffsets.Length], burstOffsets[burst], $"beam explosion {burst} bursts at the cartridge's next offset");
    AssertEqual(RotationAngle.FromSteps(12), attack.RainbowAttack.AngularWidth, "drain widening clamps at twelve steps");
    AssertEqual(MotherBrainRainbowPhase.FinishFiring, attack.Encounter.RainbowPhase,
        "drain timer underflow installs finish-firing function");

    int narrowingCalls = 0;
    bool lockedBeforeNarrowing = samus.Control.InputLocked;
    while ((attack.Encounter.Stage == MotherBrainBodyStage.RainbowBeam && attack.Encounter.RainbowPhase == MotherBrainRainbowPhase.FinishFiring))
    {
        attack.Step(samus, MotherBrainLateFightFixtures.QuietFrame(0, 0));
        narrowingCalls++;
    }
    AssertEqual(7, narrowingCalls, "$0C00 beam narrows below $0200 in seven calls");
    AssertEqual(RotationAngle.FromSteps(2), attack.RainbowAttack.AngularWidth, "beam shutdown pins angular width floor");
    AssertTrue(lockedBeforeNarrowing && !samus.Control.InputLocked, "beam shutdown runs Samus command one");
    AssertTrue(!attack.RainbowAttack.BeamActive, "beam shutdown hides the beam");

    int fallingCalls = 0;
    while (attack.Encounter.RainbowPhase is MotherBrainRainbowPhase.LetSamusFall or
           MotherBrainRainbowPhase.WaitForSamusToLand)
    {
        MotherBrainRainbowBeamAttackStepResult falling = attack.Step(samus, MotherBrainLateFightFixtures.QuietFrame(0, 0));
        fallingCalls++;
        AssertTrue(falling.Movement is not null,
            $"custom post-beam fall call {fallingCalls} owns Samus coordinates");
    }
    AssertEqual(MotherBrainRainbowPhase.LowerHead, attack.Encounter.RainbowPhase,
        "custom falling carry installs lower-head function");
    AssertEqual(0x00c0, samus.Position.Y.Whole, "custom falling reaches hardcoded floor $C0");
    AssertEqual(SamusPoseId.DrainedCrouchingLeftPose, samus.Pose,
        "let-fall controller selects left drained pose from forced $54 direction");

    attack.Step(samus, MotherBrainLateFightFixtures.QuietFrame(0, 0));
    AssertEqual(MotherBrainRainbowPhase.DecideNextAction, attack.Encounter.RainbowPhase,
        "lower-head function installs decision timer");
    AssertEqual(0x0080, attack.Encounter.PhaseTimer.CartridgeCount, "lower-head function seeds $80 timer");

    int decisionCalls = 0;
    while ((attack.Encounter.Stage == MotherBrainBodyStage.RainbowBeam && attack.Encounter.RainbowPhase == MotherBrainRainbowPhase.DecideNextAction))
    {
        attack.Step(samus, MotherBrainLateFightFixtures.QuietFrame(0, 0));
        decisionCalls++;
    }
    AssertEqual(129, decisionCalls, "$80 decision timer expires on 129th DEC/BPL call");
    AssertEqual(MotherBrainFinishOffPhase.AttackUntilSamusIsLow, attack.Encounter.FinishOffPhase,
        "post-drain health below $190 chooses finish-Samus chain");
    AssertEqual<MotherBrainBodySegment?>(MotherBrainBodySegment.WalkForwardReallySlow, attack.Encounter.BodyPlayback?.PlayingSegment,
        "low-health decision immediately requests the native forward body walk");
    EnemyFrameId? headBeforeFinishThreshold = FrozenBrainPicture(attack.Encounter.Brain);
    bool headClockStoppedBeforeFinishThreshold = attack.Encounter.Brain.Clock.IsStopped;
        _ = attack.Step(samus, MotherBrainLateFightFixtures.QuietFrame(0, 0));
        AssertEqual(MotherBrainFinishOffPhase.AttackUntilSamusIsLow, attack.Encounter.FinishOffPhase,
        "399 energy remains above no-suit finish threshold 340");
    // An attack would start her brain on its volley or bomb, restarting its clock on the attack's opening picture.
    AssertTrue(headBeforeFinishThreshold == FrozenBrainPicture(attack.Encounter.Brain)
            && headClockStoppedBeforeFinishThreshold == attack.Encounter.Brain.Clock.IsStopped,
        "RNG zero takes finish-off no-attack branch");

    // Boundary 700 uses command five; 699 uses `$18`. The second fixture also proves that
    // `$A9:C4E8` loads literal zero even when a different HUD item made the prior CMP fail.
    var low = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Health = 699,
        Missiles = 1,
        SuperMissiles = 1,
        PowerBombs = 1,
        SelectedHudItem = HudItem.SuperMissile,
        AutoCancelledItem = HudItem.XRay,
        Position = FixedPosition<RoomPixels>.FromWhole(new(220, 124)),
    };
    var lowAttack = MotherBrainLateFightFixtures.NewSequence();
    lowAttack.RainbowAttack.StartActiveBeam(low);
    AssertEqual(DrainedGetUpCheck.UnableToStand, low.Drained.GetUpCheck,
        "energy 699 selects command $18 unable handler");
    lowAttack.Step(low, MotherBrainLateFightFixtures.QuietFrame(0, 1));
    lowAttack.Step(low, MotherBrainLateFightFixtures.QuietFrame(0, 2));
        _ = lowAttack.Step(low, MotherBrainLateFightFixtures.QuietFrame(0, 0));
        AssertEqual(MotherBrainRainbowPhase.DrainingSamus, lowAttack.Encounter.RainbowPhase,
        "drain initializer falls through into first resource tick");
    AssertEqual(0, low.Missiles,
        "depleted missiles clear even when a different HUD item is selected");
    AssertEqual(0, low.SuperMissiles,
        "selected supers clear HUD item and reach zero");
    AssertEqual(0, low.PowerBombs,
        "power bombs reach the shared literal-zero reset regardless of HUD selection");
    AssertEqual(HudItem.Beam, low.SelectedHudItem, "selected depleted item clears HUD selection");
    AssertEqual<HudItem?>(null, low.AutoCancelledItem, "ammo depletion resets auto-cancel index");

    Console.WriteLine("  Mother Brain actor: rainbow drain, ammo depletion and HUD selection agree.");
}

}
