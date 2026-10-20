namespace SuperMetroid.Core.Audio;

/// <summary>
/// One semantic audio instruction of a frame (#1304): a bank load, a music command, a driver tick, or a sound-effect
/// start. Each is its own sealed record holding only what it carries.
/// </summary>
/// <remarks>
/// The game session owns the SPC driver timer and port protocol, so these events already say
/// when the driver acts: a sound effect starts only where a driver poll latches a changed
/// nonzero request, never at the CPU's port write. Backends follow the events and never
/// re-derive the timer.
/// </remarks>
public abstract record AudioEvent
{
    /// <summary>Closes the hierarchy to the events declared here.</summary>
    private AudioEvent()
    {
    }

    /// <summary>Installs <see cref="Bank"/> before the frame's driver ticks run; the driver's music port then reads empty.</summary>
    /// <param name="Bank">The bank installed.</param>
    public sealed record LoadAudioBank(AudioBank Bank) : AudioEvent
    {
        /// <inheritdoc />
        public override string ToString() => $"load:{Bank}";
    }

    /// <summary>
    /// Latches <see cref="Music"/> for the sequencer before the frame's driver ticks run; the sequencer consumes it at
    /// its next tempo tick.
    /// </summary>
    /// <param name="Music">The music command latched.</param>
    public sealed record WriteMusicPort(MusicCommand Music) : AudioEvent
    {
        /// <inheritdoc />
        public override string ToString() => $"music:{Music}";
    }

    /// <summary>One driver timer tick at its DSP-cycle offset within the frame.</summary>
    /// <param name="DspCycleOffset">DSP cycles (native 32.04-kHz stereo samples) from the frame start at which the tick runs.</param>
    /// <param name="Ticks">Elapsed timer periods the tick accounts for.</param>
    /// <param name="SoundEffectsPolled">Whether the tick also services the three sound-effect libraries.</param>
    public sealed record DriverTick(int DspCycleOffset, int Ticks, bool SoundEffectsPolled) : AudioEvent
    {
        /// <inheritdoc />
        public override string ToString() => $"tick@{DspCycleOffset}x{Ticks}{(SoundEffectsPolled ? "+poll" : "")}";
    }

    /// <summary>Starts <see cref="SoundEffect"/> at the sound-effect poll of the tick at <see cref="DspCycleOffset"/>.</summary>
    /// <param name="SoundEffect">The sound effect started.</param>
    /// <param name="DspCycleOffset">DSP cycles from the frame start of the polling tick.</param>
    public sealed record StartSoundEffect(SoundEffectId SoundEffect, int DspCycleOffset) : AudioEvent
    {
        /// <inheritdoc />
        public override string ToString() => $"start@{DspCycleOffset}:{SoundEffect}";
    }
}
