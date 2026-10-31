using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Hardware;

/// <summary>
/// Confirms #1326's fixed-point, angle, carry and Speed Booster counter types against the primitive forms they
/// replaced, each reference written as the replaced code computed it.
/// </summary>
internal static class FixedPointVocabularyVerification
{
    private static readonly ushort[] BoundaryWords = [0x0000, 0x7fff, 0x8000, 0xffff];

    /// <summary>Runs every #1326 confirmation.</summary>
    internal static void VerifyFixedPointVocabulary()
    {
        VerifyWordsAtLeast();
        VerifyFixedProducts();
        VerifyBearingConversion();
        VerifyCarryArithmetic();
        VerifySpeedBoostCounter();
        Console.WriteLine("Fixed-point vocabulary: caps, sine products, bearings, carries and the boost counter match the replaced forms.");
    }

    /// <summary>The extra-run caps' separate-word test, including fractions where an unsigned 32-bit test disagrees.</summary>
    private static void VerifyWordsAtLeast()
    {
        int disagreements = 0;
        foreach (int cap in new[] { 2, 7 })
        foreach (int whole in new[] { 0, 1, 2, 3, 7, 8, 0x7fff, 0x8000, 0xffff })
        foreach (int fraction in new[] { 0, 1, 0x7fff, 0x8000, 0xffff })
        {
            var speed = new UnsignedSixteenSixteen(whole, fraction);
            var capSpeed = new UnsignedSixteenSixteen(cap, 0);
            bool replaced = unchecked((short)(whole - cap)) >= 0 && unchecked((short)fraction) >= 0;
            AssertEqual(replaced, speed.WordsAtLeast(capSpeed), $"cap {cap} against {whole:X4}.{fraction:X4}");
            if (replaced != (speed.RawValue >= capSpeed.RawValue))
                disagreements++;
        }
        AssertTrue(disagreements > 0, "some samples separate the word test from an unsigned 32-bit comparison");
        AssertTrue(!new UnsignedSixteenSixteen(3, 0x8000).WordsAtLeast(new UnsignedSixteenSixteen(2, 0)),
            "a fraction word of $8000 fails the cap although 3.8000 exceeds 2.0000");
    }

    /// <summary>The 8-bit sine fixed products keep the replaced tuple's words, including the independent negation.</summary>
    private static void VerifyFixedProducts()
    {
        int negatedWithFraction = 0;
        foreach (byte angle in new byte[] { 0x00, 0x40, 0x80, 0xc0, 0xa3 })
        for (int radius = 0; radius <= 0x1ff; radius++)
        {
            int sample = Trigonometry.EightBitHalfWave(angle & 0x7f);
            ushort product = unchecked((ushort)(sample * (radius & 0xff)));
            ushort whole = (ushort)(product >> 8);
            ushort fraction = (ushort)((product & 0xff) << 8);
            if (angle >= 0x80)
            {
                whole = unchecked((ushort)(~whole + 1));
                fraction = unchecked((ushort)(~fraction + 1));
                if (fraction != 0)
                    negatedWithFraction++;
            }
            SignedSixteenSixteen actual = EnemyMath.ReadEightBitSineFixedProduct(new ByteAngle(angle), (ushort)radius);
            AssertEqual(unchecked((short)whole), actual.WholePixels, $"sine product whole, angle {angle:X2} radius {radius:X3}");
            AssertEqual(fraction, actual.Fraction, $"sine product fraction, angle {angle:X2} radius {radius:X3}");
        }
        AssertTrue(negatedWithFraction > 0, "the samples include a negated product with a nonzero fraction");
    }

    /// <summary><see cref="Bearing.ToTableAngle"/> is the replaced <c>0x40 - bearing</c> for every byte.</summary>
    private static void VerifyBearingConversion()
    {
        for (int bearing = 0; bearing <= byte.MaxValue; bearing++)
        {
            AssertEqual(unchecked((byte)(0x40 - bearing)), new Bearing((byte)bearing).ToTableAngle().RawValue,
                $"bearing {bearing:X2} in the table convention");
            AssertEqual((byte)bearing, new Bearing((byte)bearing).ReinterpretAsTableAngle().RawValue,
                $"bearing {bearing:X2} read unconverted");
        }
    }

