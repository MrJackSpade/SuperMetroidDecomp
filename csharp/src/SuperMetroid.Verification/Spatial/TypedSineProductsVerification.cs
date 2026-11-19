using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms the #2428 conversions of the hand-written "length times the game's sine" products: the reach rounded toward
/// negative infinity (Mother Brain's neck, Ridley's tail), the reach rounded toward zero (Phantoon's flames, the charged
/// beam combos) and the fine sine's coarse reading (the Bull, the Yapping Maw) each equal the raw 8.8 arithmetic they
/// replace over every angle and every length their callers pass; and the #2453 conversions of the enemies' raw-int
/// arithmetic, the homing pull and the aim's turn from its slope, equal it over their whole input ranges; the #2491
/// conversions, a liquid tide's sine displacement and a displacement written as a fraction of a pixel, equal the raw
/// arithmetic they replace; and the #2532 Bull step equals its raw 256ths product over every angle and speed.
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
            VerifyTideDisplacement() + VerifyPixelFractions() + VerifyBullStep();
        Console.WriteLine($"  Typed sine products: {compared} rounded-down, toward-zero and coarse fine-sine products, homing pulls, aim turns, tide displacements, pixel fractions and Bull steps equal the raw arithmetic they replace.");
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
                SignedSixteenSixteen forward = UnsignedSixteenSixteen.ProductOfTwoFiftySixths(coarseSize, speed).AsDisplacement();
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
