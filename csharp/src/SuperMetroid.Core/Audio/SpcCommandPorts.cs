namespace SuperMetroid.Core.Audio;

/// <summary>
/// The synthesis-free half of Super Metroid's SPC driver that the 65816 can observe: the
/// CPU-to-APU input latches, the sound-effect acknowledgement ports, and the driver timer
/// that decides when each sound-effect port is polled (#1304).
/// </summary>
/// <remarks>
/// The driver ticks every <see cref="SpcDriverData.DspCyclesPerDriverTick"/> DSP cycles and
/// polls the three sound-effect ports every eighth tick (512 DSP cycles), while a 60-Hz
/// update spans <see cref="DspCyclesPerFrame"/> cycles, so the poll phase drifts across
/// frames and is carried between them. Each poll acts on the value latched by the previous
/// poll, echoes it to the acknowledgement port and latches the CPU's current write, so a
/// write overwritten before a poll is never seen. The driver never reads DSP state back,
/// so the acknowledgement bytes depend on this state alone. The music command on port zero is
/// consumed, and its echo published, by the sequencer at its own tempo ticks; an
/// <see cref="IAudioBackend"/> receives it as an <see cref="AudioEvent"/> and is not part of this model.
/// </remarks>
[SpcMemoryBoundary("implementation: the SPC CPU-IO port latches and acknowledgement ports")]
public sealed class SpcCommandPorts
{
    /// <summary>DSP cycles (native 32.04-kHz stereo frames) in one 60-Hz gameplay update.</summary>
    public const int DspCyclesPerFrame = ManagedSnesDsp.NativeStereoFramesPerVideoFrame;

    /// <summary>The most driver ticks one frame can contain: one pending at the frame start plus one per whole tick period.</summary>
    public const int MaximumTicksPerFrame = DspCyclesPerFrame / SpcDriverData.DspCyclesPerDriverTick + 1;

    /// <summary><c>$F4-$F7</c> as the SPC reads them: the CPU's most recent write to each input port.</summary>
    [SpcMemoryAccess]
    private readonly byte[] inputPorts = new byte[AudioRomData.Apu.PortCount];
    /// <summary><c>$F5-$F7</c> as the CPU reads them: each sound-effect library's acknowledgement byte.</summary>
    [SpcMemoryAccess]
    private readonly byte[] soundEffectOutputPorts = new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>SPC <c>$01-$03</c>: each SFX port's value latched by the previous service's <c>$1621</c> read.</summary>
    private readonly byte[] soundCommandReads = new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>SPC <c>$09-$0B</c>: the value acted on at the previous service, for change detection.</summary>
    private readonly byte[] previousSoundCommandReads = new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>
    /// DSP cycles elapsed since the last driver tick; a tick that falls exactly on a frame
    /// boundary runs at the start of the next frame.
    /// </summary>
    private byte timerCycles;
    /// <summary>The driver's eight-bit sound-effect timer: each tick adds <c>$20</c> and a carry polls.</summary>
    private byte soundEffectTimerAccumulator;

    /// <summary>Writes one CPU-to-SPC communication port exactly as the cartridge does.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is outside ports zero through three.</exception>
    public void WritePort(ApuPort port, byte value)
    {
        if ((uint)port >= inputPorts.Length)
            throw new ArgumentOutOfRangeException(nameof(port), port, "SPC input port must be zero through three.");
        inputPorts[(int)port] = value;
    }

    /// <summary>Reads one sound-effect library's SPC-to-CPU acknowledgement port.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not a sound-effect library port.</exception>
    public byte ReadSoundEffectPort(ApuPort port)
    {
        int library = (int)port - (int)ApuPort.SoundLibrary1;
        if ((uint)library >= soundEffectOutputPorts.Length)
            throw new ArgumentOutOfRangeException(nameof(port), port, "SPC sound-effect port must be one through three.");
        return soundEffectOutputPorts[library];
    }

