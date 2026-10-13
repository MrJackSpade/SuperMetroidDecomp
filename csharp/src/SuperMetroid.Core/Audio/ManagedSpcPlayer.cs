using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Audio;

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
    /// <summary>Interpolates the native pan table into stereo DSP volumes unless a sound effect currently borrows the voice.</summary>
    private void WriteVolume(ManagedSpcMusicChannel channel, ushort volume)
    {
        // A sound effect owns this hardware voice while its bit is set. Music state continues
        // advancing underneath it, but must not overwrite the SFX's live DSP registers.
        if ((channelOnMask & currentChannelBit) != 0)
            return;

        for (int side = 0; side < 2; side++)
        {
            int panIndex = SpcFixedPoint.Whole(volume);
            int baseVolume;
            int nextVolume;
            if (panIndex >= SpcMusicTables.PanSampleCount)
            {
                // Indices21 and above start at the echo FIR data after the21 pan samples.
                // Preserve the existing mutable SPC-RAM compatibility branch.
                int address = panIndex + SpcDriverData.Music.PanVolumeTableAddress;
                (baseVolume, nextVolume) = ReadPanVolumePair(address);
            }
            else
            {
                baseVolume = SpcMusicTables.PanVolume(panIndex);
                nextVolume = SpcMusicTables.PanVolume(panIndex + 1);
            }

            // SPC SBC stores an eight-bit result before MUL YA. Descending adjacent
            // RAM bytes therefore wrap the difference instead of multiplying a negative.
            byte delta = unchecked((byte)(nextVolume - baseVolume));
            byte interpolated = unchecked((byte)(baseVolume + SpcFixedPoint.Scale(delta, SpcFixedPoint.Fraction(volume))));
            byte final = (byte)SpcFixedPoint.Scale(interpolated, channel.FinalVolume);
            if (((channel.PanFlags << side) & 0x80) != 0) // allow(BitMask): phase-inversion bit
                final = unchecked((byte)-final);
            Dsp.Voice(channel.Index).WriteVolume((DspStereoSide)side, unchecked((sbyte)final));
            volume = unchecked((ushort)(SpcDriverData.Music.FullyLeftPan - volume));
        }
    }

    /// <summary>Converts a fractional note through the twelve-semitone table and instrument pitch base into DSP frequency.</summary>
    private void WritePitchInner(ManagedSpcMusicChannel channel, ushort pitch)
    {
        byte note = unchecked((byte)(SpcFixedPoint.Whole(pitch) & SpcDriverData.Music.NoteNumberMask));
        byte octave = unchecked((byte)(note / 12));
        byte semitone = unchecked((byte)(note % 12));
        ushort basis = SpcMusicTables.BaseNoteFrequency(semitone);
        int delta = unchecked((byte)(SpcMusicTables.BaseNoteFrequency(semitone + 1) - basis));
        ushort frequency = unchecked((ushort)(basis + SpcFixedPoint.Scale(delta, SpcFixedPoint.Fraction(pitch))));
        frequency = unchecked((ushort)(frequency * 2));
        while (octave != 6)
        {
            frequency >>= 1;
            octave++;
        }

        frequency = unchecked((ushort)SpcFixedPoint.Scale(channel.InstrumentPitchBase, frequency));
        if ((currentChannelBit & channelOnMask) == 0)
        {
            Dsp.Voice(channel.Index).WritePitch(frequency);
        }
    }

    /// <summary>Applies the driver's high/low note correction before DSP pitch conversion.</summary>
    private void WritePitch(ManagedSpcMusicChannel channel, ushort pitch)
    {
        if (SpcFixedPoint.Whole(pitch) >= 0x34) // allow(HardwareMagnitude): native high-note correction threshold
            pitch = AddWord(pitch, SpcFixedPoint.Whole(pitch) - 0x34); // allow(HardwareMagnitude): native high-note correction threshold
        else if (SpcFixedPoint.Whole(pitch) < 0x13) // allow(HardwareMagnitude): native low-note correction threshold
            pitch = AddWord(pitch, unchecked((byte)((SpcFixedPoint.Whole(pitch) - 0x13) * 2)) - 256); // allow(HardwareMagnitude): native low-note correction
        WritePitchInner(channel, pitch);
    }

    /// <summary>Restores eight music voices, fades, transposition and tempo to the driver's track defaults.</summary>
    private void ResetMusicChannels()
    {
        for (int index = channels.Length - 1, bit = 0x80; index >= 0; index--, bit >>= 1) // allow(BitMask): highest voice bit
        {
            ManagedSpcMusicChannel channel = channels[index];
            currentChannelBit = unchecked((byte)bit);
            channel.ChannelVolume = SpcFixedPoint.WithWhole(channel.ChannelVolume, SpcDriverData.Music.DefaultChannelVolume);
            channel.PanFlags = SpcDriverData.Music.CenterPan;
            channel.PanValue = SpcFixedPoint.FromWhole(SpcDriverData.Music.CenterPan);
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
        masterVolume = SpcFixedPoint.WithWhole(masterVolume, SpcDriverData.Music.DefaultMasterVolume);
        tempo = SpcFixedPoint.WithWhole(tempo, SpcDriverData.Music.DefaultTrackTempo);
    }

    /// <summary>Installs an instrument's source, envelope, gain and pitch base without replacing the channel's remembered ID.</summary>
    private void SetInstrumentWithoutSavingId(ManagedSpcMusicChannel channel, byte instrument)
    {
        if ((instrument & 0x80) != 0) // allow(BitMask): percussion instrument marker
            instrument = unchecked((byte)(instrument + 54 + percussionBaseId)); // allow(HardwareMagnitude): native percussion table bias
        int address = SpcDriverData.Ram.InstrumentTable + instrument * SpcDriverData.Ram.InstrumentRecordSize;
        if ((channelOnMask & currentChannelBit) != 0)
            return;

        SnesDspVoiceControls voice = Dsp.Voice(channel.Index);
        SpcInstrumentRecord record = ReadInstrument(address);
        if ((record.SourceOrNoiseRate & SpcDriverData.Instruments.NoiseMarker) != 0)
        {
            dspFlags = unchecked((byte)((dspFlags & SpcDriverData.Echo.WriteDisable) |
                DspFlags.NoiseRateOf(record.SourceOrNoiseRate)));
            noiseEnable |= currentChannelBit;
            voice.WriteSource(0);
        }
        else
        {
            voice.WriteSource(record.SourceOrNoiseRate);
        }
        voice.WriteAdsr(record.Adsr1, record.Adsr2);
        voice.WriteGain(record.Gain);
        channel.InstrumentPitchBase = record.PitchBase;
    }

    /// <summary>Remembers the selected instrument before installing its DSP voice registers.</summary>
    private void SetInstrument(ManagedSpcMusicChannel channel, byte instrument)
    {
        channel.InstrumentId = instrument;
        SetInstrumentWithoutSavingId(channel, instrument);
    }

    /// <summary>Derives the signed per-tick pitch-slide increment toward a transposed target note.</summary>
    private static void ComputePitchAdd(ManagedSpcMusicChannel channel, byte pitch)
    {
        channel.PitchTarget = unchecked((byte)(pitch & SpcDriverData.Music.NoteNumberMask));
        channel.PitchAddPerTick = SpcFixedPoint.DivideSigned(
            channel.PitchTarget - SpcFixedPoint.Whole(channel.Pitch), channel.PitchSlideLength);
    }

    /// <summary>Consumes a following slide opcode immediately after a note when no slide is already active.</summary>
    private void CheckPitchSlideToNote(ManagedSpcMusicChannel channel)
    {
        if (channel.PitchSlideLength != 0 || PeekSequenceByte(channel.PatternOrderPointer) != (byte)SpcMusicEffect.PitchSlide)
            return;
        channel.PatternOrderPointer++;
        channel.PitchSlideDelayLeft = NextSequenceByte(ref channel.PatternOrderPointer);
        channel.PitchSlideLength = NextSequenceByte(ref channel.PatternOrderPointer);
        ComputePitchAdd(channel, unchecked((byte)(NextSequenceByte(ref channel.PatternOrderPointer) +
            globalTransposition + channel.ChannelTransposition)));
    }

    /// <summary>Decodes one music effect and its native operand length, updating channel or global driver state.</summary>
    private void HandleEffect(ManagedSpcMusicChannel channel, byte rawEffect)
    {
        int tableIndex = rawEffect - SpcDriverData.Music.FirstEffect;
        if ((uint)tableIndex >= SpcMusicTables.EffectCount)
            throw new InvalidDataException($"Unknown SPC music effect ${rawEffect:X2}.");
        byte argument = SpcMusicTables.EffectByteLength(tableIndex) != 0
            ? NextSequenceByte(ref channel.PatternOrderPointer)
            : (byte)0;

        switch ((SpcMusicEffect)rawEffect)
        {
            case SpcMusicEffect.SetInstrument:
                SetInstrument(channel, argument);
                break;
            case SpcMusicEffect.SetPan:
                channel.PanFlags = argument;
                channel.PanValue = SpcFixedPoint.FromWhole((argument & 0x1f)); // allow(BitMask): five-bit pan value
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
                channel.PatternStartPointer = unchecked((ushort)(
                    SnesLittleEndian.Word(argument, NextSequenceByte(ref channel.PatternOrderPointer))));
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
                channel.PitchEnvelopeDirection = rawEffect == (byte)SpcMusicEffect.PitchEnvelopeTo
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
                echoEnable = argument;
                echoVolumeLeft = SpcFixedPoint.FromWhole(NextSequenceByte(ref channel.PatternOrderPointer));
                echoVolumeRight = SpcFixedPoint.FromWhole(NextSequenceByte(ref channel.PatternOrderPointer));
                dspFlags &= unchecked((byte)~SpcDriverData.Echo.WriteDisable);
                break;
            case SpcMusicEffect.DisableEcho:
                echoVolumeLeft = echoVolumeRight = 0;
                dspFlags |= SpcDriverData.Echo.WriteDisable;
                break;
            case SpcMusicEffect.ConfigureEcho:
                SetupEchoDelay(argument);
                echoFeedback = NextSequenceByte(ref channel.PatternOrderPointer);
                int preset = NextSequenceByte(ref channel.PatternOrderPointer);
                int firOffset = unchecked((byte)(preset * SpcDriverData.Echo.FirTapCount));
                // `$F7` indexes from APU `$1E32`, not from an abstract four-entry host
                // array. Retail sequences use values beyond three: Kraid's post-defeat
                // room track executes `F7 02 0A 0A` and therefore reads the eight resident
                // driver bytes at `$1E82`. Reading the actual uploaded address space also
                // preserves changes to those adjacent bytes. Native MUL YA then MOV X,A
                // keeps only the product's low byte: presets32..255 alias0..31. Each
                // eight-tap group ends at or before offset255; no tap carries past X.
                for (int tap = 0; tap < SpcDriverData.Echo.FirTapCount; tap++)
                {
                    Dsp.Global.WriteFirCoefficient(tap,
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

    /// <summary>Looks ahead through pattern calls, effects and loops to keep a tied note keyed on.</summary>
    private bool WantsKeyOff(ManagedSpcMusicChannel channel)
    {
        int loops = channel.SubroutineLoops;
        ushort pointer = channel.PatternOrderPointer;
        for (int instructions = 0; instructions < SpcDriverData.MaximumFastForwardTicks; instructions++)
        {
            byte command = NextSequenceByte(ref pointer);
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
                    command = NextSequenceByte(ref pointer);
                if (command == SpcDriverData.Music.TieNote)
                    return false;
                if (command == (byte)SpcMusicEffect.CallPattern)
                    pointer = ReadWord(pointer);
                else if (command >= SpcDriverData.Music.FirstEffect)
                    pointer = AddWord(pointer, SpcMusicTables.EffectByteLength(command - SpcDriverData.Music.FirstEffect));
                else
                    return true;
            }
        }
        throw new InvalidDataException("SPC key-off lookahead did not reach a note boundary.");
    }

    /// <summary>Combines master, channel and tremolo volume with the driver's repeated fixed-point scaling.</summary>
    private void CalculateFinalVolume(ManagedSpcMusicChannel channel, byte volume)
    {
        int scaled = SpcFixedPoint.Scale(SpcFixedPoint.Whole(masterVolume), volume);
        scaled = SpcFixedPoint.Scale(scaled, channel.ChannelVolumeMaster);
        scaled = SpcFixedPoint.Scale(scaled, SpcFixedPoint.Whole(channel.ChannelVolume));
        channel.FinalVolume = (byte)SpcFixedPoint.Scale(scaled, scaled);
    }

    /// <summary>Folds the tremolo wave and applies depth before recalculating final voice volume.</summary>
    private void CalculateTremolo(ManagedSpcMusicChannel channel, byte value)
    {
        value = (value & 0x80) != 0 // allow(BitMask): tremolo waveform phase
            ? unchecked((byte)((value * 2) ^ byte.MaxValue))
            : unchecked((byte)(value * 2));
        value = (byte)SpcFixedPoint.Scale(value, channel.TremoloDepth);
        CalculateFinalVolume(channel, unchecked((byte)(value ^ byte.MaxValue)));
    }

    /// <summary>Marks volume dirty and samples tremolo at the current tempo accumulator.</summary>
    private void HandleTremolo(ManagedSpcMusicChannel channel)
    {
        affectedVolumeOrPitch |= 0x80; // allow(BitMask): dirty result marker
        CalculateTremolo(channel, (byte)SpcFixedPoint.Scale(channel.TremoloRate, mainTempoAccumulator));
    }

    /// <summary>Folds the vibrato phase into a signed pitch offset, including the driver's deep-vibrato branch.</summary>
    private void CalculateVibratoPitch(ManagedSpcMusicChannel channel, ushort pitch, byte value)
    {
        int scaled = value << 2;
        scaled ^= (scaled & 0x100) != 0 ? byte.MaxValue : 0; // allow(BitMask): folded vibrato phase
        int delta = channel.VibratoDepth >= 0xf1 // allow(HardwareMagnitude): native deep-vibrato threshold
            ? unchecked((byte)scaled) * (channel.VibratoDepth & 0x0f) // allow(BitMask): deep-vibrato fractional depth
            : SpcFixedPoint.Scale(unchecked((byte)scaled), channel.VibratoDepth);
        WritePitch(channel, AddWord(pitch, (value & 0x80) != 0 ? -delta : delta)); // allow(BitMask): vibrato direction
    }

    /// <summary>Interpolates active pan and pitch sweeps between full music ticks and publishes affected DSP registers.</summary>
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
            (--channel.NoteKeyOffTicksLeft == 0 || channel.NoteTicksLeft == 2))
        {
            if (WantsKeyOff(channel) && (currentChannelBit & channelOnMask) == 0)
                Dsp.Global.KeyOff(new DspVoiceMask(currentChannelBit));
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
                    ? unchecked((ushort)(SnesLittleEndian.Word(channel.FineTune, channel.PitchTarget)))
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

    /// <summary>Advances channel volume, tremolo and pan envelopes for one full tempo tick.</summary>
    private void HandleChannelTick(ManagedSpcMusicChannel channel)
    {
        if (channel.VolumeFadeTicks != 0)
        {
            channel.VolumeFadeTicks--;
            channel.ChannelVolume = channel.VolumeFadeTicks == 0
                ? SpcFixedPoint.FromWhole(channel.VolumeFadeTarget)
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
                ? SpcFixedPoint.FromWhole(channel.PanTarget)
                : AddWord(channel.PanValue, channel.PanAddPerTick);
            volumeDirty |= currentChannelBit;
        }
        if ((volumeDirty & currentChannelBit) != 0)
            WriteVolume(channel, channel.PanValue);
    }

    /// <summary>Consumes the CPU music port and advances track blocks, sequence channels and tempo state.</summary>
    private void HandleMusicCommand()
    {
        byte command = inputPorts[(byte)ApuPort.Music];
        inputPorts[(byte)ApuPort.Music] = (byte)MusicPortCommand.None;
        if (Enum.IsDefined((MusicPortCommand)command))
        {
            switch ((MusicPortCommand)command)
            {
                case MusicPortCommand.Pause:
                    keyOff |= unchecked((byte)~channelOnMask);
                    return;
                case MusicPortCommand.Resume:
                case MusicPortCommand.None:
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MusicPortCommand)} {command}.");
            }
        }
        else if (command != portsToSnes[(byte)ApuPort.Music])
        {
            StartTrack(command);
            return;
        }
        if (portsToSnes[(byte)ApuPort.Music] == 0)
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
            musicTopLevelPointer += 2;
            if (SnesLittleEndian.HighByte(record) >= SpcDriverData.Music.PatternPointerHighByteMinimum)
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
                    _ => throw new InvalidOperationException($"Undefined {nameof(MusicTrackControl)} {(int)control}."),
                };
            }
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
            if (SnesLittleEndian.HighByte(channel.PatternOrderPointer) != 0 && channel.InstrumentId == 0)
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
            if (SnesLittleEndian.HighByte(channel.PatternOrderPointer) == 0)
                continue;
            channel.NoteTicksLeft--;
            if (channel.NoteTicksLeft == 0)
            {
                for (int instructions = 0; ; instructions++)
                {
                    if (instructions > SpcDriverData.MaximumFastForwardTicks)
                        throw new InvalidDataException($"SPC channel {index} did not reach a timed note.");
                    byte noteCommand = NextSequenceByte(ref channel.PatternOrderPointer);
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
                        noteCommand = NextSequenceByte(ref channel.PatternOrderPointer);
                        if ((noteCommand & 0x80) == 0) // allow(BitMask): music-command marker
                        {
                            channel.NoteGateOffFixedPoint = SpcMusicTables.NoteGateOffPercentage((noteCommand >> 4) & 7);
                            channel.ChannelVolumeMaster = SpcMusicTables.NoteVolume(noteCommand & 0x0f); // allow(BitMask): volume nibble
                            noteCommand = NextSequenceByte(ref channel.PatternOrderPointer);
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
        currentChannelBit = 0;

        if (tempoFadeTicks != 0)
        {
            tempoFadeTicks--;
            tempo = tempoFadeTicks == 0
                ? SpcFixedPoint.FromWhole(tempoFadeTarget)
                : AddWord(tempo, tempoFadeAdd);
        }
        if (echoVolumeFadeTicks != 0)
        {
            echoVolumeLeft = AddWord(echoVolumeLeft, echoVolumeFadeAddLeft);
            echoVolumeRight = AddWord(echoVolumeRight, echoVolumeFadeAddRight);
            echoVolumeFadeTicks--;
            if (echoVolumeFadeTicks == 0)
            {
                echoVolumeLeft = SpcFixedPoint.FromWhole(echoVolumeFadeTargetLeft);
                echoVolumeRight = SpcFixedPoint.FromWhole(echoVolumeFadeTargetRight);
            }
        }
        if (masterVolumeFadeTicks != 0)
        {
            masterVolumeFadeTicks--;
            masterVolume = masterVolumeFadeTicks == 0
                ? SpcFixedPoint.FromWhole(masterVolumeFadeTarget)
                : AddWord(masterVolume, masterVolumeFadeAdd);
            volumeDirty = byte.MaxValue;
        }
        for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
        {
            currentChannelBit = unchecked((byte)bit);
            if (SnesLittleEndian.HighByte(channels[index].PatternOrderPointer) != 0)
                HandleChannelTick(channels[index]);
        }
        currentChannelBit = 0;
    }

    /// <summary>Resolves a numbered track's top-level pointer and resets its music-channel state.</summary>
    private void StartTrack(byte track)
    {
        portsToSnes[(byte)ApuPort.Music] = track;
        // Track zero deliberately indexes the word immediately before the numbered table.
        musicTopLevelPointer = ReadPointerEntry(SpcDriverData.Ram.DefaultMusicPointer, track);
        counter = SpcDriverData.Music.TrackStartupTicks;
        keyOff |= unchecked((byte)~channelOnMask);
    }

    /// <summary>Applies note or percussion pitch, volume and key-on state to the selected music voice.</summary>
    private void PlayNote(ManagedSpcMusicChannel channel, byte note)
    {
        if (note >= SpcDriverData.Music.FirstPercussionNote)
        {
            SetInstrument(channel, note);
            note = SpcDriverData.Music.PercussionPlaybackNote;
        }
        if (note >= SpcDriverData.Music.TieNote || (channelOnMask & currentChannelBit) != 0)
            return;

        channel.Pitch = SpcFixedPoint.Compose((note & SpcDriverData.Music.NoteNumberMask) + globalTransposition +
            channel.ChannelTransposition, channel.FineTune);
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
                channel.Pitch = AddWord(channel.Pitch, -SpcFixedPoint.FromWhole(channel.PitchEnvelopeSlide));
            ComputePitchAdd(channel, unchecked((byte)(SpcFixedPoint.Whole(channel.Pitch) + channel.PitchEnvelopeSlide)));
        }
        WritePitch(channel, channel.Pitch);
    }

    /// <summary>The next byte of a music sequence stream at <paramref name="cursor"/>, advancing it with 16-bit wrap.</summary>
    [SpcMemoryBoundary("driver-memory view: music sequence stream bytes")]
    private byte NextSequenceByte(ref ushort cursor) => ram[cursor++];

    /// <summary>The music sequence byte at <paramref name="cursor"/>, without advancing.</summary>
    [SpcMemoryBoundary("driver-memory view: music sequence stream bytes")]
    private byte PeekSequenceByte(ushort cursor) => ram[cursor];

    /// <summary>The next byte of a sound-effect stream: the channel's stream pointer plus its index, wrapping at 16 bits.</summary>
    [SpcMemoryBoundary("driver-memory view: sound-effect instruction stream bytes")]
    private byte ReadSoundByte(ManagedSpcSoundChannel channel)
    {
        byte value = ram[unchecked((ushort)(channel.Pointer + channel.PointerIndex))];
        channel.PointerIndex++;
        return value;
    }

    /// <summary>The little-endian pointer word at <paramref name="address"/>; the high byte wraps at 16 bits.</summary>
    [SpcMemoryBoundary("driver-memory view: little-endian pointer words")]
    private ushort ReadWord(int address) => SpcRamWords.Read(ram, address);

    /// <summary>Entry <paramref name="index"/> of the pointer table at <paramref name="table"/> (two bytes per entry).</summary>
    [SpcMemoryBoundary("driver-memory view: pointer tables")]
    private ushort ReadPointerEntry(int table, int index) => ReadWord(table + index * 2);

    /// <summary>The pan-volume table pair (this entry and the next) at <paramref name="address"/>.</summary>
    [SpcMemoryBoundary("driver-memory view: pan volume table")]
    private (byte Base, byte Next) ReadPanVolumePair(int address) => (ram[address], ram[address + 1]);

    /// <summary>Byte <paramref name="index"/> of the echo FIR coefficient preset table.</summary>
    [SpcMemoryBoundary("driver-memory view: echo FIR preset table")]
    private sbyte ReadFirCoefficient(int index) => unchecked((sbyte)ram[SpcDriverData.Echo.FirCoefficientTableAddress + index]);

    /// <summary>The instrument record at <paramref name="address"/> (pitch base big-endian).</summary>
    [SpcMemoryBoundary("driver-memory view: instrument records")]
    private SpcInstrumentRecord ReadInstrument(int address) => new(
        ram[address], ram[address + 1], ram[address + 2], ram[address + 3],
        unchecked((ushort)(SnesLittleEndian.Word(ram[address + 5], ram[address + 4]))));

    /// <summary>Stores an instrument record at <paramref name="address"/> in the native byte order.</summary>
    [SpcMemoryBoundary("driver-memory view: instrument records")]
    private void WriteInstrument(int address, SpcInstrumentRecord record)
    {
        ram[address] = record.SourceOrNoiseRate;
        ram[address + 1] = record.Adsr1;
        ram[address + 2] = record.Adsr2;
        ram[address + 3] = record.Gain;
        ram[address + 4] = SnesLittleEndian.HighByte(record.PitchBase);
        ram[address + 5] = unchecked((byte)record.PitchBase);
    }

    /// <summary>Copies <paramref name="bytes"/> to APU RAM at <paramref name="address"/>, wrapping at 16 bits.</summary>
    [SpcMemoryBoundary("codec: upload and compiled-definition bytes")]
    private void WriteBytes(int address, ReadOnlySpan<byte> bytes)
    {
        for (int index = 0; index < bytes.Length; index++)
            ram[unchecked((ushort)(address + index))] = bytes[index];
    }

    /// <summary>Stores one compiled-definition byte at <paramref name="address"/>.</summary>
    [SpcMemoryBoundary("codec: compiled-definition bytes")]
    private void WriteByte(int address, byte value) => ram[address] = value;

    /// <summary>Zeroes the driver work regions the native reset clears.</summary>
    [SpcMemoryBoundary("driver-memory view: reset work regions")]
    private void ClearDriverWorkRegions()
    {
        Array.Clear(ram, SpcDriverData.Ram.FirstWorkRegion, SpcDriverData.Ram.FirstWorkRegionLength);
        Array.Clear(ram, SpcDriverData.Ram.SmallWorkRegion, SpcDriverData.Ram.SmallWorkRegionLength);
        Array.Clear(ram, SpcDriverData.Ram.SoundPointerRegion, SpcDriverData.Ram.SoundPointerRegionLength);
        Array.Clear(ram, SpcDriverData.Ram.SoundStateRegion, SpcDriverData.Ram.SoundStateRegionLength);
        Array.Clear(ram, SpcDriverData.Ram.SecondarySoundStateRegion,
            SpcDriverData.Ram.SecondarySoundStateRegionLength);
    }

    /// <summary>The DSP reads BRR samples and the echo buffer straight from APU RAM.</summary>
    [SpcMemoryBoundary("implementation: the DSP shares the APU RAM it plays from")]
    private ManagedSnesDsp CreateDsp() => new(ram);

    /// <summary>
    /// One sound-effect service of library <paramref name="libraryIndex"/>: Handle CPU IO
    /// ($1EE7/$3157/$4706) followed by Write/read CPU IO ($1621).
    /// </summary>
    /// <remarks>
    /// The handler acts on the value latched by the previous service and starts a sound only
    /// when that value changed. $1621 then echoes the acted-on value and latches the CPU's
    /// current write, so a request is echoed one service after the service that reads it.
    /// </remarks>
    private void HandleSoundLibraryCommand(int libraryIndex)
    {
        int port = libraryIndex + (byte)ApuPort.SoundLibrary1;
        byte previous = previousSoundCommandReads[libraryIndex];
        byte command = soundCommandReads[libraryIndex];
        previousSoundCommandReads[libraryIndex] = command;
        ApplySoundLibraryCommand(libraryIndex, changed: command != previous, command);
        portsToSnes[port] = command;
        soundCommandReads[libraryIndex] = inputPorts[port];
    }

    /// <summary>Applies native library priority/cancellation gates, then selects a new sound stream or advances the active one.</summary>
    private void ApplySoundLibraryCommand(int libraryIndex, bool changed, byte command)
    {
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];
        bool reject = !changed || command == 0 || libraryIndex switch
        {
            0 => command != 1 && command != 2 && library.Priority != 0,
            1 => command != 0x71 && command != 0x7e && library.Priority != 0, // allow(HardwareValue): retail SFX2 cancellation overrides
            2 => command != 1 && (library.Mode == 2 || command != 2 && library.Priority != 0),
            _ => throw new ArgumentOutOfRangeException(nameof(libraryIndex)),
        };
        if (reject)
        {
            if (library.CurrentSound != 0)
                ProcessSoundLibrary(libraryIndex);
            return;
        }

        ManagedSpcSoundChannel[] libraryChannels = soundChannels[libraryIndex];
        if (library.CurrentSound != 0)
        {
            library.EnabledVoices = 0;
            foreach (ManagedSpcSoundChannel channel in libraryChannels)
                ResetSoundChannel(library, channel);
        }
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
            channel.Legato = 0;

        int tableIndex = command - 1;
        if ((uint)tableIndex >= SpcSoundEffectTables.CommandCount(libraryIndex))
        {
            throw new InvalidDataException(
                $"SPC sound library {libraryIndex + 1} has no command ${command:X2}.");
        }
        library.CurrentSoundIndex = unchecked((byte)(tableIndex * 2));
        library.CurrentPointer = SpcSoundEffectTables.StreamPointer(libraryIndex, command);
        library.CurrentSound = command;

        byte configuration = SpcSoundEffectTables.Configuration(libraryIndex, command);
        ConfigureSoundLibrary(libraryIndex, library, configuration);
    }

    /// <summary>Applies native allocation-policy writes for a validated sound library.</summary>
    /// <remarks>#1165 original-handler proof covers all policy cases. Mode is unchanged
    /// except for library-three cancel/low-health operations; low-health preserves priority.
    /// Native handler identities are documented on the SpcLibraryNPolicy catalog members.</remarks>
    private static void ConfigureSoundLibrary(
        int libraryIndex,
        ManagedSpcSoundLibrary library,
        byte configuration)
    {
        if (libraryIndex == 0)
        {
            (byte voices, byte priority) = (SpcLibrary1Policy)configuration switch
            {
                SpcLibrary1Policy.OneVoiceLowPriority => ((byte)1, (byte)0),
                SpcLibrary1Policy.OneVoiceHighPriority => ((byte)1, (byte)1),
                SpcLibrary1Policy.TwoVoicesLowPriority => ((byte)2, (byte)0),
                SpcLibrary1Policy.ThreeVoicesHighPriority => ((byte)3, (byte)1),
                SpcLibrary1Policy.FourVoicesLowPriority or SpcLibrary1Policy.PowerBombFourVoices => ((byte)4, (byte)0),
                _ => throw new InvalidDataException($"Unknown SPC SFX1 configuration {configuration}."),
            };
            library.VoicesToSetup = voices;
            library.Priority = priority;
            return;
        }
        if (libraryIndex == 1)
        {
            (byte voices, byte priority) = (SpcLibrary2Policy)configuration switch
            {
                SpcLibrary2Policy.OneVoiceLowPriority => ((byte)1, (byte)0),
                SpcLibrary2Policy.OneVoiceHighPriority => ((byte)1, (byte)1),
                SpcLibrary2Policy.TwoVoicesLowPriority => ((byte)2, (byte)0),
                SpcLibrary2Policy.TwoVoicesHighPriority => ((byte)2, (byte)1),
                _ => throw new InvalidDataException($"Unknown SPC SFX2 configuration {configuration}."),
            };
            library.VoicesToSetup = voices;
            library.Priority = priority;
            return;
        }

        switch ((SpcLibrary3Policy)configuration)
        {
            case SpcLibrary3Policy.CancelAndClearLowHealthMode:
                library.VoicesToSetup = 1;
                library.Mode = 0;
                library.Priority = 1;
                break;
            case SpcLibrary3Policy.LowHealthModePreservePriority:
                library.VoicesToSetup = 1;
                library.Mode = 2;
                break;
            case SpcLibrary3Policy.OneVoiceLowPriority:
                library.VoicesToSetup = 1;
                library.Priority = 0;
                break;
            case SpcLibrary3Policy.TwoVoicesLowPriority:
                library.VoicesToSetup = 2;
                library.Priority = 0;
                break;
            case SpcLibrary3Policy.TwoVoicesHighPriority:
                library.VoicesToSetup = 2;
                library.Priority = 1;
                break;
            case SpcLibrary3Policy.OneVoiceHighPriority:
                library.VoicesToSetup = 1;
                library.Priority = 1;
                break;
            default:
                throw new InvalidDataException($"Unknown SPC SFX3 configuration {configuration}.");
        }
    }

    /// <summary>Allocates borrowed voices on first service and then ticks each sound-effect stream.</summary>
    private void ProcessSoundLibrary(int libraryIndex)
    {
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];
        ManagedSpcSoundChannel[] libraryChannels = soundChannels[libraryIndex];
        if (library.InitializationFlag != SpcDriverData.SoundEffects.Active)
        {
            InitializeSoundLibrary(libraryIndex, library, libraryChannels);
            for (int index = 0; index < libraryChannels.Length; index++)
            {
                ManagedSpcSoundChannel channel = libraryChannels[index];
                channel.Pointer = ReadPointerEntry(library.CurrentPointer, index);
                channel.VoiceIndexTimesEight = unchecked((byte)(channel.VoiceIndex * 8));
                channel.PointerIndex = 0;
                channel.InstructionTimer = 1;
            }
        }
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
            RunSoundChannel(library, channel);
    }

    /// <summary>Scans hardware voices high to low and captures music state before assigning them to a sound library.</summary>
    private void InitializeSoundLibrary(
        int libraryIndex,
        ManagedSpcSoundLibrary library,
        ManagedSpcSoundChannel[] libraryChannels)
    {
        library.VoiceId = SpcDriverData.SoundEffects.VoiceSearchStart;
        library.VoicesRemaining = channelOnMask;
        library.InitializationFlag = SpcDriverData.SoundEffects.Active;
        library.ChannelIndex = library.ChannelIndexTimesTwo = 0;
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
        {
            channel.VoiceBitset = 0;
            channel.VoiceIndex = 0;
            channel.ChannelMask = byte.MaxValue;
            channel.Disabled = SpcDriverData.SoundEffects.Active;
        }

        while (--library.VoiceId != 0)
        {
            int occupied = library.VoicesRemaining;
            library.VoicesRemaining <<= 1;
            if ((occupied & 0x80) != 0) // allow(BitMask): high-to-low hardware voice scan
                continue;
            if (library.VoicesToSetup == 0)
                return;

            library.VoicesToSetup--;
            ManagedSpcSoundChannel soundChannel = libraryChannels[library.ChannelIndex];
            soundChannel.Disabled = 0;
            library.ChannelVoiceBitsetPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddress(libraryIndex, SpcAllocationField.VoiceBitset) + library.ChannelIndex));
            library.ChannelVoiceMaskPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddress(libraryIndex, SpcAllocationField.ChannelMask) + library.ChannelIndex));
            library.ChannelVoiceIndexPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddress(libraryIndex, SpcAllocationField.VoiceIndex) + library.ChannelIndex));
            library.ChannelIndex++;
            library.ChannelIndexTimesTwo += 2;

            library.VoiceIndex = unchecked((byte)((library.VoiceId - 1) * 2));
            soundChannel.VoiceIndex = library.VoiceIndex;
            int musicChannelIndex = library.VoiceId - 1;
            // The native field is one byte; the apparent phase byte in its source expression
            // is intentionally split into the adjacent managed field. Both halves must be
            // captured before the SFX overwrites the shared music-channel registers: the
            // cancellation stream later restores them as one packed native word.
            ManagedSpcSoundOwnership.CaptureBorrowedMusicState(
                channels[musicChannelIndex],
                soundChannel);
            byte voiceBit = unchecked((byte)(1 << musicChannelIndex));
            channelOnMask |= voiceBit;
            currentChannelBit &= unchecked((byte)~voiceBit);
            echoEnable &= unchecked((byte)~voiceBit);
            library.EnabledVoices |= voiceBit;
            soundChannel.VoiceBitset = voiceBit;
            soundChannel.ChannelMask = unchecked((byte)~voiceBit);
        }
    }

    /// <summary>Releases a borrowed DSP voice and restores its music instrument, pan and volume ownership.</summary>
    private void ResetSoundChannel(ManagedSpcSoundLibrary library, ManagedSpcSoundChannel soundChannel)
    {
        soundChannel.Disabled = SpcDriverData.SoundEffects.Active;
        soundChannel.UpdateAdsr = 0;
        library.EnabledVoices &= soundChannel.ChannelMask;
        channelOnMask &= soundChannel.ChannelMask;
        currentChannelBit |= soundChannel.VoiceBitset;
        keyOff |= soundChannel.VoiceBitset;
        ManagedSpcMusicChannel musicChannel = channels[soundChannel.VoiceIndex >> 1];
        SetInstrumentWithoutSavingId(musicChannel, musicChannel.InstrumentId);
        musicChannel.FinalVolume = soundChannel.Volume;
        musicChannel.PanFlags = soundChannel.PhaseInvert;
        if (library.EnabledVoices == 0)
        {
            library.CurrentSound = 0;
            library.Priority = 0;
            library.InitializationFlag = 0;
        }
    }


    /// <summary>Interprets one sound stream's timed note, repeat, noise, ADSR and pitch-slide commands.</summary>
    private void RunSoundChannel(ManagedSpcSoundLibrary library, ManagedSpcSoundChannel soundChannel)
    {
        if (soundChannel.Disabled == SpcDriverData.SoundEffects.Active)
            return;
        soundChannel.InstructionTimer--;
        if (soundChannel.InstructionTimer == 0)
        {
            if (soundChannel.Legato == 0)
            {
                soundChannel.EnablePitchSlide = 0;
                soundChannel.SubnoteDelta = 0;
                soundChannel.TargetNote = 0;
                if (soundChannel.ReleaseFlag != SpcDriverData.SoundEffects.Active)
                {
                    keyOff |= soundChannel.VoiceBitset;
                    soundChannel.ReleaseTimer = 2;
                    soundChannel.InstructionTimer = 1;
                    soundChannel.ReleaseFlag = SpcDriverData.SoundEffects.Active;
                }
                soundChannel.ReleaseTimer--;
                if (soundChannel.ReleaseTimer != 0)
                    return;
                soundChannel.ReleaseFlag = 0;
                currentChannelBit &= soundChannel.ChannelMask;
                noiseEnable &= soundChannel.ChannelMask;
            }

            byte command;
        NextCommand:
            command = ReadSoundByte(soundChannel);
            if (command == 0xf9) // allow(DomainDiscriminator): SFX inline ADSR override
            {
                soundChannel.AdsrSettings = ReadSoundByte(soundChannel);
                soundChannel.AdsrSettings |= SnesLittleEndian.Word(low: 0, high: ReadSoundByte(soundChannel));
                soundChannel.PointerIndex += 2;
                soundChannel.UpdateAdsr = SpcDriverData.SoundEffects.Active;
                goto NextCommand;
            }
            if (command is 0xf5 or 0xf8) // allow(DomainDiscriminator): SFX pitch-slide forms
            {
                soundChannel.EnablePitchSlideLegato = command == 0xf5 // allow(DomainDiscriminator): legato slide
                    ? (byte)0xf5 // allow(DomainDiscriminator): retained cartridge marker
                    : (byte)0;
                soundChannel.SubnoteDelta = ReadSoundByte(soundChannel);
                soundChannel.TargetNote = ReadSoundByte(soundChannel);
                soundChannel.EnablePitchSlide = SpcDriverData.SoundEffects.Active;
                command = ReadSoundByte(soundChannel);
            }
            if (command == byte.MaxValue)
            {
                ResetSoundChannel(library, soundChannel);
                return;
            }
            if (command == 0xfe) // allow(DomainDiscriminator): SFX repeat-start opcode
            {
                soundChannel.RepeatCounter = ReadSoundByte(soundChannel);
                soundChannel.RepeatPoint = soundChannel.PointerIndex;
                command = ReadSoundByte(soundChannel);
            }
            if (command is 0xfb or 0xfd) // allow(DomainDiscriminator): SFX repeat opcodes
            {
                if (command == 0xfd) // allow(DomainDiscriminator): counted repeat opcode
                {
                    soundChannel.RepeatCounter--;
                    if (soundChannel.RepeatCounter == 0)
                        goto NextCommand;
                }
                soundChannel.PointerIndex = soundChannel.RepeatPoint;
                command = ReadSoundByte(soundChannel);
            }
            if (command == 0xfc) // allow(DomainDiscriminator): SFX noise-enable opcode
            {
                noiseEnable |= soundChannel.VoiceBitset;
                goto NextCommand;
            }

            ManagedSpcMusicChannel musicChannel = channels[soundChannel.VoiceIndex >> 1];
            SetInstrumentWithoutSavingId(musicChannel, command);
            musicChannel.FinalVolume = ReadSoundByte(soundChannel);
            musicChannel.PanFlags = 0;
            WriteVolume(musicChannel, SpcFixedPoint.FromWhole(ReadSoundByte(soundChannel)));
            command = ReadSoundByte(soundChannel);
            if (command != 0xf6) // allow(DomainDiscriminator): retain prior SFX note opcode
            {
                soundChannel.Note = command;
                soundChannel.Subnote = 0;
            }
            WritePitchInner(musicChannel, unchecked((ushort)(SnesLittleEndian.Word(soundChannel.Subnote, soundChannel.Note))));
            soundChannel.InstructionTimer = ReadSoundByte(soundChannel);
            if (soundChannel.UpdateAdsr != 0)
            {
                WriteSoundEffectAdsr(soundChannel.VoiceIndexTimesEight, soundChannel.AdsrSettings);
            }
            if (soundChannel.Legato == 0)
                keyOn |= soundChannel.VoiceBitset;
        }

        if (soundChannel.EnablePitchSlide != SpcDriverData.SoundEffects.Active)
            return;
        if (soundChannel.EnablePitchSlideLegato != 0)
            soundChannel.Legato = SpcDriverData.SoundEffects.Active;

        if (soundChannel.Note >= soundChannel.TargetNote)
        {
            int next = soundChannel.Subnote - soundChannel.SubnoteDelta;
            soundChannel.Subnote = unchecked((byte)next);
            if ((next & 0x100) != 0) // allow(BitMask): subnote borrow
            {
                soundChannel.Note--;
                if (soundChannel.Note == soundChannel.TargetNote)
                    soundChannel.EnablePitchSlide = soundChannel.Legato = 0;
            }
        }
        else
        {
            int next = soundChannel.Subnote + soundChannel.SubnoteDelta;
            soundChannel.Subnote = unchecked((byte)next);
            if ((next & 0x100) != 0) // allow(BitMask): subnote carry
            {
                soundChannel.Note++;
                if (soundChannel.Note == soundChannel.TargetNote)
                    soundChannel.EnablePitchSlide = soundChannel.Legato = 0;
            }
        }
        WritePitchInner(channels[soundChannel.VoiceIndex >> 1], unchecked((ushort)(
            SnesLittleEndian.Word(soundChannel.Subnote, soundChannel.Note))));
    }

    /// <summary>Owned 64-KiB APU RAM shared by the driver views and DSP sample decoder.</summary>
    [SpcMemoryAccess]
    private readonly byte[] ram = new byte[SpcDriverData.ApuRamSize];
    /// <summary>SPC acknowledgements echoed back to the four CPU communication ports.</summary>
    private readonly byte[] portsToSnes = new byte[AudioRomData.Apu.PortCount];
    /// <summary>Latest CPU writes, latched for the next music or sound service.</summary>
    private readonly byte[] inputPorts = new byte[AudioRomData.Apu.PortCount];
    /// <summary>SPC $01-$03: each SFX port value latched by the previous $1621 service.</summary>
    private readonly byte[] soundCommandReads = new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>SPC $09-$0B: values acted on at the previous service for change detection.</summary>
    private readonly byte[] previousSoundCommandReads = new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>Eight music sequencer states that continue advancing under borrowed DSP voices.</summary>
    private readonly ManagedSpcMusicChannel[] channels = Enumerable
        .Range(0, SpcDriverData.ChannelCount)
        .Select(index => new ManagedSpcMusicChannel { Index = unchecked((byte)index) })
        .ToArray();
    /// <summary>Priority, mode and voice-allocation state for the three SFX libraries.</summary>
    private readonly ManagedSpcSoundLibrary[] soundLibraries = Enumerable
        .Range(0, SpcDriverData.SoundEffects.LibraryCount)
        .Select(_ => new ManagedSpcSoundLibrary())
        .ToArray();
    /// <summary>Per-library SFX instruction cursors and borrowed voice state.</summary>
    private readonly ManagedSpcSoundChannel[][] soundChannels =
    [
        Enumerable.Range(0, SpcDriverData.SoundEffects.LibraryOneChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
        Enumerable.Range(0, SpcDriverData.SoundEffects.OtherLibraryChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
        Enumerable.Range(0, SpcDriverData.SoundEffects.OtherLibraryChannelCount)
            .Select(_ => new ManagedSpcSoundChannel()).ToArray(),
    ];

    /// <summary>DSP renderer reading BRR and echo data from this driver's APU RAM.</summary>
    private readonly ManagedSnesDsp dsp;
    /// <summary>DSP cycles accumulated toward the next driver service tick.</summary>
    private byte timerCycles;
    /// <summary>Music block countdown before selecting the next top-level phrase.</summary>
    private ushort counter;
    /// <summary>Dirty marker shared by pan/tremolo and pitch/vibrato publication.</summary>
    private byte affectedVolumeOrPitch;
    /// <summary>Hardware voice bits currently borrowed by SFX libraries.</summary>
    private byte channelOnMask;
    /// <summary>Music sequence fast-forward flag written by its control effect.</summary>
    private byte fastForward;
    /// <summary>Current track's top-level phrase-list address.</summary>
    private ushort musicTopLevelPointer;
    /// <summary>Remaining repetitions of the current top-level music block.</summary>
    private byte blockCount;
    /// <summary>Carry accumulator that schedules services for all three SFX libraries.</summary>
    private byte soundEffectTimerAccumulator;
    /// <summary>Pending DSP voice key-on bits published by the next loop-part-one pass.</summary>
    private byte keyOn;
    /// <summary>Pending DSP voice key-off bits published before key-on.</summary>
    private byte keyOff;
    /// <summary>One-hot music voice bit selected during descending channel traversal.</summary>
    private byte currentChannelBit;
    /// <summary>Current DSP flags, including noise rate and echo-write disable.</summary>
    private byte dspFlags;
    /// <summary>Voice mask whose DSP noise generator is enabled.</summary>
    private byte noiseEnable;
    /// <summary>Voice mask admitted to the DSP echo path after settling.</summary>
    private byte echoEnable;
    /// <summary>Signed countdown while a changed echo delay settles.</summary>
    private byte echoStoredTime;
    /// <summary>Requested DSP echo delay byte.</summary>
    private byte echoDelay;
    /// <summary>Signed DSP echo feedback written after the delay settles.</summary>
    private byte echoFeedback;
    /// <summary>Semitone offset applied to note and slide targets across the track.</summary>
    private byte globalTransposition;
    /// <summary>Carry accumulator scheduling full music ticks and intermediate sweeps.</summary>
    private byte mainTempoAccumulator;
    /// <summary>Current fixed-point music tempo selected by track or fade effects.</summary>
    private ushort tempo;
    /// <summary>Remaining full ticks in a tempo fade.</summary>
    private byte tempoFadeTicks;
    /// <summary>Whole-byte tempo endpoint for an active fade.</summary>
    private byte tempoFadeTarget;
    /// <summary>Signed fixed-point increment applied per tempo-fade tick.</summary>
    private ushort tempoFadeAdd;
    /// <summary>Global fixed-point music volume before per-channel scaling.</summary>
    private ushort masterVolume;
    /// <summary>Remaining full ticks in a master-volume fade.</summary>
    private byte masterVolumeFadeTicks;
    /// <summary>Whole-byte endpoint for a master-volume fade.</summary>
    private byte masterVolumeFadeTarget;
    /// <summary>Signed fixed-point master-volume increment per tick.</summary>
    private ushort masterVolumeFadeAdd;
    /// <summary>Music voices whose volume registers need a fresh DSP write.</summary>
    private byte volumeDirty;
    /// <summary>Base instrument ID for the track's percussion-note mapping.</summary>
    private byte percussionBaseId;
    /// <summary>Fixed-point left echo volume before DSP publication.</summary>
    private ushort echoVolumeLeft;
    /// <summary>Fixed-point right echo volume before DSP publication.</summary>
    private ushort echoVolumeRight;
    /// <summary>Left echo-volume increment per fade tick.</summary>
    private ushort echoVolumeFadeAddLeft;
    /// <summary>Right echo-volume increment per fade tick.</summary>
    private ushort echoVolumeFadeAddRight;
    /// <summary>Remaining ticks of a stereo echo-volume fade.</summary>
    private byte echoVolumeFadeTicks;
    /// <summary>Left echo-volume target when its fade ends.</summary>
    private byte echoVolumeFadeTargetLeft;
    /// <summary>Right echo-volume target when its fade ends.</summary>
    private byte echoVolumeFadeTargetRight;
    /// <summary>Last DSP delay value, used to detect a requested echo-buffer reconfiguration.</summary>
    private byte lastWrittenEchoDelay;

    /// <summary>Creates an independent SPC sequencer and DSP over owned 64-KiB APU RAM, applying native driver reset state without loading an audio bank or opening a host audio device.</summary>
    /// <remarks>Uploads and instrument, music, sound-effect, and decoded sample definitions are installed separately by the audio renderer before playback.</remarks>
    public ManagedSpcPlayer()
    {
        dsp = CreateDsp();
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
        Ensure.NotNull(instruments);
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
            WriteInstrument(address, new SpcInstrumentRecord(instrument.SourceOrNoiseRate, instrument.Adsr1,
                instrument.Adsr2, instrument.Gain, instrument.PitchBase));
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
        Ensure.NotNull(programs);
        Ensure.NotNull(libraries);
        Dictionary<string, AudioSoundProgramMetadata> byId = new(StringComparer.Ordinal);
        foreach (AudioSoundProgramMetadata program in programs)
        {
            if (!byId.TryAdd(program.Id, program))
                throw new InvalidDataException($"Decoded SFX programs repeat stable ID '{program.Id}'.");
            byte[] encoded = SpcSoundEffectProgramCodec.Encode(program);
            WriteBytes(program.Address, encoded);
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
                    ushort uploadedPointer = ReadPointerEntry(effect.StreamPointer, channel);
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
        Ensure.NotNull(bank);
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
            WriteByte(address, value);
    }

    /// <summary>Writes one CPU-to-SPC communication port exactly as the cartridge does.</summary>
    public void WritePort(ApuPort port, byte value)
    {
        if ((uint)port >= inputPorts.Length)
            throw new ArgumentOutOfRangeException(nameof(port), port, "SPC input port must be zero through three.");
        inputPorts[(int)port] = value;
    }

    /// <summary>Reads one SPC-to-CPU acknowledgement port.</summary>
    public byte ReadPort(ApuPort port)
    {
        if ((uint)port >= portsToSnes.Length)
            throw new ArgumentOutOfRangeException(nameof(port), port, "SPC output port must be zero through three.");
        return portsToSnes[(int)port];
    }

    /// <summary>
    /// Applies a complete cartridge upload stream to APU RAM. Each record contains a little-
    /// endian length and destination followed by payload; a zero length terminates the stream.
    /// </summary>
    public void Upload(ReadOnlySpan<byte> stream)
    {
        Dsp.Global.WriteEchoVolume(DspStereoSide.Left, 0);
        Dsp.Global.WriteEchoVolume(DspStereoSide.Right, 0);
        Dsp.Global.KeyOff(DspVoiceMask.All);

        int offset = 0;
        for (;;)
        {
            if (offset > stream.Length - 2)
                throw new InvalidDataException("SPC upload stream ended before its record length.");
            ushort length = unchecked((ushort)(SnesLittleEndian.Word(stream[offset], stream[offset + 1])));
            offset += 2;
            if (length == 0)
                break;
            if (offset > stream.Length - 2)
                throw new InvalidDataException("SPC upload stream ended before its destination word.");
            ushort target = unchecked((ushort)(SnesLittleEndian.Word(stream[offset], stream[offset + 1])));
            offset += 2;
            if (length > stream.Length - offset)
                throw new InvalidDataException(
                    $"SPC upload record for ${target:X4} declares {length} bytes but only {stream.Length - offset} remain.");
            WriteBytes(target, stream.Slice(offset, length));
            offset += length;
        }

        portsToSnes[(byte)ApuPort.Music] = 0;
        inputPorts[(byte)ApuPort.Music] = (byte)MusicPortCommand.None;
        // $1E8B leaves $BB on output port 1 and finishes with $F1 = $31, which resets the
        // CPU-to-APU input latches. The driver's $01-$0B words are untouched.
        portsToSnes[(byte)ApuPort.SoundLibrary1] = SpcDriverData.UploadReadyLibraryOnePort;
        for (int port = (byte)ApuPort.SoundLibrary1; port < AudioRomData.Apu.PortCount; port++)
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

    /// <summary>Resets DSP/work RAM and publishes the driver's initial echo and master-register state.</summary>
    private void InitializeDriver()
    {
        dsp.Reset();
        ClearDriverWorkRegions();

        for (int index = 0; index < channels.Length; index++)
            channels[index].Index = unchecked((byte)index);
        SetupEchoDelay(SpcDriverData.Echo.InitialDelay);
        dspFlags |= SpcDriverData.Echo.WriteDisable;
        Dsp.Global.WriteMasterVolume(DspStereoSide.Left, 0x60); // allow(HardwareMagnitude): cartridge reset master volume
        Dsp.Global.WriteMasterVolume(DspStereoSide.Right, 0x60); // allow(HardwareMagnitude): cartridge reset master volume
        Dsp.Global.WriteSourceDirectory(0x6d); // allow(HardwareAddress): cartridge BRR directory page
        tempo = SpcFixedPoint.WithWhole(tempo, SpcDriverData.Music.InitialTempo);
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
        Dsp.Global.KeyOff(new DspVoiceMask(keyOff));
        Dsp.Global.WritePitchModulation(DspVoiceMask.None);
        Dsp.Global.WriteNoiseEnable(new DspVoiceMask(noiseEnable));
        Dsp.Global.KeyOff(DspVoiceMask.None);
        Dsp.Global.KeyOn(new DspVoiceMask(keyOn));
        if ((echoStoredTime & 0x80) == 0) // allow(BitMask): signed SPC countdown bit
        {
            Dsp.Global.WriteFlags(new DspFlags(dspFlags));
            if (echoStoredTime == echoDelay)
            {
                Dsp.Global.WriteEchoEnable(new DspVoiceMask(echoEnable));
                Dsp.Global.WriteEchoFeedback(unchecked((sbyte)echoFeedback));
                Dsp.Global.WriteEchoVolume(DspStereoSide.Right, unchecked((sbyte)SpcFixedPoint.Whole(echoVolumeRight)));
                Dsp.Global.WriteEchoVolume(DspStereoSide.Left, unchecked((sbyte)SpcFixedPoint.Whole(echoVolumeLeft)));
            }
        }
        keyOff = keyOn = 0;
    }

    /// <summary>Advances sound and music accumulators, servicing latched commands only on their native carry ticks.</summary>
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

        int musicAccumulator = mainTempoAccumulator + unchecked((byte)(ticks * SpcFixedPoint.Whole(tempo)));
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
        else if (portsToSnes[(byte)ApuPort.Music] != 0)
        {
            for (int index = 0, bit = 1; index < channels.Length; index++, bit <<= 1)
            {
                currentChannelBit = unchecked((byte)bit);
                if (SnesLittleEndian.HighByte(channels[index].PatternOrderPointer) != 0)
                    HandlePanAndSweep(channels[index]);
            }
            currentChannelBit = 0;
        }
    }

    /// <summary>Disables echo during delay changes, computes its countdown and selects the native buffer page.</summary>
    private void SetupEchoDelay(byte requestedDelay)
    {
        echoDelay = requestedDelay;
        if (requestedDelay != lastWrittenEchoDelay)
        {
            byte countdown = unchecked((byte)(SnesDspControls.EchoDelayOf(lastWrittenEchoDelay) ^ byte.MaxValue));
            if ((echoStoredTime & 0x80) != 0) // allow(BitMask): signed SPC countdown bit
                countdown = unchecked((byte)(countdown + echoStoredTime));
            echoStoredTime = countdown;

            Dsp.Global.WriteEchoEnable(DspVoiceMask.None);
            Dsp.Global.WriteEchoFeedback(0);
            Dsp.Global.WriteEchoVolume(DspStereoSide.Right, 0);
            Dsp.Global.WriteEchoVolume(DspStereoSide.Left, 0);
            Dsp.Global.WriteFlags(new DspFlags(unchecked((byte)(dspFlags | SpcDriverData.Echo.WriteDisable))));
            lastWrittenEchoDelay = echoDelay;
            Dsp.Global.WriteEchoDelay(echoDelay);
        }

        byte echoPage = unchecked((byte)(((echoDelay * SpcDriverData.Echo.DelayToPages) ^
            byte.MaxValue) + SpcDriverData.Echo.BufferPageBias));
        Dsp.Global.WriteEchoBufferPage(echoPage);
    }

    /// <summary>Typed register writer over the shared managed S-DSP.</summary>
    private SnesDspControls Dsp => new(dsp);

    /// <summary>
    /// The sound-effect ADSR update as the cartridge driver performs it: the register address is
    /// formed from voice × 8 rather than the sixteen-byte voice stride, so for odd voices these two
    /// writes land on another register. The quirk is preserved exactly.
    /// </summary>
    [DspRegisterBoundary("native quirk: sound-effect ADSR writes address voice × 8 + ADSR1/ADSR2, as the cartridge driver does")]
    private void WriteSoundEffectAdsr(byte voiceIndexTimesEight, ushort adsr)
    {
        dsp.WriteRegister(unchecked((byte)(voiceIndexTimesEight + SnesDspRegisterMap.Voice.Adsr1)), unchecked((byte)adsr));
        dsp.WriteRegister(unchecked((byte)(voiceIndexTimesEight + SnesDspRegisterMap.Voice.Adsr2)), SnesLittleEndian.HighByte(adsr));
    }


    /// <summary>Applies signed arithmetic with the SPC driver's 16-bit word wrap.</summary>
    private static ushort AddWord(ushort left, int right) => unchecked((ushort)(left + right));
}
