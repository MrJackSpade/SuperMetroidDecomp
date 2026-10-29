using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

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

    /// <summary>Composes and writes every enemy sheet and the set manifest; the installer validates them with <see cref="ValidateStock"/>.</summary>
    /// <param name="content">Installed stock content holding the tiles, compositions and colours the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    public static void Extract(string content, string directory, string sourceCartridgeSha256)
    {
        EnemySheetComposition composition = EnemySheetComposer.Compose(EnemySheetInputs.Load(content));
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ProvenanceFileName);
        store.WriteAll([.. composition.Sheets.Select(entry => new NamedComposedSheet(entry.Key, entry.Value))]);
        store.Finish(sourceCartridgeSha256);
        File.WriteAllBytes(Path.Combine(directory, EnemySheetDefinitions.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(composition.Manifest, JsonOptions));
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
        // A player's sheet written before enemy frames were named for their meaning names them by their older keys (#1465).
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName, names.Length,
            EnemySheetLegacyFrameNames.Current);
        var loaded = names.ToDictionary(name => name, store.Read, StringComparer.Ordinal);
        var sheets = new Dictionary<EnemySheetKey, SpriteSheet>();
        var pictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, LookRegion[][]>>();
        var spawnedPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, LookRegion[][]>>();
        var coverage = new Dictionary<string, SetLookCoverage>(StringComparer.Ordinal);
        var rotPictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, string>>();
        var spawnedRotPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, string>>();
        // The sheets' frames are named from the stock enemy compositions installed beside them.
        var artwork = new Lazy<InstalledEnemyArtwork>(() => EnemyTileArtworkFiles.Load(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(stockDirectory))
                ?? throw new ArgumentException($"Stock directory {stockDirectory} has no installation above it.", nameof(stockDirectory)),
                GameInstallationLayout.EnemyTileDirectoryName), null));
        var setRegions = new Dictionary<string, SetRegions>(StringComparer.Ordinal);
        var setCorpses = new Dictionary<string, IReadOnlyList<CorpseRotSimulation>>(StringComparer.Ordinal);
        foreach ((string set, Dictionary<string, string> members) in manifest.Sets)
        {
            EnemyDefinitionId[] drawn = [.. members.Keys.Select(enemyName => Enum.TryParse(enemyName, out EnemyDefinitionId enemy) && Enum.IsDefined(enemy)
                ? enemy : throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown enemy {enemyName}."))];
            // The corpses that rot in the set's rooms are pictured for the rot steps the extraction plays from their art.
            IReadOnlyList<CorpseRotSimulation> corpses = CorpseRotSimulation.Of(artwork.Value.Enemies, artwork.Value.Bosses, drawn);
            setCorpses[set] = [.. corpses.Where(corpse => corpse.Kind is not null)];
            RoomEnemyGraphicsSetDefinition definition = RoomEnemyGraphicsSetDefinitions.DrawnUnder
                .FirstOrDefault(known => known.Name == set) ?? throw new InvalidDataException($"Enemy sheet manifest names unknown graphics set {set}.");
            SetRegions regions = setRegions[set] = new SetRegions(SuperMetroid.AssetExtraction.Enemies.EncounterCharacterPlacements.Catalog.Of(definition),
                [.. corpses.Where(corpse => corpse.Kind is null)]);
            coverage.Add(set, regions.Coverage(definition));
            foreach ((string enemyName, string name) in members)
            {
                var enemy = Enum.Parse<EnemyDefinitionId>(enemyName);
                SpriteSheet sheet = loaded[name];
                Dictionary<string, CharacterRange[][]> framePictures = FramePictures(manifest.Pictures, set, enemyName, regions);
                Dictionary<string, string> frameRots = RotFrames(manifest.RotPictures, set, enemyName, setCorpses[set]);
                if (!sheet.HoldsExactly(EnemySheetComposer.FrameNames(artwork.Value.Enemies, enemy).SelectMany(frame => PictureNames(frame,
                        regions, framePictures, frameRots, setCorpses[set], CorpseRotSimulation.Rots(enemy)))))
                    throw new InvalidDataException($"Enemy sheet {name} does not hold exactly the pictures of the frames {enemy} shows.");
                sheets.Add(new EnemySheetKey(set, enemy), sheet);
                if (framePictures.Count > 0)
                    pictures.Add(new EnemySheetKey(set, enemy), Named(framePictures, regions));
                if (frameRots.Count > 0)
                    rotPictures.Add(new EnemySheetKey(set, enemy), frameRots);
            }
        }
        var spawned = new Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet>();
        foreach ((string set, Dictionary<string, SpawnedObjectSheetDocument> bases) in manifest.Spawned)
        {
            SetRegions regions = setRegions.TryGetValue(set, out SetRegions? known) ? known
                : throw new InvalidDataException($"Enemy sheet manifest spawns objects under graphics set {set}, which it installs no sheets for.");
            foreach ((string baseText, SpawnedObjectSheetDocument entry) in bases)
            {
                if (!int.TryParse(baseText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int tileBase))
                    throw new InvalidDataException($"Enemy sheet manifest set {set} names spawned tile base {baseText}, not a decimal number.");
                EnemyDefinitionId[] spawners = [.. entry.Spawners.Select(name => Enum.TryParse(name, out EnemyDefinitionId enemy) && Enum.IsDefined(enemy)
                    ? enemy : throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown spawner {name}."))];
                SpriteSheet sheet = loaded[entry.Sheet];
                Dictionary<string, CharacterRange[][]> framePictures = FramePictures(manifest.SpawnedPictures, set, baseText, regions);
                Dictionary<string, string> frameRots = RotFrames(manifest.SpawnedRotPictures, set, baseText, setCorpses[set]);
                if (!sheet.HoldsExactly(SpawnedObjectSheetComposer.FrameNames(spawners, entry.Anywhere).SelectMany(frame => PictureNames(frame,
                        regions, framePictures, frameRots, setCorpses[set], SpawnedObjectSheetComposer.ShowsRot(frame)))))
                    throw new InvalidDataException($"Spawned-object sheet {entry.Sheet} does not hold exactly the pictures of the objects its spawners spawn.");
                spawned.Add(new SpawnedObjectSheetKey(set, tileBase), new SpawnedObjectSheet(sheet, ImageColourGroup.Numbered(entry.Palette)));
                if (framePictures.Count > 0)
                    spawnedPictures.Add(new SpawnedObjectSheetKey(set, tileBase), Named(framePictures, regions));
                if (frameRots.Count > 0)
                    spawnedRotPictures.Add(new SpawnedObjectSheetKey(set, tileBase), frameRots);
            }
        }
        return new InstalledEnemySheets(new(sheets, pictures, rotPictures), new(spawned, spawnedPictures, spawnedRotPictures),
            SuperMetroid.AssetExtraction.Enemies.EncounterCharacterPlacements.Catalog, coverage);
    }

    /// <summary>Each frame's layers over the regions Core names by ordinal.</summary>
    internal static IReadOnlyDictionary<string, LookRegion[][]> Named(IReadOnlyDictionary<string, CharacterRange[][]> frames, SetRegions regions) =>
        frames.ToDictionary(entry => entry.Key, entry => entry.Value.Select(layer => layer.Select(regions.RegionOf).ToArray()).ToArray(), StringComparer.Ordinal);

    /// <summary>
    /// The names of the pictures a sheet holds of <paramref name="frame"/>: for a frame showing a corpse's rot, the frame
    /// and its picture of each rot the corpse can show (#1406); else its pictures for what its uploaded regions can show.
    /// </summary>
    private static IEnumerable<string> PictureNames(string frame, SetRegions regions, Dictionary<string, CharacterRange[][]> framePictures,
        Dictionary<string, string> frameRots, IReadOnlyList<CorpseRotSimulation> corpses, bool showsRot) =>
        frameRots.TryGetValue(frame, out string? corpse)
            ? corpses.Single(simulation => simulation.Flesh == corpse).PictureNames(frame)
            : regions.PictureNames(frame, framePictures.GetValueOrDefault(frame, []), showsRot);

    /// <summary>
    /// The kind of corpse the manifest says each frame of <paramref name="owner"/>'s sheet under <paramref name="set"/>
    /// drawn over a corpse's rows shows the rot of, each of which must rot in the set's rooms.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame shows the rot of a corpse that does not rot under the set.</exception>
    private static Dictionary<string, string> RotFrames(Dictionary<string, Dictionary<string, Dictionary<string, string>>> manifest,
        string set, string owner, IReadOnlyList<CorpseRotSimulation> corpses)
    {
        if (!manifest.TryGetValue(set, out Dictionary<string, Dictionary<string, string>>? owners)
            || !owners.TryGetValue(owner, out Dictionary<string, string>? frames))
            return new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((string frame, string corpse) in frames)
        {
            if (!corpses.Any(simulation => simulation.Flesh == corpse))
                throw new InvalidDataException($"Enemy sheet manifest draws {set} {owner} frame {frame} with the rot of {corpse}, which does not rot under the set.");
        }
        return new Dictionary<string, string>(frames, StringComparer.Ordinal);
    }

    /// <summary>
    /// The layers the manifest says each frame of <paramref name="owner"/>'s sheet under <paramref name="set"/> draws as,
    /// each region of which must be one the set's encounters' uploads and corpses' rows divide the characters into.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame has no layer, or a layer names a region the uploads do not make.</exception>
    private static Dictionary<string, CharacterRange[][]> FramePictures(
        Dictionary<string, Dictionary<string, Dictionary<string, CharacterRange[][]>>> manifest, string set, string owner,
        SetRegions setRegions)
    {
        if (!manifest.TryGetValue(set, out Dictionary<string, Dictionary<string, CharacterRange[][]>>? owners)
            || !owners.TryGetValue(owner, out Dictionary<string, CharacterRange[][]>? frames))
            return new Dictionary<string, CharacterRange[][]>(StringComparer.Ordinal);
        IReadOnlyList<CharacterRange> regions = setRegions.Regions;
        foreach ((string frame, CharacterRange[][] layers) in frames)
        {
            if (layers.Length == 0 || layers.Any(layer => layer.Any(region => !regions.Contains(region))))
                throw new InvalidDataException($"Enemy sheet manifest draws {set} {owner} frame {frame} in layers over regions its encounters do not upload.");
        }
        return new Dictionary<string, CharacterRange[][]>(frames, StringComparer.Ordinal);
    }

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
