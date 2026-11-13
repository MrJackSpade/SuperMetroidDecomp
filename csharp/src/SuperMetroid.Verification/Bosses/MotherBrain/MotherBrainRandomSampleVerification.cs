using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Confirms that the named questions Mother Brain asks of its random sample answer exactly as the cartridge's tests on
/// the raw sixteen-bit number do: sign and the fields its rolls read. The chances the sample takes are confirmed by
/// <see cref="MotherBrainRandomChancesVerification"/>.
/// </summary>
internal static class MotherBrainRandomSampleVerification
{
    /// <summary>Compares every sixteen-bit generator value's named answers with the native word tests they replace.</summary>
    internal static void VerifyMotherBrainRandomSample()
    {
        for (int raw = ushort.MinValue; raw <= ushort.MaxValue; raw++)
        {
            ushort word = unchecked((ushort)raw);
            MotherBrainRandomSample sample = MotherBrainRandomSample.FromGenerator(word);
            AssertEqual(unchecked((short)word) < 0, sample.IsNegative, $"sign of {raw}");
            AssertEqual(word % 32, sample.Read(MotherBrainRandomDefinitions.CorpseDustOffset), $"corpse dust offset of {raw}");
            AssertEqual((word & 2) != 0, sample.IsSet(MotherBrainRandomDefinitions.ForwardInchLongerDelay), $"inching delay bit of {raw}");
        }
        AssertTrue(MotherBrainRandomSample.FromGenerator(ushort.MaxValue).IsNegative, "the top value is negative");
        AssertTrue(!MotherBrainRandomSample.FromGenerator((ushort)short.MaxValue).IsNegative, "the largest positive value is not");
        Console.WriteLine("Mother Brain random sample: all 65536 values answer sign and field reads as the native word.");
    }

    /// <summary>Runs the focused Mother Brain random-sample check from its command-line switch.</summary>
    [VerificationCommand("--verify-mother-brain-random-sample")]
    internal static int VerifyMotherBrainRandomSampleCommand()
    {
        Suite(nameof(VerifyMotherBrainRandomSample), () => VerifyMotherBrainRandomSample());
        return 0;
    }
}
