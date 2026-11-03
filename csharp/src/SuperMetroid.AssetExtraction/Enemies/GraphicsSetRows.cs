using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The palette row each enemy drawn under one room graphics set draws in (#1512): the import's knowledge of where the
/// set's load leaves each member's colours, which shared row an enemy's initialization fixes, and whose row an enemy
/// copies. Gameplay never sees a row.
/// </summary>
internal sealed class GraphicsSetRows
{
    /// <summary>The row each record loads its member's colours into, in record order.</summary>
    private readonly int[] recordRows;

    /// <summary>The row each member's first record loads its colours into.</summary>
    private readonly Dictionary<EnemyDefinitionId, int> memberRows = [];

    /// <summary>Resolves set <paramref name="set"/>'s rows from the catalogued record rows.</summary>
    /// <exception cref="InvalidDataException">The set's rows are not catalogued for each of its records.</exception>
    internal GraphicsSetRows(RoomEnemyGraphicsSetDefinition set)
    {
        Ensure.NotNull(set);
        ReadOnlySpan<RoomEnemyGraphicsSetHeader> records = set.Records.Span;
        recordRows = records.Length == 0 ? []
            : EnemyGraphicsSetPaletteRowDefinitions.Rows.TryGetValue(set.Name, out int[]? catalogued) ? catalogued
            : throw new InvalidDataException($"Enemy graphics set {set.Name} has no catalogued palette rows.");
        if (recordRows.Length != records.Length)
            throw new InvalidDataException($"Enemy graphics set {set.Name} catalogues {recordRows.Length} palette rows for {records.Length} records.");
        for (int record = 0; record < records.Length; record++)
            memberRows.TryAdd(records[record].DefinitionId, recordRows[record]);
    }

    /// <summary>The row record <paramref name="record"/> loads its member's colours into.</summary>
    internal int RecordRow(int record) => recordRows[Ensure.BetweenInclusive(0, recordRows.Length - 1, record)];

    /// <summary>
    /// The row <paramref name="enemy"/> draws in under the set: the shared row its initialization fixes, the row of the
    /// enemy whose row it copies, its own first record's, or, for an enemy the set does not load, the enemy projectiles'
    /// row, as the game's fallback selects.
    /// </summary>
    internal int RowOf(EnemyDefinitionId enemy)
    {
        if (EnemySheetRowDefinitions.FixedRows.TryGetValue(enemy, out int fixedRow))
            return fixedRow;
        EnemyDefinitionId owner = EnemySheetRowDefinitions.DrawsInRowOf.GetValueOrDefault(enemy, enemy);
        return memberRows.TryGetValue(owner, out int row) ? row : EnemySheetRowDefinitions.EnemyProjectiles;
    }
}
