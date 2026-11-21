using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Managed implementation of the eight-voice S-DSP behavior used by Super Metroid.
/// It consumes replaceable decoded PCM while retaining cartridge-accurate Gaussian
/// interpolation, envelopes, pitch/noise modulation, and the shared FIR echo buffer. The
/// music driver continues to write the ordinary DSP registers, so samples remain data rather
/// than pre-rendered sound effects. It is the chip model: the byte register file, the signed eight-bit volumes and
/// the sixteen-bit sample arithmetic whose clamps and wraps the output reproduces live here at the chip's widths.
/// </summary>
[EncodingOwner("S-DSP register file and its signed sample arithmetic")]
public sealed class ManagedSnesDsp
{
    /// <summary>Native 32.04-kHz stereo frames accumulated for each 60-Hz gameplay update.</summary>
    public const int NativeStereoFramesPerVideoFrame = SoundClockTiming.SamplesPerUpdate;
    /// <summary>Number of independent hardware S-DSP voices.</summary>
    public const int VoiceCount = 8;
    /// <summary>The register-file address of ENDX, whose bits each voice sets when its sample ends or loops.</summary>
    private static readonly int endFlagsAddress = SnesDspRegisterLayout.AddressOf(DspGlobalRegister.EndFlags);
    /// <summary>The register-file address of DIR, the page of the BRR sample directory.</summary>
    private static readonly int sourceDirectoryAddress = SnesDspRegisterLayout.AddressOf(DspGlobalRegister.SourceDirectory);

    /// <summary>Shared 64-KiB audio RAM holding BRR directories and the echo ring.</summary>
    private readonly SpcRam apuRam;
    /// <summary>Mirrored DSP registers, including values published by each sample cycle.</summary>
    private readonly byte[] registers = new byte[SnesDspRegisterLayout.RegisterFileSize];
    /// <summary>Eight independent pitch, envelope, and PCM windows mixed each cycle.</summary>
    private readonly Voice[] voices = Enumerable.Range(0, VoiceCount)
        .Select(_ => new Voice())
        .ToArray();
    /// <summary>One gameplay update's interleaved native-rate stereo output.</summary>
    private readonly short[] sampleBuffer = new short[NativeStereoFramesPerVideoFrame * 2];
    /// <summary>Eight signed FIR coefficients written through the voice-spaced DSP registers.</summary>
    private readonly sbyte[] firValues = new sbyte[8];
    /// <summary>Recent left echo samples addressed by the FIR ring cursor.</summary>
    private readonly short[] firBufferLeft = new short[8];
    /// <summary>Recent right echo samples addressed by the FIR ring cursor.</summary>
    private readonly short[] firBufferRight = new short[8];

    /// <summary>
    /// Replaceable decoded PCM selected by key-on and loop directory reads: until the first upload installs one, a bank that
    /// maps no source, so a key-on before it is rejected by the bank as any unmapped source is.
    /// </summary>
    private ManagedPcmSampleBank sampleBank = new("no upload installed", default, new Dictionary<int, ManagedPcmSample>());
    /// <summary>FLG mute bit applied after voice and echo mixing.</summary>
    private bool mute;
    /// <summary>FLG reset bit forcing voice envelopes into release.</summary>
    private bool reset;
    /// <summary>Signed master gain for the left dry mix.</summary>
    private sbyte masterVolumeLeft;
    /// <summary>Signed master gain for the right dry mix.</summary>
    private sbyte masterVolumeRight;
    /// <summary>Current signed sample from the noise shift register.</summary>
    private short noiseSample;
    /// <summary>Noise clock period selected by FLG.</summary>
    private int noiseRate;
    /// <summary>Cycles elapsed toward the next noise shift-register step.</summary>
    private int noiseCounter;
    /// <summary>Whether echo feedback may be written into APU RAM.</summary>
    private bool echoWrites;
    /// <summary>Left filtered echo gain added to the final mix.</summary>
    private sbyte echoVolumeLeft;
    /// <summary>Right filtered echo gain added to the final mix.</summary>
    private sbyte echoVolumeRight;
    /// <summary>Gain applied when feeding filtered echo back into RAM.</summary>
    private sbyte feedbackVolume;
    /// <summary>Echo buffer base address from the DSP ESA register.</summary>
    private int echoBufferAddress;
    /// <summary>Number of stereo echo words in the configured delay loop.</summary>
    private int echoDelay;
    /// <summary>Echo positions remaining before the ring returns to its base.</summary>
    private int echoRemaining;
    /// <summary>Current stereo word offset within the echo buffer.</summary>
    private int echoBufferIndex;
    /// <summary>Current position in the eight-tap FIR history ring.</summary>
    private int firBufferIndex;

    /// <summary>Creates a reset DSP bound to the supplied complete 64-KiB APU RAM image.</summary>
    internal ManagedSnesDsp(SpcRam apuRam)
    {
        this.apuRam = Ensure.NotNull(apuRam);
        Reset();
    }

    /// <summary>Native stereo frames accumulated since the last host-buffer copy.</summary>
    public int BufferedStereoFrameCount { get; private set; }

