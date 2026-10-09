namespace SuperMetroid.Core.Audio;

/// <summary>Per-music-voice state from Super Metroid's SPC driver work RAM.</summary>
internal sealed class ManagedSpcMusicChannel
{
    /// <summary>Current pointer in the channel's top-level pattern-order stream.</summary>
    internal ushort PatternOrderPointer;
    /// <summary>Hardware voice number represented by this music channel.</summary>
    internal byte Index;
    /// <summary>Remaining duration of the current note.</summary>
    internal byte NoteTicksLeft;
    /// <summary>Remaining note duration before its scheduled key-off.</summary>
    internal byte NoteKeyOffTicksLeft;
    /// <summary>Active nested pattern-repeat depth.</summary>
    internal byte SubroutineLoops;
    /// <summary>Remaining ticks in the channel-volume fade.</summary>
    internal byte VolumeFadeTicks;
    /// <summary>Remaining ticks in the pan fade.</summary>
    internal byte PanTicks;
    /// <summary>Remaining ticks in the current pitch slide.</summary>
    internal byte PitchSlideLength;
    /// <summary>Delay ticks before the pitch slide begins.</summary>
    internal byte PitchSlideDelayLeft;
    /// <summary>Ticks elapsed during the vibrato hold delay.</summary>
    internal byte VibratoHoldCount;
    /// <summary>Current vibrato depth.</summary>
    internal byte VibratoDepth;
    /// <summary>Ticks elapsed during the tremolo hold delay.</summary>
    internal byte TremoloHoldCount;
    /// <summary>Current tremolo depth.</summary>
    internal byte TremoloDepth;
    /// <summary>Steps elapsed while fading vibrato depth.</summary>
    internal byte VibratoChangeCount;
    /// <summary>Duration byte loaded for the current note.</summary>
    internal byte NoteLength;
    /// <summary>Fixed-point fraction of the note duration before key-off.</summary>
    internal byte NoteGateOffFixedPoint;
    /// <summary>Per-note volume multiplier selected from the note prefix.</summary>
    internal byte ChannelVolumeMaster;
    /// <summary>Current instrument-table index for the channel.</summary>
    internal byte InstrumentId;
    /// <summary>Fixed-point pitch multiplier loaded from the instrument record.</summary>
    internal ushort InstrumentPitchBase;
    /// <summary>Saved return pointer for the current pattern call.</summary>
    internal ushort SavedPatternPointer;
    /// <summary>Start pointer revisited while a counted pattern repeat remains active.</summary>
    internal ushort PatternStartPointer;
    /// <summary>Remaining delay before the pitch envelope starts.</summary>
    internal byte PitchEnvelopeTicks;
    /// <summary>Initial delay configured for the pitch envelope.</summary>
    internal byte PitchEnvelopeDelay;
    /// <summary>Pitch-envelope direction selector.</summary>
    internal byte PitchEnvelopeDirection;
    /// <summary>Semitone displacement applied by the pitch envelope.</summary>
    internal byte PitchEnvelopeSlide;
    /// <summary>Current vibrato waveform phase.</summary>
    internal byte VibratoCount;
    /// <summary>Vibrato phase increment per update.</summary>
    internal byte VibratoRate;
    /// <summary>Configured delay before vibrato affects pitch.</summary>
    internal byte VibratoDelayTicks;
    /// <summary>Duration of vibrato-depth fade-in.</summary>
    internal byte VibratoFadeTicks;
    /// <summary>Signed per-tick vibrato-depth change.</summary>
    internal byte VibratoFadeAddPerTick;
    /// <summary>Destination depth for vibrato fade-in.</summary>
    internal byte VibratoDepthTarget;
    /// <summary>Current tremolo waveform phase.</summary>
    internal byte TremoloCount;
    /// <summary>Tremolo phase increment per update.</summary>
    internal byte TremoloRate;
    /// <summary>Configured delay before tremolo affects volume.</summary>
    internal byte TremoloDelayTicks;
    /// <summary>Signed semitone offset applied to channel notes.</summary>
    internal byte ChannelTransposition;
    /// <summary>Current 8.8 fixed-point channel volume.</summary>
    internal ushort ChannelVolume;
    /// <summary>Signed per-tick channel-volume fade increment.</summary>
    internal ushort VolumeFadeAddPerTick;
    /// <summary>Destination high byte of a channel-volume fade.</summary>
    internal byte VolumeFadeTarget;
    /// <summary>Most recently calculated combined channel gain.</summary>
    internal byte FinalVolume;
    /// <summary>Current 8.8 fixed-point pan position.</summary>
    internal ushort PanValue;
    /// <summary>Signed per-tick pan fade increment.</summary>
    internal ushort PanAddPerTick;
    /// <summary>Destination pan index for the current fade.</summary>
    internal byte PanTarget;
    /// <summary>Pan flags including left/right phase inversion.</summary>
    internal byte PanFlags;
    /// <summary>Current note and fine-tune packed as 8.8 fixed point.</summary>
    internal ushort Pitch;
    /// <summary>Signed per-tick pitch increment for an active slide.</summary>
    internal ushort PitchAddPerTick;
    /// <summary>Destination note byte for a pitch slide.</summary>
    internal byte PitchTarget;
    /// <summary>Fine pitch byte retained with the current note.</summary>
    internal byte FineTune;
    /// <summary>Suppresses note key-on while the driver fast-forwards a channel.</summary>
    internal byte CutKey;
}

