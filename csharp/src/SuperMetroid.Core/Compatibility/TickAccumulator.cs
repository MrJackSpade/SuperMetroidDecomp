namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A phase that advances by a step on every sound clock step and overflows once per
/// <see cref="Period"/> accumulated counts, carrying the remainder into the next step. The sound
/// program paces its music (by the track's tempo) and its request checks (by a fixed step) this
/// way, so which update a music command or a sound request is acted on depends on the carried
/// phase; that timing is gameplay-observable through the acknowledgements, so the overflow and
/// its remainder are kept exactly.
/// </summary>
[CompatibilityService("a phase that wraps at a fixed period and carries its remainder, pacing the sound clock's checks and the music tempo")]
public readonly record struct TickAccumulator
{
    /// <summary>Accumulated counts per overflow: the phase wraps back to zero after this many.</summary>
    public const int Period = 256;

    /// <summary>Stores a phase already inside [0, <see cref="Period"/>).</summary>
    internal TickAccumulator(int phase) => Phase = phase;

    /// <summary>Counts accumulated since the last overflow, zero through <see cref="Period"/> - 1.</summary>
    public int Phase { get; }

    /// <summary>
    /// Adds one tick's <paramref name="step"/>. <c>Overflowed</c> is whether the sum reached a full
    /// period, which is when the paced service runs; <c>Next</c> keeps the remainder.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="step"/> is negative.</exception>
    public TickAdvance Advance(int step)
    {
        int sum = Phase + Ensure.AtLeastZero(step);
        return new TickAdvance(new TickAccumulator(sum % Period), sum >= Period);
    }
}
