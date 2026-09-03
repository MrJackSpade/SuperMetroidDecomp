namespace SuperMetroid.Core.Audio;

public sealed partial class ManagedSpcPlayer
{
    private static ushort DivideSignedFixedPoint(int numerator, byte denominator)
    {
        int original = numerator;
        if ((numerator & 0x100) != 0) // allow(BitMask): SPC nine-bit sign test
            numerator = -numerator;
        int quotient = denominator != 0 ? (numerator & byte.MaxValue) / denominator : byte.MaxValue;
        int remainder = denominator != 0 ? (numerator & byte.MaxValue) % denominator : numerator & byte.MaxValue;
        int fixedPoint = (quotient << 8) +
            (denominator != 0 ? ((remainder << 8) / denominator) & byte.MaxValue : byte.MaxValue);
        return unchecked((ushort)(((original & 0x100) != 0) ? -fixedPoint : fixedPoint)); // allow(BitMask): SPC nine-bit sign test
    }

    private void WriteVolume(ManagedSpcMusicChannel channel, ushort volume)
    {
        // A sound effect owns this hardware voice while its bit is set. Music state continues
        // advancing underneath it, but must not overwrite the SFX's live DSP registers.
        if ((channelOnMask & currentChannelBit) != 0)
            return;

        for (int side = 0; side < 2; side++)
        {
            int panIndex = volume >> 8;
            int baseVolume;
            int nextVolume;
            if (panIndex >= SpcMusicTables.PanVolume.Length - 1)
            {
                // The retail driver intentionally reads beyond its 22-byte local curve into
                // adjacent SPC program data. Preserve that address-level behavior.
                int address = panIndex + 0x1e1d; // allow(HardwareAddress): native pan-table overread base
                baseVolume = ram[address];
                nextVolume = ram[address + 1];
            }
            else
            {
                baseVolume = SpcMusicTables.PanVolume[panIndex];
                nextVolume = SpcMusicTables.PanVolume[panIndex + 1];
            }

            byte interpolated = unchecked((byte)(baseVolume +
                ((nextVolume - baseVolume) * unchecked((byte)volume) >> 8)));
            byte final = unchecked((byte)(interpolated * channel.FinalVolume >> 8));
            if (((channel.PanFlags << side) & 0x80) != 0) // allow(BitMask): phase-inversion bit
                final = unchecked((byte)-final);
            WriteDsp(unchecked((byte)(channel.Index * SnesDspRegisterMap.VoiceStride +
                SnesDspRegisterMap.Voice.VolumeLeft + side)), final);
            volume = unchecked((ushort)(0x1400 - volume)); // allow(HardwareMagnitude): 20-step mirrored pan domain
        }
    }

    private void WritePitchInner(ManagedSpcMusicChannel channel, ushort pitch)
    {
        byte note = unchecked((byte)((pitch >> 8) & 0x7f)); // allow(BitMask): seven-bit note number
        byte octave = unchecked((byte)(note / 12));
        byte semitone = unchecked((byte)(note % 12));
        int delta = unchecked((byte)(SpcMusicTables.BaseNoteFrequencies[semitone + 1] -
            SpcMusicTables.BaseNoteFrequencies[semitone]));
        ushort frequency = unchecked((ushort)(SpcMusicTables.BaseNoteFrequencies[semitone] +
            (delta * unchecked((byte)pitch) >> 8)));
        frequency = unchecked((ushort)(frequency * 2));
        while (octave != 6)
        {
            frequency >>= 1;
            octave++;
        }

        frequency = unchecked((ushort)(channel.InstrumentPitchBase * frequency >> 8));
        if ((currentChannelBit & channelOnMask) == 0)
        {
            byte register = unchecked((byte)(channel.Index * SnesDspRegisterMap.VoiceStride));
            WriteDsp(unchecked((byte)(register + SnesDspRegisterMap.Voice.PitchLow)), frequency);
            WriteDsp(unchecked((byte)(register + SnesDspRegisterMap.Voice.PitchHigh)), frequency >> 8);
        }
    }