    /// <summary>
    /// Voices whose output reaches the main mix; all by default. The offline clip renderer (#1305)
    /// selects one sound-effect channel's voice to render it as its own looping layer. Masked voices
    /// still run, so pitch modulation and echo input are unchanged.
    /// </summary>
    internal int OutputVoices { get; set; } = byte.MaxValue;

    /// <summary>Restores the hardware-facing DSP state used by the translated SPC reset vector.</summary>
    public void Reset()
    {
        Array.Clear(registers);
        registers[endFlagsAddress] = byte.MaxValue;
        foreach (Voice voice in voices)
            voice.Reset();
        mute = true;
        reset = true;
        masterVolumeLeft = 0;
        masterVolumeRight = 0;
        noiseSample = SnesDspRegisterLayout.NoiseResetSample;
        noiseRate = 0;
        noiseCounter = 0;
        echoWrites = false;
        echoVolumeLeft = 0;
        echoVolumeRight = 0;
        feedbackVolume = 0;
        echoBufferAddress = 0;
        echoDelay = 1;
        echoRemaining = 1;
        echoBufferIndex = 0;
        firBufferIndex = 0;
        Array.Clear(firValues);
        Array.Clear(firBufferLeft);
        Array.Clear(firBufferRight);
        Array.Clear(sampleBuffer);
        BufferedStereoFrameCount = 0;
    }

    /// <summary>
    /// Installs the source-number map associated with the latest cartridge upload. Existing
    /// voices retain their current window; key-ons and subsequent loop-directory reads
    /// resolve through this bank.
    /// </summary>
    public void SetSampleBank(ManagedPcmSampleBank bank) =>
        sampleBank = Ensure.NotNull(bank);

