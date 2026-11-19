namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One music track's period as the offline analysis found it (#1305, #2554): a loop with the two arrivals that bound it,
/// an end with the tick that ended it, or no repeat within the search bound. Each case carries the data its shape needs.
/// </summary>
internal abstract record MusicTrackPeriod
{
    /// <summary>Whether the track loops, ends, or showed no repeat.</summary>
    internal abstract MusicTrackShape Shape { get; }

    /// <summary>
    /// A track that loops: the first two arrivals E1 and E2 at the first repeated sequencer state, so the exported clip is
    /// intro [start, E1) and loop [E1, E2).
    /// </summary>
    /// <param name="FirstArrival">E1, where the loop begins.</param>
    /// <param name="SecondArrival">E2, where the loop returns to E1.</param>
    internal sealed record Loop(long FirstArrival, long SecondArrival) : MusicTrackPeriod
    {
        /// <inheritdoc/>
        internal override MusicTrackShape Shape => MusicTrackShape.Loops;
    }

    /// <summary>A track that reaches its terminator and returns the music port to zero.</summary>
    /// <param name="EndSample">The tick that returned the music port to zero.</param>
    internal sealed record Ending(long EndSample) : MusicTrackPeriod
    {
        /// <inheritdoc/>
        internal override MusicTrackShape Shape => MusicTrackShape.Ends;
    }

    /// <summary>A track whose phrase state did not repeat within the search bound.</summary>
    internal sealed record NoneFound : MusicTrackPeriod
    {
        /// <inheritdoc/>
        internal override MusicTrackShape Shape => MusicTrackShape.NoPeriodFound;
    }
}