    private void WritePitch(ManagedSpcMusicChannel channel, ushort pitch)
    {
        if (HighByte(pitch) >= 0x34) // allow(HardwareMagnitude): native high-note correction threshold
            pitch = AddWord(pitch, HighByte(pitch) - 0x34); // allow(HardwareMagnitude): native high-note correction threshold
        else if (HighByte(pitch) < 0x13) // allow(HardwareMagnitude): native low-note correction threshold
            pitch = AddWord(pitch, unchecked((byte)((HighByte(pitch) - 0x13) * 2)) - 256); // allow(HardwareMagnitude): native low-note correction
        WritePitchInner(channel, pitch);
    }

    private void ResetMusicChannels()
    {
        for (int index = channels.Length - 1, bit = 0x80; index >= 0; index--, bit >>= 1) // allow(BitMask): highest voice bit
        {
            ManagedSpcMusicChannel channel = channels[index];
            currentChannelBit = unchecked((byte)bit);
            SetHighByte(ref channel.ChannelVolume, SpcDriverData.Music.DefaultChannelVolume);
            channel.PanFlags = SpcDriverData.Music.CenterPan;
            channel.PanValue = unchecked((ushort)(SpcDriverData.Music.CenterPan << 8));
            channel.InstrumentId = 0;
            channel.FineTune = 0;
            channel.ChannelTransposition = 0;
            channel.PitchEnvelopeTicks = 0;
            channel.CutKey = 0;
            channel.VibratoDepth = 0;
            channel.TremoloDepth = 0;
        }
        currentChannelBit = 0;
        masterVolumeFadeTicks = 0;
        echoVolumeFadeTicks = 0;
        tempoFadeTicks = 0;
        globalTransposition = 0;
        blockCount = 0;
        percussionBaseId = 0;
        SetHighByte(ref masterVolume, SpcDriverData.Music.DefaultMasterVolume);
        SetHighByte(ref tempo, SpcDriverData.Music.DefaultTrackTempo);
    }

    private void SetInstrumentWithoutSavingId(ManagedSpcMusicChannel channel, byte instrument)
    {
        if ((instrument & 0x80) != 0) // allow(BitMask): percussion instrument marker
            instrument = unchecked((byte)(instrument + 54 + percussionBaseId)); // allow(HardwareMagnitude): native percussion table bias
        int address = SpcDriverData.Ram.InstrumentTable + instrument * SpcDriverData.Ram.InstrumentRecordSize;
        if ((channelOnMask & currentChannelBit) != 0)
            return;

        byte register = unchecked((byte)(channel.Index * SnesDspRegisterMap.VoiceStride));
        if ((ram[address] & 0x80) != 0) // allow(BitMask): noise-instrument marker
        {
            dspFlags = unchecked((byte)((dspFlags & SpcDriverData.Echo.WriteDisable) |
                (ram[address] & SnesDspRegisterMap.Fields.NoiseRateMask)));
            noiseEnable |= currentChannelBit;
            WriteDsp(unchecked((byte)(register + SnesDspRegisterMap.Voice.SourceNumber)), 0);
        }
        else
        {
            WriteDsp(unchecked((byte)(register + SnesDspRegisterMap.Voice.SourceNumber)), ram[address]);
        }
        WriteDsp(unchecked((byte)(register + SnesDspRegisterMap.Voice.Adsr1)), ram[address + 1]);
        WriteDsp(unchecked((byte)(register + SnesDspRegisterMap.Voice.Adsr2)), ram[address + 2]);
        WriteDsp(unchecked((byte)(register + SnesDspRegisterMap.Voice.Gain)), ram[address + 3]);
        channel.InstrumentPitchBase = unchecked((ushort)((ram[address + 4] << 8) | ram[address + 5]));
    }

    private void SetInstrument(ManagedSpcMusicChannel channel, byte instrument)
    {
        channel.InstrumentId = instrument;
        SetInstrumentWithoutSavingId(channel, instrument);
    }

    private static void ComputePitchAdd(ManagedSpcMusicChannel channel, byte pitch)
    {
        channel.PitchTarget = unchecked((byte)(pitch & 0x7f)); // allow(BitMask): seven-bit note number
        channel.PitchAddPerTick = DivideSignedFixedPoint(
            channel.PitchTarget - (channel.Pitch >> 8), channel.PitchSlideLength);
    }

