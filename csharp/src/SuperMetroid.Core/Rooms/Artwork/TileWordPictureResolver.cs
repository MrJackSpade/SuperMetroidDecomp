using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what tiles written over a room's background show (#1392, #1466): each tile's picture in the room's tile word
/// sheet for the values the appearance inputs it depends on take now.
/// </summary>
public sealed class TileWordPictureResolver(TileWordSheet sheet)
{
    /// <summary>The tiles last resolved: an overlay or a BG3 plane.</summary>
    private object? source;

    /// <summary>Each tile as it showed when last resolved.</summary>
    private TileWordAppearance[] shown = [];

    /// <summary>The pictures last resolved, null where a tile is unwritten.</summary>
    private ImmutableArray<BgTilePicture?> pictures;

    /// <summary>How each tile shows this resolution, by tile.</summary>
    private readonly Dictionary<BackgroundTile, TileWordAppearance> appearances = [];

    /// <summary>The sheet.</summary>
    public TileWordSheet Sheet { get; } = Ensure.NotNull(sheet);

    /// <summary>The picture of each tile written in <paramref name="overlay"/> while the room's inputs take the values <paramref name="appearance"/> reads.</summary>
    /// <exception cref="InvalidOperationException">The sheet pictures another layer's words.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures a tile for no such values.</exception>
    public ImmutableArray<BgTilePicture?> Resolve(RoomTilemapOverlay overlay, RoomAppearanceView appearance)
    {
        if (Sheet.Layer != TileWordLayer.Bg12)
            throw new InvalidOperationException($"{Sheet.Name} pictures no {TileWordLayer.Bg12} words.");
        ImmutableArray<BackgroundTile?> tiles = Ensure.NotNull(overlay).Tiles;
        return ResolveWords(overlay, tiles.Length, index => tiles[index], appearance);
    }

    /// <summary>
    /// The picture of each of <paramref name="count"/> tiles <paramref name="tileAt"/> gives, null for none, re-resolving only
    /// the tiles that show otherwise since <paramref name="words"/> was last resolved. The layer that owns the tiles gives
    /// them, as the BG3 plane's do (Layer3PictureResolution).
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures a tile for no such values.</exception>
    internal ImmutableArray<BgTilePicture?> ResolveWords(object words, int count, Func<int, BackgroundTile?> tileAt, RoomAppearanceView appearance)
    {
        bool fresh = !ReferenceEquals(words, source);
        if (fresh)
        {
            source = words;
            shown = new TileWordAppearance[count];
        }
        appearances.Clear();
        BgTilePicture?[]? next = null;
        for (int index = 0; index < count; index++)
        {
            if (tileAt(index) is not { } tile)
                continue;
            if (!appearances.TryGetValue(tile, out TileWordAppearance now))
            {
                now = Sheet.AppearanceOf(tile, appearance);
                appearances.Add(tile, now);
            }
            if (!fresh && now == shown[index])
                continue;
            shown[index] = now;
            next ??= fresh ? new BgTilePicture?[count] : [.. pictures];
            next[index] = Sheet.TryPicture(now, out BgTilePicture picture) ? picture
                : throw new InvalidDataException($"{Sheet.Name} pictures no tile {tile} for{(now.Appearance.Length == 0 ? " the room's load" : " " + now.Appearance)}.");
        }
        if (next is not null)
            pictures = [.. next];
        else if (fresh)
            pictures = [.. new BgTilePicture?[count]];
        return pictures;
    }
}
