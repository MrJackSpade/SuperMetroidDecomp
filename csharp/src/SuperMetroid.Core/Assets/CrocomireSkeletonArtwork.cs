namespace SuperMetroid.Core.Assets;

/// <summary>Editable 4-bpp characters for Crocomire's six ordered skeleton uploads.</summary>
public sealed class CrocomireSkeletonArtwork
{
    private readonly RoomCharacterAtlas atlas;

    private CrocomireSkeletonArtwork(RoomCharacterAtlas atlas) => this.atlas = atlas;

    public static CrocomireSkeletonArtwork Load(Stream png) =>
        new(RoomCharacterAtlas.Load(png,
            CrocomireSkeletonTransferDefinitions.TotalByteCount));

    internal ReadOnlyMemory<byte> Chunk(int index)
    {
        if (!CrocomireSkeletonTransferDefinitions.TryGet(index, out _))
            throw new InvalidDataException(
                "Crocomire skeleton's terminal entry has no character upload.");
        return atlas.Transfer.Slice(index *
            CrocomireSkeletonTransferDefinitions.ChunkByteCount,
            CrocomireSkeletonTransferDefinitions.ChunkByteCount);
    }
}
