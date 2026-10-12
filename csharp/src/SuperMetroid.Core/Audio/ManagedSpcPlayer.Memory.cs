namespace SuperMetroid.Core.Audio;

/// <summary>One six-byte instrument record of the driver's instrument table (#632).</summary>
/// <param name="SourceOrNoiseRate">Sample source number, or the noise rate when <see cref="SpcDriverData.Instruments.NoiseMarker"/> is set.</param>
/// <param name="Adsr1">VxADSR1 value.</param>
/// <param name="Adsr2">VxADSR2 value.</param>
/// <param name="Gain">VxGAIN value.</param>
/// <param name="PitchBase">Pitch multiplier, stored big-endian in the record.</param>
internal readonly record struct SpcInstrumentRecord(byte SourceOrNoiseRate, byte Adsr1, byte Adsr2, byte Gain, ushort PitchBase);

/// <summary>
/// Typed views of the sound driver's APU RAM (#632). The driver's music and sound-effect logic reads
/// sequence streams, pointer tables, instrument records and the pan/FIR tables only through these
/// members; addresses, byte order and 16-bit wrapping are confined here.
/// </summary>
public sealed partial class ManagedSpcPlayer
{
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
    private ushort ReadWord(int address) => unchecked((ushort)(
        ram[address & ushort.MaxValue] | (ram[(address + 1) & ushort.MaxValue] << 8)));

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
        unchecked((ushort)((ram[address + 4] << 8) | ram[address + 5])));

    /// <summary>Stores an instrument record at <paramref name="address"/> in the native byte order.</summary>
    [SpcMemoryBoundary("driver-memory view: instrument records")]
    private void WriteInstrument(int address, SpcInstrumentRecord record)
    {
        ram[address] = record.SourceOrNoiseRate;
        ram[address + 1] = record.Adsr1;
        ram[address + 2] = record.Adsr2;
        ram[address + 3] = record.Gain;
        ram[address + 4] = unchecked((byte)(record.PitchBase >> 8));
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
}