    /// <summary>Applies one SPC write to the mirrored S-DSP register file.</summary>
    public void WriteRegister(byte address, byte value)
    {
        if (address >= SnesDspRegisterLayout.RegisterFileSize)
            throw new ArgumentOutOfRangeException(nameof(address), address, "DSP register is outside $00-$7F.");
        int voiceIndex = (address / SnesDspRegisterLayout.VoiceStride) & SnesDspRegisterLayout.VoiceIndexMask;
        Voice voice = voices[voiceIndex];
        if (SnesDspRegisterLayout.VoiceRegisterAt(address) is DspVoiceRegister voiceRegister)
        {
            switch (voiceRegister)
            {
            case DspVoiceRegister.VolumeLeft:
                voice.VolumeLeft = unchecked((sbyte)value);
                break;
            case DspVoiceRegister.VolumeRight:
                voice.VolumeRight = unchecked((sbyte)value);
                break;
            case DspVoiceRegister.PitchLow:
                voice.Pitch = LittleEndianBytes.HighByteInPlace(checked((ushort)voice.Pitch)) & SnesDspRegisterLayout.Fields.PitchMask | value;
                break;
            case DspVoiceRegister.PitchHigh:
                voice.Pitch = LittleEndianBytes.Word(LittleEndianBytes.LowByte(checked((ushort)voice.Pitch)), value) & SnesDspRegisterLayout.Fields.PitchMask;
                break;
            case DspVoiceRegister.SourceNumber:
                voice.SourceNumber = value;
                break;
            case DspVoiceRegister.Adsr1:
                voice.AttackRate = SnesDspTables.RatePeriod(
                    (value & SnesDspRegisterLayout.Fields.AdsrAttackMask) * 2 + 1);
                voice.DecayRate = SnesDspTables.RatePeriod(
                    ((value & SnesDspRegisterLayout.Fields.AdsrDecayMask) >>
                        SnesDspRegisterLayout.Fields.AdsrDecayShift) * 2 + 16);
                voice.Envelope = (value & SnesDspRegisterLayout.Fields.AdsrEnabled) == 0 ? EnvelopeSource.Gain : EnvelopeSource.Adsr;
                break;
            case DspVoiceRegister.Adsr2:
                voice.SustainRate = SnesDspTables.RatePeriod(
                    value & SnesDspRegisterLayout.Fields.AdsrSustainRateMask);
                voice.SustainLevel = unchecked((ushort)((((value &
                    SnesDspRegisterLayout.Fields.AdsrSustainLevelMask) >>
                    SnesDspRegisterLayout.Fields.AdsrSustainLevelShift) + 1) * SnesDspRegisterLayout.SustainLevelStep));
                break;
            case DspVoiceRegister.Gain:
                voice.GainForm = (value & SnesDspRegisterLayout.Fields.AdsrEnabled) == 0 ? GainForm.Direct : GainForm.Stepped;
                if (voice.GainForm == GainForm.Stepped)
                {
                    voice.GainMode = SteppedGainModeOf(value);
                    voice.GainRate = SnesDspTables.RatePeriod(
                        value & SnesDspRegisterLayout.Fields.AdsrSustainRateMask);
                }
                else
                {
                    voice.GainValue = unchecked((ushort)((value &
                        SnesDspRegisterLayout.Fields.GainValueMask) * 16));
                }
                break;
            // The DSP publishes these each sample; a write does not change the voice.
            case DspVoiceRegister.EnvelopeOutput:
            case DspVoiceRegister.SampleOutput:
                break;
            default:
                throw new InvalidOperationException($"Undefined DSP voice register {voiceRegister}.");
            }
        }

        if ((address & SnesDspRegisterLayout.VoiceRegisterMask) == SnesDspRegisterLayout.FirstFirCoefficient)
        {
            firValues[voiceIndex] = unchecked((sbyte)value);
        }
        else if (SnesDspRegisterLayout.GlobalRegisterAt(address) is DspGlobalRegister globalRegister)
        {
            switch (globalRegister)
            {
            case DspGlobalRegister.MasterVolumeLeft:
                masterVolumeLeft = unchecked((sbyte)value);
                break;
            case DspGlobalRegister.MasterVolumeRight:
                masterVolumeRight = unchecked((sbyte)value);
                break;
            case DspGlobalRegister.EchoVolumeLeft:
                echoVolumeLeft = unchecked((sbyte)value);
                break;
            case DspGlobalRegister.EchoVolumeRight:
                echoVolumeRight = unchecked((sbyte)value);
                break;
            case DspGlobalRegister.KeyOn:
                for (int index = 0; index < VoiceCount; index++)
                {
                    if ((value & (1 << index)) != 0)
                        KeyOn(voices[index]);
                }
                break;
            case DspGlobalRegister.KeyOff:
                for (int index = 0; index < VoiceCount; index++)
                {
                    if ((value & (1 << index)) != 0)
                        voices[index].AdsrState = EnvelopeState.Release;
                }
                break;
            case DspGlobalRegister.Flags:
                reset = (value & SnesDspRegisterLayout.Fields.Reset) != 0;
                mute = (value & SnesDspRegisterLayout.Fields.Mute) != 0;
                echoWrites = (value & SnesDspRegisterLayout.Fields.EchoWriteDisable) == 0;
                noiseRate = SnesDspTables.RatePeriod(value & SnesDspRegisterLayout.Fields.NoiseRateMask);
                break;
            case DspGlobalRegister.EndFlags:
                value = 0;
                break;
            case DspGlobalRegister.EchoFeedback:
                feedbackVolume = unchecked((sbyte)value);
                break;
            case DspGlobalRegister.PitchModulation:
                for (int index = 0; index < VoiceCount; index++)
                    voices[index].PitchModulation = (value & (1 << index)) != 0;
                break;
            case DspGlobalRegister.NoiseEnable:
                for (int index = 0; index < VoiceCount; index++)
                    voices[index].UseNoise = (value & (1 << index)) != 0;
                break;
            case DspGlobalRegister.EchoEnable:
                for (int index = 0; index < VoiceCount; index++)
                    voices[index].EchoEnable = (value & (1 << index)) != 0;
                break;
            case DspGlobalRegister.SourceDirectory:
                // The extracted catalog has already resolved this BRR directory into stable
                // source IDs. Preserve the register mirror because software can read it back.
                break;
            case DspGlobalRegister.EchoBufferAddress:
                echoBufferAddress = LittleEndianBytes.Word(0, value);
                break;
            case DspGlobalRegister.EchoDelay:
                echoDelay = unchecked((ushort)((value & SnesDspRegisterLayout.Fields.EchoDelayMask) * 512));
                if (echoDelay == 0)
                    echoDelay = 1;
                break;
            default:
                throw new InvalidOperationException($"Undefined DSP global register {globalRegister}.");
            }
        }
        registers[address] = value;
    }

    /// <summary>Runs one native 32.04-kHz sample cycle.</summary>
    public void Cycle()
    {
        int totalLeft = 0;
        int totalRight = 0;
        for (int index = 0; index < VoiceCount; index++)
        {
            CycleVoice(index);
            if ((OutputVoices & (1 << index)) == 0)
                continue;
            Voice voice = voices[index];
            totalLeft = Clamp16(totalLeft + ((voice.SampleOutput * voice.VolumeLeft) >> 6));
            totalRight = Clamp16(totalRight + ((voice.SampleOutput * voice.VolumeRight) >> 6));
        }
        totalLeft = Clamp16((totalLeft * masterVolumeLeft) >> 7);
        totalRight = Clamp16((totalRight * masterVolumeRight) >> 7);
        HandleEcho(ref totalLeft, ref totalRight);
        if (mute)
            totalLeft = totalRight = 0;
        HandleNoise();
        if (BufferedStereoFrameCount < NativeStereoFramesPerVideoFrame)
        {
            sampleBuffer[BufferedStereoFrameCount * 2] = unchecked((short)totalLeft);
            sampleBuffer[BufferedStereoFrameCount * 2 + 1] = unchecked((short)totalRight);
            BufferedStereoFrameCount = SpcDriverWrap.Word(BufferedStereoFrameCount + 1);
        }
    }

