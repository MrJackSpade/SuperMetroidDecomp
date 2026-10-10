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
            AnimatedTileObject.WreckedShipTreadmillRightwards,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillRightwardsWait),
        WreckedShipTreadmillDirection.Leftwards => new(direction,
            AnimatedTileObject.WreckedShipTreadmillLeftwards,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillLeftwardsWait),
        _ => throw new InvalidDataException($"Unknown Wrecked Ship treadmill direction {direction}."),
    };

    /// <summary>Resolves either named header; all other bank87 identities return false/null.</summary>
    public static bool TryResolve(AnimatedTileObject objectPointer, out WreckedShipTreadmillObjectDefinition definition)
    {
        definition = objectPointer switch
        {
            AnimatedTileObject.WreckedShipTreadmillRightwards => ForDirection(WreckedShipTreadmillDirection.Rightwards),
            AnimatedTileObject.WreckedShipTreadmillLeftwards => ForDirection(WreckedShipTreadmillDirection.Leftwards),
            AnimatedTileObject.None or AnimatedTileObject.TourianStatuePhantoon or
                AnimatedTileObject.TourianStatueRidley or AnimatedTileObject.TourianStatueKraid or
                AnimatedTileObject.TourianStatueDraygon or AnimatedTileObject.Empty or
                AnimatedTileObject.HorizontalSpikes or AnimatedTileObject.VerticalSpikes or
                AnimatedTileObject.CrateriaLake or AnimatedTileObject.UnusedCrateriaLava or
                AnimatedTileObject.BrinstarPlant or AnimatedTileObject.WreckedShipScreen or
                AnimatedTileObject.MaridiaSandCeiling or AnimatedTileObject.MaridiaSandFalling or
                AnimatedTileObject.Lava or AnimatedTileObject.Acid or AnimatedTileObject.Rain or
                AnimatedTileObject.Spores => null!,
            _ => throw new InvalidOperationException($"Undefined AnimatedTileObject {objectPointer}."),
        };
        return definition is not null;
    }
}

/// <summary>One direction-specific treadmill header and boss-gated four-frame loop.</summary>
public sealed class WreckedShipTreadmillObjectDefinition
{
    private readonly CalculatedFramePointers frameInstructionPointers;

    internal WreckedShipTreadmillObjectDefinition(
        WreckedShipTreadmillDirection direction,
        AnimatedTileObject objectPointer,
        ushort waitInstructionPointer)
    {
        Direction = direction;
        ObjectPointer = objectPointer;
        WaitInstructionPointer = waitInstructionPointer;
        frameInstructionPointers = new(LoopInstructionPointer);
    }

    // The native two-byte boss wait is followed by four four-byte timed entries.
    // Store the first cursor only; indexing and enumeration calculate the layout.
    private sealed class CalculatedFramePointers(ushort first) : IReadOnlyList<ushort>
    {
        public int Count => 4;
        public ushort this[int index] => (uint)index < 4
            ? unchecked((ushort)(first + 4 * index))
            : throw new ArgumentOutOfRangeException(nameof(index));
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private int FrameIndex(ushort pointer)
    {
        int delta = unchecked((ushort)(pointer - LoopInstructionPointer));
        return delta < 16 && (delta & 3) == 0 ? delta / 4 : -1;
    }
    /// <summary>The physical direction selected by the entrance door setup.</summary>
    public WreckedShipTreadmillDirection Direction { get; }

    /// <summary>The object-header address within bank $87.</summary>
    public AnimatedTileObject ObjectPointer { get; }

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
                $"Treadmill $87:{(int)ObjectPointer:X4} has no frame at $87:{instructionPointer:X4}.");
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
        if (pointer == (ushort)ObjectPointer)
            value = WaitInstructionPointer;
        else if (pointer == unchecked((ushort)(ObjectPointer + 2)))
            value = WreckedShipTreadmillRomData.TransferByteCount;
        else if (pointer == unchecked((ushort)(ObjectPointer + 4)))
            value = WreckedShipTreadmillRomData.EncodedVramDestination;
        else if (pointer == WaitInstructionPointer)
            value = (ushort)AnimatedTileInstruction.WaitUntilAreaBossIsDead;
        else if (FrameIndex(pointer) >= 0)
            value = 1;
        else if (pointer == GotoInstructionPointer)
            value = (ushort)AnimatedTileInstruction.Goto;
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
