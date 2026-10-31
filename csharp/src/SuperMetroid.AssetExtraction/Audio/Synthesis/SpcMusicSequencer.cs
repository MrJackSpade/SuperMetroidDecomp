using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The SPC driver's music sequencer: track commands, effects, notes and the pan, sweep, vibrato and tremolo it ticks.</summary>
internal sealed class SpcMusicSequencer
{
    /// <summary>Owned 64-KiB APU RAM shared by the driver views and DSP sample decoder.</summary>
    private readonly byte[] ram;
    /// <summary>Eight music sequencer states that continue advancing under borrowed DSP voices.</summary>
    private readonly ManagedSpcMusicChannel[] channels ;
    /// <summary>What music and sound effects share on the DSP: instruments, pitch and volume writes, the key-on, key-off, noise and echo flags, and memory reads.</summary>
    private readonly SpcVoices _voices;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal SpcMusicSequencer(byte[] ram, ManagedSpcMusicChannel[] channels, SpcVoices voices)
    {
        this.ram = ram;
        this.channels = channels;
        _voices = voices;
    }

    /// <summary>Applies the driver's high/low note correction before DSP pitch conversion.</summary>
    private void WritePitch(ManagedSpcMusicChannel channel, int pitch)
    {
        if (SpcFixedPoint.Whole(pitch) >= 0x34) // cartridge-encoding: allow(HardwareMagnitude) - native high-note correction threshold
            pitch = AddWord(pitch, SpcFixedPoint.Whole(pitch) - 0x34); // cartridge-encoding: allow(HardwareMagnitude) - native high-note correction threshold
        else if (SpcFixedPoint.Whole(pitch) < 0x13) // cartridge-encoding: allow(HardwareMagnitude) - native low-note correction threshold
            pitch = AddWord(pitch, unchecked((byte)((SpcFixedPoint.Whole(pitch) - 0x13) * 2)) - 256); // cartridge-encoding: allow(HardwareMagnitude) - native low-note correction
        _voices.WritePitchInner(channel, pitch);
    }

    /// <summary>Restores eight music voices, fades, transposition and tempo to the driver's track defaults.</summary>
    private void ResetMusicChannels()
    {
        for (int index = channels.Length - 1, bit = 0x80; index >= 0; index--, bit >>= 1) // cartridge-encoding: allow(BitMask) - highest voice bit
        {
            ManagedSpcMusicChannel channel = channels[index];
            _voices.currentChannelBit = unchecked((byte)bit);
            channel.ChannelVolume = SpcFixedPoint.WithWhole(channel.ChannelVolume, SpcDriverDefinitions.Music.DefaultChannelVolume);
            channel.PanFlags = SpcDriverDefinitions.Music.CenterPan;
            channel.PanValue = SpcFixedPoint.FromWhole(SpcDriverDefinitions.Music.CenterPan);
            channel.InstrumentId = 0;
            channel.FineTune = 0;
            channel.ChannelTransposition = 0;
            channel.PitchEnvelopeTicks = 0;
            channel.CutKey = 0;
            channel.VibratoDepth = 0;
            channel.TremoloDepth = 0;
        }
        _voices.currentChannelBit = 0;
        masterVolumeFadeTicks = 0;
        echoVolumeFadeTicks = 0;
        tempoFadeTicks = 0;
        globalTransposition = 0;
        blockCount = 0;
        _voices.percussionBaseId = 0;
        masterVolume = SpcFixedPoint.WithWhole(masterVolume, SpcDriverDefinitions.Music.DefaultMasterVolume);
        tempo = SpcFixedPoint.WithWhole(tempo, SpcDriverDefinitions.Music.DefaultTrackTempo);
    }

    /// <summary>Remembers the selected instrument before installing its DSP voice registers.</summary>
    private void SetInstrument(ManagedSpcMusicChannel channel, int instrument)
    {
        channel.InstrumentId = instrument;
        _voices.SetInstrumentWithoutSavingId(channel, instrument);
    }

    /// <summary>Derives the signed per-tick pitch-slide increment toward a transposed target note.</summary>
    private static void ComputePitchAdd(ManagedSpcMusicChannel channel, int pitch)
    {
        channel.PitchTarget = unchecked((byte)SpcDriverBytes.NoteNumber(pitch));
        channel.PitchAddPerTick = SpcFixedPoint.DivideSigned(
            channel.PitchTarget - SpcFixedPoint.Whole(channel.Pitch), channel.PitchSlideLength);
    }

    /// <summary>Consumes a following slide opcode immediately after a note when no slide is already active.</summary>
    private void CheckPitchSlideToNote(ManagedSpcMusicChannel channel)
    {
        if (channel.PitchSlideLength != 0 || PeekSequenceByte(channel.PatternOrderPointer) != SpcMusicDefinitionCodec.EffectOpcode(SpcMusicEffect.PitchSlide))
            return;
        channel.PatternOrderPointer = SpcDriverWrap.Word(channel.PatternOrderPointer + 1);
        channel.PitchSlideDelayLeft = NextSequenceByte(ref channel.PatternOrderPointer);
        channel.PitchSlideLength = NextSequenceByte(ref channel.PatternOrderPointer);
        ComputePitchAdd(channel, unchecked((byte)(NextSequenceByte(ref channel.PatternOrderPointer) +
            globalTransposition + channel.ChannelTransposition)));
    }

