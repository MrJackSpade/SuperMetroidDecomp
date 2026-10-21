using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// When each sound library accepts the game's sound requests, and which request it reports back as acknowledged. The
/// original sound program checked the three libraries' requests on its own clock, every 512 of its 32,040-per-second
/// samples, so a check falls a little earlier in each successive update. A check acts on the request it took at the
/// previous check, reports that request back as acknowledged, and takes the game's current request; a request replaced
/// before a check is never seen. The game's sound handlers compare the acknowledgement with the request they are waiting
/// on, so when a request is accepted changes which sounds play and when handlers move on to the next.
/// </summary>
/// <remarks>
/// Advanced once per update, after the update's sound requests, by the session's audio event source; its steps also
/// schedule the sound backend's sequencer. Evidence: the sound program's timer service and request read (<c>$1621</c>),
/// confirmed against the emulated sound program and the SMV recordings' acknowledgement traces.
/// </remarks>
[CompatibilityService("when each sound library accepts a request, and the acknowledgement the game reads back")]
public sealed class SoundAcceptanceClock
{
    /// <summary>The sound libraries the clock accepts requests for, addressed by zero-based index.</summary>
    public const int LibraryCount = 3;

    /// <summary>The sound clock's samples in one 60-Hz update: 32,040 / 60.</summary>
    public const int SamplesPerUpdate = 534;

    /// <summary>Samples between clock steps (the sound program's 500-Hz timer).</summary>
    public const int SamplesPerStep = 64;

    /// <summary>The most steps one update can contain: one pending at its start plus one per whole step period.</summary>
    public const int MaximumStepsPerUpdate = SamplesPerUpdate / SamplesPerStep + 1;

    /// <summary>Added to the request-check timer on every step; its overflow, every eighth step, checks the requests.</summary>
    public const int CheckTimerIncrement = TickAccumulator.Period / 8;

    /// <summary>The acknowledgement library one reports while new music loads, which no request can produce.</summary>
    public const int MusicLoadAcknowledgement = 187;

    /// <summary>The game's most recent request to each library, 0 through 255; zero requests nothing.</summary>
    private PerLibrary requested;
    /// <summary>The request each library reports back as acknowledged.</summary>
    private PerLibrary acknowledged;
    /// <summary>The request each library took at its most recent check, which the next check acts on.</summary>
    private PerLibrary taken;
    /// <summary>The request each library acted on at its most recent check, for detecting a change.</summary>
    private PerLibrary actedOn;
    /// <summary>
    /// Samples elapsed since the last step, 0 through <see cref="SamplesPerStep"/>; a step that falls exactly on an
    /// update boundary runs at the start of the next update.
    /// </summary>
    private int samplesSinceStep;
    /// <summary>The request-check timer: each step adds an eighth of its period and its overflow checks the requests.</summary>
    private TickAccumulator checkTimer;

    /// <summary>The request each library reports back as acknowledged.</summary>
    public SoundAcknowledgements Acknowledgements => new(acknowledged[0], acknowledged[1], acknowledged[2]);

    /// <summary>Records the game's current request to the library at zero-based <paramref name="library"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="library"/> is outside 0 through <see cref="LibraryCount"/> - 1, or <paramref name="request"/> is outside 0 through 255.
    /// </exception>
    public void Request(int library, int request) =>
        requested[Ensure.BetweenInclusive(0, LibraryCount - 1, library)] = Ensure.BetweenInclusive(0, byte.MaxValue, request);

    /// <summary>
    /// Loading new music stops the sound program: library one reports the load in progress instead of an acknowledgement
    /// and every pending request is dropped. Requests already taken, and the clock's phase, are kept.
    /// </summary>
    public void PauseForMusicLoad()
    {
        acknowledged[0] = MusicLoadAcknowledgement;
        for (int library = 0; library < LibraryCount; library++)
            requested[library] = 0;
    }

    /// <summary>Runs one update's <see cref="SamplesPerUpdate"/> samples, recording every step and request check in order.</summary>
    /// <param name="steps">Receives the update's steps; needs <see cref="MaximumStepsPerUpdate"/> entries.</param>
    /// <returns>The number of steps written to <paramref name="steps"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="steps"/> is too short.</exception>
    /// <exception cref="InvalidDataException">The clock holds a phase no update can leave.</exception>
    public int Advance(Span<SoundClockStep> steps)
    {
        if (steps.Length < MaximumStepsPerUpdate)
            throw new ArgumentException($"The sound clock requires {MaximumStepsPerUpdate} step entries.", nameof(steps));
        if (samplesSinceStep is < 0 or > SamplesPerStep)
            throw new InvalidDataException($"The sound clock holds impossible phase {samplesSinceStep}.");

        int count = 0;
        int sample = 0;
        while (sample < SamplesPerUpdate)
        {
            if (samplesSinceStep >= SamplesPerStep)
            {
                steps[count++] = Step(sample, samplesSinceStep / SamplesPerStep);
                samplesSinceStep %= SamplesPerStep;
            }

            // Each stride stops at the next step, so the elapsed samples never pass one step period.
            int samples = Math.Min(SamplesPerUpdate - sample, SamplesPerStep - samplesSinceStep);
            samplesSinceStep += samples;
            sample += samples;
        }
        return count;
    }

    /// <summary>Runs one step at <paramref name="sampleOffset"/>, advancing the check timer and checking every library on its overflow.</summary>
    private SoundClockStep Step(int sampleOffset, int periods)
    {
        // The sound program forms the step's timer increment in eight bits before adding it.
        (checkTimer, bool checks) = checkTimer.Advance(periods * CheckTimerIncrement % TickAccumulator.Period);
        if (!checks)
            return new SoundClockStep(sampleOffset, periods, ChecksRequests: false, default);
        SoundRequestChecks libraryChecks = default;
        for (int library = 0; library < LibraryCount; library++)
            libraryChecks[library] = Check(library);
        return new SoundClockStep(sampleOffset, periods, ChecksRequests: true, libraryChecks);
    }

    /// <summary>One library's check: it acts on the request taken at the previous check, acknowledges it and takes the current one.</summary>
    private SoundRequestCheck Check(int library)
    {
        int previous = actedOn[library];
        int request = taken[library];
        actedOn[library] = request;
        acknowledged[library] = request;
        taken[library] = requested[library];
        return new SoundRequestCheck(request, Changed: request != previous);
    }

    /// <summary>One value per sound library, indexed by zero-based library index.</summary>
    [InlineArray(LibraryCount)]
    private struct PerLibrary
    {
        /// <summary>Library one's value; the other libraries' follow it in queue order.</summary>
        private int first;
    }
}
