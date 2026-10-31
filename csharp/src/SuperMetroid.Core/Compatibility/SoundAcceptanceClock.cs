namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// When each sound library accepts the game's sound requests, and which request it reports back as acknowledged. The
/// original sound program checked the three libraries' requests on its own clock, every 512 of its 32,040-per-second
/// samples, so a check falls a little earlier in each successive update. A check acts on the request it took at the
/// previous check, reports that request back as acknowledged, and takes the game's current request; a request replaced
/// before a check is never seen. The game's sound handlers compare the acknowledgement with the request they are waiting
/// on, so when a request is accepted changes which sounds play and when handlers move on to the next.
/// </summary>
/// <typeparam name="TSound">The sound a library is asked for; the clock only compares requests.</typeparam>
/// <remarks>
/// Advanced once per update, after the update's sound requests, by the session's audio event source; its steps also
/// schedule the sound backend's sequencer. Evidence: the sound program's timer service and request read (<c>$1621</c>),
/// confirmed against the emulated sound program and the SMV recordings' acknowledgement traces.
/// </remarks>
[CompatibilityService("when each sound library accepts a request, and the acknowledgement the game reads back")]
public sealed class SoundAcceptanceClock<TSound>
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>The game's most recent request to each library; none requests nothing.</summary>
    private readonly TSound?[] requested = new TSound?[SoundClockTiming.LibraryCount];
    /// <summary>What each library reports back as acknowledged.</summary>
    private readonly SoundAcknowledgement<TSound>[] acknowledged = new SoundAcknowledgement<TSound>[SoundClockTiming.LibraryCount];
    /// <summary>The request each library took at its most recent check, which the next check acts on.</summary>
    private readonly TSound?[] taken = new TSound?[SoundClockTiming.LibraryCount];
    /// <summary>The request each library acted on at its most recent check, for detecting a change.</summary>
    private readonly TSound?[] actedOn = new TSound?[SoundClockTiming.LibraryCount];
    /// <summary>
    /// Samples elapsed since the last step, 0 through <see cref="SoundClockTiming.SamplesPerStep"/>; a step that falls
    /// exactly on an update boundary runs at the start of the next update.
    /// </summary>
    private int samplesSinceStep;
    /// <summary>The request-check timer: each step adds an eighth of its period and its overflow checks the requests.</summary>
    private SoundRequestCheckTimer checkTimer;

    /// <summary>What each library reports back as acknowledged.</summary>
    public SoundAcknowledgements<TSound> Acknowledgements => new(acknowledged[0], acknowledged[1], acknowledged[2]);

    /// <summary>Records the game's current request to the library at zero-based <paramref name="library"/>; none withdraws it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="library"/> is outside 0 through <see cref="SoundClockTiming.LibraryCount"/> - 1.</exception>
    public void Request(int library, TSound? request) =>
        requested[Ensure.BetweenInclusive(0, SoundClockTiming.LibraryCount - 1, library)] = request;

    /// <summary>
    /// Loading new music stops the sound program: library one reports the load in progress instead of an acknowledgement
    /// and every pending request is dropped. Requests already taken, and the clock's phase, are kept.
    /// </summary>
    public void PauseForMusicLoad()
    {
        acknowledged[0] = SoundAcknowledgement<TSound>.MusicLoading;
        Array.Clear(requested);
    }

    /// <summary>Runs one update's <see cref="SoundClockTiming.SamplesPerUpdate"/> samples, recording every step and request check in order.</summary>
    /// <param name="steps">Receives the update's steps; needs <see cref="SoundClockTiming.MaximumStepsPerUpdate"/> entries.</param>
    /// <returns>The number of steps written to <paramref name="steps"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="steps"/> is too short.</exception>
    /// <exception cref="InvalidDataException">The clock holds a phase no update can leave.</exception>
    public int Advance(Span<SoundClockStep<TSound>> steps)
    {
        if (steps.Length < SoundClockTiming.MaximumStepsPerUpdate)
            throw new ArgumentException($"The sound clock requires {SoundClockTiming.MaximumStepsPerUpdate} step entries.", nameof(steps));
        if (samplesSinceStep is < 0 or > SoundClockTiming.SamplesPerStep)
            throw new InvalidDataException($"The sound clock holds impossible phase {samplesSinceStep}.");

        int count = 0;
        int sample = 0;
        while (sample < SoundClockTiming.SamplesPerUpdate)
        {
            if (samplesSinceStep >= SoundClockTiming.SamplesPerStep)
            {
                steps[count++] = Step(sample, samplesSinceStep / SoundClockTiming.SamplesPerStep);
                samplesSinceStep %= SoundClockTiming.SamplesPerStep;
            }

            // Each stride stops at the next step, so the elapsed samples never pass one step period.
            int samples = Math.Min(SoundClockTiming.SamplesPerUpdate - sample, SoundClockTiming.SamplesPerStep - samplesSinceStep);
            samplesSinceStep += samples;
            sample += samples;
        }
        return count;
    }

    /// <summary>Runs one step at <paramref name="sampleOffset"/>, advancing the check timer and checking every library on its overflow.</summary>
    private SoundClockStep<TSound> Step(int sampleOffset, int periods)
    {
        (checkTimer, bool checks) = checkTimer.Advance(periods);
        if (!checks)
            return new SoundClockStep<TSound>(sampleOffset, periods, ChecksRequests: false, default);
        SoundRequestChecks<TSound> libraryChecks = default;
        for (int library = 0; library < SoundClockTiming.LibraryCount; library++)
            libraryChecks[library] = Check(library);
        return new SoundClockStep<TSound>(sampleOffset, periods, ChecksRequests: true, libraryChecks);
    }

    /// <summary>One library's check: it acts on the request taken at the previous check, acknowledges it and takes the current one.</summary>
    private SoundRequestCheck<TSound> Check(int library)
    {
        TSound? previous = actedOn[library];
        TSound? request = taken[library];
        actedOn[library] = request;
        acknowledged[library] = SoundAcknowledgement<TSound>.Of(request);
        taken[library] = requested[library];
        return new SoundRequestCheck<TSound>(request, Changed: !Nullable.Equals(request, previous));
    }
}
