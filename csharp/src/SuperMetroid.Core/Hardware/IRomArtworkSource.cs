namespace SuperMetroid.Core.Hardware;

/// <summary>Resolves a known ROM visual transfer against current host-owned artwork.</summary>
public interface IRomArtworkSource
{
    /// <summary>Returns false only when the source lies outside this artwork domain.</summary>
    bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data);
}