    private void CheckPitchSlideToNote(ManagedSpcMusicChannel channel)
    {
        if (channel.PitchSlideLength != 0 || ram[channel.PatternOrderPointer] != (byte)SpcMusicEffect.PitchSlide)
            return;
        channel.PatternOrderPointer++;
        channel.PitchSlideDelayLeft = ram[channel.PatternOrderPointer++];
        channel.PitchSlideLength = ram[channel.PatternOrderPointer++];
        ComputePitchAdd(channel, unchecked((byte)(ram[channel.PatternOrderPointer++] +
            globalTransposition + channel.ChannelTransposition)));
    }

    private void HandleEffect(ManagedSpcMusicChannel channel, byte rawEffect)
    {
        TraceDriver($"ch{channel.Index} effect {rawEffect:X2} at {channel.PatternOrderPointer - 1:X4}");
        int tableIndex = rawEffect - SpcDriverData.Music.FirstEffect;
        if ((uint)tableIndex >= SpcMusicTables.EffectByteLengths.Length)
            throw new InvalidDataException($"Unknown SPC music effect ${rawEffect:X2}.");
        byte argument = SpcMusicTables.EffectByteLengths[tableIndex] != 0
            ? ram[channel.PatternOrderPointer++]
            : (byte)0;

        switch ((SpcMusicEffect)rawEffect)
        {
            case SpcMusicEffect.SetInstrument:
                SetInstrument(channel, argument);
                break;
            case SpcMusicEffect.SetPan:
                channel.PanFlags = argument;
                channel.PanValue = unchecked((ushort)((argument & 0x1f) << 8)); // allow(BitMask): five-bit pan value
                break;
            case SpcMusicEffect.FadePan:
                channel.PanTicks = argument;
                channel.PanTarget = ram[channel.PatternOrderPointer++];
                channel.PanAddPerTick = DivideSignedFixedPoint(
                    channel.PanTarget - (channel.PanValue >> 8), argument);
                break;
            case SpcMusicEffect.EnableVibrato:
                channel.VibratoDelayTicks = argument;
                channel.VibratoRate = ram[channel.PatternOrderPointer++];
                channel.VibratoDepthTarget = channel.VibratoDepth = ram[channel.PatternOrderPointer++];
                channel.VibratoFadeTicks = 0;
                break;
            case SpcMusicEffect.DisableVibrato:
                channel.VibratoDepthTarget = channel.VibratoDepth = 0;
                channel.VibratoFadeTicks = 0;
                break;
            case SpcMusicEffect.SetMasterVolume:
                masterVolume = unchecked((ushort)(argument << 8));
                break;
            case SpcMusicEffect.FadeMasterVolume:
                masterVolumeFadeTicks = argument;
                masterVolumeFadeTarget = ram[channel.PatternOrderPointer++];
                masterVolumeFadeAdd = DivideSignedFixedPoint(
                    masterVolumeFadeTarget - (masterVolume >> 8), argument);
                break;
            case SpcMusicEffect.SetTempo:
                tempo = unchecked((ushort)(argument << 8));
                break;
            case SpcMusicEffect.FadeTempo:
                tempoFadeTicks = argument;
                tempoFadeTarget = ram[channel.PatternOrderPointer++];
                tempoFadeAdd = DivideSignedFixedPoint(tempoFadeTarget - (tempo >> 8), argument);
                break;
            case SpcMusicEffect.SetGlobalTransposition:
                globalTransposition = argument;
                break;
            case SpcMusicEffect.SetChannelTransposition:
                channel.ChannelTransposition = argument;
                break;
            case SpcMusicEffect.EnableTremolo:
                channel.TremoloDelayTicks = argument;
                channel.TremoloRate = ram[channel.PatternOrderPointer++];
                channel.TremoloDepth = ram[channel.PatternOrderPointer++];
                break;
            case SpcMusicEffect.DisableTremolo:
                channel.TremoloDepth = 0;
                break;
            case SpcMusicEffect.SetChannelVolume:
                channel.ChannelVolume = unchecked((ushort)(argument << 8));
                break;
            case SpcMusicEffect.FadeChannelVolume:
                channel.VolumeFadeTicks = argument;
                channel.VolumeFadeTarget = ram[channel.PatternOrderPointer++];
                channel.VolumeFadeAddPerTick = DivideSignedFixedPoint(
                    channel.VolumeFadeTarget - (channel.ChannelVolume >> 8), argument);
                break;
            case SpcMusicEffect.CallPattern:
                channel.PatternStartPointer = unchecked((ushort)(
                    (ram[channel.PatternOrderPointer++] << 8) | argument));
                channel.SubroutineLoops = ram[channel.PatternOrderPointer++];
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
                channel.PitchEnvelopeDirection = rawEffect == (byte)SpcMusicEffect.PitchEnvelopeTo
                    ? (byte)1 : (byte)0;
                channel.PitchEnvelopeDelay = argument;
                channel.PitchEnvelopeTicks = ram[channel.PatternOrderPointer++];
                channel.PitchEnvelopeSlide = ram[channel.PatternOrderPointer++];
                break;
            case SpcMusicEffect.DisablePitchEnvelope:
                channel.PitchEnvelopeTicks = 0;
                break;
            case SpcMusicEffect.SetFineTune:
                channel.FineTune = argument;
                break;
            case SpcMusicEffect.EnableEcho:
                echoEnable = argument;
                echoVolumeLeft = unchecked((ushort)(ram[channel.PatternOrderPointer++] << 8));
                echoVolumeRight = unchecked((ushort)(ram[channel.PatternOrderPointer++] << 8));
                dspFlags &= unchecked((byte)~SpcDriverData.Echo.WriteDisable);
                break;
            case SpcMusicEffect.DisableEcho:
                echoVolumeLeft = echoVolumeRight = 0;
                dspFlags |= SpcDriverData.Echo.WriteDisable;
                break;
            case SpcMusicEffect.ConfigureEcho:
                SetupEchoDelay(argument);
                echoFeedback = ram[channel.PatternOrderPointer++];
                int preset = ram[channel.PatternOrderPointer++];
                int firOffset = preset * SpcDriverData.Echo.FirTapCount;
                if (firOffset > SpcMusicTables.EchoFirParameters.Length - SpcDriverData.Echo.FirTapCount)
                    throw new InvalidDataException($"SPC echo FIR preset {preset} is outside the cartridge table.");
                for (int tap = 0; tap < SpcDriverData.Echo.FirTapCount; tap++)
                {
                    WriteDsp(unchecked((byte)(SnesDspRegisterMap.Global.FirstFirCoefficient +
                        tap * SnesDspRegisterMap.VoiceStride)),
                        SpcMusicTables.EchoFirParameters[firOffset + tap]);
                }
                break;
            case SpcMusicEffect.FadeEchoVolume:
                echoVolumeFadeTicks = argument;
                echoVolumeFadeTargetLeft = ram[channel.PatternOrderPointer++];
                echoVolumeFadeTargetRight = ram[channel.PatternOrderPointer++];
                echoVolumeFadeAddLeft = DivideSignedFixedPoint(
                    echoVolumeFadeTargetLeft - (echoVolumeLeft >> 8), argument);
                echoVolumeFadeAddRight = DivideSignedFixedPoint(
                    echoVolumeFadeTargetRight - (echoVolumeRight >> 8), argument);
                break;
            case SpcMusicEffect.PitchSlide:
                channel.PitchSlideDelayLeft = argument;
                channel.PitchSlideLength = ram[channel.PatternOrderPointer++];
                ComputePitchAdd(channel, unchecked((byte)(ram[channel.PatternOrderPointer++] +
                    globalTransposition + channel.ChannelTransposition)));
                break;
            case SpcMusicEffect.SetPercussionBase:
                percussionBaseId = argument;
                break;
            case SpcMusicEffect.SkipByte:
                channel.PatternOrderPointer++;
                break;
            case SpcMusicEffect.CutKey:
                channel.CutKey = unchecked((byte)(argument + 1));
                break;
            case SpcMusicEffect.FastForwardForFrames:
                fastForward = unchecked((byte)(argument + 1));
                keyOff |= unchecked((byte)~channelOnMask);
                break;
            case SpcMusicEffect.SetFastForward:
                fastForward = argument;
                keyOff |= unchecked((byte)~channelOnMask);
                break;
            default:
                throw new InvalidDataException($"Unknown SPC music effect ${rawEffect:X2}.");
        }
    }

