namespace SuperMetroid.Core.Game;

/// <summary>Typed access to the immutable compiled bank-$83 room-FX records.</summary>
public readonly struct RoomFxRecordReader
{
    public ushort Select(ushort fxPointer, ushort doorPointer) =>
        RoomFxRecordDefinitions.Select(fxPointer, doorPointer);

    public byte ReadByte(ushort record, int fieldOffset) =>
        RoomFxRecordDefinitions.Get(record).ReadByte(fieldOffset);

    public ushort ReadWord(ushort record, int fieldOffset) =>
        RoomFxRecordDefinitions.Get(record).ReadWord(fieldOffset);
}
