namespace SuperMetroid.Core.Hardware;

/// <summary>Verification fixtures that fill a <see cref="VramStagingBuffer"/> directly.</summary>
internal static class VramStagingBufferAccess
{
    /// <summary>Exposes the staged bytes for writing.</summary>
    extension(VramStagingBuffer self)
    {
        /// <summary>The staged bytes as a fixture writes them; production stages through its owners.</summary>
        internal Span<byte> WritableBytes() =>
            System.Runtime.InteropServices.MemoryMarshal.CreateSpan(
                ref System.Runtime.InteropServices.MemoryMarshal.GetReference(self.Bytes), self.Length);
        /// <summary>The buffer's current bytes, as a drained transfer reads them.</summary>
        public ReadOnlySpan<byte> Bytes => self._bytes;
    }
}
