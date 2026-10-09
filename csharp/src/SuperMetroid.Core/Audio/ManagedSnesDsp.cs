namespace SuperMetroid.Core.Audio;

/// <summary>
/// Managed implementation of the eight-voice S-DSP behavior used by Super Metroid.
/// It consumes replaceable decoded PCM while retaining cartridge-accurate Gaussian
/// interpolation, envelopes, pitch/noise modulation, and the shared FIR echo buffer. The
/// music driver continues to write the ordinary DSP registers, so samples remain data rather
/// than pre-rendered sound effects.
/// </summary>
public sealed class ManagedSnesDsp
{
    /// <summary>Native 32.04-kHz stereo frames accumulated for each 60-Hz gameplay update.</summary>
    public const int NativeStereoFramesPerVideoFrame = 534;
    /// <summary>Number of independent hardware S-DSP voices.</summary>
    public const int VoiceCount = 8;

    /// <summary>SPC RAM consulted for directory pointers, echo data, and uploaded sound bytes.</summary>
    private readonly byte[] apuRam;
    /// <summary>Mirrored 128-byte S-DSP register file, including read-only output registers.</summary>
    private readonly byte[] registers = new byte[SnesDspRegisterMap.RegisterFileSize];
    /// <summary>Mutable state for each of the eight hardware voices.</summary>
    private readonly Voice[] voices = Enumerable.Range(0, VoiceCount)
        .Select(_ => new Voice())
        .ToArray();
    /// <summary>Interleaved native-rate PCM accumulated before host-rate resampling.</summary>
    private readonly short[] sampleBuffer = new short[NativeStereoFramesPerVideoFrame * 2];
    /// <summary>Current signed coefficients of the shared eight-tap echo filter.</summary>
    private readonly sbyte[] firValues = new sbyte[8];
    /// <summary>Left-channel delay samples feeding the shared FIR filter.</summary>
    private readonly short[] firBufferLeft = new short[8];
    /// <summary>Right-channel delay samples feeding the shared FIR filter.</summary>
    private readonly short[] firBufferRight = new short[8];

    /// <summary>Sample bank resolved by key-on and subsequent live directory reads.</summary>
    private ManagedPcmSampleBank? sampleBank;
    /// <summary>FLG output-mute latch.</summary>
    private bool mute;
    /// <summary>FLG reset latch, which forces all voice envelopes into release.</summary>
    private bool reset;
    /// <summary>Signed master output gain for the left channel.</summary>
    private sbyte masterVolumeLeft;
    /// <summary>Signed master output gain for the right channel.</summary>
    private sbyte masterVolumeRight;
    /// <summary>Current pseudo-random noise sample shared by voices.</summary>
    private short noiseSample;
    /// <summary>DSP-clock interval between noise generator updates; zero disables the clock.</summary>
    private ushort noiseRate;
    /// <summary>Elapsed DSP clocks toward the next noise update.</summary>
    private ushort noiseCounter;
    /// <summary>Whether echo samples may be written back into APU RAM.</summary>
    private bool echoWrites;
    /// <summary>Signed left echo output gain.</summary>
    private sbyte echoVolumeLeft;
    /// <summary>Signed right echo output gain.</summary>
    private sbyte echoVolumeRight;
    /// <summary>Signed feedback gain mixed into the next echo-buffer write.</summary>
    private sbyte feedbackVolume;
    /// <summary>Current echo-ring base address in APU RAM.</summary>
    private ushort echoBufferAddress;
    /// <summary>Echo-ring length in DSP processing frames.</summary>
    private ushort echoDelay;
    /// <summary>Frames remaining before the echo-ring cursor wraps to its base.</summary>
    private ushort echoRemaining;
    /// <summary>Current interleaved stereo frame offset within the echo ring.</summary>
    private ushort echoBufferIndex;
    /// <summary>Next slot in the circular eight-sample FIR history.</summary>
    private byte firBufferIndex;
    /// <summary>Number of stereo frames currently staged in the native output buffer.</summary>
    private ushort sampleOffset;

    /// <summary>Creates a reset DSP bound to the supplied complete 64-KiB APU RAM image.</summary>
    public ManagedSnesDsp(byte[] apuRam)
    {
        ArgumentNullException.ThrowIfNull(apuRam);
        if (apuRam.Length != 0x10000)
            throw new ArgumentException("S-DSP requires exactly 64 KiB of APU RAM.", nameof(apuRam));
        this.apuRam = apuRam;
        Reset();
    }

