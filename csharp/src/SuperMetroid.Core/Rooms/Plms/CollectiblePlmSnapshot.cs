namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Stable debugger view of one occupied permanent-item PLM slot.</summary>
/// <param name="Header">The slot's native bank-$84 PLM header offset, retaining its kind and presentation identity.</param>
/// <param name="BlockIndex">The item owner's foreground block index in room level data.</param>
/// <param name="RoomArgument">The native room-population argument; nonnegative signed values select persistent collected-item bits.</param>
/// <param name="Kind">The permanent item kind decoded from the header's position in its presentation table.</param>
/// <param name="GraphicsSlot">The rotating dynamic-art allocation from zero through three, or -1 for an item using static art.</param>
public readonly record struct CollectiblePlmSnapshot(
    PlmHeaderId Header,
    int BlockIndex,
    int RoomArgument,
    InWorldCollectibleKind Kind,
    int GraphicsSlot);
