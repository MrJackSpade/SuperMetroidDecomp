using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyGeometry(byte[] rom)
    {
        int[] heights = Oracle(rom, GeometryResearchData.Heights, 512, 1);
        for (int shape = 0; shape < 32; shape++)
        for (int x = 0; x < 16; x++)
        {
            int expected = heights[shape * 16 + x];
            Equal(expected, (int)SlopeHeightDefinitions.Read(shape, x), $"compiled height {shape}/{x}");
            Equal(expected, Height(shape, x), $"geometric height {shape}/{x}");
        }
        foreach (int address in new[] { GeometryResearchData.SamusQuadrants, GeometryResearchData.EnemyQuadrants, GeometryResearchData.ProjectileQuadrants })
        {
            int[] native = Oracle(rom, address, 20, 1, $"SquareSlopeDefinitions_Bank{address >> 16:X2}");
            bool identity = address != GeometryResearchData.SamusQuadrants;
            for (int i = 0; i < 20; i++)
            {
                int candidate = SquareQuadrant(i / 4, i % 4) | (identity ? i % 4 : 0);
                Equal(native[i], candidate, $"quadrant ${address:X6}/{i}");
                Equal(native[i], (int)(identity ? SquareSlopeDefinitions.EnemyQuadrants[i] : SquareSlopeDefinitions.SamusQuadrants[i]), $"compiled quadrant {i}");
            }
        }
        // Every axis is checked separately; geometric clamping is not input clamping.
        for (int shape = 0; shape < 32; shape++) CheckBounds(x => Height(shape, x), 15);
        for (int x = 0; x < 16; x++) CheckBounds(shape => Height(shape, x), 31);
        for (int shape = 0; shape < 5; shape++) CheckBounds(q => SquareQuadrant(shape, q), 3);
        for (int q = 0; q < 4; q++) CheckBounds(shape => SquareQuadrant(shape, q), 4);
        Console.WriteLine("PASS: 512/512 slope heights and 60/60 square-slope bytes (three native copies), with both input axes bounded.");
    }

    private static int Height(int shape, int x)
    {
        Bound(shape, 31);
        Bound(x, 15);
        if (shape is 0 or 7) return 8;
        if (shape is >= 1 and <= 3)
            return x < 8 ? (shape == 3 ? 8 : 16) : (shape == 2 ? 8 : 0);
        if (shape is 4 or >= 8 and <= 13 or 19) return 0;
        if (shape is 5 or 6) return 16 - (shape - 4) * Math.Min(x, 15 - x);
        if (shape is 14 or 15)
        {
            int step = 1 << (16 - shape);
            return 16 - step * (x / step + 1);
        }
        if (shape == 16) return 16;
        if (shape == 17) return x < 13 ? 20 : 16;
        if (shape == 18) return 16 - x;
        if (shape is 20 or 21) return Math.Clamp(24 - (shape - 20) * 16 - x, 0, 16);
        if (shape is 22 or 23) return 16 - ((shape - 22) * 16 + x) / 2;
        if (shape is >= 24 and <= 26) return 16 - ((shape - 24) * 16 + x) / 3;
        int height = shape <= 28 ? 32 - 16 * (shape - 27) - 2 * x : 48 - 16 * (shape - 29) - 3 * x;
        return height > 16 ? 20 : Math.Max(0, height);
    }

    private static int SquareQuadrant(int shape, int quadrant)
    {
        Bound(shape, 4);
        Bound(quadrant, 3);
        bool right = (quadrant & 1) != 0, bottom = (quadrant & 2) != 0;
        bool solid = shape switch
        {
            0 => bottom,
            1 => right,
            2 => bottom && right,
            3 => bottom || right,
            _ => true,
        };
        return solid ? 128 : 0;
    }
}

internal static class GeometryResearchData
{
    /// <summary>$94:8B2B, SlopeDefinitions_SlopeTopXOffsetByYPixel, 32 sixteen-column shapes.</summary>
    internal const int Heights = 0x948b2b;
    /// <summary>$94:8E54, SquareSlopeDefinitions_Bank94, five four-quadrant masks.</summary>
    internal const int SamusQuadrants = 0x948e54;
    /// <summary>$A0:C435, SquareSlopeDefinitions_BankA0, masks plus quadrant identity.</summary>
    internal const int EnemyQuadrants = 0xa0c435;
    /// <summary>$86:8729, SquareSlopeDefinitions_Bank86, identical projectile masks/identity.</summary>
    internal const int ProjectileQuadrants = 0x868729;
}