/// <summary>Shared allocation and command state for one of the three SFX libraries.</summary>
internal sealed class ManagedSpcSoundLibrary
{
    /// <summary>Current arbitration priority class for requests to the library.</summary>
    internal byte Priority;
    /// <summary>Current library channel index multiplied by two for native table addressing.</summary>
    internal byte ChannelIndexTimesTwo;
    /// <summary>Pointer to this library's per-channel voice-bitset state.</summary>
    internal ushort ChannelVoiceBitsetPointer;
    /// <summary>One-based command currently playing, or zero when idle.</summary>
    internal byte CurrentSound;
    /// <summary>Pointer to this library's per-channel voice-mask state.</summary>
    internal ushort ChannelVoiceMaskPointer;
    /// <summary>Pointer to this library's per-channel voice-index state.</summary>
    internal ushort ChannelVoiceIndexPointer;
    /// <summary>Library-specific special mode used by low-health sound handling.</summary>
    internal byte Mode;
    /// <summary>Current SPC bytecode entry address for the sound command.</summary>
    internal ushort CurrentPointer;
    /// <summary>Bitset of voices currently allocated to this library.</summary>
    internal byte EnabledVoices;
    /// <summary>Number of voices still requested during allocation setup.</summary>
    internal byte VoicesToSetup;
    /// <summary>Next library channel receiving an allocated voice.</summary>
    internal byte ChannelIndex;
    /// <summary>Unallocated hardware voice bits scanned by the setup loop.</summary>
    internal byte VoicesRemaining;
    /// <summary>Command-table index multiplied by two.</summary>
    internal byte CurrentSoundIndex;
    /// <summary>Descending hardware voice scan cursor.</summary>
    internal byte VoiceId;
    /// <summary>Native nonzero marker indicating channel allocation has completed.</summary>
    internal byte InitializationFlag;
    /// <summary>Hardware voice index selected for the current library channel.</summary>
    internal byte VoiceIndex;
}

