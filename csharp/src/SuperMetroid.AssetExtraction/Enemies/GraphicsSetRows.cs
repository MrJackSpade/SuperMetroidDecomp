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
    /// <summary>The row each member's first record loads its colours into.</summary>
    private readonly Dictionary<EnemyDefinitionId, int> memberRows = [];

    /// <summary>The rows set <paramref name="set"/>'s records load their members' colours into.</summary>
    internal GraphicsSetRows(RoomEnemyGraphicsSetMembers set)
    {
        foreach (RoomEnemyGraphicsSetMember record in Ensure.NotNull(set).Records.Span)
        {
            // A record loading artwork for no enemy fills its row, but no enemy draws in it by that record (#1686).
            if (record.DefinitionId is { } member)
                memberRows.TryAdd(member, record.PaletteRow);
        }
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
        return memberRows.TryGetValue(owner, out int row) ? row : EnemySheetRowDefinitions.EnemyProjectiles;
    }
}