    /// <summary>Decodes one music effect and its native operand length, updating channel or global driver state.</summary>
    internal void HandleEffect(ManagedSpcMusicChannel channel, int rawEffect)
    {
        int tableIndex = rawEffect - SpcDriverDefinitions.Music.FirstEffect;
        if ((uint)tableIndex >= SpcMusicDefinitionCodec.EffectCount)
            throw new InvalidDataException($"Unknown SPC music effect ${rawEffect:X2}.");
        int argument = SpcMusicTables.EffectByteLength(tableIndex) != 0
            ? NextSequenceByte(ref channel.PatternOrderPointer)
            : 0;

        SpcMusicEffect effect = SpcMusicDefinitionCodec.DecodeEffect(rawEffect);
        switch (effect)
        {
            case SpcMusicEffect.SetInstrument:
                SetInstrument(channel, argument);
                break;
            case SpcMusicEffect.SetPan:
                channel.PanFlags = argument;
                channel.PanValue = SpcFixedPoint.FromWhole((argument & 0x1f)); // cartridge-encoding: allow(BitMask) - five-bit pan value
                break;
            case SpcMusicEffect.FadePan:
                channel.PanTicks = argument;
                channel.PanTarget = NextSequenceByte(ref channel.PatternOrderPointer);
                channel.PanAddPerTick = SpcFixedPoint.DivideSigned(
                    channel.PanTarget - SpcFixedPoint.Whole(channel.PanValue), argument);
                break;
            case SpcMusicEffect.EnableVibrato:
                channel.VibratoDelayTicks = argument;
                channel.VibratoRate = NextSequenceByte(ref channel.PatternOrderPointer);
                channel.VibratoDepthTarget = channel.VibratoDepth = NextSequenceByte(ref channel.PatternOrderPointer);
                channel.VibratoFadeTicks = 0;
                break;
            case SpcMusicEffect.DisableVibrato:
                channel.VibratoDepthTarget = channel.VibratoDepth = 0;
                channel.VibratoFadeTicks = 0;
                break;
            case SpcMusicEffect.SetMasterVolume:
                masterVolume = SpcFixedPoint.FromWhole(argument);
                break;
            case SpcMusicEffect.FadeMasterVolume:
                masterVolumeFadeTicks = argument;
                masterVolumeFadeTarget = NextSequenceByte(ref channel.PatternOrderPointer);
                masterVolumeFadeAdd = SpcFixedPoint.DivideSigned(
                    masterVolumeFadeTarget - SpcFixedPoint.Whole(masterVolume), argument);
                break;
            case SpcMusicEffect.SetTempo:
                tempo = SpcFixedPoint.FromWhole(argument);
                break;
            case SpcMusicEffect.FadeTempo:
                tempoFadeTicks = argument;
                tempoFadeTarget = NextSequenceByte(ref channel.PatternOrderPointer);
                tempoFadeAdd = SpcFixedPoint.DivideSigned(tempoFadeTarget - SpcFixedPoint.Whole(tempo), argument);
                break;
            case SpcMusicEffect.SetGlobalTransposition:
                globalTransposition = argument;
                break;
            case SpcMusicEffect.SetChannelTransposition:
                channel.ChannelTransposition = argument;
                break;
            case SpcMusicEffect.EnableTremolo:
                channel.TremoloDelayTicks = argument;
                channel.TremoloRate = NextSequenceByte(ref channel.PatternOrderPointer);
                channel.TremoloDepth = NextSequenceByte(ref channel.PatternOrderPointer);
                break;
            case SpcMusicEffect.DisableTremolo:
                channel.TremoloDepth = 0;
                break;
            case SpcMusicEffect.SetChannelVolume:
                channel.ChannelVolume = SpcFixedPoint.FromWhole(argument);
                break;
            case SpcMusicEffect.FadeChannelVolume:
                channel.VolumeFadeTicks = argument;
                channel.VolumeFadeTarget = NextSequenceByte(ref channel.PatternOrderPointer);
                channel.VolumeFadeAddPerTick = SpcFixedPoint.DivideSigned(
                    channel.VolumeFadeTarget - SpcFixedPoint.Whole(channel.ChannelVolume), argument);
                break;
            case SpcMusicEffect.CallPattern:
                channel.PatternStartPointer = unchecked(
                    LittleEndianBytes.Word(checked((byte)argument), NextSequenceByte(ref channel.PatternOrderPointer)));
                channel.SubroutineLoops = NextSequenceByte(ref channel.PatternOrderPointer);
                channel.SavedPatternPointer = channel.PatternOrderPointer;
                channel.PatternOrderPointer = channel.PatternStartPointer;
                break;
            case SpcMusicEffect.FadeVibrato:
                channel.VibratoFadeTicks = argument;
                channel.VibratoFadeAddPerTick = argument != 0
                    ? unchecked((byte)(channel.VibratoDepth / argument))
                    : byte.MaxValue;
                break;
            case SpcMusicEffect.PitchEnvelopeTo:
            case SpcMusicEffect.PitchEnvelopeFrom:
                channel.PitchEnvelopeDirection = effect == SpcMusicEffect.PitchEnvelopeTo
                    ? (byte)1 : (byte)0;
                channel.PitchEnvelopeDelay = argument;
                channel.PitchEnvelopeTicks = NextSequenceByte(ref channel.PatternOrderPointer);
                channel.PitchEnvelopeSlide = NextSequenceByte(ref channel.PatternOrderPointer);
                break;
            case SpcMusicEffect.DisablePitchEnvelope:
                channel.PitchEnvelopeTicks = 0;
                break;
            case SpcMusicEffect.SetFineTune:
                channel.FineTune = argument;
                break;
            case SpcMusicEffect.EnableEcho:
                _voices.echoEnable = argument;
                echoVolumeLeft = SpcFixedPoint.FromWhole(NextSequenceByte(ref channel.PatternOrderPointer));
                echoVolumeRight = SpcFixedPoint.FromWhole(NextSequenceByte(ref channel.PatternOrderPointer));
                _voices.dspFlags = _voices.dspFlags & unchecked((byte)~SpcDriverDefinitions.Echo.WriteDisable);
                break;
            case SpcMusicEffect.DisableEcho:
                echoVolumeLeft = echoVolumeRight = 0;
                _voices.dspFlags = _voices.dspFlags | SpcDriverDefinitions.Echo.WriteDisable;
                break;
            case SpcMusicEffect.ConfigureEcho:
                SetupEchoDelay(argument);
                echoFeedback = NextSequenceByte(ref channel.PatternOrderPointer);
                int preset = NextSequenceByte(ref channel.PatternOrderPointer);
                int firOffset = unchecked((byte)(preset * SpcDriverDefinitions.Echo.FirTapCount));
                // `$F7` indexes from APU `$1E32`, not from an abstract four-entry host
                // array. Retail sequences use values beyond three: Kraid's post-defeat
                // room track executes `F7 02 0A 0A` and therefore reads the eight resident
                // driver bytes at `$1E82`. Reading the actual uploaded address space also
                // preserves changes to those adjacent bytes. Native MUL YA then MOV X,A
                // keeps only the product's low byte: presets32..255 alias0..31. Each
                // eight-tap group ends at or before offset255; no tap carries past X.
                for (int tap = 0; tap < SpcDriverDefinitions.Echo.FirTapCount; tap++)
                {
                    _voices.Dsp.Global.WriteFirCoefficient(tap,
                        ReadFirCoefficient(firOffset + tap));
                }
                break;
            case SpcMusicEffect.FadeEchoVolume:
                echoVolumeFadeTicks = argument;
                echoVolumeFadeTargetLeft = NextSequenceByte(ref channel.PatternOrderPointer);
                echoVolumeFadeTargetRight = NextSequenceByte(ref channel.PatternOrderPointer);
                echoVolumeFadeAddLeft = SpcFixedPoint.DivideSigned(
                    echoVolumeFadeTargetLeft - SpcFixedPoint.Whole(echoVolumeLeft), argument);
                echoVolumeFadeAddRight = SpcFixedPoint.DivideSigned(
                    echoVolumeFadeTargetRight - SpcFixedPoint.Whole(echoVolumeRight), argument);
                break;
            case SpcMusicEffect.PitchSlide:
                channel.PitchSlideDelayLeft = argument;
                channel.PitchSlideLength = NextSequenceByte(ref channel.PatternOrderPointer);
                ComputePitchAdd(channel, unchecked((byte)(NextSequenceByte(ref channel.PatternOrderPointer) +
                    globalTransposition + channel.ChannelTransposition)));
                break;
            case SpcMusicEffect.SetPercussionBase:
                _voices.percussionBaseId = argument;
                break;
            case SpcMusicEffect.SkipByte:
                channel.PatternOrderPointer = SpcDriverWrap.Word(channel.PatternOrderPointer + 1);
                break;
            case SpcMusicEffect.CutKey:
                channel.CutKey = unchecked((byte)(argument + 1));
                break;
            case SpcMusicEffect.FastForwardForFrames:
                fastForward = unchecked((byte)(argument + 1));
                _voices.keyOff = _voices.keyOff | unchecked((byte)~_voices.channelOnMask);
                break;
            case SpcMusicEffect.SetFastForward:
                fastForward = argument;
                _voices.keyOff = _voices.keyOff | unchecked((byte)~_voices.channelOnMask);
                break;
            default:
                throw new InvalidDataException($"Unknown SPC music effect ${rawEffect:X2}.");
        }
    }

