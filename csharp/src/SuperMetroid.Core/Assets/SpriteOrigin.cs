namespace SuperMetroid.Core.Assets;

/// <summary>Where a spritemap's origin lies, which decides how a part wraps vertically.</summary>
internal enum SpriteOrigin
{
    /// <summary>The origin is on screen ($81:8A5F wrap).</summary>
    OnScreen,

    /// <summary>The origin is off screen ($81:8853 reversed wrap).</summary>
    OffScreen,
}
