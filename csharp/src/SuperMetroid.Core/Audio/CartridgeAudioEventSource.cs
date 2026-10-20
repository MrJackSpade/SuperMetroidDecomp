namespace SuperMetroid.Core.Audio;

/// <summary>
/// Turns one update's cartridge uploads and port writes into ordered semantic
/// <see cref="AudioEvent"/>s through the synthesis-free <see cref="SpcCommandPorts"/> (#1304):
/// bank loads and music-port commands first, then the update's driver ticks, each polling tick
/// followed by the sound-effect starts the driver acts on. It also publishes the acknowledgement
/// bytes the next update's sound handlers read. The game session and the offline clip renderer
/// (#1305) share it, so both drive a backend with identical event streams.
/// </summary>
internal sealed class CartridgeAudioEventSource
{
    /// <summary>SPC port model that owns the driver timer, sound-effect polls and acknowledgements.</summary>
    private readonly SpcCommandPorts ports = new();

    /// <summary>The acknowledgement bytes published by the most recent update, for the next one.</summary>
    /// <remarks>The music echo on port zero belongs to the sequencer and is never read by gameplay; it is published as zero.</remarks>
    internal CartridgeAudioAcknowledgements Acknowledgements { get; private set; }

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
        var events = new List<AudioEvent>(commands.Count + SpcCommandPorts.MaximumTicksPerFrame);
        foreach (CartridgeAudioCommand command in commands)
        {
            switch (command.Kind)
            {
                case CartridgeAudioCommandKind.Upload:
                    ports.ResetForUpload();
                    events.Add(AudioEvent.LoadAudioBank(command.Bank));
                    break;
                case CartridgeAudioCommandKind.WritePort:
                    ports.WritePort(command.InputPort, command.Value);
                    if (command.InputPort == ApuPort.Music)
                        events.Add(AudioEvent.WriteMusicPort(MusicCommand.FromCartridge(command.Value)));
                    break;
                default:
                    throw new InvalidDataException($"Unknown cartridge audio command {command.Kind}.");
            }
        }
        Span<SpcDriverTick> schedule = stackalloc SpcDriverTick[SpcCommandPorts.MaximumTicksPerFrame];
        int tickCount = ports.AdvanceFrame(schedule);
        foreach (SpcDriverTick tick in schedule[..tickCount])
        {
            events.Add(AudioEvent.DriverTick(tick.DspCycleOffset, tick.Ticks, tick.SoundEffectsPolled));
            if (!tick.SoundEffectsPolled)
                continue;
            for (int library = 0; library < SoundEffectLibraries.Count; library++)
                AddStart(events, tick, SoundEffectLibraries.FromQueueIndex(library), tick.LibraryPolls[library]);
        }
        Acknowledgements = new CartridgeAudioAcknowledgements(
            0,
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary1),
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary2),
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary3));
        return [.. events];
    }

    /// <summary>The driver starts a sound only for a changed nonzero request latched at the previous poll.</summary>
    private static void AddStart(List<AudioEvent> events, SpcDriverTick tick, SoundEffectLibrary library, SpcSoundEffectPoll poll)
    {
        if (poll.Changed && poll.Command != 0)
            events.Add(AudioEvent.StartSoundEffect(new SoundEffectId(library, poll.Command), tick.DspCycleOffset));
    }
}
