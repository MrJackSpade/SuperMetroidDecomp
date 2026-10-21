namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Mutable position, velocity, and packed delay or animation phase of one post-credits star.</summary>
internal struct EndingShootingStar
{
    /// <summary>Whether the star is still waiting out its authored start delay, counted by <see cref="Timer"/>.</summary>
    internal bool Delayed;
    /// <summary>Animation frame: zero during the invisible lead-in, then one more per animation period until the star leaves the screen.</summary>
    internal int AnimationFrame;
    /// <summary>Current delay or animation countdown.</summary>
    internal WrappingCountdown Timer;
    /// <summary>Fixed-point screen position before centering the sprite for OAM.</summary>
    internal FixedPosition<ScreenPixels> Position;
    /// <summary>Current signed 8.8 motion accumulated by the star's acceleration.</summary>
    internal EightEightVector<ScreenPixels> Velocity;
}
