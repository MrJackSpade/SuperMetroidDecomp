using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed pre-coloured enemy sheets (#1392), by the room graphics set that loads an enemy and the enemy: the
/// sheet shows the enemy's frames in the colours its colour set holds once that graphics set has loaded. Each set also has a
/// sheet for each owner of the objects it spawns (enemy projectiles and room sprite objects): its shared sheet and each
/// spawner's (#1513).
/// </summary>
public sealed class InstalledEnemySheets : IEnemySheetSource
{
    /// <summary>Each set's sheet for each of its enemies.</summary>
    internal readonly Dictionary<EnemySheetKey, SpriteSheet> sheets;

    /// <summary>Each set's spawned-object sheet for each owner.</summary>
    internal readonly Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet> spawned;

    /// <summary>Each set's enemy sheets' frames drawn over characters its encounters upload, and the layers they draw as.</summary>
    internal readonly Dictionary<EnemySheetKey, IReadOnlyDictionary<string, LookRegion[][]>> pictures;

    /// <summary>The same for each set's spawned-object sheets.</summary>
    internal readonly Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, LookRegion[][]>> spawnedPictures;

    /// <summary>Each set's enemy sheets' frames drawn over a rotting corpse's rows, and the kind of corpse whose rot each shows.</summary>
    internal readonly Dictionary<EnemySheetKey, IReadOnlyDictionary<string, string>> rotPictures;

    /// <summary>The same for each set's spawned-object sheets.</summary>
    internal readonly Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, string>> spawnedRotPictures;
    /// <summary>What covers each region of each set's characters, by set name.</summary>
    private readonly Dictionary<string, SetLookCoverage> coverage;

    /// <summary>
    /// The sheets by graphics set name and enemy, and the spawned-object sheets by set name and owner, with the regions
    /// each of their frames drawn over encounter looks draws as, the kind of corpse whose rot each of their frames drawn
    /// over a rotting corpse's rows shows, and the encounter looks the pictures show.
    /// </summary>
    public InstalledEnemySheets(EnemySheetGroup<EnemySheetKey, SpriteSheet> enemies,
        EnemySheetGroup<SpawnedObjectSheetKey, SpawnedObjectSheet> spawnedObjects,
        EncounterLooks looks, IReadOnlyDictionary<string, SetLookCoverage> coverage)
    {
        Ensure.NotNull(enemies);
        Ensure.NotNull(spawnedObjects);
        var (sheets, pictures, rotPictures) = enemies;
        var (spawned, spawnedPictures, spawnedRotPictures) = spawnedObjects;
        this.coverage = new Dictionary<string, SetLookCoverage>(Ensure.NotNull(coverage), StringComparer.Ordinal);
        Looks = Ensure.NotNull(looks);
        this.sheets = new Dictionary<EnemySheetKey, SpriteSheet>(Ensure.NotNull(sheets));
        this.spawned = new Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet>(Ensure.NotNull(spawned));
        this.pictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, LookRegion[][]>>(Ensure.NotNull(pictures));
        this.spawnedPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, LookRegion[][]>>(Ensure.NotNull(spawnedPictures));
        this.rotPictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, string>>(Ensure.NotNull(rotPictures));
        this.spawnedRotPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, string>>(Ensure.NotNull(spawnedRotPictures));
    }

    /// <inheritdoc/>
    public EncounterLooks Looks { get; }

    /// <inheritdoc/>
    public SetLookCoverage Coverage(RoomEnemyGraphicsSetDefinition set) =>
        coverage.TryGetValue(Ensure.NotNull(set).Name, out SetLookCoverage? covered) ? covered : SetLookCoverage.None;

    /// <inheritdoc/>
    public string? RotCorpse(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame) =>
        rotPictures.TryGetValue(new EnemySheetKey(Ensure.NotNull(set).Name, enemy), out IReadOnlyDictionary<string, string>? frames)
            && frames.TryGetValue(frame, out string? corpse) ? corpse : null;

    /// <inheritdoc/>
    public string? SpawnedRotCorpse(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string frame) =>
        spawnedRotPictures.TryGetValue(new SpawnedObjectSheetKey(Ensure.NotNull(set).Name, owner), out IReadOnlyDictionary<string, string>? frames)
            && frames.TryGetValue(frame, out string? corpse) ? corpse : null;

    /// <inheritdoc/>
    public IReadOnlyList<LookRegion[]> Layers(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame) =>
        pictures.TryGetValue(new EnemySheetKey(Ensure.NotNull(set).Name, enemy), out IReadOnlyDictionary<string, LookRegion[][]>? frames)
            && frames.TryGetValue(frame, out LookRegion[][]? layers) ? layers : [];

    /// <inheritdoc/>
    public IReadOnlyList<LookRegion[]> SpawnedLayers(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string frame) =>
        spawnedPictures.TryGetValue(new SpawnedObjectSheetKey(Ensure.NotNull(set).Name, owner), out IReadOnlyDictionary<string, LookRegion[][]>? frames)
            && frames.TryGetValue(frame, out LookRegion[][]? layers) ? layers : [];

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
