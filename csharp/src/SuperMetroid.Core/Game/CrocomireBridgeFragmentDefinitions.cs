namespace SuperMetroid.Core.Game;

/// <summary>Compiled physical launch positions for Crocomire's crumbling bridge actors.</summary>
internal static class CrocomireBridgeFragmentDefinitions
{
    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_2_Falling.XPositions</c> at
    /// <c>$A4:9156-$A4:916B</c>. The native cursor is a byte offset advancing by two.
    /// </summary>
    /// <remarks>
    /// The eleven equal-width floor columns are visited in this destruction choreography:
    /// 8,3,9,4,11,6,10,7,1,5,2. $A4:9136-$9153 advances the cursor by two and passes
    /// each selected X to the same producer. $86:9286-$92B9 fixes Y187, X velocity0 and
    /// graphics$0400; independently, each fragment gets Y velocity(RNG &amp; 63)+64.
    /// No RNG, actor direction or data-dependent choice selects the
    /// order. The permutation is the authored ordering of the visual destruction, not
    /// sampled geometry. Retain only that order; coordinates calculate from the grid.
    /// Issue1165 retains this order as the narrow nonsense exception: replacing the
    /// chosen destruction choreography with an invented shuffle would change the effect.
    /// </remarks>
    private static ReadOnlySpan<byte> AuthoredCrumbleColumnOrder => [8, 3, 9, 4, 11, 6, 10, 7, 1, 5, 2];

    /// <summary>$A4:9156-$916B cover eleven floor columns beginning at X$0710 with16-pixel spacing.</summary>
    private const int FirstColumnX = 0x0710;
    /// <summary>Returns the authored X position selected by an even native byte cursor.</summary>
    internal static ushort XPosition(ushort byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset >= AuthoredCrumbleColumnOrder.Length * 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset), byteOffset,
                "Crocomire bridge-fragment offset must be even and zero through twenty.");
        }

        return (ushort)(FirstColumnX + 16 * (AuthoredCrumbleColumnOrder[byteOffset >> 1] - 1));
    }
}
