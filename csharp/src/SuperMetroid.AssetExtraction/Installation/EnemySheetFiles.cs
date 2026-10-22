using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured enemy sheets (#1392): each distinct sheet's image, palette map and frames, the manifest
/// naming each room graphics set's sheet for each of its enemies, and the provenance of their hashes. A player override
/// of a sheet's file replaces the stock one.
/// </summary>
public static class EnemySheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "enemy-sheets-provenance.json";

    /// <summary>Composes and writes every enemy sheet and the set manifest, then validates the result.</summary>
    /// <param name="content">Installed stock content holding the tiles, compositions and colours the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    public static void Extract(string content, string directory, string sourceCartridgeSha256)
    {
        EnemySheetComposition composition = EnemySheetComposer.Compose(EnemySheetInputs.Load(content));
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ProvenanceFileName);
        foreach ((string name, ComposedSpriteSheet sheet) in composition.Sheets)
            store.Write(name, sheet);
        store.Finish(sourceCartridgeSha256);
        File.WriteAllBytes(Path.Combine(directory, EnemySheetDefinitions.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(composition.Manifest, JsonOptions));
        _ = Load(directory, null);
    }

    /// <summary>
    /// Loads the set manifest and every sheet it names, preferring a player override of each file, and checks each
    /// sheet holds exactly the frames its enemy's behaviour shows.
    /// </summary>
    /// <exception cref="InvalidDataException">The manifest, a stock hash, an image or a frame manifest is invalid.</exception>
    public static InstalledEnemySheets Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, EnemySheetDefinitions.ManifestFileName);
        EnemySheetManifestDocument manifest = JsonSerializer.Deserialize<EnemySheetManifestDocument>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Enemy sheet manifest {manifestPath} is empty.");
        if (manifest.Version != EnemySheetDefinitions.ManifestVersion)
            throw new InvalidDataException($"Enemy sheet manifest {manifestPath} has revision {manifest.Version}, not {EnemySheetDefinitions.ManifestVersion}.");
        string[] names = [.. manifest.Sets.Values.SelectMany(members => members.Values)
            .Concat(manifest.Spawned.Values.SelectMany(bases => bases.Values.Select(entry => entry.Sheet))).Distinct(StringComparer.Ordinal)];
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName, names.Length);
        var loaded = names.ToDictionary(name => name, store.Read, StringComparer.Ordinal);
        var sheets = new Dictionary<EnemySheetKey, SpriteSheet>();
        var pictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, CharacterRegion[][]>>();
        var spawnedPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, CharacterRegion[][]>>();
        // The sheets' frames are named from the stock enemy compositions installed beside them.
        var enemies = new Lazy<EnemyTileArtworkCatalog>(() => EnemyTileArtworkFiles.Load(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(stockDirectory))!, GameInstallationLayout.EnemyTileDirectoryName), null).Enemies);
        foreach ((string set, Dictionary<string, string> members) in manifest.Sets)
        {
            IReadOnlyList<RuntimeCharacterUpload> uploads = RuntimeCharacterUploadDefinitions.Catalog.Of(RoomEnemyGraphicsSetDefinitions.DrawnUnder
                .FirstOrDefault(definition => definition.Name == set) ?? throw new InvalidDataException($"Enemy sheet manifest names unknown graphics set {set}."));
            foreach ((string enemyName, string name) in members)
            {
                if (!Enum.TryParse(enemyName, out EnemyDefinitionId enemy) || !Enum.IsDefined(enemy))
                    throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown enemy {enemyName}.");
                SpriteSheet sheet = loaded[name];
                Dictionary<string, CharacterRegion[][]> framePictures = FramePictures(manifest.Pictures, set, enemyName, uploads);
                if (!sheet.HoldsExactly(PictureNames(EnemySheetComposer.FrameNames(enemies.Value, enemy), framePictures, uploads)))
                    throw new InvalidDataException($"Enemy sheet {name} does not hold exactly the pictures of the frames {enemy} shows.");
                sheets.Add(new EnemySheetKey(set, enemy), sheet);
                if (framePictures.Count > 0)
                    pictures.Add(new EnemySheetKey(set, enemy), framePictures);
            }
        }
        var spawned = new Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet>();
        foreach ((string set, Dictionary<string, SpawnedObjectSheetDocument> bases) in manifest.Spawned)
        {
            IReadOnlyList<RuntimeCharacterUpload> uploads = RuntimeCharacterUploadDefinitions.Catalog.Of(RoomEnemyGraphicsSetDefinitions.DrawnUnder
                .FirstOrDefault(definition => definition.Name == set) ?? throw new InvalidDataException($"Enemy sheet manifest names unknown graphics set {set}."));
            foreach ((string baseText, SpawnedObjectSheetDocument entry) in bases)
            {
                if (!int.TryParse(baseText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int tileBase))
                    throw new InvalidDataException($"Enemy sheet manifest set {set} names spawned tile base {baseText}, not a decimal number.");
                EnemyDefinitionId[] spawners = [.. entry.Spawners.Select(name => Enum.TryParse(name, out EnemyDefinitionId enemy) && Enum.IsDefined(enemy)
                    ? enemy : throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown spawner {name}."))];
                SpriteSheet sheet = loaded[entry.Sheet];
                Dictionary<string, CharacterRegion[][]> framePictures = FramePictures(manifest.SpawnedPictures, set, baseText, uploads);
                if (!sheet.HoldsExactly(PictureNames(SpawnedObjectSheetComposer.FrameNames(spawners, entry.Anywhere), framePictures, uploads)))
                    throw new InvalidDataException($"Spawned-object sheet {entry.Sheet} does not hold exactly the pictures of the objects its spawners spawn.");
                spawned.Add(new SpawnedObjectSheetKey(set, tileBase), new SpawnedObjectSheet(sheet, entry.Palette));
                if (framePictures.Count > 0)
                    spawnedPictures.Add(new SpawnedObjectSheetKey(set, tileBase), framePictures);
            }
        }
        return new InstalledEnemySheets(sheets, spawned, pictures, spawnedPictures, RuntimeCharacterUploadDefinitions.Catalog);
    }

    /// <summary>
    /// The layers the manifest says each frame of <paramref name="owner"/>'s sheet under <paramref name="set"/> draws as,
    /// each region of which must be one the set's encounters' <paramref name="uploads"/> divide the characters into.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame has no layer, or a layer names a region the uploads do not make.</exception>
    private static Dictionary<string, CharacterRegion[][]> FramePictures(
        Dictionary<string, Dictionary<string, Dictionary<string, CharacterRegion[][]>>> manifest, string set, string owner,
        IReadOnlyList<RuntimeCharacterUpload> uploads)
    {
        if (!manifest.TryGetValue(set, out Dictionary<string, Dictionary<string, CharacterRegion[][]>>? owners)
            || !owners.TryGetValue(owner, out Dictionary<string, CharacterRegion[][]>? frames))
            return new Dictionary<string, CharacterRegion[][]>(StringComparer.Ordinal);
        IReadOnlyList<CharacterRegion> regions = RuntimeCharacterUploadCatalog.Regions(uploads);
        foreach ((string frame, CharacterRegion[][] layers) in frames)
        {
            if (layers.Length == 0 || layers.Any(layer => layer.Any(region => !regions.Contains(region))))
                throw new InvalidDataException($"Enemy sheet manifest draws {set} {owner} frame {frame} in layers over regions its encounters do not upload.");
        }
        return new Dictionary<string, CharacterRegion[][]>(frames, StringComparer.Ordinal);
    }

    /// <summary>The names of the pictures of <paramref name="frames"/>: a frame drawn whole once, a layered one once for each picture of each layer.</summary>
    private static IEnumerable<string> PictureNames(IEnumerable<string> frames, IReadOnlyDictionary<string, CharacterRegion[][]> pictures,
        IReadOnlyList<RuntimeCharacterUpload> uploads) =>
        frames.SelectMany(frame => pictures.TryGetValue(frame, out CharacterRegion[][]? layers)
            ? layers.SelectMany((regions, layer) => RuntimeCharacterUploadCatalog.Contents(regions, uploads)
                .Select(contents => RuntimeCharacterUploadDefinitions.Catalog.PictureName(frame, layer, contents)))
            : [frame]);

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}
