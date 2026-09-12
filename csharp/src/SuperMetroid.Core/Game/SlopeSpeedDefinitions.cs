namespace SuperMetroid.Core.Game;

/// <summary>Cartridge-authored grounded horizontal movement scaling for each slope shape.</summary>
public static class SlopeSpeedDefinitions
{
    /// <summary>
    /// $94:8588 + 4*shape, SamusBlockCollisionDetection_Horizontal_Slope_NonSquare
    /// .adjustedDistanceMultiplier (odd words of kBlockColl_Horiz_Slope_NonSquare_Tab).
    /// Unsigned multipliers use eight fractional bits. The interleaved .speedModifiers
    /// participate only in discarded calculations and are not movement parameters.
    /// </summary>
    private static ReadOnlySpan<ushort> Multipliers =>
    [
        0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100,
        0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x0b0, 0x0b0,
        0x100, 0x100, 0x0c0, 0x100, 0x0c0, 0x0c0, 0x0d8, 0x0d8,
        0x0f0, 0x0f0, 0x0f0, 0x080, 0x080, 0x050, 0x050, 0x050,
    ];

    /// <summary>Selects the exact multiplier; BTS orientation and airborne admission belong to the caller.</summary>
    public static ushort HorizontalMultiplier(int shape)
    {
        if ((uint)shape >= 32) throw new ArgumentOutOfRangeException(nameof(shape));
        return Multipliers[shape];
    }
}
