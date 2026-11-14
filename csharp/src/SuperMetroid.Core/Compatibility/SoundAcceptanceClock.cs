using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// When each sound library accepts the game's sound requests, and which request it reports back as acknowledged. The
/// music player checks every library's request on its own clock, every 512 of its 32,040-per-second
/// samples, so a check falls a little earlier in each successive update. A check serves the libraries in a fixed order;
/// for each it acts on the request it took at the previous check, reports that request back as acknowledged, and takes
/// the game's current request; a request replaced before a check is never seen. The game's sound queues compare the
/// acknowledgement with the request they are waiting on, so when a request is accepted changes which sounds play and
/// when the queues move on to the next.
/// </summary>
/// <typeparam name="TLibrary">A sound library; the clock finds a library by equality and never reads its value.</typeparam>
/// <typeparam name="TSound">The sound a library is asked for; the clock only compares requests.</typeparam>
/// <remarks>
/// Created once per game session by its composition root (<c>FrontendSession</c>), which supplies the game's libraries in
/// the order each check serves them and names the library that reports a music load, and injects it into the session's
/// audio advance, so its requests, acknowledgements and request-check phase live as long as the session; an offline clip
/// renderer owns one of its own. Advanced once per update, after the update's sound requests, by the audio event source
/// it is injected into; its steps also schedule the sound backend's sequencer. Evidence: the sound program's timer
/// service and request read (<c>$1621</c>), confirmed against the emulated sound program and the SMV recordings'
/// acknowledgement traces.
/// </remarks>
[CompatibilityService("when each sound library accepts a request, and the acknowledgement the game reads back")]
public sealed class SoundAcceptanceClock<TLibrary, TSound>
    where TLibrary : struct, Enum
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>The libraries, distinct, in the order each request check serves them; every array below follows it.</summary>
    private readonly ImmutableArray<TLibrary> libraries;
    /// <summary>The library that reports a music load in progress instead of an acknowledgement.</summary>
    private readonly TLibrary musicLoadReporter;
    /// <summary>The game's most recent request to each library; none requests nothing.</summary>
    private readonly TSound?[] requested;
    /// <summary>What each library reports back as acknowledged.</summary>
    private readonly SoundAcknowledgement<TSound>[] acknowledged;
    /// <summary>The request each library took at its most recent check, which the next check acts on.</summary>
    private readonly TSound?[] taken;
    /// <summary>The request each library acted on at its most recent check, for detecting a change.</summary>
    private readonly TSound?[] actedOn;
    /// <summary>
    /// Samples elapsed since the last step, 0 through <see cref="SoundClockTiming.SamplesPerStep"/>; a step that falls
    /// exactly on an update boundary runs at the start of the next update.
    /// </summary>
    private int samplesSinceStep;
    /// <summary>The request-check timer: each step adds an eighth of its period and its overflow checks the requests.</summary>
    private SoundRequestCheckTimer checkTimer;

    /// <summary>A clock at power-on: no requests, no acknowledgements, and the request-check timer at its start.</summary>
    /// <param name="libraries">The libraries, distinct, in the order each request check serves them.</param>
    /// <param name="musicLoadReporter">The library that reports a music load in progress instead of an acknowledgement.</param>
    /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="musicLoadReporter"/> is not one of <paramref name="libraries"/>.</exception>
    public SoundAcceptanceClock(ImmutableArray<TLibrary> libraries, TLibrary musicLoadReporter)
    {
        if (libraries.IsDefault)
            throw new ArgumentException("The sound libraries are required.", nameof(libraries));
        if (libraries.Distinct().Count() != libraries.Length)
            throw new ArgumentException("Each sound library is listed once.", nameof(libraries));
        if (!libraries.Contains(musicLoadReporter))
            throw new ArgumentOutOfRangeException(nameof(musicLoadReporter), musicLoadReporter, "The music-load reporter must be one of the libraries.");
        this.libraries = libraries;
        this.musicLoadReporter = musicLoadReporter;
        requested = new TSound?[libraries.Length];
        acknowledged = new SoundAcknowledgement<TSound>[libraries.Length];
        taken = new TSound?[libraries.Length];
        actedOn = new TSound?[libraries.Length];
    }

    /// <summary>What each library reports back as acknowledged.</summary>
    public SoundAcknowledgements<TLibrary, TSound> Acknowledgements =>
        new(libraries, library => acknowledged[PositionOf(library)]);

    /// <summary>Records the game's current request to <paramref name="library"/>; none withdraws it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The clock was not given <paramref name="library"/>.</exception>
    public void Request(TLibrary library, TSound? request) => requested[PositionOf(library)] = request;

    /// <summary>
    /// Loading new music stops the music player: the music-load reporter reports the load in progress instead of an
    /// acknowledgement and every pending request is dropped. Requests already taken, and the clock's phase, are kept.
    /// </summary>
    public void PauseForMusicLoad()
    {
        acknowledged[PositionOf(musicLoadReporter)] = SoundAcknowledgement<TSound>.MusicLoading;
        Array.Clear(requested);
    }

    /// <summary>Runs one update's <see cref="SoundClockTiming.SamplesPerUpdate"/> samples, recording every step and request check in order.</summary>
    /// <param name="steps">Receives the update's steps; needs <see cref="SoundClockTiming.MaximumStepsPerUpdate"/> entries.</param>
    /// <returns>The number of steps written to <paramref name="steps"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="steps"/> is too short.</exception>
    /// <exception cref="InvalidDataException">The clock holds a phase no update can leave.</exception>
    public int Advance(Span<SoundClockStep<TLibrary, TSound>> steps)
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

    /// <summary>Runs one step at <paramref name="sampleOffset"/>, advancing the check timer and checking every library, in order, on its overflow.</summary>
    private SoundClockStep<TLibrary, TSound> Step(int sampleOffset, int periods)
    {
        (checkTimer, bool checks) = checkTimer.Advance(periods);
        if (!checks)
            return new SoundClockStep<TLibrary, TSound>(sampleOffset, periods, ChecksRequests: false, default);
        return new SoundClockStep<TLibrary, TSound>(sampleOffset, periods, ChecksRequests: true,
            new SoundRequestChecks<TLibrary, TSound>(libraries, Check));
    }

    /// <summary>One library's check: it acts on the request taken at the previous check, acknowledges it and takes the current one.</summary>
    private SoundRequestCheck<TSound> Check(TLibrary library)
    {
        int position = PositionOf(library);
        TSound? previous = actedOn[position];
        TSound? request = taken[position];
        actedOn[position] = request;
        acknowledged[position] = SoundAcknowledgement<TSound>.Of(request);
        taken[position] = requested[position];
        return new SoundRequestCheck<TSound>(request, Changed: !Nullable.Equals(request, previous));
    }

    /// <summary>Where the clock keeps <paramref name="library"/>'s state.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The clock was not given <paramref name="library"/>.</exception>
    private int PositionOf(TLibrary library)
    {
        for (int position = 0; position < libraries.Length; position++)
        {
            if (EqualityComparer<TLibrary>.Default.Equals(libraries[position], library))
                return position;
        }
        throw new ArgumentOutOfRangeException(nameof(library), library, "The sound clock was not given this library.");
    }
}