    /// <summary>
    /// A seeking boss's velocity run (#1479) over boundary words: a run toward a target whose signed distance wraps
    /// starts with an extra unit, each raise or lower lands on the word the chained arithmetic it replaced computed,
    /// and a step that crosses zero adds one unit to the next step, which a zero step then shows.
    /// </summary>
    private static void VerifyCarryArithmetic()
    {
        foreach (ushort value in BoundaryWords)
        foreach (ushort operand in BoundaryWords)
        foreach (bool wrappedDistance in new[] { false, true })
        {
            int extra = wrappedDistance ? 1 : 0;
            // From 0, a target at 1 lies ahead in both orders; one at $9000 lies behind by the signed distance.
            int target = wrappedDistance ? 0x9000 : 1;

            var raising = new SeekingVelocityRun(new SignedEightEight(value), 0, target);
            int sum = value + operand + extra;
            raising.Raise(operand);
            AssertEqual(unchecked((ushort)sum), raising.Velocity.RawValue, $"raise {value:X4}+{operand:X4} extra {extra}");
            raising.Raise(0);
            AssertEqual(unchecked((ushort)(sum + (sum > ushort.MaxValue ? 1 : 0))), raising.Velocity.RawValue,
                $"a raise {value:X4}+{operand:X4} extra {extra} crossing zero lengthens the next step");

            var lowering = new SeekingVelocityRun(new SignedEightEight(value), 0, target);
            int difference = value - operand - extra;
            lowering.Lower(operand);
            AssertEqual(unchecked((ushort)difference), lowering.Velocity.RawValue, $"lower {value:X4}-{operand:X4} extra {extra}");
            lowering.Lower(0);
            AssertEqual(unchecked((ushort)(difference - (difference < 0 ? 1 : 0))), lowering.Velocity.RawValue,
                $"a lower {value:X4}-{operand:X4} extra {extra} crossing zero lengthens the next step");
        }
    }

    /// <summary>
    /// The counter's full-stage test, countdown, borrow and stage advance (#1470): every stage the boost reaches is
    /// full only at four, a zero cadence borrows a stage, and stage zero cannot borrow.
    /// </summary>
    private static void VerifySpeedBoostCounter()
    {
        for (int stage = 0; stage <= SamusSpeedBoostCounter.FullStage; stage++)
            AssertEqual(stage == 4, new SamusSpeedBoostCounter(stage, 2).IsAtFullStage, $"stage {stage} full boost");
        AssertEqual(new SamusSpeedBoostCounter(4, 0), SamusSpeedBoostCounter.AtFullStage, "a windup's counter is full with no cadence");

        (SamusSpeedBoostCounter borrowed, bool borrowedExpired) = SamusSpeedBoostCounter.AtFullStage.DecrementForAnimationCommand();
        AssertEqual(new SamusSpeedBoostCounter(3, 255), borrowed, "a zero cadence borrows from stage four");
        AssertTrue(!borrowedExpired, "the borrowed cadence is not expired");
        AssertThrows<InvalidOperationException>(() => new SamusSpeedBoostCounter(0, 0).DecrementForAnimationCommand(),
            "stage zero with no cadence has no stage to borrow");

        for (int stage = 0; stage <= 3; stage++)
        {
            (SamusSpeedBoostCounter expired, bool cadenceExpired) = new SamusSpeedBoostCounter(stage, 1).DecrementForAnimationCommand();
            AssertEqual(new SamusSpeedBoostCounter(stage, 0), expired, $"stage {stage} cadence reaches zero");
            AssertTrue(cadenceExpired, $"stage {stage} cadence expires");
            AssertTrue(!expired.IsAtFullStage, $"stage {stage} advance guard");
            AssertEqual(new SamusSpeedBoostCounter(stage + 1, 0), expired.AdvanceStage(), $"stage {stage} advances");
            AssertEqual(new SamusSpeedBoostCounter(stage + 1, 18), expired.AdvanceStage().WithCadence(18), $"stage {stage + 1} cadence reset");
        }
    }

    /// <summary>Runs the #1326 confirmations.</summary>
    [VerificationCommand("--fixed-point-vocabulary")]
    internal static int FixedPointVocabularyCommand()
    {
        VerifyFixedPointVocabulary();
        return 0;
    }
}
