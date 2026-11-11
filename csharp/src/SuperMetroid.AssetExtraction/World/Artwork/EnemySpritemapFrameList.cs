using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Exposes the complete stock frame sequence with checked index and range access.</summary>
internal sealed class EnemySpritemapFrameList : IReadOnlyList<EnemyFrameName>
{
    /// <summary>The installed compositions in schema order, each with its document name.</summary>
    private readonly ImmutableArray<EnemyFrameName> frames =
        [.. EnemySpritemapDocumentOrder.Frames.Select(frame => new EnemyFrameName(frame, EnemyFrameNameDefinitions.Names[frame]))];
    /// <summary>Total named and appended frames in the current installed schema.</summary>
    public int Count => frames.Length;
    /// <summary>Reads one current-schema frame with an explicit bounds check.</summary>
    public EnemyFrameName this[int index] => (uint)index < Count
        ? frames[index] : throw new ArgumentOutOfRangeException(nameof(index));
    /// <summary>Traverses the materialized frame sequence in installed-schema order.</summary>
    public IEnumerator<EnemyFrameName> GetEnumerator() => ((IEnumerable<EnemyFrameName>)frames).GetEnumerator();
    /// <summary>Uses the same ordered traversal for non-generic collection consumers.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
