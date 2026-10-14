using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="CrocomireMeltingPassSequence"/> members production does not use.</summary>
internal static class CrocomireMeltingPassSequenceAccess
{
    /// <summary>Enumerates the two native melt passes in header order for verification.</summary>
    extension(CrocomireMeltingPassSequence self)
    {
        /// <summary>The two native melt passes, in header order; the indexer rejects any other index.</summary>
        internal IEnumerator<CrocomireMeltingPass> GetEnumerator()
        {
            yield return self[0];
            yield return self[1];
        }
    }
}
