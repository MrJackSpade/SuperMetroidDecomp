using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
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
    /// <summary>Each cover picture's character.</summary>
    private readonly FrozenDictionary<SamusArmCannonCoverTile, ReadOnlyMemory<byte>> tiles;

    /// <summary>Cover characters already built, by cover picture.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<CoverVariant, SpriteArtwork> coverCharacters = new();

    /// <summary>A cover's characters, distinguished by the cover picture they show: its orientation and how far open it is.</summary>
    /// <param name="Tile">The cover picture.</param>
    private sealed record CoverVariant(SamusArmCannonCoverTile Tile) : SpriteArtworkVariant;

    /// <summary>Binds the cover characters <paramref name="tiles"/> to the extracted arm cannon <paramref name="cannon"/>.</summary>
    /// <exception cref="InvalidDataException">The characters are not the twelve cover tiles.</exception>
    public SamusArmCannonCoverCharacters(SamusArmCannonSourceArtwork cannon, RoomCharacterAtlas tiles)
    {
        Cannon = Ensure.NotNull(cannon);
        this.tiles = ArmCannonTileSources.SplitPage(Ensure.NotNull(tiles).Characters);
    }

    /// <summary>The extracted descriptors, appearances and tile selections the characters are bound to.</summary>
    public SamusArmCannonSourceArtwork Cannon { get; }

    /// <summary>The character of cover picture <paramref name="tile"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tile"/> is no cover picture.</exception>
    private ReadOnlyMemory<byte> TileCharacters(SamusArmCannonCoverTile tile) => tiles.TryGetValue(tile, out ReadOnlyMemory<byte> characters)
        ? characters
        : throw new ArgumentOutOfRangeException(nameof(tile), tile, "No cover picture is held for it.");

    /// <summary>The cover character aim <paramref name="direction"/> shows at opening frame <paramref name="frame"/>, as owned artwork.</summary>
    public SpriteArtwork CoverTile(SamusProjectileDirection direction, SamusArmCannonCoverFrame frame) =>
        coverCharacters.GetOrAdd(new CoverVariant(Cannon.CoverTile(direction, frame)), key => new SpriteArtwork(
            new SpriteArtworkId(SpriteArtworkKind.SamusArmCannonCover, key), TileCharacters(key.Tile).Span));
}
