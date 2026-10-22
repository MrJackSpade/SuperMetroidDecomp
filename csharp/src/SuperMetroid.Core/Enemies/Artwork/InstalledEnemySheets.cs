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

    /// <summary>The sets whose spawned objects draw from sheets.</summary>
    private readonly HashSet<string> spawningSets;

    /// <summary>The sheets by graphics set name and enemy, and the spawned-object sheets by set name and tile base.</summary>
    public InstalledEnemySheets(IReadOnlyDictionary<EnemySheetKey, SpriteSheet> sheets,
        IReadOnlyDictionary<SpawnedObjectSheetKey, SpawnedObjectSheet> spawned)
    {
        this.sheets = new Dictionary<EnemySheetKey, SpriteSheet>(Ensure.NotNull(sheets));
        this.spawned = new Dictionary<SpawnedObjectSheetKey, SpawnedObjectSheet>(Ensure.NotNull(spawned));
        spawningSets = [.. this.spawned.Keys.Select(key => key.Set)];
    }

    /// <inheritdoc/>
    public SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy) => For(Ensure.NotNull(set).Name, enemy);

    /// <summary>The sheet of <paramref name="enemy"/> as graphics set <paramref name="set"/> loads it.</summary>
    /// <exception cref="InvalidDataException">The set does not load that enemy.</exception>
    public SpriteSheet For(string set, EnemyDefinitionId enemy) =>
        sheets.TryGetValue(new EnemySheetKey(set, enemy), out SpriteSheet? sheet) ? sheet
            : throw new InvalidDataException($"Enemy graphics set {set} installs no sheet for {enemy}.");

    /// <inheritdoc/>
    public bool SpawnsFromSheets(RoomEnemyGraphicsSetDefinition set) => spawningSets.Contains(Ensure.NotNull(set).Name);

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
    });
}
