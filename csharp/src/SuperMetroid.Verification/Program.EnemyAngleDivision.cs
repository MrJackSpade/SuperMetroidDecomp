using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEnemyAngleDivision()
    {
        AssertEqual(ushort.MaxValue, SnesUnsignedDivision.Quotient(0, 0), "Hardware zero-over-zero quotient");
        AssertEqual(ushort.MaxValue, SnesUnsignedDivision.Quotient(1234, 0), "Hardware divide-by-zero quotient");
        MethodInfo angle = typeof(RoomEnemySystem).GetMethod("CalculateCartridgeAngle",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        string[] rows = File.ReadAllLines("csharp/test-fixtures/movement-release/enemy-angle-402.csv").Skip(1).ToArray();
        AssertEqual(225, rows.Length, "Complete original-CPU enemy angle matrix");
        foreach (string row in rows)
        {
            string[] fields = row.Split(',');
            short x = short.Parse(fields[0]), y = short.Parse(fields[1]);
            byte actual = (byte)angle.Invoke(null, [x, y])!;
            AssertEqual(byte.Parse(fields[2]), actual, $"Native enemy angle ({x},{y})");
        }
    }
}
