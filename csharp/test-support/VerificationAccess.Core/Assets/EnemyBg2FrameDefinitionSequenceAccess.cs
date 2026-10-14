using SuperMetroid.Core.Assets;

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
    }
}
