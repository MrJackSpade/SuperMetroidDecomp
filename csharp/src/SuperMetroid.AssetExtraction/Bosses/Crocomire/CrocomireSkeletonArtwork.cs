using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Crocomire;

/// <summary>Editable 4-bpp characters for Crocomire's six ordered skeleton uploads.</summary>
public sealed class CrocomireSkeletonArtwork
{

    /// <summary>Owned 4-bpp characters from which the six death-sequence uploads are sliced.</summary>
    private readonly RoomCharacterAtlas atlas;

    /// <summary>Retains validated character bytes while the death sequence owns their upload timing.</summary>
    private CrocomireSkeletonArtwork(RoomCharacterAtlas atlas) => this.atlas = atlas;

    /// <summary>Retains the installed skeleton's $0C00 bytes of four-bit characters, the six ordered $0200-byte upload chunks represented by $A4:99CB/$99D9.</summary>
    /// <param name="characters">The installed skeleton sheet's characters.</param>
    /// <returns>Owned selected character bytes; the death sequence retains upload cadence and fixed OBJ placement, while skeleton poses use separate OAM compositions.</returns>
    /// <exception cref="InvalidDataException">The characters are not the required skeleton character stream's length.</exception>
    public static CrocomireSkeletonArtwork Load(RoomCharacterAtlas characters) =>
        Ensure.NotNull(characters).Characters.Length == CrocomireSkeletonCharacterDefinitions.TotalByteCount ? new CrocomireSkeletonArtwork(characters)
            : throw new InvalidDataException($"Crocomire skeleton holds {characters.Characters.Length} bytes; expected {CrocomireSkeletonCharacterDefinitions.TotalByteCount}.");

    /// <summary>
    /// Requires <paramref name="index"/> to name one of the skeleton's chunks, counted from zero; the sequence ends after
    /// its last chunk, and that end is no chunk.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> names no chunk of the sequence.</exception>
    private static int RequireChunk(int index) =>
        (uint)index < CrocomireSkeletonCharacterDefinitions.ChunkCount ? index
            : throw new InvalidDataException($"Crocomire skeleton upload index {index} names no chunk of its sequence.");

    /// <summary>Gets one 512-byte upload chunk.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> names no chunk of the sequence.</exception>
    internal ReadOnlyMemory<byte> Chunk(int index)
    {
        return atlas.Characters.Slice(RequireChunk(index) *
            CrocomireSkeletonCharacterDefinitions.ChunkByteCount,
            CrocomireSkeletonCharacterDefinitions.ChunkByteCount);
    }
}
