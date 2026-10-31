namespace SuperMetroid.Core.Assets;

/// <summary>Where a spritemap's origin lies, which decides how a part wraps vertically.</summary>
public enum SpriteOrigin
{
    /// <summary>The origin is on screen: a Y below the screen wraps above it.</summary>
    OnScreen,

    /// <summary>The origin is off screen: the wrap runs the other way.</summary>
    OffScreen,
}
