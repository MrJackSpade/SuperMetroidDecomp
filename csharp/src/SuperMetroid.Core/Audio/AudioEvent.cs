namespace SuperMetroid.Core.Audio;

/// <summary>
/// One semantic audio instruction of a frame (#1304): a bank load, a music command, a driver
/// tick, or a sound-effect start. Fields that do not belong to <see cref="Kind"/> hold their
/// defaults.
/// </summary>
/// <remarks>
/// The game session owns the SPC driver timer and port protocol, so these events already say
/// when the driver acts: a sound effect starts only where a driver poll latches a changed
/// nonzero request, never at the CPU's port write. Backends follow the events and never
/// re-derive the timer.
/// </remarks>
public readonly record struct AudioEvent
{
    /// <summary>Stores one event's fields; use the kind-specific factories.</summary>
    private AudioEvent(AudioEventKind kind, int dspCycleOffset, AudioBank bank, MusicCommand music,
        SoundEffectId soundEffect, byte ticks, bool soundEffectsPolled)
    {
        Kind = kind;
        DspCycleOffset = dspCycleOffset;
        Bank = bank;
        Music = music;
        SoundEffect = soundEffect;
        Ticks = ticks;
        SoundEffectsPolled = soundEffectsPolled;
    }

    /// <summary>What this event instructs the backend to do.</summary>
    public AudioEventKind Kind { get; }

    /// <summary>
    /// DSP cycles (native 32.04-kHz stereo samples) from the frame start at which the event takes
    /// effect. Bank loads and music commands apply before the frame's cycles and carry zero.
    /// </summary>
    public int DspCycleOffset { get; }

    /// <summary>The bank a <see cref="AudioEventKind.LoadAudioBank"/> event installs.</summary>
    public AudioBank Bank { get; }

    /// <summary>The music command a <see cref="AudioEventKind.WriteMusicPort"/> event latches for the sequencer.</summary>
    public MusicCommand Music { get; }

    /// <summary>The sound effect a <see cref="AudioEventKind.StartSoundEffect"/> event starts.</summary>
    public SoundEffectId SoundEffect { get; }

    /// <summary>Elapsed timer periods a <see cref="AudioEventKind.DriverTick"/> event accounts for.</summary>
    public byte Ticks { get; }

    /// <summary>Whether a <see cref="AudioEventKind.DriverTick"/> event also services the three sound-effect libraries.</summary>
    public bool SoundEffectsPolled { get; }

    /// <summary>Installs <paramref name="bank"/>; the driver's music port then reads empty.</summary>
    public static AudioEvent LoadAudioBank(AudioBank bank) =>
        new(AudioEventKind.LoadAudioBank, 0, bank, default, default, 0, false);

    /// <summary>Latches <paramref name="command"/> for the sequencer, which consumes it at its next tempo tick.</summary>
    public static AudioEvent WriteMusicPort(MusicCommand command) =>
        new(AudioEventKind.WriteMusicPort, 0, default, command, default, 0, false);

    /// <summary>One driver tick at <paramref name="dspCycleOffset"/>.</summary>
    /// <param name="dspCycleOffset">DSP cycles from the frame start.</param>
    /// <param name="ticks">Elapsed timer periods the tick accounts for.</param>
    /// <param name="soundEffectsPolled">Whether the tick services the sound-effect libraries.</param>
    public static AudioEvent DriverTick(int dspCycleOffset, byte ticks, bool soundEffectsPolled) =>
        new(AudioEventKind.DriverTick, dspCycleOffset, default, default, default, ticks, soundEffectsPolled);

    /// <summary>Starts <paramref name="soundEffect"/> at the sound-effect poll of the tick at <paramref name="dspCycleOffset"/>.</summary>
    public static AudioEvent StartSoundEffect(SoundEffectId soundEffect, int dspCycleOffset) =>
        new(AudioEventKind.StartSoundEffect, dspCycleOffset, default, default, soundEffect, 0, false);

    /// <inheritdoc />
    public override string ToString() => Kind switch
    {
        AudioEventKind.LoadAudioBank => $"load:{Bank}",
        AudioEventKind.WriteMusicPort => $"music:{Music}",
        AudioEventKind.DriverTick => $"tick@{DspCycleOffset}x{Ticks}{(SoundEffectsPolled ? "+poll" : "")}",
        AudioEventKind.StartSoundEffect => $"start@{DspCycleOffset}:{SoundEffect}",
        _ => throw new InvalidOperationException($"Undefined {nameof(AudioEventKind)} {(int)Kind}."),
    };
}
