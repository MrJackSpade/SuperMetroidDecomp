namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The 50-byte room scroll-zone allocation (cartridge <c>$7E:CD20-$7E:CD51</c>, <c>Scrolls</c>). It is session-scoped
/// and survives room changes: an implicit scroll table writes only the new room's cells, and the scroll routines read
/// the previous room's bytes past them. Each room's <see cref="RoomScrollGrid"/> binds to it.
/// </summary>
public sealed class RoomScrollStorage
{
    /// <summary>The allocation's bytes, one scroll state per 256-pixel screen, row-major.</summary>
    private readonly byte[] _bytes = new byte[RoomScrollGrid.StorageByteCount];

    /// <summary>One cell's raw scroll byte, by allocation index.</summary>
    internal byte this[int index]
    {
        get => _bytes[index];
        set => _bytes[index] = value;
    }

    /// <summary>Copies a room's compiled scroll table over the whole allocation.</summary>
    internal void Load(ReadOnlySpan<byte> compiled) => compiled.CopyTo(_bytes);
}
