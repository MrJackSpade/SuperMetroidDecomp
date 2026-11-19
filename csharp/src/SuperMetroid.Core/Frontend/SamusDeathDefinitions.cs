namespace SuperMetroid.Core.Frontend;

/// <summary>What the frontend plays when Samus dies.</summary>
internal static class SamusDeathDefinitions
{
    /// <summary>The death fanfare, selected after every sound and the current music stop.</summary>
    public const MusicTrack FanfareTrack = MusicTrack.SamusDeath;

    /// <summary>How long the death fanfare waits on the music queue: fourteen passes.</summary>
    public static MusicQueueDelay FanfareDelay { get; } = new(14);

    /// <summary>
    /// The parts the death fade darkens the display in, one for each of its own steps after its first; the step after the
    /// last part completes it (#2421).
    /// </summary>
    public const int FadeParts = 7;
}
