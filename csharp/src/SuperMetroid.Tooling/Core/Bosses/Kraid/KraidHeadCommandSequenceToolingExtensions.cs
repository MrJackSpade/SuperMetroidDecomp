namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Development-tool instance members of <see cref="KraidHeadCommandSequence"/>.</summary>
internal static class KraidHeadCommandSequenceToolingExtensions
{
    /// <summary>Materializes the calculated commands for tools that need an array.</summary>
    extension(KraidHeadCommandSequence self)
    {
        /// <summary>Materializes all calculated commands for consumers requiring an array.</summary>
        internal KraidHeadInstructionDefinition[] ToArray()
        {
            var result = new KraidHeadInstructionDefinition[self.Length];
            for (int index = 0; index < result.Length; index++) result[index] = self[index];
            return result;
        }
    }
}
