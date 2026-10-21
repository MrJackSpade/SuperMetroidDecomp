namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>The bank, operands, spritemaps and count of an enemy family's resources.</summary>
/// <param name="Name">The name.</param>
/// <param name="Bank">The bank.</param>
/// <param name="Operands">The operands.</param>
/// <param name="Spritemaps">The spritemaps.</param>
/// <param name="Count">The count.</param>
internal readonly record struct OrdinaryEnemyResourceFamily(string Name, byte Bank, ushort[]? Operands, ushort[]? Spritemaps, int Count);
