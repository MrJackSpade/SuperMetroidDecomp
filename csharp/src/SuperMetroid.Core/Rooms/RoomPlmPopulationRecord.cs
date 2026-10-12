using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>One immutable six-byte record decoded from a bank-$8F room population.</summary>
public readonly record struct RoomPlmPopulationRecord(
    int RecordIndex,
    ushort RecordPointer,
    PlmHeaderId HeaderPointer,
    Position16<RoomBlocks> Block,
    ushort RoomArgument);