    /// <summary>Looks ahead through pattern calls, effects and loops to keep a tied note keyed on.</summary>
    private bool WantsKeyOff(ManagedSpcMusicChannel channel)
    {
        int loops = channel.SubroutineLoops;
        int pointer = channel.PatternOrderPointer;
        for (int instructions = 0; instructions < SpcDriverDefinitions.MaximumFastForwardTicks; instructions++)
        {
            int command = NextSequenceByte(ref pointer);
            if (command == 0)
            {
                if (loops == 0)
                    return true;
                loops--;
                pointer = loops == 0 ? channel.SavedPatternPointer : channel.PatternStartPointer;
            }
            else
            {
                while ((command & 0x80) == 0) // cartridge-encoding: allow(BitMask) - music-command marker
                    command = NextSequenceByte(ref pointer);
                if (command == SpcDriverDefinitions.Music.TieNote)
                    return false;
                if (command == SpcMusicDefinitionCodec.EffectOpcode(SpcMusicEffect.CallPattern))
                    pointer = _voices.ReadWord(pointer);
                else if (command >= SpcDriverDefinitions.Music.FirstEffect)
                    pointer = AddWord(pointer, SpcMusicTables.EffectByteLength(command - SpcDriverDefinitions.Music.FirstEffect));
                else
                    return true;
            }
        }
        throw new InvalidDataException("SPC key-off lookahead did not reach a note boundary.");
    }

