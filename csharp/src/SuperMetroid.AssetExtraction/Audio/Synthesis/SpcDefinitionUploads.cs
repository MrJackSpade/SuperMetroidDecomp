using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>What the extraction uploads into the SPC's memory: instruments, sound effects, music and raw blocks.</summary>
internal sealed class SpcDefinitionUploads
{
    /// <summary>Owned 64-KiB APU RAM shared by the driver views and DSP sample decoder.</summary>
    private readonly byte[] ram;
    /// <summary>The SPC driver's music sequencer: track commands, effects, notes and the pan, sweep, vibrato and tremolo it ticks.</summary>
    private readonly SpcMusicSequencer _music;
    /// <summary>What music and sound effects share on the DSP: instruments, pitch and volume writes, the key-on, key-off, noise and echo flags, and memory reads.</summary>
    private readonly SpcVoices _voices;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal SpcDefinitionUploads(byte[] ram, SpcMusicSequencer music, SpcVoices voices)
    {
        this.ram = ram;
        _music = music;
        _voices = voices;
    }

    /// <summary>Stores an instrument record at <paramref name="address"/> in the native byte order.</summary>
    internal void WriteInstrument(int address, SpcInstrumentRecord record)
    {
        ushort pitchBase = checked((ushort)record.PitchBase);
        ram[address] = checked((byte)record.SourceOrNoiseRate);
        ram[address + 1] = checked((byte)record.Adsr1);
        ram[address + 2] = checked((byte)record.Adsr2);
        ram[address + 3] = checked((byte)record.Gain);
        ram[address + 4] = SnesLittleEndian.HighByte(pitchBase);
        ram[address + 5] = SnesLittleEndian.LowByte(pitchBase);
    }

    /// <summary>Copies <paramref name="bytes"/> to APU RAM at <paramref name="address"/>, wrapping at 16 bits.</summary>
    private void WriteBytes(int address, ReadOnlySpan<byte> bytes)
    {
        for (int index = 0; index < bytes.Length; index++)
            ram[unchecked((ushort)(address + index))] = bytes[index];
    }

    /// <summary>Stores one compiled-definition byte at <paramref name="address"/>.</summary>
    private void WriteByte(int address, byte value) => ram[address] = value;

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
        if (instruments.Count != SpcDriverDefinitions.Ram.InstrumentCount)
        {
            throw new InvalidDataException(
                $"Instrument bank contains {instruments.Count} records; " +
                $"expected {SpcDriverDefinitions.Ram.InstrumentCount}.");
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
                ((instrument.SourceOrNoiseRate & SpcDriverDefinitions.Instruments.NoiseMarker) != 0))
            {
                throw new InvalidDataException(
                    $"Instrument {index} has inconsistent usesNoise and sourceOrNoiseRate fields.");
            }

            int address = SpcDriverDefinitions.Ram.InstrumentTable +
                index * SpcDriverDefinitions.Ram.InstrumentRecordSize;
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
                    ushort uploadedPointer = _voices.ReadPointerEntry(effect.StreamPointer, channel);
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
            int pointerAddress = SpcDriverDefinitions.Ram.DefaultMusicPointer + track * 2;
            ushort uploadedPointer = _voices.ReadWord(pointerAddress);
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

    /// <summary>
    /// Applies a complete cartridge upload stream to APU RAM. Each record contains a little-
    /// endian length and destination followed by payload; a zero length terminates the stream.
    /// </summary>
    public void Upload(ReadOnlySpan<byte> stream)
    {
        _voices.Dsp.Global.WriteEchoVolume(DspStereoSide.Left, 0);
        _voices.Dsp.Global.WriteEchoVolume(DspStereoSide.Right, 0);
        _voices.Dsp.Global.KeyOff(DspVoiceMask.All);

        int offset = 0;
        for (;;)
        {
            if (offset > stream.Length - 2)
                throw new InvalidDataException("SPC upload stream ended before its record length.");
            ushort length = unchecked(SnesLittleEndian.Word(stream[offset], stream[offset + 1]));
            offset += 2;
            if (length == 0)
                break;
            if (offset > stream.Length - 2)
                throw new InvalidDataException("SPC upload stream ended before its destination word.");
            ushort target = unchecked(SnesLittleEndian.Word(stream[offset], stream[offset + 1]));
            offset += 2;
            if (length > stream.Length - offset)
                throw new InvalidDataException(
                    $"SPC upload record for ${target:X4} declares {length} bytes but only {stream.Length - offset} remain.");
            WriteBytes(target, stream.Slice(offset, length));
            offset += length;
        }

        _music.musicPortToSnes = 0;
        // The music latch reads empty after an upload; the port model resets the sound-effect latches.
        _music.musicInputLatch = (byte)MusicPortCommand.None;
        _music.musicTopLevelPointer = _voices.ReadWord(SpcDriverDefinitions.Ram.DefaultMusicPointer);
        _music.counter = SpcDriverDefinitions.Music.TrackStartupTicks;
        _voices.keyOff = _voices.keyOff | unchecked((byte)~_voices.channelOnMask);
    }
}
