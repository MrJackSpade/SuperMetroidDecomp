namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One resolved entry of a room's bank-$8F door list: a physical door header, or one of the
/// elevator pseudo-doors (<see cref="DoorHeaderRomData.ElevatorPseudoDoorPointer"/>,
/// <see cref="DoorHeaderRomData.MaridiaTourianElevatorPseudoDoorPointer"/>).
/// </summary>
/// <remarks>
/// Native code tells them apart by bit 15 of the entry's destination word (<c>$94:938B</c>,
/// <c>$94:93CE</c>): a pseudo-door's is zero, so collision treats the block as solid and
/// arms the elevator instead of publishing a transition. Its other ten bytes overlap the
/// neighbouring physical record and are never consumed, so no header is modeled for it.
/// </remarks>
public readonly record struct DoorListEntry
{
    private DoorListEntry(ushort pointer, CartridgeDoorHeader? door)
    {
        Pointer = pointer;
        Door = door;
    }

    /// <summary>The bank-$83 pointer stored in the door list.</summary>
    public ushort Pointer { get; }

    /// <summary>The physical door header, or null for an elevator pseudo-door.</summary>
    public CartridgeDoorHeader? Door { get; }

    /// <summary>True for an elevator pseudo-door (zero destination word).</summary>
    public bool IsElevatorPseudoDoor => Door is null;

    /// <summary>A physical door header.</summary>
    public static DoorListEntry Physical(CartridgeDoorHeader door)
    {
        ArgumentNullException.ThrowIfNull(door);
        return new(door.Pointer, door);
    }

    /// <summary>An elevator pseudo-door entry.</summary>
    public static DoorListEntry ElevatorPseudoDoor(ushort pointer)
    {
        if (pointer is not (DoorHeaderRomData.ElevatorPseudoDoorPointer or DoorHeaderRomData.MaridiaTourianElevatorPseudoDoorPointer))
            throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Not an elevator pseudo-door pointer.");
        return new(pointer, null);
    }
}