    /// <summary>Combines master, channel and tremolo volume with the driver's repeated fixed-point scaling.</summary>
    private void CalculateFinalVolume(ManagedSpcMusicChannel channel, int volume)
    {
        int scaled = SpcFixedPoint.Scale(SpcFixedPoint.Whole(masterVolume), volume);
        scaled = SpcFixedPoint.Scale(scaled, channel.ChannelVolumeMaster);
        scaled = SpcFixedPoint.Scale(scaled, SpcFixedPoint.Whole(channel.ChannelVolume));
        channel.FinalVolume = (byte)SpcFixedPoint.Scale(scaled, scaled);
    }

    /// <summary>Folds the tremolo wave and applies depth before recalculating final voice volume.</summary>
    private void CalculateTremolo(ManagedSpcMusicChannel channel, int value)
    {
        value = (value & 0x80) != 0 // cartridge-encoding: allow(BitMask) - tremolo waveform phase
            ? unchecked((byte)((value * 2) ^ byte.MaxValue))
            : unchecked((byte)(value * 2));
        value = (byte)SpcFixedPoint.Scale(value, channel.TremoloDepth);
        CalculateFinalVolume(channel, unchecked((byte)(value ^ byte.MaxValue)));
    }

    /// <summary>Marks volume dirty and samples tremolo at the current tempo accumulator.</summary>
    private void HandleTremolo(ManagedSpcMusicChannel channel)
    {
        affectedVolumeOrPitch = affectedVolumeOrPitch | 0x80; // cartridge-encoding: allow(BitMask) - dirty result marker
        CalculateTremolo(channel, (byte)SpcFixedPoint.Scale(channel.TremoloRate, mainTempoAccumulator));
    }

    /// <summary>Folds the vibrato phase into a signed pitch offset, including the driver's deep-vibrato branch.</summary>
    private void CalculateVibratoPitch(ManagedSpcMusicChannel channel, int pitch, int value)
    {
        int scaled = value << 2;
        scaled ^= (scaled & 0x100) != 0 ? byte.MaxValue : 0; // cartridge-encoding: allow(BitMask) - folded vibrato phase
        int delta = channel.VibratoDepth >= 0xf1 // cartridge-encoding: allow(HardwareMagnitude) - native deep-vibrato threshold
            ? unchecked((byte)scaled) * (channel.VibratoDepth & 0x0f) // cartridge-encoding: allow(BitMask) - deep-vibrato fractional depth
            : SpcFixedPoint.Scale(unchecked((byte)scaled), channel.VibratoDepth);
        WritePitch(channel, AddWord(pitch, (value & 0x80) != 0 ? -delta : delta)); // cartridge-encoding: allow(BitMask) - vibrato direction
    }

    /// <summary>Interpolates active pan and pitch sweeps between full music ticks and publishes affected DSP registers.</summary>
    internal void HandlePanAndSweep(ManagedSpcMusicChannel channel)
    {
        affectedVolumeOrPitch = 0;
        if (channel.TremoloDepth != 0 && channel.TremoloHoldCount == channel.TremoloDelayTicks)
            HandleTremolo(channel);

        int volume = channel.PanValue;
        if (channel.PanTicks != 0)
        {
            affectedVolumeOrPitch = 0x80; // cartridge-encoding: allow(BitMask) - dirty result marker
            volume = AddWord(volume, mainTempoAccumulator * unchecked((short)channel.PanAddPerTick) / 256);
        }
        if (affectedVolumeOrPitch != 0)
            _voices.WriteVolume(channel, volume);

        affectedVolumeOrPitch = 0;
        int pitch = channel.Pitch;
        if (channel.PitchSlideLength != 0 && channel.PitchSlideDelayLeft == 0)
        {
            affectedVolumeOrPitch = affectedVolumeOrPitch | 0x80; // cartridge-encoding: allow(BitMask) - dirty result marker
            pitch = AddWord(pitch, mainTempoAccumulator * unchecked((short)channel.PitchAddPerTick) / 256);
        }
        if (channel.VibratoDepth != 0 && channel.VibratoDelayTicks == channel.VibratoHoldCount)
        {
            CalculateVibratoPitch(channel, pitch, unchecked((byte)(
                SpcFixedPoint.Scale(mainTempoAccumulator, channel.VibratoRate) + channel.VibratoCount)));
            return;
        }
        if (affectedVolumeOrPitch != 0)
            WritePitch(channel, pitch);
    }

