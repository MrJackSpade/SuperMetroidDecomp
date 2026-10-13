using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks enemy angle calculation against original CPU division and recorded vectors.</summary>
internal static class EnemyAngleDivisionVerification
{
    /// <summary>Compares zero-divisor behavior and 225 native enemy-angle samples.</summary>
    internal static void VerifyEnemyAngleDivision()
    {
        AssertEqual(ushort.MaxValue, SnesUnsignedDivision.Quotient(0, 0), "Hardware zero-over-zero quotient");
        AssertEqual(ushort.MaxValue, SnesUnsignedDivision.Quotient(1234, 0), "Hardware divide-by-zero quotient");
        string[] rows = File.ReadAllLines("csharp/test-fixtures/movement-release/enemy-angle-402.csv").Skip(1).ToArray();
        AssertEqual(225, rows.Length, "Complete original-CPU enemy angle matrix");
        foreach (string row in rows)
        {
            string[] fields = row.Split(',');
            short x = short.Parse(fields[0]), y = short.Parse(fields[1]);
            byte actual = EnemyMath.CalculateCartridgeAngle(new Offset16<RoomPixels>(x, y));
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
