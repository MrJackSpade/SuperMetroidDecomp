using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Exposes every composed frame with its document name, in the authored listing the document is written in.</summary>
internal sealed class EnemySpritemapFrameList : IReadOnlyCollection<EnemyFrameName>
{
    /// <summary>The installed compositions, each with its document name.</summary>
    private readonly ImmutableArray<EnemyFrameName> frames =
        [.. EnemySpritemapComposedFrames.Frames.Select(frame => new EnemyFrameName(frame, EnemyFrameNameDefinitions.Names[frame]))];
    /// <summary>The number of compositions the document holds.</summary>
    public int Count => frames.Length;
    /// <summary>Traverses the compositions in the authored listing.</summary>
    public IEnumerator<EnemyFrameName> GetEnumerator() => ((IEnumerable<EnemyFrameName>)frames).GetEnumerator();
    /// <summary>Uses the same ordered traversal for non-generic collection consumers.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