    private bool WantsKeyOff(ManagedSpcMusicChannel channel)
    {
        int loops = channel.SubroutineLoops;
        ushort pointer = channel.PatternOrderPointer;
        for (int instructions = 0; instructions < SpcDriverData.MaximumFastForwardTicks; instructions++)
        {
            byte command = ram[pointer++];
            if (command == 0)
            {
                if (loops == 0)
                    return true;
                loops--;
                pointer = loops == 0 ? channel.SavedPatternPointer : channel.PatternStartPointer;
            }
            else
            {
                while ((command & 0x80) == 0) // allow(BitMask): music-command marker
                    command = ram[pointer++];
                if (command == SpcDriverData.Music.TieNote)
                    return false;
                if (command == (byte)SpcMusicEffect.CallPattern)
                    pointer = ReadWord(pointer);
                else if (command >= SpcDriverData.Music.FirstEffect)
                    pointer = AddWord(pointer, SpcMusicTables.EffectByteLengths[command - SpcDriverData.Music.FirstEffect]);
                else
                    return true;
            }
        }
        throw new InvalidDataException("SPC key-off lookahead did not reach a note boundary.");
    }

    private void CalculateFinalVolume(ManagedSpcMusicChannel channel, byte volume)
    {
        int scaled = HighByte(masterVolume) * volume >> 8;
        scaled = scaled * channel.ChannelVolumeMaster >> 8;
        scaled = scaled * HighByte(channel.ChannelVolume) >> 8;
        channel.FinalVolume = unchecked((byte)(scaled * scaled >> 8));
    }

