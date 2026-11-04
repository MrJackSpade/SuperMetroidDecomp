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
    /// <summary>The manifest revision this build reads.</summary>
    public const int ManifestVersion = 7;

    /// <summary>The manifest naming each graphics set's sheet for each of its enemies.</summary>
    public const string ManifestFileName = "enemy-sheets.json";

    /// <summary>The installation directory holding the sheets and their manifest.</summary>
    public const string DirectoryName = "enemy-sheets";

    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "enemy-sheets-provenance.json";

    /// <summary>Composes and writes every enemy sheet and the set manifest.</summary>
    /// <param name="content">Installed stock content holding the tiles, compositions and colours the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    public static void Extract(string content, string directory, string sourceCartridgeSha256)
    {
        EnemySheetComposition composition = EnemySheetComposer.Compose(EnemySheetInputs.Load(content));
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ProvenanceFileName);
        foreach (KeyValuePair<string, EncodedSpriteSheet> sheet in composition.Sheets)
            store.Write(sheet.Key, sheet.Value);
        store.Finish(sourceCartridgeSha256);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
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
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        EnemySheetManifestDocument manifest = JsonSerializer.Deserialize<EnemySheetManifestDocument>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Enemy sheet manifest {manifestPath} is empty.");
        if (manifest.Version != ManifestVersion)
            throw new InvalidDataException($"Enemy sheet manifest {manifestPath} has revision {manifest.Version}, not {ManifestVersion}.");
        string[] names = [.. manifest.Sets.Values.SelectMany(members => members.Values)
            .Concat(manifest.Spawned.Values.SelectMany(bases => bases.Values.Select(entry => entry.Sheet))).Distinct(StringComparer.Ordinal)];
        // A player's sheet written before enemy frames were named for their meaning names them by their older keys (#1465).
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName, names.Length,
            EnemySheetLegacyFrameNames.Current);
        var loaded = names.ToDictionary(name => name, store.Read, StringComparer.Ordinal);
        var sheets = new Dictionary<EnemySheetKey, SpriteSheet>();
        var variations = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, FrameArtworkVariation[]>>();
        var spawnedVariations = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, FrameArtworkVariation[]>>();
        var rotPictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, string>>();
        var spawnedRotPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, string>>();
        // The sheets' frames are named from the stock enemy compositions installed beside them.
        var artwork = new Lazy<InstalledEnemyArtwork>(() => EnemyTileArtworkFiles.Load(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(stockDirectory))
                ?? throw new ArgumentException($"Stock directory {stockDirectory} has no installation above it.", nameof(stockDirectory)),
                GameInstallationLayout.EnemyTileDirectoryName), null));
        var setCorpses = new Dictionary<string, IReadOnlyList<CorpseRotSimulation>>(StringComparer.Ordinal);
        foreach ((string set, Dictionary<string, string> members) in manifest.Sets)
        {
            EnemyDefinitionId[] drawn = [.. members.Keys.Select(enemyName => Enum.TryParse(enemyName, out EnemyDefinitionId enemy) && Enum.IsDefined(enemy)
                ? enemy : throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown enemy {enemyName}."))];
            // The corpses that rot in the set's rooms are pictured for the rot steps the extraction plays from their art.
            IReadOnlyList<CorpseRotSimulation> corpses = CorpseRotSimulation.Of(artwork.Value.Enemies, artwork.Value.Bosses, drawn);
            setCorpses[set] = [.. corpses.Where(corpse => corpse.Kind is not null)];
            if (!RoomEnemyGraphicsSetLists.DrawnUnder.Any(known => known.Name == set))
                throw new InvalidDataException($"Enemy sheet manifest names unknown graphics set {set}.");
            foreach ((string enemyName, string name) in members)
            {
                var enemy = Enum.Parse<EnemyDefinitionId>(enemyName);
                SpriteSheet sheet = loaded[name];
                Dictionary<string, FrameArtworkVariation[]> frameVariations = FrameVariations(manifest.Variations, set, enemyName);
                Dictionary<string, string> frameRots = RotFrames(manifest.RotPictures, set, enemyName, setCorpses[set]);
                if (!sheet.HoldsExactly(EnemySheetComposer.FrameNames(artwork.Value.Enemies, enemy).SelectMany(frame => PictureNames(frame,
                        frameVariations, frameRots, setCorpses[set]))))
                    throw new InvalidDataException($"Enemy sheet {name} does not hold exactly the pictures of the frames {enemy} shows.");
                sheets.Add(new EnemySheetKey(set, enemy), sheet);
                if (frameVariations.Count > 0)
                    variations.Add(new EnemySheetKey(set, enemy), frameVariations);
                if (frameRots.Count > 0)
                    rotPictures.Add(new EnemySheetKey(set, enemy), frameRots);
            }
        }
        var spawned = new Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet>();
        foreach ((string set, Dictionary<string, SpawnedObjectSheetDocument> owners) in manifest.Spawned)
        {
            if (!setCorpses.ContainsKey(set))
                throw new InvalidDataException($"Enemy sheet manifest spawns objects under graphics set {set}, which it installs no sheets for.");
            foreach ((string ownerName, SpawnedObjectSheetDocument entry) in owners)
            {
                SpawnedObjectOwner owner = SuperMetroid.AssetExtraction.Enemies.SpawnedObjectOwnerNames.Parse(ownerName);
                EnemyDefinitionId[] spawners = [.. entry.Spawners.Select(name => Enum.TryParse(name, out EnemyDefinitionId enemy) && Enum.IsDefined(enemy)
                    ? enemy : throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown spawner {name}."))];
                SpriteSheet sheet = loaded[entry.Sheet];
                Dictionary<string, FrameArtworkVariation[]> frameVariations = FrameVariations(manifest.SpawnedVariations, set, ownerName);
                Dictionary<string, string> frameRots = RotFrames(manifest.SpawnedRotPictures, set, ownerName, setCorpses[set]);
                if (!sheet.HoldsExactly(SpawnedObjectSheetComposer.FrameNames(spawners, entry.Anywhere).SelectMany(frame => PictureNames(frame,
                        frameVariations, frameRots, setCorpses[set]))))
                    throw new InvalidDataException($"Spawned-object sheet {entry.Sheet} does not hold exactly the pictures of the objects its spawners spawn.");
                spawned.Add(new SpawnedObjectSheetKey(set, owner), new SpawnedObjectSheet(sheet));
                if (frameVariations.Count > 0)
                    spawnedVariations.Add(new SpawnedObjectSheetKey(set, owner), frameVariations);
                if (frameRots.Count > 0)
                    spawnedRotPictures.Add(new SpawnedObjectSheetKey(set, owner), frameRots);
            }
        }
        return new InstalledEnemySheets(new(sheets, variations, rotPictures), new(spawned, spawnedVariations, spawnedRotPictures));
    }

    /// <summary>
    /// The names of the pictures a sheet holds of <paramref name="frame"/>: for a frame showing a corpse's rot, the frame
    /// and its picture of each rot the corpse can show (#1406); for a frame encounter artwork sequences change, its picture
    /// of each combination of the steps they show (#1513); else the frame itself.
    /// </summary>
    private static IEnumerable<string> PictureNames(string frame, Dictionary<string, FrameArtworkVariation[]> frameVariations,
        Dictionary<string, string> frameRots, IReadOnlyList<CorpseRotSimulation> corpses) =>
        frameRots.TryGetValue(frame, out string? corpse)
            ? corpses.Single(simulation => simulation.Flesh == corpse).PictureNames(frame)
            : frameVariations.TryGetValue(frame, out FrameArtworkVariation[]? followed) ? FrameVariationPictures.Names(frame, followed) : [frame];

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
    /// How the manifest says each frame of <paramref name="owner"/>'s sheet under <paramref name="set"/> follows the
    /// encounter artwork sequences that change it (#1513), each sequence one the game plays and each step's picture one of
    /// an earlier or the same step.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame follows no sequence, an unknown one, or shows a picture of a later step.</exception>
    private static Dictionary<string, FrameArtworkVariation[]> FrameVariations(
        Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int?[]>>>> manifest, string set, string owner)
    {
        var variations = new Dictionary<string, FrameArtworkVariation[]>(StringComparer.Ordinal);
        if (!manifest.TryGetValue(set, out Dictionary<string, Dictionary<string, Dictionary<string, int?[]>>>? owners)
            || !owners.TryGetValue(owner, out Dictionary<string, Dictionary<string, int?[]>>? frames))
            return variations;
        foreach ((string frame, Dictionary<string, int?[]> sequences) in frames)
        {
            if (sequences.Count == 0)
                throw new InvalidDataException($"Enemy sheet manifest follows {set} {owner} frame {frame} through no artwork sequence.");
            variations.Add(frame, [.. sequences.OrderBy(entry => entry.Key, StringComparer.Ordinal).Select(entry =>
            {
                if (!Enum.TryParse(entry.Key, out EncounterArtworkSequence sequence) || !Enum.IsDefined(sequence))
                    throw new InvalidDataException($"Enemy sheet manifest follows {set} {owner} frame {frame} through unknown artwork sequence {entry.Key}.");
                if (entry.Value.Select((picture, step) => picture is int shown && (shown < 0 || shown > step)).Any(invalid => invalid))
                    throw new InvalidDataException($"Enemy sheet manifest shows {set} {owner} frame {frame} at a step of {entry.Key} by a picture of a later step.");
                return new FrameArtworkVariation(sequence, [.. entry.Value]);
            }).OrderBy(variation => variation.Sequence)]);
        }
        return variations;
    }

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}
