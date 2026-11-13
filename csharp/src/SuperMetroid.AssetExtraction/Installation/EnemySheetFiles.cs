using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured enemy sheets (#1392, #1599, #1633, #1635): each distinct sheet's manifest, its pictures held in
/// the installation's picture store, the manifest
/// naming every variant of each enemy and of each owner's spawned objects and the variants each population draws, which
/// names the sheets (#1693). A player edits a sheet's installed files in place.
/// </summary>
public static class EnemySheetFiles
{
    /// <summary>
    /// The manifest revision this build reads: 26 no longer records where a variant composed a picture held once from, as
    /// nothing installed names a palette index (#1633); 27 pictures the Ceres doors' rotating overlay once, over the
    /// overlay's own characters, as the only door that draws it shows them before its first draw (#1641); 28 installs no
    /// look an enemy's draws never select, so variants the player cannot tell apart, as the Chozo statue's of one site and
    /// the elevator's, are one variant drawing one sheet (#1643); 29 installs no frame no animation shows, as the room sprite
    /// objects nothing spawns, and keys every projectile frame by what it shows (#1623); 30 keys every enemy frame by the
    /// animation segment or table key that first shows it (#1623); 31 keys the frames that rule misnamed by what they show
    /// (#1623); 32 keys the Wrecked Ship Chozo statue's walk frames by its walk (#1623); 33 installs the burst a shot or
    /// bomb shows at an enemy it hurts on every set's shared spawned-object sheet (#1713); 34 no longer records the palette row a
    /// spawned-object variant's pictures were coloured in, which only the composition reads (#1728); 35 installs no Stoke
    /// and lists no spawned objects for an enemy no room places, the Reflec, the second dead Sidehopper and the Tourian
    /// statue ghost (#1686).
    /// </summary>
    public const int ManifestVersion = 35;

    /// <summary>The manifest naming each variant's sheets and the variants each population draws.</summary>
    public const string ManifestFileName = "enemy-sheets.json";

    /// <summary>The installation directory holding the sheets and their manifest.</summary>
    public const string DirectoryName = "enemy-sheets";

    /// <summary>
    /// Composes every enemy sheet and writes the variant manifest; the sheets are returned for the picture store to write
    /// (<see cref="PictureStoreFiles"/>, #1635).
    /// </summary>
    /// <param name="content">Installed stock content holding the tiles, compositions and colours the sheets compose from.</param>
    /// <param name="directory">The family's directory, created if absent.</param>
    internal static InstalledSheetFamily Extract(string content, string directory)
    {
        EnemySheetComposition composition = EnemySheetComposer.Compose(EnemySheetInputs.Load(content));
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(composition.Manifest, JsonOptions));
        return new InstalledSheetFamily(directory,
            [.. composition.Sheets.Select(sheet => new NamedInstalledSheet(sheet.Key, sheet.Value))]);
    }

    /// <summary>
    /// Loads the variant manifest and every sheet it names from the installed files, and checks each enemy variant's sheet
    /// holds exactly the frames its enemy's behaviour shows and each spawned-object variant exactly the pictures its owner's
    /// objects show (#1599).
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
        // A spawned-object variant's pictures are on its own sheet and on the common sheets holding its common art (#1513).
        string[] names = [.. manifest.Variants.Values.Select(variant => variant.Sheet)
            .Concat(manifest.SpawnedVariants.Values.SelectMany(entry =>
                entry.Sheet is { } own ? entry.CommonPictures.Keys.Append(own) : entry.CommonPictures.Keys))
            .Distinct(StringComparer.Ordinal)];
        SpriteSheetStore store = SpriteSheetStore.Open(directory);
        var loaded = names.ToDictionary(name => name, store.Read, StringComparer.Ordinal);
        // The sheets' frames are named from the display bindings installed beside them, without a character (#1636) or a
        // composition (#1638).
        var bindings = new Lazy<InstalledEnemyFrameDisplays>(() => EnemyTileArtworkFiles.FrameDisplays(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(directory))
                ?? throw new ArgumentException($"Directory {directory} has no installation above it.", nameof(directory)),
                GameInstallationLayout.EnemyTileDirectoryName)));
        var variants = new Dictionary<string, EnemyArtworkVariant<SpriteSheet>>(StringComparer.Ordinal);
        foreach ((string name, EnemyVariantDocument entry) in manifest.Variants)
        {
            EnemyDefinitionId enemy = Enemy(entry.Enemy, $"variant {name}");
            SpriteSheet sheet = loaded[entry.Sheet];
            Dictionary<string, FrameLookChange[]> frameChanges = LookChanges(entry.LookChanges, name);
            HashSet<string> pictures = [.. bindings.Value.FrameNames(enemy).SelectMany(frame => PictureNames(frame, frameChanges))];
            // The sheet holds each of the frames' finished pictures, or names it among those that show nothing (#1538).
            if (!sheet.HoldsExactly(pictures))
                throw new InvalidDataException($"Enemy sheet {entry.Sheet} of variant {name} does not hold exactly the pictures of the frames {enemy} shows.");
            variants.Add(name, new EnemyArtworkVariant<SpriteSheet>(name, sheet, Encounters(frameChanges), CorpsePictures(entry.CorpsePictures, name, pictures)));
        }
        var spawnedVariants = new Dictionary<string, EnemyArtworkVariant<SpawnedObjectSheet>>(StringComparer.Ordinal);
        // Variants that draw one owner's pictures alike share one resolution of them.
        var resolved = new Dictionary<string, SpawnedObjectSheet>(StringComparer.Ordinal);
        foreach ((string name, SpawnedObjectSheetDocument entry) in manifest.SpawnedVariants)
        {
            EnemyDefinitionId[] spawners = [.. entry.Spawners.Select(spawner => Enemy(spawner, $"spawned-object variant {name}'s spawners"))];
            EnemyDefinitionId[] sharedSpawners = [.. entry.SharedSpawners.Select(spawner => Enemy(spawner, $"spawned-object variant {name}'s shared spawners"))];
            SpawnedObjectOwner sheetOwner = SuperMetroid.AssetExtraction.Enemies.SpawnedObjectOwnerNames.Parse(entry.Owner);
            Dictionary<string, FrameLookChange[]> frameChanges = LookChanges(entry.LookChanges, name);
            CommonSpawnedObjects[] common =
                [.. entry.Common.Select(spawner => CommonSpawners.Named(spawner).Spawned)];
            HashSet<string> pictures = [.. SpawnedObjectSheetComposer.FrameNames(sheetOwner, spawners, sharedSpawners, common).SelectMany(frame => PictureNames(frame,
                frameChanges))];
            HashSet<string> blank = Empty(entry.Empty, $"spawned-object variant {name}", pictures);
            string resolution = $"{entry.Sheet}/{string.Join("/", entry.CommonPictures.OrderBy(held => held.Key, StringComparer.Ordinal)
                .Select(held => $"{held.Key}:{string.Join(",", held.Value)}"))}/{string.Join(",", blank.Order(StringComparer.Ordinal))}";
            HashSet<string> shown = [.. pictures.Where(picture => !blank.Contains(picture))];
            if (!resolved.TryGetValue(resolution, out SpawnedObjectSheet? sheet))
                resolved.Add(resolution, sheet = SpawnedPictures(entry, loaded, shown, blank, $"spawned-object variant {name}"));
            if (!shown.SetEquals(sheet.frames.Keys) || !blank.SetEquals(sheet.empty))
                throw new InvalidDataException($"Enemy sheet manifest does not draw exactly the pictures of the objects spawned-object variant {name}'s spawners spawn.");
            spawnedVariants.Add(name, new EnemyArtworkVariant<SpawnedObjectSheet>(name, sheet, Encounters(frameChanges),
                CorpsePictures(entry.CorpsePictures, name, pictures)));
        }
        PopulationArtwork standard = Artwork(manifest.Standard, "standard", manifest.Variants, variants, spawnedVariants);
        var populations = new Dictionary<string, PopulationArtwork>(StringComparer.Ordinal);
        foreach ((string population, PopulationArtworkDocument entry) in manifest.Populations)
        {
            if (!RoomEnemyPopulationDefinitions.Definitions.Any(known => known.Name == population))
                throw new InvalidDataException($"Enemy sheet manifest names unknown population {population}.");
            populations.Add(population, Artwork(entry, population, manifest.Variants, variants, spawnedVariants));
        }
        return new InstalledEnemySheets(variants, spawnedVariants, standard, populations);
    }

    /// <summary>
    /// The artwork <paramref name="entry"/> says population <paramref name="population"/> draws: each enemy in the variant
    /// it names, which <paramref name="documents"/> says shows that enemy, and each owner's spawned objects in the variant it names.
    /// </summary>
    /// <exception cref="InvalidDataException">The entry names an unknown enemy, owner or variant, or a variant of another enemy.</exception>
    private static PopulationArtwork Artwork(PopulationArtworkDocument entry, string population, Dictionary<string, EnemyVariantDocument> documents,
        Dictionary<string, EnemyArtworkVariant<SpriteSheet>> variants, Dictionary<string, EnemyArtworkVariant<SpawnedObjectSheet>> spawnedVariants)
    {
        var enemies = new Dictionary<EnemyDefinitionId, EnemyArtworkVariant<SpriteSheet>>();
        foreach ((string enemyName, string variantName) in entry.Enemies)
        {
            EnemyDefinitionId enemy = Enemy(enemyName, $"population {population}");
            if (!variants.TryGetValue(variantName, out EnemyArtworkVariant<SpriteSheet>? variant))
                throw new InvalidDataException($"Enemy sheet manifest population {population} draws {enemy} in unknown variant {variantName}.");
            if (documents[variantName].Enemy != enemyName)
                throw new InvalidDataException($"Enemy sheet manifest population {population} draws {enemy} in variant {variantName}, a variant of {documents[variantName].Enemy}.");
            enemies.Add(enemy, variant);
        }
        var spawned = new Dictionary<SpawnedObjectOwner, EnemyArtworkVariant<SpawnedObjectSheet>>();
        foreach ((string ownerName, string variantName) in entry.Spawned)
        {
            SpawnedObjectOwner owner = SuperMetroid.AssetExtraction.Enemies.SpawnedObjectOwnerNames.Parse(ownerName);
            if (!spawnedVariants.TryGetValue(variantName, out EnemyArtworkVariant<SpawnedObjectSheet>? variant))
                throw new InvalidDataException($"Enemy sheet manifest population {population} draws {ownerName}'s spawned objects in unknown variant {variantName}.");
            spawned.Add(owner, variant);
        }
        return new PopulationArtwork(enemies, spawned);
    }

    /// <summary>The enemy definition <paramref name="name"/> names, in the manifest's <paramref name="entry"/>.</summary>
    /// <exception cref="InvalidDataException">The name is no enemy definition.</exception>
    private static EnemyDefinitionId Enemy(string name, string entry) =>
        Enum.TryParse(name, out EnemyDefinitionId enemy) && Enum.IsDefined(enemy)
            ? enemy : throw new InvalidDataException($"Enemy sheet manifest {entry} names unknown enemy {name}.");

    /// <summary>
    /// The pictures <paramref name="entry"/> says a variant draws one owner's spawned objects from (#1513): those
    /// particular to the variant from its own sheet, and each common one from the common sheet the entry names for it, together
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
        return new SpawnedObjectSheet(own, common, empty);
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
    /// Where the manifest says a rotting corpse lies in each picture of variant <paramref name="variant"/>'s sheet that shows
    /// one (#1539), each one of the sheet's frames' <paramref name="pictures"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">A named picture is no picture of the sheet's frames.</exception>
    private static Dictionary<string, CorpsePicturePlacement> CorpsePictures(Dictionary<string, CorpsePictureDocument>? named, string variant,
        HashSet<string> pictures)
    {
        var placements = new Dictionary<string, CorpsePicturePlacement>(StringComparer.Ordinal);
        foreach ((string picture, CorpsePictureDocument document) in named ?? [])
        {
            if (!pictures.Contains(picture))
                throw new InvalidDataException($"Enemy sheet manifest places a rotting corpse in variant {variant}'s picture {picture}, which is no picture of its sheet's frames.");
            placements.Add(picture, document.Placement());
        }
        return placements;
    }

    /// <summary>
    /// How the manifest says encounters' looks change each frame of variant <paramref name="variant"/>'s sheet (#1538), in
    /// encounter order: each encounter one whose looks gameplay shows, each with the names of its looks the frame is drawn in.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame is changed by no encounter, an unknown one, or in no look.</exception>
    private static Dictionary<string, FrameLookChange[]> LookChanges(Dictionary<string, Dictionary<string, string[]>>? frames, string variant)
    {
        var changes = new Dictionary<string, FrameLookChange[]>(StringComparer.Ordinal);
        foreach ((string frame, Dictionary<string, string[]> encounters) in frames ?? [])
        {
            if (encounters.Count == 0)
                throw new InvalidDataException($"Enemy sheet manifest changes variant {variant}'s frame {frame} by no encounter's looks.");
            changes.Add(frame, [.. encounters.Select(entry =>
            {
                if (!Enum.TryParse(entry.Key, out EncounterArtwork encounter) || !Enum.IsDefined(encounter))
                    throw new InvalidDataException($"Enemy sheet manifest changes variant {variant}'s frame {frame} by the looks of unknown encounter {entry.Key}.");
                if (entry.Value is not { Length: > 0 } looks)
                    throw new InvalidDataException($"Enemy sheet manifest draws variant {variant}'s frame {frame} in no look of {entry.Key}.");
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
