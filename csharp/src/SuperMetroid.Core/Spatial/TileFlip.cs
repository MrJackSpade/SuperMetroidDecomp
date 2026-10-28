namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The mirroring a sprite part or background tile is drawn with. The two axes are independent:
/// a picture is drawn unmirrored, mirrored on either axis, or on both.
/// </summary>
[Flags]
public enum TileFlip : byte
{
    /// <summary>Draws the picture as stored.</summary>
    None = 0,
    /// <summary>Mirrors the picture left to right.</summary>
    Horizontal = 1,
    /// <summary>Mirrors the picture top to bottom.</summary>
    Vertical = 2,
}