    private void CalculateTremolo(ManagedSpcMusicChannel channel, byte value)
    {
        value = (value & 0x80) != 0 // allow(BitMask): tremolo waveform phase
            ? unchecked((byte)((value * 2) ^ byte.MaxValue))
            : unchecked((byte)(value * 2));
        value = unchecked((byte)(value * channel.TremoloDepth >> 8));
        CalculateFinalVolume(channel, unchecked((byte)(value ^ byte.MaxValue)));
    }

    private void HandleTremolo(ManagedSpcMusicChannel channel)
    {
        affectedVolumeOrPitch |= 0x80; // allow(BitMask): dirty result marker
        CalculateTremolo(channel, unchecked((byte)(channel.TremoloRate * mainTempoAccumulator >> 8)));
    }

    private void CalculateVibratoPitch(ManagedSpcMusicChannel channel, ushort pitch, byte value)
    {
        int scaled = value << 2;
        scaled ^= (scaled & 0x100) != 0 ? byte.MaxValue : 0; // allow(BitMask): folded vibrato phase
        int delta = channel.VibratoDepth >= 0xf1 // allow(HardwareMagnitude): native deep-vibrato threshold
            ? unchecked((byte)scaled) * (channel.VibratoDepth & 0x0f) // allow(BitMask): deep-vibrato fractional depth
            : unchecked((byte)scaled) * channel.VibratoDepth >> 8;
        WritePitch(channel, AddWord(pitch, (value & 0x80) != 0 ? -delta : delta)); // allow(BitMask): vibrato direction
    }

    private void HandlePanAndSweep(ManagedSpcMusicChannel channel)
    {
        affectedVolumeOrPitch = 0;
        if (channel.TremoloDepth != 0 && channel.TremoloHoldCount == channel.TremoloDelayTicks)
            HandleTremolo(channel);

        ushort volume = channel.PanValue;
        if (channel.PanTicks != 0)
        {
            affectedVolumeOrPitch = 0x80; // allow(BitMask): dirty result marker
            volume = AddWord(volume, mainTempoAccumulator * unchecked((short)channel.PanAddPerTick) / 256);
        }
        if (affectedVolumeOrPitch != 0)
            WriteVolume(channel, volume);

        affectedVolumeOrPitch = 0;
        ushort pitch = channel.Pitch;
        if (channel.PitchSlideLength != 0 && channel.PitchSlideDelayLeft == 0)
        {
            affectedVolumeOrPitch |= 0x80; // allow(BitMask): dirty result marker
            pitch = AddWord(pitch, mainTempoAccumulator * unchecked((short)channel.PitchAddPerTick) / 256);
        }
        if (channel.VibratoDepth != 0 && channel.VibratoDelayTicks == channel.VibratoHoldCount)
        {
            CalculateVibratoPitch(channel, pitch, unchecked((byte)(
                (mainTempoAccumulator * channel.VibratoRate >> 8) + channel.VibratoCount)));
            return;
        }
        if (affectedVolumeOrPitch != 0)
            WritePitch(channel, pitch);
    }

