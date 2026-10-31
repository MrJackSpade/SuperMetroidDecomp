namespace SuperMetroid.Core.Assets;

/// <summary>Which half of the colour table a pre-coloured image's palette indices name (#1392).</summary>
public enum SpriteImageColours
{
    /// <summary>The eight sprite palette rows, colours 128 to 255: actors' sheets.</summary>
    Sprite,

    /// <summary>The eight background palette rows, colours 0 to 127: room block sheets.</summary>
    Background,

    /// <summary>
    /// The HUD layer's eight four-colour palette rows, colours 0 to 31: HUD and room effect sheets. Every pixel keeps its row
    /// times four plus its colour, colour zero included, so a layer that shows colour zero shows its row's (#1392).
    /// </summary>
    TwoBppBackground,

    /// <summary>
    /// All 256 colours, as a Mode 7 tile names its colours directly: the Ceres rooms' Mode 7 tiles, some of whose
    /// pixels name the sprite rows every room load restores (#1392).
    /// </summary>
    FullPalette,
}
