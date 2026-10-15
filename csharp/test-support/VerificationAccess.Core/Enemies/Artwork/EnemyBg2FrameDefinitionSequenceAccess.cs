namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification access to <see cref="EnemyBg2FrameDefinitionSequence"/> members production does not use.</summary>
internal static class EnemyBg2FrameDefinitionSequenceAccess
{
    /// <summary>Materializes BG2 frame definitions through the compiled sequence indexer.</summary>
    extension(EnemyBg2FrameDefinitionSequence self)
    {
        /// <summary>Copies all enemy BG2 frame definitions in authored order.</summary>
        internal EnemyBg2FrameDefinition[] ToArray()
        {
            var result = new EnemyBg2FrameDefinition[self.Length];
            for (int index = 0; index < result.Length; index++) result[index] = self[index];
            return result;
        }
        /// <summary>A new array of <paramref name="selector"/>'s results for each frame and its index, in frame order.</summary>
        internal TResult[] SelectToArray<TResult>(Func<EnemyBg2FrameDefinition, int, TResult> selector)
        {
            Ensure.NotNull(selector);
            var result = new TResult[self.Length];
            for (int index = 0; index < self.Length; index++)
                result[index] = selector(self[index], index);
            return result;
        }
    }
}
