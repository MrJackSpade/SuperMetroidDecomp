namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>How close Samus must come before a Boulder falls, and how it rolls (#1471, #1472, #1519).</summary>
/// <param name="TriggerHeight">The vertical distance, in pixels, Samus must be strictly within.</param>
/// <param name="TriggerReach">The horizontal distance, in pixels, ahead of it Samus must be within.</param>
/// <param name="Drop">The pixels it hangs above its resting place; zero rolls without bouncing.</param>
/// <param name="Roll">How it rolls.</param>
/// <param name="Landing">How low it lands.</param>
internal sealed record BoulderSpawnSettings(int TriggerHeight, int TriggerReach, int Drop, BoulderRoll Roll, BoulderLanding Landing) : EnemySpawnSettings;