    /// <summary>Copies the completed frame's native 32.04-kHz interleaved stereo samples unchanged and starts the next frame.</summary>
    /// <exception cref="InvalidDataException">The frame is incomplete.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is shorter than one native frame.</exception>
    public void CopyNativeSamples(Span<short> destination)
    {
        if (BufferedStereoFrameCount != NativeStereoFramesPerVideoFrame)
            throw new InvalidDataException($"S-DSP frame holds {BufferedStereoFrameCount} of {NativeStereoFramesPerVideoFrame} samples.");
        if (destination.Length < sampleBuffer.Length)
            throw new ArgumentException($"Native destination requires {sampleBuffer.Length} samples.", nameof(destination));
        sampleBuffer.CopyTo(destination);
        BufferedStereoFrameCount = 0;
    }

    /// <summary>Starts a PCM source at its beginning and resets the selected voice envelope.</summary>
    private void KeyOn(Voice voice)
    {
        voice.PreviousFlags = BrrBlockEnd.None;
        voice.ReleasedBrrCursor = null;
        voice.Source = new VoiceSource.Keyed(sampleBank.Resolve(voice.SourceNumber));
        voice.SampleCursor = 0;
        Array.Clear(voice.DecodeBuffer);
        // Hardware key-on restarts the interpolation position, so every note starts its waveform
        // at the same phase. The envelope counter restarts with it, timing envelope steps from
        // the key-on rather than from whatever the voice's previous note left.
        voice.PitchCounter = 0;
        voice.RateCounter = 0;
        voice.Gain = 0;
        voice.AdsrState = voice.Envelope == EnvelopeSource.Gain ? EnvelopeState.Gain : EnvelopeState.Attack;
    }

    /// <summary>Advances one voice's pitch, decoded window, interpolation, and envelope output.</summary>
    private void CycleVoice(int index)
    {
        Voice voice = voices[index];
        int pitch = voice.Pitch;
        if (index > 0 && voice.PitchModulation)
        {
            int factor = (voices[index - 1].SampleOutput >> 4) + SnesDspRegisterLayout.PitchModulationBias;
            pitch = Math.Min(SnesDspRegisterLayout.Fields.PitchMask, (pitch * factor) >> 10);
        }
        VoiceSource source = voice.Source;
        // A stock 32-kHz WAV is numerically identical to decoded BRR. Scaling the phase step
        // lets an HD replacement use a different sample rate without changing musical pitch.
        long scaledPitch = source is not VoiceSource.Keyed { Sample: var sampleSource }
            ? pitch
            : ((long)pitch * sampleSource.SampleRate + InstrumentSampleFormat.StockSampleRate / 2) /
                InstrumentSampleFormat.StockSampleRate;
        long nextCounter = voice.PitchCounter + scaledPitch;
        while (nextCounter > ushort.MaxValue)
        {
            if (source is not VoiceSource.Keyed keyed)
            {
                throw new InvalidOperationException(
                    $"S-DSP voice {index} advanced without a key-on PCM sample.");
            }
            DecodePcm(index, keyed.Sample);
            nextCounter -= ushort.MaxValue + 1L;
        }
        voice.PitchCounter = unchecked((ushort)nextCounter);
        short sample = voice.UseNoise
            ? noiseSample
            : source is VoiceSource.Silent
                ? (short)0
                : GetInterpolatedSample(voice, voice.PitchCounter >> 12, unchecked((byte)(voice.PitchCounter >> 4)));

        if (reset)
        {
            voice.AdsrState = EnvelopeState.Release;
            voice.Gain = 0;
        }
        bool direct = voice.AdsrState != EnvelopeState.Release && voice.Envelope == EnvelopeSource.Gain && voice.GainForm == GainForm.Direct;
        ushort rate = voice.AdsrState == EnvelopeState.Release
            ? (ushort)0
            : voice.RateOf(voice.AdsrState);
        if (voice.AdsrState != EnvelopeState.Release && !direct && rate != 0)
            voice.RateCounter = SpcDriverWrap.Word(voice.RateCounter + 1);
        if (voice.AdsrState == EnvelopeState.Release ||
            (!direct && rate != 0 && voice.RateCounter >= rate))
        {
            if (voice.AdsrState != EnvelopeState.Release)
                voice.RateCounter = 0;
            HandleGain(voice);
        }
        if (direct)
            voice.Gain = voice.GainValue;
        registers[(index * SnesDspRegisterLayout.VoiceStride) |
            SnesDspRegisterLayout.Voice.EnvelopeOutput] = unchecked((byte)(voice.Gain >> 4));
        sample = unchecked((short)((sample * voice.Gain) >> 11));
        registers[(index * SnesDspRegisterLayout.VoiceStride) |
            SnesDspRegisterLayout.Voice.SampleOutput] = unchecked((byte)(sample >> 7));
        voice.SampleOutput = sample;
    }