    private void HandleNoteTick(ManagedSpcMusicChannel channel)
    {
        if (channel.NoteKeyOffTicksLeft != 0 &&
            (--channel.NoteKeyOffTicksLeft == 0 || channel.NoteTicksLeft == 2))
        {
            if (WantsKeyOff(channel) && (currentChannelBit & channelOnMask) == 0)
                WriteDsp(SnesDspRegisterMap.Global.KeyOff, currentChannelBit);
        }

        affectedVolumeOrPitch = 0;
        if (channel.PitchSlideLength != 0)
        {
            if (channel.PitchSlideDelayLeft != 0)
            {
                channel.PitchSlideDelayLeft--;
            }
            else
            {
                affectedVolumeOrPitch = 0x80; // allow(BitMask): dirty result marker
                channel.PitchSlideLength--;
                channel.Pitch = channel.PitchSlideLength == 0
                    ? unchecked((ushort)((channel.PitchTarget << 8) | channel.FineTune))
                    : AddWord(channel.Pitch, channel.PitchAddPerTick);
            }
        }

        ushort pitch = channel.Pitch;
        if (channel.VibratoDepth != 0)
        {
            if (channel.VibratoDelayTicks == channel.VibratoHoldCount)
            {
                if (channel.VibratoChangeCount == channel.VibratoFadeTicks)
                    channel.VibratoDepth = channel.VibratoDepthTarget;
                else
                    channel.VibratoDepth = unchecked((byte)((channel.VibratoChangeCount++ == 0
                        ? 0 : channel.VibratoDepth) + channel.VibratoFadeAddPerTick));
                channel.VibratoCount = unchecked((byte)(channel.VibratoCount + channel.VibratoRate));
                CalculateVibratoPitch(channel, pitch, channel.VibratoCount);
                return;
            }
            channel.VibratoHoldCount++;
        }
        if (affectedVolumeOrPitch != 0)
            WritePitch(channel, pitch);
    }

    private void HandleChannelTick(ManagedSpcMusicChannel channel)
    {
        if (channel.VolumeFadeTicks != 0)
        {
            channel.VolumeFadeTicks--;
            channel.ChannelVolume = channel.VolumeFadeTicks == 0
                ? unchecked((ushort)(channel.VolumeFadeTarget << 8))
                : AddWord(channel.ChannelVolume, channel.VolumeFadeAddPerTick);
            volumeDirty |= currentChannelBit;
        }
        if (channel.TremoloDepth != 0)
        {
            if (channel.TremoloDelayTicks == channel.TremoloHoldCount)
            {
                volumeDirty |= currentChannelBit;
                channel.TremoloCount = (channel.TremoloCount & 0x80) != 0 && // allow(BitMask): tremolo phase
                    channel.TremoloDepth == byte.MaxValue
                    ? (byte)0x80 // allow(BitMask): tremolo phase origin
                    : unchecked((byte)(channel.TremoloCount + channel.TremoloRate));
                CalculateTremolo(channel, channel.TremoloCount);
            }
            else
            {
                channel.TremoloHoldCount++;
                CalculateFinalVolume(channel, byte.MaxValue);
            }
        }
        else
        {
            CalculateFinalVolume(channel, byte.MaxValue);
        }

        if (channel.PanTicks != 0)
        {
            channel.PanTicks--;
            channel.PanValue = channel.PanTicks == 0
                ? unchecked((ushort)(channel.PanTarget << 8))
                : AddWord(channel.PanValue, channel.PanAddPerTick);
            volumeDirty |= currentChannelBit;
        }
        if ((volumeDirty & currentChannelBit) != 0)
            WriteVolume(channel, channel.PanValue);
    }

