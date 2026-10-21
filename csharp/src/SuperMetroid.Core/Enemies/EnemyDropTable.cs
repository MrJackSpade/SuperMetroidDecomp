namespace SuperMetroid.Core.Enemies;

/// <summary>One of the authored item-drop probability tables, by its position among them.</summary>
/// <param name="Record">The table's position among the authored tables.</param>
public readonly record struct EnemyDropTable(int Record);
