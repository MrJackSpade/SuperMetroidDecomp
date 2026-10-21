using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification access to BG2 frame definition lists.</summary>
internal static class EnemyBg2FrameDefinitionSequenceAccess
{
    /// <summary>Projections of a BG2 frame definition list.</summary>
    extension(ImmutableArray<EnemySpritemapDefinition> self)
    {
        /// <summary>A new array of <paramref name="selector"/>'s results for each frame and its index, in frame order.</summary>
        internal TResult[] SelectToArray<TResult>(Func<EnemySpritemapDefinition, int, TResult> selector)
        {
            Ensure.NotNull(selector);
            var result = new TResult[self.Length];
            for (int index = 0; index < self.Length; index++)
                result[index] = selector(self[index], index);
            return result;
        }
    }
}
