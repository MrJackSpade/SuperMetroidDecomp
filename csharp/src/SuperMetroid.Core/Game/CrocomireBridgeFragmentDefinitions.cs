namespace SuperMetroid.Core.Game;

/// <summary>Compiled physical launch positions for Crocomire's crumbling bridge actors.</summary>
internal static class CrocomireBridgeFragmentDefinitions
{
    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_2_Falling.XPositions</c> at
    /// <c>$A4:9156-$A4:916B</c>. The native cursor is a byte offset advancing by two.
    /// </summary>
    /// <remarks>
    /// All eleven words match the pinned NTSC J/U v1.0 ROM. They permute the
    /// eleven whole-pixel positions <c>$0710,$0720,...,$07B0</c>, each once,
    /// in the authored projectile launch order. Retain that bounded order:
    /// computing the grid still requires an opaque shuffle to reproduce
    /// the exact sequence. The next bank-$A4 bytes begin another routine.
    /// Investigation: #625 / #674.
    /// </remarks>
    private static readonly ushort[] XPositions =
    [
        0x0780, 0x0730, 0x0790, 0x0740, 0x07b0, 0x0760,
        0x07a0, 0x0770, 0x0710, 0x0750, 0x0720,
    ];

    /// <summary>Returns the authored X position selected by an even native byte cursor.</summary>
    internal static ushort XPosition(ushort byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset >= XPositions.Length * 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset), byteOffset,
                "Crocomire bridge-fragment offset must be even and zero through twenty.");
        }

        return XPositions[byteOffset >> 1];
    }
}
