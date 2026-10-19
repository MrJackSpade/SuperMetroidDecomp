namespace SuperMetroid.Core.Hardware;

/// <summary>Resolves the installed artwork identities that OBJ character uploads name.</summary>
public interface IObjCharacterSourceResolver
{
    /// <summary>The complete bytes of an installed VRAM asset.</summary>
    /// <param name="asset">The asset.</param>
    ReadOnlyMemory<byte> ResolveAsset(VramAssetId asset);

    /// <summary>The installed artwork a native upload from <paramref name="sourceAddress"/> of <paramref name="byteCount"/> bytes selects.</summary>
    /// <param name="sourceAddress">The native upload's source address.</param>
    /// <param name="byteCount">The native upload's byte count.</param>
    ReadOnlyMemory<byte> ResolveTransfer(int sourceAddress, int byteCount);

    /// <summary>One installed Crocomire skeleton character chunk.</summary>
    /// <param name="entry">The chunk's transfer entry.</param>
    ReadOnlyMemory<byte> ResolveCrocomireSkeletonChunk(int entry);
}
