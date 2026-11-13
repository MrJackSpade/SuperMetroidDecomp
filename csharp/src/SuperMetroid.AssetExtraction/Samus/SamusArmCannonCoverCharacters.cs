using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The arm cannon's twelve 8×8 cover characters as the import reads them from its import data (#1636), bound to the
/// extracted arm cannon whose tile selections name them (#1638). The overlay sheet is composed from them; the installation
/// never holds them, and gameplay reads only the layout (<see cref="SamusArmCannonLayout"/>).
/// </summary>
public sealed class SamusArmCannonCoverCharacters
{
    /// <summary>The twelve nonclosed cover tiles' characters.</summary>
    private readonly RoomCharacterAtlas tiles;

    /// <summary>Cover characters already built, by cover tile index.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<int, SpriteArtwork> coverCharacters = new();

    /// <summary>Binds the cover characters <paramref name="tiles"/> to the extracted arm cannon <paramref name="cannon"/>.</summary>
    /// <exception cref="InvalidDataException">The characters are not the twelve cover tiles.</exception>
    public SamusArmCannonCoverCharacters(SamusArmCannonSourceArtwork cannon, RoomCharacterAtlas tiles)
    {
        Cannon = Ensure.NotNull(cannon);
        this.tiles = Ensure.NotNull(tiles);
        if (tiles.Characters.Length != SamusArmCannonArtworkFormat.CoverTileCount * SamusArmCannonArtworkFormat.CoverTileByteCount)
            throw new InvalidDataException("Arm-cannon cover characters must be the twelve cover tiles.");
    }

    /// <summary>The extracted descriptors, appearances and tile selections the characters are bound to.</summary>
    public SamusArmCannonSourceArtwork Cannon { get; }

    /// <summary>The characters of cover tile <paramref name="index"/>, 0 through 11.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index names no cover tile.</exception>
    public ReadOnlyMemory<byte> TileCharacters(int index) => (uint)index < SamusArmCannonArtworkFormat.CoverTileCount
        ? tiles.Characters.Slice(index * SamusArmCannonArtworkFormat.CoverTileByteCount, SamusArmCannonArtworkFormat.CoverTileByteCount)
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>The cover character for native direction 0-9 and opening frame 1-3, as owned artwork.</summary>
    /// <exception cref="InvalidDataException">The frame names no cover tile.</exception>
    public SpriteArtwork CoverTile(int direction, int frame)
    {
        int index = Cannon.CoverTileIndex(direction, frame) ??
            throw new InvalidDataException($"Arm-cannon direction {direction} frame {frame} names no installed cover tile.");
        return coverCharacters.GetOrAdd(index, key => new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.SamusArmCannonCover, key),
            TileCharacters(key).Span));
    }
}
