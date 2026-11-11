using System.Collections;
using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Ordered view of the extended-frame catalog without a generated lookup cache.
/// Ranges preserve the historical schema prefixes; entries are emitted by their owners.</summary>
internal readonly record struct EnemyExtendedFrameSequence(int Start, int Length) : IEnumerable<EnemySpritemapDefinition>
{
    /// <summary>Every extended frame in the document's order, each with its document name.</summary>
    private static readonly ImmutableArray<EnemySpritemapDefinition> Named =
        [.. EnemyExtendedFrameDocumentOrder.Frames.Select(frame => new EnemySpritemapDefinition(frame, EnemyFrameNameDefinitions.Names[frame]))];

    /// <summary>Streams only the selected ordinal span while owners enumerate their compiled frames.</summary>
    public IEnumerator<EnemySpritemapDefinition> GetEnumerator()
    {
        for (int index = Start; index < Start + Length; index++)
            yield return Named[index];
    }

    /// <summary>Uses the same ordered frame traversal for non-generic enumeration.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
