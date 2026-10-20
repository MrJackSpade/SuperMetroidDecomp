using SuperMetroid.Core.Audio;
using SuperMetroid.Core;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Managed translation of Super Metroid's SPC700 music and sound-effect driver.
/// The class deliberately retains the driver's byte/word state model: overflow, port echoes,
/// voice borrowing, and DSP-write order are externally observable cartridge behavior.
/// Music and sound-effect logic reads sequence streams, pointer tables, instrument records and the
/// pan/FIR tables only through its typed APU-RAM views (#632); addresses, byte order and 16-bit
/// wrapping are confined there.
/// </summary>
public sealed class ManagedSpcPlayer
{

    /// <summary>Zeroes the driver work regions the native reset clears.</summary>
    [SpcMemoryBoundary("driver-memory view: reset work regions")]
    private void ClearDriverWorkRegions()
    {
        Array.Clear(ram, SpcDriverDefinitions.Ram.FirstWorkRegion, SpcDriverDefinitions.Ram.FirstWorkRegionLength);
        Array.Clear(ram, SpcDriverDefinitions.Ram.SmallWorkRegion, SpcDriverDefinitions.Ram.SmallWorkRegionLength);
        Array.Clear(ram, SpcDriverDefinitions.Ram.SoundPointerRegion, SpcDriverDefinitions.Ram.SoundPointerRegionLength);
        Array.Clear(ram, SpcDriverDefinitions.Ram.SoundStateRegion, SpcDriverDefinitions.Ram.SoundStateRegionLength);
        Array.Clear(ram, SpcDriverDefinitions.Ram.SecondarySoundStateRegion,
            SpcDriverDefinitions.Ram.SecondarySoundStateRegionLength);
    }

    /// <summary>The DSP reads BRR samples and the echo buffer straight from APU RAM.</summary>
    [SpcMemoryBoundary("implementation: the DSP shares the APU RAM it plays from")]
    private ManagedSnesDsp CreateDsp() => new(ram);

