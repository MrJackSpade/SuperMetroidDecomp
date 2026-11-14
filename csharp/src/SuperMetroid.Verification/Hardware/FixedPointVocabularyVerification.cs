using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Hardware;

/// <summary>
/// Confirms #1326's fixed-point, angle and Speed Booster counter types against the primitive forms they
/// replaced, each reference written as the replaced code computed it.
/// </summary>
internal static class FixedPointVocabularyVerification
{
    /// <summary>Runs every #1326 confirmation.</summary>
    internal static void VerifyFixedPointVocabulary()
    {
        VerifyFixedProducts();
        VerifyBearingConversion();
        VerifySpeedBoostCounter();
        Console.WriteLine("Fixed-point vocabulary: sine products, bearings and the boost counter match the replaced forms.");
    }

    /// <summary>The 8-bit sine fixed products keep the replaced tuple's words, including the independent negation.</summary>
    private static void VerifyFixedProducts()
    {
        int negatedWithFraction = 0;
        foreach (byte angle in new byte[] { 0x00, 0x40, 0x80, 0xc0, 0xa3 })
        for (int radius = 0; radius <= 0x1ff; radius++)
        {
            int sample = Trigonometry.CoarseHalfWave(angle & 0x7f);
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
            SignedSixteenSixteen actual = EnemyMath.CircleSine(new TrigonometryAngle(angle), (ushort)radius);
            AssertEqual(unchecked((short)whole), actual.WholePixels, $"sine product whole, angle {angle:X2} radius {radius:X3}");
            AssertEqual(fraction, actual.Fraction, $"sine product fraction, angle {angle:X2} radius {radius:X3}");
        }
        AssertTrue(negatedWithFraction > 0, "the samples include a negated product with a nonzero fraction");
    }

    /// <summary><see cref="Bearing.ToTrigonometryAngle"/> is the replaced <c>0x40 - bearing</c> for every byte.</summary>
    private static void VerifyBearingConversion()
    {
        for (int bearing = 0; bearing <= byte.MaxValue; bearing++)
        {
            AssertEqual(unchecked((byte)(0x40 - bearing)), new Bearing((byte)bearing).ToTrigonometryAngle().RawValue,
                $"bearing {bearing:X2} in the table convention");
            AssertEqual((byte)bearing, new Bearing((byte)bearing).ToMirroredTrigonometryAngle().RawValue,
                $"bearing {bearing:X2} read unconverted");
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

        (SamusSpeedBoostCounter borrowed, bool borrowedExpired) = SamusSpeedBoostCounter.AtFullStage.CountDownOneRun();
        AssertEqual(new SamusSpeedBoostCounter(3, 255), borrowed, "a zero cadence borrows from stage four");
        AssertTrue(!borrowedExpired, "the borrowed cadence is not expired");
        AssertThrows<InvalidOperationException>(() => new SamusSpeedBoostCounter(0, 0).CountDownOneRun(),
            "stage zero with no cadence has no stage to borrow");

        for (int stage = 0; stage <= 3; stage++)
        {
            (SamusSpeedBoostCounter expired, bool cadenceExpired) = new SamusSpeedBoostCounter(stage, 1).CountDownOneRun();
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
