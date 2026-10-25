namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// One scanline of the suit pickup's light beam: its inclusive left and right screen columns. A left edge right of the
/// right edge leaves the scanline empty.
/// </summary>
/// <param name="Left">The inclusive left column, 0 through 255.</param>
/// <param name="Right">The inclusive right column, 0 through 255.</param>
[EncodingOwner("suit pickup beam scanline edges")]
public readonly record struct SuitPickupBeamEdges(byte Left, byte Right)
{
    /// <summary>The HDMA table word for these edges: the left column in the low byte, the right column in the high byte.</summary>
    public ushort Word => SnesLittleEndian.Word(Left, Right);
}
