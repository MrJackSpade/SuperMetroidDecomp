namespace SuperMetroid.Core.Assets;

/// <summary>Which half of CGRAM a pre-coloured image's palette indices name (#1392).</summary>
public enum SpriteImageColours
{
    /// <summary>The eight sprite palette rows, CGRAM colours 128 to 255: actors' sheets.</summary>
    Sprite,

    /// <summary>The eight background palette rows, CGRAM colours 0 to 127: room block sheets.</summary>
    Background,
}
