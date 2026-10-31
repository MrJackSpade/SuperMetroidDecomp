using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks enemy angle calculation against original CPU division and recorded vectors.</summary>
internal static class EnemyAngleDivisionVerification
{
    /// <summary>Compares zero-divisor behavior and 225 native enemy-angle samples.</summary>
    internal static void VerifyEnemyAngleDivision()
    {
        AssertEqual((int)ushort.MaxValue, AimAngleSlope.Of(0, 0), "a zero long side gives the steepest slope");
        AssertEqual((int)ushort.MaxValue, AimAngleSlope.Of(5, 512), "a long side of a multiple of 256 pixels comes to zero");
        AssertEqual(256 * 3 / 44, AimAngleSlope.Of(3, 300), "a long side past 255 pixels counts only its remainder");
        string[] rows = File.ReadAllLines("csharp/test-fixtures/movement-release/enemy-angle-402.csv").Skip(1).ToArray();
        AssertEqual(225, rows.Length, "Complete original-CPU enemy angle matrix");
        foreach (string row in rows)
        {
            string[] fields = row.Split(',');
            short x = short.Parse(fields[0]), y = short.Parse(fields[1]);
            byte actual = EnemyMath.CalculateBearing(new Offset16<RoomPixels>(x, y)).RawValue;
            AssertEqual(byte.Parse(fields[2]), actual, $"Native enemy angle ({x},{y})");
        }
    }

    /// <summary>Runs the focused enemy-angle division comparison.</summary>
    [VerificationCommand("--enemy-angle-division")]
    internal static int EnemyAngleDivisionCommand()
    {
        Suite(nameof(VerifyEnemyAngleDivision), () => VerifyEnemyAngleDivision());
        return 0;
    }
}
