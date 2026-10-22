using System.Collections.Immutable;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what a boss body's BG2 tilemap shows (#1392): each tile's picture in its body sheet over what the tile's
/// character shows. Crocomire's melt rewrites the characters it shows over, so those tiles draw from their characters
/// until the melt is drawn as an effect over his images.
/// </summary>
public sealed class BossBodyPictureResolver(BossBodySheet body)
{
    /// <summary>The words last resolved.</summary>
    private ImmutableArray<ushort> words;

    /// <summary>What each tile's character showed when last resolved.</summary>
    private BgArtworkShown[] shown = [];

    /// <summary>The pictures last resolved, null where a tile draws from its character.</summary>
    private ImmutableArray<BgTilePicture?> pictures;

    /// <summary>What each character shows this resolution, by character.</summary>
    private readonly Dictionary<int, BgArtworkShown> characters = [];

    /// <summary>The body's sheet.</summary>
    public BossBodySheet Body { get; } = Ensure.NotNull(body);

    /// <summary>The picture of each of <paramref name="tilemap"/>'s words over what <paramref name="bindings"/> show now.</summary>
    /// <exception cref="InvalidDataException">The sheet pictures a word over nothing its character shows.</exception>
    [PpuLayoutBoundary("codec: the BG characters a boss body's tile words name, from the room tileset's character base")]
    public ImmutableArray<BgTilePicture?> Resolve(ImmutableArray<ushort> tilemap, BgArtworkBindings bindings)
    {
        Ensure.NotNull(bindings);
        bool fresh = tilemap != words;
        if (fresh)
        {
            words = tilemap;
            shown = new BgArtworkShown[tilemap.Length];
        }
        characters.Clear();
        BgTilePicture?[]? next = null;
        for (int tile = 0; tile < tilemap.Length; tile++)
        {
            ushort word = tilemap[tile];
            int character = new SnesBgTilemapWord(word).CharacterIndex;
            if (!characters.TryGetValue(character, out BgArtworkShown now))
            {
                now = bindings.Showing(RoomGraphicsLayout.AreaCharacters.Offset + character * RoomBlockPictureNames.CharacterWords,
                    RoomBlockPictureNames.CharacterWords);
                characters.Add(character, now);
            }
            if (!fresh && now == shown[tile])
                continue;
            shown[tile] = now;
            next ??= fresh ? new BgTilePicture?[tilemap.Length] : [.. pictures];
            if (CrocomireMelting.IsMeltingCharacters(now.Artwork))
                next[tile] = null;
            else
            {
                next[tile] = Body.TryPicture(new BgTileArtwork(new SnesBgTilemapWord(word), now), out BgTilePicture picture) ? picture
                    : throw new InvalidDataException($"{Body.Name} pictures no tile ${word:X4} over {now.Artwork.Name} at word {now.WordOffset}.");
            }
        }
        if (next is not null)
            pictures = [.. next];
        return pictures;
    }
}
