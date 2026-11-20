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
    private readonly System.Collections.Concurrent.ConcurrentDictionary<CoverVariant, SpriteArtwork> coverCharacters = new();

    /// <summary>A cover's characters, distinguished by the aim and the opening frame they show.</summary>
    /// <param name="Direction">The aim the cover shows.</param>
    /// <param name="Frame">How far open the cover shows.</param>
    private sealed record CoverVariant(SamusProjectileDirection Direction, SamusArmCannonCoverFrame Frame) : SpriteArtworkVariant;

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
    private ReadOnlyMemory<byte> TileCharacters(int index) => (uint)index < SamusArmCannonArtworkFormat.CoverTileCount
        ? tiles.Characters.Slice(index * SamusArmCannonArtworkFormat.CoverTileByteCount, SamusArmCannonArtworkFormat.CoverTileByteCount)
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>The cover character aim <paramref name="direction"/> shows at opening frame <paramref name="frame"/>, as owned artwork.</summary>
    public SpriteArtwork CoverTile(SamusProjectileDirection direction, SamusArmCannonCoverFrame frame) =>
        coverCharacters.GetOrAdd(new CoverVariant(direction, frame), key => new SpriteArtwork(
            new SpriteArtworkId(SpriteArtworkKind.SamusArmCannonCover, key),
            TileCharacters(Cannon.CoverTileIndex(key.Direction, key.Frame)).Span));
}
