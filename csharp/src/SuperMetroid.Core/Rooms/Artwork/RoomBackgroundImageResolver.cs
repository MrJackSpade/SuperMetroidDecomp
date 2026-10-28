using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what a room's background image shows (#1392): the image, and for each 16x16 cell whose characters show
/// artwork the room bound after loading, that cell's block picture over what they show now.
/// </summary>
/// <param name="image">The background's image.</param>
/// <param name="sheet">The room tileset's block sheet, which holds the cells' pictures.</param>
/// <param name="loaded">The artwork the room binds as it loads, which the image shows.</param>
public sealed class RoomBackgroundImageResolver(RoomBackgroundImage image, RoomBlockSheet sheet, IReadOnlySet<BgArtworkIdentity> loaded)
{
    /// <summary>What each cell's tiles showed when last resolved, four per cell.</summary>
    private readonly BgArtworkShown[] shown = new BgArtworkShown[image.Tiles.Length];

    /// <summary>The cell pictures last resolved, null where the image shows.</summary>
    private SpriteSheetFrame?[] cells = new SpriteSheetFrame?[image.CellCount];

    /// <summary>Whether a resolution has been made.</summary>
    private bool resolved;

    /// <summary>The background's image.</summary>
    public RoomBackgroundImage Image { get; } = Ensure.NotNull(image);

    /// <summary>
    /// The background as BG2 shows it over what <paramref name="bindings"/> show now, and over the frame
    /// <paramref name="animated"/> says an animation that paints a tile's character selected (#1406).
    /// </summary>
    /// <exception cref="InvalidDataException">A cell over rebound artwork has no picture.</exception>
    public RoomBackgroundImagePlane Resolve(BgArtworkBindings bindings, AnimatedCharacterView animated)
    {
        Ensure.NotNull(bindings);
        SpriteSheetFrame?[]? next = resolved ? null : new SpriteSheetFrame?[Image.CellCount];
        var tiles = new BackgroundTile[RoomBlockPictureNames.Tiles];
        var now = new BgArtworkShown[RoomBlockPictureNames.Tiles];
        for (int cell = 0; cell < Image.CellCount; cell++)
        {
            int first = RoomBackgroundImage.CellWord(cell);
            tiles[0] = Image.Tiles[first];
            tiles[1] = Image.Tiles[first + 1];
            tiles[2] = Image.Tiles[first + RoomBackgroundImage.PageTiles];
            tiles[3] = Image.Tiles[first + RoomBackgroundImage.PageTiles + 1];
            bool changed = !resolved, rebound = false;
            for (int tile = 0; tile < RoomBlockPictureNames.Tiles; tile++)
            {
                int character = tiles[tile].Tile;
                if (!animated.TryShowing(character, RoomBlockPictureNames.CharacterWords, out now[tile]))
                    now[tile] = bindings.Showing(BgCharacterSet.Tiles, character, 1);
                rebound |= !loaded.Contains(now[tile].Artwork);
                changed |= now[tile] != shown[cell * RoomBlockPictureNames.Tiles + tile];
            }
            if (!changed)
                continue;
            now.CopyTo(shown.AsSpan(cell * RoomBlockPictureNames.Tiles, RoomBlockPictureNames.Tiles));
            next ??= [.. cells];
            next[cell] = rebound ? sheet.Picture(RoomBlockPictureNames.Of(tiles, now)) : null;
        }
        resolved = true;
        if (next is not null)
            cells = next;
        return new RoomBackgroundImagePlane(Image, cells);
    }
}
