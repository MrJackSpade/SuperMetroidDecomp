using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>One room-load choice between native FX bytes and the equivalent compiled catalog.</summary>
public readonly struct RoomFxRecordReader(ISnesAddressSpace bus, bool useCompiledRecords)
{
    public ushort Select(ushort fxPointer, ushort doorPointer) =>
        RoomFxRomData.SelectRecord(bus, fxPointer, doorPointer, useCompiledRecords);

    public byte ReadByte(ushort record, int fieldOffset) =>
        RoomFxRomData.ReadRecordByte(bus, record, fieldOffset, useCompiledRecords);

    public ushort ReadWord(ushort record, int fieldOffset) =>
        RoomFxRomData.ReadRecordWord(bus, record, fieldOffset, useCompiledRecords);
}
