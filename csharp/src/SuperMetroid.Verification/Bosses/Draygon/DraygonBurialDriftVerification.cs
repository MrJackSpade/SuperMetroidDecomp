using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>
/// Checks the dying Draygon's burial drift bearing (#1632): aimed across its coarse grid, it is the bearing the game took
/// after quartering both coordinates of his position and of the burial point.
/// </summary>
internal static class DraygonBurialDriftVerification
{
    /// <summary>Compares the drift bearing with the quartered-coordinate bearing for every coordinate on each axis.</summary>
    internal static void VerifyDraygonBurialDrift()
    {
        static TrigonometryAngle Quartered(int x, int y) => EnemyMath.CalculateBearing(new(
            CoordinateAxis.Difference(64, x >> 2),
            CoordinateAxis.Difference(120, y >> 2))).ToTrigonometryAngle();

        int mismatches = 0;
        foreach (int fixedCoordinate in new[] { 0, 255, 256, 257, 479, 480, 481, ushort.MaxValue })
        {
            for (int coordinate = 0; coordinate <= ushort.MaxValue; coordinate++)
            {
                mismatches += DraygonDeath.BurialDriftAngle(new(coordinate, fixedCoordinate)) == Quartered(coordinate, fixedCoordinate) ? 0 : 1;
                mismatches += DraygonDeath.BurialDriftAngle(new(fixedCoordinate, coordinate)) == Quartered(fixedCoordinate, coordinate) ? 0 : 1;
            }
        }
        AssertEqual(0, mismatches, "the burial drift's bearing is the quartered coordinates' bearing on both axes");
        Console.WriteLine("Draygon burial drift: every coordinate's bearing on both axes matches the quartered-coordinate bearing.");
    }
}
