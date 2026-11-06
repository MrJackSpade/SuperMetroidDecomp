namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Which way a Yapping Maw reaches and how near Samus must come (#1472, #1519).</summary>
/// <param name="ActivationDistance">The distance in pixels Samus must come within.</param>
/// <param name="Attack">The way it reaches.</param>
internal sealed record YappingMawSpawnSettings(int ActivationDistance, VerticalDirection Attack) : EnemySpawnSettings;