/// <summary>One logical SFX stream temporarily borrowing a hardware music voice.</summary>
internal sealed class ManagedSpcSoundChannel
{
    /// <summary>Hardware voice bit temporarily owned by this logical sound channel.</summary>
    internal byte VoiceBitset;
    /// <summary>Remaining ticks before the next sound-program instruction executes.</summary>
    internal byte InstructionTimer;
    /// <summary>Fractional pitch component of the current note.</summary>
    internal byte Subnote;
    /// <summary>Bytecode offset of the active repeat body.</summary>
    internal byte RepeatPoint;
    /// <summary>Current note number in the sound channel.</summary>
    internal byte Note;
    /// <summary>Byte offset into the channel's selected sound program.</summary>
    internal byte PointerIndex;
    /// <summary>Hardware voice index borrowed by this sound channel.</summary>
    internal byte VoiceIndex;
    /// <summary>Hardware voice index multiplied by eight for native DSP-register addressing.</summary>
    internal byte VoiceIndexTimesEight;
    /// <summary>Remaining two-tick key-off delay before a new sound instruction.</summary>
    internal byte ReleaseTimer;
    /// <summary>Saved music-channel volume restored when this SFX channel releases its voice.</summary>
    internal byte Volume;
    /// <summary>Pitch-slide destination note.</summary>
    internal byte TargetNote;
    /// <summary>Saved phase-inversion and pan state restored on voice release.</summary>
    internal byte PhaseInvert;
    /// <summary>Resident SPC-RAM address of this channel's program.</summary>
    internal ushort Pointer;
    /// <summary>Remaining count for the active counted repeat.</summary>
    internal byte RepeatCounter;
    /// <summary>Native marker indicating a pitch slide is active.</summary>
    internal byte EnablePitchSlide;
    /// <summary>Native marker indicating the channel is in its release interval.</summary>
    internal byte ReleaseFlag;
    /// <summary>Retained opcode selecting the legato pitch-slide form.</summary>
    internal byte EnablePitchSlideLegato;
    /// <summary>Packed ADSR1/ADSR2 override for the next programmed note.</summary>
    internal ushort AdsrSettings;
    /// <summary>Complement of the owned voice bit, used to clear that voice from library masks.</summary>
    internal byte ChannelMask;
    /// <summary>Native nonzero marker preserving note continuity for a legato slide.</summary>
    internal byte Legato;
    /// <summary>Native all-bits-set marker for a channel that has no allocated voice.</summary>
    internal byte Disabled;
    /// <summary>Fractional pitch step applied on each pitch-slide update.</summary>
    internal byte SubnoteDelta;
    /// <summary>Native all-bits-set marker enabling the ADSR override on DSP writes.</summary>
    internal byte UpdateAdsr;
}

