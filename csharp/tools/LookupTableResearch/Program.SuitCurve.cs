using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifySuitCurve(byte[] rom)
    {
        int[] native = Oracle(rom, SuitPickupBeamCurveDefinitions.NativeCurveAddress, 128, 1);
        var ellipseExceptions = new List<int>();
        for (int i = 0; i < 128; i++)
        {
            Equal(native[i], (int)SuitPickupBeamCurveDefinitions.OffsetAt(i), $"compiled suit curve {i}");
            Equal(native[i], SuitWidth(i), $"exact suit curve {i}");
            if (native[i] != SuitEllipse(i)) ellipseExceptions.Add(i);
        }
        Equal("31", string.Join(',', ellipseExceptions), "unmodified ellipse counterexample after seven-row opening");
        Equal(15, SuitEllipse(31), "mathematical ellipse row 31");
        Equal(16, native[31], "authored row 31 correction");

        // Repeat the precision investigation: ordinary float/double evaluation and
        // truncation/rounding of the root at 0..8 decimal or binary fractional places.
        // None justifies treating the remaining authored row as numerical noise.
        int best = 128;
        for (int precision = 0; precision <= 8; precision++)
        for (int mode = 0; mode < 3; mode++)
        {
            int bad = 0;
            for (int i = 7; i < 128; i++)
            {
                double y = (128 - i) / 127.0;
                double raw = 24 * Math.Sqrt(1 - y * y);
                double scale = mode == 2 ? Math.Pow(2, precision) : Math.Pow(10, precision);
                double quantized = (mode == 1 ? Math.Floor(raw * scale) : Math.Round(raw * scale)) / scale;
                if ((int)Math.Round(quantized) != native[i]) bad++;
                float yf = (128 - i) / 127f;
                Equal(SuitEllipse(i), (int)MathF.Round(24 * MathF.Sqrt(1 - yf * yf)), $"float ellipse {i}");
                Equal(SuitEllipse(i), (int)Math.Round(raw), $"double ellipse {i}");
            }
            best = Math.Min(best, bad);
        }
        Equal(1, best, "best reduced-precision ellipse still has a counterexample");
        VerifyMidpointEllipse(native);
        VerifySuitQuantization(native);
        CheckBounds(SuitWidth, 127);
        Console.WriteLine("PASS: 128/128 suit contour bytes; seven-row opening plus either exact ellipse/row-31 correction OR angle/Q6 double rounding without that correction.");
    }

    private static int SuitWidth(int i)
    {
        Bound(i, 127);
        return i == 31 ? 16 : SuitEllipse(i);
    }

    private static int SuitEllipse(int i)
    {
        Bound(i, 127);
        if (i < 7) return i + 1;
        int y = 128 - i;
        int rhs = 4 * 24 * 24 * (127 * 127 - y * y);
        int width = 0;
        while (127 * 127 * (2 * width + 1) * (2 * width + 1) <= rhs) width++;
        return width; // nearest integer; equality is impossible (odd LHS, RHS divisible by four).
    }

    private static void VerifyMidpointEllipse(int[] native)
    {
        // Classic two-region integer midpoint ellipse, radii 24 and 127,
        // mirrored at scanline 128. Store the last X emitted for each Y.
        const int rx = 24, ry = 127;
        var raster = new int[129];
        int x = 0, y = ry;
        long a2 = rx * rx, b2 = ry * ry, dx = 0, dy = 2 * a2 * y;
        long d = 4 * b2 - 4 * a2 * ry + a2;
        while (dx < dy)
        {
            raster[y] = x;
            x++; dx += 2 * b2;
            if (d < 0) d += 4 * dx + 4 * b2;
            else { y--; dy -= 2 * a2; d += 4 * dx - 4 * dy + 4 * b2; }
        }
        d = b2 * (2 * x + 1) * (2 * x + 1) + 4 * a2 * (y - 1) * (y - 1) - 4 * a2 * b2;
        while (y >= 0)
        {
            raster[y] = x;
            y--; dy -= 2 * a2;
            if (d > 0) d += 4 * a2 - 4 * dy;
            else { x++; dx += 2 * b2; d += 4 * dx - 4 * dy + 4 * a2; }
        }
        var exceptions = new List<int>();
        for (int i = 0; i < 128; i++)
        {
            int value = Math.Max(1, Math.Min(i + 1, raster[128 - i]));
            if (value != native[i]) exceptions.Add(i);
        }
        Equal("31", string.Join(',', exceptions), "midpoint raster has the same single counterexample");
    }

    private static void VerifySuitQuantization(int[] native)
    {
        foreach (int cycle in new[] { 8192, 16384 })
        for (int i = 0; i < 128; i++)
            Equal(native[i], SuitQuantizedWidth(i, cycle, 6), $"suit angle/Q6 generator {cycle}/{i}");
        foreach (int cycle in new[] { 512, 1024, 2048, 4096 })
        {
            int mismatches = Enumerable.Range(7, 121).Count(i => native[i] != SuitQuantizedWidth(i, cycle, 6));
            Equal(true, mismatches > 0, $"suit rejected Q6 angular resolution {cycle}");
        }
        foreach (int bits in new[] { 4, 5, 7, 8, 12 })
        {
            int mismatches = Enumerable.Range(7, 121).Count(i => native[i] != SuitQuantizedWidth(i, 8192, bits));
            Equal(true, mismatches > 0, $"suit rejected intermediate fractional precision {bits}");
        }
        CheckBounds(i => SuitQuantizedWidth(i, 8192, 6), 127);
    }

    private static int SuitQuantizedWidth(int i, int cycle, int bits)
    {
        Bound(i, 127);
        if (i < 7) return i + 1;
        int y = 128 - i, lower = 0, upper = cycle / 4;
        decimal step = 2 * ResearchData.Pi / cycle;
        // Round acos(y/127)/step without a floating-point inverse trig dependency.
        // cos is decreasing; half-step boundaries decide the nearest angle index.
        while (lower < upper)
        {
            int middle = (lower + upper) / 2;
            decimal boundary = FullCosine((middle + .5m) * step);
            bool belowLow = y <= 127 * (boundary - ResearchData.Error);
            bool belowHigh = y <= 127 * (boundary + ResearchData.Error);
            Equal(belowLow, belowHigh, $"suit angular rounding interval {cycle}/{i}/{middle}");
            if (belowLow) lower = middle + 1;
            else upper = middle;
        }
        int scale = 1 << bits;
        int fixedWidth = StableFloor(24 * scale * FullSine(lower * step) + .5m,
            24 * scale * ResearchData.Error);
        return (fixedWidth + scale / 2) / scale;
    }
}
