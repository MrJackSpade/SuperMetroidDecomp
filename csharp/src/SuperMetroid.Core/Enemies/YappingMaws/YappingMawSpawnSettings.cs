namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Which way a Yapping Maw reaches and how near Samus must come (#1472).</summary>
/// <param name="ActivationDistance">The distance, 0 through 65535 pixels, Samus must come within.</param>
/// <param name="Attack">The way it reaches.</param>
internal sealed record YappingMawSpawnSettings(int ActivationDistance, VerticalDirection Attack) : EnemySpawnSettings
{
    /// <summary>The distance, 0 through 65535 pixels, Samus must come within.</summary>
    public int ActivationDistance { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, ActivationDistance);
}
