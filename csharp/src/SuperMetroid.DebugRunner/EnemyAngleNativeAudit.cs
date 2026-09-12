using System.Reflection;
using SuperMetroid.Core.Game;

/// <summary>Original-CPU aiming matrix, including hardware-width and divide-by-zero edges.</summary>
internal static class EnemyAngleNativeAudit
{
    public static int Run(string csv)
    {
        MethodInfo angle = typeof(RoomEnemySystem).GetMethod("CalculateCartridgeAngle",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingMethodException("Production enemy aiming helper is missing.");
        int count = 0;
        foreach (string row in File.ReadAllLines(csv).Skip(1))
        {
            string[] fields = row.Split(',');
            short x = short.Parse(fields[0]), y = short.Parse(fields[1]);
            byte expected = byte.Parse(fields[2]);
            byte actual = (byte)angle.Invoke(null, [x, y])!;
            if (actual != expected)
                throw new InvalidDataException($"Enemy angle ({x},{y}): port={actual}, native={expected}.");
            count++;
        }
        if (count != 225) throw new InvalidDataException($"Expected 225 native angles, got {count}.");
        Console.WriteLine($"All {count} original-CPU enemy angles match.");
        return 0;
    }
}
