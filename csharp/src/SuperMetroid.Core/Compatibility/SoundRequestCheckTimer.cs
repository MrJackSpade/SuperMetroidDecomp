namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The timer that decides on which sound clock steps the sound libraries check the game's requests. Each clock step adds
/// an eighth of the timer's <see cref="Period"/>, and the timer checks the requests when it reaches a full period,
/// keeping the remainder; so the requests are checked on every eighth step, and the update on which a request is acted
/// on depends on the phase the timer carries from update to update. A clock step that spans several step periods adds
/// all their eighths at once, formed within one period, as the original sound program formed its increment.
/// </summary>
/// <remarks>
/// Advanced on every step of the <see cref="SoundAcceptanceClock{TSound}"/>, which runs once per update after the
/// update's sound requests; its overflow is when the clock acknowledges and takes each library's request. Evidence: the
/// sound program's timer service adds to its request-check counter on each tick of its 500-Hz timer and reads the
/// requests (<c>$1621</c>) when the counter overflows; the sound acceptance clock check compares the acceptance this
/// timer paces with the emulated sound program's, and the SMV replays compare the acknowledgements the game reads back.
/// </remarks>
[CompatibilityService("the request-check timer: every clock step adds an eighth of a period, and the overflow, with its remainder carried, checks the sound requests")]
public readonly record struct SoundRequestCheckTimer
{
    /// <summary>Counts per check: the timer checks the requests and wraps back to zero after this many.</summary>
    public const int Period = 256;

    /// <summary>Stores a phase already inside [0, <see cref="Period"/>).</summary>
    internal SoundRequestCheckTimer(int phase) => Phase = phase;

    /// <summary>Counts accumulated since the last check, zero through <see cref="Period"/> - 1.</summary>
    public int Phase { get; }

    /// <summary>
    /// Advances the timer by one clock step spanning <paramref name="stepPeriods"/> step periods: it adds that many eighths
    /// of a period, formed within one period. <c>ChecksRequests</c> is whether the sum reached a full period;
    /// <c>Next</c> keeps the remainder.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stepPeriods"/> is negative.</exception>
    public SoundRequestCheckTimerStep Advance(int stepPeriods)
    {
        int increment = Ensure.AtLeastZero(stepPeriods) * SoundClockTiming.CheckTimerIncrement % Period;
        int sum = Phase + increment;
        return new SoundRequestCheckTimerStep(new SoundRequestCheckTimer(sum % Period), sum >= Period);
    }
}