    /// <summary>Applies one attack, decay, sustain, gain-mode, or release envelope step.</summary>
    private static void HandleGain(Voice voice)
    {
        switch (voice.AdsrState)
        {
            case EnvelopeState.Attack:
                voice.Gain = unchecked((ushort)(voice.Gain +
                    (voice.AttackRate == 1 ? 1024 : 32)));
                if (voice.Gain >= SnesDspRegisterLayout.AttackEndLevel)
                    voice.AdsrState = EnvelopeState.Decay;
                if (voice.Gain > SnesDspRegisterLayout.EnvelopeMaximum)
                    voice.Gain = SnesDspRegisterLayout.EnvelopeMaximum;
                break;
            case EnvelopeState.Decay:
                voice.Gain = unchecked((ushort)(voice.Gain - ExponentialDecayStep(voice.Gain)));
                if (voice.Gain < voice.SustainLevel)
                    voice.AdsrState = EnvelopeState.Sustain;
                break;
            case EnvelopeState.Sustain:
                voice.Gain = unchecked((ushort)(voice.Gain - ExponentialDecayStep(voice.Gain)));
                break;
            case EnvelopeState.Gain:
                switch (voice.GainMode)
                {
                    case SteppedGainMode.LinearDecrease:
                        voice.Gain = unchecked((ushort)(voice.Gain - 32));
                        if (voice.Gain > SnesDspRegisterLayout.EnvelopeMaximum)
                            voice.Gain = 0;
                        break;
                    case SteppedGainMode.ExponentialDecrease:
                        voice.Gain = unchecked((ushort)(voice.Gain - ExponentialDecayStep(voice.Gain)));
                        break;
                    case SteppedGainMode.LinearIncrease:
                        voice.Gain = unchecked((ushort)(voice.Gain + 32));
                        if (voice.Gain > SnesDspRegisterLayout.EnvelopeMaximum)
                            voice.Gain = SnesDspRegisterLayout.EnvelopeMaximum;
                        break;
                    case SteppedGainMode.BentLineIncrease:
                        voice.Gain = unchecked((ushort)(voice.Gain + (voice.Gain < SnesDspRegisterLayout.BentLineThreshold ? 32 : 8)));
                        if (voice.Gain > SnesDspRegisterLayout.EnvelopeMaximum)
                            voice.Gain = SnesDspRegisterLayout.EnvelopeMaximum;
                        break;
                    default:
                        throw new InvalidDataException($"Unknown S-DSP gain mode {voice.GainMode}.");
                }
                break;
            case EnvelopeState.Release:
                voice.Gain = unchecked((ushort)(voice.Gain - 8));
                if (voice.Gain > SnesDspRegisterLayout.EnvelopeMaximum)
                    voice.Gain = 0;
                break;
            default:
                throw new InvalidDataException($"Unknown S-DSP envelope state {voice.AdsrState}.");
        }
    }

    /// <summary>Interpolates four neighboring PCM samples with the native Gaussian tap table.</summary>
    private static short GetInterpolatedSample(Voice voice, int sampleNumber, int offset)
    {
        var taps = SnesDspTables.GaussianCoefficients(offset);
        int output = (taps[0] * voice.DecodeBuffer[sampleNumber]) >> 10;
        output += (taps[1] * voice.DecodeBuffer[sampleNumber + 1]) >> 10;
        output += (taps[2] * voice.DecodeBuffer[sampleNumber + 2]) >> 10;
        output = unchecked((short)output);
        output += (taps[3] * voice.DecodeBuffer[sampleNumber + 3]) >> 10;
        return unchecked((short)(Clamp16(output) >> 1));
    }

    /// <summary>Refills a voice's PCM window and follows live loop-directory changes at BRR end.</summary>
    private void DecodePcm(int voiceIndex, ManagedPcmSample keyed)
    {
        Voice voice = voices[voiceIndex];
        if (voice.ReleasedBrrCursor is int releasedCursor)
        {
            AdvanceReleasedBrr(voiceIndex, releasedCursor);
            return;
        }
        ManagedPcmSample sample = keyed;
        voice.DecodeBuffer[0] = voice.DecodeBuffer[16];
        voice.DecodeBuffer[1] = voice.DecodeBuffer[17];
        voice.DecodeBuffer[2] = voice.DecodeBuffer[18];
        if (voice.PreviousFlags.HasFlag(BrrBlockEnd.End))
        {
            // END/LOOP re-reads DIR + SRCN in the native decoder, without a new
            // KON. The sound driver can restore an instrument during release.
            // Preserve the old interpolation window above, then follow the live
            // source for the next window rather than retaining the key-on source.
            ManagedPcmSampleBank bank = sampleBank;
            if (!bank.Samples.ContainsKey(voice.SourceNumber) &&
                voice.AdsrState == EnvelopeState.Release && voice.Gain == 0)
            {
                // Uploads can invalidate a retired voice's DIR entry ($FFFF in the
                // narration bank). The hardware continues reading RAM, but release
                // at zero gain cannot become audible again without KON. Follow its
                // block headers/ENDX without requiring a nonexistent playable WAV.
                // Audible voices still require a real, replaceable sample mapping.
                ushort loopCursor = ReadLoopAddress(voice.SourceNumber);
                voice.ReleasedBrrCursor = loopCursor;
                AdvanceReleasedBrr(voiceIndex, loopCursor);
                return;
            }
            (sample, voice.SampleCursor) = bank.ResolveLoopEntry(voice.SourceNumber);
            voice.Source = new VoiceSource.Keyed(sample);
            if (voice.PreviousFlags == BrrBlockEnd.End)
            {
                voice.AdsrState = EnvelopeState.Release;
                voice.Gain = 0;
            }
            registers[endFlagsAddress] |= unchecked((byte)(1 << voiceIndex));
        }

        ReadOnlySpan<short> source = sample.Samples.Span;
        voice.PreviousFlags = BrrBlockEnd.None;
        for (int sampleIndex = 0; sampleIndex < InstrumentSampleFormat.StreamingWindowSampleCount; sampleIndex++)
        {
            if (voice.SampleCursor >= source.Length)
            {
                if (sample.LoopSampleIndex is int loop)
                {
                    voice.SampleCursor = loop;
                    registers[endFlagsAddress] |=
                        unchecked((byte)(1 << voiceIndex));
                }
                else
                {
                    voice.PreviousFlags = BrrBlockEnd.End;
                    voice.DecodeBuffer[sampleIndex + 3] = 0;
                    continue;
                }
            }
            voice.DecodeBuffer[sampleIndex + 3] = source[voice.SampleCursor++];
        }
        if (voice.SampleCursor == source.Length)
            voice.PreviousFlags = sample.LoopSampleIndex.HasValue ? BrrBlockEnd.End | BrrBlockEnd.Loop : BrrBlockEnd.End;
    }

