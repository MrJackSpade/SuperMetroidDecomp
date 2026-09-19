namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable control skeletons for the two Wrecked Ship treadmill animated-tile objects.
/// </summary>
/// <remarks>
/// The four frame-source operands remain cartridge-backed presentation references. This
/// catalog owns only the object header, boss-wait command, durations, and loop control.
/// </remarks>
public static class WreckedShipTreadmillMechanicsDefinitions
{
    private static readonly WreckedShipTreadmillObjectDefinition[] Definitions =
    [
        new(
            WreckedShipTreadmillDirection.Rightwards,
            AnimatedTileObjectPointers.WreckedShipTreadmillRightwards,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillRightwardsWait,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillRightwardsLoop,
            [0x81e3, 0x81e7, 0x81eb, 0x81ef]),
        new(
            WreckedShipTreadmillDirection.Leftwards,
            AnimatedTileObjectPointers.WreckedShipTreadmillLeftwards,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillLeftwardsWait,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillLeftwardsLoop,
            [0x81f9, 0x81fd, 0x8201, 0x8205]),
    ];
    private static readonly IReadOnlyList<WreckedShipTreadmillObjectDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>Both direction-specific retail treadmill objects.</summary>
    public static IReadOnlyList<WreckedShipTreadmillObjectDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves the object selected by the Wrecked Ship entrance door setup.</summary>
    public static WreckedShipTreadmillObjectDefinition ForDirection(
        WreckedShipTreadmillDirection direction) =>
        Definitions.FirstOrDefault(candidate => candidate.Direction == direction) ??
        throw new InvalidDataException($"Unknown Wrecked Ship treadmill direction {direction}.");

    /// <summary>Resolves a stock bank-$87 treadmill object header.</summary>
    public static bool TryResolve(
        ushort objectPointer,
        out WreckedShipTreadmillObjectDefinition definition)
    {
        foreach (WreckedShipTreadmillObjectDefinition candidate in Definitions)
        {
            if (candidate.ObjectPointer != objectPointer)
                continue;

            definition = candidate;
            return true;
        }

        definition = null!;
        return false;
    }
}

/// <summary>One direction-specific treadmill header and boss-gated four-frame loop.</summary>
public sealed class WreckedShipTreadmillObjectDefinition
{
    private readonly ushort[] frameInstructionPointers;
    private readonly IReadOnlyList<ushort> readOnlyFrameInstructionPointers;

    internal WreckedShipTreadmillObjectDefinition(
        WreckedShipTreadmillDirection direction,
        ushort objectPointer,
        ushort waitInstructionPointer,
        ushort loopInstructionPointer,
        ushort[] frameInstructionPointers)
    {
        if (frameInstructionPointers.Length != 4)
        {
            throw new ArgumentException(
                "A Wrecked Ship treadmill requires exactly four animation frames.",
                nameof(frameInstructionPointers));
        }

        Direction = direction;
        ObjectPointer = objectPointer;
        WaitInstructionPointer = waitInstructionPointer;
        LoopInstructionPointer = loopInstructionPointer;
        this.frameInstructionPointers = frameInstructionPointers;
        readOnlyFrameInstructionPointers = Array.AsReadOnly(frameInstructionPointers);
    }

    /// <summary>The physical direction selected by the entrance door setup.</summary>
    public WreckedShipTreadmillDirection Direction { get; }

    /// <summary>The object-header address within bank $87.</summary>
    public ushort ObjectPointer { get; }

    /// <summary>The boss-wait command installed by the object header.</summary>
    public ushort WaitInstructionPointer { get; }

    /// <summary>The first timed frame reached after Phantoon is defeated.</summary>
    public ushort LoopInstructionPointer { get; }

    /// <summary>The four timed frame-control addresses in execution order.</summary>
    public IReadOnlyList<ushort> FrameInstructionPointers =>
        readOnlyFrameInstructionPointers;

    /// <summary>The terminal loop command after the fourth frame.</summary>
    public ushort GotoInstructionPointer =>
        unchecked((ushort)(frameInstructionPointers[^1] + 4));

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
        else if (frameInstructionPointers.Contains(pointer))
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
