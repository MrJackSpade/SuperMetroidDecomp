namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What an escaping Etecoon does (#1472).</summary>
/// <param name="Role">Its role.</param>
internal sealed partial record EscapeEtecoonSpawnSettings(EscapeEtecoonRole Role) : EnemySpawnSettings;