    /// <summary>Advances note key-off, pitch slide and vibrato, honoring tie lookahead before releasing the voice.</summary>
    private void HandleNoteTick(ManagedSpcMusicChannel channel)
    {
        if (channel.NoteKeyOffTicksLeft != 0 &&
            ((channel.NoteKeyOffTicksLeft = SpcDriverWrap.Byte(channel.NoteKeyOffTicksLeft - 1)) == 0 || channel.NoteTicksLeft == 2))
        {
            if (WantsKeyOff(channel) && (_voices.currentChannelBit & _voices.channelOnMask) == 0)
                _voices.Dsp.Global.KeyOff(new DspVoiceMask(_voices.currentChannelBit));
        }

        affectedVolumeOrPitch = 0;
        if (channel.PitchSlideLength != 0)
        {
            if (channel.PitchSlideDelayLeft != 0)
            {
                channel.PitchSlideDelayLeft = SpcDriverWrap.Byte(channel.PitchSlideDelayLeft - 1);
            }
            else
            {
                affectedVolumeOrPitch = 0x80; // cartridge-encoding: allow(BitMask) - dirty result marker
                channel.PitchSlideLength = SpcDriverWrap.Byte(channel.PitchSlideLength - 1);
                channel.Pitch = channel.PitchSlideLength == 0
                    ? SpcFixedPoint.Compose(channel.PitchTarget, channel.FineTune)
                    : AddWord(channel.Pitch, channel.PitchAddPerTick);
            }
        }

        int pitch = channel.Pitch;
        if (channel.VibratoDepth != 0)
        {
            if (channel.VibratoDelayTicks == channel.VibratoHoldCount)
            {
                if (channel.VibratoChangeCount == channel.VibratoFadeTicks)
                    channel.VibratoDepth = channel.VibratoDepthTarget;
                else
                {
                    int changeCount = channel.VibratoChangeCount;
                    channel.VibratoChangeCount = SpcDriverWrap.Byte(changeCount + 1);
                    channel.VibratoDepth = unchecked((byte)((changeCount == 0
                        ? 0 : channel.VibratoDepth) + channel.VibratoFadeAddPerTick));
                }
                channel.VibratoCount = unchecked((byte)(channel.VibratoCount + channel.VibratoRate));
                CalculateVibratoPitch(channel, pitch, channel.VibratoCount);
                return;
            }
            channel.VibratoHoldCount = SpcDriverWrap.Byte(channel.VibratoHoldCount + 1);
        }
        if (affectedVolumeOrPitch != 0)
            WritePitch(channel, pitch);
    }

    /// <summary>Advances channel volume, tremolo and pan envelopes for one full tempo tick.</summary>
    private void HandleChannelTick(ManagedSpcMusicChannel channel)
    {
        if (channel.VolumeFadeTicks != 0)
        {
            channel.VolumeFadeTicks = SpcDriverWrap.Byte(channel.VolumeFadeTicks - 1);
            channel.ChannelVolume = channel.VolumeFadeTicks == 0
                ? SpcFixedPoint.FromWhole(channel.VolumeFadeTarget)
                : AddWord(channel.ChannelVolume, channel.VolumeFadeAddPerTick);
            volumeDirty = volumeDirty | _voices.currentChannelBit;
        }
        if (channel.TremoloDepth != 0)
        {
            if (channel.TremoloDelayTicks == channel.TremoloHoldCount)
            {
                volumeDirty = volumeDirty | _voices.currentChannelBit;
                channel.TremoloCount = (channel.TremoloCount & 0x80) != 0 && // cartridge-encoding: allow(BitMask) - tremolo phase
                    channel.TremoloDepth == byte.MaxValue
                    ? (byte)0x80 // cartridge-encoding: allow(BitMask) - tremolo phase origin
                    : unchecked((byte)(channel.TremoloCount + channel.TremoloRate));
                CalculateTremolo(channel, channel.TremoloCount);
            }
            else
            {
                channel.TremoloHoldCount = SpcDriverWrap.Byte(channel.TremoloHoldCount + 1);
                CalculateFinalVolume(channel, byte.MaxValue);
            }
        }
        else
        {
            CalculateFinalVolume(channel, byte.MaxValue);
        }

        if (channel.PanTicks != 0)
        {
            channel.PanTicks = SpcDriverWrap.Byte(channel.PanTicks - 1);
            channel.PanValue = channel.PanTicks == 0
                ? SpcFixedPoint.FromWhole(channel.PanTarget)
                : AddWord(channel.PanValue, channel.PanAddPerTick);
            volumeDirty = volumeDirty | _voices.currentChannelBit;
        }
        if ((volumeDirty & _voices.currentChannelBit) != 0)
            _voices.WriteVolume(channel, channel.PanValue);
    }

