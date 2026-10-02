using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Ordered view of the extended-frame catalog without a generated lookup cache.
/// Ranges preserve the historical schema prefixes; entries are emitted by their owners.</summary>
internal readonly record struct EnemyExtendedFrameSequence(int Start, int Length) : IEnumerable<EnemyExtendedFrameDefinition>
{
    internal EnemyExtendedFrameDefinition this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            return EnemyExtendedFrameDefinitions.EnumerateFrames().ElementAt(Start + index);
        }
    }

    internal EnemyExtendedFrameSequence this[Range range]
    {
        get
        {
            var (offset, length) = range.GetOffsetAndLength(Length);
            return new(Start + offset, length);
        }
    }

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
