namespace SuperMetroid.Core.Rooms;

/// <summary>An immutable compiled room door-list and its native BTS-index ordering.</summary>
public sealed class DoorListDefinition
{
    public DoorListDefinition(ushort pointer, ushort[] doorPointers)
    {
        ArgumentNullException.ThrowIfNull(doorPointers);
        Pointer = pointer;
        DoorPointers = doorPointers.ToArray();
    }

    public ushort Pointer { get; }

    public ReadOnlyMemory<ushort> DoorPointers { get; }
}
