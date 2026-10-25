namespace SuperMetroid.Core.Fx;

/// <summary>The rain's randomly chosen sideways drift (#1415).</summary>
public static class AtmosphereDrift
{
    /// <summary>Calculates rain's signed 8.8 horizontal velocity for selector 0..3.</summary>
    /// <remarks>Native $88:D981 selects a word at $88:D992 using RNG bits two/three.
    /// Selector bit one reduces the speed from six to four pixels/frame; bit zero
    /// selects positive rather than negative motion. Multiply by256, then preserve
    /// the two's-complement ushort representation. The four original values and
    /// all ushort RNG inputs are independently verified against NTSC J/U v1.0 and
    /// pinned bank_88.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// Invalid selectors retain the former indexed span's rejection.</remarks>
    public static ushort RainHorizontalVelocity(int selection)
    {
        if ((uint)selection >= 4)
            throw new ArgumentOutOfRangeException(nameof(selection));
        int speed = SnesEightEight.FromWhole(6 - (selection & 2));
        return unchecked((ushort)((selection & 1) == 0 ? -speed : speed));
    }
}
