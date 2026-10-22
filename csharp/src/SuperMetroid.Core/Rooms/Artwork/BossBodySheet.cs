using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One installed pre-coloured boss body (#1392): an image of each part the boss writes into the BG2 tilemap it shows its
/// body on, coloured with its room tileset's standard palette over the characters its room loads, and where each tile word
/// is pictured over each artwork that can show at its character.
/// </summary>
public sealed class BossBodySheet
{
    /// <summary>The frame picturing tile words over artwork the room binds after loading, which no part shows over.</summary>
    public const string ReboundFrameName = "rebound";

    /// <summary>Where each tile word is pictured over each artwork at its character.</summary>
    private readonly Dictionary<BgTileArtwork, BgTilePicture> pictures = [];

    /// <summary>Indexes <paramref name="tiles"/> into <paramref name="sheet"/>'s frames.</summary>
    /// <exception cref="InvalidDataException">
    /// The sheet is not background-coloured, a tile lies outside its frame, or a word over one artwork is pictured twice.
    /// </exception>
    public BossBodySheet(BossBody body, TilesetDefinition tileset, SpriteSheet sheet, IEnumerable<BossBodyTile> tiles)
    {
        Body = body;
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        Tiles = [.. Ensure.NotNull(tiles)];
        foreach (BossBodyTile tile in Tiles)
        {
            SpriteSheetFrame frame = sheet.Frame(tile.Frame);
            if (frame.Sheet.Colours != SpriteImageColours.Background)
                throw new InvalidDataException($"{Name} is not coloured from the background palette.");
            if (tile.Corner.X + 8 > frame.Size.X || tile.Corner.Y + 8 > frame.Size.Y)
                throw new InvalidDataException($"{Name} pictures tile ${tile.Tile.Word.Raw:X4} outside its frame {tile.Frame}.");
            if (!pictures.TryAdd(tile.Tile, new BgTilePicture(frame, tile.Corner)))
                throw new InvalidDataException($"{Name} pictures tile ${tile.Tile.Word.Raw:X4} over {tile.Tile.Shown.Artwork.Name} twice.");
        }
    }

    /// <summary>The boss.</summary>
    public BossBody Body { get; }

    /// <summary>The tileset of the room the boss shows its body in.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Body, Tileset);

    /// <summary>The part images.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>Every tile pictured.</summary>
    public IReadOnlyList<BossBodyTile> Tiles { get; }

    /// <summary>Where <paramref name="tile"/> is pictured, if it is.</summary>
    public bool TryPicture(BgTileArtwork tile, out BgTilePicture picture) => pictures.TryGetValue(tile, out picture);

    /// <summary>The installed name of <paramref name="body"/>'s sheet in a room of <paramref name="tileset"/>.</summary>
    public static string NameOf(BossBody body, TilesetDefinition tileset) => $"{body}Body.{RoomBlockSheet.NameOf(tileset)}";
}
