using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rendering;

/// <summary>Exact pre-scaled and unscaled Power Bomb ellipse generation.</summary>
public static class PowerBombShapeDefinitions
{
    // Rasterize the quarter ellipse on a 1,024-step circle before radius scaling.
    // Select the last angular sample below the next row boundary. Binary search
    // preserves that convention without retaining a generated sample cache.
    /// <summary>Evaluates one row of the shared 192-row quarter-ellipse width basis.</summary>
    private static byte PreScaledBasis(int row)
    {
        if (row == 191) return 0;
        int low = 0, high = 256;
        while (low + 1 < high)
        {
            int middle = (low + high) / 2;
            if (192 * QuarterSine(middle) <= row + 1) low = middle;
            else high = middle;
        }
        return (byte)Math.Min(255, (int)(256 * QuarterSine(256 - low)));
    }

    // Domain 0..256 is established by the raster search. Exact endpoints avoid
    // integer-boundary drift. Twelve alternating terms on [0, pi/2] leave less
    // than 5.2e-21 remainder; 1e-19 also covers decimal and pi rounding.
    /// <summary>Computes the unit quarter-wave sample used by the native shape raster search.</summary>
    private static decimal QuarterSine(int angle)
    {
        if (angle == 0) return 0;
        if (angle == 256) return 1;
        decimal x = angle * 3.1415926535897932384626433833m / 512;
        decimal squared = x * x;
        decimal term = x, sum = x;
        for (int k = 1; k < 12; k++)
        {
            term = -term * squared / ((2 * k) * (2 * k + 1));
            sum += term;
        }
        return sum;
    }

    /// <summary>Reads one exact $88:9246-$A205 pre-scaled shape byte.</summary>
    /// <remarks>All 21 profiles share a 192-row ellipse rasterized on a 1,024-step
    /// circle: choose the largest k with 192*sin(k*pi/512) &lt;= sourceRow+1,
    /// then truncate 256*cos(k*pi/512), saturating to a byte. The final row is zero.
    /// White/yellow radii integrate their native acceleration/deceleration; select
    /// the lower row at exact inverse-scaling boundaries before multiplying by radius.
    /// All 4,032 bytes independently match the NTSC ROM and pinned bank_88.asm.
    /// Only aligned native profile pointers and rows 0..191 are accepted.</remarks>
    public static byte ReadPreScaledHalfWidth(ushort shapePointer, int row)
    {
        if ((uint)row >= SamusSpecialSequenceRomData.PowerBomb.ShapeStride)
            throw new ArgumentOutOfRangeException(nameof(row));

        int frame;
        if (shapePointer >= SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape &&
            shapePointer < SamusSpecialSequenceRomData.PowerBomb.WhiteShapeEnd &&
            (shapePointer - SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape) %
                SamusSpecialSequenceRomData.PowerBomb.ShapeStride == 0)
        {
            frame = (shapePointer - SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape) /
                SamusSpecialSequenceRomData.PowerBomb.ShapeStride;
        }
        else if (shapePointer >= SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape &&
            shapePointer < SamusSpecialSequenceRomData.PowerBomb.YellowShapeEnd &&
            (shapePointer - SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape) %
                SamusSpecialSequenceRomData.PowerBomb.ShapeStride == 0)
        {
            frame = 17 + (shapePointer - SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape) /
                SamusSpecialSequenceRomData.PowerBomb.ShapeStride;
        }
        else
        {
            throw new InvalidDataException(
                $"Power Bomb shape pointer $88:{shapePointer:X4} is outside the compiled profiles.");
        }

        int n = frame < 17 ? frame + 37 : frame - 14;
        int radius = frame < 17
            ? Math.Min(255, 4 + 3 * n * (n - 1) / 32)
            : Math.Min(255, (16 + 192 * n - n * (n - 1)) / 4);
        int sourceRow = Math.Max(0, (256 * row - 1) / radius);
        return sourceRow >= SamusSpecialSequenceRomData.PowerBomb.ShapeStride
            ? (byte)0
            : (byte)(PreScaledBasis(sourceRow) * radius / 256);
    }
    /// <summary>$88:A266, PowerBombExplosion_ShapeDefinitionTable_Unscaled_width: 32 bottom-to-center widths.</summary>
    /// <remarks>For index 0..31, floor(256*sin(index*pi/64)). The shared deterministic
    /// half-wave evaluator reproduces every original NTSC byte. Radius scaling follows
    /// this quantization in the renderer. Invalid indices preserve span bounds behavior.</remarks>
    public static byte Width(int index)
    {
        if ((uint)index >= 32) throw new IndexOutOfRangeException();
        return EnemyTrigonometryTables.EightBitHalfWave(2 * index);
    }

    /// <summary>$88:A286, PowerBombExplosion_ShapeDefinitionTable_Unscaled_topOffset: inclusive vertical band boundaries.</summary>
    /// <remarks>For index 0..31, floor(3*floor(256*cos((2*index+1)*pi/128))/4).
    /// Quantize the half-step cosine before the 3/4 vertical aspect ratio; direct
    /// truncation of 192*cos differs. All original NTSC bytes independently match.
    /// The renderer then scales by radius and fills inclusive band boundaries.</remarks>
    public static byte TopOffset(int index)
    {
        if ((uint)index >= 32) throw new IndexOutOfRangeException();
        return (byte)(EnemyTrigonometryTables.EightBitHalfWave(63 - 2 * index) * 3 / 4);
    }
}
