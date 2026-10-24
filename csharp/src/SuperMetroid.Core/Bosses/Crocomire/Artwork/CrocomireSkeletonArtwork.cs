using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Editable 4-bpp characters for Crocomire's six ordered skeleton uploads.</summary>
public sealed class CrocomireSkeletonArtwork
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-crocomire-skeleton-v1",
        content => content.Append("tiles", atlas.Characters.Span));

    /// <summary>Owned 4-bpp characters from which the six death-sequence uploads are sliced.</summary>
    private readonly RoomCharacterAtlas atlas;

    /// <summary>Retains validated character bytes while the death sequence owns their upload timing.</summary>
    private CrocomireSkeletonArtwork(RoomCharacterAtlas atlas) => this.atlas = atlas;

    /// <summary>Retains the installed skeleton's $0C00 bytes of four-bit characters, the six ordered $0200-byte upload chunks represented by $A4:99CB/$99D9.</summary>
    /// <param name="characters">The installed skeleton sheet's characters.</param>
    /// <returns>Owned selected character bytes; the death sequence retains upload cadence and fixed OBJ placement, while skeleton poses use separate OAM compositions.</returns>
    /// <exception cref="InvalidDataException">The characters are not the required skeleton character stream's length.</exception>
    public static CrocomireSkeletonArtwork Load(RoomCharacterAtlas characters) =>
        Ensure.NotNull(characters).Characters.Length == CrocomireSkeletonTransferDefinitions.TotalByteCount ? new CrocomireSkeletonArtwork(characters)
            : throw new InvalidDataException($"Crocomire skeleton holds {characters.Characters.Length} bytes; expected {CrocomireSkeletonTransferDefinitions.TotalByteCount}.");

    /// <summary>Gets one $0200-byte upload chunk; rejects the terminal entry because it has no character transfer.</summary>
    internal ReadOnlyMemory<byte> Chunk(int index)
    {
        if (!CrocomireSkeletonTransferDefinitions.TryGet(index, out _))
            throw new InvalidDataException(
                "Crocomire skeleton's terminal entry has no character upload.");
        return atlas.Characters.Slice(index *
            CrocomireSkeletonTransferDefinitions.ChunkByteCount,
            CrocomireSkeletonTransferDefinitions.ChunkByteCount);
    }
}
