namespace SuperMetroid.Core.Fx;

/// <summary>One direction-specific treadmill header and boss-gated four-frame loop.</summary>
public sealed class WreckedShipTreadmillObjectDefinition
{
    /// <summary>Four native timed-frame cursors calculated from the first instruction pointer.</summary>
    internal readonly CalculatedFramePointers frameInstructionPointers;

    /// <summary>Builds one direction's boss-gated header and first frame cursor.</summary>
    internal WreckedShipTreadmillObjectDefinition(
        WreckedShipTreadmillDirection direction,
        AnimatedTileObject animatedObject,
        ushort waitInstructionPointer)
    {
        Direction = direction;
        AnimatedObject = animatedObject;
        WaitInstructionPointer = waitInstructionPointer;
        frameInstructionPointers = new(LoopInstructionPointer);
    }

    /// <summary>The two-byte boss wait is followed by four four-byte timed entries; only the first cursor is stored.</summary>
    internal sealed class CalculatedFramePointers(ushort first) : IReadOnlyList<ushort>
    {
        /// <summary>The four frame entries following the boss wait.</summary>
        public int Count => 4;
        /// <summary>Advances four bytes per timed frame from the first cursor.</summary>
        public ushort this[int index] => (uint)index < 4
            ? unchecked((ushort)(first + 4 * index))
            : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Visits the four frame instruction pointers in native loop order.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same frame order through the non-generic interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The physical direction selected by the entrance door setup.</summary>
    public WreckedShipTreadmillDirection Direction { get; }

    /// <summary>The object-header address within bank $87.</summary>
    public AnimatedTileObject AnimatedObject { get; }

    /// <summary>The boss-wait command installed by the object header.</summary>
    public ushort WaitInstructionPointer { get; }

    /// <summary>The first timed frame reached after Phantoon is defeated.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(WaitInstructionPointer + 2));

    /// <summary>
    /// The artwork identity of loop frame <paramref name="frameIndex"/>, 0 through 3: the leftward loop shows the
    /// rightward loop's art in reverse order.
    /// </summary>
    public int FrameSource(int frameIndex) =>
        WreckedShipTreadmillRomData.FrameSource(
            Direction == WreckedShipTreadmillDirection.Rightwards
                ? frameIndex : WreckedShipTreadmillMechanicsDefinitions.FrameCount - 1 - frameIndex);
}
