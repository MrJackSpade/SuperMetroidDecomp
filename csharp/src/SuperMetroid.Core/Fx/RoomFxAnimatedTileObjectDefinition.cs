namespace SuperMetroid.Core.Fx;

/// <summary>One engine-owned bank-$87 animated-tile object header and loop skeleton.</summary>
public sealed class RoomFxAnimatedTileObjectDefinition
{
    /// <summary>Arithmetic view of timed records, retaining no presentation source pointers.</summary>
    internal readonly CalculatedFrames frames;

    /// <summary>Binds a bank-$87 object header to its transfer geometry and frame schedule.</summary>
    internal RoomFxAnimatedTileObjectDefinition(
        AnimatedTileObject objectPointer,
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
    internal sealed class CalculatedFrames(ushort first, int count, ushort duration)
        : IReadOnlyList<RoomFxAnimatedTileFrameDefinition>
    {
        /// <summary>Number of timed entries before the terminal goto command.</summary>
        public int Count => count;
        /// <summary>Computes a four-byte record cursor and shared duration for a bounded frame.</summary>
        public RoomFxAnimatedTileFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= (uint)count) throw new ArgumentOutOfRangeException(nameof(index));
                return new(unchecked((ushort)(first + index * 4)), duration);
            }
        }
        /// <summary>Yields frame cursors in native instruction order.</summary>
        public IEnumerator<RoomFxAnimatedTileFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < count; index++) yield return this[index];
        }
        /// <summary>Exposes the same native frame order to non-generic collection callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The object-header address within bank $87.</summary>
    public AnimatedTileObject ObjectPointer { get; }

    /// <summary>The first timed instruction in the object's bank-$87 loop.</summary>
    public ushort InstructionPointer { get; }

    /// <summary>The number of presentation bytes copied for every displayed frame.</summary>
    public ushort TransferByteCount { get; }

    /// <summary>The hardware-encoded VRAM destination shared by every frame.</summary>
    public ushort EncodedVramDestination { get; }

    /// <summary>The timed frame-control records in cartridge execution order.</summary>
    public IReadOnlyList<RoomFxAnimatedTileFrameDefinition> Frames => frames;
}