    /// <summary>Owned 64-KiB APU RAM shared by the driver views and DSP sample decoder.</summary>
    [SpcMemoryAccess]
    internal readonly byte[] ram = new byte[SpcDriverDefinitions.ApuRamSize];
    /// <summary>Eight music sequencer states that continue advancing under borrowed DSP voices.</summary>
    private readonly ManagedSpcMusicChannel[] channels = Enumerable
        .Range(0, SpcDriverDefinitions.ChannelCount)
        .Select(index => new ManagedSpcMusicChannel { Index = unchecked((byte)index) })
        .ToArray();
    /// <summary>Priority, mode and voice-allocation state for the three SFX libraries.</summary>
    internal readonly ManagedSpcSoundLibrary[] soundLibraries = Enumerable
        .Range(0, SpcDriverDefinitions.SoundEffects.LibraryCount)
        .Select(_ => new ManagedSpcSoundLibrary())
        .ToArray();
    /// <summary>Per-library SFX instruction cursors and borrowed voice state.</summary>
    internal readonly ManagedSpcSoundChannel[][] soundChannels =
    [
        Enumerable.Range(0, SpcDriverDefinitions.SoundEffects.LibraryOneChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
        Enumerable.Range(0, SpcDriverDefinitions.SoundEffects.OtherLibraryChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
        Enumerable.Range(0, SpcDriverDefinitions.SoundEffects.OtherLibraryChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
    ];

    /// <summary>DSP renderer reading BRR and echo data from this driver's APU RAM.</summary>
    internal readonly ManagedSnesDsp dsp;

    /// <summary>The SPC driver's music sequencer: track commands, effects, notes and the pan, sweep, vibrato and tremolo it ticks.</summary>
    internal SpcMusicSequencer Music { get; }

    /// <summary>What music and sound effects share on the DSP: instruments, pitch and volume writes, the key-on, key-off, noise and echo flags, and memory reads.</summary>
    internal SpcVoices Voices { get; }

    /// <summary>The SPC driver's three sound-effect libraries: their requests, configuration and channels.</summary>
    internal SpcSoundLibraries SoundLibraries { get; }

    /// <summary>What the extraction uploads into the SPC's memory: instruments, sound effects, music and raw blocks.</summary>
    internal SpcDefinitionUploads Uploads { get; }

    /// <summary>Creates an independent SPC sequencer and DSP over owned 64-KiB APU RAM, applying native driver reset state without loading an audio bank or opening a host audio device.</summary>
    /// <remarks>Uploads and instrument, music, sound-effect, and decoded sample definitions are installed separately by <see cref="ManagedSpcAudioBackend"/> before playback.</remarks>
    [SpcMemoryBoundary("implementation: hands its APU RAM to the driver's parts")]
    public ManagedSpcPlayer()
    {
        dsp = CreateDsp();
        Voices = new SpcVoices(ram, dsp);
        Music = new SpcMusicSequencer(ram, channels, Voices);
        SoundLibraries = new SpcSoundLibraries(ram, channels, soundLibraries, soundChannels, dsp, Voices);
        Uploads = new SpcDefinitionUploads(ram, Music, Voices);
        InitializeDriver();
    }

    /// <summary>
    /// Selects the replaceable PCM sources corresponding to the most recent cartridge upload.
    /// Sequence, pitch, envelope, pan, cancellation, and echo behavior remain live DSP work.
    /// </summary>
    public void SetSampleBank(ManagedPcmSampleBank bank) => dsp.SetSampleBank(bank);

    /// <summary>Latches one CPU write to the music port; the sequencer consumes it at its next tempo tick.</summary>
    public void WriteMusicPort(byte value) => Music.musicInputLatch = value;

    /// <summary>
    /// Advances one 60-Hz frame and returns the S-DSP's native
    /// 32.04-kHz output (<see cref="ManagedSnesDsp.NativeStereoFramesPerVideoFrame"/> interleaved
    /// stereo frames) before the host resample, for the offline clip renderer (#1305).
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too short.</exception>
    /// <exception cref="InvalidDataException">The schedule is out of order or outside the frame, or the driver state cannot be synthesized.</exception>
    internal void GenerateNativeFrame(ReadOnlySpan<SpcDriverTick> schedule, Span<short> destination)
    {
        RunFrame(schedule, synthesize: true);
        dsp.CopyNativeSamples(destination);
    }

    /// <summary>
    /// Advances the sequencer and sound-effect driver through one frame's ticks without clocking
    /// the S-DSP. The driver never reads DSP state back, so its state evolves exactly as in
    /// <see cref="GenerateNativeFrame"/>; the DSP only receives the register writes.
    /// </summary>
    /// <exception cref="InvalidDataException">The schedule is out of order or outside the frame.</exception>
    internal void AdvanceSequencer(ReadOnlySpan<SpcDriverTick> schedule) => RunFrame(schedule, synthesize: false);

    /// <summary>Services the frame's driver ticks in order, clocking the DSP up to each tick when synthesizing.</summary>
    private void RunFrame(ReadOnlySpan<SpcDriverTick> schedule, bool synthesize)
    {
        if (dsp.BufferedStereoFrameCount != 0)
            throw new InvalidDataException("S-DSP sample cursor is not at a frame boundary.");

        int cycle = 0;
        foreach (SpcDriverTick tick in schedule)
        {
            if (tick.DspCycleOffset < cycle || tick.DspCycleOffset >= SpcCommandPorts.DspCyclesPerFrame)
                throw new InvalidDataException($"Driver tick at DSP cycle {tick.DspCycleOffset} is out of order or outside the frame.");
            for (; synthesize && cycle < tick.DspCycleOffset; cycle++)
                dsp.Cycle();
            cycle = tick.DspCycleOffset;
            Music.observer?.TickStarting(tick);
            LoopPartTwo(tick);
            LoopPartOne();
        }
        for (; synthesize && cycle < SpcCommandPorts.DspCyclesPerFrame; cycle++)
            dsp.Cycle();
    }

    /// <summary>
    /// Writes the complete sequencer and sound-effect driver state that determines all future
    /// driver behavior, for offline loop and entry-state analysis (#1305). The DSP, APU RAM and
    /// observer are excluded. <paramref name="includeTempoPhase"/> false omits the sub-tick tempo
    /// accumulator, whose phase shifts music ticks by less than one driver tick. The global repeat
    /// counter is written by behavior class: zero and $81-$FF all reload at the next repeat record.
    /// </summary>
    internal void WriteSequencerState(List<byte> state, bool includeTempoPhase)
    {
        Ensure.NotNull(state);
        SpcDriverSnapshot.AddBytes(state,
        [
            Music.musicInputLatch, Music.musicPortToSnes, Music.affectedVolumeOrPitch, Voices.channelOnMask, Music.fastForward,
            Music.blockCount is 0 or > SpcDriverDefinitions.Music.RepeatReloadClassMinimum ? 0 : Music.blockCount,
            Voices.keyOn, Voices.keyOff, Voices.currentChannelBit, Voices.dspFlags, Voices.noiseEnable, Voices.echoEnable, Music.echoStoredTime, Music.echoDelay,
            Music.echoFeedback, Music.globalTransposition, includeTempoPhase ? Music.mainTempoAccumulator : 0,
            Music.tempoFadeTicks, Music.tempoFadeTarget, Music.masterVolumeFadeTicks, Music.masterVolumeFadeTarget, Music.volumeDirty,
            Voices.percussionBaseId, Music.echoVolumeFadeTicks, Music.echoVolumeFadeTargetLeft, Music.echoVolumeFadeTargetRight,
            Music.lastWrittenEchoDelay,
        ]);
        SpcDriverSnapshot.AddWords(state, [
            Music.counter, Music.musicTopLevelPointer, Music.tempo, Music.tempoFadeAdd, Music.masterVolume, Music.masterVolumeFadeAdd,
            Music.echoVolumeLeft, Music.echoVolumeRight, Music.echoVolumeFadeAddLeft, Music.echoVolumeFadeAddRight]);
        foreach (ManagedSpcMusicChannel channel in channels)
            channel.WriteState(state);
        foreach (ManagedSpcSoundLibrary library in soundLibraries)
            library.WriteState(state);
        foreach (ManagedSpcSoundChannel[] libraryChannels in soundChannels)
        {
            foreach (ManagedSpcSoundChannel channel in libraryChannels)
                channel.WriteState(state);
        }
    }

    /// <summary>The address of the next top-level music record, for offline loop analysis (#1305).</summary>
    internal int ReadMusicTopLevelPointer() => Music.musicTopLevelPointer;

    /// <summary>The tempo's whole byte, added to the accumulator per driver tick, for clip export (#1305).</summary>
    internal byte ReadTempo() => unchecked(SpcFixedPoint.Whole(Music.tempo));

    /// <summary>Resets DSP/work RAM and publishes the driver's initial echo and master-register state.</summary>
    private void InitializeDriver()
    {
        dsp.Reset();
        ClearDriverWorkRegions();

        for (int index = 0; index < channels.Length; index++)
            channels[index].Index = unchecked((byte)index);
        Music.SetupEchoDelay(SpcDriverDefinitions.Echo.InitialDelay);
        Voices.dspFlags = Voices.dspFlags | SpcDriverDefinitions.Echo.WriteDisable;
        Voices.Dsp.Global.WriteMasterVolume(DspStereoSide.Left, 0x60); // cartridge-encoding: allow(HardwareMagnitude) - cartridge reset master volume
        Voices.Dsp.Global.WriteMasterVolume(DspStereoSide.Right, 0x60); // cartridge-encoding: allow(HardwareMagnitude) - cartridge reset master volume
        Voices.Dsp.Global.WriteSourceDirectory(0x6d); // cartridge-encoding: allow(HardwareAddress) - cartridge BRR directory page
        Music.tempo = SpcFixedPoint.WithWhole(Music.tempo, SpcDriverDefinitions.Music.InitialTempo);
        LoopPartOne();
    }

    /// <summary>Explicit native DSP publication dispatch, replacing the descending
    /// SPC1E52 destination and1E5C direct-page source maps (ten paired entries).
    /// Preserve both KOF writes, the intervening noise/pitch updates and KON ordering.
    /// Negative echo countdown skips FLG/echo; an unsettled delay skips only echo.
    /// Independently checked against the original maps and pinned Spc_Loop_Part1
    /// for1165; the pending key masks clear only after publication.</summary>
    internal void LoopPartOne()
    {
        Music.observer?.KeysPublished(Voices.keyOn, Voices.keyOff);
        Voices.Dsp.Global.KeyOff(new DspVoiceMask(Voices.keyOff));
        Voices.Dsp.Global.WritePitchModulation(DspVoiceMask.None);
        Voices.Dsp.Global.WriteNoiseEnable(new DspVoiceMask(Voices.noiseEnable));
        Voices.Dsp.Global.KeyOff(DspVoiceMask.None);
        Voices.Dsp.Global.KeyOn(new DspVoiceMask(Voices.keyOn));
        if ((Music.echoStoredTime & 0x80) == 0) // cartridge-encoding: allow(BitMask) - signed SPC countdown bit
        {
            Voices.Dsp.Global.WriteFlags(DspFlags.FromDriverByte(Voices.dspFlags));
            if (Music.echoStoredTime == Music.echoDelay)
            {
                Voices.Dsp.Global.WriteEchoEnable(new DspVoiceMask(Voices.echoEnable));
                Voices.Dsp.Global.WriteEchoFeedback(unchecked((sbyte)Music.echoFeedback));
                Voices.Dsp.Global.WriteEchoVolume(DspStereoSide.Right, unchecked((sbyte)SpcFixedPoint.Whole(Music.echoVolumeRight)));
                Voices.Dsp.Global.WriteEchoVolume(DspStereoSide.Left, unchecked((sbyte)SpcFixedPoint.Whole(Music.echoVolumeLeft)));
            }
        }
        Voices.keyOff = Voices.keyOn = 0;
    }

    /// <summary>Advances sound and music accumulators, servicing latched commands only on their native carry ticks.</summary>
    private void LoopPartTwo(SpcDriverTick tick)
    {
        int ticks = tick.Ticks;
        if (tick.SoundEffectsPolled)
        {
            for (int library = 0; library < SoundEffectLibraries.Count; library++)
                SoundLibraries.HandleSoundLibraryCommand(library, tick.LibraryPolls[library]);
            if (Music.echoStoredTime != Music.echoDelay)
                Music.echoStoredTime = SpcDriverWrap.Byte(Music.echoStoredTime + 1);
        }

        int musicAccumulator = Music.mainTempoAccumulator + unchecked((byte)(ticks * SpcFixedPoint.Whole(Music.tempo)));
        Music.mainTempoAccumulator = unchecked((byte)musicAccumulator);
        if (musicAccumulator >= 0x100) // cartridge-encoding: allow(HardwareMagnitude) - eight-bit timer carry
        {
            int fastForwardIterations = 0;
            do
            {
                Music.HandleMusicCommand();
                if (++fastForwardIterations > SpcDriverDefinitions.MaximumFastForwardTicks)
                    throw new InvalidDataException("SPC music fast-forward failed to reach a timed event.");
            }
            while (Music.fastForward != 0);
        }
        else if (Music.musicPortToSnes != 0)
        {
            for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
            {
                Voices.currentChannelBit = unchecked((byte)bit);
                if (channels[index].PatternOrderPointer > byte.MaxValue)
                    Music.HandlePanAndSweep(channels[index]);
            }
            Voices.currentChannelBit = 0;
        }
    }
}
