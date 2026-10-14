namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Mutable position, velocity, and packed delay or animation phase of one post-credits star.</summary>
internal struct EndingShootingStar
{
    /// <summary>Native packed star index or animation frame.</summary>
    internal ushort IndexAndFrame;
    /// <summary>Current delay or animation countdown.</summary>
    internal NativeCountdown Timer;
    /// <summary>Fixed-point screen position before centering the sprite for OAM.</summary>
    internal FixedPosition<ScreenPixels> Position;
    /// <summary>Current signed 8.8 motion accumulated by the star's acceleration.</summary>
    internal EightEightVector<ScreenPixels> Velocity;
}
