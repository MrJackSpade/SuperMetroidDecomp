namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The codec between <see cref="SnesWindowSelectionPair"/> and each layer's <see cref="SnesWindowSelection"/>: the
/// lower layer's selection is the pair's low four bits and the upper layer's the high four, as W12SEL, W34SEL and
/// WOBJSEL pack them. This type is the one place a pair is split into its layers or written as the register byte.
/// </summary>
[SnesEncodingOwner]
public static class SnesWindowSelectionPairCodec
{
    /// <summary>The bits one layer's selection occupies.</summary>
    private const int LayerBits = 4;

    /// <summary>The bits of one layer's selection once shifted to the low end.</summary>
    private const int LayerMask = (1 << LayerBits) - 1;

    /// <summary>The window selection of the lower or upper layer of <paramref name="pair"/>.</summary>
    /// <param name="pair">The two layers' selections.</param>
    /// <param name="upper">True for BG2, BG4 or the colour window; false for BG1, BG3 or OBJ.</param>
    public static SnesWindowSelection Layer(this SnesWindowSelectionPair pair, bool upper) =>
        (SnesWindowSelection)(((byte)pair >> (upper ? LayerBits : 0)) & LayerMask);

    /// <summary>The pair as the selection register byte that stores it.</summary>
    /// <param name="pair">The two layers' selections.</param>
    public static byte ToNativeByte(this SnesWindowSelectionPair pair) => (byte)pair;
}
