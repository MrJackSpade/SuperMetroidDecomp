namespace SuperMetroid.Core.Audio;

/// <summary>
/// Turns one update's cartridge uploads and port writes into ordered semantic
/// <see cref="AudioEvent"/>s through the synthesis-free <see cref="SoundAcceptanceClock"/> (#1304):
/// bank loads and music-port commands first, then the update's driver ticks, each polling tick
/// followed by the sound-effect starts the driver acts on. It also publishes the acknowledgement
/// bytes the next update's sound handlers read. The game session and the offline clip renderer
/// (#1305) share it, so both drive a backend with identical event streams.
/// </summary>
internal sealed class CartridgeAudioEventSource
{
    /// <summary>The clock that decides when each library accepts and acknowledges a request.</summary>
    private readonly SoundAcceptanceClock clock = new();

    /// <summary>The acknowledgement bytes published by the most recent update, for the next one.</summary>
    internal SoundAcknowledgements Acknowledgements { get; private set; }

    /// <summary>
    /// Applies <paramref name="commands"/> in order, walks the driver timer for one update and
    /// publishes the next update's acknowledgements.
    /// </summary>
    /// <returns>The update's ordered semantic events (see <see cref="AudioFrameEvents"/>).</returns>
    /// <exception cref="InvalidDataException">A command kind is unknown.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A port write selects a port outside 0..3.</exception>
    internal AudioEvent[] Advance(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        Ensure.NotNull(commands);
        var events = new List<AudioEvent>(commands.Count + SoundAcceptanceClock.MaximumStepsPerUpdate);
        foreach (CartridgeAudioCommand command in commands)
        {
            switch (command)
            {
                case CartridgeAudioCommand.Upload upload:
                    clock.PauseForMusicLoad();
                    events.Add(new AudioEvent.LoadAudioBank(upload.Bank));
                    break;
                case CartridgeAudioCommand.WritePort writePort:
                    if (writePort.InputPort == ApuPort.Music)
                        events.Add(new AudioEvent.WriteMusicPort(MusicCommand.FromCartridge(writePort.Value)));
                    else
                        clock.Request(ApuPorts.LibraryIndexAt(writePort.InputPort), writePort.Value);
                    break;
                default:
                    throw new InvalidDataException($"Unknown cartridge audio command {command}.");
            }
        }
        Span<SoundClockStep> schedule = stackalloc SoundClockStep[SoundAcceptanceClock.MaximumStepsPerUpdate];
        int tickCount = clock.Advance(schedule);
        foreach (SoundClockStep tick in schedule[..tickCount])
        {
            events.Add(new AudioEvent.DriverTick(tick.SampleOffset, tick.Steps, tick.ChecksRequests));
            if (!tick.ChecksRequests)
                continue;
            for (int library = 0; library < SoundEffectLibraries.Count; library++)
                AddStart(events, tick, SoundEffectLibraries.FromQueueIndex(library), tick.Checks[library]);
        }
        Acknowledgements = clock.Acknowledgements;
        return [.. events];
    }

    /// <summary>The driver starts a sound only for a changed nonzero request latched at the previous poll.</summary>
    private static void AddStart(List<AudioEvent> events, SoundClockStep tick, SoundEffectLibrary library, SoundRequestCheck poll)
    {
        if (poll.Changed && poll.Request != 0)
            events.Add(new AudioEvent.StartSoundEffect(new SoundEffectId(library, poll.Request), tick.SampleOffset));
    }
}
