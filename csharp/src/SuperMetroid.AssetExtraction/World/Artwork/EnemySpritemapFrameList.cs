using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Exposes the complete stock frame sequence with checked index and range access.</summary>
internal sealed class EnemySpritemapFrameList : IReadOnlyList<EnemySpritemapDefinition>
{
    /// <summary>The installed compositions in schema order.</summary>
    private readonly ImmutableArray<EnemySpritemapDefinition> frames = EnemySpritemapCompositions.All;
    /// <summary>Total named and appended frames in the current installed schema.</summary>
    public int Count => frames.Length;
    /// <summary>Array-compatible length for internal catalog consumers.</summary>
    internal int Length => Count;
    /// <summary>Reads one current-schema frame with an explicit bounds check.</summary>
    public EnemySpritemapDefinition this[int index] => (uint)index < Count
        ? frames[index] : throw new ArgumentOutOfRangeException(nameof(index));
    /// <summary>Copies a contiguous schema prefix or family range for compatibility checks.</summary>
    internal EnemySpritemapDefinition[] this[Range range] => frames.AsSpan()[range].ToArray();
    /// <summary>Traverses the materialized frame sequence in installed-schema order.</summary>
    public IEnumerator<EnemySpritemapDefinition> GetEnumerator() => ((IEnumerable<EnemySpritemapDefinition>)frames).GetEnumerator();
    /// <summary>Uses the same ordered traversal for non-generic collection consumers.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
