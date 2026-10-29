namespace SuperMetroid.Core.Assets;

/// <summary>Which of its image's colour groups a sprite draw's pixel takes its colours from (#1405, #1464).</summary>
public enum ColourKeying
{
    /// <summary>The colour group the pixel's ink was coloured from.</summary>
    InkGroup,

    /// <summary>The pixel's part colour group, as a drawn colouring recolours it.</summary>
    PartGroup,
}
