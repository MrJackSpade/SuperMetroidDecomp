namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The mirroring a sprite part or background tile is drawn with. The two axes are independent:
/// a picture is drawn unmirrored, mirrored on either axis, or on both.
/// </summary>
[Flags]
public enum TileFlip : ushort
{
    /// <summary>Leaves both axes in their stored pixel order.</summary>
    None = 0,
    /// <summary>Sets bit 14 to reverse the tile's horizontal pixel order.</summary>
    Horizontal = 0x4000,
    /// <summary>Sets bit 15 to reverse the tile's vertical pixel order.</summary>
    Vertical = 0x8000,
}
