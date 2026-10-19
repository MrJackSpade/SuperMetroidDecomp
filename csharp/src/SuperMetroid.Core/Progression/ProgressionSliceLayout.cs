namespace SuperMetroid.Core.Progression;

/// <summary>
/// Byte offsets of the persistent-progress slice of the cartridge's SRAM mirror, from the event bitset at
/// <c>$7E:D820</c> through the saved dispatcher word at <c>$7E:D914</c>, in native order. SpaceTime's malformed palette
/// copy addresses progression by these offsets (#1318, #1330).
/// </summary>
internal static class ProgressionSliceLayout
{
    /// <summary>Offset of the event bitset, the slice's first byte.</summary>
    internal const int EventsOffset = 0;

    /// <summary>Offset of the per-area boss bitset.</summary>
    internal const int BossBitsOffset = EventsOffset + Bank80SystemState.EventByteCount;

    /// <summary>Offset of the room-indexed opened-Chozo-orb bitset.</summary>
    internal const int RoomChozoBitsOffset = BossBitsOffset + Bank80SystemState.AreaCount;

    /// <summary>Offset of the room-indexed collected-item bitset.</summary>
    internal const int CollectedItemBitsOffset = RoomChozoBitsOffset + Bank80SystemState.RoomChozoBitByteCount;

    /// <summary>Offset of the room-indexed opened-door bitset.</summary>
    internal const int OpenedDoorBitsOffset = CollectedItemBitsOffset + Bank80SystemState.ItemBitByteCount;

    /// <summary>
    /// Offset of the padding after the opened-door bits (<c>$7E:D8F0-$D8F7</c>), which no game logic reads; save schema
    /// two zeroes it, and it is not state.
    /// </summary>
    internal const int PaddingOffset = OpenedDoorBitsOffset + Bank80SystemState.DoorBitByteCount;

    /// <summary>Bytes of padding after the opened-door bits.</summary>
    internal const int PaddingByteCount = 8;

    /// <summary>Offset of the per-area used save-station and elevator bitset.</summary>
    internal const int UsedSaveStationsOffset = PaddingOffset + PaddingByteCount;

    /// <summary>Offset of the per-area acquired map-station flags.</summary>
    internal const int MapStationsOffset = UsedSaveStationsOffset + Bank80SystemState.UsedSaveStationByteCount;

    /// <summary>Offset of the saved dispatcher word, the slice's last two bytes, low byte first.</summary>
    internal const int LoadingGameStateOffset = MapStationsOffset + Bank80SystemState.MapStationByteCount;

    /// <summary>Bytes in the slice.</summary>
    internal const int ByteCount = LoadingGameStateOffset + sizeof(ushort);
}
