namespace SuperMetroid.Core.Game;

/// <summary>Shared discrete collision heights for Samus, enemies, projectiles and bomb spread.</summary>
public static class SlopeHeightDefinitions
{

    /// <summary>Returns the native five-bit height for shape 0..31 and column 0..15.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_94.asm.
    /// Flat, half-block, V, stair and rational-slope families below reproduce all 512
    /// original bytes. Integer division floors nonnegative profile coordinates; the
    /// third-height family carries its division phase across tile seams. Steep slopes
    /// encode heights above sixteen as twenty, not a saturation to twenty. Shape 17
    /// is an explicit authored overhang (twenty until column thirteen, then sixteen).
    /// Bounds are checked before arithmetic. Geometric clipping never clamps input;
    /// BTS mirroring, collision comparison and world-coordinate wrapping stay in callers.
    /// </remarks>
    public static byte Read(int shape, int column)
    {
        if ((uint)shape >= 32) throw new ArgumentOutOfRangeException(nameof(shape));
        if ((uint)column >= 16) throw new ArgumentOutOfRangeException(nameof(column));
        int height = shape switch
        {
            0 or 7 => 8,
            1 => column < 8 ? 16 : 0,
            2 => column < 8 ? 16 : 8,
            3 => column < 8 ? 8 : 0,
            4 or >= 8 and <= 13 or 19 => 0,
            5 or 6 => 16 - (shape - 4) * Math.Min(column, 15 - column),
            14 => 12 - 4 * (column / 4),
            15 => 14 - 2 * (column / 2),
            16 => 16,
            17 => column < 13 ? 20 : 16,
            18 => 16 - column,
            20 or 21 => Math.Clamp(24 - 16 * (shape - 20) - column, 0, 16),
            22 or 23 => 16 - (16 * (shape - 22) + column) / 2,
            >= 24 and <= 26 => 16 - (16 * (shape - 24) + column) / 3,
            27 or 28 => EncodeSteepHeight(32 - 16 * (shape - 27) - 2 * column),
            _ => EncodeSteepHeight(48 - 16 * (shape - 29) - 3 * column),
        };
        return (byte)height;
    }

    private static int EncodeSteepHeight(int height) => height > 16 ? 20 : Math.Max(0, height);
}
