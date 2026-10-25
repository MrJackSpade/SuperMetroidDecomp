using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Confirms the Ceres getaway's typed Mode 7 coefficients keep the native word arithmetic: the C coefficient negates B
/// with the sixteen-bit two's-complement wrap, and the published floor transform carries each coefficient's signed value.
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
            AssertEqual(nativeNegation, CeresRidleyGetaway.NegateFloorCoefficient(new SignedEightEight(word)).RawValue,
                $"Mode 7 coefficient {raw} negates as the native word does");
        }
        AssertEqual(short.MinValue,
            CeresRidleyGetaway.NegateFloorCoefficient(SignedEightEight.FromSignedRaw(short.MinValue)).SignedRawValue,
            "the most negative coefficient negates to itself");

        var getaway = new RidleyEnemyState
        {
            GetawayActive = true,
            FloorMatrixA = SignedEightEight.FromSignedRaw(240),
            FloorMatrixB = SignedEightEight.FromSignedRaw(33),
            FloorMatrixC = SignedEightEight.FromSignedRaw(-33),
            FloorMatrixD = SignedEightEight.FromSignedRaw(short.MinValue),
        };
        var floor = CeresRidleyGetaway.FloorTransform(getaway)
            ?? throw new InvalidOperationException("An active getaway publishes the floor's transform.");
        AssertEqual((short)240, floor.MatrixA, "floor transform A coefficient");
        AssertEqual((short)33, floor.MatrixB, "floor transform B coefficient");
        AssertEqual((short)-33, floor.MatrixC, "floor transform C coefficient");
        AssertEqual(short.MinValue, floor.MatrixD, "floor transform D coefficient keeps the wrapped minimum");
        Console.WriteLine("Ceres Ridley floor coefficients: all 65536 negations and the floor transform's signed values match the native words.");
    }

    /// <summary>Runs the focused Ceres getaway coefficient check from its command-line switch.</summary>
    [VerificationCommand("--verify-ceres-ridley-mode7-coefficients")]
    internal static int VerifyCeresRidleyMode7CoefficientsCommand()
    {
        Suite(nameof(VerifyCeresRidleyMode7Coefficients), () => VerifyCeresRidleyMode7Coefficients());
        return 0;
    }
}
