namespace SuperMetroid.Core.Hardware;

/// <summary>Owned buffers that stand for a range of the cartridge's WRAM, addressed by native bank-$7E source.</summary>
public interface IWorkRamStagingSource
{
    /// <summary>
    /// Whether this source holds the <paramref name="byteCount"/> bytes at native <paramref name="nativeAddress"/>; when
    /// it does, <paramref name="buffer"/> and <paramref name="offset"/> locate them.
    /// </summary>
    bool TryResolve(int nativeAddress, int byteCount, out VramStagingBuffer buffer, out int offset);
}
