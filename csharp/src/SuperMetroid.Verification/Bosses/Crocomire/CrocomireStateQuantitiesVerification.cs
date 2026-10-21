using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Crocomire's encounter quantities stored as ordinary integers and typed fixed-point values instead of the body
/// slot's native words: the step count's range guard and the power-bomb charge's end, the sink and rise motion,
/// the skeleton's 16.16 motion and the melt shake counter. Each production update is compared with a reference
/// that keeps the cartridge's own words and byte arithmetic, including the carries, borrows, clamps and wraps the
/// widened storage must reproduce.
/// </summary>
internal static class CrocomireStateQuantitiesVerification
{
    /// <summary>Runs every Crocomire quantity check.</summary>
    internal static void VerifyCrocomireStateQuantities()
    {
        Suite(nameof(VerifyStepCounterRange), VerifyStepCounterRange);
        Suite(nameof(VerifyPowerBombChargeEnd), VerifyPowerBombChargeEnd);
        Suite(nameof(VerifySinkMatchesNativeBytes), VerifySinkMatchesNativeBytes);
        Suite(nameof(VerifyRiseMatchesNativeBytes), VerifyRiseMatchesNativeBytes);
        Suite(nameof(VerifySkeletonMotionMatchesNativeWords), VerifySkeletonMotionMatchesNativeWords);
        Suite(nameof(VerifyMeltShakeCounterWraps), VerifyMeltShakeCounterWraps);
        Console.WriteLine("Crocomire state quantities: step range, power-bomb charge, sink, rise, skeleton motion and melt shake match the native words.");
    }

    /// <summary>
    /// The queued step count stores the whole range its signed comparisons read correctly and rejects anything
    /// outside it without changing the count, instead of wrapping as a native word would.
    /// </summary>
    private static void VerifyStepCounterRange()
    {
        (_, CrocomireEnemyState state) = NewCrocomire();
        state.StepCounter = short.MaxValue;
        AssertEqual(short.MaxValue, state.StepCounter, "the largest step count reads back");
        AssertThrows<ArgumentOutOfRangeException>(() => state.StepCounter = short.MaxValue + 1,
            "a step count the signed comparisons would read as negative is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => state.StepCounter = -1,
            "a step count below zero is rejected instead of wrapping");
        AssertEqual(short.MaxValue, state.StepCounter, "a rejected step count leaves the count unchanged");
    }

