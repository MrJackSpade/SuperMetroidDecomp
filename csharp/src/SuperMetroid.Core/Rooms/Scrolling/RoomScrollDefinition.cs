namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>An immutable application-owned copy of one native 50-byte room scroll allocation.</summary>
public sealed class RoomScrollDefinition
{
    /// <summary>Copies a decoded native scroll allocation while retaining its bank-$8F source identity.</summary>
    /// <param name="pointer">Explicit native scroll-data pointer used as a compiled lookup identity.</param>
    /// <param name="storage">Raw allocation bytes to copy, including bytes beyond the room's logical screen cells; length/state validation belongs to catalog/grid consumers.</param>
    /// <exception cref="ArgumentNullException">The storage array is null.</exception>
    public RoomScrollDefinition(ushort pointer, byte[] storage)
    {
        Ensure.NotNull(storage);
        Pointer = pointer;
        Storage = storage.ToArray();
    }

    /// <summary>Bank-$8F source pointer identifying this explicit allocation, not a runtime cartridge-read capability.</summary>
    public ushort Pointer { get; }

    /// <summary>Read-only view of the owned raw native allocation; logical cells use red/blue/green states, while retained trailing bytes need not be valid states.</summary>
    public ReadOnlyMemory<byte> Storage { get; }
}
