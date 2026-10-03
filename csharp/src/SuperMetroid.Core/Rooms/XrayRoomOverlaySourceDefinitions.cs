namespace SuperMetroid.Core.Rooms;

/// <summary>Required room-specific X-ray presentation keys selected by compiled room states.</summary>
public static class XrayRoomOverlaySourceDefinitions
{
    /// <summary>Derives distinct ascending nonzero room overlay IDs on enumeration;
    /// zero selects no room-specific overlay. No persistent key cache is retained.</summary>
    public static IEnumerable<ushort> All => RoomStateDefinitions.All
        .Select(state => state.XrayPointer).Where(pointer => pointer != 0).Distinct().Order();
}
