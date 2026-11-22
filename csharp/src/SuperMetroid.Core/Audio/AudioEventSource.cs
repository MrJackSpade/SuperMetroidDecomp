namespace SuperMetroid.Core.Audio;

/// <summary>
/// Turns one update's <see cref="AudioRequest"/>s into ordered semantic <see cref="AudioEvent"/>s through the
/// <see cref="SoundAcceptanceClock{TLibrary, TSound}"/> (#1304, #1410): data set changes and track selections first, then the update's
/// clock steps, each step that checks the requests followed by the sounds it starts. It also publishes the
/// acknowledgements the next update's sound queues read. The game session and the session that records clips offline
/// (#1305) share it, so both drive a backend with identical event streams.
/// </summary>
internal sealed partial class AudioEventSource
{
    /// <summary>Turns updates' requests into events on <paramref name="clock"/>, which its owner created with the state's lifetime.</summary>
    /// <param name="clock">The clock that decides when each library accepts and acknowledges a request: the game session's, or the offline clip recording's own.</param>
    internal AudioEventSource(SoundAcceptanceClock<SoundEffectLibrary, SoundEffectId> clock) => this.clock = Ensure.NotNull(clock);

    /// <summary>The clock that decides when each library accepts and acknowledges a request.</summary>
    private readonly SoundAcceptanceClock<SoundEffectLibrary, SoundEffectId> clock;

    /// <summary>The acknowledgements published by the most recent update, for the next one.</summary>
    internal SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> Acknowledgements { get; private set; }

    /// <summary>
    /// Applies <paramref name="requests"/> in order, runs the clock for one update and publishes the next update's
    /// acknowledgements.
    /// </summary>
    /// <returns>The update's ordered semantic events (see <see cref="AudioFrameEvents"/>).</returns>
    internal AudioEvent[] Advance(IReadOnlyList<AudioRequest> requests)
    {
        Ensure.NotNull(requests);
        var events = new List<AudioEvent>(requests.Count + SoundClockTiming.MaximumStepsPerUpdate);
        var applying = new RequestApplying(clock, events);
        foreach (AudioRequest request in requests)
            request.React(RequestReactions, applying);
        var schedule = new SoundClockStep<SoundEffectLibrary, SoundEffectId>[SoundClockTiming.MaximumStepsPerUpdate];
        int stepCount = clock.Advance(schedule);
        foreach (SoundClockStep<SoundEffectLibrary, SoundEffectId> step in schedule.AsSpan(0, stepCount))
        {
            events.Add(new AudioEvent.ClockStep(step.SampleOffset, step.Steps, step.ChecksRequests));
            if (step.Checks is not { } checks)
                continue;
            // The clock checks the libraries in queue order, so the starts follow it.
            foreach (SoundEffectLibrary library in SoundEffectLibraries.InQueueOrder)
                AddStart(events, step, checks[library]);
        }
        Acknowledgements = clock.Acknowledgements;
        return [.. events];
    }

    /// <summary>
    /// What an update does with each request (#2553): a data-set change or the shared sounds' install pauses the clock for the load and is published, a
    /// track or a stop is published as the track selected, and a sound request or its withdrawal goes to the clock.
    /// </summary>
    private static AudioRequestReactions<RequestApplying> RequestReactions { get; } = new(
        PlayMusicTrack: static (play, applying) => applying.Events.Add(new AudioEvent.SelectMusicTrack(play.Track)),
        StopMusic: static (_, applying) => applying.Events.Add(new AudioEvent.StopMusic()),
        ChangeMusicDataSet: static (change, applying) =>
        {
            applying.Clock.PauseForMusicLoad();
            applying.Events.Add(new AudioEvent.ChangeMusicDataSet(change.DataSet));
        },
        InstallSharedSounds: static (_, applying) =>
        {
            applying.Clock.PauseForMusicLoad();
            applying.Events.Add(new AudioEvent.InstallSharedSounds());
        },
        RequestSound: static (sound, applying) => applying.Clock.Request(sound.Sound.Library, new RequestedSound<SoundEffectId>(sound.Sound)),
        ClearSoundRequest: static (clear, applying) => applying.Clock.Request(clear.Library, null));

    /// <summary>An update applying its requests (#2553): the clock they reach and the events it publishes.</summary>
    /// <param name="Clock">The sound acceptance clock.</param>
    /// <param name="Events">The update's events so far.</param>
    private readonly partial record struct RequestApplying(SoundAcceptanceClock<SoundEffectLibrary, SoundEffectId> Clock, List<AudioEvent> Events);

    /// <summary>A library starts a sound only for a changed request, other than none, taken at the previous check.</summary>
    private static void AddStart(List<AudioEvent> events, SoundClockStep<SoundEffectLibrary, SoundEffectId> step, SoundRequestCheck<SoundEffectId> check)
    {
        if (check.Changed && check.Request?.Sound is SoundEffectId sound)
            events.Add(new AudioEvent.StartSoundEffect(sound, step.SampleOffset));
    }
}
