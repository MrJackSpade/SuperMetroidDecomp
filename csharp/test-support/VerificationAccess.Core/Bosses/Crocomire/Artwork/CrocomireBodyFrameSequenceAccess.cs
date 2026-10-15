namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Verification access to <see cref="CrocomireBodyFrameSequence"/> members production does not use.</summary>
internal static class CrocomireBodyFrameSequenceAccess
{
    /// <summary>Materializes the sequence through its public indexed access.</summary>
    extension(CrocomireBodyFrameSequence self)
    {
        /// <summary>Copies each compiled Crocomire body frame word in sequence order.</summary>
        internal ushort[] ToArray()
        {
            var result = new ushort[self.Length];
            for (int i = 0; i < result.Length; i++) result[i] = self[i];
            return result;
        }
    }
}
