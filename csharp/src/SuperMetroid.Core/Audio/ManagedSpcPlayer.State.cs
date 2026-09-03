namespace SuperMetroid.Core.Audio;

/// <summary>Per-music-voice state from Super Metroid's SPC driver work RAM.</summary>
internal sealed class ManagedSpcMusicChannel
{
    internal ushort PatternOrderPointer;
    internal byte Index;
    internal byte NoteTicksLeft;
    internal byte NoteKeyOffTicksLeft;
    internal byte SubroutineLoops;
    internal byte VolumeFadeTicks;
    internal byte PanTicks;
    internal byte PitchSlideLength;
    internal byte PitchSlideDelayLeft;
    internal byte VibratoHoldCount;
    internal byte VibratoDepth;
    internal byte TremoloHoldCount;
    internal byte TremoloDepth;
    internal byte VibratoChangeCount;
    internal byte NoteLength;
    internal byte NoteGateOffFixedPoint;
    internal byte ChannelVolumeMaster;
    internal byte InstrumentId;
    internal ushort InstrumentPitchBase;
    internal ushort SavedPatternPointer;
    internal ushort PatternStartPointer;
    internal byte PitchEnvelopeTicks;
    internal byte PitchEnvelopeDelay;
    internal byte PitchEnvelopeDirection;
    internal byte PitchEnvelopeSlide;
    internal byte VibratoCount;
    internal byte VibratoRate;
    internal byte VibratoDelayTicks;
    internal byte VibratoFadeTicks;
    internal byte VibratoFadeAddPerTick;
    internal byte VibratoDepthTarget;
    internal byte TremoloCount;
    internal byte TremoloRate;
    internal byte TremoloDelayTicks;
    internal byte ChannelTransposition;
    internal ushort ChannelVolume;
    internal ushort VolumeFadeAddPerTick;
    internal byte VolumeFadeTarget;
    internal byte FinalVolume;
    internal ushort PanValue;
    internal ushort PanAddPerTick;
    internal byte PanTarget;
    internal byte PanFlags;
    internal ushort Pitch;
    internal ushort PitchAddPerTick;
    internal byte PitchTarget;
    internal byte FineTune;
    internal byte CutKey;
}

/// <summary>Shared allocation and command state for one of the three SFX libraries.</summary>
internal sealed class ManagedSpcSoundLibrary
{
    internal byte Priority;
    internal byte ChannelIndexTimesTwo;
    internal ushort ChannelVoiceBitsetPointer;
    internal byte CurrentSound;
    internal ushort ChannelVoiceMaskPointer;
    internal ushort ChannelVoiceIndexPointer;
    internal byte Mode;
    internal ushort CurrentPointer;
    internal byte EnabledVoices;
    internal byte VoicesToSetup;
    internal byte ChannelIndex;
    internal byte VoicesRemaining;
    internal byte CurrentSoundIndex;
    internal byte VoiceId;
    internal byte InitializationFlag;
    internal byte VoiceIndex;
}

/// <summary>One logical SFX stream temporarily borrowing a hardware music voice.</summary>
internal sealed class ManagedSpcSoundChannel
{
    internal byte VoiceBitset;
    internal byte InstructionTimer;
    internal byte Subnote;
    internal byte RepeatPoint;
    internal byte Note;
    internal byte PointerIndex;
    internal byte VoiceIndex;
    internal byte VoiceIndexTimesEight;
    internal byte ReleaseTimer;
    internal byte Volume;
    internal byte TargetNote;
    internal byte PhaseInvert;
    internal ushort Pointer;
    internal byte RepeatCounter;
    internal byte EnablePitchSlide;
    internal byte ReleaseFlag;
    internal byte EnablePitchSlideLegato;
    internal ushort AdsrSettings;
    internal byte ChannelMask;
    internal byte Legato;
    internal byte Disabled;
    internal byte SubnoteDelta;
    internal byte UpdateAdsr;
}

