using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Ordered view of the extended-frame catalog without a generated lookup cache.
/// Ranges preserve the historical schema prefixes; entries are emitted by their owners.</summary>
internal readonly record struct EnemyExtendedFrameSequence(int Start, int Length) : IEnumerable<EnemyExtendedFrameDefinition>
{
    /// <summary>Creates a range-relative subview without materializing or copying frame definitions.</summary>
    internal EnemyExtendedFrameSequence this[Range range]
    {
        get
        {
            var (offset, length) = range.GetOffsetAndLength(Length);
            return new(Start + offset, length);
        }
    }

    /// <summary>Streams only the selected ordinal span while owners enumerate their compiled frames.</summary>
    public IEnumerator<EnemyExtendedFrameDefinition> GetEnumerator()
    {
        int index = 0;
        foreach (var frame in EnemyExtendedFrameDefinitions.EnumerateFrames())
        {
            if (index >= Start && index < Start + Length) yield return frame;
            index++;
        }
    }

    /// <summary>Uses the same ordered frame traversal for non-generic enumeration.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
