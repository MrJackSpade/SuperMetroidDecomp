namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The bytes of the native Mode 7 transfer queue (cartridge WRAM <c>$02D0-$032F</c>), owned for the session. Ceres
/// Ridley's <see cref="Mode7TransferQueue"/> writes descriptors here, and the NMI handler clears only the first control
/// word, so the rest outlives the transfer: running-cadence stage five and the invalid-pose table overread later read it
/// as delay bytes through bank $91's low half (#1338).
/// </summary>
public sealed class Mode7TransferQueueResidue
{
    /// <summary>The queue bytes in native address order.</summary>
    internal readonly byte[] _bytes = new byte[Mode7TransferQueueLayout.QueueByteCount];

    /// <summary>
    /// The queue byte at bank-$91 low-half <paramref name="offset"/>, which aliases WRAM: running cadence stage five's
    /// <c>$91:0303</c>, or the pose-table overread of invalid poses at <c>$91:0302</c>. Only the queue's bytes are
    /// modeled; any other low-half offset is corrupt state.
    /// </summary>
    internal byte ReadNative(ushort offset)
    {
        int index = (int)new SnesAddress(SnesLittleEndian.BankByte(Mode7TransferQueueLayout.QueueStart), offset) -
            Mode7TransferQueueLayout.QueueStart;
        if ((uint)index >= Mode7TransferQueueLayout.QueueByteCount)
            throw new InvalidDataException($"Bank $91 low-half byte ${offset:X4} lies outside the Mode 7 transfer queue.");
        return _bytes[index];
    }
}
