namespace SuperMetroid.Core.Frontend;

/// <summary>What the frontend plays when Samus dies.</summary>
internal static class SamusDeathDefinitions
{
    /// <summary>The death fanfare, selected after every sound and the current music stop.</summary>
    public const MusicTrack FanfareTrack = MusicTrack.SamusDeath;

    /// <summary>How long the death fanfare waits on the music queue: fourteen passes.</summary>
    public static MusicQueueDelay FanfareDelay { get; } = new(14);

    /// <summary>
    /// The shared fade counter's last value at which a step of the death fade moves the display toward black: the step
    /// after it completes the fade. The display darkens in this many parts, one for each of the fade's own steps after its
    /// first (#2421).
    /// </summary>
    public const int FadeLastStep = 7;
}
