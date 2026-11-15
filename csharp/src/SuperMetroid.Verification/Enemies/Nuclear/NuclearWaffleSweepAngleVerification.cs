using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Nuclear;

/// <summary>
/// Checks that the Nuclear Waffle's typed sweep angles and turning speed (#2042) reproduce the integer angles and the
/// 16.16 sweep angle and speed they replaced, over every value each can take.
/// </summary>
internal static class NuclearWaffleSweepAngleVerification
{
    /// <summary>The most whole steps the replaced 16.16 sweep angle read as a nonnegative angle.</summary>
    private const int MostOldSteps = short.MaxValue;

    /// <summary>
    /// Compares both sweep definitions with the integers they replaced; every sweep angle, whole steps and fraction, with
    /// the 16.16 word's whole part, value and wrapped orbit angle; the order of every pair of whole angles a sweep can
    /// compare; every link offset; and every linear-speed row, forward and reversed, turning every angle a sweep reaches.
    /// </summary>
    internal static void VerifyNuclearWaffleSweepAngle()
    {
        AssertSweep(NuclearWaffleDefinitions.BackwardSweep, new SweepIntegers(400, 240, -24, -12, 384, 256), "backward");
        AssertSweep(NuclearWaffleDefinitions.ForwardSweep, new SweepIntegers(240, 400, 24, 12, 256, 384), "forward");

        string? mismatch = null;
        for (int steps = 0; steps <= MostOldSteps; steps++)
        {
            NuclearWaffleSweepAngle whole = NuclearWaffleSweepAngle.FromSteps(steps);
            for (int fraction = 0; fraction < 256; fraction++)
            {
                NuclearWaffleSweepAngle angle = whole + AngularSpeed.FromFractionsOfStep(fraction);
                SignedSixteenSixteen old = SignedSixteenSixteen.FromParts(steps, fraction * 256);
                if (Widened(angle) != old || angle.WholeStep != whole ||
                    angle.ToRotationAngle().ToTrigonometryAngle() != TrigonometryAngle.Wrapped(old.WholePixels))
                {
                    mismatch ??= $"sweep angle {steps} and {fraction}/256 differs from the 16.16 angle";
                }
            }
        }

        // Every whole angle a sweep compares lies from 140 to 500 steps; the pairs cover every whole angle below three turns.
        const int mostComparedSteps = 3 * RotationAngle.StepsPerTurn;
        for (int left = 0; left < mostComparedSteps; left++)
        {
            NuclearWaffleSweepAngle leftAngle = NuclearWaffleSweepAngle.FromSteps(left);
            for (int right = 0; right < mostComparedSteps; right++)
            {
                NuclearWaffleSweepAngle rightAngle = NuclearWaffleSweepAngle.FromSteps(right);
                if ((left < right) != (leftAngle < rightAngle) || (left >= right) != (leftAngle >= rightAngle))
                    mismatch ??= $"sweep angles {left} and {right} compare unlike the integers";
            }
            for (int offset = -4 * 24; offset <= 4 * 24; offset++)
            {
                if (left + offset >= 0 && leftAngle.TurnedBySteps(offset) != NuclearWaffleSweepAngle.FromSteps(left + offset))
                    mismatch ??= $"sweep angle {left} turned by {offset} steps differs from the integer sum";
            }
        }

        for (int row = 0; row < EnemyLinearSpeedTable.RowCount; row++)
        {
            AngularSpeed speed = EnemyTurnRateSelectors.LinearTurnRate("Nuclear Waffle speed", row);
            AssertEqual(EnemyLinearSpeedTable.Speed(row), Widened(speed), $"linear-speed row {row} turns at its 16.16 speed");
            AssertEqual(row, EnemyTurnRateSelectors.LinearTurnRateRow("Nuclear Waffle speed", speed), $"linear-speed row {row} round-trips");
            foreach (AngularSpeed turn in new[] { speed, -speed })
            {
                for (int steps = 128; steps < 2 * RotationAngle.StepsPerTurn; steps++)
                {
                    for (int sixteenths = 0; sixteenths < 16; sixteenths++)
                    {
                        NuclearWaffleSweepAngle angle = NuclearWaffleSweepAngle.FromSteps(steps) + AngularSpeed.FromFractionsOfStep(sixteenths * 16);
                        if (Widened(angle + turn) != Widened(angle) + Widened(turn))
                            mismatch ??= $"sweep angle {angle} turned at {turn} differs from the 16.16 sum";
                    }
                }
            }
        }
        AssertThrows<InvalidDataException>(() => EnemyTurnRateSelectors.LinearTurnRate("Nuclear Waffle speed", EnemyLinearSpeedTable.RowCount),
            "a turning-speed selector past the linear-speed table is rejected");

        AssertEqual<string?>(null, mismatch, "every typed sweep angle and speed reproduces the integer arithmetic");
        Console.WriteLine("Nuclear Waffle sweep angle: every sweep angle, compared pair, link offset and linear turning speed matches the integer arithmetic.");
    }

    /// <summary>The integers a sweep definition held before #2042, in its parameters' order.</summary>
    private readonly record struct SweepIntegers(int Start, int End, int Spacing, int InterleavedOffset, int Rising, int Falling);

    /// <summary>Checks one sweep definition against the integers it replaced.</summary>
    private static void AssertSweep(NuclearWaffleSweepDefinition sweep, SweepIntegers expected, string name)
    {
        AssertEqual(NuclearWaffleSweepAngle.FromSteps(expected.Start), sweep.StartAngle, $"the {name} sweep's start angle");
        AssertEqual(NuclearWaffleSweepAngle.FromSteps(expected.End), sweep.EndAngle, $"the {name} sweep's end angle");
        AssertEqual(expected.Spacing, sweep.SegmentSpacing, $"the {name} sweep's link spacing");
        AssertEqual(expected.InterleavedOffset, sweep.InterleavedSegmentOffset, $"the {name} sweep's interleaved offset");
        AssertEqual(NuclearWaffleSweepAngle.FromSteps(expected.Rising), sweep.RisingTurnThreshold, $"the {name} sweep's rising threshold");
        AssertEqual(NuclearWaffleSweepAngle.FromSteps(expected.Falling), sweep.FallingTurnThreshold, $"the {name} sweep's falling threshold");
    }

    /// <summary>The 16.16 word the sweep angle was held in, for <paramref name="angle"/>.</summary>
    private static SignedSixteenSixteen Widened(NuclearWaffleSweepAngle angle) =>
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(angle.FractionsOfStep);

    /// <summary>The 16.16 word the sweep speed was held in, for <paramref name="speed"/>.</summary>
    private static SignedSixteenSixteen Widened(AngularSpeed speed) =>
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(speed.FractionsOfStep);

    /// <summary>Runs the focused Nuclear Waffle sweep-angle check.</summary>
    [VerificationCommand("--nuclear-waffle-sweep-angle")]
    internal static int NuclearWaffleSweepAngleCommand()
    {
        VerifyNuclearWaffleSweepAngle();
        return 0;
    }
}