    /// <summary>Reads a source's loop pointer from the live BRR directory in audio RAM.</summary>
    private ushort ReadLoopAddress(int source)
    {
        int directory = LittleEndianBytes.Word(0, registers[sourceDirectoryAddress]) +
            source * DspBrrLayout.DirectoryEntryBytes + DspBrrLayout.LoopPointerOffset;
        return SpcRamWords.Read(apuRam, directory);
    }

    /// <summary>
    /// Preserves native BRR address wrapping and ENDX for an irreversibly silent
    /// released voice. Predictor samples cannot affect output, echo or pitch modulation
    /// at zero gain; KON clears this state and starts a fully validated PCM voice.
    /// </summary>
    private void AdvanceReleasedBrr(int index, int cursor)
    {
        Voice voice = voices[index];
        if (voice.PreviousFlags.HasFlag(BrrBlockEnd.End))
        {
            cursor = ReadLoopAddress(voice.SourceNumber);
            registers[endFlagsAddress] |= unchecked((byte)(1 << index));
        }
        voice.PreviousFlags = DspBrrLayout.EndOf(apuRam[cursor]);
        voice.ReleasedBrrCursor = SpcDriverWrap.Word(cursor + DspBrrLayout.BlockBytes);
    }

    /// <summary>Advances the noise shift register at the configured DSP clock period.</summary>
    private void HandleNoise()
    {
        if (noiseRate != 0)
            noiseCounter = SpcDriverWrap.Word(noiseCounter + 1);
        if (noiseRate == 0 || noiseCounter < noiseRate)
            return;
        int bit = (noiseSample & 1) ^ ((noiseSample >> 1) & 1);
        int next = ((noiseSample >> 1) & SnesDspRegisterLayout.NoiseShiftMask) | (bit << 14);
        noiseSample = unchecked((short)((unchecked((short)((next & SnesDspRegisterLayout.NoiseSampleMask) << 1))) >> 1));
        noiseCounter = 0;
    }

    /// <summary>Filters echo history, mixes it into both channels, and optionally writes feedback to RAM.</summary>
    private void HandleEcho(ref int outputLeft, ref int outputRight)
    {
        ushort address = unchecked((ushort)(echoBufferAddress + echoBufferIndex * 4));
        // Echo RAM contains signed PCM. Cast before shifting so negative samples use an
        // arithmetic shift, exactly matching the cartridge DSP implementation.
        firBufferLeft[firBufferIndex] = unchecked((short)(unchecked((short)ReadWord(address)) >> 1));
        firBufferRight[firBufferIndex] = unchecked((short)(
            unchecked((short)ReadWord(unchecked((ushort)(address + 2)))) >> 1));
        int sumLeft = 0;
        int sumRight = 0;
        for (int tap = 0; tap < 8; tap++)
        {
            int index = (firBufferIndex + tap + 1) & 7;
            sumLeft += (firBufferLeft[index] * firValues[tap]) >> 6;
            sumRight += (firBufferRight[index] * firValues[tap]) >> 6;
            if (tap == 6)
            {
                sumLeft = unchecked((short)sumLeft);
                sumRight = unchecked((short)sumRight);
            }
        }
        sumLeft = Clamp16(sumLeft);
        sumRight = Clamp16(sumRight);
        outputLeft = Clamp16(outputLeft + ((sumLeft * echoVolumeLeft) >> 7));
        outputRight = Clamp16(outputRight + ((sumRight * echoVolumeRight) >> 7));

        int inputLeft = 0;
        int inputRight = 0;
        foreach (Voice voice in voices)
        {
            if (!voice.EchoEnable)
                continue;
            inputLeft = Clamp16(inputLeft + ((voice.SampleOutput * voice.VolumeLeft) >> 6));
            inputRight = Clamp16(inputRight + ((voice.SampleOutput * voice.VolumeRight) >> 6));
        }
        inputLeft = Clamp16(inputLeft + ((sumLeft * feedbackVolume) >> 7)) & ~1;
        inputRight = Clamp16(inputRight + ((sumRight * feedbackVolume) >> 7)) & ~1;
        if (echoWrites)
        {
            WriteWord(address, unchecked((ushort)inputLeft));
            WriteWord(unchecked((ushort)(address + 2)), unchecked((ushort)inputRight));
        }
        firBufferIndex = unchecked((byte)((firBufferIndex + 1) & 7));
        echoBufferIndex = SpcDriverWrap.Word(echoBufferIndex + 1);
        echoRemaining = SpcDriverWrap.Word(echoRemaining - 1);
        if (echoRemaining == 0)
        {
            echoRemaining = echoDelay;
            echoBufferIndex = 0;
        }
    }

