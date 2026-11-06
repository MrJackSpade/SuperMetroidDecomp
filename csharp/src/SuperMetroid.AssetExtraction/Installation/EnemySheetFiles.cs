using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured enemy sheets (#1392): each distinct sheet's image, palette map and frames, the manifest
/// naming each room graphics set's sheet for each of its enemies, and the provenance of their hashes. A player edits a
/// sheet's installed files in place.
/// </summary>
public static class EnemySheetFiles
{
    /// <summary>The manifest revision this build reads.</summary>
    public const int ManifestVersion = 17;

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
    /// Loads the set manifest and every sheet it names from the installed files, and checks each sheet holds exactly the
    /// frames its enemy's behaviour shows.
    /// </summary>
    /// <exception cref="InvalidDataException">The manifest, an image or a frame manifest is invalid.</exception>
    public static InstalledEnemySheets Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        EnemySheetManifestDocument manifest = JsonSerializer.Deserialize<EnemySheetManifestDocument>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Enemy sheet manifest {manifestPath} is empty.");
        if (manifest.Version != ManifestVersion)
            throw new InvalidDataException($"Enemy sheet manifest {manifestPath} has revision {manifest.Version}, not {ManifestVersion}.");
        // A spawned-object owner's pictures are on its own sheet and on the common sheets holding its common art (#1513).
        string[] names = [.. manifest.Sets.Values.SelectMany(members => members.Values)
            .Concat(manifest.Spawned.Values.SelectMany(bases => bases.Values.SelectMany(entry =>
                entry.Sheet is { } own ? entry.CommonPictures.Keys.Append(own) : entry.CommonPictures.Keys)))
            .Distinct(StringComparer.Ordinal)];
        SpriteSheetStore store = SpriteSheetStore.Open(directory, ProvenanceFileName, names.Length);
        var loaded = names.ToDictionary(name => name, store.Read, StringComparer.Ordinal);
        var sheets = new Dictionary<EnemySheetKey, SpriteSheet>();
        var variations = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, FrameArtworkVariation[]>>();
        var spawnedVariations = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, FrameArtworkVariation[]>>();
        var corpsePictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, CorpsePictureLayout>>();
        var spawnedCorpsePictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, CorpsePictureLayout>>();
        var blankPictures = new Dictionary<EnemySheetKey, IReadOnlySet<string>>();
        var spawnedBlankPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlySet<string>>();
        // The sheets' frames are named from the stock enemy compositions installed beside them.
        var artwork = new Lazy<InstalledEnemyArtwork>(() => EnemyTileArtworkFiles.Load(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(directory))
                ?? throw new ArgumentException($"Directory {directory} has no installation above it.", nameof(directory)),
                GameInstallationLayout.EnemyTileDirectoryName)));
        foreach ((string set, Dictionary<string, string> members) in manifest.Sets)
        {
            if (members.Keys.FirstOrDefault(enemyName => !Enum.TryParse(enemyName, out EnemyDefinitionId enemy) || !Enum.IsDefined(enemy)) is { } unknown)
                throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown enemy {unknown}.");
            if (!RoomEnemyGraphicsSetLists.DrawnUnder.Any(known => known.Name == set))
                throw new InvalidDataException($"Enemy sheet manifest names unknown graphics set {set}.");
            foreach ((string enemyName, string name) in members)
            {
                var enemy = Enum.Parse<EnemyDefinitionId>(enemyName);
                SpriteSheet sheet = loaded[name];
                Dictionary<string, FrameArtworkVariation[]> frameVariations = FrameVariations(manifest.Variations, set, enemyName);
                HashSet<string> pictures = [.. EnemySheetComposer.FrameNames(artwork.Value.Enemies, enemy).SelectMany(frame => PictureNames(frame, frameVariations))];
                Dictionary<string, CorpsePictureLayout> frameCorpses = CorpsePictures(manifest.CorpsePictures, set, enemyName, pictures);
                HashSet<string> blank = Blank(manifest.BlankPictures, set, enemyName, pictures);
                if (!sheet.HoldsExactly(pictures.Where(picture => !blank.Contains(picture))))
                    throw new InvalidDataException($"Enemy sheet {name} does not hold exactly the pictures of the frames {enemy} shows.");
                sheets.Add(new EnemySheetKey(set, enemy), sheet);
                if (frameVariations.Count > 0)
                    variations.Add(new EnemySheetKey(set, enemy), frameVariations);
                if (frameCorpses.Count > 0)
                    corpsePictures.Add(new EnemySheetKey(set, enemy), frameCorpses);
                if (blank.Count > 0)
                    blankPictures.Add(new EnemySheetKey(set, enemy), blank);
            }
        }
        var spawned = new Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet>();
        foreach ((string set, Dictionary<string, SpawnedObjectSheetDocument> owners) in manifest.Spawned)
        {
            if (!manifest.Sets.ContainsKey(set))
                throw new InvalidDataException($"Enemy sheet manifest spawns objects under graphics set {set}, which it installs no sheets for.");
            // Owners the set draws from one sheet's pictures share one resolution of them.
            var resolved = new Dictionary<string, SpawnedObjectSheet>(StringComparer.Ordinal);
            foreach ((string ownerName, SpawnedObjectSheetDocument entry) in owners)
            {
                SpawnedObjectOwner owner = SuperMetroid.AssetExtraction.Enemies.SpawnedObjectOwnerNames.Parse(ownerName);
                EnemyDefinitionId[] spawners = Spawners(entry.Spawners, set);
                EnemyDefinitionId[] sharedSpawners = Spawners(entry.SharedSpawners, set);
                SpawnedObjectOwner sheetOwner = SuperMetroid.AssetExtraction.Enemies.SpawnedObjectOwnerNames.Parse(entry.Owner);
                Dictionary<string, FrameArtworkVariation[]> frameVariations = FrameVariations(manifest.SpawnedVariations, set, ownerName);
                CommonSpawnedObjects[] common =
                    [.. entry.Common.Select(spawner => CommonSpawners.Named(spawner).Spawned)];
                HashSet<string> pictures = [.. SpawnedObjectSheetComposer.FrameNames(sheetOwner, spawners, sharedSpawners, common).SelectMany(frame => PictureNames(frame,
                    frameVariations))];
                Dictionary<string, CorpsePictureLayout> frameCorpses = CorpsePictures(manifest.SpawnedCorpsePictures, set, ownerName, pictures);
                HashSet<string> blank = Blank(manifest.SpawnedBlankPictures, set, ownerName, pictures);
                string resolution = $"{entry.Sheet}/{string.Join("/", entry.CommonPictures.OrderBy(held => held.Key, StringComparer.Ordinal)
                    .Select(held => $"{held.Key}:{string.Join(",", held.Value)}"))}";
                HashSet<string> shown = [.. pictures.Where(picture => !blank.Contains(picture))];
                if (!resolved.TryGetValue(resolution, out SpawnedObjectSheet? sheet))
                    resolved.Add(resolution, sheet = SpawnedPictures(entry, loaded, shown, $"{set} {ownerName}"));
                if (!shown.SetEquals(sheet.frames.Keys))
                    throw new InvalidDataException($"Enemy sheet manifest does not draw exactly the pictures of the objects {set} {ownerName}'s spawners spawn.");
                spawned.Add(new SpawnedObjectSheetKey(set, owner), sheet);
                if (frameVariations.Count > 0)
                    spawnedVariations.Add(new SpawnedObjectSheetKey(set, owner), frameVariations);
                if (frameCorpses.Count > 0)
                    spawnedCorpsePictures.Add(new SpawnedObjectSheetKey(set, owner), frameCorpses);
                if (blank.Count > 0)
                    spawnedBlankPictures.Add(new SpawnedObjectSheetKey(set, owner), blank);
            }
        }
        return new InstalledEnemySheets(new(sheets, variations, corpsePictures, blankPictures),
            new(spawned, spawnedVariations, spawnedCorpsePictures, spawnedBlankPictures));
    }

    /// <summary>
    /// The pictures <paramref name="entry"/> says a graphics set draws one owner's spawned objects from (#1513): those
    /// particular to the set from its own sheet, and each common one from the common sheet the entry names for it, together
    /// exactly <paramref name="shown"/>, the pictures of the owner's frames that show something.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The entry draws a picture its frames do not show, draws one from two common sheets, or a sheet does not hold the
    /// pictures the entry draws from it.
    /// </exception>
    private static SpawnedObjectSheet SpawnedPictures(SpawnedObjectSheetDocument entry, Dictionary<string, SpriteSheet> loaded, HashSet<string> shown,
        string owner)
    {
        var common = new Dictionary<string, SpriteSheet>(StringComparer.Ordinal);
        foreach ((string sheetName, List<string> held) in entry.CommonPictures)
        {
            SpriteSheet sheet = loaded[sheetName];
            foreach (string picture in held)
            {
                if (!shown.Contains(picture))
                    throw new InvalidDataException($"Enemy sheet manifest draws {owner} picture {picture} from common sheet {sheetName}, but no frame of its objects shows it.");
                if (!common.TryAdd(picture, sheet))
                    throw new InvalidDataException($"Enemy sheet manifest draws {owner} picture {picture} from two common sheets.");
            }
        }
        string[] particular = [.. shown.Where(picture => !common.ContainsKey(picture))];
        SpriteSheet? own = entry.Sheet is { } name ? loaded[name] : null;
        if (own is null ? particular.Length > 0 : !own.HoldsExactly(particular))
            throw new InvalidDataException($"Spawned-object sheet {entry.Sheet ?? "(none)"} does not hold exactly the pictures particular to {owner}'s objects.");
        return new SpawnedObjectSheet(own, common);
    }

    /// <summary>
    /// The pictures the manifest says frames of <paramref name="owner"/>'s sheet under <paramref name="set"/> show nothing
    /// in (#1513): each one of the sheet's frames' <paramref name="pictures"/>, in which every character the frame reads is
    /// blank, so the sheet holds no such picture.
    /// </summary>
    /// <exception cref="InvalidDataException">A named picture is no picture of the sheet's frames.</exception>
    private static HashSet<string> Blank(Dictionary<string, Dictionary<string, string[]>> manifest, string set, string owner,
        HashSet<string> pictures)
    {
        var blank = new HashSet<string>(StringComparer.Ordinal);
        if (!manifest.TryGetValue(set, out Dictionary<string, string[]>? owners) || !owners.TryGetValue(owner, out string[]? named))
            return blank;
        foreach (string picture in named)
        {
            if (!pictures.Contains(picture))
                throw new InvalidDataException($"Enemy sheet manifest says {set} {owner} picture {picture} shows nothing, but it is no picture of its frames.");
            blank.Add(picture);
        }
        return blank;
    }

    /// <summary>The definitions <paramref name="names"/> names, in the manifest's set <paramref name="set"/>.</summary>
    /// <exception cref="InvalidDataException">A name is no enemy definition.</exception>
    private static EnemyDefinitionId[] Spawners(IEnumerable<string> names, string set) =>
        [.. names.Select(name => Enum.TryParse(name, out EnemyDefinitionId enemy) && Enum.IsDefined(enemy)
            ? enemy : throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown spawner {name}."))];

    /// <summary>
    /// The names of the pictures a sheet holds of <paramref name="frame"/>: for a frame encounter artwork sequences change,
    /// its picture of each combination of the steps they show (#1513); else the frame itself.
    /// </summary>
    private static IEnumerable<string> PictureNames(string frame, Dictionary<string, FrameArtworkVariation[]> frameVariations) =>
        frameVariations.TryGetValue(frame, out FrameArtworkVariation[]? followed) ? FrameVariationPictures.Names(frame, followed) : [frame];

    /// <summary>
    /// Where the manifest says a rotting corpse lies in each picture of <paramref name="owner"/>'s sheet under
    /// <paramref name="set"/> that shows one (#1531), each one of the sheet's frames' <paramref name="pictures"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">A named picture is no picture of the sheet's frames, or its layout spells no columns.</exception>
    private static Dictionary<string, CorpsePictureLayout> CorpsePictures(
        Dictionary<string, Dictionary<string, Dictionary<string, CorpsePictureDocument>>> manifest, string set, string owner, HashSet<string> pictures)
    {
        var layouts = new Dictionary<string, CorpsePictureLayout>(StringComparer.Ordinal);
        if (!manifest.TryGetValue(set, out Dictionary<string, Dictionary<string, CorpsePictureDocument>>? owners)
            || !owners.TryGetValue(owner, out Dictionary<string, CorpsePictureDocument>? named))
            return layouts;
        foreach ((string picture, CorpsePictureDocument document) in named)
        {
            if (!pictures.Contains(picture))
                throw new InvalidDataException($"Enemy sheet manifest places a rotting corpse in {set} {owner} picture {picture}, which is no picture of its sheet's frames.");
            layouts.Add(picture, document.Layout());
        }
        return layouts;
    }

    /// <summary>
    /// How the manifest says each frame of <paramref name="owner"/>'s sheet under <paramref name="set"/> follows the
    /// encounter artwork sequences that change it (#1513), each sequence one the game plays and each step's picture one of
    /// its steps: a frame whose artwork a run of pages completes shows that step's picture at every step it is drawn at.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame follows no sequence, an unknown one, or shows a picture of a step the sequence does not have.</exception>
    private static Dictionary<string, FrameArtworkVariation[]> FrameVariations(
        Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>>>> manifest, string set, string owner)
    {
        var variations = new Dictionary<string, FrameArtworkVariation[]>(StringComparer.Ordinal);
        if (!manifest.TryGetValue(set, out Dictionary<string, Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>>>? owners)
            || !owners.TryGetValue(owner, out Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>>? frames))
            return variations;
        foreach ((string frame, Dictionary<string, FrameArtworkVariationDocument> sequences) in frames)
        {
            if (sequences.Count == 0)
                throw new InvalidDataException($"Enemy sheet manifest follows {set} {owner} frame {frame} through no artwork sequence.");
            variations.Add(frame, [.. sequences.OrderBy(entry => entry.Key, StringComparer.Ordinal).Select(entry =>
            {
                if (!Enum.TryParse(entry.Key, out EncounterArtworkSequence sequence) || !Enum.IsDefined(sequence))
                    throw new InvalidDataException($"Enemy sheet manifest follows {set} {owner} frame {frame} through unknown artwork sequence {entry.Key}.");
                FrameArtworkVariation variation = entry.Value.Variation(sequence);
                if (variation.Pictures.Any(picture => picture is int shown && (uint)shown >= (uint)variation.Pictures.Length))
                    throw new InvalidDataException($"Enemy sheet manifest shows {set} {owner} frame {frame} at a step of {entry.Key} by a picture of a step the sequence does not have.");
                return variation;
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
