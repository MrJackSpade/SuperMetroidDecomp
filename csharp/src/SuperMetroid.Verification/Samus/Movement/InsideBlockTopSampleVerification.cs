using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks which points Samus's inside-block reactions sample (#1621).</summary>
internal static class InsideBlockTopSampleVerification
{
    /// <summary>
    /// Confirms <see cref="InsideBlockTopSample"/> and the sample sequence that consults it: the top is skipped when it
    /// shares the bottom's row, and also when the top's and bottom's rows combined bit by bit give the centre's row (rows
    /// 0, 1, 1 and rows 1, 2, 3), but sampled on rows the combination does not name; then compares every centre and
    /// radius with the coordinate combinations the sequence replaced.
    /// </summary>
    internal static void VerifyInsideBlockTopSample()
    {
        AssertSequence(40, 21, [(60, SamusInsideBlockPoint.Bottom), (40, SamusInsideBlockPoint.Center)],
            "rows 1, 2 and 3: the top is skipped because rows 1 and 3 combine to the centre's row 2");
        AssertSequence(72, 21, [(92, SamusInsideBlockPoint.Bottom), (72, SamusInsideBlockPoint.Center), (51, SamusInsideBlockPoint.Top)],
            "rows 3, 4 and 5: rows 3 and 5 combine to row 6, so the top is sampled");
        AssertSequence(20, 10, [(29, SamusInsideBlockPoint.Bottom)],
            "rows 0, 1 and 1: the centre shares the bottom's row and the top is skipped although it lies on row 0");
        AssertSequence(36, 4, [(39, SamusInsideBlockPoint.Bottom)], "a body within one row samples only its bottom");
        AssertTrue(!InsideBlockTopSample.IsSampled(top: 18, bottom: 30, centre: 24), "a top sharing the bottom's row is not sampled");
        AssertTrue(InsideBlockTopSample.IsSampled(top: 70, bottom: 100, centre: 90), "rows 4 and 6 combine to row 2, not the centre's row 5");

        int compared = 0;
        for (int centre = 0; centre < 1024; centre++)
        for (int radius = 1; radius <= 24; radius++)
        {
            var visited = new List<(int, SamusInsideBlockPoint)>();
            SamusInsideBlockSamplePoints.Visit(centre, radius, (y, point) => visited.Add((y, point)));
            AssertTrue(Replaced(centre, radius).SequenceEqual(visited), $"centre {centre} radius {radius}");
            compared++;
        }

        Console.WriteLine($"Inside-block top sample: the row skips hold; {compared} bodies sample as the replaced sequence did.");
    }

    private static void AssertSequence(int centre, int radius, (int, SamusInsideBlockPoint)[] expected, string message)
    {
        var visited = new List<(int, SamusInsideBlockPoint)>();
        SamusInsideBlockSamplePoints.Visit(centre, radius, (y, point) => visited.Add((y, point)));
        AssertTrue(expected.SequenceEqual(visited), $"{message}: visited {string.Join(", ", visited)}");
    }

    /// <summary>The replaced sequence: the coordinates combined by exclusive or, the top's test chained onto the first result.</summary>
    private static List<(int, SamusInsideBlockPoint)> Replaced(int centre, int radius)
    {
        int bottom = (centre + radius - 1) & 0xffff;
        int top = (centre - radius) & 0xffff;
        var points = new List<(int, SamusInsideBlockPoint)> { (bottom, SamusInsideBlockPoint.Bottom) };
        if (((centre ^ bottom) & ~15) != 0)
            points.Add((centre, SamusInsideBlockPoint.Center));
        int topBottomRows = (top ^ bottom) & ~15;
        if (topBottomRows != 0 && ((topBottomRows ^ centre) & ~15) != 0)
            points.Add((top, SamusInsideBlockPoint.Top));
        return points;
    }
}
