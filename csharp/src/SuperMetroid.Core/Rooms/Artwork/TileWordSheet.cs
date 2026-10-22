using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured tile words of one tileset's rooms (#1392): an image of each part something writes over the
/// rooms' BG1 or BG2 after they load (a boss's body frames, typed text), coloured with the tileset's standard palette
/// over the characters the rooms load, and where each tile word is pictured over each artwork that can show at its
/// character.
/// </summary>
public sealed class TileWordSheet
{
    /// <summary>The frame picturing tile words over artwork the room binds after loading, which no part shows over.</summary>
    public const string ReboundFrameName = "rebound";

    /// <summary>Where each tile word is pictured over each artwork at its character.</summary>
    private readonly Dictionary<BgTileArtwork, BgTilePicture> pictures = [];

    /// <summary>Indexes <paramref name="tiles"/> into <paramref name="sheet"/>'s frames.</summary>
    /// <exception cref="InvalidDataException">
    /// The sheet is not background-coloured, a tile lies outside its frame, or a word over one artwork is pictured twice.
    /// </exception>
    public TileWordSheet(TilesetDefinition tileset, SpriteSheet sheet, IEnumerable<TileWordSheetTile> tiles)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        Tiles = [.. Ensure.NotNull(tiles)];
        foreach (TileWordSheetTile tile in Tiles)
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

    /// <summary>The tileset whose rooms the words are written in.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The part images.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>Every tile pictured.</summary>
    public IReadOnlyList<TileWordSheetTile> Tiles { get; }

    /// <summary>Where <paramref name="tile"/> is pictured, if it is.</summary>
    public bool TryPicture(BgTileArtwork tile, out BgTilePicture picture) => pictures.TryGetValue(tile, out picture);

    /// <summary>The installed name of the tile word sheet of <paramref name="tileset"/>'s rooms.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Words.{RoomBlockSheet.NameOf(tileset)}";
}
