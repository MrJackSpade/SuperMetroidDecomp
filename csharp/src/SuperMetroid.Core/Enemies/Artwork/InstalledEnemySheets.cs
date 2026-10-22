using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed pre-coloured enemy sheets (#1392), by the room graphics set that loads an enemy and the enemy: the
/// sheet shows the enemy's frames in the colours its palette row holds once that set has loaded.
/// </summary>
public sealed class InstalledEnemySheets : IEnemySheetSource
{
    /// <summary>Each set's sheet for each of its enemies.</summary>
    private readonly Dictionary<EnemySheetKey, SpriteSheet> sheets;

    /// <summary>The sheets by graphics set name and enemy.</summary>
    public InstalledEnemySheets(IReadOnlyDictionary<EnemySheetKey, SpriteSheet> sheets) =>
        this.sheets = new Dictionary<EnemySheetKey, SpriteSheet>(Ensure.NotNull(sheets));

    /// <inheritdoc/>
    public SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy) => For(Ensure.NotNull(set).Name, enemy);

    /// <summary>The sheet of <paramref name="enemy"/> as graphics set <paramref name="set"/> loads it.</summary>
    /// <exception cref="InvalidDataException">The set does not load that enemy.</exception>
    public SpriteSheet For(string set, EnemyDefinitionId enemy) =>
        sheets.TryGetValue(new EnemySheetKey(set, enemy), out SpriteSheet? sheet) ? sheet
            : throw new InvalidDataException($"Enemy graphics set {set} installs no sheet for {enemy}.");

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
    });
}
