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
    public const int NativeStereoFramesPerVideoFrame = 534;
    public const int VoiceCount = 8;

    private readonly byte[] apuRam;
    private readonly byte[] registers = new byte[SnesDspRegisterMap.RegisterFileSize];
    private readonly Voice[] voices = Enumerable.Range(0, VoiceCount)
        .Select(_ => new Voice())
        .ToArray();
    private readonly short[] sampleBuffer = new short[NativeStereoFramesPerVideoFrame * 2];
    private readonly sbyte[] firValues = new sbyte[8];
    private readonly short[] firBufferLeft = new short[8];
    private readonly short[] firBufferRight = new short[8];

    private ManagedPcmSampleBank? sampleBank;
    private bool mute;
    private bool reset;
    private sbyte masterVolumeLeft;
    private sbyte masterVolumeRight;
    private short noiseSample;
    private ushort noiseRate;
    private ushort noiseCounter;
    private bool echoWrites;
    private sbyte echoVolumeLeft;
    private sbyte echoVolumeRight;
    private sbyte feedbackVolume;
    private ushort echoBufferAddress;
    private ushort echoDelay;
    private ushort echoRemaining;
    private ushort echoBufferIndex;
    private byte firBufferIndex;
    private ushort sampleOffset;

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

    public byte ReadRegister(byte address) => registers[address & 0x7f];

    /// <summary>
    /// Installs the source-number map associated with the latest cartridge upload. Existing
    /// voices retain their waveform while releasing; future key-ons resolve through this bank.
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
                voice.AdsrRates[0] = SnesDspTables.RateValues[
                    (value & SnesDspRegisterMap.Fields.AdsrAttackMask) * 2 + 1];
                voice.AdsrRates[1] = SnesDspTables.RateValues[
                    ((value & SnesDspRegisterMap.Fields.AdsrDecayMask) >>
                        SnesDspRegisterMap.Fields.AdsrDecayShift) * 2 + 16];
                voice.UseGain = (value & SnesDspRegisterMap.Fields.AdsrEnabled) == 0;
                break;
            case SnesDspRegisterMap.Voice.Adsr2:
                voice.AdsrRates[2] = SnesDspTables.RateValues[
                    value & SnesDspRegisterMap.Fields.AdsrSustainRateMask];
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
                    voice.AdsrRates[3] = SnesDspTables.RateValues[
                        value & SnesDspRegisterMap.Fields.AdsrSustainRateMask];
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
                noiseRate = SnesDspTables.RateValues[value & SnesDspRegisterMap.Fields.NoiseRateMask];
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

    private void KeyOn(Voice voice)
    {
        ManagedPcmSampleBank bank = sampleBank ?? throw new InvalidOperationException(
            "S-DSP received key-on before an extracted PCM sample bank was installed.");
        voice.PreviousFlags = 0;
        voice.Sample = bank.Resolve(voice.SourceNumber);
        voice.SampleCursor = 0;
        Array.Clear(voice.DecodeBuffer);
        voice.Gain = 0;
        voice.AdsrState = voice.UseGain ? EnvelopeState.Gain : EnvelopeState.Attack;
    }

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

    private static short GetInterpolatedSample(Voice voice, int sampleNumber, int offset)
    {
        int output = (SnesDspTables.GaussianValues[0xff - offset] * voice.DecodeBuffer[sampleNumber]) >> 10;
        output += (SnesDspTables.GaussianValues[0x1ff - offset] * voice.DecodeBuffer[sampleNumber + 1]) >> 10;
        output += (SnesDspTables.GaussianValues[0x100 + offset] * voice.DecodeBuffer[sampleNumber + 2]) >> 10;
        output = unchecked((short)output);
        output += (SnesDspTables.GaussianValues[offset] * voice.DecodeBuffer[sampleNumber + 3]) >> 10;
        return unchecked((short)(Clamp16(output) >> 1));
    }

    private void DecodePcm(int voiceIndex)
    {
        Voice voice = voices[voiceIndex];
        ManagedPcmSample sample = voice.Sample ?? throw new InvalidOperationException(
            $"S-DSP voice {voiceIndex} cannot decode without a PCM sample.");
        ReadOnlySpan<short> source = sample.Samples.Span;
        voice.DecodeBuffer[0] = voice.DecodeBuffer[16];
        voice.DecodeBuffer[1] = voice.DecodeBuffer[17];
        voice.DecodeBuffer[2] = voice.DecodeBuffer[18];
        if (voice.PreviousFlags is 1 or 3)
        {
            voice.SampleCursor = sample.LoopSampleIndex ?? 0;
            if (voice.PreviousFlags == 1)
            {
                voice.AdsrState = EnvelopeState.Release;
                voice.Gain = 0;
            }
            registers[SnesDspRegisterMap.Global.EndFlags] |= unchecked((byte)(1 << voiceIndex));
        }

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

    private ushort ReadWord(int address) => unchecked((ushort)(
        apuRam[address & 0xffff] | (apuRam[(address + 1) & 0xffff] << 8)));

    private void WriteWord(int address, ushort value)
    {
        apuRam[address & 0xffff] = unchecked((byte)value);
        apuRam[(address + 1) & 0xffff] = unchecked((byte)(value >> 8));
    }

    private static int Clamp16(int value) => Math.Clamp(value, short.MinValue, short.MaxValue);

    private enum EnvelopeState : byte
    {
        Attack,
        Decay,
        Sustain,
        Gain,
        Release,
    }

    private sealed class Voice
    {
        public ushort Pitch;
        public ushort PitchCounter;
        public bool PitchModulation;
        public short[] DecodeBuffer { get; } = new short[19];
        public byte SourceNumber;
        public ManagedPcmSample? Sample;
        public int SampleCursor;
        public byte PreviousFlags;
        public bool UseNoise;
        public ushort[] AdsrRates { get; } = new ushort[4];
        public ushort RateCounter;
        public EnvelopeState AdsrState;
        public ushort SustainLevel;
        public bool UseGain;
        public byte GainMode;
        public bool DirectGain;
        public ushort GainValue;
        public ushort Gain;
        public short SampleOutput;
        public sbyte VolumeLeft;
        public sbyte VolumeRight;
        public bool EchoEnable;

        public void Reset()
        {
            Pitch = PitchCounter = 0;
            PitchModulation = false;
            Array.Clear(DecodeBuffer);
            SourceNumber = 0;
            Sample = null;
            SampleCursor = 0;
            PreviousFlags = 0;
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