    private void HandleMusicCommand()
    {
        TraceDriver($"music entry ptr={musicTopLevelPointer:X4} ff={fastForward:X2} ctr={counter:X4}");
        byte command = inputPorts[AudioRomData.Apu.MusicPort];
        inputPorts[AudioRomData.Apu.MusicPort] = SpcDriverData.NoPortCommand;
        if (command == SpcDriverData.PauseMusicCommand)
        {
            keyOff |= unchecked((byte)~channelOnMask);
            return;
        }

        if (command != SpcDriverData.ResumeMusicCommand && command != SpcDriverData.NoPortCommand &&
            command != portsToSnes[AudioRomData.Apu.MusicPort])
        {
            StartTrack(command);
            return;
        }
        if (portsToSnes[AudioRomData.Apu.MusicPort] == 0)
            return;
        if (counter == 0)
            goto ProcessChannels;
        if (counter != 0)
        {
            counter--;
            if (counter != 0)
            {
                ResetMusicChannels();
                return;
            }
        }

    NextPhrase:
        ushort patternTable;
        for (int records = 0; ; records++)
        {
            if (records > SpcDriverData.MaximumFastForwardTicks)
                throw new InvalidDataException("SPC top-level music stream did not reach a pattern table.");
            ushort record = ReadWord(musicTopLevelPointer);
            TraceDriver($"top {musicTopLevelPointer:X4}={record:X4}");
            musicTopLevelPointer += 2;
            if (HighByte(record) >= SpcDriverData.Music.PatternPointerHighByteMinimum)
            {
                patternTable = record;
                break;
            }
            if (record == 0)
            {
                StartTrack(0);
                return;
            }
            if (record == SpcDriverData.Music.PatternFastForwardOn)
                fastForward = SpcDriverData.Music.PatternFastForwardOn;
            else if (record == SpcDriverData.Music.PatternFastForwardOff)
                fastForward = 0;
            else
            {
                blockCount--;
                if ((blockCount & 0x80) != 0) // allow(BitMask): signed block-counter underflow
                    blockCount = unchecked((byte)record);
                ushort loopPointer = ReadWord(musicTopLevelPointer);
                musicTopLevelPointer += 2;
                if (blockCount != 0)
                    musicTopLevelPointer = loopPointer;
            }
        }

        for (int index = 0; index < channels.Length; index++)
        {
            channels[index].PatternOrderPointer = ReadWord(patternTable);
            patternTable += 2;
        }
        for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
        {
            ManagedSpcMusicChannel channel = channels[index];
            currentChannelBit = unchecked((byte)bit);
            if (HighByte(channel.PatternOrderPointer) != 0 && channel.InstrumentId == 0)
                SetInstrument(channel, 0);
            channel.SubroutineLoops = 0;
            channel.VolumeFadeTicks = 0;
            channel.PanTicks = 0;
            channel.NoteTicksLeft = 1;
        }
        currentChannelBit = 0;

    ProcessChannels:
        volumeDirty = 0;
        for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
        {
            ManagedSpcMusicChannel channel = channels[index];
            currentChannelBit = unchecked((byte)bit);
            if (HighByte(channel.PatternOrderPointer) == 0)
                continue;
            channel.NoteTicksLeft--;
            if (channel.NoteTicksLeft == 0)
            {
                for (int instructions = 0; ; instructions++)
                {
                    if (instructions > SpcDriverData.MaximumFastForwardTicks)
                        throw new InvalidDataException($"SPC channel {index} did not reach a timed note.");
                    byte noteCommand = ram[channel.PatternOrderPointer++];
                    TraceDriver($"ch{index} cmd {noteCommand:X2} at {channel.PatternOrderPointer - 1:X4}");
                    if (noteCommand == 0)
                    {
                        if (channel.SubroutineLoops == 0)
                            goto NextPhrase;
                        channel.SubroutineLoops--;
                        channel.PatternOrderPointer = channel.SubroutineLoops == 0
                            ? channel.SavedPatternPointer : channel.PatternStartPointer;
                        continue;
                    }
                    if ((noteCommand & 0x80) == 0) // allow(BitMask): music-command marker
                    {
                        channel.NoteLength = noteCommand;
                        noteCommand = ram[channel.PatternOrderPointer++];
                        if ((noteCommand & 0x80) == 0) // allow(BitMask): music-command marker
                        {
                            channel.NoteGateOffFixedPoint = SpcMusicTables.NoteGateOffPercentages[
                                (noteCommand >> 4) & 7];
                            channel.ChannelVolumeMaster = SpcMusicTables.NoteVolumes[noteCommand & 0x0f]; // allow(BitMask): volume nibble
                            noteCommand = ram[channel.PatternOrderPointer++];
                        }
                    }
                    if (noteCommand >= SpcDriverData.Music.FirstEffect)
                    {
                        HandleEffect(channel, noteCommand);
                        continue;
                    }
                    if ((channel.CutKey | fastForward) == 0)
                        PlayNote(channel, noteCommand);
                    channel.NoteTicksLeft = channel.NoteLength;
                    int gateTicks = channel.NoteTicksLeft * channel.NoteGateOffFixedPoint >> 8;
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
        currentChannelBit = 0;

        if (tempoFadeTicks != 0)
        {
            tempoFadeTicks--;
            tempo = tempoFadeTicks == 0
                ? unchecked((ushort)(tempoFadeTarget << 8))
                : AddWord(tempo, tempoFadeAdd);
        }
        if (echoVolumeFadeTicks != 0)
        {
            echoVolumeLeft = AddWord(echoVolumeLeft, echoVolumeFadeAddLeft);
            echoVolumeRight = AddWord(echoVolumeRight, echoVolumeFadeAddRight);
            echoVolumeFadeTicks--;
            if (echoVolumeFadeTicks == 0)
            {
                echoVolumeLeft = unchecked((ushort)(echoVolumeFadeTargetLeft << 8));
                echoVolumeRight = unchecked((ushort)(echoVolumeFadeTargetRight << 8));
            }
        }
        if (masterVolumeFadeTicks != 0)
        {
            masterVolumeFadeTicks--;
            masterVolume = masterVolumeFadeTicks == 0
                ? unchecked((ushort)(masterVolumeFadeTarget << 8))
                : AddWord(masterVolume, masterVolumeFadeAdd);
            volumeDirty = byte.MaxValue;
        }
        for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
        {
            currentChannelBit = unchecked((byte)bit);
            if (HighByte(channels[index].PatternOrderPointer) != 0)
                HandleChannelTick(channels[index]);
        }
        currentChannelBit = 0;
    }

    private void StartTrack(byte track)
    {
        portsToSnes[AudioRomData.Apu.MusicPort] = track;
        // Track zero deliberately indexes the word immediately before the numbered table.
        musicTopLevelPointer = ReadWord(SpcDriverData.Ram.DefaultMusicPointer + track * 2);
        counter = SpcDriverData.Music.TrackStartupTicks;
        keyOff |= unchecked((byte)~channelOnMask);
    }

    private void PlayNote(ManagedSpcMusicChannel channel, byte note)
    {
        if (note >= SpcDriverData.Music.FirstPercussionNote)
        {
            SetInstrument(channel, note);
            note = SpcDriverData.Music.PercussionPlaybackNote;
        }
        if (note >= SpcDriverData.Music.TieNote || (channelOnMask & currentChannelBit) != 0)
            return;

        channel.Pitch = unchecked((ushort)((((note & 0x7f) + globalTransposition + // allow(BitMask): seven-bit note number
            channel.ChannelTransposition) << 8) | channel.FineTune));
        channel.VibratoCount = unchecked((byte)(channel.VibratoFadeTicks << 7));
        channel.VibratoHoldCount = 0;
        channel.VibratoChangeCount = 0;
        channel.TremoloCount = 0;
        channel.TremoloHoldCount = 0;
        volumeDirty |= currentChannelBit;
        keyOn |= currentChannelBit;
        channel.PitchSlideLength = channel.PitchEnvelopeTicks;
        if (channel.PitchSlideLength != 0)
        {
            channel.PitchSlideDelayLeft = channel.PitchEnvelopeDelay;
            if (channel.PitchEnvelopeDirection == 0)
                channel.Pitch = AddWord(channel.Pitch, -(channel.PitchEnvelopeSlide << 8));
            ComputePitchAdd(channel, unchecked((byte)((channel.Pitch >> 8) + channel.PitchEnvelopeSlide)));
        }
        WritePitch(channel, channel.Pitch);
    }
}