    /// <summary>
    /// The port half of an upload. The music input latch reads empty; <c>$1E8B</c> leaves
    /// <c>$BB</c> on output port 1 and finishes with <c>$F1 = $31</c>, which resets the
    /// CPU-to-APU input latches. The driver's <c>$01-$0B</c> words are untouched.
    /// </summary>
    public void ResetForUpload()
    {
        inputPorts[(int)ApuPort.Music] = (byte)MusicPortCommand.None;
        soundEffectOutputPorts[0] = SpcDriverData.UploadReadyLibraryOnePort;
        for (int port = (int)ApuPort.SoundLibrary1; port < inputPorts.Length; port++)
            inputPorts[port] = 0;
    }

    /// <summary>
    /// Walks one frame's <see cref="DspCyclesPerFrame"/> DSP cycles, running every driver tick
    /// and sound-effect poll that falls in it, and records them in order.
    /// </summary>
    /// <param name="schedule">Receives the frame's ticks; needs <see cref="MaximumTicksPerFrame"/> entries.</param>
    /// <returns>The number of ticks written to <paramref name="schedule"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="schedule"/> is too short.</exception>
    /// <exception cref="InvalidDataException">The timer holds a value no frame walk can leave.</exception>
    public int AdvanceFrame(Span<SpcDriverTick> schedule)
    {
        if (schedule.Length < MaximumTicksPerFrame)
            throw new ArgumentException($"Driver schedule requires {MaximumTicksPerFrame} entries.", nameof(schedule));
        if (timerCycles > SpcDriverData.DspCyclesPerDriverTick)
            throw new InvalidDataException($"SPC timer contains impossible value {timerCycles}.");

        int count = 0;
        int cycle = 0;
        while (cycle < DspCyclesPerFrame)
        {
            if (timerCycles >= SpcDriverData.DspCyclesPerDriverTick)
            {
                schedule[count++] = Tick(cycle, unchecked((byte)(timerCycles / SpcDriverData.DspCyclesPerDriverTick)));
                timerCycles &= SpcDriverData.DspCyclesPerDriverTick - 1;
            }

            int cycles = Math.Min(DspCyclesPerFrame - cycle, SpcDriverData.DspCyclesPerDriverTick - timerCycles);
            timerCycles = unchecked((byte)(timerCycles + cycles));
            cycle += cycles;
        }
        return count;
    }

    /// <summary>Runs one driver tick at <paramref name="dspCycleOffset"/>, advancing the sound-effect timer and polling the SFX ports on its carry.</summary>
    private SpcDriverTick Tick(int dspCycleOffset, byte ticks)
    {
        int soundAccumulator = soundEffectTimerAccumulator + unchecked((byte)(ticks * SpcDriverData.SoundEffectTimerStep));
        soundEffectTimerAccumulator = unchecked((byte)soundAccumulator);
        if (soundAccumulator <= byte.MaxValue)
            return new SpcDriverTick(dspCycleOffset, ticks, SoundEffectsPolled: false, default, default, default);
        return new SpcDriverTick(dspCycleOffset, ticks, SoundEffectsPolled: true,
            PollSoundLibrary(0), PollSoundLibrary(1), PollSoundLibrary(2));
    }

    /// <summary>
    /// The port half of one sound-effect service: the handler acts on the value latched by
    /// the previous service, and <c>$1621</c> then echoes it and latches the CPU's current write.
    /// </summary>
    private SpcSoundEffectPoll PollSoundLibrary(int libraryIndex)
    {
        byte previous = previousSoundCommandReads[libraryIndex];
        byte command = soundCommandReads[libraryIndex];
        previousSoundCommandReads[libraryIndex] = command;
        soundEffectOutputPorts[libraryIndex] = command;
        soundCommandReads[libraryIndex] = inputPorts[libraryIndex + (int)ApuPort.SoundLibrary1];
        return new SpcSoundEffectPoll(command, Changed: command != previous);
    }
}
