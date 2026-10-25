namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The room argument a grey door is placed with: the unlock condition it waits for, its door bit and its never-persist
/// flag. Setup keeps only the door bit and the flag.
/// </summary>
/// <param name="Word">The PLM's room argument, 0 through 65535.</param>
internal readonly record struct GreyDoorRoomArgument(int Word)
{
    /// <summary>The PLM's room argument, 0 through 65535.</summary>
    public int Word { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Word);

    /// <summary>Bits of the door bit's index: the low ten.</summary>
    private const int DoorBitIndexBits = (1 << 10) - 1;

    /// <summary>The never-persist flag: the top bit.</summary>
    private const int NeverPersistBit = 1 << 15;

    /// <summary>The lowest bit of the unlock condition's index.</summary>
    private const int ConditionShift = 10;

    /// <summary>Bits of the unlock condition's index once shifted down: five.</summary>
    private const int ConditionIndexBits = (1 << 5) - 1;

    /// <summary>The index of the unlock condition in the grey-door condition table.</summary>
    internal int ConditionIndex => (Word >> ConditionShift) & ConditionIndexBits;

    /// <summary>The argument setup leaves the door with: its door bit and never-persist flag.</summary>
    internal int DoorBitArgument => Word & (NeverPersistBit | DoorBitIndexBits);
}
