namespace SuperMetroid.Core.Rendering;

/// <summary>How a gradient's band changes the screen under it (#1468, #1514).</summary>
public enum GradientBlend
{
    /// <summary>The band's colour darkens the floor and the backdrop, subtracted from them; the sprites keep their colours.</summary>
    DarkensFloorAndBackdrop,

    /// <summary>
    /// The band's colour brightens the floor, the backdrop and the sprites that take part in blends, added to them; the other
    /// sprites keep their colours.
    /// </summary>
    BrightensFloorBackdropAndSprites,
}
