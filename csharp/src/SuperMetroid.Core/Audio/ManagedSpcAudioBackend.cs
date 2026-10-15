namespace SuperMetroid.Core.Audio;

/// <summary>
/// The emulated SPC sequencer and S-DSP as an <see cref="IAudioBackend"/> (#1304): it installs
/// extracted banks, latches music commands, and runs the translated driver at the tick offsets
/// and sound-effect starts the session's <see cref="SpcCommandPorts"/> published. One owner must
/// call it in update order.
/// </summary>
public sealed class ManagedSpcAudioBackend : IAudioBackend
{
    /// <summary>Decoded cartridge uploads, instruments, music, sound programs, and sample banks.</summary>
    private readonly ExtractedAudioAssetCatalog assets;
    /// <summary>Reusable driver schedule rebuilt from each update's tick events.</summary>
    private readonly SpcDriverTick[] schedule = new SpcDriverTick[SpcCommandPorts.MaximumTicksPerFrame];

    /// <summary>Creates the backend over a catalog, continuing an existing player or creating a new one.</summary>
    /// <param name="assets">Upload, instrument, music, sound-program, and sample definitions installed by bank loads.</param>
    /// <param name="player">Existing player continued without reset, such as one restored with a debugger state; null creates a fresh player.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assets"/> is null.</exception>
    public ManagedSpcAudioBackend(ExtractedAudioAssetCatalog assets, ManagedSpcPlayer? player = null)
    {
        this.assets = Ensure.NotNull(assets);
        Player = player ?? new ManagedSpcPlayer();
    }

    /// <summary>The emulated driver this backend plays through.</summary>
    internal ManagedSpcPlayer Player { get; }

    /// <inheritdoc />
    /// <remarks>
    /// A bank load installs the opaque upload, then the decoded instrument, music, sound-effect
    /// and sample definitions. A sound-effect start is the driver acting on a changed nonzero
    /// latched request at that tick's poll; library priority still decides whether it plays.
    /// Polls without a start service each library's playing sound. Processing is not
    /// transactional: a failing event may leave earlier events applied.
    /// </remarks>
    public void Render(in AudioFrameEvents frame, Span<short> stereo48k)
    {
        IReadOnlyList<AudioEvent> events = Ensure.NotNull(frame.Events);
        int ticks = 0;
        foreach (AudioEvent audioEvent in events)
        {
            switch (audioEvent.Kind)
            {
                case AudioEventKind.LoadAudioBank:
                    RequireBeforeTicks(ticks, audioEvent);
                    LoadBank(audioEvent.Bank);
                    break;
                case AudioEventKind.WriteMusicPort:
                    RequireBeforeTicks(ticks, audioEvent);
                    Player.WriteMusicPort(unchecked((byte)audioEvent.Music.RawValue));
                    break;
                case AudioEventKind.DriverTick:
                    if (ticks == schedule.Length)
                        throw new InvalidDataException($"An update holds more than {schedule.Length} driver ticks.");
                    schedule[ticks++] = new SpcDriverTick(audioEvent.DspCycleOffset, audioEvent.Ticks,
                        audioEvent.SoundEffectsPolled, default, default, default);
                    break;
                case AudioEventKind.StartSoundEffect:
                    if (ticks == 0 || !schedule[ticks - 1].SoundEffectsPolled ||
                        schedule[ticks - 1].DspCycleOffset != audioEvent.DspCycleOffset)
                        throw new InvalidDataException($"{audioEvent} does not follow its polling driver tick.");
                    schedule[ticks - 1] = StartAt(schedule[ticks - 1], audioEvent.SoundEffect);
                    break;
                default:
                    throw new InvalidDataException($"Unknown audio event {audioEvent.Kind}.");
            }
        }
        Player.GenerateFrame(schedule.AsSpan(0, ticks), stereo48k);
    }

    /// <summary>Installs one bank's upload and its decoded definitions, as the cartridge upload does.</summary>
    private void LoadBank(AudioBank bank)
    {
        Player.Upload(assets.GetUpload(bank).Span);
        Player.ApplyInstrumentDefinitions(assets.GetInstrumentBank(bank));
        Player.ApplyMusicDefinitions(assets.GetMusicBank(bank));
        Player.ApplySoundEffectDefinitions(assets.SoundPrograms, assets.SoundLibraries);
        Player.SetSampleBank(assets.GetSampleBank(bank));
    }

    /// <summary>Bank loads and music commands precede the update's cycles; one after a tick is malformed.</summary>
    private static void RequireBeforeTicks(int ticks, AudioEvent audioEvent)
    {
        if (ticks != 0)
            throw new InvalidDataException($"{audioEvent} follows a driver tick; it must precede the update's ticks.");
    }

    /// <summary>Marks <paramref name="soundEffect"/>'s library as acting on a changed request at <paramref name="tick"/>'s poll.</summary>
    private static SpcDriverTick StartAt(SpcDriverTick tick, SoundEffectId soundEffect)
    {
        var start = new SpcSoundEffectPoll(soundEffect.Value, Changed: true);
        return SoundEffectLibraries.ToQueueIndex(soundEffect.Library) switch
        {
            0 => tick with { Library1 = start },
            1 => tick with { Library2 = start },
            _ => tick with { Library3 = start },
        };
    }
}
