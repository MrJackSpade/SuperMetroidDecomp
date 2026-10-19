namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Verification views of <see cref="RoomScrollStorage"/>.</summary>
internal static class RoomScrollStorageAccess
{
    /// <summary>Exposes the allocation's bytes as one array.</summary>
    extension(RoomScrollStorage self)
    {
        /// <summary>A copy of all 50 bytes, read through the allocation's indexer.</summary>
        internal byte[] Bytes => [.. Enumerable.Range(0, RoomScrollGrid.StorageByteCount).Select(index => self[index])];
    }
}
