namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Development-tool bank-$84 PLM instruction lists; never linked by player hosts.
/// The PLM program audit enumerates these lists alongside the shipped catalog by reflection.
/// </summary>
internal static class RoomPlmInstructionListsTooling
{
    /// <summary>
    /// <c>$84:BB34 InstList_PLM_GateThatClosesDuringEscapeAfterMotherBrain_0</c>:
    /// draw the already-closed gate for six frames, then release the resident PLM slot.
    /// </summary>
    [AccessedByReflection]
    public const ushort MotherBrainEscapeRoomGateClosed = 0xbb34;
}
