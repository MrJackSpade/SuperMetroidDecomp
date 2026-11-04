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
    /// <summary>The set whose rows these are.</summary>
    private readonly RoomEnemyGraphicsSetDefinition set;

    /// <summary>The set's catalogued rows, resolved when a lookup first needs them.</summary>
    private Rows? resolved;

    /// <summary>Keeps set <paramref name="set"/>, whose catalogued record rows are resolved when a lookup needs them.</summary>
    internal GraphicsSetRows(RoomEnemyGraphicsSetDefinition set) => this.set = Ensure.NotNull(set);

    /// <summary>The set's catalogued record rows and the row each member's first record loads its colours into.</summary>
    /// <exception cref="InvalidDataException">The set's rows are not catalogued for each of its records.</exception>
    private Rows Catalogued()
    {
        if (resolved is not null)
            return resolved;
        ReadOnlySpan<RoomEnemyGraphicsSetMember> records = set.Records.Span;
        int[] recordRows = records.Length == 0 ? []
            : EnemyGraphicsSetPaletteRowDefinitions.Rows.TryGetValue(set.Name, out int[]? catalogued) ? catalogued
            : throw new InvalidDataException($"Enemy graphics set {set.Name} has no catalogued palette rows.");
        if (recordRows.Length != records.Length)
            throw new InvalidDataException($"Enemy graphics set {set.Name} catalogues {recordRows.Length} palette rows for {records.Length} records.");
        var memberRows = new Dictionary<EnemyDefinitionId, int>();
        for (int record = 0; record < records.Length; record++)
            memberRows.TryAdd(records[record].DefinitionId, recordRows[record]);
        return resolved = new Rows(recordRows, memberRows);
    }

    /// <summary>The row record <paramref name="record"/> loads its member's colours into.</summary>
    internal int RecordRow(int record)
    {
        int[] recordRows = Catalogued().RecordRows;
        return recordRows[Ensure.BetweenInclusive(0, recordRows.Length - 1, record)];
    }

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
        return Catalogued().MemberRows.TryGetValue(owner, out int row) ? row : EnemySheetRowDefinitions.EnemyProjectiles;
    }

    /// <summary>A set's catalogued rows.</summary>
    /// <param name="RecordRows">The row each record loads its member's colours into, in record order.</param>
    /// <param name="MemberRows">The row each member's first record loads its colours into.</param>
    private sealed record Rows(int[] RecordRows, Dictionary<EnemyDefinitionId, int> MemberRows);
}