/// <summary>
/// Managed translation of Super Metroid's SPC700 music and sound-effect driver.
/// The class deliberately retains the driver's byte/word state model: overflow, port echoes,
/// voice borrowing, and DSP-write order are externally observable cartridge behavior.
/// </summary>
public sealed partial class ManagedSpcPlayer
{
    /// <summary>Complete SPC RAM image shared with the managed DSP.</summary>
    private readonly byte[] ram = new byte[SpcDriverData.ApuRamSize];
    /// <summary>Latest SPC-to-SNES values published on the four output ports.</summary>
    private readonly byte[] portsToSnes = new byte[AudioRomData.Apu.PortCount];
    /// <summary>Most recently sampled SNES-to-SPC input values for the four ports.</summary>
    private readonly byte[] inputPorts = new byte[AudioRomData.Apu.PortCount];
    /// <summary>SPC $01-$03 values latched by the previous sound-service port read.</summary>
    private readonly byte[] soundCommandReads = new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>SPC $09-$0B request values last acted on, used to detect new port commands.</summary>
    private readonly byte[] previousSoundCommandReads = new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>Eight music-channel sequencer states, one for each hardware voice.</summary>
    private readonly ManagedSpcMusicChannel[] channels = Enumerable
        .Range(0, SpcDriverData.ChannelCount)
        .Select(index => new ManagedSpcMusicChannel { Index = unchecked((byte)index) })
        .ToArray();
    /// <summary>Allocation and active-command state for the three independent SFX libraries.</summary>
    private readonly ManagedSpcSoundLibrary[] soundLibraries = Enumerable
        .Range(0, SpcDriverData.SoundEffects.LibraryCount)
        .Select(_ => new ManagedSpcSoundLibrary())
        .ToArray();
    /// <summary>Per-library logical sound channels that temporarily borrow music voices.</summary>
    private readonly ManagedSpcSoundChannel[][] soundChannels =
    [
        Enumerable.Range(0, SpcDriverData.SoundEffects.LibraryOneChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
        Enumerable.Range(0, SpcDriverData.SoundEffects.OtherLibraryChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
        Enumerable.Range(0, SpcDriverData.SoundEffects.OtherLibraryChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
    ];

    /// <summary>DSP implementation writing into the shared APU RAM image.</summary>
    private readonly ManagedSnesDsp dsp;
    /// <summary>Residual DSP cycles carried into the next driver tick.</summary>
    private byte timerCycles;
    /// <summary>Track-start and scheduling countdown shared by the music driver.</summary>
    private ushort counter;
    /// <summary>Dirty marker for current channel volume or pitch work.</summary>
    private byte affectedVolumeOrPitch;
    /// <summary>Bitset of hardware voices currently enabled for music or SFX.</summary>
    private byte channelOnMask;
    /// <summary>Native nonzero marker that runs music commands without waiting for tempo ticks.</summary>
    private byte fastForward;
    /// <summary>Current pointer in the top-level track/pattern-order stream.</summary>
    private ushort musicTopLevelPointer;
    /// <summary>Remaining iterations in the active top-level track repeat.</summary>
    private byte blockCount;
    /// <summary>Fractional SFX service timer accumulator.</summary>
    private byte soundEffectTimerAccumulator;
    /// <summary>Pending DSP key-on voice mask.</summary>
    private byte keyOn;
    /// <summary>Pending DSP key-off voice mask.</summary>
    private byte keyOff;
    /// <summary>Bit corresponding to the channel currently being processed.</summary>
    private byte currentChannelBit;
    /// <summary>Cached global DSP flag byte, including reset, mute, and echo-write state.</summary>
    private byte dspFlags;
    /// <summary>Bitset selecting channels driven by the shared noise generator.</summary>
    private byte noiseEnable;
    /// <summary>Bitset selecting channels mixed into echo feedback.</summary>
    private byte echoEnable;
    /// <summary>Signed countdown tracking echo-delay register reconfiguration.</summary>
    private byte echoStoredTime;
    /// <summary>Current configured echo-ring delay.</summary>
    private byte echoDelay;
    /// <summary>Signed feedback gain for the echo ring.</summary>
    private byte echoFeedback;
    /// <summary>Signed global note transposition.</summary>
    private byte globalTransposition;
    /// <summary>Fractional accumulator used to schedule tempo-driven music ticks.</summary>
    private byte mainTempoAccumulator;
    /// <summary>Current 8.8 fixed-point music tempo.</summary>
    private ushort tempo;
    /// <summary>Remaining ticks in a global tempo fade.</summary>
    private byte tempoFadeTicks;
    /// <summary>Destination tempo high byte.</summary>
    private byte tempoFadeTarget;
    /// <summary>Signed fixed-point increment for the active tempo fade.</summary>
    private ushort tempoFadeAdd;
    /// <summary>Current 8.8 fixed-point master music volume.</summary>
    private ushort masterVolume;
    /// <summary>Remaining ticks in the master-volume fade.</summary>
    private byte masterVolumeFadeTicks;
    /// <summary>Destination master-volume high byte.</summary>
    private byte masterVolumeFadeTarget;
    /// <summary>Signed fixed-point increment for the active master-volume fade.</summary>
    private ushort masterVolumeFadeAdd;
    /// <summary>Voice bitset whose volume registers need publication.</summary>
    private byte volumeDirty;
    /// <summary>Base instrument offset used to map percussion notes to instrument records.</summary>
    private byte percussionBaseId;
    /// <summary>Current 8.8 fixed-point left echo output volume.</summary>
    private ushort echoVolumeLeft;
    /// <summary>Current 8.8 fixed-point right echo output volume.</summary>
    private ushort echoVolumeRight;
    /// <summary>Signed left echo-volume fade increment.</summary>
    private ushort echoVolumeFadeAddLeft;
    /// <summary>Signed right echo-volume fade increment.</summary>
    private ushort echoVolumeFadeAddRight;
    /// <summary>Remaining ticks in the stereo echo-volume fade.</summary>
    private byte echoVolumeFadeTicks;
    /// <summary>Destination left echo-volume high byte.</summary>
    private byte echoVolumeFadeTargetLeft;
    /// <summary>Destination right echo-volume high byte.</summary>
    private byte echoVolumeFadeTargetRight;
    /// <summary>Echo-delay value most recently written to the DSP.</summary>
    private byte lastWrittenEchoDelay;

    /// <summary>Creates an independent SPC sequencer and DSP over owned 64-KiB APU RAM, applying native driver reset state without loading an audio bank or opening a host audio device.</summary>
    /// <remarks>Uploads and instrument, music, sound-effect, and decoded sample definitions are installed separately by the audio renderer before playback.</remarks>
    public ManagedSpcPlayer()
    {
        dsp = new ManagedSnesDsp(ram);
        InitializeDriver();
    }

    /// <summary>
    /// Selects the replaceable PCM sources corresponding to the most recent cartridge upload.
    /// Sequence, pitch, envelope, pan, cancellation, and echo behavior remain live DSP work.
    /// </summary>
    public void SetSampleBank(ManagedPcmSampleBank bank) => dsp.SetSampleBank(bank);

    /// <summary>
    /// Installs the manifest-owned source/noise selector, ADSR, gain, and pitch-base words
    /// for one audio bank. Uploads still provide sequence bytes and the native driver image;
    /// applying this complete table afterward makes instrument edits deterministic and keeps
    /// their six-byte SPC layout visible to diagnostics.
    /// </summary>
    public void ApplyInstrumentDefinitions(
        IReadOnlyList<AudioInstrumentMetadata> instruments)
    {
        ArgumentNullException.ThrowIfNull(instruments);
        if (instruments.Count != SpcDriverData.Ram.InstrumentCount)
        {
            throw new InvalidDataException(
                $"Instrument bank contains {instruments.Count} records; " +
                $"expected {SpcDriverData.Ram.InstrumentCount}.");
        }

        for (int index = 0; index < instruments.Count; index++)
        {
            AudioInstrumentMetadata instrument = instruments[index];
            if (instrument.Instrument != index)
            {
                throw new InvalidDataException(
                    $"Instrument bank index {index} contains identity {instrument.Instrument}.");
            }
            if (instrument.UsesNoise !=
                ((instrument.SourceOrNoiseRate & SpcDriverData.Instruments.NoiseMarker) != 0))
            {
                throw new InvalidDataException(
                    $"Instrument {index} has inconsistent usesNoise and sourceOrNoiseRate fields.");
            }

            int address = SpcDriverData.Ram.InstrumentTable +
                index * SpcDriverData.Ram.InstrumentRecordSize;
            ram[address] = instrument.SourceOrNoiseRate;
            ram[address + 1] = instrument.Adsr1;
            ram[address + 2] = instrument.Adsr2;
            ram[address + 3] = instrument.Gain;
            ram[address + 4] = unchecked((byte)(instrument.PitchBase >> 8));
            ram[address + 5] = unchecked((byte)instrument.PitchBase);
        }
    }

    /// <summary>
    /// Recompiles decoded authored SFX programs into their fixed resident slots. Program
    /// routing and voice allocation remain compiled mechanics and must still match the
    /// uploaded driver's pointer tables exactly.
    /// </summary>
    public void ApplySoundEffectDefinitions(
        IReadOnlyList<AudioSoundProgramMetadata> programs,
        IReadOnlyList<AudioSoundLibraryMetadata> libraries)
    {
        ArgumentNullException.ThrowIfNull(programs);
        ArgumentNullException.ThrowIfNull(libraries);
        Dictionary<string, AudioSoundProgramMetadata> byId = new(StringComparer.Ordinal);
        foreach (AudioSoundProgramMetadata program in programs)
        {
            if (!byId.TryAdd(program.Id, program))
                throw new InvalidDataException($"Decoded SFX programs repeat stable ID '{program.Id}'.");
            byte[] encoded = SpcSoundEffectProgramCodec.Encode(program);
            encoded.CopyTo(ram.AsSpan(program.Address, encoded.Length));
        }

        foreach (AudioSoundLibraryMetadata library in libraries)
        {
            foreach (AudioSoundEffectMetadata effect in library.Effects)
            {
                for (int channel = 0; channel < effect.ChannelPrograms.Count; channel++)
                {
                    if (!byId.TryGetValue(effect.ChannelPrograms[channel], out AudioSoundProgramMetadata? program))
                    {
                        throw new InvalidDataException(
                            $"SFX effect '{effect.Id}' references unknown program " +
                            $"'{effect.ChannelPrograms[channel]}'.");
                    }
                    ushort uploadedPointer = ReadWord(effect.StreamPointer + channel * 2);
                    if (uploadedPointer != program.Address)
                    {
                        throw new InvalidDataException(
                            $"SFX effect '{effect.Id}' channel {channel} routes to uploaded " +
                            $"${uploadedPointer:X4}, not decoded program '{program.Id}' at ${program.Address:X4}.");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recompiles decoded track flow, phrase routing, and channel programs into their fixed
    /// authored slots. The driver interpreter and lookup tables remain ordinary managed code.
    /// </summary>
    public void ApplyMusicDefinitions(AudioBankMetadata bank)
    {
        ArgumentNullException.ThrowIfNull(bank);
        for (int track = 0; track < bank.MusicTracks.Count; track++)
        {
            int pointerAddress = SpcDriverData.Ram.DefaultMusicPointer + track * 2;
            ushort uploadedPointer = ReadWord(pointerAddress);
            AudioMusicTrackMetadata definition = bank.MusicTracks[track];
            if (uploadedPointer != definition.Address || bank.TrackPointers[track] != definition.Address)
            {
                throw new InvalidDataException(
                    $"Music track '{definition.Id}' routes to uploaded ${uploadedPointer:X4}, " +
                    $"not decoded address ${definition.Address:X4}.");
            }
        }
        foreach ((int address, byte value) in SpcMusicDefinitionCodec.CompileBank(bank))
            ram[address] = value;
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
        inputPorts[AudioRomData.Apu.MusicPort] = SpcDriverData.NoPortCommand;
        // $1E8B leaves $BB on output port 1 and finishes with $F1 = $31, which resets the
        // CPU-to-APU input latches. The driver's $01-$0B words are untouched.
        portsToSnes[AudioRomData.Apu.LibraryOnePort] = SpcDriverData.UploadReadyLibraryOnePort;
        for (int port = AudioRomData.Apu.FirstSoundPort; port < AudioRomData.Apu.PortCount; port++)
            inputPorts[port] = 0;
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

    /// <summary>Applies reset-vector RAM clearing, DSP defaults, echo setup, and the initial SPC tick.</summary>
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

    /// <summary>Explicit native DSP publication dispatch, replacing the descending
    /// SPC1E52 destination and1E5C direct-page source maps (ten paired entries).
    /// Preserve both KOF writes, the intervening noise/pitch updates and KON ordering.
    /// Negative echo countdown skips FLG/echo; an unsettled delay skips only echo.
    /// Independently checked against the original maps and pinned Spc_Loop_Part1
    /// for1165; the pending key masks clear only after publication.</summary>
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

    /// <summary>Advances fractional SFX/music timers and runs due sequencer work for the elapsed DSP ticks.</summary>
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

    /// <summary>Reconfigures the native echo countdown and places the ring buffer below APU RAM's reserved tail.</summary>
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

    /// <summary>Narrows a native integer value to one byte and writes the selected DSP register.</summary>
    private void WriteDsp(byte register, int value)
    {
        byte narrowed = unchecked((byte)value);
        dsp.WriteRegister(register, narrowed);
    }

    /// <summary>Reads a little-endian word from SPC RAM with 16-bit address wrap.</summary>
    private ushort ReadWord(int address) => unchecked((ushort)(
        ram[address & ushort.MaxValue] | (ram[(address + 1) & ushort.MaxValue] << 8)));

    /// <summary>Extracts the high byte of a native 16-bit fixed-point value.</summary>
    private static byte HighByte(ushort value) => unchecked((byte)(value >> 8));

    /// <summary>Replaces a word's high byte while retaining its fractional low byte.</summary>
    private static void SetHighByte(ref ushort target, byte value) =>
        target = unchecked((ushort)((target & byte.MaxValue) | (value << 8)));

    /// <summary>Adds with native 16-bit wraparound.</summary>
    private static ushort AddWord(ushort left, int right) => unchecked((ushort)(left + right));
}
