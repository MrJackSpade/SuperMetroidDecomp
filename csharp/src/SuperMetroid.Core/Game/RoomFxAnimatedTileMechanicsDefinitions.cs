namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable engine-owned header and control data for the simple retail room-FX
/// animated-tile objects in bank <c>$87</c>.
/// </summary>
/// <remarks>
/// Frame source pointers deliberately are not stored here. They identify replaceable
/// installed character artwork. This catalog owns
/// only the instruction cursor, transfer geometry, duration, and loop control required
/// to schedule that artwork.
/// </remarks>
public static class RoomFxAnimatedTileMechanicsDefinitions
{
    /// <summary>Direct object-to-header dispatch; unknown bank87 identities return null.</summary>
    /// <remarks>Seven named spike/sand/liquid/weather objects select native instruction starts,
    /// transfer sizes and encoded VRAM destinations. Each field and the full ushort
    /// identity domain are independently verified against NTSC J/U v1.0 and pinned
    /// bank_87.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// Frame schedules use the separately verified arithmetic view; neither headers
    /// nor frames are retained in a generated table.</remarks>
    private static RoomFxAnimatedTileObjectDefinition? SelectObject(ushort objectPointer) => objectPointer switch
    {
        AnimatedTileObjectPointers.HorizontalSpikes => new(AnimatedTileObjectPointers.HorizontalSpikes,
            instructionPointer: 0x817e,
            transferByteCount: 0x0080,
            encodedVramDestination: 0x3d60,
            frameCount: 4, frameDuration: 8),
        AnimatedTileObjectPointers.MaridiaSandCeiling => new(AnimatedTileObjectPointers.MaridiaSandCeiling,
            instructionPointer: 0x8221,
            transferByteCount: 0x0040,
            encodedVramDestination: 0x1000,
            frameCount: 4, frameDuration: 0x000a),
        AnimatedTileObjectPointers.MaridiaSandFalling => new(AnimatedTileObjectPointers.MaridiaSandFalling,
            instructionPointer: 0x8235,
            transferByteCount: 0x0020,
            encodedVramDestination: 0x1020,
            frameCount: 4, frameDuration: 0x000a),
        AnimatedTileObjectPointers.Lava => new(AnimatedTileObjectPointers.Lava,
            instructionPointer: RoomFxRomData.Layer3AnimatedTiles.LavaFirstInstruction,
            transferByteCount: RoomFxRomData.Layer3AnimatedTiles.LiquidFrameByteCount,
            encodedVramDestination: RoomFxRomData.Layer3AnimatedTiles.LiquidDestinationWord,
            frameCount: 5, frameDuration: 0x000d),
        AnimatedTileObjectPointers.Acid => new(AnimatedTileObjectPointers.Acid,
            instructionPointer: RoomFxRomData.Layer3AnimatedTiles.AcidFirstInstruction,
            transferByteCount: RoomFxRomData.Layer3AnimatedTiles.LiquidFrameByteCount,
            encodedVramDestination: RoomFxRomData.Layer3AnimatedTiles.LiquidDestinationWord,
            frameCount: 5, frameDuration: 0x000a),
        AnimatedTileObjectPointers.Rain => new(AnimatedTileObjectPointers.Rain,
            instructionPointer: RoomFxRomData.Layer3AnimatedTiles.RainFirstInstruction,
            transferByteCount: RoomFxRomData.Layer3AnimatedTiles.RainFrameByteCount,
            encodedVramDestination: RoomFxRomData.Layer3AnimatedTiles.RainDestinationWord,
            frameCount: 5, frameDuration: 0x000a),
        AnimatedTileObjectPointers.Spores => new(AnimatedTileObjectPointers.Spores,
            instructionPointer: 0x82ed,
            transferByteCount: 0x0030,
            encodedVramDestination: 0x4280,
            frameCount: 3, frameDuration: 10),
        _ => null,
    };

    /// <summary>Enumerates the seven supported objects without stored headers.</summary>
    public static IEnumerable<RoomFxAnimatedTileObjectDefinition> All
    {
        get
        {
            yield return SelectObject(AnimatedTileObjectPointers.HorizontalSpikes)!;
            yield return SelectObject(AnimatedTileObjectPointers.MaridiaSandCeiling)!;
            yield return SelectObject(AnimatedTileObjectPointers.MaridiaSandFalling)!;
            yield return SelectObject(AnimatedTileObjectPointers.Lava)!;
            yield return SelectObject(AnimatedTileObjectPointers.Acid)!;
            yield return SelectObject(AnimatedTileObjectPointers.Rain)!;
            yield return SelectObject(AnimatedTileObjectPointers.Spores)!;
        }
    }

    /// <summary>Resolves a supported header; unknown identities preserve false and null output.</summary>
    public static bool TryResolve(ushort objectPointer, out RoomFxAnimatedTileObjectDefinition definition)
    {
        definition = SelectObject(objectPointer)!;
        return definition is not null;
    }
}

/// <summary>One engine-owned bank-$87 animated-tile object header and loop skeleton.</summary>
public sealed class RoomFxAnimatedTileObjectDefinition
{
    private readonly CalculatedFrames frames;

    internal RoomFxAnimatedTileObjectDefinition(
        ushort objectPointer,
        ushort instructionPointer,
        ushort transferByteCount,
        ushort encodedVramDestination,
        int frameCount,
        ushort frameDuration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameCount);

        ObjectPointer = objectPointer;
        InstructionPointer = instructionPointer;
        TransferByteCount = transferByteCount;
        EncodedVramDestination = encodedVramDestination;
        frames = new CalculatedFrames(instructionPointer, frameCount, frameDuration);
    }

    /// <summary>Bounded four-byte frame cursors with one native duration per object.</summary>
    /// <remarks>The seven simple native loops at $87:817E,8221,8235,8293,82B1,82CF,82ED
    /// have counts4,4,4,5,5,5,3. Spikes wait8 ticks, lava13, and all others10.
    /// Native timed entries consist of duration then artwork-source operand; goto
    /// follows the last entry. Both cursor and duration mappings are independently
    /// verified against NTSC J/U v1.0 and pinned bank_87.asm
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d). This view stores parameters only,
    /// never a generated frame table. Index bounds match the former read-only list.</remarks>
    private sealed class CalculatedFrames(ushort first, int count, ushort duration)
        : IReadOnlyList<RoomFxAnimatedTileFrameDefinition>
    {
        public int Count => count;
        public RoomFxAnimatedTileFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= (uint)count) throw new ArgumentOutOfRangeException(nameof(index));
                return new(unchecked((ushort)(first + index * 4)), duration);
            }
        }
        public IEnumerator<RoomFxAnimatedTileFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
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
    public IReadOnlyList<RoomFxAnimatedTileFrameDefinition> Frames => frames;

    /// <summary>The address of the terminal <c>goto</c> command after the final frame.</summary>
    public ushort GotoInstructionPointer =>
        unchecked((ushort)(InstructionPointer + frames.Count * 4));

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
}
