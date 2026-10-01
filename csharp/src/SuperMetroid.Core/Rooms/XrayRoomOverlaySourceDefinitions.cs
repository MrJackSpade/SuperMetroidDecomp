namespace SuperMetroid.Core.Rooms;

/// <summary>Required room-specific X-ray presentation keys selected by compiled room states.</summary>
public static class XrayRoomOverlaySourceDefinitions
{
    /// <summary>Distinct nonzero room overlay IDs; zero selects no room-specific overlay.</summary>
    public static IReadOnlyList<ushort> All { get; } = Array.AsReadOnly(RoomStateDefinitions.All
        .Select(state => state.XrayPointer).Where(pointer => pointer != 0).Distinct().Order().ToArray());
}