/// <summary>
/// Managed translation of Super Metroid's SPC700 music and sound-effect driver.
/// The class deliberately retains the driver's byte/word state model: overflow, port echoes,
/// voice borrowing, and DSP-write order are externally observable cartridge behavior.
/// </summary>
public sealed partial class ManagedSpcPlayer
{
    private readonly byte[] ram = new byte[SpcDriverData.ApuRamSize];
    private readonly byte[] portsToSnes = new byte[AudioRomData.Apu.PortCount];
    private readonly byte[] inputPorts = new byte[AudioRomData.Apu.PortCount];
    private readonly ManagedSpcMusicChannel[] channels = Enumerable
        .Range(0, SpcDriverData.ChannelCount)
        .Select(index => new ManagedSpcMusicChannel { Index = unchecked((byte)index) })
        .ToArray();
    private readonly ManagedSpcSoundLibrary[] soundLibraries = Enumerable
        .Range(0, SpcDriverData.SoundEffects.LibraryCount)
        .Select(_ => new ManagedSpcSoundLibrary())
        .ToArray();
    private readonly ManagedSpcSoundChannel[][] soundChannels =
    [
        Enumerable.Range(0, SpcDriverData.SoundEffects.LibraryOneChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
        Enumerable.Range(0, SpcDriverData.SoundEffects.OtherLibraryChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
        Enumerable.Range(0, SpcDriverData.SoundEffects.OtherLibraryChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
    ];

    private readonly ManagedSnesDsp dsp;
    private byte timerCycles;
    private ushort counter;
    private byte affectedVolumeOrPitch;
    private byte channelOnMask;
    private byte fastForward;
    private ushort musicTopLevelPointer;
    private byte blockCount;
    private byte soundEffectTimerAccumulator;
    private byte keyOn;
    private byte keyOff;
    private byte currentChannelBit;
    private byte dspFlags;
    private byte noiseEnable;
    private byte echoEnable;
    private byte echoStoredTime;
    private byte echoDelay;
    private byte echoFeedback;
    private byte globalTransposition;
    private byte mainTempoAccumulator;
    private ushort tempo;
    private byte tempoFadeTicks;
    private byte tempoFadeTarget;
    private ushort tempoFadeAdd;
    private ushort masterVolume;
    private byte masterVolumeFadeTicks;
    private byte masterVolumeFadeTarget;
    private ushort masterVolumeFadeAdd;
    private byte volumeDirty;
    private byte percussionBaseId;
    private ushort echoVolumeLeft;
    private ushort echoVolumeRight;
    private ushort echoVolumeFadeAddLeft;
    private ushort echoVolumeFadeAddRight;
    private byte echoVolumeFadeTicks;
    private byte echoVolumeFadeTargetLeft;
    private byte echoVolumeFadeTargetRight;
    private byte lastWrittenEchoDelay;

    public ManagedSpcPlayer()
    {
        dsp = new ManagedSnesDsp(ram);
        InitializeDriver();
    }

    /// <summary>Writes one CPU-to-SPC communication port exactly as the cartridge does.</summary>
    public void WritePort(int port, byte value)
    {
        if ((uint)port >= inputPorts.Length)
            throw new ArgumentOutOfRangeException(nameof(port), port, "SPC input port must be zero through three.");
        inputPorts[port] = value;
    }

    /// <summary>Reads one SPC-to-CPU acknowledgement port.</summary>
    public byte ReadPort(int port)
    {
        if ((uint)port >= portsToSnes.Length)
            throw new ArgumentOutOfRangeException(nameof(port), port, "SPC output port must be zero through three.");
        return portsToSnes[port];
    }

    /// <summary>
    /// Applies a complete cartridge upload stream to APU RAM. Each record contains a little-
    /// endian length and destination followed by payload; a zero length terminates the stream.
    /// </summary>
    public void Upload(ReadOnlySpan<byte> stream)
    {
        WriteDsp(SnesDspRegisterMap.Global.EchoVolumeLeft, 0);
        WriteDsp(SnesDspRegisterMap.Global.EchoVolumeRight, 0);
        WriteDsp(SnesDspRegisterMap.Global.KeyOff, byte.MaxValue);

        int offset = 0;
        for (;;)
        {
            if (offset > stream.Length - 2)
                throw new InvalidDataException("SPC upload stream ended before its record length.");
            ushort length = unchecked((ushort)(stream[offset] | (stream[offset + 1] << 8)));
            offset += 2;
            if (length == 0)
                break;
            if (offset > stream.Length - 2)
                throw new InvalidDataException("SPC upload stream ended before its destination word.");
            ushort target = unchecked((ushort)(stream[offset] | (stream[offset + 1] << 8)));
            offset += 2;
            if (length > stream.Length - offset)
                throw new InvalidDataException(
                    $"SPC upload record for ${target:X4} declares {length} bytes but only {stream.Length - offset} remain.");
            for (int index = 0; index < length; index++)
                ram[unchecked((ushort)(target + index))] = stream[offset + index];
            offset += length;
        }

        portsToSnes[AudioRomData.Apu.MusicPort] = 0;
        Array.Fill(inputPorts, SpcDriverData.NoPortCommand);
        musicTopLevelPointer = ReadWord(SpcDriverData.Ram.DefaultMusicPointer);
        counter = SpcDriverData.Music.TrackStartupTicks;
        keyOff |= unchecked((byte)~channelOnMask);
    }

    /// <summary>Advances one 60-Hz emulation frame and returns 48-kHz interleaved stereo PCM.</summary>
    public void GenerateFrame(Span<short> destination)
    {
        if (destination.Length < SpcDriverData.HostStereoFramesPerVideoFrame * 2)
        {
            throw new ArgumentException(
                $"Audio destination requires {SpcDriverData.HostStereoFramesPerVideoFrame * 2} samples.",
                nameof(destination));
        }
        if (timerCycles > SpcDriverData.DspCyclesPerDriverTick)
            throw new InvalidDataException($"SPC timer contains impossible value {timerCycles}.");
        if (dsp.BufferedStereoFrameCount > ManagedSnesDsp.NativeStereoFramesPerVideoFrame)
            throw new InvalidDataException("S-DSP sample cursor escaped its native frame buffer.");

        while (dsp.BufferedStereoFrameCount < ManagedSnesDsp.NativeStereoFramesPerVideoFrame)
        {
            if (timerCycles >= SpcDriverData.DspCyclesPerDriverTick)
            {
                LoopPartTwo(unchecked((byte)(timerCycles / SpcDriverData.DspCyclesPerDriverTick)));
                LoopPartOne();
                timerCycles &= SpcDriverData.DspCyclesPerDriverTick - 1;
            }

            int cycles = Math.Min(
                ManagedSnesDsp.NativeStereoFramesPerVideoFrame - dsp.BufferedStereoFrameCount,
                SpcDriverData.DspCyclesPerDriverTick - timerCycles);
            timerCycles = unchecked((byte)(timerCycles + cycles));
            for (int index = 0; index < cycles; index++)
                dsp.Cycle();
        }

        dsp.CopyResampledSamples(destination, SpcDriverData.HostStereoFramesPerVideoFrame);
    }

    private void InitializeDriver()
    {
        dsp.Reset();
        Array.Clear(ram, SpcDriverData.Ram.FirstWorkRegion, SpcDriverData.Ram.FirstWorkRegionLength);
        Array.Clear(ram, SpcDriverData.Ram.SmallWorkRegion, SpcDriverData.Ram.SmallWorkRegionLength);
        Array.Clear(ram, SpcDriverData.Ram.SoundPointerRegion, SpcDriverData.Ram.SoundPointerRegionLength);
        Array.Clear(ram, SpcDriverData.Ram.SoundStateRegion, SpcDriverData.Ram.SoundStateRegionLength);
        Array.Clear(ram, SpcDriverData.Ram.SecondarySoundStateRegion,
            SpcDriverData.Ram.SecondarySoundStateRegionLength);

        for (int index = 0; index < channels.Length; index++)
            channels[index].Index = unchecked((byte)index);
        SetupEchoDelay(SpcDriverData.Echo.InitialDelay);
        dspFlags |= SpcDriverData.Echo.WriteDisable;
        WriteDsp(SnesDspRegisterMap.Global.MasterVolumeLeft, 0x60); // allow(HardwareMagnitude): cartridge reset master volume
        WriteDsp(SnesDspRegisterMap.Global.MasterVolumeRight, 0x60); // allow(HardwareMagnitude): cartridge reset master volume
        WriteDsp(SnesDspRegisterMap.Global.SourceDirectory, 0x6d); // allow(HardwareAddress): cartridge BRR directory page
        SetHighByte(ref tempo, SpcDriverData.Music.InitialTempo);
        timerCycles = 0;
        LoopPartOne();
    }

    private void LoopPartOne()
    {
        WriteDsp(SnesDspRegisterMap.Global.KeyOff, keyOff);
        WriteDsp(SnesDspRegisterMap.Global.PitchModulation, 0);
        WriteDsp(SnesDspRegisterMap.Global.NoiseEnable, noiseEnable);
        WriteDsp(SnesDspRegisterMap.Global.KeyOff, 0);
        WriteDsp(SnesDspRegisterMap.Global.KeyOn, keyOn);
        if ((echoStoredTime & 0x80) == 0) // allow(BitMask): signed SPC countdown bit
        {
            WriteDsp(SnesDspRegisterMap.Global.Flags, dspFlags);
            if (echoStoredTime == echoDelay)
            {
                WriteDsp(SnesDspRegisterMap.Global.EchoEnable, echoEnable);
                WriteDsp(SnesDspRegisterMap.Global.EchoFeedback, echoFeedback);
                WriteDsp(SnesDspRegisterMap.Global.EchoVolumeRight, HighByte(echoVolumeRight));
                WriteDsp(SnesDspRegisterMap.Global.EchoVolumeLeft, HighByte(echoVolumeLeft));
            }
        }
        keyOff = keyOn = 0;
    }

    private void LoopPartTwo(byte ticks)
    {
        int soundAccumulator = soundEffectTimerAccumulator + unchecked((byte)(ticks * 0x20)); // magic-number-audit: allow(AudioId) - SPC timer scale, not a sequence ID
        soundEffectTimerAccumulator = unchecked((byte)soundAccumulator);
        if (soundAccumulator >= 0x100) // allow(HardwareMagnitude): eight-bit timer carry
        {
            HandleSoundLibraryCommand(0);
            HandleSoundLibraryCommand(1);
            HandleSoundLibraryCommand(2);
            if (echoStoredTime != echoDelay)
                echoStoredTime++;
        }

        int musicAccumulator = mainTempoAccumulator + unchecked((byte)(ticks * HighByte(tempo)));
        mainTempoAccumulator = unchecked((byte)musicAccumulator);
        if (musicAccumulator >= 0x100) // allow(HardwareMagnitude): eight-bit timer carry
        {
            int fastForwardIterations = 0;
            do
            {
                HandleMusicCommand();
                if (++fastForwardIterations > SpcDriverData.MaximumFastForwardTicks)
                    throw new InvalidDataException("SPC music fast-forward failed to reach a timed event.");
            }
            while (fastForward != 0);
        }
        else if (portsToSnes[AudioRomData.Apu.MusicPort] != 0)
        {
            for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
            {
                currentChannelBit = unchecked((byte)bit);
                if (HighByte(channels[index].PatternOrderPointer) != 0)
                    HandlePanAndSweep(channels[index]);
            }
            currentChannelBit = 0;
        }
    }

    private void SetupEchoDelay(byte requestedDelay)
    {
        echoDelay = requestedDelay;
        if (requestedDelay != lastWrittenEchoDelay)
        {
            byte countdown = unchecked((byte)((lastWrittenEchoDelay &
                SnesDspRegisterMap.Fields.EchoDelayMask) ^ byte.MaxValue));
            if ((echoStoredTime & 0x80) != 0) // allow(BitMask): signed SPC countdown bit
                countdown = unchecked((byte)(countdown + echoStoredTime));
            echoStoredTime = countdown;

            WriteDsp(SnesDspRegisterMap.Global.EchoEnable, 0);
            WriteDsp(SnesDspRegisterMap.Global.EchoFeedback, 0);
            WriteDsp(SnesDspRegisterMap.Global.EchoVolumeRight, 0);
            WriteDsp(SnesDspRegisterMap.Global.EchoVolumeLeft, 0);
            WriteDsp(SnesDspRegisterMap.Global.Flags, unchecked((byte)(dspFlags |
                SpcDriverData.Echo.WriteDisable)));
            lastWrittenEchoDelay = echoDelay;
            WriteDsp(SnesDspRegisterMap.Global.EchoDelay, echoDelay);
        }

        byte echoPage = unchecked((byte)(((echoDelay * SpcDriverData.Echo.DelayToPages) ^
            byte.MaxValue) + SpcDriverData.Echo.BufferPageBias));
        WriteDsp(SnesDspRegisterMap.Global.EchoBufferAddress, echoPage);
    }

    private void WriteDsp(byte register, int value)
    {
        byte narrowed = unchecked((byte)value);
        dsp.WriteRegister(register, narrowed);
    }

    private ushort ReadWord(int address) => unchecked((ushort)(
        ram[address & ushort.MaxValue] | (ram[(address + 1) & ushort.MaxValue] << 8)));

    private static byte HighByte(ushort value) => unchecked((byte)(value >> 8));

    private static void SetHighByte(ref ushort target, byte value) =>
        target = unchecked((ushort)((target & byte.MaxValue) | (value << 8)));

    private static ushort AddWord(ushort left, int right) => unchecked((ushort)(left + right));
}
