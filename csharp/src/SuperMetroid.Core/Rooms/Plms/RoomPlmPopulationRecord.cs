namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One immutable six-byte record decoded from a bank-$8F room population.</summary>
public readonly record struct RoomPlmPopulationRecord(
    int RecordIndex,
    PlmHeaderId HeaderPointer,
    Position16<RoomBlocks> Block,
    ushort RoomArgument);
