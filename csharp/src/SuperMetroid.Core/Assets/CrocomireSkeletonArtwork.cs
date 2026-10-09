namespace SuperMetroid.Core.Assets;

/// <summary>Editable 4-bpp characters for Crocomire's six ordered skeleton uploads.</summary>
public sealed class CrocomireSkeletonArtwork
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-crocomire-skeleton-v1",
        content => content.Append("tiles", atlas.Transfer.Span));

    /// <summary>Decoded indexed character bytes backing the six native skeleton upload chunks.</summary>
    private readonly RoomCharacterAtlas atlas;

    /// <summary>Stores the atlas whose bytes are exposed in the cartridge's upload cadence.</summary>
    /// <param name="atlas">Decoded character data containing all six contiguous upload chunks.</param>
    private CrocomireSkeletonArtwork(RoomCharacterAtlas atlas) => this.atlas = atlas;

    /// <summary>Compiles the 256-by-24 indexed skeleton sheet into $0C00 bytes of four-bit characters, retaining the six ordered $0200-byte upload chunks represented by $A4:99CB/$99D9.</summary>
    /// <param name="png">Caller-owned indexed PNG stream, left open, containing 96 row-major 8-by-8 characters with pen indices 0..15; PNG palette RGB values do not select runtime colors.</param>
    /// <returns>Owned selected character bytes; the death sequence retains upload cadence and fixed OBJ placement, while skeleton poses use separate OAM compositions.</returns>
    /// <exception cref="InvalidDataException">The PNG format, dimensions, or pixel indices do not represent the required skeleton character stream.</exception>
    public static CrocomireSkeletonArtwork Load(Stream png) =>
        new(RoomCharacterAtlas.Load(png,
            CrocomireSkeletonTransferDefinitions.TotalByteCount));

    /// <summary>Returns one $0200-byte character upload chunk in native transfer order.</summary>
    /// <param name="index">Zero-based upload position from zero through five.</param>
    /// <returns>A read-only view of the selected chunk within the owned atlas data.</returns>
    /// <exception cref="InvalidDataException">The index is the terminal sentinel or falls outside the six uploads.</exception>
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
