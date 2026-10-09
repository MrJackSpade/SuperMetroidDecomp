using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Ordered view of the extended-frame catalog without a generated lookup cache.
/// Ranges preserve the historical schema prefixes; entries are emitted by their owners.</summary>
/// <param name="Start">Zero-based offset of the first included frame in the catalog's published order.</param>
/// <param name="Length">Number of consecutive catalog frames included in this view.</param>
internal readonly record struct EnemyExtendedFrameSequence(int Start, int Length) : IEnumerable<EnemyExtendedFrameDefinition>
{
    /// <summary>Returns a view sliced by a range relative to this sequence's own frame positions.</summary>
    /// <param name="range">Slice of this sequence to represent in the returned view.</param>
    /// <returns>A sequence view with the selected offset and length.</returns>
    internal EnemyExtendedFrameSequence this[Range range]
    {
        get
        {
            var (offset, length) = range.GetOffsetAndLength(Length);
            return new(Start + offset, length);
        }
    }

    /// <summary>Enumerates the selected frame definitions in their original catalog order.</summary>
    /// <returns>An enumerator over this sequence's frames.</returns>
    public IEnumerator<EnemyExtendedFrameDefinition> GetEnumerator()
    {
        int index = 0;
        foreach (var frame in EnemyExtendedFrameDefinitions.EnumerateFrames())
        {
            if (index >= Start && index < Start + Length) yield return frame;
            index++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
