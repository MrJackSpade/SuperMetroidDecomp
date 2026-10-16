namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="StagingByteView"/> members production does not use.</summary>
internal static class StagingByteViewAccess
{
    /// <summary>Exposes <see cref="StagingByteView"/> state that only verification reads.</summary>
    extension(StagingByteView self)
    {
        /// <summary>Writes <paramref name="source"/> over the whole view, in address order.</summary>
        public void CopyFrom(ReadOnlySpan<byte> source) => Ensure.LengthEqual(source, self.Length).CopyTo(self.Span);
    }
}
