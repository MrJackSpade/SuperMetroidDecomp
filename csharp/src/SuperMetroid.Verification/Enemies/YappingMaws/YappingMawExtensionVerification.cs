using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Enemies.YappingMaws;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Checks the Yapping Maw's signed 16.16 curl through the production extension update: the fraction's carry into the
/// whole part, the +128 turn-around clamp and its one-unit miss, and the sign test whose borrow below zero ends the
/// retraction while an exact return to zero does not.
/// </summary>
internal static class YappingMawExtensionVerification
{
    /// <summary>Runs the ROM-free Yapping Maw curl check.</summary>
    [VerificationCommand("--yapping-maw-extension", Description = "Yapping Maw curl carry, clamp and retraction sign test through production.")]
    internal static int YappingMawExtensionCommand()
    {
        Suite(nameof(VerifyYappingMawExtension), VerifyYappingMawExtension);
        return 0;
    }

    /// <summary>
    /// Drives one extension update per case from a chosen curl and quadratic speed row. The row is the first
    /// whose extending speed is positive with a nonzero fraction and whose retracting speed is negative, so every case
    /// below has the carry or borrow it names. Expected curls are built from their whole and fractional parts.
    /// </summary>
    internal static void VerifyYappingMawExtension()
    {
        int row = Enumerable.Range(0, EnemyQuadraticSpeedDefinitions.RowCount).First(candidate =>
            EnemyQuadraticSpeeds.Row(checked((ushort)candidate)).Positive.RawValue is int extending &&
            extending > 0 && SixteenSixteenWords.ToDisplacement(extending).Fraction != 0 &&
            EnemyQuadraticSpeeds.Row(checked((ushort)candidate)).Negative.RawValue < 0);
        int extend = EnemyQuadraticSpeeds.Row(checked((ushort)row)).Positive.RawValue;
        int retract = EnemyQuadraticSpeeds.Row(checked((ushort)row)).Negative.RawValue;
        int extendWhole = SixteenSixteenWords.ToDisplacement(extend).WholePixels;
        ushort extendFraction = SixteenSixteenWords.ToDisplacement(extend).Fraction;
        var maximum = SignedSixteenSixteen.FromParts(128, 0);
        AssertTrue(extendWhole + 1 < maximum.WholePixels && -retract < maximum.RawValue,
            "fixture speeds stay inside the curl's extending range");

        // The starting fraction complements the speed's, so the fraction sum is exactly one whole unit: the result's
        // fraction is zero and its whole part is the speed's whole part plus the carried one.
        YappingMawEnemyState carried = Advance(
            SignedSixteenSixteen.FromParts(0, checked((ushort)(ushort.MaxValue + 1 - extendFraction))), row, EnemySpeedSign.Positive);
        AssertEqual(SignedSixteenSixteen.FromParts(checked((short)(extendWhole + 1)), 0), carried.Extension,
            "the fraction's carry reaches the whole part");
        AssertEqual(YappingMawAiFunction.ExtendingOrRetracting, carried.Function, "a positive curl keeps extending");
        AssertEqual(row + 1, carried.QuadraticSpeedRow, "below the clamp the row advances once");
        AssertEqual(EnemySpeedSign.Positive, carried.QuadraticSpeedSign, "below the clamp the speed stays extending");

        YappingMawEnemyState clamped = Advance(maximum - SixteenSixteenWords.ToDisplacement(extend), row, EnemySpeedSign.Positive);
        AssertEqual(maximum, clamped.Extension, "reaching +128 clamps the curl there with a cleared fraction");
        AssertEqual(row + 1, clamped.QuadraticSpeedRow, "the clamp keeps the update's row advance");
        AssertEqual(EnemySpeedSign.Negative, clamped.QuadraticSpeedSign, "the clamp turns the speed around to the row's retracting speed");

        YappingMawEnemyState belowClamp = Advance(
            maximum - SixteenSixteenWords.ToDisplacement(extend) - SignedSixteenSixteen.FromParts(0, 1), row, EnemySpeedSign.Positive);
        AssertEqual(SignedSixteenSixteen.FromParts(127, ushort.MaxValue), belowClamp.Extension,
            "one fraction unit short of +128 is not clamped");
        AssertEqual(row + 1, belowClamp.QuadraticSpeedRow, "one fraction unit short of +128 advances the row once");
        AssertEqual(EnemySpeedSign.Positive, belowClamp.QuadraticSpeedSign, "one fraction unit short of +128 keeps extending");

        YappingMawEnemyState atZero = Advance(SixteenSixteenWords.ToDisplacement(-retract), row, EnemySpeedSign.Negative);
        AssertEqual(SignedSixteenSixteen.Zero, atZero.Extension, "retraction can return the curl exactly to zero");
        AssertEqual(YappingMawAiFunction.ExtendingOrRetracting, atZero.Function, "a zero curl is not negative and keeps retracting");

        YappingMawEnemyState pastZero = Advance(SixteenSixteenWords.ToDisplacement(-retract - 1), row, EnemySpeedSign.Negative);
        AssertEqual(SignedSixteenSixteen.FromParts(-1, ushort.MaxValue), pastZero.Extension,
            "one fraction unit past zero borrows the whole part to -1");
        AssertEqual(YappingMawAiFunction.RetractedDelay, pastZero.Function, "a negative whole part ends the retraction");
        Console.WriteLine($"Yapping Maw extension: row {row} carry, +128 clamp and miss, and zero/negative retraction sign test pass through production.");
    }

    /// <summary>
    /// Brings a production-initialized Maw through its targeting and setup updates, then runs one extension update
    /// from the given curl and speed row.
    /// </summary>
    private static YappingMawEnemyState Advance(SignedSixteenSixteen start, int row, EnemySpeedSign sign)
    {
        (RoomEnemySystem enemies, RoomEnemySlot slot, YappingMawEnemyState state) =
            YappingMawInstructionProgramDefinitionsVerification.NewInitializedYappingMaw(301);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        samus.Position = samus.Position.WithWhole(new(128 + 300, 128));
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
            new FrameCounters(0));
        enemies.YappingMawBehavior.RunYappingMawMainAi(slot, frame);
        enemies.YappingMawBehavior.RunYappingMawMainAi(slot, frame);
        AssertEqual(YappingMawAiFunction.ExtendingOrRetracting, state.Function, "setup begins the extension");
        AssertEqual(SignedSixteenSixteen.Zero, state.Extension, "setup starts the curl at zero");

        state.Extension = start;
        state.QuadraticSpeedRow = row;
        state.QuadraticSpeedSign = sign;
        enemies.YappingMawBehavior.RunYappingMawMainAi(slot, frame);
        return state;
    }
}
