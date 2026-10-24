using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>What music and sound effects share on the DSP: instruments, pitch and volume writes, the key-on, key-off, noise and echo flags, and memory reads.</summary>
internal sealed class SpcVoices
{
    /// <summary>Owned 64-KiB APU RAM shared by the driver views and DSP sample decoder.</summary>
    [SpcMemoryAccess]
    private readonly byte[] ram;
    /// <summary>DSP renderer reading BRR and echo data from this driver's APU RAM.</summary>
    private readonly ManagedSnesDsp dsp;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    [SpcMemoryBoundary("implementation: shares the APU RAM the player owns")]
    internal SpcVoices(byte[] ram, ManagedSnesDsp dsp)
    {
        this.ram = ram;
        this.dsp = dsp;
    }

    /// <summary>Interpolates the native pan table into stereo DSP volumes unless a sound effect currently borrows the voice.</summary>
    internal void WriteVolume(ManagedSpcMusicChannel channel, int volume)
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
                int address = panIndex + SpcDriverDefinitions.Music.PanVolumeTableAddress;
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
            if (((channel.PanFlags << side) & 0x80) != 0) // cartridge-encoding: allow(BitMask) - phase-inversion bit
                final = unchecked((byte)-final);
            Dsp.Voice(channel.Index).WriteVolume((DspStereoSide)side, unchecked((sbyte)final));
            volume = unchecked((ushort)(SpcDriverDefinitions.Music.FullyLeftPan - volume));
        }
    }

    /// <summary>Converts a fractional note through the twelve-semitone table and instrument pitch base into DSP frequency.</summary>
    internal void WritePitchInner(ManagedSpcMusicChannel channel, int pitch)
    {
        byte note = unchecked((byte)(SpcFixedPoint.Whole(pitch) & SpcDriverDefinitions.Music.NoteNumberMask));
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

    /// <summary>Installs an instrument's source, envelope, gain and pitch base without replacing the channel's remembered ID.</summary>
    internal void SetInstrumentWithoutSavingId(ManagedSpcMusicChannel channel, int instrument)
    {
        if ((instrument & SpcDriverDefinitions.Instruments.PercussionMarker) != 0)
            instrument = unchecked((byte)(instrument + SpcDriverDefinitions.Instruments.PercussionBias + percussionBaseId));
        int address = SpcDriverDefinitions.Ram.InstrumentTable + instrument * SpcDriverDefinitions.Ram.InstrumentRecordSize;
        if ((channelOnMask & currentChannelBit) != 0)
            return;

        SnesDspVoiceControls voice = Dsp.Voice(channel.Index);
        SpcInstrumentRecord record = ReadInstrument(address);
        if ((record.SourceOrNoiseRate & SpcDriverDefinitions.Instruments.NoiseMarker) != 0)
        {
            dspFlags = unchecked((byte)((dspFlags & SpcDriverDefinitions.Echo.WriteDisable) |
                DspFlags.NoiseRateOf(record.SourceOrNoiseRate)));
            noiseEnable = noiseEnable | currentChannelBit;
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

    /// <summary>The little-endian pointer word at <paramref name="address"/>; the high byte wraps at 16 bits.</summary>
    [SpcMemoryBoundary("driver-memory view: little-endian pointer words")]
    internal ushort ReadWord(int address) => SpcRamWords.Read(ram, address);

    /// <summary>Entry <paramref name="index"/> of the pointer table at <paramref name="table"/> (two bytes per entry).</summary>
    [SpcMemoryBoundary("driver-memory view: pointer tables")]
    internal ushort ReadPointerEntry(int table, int index) => ReadWord(table + index * 2);

    /// <summary>The pan-volume table pair (this entry and the next) at <paramref name="address"/>.</summary>
    [SpcMemoryBoundary("driver-memory view: pan volume table")]
    internal PanVolumePair ReadPanVolumePair(int address) => new PanVolumePair(ram[address], ram[address + 1]);

    /// <summary>The instrument record at <paramref name="address"/> (pitch base big-endian).</summary>
    [SpcMemoryBoundary("driver-memory view: instrument records")]
    internal SpcInstrumentRecord ReadInstrument(int address) => new(
        ram[address], ram[address + 1], ram[address + 2], ram[address + 3],
        unchecked(SnesLittleEndian.Word(ram[address + 5], ram[address + 4])));

    /// <summary>Hardware voice bits currently borrowed by SFX libraries.</summary>
    internal int channelOnMask;

    /// <summary>Pending DSP voice key-on bits published by the next loop-part-one pass.</summary>
    internal int keyOn;

    /// <summary>Pending DSP voice key-off bits published before key-on.</summary>
    internal int keyOff;

    /// <summary>One-hot music voice bit selected during descending channel traversal.</summary>
    internal int currentChannelBit;

    /// <summary>Current DSP flags, including noise rate and echo-write disable.</summary>
    internal int dspFlags;

    /// <summary>Voice mask whose DSP noise generator is enabled.</summary>
    internal int noiseEnable;

    /// <summary>Voice mask admitted to the DSP echo path after settling.</summary>
    internal int echoEnable;

    /// <summary>Base instrument ID for the track's percussion-note mapping.</summary>
    internal int percussionBaseId;

    /// <summary>Typed register writer over the shared managed S-DSP.</summary>
    internal SnesDspControls Dsp => new(dsp);
}
