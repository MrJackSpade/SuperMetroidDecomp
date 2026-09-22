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
        CheckBounds(SuitWidth, 127);
        Console.WriteLine("PASS: 128/128 suit contour bytes; explicit seven-row opening and row-31 correction; precision/raster alternatives checked.");
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
}
