using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed pre-coloured enemy sheets (#1392), by the room graphics set that loads an enemy and the enemy: the
/// sheet shows the enemy's frames in the colours its colour set holds once that graphics set has loaded. Each set also has,
/// for each owner of the objects it spawns (enemy projectiles and room sprite objects), the pictures it draws them from: its
/// shared owner's and each spawner's, on the set's own sheet where they are particular to it and on a common sheet where
/// many sets draw them alike, each common picture held once (#1513).
/// </summary>
public sealed class InstalledEnemySheets : IEnemySheetSource
{
    /// <summary>Each set's sheet for each of its enemies.</summary>
    internal readonly Dictionary<EnemySheetKey, SpriteSheet> sheets;

    /// <summary>Each set's spawned-object pictures for each owner.</summary>
    internal readonly Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet> spawned;

    /// <summary>Each set's enemy sheets' frames its encounters' artwork sequences change, and how each follows them (#1513).</summary>
    internal readonly Dictionary<EnemySheetKey, IReadOnlyDictionary<string, FrameArtworkVariation[]>> variations;

    /// <summary>The same for each set's spawned-object sheets.</summary>
    internal readonly Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, FrameArtworkVariation[]>> spawnedVariations;

    /// <summary>Each set's enemy sheets' pictures that show a rotting corpse, and where the corpse lies in each (#1531).</summary>
    internal readonly Dictionary<EnemySheetKey, IReadOnlyDictionary<string, CorpsePictureLayout>> corpsePictures;

    /// <summary>The same for each set's spawned-object sheets.</summary>
    internal readonly Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, CorpsePictureLayout>> spawnedCorpsePictures;

    /// <summary>Each set's enemy sheets' frames' pictures that show nothing (#1513).</summary>
    internal readonly Dictionary<EnemySheetKey, IReadOnlySet<string>> blankPictures;

    /// <summary>The same for each set's spawned-object sheets.</summary>
    internal readonly Dictionary<SpawnedObjectSheetKey, IReadOnlySet<string>> spawnedBlankPictures;

    /// <summary>
    /// The sheets by graphics set name and enemy, and the spawned-object sheets by set name and owner, with how each of
    /// their frames an encounter's artwork sequence changes follows it and where a rotting corpse lies in each of their
    /// pictures that shows one.
    /// </summary>
    public InstalledEnemySheets(EnemySheetGroup<EnemySheetKey, SpriteSheet> enemies,
        EnemySheetGroup<SpawnedObjectSheetKey, SpawnedObjectSheet> spawnedObjects)
    {
        Ensure.NotNull(enemies);
        Ensure.NotNull(spawnedObjects);
        var (sheets, variations, corpsePictures, blankPictures) = enemies;
        var (spawned, spawnedVariations, spawnedCorpsePictures, spawnedBlankPictures) = spawnedObjects;
        this.sheets = new Dictionary<EnemySheetKey, SpriteSheet>(Ensure.NotNull(sheets));
        this.spawned = new Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet>(Ensure.NotNull(spawned));
        this.variations = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, FrameArtworkVariation[]>>(Ensure.NotNull(variations));
        this.spawnedVariations = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, FrameArtworkVariation[]>>(Ensure.NotNull(spawnedVariations));
        this.corpsePictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, CorpsePictureLayout>>(Ensure.NotNull(corpsePictures));
        this.spawnedCorpsePictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, CorpsePictureLayout>>(Ensure.NotNull(spawnedCorpsePictures));
        this.blankPictures = new Dictionary<EnemySheetKey, IReadOnlySet<string>>(Ensure.NotNull(blankPictures));
        this.spawnedBlankPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlySet<string>>(Ensure.NotNull(spawnedBlankPictures));
    }

    /// <inheritdoc/>
    public bool ShowsNothing(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string picture) =>
        blankPictures.TryGetValue(new EnemySheetKey(Ensure.NotNull(set).Name, enemy), out IReadOnlySet<string>? blank)
            && blank.Contains(picture);

    /// <inheritdoc/>
    public bool SpawnedShowsNothing(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string picture) =>
        spawnedBlankPictures.TryGetValue(new SpawnedObjectSheetKey(Ensure.NotNull(set).Name, owner), out IReadOnlySet<string>? blank)
            && blank.Contains(picture);

    /// <inheritdoc/>
    public CorpsePictureLayout? CorpseLayout(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string picture) =>
        corpsePictures.TryGetValue(new EnemySheetKey(Ensure.NotNull(set).Name, enemy), out IReadOnlyDictionary<string, CorpsePictureLayout>? pictures)
            && pictures.TryGetValue(picture, out CorpsePictureLayout? layout) ? layout : null;

    /// <inheritdoc/>
    public CorpsePictureLayout? SpawnedCorpseLayout(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string picture) =>
        spawnedCorpsePictures.TryGetValue(new SpawnedObjectSheetKey(Ensure.NotNull(set).Name, owner), out IReadOnlyDictionary<string, CorpsePictureLayout>? pictures)
            && pictures.TryGetValue(picture, out CorpsePictureLayout? layout) ? layout : null;

    /// <inheritdoc/>
    public IReadOnlyList<FrameArtworkVariation> Variations(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame) =>
        variations.TryGetValue(new EnemySheetKey(Ensure.NotNull(set).Name, enemy), out IReadOnlyDictionary<string, FrameArtworkVariation[]>? frames)
            && frames.TryGetValue(frame, out FrameArtworkVariation[]? followed) ? followed : [];

    /// <inheritdoc/>
    public IReadOnlyList<FrameArtworkVariation> SpawnedVariations(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string frame) =>
        spawnedVariations.TryGetValue(new SpawnedObjectSheetKey(Ensure.NotNull(set).Name, owner), out IReadOnlyDictionary<string, FrameArtworkVariation[]>? frames)
            && frames.TryGetValue(frame, out FrameArtworkVariation[]? followed) ? followed : [];

    /// <inheritdoc/>
    public SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy) => For(Ensure.NotNull(set).Name, enemy);

    /// <summary>The sheet of <paramref name="enemy"/> as graphics set <paramref name="set"/> loads it.</summary>
    /// <exception cref="InvalidDataException">The set does not load that enemy.</exception>
    public SpriteSheet For(string set, EnemyDefinitionId enemy) =>
        sheets.TryGetValue(new EnemySheetKey(set, enemy), out SpriteSheet? sheet) ? sheet
            : throw new InvalidDataException($"Enemy graphics set {set} installs no sheet for {enemy}.");

    /// <inheritdoc/>
    public SpawnedObjectSheet Spawned(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner) => Spawned(Ensure.NotNull(set).Name, owner);

    /// <summary>The sheet graphics set <paramref name="set"/> draws the objects <paramref name="owner"/> owns from.</summary>
    /// <exception cref="InvalidDataException">The set installs no sheet for that owner.</exception>
    public SpawnedObjectSheet Spawned(string set, SpawnedObjectOwner owner) =>
        spawned.TryGetValue(new SpawnedObjectSheetKey(set, owner), out SpawnedObjectSheet? sheet) ? sheet
            : throw new InvalidDataException($"Enemy graphics set {set} installs no spawned-object sheet for {owner}.");
}
