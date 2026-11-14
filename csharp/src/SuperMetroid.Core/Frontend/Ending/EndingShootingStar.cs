namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Mutable position, velocity, start delay and animation frame of one post-credits star.</summary>
internal struct EndingShootingStar
{
    /// <summary>Whether the star is still waiting out its authored start delay, counted by <see cref="Timer"/>.</summary>
    internal bool Delayed;
    /// <summary>Animation frame: zero during the invisible lead-in, then one more per animation period until the star leaves the screen.</summary>
    internal int AnimationFrame;
    /// <summary>Current delay or animation countdown.</summary>
    internal int Timer;
    /// <summary>Fixed-point screen position before centering the sprite.</summary>
    internal FixedPosition<ScreenPixels> Position;
    /// <summary>Current signed 1/256-pixel motion accumulated by the star's acceleration.</summary>
    internal EightEightVector<ScreenPixels> Velocity;
}
