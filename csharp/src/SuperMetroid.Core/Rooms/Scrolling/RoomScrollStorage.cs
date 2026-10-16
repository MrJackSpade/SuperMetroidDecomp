namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The 50-byte room scroll-zone allocation (cartridge <c>$7E:CD20-$7E:CD51</c>, <c>Scrolls</c>). It is session-scoped
/// and survives room changes: an implicit scroll table writes only the new room's cells, and the scroll routines read
/// the previous room's bytes past them. Each room's <see cref="RoomScrollGrid"/> binds to it.
/// </summary>
public sealed class RoomScrollStorage
{
    /// <summary>The allocation's bytes, one scroll state per 256-pixel screen, row-major.</summary>
    internal readonly byte[] _bytes = new byte[RoomScrollGrid.StorageByteCount];
}
