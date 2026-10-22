using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured room block sheets (#1392): each tileset's sheet image, palette map and pictures, and the
/// provenance of their hashes. A player override of a sheet's file replaces the stock one.
/// </summary>
public static class RoomBlockSheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "room-block-sheets-provenance.json";

    /// <summary>Composes and writes every tileset's block sheet, then validates the result.</summary>
    /// <param name="content">Installed stock content holding the characters, definitions, palettes and uploads the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    public static void Extract(string content, string directory, string sourceCartridgeSha256)
    {
        IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> sheets = RoomBlockSheetComposer.Compose(RoomBlockSheetInputs.Load(content));
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ProvenanceFileName);
        foreach ((TilesetDefinition tileset, ComposedSpriteSheet sheet) in sheets)
            store.Write(RoomBlockSheet.NameOf(tileset), sheet);
        store.Finish(sourceCartridgeSha256);
        _ = Load(directory, null);
    }

    /// <summary>Loads the block sheet of every tileset a retail room loads, preferring a player override of each file.</summary>
    /// <exception cref="InvalidDataException">A stock hash, an image or a picture manifest is invalid, or a sheet is not of blocks.</exception>
    public static RoomBlockSheetCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        IReadOnlyList<TilesetDefinition> tilesets = RoomBlockUploads.Tilesets();
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName, tilesets.Count);
        return new RoomBlockSheetCatalog(tilesets.ToDictionary(tileset => tileset, tileset =>
        {
            SpriteSheet sheet = store.Read(RoomBlockSheet.NameOf(tileset));
            return new RoomBlockSheet(tileset, sheet);
        }));
    }

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);
}
