namespace SuperMetroid.Core.Audio;

/// <summary>
/// Turns one update's <see cref="AudioRequest"/>s into ordered semantic <see cref="AudioEvent"/>s through the
/// <see cref="SoundAcceptanceClock"/> (#1304, #1410): data set changes and track selections first, then the update's
/// clock steps, each step that checks the requests followed by the sounds it starts. It also publishes the
/// acknowledgements the next update's sound handlers read. The game session and the offline clip renderer (#1305)
/// share it, so both drive a backend with identical event streams.
/// </summary>
internal sealed class CartridgeAudioEventSource
{
    /// <summary>The clock that decides when each library accepts and acknowledges a request.</summary>
    private readonly SoundAcceptanceClock clock = new();

    /// <summary>The acknowledgements published by the most recent update, for the next one.</summary>
    internal SoundAcknowledgements Acknowledgements { get; private set; }

    /// <summary>
    /// Applies <paramref name="requests"/> in order, runs the clock for one update and publishes the next update's
    /// acknowledgements.
    /// </summary>
    /// <returns>The update's ordered semantic events (see <see cref="AudioFrameEvents"/>).</returns>
    /// <exception cref="InvalidDataException">A request kind is unknown.</exception>
    internal AudioEvent[] Advance(IReadOnlyList<AudioRequest> requests)
    {
        Ensure.NotNull(requests);
        var events = new List<AudioEvent>(requests.Count + SoundAcceptanceClock.MaximumStepsPerUpdate);
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
                    clock.Request(SoundEffectLibraries.ToQueueIndex(sound.Sound.Library), sound.Sound.Value);
                    break;
                case AudioRequest.ClearSoundRequest clear:
                    clock.Request(SoundEffectLibraries.ToQueueIndex(clear.Library), 0);
                    break;
                default:
                    throw new InvalidDataException($"Unknown audio request {request}.");
            }
        }
        Span<SoundClockStep> schedule = stackalloc SoundClockStep[SoundAcceptanceClock.MaximumStepsPerUpdate];
        int stepCount = clock.Advance(schedule);
        foreach (SoundClockStep step in schedule[..stepCount])
        {
            events.Add(new AudioEvent.ClockStep(step.SampleOffset, step.Steps, step.ChecksRequests));
            if (!step.ChecksRequests)
                continue;
            for (int library = 0; library < SoundEffectLibraries.Count; library++)
                AddStart(events, step, SoundEffectLibraries.FromQueueIndex(library), step.Checks[library]);
        }
        Acknowledgements = clock.Acknowledgements;
        return [.. events];
    }

    /// <summary>A library starts a sound only for a changed request, other than none, taken at the previous check.</summary>
    private static void AddStart(List<AudioEvent> events, SoundClockStep step, SoundEffectLibrary library, SoundRequestCheck check)
    {
        if (check.Changed && check.Request != 0)
            events.Add(new AudioEvent.StartSoundEffect(new SoundEffectId(library, check.Request), step.SampleOffset));
    }
}