    /// <summary>Native stereo frames accumulated since the last host-buffer copy.</summary>
    public int BufferedStereoFrameCount => sampleOffset;

    /// <summary>Restores the hardware-facing DSP state used by the translated SPC reset vector.</summary>
    public void Reset()
    {
        Array.Clear(registers);
        registers[SnesDspRegisterMap.Global.EndFlags] = byte.MaxValue;
        foreach (Voice voice in voices)
            voice.Reset();
        mute = true;
        reset = true;
        masterVolumeLeft = 0;
        masterVolumeRight = 0;
        noiseSample = -0x4000;
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
        sampleOffset = 0;
    }

    /// <summary>
    /// Installs the source-number map associated with the latest cartridge upload. Existing
    /// voices retain their current window; key-ons and subsequent loop-directory reads
    /// resolve through this bank.
    /// </summary>
    public void SetSampleBank(ManagedPcmSampleBank bank) =>
        sampleBank = bank ?? throw new ArgumentNullException(nameof(bank));

    /// <summary>Applies one SPC write to the mirrored S-DSP register file.</summary>
    public void WriteRegister(byte address, byte value)
    {
        if (address >= SnesDspRegisterMap.RegisterFileSize)
            throw new ArgumentOutOfRangeException(nameof(address), address, "DSP register is outside $00-$7F.");
        int voiceIndex = (address / SnesDspRegisterMap.VoiceStride) & SnesDspRegisterMap.VoiceIndexMask;
        Voice voice = voices[voiceIndex];
        int voiceRegister = address & SnesDspRegisterMap.VoiceRegisterMask;
        if (voiceRegister <= SnesDspRegisterMap.Voice.LastWritable)
        {
            switch (voiceRegister)
            {
            case SnesDspRegisterMap.Voice.VolumeLeft:
                voice.VolumeLeft = unchecked((sbyte)value);
                break;
            case SnesDspRegisterMap.Voice.VolumeRight:
                voice.VolumeRight = unchecked((sbyte)value);
                break;
            case SnesDspRegisterMap.Voice.PitchLow:
                voice.Pitch = unchecked((ushort)((voice.Pitch &
                    (SnesDspRegisterMap.Fields.PitchHighMask << 8)) | value));
                break;
            case SnesDspRegisterMap.Voice.PitchHigh:
                voice.Pitch = unchecked((ushort)(((voice.Pitch & byte.MaxValue) | (value << 8)) &
                    SnesDspRegisterMap.Fields.PitchMask));
                break;
            case SnesDspRegisterMap.Voice.SourceNumber:
                voice.SourceNumber = value;
                break;
            case SnesDspRegisterMap.Voice.Adsr1:
                voice.AdsrRates[0] = SnesDspTables.RatePeriod(
                    (value & SnesDspRegisterMap.Fields.AdsrAttackMask) * 2 + 1);
                voice.AdsrRates[1] = SnesDspTables.RatePeriod(
                    ((value & SnesDspRegisterMap.Fields.AdsrDecayMask) >>
                        SnesDspRegisterMap.Fields.AdsrDecayShift) * 2 + 16);
                voice.UseGain = (value & SnesDspRegisterMap.Fields.AdsrEnabled) == 0;
                break;
            case SnesDspRegisterMap.Voice.Adsr2:
                voice.AdsrRates[2] = SnesDspTables.RatePeriod(
                    value & SnesDspRegisterMap.Fields.AdsrSustainRateMask);
                voice.SustainLevel = unchecked((ushort)((((value &
                    SnesDspRegisterMap.Fields.AdsrSustainLevelMask) >>
                    SnesDspRegisterMap.Fields.AdsrSustainLevelShift) + 1) * 0x100));
                break;
            case SnesDspRegisterMap.Voice.Gain:
                voice.DirectGain = (value & SnesDspRegisterMap.Fields.AdsrEnabled) == 0;
                if ((value & SnesDspRegisterMap.Fields.AdsrEnabled) != 0)
                {
                    voice.GainMode = unchecked((byte)((value & SnesDspRegisterMap.Fields.GainModeMask) >>
                        SnesDspRegisterMap.Fields.GainModeShift));
                    voice.AdsrRates[3] = SnesDspTables.RatePeriod(
                        value & SnesDspRegisterMap.Fields.AdsrSustainRateMask);
                }
                else
                {
                    voice.GainValue = unchecked((ushort)((value &
                        SnesDspRegisterMap.Fields.GainValueMask) * 16));
                }
                break;
            }
        }

        switch (address)
        {
            case SnesDspRegisterMap.Global.MasterVolumeLeft:
                masterVolumeLeft = unchecked((sbyte)value);
                break;
            case SnesDspRegisterMap.Global.MasterVolumeRight:
                masterVolumeRight = unchecked((sbyte)value);
                break;
            case SnesDspRegisterMap.Global.EchoVolumeLeft:
                echoVolumeLeft = unchecked((sbyte)value);
                break;
            case SnesDspRegisterMap.Global.EchoVolumeRight:
                echoVolumeRight = unchecked((sbyte)value);
                break;
            case SnesDspRegisterMap.Global.KeyOn:
                for (int index = 0; index < VoiceCount; index++)
                {
                    if ((value & (1 << index)) != 0)
                        KeyOn(voices[index]);
                }
                break;
            case SnesDspRegisterMap.Global.KeyOff:
                for (int index = 0; index < VoiceCount; index++)
                {
                    if ((value & (1 << index)) != 0)
                        voices[index].AdsrState = EnvelopeState.Release;
                }
                break;
            case SnesDspRegisterMap.Global.Flags:
                reset = (value & SnesDspRegisterMap.Fields.Reset) != 0;
                mute = (value & SnesDspRegisterMap.Fields.Mute) != 0;
                echoWrites = (value & SnesDspRegisterMap.Fields.EchoWriteDisable) == 0;
                noiseRate = SnesDspTables.RatePeriod(value & SnesDspRegisterMap.Fields.NoiseRateMask);
                break;
            case SnesDspRegisterMap.Global.EndFlags:
                value = 0;
                break;
            case SnesDspRegisterMap.Global.EchoFeedback:
                feedbackVolume = unchecked((sbyte)value);
                break;
            case SnesDspRegisterMap.Global.PitchModulation:
                for (int index = 0; index < VoiceCount; index++)
                    voices[index].PitchModulation = (value & (1 << index)) != 0;
                break;
            case SnesDspRegisterMap.Global.NoiseEnable:
                for (int index = 0; index < VoiceCount; index++)
                    voices[index].UseNoise = (value & (1 << index)) != 0;
                break;
            case SnesDspRegisterMap.Global.EchoEnable:
                for (int index = 0; index < VoiceCount; index++)
                    voices[index].EchoEnable = (value & (1 << index)) != 0;
                break;
            case SnesDspRegisterMap.Global.SourceDirectory:
                // The extracted catalog has already resolved this BRR directory into stable
                // source IDs. Preserve the register mirror because software can read it back.
                break;
            case SnesDspRegisterMap.Global.EchoBufferAddress:
                echoBufferAddress = unchecked((ushort)(value << 8));
                break;
            case SnesDspRegisterMap.Global.EchoDelay:
                echoDelay = unchecked((ushort)((value & SnesDspRegisterMap.Fields.EchoDelayMask) * 512));
                if (echoDelay == 0)
                    echoDelay = 1;
                break;
            case var firAddress when
                (firAddress & SnesDspRegisterMap.VoiceRegisterMask) ==
                    SnesDspRegisterMap.Global.FirstFirCoefficient:
                firValues[voiceIndex] = unchecked((sbyte)value);
                break;
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
        if (sampleOffset < NativeStereoFramesPerVideoFrame)
        {
            sampleBuffer[sampleOffset * 2] = unchecked((short)totalLeft);
            sampleBuffer[sampleOffset * 2 + 1] = unchecked((short)totalRight);
            sampleOffset++;
        }
    }

    /// <summary>
    /// Converts the native 32.04-kHz frame to the host rate without zero-order-hold imaging.
    /// </summary>
    public void CopyResampledSamples(Span<short> destination, int stereoFrameCount)
    {
        if (stereoFrameCount <= 0 || destination.Length < stereoFrameCount * 2)
            throw new ArgumentOutOfRangeException(nameof(stereoFrameCount));
        PcmFrameResampler.ResampleStereoLinear(
            sampleBuffer,
            destination[..(stereoFrameCount * 2)]);
        sampleOffset = 0;
    }

    /// <summary>Resolves the selected sample and initializes interpolation and envelope state for a newly keyed voice.</summary>
    private void KeyOn(Voice voice)
    {
        ManagedPcmSampleBank bank = sampleBank ?? throw new InvalidOperationException(
            "S-DSP received key-on before an extracted PCM sample bank was installed.");
        voice.PreviousFlags = 0;
        voice.ReleasedBrrCursor = null;
        voice.Sample = bank.Resolve(voice.SourceNumber);
        voice.SampleCursor = 0;
        Array.Clear(voice.DecodeBuffer);
        voice.Gain = 0;
        voice.AdsrState = voice.UseGain ? EnvelopeState.Gain : EnvelopeState.Attack;
    }

    /// <summary>Advances one voice's pitch phase, sample window, envelope, and output-register values.</summary>
    private void CycleVoice(int index)
    {
        Voice voice = voices[index];
        int pitch = voice.Pitch;
        if (index > 0 && voice.PitchModulation)
        {
            int factor = (voices[index - 1].SampleOutput >> 4) + 0x400;
            pitch = Math.Min(0x3fff, (pitch * factor) >> 10);
        }
        ManagedPcmSample? sampleSource = voice.Sample;
        // A stock 32-kHz WAV is numerically identical to decoded BRR. Scaling the phase step
        // lets an HD replacement use a different sample rate without changing musical pitch.
        long scaledPitch = sampleSource is null
            ? pitch
            : ((long)pitch * sampleSource.SampleRate + PcmSampleFormat.StockSampleRate / 2) /
                PcmSampleFormat.StockSampleRate;
        long nextCounter = voice.PitchCounter + scaledPitch;
        while (nextCounter > ushort.MaxValue)
        {
            if (sampleSource is null)
            {
                throw new InvalidOperationException(
                    $"S-DSP voice {index} advanced without a key-on PCM sample.");
            }
            DecodePcm(index);
            nextCounter -= ushort.MaxValue + 1L;
        }
        voice.PitchCounter = unchecked((ushort)nextCounter);
        short sample = voice.UseNoise
            ? noiseSample
            : sampleSource is null
                ? (short)0
                : GetInterpolatedSample(voice, voice.PitchCounter >> 12, (voice.PitchCounter >> 4) & 0xff);

        if (reset)
        {
            voice.AdsrState = EnvelopeState.Release;
            voice.Gain = 0;
        }
        bool direct = voice.AdsrState != EnvelopeState.Release && voice.UseGain && voice.DirectGain;
        ushort rate = voice.AdsrState == EnvelopeState.Release
            ? (ushort)0
            : voice.AdsrRates[(int)voice.AdsrState];
        if (voice.AdsrState != EnvelopeState.Release && !direct && rate != 0)
            voice.RateCounter++;
        if (voice.AdsrState == EnvelopeState.Release ||
            (!direct && rate != 0 && voice.RateCounter >= rate))
        {
            if (voice.AdsrState != EnvelopeState.Release)
                voice.RateCounter = 0;
            HandleGain(voice);
        }
        if (direct)
            voice.Gain = voice.GainValue;
        registers[(index * SnesDspRegisterMap.VoiceStride) |
            SnesDspRegisterMap.Voice.EnvelopeOutput] = unchecked((byte)(voice.Gain >> 4));
        sample = unchecked((short)((sample * voice.Gain) >> 11));
        registers[(index * SnesDspRegisterMap.VoiceStride) |
            SnesDspRegisterMap.Voice.SampleOutput] = unchecked((byte)(sample >> 7));
        voice.SampleOutput = sample;
    }

    /// <summary>Applies one scheduled ADSR, gain-mode, or release envelope step.</summary>
    private static void HandleGain(Voice voice)
    {
        switch (voice.AdsrState)
        {
            case EnvelopeState.Attack:
                voice.Gain = unchecked((ushort)(voice.Gain +
                    (voice.AdsrRates[(int)EnvelopeState.Attack] == 1 ? 1024 : 32)));
                if (voice.Gain >= 0x7e0)
                    voice.AdsrState = EnvelopeState.Decay;
                if (voice.Gain > 0x7ff)
                    voice.Gain = 0x7ff;
                break;
            case EnvelopeState.Decay:
                voice.Gain = unchecked((ushort)(voice.Gain - (((voice.Gain - 1) >> 8) + 1)));
                if (voice.Gain < voice.SustainLevel)
                    voice.AdsrState = EnvelopeState.Sustain;
                break;
            case EnvelopeState.Sustain:
                voice.Gain = unchecked((ushort)(voice.Gain - (((voice.Gain - 1) >> 8) + 1)));
                break;
            case EnvelopeState.Gain:
                switch (voice.GainMode)
                {
                    case 0:
                        voice.Gain = unchecked((ushort)(voice.Gain - 32));
                        if (voice.Gain > 0x7ff)
                            voice.Gain = 0;
                        break;
                    case 1:
                        voice.Gain = unchecked((ushort)(voice.Gain - (((voice.Gain - 1) >> 8) + 1)));
                        break;
                    case 2:
                        voice.Gain = unchecked((ushort)(voice.Gain + 32));
                        if (voice.Gain > 0x7ff)
                            voice.Gain = 0x7ff;
                        break;
                    case 3:
                        voice.Gain = unchecked((ushort)(voice.Gain + (voice.Gain < 0x600 ? 32 : 8)));
                        if (voice.Gain > 0x7ff)
                            voice.Gain = 0x7ff;
                        break;
                    default:
                        throw new InvalidDataException($"Unknown S-DSP gain mode {voice.GainMode}.");
                }
                break;
            case EnvelopeState.Release:
                voice.Gain = unchecked((ushort)(voice.Gain - 8));
                if (voice.Gain > 0x7ff)
                    voice.Gain = 0;
                break;
            default:
                throw new InvalidDataException($"Unknown S-DSP envelope state {voice.AdsrState}.");
        }
    }

    /// <summary>Combines four adjacent decoded samples with the S-DSP Gaussian coefficients.</summary>
    private static short GetInterpolatedSample(Voice voice, int sampleNumber, int offset)
    {
        var taps = SnesDspTables.GaussianCoefficients(offset);
        int output = (taps.Tap0 * voice.DecodeBuffer[sampleNumber]) >> 10;
        output += (taps.Tap1 * voice.DecodeBuffer[sampleNumber + 1]) >> 10;
        output += (taps.Tap2 * voice.DecodeBuffer[sampleNumber + 2]) >> 10;
        output = unchecked((short)output);
        output += (taps.Tap3 * voice.DecodeBuffer[sampleNumber + 3]) >> 10;
        return unchecked((short)(Clamp16(output) >> 1));
    }

    /// <summary>Refills one voice's four-tap interpolation window and follows native loop-directory behavior.</summary>
    private void DecodePcm(int voiceIndex)
    {
        Voice voice = voices[voiceIndex];
        if (voice.ReleasedBrrCursor.HasValue)
        {
            AdvanceReleasedBrr(voiceIndex);
            return;
        }
        ManagedPcmSample sample = voice.Sample ?? throw new InvalidOperationException(
            $"S-DSP voice {voiceIndex} cannot decode without a PCM sample.");
        voice.DecodeBuffer[0] = voice.DecodeBuffer[16];
        voice.DecodeBuffer[1] = voice.DecodeBuffer[17];
        voice.DecodeBuffer[2] = voice.DecodeBuffer[18];
        if (voice.PreviousFlags is 1 or 3)
        {
            // END/LOOP re-reads DIR + SRCN in the native decoder, without a new
            // KON. The sound driver can restore an instrument during release.
            // Preserve the old interpolation window above, then follow the live
            // source for the next window rather than retaining the key-on source.
            ManagedPcmSampleBank bank = sampleBank ?? throw new InvalidOperationException("PCM loop has no installed sample bank.");
            if (!bank.Samples.ContainsKey(voice.SourceNumber) &&
                voice.AdsrState == EnvelopeState.Release && voice.Gain == 0)
            {
                // Uploads can invalidate a retired voice's DIR entry ($FFFF in the
                // narration bank). The hardware continues reading RAM, but release
                // at zero gain cannot become audible again without KON. Follow its
                // block headers/ENDX without requiring a nonexistent playable WAV.
                // Audible voices still require a real, replaceable sample mapping.
                voice.ReleasedBrrCursor = ReadLoopAddress(voice.SourceNumber);
                AdvanceReleasedBrr(voiceIndex);
                return;
            }
            (sample, voice.SampleCursor) = bank.ResolveLoopEntry(voice.SourceNumber);
            voice.Sample = sample;
            if (voice.PreviousFlags == 1)
            {
                voice.AdsrState = EnvelopeState.Release;
                voice.Gain = 0;
            }
            registers[SnesDspRegisterMap.Global.EndFlags] |= unchecked((byte)(1 << voiceIndex));
        }

        ReadOnlySpan<short> source = sample.Samples.Span;
        voice.PreviousFlags = 0;
        for (int sampleIndex = 0; sampleIndex < PcmSampleFormat.StreamingWindowSampleCount; sampleIndex++)
        {
            if (voice.SampleCursor >= source.Length)
            {
                if (sample.LoopSampleIndex is int loop)
                {
                    voice.SampleCursor = loop;
                    registers[SnesDspRegisterMap.Global.EndFlags] |=
                        unchecked((byte)(1 << voiceIndex));
                }
                else
                {
                    voice.PreviousFlags = 1;
                    voice.DecodeBuffer[sampleIndex + 3] = 0;
                    continue;
                }
            }
            voice.DecodeBuffer[sampleIndex + 3] = source[voice.SampleCursor++];
        }
        if (voice.SampleCursor == source.Length)
            voice.PreviousFlags = sample.LoopSampleIndex.HasValue ? (byte)3 : (byte)1;
    }

    /// <summary>Reads the selected source's live little-endian DIR loop pointer from APU RAM.</summary>
    private ushort ReadLoopAddress(byte source)
    {
        int directory = (registers[SnesDspRegisterMap.Global.SourceDirectory] << 8) +
            source * DspBrrLayout.DirectoryEntryBytes + DspBrrLayout.LoopPointerOffset;
        return unchecked((ushort)(apuRam[directory & ushort.MaxValue] |
            apuRam[(directory + 1) & ushort.MaxValue] << 8));
    }

    /// <summary>
    /// Preserves native BRR address wrapping and ENDX for an irreversibly silent
    /// released voice. Predictor samples cannot affect output, echo or pitch modulation
    /// at zero gain; KON clears this state and starts a fully validated PCM voice.
    /// </summary>
    private void AdvanceReleasedBrr(int index)
    {
        Voice voice = voices[index];
        ushort cursor = voice.ReleasedBrrCursor!.Value;
        if ((voice.PreviousFlags & DspBrrLayout.EndFlag) != 0)
        {
            cursor = ReadLoopAddress(voice.SourceNumber);
            registers[SnesDspRegisterMap.Global.EndFlags] |= unchecked((byte)(1 << index));
        }
        voice.PreviousFlags = unchecked((byte)(apuRam[cursor] & DspBrrLayout.HeaderFlagsMask));
        voice.ReleasedBrrCursor = unchecked((ushort)(cursor + DspBrrLayout.BlockBytes));
    }

    /// <summary>Advances the shared 15-bit noise generator when its configured rate expires.</summary>
    private void HandleNoise()
    {
        if (noiseRate != 0)
            noiseCounter++;
        if (noiseRate == 0 || noiseCounter < noiseRate)
            return;
        int bit = (noiseSample & 1) ^ ((noiseSample >> 1) & 1);
        int next = ((noiseSample >> 1) & 0x3fff) | (bit << 14);
        noiseSample = unchecked((short)((unchecked((short)((next & 0x7fff) << 1))) >> 1));
        noiseCounter = 0;
    }

    /// <summary>Filters the echo ring, mixes its output, and optionally writes feedback back to APU RAM.</summary>
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
        echoBufferIndex++;
        echoRemaining--;
        if (echoRemaining == 0)
        {
            echoRemaining = echoDelay;
            echoBufferIndex = 0;
        }
    }

