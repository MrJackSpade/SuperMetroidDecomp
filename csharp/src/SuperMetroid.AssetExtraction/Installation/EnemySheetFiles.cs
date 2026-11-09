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
    public const int ManifestVersion = 24;

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
        var changingEncounters = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, EncounterArtwork[]>>();
        var spawnedChangingEncounters = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, EncounterArtwork[]>>();
        var corpsePictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, CorpsePicturePlacement>>();
        var spawnedCorpsePictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, CorpsePicturePlacement>>();
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
                Dictionary<string, FrameLookChange[]> frameChanges = LookChanges(manifest.LookChanges, set, enemyName);
                HashSet<string> pictures = [.. EnemySheetComposer.FrameNames(artwork.Value.Enemies, enemy).SelectMany(frame => PictureNames(frame, frameChanges))];
                Dictionary<string, CorpsePicturePlacement> frameCorpses = CorpsePictures(manifest.CorpsePictures, set, enemyName, pictures);
                // The sheet holds each of the frames' finished pictures, or names it among those that show nothing (#1538).
                if (!sheet.HoldsExactly(pictures))
                    throw new InvalidDataException($"Enemy sheet {name} does not hold exactly the pictures of the frames {enemy} shows.");
                sheets.Add(new EnemySheetKey(set, enemy), sheet);
                if (frameChanges.Count > 0)
                    changingEncounters.Add(new EnemySheetKey(set, enemy), Encounters(frameChanges));
                if (frameCorpses.Count > 0)
                    corpsePictures.Add(new EnemySheetKey(set, enemy), frameCorpses);
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
                Dictionary<string, FrameLookChange[]> frameChanges = LookChanges(manifest.SpawnedLookChanges, set, ownerName);
                CommonSpawnedObjects[] common =
                    [.. entry.Common.Select(spawner => CommonSpawners.Named(spawner).Spawned)];
                HashSet<string> pictures = [.. SpawnedObjectSheetComposer.FrameNames(sheetOwner, spawners, sharedSpawners, common).SelectMany(frame => PictureNames(frame,
                    frameChanges))];
                Dictionary<string, CorpsePicturePlacement> frameCorpses = CorpsePictures(manifest.SpawnedCorpsePictures, set, ownerName, pictures);
                HashSet<string> blank = Empty(entry.Empty, $"{set} {ownerName}", pictures);
                string resolution = $"{entry.Sheet}/{string.Join("/", entry.CommonPictures.OrderBy(held => held.Key, StringComparer.Ordinal)
                    .Select(held => $"{held.Key}:{string.Join(",", held.Value)}"))}/{string.Join(",", blank.Order(StringComparer.Ordinal))}/{string.Join("/",
                    (entry.FrameSources ?? []).OrderBy(source => source.Key, StringComparer.Ordinal).Select(source =>
                        $"{source.Key}:{source.Value.PaletteBits}:{string.Join(",", source.Value.SheetInks)}:{string.Join(",", source.Value.Inks)}"))}";
                HashSet<string> shown = [.. pictures.Where(picture => !blank.Contains(picture))];
                if (!resolved.TryGetValue(resolution, out SpawnedObjectSheet? sheet))
                    resolved.Add(resolution, sheet = SpawnedPictures(entry, loaded, shown, blank, $"{set} {ownerName}"));
                if (!shown.SetEquals(sheet.frames.Keys) || !blank.SetEquals(sheet.empty))
                    throw new InvalidDataException($"Enemy sheet manifest does not draw exactly the pictures of the objects {set} {ownerName}'s spawners spawn.");
                spawned.Add(new SpawnedObjectSheetKey(set, owner), sheet);
                if (frameChanges.Count > 0)
                    spawnedChangingEncounters.Add(new SpawnedObjectSheetKey(set, owner), Encounters(frameChanges));
                if (frameCorpses.Count > 0)
                    spawnedCorpsePictures.Add(new SpawnedObjectSheetKey(set, owner), frameCorpses);
            }
        }
        return new InstalledEnemySheets(new(sheets, changingEncounters, corpsePictures),
            new(spawned, spawnedChangingEncounters, spawnedCorpsePictures));
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
        HashSet<string> empty, string owner)
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
        var pictures = new SpawnedObjectSheet(own, common, empty);
        // A picture held once is sourced as its first holder composed it; where this owner's set composed it elsewhere, the
        // set draws its own frame of the same rectangle, which keeps where the set composed it from (#1544).
        foreach ((string picture, InstalledFrameSource source) in entry.FrameSources ?? [])
        {
            if (!pictures.frames.TryGetValue(picture, out SpriteSheetFrame? held))
                throw new InvalidDataException($"Enemy sheet manifest names where {owner} composed picture {picture} from, but draws no such picture.");
            SpriteSheetFrame drawn = held with { };
            pictures.frames[picture] = drawn;
            InstalledPictureColours.RecordFrame(drawn, source);
        }
        return pictures;
    }

    /// <summary>
    /// The finished pictures the manifest says frames of <paramref name="owner"/>'s spawned objects show nothing in (#1538):
    /// each one of the frames' <paramref name="pictures"/>, which no sheet holds.
    /// </summary>
    /// <exception cref="InvalidDataException">The manifest names none, or a named picture is no picture of the owner's frames.</exception>
    private static HashSet<string> Empty(List<string> named, string owner, HashSet<string> pictures)
    {
        var empty = new HashSet<string>(StringComparer.Ordinal);
        foreach (string picture in named ?? throw new InvalidDataException($"Enemy sheet manifest names no pictures {owner} shows nothing in."))
        {
            if (!pictures.Contains(picture))
                throw new InvalidDataException($"Enemy sheet manifest says {owner} picture {picture} shows nothing, but it is no picture of its frames.");
            empty.Add(picture);
        }
        return empty;
    }

    /// <summary>The definitions <paramref name="names"/> names, in the manifest's set <paramref name="set"/>.</summary>
    /// <exception cref="InvalidDataException">A name is no enemy definition.</exception>
    private static EnemyDefinitionId[] Spawners(IEnumerable<string> names, string set) =>
        [.. names.Select(name => Enum.TryParse(name, out EnemyDefinitionId enemy) && Enum.IsDefined(enemy)
            ? enemy : throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown spawner {name}."))];

    /// <summary>
    /// The names of the finished pictures a sheet holds of <paramref name="frame"/>: for a frame encounters' looks change, its
    /// picture of each combination of the looks it is drawn in (#1538); else the frame itself.
    /// </summary>
    private static IEnumerable<string> PictureNames(string frame, Dictionary<string, FrameLookChange[]> frameChanges) =>
        frameChanges.TryGetValue(frame, out FrameLookChange[]? changes) ? FrameLookPictures.Names(frame, changes) : [frame];

    /// <summary>The encounters whose looks change each frame of <paramref name="frameChanges"/>, in encounter order.</summary>
    private static Dictionary<string, EncounterArtwork[]> Encounters(Dictionary<string, FrameLookChange[]> frameChanges) =>
        frameChanges.ToDictionary(frame => frame.Key, frame => frame.Value.Select(change => change.Encounter).ToArray(), StringComparer.Ordinal);

    /// <summary>
    /// Where the manifest says a rotting corpse lies in each picture of <paramref name="owner"/>'s sheet under
    /// <paramref name="set"/> that shows one (#1539), each one of the sheet's frames' <paramref name="pictures"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">A named picture is no picture of the sheet's frames.</exception>
    private static Dictionary<string, CorpsePicturePlacement> CorpsePictures(
        Dictionary<string, Dictionary<string, Dictionary<string, CorpsePictureDocument>>> manifest, string set, string owner, HashSet<string> pictures)
    {
        var placements = new Dictionary<string, CorpsePicturePlacement>(StringComparer.Ordinal);
        if (!manifest.TryGetValue(set, out Dictionary<string, Dictionary<string, CorpsePictureDocument>>? owners)
            || !owners.TryGetValue(owner, out Dictionary<string, CorpsePictureDocument>? named))
            return placements;
        foreach ((string picture, CorpsePictureDocument document) in named)
        {
            if (!pictures.Contains(picture))
                throw new InvalidDataException($"Enemy sheet manifest places a rotting corpse in {set} {owner} picture {picture}, which is no picture of its sheet's frames.");
            placements.Add(picture, document.Placement());
        }
        return placements;
    }

    /// <summary>
    /// How the manifest says encounters' looks change each frame of <paramref name="owner"/>'s sheet under
    /// <paramref name="set"/> (#1538), in encounter order: each encounter one whose looks gameplay shows, each with the names
    /// of its looks the frame is drawn in.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame is changed by no encounter, an unknown one, or in no look.</exception>
    private static Dictionary<string, FrameLookChange[]> LookChanges(
        Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string[]>>>> manifest, string set, string owner)
    {
        var changes = new Dictionary<string, FrameLookChange[]>(StringComparer.Ordinal);
        if (!manifest.TryGetValue(set, out Dictionary<string, Dictionary<string, Dictionary<string, string[]>>>? owners)
            || !owners.TryGetValue(owner, out Dictionary<string, Dictionary<string, string[]>>? frames))
            return changes;
        foreach ((string frame, Dictionary<string, string[]> encounters) in frames)
        {
            if (encounters.Count == 0)
                throw new InvalidDataException($"Enemy sheet manifest changes {set} {owner} frame {frame} by no encounter's looks.");
            changes.Add(frame, [.. encounters.Select(entry =>
            {
                if (!Enum.TryParse(entry.Key, out EncounterArtwork encounter) || !Enum.IsDefined(encounter))
                    throw new InvalidDataException($"Enemy sheet manifest changes {set} {owner} frame {frame} by the looks of unknown encounter {entry.Key}.");
                if (entry.Value is not { Length: > 0 } looks)
                    throw new InvalidDataException($"Enemy sheet manifest draws {set} {owner} frame {frame} in no look of {entry.Key}.");
                return new FrameLookChange(encounter, looks);
            }).OrderBy(change => change.Encounter)]);
        }
        return changes;
    }

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}