    /// <summary>Reads a wrapped little-endian DSP word from shared audio RAM.</summary>
    private ushort ReadWord(int address) => SpcRamWords.Read(apuRam, address);

    /// <summary>Writes a wrapped little-endian echo word into shared audio RAM.</summary>
    private void WriteWord(int address, ushort value) => SpcRamWords.Write(apuRam, address, value);

    /// <summary>
    /// How far an exponential envelope falls in one step from <paramref name="gain"/>: a 256th of the level below it,
    /// rounded toward negative infinity, plus one, so a level of zero stays where it is.
    /// </summary>
    private static int ExponentialDecayStep(int gain) => ((gain - 1) >> 8) + 1;

    /// <summary>Clamps an intermediate mix or echo sum to signed 16-bit amplitude.</summary>
    private static int Clamp16(int value) => Math.Clamp(value, short.MinValue, short.MaxValue);

    /// <summary>The stepped gain mode a GAIN register byte's mode field selects (#2560), decoded once at the write.</summary>
    private static SteppedGainMode SteppedGainModeOf(int value) =>
        ((value & SnesDspRegisterLayout.Fields.GainModeMask) >> SnesDspRegisterLayout.Fields.GainModeShift) switch
        {
            0 => SteppedGainMode.LinearDecrease,
            1 => SteppedGainMode.ExponentialDecrease,
            2 => SteppedGainMode.LinearIncrease,
            3 => SteppedGainMode.BentLineIncrease,
            var field => throw new InvalidDataException($"Unknown S-DSP gain mode {field}."),
        };

    /// <summary>Which envelope a voice runs at key-on, as ADSR1's enable bit selects (#2560).</summary>
    private enum EnvelopeSource : byte
    {
        /// <summary>The attack, decay and sustain rates.</summary>
        Adsr,
        /// <summary>The GAIN register's direct level or stepped mode.</summary>
        Gain,
    }

    /// <summary>Which form the GAIN register's top bit selects (#2560).</summary>
    private enum GainForm : byte
    {
        /// <summary>A constant level applied at once.</summary>
        Direct,
        /// <summary>A timed step in one of the four stepped modes.</summary>
        Stepped,
    }

    /// <summary>The four stepped GAIN modes (#2560).</summary>
    private enum SteppedGainMode : byte
    {
        /// <summary>Lowers the envelope by a fixed step, stopping at silence.</summary>
        LinearDecrease,
        /// <summary>Lowers the envelope by a step proportional to its level.</summary>
        ExponentialDecrease,
        /// <summary>Raises the envelope by a fixed step, stopping at its maximum.</summary>
        LinearIncrease,
        /// <summary>Raises the envelope quickly, then slowly past the bent-line threshold.</summary>
        BentLineIncrease,
    }

    /// <summary>The S-DSP envelope phases selected by ADSR, gain mode, and key-off.</summary>
    private enum EnvelopeState : byte
    {
        /// <summary>Raises gain after a key-on until the decay threshold.</summary>
        Attack,
        /// <summary>Reduces gain toward the configured sustain level.</summary>
        Decay,
        /// <summary>Continues timed exponential reduction at the sustain rate.</summary>
        Sustain,
        /// <summary>Uses the direct or stepped GAIN register behavior.</summary>
        Gain,
        /// <summary>Reduces gain by eight on every sample cycle after key-off.</summary>
        Release,
    }

    /// <summary>What a voice plays: nothing since reset, or the decoded source its last key-on or loop bound.</summary>
    private abstract record VoiceSource
    {
        /// <summary>No source since reset: the voice outputs silence and has no block to decode.</summary>
        internal sealed record Silent : VoiceSource
        {
            /// <summary>The one silent source.</summary>
            internal static Silent Instance { get; } = new();
        }