    /// <summary>Reads a little-endian word with 16-bit APU-RAM address wrapping.</summary>
    private ushort ReadWord(int address) => unchecked((ushort)(
        apuRam[address & 0xffff] | (apuRam[(address + 1) & 0xffff] << 8)));

    /// <summary>Writes a little-endian word with 16-bit APU-RAM address wrapping.</summary>
    private void WriteWord(int address, ushort value)
    {
        apuRam[address & 0xffff] = unchecked((byte)value);
        apuRam[(address + 1) & 0xffff] = unchecked((byte)(value >> 8));
    }

    /// <summary>Clamps an intermediate DSP result to signed PCM16 range.</summary>
    private static int Clamp16(int value) => Math.Clamp(value, short.MinValue, short.MaxValue);

    /// <summary>Envelope phases used by ADSR and direct-gain voice processing.</summary>
    private enum EnvelopeState : byte
    {
        /// <summary>Rising ADSR attack toward the decay threshold.</summary>
        Attack,
        /// <summary>Exponential decay toward the configured sustain level.</summary>
        Decay,
        /// <summary>Exponential sustain at the configured rate.</summary>
        Sustain,
        /// <summary>One of the hardware gain modes independent of ADSR.</summary>
        Gain,
        /// <summary>Fixed-rate envelope fall after key-off or reset.</summary>
        Release,
    }

