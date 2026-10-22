using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured room artwork (#1392): each tileset's block sheet and each library background's image, each
/// as an image, palette map and pictures, the tilemap words each background pictures, and the provenance of their
/// hashes. A player override of a sheet's or image's file replaces the stock one.
/// </summary>
public static class RoomBlockSheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "room-block-sheets-provenance.json";

    /// <summary>The manifest naming each background image and the tilemap words it pictures.</summary>
    public const string BackgroundManifestFileName = "room-backgrounds.json";

    /// <summary>The background manifest's schema revision.</summary>
    private const int BackgroundManifestVersion = 1;

    /// <summary>Composes and writes every tileset's block sheet and background image, then validates the result.</summary>
    /// <param name="content">Installed stock content holding the characters, definitions, palettes, tilemaps and uploads the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    public static void Extract(string content, string directory, string sourceCartridgeSha256)
    {
        RoomSheetComposition composition = RoomBlockSheetComposer.Compose(RoomBlockSheetInputs.Load(content));
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ProvenanceFileName);
        foreach ((TilesetDefinition tileset, ComposedSpriteSheet sheet) in composition.BlockSheets)
            store.Write(RoomBlockSheet.NameOf(tileset), sheet);
        foreach ((string name, ComposedRoomBackground background) in composition.Backgrounds)
            store.Write(name, background.Sheet);
        store.Finish(sourceCartridgeSha256);
        var manifest = new BackgroundManifest(BackgroundManifestVersion,
            composition.Backgrounds.ToDictionary(entry => entry.Key, entry => entry.Value.Words, StringComparer.Ordinal));
        File.WriteAllBytes(Path.Combine(directory, BackgroundManifestFileName), JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
        _ = Load(directory, null);
    }

    /// <summary>
    /// Loads the block sheet of every tileset a retail room loads and every background image the manifest names,
    /// preferring a player override of each file.
    /// </summary>
    /// <exception cref="InvalidDataException">The manifest, a stock hash, an image or a picture manifest is invalid, or a sheet is not of blocks.</exception>
    public static RoomBlockSheetCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, BackgroundManifestFileName);
        BackgroundManifest manifest = JsonSerializer.Deserialize<BackgroundManifest>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Room background manifest {manifestPath} is empty.");
        if (manifest.Version != BackgroundManifestVersion || manifest.Words is null)
            throw new InvalidDataException($"Room background manifest {manifestPath} is not revision {BackgroundManifestVersion}.");
        IReadOnlyList<TilesetDefinition> tilesets = RoomBlockUploads.Tilesets();
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName, tilesets.Count + manifest.Words.Count);
        return new RoomBlockSheetCatalog(
            tilesets.ToDictionary(tileset => tileset, tileset => new RoomBlockSheet(tileset, store.Read(RoomBlockSheet.NameOf(tileset)))),
            manifest.Words.ToDictionary(entry => entry.Key, entry => new RoomBackgroundImage(entry.Key, store.Read(entry.Key), [.. entry.Value]),
                StringComparer.Ordinal));
    }

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>The background manifest: each image's name and the tilemap words it pictures.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Words">Each image's tilemap words, page by page.</param>
    private sealed record BackgroundManifest(int Version, Dictionary<string, ushort[]> Words);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
}
