using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what a room's library background shows (#1392): its image, while BG2's tilemap pages hold the words it
/// pictures, and for each 16x16 cell whose characters show artwork the room bound after loading, that cell's block picture
/// over what they show now.
/// </summary>
/// <param name="image">The background's image.</param>
/// <param name="sheet">The room tileset's block sheet, which holds the cells' pictures.</param>
/// <param name="loaded">The artwork the room binds as it loads, which the image shows.</param>
public sealed class RoomBackgroundImageResolver(RoomBackgroundImage image, RoomBlockSheet sheet, IReadOnlySet<BgArtworkIdentity> loaded)
{
    /// <summary>Cells in the image's pages.</summary>
    private const int Cells = RoomBackgroundImage.Words / RoomBlockPictureNames.Tiles;

    /// <summary>What each cell's tiles showed when last resolved, four per cell.</summary>
    private readonly BgArtworkShown[] shown = new BgArtworkShown[RoomBackgroundImage.Words];

    /// <summary>The cell pictures last resolved, null where the image shows.</summary>
    private SpriteSheetFrame?[] cells = new SpriteSheetFrame?[Cells];

    /// <summary>Whether a resolution has been made.</summary>
    private bool resolved;

    /// <summary>The background's image.</summary>
    public RoomBackgroundImage Image { get; } = Ensure.NotNull(image);

    /// <summary>The background as BG2 shows it over what <paramref name="bindings"/> show now.</summary>
    /// <exception cref="InvalidDataException">
    /// BG2's tilemap pages no longer hold the image's words, or a cell over rebound artwork has no picture.
    /// </exception>
    [PpuLayoutBoundary("codec: the BG2 tilemap pages and BG characters a library background's tiles name")]
    public RoomBackgroundImagePlane Resolve(BgArtworkBindings bindings)
    {
        Ensure.NotNull(bindings);
        for (int word = 0; word < RoomBackgroundImage.Words; word++)
        {
            // Artwork bound over the tilemap since the background loaded shows there instead of its words.
            if (bindings.ShowsOver(GameplayVramLayout.Bg2TilemapWord + word, out BgArtworkIdentity over))
                throw new InvalidDataException($"{over.Name} is bound over BG2's tilemap word {word}, which room background {Image.Name} pictures.");
            ushort shownWord = bindings.Vram.ReadWord(GameplayVramLayout.Bg2TilemapWord + word);
            if (shownWord != Image.TilemapWords[word])
                throw new InvalidDataException(
                    $"BG2's tilemap word {word} holds ${shownWord:X4}, not the ${Image.TilemapWords[word]:X4} room background {Image.Name} pictures.");
        }
        SpriteSheetFrame?[]? next = resolved ? null : new SpriteSheetFrame?[Cells];
        Span<ushort> words = stackalloc ushort[RoomBlockPictureNames.Tiles];
        var now = new BgArtworkShown[RoomBlockPictureNames.Tiles];
        for (int cell = 0; cell < Cells; cell++)
        {
            int first = RoomBackgroundImage.CellWord(cell);
            words[0] = Image.TilemapWords[first];
            words[1] = Image.TilemapWords[first + 1];
            words[2] = Image.TilemapWords[first + RoomBackgroundImage.PageTiles];
            words[3] = Image.TilemapWords[first + RoomBackgroundImage.PageTiles + 1];
            bool changed = !resolved, rebound = false;
            for (int tile = 0; tile < RoomBlockPictureNames.Tiles; tile++)
            {
                now[tile] = bindings.Showing(new SnesBgTilemapWord(words[tile]).CharacterIndex * RoomBlockPictureNames.CharacterWords,
                    RoomBlockPictureNames.CharacterWords);
                rebound |= !loaded.Contains(now[tile].Artwork);
                changed |= now[tile] != shown[cell * RoomBlockPictureNames.Tiles + tile];
            }
            if (!changed)
                continue;
            now.CopyTo(shown.AsSpan(cell * RoomBlockPictureNames.Tiles, RoomBlockPictureNames.Tiles));
            next ??= [.. cells];
            next[cell] = rebound ? sheet.Picture(RoomBlockPictureNames.Of(words, now)) : null;
        }
        resolved = true;
        if (next is not null)
            cells = next;
        return new RoomBackgroundImagePlane(Image, cells);
    }
}
