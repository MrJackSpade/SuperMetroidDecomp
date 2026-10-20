using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Confirms the Ceres getaway's typed Mode 7 coefficients keep the native word arithmetic: the C coefficient negates B
/// with the sixteen-bit two's-complement wrap, and the published snapshot carries each coefficient's signed value.
/// </summary>
internal static class CeresRidleyMode7CoefficientVerification
{
    /// <summary>Compares the typed negation with the native word negation for every coefficient, including the wrapping minimum.</summary>
    internal static void VerifyCeresRidleyMode7Coefficients()
    {
        for (int raw = ushort.MinValue; raw <= ushort.MaxValue; raw++)
        {
            ushort word = unchecked((ushort)raw);
            ushort nativeNegation = unchecked((ushort)-(short)word);
            AssertEqual(nativeNegation, CeresRidleyGetaway.NegateMode7Coefficient(new SnesSignedEightEight(word)).RawValue,
                $"Mode 7 coefficient {raw} negates as the native word does");
        }
        AssertEqual(short.MinValue,
            CeresRidleyGetaway.NegateMode7Coefficient(SnesSignedEightEight.FromSignedRaw(short.MinValue)).SignedRawValue,
            "the most negative coefficient negates to itself");

        var getaway = new RidleyEnemyState
        {
            Mode7Active = true,
            Mode7MatrixA = SnesSignedEightEight.FromSignedRaw(240),
            Mode7MatrixB = SnesSignedEightEight.FromSignedRaw(33),
            Mode7MatrixC = SnesSignedEightEight.FromSignedRaw(-33),
            Mode7MatrixD = SnesSignedEightEight.FromSignedRaw(short.MinValue),
        };
        CeresRidleyMode7Snapshot snapshot = CeresRidleyMode7Snapshot.Capture(getaway)
            ?? throw new InvalidOperationException("An active getaway publishes a Mode 7 snapshot.");
        AssertEqual((short)240, snapshot.MatrixA, "snapshot A coefficient");
        AssertEqual((short)33, snapshot.MatrixB, "snapshot B coefficient");
        AssertEqual((short)-33, snapshot.MatrixC, "snapshot C coefficient");
        AssertEqual(short.MinValue, snapshot.MatrixD, "snapshot D coefficient keeps the wrapped minimum");
        Console.WriteLine("Ceres Ridley Mode 7 coefficients: all 65536 negations and the snapshot's signed values match the native words.");
    }

    /// <summary>Runs the focused Ceres getaway coefficient check from its command-line switch.</summary>
    [VerificationCommand("--verify-ceres-ridley-mode7-coefficients")]
    internal static int VerifyCeresRidleyMode7CoefficientsCommand()
    {
        Suite(nameof(VerifyCeresRidleyMode7Coefficients), () => VerifyCeresRidleyMode7Coefficients());
        return 0;
    }
}
