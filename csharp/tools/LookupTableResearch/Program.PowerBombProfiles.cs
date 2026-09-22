using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyPowerBombProfiles(byte[] rom)
    {
        int[] white = Oracle(rom, PowerBombProfileResearchData.WhiteAddress, 17 * 192, 1);
        int[] yellow = Oracle(rom, PowerBombProfileResearchData.YellowAddress, 4 * 192, 1);
        Equal(PowerBombProfileResearchData.WhiteAddress & 65535, (int)SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape, "white source pointer");
        Equal(PowerBombProfileResearchData.YellowAddress & 65535, (int)SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape, "yellow source pointer");
        Equal(192, (int)SamusSpecialSequenceRomData.PowerBomb.ShapeStride, "pre-scaled shape stride");
        int[] basis = PowerBombBasis(1024, ResearchData.Pi);
        int[] reducedPi = PowerBombBasis(1024, ResearchData.GeneratorPi);
        for (int row = 0; row < 192; row++) Equal(basis[row], reducedPi[row], $"Power Bomb reduced-pi equivalence {row}");
        for (int frame = 0; frame < 21; frame++)
        {
            int ticks = frame < 17 ? frame + 37 : frame - 17 + 3;
            int radius = SamusSpecialSequenceRomData.PowerBomb.InitialRadius;
            int velocity = frame < 17 ? 0 : SamusSpecialSequenceRomData.PowerBomb.InitialPreExplosionSpeed;
            int acceleration = frame < 17 ? SamusSpecialSequenceRomData.PowerBomb.ExplosionAcceleration
                : -SamusSpecialSequenceRomData.PowerBomb.PreExplosionAcceleration;
            for (int tick = 0; tick < ticks; tick++) { radius += velocity; velocity += acceleration; }
            Equal(Math.Min(255, radius >> 8), PowerBombProfileRadius(frame), $"quadratic matches native-constant integration {frame}");
        }
        int wrongBoundary = 0, smoothEllipse = 0;
        for (int frame = 0; frame < 21; frame++)
        for (int row = 0; row < 192; row++)
        {
            int expected = frame < 17 ? white[192 * frame + row] : yellow[192 * (frame - 17) + row];
            int radius = PowerBombProfileRadius(frame);
            Equal(expected, PowerBombProfile(frame, row, basis), $"pre-scaled Power Bomb {frame}/{row}");
            int wrongIndex = 256 * row / radius;
            int wrongValue = wrongIndex >= 192 ? 0 : basis[wrongIndex] * radius / 256;
            if (expected != wrongValue) wrongBoundary++;
            int correctIndex = Math.Max(0, (256 * row - 1) / radius);
            int smooth = 0;
            if (correctIndex < 192)
            {
                int y = correctIndex + 1;
                long squared = 256L * 256 * (192 * 192 - y * y);
                int x = 0;
                while ((long)(x + 1) * (x + 1) * 192 * 192 <= squared && x < 255) x++;
                smooth = x * radius / 256;
            }
            if (expected != smooth) smoothEllipse++;
        }
        Equal(59, wrongBoundary, "ordinary inverse scaling incorrectly advances exact boundaries");
        Equal(955, smoothEllipse, "continuous ellipse is not the quantized-angle basis");
        foreach (int cycle in new[] { 256, 512, 2048, 4096 })
        {
            int[] alternative = PowerBombBasis(cycle, ResearchData.Pi);
            int mismatches = Enumerable.Range(0, 192).Count(i => alternative[i] != basis[i]);
            Equal(true, mismatches > 0, $"reject alternate angular resolution {cycle}");
            Console.WriteLine($"Power Bomb negative control: {cycle}-step circle differs at {mismatches}/192 basis rows.");
        }
        CheckBounds(PowerBombProfileRadius, 20);
        CheckBounds(i => PowerBombProfile(0, i, basis), 191);
        Console.WriteLine($"PASS: 4,032/4,032 pre-scaled Power Bomb bytes from a 1,024-step ellipse raster and quadratic frame radii; no table or corrections. Smooth basis misses {smoothEllipse} bytes.");
    }

    private static int[] PowerBombBasis(int cycle, decimal pi)
    {
        int quarter = cycle / 4;
        var result = new int[192];
        int angleIndex = 0;
        for (int row = 0; row < 192; row++)
        {
            if (row == 191) { result[row] = 0; continue; }
            while (angleIndex < quarter)
            {
                decimal sine = FullSine(2 * pi * (angleIndex + 1) / cycle);
                decimal low = 192 * (sine - ResearchData.Error), high = 192 * (sine + ResearchData.Error);
                Equal(low <= row + 1, high <= row + 1, $"Power Bomb raster interval {cycle}/{row}/{angleIndex}");
                if (high > row + 1) break;
                angleIndex++;
            }
            result[row] = angleIndex == 0 ? 255 : Math.Min(255,
                StableFloor(256 * FullCosine(2 * pi * angleIndex / cycle), 256 * ResearchData.Error));
        }
        return result;
    }

    private static int PowerBombProfile(int frame, int row, int[] basis)
    {
        int radius = PowerBombProfileRadius(frame);
        Bound(row, 191);
        int sourceRow = Math.Max(0, (256 * row - 1) / radius);
        return sourceRow >= 192 ? 0 : basis[sourceRow] * radius / 256;
    }

    private static int PowerBombProfileRadius(int frame)
    {
        Bound(frame, 20);
        int n = frame < 17 ? frame + 37 : frame - 17 + 3;
        return frame < 17
            ? Math.Min(255, 4 + 3 * n * (n - 1) / 32)
            : Math.Min(255, (16 + 192 * n - n * (n - 1)) / 4);
    }
}

internal static class PowerBombProfileResearchData
{
    /// <summary>$88:9246, PowerBomb_Explosion_ShapeDefinitionTiles_PreScaled, seventeen 192-byte frames.</summary>
    internal const int WhiteAddress = 0x889246;
    /// <summary>$88:9F06, PowerBomb_PreExplosion_ShapeDefinitionTables_PreScaled, four 192-byte frames.</summary>
    internal const int YellowAddress = 0x889f06;
}
