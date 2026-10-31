namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>How an Sbug swarms (#1472).</summary>
/// <param name="Speed">Its speed, 0 through 255.</param>
/// <param name="InitialAngle">Its first angle, 0 through 255.</param>
/// <param name="ActivationRadius">The distance, 0 through 255 pixels, Samus must come within.</param>
/// <param name="Behavior">Its activation behaviour, 0 through 255.</param>
internal sealed record SbugSpawnSettings(int Speed, int InitialAngle, int ActivationRadius, int Behavior) : EnemySpawnSettings
{
    /// <summary>Its speed, 0 through 255.</summary>
    public int Speed { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Speed);

    /// <summary>Its first angle, 0 through 255.</summary>
    public int InitialAngle { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, InitialAngle);

    /// <summary>The distance, 0 through 255 pixels, Samus must come within.</summary>
    public int ActivationRadius { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, ActivationRadius);

    /// <summary>Its activation behaviour, 0 through 255.</summary>
    public int Behavior { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Behavior);
}
