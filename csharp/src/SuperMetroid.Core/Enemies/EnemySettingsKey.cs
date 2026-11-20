namespace SuperMetroid.Core.Enemies;

/// <summary>Which factory builds an entry's enemy (#2554): the kind of settings the entry tells, for the entry's definition.</summary>
/// <param name="Settings">The settings' type.</param>
/// <param name="Definition">The entry's definition.</param>
internal readonly record struct EnemySettingsKey(Type Settings, EnemyDefinitionId Definition);
