namespace SuperMetroid.Core.Assets;

/// <summary>Which row a sprite draw's pixel takes its colours from (#1405).</summary>
public enum ColourKeying
{
    /// <summary>The row the pixel's image was coloured in.</summary>
    InkRow,

    /// <summary>The pixel's part row, as a draw's override leaves it.</summary>
    PartRow,
}
