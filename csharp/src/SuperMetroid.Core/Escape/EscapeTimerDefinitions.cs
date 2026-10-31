namespace SuperMetroid.Core.Escape;

/// <summary>The escape timer's durations, clock units, startup delays and on-screen motion.</summary>
internal static class EscapeTimerDefinitions
{
    /// <summary>Seconds in one minute of the countdown.</summary>
    internal const int SecondsPerMinute = 60;

    /// <summary>Centiseconds in one minute of the countdown.</summary>
    internal const int CentisecondsPerMinute = SecondsPerMinute * EscapeTimerCadenceDefinitions.CentisecondsPerSecond;

    /// <summary>The Ceres station's escape countdown: one minute.</summary>
    internal const int CeresDuration = 1 * CentisecondsPerMinute;

    /// <summary>Zebes's escape countdown after Mother Brain: three minutes.</summary>
    internal const int MotherBrainDuration = 3 * CentisecondsPerMinute;

    /// <summary>The longest time the timer's two minute digits show: 99:59.99.</summary>
    internal const int LongestTime = 100 * CentisecondsPerMinute - 1;

    /// <summary>The time the escape-timeout floor holds the countdown at: one second.</summary>
    internal const int TimeoutFloor = EscapeTimerCadenceDefinitions.CentisecondsPerSecond;

    /// <summary>The updates from the start the timer waits at the centre before it counts down.</summary>
    internal const int InitialDelayFrames = 16;

    /// <summary>The updates from the start before the timer leaves the centre for its corner.</summary>
    internal const int MovementDelayFrames = 96;

    /// <summary>The pixel each axis of the timer's cleared position sits at: the screen's centre.</summary>
    internal const int ClearedPixel = 128;

    /// <summary>The pixel column the timer settles at.</summary>
    internal const int SettledX = 220;

    /// <summary>The pixel row the timer settles at.</summary>
    internal const int SettledY = 48;

    /// <summary>How far right the timer moves each update while moving into place, in 1/256 pixels.</summary>
    internal const int RightwardSubpixelsPerFrame = 224;

    /// <summary>How far up the timer moves each update while moving into place, in 1/256 pixels.</summary>
    internal const int UpwardSubpixelsPerFrame = 193;
}
