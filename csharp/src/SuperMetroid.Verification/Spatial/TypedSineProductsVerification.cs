using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms the #2428 conversions of the hand-written "length times the game's sine" products: the reach rounded toward
/// negative infinity (Mother Brain's neck, Ridley's tail), the reach rounded toward zero (Phantoon's flames, the charged
/// beam combos) and the fine sine's coarse reading (the Bull, the Yapping Maw) each equal the raw 8.8 arithmetic they
/// replace over every angle and every length their callers pass; and the #2453 conversions of the enemies' raw-int
/// arithmetic, the homing pull and the aim's turn from its slope, equal it over their whole input ranges; the #2491
/// conversions, a liquid tide's sine displacement and a displacement written as a fraction of a pixel, equal the raw
/// arithmetic they replace; the #2532 Bull step equals its raw product over every angle and speed; the #2537 short circle
/// sine, coarse curve rise and finest curve steps equal the arithmetic they replace over their input ranges; and the #2539
/// velocity along an angle, the bosses' sine velocities and a speed written as a fraction of a pixel equal the
/// hand-written arithmetic they replace.
/// </summary>
internal static class TypedSineProductsVerification
{
    /// <summary>The shortest length compared: the most negative whole-pixel distance a signed 8.8 distance holds.</summary>
    private const int ShortestLength = sbyte.MinValue;

    /// <summary>The longest length compared: past the short circle product's radius span, so its wrap is covered too.</summary>
    private const int LongestLength = 511;

    /// <summary>Runs every comparison of the typed sine products with the arithmetic they replace.</summary>
    internal static void VerifyTypedSineProducts()
    {
        int compared = VerifyReachRoundedDown() + VerifyReachTowardZero() + VerifyCoarseSizeOfFineSine() + VerifyHomingPull() + VerifyAimTurnSteps() +
            VerifyTideDisplacement() + VerifyPixelFractions() + VerifyBullStep() + VerifyCircleSine() + VerifyCoarseRise() +
            VerifyCurveSteps() + VerifyVelocityAlong() + VerifyBossSineVelocities() + VerifyUnsignedPixelFractions();
        Console.WriteLine($"  Typed sine products: {compared} rounded-down, toward-zero and coarse fine-sine products, homing pulls, aim turns, tide displacements, pixel fractions, Bull steps, short circle sines, coarse rises, curve steps, velocities along an angle and boss sine velocities equal the raw arithmetic they replace.");
    }

    /// <summary>The longest radius the short circle sine is compared at: two of the product's radius spans.</summary>
    private static readonly int LongestCircleRadius = 2 * ShortCircleProduct.RadiusSpan - 1;

