namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable control skeletons for the two Wrecked Ship treadmill animated-tile objects.
/// </summary>
/// <remarks>
/// The four frame-source operands are presentation identities resolved to installed art.
/// This catalog owns the object header, boss-wait command, durations, and loop control.
/// </remarks>
public static class WreckedShipTreadmillMechanicsDefinitions
{
    /// <summary>Dispatches the two physical conveyor directions to their native headers.</summary>
    public static WreckedShipTreadmillObjectDefinition ForDirection(
        WreckedShipTreadmillDirection direction) => direction switch
    {
        WreckedShipTreadmillDirection.Rightwards => new(direction,
            AnimatedTileObjectPointers.WreckedShipTreadmillRightwards,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillRightwardsWait),
        WreckedShipTreadmillDirection.Leftwards => new(direction,
            AnimatedTileObjectPointers.WreckedShipTreadmillLeftwards,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillLeftwardsWait),
        _ => throw new InvalidDataException($"Unknown Wrecked Ship treadmill direction {direction}."),
    };

    /// <summary>Resolves either named header; all other bank87 identities return false/null.</summary>
    public static bool TryResolve(ushort objectPointer, out WreckedShipTreadmillObjectDefinition definition)
    {
        definition = objectPointer switch
        {
            AnimatedTileObjectPointers.WreckedShipTreadmillRightwards => ForDirection(WreckedShipTreadmillDirection.Rightwards),
            AnimatedTileObjectPointers.WreckedShipTreadmillLeftwards => ForDirection(WreckedShipTreadmillDirection.Leftwards),
            _ => null!,
        };
        return definition is not null;
    }
}

/// <summary>One direction-specific treadmill header and boss-gated four-frame loop.</summary>
public sealed class WreckedShipTreadmillObjectDefinition
{
    /// <summary>Calculates the four timed frame-entry pointers from the loop's first instruction address.</summary>
    private readonly CalculatedFramePointers frameInstructionPointers;

    /// <summary>Creates the object header and derives its loop-entry addresses from the selected wait command.</summary>
    /// <param name="direction">Physical conveyor direction represented by this object.</param>
    /// <param name="objectPointer">Bank-$87 object-header address.</param>
    /// <param name="waitInstructionPointer">Instruction that waits for the area boss to be defeated.</param>
    internal WreckedShipTreadmillObjectDefinition(
        WreckedShipTreadmillDirection direction,
        ushort objectPointer,
        ushort waitInstructionPointer)
    {
        Direction = direction;
        ObjectPointer = objectPointer;
        WaitInstructionPointer = waitInstructionPointer;
        frameInstructionPointers = new(LoopInstructionPointer);
    }

    // The native two-byte boss wait is followed by four four-byte timed entries.
    // Store the first cursor only; indexing and enumeration calculate the layout.
    /// <summary>Calculates frame-entry pointers for the fixed four-record timed treadmill loop.</summary>
    /// <param name="first">Pointer to the first frame record after the boss-wait command.</param>
    private sealed class CalculatedFramePointers(ushort first) : IReadOnlyList<ushort>
    {
        /// <summary>Gets the number of timed frame records in the loop.</summary>
        public int Count => 4;

        /// <summary>Gets a frame-entry pointer by adding its four-byte record stride to the first entry.</summary>
        /// <param name="index">Zero-based frame record index, from 0 through 3.</param>
        /// <returns>The bank-relative pointer to the selected timed frame record.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the four-record loop.</exception>
        public ushort this[int index] => (uint)index < 4
            ? unchecked((ushort)(first + 4 * index))
            : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Enumerates all four calculated frame-entry pointers in loop order.</summary>
        /// <returns>An enumerator over the timed record addresses.</returns>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Maps an aligned pointer within this object's four frame records to its zero-based frame index.</summary>
    /// <param name="pointer">Candidate bank-relative instruction pointer.</param>
    /// <returns>The frame index from 0 through 3, or -1 when the pointer is outside the record run.</returns>
    private int FrameIndex(ushort pointer)
    {
        int delta = unchecked((ushort)(pointer - LoopInstructionPointer));
        return delta < 16 && (delta & 3) == 0 ? delta / 4 : -1;
    }
    /// <summary>The physical direction selected by the entrance door setup.</summary>
    public WreckedShipTreadmillDirection Direction { get; }

    /// <summary>The object-header address within bank $87.</summary>
    public ushort ObjectPointer { get; }

    /// <summary>The boss-wait command installed by the object header.</summary>
    public ushort WaitInstructionPointer { get; }

    /// <summary>The first timed frame reached after Phantoon is defeated.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(WaitInstructionPointer + 2));

    /// <summary>
    /// Returns the native artwork identity for a frame-control record without reading
    /// its bank-$87 presentation operand during installed play.
    /// </summary>
    public int FrameSourceAddress(ushort instructionPointer)
    {
        int listIndex = FrameIndex(instructionPointer);
        if (listIndex < 0)
            throw new InvalidDataException(
                $"Treadmill $87:{ObjectPointer:X4} has no frame at $87:{instructionPointer:X4}.");
        return WreckedShipTreadmillRomData.FrameSource(
            Direction == WreckedShipTreadmillDirection.Rightwards
                ? listIndex : 3 - listIndex);
    }

    /// <summary>The terminal loop command after the fourth frame.</summary>
    public ushort GotoInstructionPointer =>
        unchecked((ushort)(LoopInstructionPointer + 16));

    /// <summary>Reads one immutable mechanics word, excluding presentation source operands.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (pointer == ObjectPointer)
            value = WaitInstructionPointer;
        else if (pointer == unchecked((ushort)(ObjectPointer + 2)))
            value = WreckedShipTreadmillRomData.TransferByteCount;
        else if (pointer == unchecked((ushort)(ObjectPointer + 4)))
            value = WreckedShipTreadmillRomData.EncodedVramDestination;
        else if (pointer == WaitInstructionPointer)
            value = AnimatedTileInstructionCodes.WaitUntilAreaBossIsDead;
        else if (FrameIndex(pointer) >= 0)
            value = 1;
        else if (pointer == GotoInstructionPointer)
            value = AnimatedTileInstructionCodes.Goto;
        else if (pointer == unchecked((ushort)(GotoInstructionPointer + 2)))
            value = LoopInstructionPointer;
        else
        {
            value = 0;
            return false;
        }

        return true;
    }
}