        /// <summary>The decoded source a key-on or loop bound.</summary>
        /// <param name="Sample">The decoded PCM.</param>
        internal sealed record Keyed(ManagedPcmSample Sample) : VoiceSource;
    }

    /// <summary>One hardware voice's interpolation window, pitch, envelope, and routing state.</summary>
    private sealed class Voice
    {
        /// <summary>Fourteen-bit playback pitch written through the two voice registers.</summary>
        public int Pitch;
        /// <summary>Fractional sample cursor advanced by scaled pitch each DSP cycle.</summary>
        public int PitchCounter;
        /// <summary>Whether the preceding voice's output modulates this pitch.</summary>
        public bool PitchModulation;
        /// <summary>Three retained samples plus the next sixteen-sample interpolation window.</summary>
        public short[] DecodeBuffer { get; } = new short[19];
        /// <summary>Live BRR directory source number used at key-on and loop boundaries.</summary>
        public int SourceNumber;
        /// <summary>The decoded PCM source bound to this voice, or none since reset.</summary>
        public VoiceSource Source = VoiceSource.Silent.Instance;
        /// <summary>Next sample index within the bound PCM source.</summary>
        public int SampleCursor;
        /// <summary>How the previous window's last block ended its sample, read when the next window starts.</summary>
        public BrrBlockEnd PreviousFlags;
        /// <summary>BRR header cursor for a silent released voice whose PCM mapping vanished.</summary>
        public int? ReleasedBrrCursor;
        /// <summary>Routes the shared noise sample into this voice instead of PCM.</summary>
        public bool UseNoise;
        /// <summary>The attack's rate period, from ADSR1.</summary>
        public ushort AttackRate;
        /// <summary>The decay's rate period, from ADSR1.</summary>
        public ushort DecayRate;
        /// <summary>The sustain's rate period, from ADSR2.</summary>
        public ushort SustainRate;
        /// <summary>The stepped GAIN mode's rate period, from GAIN.</summary>
        public ushort GainRate;
        /// <summary>Cycles accumulated toward the next non-release envelope step.</summary>
        public int RateCounter;
        /// <summary>Current attack, decay, sustain, gain, or release phase.</summary>
        public EnvelopeState AdsrState;
        /// <summary>Gain threshold at which decay changes to sustain.</summary>
        public int SustainLevel;
        /// <summary>Which envelope the voice runs at key-on, from ADSR1 (#2560).</summary>
        public EnvelopeSource Envelope;
        /// <summary>The stepped GAIN mode the GAIN register last selected; kept while the register holds a direct level.</summary>
        public SteppedGainMode GainMode;
        /// <summary>Whether the GAIN register holds a direct level or a stepped mode (#2560).</summary>
        public GainForm GainForm;
        /// <summary>Constant envelope level the GAIN register last selected; kept while the register holds a stepped mode.</summary>
        public int GainValue;
        /// <summary>Current eleven-bit envelope amplitude.</summary>
        public int Gain;
        /// <summary>Interpolated sample after envelope scaling, used for mixing and pitch modulation.</summary>
        public short SampleOutput;
        /// <summary>Signed left-channel voice volume.</summary>
        public sbyte VolumeLeft;
        /// <summary>Signed right-channel voice volume.</summary>
        public sbyte VolumeRight;
        /// <summary>Includes this voice in the shared echo input.</summary>
        public bool EchoEnable;

        /// <summary>
        /// The rate period of a timed envelope phase (#2567): each phase names its own rate, so no rate is found by the
        /// phase's place in a list.
        /// </summary>
        /// <exception cref="InvalidDataException">The phase is the release, which steps every cycle and has no rate.</exception>
        public ushort RateOf(EnvelopeState phase) => phase switch
        {
            EnvelopeState.Attack => AttackRate,
            EnvelopeState.Decay => DecayRate,
            EnvelopeState.Sustain => SustainRate,
            EnvelopeState.Gain => GainRate,
            EnvelopeState.Release => throw new InvalidDataException("The S-DSP release steps every cycle and has no rate."),
            _ => throw new InvalidDataException($"Unknown S-DSP envelope phase {phase}."),
        };

        /// <summary>Clears all voice registers, PCM cursors, and envelope state for DSP reset.</summary>
        public void Reset()
        {
            Pitch = PitchCounter = 0;
            PitchModulation = false;
            Array.Clear(DecodeBuffer);
            SourceNumber = 0;
            Source = VoiceSource.Silent.Instance;
            SampleCursor = 0;
            PreviousFlags = BrrBlockEnd.None;
            ReleasedBrrCursor = null;
            UseNoise = false;
            AttackRate = DecayRate = SustainRate = GainRate = 0;
            RateCounter = 0;
            AdsrState = EnvelopeState.Attack;
            SustainLevel = 0;
            Envelope = EnvelopeSource.Adsr;
            GainMode = SteppedGainMode.LinearDecrease;
            GainForm = GainForm.Stepped;
            GainValue = Gain = 0;
            SampleOutput = 0;
            VolumeLeft = VolumeRight = 0;
            EchoEnable = false;
        }
    }
}
