namespace SuperMetroid.Core.Rooms;

/// <summary>An immutable compiled room door-list and its native BTS-index ordering.</summary>
public sealed class DoorListDefinition
{
    /// <summary>Copies a native door-list identity and its ordered door pointers into a caller-independent definition.</summary>
    /// <param name="pointer">Sixteen-bit door-list address in bank $8F.</param>
    /// <param name="doorPointers">Bank-$83 door-record pointers in the zero-based order selected by door-block BTS values.</param>
    public DoorListDefinition(ushort pointer, ushort[] doorPointers)
    {
        ArgumentNullException.ThrowIfNull(doorPointers);
        Pointer = pointer;
        DoorPointers = doorPointers.ToArray();
    }

    /// <summary>Gets the native bank-$8F address identifying this room's door list.</summary>
    public ushort Pointer { get; }

    /// <summary>Gets the copied bank-$83 door pointers in native BTS-index order.</summary>
    public ReadOnlyMemory<ushort> DoorPointers { get; }
}
