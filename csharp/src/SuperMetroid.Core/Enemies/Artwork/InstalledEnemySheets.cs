using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed pre-coloured enemy sheets (#1392), by the room graphics set that loads an enemy and the enemy: the
/// sheet shows the enemy's frames in the colours its palette row holds once that set has loaded. Each set also has a
/// sheet for each tile base its spawned objects (enemy projectiles and room sprite objects) draw at.
/// </summary>
public sealed class InstalledEnemySheets : IEnemySheetSource
{
    /// <summary>Each set's sheet for each of its enemies.</summary>
    private readonly Dictionary<EnemySheetKey, SpriteSheet> sheets;

    /// <summary>Each set's spawned-object sheet for each tile base.</summary>
    private readonly Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet> spawned;

    /// <summary>Each set's enemy sheets' frames drawn over characters its encounters upload, and the layers they draw as.</summary>
    private readonly Dictionary<EnemySheetKey, IReadOnlyDictionary<string, CharacterRegion[][]>> pictures;

    /// <summary>The same for each set's spawned-object sheets.</summary>
    private readonly Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, CharacterRegion[][]>> spawnedPictures;

    /// <summary>Each set's enemy sheets' frames drawn over a rotting corpse's rows, and the kind of corpse whose rot each shows.</summary>
    private readonly Dictionary<EnemySheetKey, IReadOnlyDictionary<string, string>> rotPictures;

    /// <summary>The same for each set's spawned-object sheets.</summary>
    private readonly Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, string>> spawnedRotPictures;
    /// <summary>What covers each region of each set's characters, by set name.</summary>
    private readonly Dictionary<string, SetCharacterCoverage> coverage;

    /// <summary>
    /// The sheets by graphics set name and enemy, and the spawned-object sheets by set name and tile base, with the regions
    /// each of their frames drawn over uploaded characters draws as, the kind of corpse whose rot each of their frames drawn
    /// over a rotting corpse's rows shows, and the uploads the pictures show.
    /// </summary>
    public InstalledEnemySheets(IReadOnlyDictionary<EnemySheetKey, SpriteSheet> sheets,
        IReadOnlyDictionary<SpawnedObjectSheetKey, SpawnedObjectSheet> spawned,
        IReadOnlyDictionary<EnemySheetKey, IReadOnlyDictionary<string, CharacterRegion[][]>> pictures,
        IReadOnlyDictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, CharacterRegion[][]>> spawnedPictures,
        IReadOnlyDictionary<EnemySheetKey, IReadOnlyDictionary<string, string>> rotPictures,
        IReadOnlyDictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, string>> spawnedRotPictures,
        RuntimeCharacterUploadCatalog uploads, IReadOnlyDictionary<string, SetCharacterCoverage> coverage)
    {
        this.coverage = new Dictionary<string, SetCharacterCoverage>(Ensure.NotNull(coverage), StringComparer.Ordinal);
        Uploads = Ensure.NotNull(uploads);
        this.sheets = new Dictionary<EnemySheetKey, SpriteSheet>(Ensure.NotNull(sheets));
        this.spawned = new Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet>(Ensure.NotNull(spawned));
        this.pictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, CharacterRegion[][]>>(Ensure.NotNull(pictures));
        this.spawnedPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, CharacterRegion[][]>>(Ensure.NotNull(spawnedPictures));
        this.rotPictures = new Dictionary<EnemySheetKey, IReadOnlyDictionary<string, string>>(Ensure.NotNull(rotPictures));
        this.spawnedRotPictures = new Dictionary<SpawnedObjectSheetKey, IReadOnlyDictionary<string, string>>(Ensure.NotNull(spawnedRotPictures));
    }

    /// <inheritdoc/>
    public RuntimeCharacterUploadCatalog Uploads { get; }

    /// <inheritdoc/>
    public SetCharacterCoverage Coverage(RoomEnemyGraphicsSetDefinition set) =>
        coverage.TryGetValue(Ensure.NotNull(set).Name, out SetCharacterCoverage? covered) ? covered : SetCharacterCoverage.None;

    /// <inheritdoc/>
    public string? RotCorpse(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame) =>
        rotPictures.TryGetValue(new EnemySheetKey(Ensure.NotNull(set).Name, enemy), out IReadOnlyDictionary<string, string>? frames)
            && frames.TryGetValue(frame, out string? corpse) ? corpse : null;

    /// <inheritdoc/>
    public string? SpawnedRotCorpse(RoomEnemyGraphicsSetDefinition set, int tileBase, string frame) =>
        spawnedRotPictures.TryGetValue(new SpawnedObjectSheetKey(Ensure.NotNull(set).Name, tileBase), out IReadOnlyDictionary<string, string>? frames)
            && frames.TryGetValue(frame, out string? corpse) ? corpse : null;

    /// <inheritdoc/>
    public IReadOnlyList<CharacterRegion[]> Layers(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame) =>
        pictures.TryGetValue(new EnemySheetKey(Ensure.NotNull(set).Name, enemy), out IReadOnlyDictionary<string, CharacterRegion[][]>? frames)
            && frames.TryGetValue(frame, out CharacterRegion[][]? layers) ? layers : [];

    /// <inheritdoc/>
    public IReadOnlyList<CharacterRegion[]> SpawnedLayers(RoomEnemyGraphicsSetDefinition set, int tileBase, string frame) =>
        spawnedPictures.TryGetValue(new SpawnedObjectSheetKey(Ensure.NotNull(set).Name, tileBase), out IReadOnlyDictionary<string, CharacterRegion[][]>? frames)
            && frames.TryGetValue(frame, out CharacterRegion[][]? layers) ? layers : [];

    /// <inheritdoc/>
    public SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy) => For(Ensure.NotNull(set).Name, enemy);

    /// <summary>The sheet of <paramref name="enemy"/> as graphics set <paramref name="set"/> loads it.</summary>
    /// <exception cref="InvalidDataException">The set does not load that enemy.</exception>
    public SpriteSheet For(string set, EnemyDefinitionId enemy) =>
        sheets.TryGetValue(new EnemySheetKey(set, enemy), out SpriteSheet? sheet) ? sheet
            : throw new InvalidDataException($"Enemy graphics set {set} installs no sheet for {enemy}.");

    /// <inheritdoc/>
    public SpawnedObjectSheet Spawned(RoomEnemyGraphicsSetDefinition set, int tileBase) => Spawned(Ensure.NotNull(set).Name, tileBase);

    /// <summary>The sheet graphics set <paramref name="set"/> draws the objects spawned at <paramref name="tileBase"/> from.</summary>
    /// <exception cref="InvalidDataException">No spawner the set loads draws at that base.</exception>
    public SpawnedObjectSheet Spawned(string set, int tileBase) =>
        spawned.TryGetValue(new SpawnedObjectSheetKey(set, tileBase), out SpawnedObjectSheet? sheet) ? sheet
            : throw new InvalidDataException($"Enemy graphics set {set} installs no spawned-object sheet for tile base ${tileBase:X3}.");

    /// <summary>SHA-256 of every distinct sheet and each set's choice of them.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledEnemySheets), content =>
    {
        foreach ((EnemySheetKey key, SpriteSheet sheet) in sheets.OrderBy(entry => entry.Key.Set, StringComparer.Ordinal)
                     .ThenBy(entry => entry.Key.Enemy))
        {
            content.Append("set", System.Text.Encoding.UTF8.GetBytes(key.Set));
            content.Append("enemy", (int)key.Enemy);
            sheet.AppendIdentity(content);
        }
        foreach ((SpawnedObjectSheetKey key, SpawnedObjectSheet sheet) in spawned.OrderBy(entry => entry.Key.Set, StringComparer.Ordinal)
                     .ThenBy(entry => entry.Key.TileBase))
        {
            content.Append("spawned-set", System.Text.Encoding.UTF8.GetBytes(key.Set));
            content.Append("tile-base", key.TileBase);
            content.Append("palette", sheet.Palette);
            sheet.Sheet.AppendIdentity(content);
        }
        AppendPictures(content, "pictures", pictures.Select(entry => new PictureOwner(entry.Key.Set, entry.Key.Enemy.ToString(), entry.Value)));
        AppendPictures(content, "spawned-pictures", spawnedPictures.Select(entry => new PictureOwner(entry.Key.Set,
            entry.Key.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture), entry.Value)));
        AppendRotPictures(content, "rot-pictures", rotPictures.Select(entry => new RotPictureOwner(entry.Key.Set, entry.Key.Enemy.ToString(), entry.Value)));
        AppendRotPictures(content, "spawned-rot-pictures", spawnedRotPictures.Select(entry => new RotPictureOwner(entry.Key.Set,
            entry.Key.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture), entry.Value)));
    });

    /// <summary>Appends each sheet's frames drawn over a rotting corpse's rows and the corpse each shows, in a stable order.</summary>
    private static void AppendRotPictures(SelectedPresentationHash content, string label, IEnumerable<RotPictureOwner> owners)
    {
        foreach (RotPictureOwner owner in owners.OrderBy(owner => owner.Set, StringComparer.Ordinal).ThenBy(owner => owner.Owner, StringComparer.Ordinal))
        {
            content.Append(label, System.Text.Encoding.UTF8.GetBytes($"{owner.Set}/{owner.Owner}"));
            foreach ((string frame, string corpse) in owner.Frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                content.Append("rot-frame", System.Text.Encoding.UTF8.GetBytes($"{frame}={corpse}"));
        }
    }

    /// <summary>One sheet's frames drawn over a rotting corpse's rows.</summary>
    /// <param name="Set">The graphics set's name.</param>
    /// <param name="Owner">The enemy's name, or the spawned objects' tile base.</param>
    /// <param name="Frames">The frames and the kind of corpse whose rot each shows.</param>
    private sealed record RotPictureOwner(string Set, string Owner, IReadOnlyDictionary<string, string> Frames);

    /// <summary>Appends each sheet's frames drawn over uploaded characters and their regions, in a stable order.</summary>
    private static void AppendPictures(SelectedPresentationHash content, string label, IEnumerable<PictureOwner> owners)
    {
        foreach (PictureOwner owner in owners.OrderBy(owner => owner.Set, StringComparer.Ordinal).ThenBy(owner => owner.Owner, StringComparer.Ordinal))
        {
            content.Append(label, System.Text.Encoding.UTF8.GetBytes($"{owner.Set}/{owner.Owner}"));
            foreach ((string frame, CharacterRegion[][] layers) in owner.Frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                content.Append("frame", System.Text.Encoding.UTF8.GetBytes(frame));
                foreach (CharacterRegion[] layer in layers)
                {
                    content.Append("layer", layer.Length);
                    foreach (CharacterRegion region in layer)
                    {
                        content.Append("region", region.Ordinal);
                        content.Append("region-name", System.Text.Encoding.UTF8.GetBytes(region.Name));
                    }
                }
            }
        }
    }

    /// <summary>One sheet's frames drawn over uploaded characters.</summary>
    /// <param name="Set">The graphics set's name.</param>
    /// <param name="Owner">The enemy's name, or the spawned objects' tile base.</param>
    /// <param name="Frames">The frames and the layers each draws as.</param>
    private sealed record PictureOwner(string Set, string Owner, IReadOnlyDictionary<string, CharacterRegion[][]> Frames);
}
