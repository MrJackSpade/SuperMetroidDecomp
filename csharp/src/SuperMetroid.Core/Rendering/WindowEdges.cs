namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One scanline's PPU window: its inclusive left and right screen columns. A left edge right of the right edge leaves
/// the scanline empty.
/// </summary>
/// <param name="Left">The inclusive left column, 0 through 255.</param>
/// <param name="Right">The inclusive right column, 0 through 255.</param>
[SnesEncodingOwner]
public readonly record struct WindowEdges(byte Left, byte Right)
{
    /// <summary>The HDMA table word for these edges: the left column in the low byte, the right column in the high byte.</summary>
    public ushort Word => SnesLittleEndian.Word(Left, Right);
}
