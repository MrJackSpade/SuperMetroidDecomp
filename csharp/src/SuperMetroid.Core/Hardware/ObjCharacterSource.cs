namespace SuperMetroid.Core.Hardware;

/// <summary>What one OBJ character upload showed: an installed artwork identity, or bytes from game memory.</summary>
public sealed record ObjCharacterSource
{
    /// <summary>Restricts construction to the named source kinds.</summary>
    private ObjCharacterSource(ObjCharacterSourceKind kind, VramAssetId asset, int sourceAddress, int byteCount, byte[]? bytes)
    {
        Kind = kind;
        Asset = asset;
        SourceAddress = sourceAddress;
        ByteCount = byteCount;
        Bytes = bytes;
    }

    /// <summary>Which kind of source this is.</summary>
    public ObjCharacterSourceKind Kind { get; }
    /// <summary>The installed VRAM asset, for <see cref="ObjCharacterSourceKind.Asset"/>.</summary>
    public VramAssetId Asset { get; }
    /// <summary>The native upload's source address for <see cref="ObjCharacterSourceKind.Transfer"/>; the chunk entry for <see cref="ObjCharacterSourceKind.CrocomireSkeletonChunk"/>.</summary>
    public int SourceAddress { get; }
    /// <summary>The native upload's byte count for <see cref="ObjCharacterSourceKind.Transfer"/>; the copied byte count for <see cref="ObjCharacterSourceKind.Memory"/>.</summary>
    public int ByteCount { get; }
    /// <summary>Bytes copied from game memory, for <see cref="ObjCharacterSourceKind.Memory"/>.</summary>
    public byte[]? Bytes { get; }

    /// <summary>An installed VRAM asset.</summary>
    /// <param name="asset">The asset.</param>
    public static ObjCharacterSource FromAsset(VramAssetId asset) => new(ObjCharacterSourceKind.Asset, asset, 0, 0, null);

    /// <summary>The installed artwork a native upload from <paramref name="sourceAddress"/> selects.</summary>
    /// <param name="sourceAddress">The native upload's source address.</param>
    /// <param name="byteCount">The native upload's byte count.</param>
    public static ObjCharacterSource FromTransfer(int sourceAddress, int byteCount) =>
        new(ObjCharacterSourceKind.Transfer, VramAssetId.None, sourceAddress, byteCount, null);

    /// <summary>One installed Crocomire skeleton chunk.</summary>
    /// <param name="entry">The chunk's transfer entry.</param>
    public static ObjCharacterSource FromCrocomireSkeletonChunk(int entry) =>
        new(ObjCharacterSourceKind.CrocomireSkeletonChunk, VramAssetId.None, entry, 0, null);

    /// <summary>Bytes from game memory, copied because no installed artwork names them.</summary>
    /// <param name="bytes">The bytes uploaded.</param>
    public static ObjCharacterSource FromMemory(ReadOnlySpan<byte> bytes) =>
        new(ObjCharacterSourceKind.Memory, VramAssetId.None, 0, bytes.Length, bytes.ToArray());

    /// <summary>The source's current bytes.</summary>
    /// <param name="resolver">Resolves installed artwork identities.</param>
    public ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver)
    {
        Ensure.NotNull(resolver);
        return Kind switch
        {
            ObjCharacterSourceKind.Asset => resolver.ResolveAsset(Asset),
            ObjCharacterSourceKind.Transfer => resolver.ResolveTransfer(SourceAddress, ByteCount),
            ObjCharacterSourceKind.CrocomireSkeletonChunk => resolver.ResolveCrocomireSkeletonChunk(SourceAddress),
            ObjCharacterSourceKind.Memory => Bytes ?? throw new InvalidOperationException("A memory OBJ source has no bytes."),
            _ => throw new InvalidOperationException($"Unknown OBJ character source kind {Kind}."),
        };
    }

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => Kind switch
    {
        ObjCharacterSourceKind.Asset => $"asset {Asset}",
        ObjCharacterSourceKind.Transfer => $"transfer ${SourceAddress:X6}+{ByteCount}",
        ObjCharacterSourceKind.CrocomireSkeletonChunk => $"Crocomire skeleton chunk {SourceAddress}",
        ObjCharacterSourceKind.Memory => $"{ByteCount} memory bytes",
        _ => throw new InvalidOperationException($"Unknown OBJ character source kind {Kind}."),
    };
}
