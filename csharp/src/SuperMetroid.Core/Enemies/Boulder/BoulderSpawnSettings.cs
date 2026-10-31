namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>How close Samus must come before a Boulder falls, and how it rolls (#1471, #1472).</summary>
/// <param name="TriggerHeight">The vertical distance, 0 through 255 pixels, Samus must be strictly within.</param>
/// <param name="TriggerReach">The horizontal distance, 0 through 255 pixels, ahead of it Samus must be within.</param>
/// <param name="Drop">Pixels, 0 through 255, it hangs above its resting place; zero rolls without bouncing.</param>
/// <param name="Roll">How it rolls.</param>
/// <param name="Landing">How low it lands.</param>
internal sealed record BoulderSpawnSettings(int TriggerHeight, int TriggerReach, int Drop, BoulderRoll Roll, BoulderLanding Landing) : EnemySpawnSettings
{
    /// <summary>The vertical distance, 0 through 255 pixels, Samus must be strictly within.</summary>
    public int TriggerHeight { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, TriggerHeight);

    /// <summary>The horizontal distance, 0 through 255 pixels, ahead of it Samus must be within.</summary>
    public int TriggerReach { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, TriggerReach);

    /// <summary>Pixels, 0 through 255, it hangs above its resting place; zero rolls without bouncing.</summary>
    public int Drop { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Drop);
}