    /// <summary>
    /// The power-bomb charge consumes one step per fight-AI cue and ends once fewer than two remain, exactly as the
    /// cartridge's decrement and signed test do, including a charge entered with no steps, whose native word wraps.
    /// </summary>
    private static void VerifyPowerBombChargeEnd()
    {
        foreach (int start in new[] { 3, 2, 0 })
        {
            (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
            state.FightFunction = CrocomireFightFunction.PowerBombCharge;
            state.StepCounter = start;
            state.BodyCursor = AnimationProgramCursor<CrocomireBodyProgram>.Start(CrocomireBodyProgram.Initial);
            state.Body.InstructionTimer = WrappingCountdown.Start(1);
            // The initial program shows its frame on the first update and runs its fight-AI cue on every later one.
            Pass(enemies, state);

            ushort nativeSteps = checked((ushort)start);
            for (int cue = 1; cue <= start + 1; cue++)
            {
                nativeSteps = WrappingCounter.Decrement(nativeSteps).Value;
                bool nativeEnds = unchecked((short)(nativeSteps - 2)) < 0;
                if (nativeEnds)
                    nativeSteps = 0;
                Pass(enemies, state);
                AssertEqual(nativeSteps, state.StepCounter, $"charge from {start} steps, cue {cue}: steps left");
                AssertEqual(nativeEnds ? CrocomireFightFunction.SteppingForward : CrocomireFightFunction.PowerBombCharge,
                    state.FightFunction, $"charge from {start} steps, cue {cue}: fight function");
                if (nativeEnds)
                    break;
            }
            AssertEqual(CrocomireFightFunction.SteppingForward, state.FightFunction, $"charge from {start} steps ends");
        }
    }

    /// <summary>
    /// The sink matches the cartridge's byte-wise acceleration, speed and subposition update every frame, from the
    /// bridge collapse's resting start and from a start that drives both clamps, every carry and a nonzero low byte
    /// beneath the subposition.
    /// </summary>
    private static void VerifySinkMatchesNativeBytes()
    {
        CompareSink(SnesLittleEndian.Word(0, 8), 0, 0, 144, "resting sink");
        CompareSink(SnesLittleEndian.Word(200, 47), SnesLittleEndian.Word(250, 3), SnesLittleEndian.Word(77, 250), 150,
            "clamped sink");
    }

    /// <summary>
    /// The rise matches the cartridge's byte-wise update every frame, from the submerged pause's start and from a
    /// start whose deceleration stalls the speed below zero every frame.
    /// </summary>
    private static void VerifyRiseMatchesNativeBytes()
    {
        CompareRise(0, SnesLittleEndian.Word(0, 3), 0, 280, "resting rise");
        CompareRise(SnesLittleEndian.Word(0, 30), SnesLittleEndian.Word(5, 0), SnesLittleEndian.Word(77, 9), 400,
            "stalled rise");
    }

    /// <summary>Runs the production sink beside the native reference until the reference reaches the acid floor.</summary>
    private static void CompareSink(ushort acceleration, ushort speed, ushort subposition, ushort y, string name)
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        RoomEnemySlot body = state.Body;
        state.DeathSequenceIndex = CrocomireDeathPhase.FirstHopSink;
        state.VerticalAcceleration = acceleration;
        state.VerticalSpeed = speed;
        // The whole subposition word, whose high byte is the vertical subpixel; combat names it the projectile counter.
        state.ProjectileCounter = subposition;
        body.Position = body.Position.WithWholeY(y);
        for (int frame = 0; frame < 400; frame++)
        {
            bool moving = NativeSinkStep(ref acceleration, ref speed, ref subposition, ref y);
            enemies.CrocomireBehavior.BridgeCollapse.RunCrocomireSink(state);
            if (!moving)
            {
                AssertEqual(CrocomireDeathPhase.FirstHopSink.Next(), state.DeathSequenceIndex, $"{name}: ends on the native frame");
                return;
            }
            AssertMotionWords(state, acceleration, speed, subposition, y, $"{name} frame {frame}");
        }
        throw new InvalidOperationException($"Verification failed: {name} did not reach the acid floor.");
    }

