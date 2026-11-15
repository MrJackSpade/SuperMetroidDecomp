namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Mutable position, velocity, start delay and animation frame of one post-credits star.</summary>
internal struct EndingShootingStar
{
    /// <summary>Whether the star is still waiting out its authored start delay, counted by <see cref="Timer"/>.</summary>
    internal bool Delayed;
    /// <summary>Animation frame: zero during the invisible lead-in, then one more per animation period until the star leaves the screen; a still star stays at one.</summary>
    internal int AnimationFrame;
    /// <summary>Current delay or animation countdown.</summary>
    internal int Timer;
    /// <summary>
    /// Fixed-point signed screen place before centering the sprite: left of or above the screen is negative, so a star
    /// leaving by the top or left edge is tested against <see cref="EndingShootingStarDefinitions.SpriteOffset"/> as it is.
    /// </summary>
    internal FixedVector<ScreenPixels> Position;
    /// <summary>Current signed 1/256-pixel motion accumulated by the star's acceleration.</summary>
    internal EightEightVector<ScreenPixels> Velocity;
}
