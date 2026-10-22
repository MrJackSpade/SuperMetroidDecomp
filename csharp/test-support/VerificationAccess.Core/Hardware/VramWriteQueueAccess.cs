namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="VramWriteQueue"/> members production does not use.</summary>
internal static class VramWriteQueueAccess
{
    /// <summary>Exposes <see cref="VramWriteQueue"/> state that only verification reads.</summary>
    extension(VramWriteQueue self)
    {
        /// <summary>
        /// Queues a transfer from an owned staging buffer that stands for the WRAM at <paramref name="nativeSourceAddress"/>.
        /// The buffer is read when the queue drains; the native address stays on the record for debugger comparison.
        /// </summary>
        public void EnqueueStaging(int sizeInBytes, int nativeSourceAddress, VramStagingBuffer staging, int offset,
            VramDmaDestination destination)
        {
            Ensure.NotNull(staging);
            if (sizeInBytes is <= 0 or > VramWriteEntry.MaximumSizeInBytes) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
            if (offset < 0 || offset + sizeInBytes > staging.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, $"Transfer {offset}+{sizeInBytes} exceeds the {staging.Length}-byte staging buffer.");
            self.Append(new VramWriteEntry(sizeInBytes, nativeSourceAddress, destination) { Staging = staging, StagingOffset = offset });
        }
    }
}
