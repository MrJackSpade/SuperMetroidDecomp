using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="CrocomireMeltingUploadSequence"/> members production does not use.</summary>
internal static class CrocomireMeltingUploadSequenceAccess
{
    /// <summary>Enumerates the ordered graphics uploads issued by a Crocomire melt pass.</summary>
    extension(CrocomireMeltingUploadSequence self)
    {
        /// <summary>Visits compiled melting uploads in their native index order.</summary>
        internal IEnumerator<CrocomireMeltingUpload> GetEnumerator()
        {
            for (int index = 0; index < self.Length; index++)
                yield return self[index];
        }
    }
}
