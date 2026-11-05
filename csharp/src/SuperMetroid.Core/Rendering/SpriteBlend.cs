namespace SuperMetroid.Core.Rendering;

/// <summary>How a sprite layer's sprites show over the layers behind them (#1514).</summary>
public enum SpriteBlend
{
    /// <summary>Each sprite pixel covers what lies behind it.</summary>
    Drawn,

    /// <summary>
    /// Each sprite pixel glows: its colour is added to what lies behind it, each channel clamped at full, as the ending's
    /// explosion debris and shooting stars brighten the planet and the sky they cross.
    /// </summary>
    Glows,
}