    /// <summary>
    /// The short circle product's sine distance (#2537) equals the arithmetic it replaced for every angle and every radius
    /// through two of the product's radius spans: the coarse half-wave times the radius's place within the span, counted
    /// in 256ths of a pixel and reversed partwise in the turn's second half. A negative radius is refused, as the replaced
    /// product refused it.
    /// </summary>
    private static int VerifyCircleSine()
    {
        int compared = 0;
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var angle = new TrigonometryAngle(raw);
            int sample = Trigonometry.CoarseHalfWave(angle.StepsIntoHalfTurn);
            for (int radius = 0; radius <= LongestCircleRadius; radius++)
            {
                SignedSixteenSixteen product = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(sample * (radius % ShortCircleProduct.RadiusSpan));
                SignedSixteenSixteen old = angle.IsInSecondHalf ? ShortCircleProduct.Reversed(product) : product;
                SignedSixteenSixteen sine = EnemyMath.CircleSine(angle, radius);
                if (sine != old)
                    throw new InvalidDataException($"Angle {raw}, radius {radius}: the short circle sine {sine.RawValue} differs from {old.RawValue}.");
                compared++;
            }
            AssertThrows<ArgumentOutOfRangeException>(() => EnemyMath.CircleSine(angle, -1), $"a short circle sine at angle {raw} refuses a negative radius");
        }
        return compared;
    }

    /// <summary>
    /// A quadratic curve speed's coarse rise (#2537) equals the coarse speed it replaced, counted back into a displacement,
    /// at every mark of the curve; and the displacement read at that precision equals its reading in 256ths counted back,
    /// for every whole pixel at the fractions around a 256th's edges and for every fraction of the whole pixels around zero.
    /// </summary>
    private static int VerifyCoarseRise()
    {
        int compared = 0;
        for (int mark = 0; mark < QuadraticSpeedCurveDefinitions.MarkCount; mark++)
        {
            QuadraticCurveSpeed speed = QuadraticSpeedCurve.AtMark(mark);
            SignedSixteenSixteen old = speed.CoarseToward(VerticalDirection.Down);
            if (speed.CoarseRise != old)
                throw new InvalidDataException($"Mark {mark}: the coarse rise {speed.CoarseRise.RawValue} differs from {old.RawValue}.");
            compared++;
        }
        for (int whole = short.MinValue; whole <= short.MaxValue; whole++)
        {
            foreach (int fraction in (int[])[0, 1, 255, 256, 257, 32768, 65280, 65535])
                compared += CompareRoundedDown(SignedSixteenSixteen.FromParts(whole, fraction));
        }
        foreach (int whole in (int[])[-1, 0, 1])
        {
            for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
                compared += CompareRoundedDown(SignedSixteenSixteen.FromParts(whole, fraction));
        }
        return compared;

        static int CompareRoundedDown(SignedSixteenSixteen value)
        {
            SignedSixteenSixteen old = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(value.InTwoFiftySixths);
            if (value.RoundedDownToTwoFiftySixths() != old)
                throw new InvalidDataException($"{value.RawValue}: rounded down it is {value.RoundedDownToTwoFiftySixths().RawValue}, not {old.RawValue}.");
            return 1;
        }
    }

    /// <summary>
    /// The finest steps that reach a place along the quadratic curve (#2537) equal the place's reading in 256ths of a mark,
    /// which the climb's pass limit read, for every place from the curve's start to one mark past its last; a place before
    /// the start and a step that does not move forward are refused.
    /// </summary>
    private static int VerifyCurveSteps()
    {
        int compared = 0;
        for (int raw = 0; raw <= QuadraticSpeedCurvePlace.PastLastMark.InTwoHundredFiftySixthsOfMark; raw++)
        {
            QuadraticSpeedCurvePlace place = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(raw);
            if (place.StepsOf(QuadraticSpeedCurvePlace.FinestStep) != raw)
                throw new InvalidDataException($"Place {raw}: {place.StepsOf(QuadraticSpeedCurvePlace.FinestStep)} finest steps reach it, not {raw}.");
            compared++;
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(-1).StepsOf(QuadraticSpeedCurvePlace.FinestStep),
            "a place before the curve's start is refused");
        AssertThrows<ArgumentOutOfRangeException>(
            () => QuadraticSpeedCurvePlace.OneMark.StepsOf(QuadraticSpeedCurvePlace.Start), "a step that does not move is refused");
        return compared;
    }

    /// <summary>The slowest speed compared along an angle: none.</summary>
    private const int SlowestAngledSpeed = 0;

    /// <summary>The fastest speed compared along an angle: the most whole pixels a signed displacement holds.</summary>
    private const int FastestAngledSpeed = short.MaxValue;

    /// <summary>The speeds the velocity along an angle is read at for every angle, past its axes' whole-range sweep.</summary>
    private static readonly int[] SampledAngledSpeeds = [0, 1, 2, 3, 4, 5, 8, 16, 127, 128, 255, 256, 4095, FastestAngledSpeed];

    /// <summary>
    /// The velocity along an angle (#2539) equals each hand-written arithmetic it replaces. Each axis's share of every angle
    /// (<see cref="FineUnitComponent"/>), at every whole-pixel speed from none to the most a signed displacement holds, equals
    /// the unsigned half-wave a quarter turn (across) or a half turn (down) ahead times the speed, negated where that turned
    /// angle lies in the turn's second half, as Botwoon's head, Spore Spawn's death drift and the Sbug's forward leg signed
    /// it. At every speed sampled it moves a coordinate as the same magnitude moved back or forward by the quadrant, as
    /// Draygon's angled move and the angled projectiles moved, and reversed equals the magnitude signed the other way, as the
    /// burial Evirs' drift took it. The velocity along every angle is those two shares' speeds at every speed sampled; the launched angled flight and the
    /// six burial Evirs' drifts read it at their sites; and a negative speed is refused, as the replaced products refused it.
    /// </summary>
    private static int VerifyVelocityAlong()
    {
        var start = FixedCoordinate.Of(512, 32769);
        ParallelSweep.For(0, byte.MaxValue + 1, raw =>
        {
            var angle = new TrigonometryAngle(raw);
            foreach ((FineUnitComponent share, TrigonometryAngle turned, string axis) in new (FineUnitComponent, TrigonometryAngle, string)[]
                     {
                         (FineUnitComponent.AcrossOf(angle), angle.TurnedBy(TrigonometryAngle.QuarterTurn), "across"),
                         (FineUnitComponent.DownOf(angle), angle.TurnedBy(TrigonometryAngle.HalfTurn), "down"),
                     })
            {
                bool back = turned.IsInSecondHalf;
                int halfWave = Trigonometry.UnsignedHalfWave(turned.StepsIntoHalfTurn);
                for (int speed = SlowestAngledSpeed; speed <= FastestAngledSpeed; speed++)
                {
                    UnsignedSixteenSixteen magnitude = UnsignedSixteenSixteen.ShareOfWholePixels(speed, halfWave);
                    SignedSixteenSixteen signed = back ? -magnitude.AsDisplacement() : magnitude.AsDisplacement();
                    SignedSixteenSixteen actual = share.SpeedAt(speed);
                    if (actual != signed)
                        throw new InvalidDataException($"Angle {raw}, speed {speed}: the speed {axis} {actual.RawValue} differs from the signed magnitude {signed.RawValue}.");
                }
                AssertThrows<ArgumentOutOfRangeException>(() => share.SpeedAt(-1), $"the speed {axis} along angle {raw} refuses a negative speed");

                // The moves and the reversal follow from the signed speed alone, so the sampled speeds cover them.
                foreach (int speed in SampledAngledSpeeds)
                {
                    UnsignedSixteenSixteen magnitude = UnsignedSixteenSixteen.ShareOfWholePixels(speed, halfWave);
                    SignedSixteenSixteen actual = share.SpeedAt(speed);
                    if (start.MovedBy(actual) != (back ? start.MovedBack(magnitude) : start.MovedForward(magnitude)))
                        throw new InvalidDataException($"Angle {raw}, speed {speed}: the speed {axis} moves a coordinate other than the quadrant's move back or forward.");
                    if (-actual != (back ? magnitude.AsDisplacement() : -magnitude.AsDisplacement()))
                        throw new InvalidDataException($"Angle {raw}, speed {speed}: the reversed speed {axis} differs from the magnitude signed the other way.");
                }
            }

            foreach (int speed in SampledAngledSpeeds)
            {
                FixedVector<RoomPixels> expected = new(FineUnitComponent.AcrossOf(angle).SpeedAt(speed), FineUnitComponent.DownOf(angle).SpeedAt(speed));
                if (EnemyMath.VelocityAlong(angle, speed) != expected)
                    throw new InvalidDataException($"Angle {raw}, speed {speed}: the velocity along it differs from its shares' speeds.");
            }
            AssertThrows<ArgumentOutOfRangeException>(() => EnemyMath.VelocityAlong(angle, -1), $"a velocity along angle {raw} refuses a negative speed");
            if (AngledFlightState.Launch(angle, DraygonProjectileSpeeds.Goop).Velocity != EnemyMath.VelocityAlong(angle, DraygonProjectileSpeeds.Goop))
                throw new InvalidDataException($"Angle {raw}: a launched angled flight's velocity differs from the speed along its heading.");
        });
        foreach (DraygonBurialEvirDefinition evir in DraygonBurialEvirDefinitions.InSpawnOrder)
        {
            TrigonometryAngle acrossAngle = evir.Heading.TurnedBy(TrigonometryAngle.QuarterTurn);
            TrigonometryAngle downAngle = evir.Heading.TurnedBy(TrigonometryAngle.HalfTurn);
            SignedSixteenSixteen across = UnsignedSixteenSixteen.ShareOfWholePixels(1, Trigonometry.UnsignedHalfWave(acrossAngle.StepsIntoHalfTurn)).AsDisplacement();
            SignedSixteenSixteen down = UnsignedSixteenSixteen.ShareOfWholePixels(1, Trigonometry.UnsignedHalfWave(downAngle.StepsIntoHalfTurn)).AsDisplacement();
            var old = new FixedVector<RoomPixels>(acrossAngle.IsInSecondHalf ? across : -across,
                downAngle.IsInSecondHalf ? down : -down);
            AssertEqual(old, DraygonDeath.BurialDrift(evir), $"the burial Evir heading {evir.Heading.RawValue} drifts back along it");
        }

        return (byte.MaxValue + 1) * (2 * (FastestAngledSpeed - SlowestAngledSpeed + 1) + SampledAngledSpeeds.Length);
    }

    /// <summary>
    /// The bosses' sine velocities (#2539) equal the raw products they replace at every angle: Crocomire's volley shot, its
    /// signed sine and negative cosine each four times in 256ths of a pixel, and Mother Brain's glass shard, its sine once
    /// and its negative cosine four times.
    /// </summary>
    private static int VerifyBossSineVelocities()
    {
        int compared = 0;
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            // The cartridge reads the aim word clockwise from up as its sine's angle: X its sine, Y its negative cosine.
            var heading = new TrigonometryAngle(raw);
            var volley = new PixelVelocity<RoomPixels>(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(Trigonometry.SignedSine(heading) * 4), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(Trigonometry.SignedSine(heading - TrigonometryAngle.QuarterTurn) * 4));
            AssertEqual(volley, CrocomireCombat.CalculateCrocomireProjectileVelocity(AimAngleConstants.FromClockwiseSteps(raw)), $"Crocomire's volley velocity at bearing {raw}");

            var angle = new TrigonometryAngle(raw);
            AssertEqual(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(Trigonometry.SignedSine(angle)),
                EnemyMath.SineVelocity(angle, MotherBrainGlassShardDefinitions.LaunchAcrossSpeed), $"a glass shard's sideways launch at angle {raw}");
            AssertEqual(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4 * Trigonometry.SignedSine(angle - TrigonometryAngle.QuarterTurn)),
                EnemyMath.SineVelocity(angle - TrigonometryAngle.QuarterTurn, MotherBrainGlassShardDefinitions.LaunchDownSpeed),
                $"a glass shard's vertical launch at angle {raw}");
            compared += 3;
        }

        return compared;
    }

    /// <summary>
    /// A speed written as a fraction of a pixel (#2539) equals the fractional word it replaces for every power-of-two
    /// denominator and the numerators the catalogs pass, and a negative numerator or a denominator that is no power of two
    /// fails.
    /// </summary>
    private static int VerifyUnsignedPixelFractions()
    {
        int compared = 0;
        for (int denominator = 1; denominator <= 65536; denominator *= 2)
        {
            foreach (int numerator in (int[])[0, 1, 3, 7])
            {
                UnsignedSixteenSixteen expected = UnsignedSixteenSixteen.FromRaw((uint)(numerator * (65536 / denominator)));
                if (UnsignedSixteenSixteen.FromPixelFraction(numerator, denominator) != expected)
                    throw new InvalidDataException($"{numerator}/{denominator} of a pixel differs from {expected.RawValue} 65536ths.");
                compared++;
            }
        }
        AssertEqual(new UnsignedSixteenSixteen(0, 384), UnsignedSixteenSixteen.FromPixelFraction(3, 512), "Crocomire's distortion slope growth");
        AssertThrows<ArgumentOutOfRangeException>(() => UnsignedSixteenSixteen.FromPixelFraction(1, 3),
            "a third of a pixel, which no 65536ths hold exactly, is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => UnsignedSixteenSixteen.FromPixelFraction(-1, 2), "a negative speed is rejected");
        return compared + 1;
    }

    /// <summary>
    /// A Bull's step along an axis (#2532) equals the arithmetic it replaced for every angle and every speed from zero to the
    /// largest a signed 8.8 speed holds: the fine sine's coarse size, its low eight bits shifted out, times the speed in
    /// 256ths, counted in 65536ths of a pixel and reversed through <see cref="BullBackwardStep"/> where the fine sine is
    /// negative. A negative speed is refused, as the replaced product refused it.
    /// </summary>
    private static int VerifyBullStep()
    {
        int compared = 0;
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var angle = new TrigonometryAngle(raw);
            int fineSine = Trigonometry.FineSignedSine(angle);
            int coarseSize = EightEightOracle.UnsignedWhole(Math.Abs(fineSine));
            UnitComponent share = UnitComponent.CoarseFineSineOf(angle);
            for (int speed = 0; speed <= short.MaxValue; speed++)
            {
                SignedSixteenSixteen forward = UnsignedSixteenSixteenConstants.ProductOfTwoFiftySixths(coarseSize, speed).AsDisplacement();
                SignedSixteenSixteen old = fineSine < 0 ? BullBackwardStep.Of(forward) : forward;
                SignedSixteenSixteen step = BullBehavior.BullStep(share, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(speed));
                if (step != old)
                    throw new InvalidDataException($"Angle {raw}, speed {speed}/256: the Bull step {step.RawValue} differs from {old.RawValue}.");
                compared++;
            }
            AssertThrows<ArgumentOutOfRangeException>(
                () => BullBehavior.BullStep(share, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1)),
                $"a Bull step at angle {raw} refuses a negative speed");
        }

        return compared;
    }

    /// <summary>
    /// A liquid tide's displacement (#2491) equals the signed sine times the tide's crest height counted in 256ths of a pixel,
    /// as the tide computed it, over every angle and both tides, and lies above the base line exactly while the sine is
    /// negative, which selects the tide's phase step.
    /// </summary>
    private static int VerifyTideDisplacement()
    {
        int compared = 0;
        foreach (int peak in (int[])[LiquidTideDefinitions.SmallTidePeakPixels, LiquidTideDefinitions.LargeTidePeakPixels])
        {
            for (int raw = 0; raw <= byte.MaxValue; raw++)
            {
                var angle = new TrigonometryAngle(raw);
                int sample = Trigonometry.SignedSine(angle);
                SignedSixteenSixteen displacement = Trigonometry.SineDisplacement(angle, peak);
                if (displacement != SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(sample * peak) || displacement.IsNegative != sample < 0)
                    throw new InvalidDataException($"Angle {raw}, crest {peak}: the tide displacement {displacement.RawValue} differs from the sine {sample} times the crest.");
                compared++;
            }
        }
        return compared;
    }

    /// <summary>
    /// A displacement written as a fraction of a pixel (#2491) equals the fractional word it replaces for every power-of-two
    /// denominator and the numerators the catalogs pass, and a denominator that is no power of two fails.
    /// </summary>
    private static int VerifyPixelFractions()
    {
        int compared = 0;
        for (int denominator = 1; denominator <= 65536; denominator *= 2)
        {
            foreach (int numerator in (int[])[-3, -1, 0, 1, 3])
            {
                SignedSixteenSixteen expected = SignedSixteenSixteen.FromParts(0, 1) * (numerator * (65536 / denominator));
                if (SignedSixteenSixteen.FromPixelFraction(numerator, denominator) != expected)
                    throw new InvalidDataException($"{numerator}/{denominator} of a pixel differs from {expected.RawValue} 65536ths.");
                compared++;
            }
        }
        if (SignedSixteenSixteen.FromPixelFraction(1, 512) != SignedSixteenSixteen.FromParts(0, 128) ||
            SignedSixteenSixteen.FromPixelFraction(1, 2048) != SignedSixteenSixteen.FromParts(0, 32))
            throw new InvalidDataException("The ending's 512th and 2048th of a pixel differ from the fractional words 128 and 32.");
        AssertThrows<ArgumentOutOfRangeException>(() => SignedSixteenSixteen.FromPixelFraction(1, 3),
            "a third of a pixel, which no 65536ths hold exactly, is rejected");
        return compared + 2;
    }

    /// <summary>
    /// The reach rounded toward negative infinity equals the signed sine times the length with its 256ths shifted out, as
    /// Mother Brain's neck and Ridley's tail computed it, for every angle and every length, negative lengths included.
    /// </summary>
    private static int VerifyReachRoundedDown()
    {
        int compared = 0;
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var angle = new TrigonometryAngle(raw);
            int sine = Trigonometry.SignedSine(angle);
            UnitComponent component = UnitComponent.SineOf(angle);
            for (int length = ShortestLength; length <= LongestLength; length++)
            {
                int neckProduct = EightEightOracle.WholeOf(sine * length);
                int tailProduct = EightEightOracle.Multiply(length, sine);
                int reach = component.ReachRoundedDownOf(length);
                if (reach != neckProduct || reach != tailProduct)
                    throw new InvalidDataException(
                        $"Angle {raw}, length {length}: the rounded-down reach {reach} differs from the neck's {neckProduct} or the tail's {tailProduct}.");
                if (length >= 0 && MotherBrainNeckKinematics.CalculateSignedComponent(angle, length) != neckProduct)
                    throw new InvalidDataException($"Angle {raw}, distance {length}: Mother Brain's neck component differs from {neckProduct}.");
                compared++;
            }
        }

        return compared;
    }

    /// <summary>
    /// The reach rounded toward zero equals Phantoon's flame product (the radius times the half-turn sample's fraction and
    /// whole part, negated over the second half turn) and the combos' product (the radius times the sample's size, its
    /// sign restored) for every angle and every non-negative length, and the two call sites read it.
    /// </summary>
    private static int VerifyReachTowardZero()
    {
        int compared = 0;
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var angle = new TrigonometryAngle(raw);
            UnitComponent component = UnitComponent.SineOf(angle);
            for (int length = 0; length <= LongestLength; length++)
            {
                int flame = OldFlameComponent(angle, length);
                int combo = OldComboComponent(angle, length);
                int reach = component.ReachOf(length);
                if (reach != flame || reach != combo)
                    throw new InvalidDataException(
                        $"Angle {raw}, length {length}: the toward-zero reach {reach} differs from the flame's {flame} or the combo's {combo}.");
                int flameAtSite = PhantoonCombat.ReadPhantoonFlameComponent(angle, length);
                int expectedAtSite = OldFlameComponent(angle, ShortCircleProduct.RadiusInUse(length));
                if (flameAtSite != expectedAtSite)
                    throw new InvalidDataException($"Angle {raw}, radius {length}: Phantoon's flame component {flameAtSite} differs from {expectedAtSite}.");
                if (length <= ComboRingCollapse.LargestRadius)
                {
                    Offset16<RoomPixels> offset = SamusComboMechanics.GetSineOffset(angle, length);
                    var expected = new Offset16<RoomPixels>(combo, OldComboComponent(angle - TrigonometryAngle.QuarterTurn, length));
                    if (offset != expected)
                        throw new InvalidDataException($"Angle {raw}, amplitude {length}: the combo offset {offset} differs from {expected}.");
                }

                compared++;
            }
        }

        return compared;
    }

    /// <summary>
    /// The fine sine's coarse size equals the fine sine's magnitude with its low eight bits shifted out, as the Bull and the
    /// Yapping Maw read it, for every angle, and the Maw's offset equals its replaced arithmetic for every angle and length.
    /// </summary>
    private static int VerifyCoarseSizeOfFineSine()
    {
        int compared = 0;
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var angle = new TrigonometryAngle(raw);
            int expected = EightEightOracle.UnsignedWhole(Math.Abs(Trigonometry.FineSignedSine(angle)));
            AssertEqual(expected, Trigonometry.CoarseSizeOfFineSine(angle), $"the fine sine's coarse size at angle {raw}");
            compared++;
            for (int length = 0; length <= LongestLength; length++)
            {
                int signedSample = -Trigonometry.FineSignedSine(angle);
                int sampleWhole = EightEightOracle.UnsignedWhole(Math.Abs(signedSample));
                SignedEightEight offset = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(sampleWhole * ShortCircleProduct.RadiusInUse(length));
                int old = 2 * (signedSample < 0 ? -offset : offset).WholePixels;
                int actual = YappingMawBehavior.CalculateYappingMawY(angle, length);
                if (actual != old)
                    throw new InvalidDataException($"Angle {raw}, length {length}: the Yapping Maw offset {actual} differs from {old}.");
                compared++;
            }
        }

        return compared;
    }

    /// <summary>
    /// The homing pull equals the distance shifted right twice into 256ths of a pixel, as the Metroids computed it, for
    /// every distance a signed distance holds.
    /// </summary>
    private static int VerifyHomingPull()
    {
        int compared = 0;
        for (int distance = short.MinValue; distance <= short.MaxValue; distance++)
        {
            SignedSixteenSixteen old = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(distance >> 2);
            SignedSixteenSixteen pull = EnemyMath.HomingPull(distance);
            if (pull != old)
                throw new InvalidDataException($"Distance {distance}: the homing pull {pull} differs from {old}.");
            compared++;
        }

        return compared;
    }

    /// <summary>
    /// The aim's turn in steps equals the slope shifted right three times, as the bearing computed it, for every pair of
    /// sides up to past the long side's span, so its remainder and the steepest slope are covered too.
    /// </summary>
    private static int VerifyAimTurnSteps()
    {
        int compared = 0;
        for (int shortSide = 0; shortSide <= LongestLength; shortSide++)
        {
            for (int longSide = 0; longSide <= LongestLength; longSide++)
            {
                int old = AimAngleSlope.Of(shortSide, longSide) >> 3;
                int turn = AimAngleSlope.TurnSteps(shortSide, longSide);
                if (turn != old)
                    throw new InvalidDataException($"Sides {shortSide} and {longSide}: the aim's turn {turn} differs from {old}.");
                compared++;
            }
        }

        return compared;
    }

    /// <summary>Phantoon's replaced flame product: the half-turn sample's fraction and whole part each times the radius.</summary>
    private static int OldFlameComponent(TrigonometryAngle angle, int radius)
    {
        int sample = Trigonometry.SignedSine(angle.InFirstHalfTurn());
        int magnitude = EightEightOracle.Multiply(EightEightOracle.FractionOf(sample), radius) + EightEightOracle.WholeOf(sample) * radius;
        return angle.IsInSecondHalf ? -magnitude : magnitude;
    }

    /// <summary>The combos' replaced product: the sample's size times the amplitude, its 256ths dropped, the sign restored.</summary>
    private static int OldComboComponent(TrigonometryAngle angle, int amplitude)
    {
        int sample = Trigonometry.SignedSine(angle);
        int magnitude = EightEightOracle.Multiply(Math.Abs(sample), amplitude);
        return sample < 0 ? -magnitude : magnitude;
    }
}
