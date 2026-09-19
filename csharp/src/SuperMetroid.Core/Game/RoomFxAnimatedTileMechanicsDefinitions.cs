namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable engine-owned header and control data for the simple retail room-FX
/// animated-tile objects in bank <c>$87</c>.
/// </summary>
/// <remarks>
/// Frame source pointers deliberately are not stored here. They select replaceable
/// character artwork and remain cartridge-backed presentation data. This catalog owns
/// only the instruction cursor, transfer geometry, duration, and loop control required
/// to schedule that artwork.
/// </remarks>
public static class RoomFxAnimatedTileMechanicsDefinitions
{
    private static readonly RoomFxAnimatedTileObjectDefinition[] Definitions =
    [
        new(
            AnimatedTileObjectPointers.MaridiaSandCeiling,
            instructionPointer: 0x8221,
            transferByteCount: 0x0040,
            encodedVramDestination: 0x1000,
            [
                new(0x8221, 0x000a),
                new(0x8225, 0x000a),
                new(0x8229, 0x000a),
                new(0x822d, 0x000a),
            ]),
        new(
            AnimatedTileObjectPointers.MaridiaSandFalling,
            instructionPointer: 0x8235,
            transferByteCount: 0x0020,
            encodedVramDestination: 0x1020,
            [
                new(0x8235, 0x000a),
                new(0x8239, 0x000a),
                new(0x823d, 0x000a),
                new(0x8241, 0x000a),
            ]),
        new(
            AnimatedTileObjectPointers.Lava,
            instructionPointer: RoomFxRomData.Layer3AnimatedTiles.LavaFirstInstruction,
            transferByteCount: RoomFxRomData.Layer3AnimatedTiles.LiquidFrameByteCount,
            encodedVramDestination: RoomFxRomData.Layer3AnimatedTiles.LiquidDestinationWord,
            [
                new(0x8293, 0x000d),
                new(0x8297, 0x000d),
                new(0x829b, 0x000d),
                new(0x829f, 0x000d),
                new(0x82a3, 0x000d),
            ]),
        new(
            AnimatedTileObjectPointers.Acid,
            instructionPointer: RoomFxRomData.Layer3AnimatedTiles.AcidFirstInstruction,
            transferByteCount: RoomFxRomData.Layer3AnimatedTiles.LiquidFrameByteCount,
            encodedVramDestination: RoomFxRomData.Layer3AnimatedTiles.LiquidDestinationWord,
            [
                new(0x82b1, 0x000a),
                new(0x82b5, 0x000a),
                new(0x82b9, 0x000a),
                new(0x82bd, 0x000a),
                new(0x82c1, 0x000a),
            ]),
        new(
            AnimatedTileObjectPointers.Rain,
            instructionPointer: RoomFxRomData.Layer3AnimatedTiles.RainFirstInstruction,
            transferByteCount: RoomFxRomData.Layer3AnimatedTiles.RainFrameByteCount,
            encodedVramDestination: RoomFxRomData.Layer3AnimatedTiles.RainDestinationWord,
            [
                new(0x82cf, 0x000a),
                new(0x82d3, 0x000a),
                new(0x82d7, 0x000a),
                new(0x82db, 0x000a),
                new(0x82df, 0x000a),
            ]),
    ];
    private static readonly IReadOnlyList<RoomFxAnimatedTileObjectDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The five simple retail objects translated by this owner.</summary>
    public static IReadOnlyList<RoomFxAnimatedTileObjectDefinition> All => ReadOnlyDefinitions;

    /// <summary>Resolves a bank-$87 object header selected by translated room setup.</summary>
    public static bool TryResolve(
        ushort objectPointer,
        out RoomFxAnimatedTileObjectDefinition definition)
    {
        foreach (RoomFxAnimatedTileObjectDefinition candidate in Definitions)
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

/// <summary>One engine-owned bank-$87 animated-tile object header and loop skeleton.</summary>
public sealed class RoomFxAnimatedTileObjectDefinition
{
    private readonly RoomFxAnimatedTileFrameDefinition[] frames;
    private readonly IReadOnlyList<RoomFxAnimatedTileFrameDefinition> readOnlyFrames;

    internal RoomFxAnimatedTileObjectDefinition(
        ushort objectPointer,
        ushort instructionPointer,
        ushort transferByteCount,
        ushort encodedVramDestination,
        RoomFxAnimatedTileFrameDefinition[] frames)
    {
        if (frames.Length == 0)
            throw new ArgumentException("An animated-tile object requires at least one frame.", nameof(frames));

        ObjectPointer = objectPointer;
        InstructionPointer = instructionPointer;
        TransferByteCount = transferByteCount;
        EncodedVramDestination = encodedVramDestination;
        this.frames = frames;
        readOnlyFrames = Array.AsReadOnly(frames);
    }

    /// <summary>The object-header address within bank $87.</summary>
    public ushort ObjectPointer { get; }

    /// <summary>The first timed instruction in the object's bank-$87 loop.</summary>
    public ushort InstructionPointer { get; }

    /// <summary>The number of presentation bytes copied for every displayed frame.</summary>
    public ushort TransferByteCount { get; }

    /// <summary>The hardware-encoded VRAM destination shared by every frame.</summary>
    public ushort EncodedVramDestination { get; }

    /// <summary>The timed frame-control records in cartridge execution order.</summary>
    public IReadOnlyList<RoomFxAnimatedTileFrameDefinition> Frames => readOnlyFrames;

    /// <summary>The address of the terminal <c>goto</c> command after the final frame.</summary>
    public ushort GotoInstructionPointer =>
        unchecked((ushort)(frames[^1].InstructionPointer + 4));

    /// <summary>
    /// Reads one immutable mechanics word. Frame source operands are intentionally absent.
    /// </summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (pointer == ObjectPointer)
            value = InstructionPointer;
        else if (pointer == unchecked((ushort)(ObjectPointer + 2)))
            value = TransferByteCount;
        else if (pointer == unchecked((ushort)(ObjectPointer + 4)))
            value = EncodedVramDestination;
        else if (pointer == GotoInstructionPointer)
            value = AnimatedTileInstructionCodes.Goto;
        else if (pointer == unchecked((ushort)(GotoInstructionPointer + 2)))
            value = InstructionPointer;
        else
        {
            foreach (RoomFxAnimatedTileFrameDefinition frame in frames)
            {
                if (pointer != frame.InstructionPointer)
                    continue;

                value = frame.Duration;
                return true;
            }

            value = 0;
            return false;
        }

        return true;
    }
}

/// <summary>One timed control word preceding a live presentation-source operand.</summary>
public readonly record struct RoomFxAnimatedTileFrameDefinition(
    ushort InstructionPointer,
    ushort Duration)
{
    /// <summary>The cartridge-backed artwork pointer immediately after this control word.</summary>
    public ushort SourceOperandPointer => unchecked((ushort)(InstructionPointer + 2));
}
