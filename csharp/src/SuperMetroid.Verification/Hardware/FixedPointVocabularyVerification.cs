using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
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
            var speed = new SnesUnsignedSixteenSixteen(whole, fraction);
            var capSpeed = new SnesUnsignedSixteenSixteen(cap, 0);
            bool replaced = unchecked((short)(whole - cap)) >= 0 && unchecked((short)fraction) >= 0;
            AssertEqual(replaced, speed.WordsAtLeast(capSpeed), $"cap {cap} against {whole:X4}.{fraction:X4}");
            if (replaced != (speed.RawValue >= capSpeed.RawValue))
                disagreements++;
        }
        AssertTrue(disagreements > 0, "some samples separate the word test from an unsigned 32-bit comparison");
        AssertTrue(!new SnesUnsignedSixteenSixteen(3, 0x8000).WordsAtLeast(new SnesUnsignedSixteenSixteen(2, 0)),
            "a fraction word of $8000 fails the cap although 3.8000 exceeds 2.0000");
    }

    /// <summary>The 8-bit sine fixed products keep the replaced tuple's words, including the independent negation.</summary>
    private static void VerifyFixedProducts()
    {
        int negatedWithFraction = 0;
        foreach (byte angle in new byte[] { 0x00, 0x40, 0x80, 0xc0, 0xa3 })
        for (int radius = 0; radius <= 0x1ff; radius++)
        {
            int sample = EnemyTrigonometryTables.EightBitHalfWave(angle & 0x7f);
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
            SnesSignedSixteenSixteen actual = EnemyMath.ReadEightBitSineFixedProduct(new SnesByteAngle(angle), (ushort)radius);
            AssertEqual(unchecked((short)whole), actual.WholePixels, $"sine product whole, angle {angle:X2} radius {radius:X3}");
            AssertEqual(fraction, actual.Fraction, $"sine product fraction, angle {angle:X2} radius {radius:X3}");
        }
        AssertTrue(negatedWithFraction > 0, "the samples include a negated product with a nonzero fraction");
    }

    /// <summary><see cref="SnesBearing.ToTableAngle"/> is the replaced <c>0x40 - bearing</c> for every byte.</summary>
    private static void VerifyBearingConversion()
    {
        for (int bearing = 0; bearing <= byte.MaxValue; bearing++)
        {
            AssertEqual(unchecked((byte)(0x40 - bearing)), new SnesBearing((byte)bearing).ToTableAngle().RawValue,
                $"bearing {bearing:X2} in the table convention");
            AssertEqual((byte)bearing, new SnesBearing((byte)bearing).ReinterpretAsTableAngle().RawValue,
                $"bearing {bearing:X2} read unconverted");
        }
    }

    /// <summary>SnesCarry's ADC and SBC match the private helpers they replaced over boundary words.</summary>
    private static void VerifyCarryArithmetic()
    {
        foreach (ushort value in BoundaryWords)
        foreach (ushort operand in BoundaryWords)
        foreach (bool carryIn in new[] { false, true })
        {
            int sum = value + operand + (carryIn ? 1 : 0);
            bool carry = carryIn;
            AssertEqual(unchecked((ushort)sum), SnesCarry.AddWithCarry(value, operand, ref carry), $"ADC {value:X4}+{operand:X4} c{carryIn}");
            AssertEqual(sum > ushort.MaxValue, carry, $"ADC carry {value:X4}+{operand:X4} c{carryIn}");

            int difference = value - operand - (carryIn ? 0 : 1);
            carry = carryIn;
            AssertEqual(unchecked((ushort)difference), SnesCarry.SubtractWithBorrow(value, operand, ref carry), $"SBC {value:X4}-{operand:X4} c{carryIn}");
            AssertEqual(difference >= 0, carry, $"SBC carry {value:X4}-{operand:X4} c{carryIn}");
        }
    }

    /// <summary>The counter's predicates, decrement and stage advance match the replaced word expressions.</summary>
    private static void VerifySpeedBoostCounter()
    {
        foreach (int word in new[] { 0x03ff, 0x0400, 0x0402, 0x0f00 })
        {
            var counter = new SamusSpeedBoostCounter(word);
            AssertEqual((word & 0x0f00) == 0x0400, counter.IsActivelySpeedBoosting, $"active boost {word:X4}");
            AssertEqual((word & 1 << 10) != 0, counter.HasActiveStageBit, $"stage-four bit {word:X4}");
            AssertEqual(word / 0x100 * 0x100 == 0x0400, counter.HighByteIsStageFour, $"full boost {word:X4}");
            AssertEqual(unchecked((short)((word & 0xff00) - 0x0400)) >= 0, counter.MeetsShinesparkStoreThreshold, $"shinespark store {word:X4}");
        }

        (SamusSpeedBoostCounter borrowed, bool borrowedExpired) = new SamusSpeedBoostCounter(0x0400).DecrementForAnimationCommand();
        AssertEqual(0x03ff, borrowed.RawValue, "a zero cadence borrows from stage four");
        AssertEqual(3, borrowed.Stage, "the borrow leaves stage three");
        AssertTrue(!borrowedExpired, "$03FF's cadence is not expired");

        for (int stage = 0; stage <= 3; stage++)
        {
            (SamusSpeedBoostCounter expired, bool cadenceExpired) = new SamusSpeedBoostCounter(stage * 0x100 + 1).DecrementForAnimationCommand();
            AssertEqual(stage * 0x100, expired.RawValue, $"stage {stage} cadence reaches zero");
            AssertTrue(cadenceExpired, $"stage {stage} cadence expires");
            AssertTrue((expired.RawValue & 0x0400) == 0 == !expired.HasActiveStageBit, $"stage {stage} advance guard");
            AssertEqual(stage * 0x100 + 0x100, expired.AdvanceStage().RawValue, $"stage {stage} advances");
            AssertEqual((stage + 1) * 0x100 | 0x0012, expired.AdvanceStage().WithCadenceWord(0x0012).RawValue, $"stage {stage + 1} cadence reset");
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
