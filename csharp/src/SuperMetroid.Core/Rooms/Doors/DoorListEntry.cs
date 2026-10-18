using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One resolved entry of a room's bank-$8F door list: a physical door header, or one of the
/// elevator pseudo-doors (<see cref="ElevatorPseudoDoor.Shared"/>,
/// <see cref="ElevatorPseudoDoor.MaridiaTourian"/>).
/// </summary>
/// <remarks>
/// Native code tells them apart by bit 15 of the entry's destination word (<c>$94:938B</c>,
/// <c>$94:93CE</c>): a pseudo-door's is zero, so collision treats the block as solid and
/// arms the elevator instead of publishing a transition. Its other ten bytes overlap the
/// neighbouring physical record and are never consumed, so no header is modeled for it.
/// </remarks>
public readonly record struct DoorListEntry
{
    /// <summary>Pairs a physical header, or a pseudo-door kind for an elevator entry.</summary>
    private DoorListEntry(CartridgeDoorHeader? door, ElevatorPseudoDoor? pseudoDoor)
    {
        Door = door;
        PseudoDoor = pseudoDoor;
    }

    /// <summary>The physical door header, or null for an elevator pseudo-door.</summary>
    public CartridgeDoorHeader? Door { get; }

    /// <summary>The elevator pseudo-door, or null for a physical door.</summary>
    public ElevatorPseudoDoor? PseudoDoor { get; }

    /// <summary>True for an elevator pseudo-door (zero destination word).</summary>
    public bool IsElevatorPseudoDoor => Door is null;

    /// <summary>A physical door header.</summary>
    public static DoorListEntry Physical(CartridgeDoorHeader door) => new(Ensure.NotNull(door), null);

    /// <summary>An elevator pseudo-door entry.</summary>
    public static DoorListEntry ElevatorPseudoDoor(ElevatorPseudoDoor pseudoDoor)
    {
        if (!Enum.IsDefined(pseudoDoor))
            throw new ArgumentOutOfRangeException(nameof(pseudoDoor), pseudoDoor, "Not an elevator pseudo-door.");
        return new(null, pseudoDoor);
    }
}
