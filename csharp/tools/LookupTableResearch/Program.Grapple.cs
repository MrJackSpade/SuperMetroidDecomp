using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyGrapple(byte[] rom)
    {
        int[] velocities = Oracle(rom, GrappleResearchData.X, 20, 2, "GrappleBeamFireVelocityTable");
        int[] x = velocities[..10];
        int[] y = velocities[10..];
        int[] angles = Oracle(rom, GrappleResearchData.Angles, 10, 2, "GrappleBeamFireAngles");
        for (int direction = 0; direction < 10; direction++)
        {
            var compiled = GrappleFiringDefinitions.Launch((byte)direction);
            Equal((int)unchecked((short)x[direction]), GrappleX(direction), $"grapple X {direction}");
            Equal((int)unchecked((short)y[direction]), GrappleY(direction), $"grapple Y {direction}");
            Equal(angles[direction], GrappleAngle(direction), $"grapple angle {direction}");
            Equal(unchecked((short)x[direction]), compiled.XVelocity, $"compiled grapple X {direction}");
            Equal(unchecked((short)y[direction]), compiled.YVelocity, $"compiled grapple Y {direction}");
            Equal((ushort)angles[direction], compiled.Angle, $"compiled grapple angle {direction}");
        }
        CheckBounds(GrappleX, 9);
        CheckBounds(GrappleY, 9);
        CheckBounds(GrappleAngle, 9);
        // Direct full-precision scaling would give 3072, not the native 3060.
        Equal(3060, GrappleX(2), "byte saturation precedes grapple scaling");
        Equal(2172, GrappleX(1), "byte truncation precedes grapple scaling");
        Console.WriteLine("PASS: 30/30 grapple launch words; quantize sine to a saturated byte BEFORE multiplying by 12.");
    }

    private static int GrappleOctant(int direction) { Bound(direction, 9); return direction <= 4 ? direction : direction - 1; }
    private static int SignedByteSine(int angle)
    {
        int phase = angle & 255;
        return (phase < 128 ? 1 : -1) * ByteSine(phase & 127);
    }
    private static int GrappleX(int direction) => 12 * SignedByteSine(32 * GrappleOctant(direction));
    private static int GrappleY(int direction) => 12 * SignedByteSine(32 * GrappleOctant(direction) - 64);
    private static int GrappleAngle(int direction) => (0x8000 + 0x2000 * GrappleOctant(direction)) & 65535;
}

internal static class GrappleResearchData
{
    /// <summary>$9B:C0DB, GrappleBeamFireVelocityTable.X, ten signed words.</summary>
    internal const int X = 0x9bc0db;
    /// <summary>$9B:C0EF, GrappleBeamFireVelocityTable.Y, ten signed words.</summary>
    internal const int Y = 0x9bc0ef;
    /// <summary>$9B:C104, GrappleBeamFireAngles, ten angle words.</summary>
    internal const int Angles = 0x9bc104;
}
