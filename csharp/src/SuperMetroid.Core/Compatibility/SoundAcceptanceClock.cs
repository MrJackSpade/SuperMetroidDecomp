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
/// audio advance, so its requests, acknowledgements and request-check phase live as long as the session; the offline clip
/// recording owns one of its own. Advanced once per update, after the update's sound requests, by the audio event source
/// it is injected into; its steps also schedule the sound backend's sequencer. Evidence: the sound program's timer
/// service and request read (<c>$1621</c>), confirmed against the emulated sound program and the SMV recordings'
/// acknowledgement traces.
/// </remarks>
[CompatibilityService("when each sound library accepts a request, and the acknowledgement the game reads back")]
public sealed class SoundAcceptanceClock<TLibrary, TSound>
    where TLibrary : struct, Enum
    where TSound : notnull, IEquatable<TSound>
{
    /// <summary>The libraries, distinct, in the order each request check serves them; every array below follows it.</summary>
    private readonly ImmutableArray<TLibrary> libraries;
    /// <summary>The library that reports a music load in progress instead of an acknowledgement.</summary>
    private readonly TLibrary musicLoadReporter;
    /// <summary>The game's most recent request to each library; none requests nothing.</summary>
    private readonly RequestedSound<TSound>?[] requested;
    /// <summary>What each library reports back as acknowledged.</summary>
    private readonly SoundAcknowledgement<TSound>[] acknowledged;
    /// <summary>The request each library took at its most recent check, which the next check acts on.</summary>
    private readonly RequestedSound<TSound>?[] taken;
    /// <summary>The request each library acted on at its most recent check, for detecting a change.</summary>
    private readonly RequestedSound<TSound>?[] actedOn;
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
        requested = new RequestedSound<TSound>?[libraries.Length];
        acknowledged = new SoundAcknowledgement<TSound>[libraries.Length];
        taken = new RequestedSound<TSound>?[libraries.Length];
        actedOn = new RequestedSound<TSound>?[libraries.Length];
    }

    /// <summary>What each library reports back as acknowledged.</summary>
    public SoundAcknowledgements<TLibrary, TSound> Acknowledgements =>
        new(libraries, library => acknowledged[PositionOf(library)]);

    /// <summary>Records the game's current request to <paramref name="library"/>; none withdraws it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The clock was not given <paramref name="library"/>.</exception>
    public void Request(TLibrary library, RequestedSound<TSound>? request) => requested[PositionOf(library)] = request;

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
        RequestedSound<TSound>? previous = actedOn[position];
        RequestedSound<TSound>? request = taken[position];
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

    /// <summary>
    /// The clock's reports by library: one value for each of a set of the clock's libraries, what each reports back
    /// (<see cref="SoundAcknowledgements{TLibrary, TSound}"/>) or what each one's request check acted on
    /// (<see cref="SoundRequestChecks{TLibrary, TSound}"/>). A library the set does not hold, and every library of the
    /// default value, reads as the value's default, so two sets are equal when every library reads the same in both.
    /// </summary>
    /// <typeparam name="TValue">What each library holds.</typeparam>
    internal readonly struct ByLibrary<TValue> : IEquatable<ByLibrary<TValue>>
        where TValue : struct, IEquatable<TValue>
    {
        /// <summary>The libraries the set holds, distinct; default for the default value, which holds none.</summary>
        private readonly ImmutableArray<TLibrary> held;

        /// <summary>Each held library's value, in the order of <see cref="held"/>; default exactly when the libraries are.</summary>
        private readonly ImmutableArray<TValue> values;

        /// <summary>Holds <paramref name="valueOf"/> of each of <paramref name="libraries"/>.</summary>
        /// <param name="libraries">The libraries, distinct.</param>
        /// <param name="valueOf">Each library's value, asked once per library in the order of <paramref name="libraries"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="valueOf"/> is null.</exception>
        public ByLibrary(ImmutableArray<TLibrary> libraries, Func<TLibrary, TValue> valueOf)
        {
            held = RequireDistinct(libraries);
            values = [.. libraries.Select(Ensure.NotNull(valueOf))];
        }

        /// <summary>The value of <paramref name="library"/>; the default value for a library the set does not hold.</summary>
        public TValue this[TLibrary library] => PositionIn(library) is int position ? values[position] : default;

        /// <summary>Two sets are equal when every library either holds reads the same in both.</summary>
        public bool Equals(ByLibrary<TValue> other) => ReadsSameIn(other) && other.ReadsSameIn(this);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ByLibrary<TValue> other && Equals(other);

        /// <summary>
        /// Combines each library's hash with its value's, for every library whose value is not the default; unordered, and
        /// blind to default values, as the equality is.
        /// </summary>
        public override int GetHashCode() => Held().Where(entry => !entry.Value.Equals(default))
            .Aggregate(0, (hash, entry) => hash ^ HashCode.Combine(entry.Key, entry.Value));

        /// <inheritdoc />
        public override string ToString() => $"[{string.Join(", ", Held().Select(entry => $"{entry.Key}: {entry.Value}"))}]";

        /// <summary>Each library the set holds with its value, in order; none for the default value.</summary>
        private IEnumerable<KeyValuePair<TLibrary, TValue>> Held() => held.IsDefault
            ? []
            : held.Zip(values, (library, value) => new KeyValuePair<TLibrary, TValue>(library, value));

        /// <summary>Where the set holds <paramref name="library"/>'s value, or null when it holds none.</summary>
        private int? PositionIn(TLibrary library) => !held.IsDefault && held.IndexOf(library) is var position and >= 0 ? position : null;

        /// <summary>Whether every library this set holds reads the same in <paramref name="other"/>.</summary>
        private bool ReadsSameIn(ByLibrary<TValue> other) =>
            Held().All(entry => entry.Value.Equals(other[entry.Key]));

        /// <summary>Returns <paramref name="libraries"/> when it holds each library once.</summary>
        /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
        private static ImmutableArray<TLibrary> RequireDistinct(ImmutableArray<TLibrary> libraries) =>
            libraries.IsDefault ? throw new ArgumentException("The sound libraries are required.", nameof(libraries))
            : libraries.Distinct().Count() != libraries.Length ? throw new ArgumentException("Each sound library is listed once.", nameof(libraries))
            : libraries;
    }
}