    /// <summary>Consumes the CPU music port and advances track blocks, sequence channels and tempo state.</summary>
    internal void HandleMusicCommand()
    {
        int command = musicInputLatch;
        musicInputLatch = (int)MusicPortCommand.None;
        if (Enum.IsDefined((MusicPortCommand)command))
        {
            switch ((MusicPortCommand)command)
            {
                case MusicPortCommand.Pause:
                    _voices.keyOff = _voices.keyOff | unchecked((byte)~_voices.channelOnMask);
                    return;
                case MusicPortCommand.Resume:
                case MusicPortCommand.None:
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MusicPortCommand)} {command}.");
            }
        }
        else if (command != musicPortToSnes)
        {
            StartTrack(command);
            return;
        }
        if (musicPortToSnes == 0)
            return;
        if (counter == 0)
            goto ProcessChannels;
        if (counter != 0)
        {
            counter = SpcDriverWrap.Word(counter - 1);
            if (counter != 0)
            {
                ResetMusicChannels();
                return;
            }
        }

    NextPhrase:
        observer?.PhraseStarting();
        int patternTable;
        for (int records = 0; ; records++)
        {
            if (records > SpcDriverDefinitions.MaximumFastForwardTicks)
                throw new InvalidDataException("SPC top-level music stream did not reach a pattern table.");
            int record = _voices.ReadWord(musicTopLevelPointer);
            musicTopLevelPointer = SpcDriverWrap.Word(musicTopLevelPointer + 2);
            if (LittleEndianBytes.HighByte(record) >= SpcDriverDefinitions.Music.PatternPointerHighByteMinimum)
            {
                patternTable = record;
                break;
            }
            if (record == 0)
            {
                StartTrack(0);
                return;
            }
            if (MusicTrackControls.TryDecode(record, out MusicTrackControl control))
            {
                // The driver stores the control word's low byte as its fast-forward flag.
                fastForward = control switch
                {
                    MusicTrackControl.FastForwardOn => unchecked((byte)record),
                    MusicTrackControl.FastForwardOff => 0,
                    _ => throw new InvalidOperationException($"Undefined {nameof(MusicTrackControl)} {control}."),
                };
            }
            else
            {
                blockCount = SpcDriverWrap.Byte(blockCount - 1);
                if ((blockCount & 0x80) != 0) // cartridge-encoding: allow(BitMask) - signed block-counter underflow
                    blockCount = unchecked((byte)record);
                int loopPointer = _voices.ReadWord(musicTopLevelPointer);
                musicTopLevelPointer = SpcDriverWrap.Word(musicTopLevelPointer + 2);
                if (blockCount != 0)
                    musicTopLevelPointer = loopPointer;
            }
        }

        for (int index = 0; index < channels.Length; index++)
        {
            channels[index].PatternOrderPointer = _voices.ReadWord(patternTable);
            patternTable = SpcDriverWrap.Word(patternTable + 2);
        }
        for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
        {
            ManagedSpcMusicChannel channel = channels[index];
            _voices.currentChannelBit = unchecked((byte)bit);
            if (channel.PatternOrderPointer > byte.MaxValue && channel.InstrumentId == 0)
                SetInstrument(channel, 0);
            channel.SubroutineLoops = 0;
            channel.VolumeFadeTicks = 0;
            channel.PanTicks = 0;
            channel.NoteTicksLeft = 1;
        }
        _voices.currentChannelBit = 0;

    ProcessChannels:
        volumeDirty = 0;
        for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
        {
            ManagedSpcMusicChannel channel = channels[index];
            _voices.currentChannelBit = unchecked((byte)bit);
            if (channel.PatternOrderPointer <= byte.MaxValue)
                continue;
            channel.NoteTicksLeft = SpcDriverWrap.Byte(channel.NoteTicksLeft - 1);
            if (channel.NoteTicksLeft == 0)
            {
                for (int instructions = 0; ; instructions++)
                {
                    if (instructions > SpcDriverDefinitions.MaximumFastForwardTicks)
                        throw new InvalidDataException($"SPC channel {index} did not reach a timed note.");
                    int noteCommand = NextSequenceByte(ref channel.PatternOrderPointer);
                    if (noteCommand == 0)
                    {
                        if (channel.SubroutineLoops == 0)
                            goto NextPhrase;
                        channel.SubroutineLoops = SpcDriverWrap.Byte(channel.SubroutineLoops - 1);
                        channel.PatternOrderPointer = channel.SubroutineLoops == 0
                            ? channel.SavedPatternPointer : channel.PatternStartPointer;
                        continue;
                    }
                    if ((noteCommand & 0x80) == 0) // cartridge-encoding: allow(BitMask) - music-command marker
                    {
                        channel.NoteLength = noteCommand;
                        noteCommand = NextSequenceByte(ref channel.PatternOrderPointer);
                        if ((noteCommand & 0x80) == 0) // cartridge-encoding: allow(BitMask) - music-command marker
                        {
                            channel.NoteGateOffFixedPoint = SpcMusicTables.NoteGateOffPercentage((noteCommand >> 4) & 7);
                            channel.ChannelVolumeMaster = SpcMusicTables.NoteVolume(noteCommand & 0x0f); // cartridge-encoding: allow(BitMask) - volume nibble
                            noteCommand = NextSequenceByte(ref channel.PatternOrderPointer);
                        }
                    }
                    if (noteCommand >= SpcDriverDefinitions.Music.FirstEffect)
                    {
                        HandleEffect(channel, noteCommand);
                        continue;
                    }
                    if ((channel.CutKey | fastForward) == 0)
                        PlayNote(channel, noteCommand);
                    channel.NoteTicksLeft = channel.NoteLength;
                    int gateTicks = SpcFixedPoint.Scale(channel.NoteTicksLeft, channel.NoteGateOffFixedPoint);
                    channel.NoteKeyOffTicksLeft = unchecked((byte)(gateTicks != 0 ? gateTicks : 1));
                    CheckPitchSlideToNote(channel);
                    break;
                }
            }
            else if (fastForward == 0)
            {
                HandleNoteTick(channel);
                CheckPitchSlideToNote(channel);
            }
        }
        _voices.currentChannelBit = 0;

        if (tempoFadeTicks != 0)
        {
            tempoFadeTicks = SpcDriverWrap.Byte(tempoFadeTicks - 1);
            tempo = tempoFadeTicks == 0
                ? SpcFixedPoint.FromWhole(tempoFadeTarget)
                : AddWord(tempo, tempoFadeAdd);
        }
        if (echoVolumeFadeTicks != 0)
        {
            echoVolumeLeft = AddWord(echoVolumeLeft, echoVolumeFadeAddLeft);
            echoVolumeRight = AddWord(echoVolumeRight, echoVolumeFadeAddRight);
            echoVolumeFadeTicks = SpcDriverWrap.Byte(echoVolumeFadeTicks - 1);
            if (echoVolumeFadeTicks == 0)
            {
                echoVolumeLeft = SpcFixedPoint.FromWhole(echoVolumeFadeTargetLeft);
                echoVolumeRight = SpcFixedPoint.FromWhole(echoVolumeFadeTargetRight);
            }
        }
        if (masterVolumeFadeTicks != 0)
        {
            masterVolumeFadeTicks = SpcDriverWrap.Byte(masterVolumeFadeTicks - 1);
            masterVolume = masterVolumeFadeTicks == 0
                ? SpcFixedPoint.FromWhole(masterVolumeFadeTarget)
                : AddWord(masterVolume, masterVolumeFadeAdd);
            volumeDirty = byte.MaxValue;
        }
        for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
        {
            _voices.currentChannelBit = unchecked((byte)bit);
            if (channels[index].PatternOrderPointer > byte.MaxValue)
                HandleChannelTick(channels[index]);
        }
        _voices.currentChannelBit = 0;
    }

    /// <summary>Resolves a numbered track's top-level pointer and resets its music-channel state.</summary>
    private void StartTrack(int track)
    {
        musicPortToSnes = track;
        // Track zero deliberately indexes the word immediately before the numbered table.
        musicTopLevelPointer = _voices.ReadPointerEntry(SpcDriverDefinitions.Ram.DefaultMusicPointer, track);
        counter = SpcDriverDefinitions.Music.TrackStartupTicks;
        _voices.keyOff = _voices.keyOff | unchecked((byte)~_voices.channelOnMask);
    }

    /// <summary>Applies note or percussion pitch, volume and key-on state to the selected music voice.</summary>
    private void PlayNote(ManagedSpcMusicChannel channel, int note)
    {
        if (note >= SpcDriverDefinitions.Music.FirstPercussionNote)
        {
            SetInstrument(channel, note);
            note = SpcDriverDefinitions.Music.PercussionPlaybackNote;
        }
        if (note >= SpcDriverDefinitions.Music.TieNote || (_voices.channelOnMask & _voices.currentChannelBit) != 0)
            return;

        channel.Pitch = SpcFixedPoint.Compose(SpcDriverBytes.NoteNumber(note) + globalTransposition +
            channel.ChannelTransposition, channel.FineTune);
        channel.VibratoCount = unchecked((byte)(channel.VibratoFadeTicks << 7));
        channel.VibratoHoldCount = 0;
        channel.VibratoChangeCount = 0;
        channel.TremoloCount = 0;
        channel.TremoloHoldCount = 0;
        volumeDirty = volumeDirty | _voices.currentChannelBit;
        _voices.keyOn = _voices.keyOn | _voices.currentChannelBit;
        channel.PitchSlideLength = channel.PitchEnvelopeTicks;
        if (channel.PitchSlideLength != 0)
        {
            channel.PitchSlideDelayLeft = channel.PitchEnvelopeDelay;
            if (channel.PitchEnvelopeDirection == 0)
                channel.Pitch = AddWord(channel.Pitch, -SpcFixedPoint.FromWhole(channel.PitchEnvelopeSlide));
            ComputePitchAdd(channel, unchecked((byte)(SpcFixedPoint.Whole(channel.Pitch) + channel.PitchEnvelopeSlide)));
        }
        WritePitch(channel, channel.Pitch);
    }

    /// <summary>The next byte of a music sequence stream at <paramref name="cursor"/>, advancing it with 16-bit wrap.</summary>
    internal byte NextSequenceByte(ref int cursor)
    {
        int position = cursor;
        cursor = SpcDriverWrap.Word(cursor + 1);
        return ram[position];
    }

    /// <summary>The music sequence byte at <paramref name="cursor"/>, without advancing.</summary>
    internal byte PeekSequenceByte(int cursor) => ram[cursor];

    /// <summary>Byte <paramref name="index"/> of the echo FIR coefficient preset table.</summary>
    internal sbyte ReadFirCoefficient(int index) => unchecked((sbyte)ram[SpcDriverDefinitions.Echo.FirCoefficientTableAddress + index]);

    /// <summary>
    /// <c>$F4</c> as the sequencer reads it: the CPU's latest music command, or
    /// <see cref="MusicPortCommand.None"/> once a tempo tick consumed it. The sound-effect
    /// latches, their acknowledgements and the driver timer belong to the game session's
    /// <see cref="Core.Compatibility.SoundAcceptanceClock{TSound}"/>, which schedules this player's ticks (#1304).
    /// </summary>
    internal int musicInputLatch;

    /// <summary>Output port zero: the sequencer's echo of the playing track.</summary>
    internal int musicPortToSnes;

    /// <summary>Music block countdown before selecting the next top-level phrase.</summary>
    internal int counter;

    /// <summary>Dirty marker shared by pan/tremolo and pitch/vibrato publication.</summary>
    internal int affectedVolumeOrPitch;

    /// <summary>Music sequence fast-forward flag written by its control effect.</summary>
    internal int fastForward;

    /// <summary>Current track's top-level phrase-list address.</summary>
    internal int musicTopLevelPointer;

    /// <summary>Remaining repetitions of the current top-level music block.</summary>
    internal int blockCount;

    /// <summary>Signed countdown while a changed echo delay settles.</summary>
    internal int echoStoredTime;

    /// <summary>Requested DSP echo delay byte.</summary>
    internal int echoDelay;

    /// <summary>Signed DSP echo feedback written after the delay settles.</summary>
    internal int echoFeedback;

    /// <summary>Semitone offset applied to note and slide targets across the track.</summary>
    internal int globalTransposition;

    /// <summary>Carry accumulator scheduling full music ticks and intermediate sweeps.</summary>
    internal int mainTempoAccumulator;

    /// <summary>Current fixed-point music tempo selected by track or fade effects.</summary>
    internal int tempo;

    /// <summary>Remaining full ticks in a tempo fade.</summary>
    internal int tempoFadeTicks;

    /// <summary>Whole-byte tempo endpoint for an active fade.</summary>
    internal int tempoFadeTarget;

    /// <summary>Signed fixed-point increment applied per tempo-fade tick.</summary>
    internal int tempoFadeAdd;

    /// <summary>Global fixed-point music volume before per-channel scaling.</summary>
    internal int masterVolume;

    /// <summary>Remaining full ticks in a master-volume fade.</summary>
    internal int masterVolumeFadeTicks;

    /// <summary>Whole-byte endpoint for a master-volume fade.</summary>
    internal int masterVolumeFadeTarget;

    /// <summary>Signed fixed-point master-volume increment per tick.</summary>
    internal int masterVolumeFadeAdd;

    /// <summary>Music voices whose volume registers need a fresh DSP write.</summary>
    internal int volumeDirty;

    /// <summary>Fixed-point left echo volume before DSP publication.</summary>
    internal int echoVolumeLeft;

    /// <summary>Fixed-point right echo volume before DSP publication.</summary>
    internal int echoVolumeRight;

    /// <summary>Left echo-volume increment per fade tick.</summary>
    internal int echoVolumeFadeAddLeft;

    /// <summary>Right echo-volume increment per fade tick.</summary>
    internal int echoVolumeFadeAddRight;

    /// <summary>Remaining ticks of a stereo echo-volume fade.</summary>
    internal int echoVolumeFadeTicks;

    /// <summary>Left echo-volume target when its fade ends.</summary>
    internal int echoVolumeFadeTargetLeft;

    /// <summary>Right echo-volume target when its fade ends.</summary>
    internal int echoVolumeFadeTargetRight;

    /// <summary>Last DSP delay value, used to detect a requested echo-buffer reconfiguration.</summary>
    internal int lastWrittenEchoDelay;
    /// <summary>The analysis observer, or null during playback; never part of saved driver state.</summary>
    [NonSerialized]
    internal ISpcDriverObserver? observer;

    /// <summary>Disables echo during delay changes, computes its countdown and selects the native buffer page.</summary>
    internal void SetupEchoDelay(int requestedDelay)
    {
        echoDelay = requestedDelay;
        if (requestedDelay != lastWrittenEchoDelay)
        {
            int countdown = unchecked((byte)(SnesDspControls.EchoDelayOf(lastWrittenEchoDelay) ^ byte.MaxValue));
            if ((echoStoredTime & 0x80) != 0) // cartridge-encoding: allow(BitMask) - signed SPC countdown bit
                countdown = unchecked((byte)(countdown + echoStoredTime));
            echoStoredTime = countdown;

            _voices.Dsp.Global.WriteEchoEnable(DspVoiceMask.None);
            _voices.Dsp.Global.WriteEchoFeedback(0);
            _voices.Dsp.Global.WriteEchoVolume(DspStereoSide.Right, 0);
            _voices.Dsp.Global.WriteEchoVolume(DspStereoSide.Left, 0);
            _voices.Dsp.Global.WriteFlags(new DspFlags(unchecked((byte)(_voices.dspFlags | SpcDriverDefinitions.Echo.WriteDisable))));
            lastWrittenEchoDelay = echoDelay;
            _voices.Dsp.Global.WriteEchoDelay(echoDelay);
        }

        int echoPage = unchecked((byte)(((echoDelay * SpcDriverDefinitions.Echo.DelayToPages) ^
            byte.MaxValue) + SpcDriverDefinitions.Echo.BufferPageBias));
        _voices.Dsp.Global.WriteEchoBufferPage(echoPage);
    }

    /// <summary>Applies signed arithmetic with the SPC driver's 16-bit word wrap.</summary>
    private static int AddWord(int left, int right) => SpcDriverWrap.Word(left + right);
}
