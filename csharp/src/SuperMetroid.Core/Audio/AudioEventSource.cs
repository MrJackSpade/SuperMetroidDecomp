namespace SuperMetroid.Core.Audio;

/// <summary>
/// Turns one update's <see cref="AudioRequest"/>s into ordered semantic <see cref="AudioEvent"/>s through the
/// <see cref="SoundAcceptanceClock{TSound}"/> (#1304, #1410): data set changes and track selections first, then the update's
/// clock steps, each step that checks the requests followed by the sounds it starts. It also publishes the
/// acknowledgements the next update's sound handlers read. The game session and the offline clip renderer (#1305)
/// share it, so both drive a backend with identical event streams.
/// </summary>
internal sealed class AudioEventSource
{
    /// <summary>Turns updates' requests into events on <paramref name="clock"/>, which its owner created with the state's lifetime.</summary>
    /// <param name="clock">The clock that decides when each library accepts and acknowledges a request: the game session's, or an offline renderer's own.</param>
    internal AudioEventSource(SoundAcceptanceClock<SoundEffectId> clock) => this.clock = Ensure.NotNull(clock);

    /// <summary>The clock that decides when each library accepts and acknowledges a request.</summary>
    private readonly SoundAcceptanceClock<SoundEffectId> clock;

    /// <summary>The acknowledgements published by the most recent update, for the next one.</summary>
    internal SoundAcknowledgements<SoundEffectId> Acknowledgements { get; private set; }

    /// <summary>
    /// Applies <paramref name="requests"/> in order, runs the clock for one update and publishes the next update's
    /// acknowledgements.
    /// </summary>
    /// <returns>The update's ordered semantic events (see <see cref="AudioFrameEvents"/>).</returns>
    /// <exception cref="InvalidDataException">A request kind is unknown.</exception>
    internal AudioEvent[] Advance(IReadOnlyList<AudioRequest> requests)
    {
        Ensure.NotNull(requests);
        var events = new List<AudioEvent>(requests.Count + SoundClockTiming.MaximumStepsPerUpdate);
        foreach (AudioRequest request in requests)
        {
            switch (request)
            {
                case AudioRequest.ChangeMusicDataSet change:
                    clock.PauseForMusicLoad();
                    events.Add(new AudioEvent.ChangeMusicDataSet(change.DataSet));
                    break;
                case AudioRequest.PlayMusicTrack play:
                    events.Add(new AudioEvent.SelectMusicTrack(play.Track));
                    break;
                case AudioRequest.StopMusic:
                    events.Add(new AudioEvent.SelectMusicTrack(null));
                    break;
                case AudioRequest.RequestSound sound:
                    clock.Request(SoundEffectLibraries.ToQueueIndex(sound.Sound.Library), sound.Sound);
                    break;
                case AudioRequest.ClearSoundRequest clear:
                    clock.Request(SoundEffectLibraries.ToQueueIndex(clear.Library), null);
                    break;
                default:
                    throw new InvalidDataException($"Unknown audio request {request}.");
            }
        }
        Span<SoundClockStep<SoundEffectId>> schedule = stackalloc SoundClockStep<SoundEffectId>[SoundClockTiming.MaximumStepsPerUpdate];
        int stepCount = clock.Advance(schedule);
        foreach (SoundClockStep<SoundEffectId> step in schedule[..stepCount])
        {
            events.Add(new AudioEvent.ClockStep(step.SampleOffset, step.Steps, step.ChecksRequests));
            if (!step.ChecksRequests)
                continue;
            for (int library = 0; library < SoundEffectLibraries.Count; library++)
                AddStart(events, step, step.Checks[library]);
        }
        Acknowledgements = clock.Acknowledgements;
        return [.. events];
    }

    /// <summary>A library starts a sound only for a changed request, other than none, taken at the previous check.</summary>
    private static void AddStart(List<AudioEvent> events, SoundClockStep<SoundEffectId> step, SoundRequestCheck<SoundEffectId> check)
    {
        if (check.Changed && check.Request is SoundEffectId sound)
            events.Add(new AudioEvent.StartSoundEffect(sound, step.SampleOffset));
    }
}