    /// <summary>Per-voice register, decoder, pitch, envelope, and routing state.</summary>
    private sealed class Voice
    {
        /// <summary>Fourteen-bit programmed pitch step.</summary>
        public ushort Pitch;
        /// <summary>Fractional pitch accumulator that selects the current sample position.</summary>
        public ushort PitchCounter;
        /// <summary>Whether the previous voice modulates this voice's pitch.</summary>
        public bool PitchModulation;
        /// <summary>History window used by four-point Gaussian interpolation.</summary>
        public short[] DecodeBuffer { get; } = new short[19];
        /// <summary>Current sample-directory source number.</summary>
        public byte SourceNumber;
        /// <summary>PCM source selected by the last key-on or loop-directory resolution.</summary>
        public ManagedPcmSample? Sample;
        /// <summary>Next source PCM frame copied into the decode window.</summary>
        public int SampleCursor;
        /// <summary>END/LOOP header flags from the most recently consumed source block.</summary>
        public byte PreviousFlags;
        /// <summary>Live BRR cursor retained for a released silent voice whose source has no PCM mapping.</summary>
        public ushort? ReleasedBrrCursor;
        /// <summary>Whether the voice substitutes the shared noise generator for PCM.</summary>
        public bool UseNoise;
        /// <summary>DSP-clock periods for attack, decay, sustain, and release updates.</summary>
        public ushort[] AdsrRates { get; } = new ushort[4];
        /// <summary>Elapsed clock count toward the next envelope step.</summary>
        public ushort RateCounter;
        /// <summary>Current ADSR or gain envelope phase.</summary>
        public EnvelopeState AdsrState;
        /// <summary>Envelope threshold at which decay changes into sustain.</summary>
        public ushort SustainLevel;
        /// <summary>Whether GAIN mode controls the envelope instead of ADSR.</summary>
        public bool UseGain;
        /// <summary>GAIN mode selector for direct, decreasing, or increasing gain.</summary>
        public byte GainMode;
        /// <summary>Whether GAIN directly assigns a fixed envelope value.</summary>
        public bool DirectGain;
        /// <summary>Seven-bit direct gain value from the voice GAIN register.</summary>
        public ushort GainValue;
        /// <summary>Current 11-bit envelope level multiplied into the voice sample.</summary>
        public ushort Gain;
        /// <summary>Latest signed sample emitted by this voice.</summary>
        public short SampleOutput;
        /// <summary>Signed per-voice left-channel gain.</summary>
        public sbyte VolumeLeft;
        /// <summary>Signed per-voice right-channel gain.</summary>
        public sbyte VolumeRight;
        /// <summary>Whether the voice contributes to echo input and feedback.</summary>
        public bool EchoEnable;

        /// <summary>Restores register-derived and decoder state to a silent newly initialized voice.</summary>
        public void Reset()
        {
            Pitch = PitchCounter = 0;
            PitchModulation = false;
            Array.Clear(DecodeBuffer);
            SourceNumber = 0;
            Sample = null;
            SampleCursor = 0;
            PreviousFlags = 0;
            ReleasedBrrCursor = null;
            UseNoise = false;
            Array.Clear(AdsrRates);
            RateCounter = 0;
            AdsrState = EnvelopeState.Attack;
            SustainLevel = 0;
            UseGain = false;
            GainMode = 0;
            DirectGain = false;
            GainValue = Gain = 0;
            SampleOutput = 0;
            VolumeLeft = VolumeRight = 0;
            EchoEnable = false;
        }
    }
}
