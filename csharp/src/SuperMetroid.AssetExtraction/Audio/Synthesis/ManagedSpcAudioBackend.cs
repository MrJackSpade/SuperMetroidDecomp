using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The emulated SPC sequencer and S-DSP driven by the session's audio events (#1304): it installs
/// extracted banks, latches music commands, and runs the translated driver at the tick offsets
/// and sound-effect starts the session's <see cref="SoundAcceptanceClock{TSound}"/> published. One owner must
/// call it in update order.
/// </summary>
public sealed class ManagedSpcAudioBackend
{
    /// <summary>Decoded cartridge uploads, instruments, music, sound programs, and sample banks.</summary>
    private readonly ExtractedAudioAssetCatalog assets;
    /// <summary>Reusable driver schedule rebuilt from each update's tick events.</summary>
    private readonly SoundClockStep<SoundEffectId>[] schedule = new SoundClockStep<SoundEffectId>[SoundClockTiming.MaximumStepsPerUpdate];

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

    /// <summary>Renders the update into native 32.04-kHz PCM, the offline clip renderer's output (#1305).</summary>
    /// <exception cref="InvalidDataException">An event is malformed or out of order, or the driver cannot synthesize it.</exception>
    internal void RenderNative(in AudioFrameEvents frame, Span<short> native) =>
        Player.GenerateNativeFrame(Schedule(frame), native);

    /// <summary>Applies the update to the driver without clocking the DSP; driver state evolves exactly as in <see cref="RenderNative"/> (#1305).</summary>
    /// <exception cref="InvalidDataException">An event is malformed or out of order.</exception>
    internal void AdvanceSequencer(in AudioFrameEvents frame) =>
        Player.AdvanceSequencer(Schedule(frame));

    /// <summary>Applies the update's bank loads and music commands, and builds its driver schedule with each sound-effect start at its poll.</summary>
    private ReadOnlySpan<SoundClockStep<SoundEffectId>> Schedule(in AudioFrameEvents frame)
    {
        IReadOnlyList<AudioEvent> events = Ensure.NotNull(frame.Events);
        int ticks = 0;
        foreach (AudioEvent audioEvent in events)
        {
            switch (audioEvent)
            {
                case AudioEvent.ChangeMusicDataSet change:
                    RequireBeforeTicks(ticks, audioEvent);
                    LoadBank(change.DataSet);
                    break;
                case AudioEvent.SelectMusicTrack selection:
                    RequireBeforeTicks(ticks, audioEvent);
                    Player.WriteMusicPort(checked((byte)(selection.Track?.Number ?? 0)));
                    break;
                case AudioEvent.ClockStep step:
                    if (ticks == schedule.Length)
                        throw new InvalidDataException($"An update holds more than {schedule.Length} driver ticks.");
                    schedule[ticks++] = new SoundClockStep<SoundEffectId>(step.SampleOffset, step.Steps, step.ChecksRequests, default);
                    break;
                case AudioEvent.StartSoundEffect startSoundEffect:
                    if (ticks == 0 || !schedule[ticks - 1].ChecksRequests ||
                        schedule[ticks - 1].SampleOffset != startSoundEffect.SampleOffset)
                        throw new InvalidDataException($"{audioEvent} does not follow its polling driver tick.");
                    schedule[ticks - 1] = StartAt(schedule[ticks - 1], startSoundEffect.SoundEffect);
                    break;
                default:
                    throw new InvalidDataException($"Unknown audio event {audioEvent}.");
            }
        }
        return schedule.AsSpan(0, ticks);
    }

    /// <summary>Installs one bank's upload and its decoded definitions, as the cartridge upload does.</summary>
    private void LoadBank(AudioBank bank)
    {
        Player.Uploads.Upload(assets.GetUpload(bank).Span);
        Player.Uploads.ApplyInstrumentDefinitions(assets.GetInstrumentBank(bank));
        Player.Uploads.ApplyMusicDefinitions(assets.GetMusicBank(bank));
        Player.Uploads.ApplySoundEffectDefinitions(assets.SoundPrograms, assets.SoundLibraries);
        Player.SetSampleBank(assets.GetSampleBank(bank));
    }

    /// <summary>Bank loads and music commands precede the update's cycles; one after a tick is malformed.</summary>
    private static void RequireBeforeTicks(int ticks, AudioEvent audioEvent)
    {
        if (ticks != 0)
            throw new InvalidDataException($"{audioEvent} follows a driver tick; it must precede the update's ticks.");
    }

    /// <summary>Marks <paramref name="soundEffect"/>'s library as acting on a changed request at <paramref name="tick"/>'s poll.</summary>
    private static SoundClockStep<SoundEffectId> StartAt(SoundClockStep<SoundEffectId> tick, SoundEffectId soundEffect)
    {
        SoundRequestCheck<SoundEffectId> start = new SoundRequestCheck<SoundEffectId>(soundEffect, Changed: true);
        SoundRequestChecks<SoundEffectId> polls = tick.Checks;
        polls[SoundEffectLibraries.ToQueueIndex(soundEffect.Library)] = start;
        return tick with { Checks = polls };
    }
}
