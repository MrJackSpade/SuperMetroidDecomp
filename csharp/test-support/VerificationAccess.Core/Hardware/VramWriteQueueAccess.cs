using SuperMetroid.AssetExtraction.Video;
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
        /// <summary>
        /// Queues a transfer of the first <paramref name="sizeInBytes"/> bytes of an owned word table that stands for the WRAM
        /// at <paramref name="nativeSourceAddress"/>. The table is read when the queue drains, low byte first.
        /// </summary>
        public void EnqueueWords(ushort sizeInBytes, int nativeSourceAddress, ushort[] words, VramDmaDestination destination)
        {
            Ensure.NotNull(words);
            if (sizeInBytes == 0 || (sizeInBytes & 1) != 0 || sizeInBytes > words.Length * sizeof(ushort))
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes), sizeInBytes, $"Transfer must be a nonzero whole number of the table's {words.Length} words.");
            self.Append(new VramWriteEntry(sizeInBytes, nativeSourceAddress, destination) { StagingWords = words });
        }
        /// <summary>Queues an upload of named owned artwork, resolved when the queue drains.</summary>
        /// <param name="source">The owned artwork uploaded.</param>
        /// <param name="sizeInBytes">Bytes uploaded.</param>
        /// <param name="destination">Where the upload lands.</param>
        public void EnqueueSource(ObjCharacterSource source, ushort sizeInBytes, VramDmaDestination destination)
        {
            Ensure.NotNull(source);
            if (sizeInBytes == 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
            self.Append(new VramWriteEntry(sizeInBytes, 0, destination) { OwnedSource = source });
        }
    }
}
