using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured room artwork (#1392): the block sheet of each tileset a room can load, and the image of each
/// library background a room state loads.
/// </summary>
public sealed class RoomBlockSheetCatalog
{
    /// <summary>
    /// Keeps the sheets, each under its own tileset, the background images, each under its own name, and the boss bodies,
    /// each under the tileset of its room.
    /// </summary>
    /// <exception cref="ArgumentException">A sheet or body is filed under another tileset, or an image under another name.</exception>
    public RoomBlockSheetCatalog(IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> sheets,
        IReadOnlyDictionary<string, RoomBackgroundImage> backgrounds, IReadOnlyDictionary<TilesetDefinition, BossBodySheet> bossBodies)
    {
        Sheets = Ensure.NotNull(sheets);
        Backgrounds = Ensure.NotNull(backgrounds);
        BossBodies = Ensure.NotNull(bossBodies);
        foreach ((TilesetDefinition tileset, BossBodySheet body) in bossBodies)
        {
            if (body.Tileset != tileset)
                throw new ArgumentException($"{body.Name} is filed under {tileset}.", nameof(bossBodies));
        }
        foreach ((string name, RoomBackgroundImage background) in backgrounds)
        {
            if (background.Name != name)
                throw new ArgumentException($"Room background {background.Name} is filed under {name}.", nameof(backgrounds));
        }
        foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in sheets)
        {
            if (sheet.Tileset != tileset)
                throw new ArgumentException($"The block sheet of {sheet.Tileset} is filed under {tileset}.", nameof(sheets));
        }
    }

    /// <summary>The sheets by tileset.</summary>
    public IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> Sheets { get; }

    /// <summary>The library background images by name.</summary>
    public IReadOnlyDictionary<string, RoomBackgroundImage> Backgrounds { get; }

    /// <summary>The boss bodies by the tileset of the room each boss shows its body in.</summary>
    public IReadOnlyDictionary<TilesetDefinition, BossBodySheet> BossBodies { get; }

    /// <summary>A content identity of every sheet, in tileset order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomBlockSheetCatalog), content =>
    {
        foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in Sheets.OrderBy(entry => RoomBlockSheet.NameOf(entry.Key), StringComparer.Ordinal))
        {
            content.AppendName("tileset", RoomBlockSheet.NameOf(tileset));
            sheet.Sheet.AppendIdentity(content);
        }
        foreach ((string name, RoomBackgroundImage background) in Backgrounds.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            content.AppendName("background", name);
            background.Sheet.AppendIdentity(content);
            foreach (ushort word in background.TilemapWords)
                content.Append("word", word);
        }
        foreach (BossBodySheet body in BossBodies.Values.OrderBy(body => body.Name, StringComparer.Ordinal))
        {
            content.AppendName("body", body.Name);
            body.Sheet.AppendIdentity(content);
            foreach (BossBodyTile tile in body.Tiles)
            {
                content.Append("word", tile.Tile.Word.Raw);
                content.AppendName("artwork", tile.Tile.Shown.Artwork.Name);
                content.Append("offset", tile.Tile.Shown.WordOffset);
                content.AppendName("frame", tile.Frame);
                content.Append("x", tile.Corner.X);
                content.Append("y", tile.Corner.Y);
            }
        }
    });

    /// <summary>The library background image named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">No such image is installed.</exception>
    public RoomBackgroundImage Background(string name) =>
        Backgrounds.TryGetValue(name, out RoomBackgroundImage? background) ? background
            : throw new InvalidDataException($"No room background image {name} is installed.");

    /// <summary>The block sheet of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No sheet is installed for the tileset.</exception>
    public RoomBlockSheet Get(TilesetDefinition tileset) =>
        Sheets.TryGetValue(tileset, out RoomBlockSheet? sheet) ? sheet : throw new InvalidDataException($"No block sheet is installed for {tileset}.");
}
