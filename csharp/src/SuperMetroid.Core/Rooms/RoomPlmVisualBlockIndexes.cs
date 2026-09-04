namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Visual block indexes synthesized by bank-$84 PLM setup routines rather than read from
/// room level data. These are low ten-bit block-definition indexes, not complete packed
/// level words or bank-$84 draw-list pointers.
/// </summary>
internal static class RoomPlmVisualBlockIndexes
{
    /// <summary>
    /// Contact-crumble parent visual block `$0BC`, written by setup `$84:CE37` before its
    /// authored animation list replaces it.
    /// </summary>
    public const ushort ContactCrumbleParent = 0x00bc;
}