    /// <summary>Runs the production rise beside the native reference until the reference clears the surface.</summary>
    private static void CompareRise(ushort acceleration, ushort speed, ushort subposition, ushort y, string name)
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        RoomEnemySlot body = state.Body;
        state.DeathSequenceIndex = CrocomireDeathPhase.FirstHopRise;
        state.VerticalAcceleration = acceleration;
        state.VerticalSpeed = speed;
        // The whole subposition word, whose high byte is the vertical subpixel; combat names it the projectile counter.
        state.ProjectileCounter = subposition;
        body.Position = body.Position.WithWholeY(y);
        for (int frame = 0; frame < 400; frame++)
        {
            bool moving = NativeRiseStep(ref acceleration, ref speed, ref subposition, ref y);
            enemies.CrocomireBehavior.BridgeCollapse.RunCrocomireRise(state);
            if (!moving)
            {
                AssertEqual(CrocomireDeathPhase.FirstHopRise.Next(), state.DeathSequenceIndex, $"{name}: ends on the native frame");
                return;
            }
            AssertMotionWords(state, acceleration, speed, subposition, y, $"{name} frame {frame}");
        }
        throw new InvalidOperationException($"Verification failed: {name} did not clear the surface.");
    }

    /// <summary>Compares the three motion words and the body's Y with the native reference.</summary>
    private static void AssertMotionWords(CrocomireEnemyState state, ushort acceleration, ushort speed,
        ushort subposition, ushort y, string at)
    {
        AssertEqual(acceleration, state.VerticalAcceleration, $"{at}: acceleration word");
        AssertEqual(speed, state.VerticalSpeed, $"{at}: speed word");
        AssertEqual(subposition, state.ProjectileCounter, $"{at}: subposition word");
        AssertEqual(y, state.Body.Position.Y.Whole, $"{at}: Y");
    }

    /// <summary>
    /// The cartridge's sink, byte by byte: the acceleration's fraction carries into a whole part clamped at
    /// forty-eight, that whole part carries into a speed clamped at three, and the speed's fraction carries from the
    /// subposition's high byte into Y. Returns false on the frame the body has reached the acid floor.
    /// </summary>
    private static bool NativeSinkStep(ref ushort acceleration, ref ushort speed, ref ushort subposition, ref ushort y)
    {
        if (unchecked((short)(y - 280)) >= 0)
            return false;
        byte accelerationFraction = SnesLittleEndian.LowByte(acceleration);
        int accelerationCarry = accelerationFraction + 128 > byte.MaxValue ? 1 : 0;
        accelerationFraction = unchecked((byte)(accelerationFraction + 128));
        int accelerationWhole = Math.Min(SnesLittleEndian.HighByte(acceleration) + 3 + accelerationCarry, 48);
        acceleration = SnesLittleEndian.Word(accelerationFraction, checked((byte)accelerationWhole));

        byte speedFraction = SnesLittleEndian.LowByte(speed);
        int speedCarry = speedFraction + accelerationWhole > byte.MaxValue ? 1 : 0;
        speedFraction = unchecked((byte)(speedFraction + accelerationWhole));
        int speedWhole = Math.Min(SnesLittleEndian.HighByte(speed) + speedCarry, 3);
        speed = SnesLittleEndian.Word(speedFraction, checked((byte)speedWhole));

        int subpositionSum = SnesLittleEndian.HighByte(subposition) + speedFraction;
        subposition = SnesLittleEndian.Word(SnesLittleEndian.LowByte(subposition), unchecked((byte)subpositionSum));
        y = unchecked((ushort)(y + speedWhole + (subpositionSum > byte.MaxValue ? 1 : 0)));
        return true;
    }

    /// <summary>
    /// The cartridge's rise, byte by byte: the acceleration grows by one pixel up to thirty-one, its whole part is
    /// borrowed from the speed, a speed that would go negative stalls at 255/256, and the speed's fraction borrows
    /// from the subposition's high byte. Returns false on the frame the body has cleared the surface.
    /// </summary>
    private static bool NativeRiseStep(ref ushort acceleration, ref ushort speed, ref ushort subposition, ref ushort y)
    {
        if (unchecked((short)(y - 218)) < 0)
            return false;
        acceleration = unchecked((ushort)Math.Min(acceleration + SnesEightEight.FromWhole(1), SnesEightEight.FromWhole(31)));
        int accelerationWhole = SnesLittleEndian.HighByte(acceleration);

        byte speedFraction = SnesLittleEndian.LowByte(speed);
        bool speedBorrow = speedFraction < accelerationWhole;
        speedFraction = unchecked((byte)(speedFraction - accelerationWhole));
        int speedWhole = SnesLittleEndian.HighByte(speed) - (speedBorrow ? 1 : 0);
        if (speedWhole < 0)
        {
            speedFraction = byte.MaxValue;
            speedWhole = 0;
        }
        speed = SnesLittleEndian.Word(speedFraction, checked((byte)speedWhole));

        byte subpositionHigh = SnesLittleEndian.HighByte(subposition);
        bool subpositionBorrow = subpositionHigh < speedFraction;
        subposition = SnesLittleEndian.Word(SnesLittleEndian.LowByte(subposition), unchecked((byte)(subpositionHigh - speedFraction)));
        y = unchecked((ushort)(y - speedWhole - (subpositionBorrow ? 1 : 0)));
        return true;
    }

    /// <summary>
    /// The skeleton's wall-break drift, arc and collapse keep its 16.16 speed in the two slot words exactly as the
    /// cartridge's split fraction and whole words do: each acceleration carries from the fraction, each clamp keeps
    /// the fraction, the end of the drift clears only the fraction, and every move carries into the position.
    /// </summary>
    private static void VerifySkeletonMotionMatchesNativeWords()
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        RoomEnemySlot body = state.Body;
        CrocomireBehavior behavior = enemies.CrocomireBehavior;
        CrocomireDeathState death = behavior.Access.CrocomireDeath ?? throw new InvalidOperationException("Verification requires the death state.");
        ushort fraction = 0;
        ushort whole = 0;
        state.SkeletonVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)whole), fraction);
        body.Position = body.Position.WithWhole(new(100, 54));
        FixedPosition<RoomPixels> position = body.Position;
        int delay = 40;
        death.RumbleCooldown = WrappingCountdown.Start(delay);
        state.DeathSequenceIndex = CrocomireDeathPhase.DelaySkeletonFall;

        // The drift right of the broken wall, while the skeleton is left of X 224.
        for (int frame = 1; frame <= delay; frame++)
        {
            NativeAccelerate(ref fraction, ref whole, SnesSixteenSixteen.FromEightEight(128u), 2);
            position = position.WithX(NativeAdvance(position.X, fraction, whole));
            if (frame == delay)
                fraction = 0;
            behavior.Skeleton.RunCrocomireWallBreakDelay(state);
            AssertSkeletonWords(state, fraction, whole, position, $"skeleton drift frame {frame}");
        }
        AssertEqual(CrocomireDeathPhase.DelaySkeletonFall.Next(), state.DeathSequenceIndex, "the drift ends with its delay");

        // The arc into the arena, falling 224/256 pixel a frame, until the skeleton passes X 576.
        for (int frame = 1; frame <= 400; frame++)
        {
            NativeAccelerate(ref fraction, ref whole, SnesSixteenSixteen.FromEightEight(8u), 5);
            position = position.WithY(NativeAdvance(position.Y, SnesLittleEndian.Word(0, 224), 0));
            position = position.WithX(NativeAdvance(position.X, fraction, whole));
            behavior.Skeleton.RunCrocomireSkeletonArc(state);
            AssertSkeletonWords(state, fraction, whole, position, $"skeleton arc frame {frame}");
            if (unchecked((short)(position.X.Whole - 576)) >= 0)
                break;
        }
        AssertEqual(CrocomireDeathPhase.ArcSkeletonIntoArena.Next(), state.DeathSequenceIndex, "the arc ends past X 576");

        // The collapse keeps accelerating the speed, clamped at six pixels, while the skeleton falls apart.
        state.BodyCursor = AnimationProgramCursor<CrocomireBodyProgram>.Start(CrocomireBodyProgram.SkeletonFallsApart);
        for (int frame = 1; frame <= 20; frame++)
        {
            NativeAccelerate(ref fraction, ref whole, SnesSixteenSixteen.FromEightEight(16u), 6);
            behavior.Skeleton.RunCrocomireSkeletonCollapse(state);
            AssertEqual(fraction, state.SkeletonVelocity.Fraction, $"skeleton collapse frame {frame}: speed fraction word");
            AssertEqual(whole, state.SkeletonVelocity.WholeWord, $"skeleton collapse frame {frame}: speed whole word");
        }
    }

    /// <summary>Compares the skeleton's two speed words and its 16.16 position with the native reference.</summary>
    private static void AssertSkeletonWords(CrocomireEnemyState state, ushort fraction, ushort whole,
        FixedPosition<RoomPixels> position, string at)
    {
        AssertEqual(fraction, state.SkeletonVelocity.Fraction, $"{at}: speed fraction word");
        AssertEqual(whole, state.SkeletonVelocity.WholeWord, $"{at}: speed whole word");
        AssertEqual(position, state.Body.Position, $"{at}: position");
    }

    /// <summary>The cartridge's split 16.16 speed update: add, then clamp the whole word and keep the fraction.</summary>
    private static void NativeAccelerate(ref ushort fraction, ref ushort whole, uint delta, ushort maximumWhole)
    {
        uint speed = unchecked(SnesSixteenSixteen.Compose(whole, fraction) + delta);
        if (SnesFixedPosition.WholeOf(unchecked((int)speed)) >= maximumWhole)
            speed = SnesSixteenSixteen.Compose(maximumWhole, unchecked((ushort)SnesSixteenSixteen.FractionOf(speed)));
        fraction = unchecked((ushort)SnesSixteenSixteen.FractionOf(speed));
        whole = SnesFixedPosition.WholeOf(unchecked((int)speed));
    }

    /// <summary>The cartridge's 32-bit position add of a split whole and fraction speed.</summary>
    private static SnesFixedPosition NativeAdvance(SnesFixedPosition position, ushort fraction, ushort whole) =>
        SnesFixedPosition.FromRaw(unchecked(position.RawValue + SnesSixteenSixteen.Compose(whole, fraction)));

    /// <summary>
    /// The melt shake counter starts from zero, so its first dissolve frame wraps it to the native word's maximum,
    /// and every frame its second bit chooses whether the body stands four pixels right of where the melt began.
    /// </summary>
    private static void VerifyMeltShakeCounterWraps()
    {
        var fixture = new EnemyEffectsFixtureVerification.CrocomireEffectFixture(EnemyEffectsFixtureVerification.MeltEffectArtwork());
        CrocomireMeltingPass pass = CrocomireMeltingTransferDefinitions.Passes[0];
        fixture.InitializeMap((int)CrocomireMeltingTilemapAddress.FirstTilemap);
        fixture.Call(c => c.Melting.InitializeCrocomireMeltingGraphics);
        for (int record = 0; record <= pass.Uploads.Length; record++)
            fixture.Call(c => c.Melting.UploadNextCrocomireMeltingGraphicsSlice);
        fixture.Call(c => c.Melting.BeginCrocomireMelting);
        AssertEqual(0, fixture.Effect.MeltShakeCounter.RawValue, "the melt shake counter starts from zero");

        int startX = fixture.Effect.BodyXBeforeMelting;
        ushort nativeCounter = 0;
        for (int frame = 1; frame <= 8; frame++)
        {
            nativeCounter = WrappingCounter.Decrement(nativeCounter).Value;
            int jitter = (nativeCounter & 2) != 0 ? 4 : 0;
            fixture.Dissolve();
            AssertEqual(nativeCounter, fixture.Effect.MeltShakeCounter.RawValue, $"melt frame {frame}: shake counter word");
            AssertEqual(startX + jitter, fixture.Actor.Body.Position.X.Whole, $"melt frame {frame}: shaken X");
        }
    }

    /// <summary>Creates a Crocomire body with its encounter and death state in a fresh enemy system.</summary>
    private static NewCrocomireResult NewCrocomire()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Crocomire, body.Definition);
        body.Position = body.Position.WithWhole(new(576, 144));
        enemies.Presentation.BindVram(BgArtworkFixtures.For(new SnesVram()));
        var state = new CrocomireEnemyState(body);
        enemies.CrocomireBehavior.Access.Crocomire = state;
        enemies.CrocomireBehavior.Access.CrocomireDeath = new CrocomireDeathState();
        enemies.VerificationServices().ReadRandom = () => ushort.MaxValue;
        enemies.VerificationServices().NextRandom = () => ushort.MaxValue;
        return new NewCrocomireResult(enemies, state);
    }

    /// <summary>Runs one body update through the production interpreter.</summary>
    private static void Pass(RoomEnemySystem enemies, CrocomireEnemyState state)
    {
        enemies.CrocomireBehavior.BeginEnemyFrame(new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false });
        EnemyInstructionInterpreter.ProcessInstructions(state.Body, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>Runs Crocomire's state-quantity checks from the command dispatcher.</summary>
    [VerificationCommand("--crocomire-state-quantities")]
    internal static int CrocomireStateQuantitiesCommand()
    {
        Suite(nameof(VerifyCrocomireStateQuantities), VerifyCrocomireStateQuantities);
        return 0;
    }
}
