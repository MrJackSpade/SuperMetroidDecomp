using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>The installed block sheet of each tileset a room can load (#1392).</summary>
public sealed class RoomBlockSheetCatalog
{
    /// <summary>Keeps the sheets, each under its own tileset.</summary>
    /// <exception cref="ArgumentException">A sheet is filed under another tileset.</exception>
    public RoomBlockSheetCatalog(IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> sheets)
    {
        Sheets = Ensure.NotNull(sheets);
        foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in sheets)
        {
            if (sheet.Tileset != tileset)
                throw new ArgumentException($"The block sheet of {sheet.Tileset} is filed under {tileset}.", nameof(sheets));
        }
    }

    /// <summary>The sheets by tileset.</summary>
    public IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> Sheets { get; }

    /// <summary>A content identity of every sheet, in tileset order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomBlockSheetCatalog), content =>
    {
        foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in Sheets.OrderBy(entry => RoomBlockSheet.NameOf(entry.Key), StringComparer.Ordinal))
        {
            content.AppendName("tileset", RoomBlockSheet.NameOf(tileset));
            sheet.Sheet.AppendIdentity(content);
        }
    });

    /// <summary>The block sheet of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No sheet is installed for the tileset.</exception>
    public RoomBlockSheet Get(TilesetDefinition tileset) =>
        Sheets.TryGetValue(tileset, out RoomBlockSheet? sheet) ? sheet : throw new InvalidDataException($"No block sheet is installed for {tileset}.");
}
